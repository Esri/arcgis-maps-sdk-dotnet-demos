using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using OfflineWorkflowsSample;
using OfflineWorkflowSample.ViewModels;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace OfflineWorkflowSample.Views.ItemPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GenericItemPage : Page
    {
        private MainViewModel _mainVM => (MainViewModel)Application.Current.Resources[nameof(MainViewModel)];
        public GenericItemPage()
        {
            this.InitializeComponent();
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
