using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace OfflineWorkflowsSample.DownloadMapArea
{
    public sealed partial class DownloadMapAreaView : UserControl
    {
        public DownloadMapAreaView()
        {
            this.InitializeComponent();
        }

        private async void Compass_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // When tapping the compass, reset the rotation
            await MyMapView.SetViewpointRotationAsync(0);
        }
    }
}
