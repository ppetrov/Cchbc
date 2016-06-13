using System.Xml.Serialization;

namespace Cchbc.Weather
{
	public sealed class ForecastClouds
	{
		[XmlAttribute("value")]
		public string Value { get; set; }

		[XmlAttribute("all")]
		public int All { get; set; }

		[XmlAttribute("unit")]
		public string Unit { get; set; }
	}
}
