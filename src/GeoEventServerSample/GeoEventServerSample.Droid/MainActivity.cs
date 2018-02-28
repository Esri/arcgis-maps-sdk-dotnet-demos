using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Javax.Net.Ssl;
using Android.Widget;

namespace GeoEventServerSample.Droid
{
    [Activity (Label = "@string/ApplicationName", MainLauncher = true, Icon = "@drawable/icon", ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : Activity
    {
        private MapViewModel _mapViewModel = new MapViewModel();
        private MapView _mapView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set the view from the "Main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get MapView from the view and assign map from view-model
            _mapView = FindViewById<MapView>(Resource.Id.MyMapView);
            _mapView.Map = _mapViewModel.Map;
            _mapView.GraphicsOverlays = _mapViewModel.Overlays;

            // Listen for changes on the view model
            _mapViewModel.PropertyChanged += MapViewModel_PropertyChanged;
        }

        private void MapViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MapViewModel.Map) && _mapView != null)
            {
                _mapView.Map = _mapViewModel.Map;
            }
            else if(e.PropertyName == nameof(MapViewModel.VehicleCount))
            {
                var tv = FindViewById<TextView>(Resource.Id.VehicleCount);
                tv.Text = "Vehicle count: " + ((MapViewModel)sender).VehicleCount.ToString();
            }
            else if (e.PropertyName == nameof(MapViewModel.StreamInfo))
            {
                var tv = FindViewById<TextView>(Resource.Id.StatusText);
                tv.Text = ((MapViewModel)sender).StreamInfo;
            }
        }
    }
}