using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping.Popups;
using Esri.ArcGISRuntime.Toolkit.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Popup = Esri.ArcGISRuntime.Mapping.Popups.Popup;

namespace ArcGISMapViewer.Controls
{
    public sealed partial class IdentifyResultView : UserControl
    {
        public IdentifyResultView()
        {
            this.InitializeComponent();
            this.Unloaded += IdentifyResultView_Unloaded;
        }

        public GeoViewController? GeoViewController
        {
            get { return (GeoViewController?)GetValue(GeoViewControllerProperty); }
            set { SetValue(GeoViewControllerProperty, value); }
        }

        public static readonly DependencyProperty GeoViewControllerProperty =
            DependencyProperty.Register("GeoViewController", typeof(GeoViewController), typeof(IdentifyResultView), new PropertyMetadata(null));

        private void IdentifyResultView_Unloaded(object sender, RoutedEventArgs e)
        {
            if (flipview.SelectedItem is Popup popup && popup.GeoElement is GeoElement element)
            {
                if (element is Feature feature && feature.FeatureTable?.Layer is FeatureLayer fl)
                {
                    fl.UnselectFeature(feature);
                }
                // else if(element is Graphic)
            }
        }

        public IReadOnlyList<IdentifyLayerResult>? IdentifyResult
        {
            get { return (IReadOnlyList<IdentifyLayerResult>?)GetValue(IdentifyResultProperty); }
            set { SetValue(IdentifyResultProperty, value); }
        }

        public static readonly DependencyProperty IdentifyResultProperty =
            DependencyProperty.Register(nameof(IdentifyResult), typeof(IReadOnlyList<IdentifyLayerResult>), typeof(IdentifyResultView), new PropertyMetadata(null, (s,e) => ((IdentifyResultView)s).OnIdentifyResultPropertyChanged(e.NewValue as IReadOnlyList<IdentifyLayerResult>)));

        private void OnIdentifyResultPropertyChanged(IReadOnlyList<IdentifyLayerResult>? identifyLayerResults)
        {
            if (flipview.SelectedItem is Popup popup && popup.GeoElement is GeoElement element)
            {
                if (element is Feature feature && feature.FeatureTable?.Layer is FeatureLayer fl)
                {
                    fl.UnselectFeature(feature);
                }
                // else if(element is Graphic)
            }
            var popups = GetPopup(identifyLayerResults).ToList();
            this.Items = popups;
        }

        public IList? Items
        {
            get { return (IList?)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(IList), typeof(IdentifyResultView), new PropertyMetadata(null));




        private void flipview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.RemovedItems != null)
            {
                foreach (var item in e.RemovedItems)
                {
                    if(item is Popup p && p.GeoElement is Feature f && f.FeatureTable?.Layer is FeatureLayer fl)
                    {
                        fl.UnselectFeature(f);
                    }
                }
            }
            if (flipview.SelectedItem is Popup popup)
            {
                bool canEdit = false;
                if (popup != null)
                {
                    canEdit = (popup.GeoElement is Feature feature && feature.FeatureTable?.CanUpdate(feature) == true);
                    if (popup.GeoElement is Feature f && f.FeatureTable?.Layer is FeatureLayer fl)
                    {
                        fl.SelectFeature(f);
                    }
                }
                EditButton.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private IEnumerable<Popup> GetPopup(IdentifyLayerResult? result)
        {
            if (result != null)
            {
                if (result.Popups.Any())
                {
                    foreach (var item in result.Popups)
                        yield return item;
                }
                else
                {
                    foreach (var elm in result.GeoElements)
                    {
                        if (result.LayerContent is IPopupSource)
                        {
                            var popupDefinition = ((IPopupSource)result.LayerContent).PopupDefinition;
                            if (popupDefinition != null)
                            {
                                yield return new Popup(elm, popupDefinition);
                            }
                        }

                        yield return Popup.FromGeoElement(elm);
                    }
                }
            }
        }

        private IEnumerable<Popup> GetPopup(IEnumerable<IdentifyLayerResult>? results)
        {
            if (results != null)
            {
                foreach (var result in results)
                {
                    foreach (var p in GetPopup(result))
                        yield return p;
                    foreach (var subResult in result.SublayerResults)
                        foreach (var p2 in GetPopup(subResult))
                            yield return p2;
                }
            }
        }

        private void ZoomTo_Click(object sender, RoutedEventArgs e)
        {
            if (flipview.SelectedItem is Esri.ArcGISRuntime.Mapping.Popups.Popup popup && popup.GeoElement is GeoElement element)
            {
                var geometry = element.Geometry;
                if(geometry is MapPoint p && !p.IsEmpty)
                {
                    var vp = GeoViewController?.GetCurrentViewpoint(ViewpointType.CenterAndScale);
                    if (vp is not null)
                        GeoViewController?.SetViewpointAsync(new Viewpoint(p, vp.TargetScale / 2));
                }
                else if(geometry?.Extent is not null && !geometry.Extent.IsEmpty)
                {
                    GeoViewController?.SetViewpointAsync(new Viewpoint(geometry.Extent));
                }
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {

            if (flipview.SelectedItem is Esri.ArcGISRuntime.Mapping.Popups.Popup popup && popup.GeoElement is GeoElement element)
                EditRequested?.Invoke(this, element);
        }

        public event EventHandler<GeoElement>? EditRequested;

        public event EventHandler? CloseRequested;

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
