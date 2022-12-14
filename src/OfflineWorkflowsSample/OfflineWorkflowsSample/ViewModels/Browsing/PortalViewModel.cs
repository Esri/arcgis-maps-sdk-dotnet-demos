using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using OfflineWorkflowSample.ViewModels;
using Prism.Windows.Mvvm;

namespace OfflineWorkflowSample
{
    public class PortalViewModel : ViewModelBase
    {
        private static readonly List<PortalItemType?> DefaultTypeFilters = new List<PortalItemType?>
        {
            null,
            PortalItemType.WebMap,
            PortalItemType.WebScene,
            PortalItemType.MobileMapPackage
        };

        private readonly List<Basemap> _defaultBasemaps = new List<Basemap>
        {
        };

        private readonly List<Basemap> _orgBasemaps = new List<Basemap>();

        private bool _offlineOnlyFilter;

        private string _searchFilter;

        private PortalFolderViewModel _selectedFolder;

        private PortalFolderViewModel _selectedGroup;

        private PortalItemType? _typeFilter;
        private List<PortalFolderViewModel> Folders { get; } = new List<PortalFolderViewModel>();
        private List<PortalFolderViewModel> Groups { get; } = new List<PortalFolderViewModel>();

        public List<PortalFolderViewModel> VisibleFolders => Folders.Where(folder => folder.SectionHasContent).ToList();
        public List<PortalFolderViewModel> VisibleGroups => Groups.Where(group => group.SectionHasContent).ToList();

        public PortalSearchViewModel SearchViewModel { get; } = new PortalSearchViewModel();

        public List<Basemap> OrgBasemaps => _orgBasemaps.Any() ? _orgBasemaps : _defaultBasemaps;

        public PortalFolderViewModel SelectedFolder
        {
            get => _selectedFolder;
            set => SetProperty(ref _selectedFolder, value);
        }

        public PortalFolderViewModel SelectedGroup
        {
            get => _selectedGroup;
            set => SetProperty(ref _selectedGroup, value);
        }

        public ArcGISPortal Portal { get; private set; }

        public string SearchFilter
        {
            get => _searchFilter;
            set
            {
                SetProperty(ref _searchFilter, value);

                foreach (PortalFolderViewModel container in Folders.Concat(Groups)) container.SearchFilter = value;

                HandleFilterChangesForFolders();
            }
        }

        public bool OfflineOnlyFilter
        {
            get => _offlineOnlyFilter;
            set
            {
                SetProperty(ref _offlineOnlyFilter, value);

                foreach (PortalFolderViewModel container in Folders.Concat(Groups)) container.OfflineOnlyFilter = OfflineOnlyFilter;

                HandleFilterChangesForFolders();
            }
        }

        public PortalItemType? TypeFilter
        {
            get => _typeFilter;
            set
            {
                SetProperty(ref _typeFilter, value);

                foreach (PortalFolderViewModel container in Folders.Concat(Groups)) container.TypeFilter = value;

                HandleFilterChangesForFolders();
            }
        }

        public List<PortalItemType?> AvailableTypeFilters => DefaultTypeFilters;

        public async Task LoadPortalAsync(ArcGISPortal portal)
        {
            Portal = portal;

            SearchViewModel.Initialize(portal);

            try
            {
                // Get 'featured content'
                IEnumerable<PortalItem> featuredItems = await portal.GetFeaturedItemsAsync();
                Folders.Add(new PortalFolderViewModel("Featured", featuredItems.ToList()));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                // Ignore - if the portal has no featured items, there will be no featured item group id,
                // which causes ArcGIS Runtime to throw an exception.
            }

            try
            {
                // Get the 'my content' group
                var userContent = await portal.User.GetContentAsync();
                Folders.Add(new PortalFolderViewModel("All my content", userContent.Items.ToList()));

                // Get all other folders
                foreach (PortalFolder folder in userContent.Folders)
                {
                    IEnumerable<PortalItem> itemsForFolder = await portal.User.GetContentAsync(folder.FolderId);
                    Folders.Add(new PortalFolderViewModel(folder.Title, itemsForFolder.ToList()));
                }

                // Get the groups
                foreach (var item in portal.User.Groups)
                {
                    PortalQueryParameters parameters = PortalQueryParameters.CreateForItemsInGroup(item.GroupId);
                    PortalQueryResultSet<PortalItem> itemResults = await portal.FindItemsAsync(parameters);
                    // TO-DO - update for query pagination
                    Groups.Add(new PortalFolderViewModel(item.Title, itemResults.Results.ToList()));
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                // Ignore
            }

            try
            {
                // Get the basemaps.
                _orgBasemaps.Clear();
                _orgBasemaps.AddRange(await Portal.GetBasemapsAsync());

                // Make sure the basemaps are loaded - ensures the name shows up in the UI.
                foreach (var item in OrgBasemaps)
                {
                    await item.LoadAsync();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                // Ignore
            }

            // Set the initial selections.
            SelectedFolder = Folders.FirstOrDefault();
            SelectedGroup = Groups.FirstOrDefault();
        }

        private void HandleFilterChangesForFolders()
        {
            RaisePropertyChanged(nameof(VisibleFolders));
            RaisePropertyChanged(nameof(VisibleGroups));

            // Set the initial selections now that visible folders have changed.
            SelectedFolder = VisibleFolders.FirstOrDefault();
            SelectedGroup = VisibleGroups.FirstOrDefault();
        }
    }

    public class PortalFolderViewModel : ViewModelBase
    {
        private readonly List<PortalItemViewModel> _allItems;
        private bool _offlineOnly;
        private string _searchFilter;
        private PortalItemType? _typeFilter;

        public PortalFolderViewModel(string title, List<PortalItem> items)
        {
            _allItems = items.Select(item => new PortalItemViewModel(item)).ToList();
            Title = title;
        }

        public string Title { get; }

        public string SearchFilter
        {
            get => _searchFilter;
            set
            {
                SetProperty(ref _searchFilter, value);
                RaisePropertyChanged(nameof(Items));
                RaisePropertyChanged(nameof(SectionHasContent));
            }
        }

        public PortalItemType? TypeFilter
        {
            get => _typeFilter;
            set
            {
                SetProperty(ref _typeFilter, value);
                RaisePropertyChanged(nameof(Items));
                RaisePropertyChanged(nameof(SectionHasContent));
            }
        }

        public bool OfflineOnlyFilter
        {
            get => _offlineOnly;
            set
            {
                SetProperty(ref _offlineOnly, value);
                RaisePropertyChanged(nameof(Items));
                RaisePropertyChanged(nameof(SectionHasContent));
            }
        }

        public IEnumerable<PortalItemViewModel> Items
        {
            get
            {
                IEnumerable<PortalItemViewModel> items = _allItems;
                if (!String.IsNullOrWhiteSpace(SearchFilter))
                {
                    items = items.Where(item => item.Item.Title.Contains(SearchFilter));
                }

                if (TypeFilter != null)
                {
                    items = items.Where(item => item.Item is PortalItem).Where(item => ((PortalItem)item.Item).Type == TypeFilter);
                }

                if (_offlineOnly)
                {
                    items = items.Where(item => item.Item.TypeKeywords.Contains("Offline"));
                }

                return items;
            }
        }

        public bool SectionHasContent => Items.Any();
    }
}