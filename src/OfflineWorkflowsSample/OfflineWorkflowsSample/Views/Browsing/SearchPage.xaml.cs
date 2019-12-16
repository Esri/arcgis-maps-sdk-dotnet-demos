using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using OfflineWorkflowsSample;
using OfflineWorkflowSample.ViewModels;

namespace OfflineWorkflowSample.Views
{
    public sealed partial class SearchPage : Page
    {
        public SearchPage()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public MainViewModel ViewModel => (MainViewModel) Application.Current.Resources[nameof(MainViewModel)];
        public PortalSearchViewModel SearchViewModel => ViewModel.PortalViewModel.SearchViewModel;
    }
}