using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Esri.ArcGISRuntime.Portal;
using Prism.Commands;
using Prism.Windows.Mvvm;

namespace OfflineWorkflowSample.ViewModels
{
    public class PortalSearchViewModel : ViewModelBase
    {
        private readonly DelegateCommand _goBackCommand;
        private readonly DelegateCommand _goForwardCommand;
        private const int ResultsPerPage = 25;
        private bool _includePublicResults;
        private int _page = 1;
        private ArcGISPortal _portal;
        private string _searchQuery;
        private int _totalResults;

        public PortalSearchViewModel()
        {
            _goForwardCommand = new DelegateCommand(() => Page++, () => _totalResults >= _page * ResultsPerPage);
            _goBackCommand = new DelegateCommand(() => Page--, () => _page > 1);
        }

        public string SearchQueryText
        {
            get => _searchQuery;
            set
            {
                if (value != _searchQuery)
                {
                    SetProperty(ref _searchQuery, value);
                    RaiseSearchChanged();
                    UpdateQueryResult();
                }
            }
        }

        public int Page
        {
            get => _page;
            set
            {
                if (value != _page)
                {
                    SetProperty(ref _page, value);
                    UpdateQueryResult();
                }
            }
        }

        public bool IncludePublicResults
        {
            get => _includePublicResults;
            set
            {
                if (value != _includePublicResults)
                {
                    SetProperty(ref _includePublicResults, value);
                    UpdateQueryResult();
                }
            }
        }

        public int TotalResults => _totalResults;

        public ObservableCollection<PortalItemViewModel> SearchResults { get; } = new ObservableCollection<PortalItemViewModel>();
        public ICommand GoBackCommand => _goBackCommand;
        public ICommand GoForwardCommand => _goForwardCommand;

        public void Initialize(ArcGISPortal portal)
        {
            _portal = portal;
        }

        private async void UpdateQueryResult()
        {
            try
            {
                PortalQueryParameters parameters = new PortalQueryParameters(_searchQuery)
                {
                    CanSearchPublic = _includePublicResults,
                    Limit = ResultsPerPage,
                    StartIndex = (_page - 1) * ResultsPerPage
                };

                SearchResults.Clear();
                PortalQueryResultSet<PortalItem> portalResults = await _portal.FindItemsAsync(parameters);
                SetProperty(ref _totalResults, portalResults.TotalResultsCount, nameof(TotalResults));
                foreach (var result in portalResults.Results)
                {
                    SearchResults.Add(new PortalItemViewModel(result));
                }

                // Go to the first page if the page is higher than the results should allow
                if (_page > _totalResults / ResultsPerPage + 1) Page = 1;

                // Update commands
                _goForwardCommand.RaiseCanExecuteChanged();
                _goBackCommand.RaiseCanExecuteChanged();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public event EventHandler SearchChanged;

        private void RaiseSearchChanged()
        {
            SearchChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}