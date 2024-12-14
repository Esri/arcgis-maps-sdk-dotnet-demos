using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Esri.ArcGISRuntime.Data;
using System.Threading;
using System.Diagnostics;

namespace ArcGISMapViewer.Controls
{
    public sealed partial class FeatureTableView : Control
    {
        private CancellationTokenSource? featureQueryTokenSource;
        private static SolidColorBrush oddRowBackground = new SolidColorBrush(Windows.UI.Color.FromArgb(20, 0, 0, 0));
        private FeatureTableQuerySource? datasource;
        private bool isDefaultSizingApplied = false;
        public FeatureTableView()
        {
            this.DefaultStyleKey = typeof(FeatureTableView);
            SizeChanged += FeatureTableView_SizeChanged;
        }

        private void FeatureTableView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!isDefaultSizingApplied)
                SetDefaultColumnSize(e.NewSize.Width);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if(GetTemplateChild("ScrollViewHost") is ScrollView scrollView)
            {
                scrollView.ViewChanged += ScrollView_ViewChanged;
                scrollView.EffectiveViewportChanged += ScrollViewEffectiveViewportChangedHandler;
                scrollView.ExtentChanged += ScrollView_ExtentChanged;
            }

            if (GetTemplateChild("ItemsPresenter") is ItemsRepeater itemsPresenter)
            {
                itemsPresenter.ElementPrepared += ItemsPresenter_ElementPrepared;
                itemsPresenter.ItemsSource = datasource;
            }
            SetIsBusyState();
            if (Table?.LoadStatus == Esri.ArcGISRuntime.LoadStatus.FailedToLoad)
                VisualStateManager.GoToState(this, "TableFailedToLoad", false);
        }

        private void SetDefaultColumnSize(double width)
        {
            if (Columns is null) return;
            isDefaultSizingApplied = true;
            double reserved = Columns.Count * 6;
            int nanSizeCount = 0;
            foreach (var item in Columns)
            {
                if (!double.IsNaN(item.Width))
                    reserved += item.Width;
                else nanSizeCount++;
            }
            if (nanSizeCount > 0)
            {
                var columnWidth = Math.Max(50, (width - reserved) / nanSizeCount);
                foreach (var item in Columns)
                {
                    if (double.IsNaN(item.Width))
                        item.Width = Math.Min(150, Math.Max(50, columnWidth));
                }
            }
            if (GetTemplateChild("GridLines") is ItemsControl elm)
            {
                // Trigger re-layout, due to issue with Width binding not updating on first load
                elm.ItemsSource = null;
                elm.ItemsSource = Columns;
            }
        }

        private void ScrollView_ExtentChanged(ScrollView sender, object args) => LoadMoreData(sender);

        private void ScrollViewEffectiveViewportChangedHandler(FrameworkElement sender, EffectiveViewportChangedEventArgs args)
        {
            if (sender is ScrollView scrollView)
                LoadMoreData(scrollView);
        }

        private void ItemsPresenter_ElementPrepared(ItemsRepeater sender, ItemsRepeaterElementPreparedEventArgs args)
        {
            if (args.Element is FeatureDataRow row)
            {
                row.Columns = Columns;
                //row.Fields = Table?.Fields;
                if (datasource is not null && datasource.Count > args.Index)
                    row.Feature = datasource[args.Index];
                row.Background = args.Index % 2 == 0 ? null : oddRowBackground;
            }
        }

        public FeatureTable? Table
        {
            get { return (FeatureTable?)GetValue(TableProperty); }
            set { SetValue(TableProperty, value); }
        }

        public static readonly DependencyProperty TableProperty =
            DependencyProperty.Register(nameof(Table), typeof(FeatureTable), typeof(FeatureTableView), new PropertyMetadata(null, ((s, e) => ((FeatureTableView)s).OnTablePropertyChanged(e.OldValue as FeatureTable, e.NewValue as FeatureTable))));

        private void OnTablePropertyChanged(FeatureTable? oldTable, FeatureTable? newTable)
        {
            if (oldTable is not null)
            {
                // Clear out data
                featureQueryTokenSource?.Cancel();
                featureQueryTokenSource = null;
                datasource = null;
                if (GetTemplateChild("ItemsPresenter") is ItemsRepeater itemsPresenter)
                {
                    itemsPresenter.ItemsSource = null;
                }
            }
            if (newTable is not null)
            {
                LoadData(newTable);
            }
        }

        public string? WhereClause
        {
            get { return (string?)GetValue(WhereClauseProperty); }
            set { SetValue(WhereClauseProperty, value); }
        }

        public static readonly DependencyProperty WhereClauseProperty =
            DependencyProperty.Register("WhereClause", typeof(string), typeof(FeatureTableView), new PropertyMetadata(null));

        private void ScrollView_ViewChanged(ScrollView sender, object e) => LoadMoreData(sender);

        private void LoadMoreData(ScrollView sender)
        { 
            if (datasource is null || !datasource.HasMoreItems || datasource.IsBusy) return;
            var distanceToEnd = sender.ExtentHeight - (sender.VerticalOffset + sender.ViewportHeight);
            if (distanceToEnd <= 2.0 * sender.ViewportHeight)
            {
                _ = datasource.LoadMoreItemsAsync((uint)(sender.ViewportHeight * 3 / 24d));
            }
        }
        
        private async void LoadData(FeatureTable? newTable)
        {
            if (newTable is null || newTable.LoadStatus == Esri.ArcGISRuntime.LoadStatus.FailedToLoad)
                return;
            featureQueryTokenSource = new CancellationTokenSource();
            // Load data
            ServiceFeatureTable? stable = newTable as ServiceFeatureTable;
            
            var parameters = new QueryParameters() { WhereClause = string.IsNullOrEmpty(WhereClause) ? "1=1" : WhereClause, ReturnGeometry = true };
            if(newTable.LoadStatus == Esri.ArcGISRuntime.LoadStatus.NotLoaded || newTable.LoadStatus == Esri.ArcGISRuntime.LoadStatus.Loading)
            {
                try
                {
                    Debug.WriteLine("Loading table...");
                    var loadTask = newTable.LoadAsync();
                    SetIsBusyState();
                    await loadTask;
                    Debug.WriteLine("Table loaded");
                }
                catch(System.Exception ex)
                {
                    Debug.WriteLine("Table failed to load: " + ex.Message);
                    VisualStateManager.GoToState(this, "TableFailedToLoad", true);
                    SetIsBusyState();
                    return;
                }
            }
            var column = new TableColumnCollection();
            var actionColumn = new ActionColumn() { Icon = new SymbolIconSource() { Symbol = Symbol.More } };
            actionColumn.Invoked += (s, feature) =>
            {
                if (feature is not null)
                {
                    FeatureActionInvoked?.Invoke(s, feature);
                }
            };

            column.Add(actionColumn);
            foreach (var f in newTable.Fields)
            {
                column.Add(new FeatureAttibuteColumn(f));
            }
            Columns = column;
            if (datasource is not null)
                datasource.IsBusyChanged -= IsBusyChanged;
            datasource = new FeatureTableQuerySource(newTable, parameters);
            datasource.IsBusyChanged += IsBusyChanged;
            _ = await datasource.LoadMoreItemsAsync(50);
            if (GetTemplateChild("ItemsPresenter") is ItemsRepeater itemsPresenter)
            {
                itemsPresenter.ItemsSource = datasource;
            }
        }

        public event EventHandler<Esri.ArcGISRuntime.Data.Feature>? FeatureActionInvoked;

        private void IsBusyChanged(object? sender, EventArgs e) => SetIsBusyState();
        private void SetIsBusyState()
        {
            bool busy = false;
            if (datasource is not null && datasource.IsBusy || Table is not null && Table.LoadStatus == Esri.ArcGISRuntime.LoadStatus.Loading)
            {
                busy = true;
            }
            // TODO: Should use visual states
            if (GetTemplateChild("BusyIndicator") is ProgressBar bar)
            {
                bar.IsIndeterminate = busy;
                bar.Visibility = busy ? Visibility.Visible : Visibility.Collapsed;
            }
            if (GetTemplateChild("BusyIndicator") is ProgressRing ring)
            {
                ring.IsActive = busy;
                ring.Visibility = busy ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public TableColumnCollection? Columns
        {
            get { return (TableColumnCollection)GetValue(ColumnsProperty); }
            private set { SetValue(ColumnsProperty, value); }
        }

        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.Register("Columns", typeof(TableColumnCollection), typeof(FeatureTableView), new PropertyMetadata(null, (s, e) => ((FeatureTableView)s).OnColumnsPropertyChanged(e.NewValue as FeatureCollection)));

        private void OnColumnsPropertyChanged(FeatureCollection? featureCollection)
        {
            isDefaultSizingApplied = false;
            if (this.ActualWidth > 0)
            {
                SetDefaultColumnSize(ActualWidth);
            }
        }
    }
}
