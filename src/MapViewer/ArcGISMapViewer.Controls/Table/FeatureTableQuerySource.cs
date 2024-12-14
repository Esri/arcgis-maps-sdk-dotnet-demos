using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Data;
using Microsoft.UI.Xaml.Data;
using Windows.Foundation;

namespace ArcGISMapViewer.Controls
{
    internal sealed partial class FeatureTableQuerySource : ObservableCollection<Feature>, ISupportIncrementalLoading
    {
        private readonly FeatureTable _table;
        private Exception? _error;
        private QueryParameters? _query;

        public FeatureTableQuerySource(FeatureTable table, QueryParameters? query)
        {
            _table = table;
            _query = query;
        }
        public EventHandler? IsBusyChanged;

        public EventHandler? ErrorChanged;

        public bool IsBusy { get; private set; }
        public static FeatureTableQuerySource Empty { get; } = new FeatureTableQuerySource(null!, null!);

        public bool HasMoreItems => _error is null && _query is not null;

        public bool HasNoResults => !HasMoreItems && Items.Count == 0 && this != Empty;

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count) => LoadMoreItemsTaskAsync(count).AsAsyncOperation();

        private async Task<LoadMoreItemsResult> LoadMoreItemsTaskAsync(uint count)
        {

            LoadMoreItemsResult loadMoreItemsResult = new LoadMoreItemsResult() { Count = 0 };
            if (_query is not null && !IsBusy)
            {
                int index = this.Items.Count;
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"Loading {count} more items");
#endif
                _query.MaxFeatures = Math.Max(10, (int)count); //Get at least 10
                FeatureQueryResult result;
                try
                {
                    IsBusy = true;
                    IsBusyChanged?.Invoke(this, EventArgs.Empty);
                    if (_table is ServiceFeatureTable stable)
                        result = await stable.QueryFeaturesAsync(_query, QueryFeatureFields.LoadAll);
                    else
                        result = await _table.QueryFeaturesAsync(_query);
                }
                catch (Exception ex)
                {
                    _error = ex;
                    IsBusy = false;
                    IsBusyChanged?.Invoke(this, EventArgs.Empty);
                    ErrorChanged?.Invoke(this, EventArgs.Empty);
                    return loadMoreItemsResult;
                }
                if (result.Any())
                {
                    var list = result.ToList();
                    if (list != null)
                    {
                        foreach (var item in list)
                            base.Items.Add(item);
                        try
                        {
                            OnCollectionChanged(new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized.NotifyCollectionChangedAction.Add, changedItems: list, index));
                        }
                        catch (System.Exception)
                        {

                        }
                        if (result.IsTransferLimitExceeded)
                            _query.ResultOffset += list.Count;
                        else
                            _query = null;
                        loadMoreItemsResult.Count = (uint)list.Count;
                    }
                }
                IsBusy = false;
                IsBusyChanged?.Invoke(this, EventArgs.Empty);
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
}
