using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using OfflineWorkflowsSample;
using OfflineWorkflowsSample.Infrastructure.ViewServices;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace OfflineWorkflowSample.Views.ItemPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MapPage : Page
    {
        private MainViewModel _mainVM = (MainViewModel) Application.Current.Resources[nameof(MainViewModel)];
        public MapPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            try
            {
                if (_mainVM.SelectedItem is LocalItem localItem)
                {
                    // This logic is quite brittle and only valid for MMPKs created as a result of 
                    //   taking a map offline with this app. 
                    string mmpkPath = _mainVM.OfflineMapsViewModel.PathsForItems[localItem];

                    var mmpk = await MobileMapPackage.OpenAsync(mmpkPath);

                    // Get the first map.
                    this.DataContext = mmpk.Maps.First();
                }
                else
                {
                    this.DataContext = new Map(_mainVM.SelectedItem as PortalItem);
                }
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
            }
        }
    }
}
