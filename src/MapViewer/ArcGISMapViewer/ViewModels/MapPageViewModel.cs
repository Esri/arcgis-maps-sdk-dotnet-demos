using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Toolkit.UI;
using Esri.ArcGISRuntime.UI;

namespace ArcGISMapViewer.ViewModels
{
    public partial class MapPageViewModel : ObservableObject
    {
        public MapPageViewModel()
        {
        }

        public GeoViewController ViewController { get; } = new GeoViewController();

        [ObservableProperty]
        private Feature? currentFeature;

        partial void OnCurrentFeatureChanged(Feature? oldValue, Feature? newValue)
        {
            if(oldValue is not null && oldValue.FeatureTable?.Layer is FeatureLayer fl)
            {
                fl.UnselectFeature(oldValue);
            }
            if (newValue is ArcGISFeature afeature)
            {
                var definition = (newValue?.FeatureTable as ArcGISFeatureTable)?.FeatureFormDefinition ?? (newValue?.FeatureTable?.Layer as FeatureLayer)?.FeatureFormDefinition;
                FeatureForm = (definition != null) ? new Esri.ArcGISRuntime.Mapping.FeatureForms.FeatureForm(afeature, definition) : null;
                if (afeature.FeatureTable?.Layer is FeatureLayer layer)
                {
                    layer.SelectFeature(afeature);
                }
            }
            else
                FeatureForm = null;
        }

        [ObservableProperty]
        private Esri.ArcGISRuntime.Mapping.FeatureForms.FeatureForm? _featureForm;

        public void ZoomTo(Layer? layer)
        {
            if (layer?.FullExtent is not null)
                ViewController.SetViewpointAsync(new Viewpoint(layer.FullExtent));
        }
    }
}
