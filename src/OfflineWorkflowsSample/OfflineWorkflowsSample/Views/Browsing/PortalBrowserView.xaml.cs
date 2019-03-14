using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using OfflineWorkflowsSample;

namespace OfflineWorkflowSample.Views
{
    public sealed partial class PortalBrowserView : Page
    {
        public PortalBrowserView()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public MainViewModel ViewModel => (MainViewModel) Application.Current.Resources[nameof(MainViewModel)];
        public PortalViewModel PortalViewModel => ViewModel.PortalViewModel;
    }
}