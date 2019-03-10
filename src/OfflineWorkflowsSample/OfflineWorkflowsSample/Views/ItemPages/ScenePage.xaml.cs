using Esri.ArcGISRuntime.Mapping;
using OfflineWorkflowsSample;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using OfflineWorkflowSample.ViewModels.ItemPages;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace OfflineWorkflowSample.Views.ItemPages
{
    public sealed partial class ScenePage : Page
    {
        private MainViewModel _mainVM = (MainViewModel) Application.Current.Resources[nameof(MainViewModel)];
        private ScenePageViewModel ViewModel;

        public ScenePage()
        {
            InitializeComponent();
            ViewModel = (ScenePageViewModel)Resources[nameof(ViewModel)];
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ViewModel.Initialize(new Scene(_mainVM.SelectedItem.Item), _mainVM.SelectedItem);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            // Reset the view model to avoid object already owned exceptions.
            ViewModel.Reset();
        }
    }
}