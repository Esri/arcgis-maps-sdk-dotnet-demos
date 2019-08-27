using System;

namespace RoutingSample.Converters
{
    public class EnumFormatter : ValueConverterBase
    {
        protected override object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return string.Empty;

            try
            {
                if (value is SimulationState state)
                {
                    switch (state)
                    {
                        case SimulationState.Stopped:
                            return "Stopped";

                        case SimulationState.Following:
                        case SimulationState.Seeking:
                            return "Following";

                        case SimulationState.Wandering:
                            return "Wandering";
                    }
                }

                return Enum.GetName(value.GetType(), value);
            }
            catch
            {
                return string.Empty;
            }
        }

        protected override object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
