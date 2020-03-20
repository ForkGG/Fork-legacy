using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using nihilus.Logic.Manager;
using nihilus.Properties;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace nihilus.Logic.Model
{
    public class Player
    {
        private bool offlineChar = false;
        
        public string Name { get; set; }
        public string Uid { get; set; }
        public string Head { get; set; }
        
        [JsonIgnore]
        public Player Self => this;

        public Player(string name)
        {
            Name = name;
            RetrieveUid();
            if (offlineChar)
            {
                Head = SetOfflineHead();
            }
            else
            {
                RetrieveHead();
            }
        }

        public override string ToString()
        {
            return Name;
        }

        private void RetrieveUid()
        {
            string cachedUid = UidFromCache(Name);
            if (cachedUid!=null)
            {
                Uid = cachedUid;
                return;
            }
            var client = new HttpClient();
            var uri = new Uri("https://api.mojang.com/users/profiles/minecraft/" + Name);
            var response = client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead).Result;
            while (response.StatusCode == (HttpStatusCode) 429)
            {
                Thread.Sleep(5000);
                response = client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead).Result;
            }

            response.EnsureSuccessStatusCode();
            
            
            //Exception for user not found: Response 204
            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                Uid = Name;
                offlineChar = true;
                return;
            }

            Stream respStream = response.Content.ReadAsStreamAsync().Result;
            string nameUidString = new StreamReader(respStream).ReadToEnd();
            NameUid nameUid = JsonConvert.DeserializeObject<NameUid>(nameUidString);
            
            Uid = nameUid.id;
        }

        private void CacheProfileJson(FullProfile profile)
        {
            DirectoryInfo dirInfo= Directory.CreateDirectory(Path.Combine("players",profile.id));
            string json = JsonConvert.SerializeObject(profile);
            using (FileStream fs =File.Create(Path.Combine(dirInfo.FullName,"profile.json")))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.WriteLine(json);
            }
        }
        private string UidFromCache(string uid)
        {
            string path = Path.Combine("players", uid, "profile.json");
            if (!File.Exists(path))
            {
                return null;
            }

            if (File.GetCreationTime(path).AddDays(2)<DateTime.Now)
            {
                return null;
            }
            FullProfile profile = JsonConvert.DeserializeObject<FullProfile>(File.ReadAllText(path));
            return profile.id;
        }

        private string HeadFromCache(string uid)
        {
            string path = Path.Combine("players", uid, "head.jpg");
            if (!File.Exists(path))
            {
                return null;
            }

            if (File.GetCreationTime(path).AddDays(2)<DateTime.Now)
            {
                return null;
            }

            return Path.GetFullPath(path);
        }

        private void RetrieveHead()
        {
            string cachedHead = HeadFromCache(Uid);
            if (cachedHead!=null)
            {
                Head = cachedHead;
                return;
            }
            var client = new HttpClient();
            var uri = new Uri("https://sessionserver.mojang.com/session/minecraft/profile/" + Uid);
            var response = client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead).Result;
            while (response.StatusCode == (HttpStatusCode) 429)
            {
                Thread.Sleep(5000);
                try
                {
                    response = client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead).Result;
                } catch(Exception e)
                {
                    Console.WriteLine("Error while retrieving Player Head for " + Name+": "+e.Message+".  Aborting...");
                    return;
                }
            }

            response.EnsureSuccessStatusCode();

            Stream respStream = response.Content.ReadAsStreamAsync().Result;
            string fullProfileString = new StreamReader(respStream).ReadToEnd();
            FullProfile fullProfile = JsonConvert.DeserializeObject<FullProfile>(fullProfileString);
            CacheProfileJson(fullProfile);
            Head = RetrieveImageFromBase64(fullProfile.properties[0].value);
        }

        private string RetrieveImageFromBase64(string base64String)
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
                //TODO implement default skin for people without skin
                var uri = new Uri(profile.textures.SKIN.url);
                var response = client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead).Result;
                while (response.StatusCode == (HttpStatusCode) 429)
                {
                    Thread.Sleep(5000);
                    response = client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead).Result;
                }

                response.EnsureSuccessStatusCode();

                Stream respStream = response.Content.ReadAsStreamAsync().Result;
                Image image = Image.FromStream(respStream);

                Bitmap skinBmp = new Bitmap(image);
                Bitmap headBmp = skinBmp.Clone(new Rectangle(8, 8, 8, 8), skinBmp.PixelFormat);
                Bitmap overlayHead = skinBmp.Clone(new Rectangle(40, 8, 8, 8), skinBmp.PixelFormat);
            
                //Remove Transparency
                b = new Bitmap(8,8);
                Graphics g = Graphics.FromImage(b);
                g.Clear(Color.Black);
                g.DrawImage(headBmp,0,0);
                g.DrawImage(overlayHead,0,0);
                
                //Dispose
                headBmp.Dispose();
                skinBmp.Dispose();
            }
            //Write .jpg file
            Directory.CreateDirectory(Path.Combine(App.ApplicationPath,"players",profile.profileId));
            string path = Path.Combine(App.ApplicationPath,"players",profile.profileId,"head.jpg");
            path = new DirectoryInfo(path).FullName;
            b.Save(path);
            b.Dispose();
            
            return path;
        }

        private string SetOfflineHead()
        {
            Image defaultHead = Resources.DeafultHead;
             Bitmap b = new Bitmap(defaultHead);
             Directory.CreateDirectory(Path.Combine(App.ApplicationPath,"players",Name));
             string path = Path.Combine(App.ApplicationPath,"players",Name,"head.jpg");
             path = new DirectoryInfo(path).FullName;
             b.Save(path);
             b.Dispose();
            
             return path;
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