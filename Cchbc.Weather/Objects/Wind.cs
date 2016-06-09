using System.Xml.Serialization;

namespace Cchbc.Weather.Objects
{
	public class Wind
	{
		[XmlElement("speed")]
		public Speed Speed { get; set; }

		[XmlElement("direction")]
		public Direction Direction { get; set; }
	}
}
