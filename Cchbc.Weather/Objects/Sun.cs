using System;
using System.Xml.Serialization;

namespace Cchbc.Weather.Objects
{
	public sealed class Sun
	{
		[XmlAttribute("rise")]
		public DateTime Rise { get; set; }

		[XmlAttribute("set")]
		public DateTime Set { get; set; }
	}
}
