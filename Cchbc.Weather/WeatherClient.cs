using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Cchbc.Weather
{
	public sealed class WeatherClient
	{
		private string Module { get; }
		private Dictionary<string, string> Parameters { get; }

		public WeatherClient(string appId, string module)
		{
			if (appId == null) throw new ArgumentNullException(nameof(appId));
			if (module == null) throw new ArgumentNullException(nameof(module));

			this.Module = module;
			this.Parameters = new Dictionary<string, string> { { @"APPID", appId } };
		}

		public Task<T> GetByNameAsync<T>(string cityName, MetricSystem metric, Language language, int? count = null, bool daily = false)
		{
			if (cityName == null) throw new ArgumentNullException(nameof(cityName));

			this.Parameters.Add(@"q", Uri.EscapeDataString(cityName));

			AddParameters(metric, language, count);

			return this.GetRequestResultAsync<T>(daily);
		}

		public Task<T> GetByCoordinatesAsync<T>(Coordinates coordinates, MetricSystem metric, Language language, int? count = null, bool daily = false)
		{
			if (coordinates == null) throw new ArgumentNullException(nameof(coordinates));

			this.Parameters.Add(@"lat", coordinates.Latitude.ToString(CultureInfo.InvariantCulture));
			this.Parameters.Add(@"lon", coordinates.Longitude.ToString(CultureInfo.InvariantCulture));

			AddParameters(metric, language, count);

			return this.GetRequestResultAsync<T>(daily);
		}

		public Task<T> GetByCityIdAsync<T>(int cityId, MetricSystem metric, Language language, int? count = null, bool daily = false)
		{
			this.Parameters.Add(@"id", cityId.ToString(CultureInfo.InvariantCulture));

			AddParameters(metric, language, count);

			return this.GetRequestResultAsync<T>(daily);
		}

		private async Task<T> GetRequestResultAsync<T>(bool daily)
		{
			var uri = new Uri(@"http://api.openweathermap.org/data/2.5/" + this.Module);
			if (daily)
			{
				uri = new Uri(uri.OriginalString + @"/daily");
			}

			this.Parameters.Add(@"mode", @"xml");

			var buffer = new StringBuilder();

			foreach (var p in this.Parameters)
			{
				if (buffer.Length > 0)
				{
					buffer.Append('&');
				}
				buffer.Append(Uri.EscapeUriString(p.Key));
				buffer.Append('=');
				buffer.Append(Uri.EscapeUriString(p.Value));
			}

			using (var request = new HttpRequestMessage(HttpMethod.Get, new UriBuilder(uri) { Query = buffer.ToString() }.Uri))
			{
				using (var httpClient = new HttpClient(new HttpClientHandler()))
				{
					using (var response = await httpClient.SendAsync(request))
					{
						if (!response.IsSuccessStatusCode)
						{
							throw new Exception(response.ToString());
						}
						using (var s = await response.Content.ReadAsStreamAsync())
						{
							using (var r = XmlReader.Create(s))
							{
								return (T)new XmlSerializer(typeof(T)).Deserialize(r);
							}
						}
					}
				}
			}
		}

		private void AddParameters(MetricSystem metric, Language language, int? count)
		{
			if (metric != MetricSystem.Internal)
			{
				this.Parameters.Add(@"units", metric.ToString().ToLowerInvariant());
			}
			if (language != Language.EN)
			{
				this.Parameters.Add(@"lang", language.ToString().ToLowerInvariant());
			}
			if (count.HasValue)
			{
				this.Parameters.Add(@"cnt", count.Value.ToString(CultureInfo.InvariantCulture));
			}
		}
	}
}