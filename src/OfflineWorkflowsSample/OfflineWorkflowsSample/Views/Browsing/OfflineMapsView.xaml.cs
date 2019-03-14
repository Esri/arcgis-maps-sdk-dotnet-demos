using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using OfflineWorkflowsSample;

namespace OfflineWorkflowSample.Views
{
    public sealed partial class OfflineMapsView : Page
    {
        public OfflineMapsView()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public MainViewModel ViewModel => (MainViewModel) Application.Current.Resources[nameof(MainViewModel)];
    }
}