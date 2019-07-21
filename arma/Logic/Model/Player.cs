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
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace nihilus.Logic.Model
{
    public class Player
    {
        public string Name { get; set; }
        public string Uid { get; set; }
        public ImageSource Head { get; set; }

        public Player(string name)
        {
            Name = name;
            new Thread(() =>
            {
                RetrieveUid();
                RetrieveHead();
            }).Start();
        }

        private void RetrieveUid()
        {
            var client = new HttpClient();
            var uri = new Uri("https://api.mojang.com/users/profiles/minecraft/" + Name);
            var response = client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead).Result;
            while (response.StatusCode==(HttpStatusCode)429)
            {
                Thread.Sleep(5000);
                response = client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead).Result;
            }

            response.EnsureSuccessStatusCode();

            Stream respStream = response.Content.ReadAsStreamAsync().Result;
            string nameUidString = new StreamReader(respStream).ReadToEnd();
            NameUid nameUid = JsonConvert.DeserializeObject<NameUid>(nameUidString);
            Uid = nameUid.id;
        }

        private void RetrieveHead()
        {
            var client = new HttpClient();
            var uri = new Uri("https://sessionserver.mojang.com/session/minecraft/profile/"+Uid);
            var response = client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead).Result;
            while (response.StatusCode==(HttpStatusCode)429)
            {
                Thread.Sleep(5000);
                response = client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead).Result;
            }

            response.EnsureSuccessStatusCode();

            Stream respStream = response.Content.ReadAsStreamAsync().Result;
            string fullProfileString = new StreamReader(respStream).ReadToEnd();
            FullProfile fullProfile = JsonConvert.DeserializeObject<FullProfile>(fullProfileString);
            RetrieveImageFromBase64(fullProfile.properties[0].value);
        }

        private void RetrieveImageFromBase64(string base64String)
        {
            var profileJson = Convert.FromBase64String(base64String);
            string profileJsonString = Encoding.UTF8.GetString(profileJson);
            Profile profile = JsonConvert.DeserializeObject<Profile>(profileJsonString);
            
            var client = new HttpClient();
            var uri = new Uri(profile.textures.SKIN.url);
            var response = client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead).Result;
            while (response.StatusCode==(HttpStatusCode)429)
            {
                Thread.Sleep(5000);
                response = client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead).Result;
            }

            response.EnsureSuccessStatusCode();

            Stream respStream = response.Content.ReadAsStreamAsync().Result;
            Image image = Image.FromStream(respStream);
            
            Bitmap headBmp = new Bitmap(8,8);
            Bitmap skinBmp = new Bitmap(image);
            headBmp = skinBmp.Clone(new Rectangle(8, 8, 8, 8),skinBmp.PixelFormat);
            //headBmp.Save("icon"+profile.profileName+".jpg");

            Head = BitmapToImageSource(headBmp);
            //System.Console.WriteLine(profile.profileName+"s Head has a width of "+Head.Width+" and a Height of "+Head.Height);
        }
        
        private BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
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