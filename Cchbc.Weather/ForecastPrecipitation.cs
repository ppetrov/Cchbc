using System.Xml.Serialization;

namespace Cchbc.Weather
{
    public sealed class ForecastPrecipitation
    {
        [XmlAttribute("value")]
        public double Value { get; set; }

        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlAttribute("unit")]
        public string Unit { get; set; }
    }
}
