﻿using System.Xml.Serialization;

namespace Cchbc.Weather
{
	public sealed class Speed
	{
		[XmlAttribute("value")]
		public double Value { get; set; }

		[XmlAttribute("name")]
		public string Name { get; set; }
	}
}