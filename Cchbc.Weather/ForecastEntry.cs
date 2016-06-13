using System;
using System.Xml.Serialization;

namespace Cchbc.Weather
{
    public sealed class ForecastEntry
    {
        [XmlAttribute("from")]
        public DateTime From { get; set; }

        [XmlAttribute("to")]
        public DateTime To { get; set; }

        [XmlAttribute("day")]
        public DateTime Day { get; set; }

        [XmlElement("symbol")]
        public Symbol Symbol { get; set; }

        [XmlElement("precipitation")]
        public ForecastPrecipitation Precipitation { get; set; }

        [XmlElement("windDirection")]
        public WindDirection WindDirection { get; set; }

        [XmlElement("windSpeed")]
        public WindSpeed WindSpeed { get; set; }

        [XmlElement("temperature")]
        public Temperature Temperature { get; set; }

        [XmlElement("pressure")]
        public Pressure Pressure { get; set; }

        [XmlElement("humidity")]
        public Humidity Humidity { get; set; }

        [XmlElement("clouds")]
        public ForecastClouds Clouds { get; set; }
    }
}
