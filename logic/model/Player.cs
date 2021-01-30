using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Fork.Properties;
using Newtonsoft.Json;

namespace Fork.Logic.Model
{
    public class Player : IEquatable<Player>
    {
        public bool OfflineChar { get; set; }
        public string Name { get; set; }
        public string Uid { get; set; }
        public string Head { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.MinValue;

        [JsonIgnore] public Player Self => this;

        public bool Equals(Player other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return OfflineChar == other.OfflineChar && Name == other.Name && Uid == other.Uid;
        }

        public async Task InitWithName(string name)
        {
            Name = name;
            await RetrieveUid();
            if (!OfflineChar)
                await Task.Run(RetrieveHead);
            else
                Head = SetOfflineHead();
            LastUpdated = DateTime.Now;
        }

        public async Task InitWithUid(string uid)
        {
            Uid = uid;
            await RetrieveNameAndHead();
            LastUpdated = DateTime.Now;
        }

        public async Task Update()
        {
            await RetrieveNameAndHead();
            LastUpdated = DateTime.Now;
        }

        public override string ToString()
        {
            return Name;
        }

        private async Task RetrieveNameAndHead()
        {
            string cachedName = NameFromCache(Uid);
            if (cachedName != null)
            {
                Name = cachedName;
                return;
            }

            var client = new HttpClient();
            var uri = new Uri("https://sessionserver.mojang.com/session/minecraft/profile/" + Uid);
            var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
            while (response.StatusCode == (HttpStatusCode) 429)
            {
                await Task.Delay(5000);
                response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
            }

            //Exception for user not found: Response 204 and 404
            if (response.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine("Error while retrieving Mojang data for UUID: " + Uid);
                Name = Uid;
                OfflineChar = true;
                Head = SetOfflineHead();
                return;
            }

            Stream respStream = await response.Content.ReadAsStreamAsync();
            string fullProfileString = await new StreamReader(respStream).ReadToEndAsync();
            FullProfile fullProfile = JsonConvert.DeserializeObject<FullProfile>(fullProfileString);
            CacheProfileJson(fullProfile);
            Name = fullProfile.name;
            Head = SetOfflineHead();
            if (fullProfile.properties.Count != 0)
                await Task.Run(async () => Head = await RetrieveImageFromBase64(fullProfile.properties[0].value));
        }

        private async Task RetrieveUid()
        {
            var client = new HttpClient();
            var uri = new Uri("https://api.mojang.com/users/profiles/minecraft/" + Name);
            var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
            while (response.StatusCode == (HttpStatusCode) 429)
            {
                await Task.Delay(5000);
                response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
            }

            //Exception for user not found: Response 204 or 403, 404 etc.
            if (response.StatusCode != HttpStatusCode.OK)
            {
                Uid = CalculateOfflineUuid(Name);
                OfflineChar = true;
                return;
            }

            Stream respStream = await response.Content.ReadAsStreamAsync();
            string nameUidString = await new StreamReader(respStream).ReadToEndAsync();
            NameUid nameUid = JsonConvert.DeserializeObject<NameUid>(nameUidString);

            Uid = nameUid.id;
        }

        private void CacheProfileJson(FullProfile profile)
        {
            DirectoryInfo dirInfo = Directory.CreateDirectory(Path.Combine(App.ApplicationPath, "players", profile.id));
            string json = JsonConvert.SerializeObject(profile, Formatting.Indented);
            using (FileStream fs = File.Create(Path.Combine(dirInfo.FullName, "profile.json")))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.WriteLine(json);
            }
        }

        private string NameFromCache(string uid)
        {
            string path = Path.Combine(App.ApplicationPath, "players", uid, "profile.json");
            if (!File.Exists(path)) return null;

            if (File.GetCreationTime(path).AddDays(2) < DateTime.Now) return null;

            FullProfile profile = JsonConvert.DeserializeObject<FullProfile>(File.ReadAllText(path));
            return profile.name;
        }

        private string HeadFromCache(string uid)
        {
            string path = Path.Combine(App.ApplicationPath, "players", uid, "head.jpg");
            if (!File.Exists(path)) return null;

            if (File.GetCreationTime(path).AddDays(2) < DateTime.Now) return null;

            return Path.GetFullPath(path);
        }

