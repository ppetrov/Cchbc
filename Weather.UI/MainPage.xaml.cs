using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Cchbc.Weather;

namespace Weather.UI
{
	public sealed partial class MainPage
	{
		public WeatherViewModel ViewModel { get; } = new WeatherViewModel();

		public MainPage()
		{
			this.InitializeComponent();
		}

		private async void MainPage_OnLoaded(object sender, RoutedEventArgs e)
		{
			try
			{
				await this.ViewModel.LoadAsync();
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.ToString());
			}
		}
	}

	public sealed class WeatherViewModel : INotifyPropertyChanged
	{

		public ObservableCollection<WeatherForcastDay> ForcastDays { get; } = new ObservableCollection<WeatherForcastDay>();

		private string _currentStep = string.Empty;
		public string CurrentStep
		{
			get { return _currentStep; }
			set
			{
				_currentStep = value;
				this.OnPropertyChanged();
			}
		}

		public async Task LoadAsync()
		{
			this.ForcastDays.Clear();

			var iconsMap = new Dictionary<string, string>
			{
				{@"01d", @"clear sky"},
				{@"02d", @"few clouds"},
				{@"03d", @"clouds"},
				{@"04d", @"clouds"},
				{@"09d", @"shower rain"},
				{@"10d", @"rain"},
				{@"11d", @"thunderstorm"},
				{@"13d", @"snow"},
			};

			this.CurrentStep = @"Calling the weather service ...";

			var client = new ForecastClient(@"87f3b3402228bff038c4f69cbeebb484");
			var forecastData = await client.GetByCoordinatesAsync(new Coordinates(42.7, 23.33), true, MetricSystem.Metric, count: 7);

			this.CurrentStep = @"Weather data downloaded";

			this.CurrentStep = @"Parse data & load images";
			foreach (var entry in forecastData.Forecast)
			{
				var clouds = entry.Clouds;
				var symbol = entry.Symbol;

				var src = new Uri(string.Format("ms-appx:///Assets/{0}.png", iconsMap[symbol.Var]));
				var file = await StorageFile.GetFileFromApplicationUriAsync(src);

				using (var ms = new InMemoryRandomAccessStream())
				{
					using (var s = await file.OpenStreamForReadAsync())
					{
						int readedBytes;
						var buffer = new byte[64 * 1024];
						while ((readedBytes = await s.ReadAsync(buffer, 0, buffer.Length)) != 0)
						{
							var copy = new byte[readedBytes];
							Array.Copy(buffer, 0, copy, 0, readedBytes);
							await ms.WriteAsync(copy.AsBuffer());
						}

						ms.Seek(0);

						var image = new BitmapImage();
						await image.SetSourceAsync(ms);

						this.ForcastDays.Add(new WeatherForcastDay(entry.Day, image, entry.Temperature, clouds.Value, clouds.All + @" " + clouds.Unit));
					}
				}
			}

			this.CurrentStep = @"Done";
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	public sealed class WeatherForcastDay
	{
		public string Date { get; }
		public string Day { get; }
		public ImageSource Source { get; }
		public string Temperature { get; }
		public double Min { get; }
		public double Max { get; }
		public string Description { get; }
		public string ChanceOfRain { get; }

		public WeatherForcastDay(DateTime date, ImageSource source, Temperature temperature, string description, string chanceOfRain)
		{
			if (date == null) throw new ArgumentNullException(nameof(date));
			if (temperature == null) throw new ArgumentNullException(nameof(temperature));
			if (description == null) throw new ArgumentNullException(nameof(description));

			this.Date = date.ToString(@"MMMM dd");
			this.Day = date.ToString(@"dddd");
			this.Source = source;
			this.Min = Math.Round(temperature.Min, 0);
			this.Max = Math.Round(temperature.Max, 0);
			this.Temperature = this.Min + @" / " + this.Max;
			this.Description = description;
			this.ChanceOfRain = chanceOfRain;
		}
	}
}
