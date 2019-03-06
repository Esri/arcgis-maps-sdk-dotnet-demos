using OfflineWorkflowsSample;
using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace OfflineWorkflowSample.Views.ItemPages
{
    public sealed partial class GenericItemPage : Page
    {
        private MainViewModel _mainVM => (MainViewModel) Application.Current.Resources[nameof(MainViewModel)];

        public GenericItemPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs navigationEventArgs)
        {
            base.OnNavigatedTo(navigationEventArgs);

            try
            {
                DescriptionWebView.NavigateToString(_mainVM.SelectedItem.Description);
                TermsWebView.NavigateToString(_mainVM.SelectedItem.TermsOfUse);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                // Ignore
            }
        }
    }
}