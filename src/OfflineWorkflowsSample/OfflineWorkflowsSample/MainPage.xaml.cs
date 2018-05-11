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
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;

namespace OfflineWorkflowsSample
{
	/// <summary>
	/// A map page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page
	{
		public MainPage()
		{
			this.InitializeComponent();
            DataContext = ViewModel;
		}

        /// <summary>
        /// Gets the view-model that provides mapping capabilities to the view
        /// </summary>
        public MainViewModel ViewModel { get; } = new MainViewModel();

        private async void Compass_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // When tapping the compass, reset the rotation
            await MyMapView.SetViewpointRotationAsync(0);
        }
    }
}
