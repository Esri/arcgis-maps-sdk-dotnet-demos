using System;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace OfflineWorkflowsSample
{
    /// <summary>
    /// A map page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, IDialogService
	{
		public MainPage()
		{            
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var vm = (MainViewModel)e.Parameter;
            await vm.Initialize();

            ViewModel = vm;
            this.DataContext = ViewModel;
        }

        /// <summary>
        /// Gets the view-model that provides mapping capabilities to the view
        /// </summary>
        private MainViewModel ViewModel { get; set; }

        public async Task ShowMessageAsync(string message)
        {
            var messageDialog = new MessageDialog(message);
            await messageDialog.ShowAsync();
        }
    }
}
