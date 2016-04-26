using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Cchbc.Features.Admin.FeatureDetailsModule;

namespace Cchbc.Features.Admin.UI
{
	public sealed class FeatureSortOrderConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			var sortOrder = value as FeatureSortOrder;

			if (sortOrder == FeatureSortOrder.Alphabetical)
			{
				return Symbol.FontSize;
			}
			if (sortOrder == FeatureSortOrder.MostUsed)
			{
				return Symbol.Priority;
			}

			throw new ArgumentOutOfRangeException();
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotSupportedException();
		}
	}
}