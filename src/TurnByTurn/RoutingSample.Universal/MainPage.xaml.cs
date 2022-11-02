using RoutingSample.ViewModels;
using System;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace RoutingSample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly MainViewModel _mainViewModel;

        public MainPage()
        {
            InitializeComponent();

            _mainViewModel = new MainViewModel();
            _mainViewModel.LocationDisplay = MapView.LocationDisplay;
            _mainViewModel.LocationDisplay.NavigationPointHeightFactor = 0.5;

            DataContext = _mainViewModel;
        }
    }
}
