using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace OfflineWorkflowsSample.GenerateMapArea
{
    public sealed partial class GenerateMapAreaView : UserControl
    {
        public GenerateMapAreaView() 
        {
            InitializeComponent();
        }

        private async void Compass_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // When tapping the compass, reset the rotation
            await MyMapView.SetViewpointRotationAsync(0);
        }
    }
}
