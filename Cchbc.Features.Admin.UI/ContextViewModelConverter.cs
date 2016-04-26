using System;
using Windows.UI.Xaml.Data;
using Cchbc.Features.Admin.FeatureDetailsModule;

namespace Cchbc.Features.Admin.UI
{
	public sealed class ContextViewModelConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			return value as ContextViewModel;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			return value as ContextViewModel;
		}
	}
}