using System;
using System.Xml.Serialization;

namespace Cchbc.Weather.Objects
{
	public sealed class LastUpdate
	{
		[XmlAttribute("value")]
		public DateTime Value { get; set; }
	}
}
