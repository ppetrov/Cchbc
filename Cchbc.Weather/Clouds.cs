﻿using System.Xml.Serialization;

namespace Cchbc.Weather
{
	public sealed class Clouds
	{
		[XmlAttribute("value")]
		public int Value { get; set; }

		[XmlAttribute("name")]
		public string Name { get; set; }
	}
}