using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace OfflineWorkflowSample
{
    public sealed partial class MapWithTools : UserControl
    {
        public MapWithTools()
        {
            InitializeComponent();
        }

        private async void Compass_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // When the compass is tapped, reset map rotation.
            await MyMapView.SetViewpointRotationAsync(0);
        }

        private void MenuButtonClicked(object sender, RoutedEventArgs e)
        {
            // Pivot item hack needed to prevent UWP layout cycle, which results in a crash.
            var content = OuterPivot.Items.ToList();
            OuterPivot.Items.Clear();
            MapLegendSplitView.IsPaneOpen = !MapLegendSplitView.IsPaneOpen;
            foreach (var item in content)
            {
                OuterPivot.Items.Add(item);
            }
        }
    }
}