using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Navigation;
using System;

namespace RoutingSample.Converters
{
    public class DistanceFormatter : StringFormatter
    {
        protected override string Format(object value, object paramter, string language)
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

        /*
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

            throw new NotSupportedException();
        }
        */
    }
}
