using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using OfflineWorkflowsSample;
using System;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

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
            InitializeComponent();
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
                    string mmpkPath = _mainVM.LocalContentViewModel.PathsForItems[localItem];

                    var mmpk = await MobileMapPackage.OpenAsync(mmpkPath);

                    // Get the first map.
                    DataContext = mmpk.Maps.First();
                }
                else
                {
                    DataContext = new Map(_mainVM.SelectedItem as PortalItem);
                }
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
            }
        }
    }
}