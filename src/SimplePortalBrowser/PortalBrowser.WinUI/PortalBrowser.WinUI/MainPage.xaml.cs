using Esri.ArcGISRuntime.Portal;
using Microsoft.UI.Xaml.Controls;

namespace PortalBrowser.WinUI;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();
    }
    private void GridView_ItemClick(object sender, ItemClickEventArgs e)
    {
        var item = (e.ClickedItem as PortalItem);
        base.Frame.Navigate(typeof(MapPage), item);
    }
}
