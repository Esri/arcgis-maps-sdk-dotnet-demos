using Esri.ArcGISRuntime.Portal;
using System.Collections.ObjectModel;

namespace PortalBrowser.ViewModels;


/// <summary>
/// Portal View Model that handles all logic related to the portal object
/// </summary>
public class PortalVM : BaseViewModel
{
    private PortalInfo? m_portalInfo;
    private IEnumerable<MapGroup>? m_groups;
    private IEnumerable<PortalItem>? m_Featured;
    private IEnumerable<PortalItem>? m_Basemaps;
    private string m_StatusMessage = "OK";
    private bool m_IsLoadingPortal = true;
    private bool m_IsLoadingBasemaps = true;

    public PortalVM()
    {
        Initialize();
    }

    private async void Initialize()
    {
        try
        {
            await LoadPortal();
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
    }

    /// <summary>
    /// Async task to load a portal instance
    /// </summary>
    private async Task LoadPortal()
    {
        StatusMessage = "Initializing Portal...";
        var portal = await ArcGISPortal.CreateAsync();
        PortalInfo = portal.PortalInfo;
        IsLoadingPortal = false;
        await LoadMaps(portal);
    }

    /// <summary>
    /// Async task to load maps from the portal instance and put them in groups
    /// </summary>
    /// <param name="portal">Portal instance</param>
    /// <returns></returns>
    private async Task LoadMaps(ArcGISPortal portal)
    {
        StatusMessage = "Loading maps...";

        var task1 = portal.GetBasemapsAsync();
        var items = await task1;
        Basemaps = items.Select(b => b.Item).OfType<PortalItem>();
        var groups = new ObservableCollection<MapGroup>();
        Groups = groups;
        groups.Add(new MapGroup() { Name = "Base maps", Items = Basemaps });
        IsLoadingBasemaps = false;
        StatusMessage = $"Connected to {portal.PortalInfo?.PortalName}";
        foreach (var item in await portal.GetFeaturedGroupsAsync())
        {
            var query = PortalQueryParameters.CreateForItemsOfTypeInGroup(PortalItemType.WebMap, item.GroupId);
            query.Limit = 20;
            var result = await portal.FindItemsAsync(query);
            if (result.TotalResultsCount > 0)
            {
                groups.Add(new MapGroup() { Name = item.Title, Items = result.Results });
                if (Featured == null)
                    Featured = result.Results;
            }
        }
    }

    /// <summary>
    /// Property holds information about the loaded portal 
    /// </summary>
    public PortalInfo? PortalInfo
    {
        get => m_portalInfo;
        set => SetProperty(ref m_portalInfo, value);
    }

    /// <summary>
    /// Groups property to hold the map groups created from the portal
    /// </summary>
    public IEnumerable<MapGroup>? Groups
    {
        get => m_groups;
        set => SetProperty(ref m_groups, value);
    }

    /// <summary>
    /// Property holding the list of basemaps to be added to the UI
    /// </summary>
    public IEnumerable<PortalItem>? Basemaps
    {
        get => m_Basemaps;
        set => SetProperty(ref m_Basemaps, value);
    }

    /// <summary>
    /// Property holding the list of featured maps to be added to the UI
    /// </summary>
    public IEnumerable<PortalItem>? Featured
    {
        get => m_Featured;
        set => SetProperty(ref m_Featured, value);
    }

    /// <summary>
    /// Property holding the status message to inform user of progress
    /// </summary>
    public string StatusMessage
    {
        get => m_StatusMessage;
        set => SetProperty(ref m_StatusMessage, value);
    }

    /// <summary>
    /// Boolean to reflect whether the portal has finished loading
    /// </summary>
    public bool IsLoadingPortal
    {
        get => m_IsLoadingPortal;
        set
        {
            m_IsLoadingPortal = value;
            OnPropertyChanged(nameof(IsLoadingPortal));
            OnPropertyChanged(nameof(IsLoading));
        }
    }

    /// <summary>
    /// Boolean to reflect whether the basemaps have finished loading
    /// </summary>
    public bool IsLoadingBasemaps
    {
        get => m_IsLoadingBasemaps;
        set
        {
            m_IsLoadingBasemaps = value;
            OnPropertyChanged(nameof(IsLoadingBasemaps));
            OnPropertyChanged(nameof(IsLoading));
        }
    }

    /// <summary>
    /// Composite property to reflect if any item is currently loading
    /// </summary>
    public bool IsLoading => IsLoadingBasemaps && IsLoadingPortal;
}

/// <summary>
/// Class that holds map groups with name and map items
/// </summary>
public class MapGroup
{
    public string? Name { get; set; }
    public IEnumerable<PortalItem>? Items { get; set; }
}