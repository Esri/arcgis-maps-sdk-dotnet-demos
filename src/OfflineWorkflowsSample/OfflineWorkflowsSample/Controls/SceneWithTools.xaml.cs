using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace OfflineWorkflowSample.Controls
{
    public sealed partial class SceneWithTools : UserControl
    {
        public SceneWithTools()
        {
            InitializeComponent();
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