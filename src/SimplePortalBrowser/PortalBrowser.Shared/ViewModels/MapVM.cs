using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;

namespace PortalBrowser.ViewModels
{
    /// <summary>
    /// Map View Model that handles all logic related to the map object
    /// </summary>
	public class MapVM : BaseViewModel
	{
		private PortalItem _portalItem;
		private Map _map;
		private string _statusMessage;
		private bool _isLoadingWebMap = true;

		/// <summary>
		/// Method runs when a portal item is selected by the user
		/// </summary>
		/// <param name="item">Item selected by user</param>
		private async void LoadPortalItem(PortalItem item)
		{
			try
			{
				if (item == null)
                    Map = null;
				else
				{
                    StatusMessage = "Loading Webmap...";
                    IsLoadingWebMap = true;
                    // Create a new map from the portal item and load it
                    Map = new Map(item);
                    await Map.LoadAsync();
                    Map = Map;
                    IsLoadingWebMap = false;
                    StatusMessage = "";
                }
			}
			catch (System.Exception ex)
			{
				StatusMessage = "Webmap load failed: " + ex.Message;
				IsLoadingWebMap = false;
			}
		}

		/// <summary>
		/// Gets or sets the selected PortalItem.
		/// </summary>
		public PortalItem PortalItem
		{
			get { return _portalItem; }
			set { _portalItem = value; LoadPortalItem(value); OnPropertyChanged("PortalItem"); }
		}

		/// <summary>
		/// Gets or sets the map item used by the MapView.
		/// </summary>
		public Map Map
        {
			get { return _map; }
			set
			{
                if (_map != value)
                {
                    _map = value;
                    OnPropertyChanged("Map");
                }
			}
		}
		
        /// <summary>
        /// Gets or sets a status message to inform user of loading progress.
        /// </summary>
		public string StatusMessage
		{
			get { return _statusMessage; }
			set
			{
				_statusMessage = value;
				OnPropertyChanged("StatusMessage");
			}
		}

        /// <summary>
        /// Gets or sets a value that indicates whether the map has finished loading.
        /// </summary>
		public bool IsLoadingWebMap
		{
			get { return _isLoadingWebMap; }
			set
			{
				_isLoadingWebMap = value;
				OnPropertyChanged("IsLoadingWebMap");
			}
		}
	}
}