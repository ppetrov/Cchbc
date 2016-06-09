using System.Xml.Serialization;

namespace Cchbc.Weather.Objects
{
    public sealed class WindSpeed
    {
        [XmlAttribute("mps")]
        public double Mps { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }
    }
}
