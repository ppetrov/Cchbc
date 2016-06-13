using System.Xml.Serialization;

namespace Cchbc.Weather
{
	public sealed class Pressure
	{
		[XmlAttribute("value")]
		public double Value { get; set; }

		[XmlAttribute("unit")]
		public string Unit { get; set; }
	}
}
