using System;
using System.Globalization;
using System.Windows.Data;

namespace BAT_Man.Converters
{
    /// <summary>
    /// Konvertiert mehrere gebundene Werte in ein einziges Objekt-Array.
    /// <para>
    /// ZWECK:
    /// Ermöglicht die Übergabe von mehreren Parametern an einen Command,
    /// da die Eigenschaft CommandParameter standardmäßig nur ein einzelnes Objekt akzeptiert.
    /// Wird in Verbindung mit MultiBinding im XAML verwendet.
    /// </para>
    /// </summary>
    public class MultiParameterConverter : IMultiValueConverter
    {
        /// <summary>
        /// Führt die Konvertierung durch (Vom XAML zum ViewModel).
        /// </summary>
        /// <param name="values">Das Array der gebundenen Werte (z.B. zwei PasswordBox-Objekte).</param>
        /// <returns>Ein Klon des Arrays, der als CommandParameter übergeben wird.</returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // Rückgabe des Arrays der Werte (z.B. [PasswordBox1, PasswordBox2]).
            // Die Methode Clone() wird verwendet, um eine flache Kopie des Arrays zu erstellen.
            return values.Clone();
        }

        /// <summary>
        /// Rückkonvertierung (Vom ViewModel zum XAML).
        /// Wird in diesem Szenario nicht benötigt (One-Way-Binding).
        /// </summary>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}