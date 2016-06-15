using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Cchbc.Weather
{
    public sealed class MsnWeather
    {
        public MsnCurrentWeather CurrentWeather { get; }
        public List<MsnForecastWeather> Forecasts { get; }

        public MsnWeather(MsnCurrentWeather currentWeather, List<MsnForecastWeather> forecasts)
        {
            if (currentWeather == null) throw new ArgumentNullException(nameof(currentWeather));
            if (forecasts == null) throw new ArgumentNullException(nameof(forecasts));

            this.CurrentWeather = currentWeather;
            this.Forecasts = forecasts;
        }

        public static async Task<List<MsnWeather>> GetWeatherAsync(MsnCityLocation cityLocation)
        {
            if (cityLocation == null) throw new ArgumentNullException(nameof(cityLocation));

            var latitude = cityLocation.Latitude.ToString(CultureInfo.InvariantCulture);
            var longitude = cityLocation.Longitude.ToString(CultureInfo.InvariantCulture);
            var uri = new Uri($@"http://weather.service.msn.com/data.aspx?weasearchstr={latitude},{longitude}&weadegreetype=C&src=msn");

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

        private static List<MsnWeather> Parse(XDocument document)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));

            var weathers = new List<MsnWeather>();

            foreach (var d in document.Descendants(XName.Get(@"weather")))
            {
                var current = ParseCurrent(d);

                var forecast = new List<MsnForecastWeather>(8);
                foreach (var f in d.Descendants(XName.Get(@"forecast")))
                {
                    forecast.Add(ParseForecast(f));
                }

                weathers.Add(new MsnWeather(current, forecast));
            }

            return weathers;
        }

        private static MsnCurrentWeather ParseCurrent(XElement input)
        {
            var current = new MsnCurrentWeather();

            foreach (var element in input.Descendants(XName.Get(@"current")))
            {
                foreach (var attribute in element.Attributes())
                {
                    var value = attribute.Value.Trim();

                    var name = attribute.Name.LocalName;
                    if (name.Equals(@"temperature", StringComparison.OrdinalIgnoreCase))
                    {
                        current.Temperature = GetDouble(value);
                        continue;
                    }
                    if (name.Equals(@"skycode", StringComparison.OrdinalIgnoreCase))
                    {
                        current.IconCode = GetIconCode(value);
                        continue;
                    }
                    if (name.Equals(@"skytext", StringComparison.OrdinalIgnoreCase))
                    {
                        current.Description = value;
                        continue;
                    }
                    if (name.Equals(@"date", StringComparison.OrdinalIgnoreCase))
                    {
                        current.Date = GetDate(value);
                        continue;
                    }
                    if (name.Equals(@"observationtime", StringComparison.OrdinalIgnoreCase))
                    {
                        current.Date = current.Date.Add(GetTimeSpan(value));
                        continue;
                    }
                    if (name.Equals(@"observationpoint", StringComparison.OrdinalIgnoreCase))
                    {
                        current.Point = value;
                        continue;
                    }
                    if (name.Equals(@"feelslike", StringComparison.OrdinalIgnoreCase))
                    {
                        current.Feelslike = GetDouble(value);
                        continue;
                    }
                    if (name.Equals(@"humidity", StringComparison.OrdinalIgnoreCase))
                    {
                        current.Humidity = GetDouble(value);
                    }
                }
                break;
            }


            return current;
        }

        private static MsnForecastWeather ParseForecast(XElement input)
        {
            var forecast = new MsnForecastWeather();

            foreach (var attribute in input.Attributes())
            {
                var value = (attribute.Value).Trim();

                var name = attribute.Name.LocalName;
                if (name.Equals(@"low", StringComparison.OrdinalIgnoreCase))
                {
                    forecast.Low = GetDouble(value);
                    continue;
                }
                if (name.Equals(@"high", StringComparison.OrdinalIgnoreCase))
                {
                    forecast.High = GetDouble(value);
                    continue;
                }
                if (name.Equals(@"skycodeday", StringComparison.OrdinalIgnoreCase))
                {
                    forecast.IconCode = GetIconCode(value);
                    continue;
                }
                if (name.Equals(@"skytextday", StringComparison.OrdinalIgnoreCase))
                {
                    forecast.Description = value;
                    continue;
                }
                if (name.Equals(@"date", StringComparison.OrdinalIgnoreCase))
                {
                    forecast.Date = GetDate(value);
                    continue;
                }
                if (name.Equals(@"precip", StringComparison.OrdinalIgnoreCase))
                {
                    forecast.Precipitation = GetDouble(value);
                }
            }

            return forecast;
        }

        private static TimeSpan GetTimeSpan(string input)
        {
            if (input != string.Empty)
            {
                TimeSpan value;
                if (TimeSpan.TryParse(input, out value))
                {
                    return value;
                }
            }

            return TimeSpan.Zero;
        }

        private static DateTime GetDate(string input)
        {
            if (input != string.Empty)
            {
                DateTime value;
                if (DateTime.TryParse(input, out value))
                {
                    return value;
                }
            }

            return DateTime.Today;
        }

        private static double GetDouble(string input)
        {
            if (input != string.Empty)
            {
                double number;
                if (double.TryParse(input, out number))
                {
                    return number;
                }
            }

            return 0;
        }

        private static string GetIconCode(string input)
        {
            if (input != string.Empty)
            {
                int number;
                if (int.TryParse(input, out number))
                {
                    foreach (var _ in new[] { 0, 1, 2, 3, 4, 17, 35, 37, 38, 44, 47 })
                    {
                        if (_ == number)
                        {
                            return @"11x";
                        }
                    }
                    foreach (var _ in new[] { 5, 6, 7, 8, 9, 13, 14, 16, 42, 43, 15, 25, 41, 46 })
                    {
                        if (_ == number)
                        {
                            return @"13x";
                        }
                    }
                    foreach (var _ in new[] { 10, 11, 12, 18, 40 })
                    {
                        if (_ == number)
                        {
                            return @"13x";
                        }
                    }
                    foreach (var _ in new[] { 39, 45 })
                    {
                        if (_ == number)
                        {
                            return @"10x";
                        }
                    }
                    foreach (var _ in new[] { 31, 32, 36 })
                    {
                        if (_ == number)
                        {
                            return @"01x";
                        }
                    }
                    foreach (var _ in new[] { 19, 20, 21, 22, 26 })
                    {
                        if (_ == number)
                        {
                            return @"03x";
                        }
                    }
                    foreach (var _ in new[] { 23, 24 })
                    {
                        if (_ == number)
                        {
                            return @"04x";
                        }
                    }
                    foreach (var _ in new[] { 27, 28, 29, 30, 33, 34 })
                    {
                        if (_ == number)
                        {
                            return @"02x";
                        }
                    }
                }
            }

            return string.Empty;
        }
    }

    public sealed class MsnCityLocation
    {
        public double Latitude { get; }
        public double Longitude { get; }

        public MsnCityLocation(double latitude, double longitude)
        {
            this.Longitude = longitude;
            this.Latitude = latitude;
        }
    }

    public sealed class MsnCurrentWeather
    {
        public double Temperature { get; set; }
        public string IconCode { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.Today;
        public string Point { get; set; } = string.Empty;
        public double Feelslike { get; set; }
        public double Humidity { get; set; }
    }

    public sealed class MsnForecastWeather
    {
        public double Low { get; set; }
        public double High { get; set; }
        public string IconCode { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.Today;
        public double Precipitation { get; set; }
    }
}