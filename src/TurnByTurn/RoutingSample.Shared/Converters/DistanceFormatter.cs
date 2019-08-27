using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Navigation;
using System;

namespace RoutingSample.Converters
{
    public class DistanceFormatter : ValueConverterBase
    {
        protected override object Convert(object value, Type targetType, object paramter, string language)
        {
            if (value == null)
            {
                // Placeholder for NULL
                return "0 ft";
            }

            if (value is TrackingDistance distance)
            {
                return $"{distance.DisplayText} {distance.DisplayTextUnits.Abbreviation}";
            }

            throw new NotSupportedException();
        }

        private double ToMiles(TrackingDistance distance)
        {
            switch (distance.DisplayTextUnits.Name)
            {
                case "Kilometers":
                    return LinearUnits.Kilometers.ConvertTo(LinearUnits.Miles, distance.Distance);

                case "Meters":
                    return LinearUnits.Meters.ConvertTo(LinearUnits.Miles, distance.Distance);

                case "Statute Miles":
                case "Miles":
                    return distance.Distance;
            }

            // We won't bother
            throw new NotSupportedException();
        }

        protected override object ConvertBack(object value, Type targetType, object paramter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
