using System.Xml.Serialization;

namespace Cchbc.Weather
{
    public sealed class Meta
    {
        [XmlElement("lastupdate")]
        public string LastUpdate { get; set; }

        [XmlElement("calctime")]
        public double CalcTime { get; set; }

        [XmlElement("nextupdate")]
        public string NextUpdate { get; set; }
    }
}
