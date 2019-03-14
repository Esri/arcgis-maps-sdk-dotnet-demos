using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using OfflineWorkflowsSample;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace OfflineWorkflowSample.Views
{
    public sealed partial class OfflineMapsView : Page
    {
        public MainViewModel ViewModel => (MainViewModel) Application.Current.Resources[nameof(MainViewModel)];
        public OfflineMapsView()
        {
            InitializeComponent();
            this.DataContext = this;
        }
    }
}