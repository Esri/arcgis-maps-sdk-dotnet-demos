using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Esri.ArcGISRuntime.Mapping;
using OfflineWorkflowSample.ViewModels;

namespace OfflineWorkflowSample
{
    public sealed partial class MapWithTools : UserControl
    {
        public MapWithTools()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty MapProperty =
            DependencyProperty.Register(
                nameof(Map), typeof(Map),
                typeof(MapWithTools), null
            );

        public Map Map
        {
            get => (Map)GetValue(MapProperty);
            set => SetValue(MapProperty, value);
        }

        private async void Compass_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // When the compass is tapped, reset map rotation.
            await MyMapView.SetViewpointRotationAsync(0);
        }

        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register(
                nameof(Item), typeof(PortalItemViewModel),
                typeof(MapWithTools), null
            );

        public PortalItemViewModel Item
        {
            get => (PortalItemViewModel)GetValue(ItemProperty);
            set => SetValue(ItemProperty, value);
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