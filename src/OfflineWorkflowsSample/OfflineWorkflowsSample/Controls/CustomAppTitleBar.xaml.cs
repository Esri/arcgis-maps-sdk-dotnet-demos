using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace OfflineWorkflowSample
{
    public sealed partial class CustomAppTitleBar : UserControl, INotifyPropertyChanged
    {
        private string _title = "ArcGIS Maps Offline";

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        private Page _containingPage;
        public CustomAppTitleBar()
        {
            this.InitializeComponent();
            Window.Current.SetTitleBar(DraggablePart);
            ApplicationView.GetForCurrentView().TitleBar.ButtonForegroundColor = Colors.Black;
            DataContext = this;
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

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
