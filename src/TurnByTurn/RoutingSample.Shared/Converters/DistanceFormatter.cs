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
    }
}
