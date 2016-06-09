using System.Xml.Serialization;

namespace Cchbc.Weather.Objects
{
    public sealed class WindDirection
    {
        [XmlAttribute("deg")]
        public double Deg { get; set; }

        [XmlAttribute("code")]
        public string Code { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }
    }
}
