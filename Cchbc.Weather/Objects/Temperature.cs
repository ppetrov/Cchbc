using System.Xml.Serialization;

namespace Cchbc.Weather.Objects
{
	public sealed class Temperature
	{
		[XmlAttribute("value")]
		public double Value { get; set; }

		[XmlAttribute("min")]
		public double Min { get; set; }

		[XmlAttribute("max")]
		public double Max { get; set; }

		[XmlAttribute("unit")]
		public string Unit { get; set; }
	}
}
