using System.Xml.Serialization;

namespace AusGovLabellingAgent
{
    [XmlRoot]
    public class Classification
    {
        [XmlAttribute]
        public string Key { get; set; } = null;
        [XmlAttribute]
        public string Shortcut { get; set; } = null;
        [XmlElement]
        public ProtectiveMarking Marking { get; set; } = null;
        [XmlElement]
        public string MsipLabel { get; set; } = null;

        public Classification() { }

        public Classification(string key, string shortcut, ProtectiveMarking marking, string msipLabel)
        {
            Key         = key;
            Shortcut    = shortcut;
            Marking     = marking;
            MsipLabel   = msipLabel;
        }
    }
}
