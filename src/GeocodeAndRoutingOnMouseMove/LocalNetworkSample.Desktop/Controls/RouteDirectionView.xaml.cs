﻿using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Tasks.NetworkAnalysis;
using System.Windows;
using System.Windows.Controls;

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

        public DirectionManeuver RouteDirection
        {
            get { return (DirectionManeuver)GetValue(RouteDirectionProperty); }
            set { SetValue(RouteDirectionProperty, value); }
        }

        public static readonly DependencyProperty RouteDirectionProperty =
            DependencyProperty.Register("RouteDirection", typeof(DirectionManeuver),
            typeof(RouteDirectionView), new PropertyMetadata(null, OnRouteDirectionPropertyChanged));

        private static void OnRouteDirectionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var direction = e.NewValue as DirectionManeuver;
            (d as RouteDirectionView).UpdateDirection(direction);
        }

        private void UpdateDirection(DirectionManeuver direction)
        {
            if (direction == null)
            {
                LayoutRoot.Visibility = Visibility.Collapsed;
                return;
            }
            LayoutRoot.Visibility = Visibility.Visible;
            LayoutRoot.DataContext = direction;
            var d = LinearUnits.Miles.ConvertTo(LinearUnits.Meters, direction.Length);
            if (d == 0)
                distance.Text = "";
            else if (d >= .25)
                distance.Text = d.ToString("0.0 mi");
            else
            {
                d = LinearUnits.Yards.ConvertTo(LinearUnits.Meters, direction.Length);
                distance.Text = d.ToString("0 yd");
            }
            if (direction.Duration.TotalHours >= 1)
                time.Text = direction.Duration.ToString("hh\\:mm");
            else if (direction.Duration.TotalMinutes > 1)
                time.Text = direction.Duration.ToString("mm\\:ss");
            else if (direction.Duration.TotalSeconds > 0)
                time.Text = direction.Duration.ToString("ss") + " sec";
            else
                time.Text = "";
        }
    }
}
