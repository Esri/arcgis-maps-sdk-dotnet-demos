
using Esri.ArcGISRuntime.Portal;
using PortalBrowser.ViewModels;
using System;
using Xamarin.Forms;

namespace XamarinPortalBrowser
{
    public partial class StartPage : TabbedPage
    {
        //public MapVM mapVM;
        public StartPage()
        {
            //mapVM = new MapVM();
            InitializeComponent();
            
           
        }

        private void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            ListView listView = sender as ListView;
            MapVM mapVM = new MapVM();
           
            try
            {
                //MapVM mapVM = (MapVM)this.Resources["mapVM"];
                //MapPage mapPage = new MapPage((MapVM)this.Resources["mapVM"]);
                this.Navigation.PushAsync(new MapPage(mapVM));
                if (listView.SelectedItem != null)
                    mapVM.PortalItem = listView.SelectedItem as ArcGISPortalItem;
            }
            catch (Exception ex)
            { }
            
        }
    }
}
