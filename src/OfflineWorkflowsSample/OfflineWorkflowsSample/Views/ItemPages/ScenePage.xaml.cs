using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Mapping;
using OfflineWorkflowsSample;
using OfflineWorkflowSample.ViewModels.ItemPages;
using System.Diagnostics;
using Windows.UI.Popups;

namespace OfflineWorkflowSample.Views.ItemPages
{
    public sealed partial class ScenePage : Page
    {
        private MainViewModel _mainVM = (MainViewModel) Application.Current.Resources[nameof(MainViewModel)];

        public ScenePage()
        {
            InitializeComponent();
        }

        private ScenePageViewModel ViewModel => (ScenePageViewModel) Resources[nameof(ViewModel)];

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            try
            {
                Scene scene = new Scene(_mainVM.SelectedItem.Item);

                await scene.LoadAsync();

                if (scene.LoadStatus != LoadStatus.Loaded)
                {
                    throw new Exception("Scene couldn't be loaded.");
                }

                ViewModel.Initialize(scene, _mainVM.SelectedItem);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
                this.Frame.GoBack();
                await new MessageDialog("Couldn't load scene.", "Error").ShowAsync();
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            // Reset the view model to avoid object already owned exceptions.
            ViewModel.Reset();
        }
    }
}