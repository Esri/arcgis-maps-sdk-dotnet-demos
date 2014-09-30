using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Tasks.NetworkAnalyst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LocalNetworkSample.Controls
{
	/// <summary>
	/// Interaction logic for RouteDirectionView.xaml
	/// </summary>
	public partial class RouteDirectionView : UserControl
	{
		public RouteDirectionView()
		{
			InitializeComponent();
		}

		public Esri.ArcGISRuntime.Tasks.NetworkAnalyst.RouteDirection RouteDirection
		{
			get { return (Esri.ArcGISRuntime.Tasks.NetworkAnalyst.RouteDirection)GetValue(RouteDirectionProperty); }
			set { SetValue(RouteDirectionProperty, value); }
		}

		public static readonly DependencyProperty RouteDirectionProperty =
			DependencyProperty.Register("RouteDirection", typeof(Esri.ArcGISRuntime.Tasks.NetworkAnalyst.RouteDirection),
			typeof(RouteDirectionView), new PropertyMetadata(null, OnRouteDirectionPropertyChanged));

		private static void OnRouteDirectionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var direction = e.NewValue as RouteDirection;
			(d as RouteDirectionView).UpdateDirection(direction);
		}

		private void UpdateDirection(RouteDirection direction)
		{
			if (direction == null)
			{
				LayoutRoot.Visibility = Visibility.Collapsed;
				return;
			}
			LayoutRoot.Visibility = Visibility.Visible;
			LayoutRoot.DataContext = direction;
			var d = direction.GetLength(LinearUnits.Miles);
			if (d == 0)
				distance.Text = "";
			else if (d >= .25)
				distance.Text = d.ToString("0.0 mi");
			else
			{
				d = direction.GetLength(LinearUnits.Yards);
				distance.Text = d.ToString("0 yd");
			}
			if (direction.Time.TotalHours >= 1)
				time.Text = direction.Time.ToString("hh\\:mm");
			else if (direction.Time.TotalMinutes > 1)
				time.Text = direction.Time.ToString("mm\\:ss");
			else if (direction.Time.TotalSeconds > 0)
				time.Text = direction.Time.ToString("ss") + " sec";
			else
				time.Text = "";
		}
	}
}
