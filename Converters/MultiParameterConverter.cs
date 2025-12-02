using System;
using System.Globalization;
using System.Windows.Data;

namespace BAT_Man.Converters
{
    // <!-- 
    // Dieser Converter nimmt mehrere Bindings entgegen und gibt sie als Objekt-Array zurück.
    // Das ermöglicht es uns, mehrere Parameter an einen Command zu übergeben.
    // -->
    public class MultiParameterConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // Gibt einfach das Array der Werte zurück (z.B. [PasswordBox1, PasswordBox2])
            return values.Clone();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}