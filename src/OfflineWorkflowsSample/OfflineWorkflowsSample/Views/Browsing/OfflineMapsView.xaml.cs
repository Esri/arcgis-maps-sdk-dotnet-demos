using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using OfflineWorkflowsSample;

namespace OfflineWorkflowSample.Views
{
    public sealed partial class OfflineMapsView : Page
    {
        private bool _isFirstTime = true;

        public OfflineMapsView()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public MainViewModel ViewModel => (MainViewModel) Application.Current.Resources[nameof(MainViewModel)];

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (!_isFirstTime && ViewModel.LocalContentViewModel.RefreshCommand.CanExecute(null))
            {
                await ViewModel.LocalContentViewModel.Initialize();
            } 
            else if (_isFirstTime)
            {
                _isFirstTime = false;
            }
        }
    }
}