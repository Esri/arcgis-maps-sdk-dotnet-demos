using System;
using Windows.UI.Xaml.Data;
using Esri.ArcGISRuntime.Portal;
using OfflineWorkflowsSample;

namespace OfflineWorkflowSample.Infrastructure.Converter
{
    /// <summary>
    /// Converts from a nullable enum (PortalItemType?) to a string.
    /// Note that it expects to sometimes not be given a PortalItemType? due to presumed UWP issue.
    /// </summary>
    class NullEnumToBlankStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is PortalItemType pItemType)
                return pItemType;
            // Hack to word around weird combobox behavior.
            if (value is MainViewModel) return "";

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}