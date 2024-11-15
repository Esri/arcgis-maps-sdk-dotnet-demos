using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ArcGISMapViewer.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Mapping.FeatureForms;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Toolkit.UI;
using Esri.ArcGISRuntime.UI;
using Windows.Foundation;

namespace ArcGISMapViewer.ViewModels
{
    public partial class MapPageViewModel : ObservableObject
    {
        public MapPageViewModel()
        {
        }

        public MyViewController ViewController { get; } = new MyViewController();

        [ObservableProperty]
        public partial Feature? CurrentFeature { get; set; }

        partial void OnCurrentFeatureChanged(Feature? oldValue, Feature? newValue)
        {
            if(oldValue is not null && oldValue.FeatureTable?.Layer is FeatureLayer fl)
            {
                fl.UnselectFeature(oldValue);
            }
            if (newValue is ArcGISFeature afeature)
            {
                FeatureForm = new FeatureForm(afeature);
                if (afeature.FeatureTable?.Layer is FeatureLayer layer)
                {
                    layer.SelectFeature(afeature);
                }
            }
            else
                FeatureForm = null;
        }

        [ObservableProperty]
        public partial FeatureForm? FeatureForm { get; set; }

        partial void OnFeatureFormChanged(FeatureForm? value)
        {
            OnPropertyChanged(nameof(CanEdit));
        }

        public void ZoomTo(Layer? layer)
        {
            if (layer?.FullExtent is not null)
                ViewController.SetViewpointAsync(new Viewpoint(layer.FullExtent));
        }

        public bool CanEdit => FeatureForm is not null;


        private CancellationTokenSource? identifyToken;

        public async void OnGeoViewTapped(object sender, Esri.ArcGISRuntime.UI.Controls.GeoViewInputEventArgs e)
        {
            ViewController.DismissCallout();
            if (e.Location is null) return;
            try
            {
                identifyToken?.Cancel();
                identifyToken = new CancellationTokenSource();
                var result = await ViewController.IdentifyLayersAsync(e.Position, 2, false, 10, identifyToken.Token);
                identifyToken = null;
                if (result.SelectMany(r => r.GeoElements).Any())
                {
                    var calloutview = new Controls.IdentifyResultView() { IdentifyResult = result, GeoViewController = ViewController };
                    ViewController.ShowCalloutAt(e.Location, calloutview);
                    calloutview.EditRequested += (s, e) =>
                    {
                        WeakReferenceMessenger.Default.Send(new Views.MapPage.ShowRightPanelMessage(Views.MapPage.ShowRightPanelMessage.PanelId.EditFeature, e as Feature));
                    };
                    calloutview.CloseRequested += (s, e) => ViewController.DismissCallout();
                }
            }
            catch
            {
            }
        }

        public void OnFeatureEditComplete()
        {
            WeakReferenceMessenger.Default.Send(new Views.MapPage.ShowRightPanelMessage(Views.MapPage.ShowRightPanelMessage.PanelId.ClosePanel));
            CurrentFeature = null;
        }

        public class MyViewController : GeoViewController
        {
            public Task<IReadOnlyList<IdentifyLayerResult>> IdentifyLayersAsync(Point position, int tolerance, bool returnPopupsOnly, int maximumResultsPerLayer, CancellationToken token)
            {
                if (ConnectedView is null)
                    return Task.FromResult<IReadOnlyList<IdentifyLayerResult>>(new System.Collections.ObjectModel.ReadOnlyCollection<IdentifyLayerResult>(new List<IdentifyLayerResult>(0)));
                return ConnectedView.IdentifyLayersAsync(position, tolerance, returnPopupsOnly, maximumResultsPerLayer, token);
            }

            internal void ShowCalloutAt(MapPoint location, UIElement calloutContent) => ConnectedView?.ShowCalloutAt(location, calloutContent);
        }
    }
}
