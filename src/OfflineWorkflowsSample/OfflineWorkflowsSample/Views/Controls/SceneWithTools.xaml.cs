using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Esri.ArcGISRuntime.Mapping;
using OfflineWorkflowSample.ViewModels;

namespace OfflineWorkflowSample.Controls
{
    public sealed partial class SceneWithTools : UserControl
    {
        public SceneWithTools()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty SceneProperty =
            DependencyProperty.Register(
                nameof(Scene), typeof(Scene),
                typeof(SceneWithTools), null
            );

        public Scene Scene
        {
            get => (Scene)GetValue(SceneProperty);
            set => SetValue(SceneProperty, value);
        }

        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register(
                nameof(Item), typeof(PortalItemViewModel),
                typeof(SceneWithTools), null
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