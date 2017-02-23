using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if NETFX_CORE
using Windows.UI.Xaml;
#else
using System.Windows;
#endif

namespace RoutingSample
{
	/// <summary>
	/// Binding helpers
	/// </summary>
	public class CommandBinder
	{
		/// <summary>
		/// This command binding allows you to set the extent on a mapView from your view-model through binding
		/// </summary>
		public static Envelope GetRequestView(DependencyObject obj)
		{
			return (Envelope)obj.GetValue(RequestViewProperty);
		}

		/// <summary>
		/// This command binding allows you to set the extent on a mapView from your view-model through binding
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="extent"></param>
		public static void SetRequestView(DependencyObject obj, Envelope extent)
		{
			obj.SetValue(RequestViewProperty, extent);
		}

		/// <summary>
		/// Identifies the ZoomTo Attached Property.
		/// </summary>
		public static readonly DependencyProperty RequestViewProperty =
			DependencyProperty.RegisterAttached("RequestView", typeof(Viewpoint), typeof(CommandBinder), new PropertyMetadata(null, RequestViewPropertyChanged));

		private static void RequestViewPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is MapView)
			{
				MapView mapView = d as MapView;
				if (e.NewValue is Viewpoint)
				{
					mapView.SetViewpoint((Viewpoint)e.NewValue);
				}
			}
		}
	}
}
