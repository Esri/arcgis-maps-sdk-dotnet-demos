using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RoutingSample.ViewModels
{
	/// <summary>
	/// View model for main page
	/// </summary>
	public class MainPageVM : ModelBase
	{
		#region Fields
		private CancellationTokenSource m_routeTaskCancellationToken;
		private string m_RouteToAddress;
		private bool firstLocation = true;
		private RouteDataSource m_routeDataSource;
		private bool m_IsCalculatingRoute;
		private Viewpoint m_ViewpointRequested;
		private string m_RouteCalculationErrorMessage;
		private LocationDisplay m_locationDisplay;
		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="MainPageVM"/> class.
		/// </summary>
		public MainPageVM()
		{
			if(IsDesignMode)
				Route = new RouteDataSource(null);
		}

		#region ViewModel Properties

		/// <summary>
		/// Gets or sets the address to route to. Setting this recalculates the route
		/// </summary>
		public string RouteToAddress
		{
			get { return m_RouteToAddress; }
			set
			{
				if (m_RouteToAddress != value)
				{
					m_RouteToAddress = value;
					RaisePropertyChanged("RouteToAddress");
					GenerateRoute(value);
				}
			}
		}

		/// <summary>
		/// The current route
		/// </summary>
		public RouteDataSource Route
		{
			get { return m_routeDataSource; }
			private set
			{
				m_routeDataSource = value;
				RaisePropertyChanged("Route");
				if (m_locationDisplay != null)
				{
					if(value != null)
						m_locationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Navigation;
					else
						m_locationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Off;
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether a route calculate is currently in progress
		/// </summary>
		public bool IsCalculatingRoute
		{
			get { return m_IsCalculatingRoute; }
			private set
			{
				m_IsCalculatingRoute = value;
				RaisePropertyChanged("IsCalculatingRoute");
			}
		}

		/// <summary>
		/// Gets the route calculation error message (if any)
		/// </summary>
		public string RouteCalculationErrorMessage
		{
			get { return m_RouteCalculationErrorMessage; }
			private set
			{
				m_RouteCalculationErrorMessage = value;
				RaisePropertyChanged("RouteCalculationErrorMessage");
			}
		}

		/// <summary>
		/// Used for requesting an extent for the mapView to ZoomTo
		/// </summary>
		public Viewpoint ViewpointRequested
		{
			get { return m_ViewpointRequested; }
			private set
			{
				m_ViewpointRequested = value;
				RaisePropertyChanged("ViewpointRequested");
			}
        }

        /// <summary>
        /// The current location display used for displaying location on the mapView
        /// </summary>
        public LocationDisplay LocationDisplay
        {
            get { return m_locationDisplay; }
            set
            {
                if (m_locationDisplay != null)
                    m_locationDisplay.PropertyChanged -= LocationDisplay_PropertyChanged;
                m_locationDisplay = value;
                if(m_locationDisplay != null)
                {                    
                    m_locationDisplay.PropertyChanged += LocationDisplay_PropertyChanged;
                    m_locationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Navigation;
                    m_locationDisplay.Start();
                }
                RaisePropertyChanged("LocationDisplay");
            }
        }

        #endregion

        #region Methods

        private async void GenerateRoute(string address)
		{
			if (!string.IsNullOrWhiteSpace(address) && LocationDisplay != null && LocationDisplay.Location != null)
			{
				if (m_routeTaskCancellationToken != null)
					m_routeTaskCancellationToken.Cancel();
				try //this is an async void method, so we need to try/catch everything that's async
				{
					RouteCalculationErrorMessage = null;
					m_routeTaskCancellationToken = new CancellationTokenSource();
					IsCalculatingRoute = true;
					var result = await new Models.RouteService().GetRoute(
						address, LocationDisplay.Location.Position, 
						m_routeTaskCancellationToken.Token);

					m_routeTaskCancellationToken = null;
					Route = new RouteDataSource(result);
					Route.SetCurrentLocation(LocationDisplay.Location.Position);
#if DEBUG
					// When debugging use a simulator for the generated route
					LocationDisplay.DataSource = new RouteLocationSimulator(result);
#endif
					LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Navigation;
				}
				catch (OperationCanceledException)
				{
					//Do nothing when calculation was cancelled
				}
				catch (System.Exception ex)
				{
					RouteCalculationErrorMessage = ex.Message;
				}
				finally
				{
					IsCalculatingRoute = false;
				}
			}
		}

		//When location changes, push this location to the route datasource
		private void LocationDisplay_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "CurrentLocation")
			{
				if (LocationDisplay.Location != null && !LocationDisplay.Location.Position.IsEmpty)
				{
					if (Route != null)
						Route.SetCurrentLocation(LocationDisplay.Location.Position);
					if (firstLocation)
					{
						var accuracy = double.IsNaN(LocationDisplay.Location.HorizontalAccuracy) ? 0 :
							LocationDisplay.Location.HorizontalAccuracy;
						ViewpointRequested = new Viewpoint(
							GeometryEngine.BufferGeodesic(LocationDisplay.Location.Position, accuracy + 500, LinearUnits.Meters)
						);
						firstLocation = false;
						if (Route == null && m_routeTaskCancellationToken == null && !string.IsNullOrWhiteSpace(RouteToAddress)) //calculate route now
							//Calculate a route from the address
							GenerateRoute(RouteToAddress);
					}
				}
			}
		}
		#endregion
	}
}
