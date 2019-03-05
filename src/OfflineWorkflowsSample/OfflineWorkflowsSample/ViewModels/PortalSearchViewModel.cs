using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Esri.ArcGISRuntime.Portal;
using Prism.Commands;
using Prism.Windows.Mvvm;

namespace OfflineWorkflowSample.ViewModels
{
    public class PortalSearchViewModel : ViewModelBase
    {
        private ArcGISPortal _portal;
        private string _searchQuery;
        private int _page = 1;
        private int _resultsPerPage = 25;
        private bool _includePublicResults = false;
        private int _totalResults = 0;

        public PortalSearchViewModel()
        {
            _goForwardCommand = new DelegateCommand(() => Page++, () => _totalResults >= _page * _resultsPerPage);
            _goBackCommand = new DelegateCommand(() => Page--, () => _page > 1);
        }

        public void Initialize(ArcGISPortal portal)
        {
            _portal = portal;
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

        public int TotalResults
        {
            get => _totalResults;
        }

        private async void UpdateQueryResult()
        {
            try
            {
                PortalQueryParameters parameters = new PortalQueryParameters(_searchQuery);
                parameters.CanSearchPublic = _includePublicResults;
                parameters.Limit = _resultsPerPage;
                parameters.StartIndex = (_page - 1) * _resultsPerPage;

                Items.Clear();
                var portalResults = await _portal.FindItemsAsync(parameters);
                SetProperty(ref _totalResults, portalResults.TotalResultsCount, nameof(TotalResults));
                foreach (var result in portalResults.Results)
                {
                    Items.Add(result);
                }

                // Go to the first page if the page is higher than the results should allow
                if (_page > (_totalResults / _resultsPerPage) + 1)
                {
                    Page = 1;
                }
                
                // Update commands
                _goForwardCommand.RaiseCanExecuteChanged();
                _goBackCommand.RaiseCanExecuteChanged();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public ObservableCollection<PortalItem> Items { get; } = new ObservableCollection<PortalItem>();

        private DelegateCommand _goBackCommand;
        private DelegateCommand _goForwardCommand;
        public ICommand GoBackCommand => _goBackCommand;
        public ICommand GoForwardCommand => _goForwardCommand;

        public event EventHandler SearchChanged;

        private void RaiseSearchChanged()
        {
            SearchChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
