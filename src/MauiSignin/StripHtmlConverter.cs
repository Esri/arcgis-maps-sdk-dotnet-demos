using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MauiSignin
{
    public class StripHtmlConverter : IValueConverter
    {
        private const string HtmlLineBreakRegex = @"<br ?/?>";
        private const string HtmlStripperRegex = @"<(.|\n)*?>";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is string htmlText && targetType == typeof(string))
            {
                return ToPlainText(htmlText);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static string ToPlainText(string htmlText)
        {
            // Replace HTML line break tags.
            htmlText = Regex.Replace(htmlText, HtmlLineBreakRegex, System.Environment.NewLine);
            htmlText = htmlText.Replace("</p>", System.Environment.NewLine + System.Environment.NewLine);
            htmlText = htmlText.Replace("&nbsp;", " ");
            // Remove the rest of HTML tags.
            htmlText = Regex.Replace(htmlText, HtmlStripperRegex, string.Empty);

            return htmlText.Trim();
        }
    }
}
