using System;
using System.Xml.Serialization;

namespace Cchbc.Weather
{
	public sealed class LastUpdate
	{
		[XmlAttribute("value")]
		public DateTime Value { get; set; }
	}
}
