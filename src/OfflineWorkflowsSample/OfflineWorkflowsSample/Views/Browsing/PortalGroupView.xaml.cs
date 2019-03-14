using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using OfflineWorkflowsSample;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace OfflineWorkflowSample.Views
{
    public sealed partial class PortalGroupView : Page
    {
        public MainViewModel ViewModel => (MainViewModel)Application.Current.Resources[nameof(MainViewModel)];
        public PortalViewModel PortalViewModel => ViewModel.PortalViewModel;
        public PortalGroupView()
        {
            InitializeComponent();
            this.DataContext = this;
        }
    }
}