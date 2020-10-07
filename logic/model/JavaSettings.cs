using System;

namespace Fork.Logic.Model
{
    [Serializable]
    public class JavaSettings
    {
        public int MaxRam { get; set; } = 2048;
        public int MinRam { get; set; } = 512;
        public string JavaPath { get; set; } = "java.exe";
        public string StartupParameters { get; set; } = "";
        
        public JavaSettings(){}

        public JavaSettings(JavaSettings javaSettings)
        {
            MaxRam = javaSettings.MaxRam;
            MinRam = javaSettings.MinRam;
            JavaPath = javaSettings.JavaPath;
            StartupParameters = javaSettings.StartupParameters;
        }
    }
}