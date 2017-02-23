using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;

namespace PortalBrowser.ViewModels
{
    /// <summary>
    /// Map View Model that handles all logic related to the map object
    /// </summary>
	public class MapVM : BaseViewModel
	{
		private PortalItem m_portalItem;

		public PortalItem PortalItem
		{
			get { return m_portalItem; }
			set { m_portalItem = value; LoadPortalItem(value); OnPropertyChanged("PortalItem"); }
		}
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

		private Map m_Map;

        /// <summary>
        /// Property holding the map item used by the MapView
        /// </summary>
		public Map Map
        {
			get { return m_Map; }
			set
			{
                if (m_Map != value)
                {
                    m_Map = value;
                    OnPropertyChanged("Map");
                }
			}
		}

		private string m_StatusMessage;
        /// <summary>
        /// Status message to inform user or loading progress
        /// </summary>
		public string StatusMessage
		{
			get { return m_StatusMessage; }
			set
			{
				m_StatusMessage = value;
				OnPropertyChanged("StatusMessage");
			}
		}

		private bool m_IsLoadingWebMap = true;
        /// <summary>
        /// Boolean to reflect whether the map has finished loading
        /// </summary>
		public bool IsLoadingWebMap
		{
			get { return m_IsLoadingWebMap; }
			set
			{
				m_IsLoadingWebMap = value;
				OnPropertyChanged("IsLoadingWebMap");
			}
		}
	}
}