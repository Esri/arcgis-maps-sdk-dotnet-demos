using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using XLabs.Forms.Controls;

namespace OfficeLocator.Forms
{
	public partial class CampusMapPage : ContentPage
	{
		public CampusMapPage()
		{
            InitializeComponent();
		}

        public MapViewModel VM { get; } = new MapViewModel();

        protected async override void OnAppearing()
        {            
            base.OnAppearing();
            BindingContext = VM;
            VM.RequestViewpoint += VM_RequestViewpoint;
            await VM.LoadAsync();
            loadingStatus.IsVisible = false;
        }

        private void Search_SelectedItemChanged(object sender, SelectedItemChangedEventArgs e)
        {
            if (sender == searchFrom)
                VM.GeocodeFromLocation(e.SelectedItem as string);
            else
                VM.GeocodeToLocation(e.SelectedItem as string);
        }

        private async void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.NewTextValue))
                (sender as AutoCompleteView).Suggestions = null;
            else
            {
                var suggestions = await GeocodeHelper.SuggestAsync(e.NewTextValue);
                (sender as AutoCompleteView).Suggestions = suggestions.ToList();
            }
        }

        private void VM_RequestViewpoint(object sender, Esri.ArcGISRuntime.Mapping.Viewpoint viewpoint)
        {
            CampusView.SetViewpointAsync(viewpoint, TimeSpan.FromSeconds(3));
        }
    }
}
