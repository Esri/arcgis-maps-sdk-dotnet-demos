﻿using System;
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
            LoadUserItems();
        }

        private void Instance_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(ApplicationViewModel.PortalUser))
            {
                UserItems.Clear();
                if (ApplicationViewModel.Instance.PortalUser is not null)
                    LoadUserItems();
                MapItems = PortalItemQuerySource.Empty;
            }
        }

        public void SearchQuerySubmitted(AutoSuggestBox box, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            Search(args.QueryText);
        }

        public ObservableCollection<PortalItem> UserItems = new ObservableCollection<PortalItem>();

        public static PortalPageViewModel Instance { get; } = new PortalPageViewModel();

        public void Search(string? query)
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
                var queryParams = CreateQuery(query);
                MapItems = new PortalItemQuerySource(portal, queryParams);
            }
            catch (System.Exception ex)
            {
                SearchError = ex.Message;
            }
        }

        public async void LoadUserItems()
        { 
            try
            {
                var contents = await ApplicationViewModel.Instance.PortalUser!.GetContentAsync();
                UserItems.Clear();
                foreach (var item in contents.Items)
                {
                    if (SupportedItemTypes.Contains(item.Type))
                        UserItems.Add(item);
                }
            }
            catch
            {
            }
        }

        private static PortalItemType[] SupportedItemTypes =
        {
                    PortalItemType.WebMap,
                    PortalItemType.FeatureService,
                    PortalItemType.WMS,
                    // PortalItemType.WMTS,
                    // PortalItemType.WFS,
                    PortalItemType.VectorTileService,
                    PortalItemType.MapService,
                    PortalItemType.KML,
                    PortalItemType.FeatureCollection,
                    PortalItemType.SceneService,
                    PortalItemType.WebScene,
        };

        private static PortalQueryParameters CreateQuery(string? query)
        {
            return PortalQueryParameters.CreateForItemsOfTypes(SupportedItemTypes, search: query);
        }

        [ObservableProperty]
        public partial string? SearchError { get; set; }

        [ObservableProperty]
        public partial PortalItemQuerySource? MapItems { get; set; } = PortalItemQuerySource.Empty;

        public partial class PortalItemQuerySource : ObservableCollection<PortalItem>, ISupportIncrementalLoading
        {
            private readonly ArcGISPortal _portal;
            private Exception? _error;
            private PortalQueryParameters? _query;
    
            public PortalItemQuerySource(ArcGISPortal portal, PortalQueryParameters? query)
            {
                _portal = portal;
                _query = query;
            }
            public static PortalItemQuerySource Empty { get; } = new PortalItemQuerySource(null!, null!);

            public bool HasMoreItems => _error is null && _query is not null;

            public bool HasNoResults => !HasMoreItems && Items.Count == 0 && this != Empty;

            public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count) => LoadMoreItemsTaskAsync(count).AsAsyncOperation();

            private async Task<LoadMoreItemsResult> LoadMoreItemsTaskAsync(uint count)
            {
                LoadMoreItemsResult loadMoreItemsResult = new LoadMoreItemsResult() { Count = 0 };
                if (_query is not null)
                {
                    int index = this.Items.Count;
                    System.Diagnostics.Debug.WriteLine($"Loading {count} more items");
                    _query.Limit = Math.Max(10, (int)count); //Get at least 10
                    PortalQueryResultSet<PortalItem> result;
                    try
                    {
                        result = await _portal.FindItemsAsync(_query);
                    }
                    catch (Exception ex)
                    {
                        _error = ex;
                        return loadMoreItemsResult;
                    }
                    _query = null;
                    if (result.Results.Any())
                    {
                        var list = result.Results?.ToList();
                        if (list != null)
                        {
                            foreach (var item in list)
                                base.Items.Add(item);
                            OnCollectionChanged(new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized.NotifyCollectionChangedAction.Add, changedItems: list, index));
                            _query = result.NextQueryParameters;
                            loadMoreItemsResult.Count = (uint)list.Count;
                        }
                    }
                    if (_query is null)
                    {
                        OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(nameof(HasMoreItems)));
                        if (Items.Count == 0)
                            OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(nameof(HasNoResults)));
                    }
                }
                return loadMoreItemsResult;
            }
        }

        public partial class PortalGroupItemQuerySource : ObservableCollection<PortalItem>, ISupportIncrementalLoading
        {
            private Exception? _error;
            private PortalGroupContentSearchParameters? _query;
            private readonly PortalGroup _group;
            public PortalGroupItemQuerySource(PortalGroup group, PortalGroupContentSearchParameters? groupQuery)
            {
                _group = group;
                _query = groupQuery;
            }
            public static PortalItemQuerySource Empty { get; } = new PortalItemQuerySource(null!, null);

            public bool IsEmpty => !HasMoreItems && Items.Count == 0;

            public bool HasMoreItems => _error is null && _query is not null;

            public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count) => LoadMoreItemsTaskAsync(count).AsAsyncOperation();

            private async Task<LoadMoreItemsResult> LoadMoreItemsTaskAsync(uint count)
            {
                LoadMoreItemsResult loadMoreItemsResult = new LoadMoreItemsResult() { Count = 0 };
                if (_query is not null || _query is not null)
                {
                    int index = this.Items.Count;
                    System.Diagnostics.Debug.WriteLine($"Loading {count} more items");
                    _query.Limit = Math.Max(10, (int)count); //Get at least 10
                    PortalGroupContentSearchResultSet result;
                    try
                    {
                        result = await _group.FindItemsAsync(_query);
                    }
                    catch (Exception ex)
                    {
                        _error = ex;
                        return loadMoreItemsResult;
                    }
                    _query = null;
                    if (result.Results.Any())
                    {
                        var list = result.Results?.ToList();
                        if (list != null)
                        {
                            foreach (var item in list)
                                base.Items.Add(item);
                            OnCollectionChanged(new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized.NotifyCollectionChangedAction.Add, changedItems: list, index));
                            _query = result.NextSearchParameters;
                            loadMoreItemsResult.Count = (uint)list.Count;
                        }
                    }
                    if (_query is null)
                    {
                        OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(nameof(HasMoreItems)));
                        if (Items.Count == 0)
                            OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(nameof(IsEmpty)));
                    }
                }
                return loadMoreItemsResult;
            }
        }

    }
}
