namespace MauiSignin;

public partial class PortalPage : ContentPage
{
    public PortalPage()
    {
        InitializeComponent();
    }
    
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        Shell.Current.FlyoutBehavior = FlyoutBehavior.Flyout;
    }

    private void MapItem_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        var item = e.Group as Esri.ArcGISRuntime.Portal.PortalItem;
        if(item != null)
        {
            Navigation.PushAsync(new MapPage(item));
        }
    }
}