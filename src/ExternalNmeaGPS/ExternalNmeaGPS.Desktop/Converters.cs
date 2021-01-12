using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using Esri.ArcGISRuntime.Location;

namespace ExternalNmeaGPS
{
    public class FixTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is NmeaFixType type)
            {
                switch (type)
                {
                    case NmeaFixType.Invalid: return "Invalid";
                    case NmeaFixType.Standard: return "GPS";
                    case NmeaFixType.Dgps: return "Differential";
                    case NmeaFixType.Frtk: return "Float RTK";
                    case NmeaFixType.Rtk: return "RTK";
                    case NmeaFixType.Estimated: return "Estimated";
                    case NmeaFixType.Manual: return "Manual";
                    case NmeaFixType.Pps: return "PPS";
                    case NmeaFixType.Simulation: return "Simulated";
                }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
