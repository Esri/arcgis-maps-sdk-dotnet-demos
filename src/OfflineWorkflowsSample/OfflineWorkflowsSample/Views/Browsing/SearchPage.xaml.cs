using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using OfflineWorkflowsSample;
using OfflineWorkflowSample.ViewModels;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace OfflineWorkflowSample.Views
{
    public sealed partial class SearchPage : Page
    {
        public MainViewModel ViewModel => (MainViewModel)Application.Current.Resources[nameof(MainViewModel)];
        public PortalSearchViewModel SearchViewModel => ViewModel.PortalViewModel.SearchViewModel;
        public SearchPage()
        {
            InitializeComponent();
            this.DataContext = this;
        }
    }
}