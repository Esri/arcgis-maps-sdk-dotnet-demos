using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace OfflineWorkflowSample
{
    public sealed partial class CustomAppTitleBar : UserControl
    {
        private Page _containingPage;
        public CustomAppTitleBar()
        {
            this.InitializeComponent();
            Window.Current.SetTitleBar(DraggablePart);
        }

        public void EnableBackButton(Page containingPage)
        {
            _containingPage = containingPage;
            BackButton.Visibility = Visibility.Visible;
        }

        private void BackButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_containingPage?.Frame != null && _containingPage.Frame.CanGoBack)
            {
                _containingPage.Frame.GoBack();
            }
        }
    }
}
