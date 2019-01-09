using AusGovLabellingAgent.Data;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Serialization;

namespace AusGovLabellingAgent
{
    [XmlRoot]
    public class Config
    {
        [XmlElement]
        public bool AutoReload { get; set; }
        [XmlElement]
        public LogLevel AuditLevel { get; set; }
        [XmlElement]
        public double MaxLogMb { get; set; }
        [XmlElement]
        public string HeaderName { get; set; }
        [XmlElement]
        public string MsipHeaderName { get; set; }
        //public string MsipHeaderKeyRegex { get; set; }
        [XmlElement]
        public string MsipOwnerTag { get; set; }
        [XmlElement]
        public string MsipSetDateTag { get; set; }
        [XmlElement]
        public string MsipMethodTag { get; set; }
        [XmlElement]
        public string MsipSetDateFormat { get; set; }
        [XmlElement]
        public string MsipRegexOwner { get; set; }
        [XmlElement]
        public string MsipRegexSetDate { get; set; }
        [XmlElement]
        public string MsipRegexMethod { get; set; }
        [XmlElement]
        public string MsipBypassLabel { get; set; }
        [XmlElement]
        public string MsipMethodDefault { get; set; }
        [XmlElement]
        public string MsipMethodManual { get; set; }
        [XmlElement]
        public string MsipMethodAutomatic { get; set; }
        [XmlElement]
        public string MsipMethodMandatory { get; set; }
        // Note: Recommended method is not accommodated.

        [XmlElement]
        public string NdrDisplayName { get; set; }
        [XmlElement]
        public string NdrEmailAddress { get; set; }
        [XmlElement]
        public string NdrSubject { get; set; }
        [XmlElement]
        public bool NdrIncludeAttachment { get; set; }
        [XmlElement]
        public string SubjectRegex { get; set; }
        [XmlElement]
        public string HeaderRegex { get; set; }
        [XmlElement]
        public RegexOptions RegexOptions { get; set; }
        [XmlElement]
        public bool EnableShortcuts { get; set; }
        [XmlElement]
        public string DefaultOutboundLabel { get; set; }
        [XmlElement]
        public string DefaultInboundLabel { get; set; }
        [XmlElement]
        public bool BounceUnlabelledEmails { get; set; }
        [XmlArray]
        public Classification[] Classifications { get; set; }
        [XmlArray]
        public StringPair[] MandatoryClassifications { get; set; }


        //public string PickupPath { get; set; } = @"C:\Program Files\Microsoft\Exchange Server\TransportRoles\Pickup\";

        private static readonly XmlSerializer ConfigurationSerializer = new XmlSerializer(typeof(Config));

        public const string Filename = "LabellingAgentConfig.xml";
        public static string FilePath;

        private static FileSystemWatcher _configWatcher;
        private static int _configLoading = 0;

        public static Config Current = null;

        static Config()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string configurationDirectory = Path.GetDirectoryName(assembly.Location);

            FilePath = Path.Combine(configurationDirectory, Filename);

            Load();

            // Watch the configuration file for changes ...
            // TODO: FileSystemWatcher is IDisposable.
            _configWatcher = new FileSystemWatcher(configurationDirectory)
            {
                NotifyFilter    = NotifyFilters.LastWrite,
                Filter          = Filename,
            };

            _configWatcher.Changed += FileChanged;
        }

        public static void Load()
        {
            Logging.Log.Information("Config.Load()");

            if (Interlocked.CompareExchange(ref _configLoading, 1, 0) != 0)
                return;

            if (_configWatcher != null)
                _configWatcher.EnableRaisingEvents = false;

            Config newConfig;
            using (var stream = File.Open(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                newConfig = (Config)ConfigurationSerializer.Deserialize(stream);
            }

            Interlocked.Exchange(ref Current, newConfig);

            _configLoading = 0;

            if (_configWatcher != null)
                _configWatcher.EnableRaisingEvents = Current.AutoReload;
        }

        public static void Save()
        {
            Logging.Log.Debug("Config.Save()");

            if (_configWatcher != null)
                _configWatcher.EnableRaisingEvents = false;

            using (var stream = File.Create(FilePath))
                ConfigurationSerializer.Serialize(stream, Current);

            if (_configWatcher != null)
                _configWatcher.EnableRaisingEvents = Current.AutoReload;
        }

        private static void FileChanged(object sender, FileSystemEventArgs e)
        {
            Logging.Log.Information("Config.FileChanged()");

            Load();
        }
    }
}
