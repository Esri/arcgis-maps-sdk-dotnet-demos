using System;

namespace RoutingSample.Converters
{
    public class TimeFormatter : StringFormatter
    {
        protected override string Format(object value, object paramter, string language)
        {
            if (value == null)
            {
                return "0 min";
            }

            if (value is TimeSpan time)
            {
                if (time.TotalDays > 1)
                {
                    return $"{time.Days} days";
                }
                else if (time.TotalHours > 1)
                {
                    return $"{time.Hours} hr {time.Minutes} min";
                }
                else if (time.TotalMinutes > 1)
                {
                    return $"{time.Minutes} min";
                }
                else
                {
                    return $"{time.Seconds} sec";
                }
            }

            throw new NotSupportedException();
        }
    }
}
