using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Fork.Annotations;
using Fork.Logic.Persistence;

namespace Fork.Logic.Model
{
    [Serializable]
    public class JavaSettings : INotifyPropertyChanged
    {
        public int MaxRam { get; set; } = 2048;
        public string JavaPath { get; set; } = AppSettingsSerializer.Instance.AppSettings.DefaultJavaPath;
        public string StartupParameters { get; set; } = "";
        
        public JavaSettings(){}

        public JavaSettings(JavaSettings javaSettings)
        {
            MaxRam = javaSettings.MaxRam;
            JavaPath = javaSettings.JavaPath;
            StartupParameters = javaSettings.StartupParameters;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}