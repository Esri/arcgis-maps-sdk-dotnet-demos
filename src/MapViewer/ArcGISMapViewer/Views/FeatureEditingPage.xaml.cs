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
using CommunityToolkit.Mvvm.Messaging;

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

            WeakReferenceMessenger.Default.Register<EditFeatureMessage>(this, (r, m) =>
                {
                    RootFrame.Navigate(typeof(FeatureFormPage), m.GeoElement);
                });
            RootFrame.Navigate(typeof(FeatureEditingNewPage));
        }

        public static void EditFeature(GeoElement element)
        {
            var msg = new EditFeatureMessage(element);
            WeakReferenceMessenger.Default.Send(msg);
        }
    }

    public class EditFeatureMessage
    {
        public EditFeatureMessage(GeoElement element)
        {
            GeoElement = element;
        }
        public GeoElement GeoElement { get; }
    }
}
