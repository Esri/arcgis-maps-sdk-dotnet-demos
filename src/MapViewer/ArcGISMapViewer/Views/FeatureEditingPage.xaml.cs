using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping.FeatureForms;

namespace ArcGISMapViewer.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FeatureEditingPage : Page
    {
        public FeatureEditingPage()
        {
            this.InitializeComponent();
        }

        public GeoElement GeoElement
        {
            get { return (GeoElement)GetValue(GeoElementProperty); }
            set { SetValue(GeoElementProperty, value); }
        }

        public static readonly DependencyProperty GeoElementProperty =
            DependencyProperty.Register(nameof(GeoElement), typeof(GeoElement), typeof(FeatureEditingPage), new PropertyMetadata(null, (s,e) => ((FeatureEditingPage)s).GeoElementPropertyChanged(e.OldValue as GeoElement, e.NewValue as GeoElement)));

        private void GeoElementPropertyChanged(GeoElement? oldValue, GeoElement? newValue)
        {
            if (oldValue is Feature oldFeature && oldFeature.FeatureTable?.Layer is FeatureLayer fl)
            {
                fl.UnselectFeature(oldFeature);
            }
            if (newValue is ArcGISFeature afeature)
            {
                this.FeatureForm = new FeatureForm(afeature);
                if (afeature.FeatureTable?.Layer is FeatureLayer layer)
                {
                    layer.SelectFeature(afeature);
                }
            }
            else
                this.FeatureForm = null;
        }

        public FeatureForm? FeatureForm
        {
            get { return (FeatureForm?)GetValue(FeatureFormProperty); }
            set { SetValue(FeatureFormProperty, value); }
        }

        public static readonly DependencyProperty FeatureFormProperty =
            DependencyProperty.Register("FeatureForm", typeof(FeatureForm), typeof(FeatureEditingPage), new PropertyMetadata(null));


        private async void Cancel_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Cancel edit?",
                PrimaryButtonText = "OK",
                CloseButtonText = "Cancel",
                XamlRoot = XamlRoot
            };
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                await FeatureFormView.DiscardEditsAsync();
                FeatureForm = null;
                EditingEnded?.Invoke(this, EventArgs.Empty);
            }
        }

        private async void Update_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await FeatureFormView.FinishEditingAsync();
            }
            catch(System.Exception ex)
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = ex.Message,

                    PrimaryButtonText = "OK",
                    XamlRoot = XamlRoot
                };
                var result = await dialog.ShowAsync();
            }
            EditingEnded?.Invoke(this, EventArgs.Empty);
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Delete Feature?",
                PrimaryButtonText = "OK",
                CloseButtonText = "Cancel",
                XamlRoot = XamlRoot
            };
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                _ = FeatureForm?.Feature.FeatureTable?.DeleteFeatureAsync(FeatureForm.Feature);
                FeatureForm = null;
                EditingEnded?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler? EditingEnded;
    }
}
