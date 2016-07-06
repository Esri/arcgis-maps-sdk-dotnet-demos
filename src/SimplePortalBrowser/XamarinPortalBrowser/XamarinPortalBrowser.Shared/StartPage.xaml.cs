
using Esri.ArcGISRuntime.Portal;
using System;
using Xamarin.Forms;

namespace XamarinPortalBrowser
{
    public partial class StartPage : MasterDetailPage
    {
        public StartPage()
        {
            InitializeComponent();
        }

        private void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var portalItem = ((ListView)sender).SelectedItem as ArcGISPortalItem;
            if (portalItem == null)
                return;
            new NavigationPage((Page)Activator.CreateInstance(typeof(MapPage)));
        }        
    }
}
