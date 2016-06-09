using System;
using System.Threading.Tasks;
using Cchbc.Weather.Objects;

namespace Cchbc.Weather
{
	public sealed class ForecastClient
	{
		public WeatherClient Client { get; }

		public ForecastClient(string appId)
		{
			if (appId == null) throw new ArgumentNullException(nameof(appId));

			this.Client = new WeatherClient(appId, @"forecast");
		}

		public Task<ForecastData> GetByNameAsync(string cityName, bool daily = false, MetricSystem metric = MetricSystem.Internal, Language language = Language.EN, int? count = null)
		{
			return this.Client.GetByNameAsync<ForecastData>(cityName, metric, language, count, daily);
		}

		public Task<ForecastData> GetByCoordinatesAsync(Coordinates coordinates, bool daily = false, MetricSystem metric = MetricSystem.Internal, Language language = Language.EN, int? count = null)
		{
			return this.Client.GetByCoordinatesAsync<ForecastData>(coordinates, metric, language, count, daily);
		}

		public Task<ForecastData> GetByCityIdAsync(int cityId, bool daily = false, MetricSystem metric = MetricSystem.Internal, Language language = Language.EN, int? count = null)
		{
			return this.Client.GetByCityIdAsync<ForecastData>(cityId, metric, language, count, daily);
		}
	}
}