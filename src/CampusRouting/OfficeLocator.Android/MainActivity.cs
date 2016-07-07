using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Esri.ArcGISRuntime.UI;
using System.Collections.Generic;
using Java.Lang;

namespace OfficeLocator
{
    /// <summary>
    /// The main activity for office location and routing
    /// </summary>
	[Activity(Label = "Esri Campus Viewer", MainLauncher = true, Icon = "@drawable/icon", Theme = "@android:style/Theme.NoTitleBar.Fullscreen" )]
	public class MainActivity : Activity
	{
        private MapViewModel VM;

        protected async override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.Main);

            var autocompleteTextView1 = FindViewById<AutoCompleteTextView>(Resource.Id.FromInput);
			autocompleteTextView1.TextChanged += AutocompleteTextView_TextChanged;
			autocompleteTextView1.ItemClick += AutocompleteTextViewFrom_ItemClick;
			autocompleteTextView1.Threshold = 0;
            autocompleteTextView1.Enabled = false;
            var autoCompleteAdapter = new SuggestGeocodeCompleteAdapter(this, Android.Resource.Layout.SimpleDropDownItem1Line);
			autocompleteTextView1.Adapter = autoCompleteAdapter;

			var autocompleteTextView2 = FindViewById<AutoCompleteTextView>(Resource.Id.ToInput);
			autocompleteTextView2.TextChanged += AutocompleteTextView_TextChanged;
			autocompleteTextView2.ItemClick += AutocompleteTextViewTo_ItemClick;
			autocompleteTextView2.Threshold = 0;
            autocompleteTextView2.Enabled = false;
            autoCompleteAdapter = new SuggestGeocodeCompleteAdapter(this, Android.Resource.Layout.SimpleDropDownItem1Line);
			autocompleteTextView2.Adapter = autoCompleteAdapter;

			RunOnUiThread(() =>
			{
				GridLayout panel = FindViewById<GridLayout>(Resource.Id.WalkTimePanel);
				panel.Visibility = ViewStates.Invisible;
			});
            
            GeoView campusView = FindViewById<GeoView>(Resource.Id.CampusView);
            // Cast to 2D and 3D versions of GeoView so we can handle either 
            // Try going to Resources\layout\Main.axml and change the MapView to a SceneView to enable a 3D view of the campus
            MapView mapView = campusView as MapView;
            SceneView sceneView = campusView as SceneView;

            VM = new MapViewModel();
			VM.RequestViewpoint += VM_RequestViewpoint;
			VM.RouteUpdated += VM_RouteUpdated;
            VM.PropertyChanged += VM_PropertyChanged;
			await VM.LoadAsync();
            RunOnUiThread(() =>
            {
                autocompleteTextView1.Enabled = true;
                autocompleteTextView2.Enabled = true;
                TextView statusView = FindViewById<TextView>(Resource.Id.downloadDataStatus);
                statusView.Visibility = ViewStates.Gone;
                if (mapView != null)
                {
                    mapView.BackgroundGrid.Color = System.Drawing.Color.FromArgb(255, 255, 255);
                    mapView.BackgroundGrid.GridLineWidth = 0;
                    mapView.Map = VM.Map;
                }
                else if (sceneView != null)
                {
                    sceneView.Scene = VM.Scene;
                }
                campusView.GraphicsOverlays = VM.Overlays;
            });
		}

        private void VM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "LoadStatus")
            {
                RunOnUiThread(() =>
                {
                    TextView statusView = FindViewById<TextView>(Resource.Id.downloadDataStatus);
                    statusView.Text = VM.LoadStatus;
                });
            }
        }

		private void VM_RouteUpdated(object sender, EventArgs e)
		{
			var panel = FindViewById<GridLayout>(Resource.Id.WalkTimePanel);
			if (VM.HasRoute)
			{
				panel.Visibility = ViewStates.Visible;
				TextView walkTime = FindViewById<TextView>(Resource.Id.textViewWalkTime);
				TextView walkTimeAlt = FindViewById<TextView>(Resource.Id.textViewWalkTimeAlt);
				walkTime.Text = VM.WalkTime;
				walkTimeAlt.Text = VM.WalkTimeAlt;
			}
			else
			{
				panel.Visibility = ViewStates.Invisible;
			}
		}

		private void VM_RequestViewpoint(object sender, Esri.ArcGISRuntime.Mapping.Viewpoint viewpoint)
		{
            GeoView campusView = FindViewById<GeoView>(Resource.Id.CampusView);
            campusView.SetViewpointAsync(viewpoint, TimeSpan.FromSeconds(3));
		}

		private void AutocompleteTextViewFrom_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
		{
			var autocompleteTextView = (AutoCompleteTextView)sender;
			DismissKeyboard(autocompleteTextView);
			VM.GeocodeFromLocation(autocompleteTextView.Text);
		}

		private void AutocompleteTextViewTo_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
		{
			var autocompleteTextView = (AutoCompleteTextView)sender;
			DismissKeyboard(autocompleteTextView);
			VM.GeocodeToLocation(autocompleteTextView.Text);
		}

		private async void AutocompleteTextView_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
		{
			var autocompleteTextView = (AutoCompleteTextView)sender;
			if (string.IsNullOrWhiteSpace(e.Text.ToString()))
			{
				var adapter = autocompleteTextView.Adapter as SuggestGeocodeCompleteAdapter;
				adapter.UpdateList(new string[] { });
			}
			else
			{
				string text = e.Text.ToString();
				var suggestions = await GeocodeHelper.SuggestAsync(e.Text.ToString());
				var adapter = autocompleteTextView.Adapter as SuggestGeocodeCompleteAdapter;
				adapter.UpdateList(suggestions);
			}
		}

		private void DismissKeyboard(AutoCompleteTextView textView)
		{
			var imm = (Android.Views.InputMethods.InputMethodManager)GetSystemService(Context.InputMethodService);
			imm.HideSoftInputFromWindow(textView.WindowToken, 0);
		}		
	}
}

