using System.Xml.Serialization;

namespace Cchbc.Weather.Objects
{
	public sealed class Location
	{
		[XmlElement("name")]
		public string Name { get; set; }

		[XmlElement("type")]
		public string Type { get; set; }

		[XmlElement("country")]
		public string Country { get; set; }

		[XmlElement("timezone")]
		public string TimeZone { get; set; }

		[XmlElement("location")]
		public CityLocation CityLocation { get; set; }
	}
}
