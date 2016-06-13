using System.Xml.Serialization;

namespace Cchbc.Weather
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
        [XmlArrayItem("time", Type = typeof(ForecastEntry))]
        public ForecastEntry[] Forecast { get; set; }
    }
}
