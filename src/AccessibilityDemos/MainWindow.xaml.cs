using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DemoApplicationAccessibility
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void GeometryEditingUsingReticle_Click(object sender, RoutedEventArgs e)
        {
            var sample = new GeometryEditing.GeometryEditing();
            SampleContainer.Child = sample;
        }
        private void FeatureIdentificationUnderRectangle_Click(object sender, RoutedEventArgs e)
        {
            var sample = new IdentifyFeatures.IdentifyFeatures();
            SampleContainer.Child = sample;
        }
        private void FeatureMap_Click(object sender, RoutedEventArgs e)
        {
            var sample = new DescribingNonTextContent.FeatureMap();
            SampleContainer.Child = sample;
        }
        private void ThematicMap_Click(object sender, RoutedEventArgs e)
        {
            var sample = new DescribingNonTextContent.ThematicMap();
            SampleContainer.Child = sample;
        }
        private void BasemapContrast_Click(object sender, RoutedEventArgs e)
        {
            var sample = new Contrast.AdaptiveContrast();
            SampleContainer.Child = sample;
        }
    }
}
