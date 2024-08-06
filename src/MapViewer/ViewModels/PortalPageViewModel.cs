using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Esri.ArcGISRuntime.Portal;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Windows.Foundation;

namespace ArcGISMapViewer.ViewModels
{
    public partial class PortalPageViewModel : ObservableObject
    {
        private PortalPageViewModel()
        {
            ApplicationViewModel.Instance.PropertyChanged += Instance_PropertyChanged;
            Reload(null);
        }

        private void Instance_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(ApplicationViewModel.PortalUser))
            {
                Reload(null);
            }
        }

        public void SearchQuerySubmitted(AutoSuggestBox box, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            Reload(args.QueryText);
        }

        public static PortalPageViewModel Instance { get; } = new PortalPageViewModel();

        public async void Reload(string? query)
        {
            var portal = ApplicationViewModel.Instance.PortalUser?.Portal;
            SearchError = "";
            if (portal is null)
            {
                MapItems = null;
                return;
            }
            try
            {
                IsLoading = true;
                var queryParams = PortalQueryParameters.CreateForItemsOfTypes(
                   [PortalItemType.WebMap,
                    PortalItemType.FeatureService,
                    PortalItemType.WMS,
                    // PortalItemType.WMTS,
                    // PortalItemType.WFS,
                    PortalItemType.VectorTileService,
                    PortalItemType.MapService,
                    PortalItemType.KML, 
                    PortalItemType.FeatureCollection], search: query);
                var result = await portal.FindItemsAsync(queryParams);
                MapItems = new PortalItemQuerySource(portal, result);
            }
            catch(System.Exception ex)
            {
                SearchError = ex.Message;
            }
            IsLoading = false;
        }

        [ObservableProperty]
        private string _searchError;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private IEnumerable<PortalItem>? _mapItems;


        private class PortalItemQuerySource : ObservableCollection<PortalItem>, ISupportIncrementalLoading
        {
            private PortalQueryResultSet<PortalItem> _result;
            private readonly ArcGISPortal _portal;
            public PortalItemQuerySource(ArcGISPortal portal, PortalQueryResultSet<PortalItem> result)
            {
                _portal = portal;
                _result = result;
                foreach (var item in result.Results)
                    base.Items.Add(item);

            }
            public bool HasMoreItems => _result.NextQueryParameters is not null;

            public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count) => LoadMoreItemsTaskAsync(count).AsAsyncOperation();

            private async Task<LoadMoreItemsResult> LoadMoreItemsTaskAsync(uint count)
            {
                LoadMoreItemsResult loadMoreItemsResult = new LoadMoreItemsResult() { Count = 0 };
                if (_result.NextQueryParameters is not null)
                {
                    int index = this.Items.Count;
                    var query = _result.NextQueryParameters;
                    query.Limit = (int)count;
                    PortalQueryResultSet<PortalItem> result;
                    try
                    {
                        result = await _portal.FindItemsAsync(query);
                    }
                    catch (Exception)
                    {
                        return loadMoreItemsResult;
                    }
                    if (result.Results.Any())
                    {
                        var list = result.Results?.ToList();
                        if (list != null)
                        {
                            foreach (var item in list)
                                base.Items.Add(item);
                            OnCollectionChanged(new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized.NotifyCollectionChangedAction.Add, changedItems: list, index));
                            _result = result;
                            loadMoreItemsResult.Count = (uint)list.Count;
                        }
                    }
                }
                return loadMoreItemsResult;
            }
        }

    }
}
