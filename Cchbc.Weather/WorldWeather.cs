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
    public sealed class WorldWeather
    {
        public WorldCurrentWeather CurrentWeather { get; }
        public List<WorldForecastWeather> Forecasts { get; }

        public WorldWeather(WorldCurrentWeather currentWeather, List<WorldForecastWeather> forecasts)
        {
            //if (currentWeather == null) throw new ArgumentNullException(nameof(currentWeather));
            //if (forecasts == null) throw new ArgumentNullException(nameof(forecasts));

            this.CurrentWeather = currentWeather;
            this.Forecasts = forecasts;
        }

        public static async Task<List<WorldWeather>> GetWeatherAsync(string appKey, WorldCityLocation cityLocation, int days = 14)
        {
            if (appKey == null) throw new ArgumentNullException(nameof(appKey));
            if (cityLocation == null) throw new ArgumentNullException(nameof(cityLocation));

            var latitude = cityLocation.Latitude.ToString(CultureInfo.InvariantCulture);
            var longitude = cityLocation.Longitude.ToString(CultureInfo.InvariantCulture);
            var uri = new Uri($@"http://api.worldweatheronline.com/premium/v1/weather.ashx?key={appKey}&q={latitude},{longitude}&format=xml&num_of_days={days}");

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

        private static List<WorldWeather> Parse(XDocument document)
        {
            var current = ParseCurrent(document);

            var today = DateTime.Today;
            var currentTime = DateTime.Now;
            var forecast = new List<WorldForecastWeather>(16);

            foreach (var d in document.Descendants(XName.Get(@"weather")))
            {
                var date = DateTime.Parse(d.Descendants(@"date").Single().Value);
                var min = double.Parse(d.Descendants(@"mintempC").Single().Value);
                var max = double.Parse(d.Descendants(@"maxtempC").Single().Value);

                var forecastOffset = int.MaxValue;
                var bestForcast = default(WorldForecastHourly);

                foreach (var byHours in d.Descendants(@"hourly"))
                {
                    int number;
                    int.TryParse(byHours.Descendants(@"time").Single().Value, out number);

                    var hours = number / 100;
                    var offset = Convert.ToInt32(Math.Abs((currentTime - today.AddHours(hours)).TotalSeconds));
                    if (offset < forecastOffset)
                    {
                        forecastOffset = offset;

                        var iconUrl = byHours.Descendants(@"weatherIconUrl").Single();
                        var description = byHours.Descendants(@"weatherDesc").Single();
                        var precipitation = byHours.Descendants(@"precipMM").Single();
                        bestForcast = new WorldForecastHourly
                        {
                            Date = currentTime,
                            IconUrl = iconUrl.Value,
                            Description = description.Value,
                            Precipitation = double.Parse(precipitation.Value)
                        };
                    }
                }

                if (bestForcast != null)
                {
                    forecast.Add(new WorldForecastWeather
                    {
                        Low = min,
                        High = max,
                        IconCode = ParseIcon(bestForcast.IconUrl),
                        Description = bestForcast.Description,
                        Date = date,
                        Precipitation = bestForcast.Precipitation,
                    });
                }
            }

            return new List<WorldWeather> { new WorldWeather(current, forecast) };
        }

        private static WorldCurrentWeather ParseCurrent(XDocument document)
        {
            var current = document.Descendants(@"current_condition").Single();

            return new WorldCurrentWeather
            {
                Temperature = double.Parse(current.Descendants(@"temp_C").Single().Value),
                IconCode = ParseIcon(current.Descendants(@"weatherIconUrl").Single().Value),
                Description = current.Descendants(@"weatherDesc").Single().Value,
                Date = DateTime.Parse(current.Descendants(@"observation_time").Single().Value),
                Point = document.Descendants(@"request").Single().Descendants(@"query").Single().Value,
                Feelslike = double.Parse(current.Descendants(@"FeelsLikeC").Single().Value),
                Humidity = double.Parse(current.Descendants(@"humidity").Single().Value)
            };
        }

        private static string ParseIcon(string iconUrl)
        {
            var lookup = new Dictionary<string, string>
            {
                {@"wsymbol_0001_sunny.png", @"01x"},
                {@"wsymbol_0002_sunny_intervals.png", @"02x"},
                {@"wsymbol_0003_white_cloud.png", @"03x"},
                {@"wsymbol_0004_black_low_cloud.png", @"04x"},
                {@"wsymbol_0006_mist.png", @"03x"},
                {@"wsymbol_0007_fog.png", @"03x"},
                {@"wsymbol_0009_light_rain_showers.png", @"10x"},
                {@"wsymbol_0010_heavy_rain_showers.png", @"10x"},
                {@"wsymbol_0011_light_snow_showers.png", @"13x"},
                {@"wsymbol_0012_heavy_snow_showers.png", @"13x"},
                {@"wsymbol_0013_sleet_showers.png", @"10x"},
                {@"wsymbol_0016_thundery_showers.png", @"11x"},
                {@"wsymbol_0017_cloudy_with_light_rain.png", @"09x"},
                {@"wsymbol_0018_cloudy_with_heavy_rain.png", @"09x"},
                {@"wsymbol_0019_cloudy_with_light_snow.png", @"13x"},
                {@"wsymbol_0020_cloudy_with_heavy_snow.png", @"13x"},
                {@"wsymbol_0021_cloudy_with_sleet.png", @"09x"},
                {@"wsymbol_0024_thunderstorms.png", @"11x"},
            };

            var name = Path.GetFileName(iconUrl);
            string value;
            lookup.TryGetValue(name, out value);

            return value ?? string.Empty;
        }
    }

    public sealed class WorldCityLocation
    {
        public double Latitude { get; }
        public double Longitude { get; }

        public WorldCityLocation(double latitude, double longitude)
        {
            this.Longitude = longitude;
            this.Latitude = latitude;
        }
    }

    public sealed class WorldCurrentWeather
    {
        public double Temperature { get; set; }
        public string IconCode { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.Today;
        public string Point { get; set; } = string.Empty;
        public double Feelslike { get; set; }
        public double Humidity { get; set; }
    }

    public sealed class WorldForecastWeather
    {
        public double Low { get; set; }
        public double High { get; set; }
        public string IconCode { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.Today;
        public double Precipitation { get; set; }
    }

    public sealed class WorldForecastHourly
    {
        public DateTime Date { get; set; }
        public string IconUrl { get; set; }
        public string Description { get; set; }
        public double Precipitation { get; set; }
    }
}