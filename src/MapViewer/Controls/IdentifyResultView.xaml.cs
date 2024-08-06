using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping.Popups;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace ArcGISMapViewer.Controls
{
    public sealed partial class IdentifyResultView : UserControl
    {
        public IdentifyResultView()
        {
            this.InitializeComponent();
        }

        public IReadOnlyList<IdentifyLayerResult> IdentifyResult
        {
            get { return (IReadOnlyList<IdentifyLayerResult>)GetValue(IdentifyResultProperty); }
            set { SetValue(IdentifyResultProperty, value); }
        }

        public static readonly DependencyProperty IdentifyResultProperty =
            DependencyProperty.Register(nameof(IdentifyResult), typeof(IReadOnlyList<IdentifyLayerResult>), typeof(IdentifyResultView), new PropertyMetadata(null, (s,e) => ((IdentifyResultView)s).OnIdentifyResultPropertyChanged(e.NewValue as IReadOnlyList<IdentifyLayerResult>)));

        private void OnIdentifyResultPropertyChanged(IReadOnlyList<IdentifyLayerResult>? identifyLayerResults)
        {
            var popup = GetPopup(identifyLayerResults);
            popupViewer.Popup = popup;
            bool canEdit = false;
            if (popup != null)
            {
                canEdit = (popup.GeoElement is Feature feature && feature.FeatureTable?.CanUpdate(feature) == true);
            }
            EditButton.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
        }


        private Esri.ArcGISRuntime.Mapping.Popups.Popup? GetPopup(IdentifyLayerResult? result)
        {
            if (result == null)
            {
                return null;
            }

            var popup = result.Popups.FirstOrDefault();
            if (popup != null)
            {
                return popup;
            }

            var geoElement = result.GeoElements.FirstOrDefault();
            if (geoElement != null)
            {
                if (result.LayerContent is IPopupSource)
                {
                    var popupDefinition = ((IPopupSource)result.LayerContent).PopupDefinition;
                    if (popupDefinition != null)
                    {
                        return new Esri.ArcGISRuntime.Mapping.Popups.Popup(geoElement, popupDefinition);
                    }
                }

                return Esri.ArcGISRuntime.Mapping.Popups.Popup.FromGeoElement(geoElement);
            }

            return null;
        }

        private Esri.ArcGISRuntime.Mapping.Popups.Popup? GetPopup(IEnumerable<IdentifyLayerResult>? results)
        {
            if (results == null)
            {
                return null;
            }
            foreach (var result in results)
            {
                var popup = GetPopup(result);
                if (popup != null)
                {
                    return popup;
                }

                foreach (var subResult in result.SublayerResults)
                {
                    popup = GetPopup(subResult);
                    if (popup != null)
                    {
                        return popup;
                    }
                }
            }

            return null;
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (popupViewer?.Popup?.GeoElement is GeoElement element)
                EditRequested?.Invoke(this, element);
        }

        public event EventHandler<GeoElement>? EditRequested;
    }
}
