using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KmlViewer
{
    public class BasemapSelector : ListView
    {
		public BasemapSelector()
		{
			DefaultStyleKey = typeof(BasemapSelector);
		}
		public static string GetThumbnail(DependencyObject obj)
		{
			return (string)obj.GetValue(ThumbnailProperty);
		}

		public static void SetThumbnail(DependencyObject obj, string value)
		{
			obj.SetValue(ThumbnailProperty, value);
		}

		public static readonly DependencyProperty ThumbnailProperty =
			DependencyProperty.RegisterAttached("Thumbnail", typeof(string), typeof(BasemapSelector), new PropertyMetadata(null));

		
    }
}
