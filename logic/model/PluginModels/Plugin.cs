using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Media;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Fork.Logic.Model.PluginModels
{
    /// <summary>
    /// This class represents a Spigot Plugin in the form that is provided by spiget.org
    /// </summary>
    public class File {
        public string actualType { get; set; }
        public string type { get; set; } 
        public double size { get; set; } 
        public string sizeUnit { get; set; } 
        public string url { get; set; } 
    }

    public class Rating {
        public int count { get; set; }

        private double averageInternal;
        public double average
        {
            get => Math.Round(averageInternal, 2);
            set => averageInternal = value;
        }
    }

    public class Icon {
        public string url { get; set; } 
        public string data { get; set; }
        [JsonIgnore] public string URL => "https://www.spigotmc.org/" + url;
    }

    public class Author {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Category
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Plugin{
        public int id { get; set; } 
        public string name { get; set; } 
        public string tag { get; set; } 
        public string contributors { get; set; } 
        public int likes { get; set; } 
        public File file { get; set; } 
        public List<string> testedVersions { get; set; }
        public Rating rating { get; set; } 
        public Author author { get; set; }
        public Category category { get; set; }
        public long releaseDate { get; set; }
        [JsonIgnore] public string ReleaseDate => DateTimeConverter(releaseDate);
        public long updateDate { get; set; }
        [JsonIgnore] public string UpdateDate => DateTimeConverter(updateDate);
        public int downloads { get; set; } 
        public bool external { get; set; } 
        public Icon icon { get; set; } 
        public bool premium { get; set; } 
        public double price { get; set; } 
        public string currency { get; set; } 

        private string DateTimeConverter(long date)
        {
            DateTime dateTime = new DateTime(1970,1,1,0,0,0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(date);
            return dateTime.ToString("MMM dd, yyyy", new CultureInfo("ISO"));
        }
    }
}