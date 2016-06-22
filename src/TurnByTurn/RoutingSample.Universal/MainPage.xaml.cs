using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Security;
using RoutingSample.ViewModels;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace RoutingSample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            
            var viewModel = (MainPageVM)MyMapView.DataContext;
            viewModel.LocationDisplay = MyMapView.LocationDisplay;
        }
    }
}
