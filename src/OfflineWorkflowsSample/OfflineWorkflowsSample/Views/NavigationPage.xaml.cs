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
            ContentFrame.BackStack.Clear();
            ViewModel.SelectedItem = null;
            // Determine which page should be navigated to.
            Type nextPageType = null;
            if (args.IsSettingsInvoked)
            {
                nextPageType = typeof(SettingsPage);
            }
            else
            {
                switch (args.InvokedItemContainer.Tag.ToString())
                {
                    case "Local":
                        nextPageType = typeof(OfflineMapsView);
                        break;
                    case "Folders":
                        nextPageType = typeof(PortalBrowserView);
                        break;
                    case "Groups":
                        nextPageType = typeof(PortalGroupView);
                        break;
                    case "Search":
                        nextPageType = typeof(SearchPage);
                        break;
                }
            }

            // Only navigate if the new page is different from the active page.
            if (nextPageType != ContentFrame.SourcePageType)
            {
                ContentFrame.Navigate(nextPageType, new SuppressNavigationTransitionInfo());
            }
        }

        public void NavigateToPageForItem(PortalItemViewModel itemVM)
        {
            ContentFrame.BackStack.Clear();
            Item item = itemVM.Item;
            if (item is LocalItem localItem)
            {
                switch (localItem.Type)
                {
                    case LocalItemType.MobileMapPackage:
                        ContentFrame.Navigate(typeof(MapPage));
                        break;
                    default:
                        ContentFrame.Navigate(typeof(GenericItemPage));
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
                        {
                            ContentFrame.Navigate(typeof(MapPage));
                        }

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
                ViewModel.SelectedItem = null;
                ContentFrame.GoBack(new SuppressNavigationTransitionInfo());

                // Reset selected item when showing browsing views.
                if (ContentFrame.Content is OfflineMapsView)
                {
                    NavigationView.SelectedItem = LocalContentMenuItem;
                }
                if (ContentFrame.Content is PortalBrowserView)
                {
                    NavigationView.SelectedItem = MyFoldersMenuItem;
                }
                if (ContentFrame.Content is PortalGroupView)
                {
                    NavigationView.SelectedItem = MyGroupsMenuItem;
                }
                if (ContentFrame.Content is SearchPage)
                {
                    NavigationView.SelectedItem = SearchMenuItem;
                }
            }
            ContentFrame.BackStack.Clear();
        }
    }
}