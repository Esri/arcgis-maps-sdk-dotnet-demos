﻿using dotMorten.Xamarin.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace OfficeLocator.Forms
{
	public partial class CampusMapPage : ContentPage
	{
		public CampusMapPage()
		{
            InitializeComponent();
        }

        private void OnHeaderSizeChanged(object sender, EventArgs e)
        {
            //Update insets so the areas behind the header and footer are taken into account when zooming to the route
            if(!loadingStatus.IsVisible)
                CampusView.ViewInsets = new Thickness(0, Header.Height, 0, Math.Max(0, RouteDetails.Height));
        }

        public MapViewModel VM { get; } = new MapViewModel(Xamarin.Forms.Device.BeginInvokeOnMainThread);

        protected async override void OnAppearing()
        {            
            base.OnAppearing();
            BindingContext = VM;
            VM.RequestViewpoint += VM_RequestViewpoint;
            await VM.LoadAsync();
            loadingStatus.IsVisible = false;
            OnHeaderSizeChanged(null, null);
            CampusView.LocationDisplay.IsEnabled = true;
        }

        private void VM_RequestViewpoint(object sender, Esri.ArcGISRuntime.Mapping.Viewpoint viewpoint)
        {
            CampusView.SetViewpointAsync(viewpoint, TimeSpan.FromSeconds(3));
        }

        private async void search_QuerySubmitted(object s, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            string query = null;
            AutoSuggestBox sender = (AutoSuggestBox)s;
            if(args.ChosenSuggestion == null)
            {
                query = (await GeocodeHelper.SuggestAsync(args.QueryText))?.FirstOrDefault();
                sender.Text = query;
            }
            else
            {
                query = args.ChosenSuggestion as string;
            }
            if (query != null)
            {
                if (sender == searchFrom)
                    VM.GeocodeFromLocation(query);
                else
                    VM.GeocodeToLocation(query);
            }
        }

        private async void search_TextChanged(object s, AutoSuggestBoxTextChangedEventArgs args)
        {
            AutoSuggestBox sender = (AutoSuggestBox)s;
            // Only get results when it was a user typing, 
            // otherwise assume the value got filled in by TextMemberPath 
            // or the handler for SuggestionChosen.
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                if (string.IsNullOrWhiteSpace(sender.Text))
                    sender.ItemsSource = null;
                else
                {
                    var suggestions = await GeocodeHelper.SuggestAsync(sender.Text);
                    sender.ItemsSource = suggestions.ToList();
                }
            }
        }

        private void search_SuggestionChosen(object s, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            AutoSuggestBox sender = (AutoSuggestBox)s;
            var selection = args.SelectedItem as string;
            sender.Text = selection;
        }
    }
}
