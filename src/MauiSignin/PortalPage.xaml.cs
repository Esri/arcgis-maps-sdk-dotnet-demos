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

    private void MapItem_ItemTapped(object sender, SelectionChangedEventArgs e)
    {
        var item = e.CurrentSelection?.FirstOrDefault() as Esri.ArcGISRuntime.Portal.PortalItem;

        if (item != null)
        {
            ((CollectionView)sender).SelectedItem = null;
            Navigation.PushAsync(new MapPage(item));
        }
    }
}