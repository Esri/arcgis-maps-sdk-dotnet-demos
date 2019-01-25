using OfflineWorkflowsSample;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace OfflineWorkflowSample.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MapPage : Page
    {
        private MainViewModel ViewModel;

        public MapPage()
        {
            this.InitializeComponent();
            TitleBar.EnableBackButton(this);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var vm = (MainViewModel)e.Parameter;

            ViewModel = vm;
            this.DataContext = ViewModel;
            
            TitleBar.Title = ViewModel.Map.Item.Title;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            DownloadControlsSplitView.IsPaneOpen = !DownloadControlsSplitView.IsPaneOpen;
        }
    }
}
