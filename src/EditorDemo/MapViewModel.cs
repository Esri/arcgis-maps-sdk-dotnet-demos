using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Toolkit.UI;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.UI.Editing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EditorDemo
{
    public partial class MapViewModel : ObservableObject
    {
        public MapViewModel()
        {
            LoadMap();
        }

        private async void LoadMap()
        {
            Map = await MapCreator.CreateMap(true);
        }

        public MapViewController Controller { get; } = new MapViewController();

        [ObservableProperty]
        private Map? _map;

        partial void OnMapChanging(Map? value)
        {
            // Clean up
            Controller.DismissCallout();
            Selection = null;
            EditFeatureSelection = null;
        }

        public GraphicsOverlayCollection? GraphicsOverlays { get; } = new GraphicsOverlayCollection();

        [RelayCommand]
        public async Task OnGeoViewTapped(GeoViewInputEventArgs eventArgs) => await Identify(eventArgs.Position, eventArgs.Location);

        public async Task Identify(Point location, MapPoint? mapLocation)
        {
            if (EditFeatureSelection is not null)
                return; // Don't do identify during an edit session
            Controller.DismissCallout();
            Selection = null;
            var result = await Controller.IdentifyLayersAsync(location, 1);
            if (result.FirstOrDefault()?.GeoElements?.FirstOrDefault() is GeoElement element)
            {
                var def = new CalloutDefinition(element) { TextExpression = "$feature.name" };
                Selection = element;
                def.Tag = element;
                if (EditorToolbar.CanEditGeometry(element))
                {
                    def.ButtonImage = new RuntimeImage(new Uri("edit-geometry-16.png", UriKind.Relative));
                    def.OnButtonClick = (obj) => Edit(obj as GeoElement);
                }
                Controller.ShowCalloutForGeoElement(element, location, def);
            }
            else if (mapLocation is not null)
            {
                Controller.ShowCalloutAt(mapLocation, new CalloutDefinition("No features found"));
            }
        }

        private void Edit(GeoElement? element)
        {
            if (element?.Geometry is not null)
            {
                EditFeatureSelection = element;
                Controller.DismissCallout();
                Controller.SetViewpointGeometryAsync(element.Geometry, 50);
            }
        }

        [RelayCommand]
        public void OnEditingCompleted(EditorToolbar.EditingCompletedEventArgs e)
        {
            if (EditFeatureSelection != null)
            {
                EditFeatureSelection.Geometry = e.Geometry;
                if(EditFeatureSelection is Feature feature && feature.FeatureTable?.CanUpdate(feature) == true)
                {
                    feature.FeatureTable.UpdateFeatureAsync(feature);
                }
                EditFeatureSelection = null;
            }
        }

        [RelayCommand]
        public void OnEditingCancelled()
        {
            EditFeatureSelection = null;
        }

        [ObservableProperty]
        private GeoElement? _editFeatureSelection;

        [ObservableProperty]
        private GeoElement? _selection;

        partial void OnSelectionChanged(GeoElement? oldValue, GeoElement? newValue)
        {
            Select(oldValue, false);
            Select(newValue, true);
        }

        private void Select(GeoElement? value, bool selected)
        {
            if (value is Graphic g)
                g.IsSelected = selected;
            else if (value is Feature feature && feature.FeatureTable?.Layer is FeatureLayer layer)
            {
                if (selected) layer.SelectFeature(feature);
                else layer.UnselectFeature(feature);
            }
        }
    }

    public class MapViewController  : GeoViewController
    {
        public Task<bool> SetViewpointGeometryAsync(Geometry? boundingGeometry, double padding = 0d)
        {
            if(base.ConnectedView is MapView mapView && boundingGeometry is not null)
            {
                return mapView.SetViewpointGeometryAsync(boundingGeometry, padding);
            }
            return Task.FromResult(false);
        }
    }
}
