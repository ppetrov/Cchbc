using System.Xml.Serialization;

namespace Cchbc.Weather.Objects
{
	public sealed class Coordinates
	{
		[XmlAttribute("lat")]
		public double Latitude { get; }

		[XmlAttribute("lon")]
		public double Longitude { get; }

		public Coordinates(double latitude, double longitude)
		{
			this.Longitude = longitude;
			this.Latitude = latitude;
		}
	}
}