        private async Task RetrieveHead()
        {
            string cachedHead = HeadFromCache(Uid);
            if (cachedHead != null)
            {
                Head = cachedHead;
                return;
            }

            var client = new HttpClient();
            var uri = new Uri("https://sessionserver.mojang.com/session/minecraft/profile/" + Uid);
            var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
            while (response.StatusCode == (HttpStatusCode) 429)
            {
                await Task.Delay(5000);
                try
                {
                    response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error while retrieving Player Head for " + Name + ": " + e.Message +
                                      ".  Aborting...");
                    return;
                }
            }

            response.EnsureSuccessStatusCode();

            Stream respStream = await response.Content.ReadAsStreamAsync();
            string fullProfileString = await new StreamReader(respStream).ReadToEndAsync();
            FullProfile fullProfile = JsonConvert.DeserializeObject<FullProfile>(fullProfileString);
            CacheProfileJson(fullProfile);
            if (fullProfile.properties.Count == 0)
                Head = SetOfflineHead();
            else
                Head = await RetrieveImageFromBase64(fullProfile.properties[0].value);
        }

        private async Task<string> RetrieveImageFromBase64(string base64String)
        {
            var profileJson = Convert.FromBase64String(base64String);
            string profileJsonString = Encoding.UTF8.GetString(profileJson);
            Profile profile = JsonConvert.DeserializeObject<Profile>(profileJsonString);
            Bitmap b;

            if (profile.textures.SKIN == null)
            {
                Image defaultHead = Resources.DeafultHead;
                b = new Bitmap(defaultHead);
            }
            else
            {
                var client = new HttpClient();
                var uri = new Uri(profile.textures.SKIN.url);
                var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
                while (response.StatusCode == (HttpStatusCode) 429)
                {
                    await Task.Delay(5000);
                    response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
                }

                response.EnsureSuccessStatusCode();

                Stream respStream = await response.Content.ReadAsStreamAsync();
                Image image = Image.FromStream(respStream);

                Bitmap skinBmp = new Bitmap(image);
                Bitmap headBmp = skinBmp.Clone(new Rectangle(8, 8, 8, 8), skinBmp.PixelFormat);
                Bitmap overlayHead = skinBmp.Clone(new Rectangle(40, 8, 8, 8), skinBmp.PixelFormat);

                //Remove Transparency
                b = new Bitmap(8, 8);
                Graphics g = Graphics.FromImage(b);
                g.Clear(Color.Black);
                g.DrawImage(headBmp, 0, 0);
                g.DrawImage(overlayHead, 0, 0);

                //Dispose
                headBmp.Dispose();
                skinBmp.Dispose();
            }

            //Write .jpg file
            Directory.CreateDirectory(Path.Combine(App.ApplicationPath, "players", profile.profileId));
            string path = Path.Combine(App.ApplicationPath, "players", profile.profileId, "head.jpg");
            path = new DirectoryInfo(path).FullName;
            b.Save(path);
            b.Dispose();

            return path;
        }

        private string SetOfflineHead()
        {
            Image defaultHead = Resources.DeafultHead;
            Bitmap b = new Bitmap(defaultHead);
            Directory.CreateDirectory(Path.Combine(App.ApplicationPath, "players", Uid));
            string path = Path.Combine(App.ApplicationPath, "players", Uid, "head.jpg");
            path = new DirectoryInfo(path).FullName;
            b.Save(path);
            b.Dispose();

            return path;
        }

        private string CalculateOfflineUuid(string playerName)
        {
            byte[] input = Encoding.UTF8.GetBytes("OfflinePlayer:" + playerName);
            MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(input);
            hash[6] &= 0x0f;
            hash[6] |= 0x30;
            hash[8] &= 0x3f;
            hash[8] |= 0x80;
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Player) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(OfflineChar, Name, Uid);
        }


        private class NameUid
        {
            public string name { get; set; }
            public string id { get; set; }
        }

        private class FullProfile
        {
            public string id { get; set; }
            public string name { get; set; }
            public List<Property> properties { get; set; }
        }

        private class Property
        {
            public string name { get; set; }
            public string value { get; set; }
        }

        private class Profile
        {
            public long timestamp { get; set; }
            public string profileId { get; set; }
            public string profileName { get; set; }
            public Textures textures { get; set; }
        }

        private class Textures
        {
            public Skin SKIN { get; set; }
        }

        private class Skin
        {
            public string url { get; set; }
        }

        private class Cape
        {
            public string Url { get; set; }
        }
    }
}