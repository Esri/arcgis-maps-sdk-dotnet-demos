using Esri.ArcGISRuntime.Portal;
using OfflineWorkflowSample.ViewModels;
using OfflineWorkflowSample.Views.ItemPages;
using OfflineWorkflowsSample;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using NavigationView = Microsoft.UI.Xaml.Controls.NavigationView;
using NavigationViewBackRequestedEventArgs = Microsoft.UI.Xaml.Controls.NavigationViewBackRequestedEventArgs;
using NavigationViewItemInvokedEventArgs = Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace OfflineWorkflowSample.Views
{
    public sealed partial class NavigationPage : Page, IWindowService
    {
        private MainViewModel ViewModel => (MainViewModel) Application.Current.Resources[nameof(MainViewModel)];

        public NavigationPage()
        {
            InitializeComponent();
            Window.Current.SetTitleBar(DraggablePart);
            ApplicationView.GetForCurrentView().TitleBar.ButtonForegroundColor = Colors.DarkRed;

            // Show local content by default.
            ContentFrame.Navigate(typeof(OfflineMapsView), new SuppressNavigationTransitionInfo());
            NavigationView.SelectedItem = NavigationView.MenuItems.First();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (!ViewModel.IsInitialized)
            {
                LoginViewModel vm = (LoginViewModel) e.Parameter;
                await ViewModel.Initialize(vm.UserProfile, this);

                // Listen for search changes
                ViewModel.PortalViewModel.SearchViewModel.SearchChanged += (sender, args) =>
                {
                    if (!(ContentFrame.Content is SearchPage))
                    {
                        ContentFrame.Navigate(typeof(SearchPage), new SuppressNavigationTransitionInfo());
                        NavigationView.SelectedItem = NavigationView.MenuItems.Last();
                    }
                };
            }
        }

        public async Task ShowAlertAsync(string message)
        {
            await ShowAlertAsync(message, "");
        }

        public async Task ShowAlertAsync(string message, string title)
        {
            var messageDialog = new MessageDialog(message, title);
            await messageDialog.ShowAsync();
        }

        public void SetBusy(bool isBusy)
        {
            ViewModel.IsBusy = isBusy;
        }

        public void SetBusyMessage(string message)
        {
            ViewModel.IsBusyText = message;
        }

        private void NavigationView_OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                ContentFrame.Navigate(typeof(SettingsPage), new SuppressNavigationTransitionInfo());
            }
            else
            {
                switch (args.InvokedItemContainer.Tag.ToString())
                {
                    case "Local":
                        ContentFrame.Navigate(typeof(OfflineMapsView), new SuppressNavigationTransitionInfo());
                        break;
                    case "Folders":
                        ContentFrame.Navigate(typeof(PortalBrowserView), new SuppressNavigationTransitionInfo());
                        break;
                    case "Groups":
                        ContentFrame.Navigate(typeof(PortalGroupView), new SuppressNavigationTransitionInfo());
                        break;
                    case "Search":
                        ContentFrame.Navigate(typeof(SearchPage), new SuppressNavigationTransitionInfo());
                        break;
                }
            }
        }

        public void NavigateToPageForItem(Item item)
        {
            if (item is LocalItem localItem)
            {
                switch (localItem.Type)
                {
                    case LocalItemType.MobileMap:
                        ContentFrame.Navigate(typeof(MapPage));
                        break;
                }
            }
            else if (item is PortalItem portalItem)
            {
                switch (portalItem.Type)
                {
                    case PortalItemType.WebMap:
                        if (portalItem.TypeKeywords.Contains("Offline"))
                        {
                            ContentFrame.Navigate(typeof(OfflineMapPage));
                        }
                        else
                            ContentFrame.Navigate(typeof(MapPage));
                        break;
                    case PortalItemType.WebScene:
                        ContentFrame.Navigate(typeof(ScenePage));
                        break;
                    default:
                        ContentFrame.Navigate(typeof(GenericItemPage), new SuppressNavigationTransitionInfo());
                        break;
                }
            }
        }

        public void NavigateToLoginPage()
        {
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(LoginPage));
        }

        public async void LaunchItem(Item item)
        {
            try
            {
                if (item is PortalItem portalItem)
                {
                    await Launcher.LaunchUriAsync(new Uri($"https://www.arcgis.com/home/item.html?id={portalItem.ItemId}"));
                }
                else if (item is LocalItem localItem)
                {
                    await Launcher.LaunchUriAsync(new Uri($"https://www.arcgis.com/home/item.html?id={localItem.OriginalPortalItemId}"));
                }
                else
                {
                    await ShowAlertAsync("Couldn't open item in ArcGIS Online.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                await ShowAlertAsync("Couldn't open item in ArcGIS Online.");
            }
        }

        private void NavigationView_OnBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (ContentFrame.CanGoBack)
            {
                ContentFrame.GoBack(new SuppressNavigationTransitionInfo());

                // Reset selected item when showing browsing views.
                if (ContentFrame.Content is OfflineMapsView ||
                    ContentFrame.Content is PortalBrowserView ||
                    ContentFrame.Content is PortalGroupView)
                {
                    ViewModel.SelectedItem = null;
                }
            }
        }
    }
}