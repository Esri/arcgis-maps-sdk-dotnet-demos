using System.Windows;

namespace PortalBrowser.WPF;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        MyMapView.LocationDisplay.IsEnabled = true;
    }

    private void Exit_Clicked(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}
