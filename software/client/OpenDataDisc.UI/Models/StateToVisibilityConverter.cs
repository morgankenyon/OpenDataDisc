using Avalonia.Data.Converters;
using OpenDataDisc.UI.ViewModels;
using System;
using System.Globalization;

namespace OpenDataDisc.UI.Models
{
    public class StateToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MainWindowState state && parameter is string targetState)
            {
                return state.ToString() == targetState;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
