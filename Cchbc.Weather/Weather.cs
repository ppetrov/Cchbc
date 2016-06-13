using System.Xml.Serialization;

namespace Cchbc.Weather
{
	public sealed class Weather
	{
		[XmlAttribute("number")]
		public int Number { get; set; }

		[XmlAttribute("value")]
		public string Value { get; set; }

		[XmlAttribute("icon")]
		public string Icon { get; set; }
	}
}
