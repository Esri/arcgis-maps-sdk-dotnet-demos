using System;

namespace RoutingSample.Converters
{
    public class EnumFormatter : StringFormatter
    {
        protected override string Format(object value, object parameter, string language)
        {
            if (value == null)
                return string.Empty;

            try
            {
                return Enum.GetName(value.GetType(), value);
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
