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

        private void IdentifyResultView_Unloaded(object sender, RoutedEventArgs e)
        {
            if (flipview.SelectedItem is Esri.ArcGISRuntime.Mapping.Popups.Popup popup && popup.GeoElement is GeoElement element)
            {
                if (element is Feature feature && feature.FeatureTable?.Layer is FeatureLayer fl)
                {
                    fl.UnselectFeature(feature);
                }
                // else if(element is Graphic)
            }
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
            if (flipview.SelectedItem is Esri.ArcGISRuntime.Mapping.Popups.Popup popup && popup.GeoElement is GeoElement element)
            {
                if (element is Feature feature && feature.FeatureTable?.Layer is FeatureLayer fl)
                {
                    fl.UnselectFeature(feature);
                }
                // else if(element is Graphic)
            }
            var popups = GetPopup(identifyLayerResults).ToList();
            flipview.ItemsSource = popups;
            FlipViewPipsPager.NumberOfPages = popups.Count;

        }


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
            if (flipview.SelectedItem is Esri.ArcGISRuntime.Mapping.Popups.Popup popup)
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

        private IEnumerable<Esri.ArcGISRuntime.Mapping.Popups.Popup> GetPopup(IdentifyLayerResult? result)
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
                                yield return new Esri.ArcGISRuntime.Mapping.Popups.Popup(elm, popupDefinition);
                            }
                        }

                        yield return Esri.ArcGISRuntime.Mapping.Popups.Popup.FromGeoElement(elm);
                    }
                }
            }
        }

        private IEnumerable<Esri.ArcGISRuntime.Mapping.Popups.Popup> GetPopup(IEnumerable<IdentifyLayerResult>? results)
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
