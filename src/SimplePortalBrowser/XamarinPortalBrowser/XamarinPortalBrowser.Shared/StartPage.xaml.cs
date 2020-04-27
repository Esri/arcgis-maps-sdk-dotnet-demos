
using Esri.ArcGISRuntime.Portal;
using PortalBrowser.ViewModels;
using System;
using Xamarin.Forms;

namespace XamarinPortalBrowser
{
    public partial class StartPage : TabbedPage
    {
        public StartPage()
        {
            InitializeComponent();
        }

        private void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var listView = sender as ListView;
            var mapVM = new MapVM();
           
            try
            {
                Navigation.PushAsync(new MapPage(mapVM));
                if (listView.SelectedItem != null)
                    mapVM.PortalItem = listView.SelectedItem as PortalItem;
            }
            catch
            { 
            }
        }
    }
}
