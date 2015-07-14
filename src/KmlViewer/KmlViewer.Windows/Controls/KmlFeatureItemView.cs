using Esri.ArcGISRuntime.Layers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KmlViewer.Controls
{
	public class KmlFeatureItemView : Control
	{
		public KmlFeatureItemView()
		{
			DefaultStyleKey = typeof(KmlFeatureItemView);
		}



		public KmlFeature KmlFeature
		{
			get { return (KmlFeature)GetValue(KmlFeatureProperty); }
			set { SetValue(KmlFeatureProperty, value); }
		}

		// Using a DependencyProperty as the backing store for KmlFeature.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty KmlFeatureProperty =
			DependencyProperty.Register("KmlFeature", typeof(KmlFeature), typeof(KmlFeatureItemView), new PropertyMetadata(null));

		
	}
}
