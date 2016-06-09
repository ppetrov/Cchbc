using System.Xml.Serialization;

namespace Cchbc.Weather.Objects
{
	[XmlRoot("weatherdata", Namespace = "")]
	public class ForecastData
    {
        [XmlElement("location")]
        public Location Location { get; set; }

        [XmlElement("meta")]
        public Meta Meta { get; set; }

        [XmlElement("sun")]
        public Sun Sun { get; set; }

        [XmlArray("forecast")]
        [XmlArrayItem("time", Type = typeof(ForecastTime))]
        public ForecastTime[] Forecast { get; set; }
    }
}
