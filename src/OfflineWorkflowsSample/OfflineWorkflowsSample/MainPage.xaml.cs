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
using Windows.UI.Popups;
using System.Threading.Tasks;

namespace OfflineWorkflowsSample
{
	/// <summary>
	/// A map page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page, IDialogService
	{
		public MainPage()
		{
			this.InitializeComponent();
            DataContext = ViewModel;
            // Using view service approach with the dialog service to abstract messages from ViewModels
            ViewModel = new MainViewModel(this as IDialogService);
        }

        /// <summary>
        /// Gets the view-model that provides mapping capabilities to the view
        /// </summary>
        public MainViewModel ViewModel { get; } 

        private async void Compass_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // When tapping the compass, reset the rotation
            await MyMapView.SetViewpointRotationAsync(0);
            await ShowMessageAsync("test");
        }

        public async Task ShowMessageAsync(string message)
        {

            var messageDialog = new MessageDialog(message);
            await messageDialog.ShowAsync();
        }
    }
}
