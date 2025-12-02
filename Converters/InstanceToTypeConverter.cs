using System;
using System.Globalization;
using System.Windows.Data;

namespace BAT_Man.Converters
{
    /// <summary>
    /// Wandelt ein Objekt (z.B. das aktuelle ViewModel) in seinen Typ um.
    /// Das ermöglicht, im XAML zu prüfen: "Ist das aktuelle ViewModel vom Typ XY?"
    /// </summary>
    public class InstanceToTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Wenn der Wert null ist, gib null zurück
            if (value == null) return null;

            // Gib den TYP des Objekts zurück (z.B. typeof(FirmenUebersichtViewModel))
            return value.GetType();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException(); // Brauchen wir hier nicht
        }
    }
}