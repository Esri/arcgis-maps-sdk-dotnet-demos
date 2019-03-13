using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using OfflineWorkflowsSample;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace OfflineWorkflowSample.Views
{
    public sealed partial class PortalBrowserView : Page
    {
        public MainViewModel ViewModel => (MainViewModel)Application.Current.Resources[nameof(MainViewModel)];
        public PortalViewModel PortalViewModel => ViewModel.PortalViewModel;
        public PortalBrowserView()
        {
            InitializeComponent();
            this.DataContext = this;
        }
    }
}