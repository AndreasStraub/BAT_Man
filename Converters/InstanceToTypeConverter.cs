using System;
using System.Globalization;
using System.Windows.Data;

namespace BAT_Man.Converters
{
    /// <summary>
    /// Konvertiert eine Objektinstanz in ihren Typ.
    /// Wird im XAML verwendet, um den Typ des aktuellen ViewModels für Trigger zu ermitteln.
    /// </summary>
    public class InstanceToTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            // Gibt den Typ der Instanz zurück (z.B. BAT_Man.ViewModels.WelcomeViewModel)
            return value.GetType();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}