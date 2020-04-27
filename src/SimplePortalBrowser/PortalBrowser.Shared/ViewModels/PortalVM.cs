using Esri.ArcGISRuntime.Portal;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace PortalBrowser.ViewModels
{
	/// <summary>
	/// Portal View Model that handles all logic related to the portal object.
	/// </summary>
	public class PortalVM : BaseViewModel
	{
		private PortalInfo _portalInfo;
		private IEnumerable<MapGroup> _groups;
		private IEnumerable<PortalItem> _basemaps;
		private IEnumerable<PortalItem> _featured;
		private string _statusMessage = "OK";
		private bool _isLoadingPortal = true;
		private bool _isLoadingBasemaps = true;

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
			catch (System.Exception ex)
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
			StatusMessage = $"Connected to {portal.PortalInfo.PortalName} ({portal.PortalInfo.PortalName})";
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
		/// Gets or sets information about the loaded portal.
		/// </summary>
		public PortalInfo PortalInfo
		{
			get { return _portalInfo; }
			set
			{
				_portalInfo = value;
				OnPropertyChanged(nameof(PortalInfo));
			}
		}
		
		/// <summary>
		/// Gets or sets the list of map groups created from the portal.
		/// </summary>
		public IEnumerable<MapGroup> Groups
		{
			get { return _groups; }
			set
			{
				_groups = value;
				OnPropertyChanged(nameof(Groups));
			}
		}
		
		/// <summary>
		/// Gets or sets the list of basemaps.
		/// </summary>
		public IEnumerable<PortalItem> Basemaps
		{
			get { return _basemaps; }
			set
			{
				_basemaps = value;
				OnPropertyChanged(nameof(Basemaps));
			}
		}
		
		/// <summary>
		/// Gets or setsthe list of featured maps.
		/// </summary>
		public IEnumerable<PortalItem> Featured
		{
			get { return _featured; }
			set
			{
				_featured = value;
				OnPropertyChanged(nameof(Featured));
			}
		}

		
		/// <summary>
		/// Gets or sets the status message to inform the user of progress.
		/// </summary>
		public string StatusMessage
		{
			get { return _statusMessage; }
			set
			{
				_statusMessage = value;
				OnPropertyChanged(nameof(StatusMessage));
			}
		}
		
		/// <summary>
		/// Gets or sets a value that indicates whether the portal has finished loading.
		/// </summary>
		public bool IsLoadingPortal
		{
			get { return _isLoadingPortal; }
			set
			{
				_isLoadingPortal = value;
				OnPropertyChanged(nameof(IsLoadingPortal));
				OnPropertyChanged(nameof(IsLoading));
			}
		}
		
		/// <summary>
		/// Gets or sets a value that indicates whether the basemaps have finished loading.
		/// </summary>
		public bool IsLoadingBasemaps
		{
			get { return _isLoadingBasemaps; }
			set
			{
				_isLoadingBasemaps = value;
				OnPropertyChanged(nameof(IsLoadingBasemaps));
				OnPropertyChanged(nameof(IsLoading));
			}
		}

		/// <summary>
		/// Gets a value that indicates whether any item is currently loading.
		/// </summary>
		public bool IsLoading
		{
			get
			{
				return IsLoadingBasemaps && IsLoadingPortal;
			}
		}
	}
	/// <summary>
	/// Class that holds map groups with name and map items
	/// </summary>
	public class MapGroup
	{
		public string Name { get; set; }

		public IEnumerable<PortalItem> Items { get; set; }
	}
}