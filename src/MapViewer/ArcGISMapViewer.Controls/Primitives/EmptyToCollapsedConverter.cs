using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace ArcGISMapViewer.Controls.Primitives
{
    public sealed partial class EmptyToCollapsedConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, string language)
        {
            if(value is string str)
                return string.IsNullOrEmpty(str) ? Visibility.Collapsed : Visibility.Visible;
            if(value is null)
                return Visibility.Collapsed;
            if(value is IList)
                return ((IList)value).Count == 0 ? Visibility.Collapsed : Visibility.Visible;
            if(value is Array)
                return ((Array)value).Length == 0 ? Visibility.Collapsed : Visibility.Visible;
            if(value is IEnumerable)
            {
                var enumerator = ((IEnumerable)value).GetEnumerator();
                if(enumerator.MoveNext())
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
