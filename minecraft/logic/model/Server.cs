using System;
using System.IO;
using System.Xml.Serialization;
using nihilus.Logic.Persistence;

namespace nihilus.Logic.Model
{
    [Serializable]
    public class Server
    {
        private ServerSettings serverSettings;
        //Backups
        private ServerSettings backupServerSettings;
        private ServerJavaSettings backupJavaSettings;
        private ServerRestart backupRestart1;
        private ServerRestart backupRestart2;
        private ServerRestart backupRestart3;
        private ServerRestart backupRestart4;
        
        public String Name { get; set; }
        public ServerVersion Version { get; set; }
        public ServerJavaSettings JavaSettings { get; set; }
        
        //TODO: Apply restarts
        public ServerRestart Restart1 { get; set; }
        public ServerRestart Restart2 { get; set; }
        public ServerRestart Restart3 { get; set; }
        public ServerRestart Restart4 { get; set; }
        [XmlIgnore]
        public ServerSettings ServerSettings {
            get
            {
                if (serverSettings == null)
                    serverSettings = new ServerSettings(new FileReader().ReadServerSettings(Path.Combine(App.ApplicationPath,Name)));
                return serverSettings;
            }
            set => serverSettings = value;
        }

        public Server(String name, ServerVersion version, ServerSettings serverSettings, ServerJavaSettings javaSettings) {
            Name = name;
            Version = version;
            JavaSettings = javaSettings;
            ServerSettings = serverSettings;
            Restart1 = new ServerRestart(false,new SimpleTime(0,0));
            Restart2 = new ServerRestart(false,new SimpleTime(6,0));
            Restart3 = new ServerRestart(false,new SimpleTime(12,0));
            Restart4 = new ServerRestart(false,new SimpleTime(18,0));

        }

        /// <summary>
        /// Constructor for xml deserializer
        /// </summary>
        public Server() {}

        public void CreateBackup()
        {
            backupServerSettings = new ServerSettings(ServerSettings);
            backupJavaSettings = new ServerJavaSettings(JavaSettings);
            backupRestart1 = new ServerRestart(Restart1);
            backupRestart2 = new ServerRestart(Restart2);
            backupRestart3 = new ServerRestart(Restart3);
            backupRestart4 = new ServerRestart(Restart4);
        }

        public bool ApplyBackup()
        {
            if (backupServerSettings == null)
            {
                return false;
            }
            ServerSettings = backupServerSettings;
            JavaSettings = backupJavaSettings;
            Restart1 = backupRestart1;
            Restart2 = backupRestart2;
            Restart3 = backupRestart3;
            Restart4 = backupRestart4;
            return true;
        }

        public override string ToString()
        {
            return Name + "("+Version.Version+")";
        }
    }
}
