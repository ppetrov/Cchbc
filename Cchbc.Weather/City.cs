using System.Xml.Serialization;

namespace Cchbc.Weather
{
	public sealed class City
	{
		[XmlAttribute("id")]
		public int Id { get; set; }

		[XmlAttribute("name")]
		public string Name { get; set; }

		[XmlElement("coord")]
		public Coordinates Coordinates { get; set; }

		[XmlElement("sun")]
		public Sun Sun { get; set; }
	}
}
