using System;
using System.Threading.Tasks;

namespace Cchbc.Weather
{
	public sealed class CurrentWeatherClient
	{
		private WeatherClient Client { get; }

		public CurrentWeatherClient(string appId)
		{
			if (appId == null) throw new ArgumentNullException(nameof(appId));

			this.Client = new WeatherClient(appId, @"weather");
		}

		public Task<WeatherData> GetByNameAsync(string cityName, MetricSystem metric = MetricSystem.Internal, Language language = Language.EN)
		{
			return this.Client.GetByNameAsync<WeatherData>(cityName, metric, language);
		}

		public Task<WeatherData> GetByCoordinatesAsync(Coordinates coordinates, MetricSystem metric = MetricSystem.Internal, Language language = Language.EN)
		{
			return this.Client.GetByCoordinatesAsync<WeatherData>(coordinates, metric, language);
		}

		public Task<WeatherData> GetByCityIdAsync(int cityId, MetricSystem metric = MetricSystem.Internal, Language language = Language.EN)
		{
			return this.Client.GetByCityIdAsync<WeatherData>(cityId, metric, language);
		}
	}
}