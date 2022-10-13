using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;

namespace PortalBrowser.ViewModels;

/// <summary>
/// Map View Model that handles all logic related to the map object
/// </summary>
public class MapVM : BaseViewModel
{
    private PortalItem? m_portalItem;
    private Map? m_Map;
    private string m_StatusMessage = "Not yet initialized";
    private bool m_IsLoadingWebMap = true;

    public PortalItem? PortalItem
    {
        get => m_portalItem;
        set
        {
            if (value != m_portalItem)
            {
                m_portalItem = value;
                OnPropertyChanged();
                LoadPortalItem(value);
            }
        }
    }

    /// <summary>
    /// Method runs when a portal item is selected by the user
    /// </summary>
    /// <param name="item">Item selected by user</param>
	private async void LoadPortalItem(PortalItem? item)
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
        catch (Exception ex)
        {
            StatusMessage = "Webmap load failed: " + ex.Message;
            IsLoadingWebMap = false;
        }
    }

    /// <summary>
    /// Property holding the map item used by the MapView
    /// </summary>
	public Map? Map
    {
        get => m_Map;
        set => SetProperty(ref m_Map, value);
    }

    /// <summary>
    /// Status message to inform user or loading progress
    /// </summary>
	public string StatusMessage
    {
        get => m_StatusMessage;
        set => SetProperty(ref m_StatusMessage, value);
    }

    /// <summary>
    /// Boolean to reflect whether the map has finished loading
    /// </summary>
	public bool IsLoadingWebMap
    {
        get => m_IsLoadingWebMap;
        set => SetProperty(ref m_IsLoadingWebMap, value);
    }
}