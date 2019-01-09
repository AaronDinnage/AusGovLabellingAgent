using System.Xml.Serialization;

namespace AusGovLabellingAgent
{
    [XmlRoot]
    public class StringPair
    {
        [XmlAttribute]
        public string Key { get; set; }
        [XmlAttribute]
        public string Value { get; set; }
    }
}
