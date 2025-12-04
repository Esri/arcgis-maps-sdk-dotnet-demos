using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using CommunityToolkit.Mvvm.Messaging;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.UI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;

namespace ArcGISMapViewer.Controls
{
    public sealed partial class MapPropertiesView : UserControl
    {
        public MapPropertiesView()
        {
            this.InitializeComponent();
        }

        public GeoModel GeoModel
        {
            get { return (GeoModel)GetValue(GeoModelProperty); }
            set { SetValue(GeoModelProperty, value); }
        }

        public static readonly DependencyProperty GeoModelProperty =
            DependencyProperty.Register("GeoModel", typeof(GeoModel), typeof(MapPropertiesView), new PropertyMetadata(null, (s,e) => ((MapPropertiesView)s).OnGeoModelPropertyChanged(e.NewValue as GeoModel)));

        private void OnGeoModelPropertyChanged(GeoModel? geoModel)
        {
        }

        public GeoView GeoView
        {
            get { return (GeoView)GetValue(GeoViewProperty); }
            set { SetValue(GeoViewProperty, value); }
        }

        public static readonly DependencyProperty GeoViewProperty =
            DependencyProperty.Register("GeoView", typeof(GeoView), typeof(MapPropertiesView), new PropertyMetadata(null));

        public static bool IsSunLightMode(SceneLighting lighting) => lighting is SunLighting;


        public global::Windows.UI.Color BackgroundColor
        {
            get => ConvertColor((GeoModel as Scene)?.Environment?.BackgroundColor);
            set
            {
                if ((GeoModel as Scene)?.Environment is SceneEnvironment env)
                    env.BackgroundColor = ConvertColor(value);
            }
        }
        public static global::Windows.UI.Color ConvertColor(System.Drawing.Color? color)
        {
            if (color.HasValue)
                return global::Windows.UI.Color.FromArgb(color.Value.A, color.Value.R, color.Value.G, color.Value.B);
            return Microsoft.UI.Colors.Transparent;
        }
        public static global::System.Drawing.Color ConvertColor(global::Windows.UI.Color? color)
        {
            if (color.HasValue)
                return global::System.Drawing.Color.FromArgb(color.Value.A, color.Value.R, color.Value.G, color.Value.B);
            return System.Drawing.Color.Transparent;
        }

        private void LightingSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            var switchControl = (ToggleSwitch)sender;
            if (GeoModel is Scene scene)
            {
                if (switchControl.IsOn && scene.Environment.Lighting is not SunLighting)
                    scene.Environment.Lighting = new SunLighting(DateTimeOffset.UtcNow, scene.Environment?.Lighting?.AreDirectShadowsEnabled ?? true);
                else if (scene.Environment.Lighting is not VirtualLighting)
                    scene.Environment.Lighting = new VirtualLighting(scene.Environment?.Lighting?.AreDirectShadowsEnabled ?? false);
            }
        }

        public static double GetDayOfTheYear(SceneLighting lighting)
        {
            if (lighting is SunLighting sl)
                return sl.SimulatedDate.DayOfYear;
            else return 0d;
        }
        public static double GetTimeOfDay(SceneLighting lighting)
        {
            if (lighting is SunLighting sl)
                return sl.SimulatedDate.TimeOfDay.TotalHours;
            else return 0d;
        }

        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (GeoModel is Scene scene && scene.Environment.Lighting is SunLighting sl)
            {
                sl.SimulatedDate = sl.SimulatedDate.Date.AddHours(e.NewValue);
            }
        }

        private void Date_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (GeoModel is Scene scene && scene.Environment.Lighting is SunLighting sl)
            {
                sl.SimulatedDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero).AddDays(e.NewValue).Add(sl.SimulatedDate.TimeOfDay);
            }
        }

        private void Grid_Changed(object sender, SelectionChangedEventArgs e)
        {
            var selection = ((sender as ComboBox)?.SelectedItem as ComboBoxItem)?.Content as string;
            Esri.ArcGISRuntime.UI.Grid? grid = null;
            if(selection == "MGRS")
            {
                grid = new Esri.ArcGISRuntime.UI.MgrsGrid();
            }
            else if (selection == "USNG")
            {
                grid = new Esri.ArcGISRuntime.UI.UsngGrid();
            }
            else if (selection == "UTM")
            {
                grid = new Esri.ArcGISRuntime.UI.UtmGrid();
            }
            else if (selection == "Lat/Long")
            {
                grid = new Esri.ArcGISRuntime.UI.LatitudeLongitudeGrid();
            }
            if (GeoView is MapView mv)
            {
                mv.Grid = grid;
            }
            else if (GeoView is SceneView sv)
            {
                sv.Grid = grid;
            }
            else if (GeoView is LocalSceneView lsv)
            {
                lsv.Grid = grid;
            }
        }
    }

    public class TimeOfYearToolTipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double d)
            {
                int dayOfYear = (int)d;
                DateTime date = new DateTime(2025, 1, 1).AddDays(dayOfYear);
                return date.ToString("MMMM dd");
            }
            else
            {
                return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public partial class MapPropertiesTemplateSelector : DataTemplateSelector
    {
        public MapPropertiesTemplateSelector()
        {

        }
        protected override DataTemplate? SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is Map)
                return MapTemplate;
            if (item is Scene scene)
            {
                if (scene.ViewingMode == SceneViewingMode.Global)
                    return GlobeTemplate;
                if (scene.ViewingMode == SceneViewingMode.Local)
                    return LocalSceneTemplate;
            }
            return base.SelectTemplateCore(item, container);
        }
        public DataTemplate? MapTemplate { get; set; }
        public DataTemplate? GlobeTemplate { get; set; }
        public DataTemplate? LocalSceneTemplate { get; set; }
    }
}