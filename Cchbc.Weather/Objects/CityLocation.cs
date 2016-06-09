using System.Xml.Serialization;

namespace Cchbc.Weather.Objects
{
	public sealed class CityLocation
	{
		[XmlAttribute("altitude")]
		public double Altitude { get; set; }

		[XmlAttribute("latitude")]
		public double Latitude { get; set; }

		[XmlAttribute("longitude")]
		public double Longitude { get; set; }

		[XmlAttribute("geobase")]
		public string GeoBase { get; set; }

		[XmlAttribute("geobaseid")]
		public int GeoBaseId { get; set; }
	}
}
