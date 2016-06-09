using System.Xml.Serialization;

namespace Cchbc.Weather.Objects
{
	public sealed class Humidity
	{
		[XmlAttribute("value")]
		public int Value { get; set; }

		[XmlAttribute("unit")]
		public string Unit { get; set; }
	}
}
