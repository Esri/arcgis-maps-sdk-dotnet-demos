using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace SymbolEditorApp.Converters
{
    public class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if((targetType == typeof(System.Drawing.Color) || targetType == typeof(System.Drawing.Color?)) && value is System.Windows.Media.Color c)
            {
                return ConvertToSDC(c);
            }
            else if ((targetType == typeof(System.Windows.Media.Color) || targetType == typeof(System.Windows.Media.Color?)) && value is System.Drawing.Color c2)
            {
                return ConvertFromSDC(c2);
            }
            return value;
        }

        private System.Windows.Media.Color ConvertFromSDC(System.Drawing.Color c) => System.Windows.Media.Color.FromArgb(c.A, c.R, c.G, c.B);

        private System.Drawing.Color ConvertToSDC(System.Windows.Media.Color c) => System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((targetType == typeof(System.Drawing.Color) || targetType == typeof(System.Drawing.Color?)) && value is System.Windows.Media.Color c)
            {
                return ConvertToSDC(c);
            }
            else if ((targetType == typeof(System.Windows.Media.Color) || targetType == typeof(System.Windows.Media.Color?)) && value is System.Drawing.Color c2)
            {
                return ConvertFromSDC(c2);
            }
            return value;
        }
    }
}
