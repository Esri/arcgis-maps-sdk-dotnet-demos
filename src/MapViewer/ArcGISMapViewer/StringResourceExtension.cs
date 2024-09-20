using Microsoft.UI.Xaml.Markup;
using Microsoft.Windows.ApplicationModel.Resources;

namespace ArcGISMapViewer
{
    [MarkupExtensionReturnType(ReturnType = typeof(string))]
    public partial class StringResourceExtension : MarkupExtension
    {
        public StringResourceExtension() { }

        public string Key { get; set; } = "";

        protected override object ProvideValue()
        {
            return Resources.GetString(Key);
        }
    }
    public static class Resources
    {
        private static readonly ResourceLoader _resourceLoader = new();
        public static string GetString(string key)
        {
            return _resourceLoader.GetString(key);
        }
    }
}
