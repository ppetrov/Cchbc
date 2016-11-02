using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Cchbc.Weather
{
	public sealed class ForecaWeather
	{
		public ForecaCurrentWeather CurrentWeather { get; }
		public List<ForecaForecastWeather> Forecasts { get; }

		public ForecaWeather(ForecaCurrentWeather currentWeather, List<ForecaForecastWeather> forecasts)
		{
			if (currentWeather == null) throw new ArgumentNullException(nameof(currentWeather));
			if (forecasts == null) throw new ArgumentNullException(nameof(forecasts));

			this.CurrentWeather = currentWeather;
			this.Forecasts = forecasts;
		}

		public static async Task<List<ForecaWeather>> GetWeatherAsync(ForecaCityLocation cityLocation, int days = 14)
		{
			if (cityLocation == null) throw new ArgumentNullException(nameof(cityLocation));

			var latitude = Math.Round(cityLocation.Latitude, 2).ToString(CultureInfo.InvariantCulture);
			var longitude = Math.Round(cityLocation.Longitude, 2).ToString(CultureInfo.InvariantCulture);
			var uri = new Uri($@"http://feed.foreca.com/cocacola-aug16gr/cocacola-data.php?lon={longitude}&lat={latitude}&products=daily");

			using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
			{
				using (var httpClient = new HttpClient(new HttpClientHandler()))
				{
					using (var response = await httpClient.SendAsync(request))
					{
						var content = await response.Content.ReadAsStringAsync();
						using (var s = new StringReader(content))
						{
							return Parse(XDocument.Load(s));
						}
					}
				}
			}
		}

		private static List<ForecaWeather> Parse(XDocument document)
		{
			var current = ParseCurrent(document.Descendants(XName.Get(@"loc")).Single());
			var forecast = new List<ForecaForecastWeather>(16);

			foreach (var weather in document.Descendants(XName.Get(@"weather")))
			{
				foreach (var forecastNode in weather.Descendants(XName.Get(@"fc")))
				{
					var item = new ForecaForecastWeather();

					item.Low = GetDoubleValue(forecastNode.Attribute(XName.Get(@"tn")).Value);
					item.High = GetDoubleValue(forecastNode.Attribute(XName.Get(@"tx")).Value);
					item.IconCode = ParseIcon(forecastNode.Attribute(XName.Get(@"s")).Value);
					item.Date = DateTime.Parse(forecastNode.Attribute(XName.Get(@"dt")).Value);
					item.Description = forecastNode.Attribute(XName.Get(@"sT")).Value;

					forecast.Add(item);
				}
			}

			return new List<ForecaWeather> { new ForecaWeather(current, forecast) };
		}

		private static ForecaCurrentWeather ParseCurrent(XElement root)
		{
			var current = new ForecaCurrentWeather();
			current.LocationId = root.Attribute(XName.Get(@"id")).Value;
			var city = root.Attribute(XName.Get(@"name")).Value;
			var country = root.Attribute(XName.Get(@"country")).Value;			
			current.Point = city + @", " + country;

			var currentValues = root.Descendants(XName.Get(@"cc")).SingleOrDefault();
			if (currentValues != null)
			{
				current.Date = DateTime.Parse(currentValues.Attribute(XName.Get(@"dt")).Value);
				current.Temperature = GetDoubleValue(currentValues.Attribute(XName.Get(@"t")).Value);
				current.IconCode = ParseIcon(currentValues.Attribute(XName.Get(@"s")).Value);
				current.Description = currentValues.Attribute(XName.Get(@"station")).Value;
				current.Feelslike = GetDoubleValue(currentValues.Attribute(XName.Get(@"tf")).Value);
				current.Humidity = GetDoubleValue(currentValues.Attribute(XName.Get(@"rh")).Value);
			}

			return current;
		}

		private static double GetDoubleValue(string input)
		{
			var value = (input ?? string.Empty).Trim();
			if (value != string.Empty)
			{
				double number;
				if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out number))
				{
					return number;
				}
			}

			return 0;
		}

		private static string ParseIcon(string code)
		{
			var map = new Dictionary<string, string>(32)
			{
				{@"d000", @"01x"},
				{@"d100", @"02x"},
				{@"d200", @"02x"},
				{@"d300", @"02x"},
				{@"d500", @"02x"},
				{@"d400", @"03x"},
				{@"d600", @"04x"},
				{@"d410", @"09x"},
				{@"d420", @"09x"},
				{@"d430", @"09x"},
				{@"d210", @"10x"},
				{@"d220", @"10x"},
				{@"d240", @"10x"},
				{@"d310", @"10x"},
				{@"d320", @"10x"},
				{@"d340", @"10x"},
				{@"d440", @"11x"},
				{@"d211", @"13x"},
				{@"d212", @"13x"},
				{@"d221", @"13x"},
				{@"d222", @"13x"},
				{@"d311", @"13x"},
				{@"d312", @"13x"},
				{@"d321", @"13x"},
				{@"d322", @"13x"},
				{@"d411", @"13x"},
				{@"d412", @"13x"},
				{@"d421", @"13x"},
				{@"d422", @"13x"},
				{@"d431", @"13x"},
				{@"d432", @"13x"}
			};

			var name = Path.GetFileName(code);
			string value;
			map.TryGetValue(name, out value);

			return value ?? string.Empty;
		}
	}

	public sealed class ForecaCityLocation
	{
		public double Latitude { get; }
		public double Longitude { get; }

		public ForecaCityLocation(double latitude, double longitude)
		{
			this.Longitude = longitude;
			this.Latitude = latitude;
		}
	}

	public sealed class ForecaCurrentWeather
	{
		public string LocationId { get; set; }
		public double Temperature { get; set; }
		public string IconCode { get; set; }
		public string Description { get; set; } = string.Empty;
		public DateTime Date { get; set; } = DateTime.Today;
		public string Point { get; set; } = string.Empty;
		public double Feelslike { get; set; }
		public double Humidity { get; set; }
	}

	public sealed class ForecaForecastWeather
	{
		public double Low { get; set; }
		public double High { get; set; }
		public string IconCode { get; set; }
		public string Description { get; set; } = string.Empty;
		public DateTime Date { get; set; } = DateTime.Today;
		public double Precipitation { get; set; }
	}

	public sealed class ForecaForecastHourly
	{
		public DateTime Date { get; set; }
		public string IconUrl { get; set; }
		public string Description { get; set; }
		public double Precipitation { get; set; }
	}
}