using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ArcGISMapViewer.Controls
{
    public sealed partial class CollapsibleSidePanel : UserControl
    {
        public CollapsibleSidePanel()
        {
            this.InitializeComponent();
            OnIsOpenPropertyChanged();
            OnIsExpandedPropertyChanged();
            OnPanePlacementPropertyChanged(false);
            ((INotifyCollectionChanged)_items).CollectionChanged += CollapsibleSidePanel_CollectionChanged;
        }

        private void CollapsibleSidePanel_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var item in e.OldItems?.OfType<PanelItem>() ?? Enumerable.Empty<PanelItem>())
            {
                item.Tapped -= ItemTapped;
            }
            foreach (var item in e.NewItems?.OfType<PanelItem>() ?? Enumerable.Empty<PanelItem>())
            {
                item.IsExpanded = IsExpanded;
                item.Tapped += ItemTapped;
                if(item.IsSelected)
                    PART_ContentPresenter.Content = item.FrameContent;
            }
        }

        private void ItemTapped(object sender, TappedRoutedEventArgs e)
        {
            if(sender is PanelItem item && item.IsEnabled)
            {
                if (item.IsSelected)
                    IsOpen = !IsOpen;
                else
                {
                    IsOpen = true;
                    item.IsSelected = true;
                    PART_ContentPresenter.Content = item.FrameContent;
                    foreach(var otheritem in Items)
                    {
                        if (otheritem == item) continue;
                        otheritem.IsSelected = false;
                    }
                }
            }
        }

        private readonly IList<PanelItem> _items = new ObservableCollection<PanelItem>();

        public IList<PanelItem> Items => _items;

        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register("IsOpen", typeof(bool), typeof(CollapsibleSidePanel), new PropertyMetadata(false, (s, e) => ((CollapsibleSidePanel)s).OnIsOpenPropertyChanged()));

        private void OnIsOpenPropertyChanged()
        {
            ContentPanel.Visibility = IsOpen ? Visibility.Visible : Visibility.Collapsed;
        }

        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register("IsExpanded", typeof(bool), typeof(CollapsibleSidePanel), new PropertyMetadata(false, (s,e) => ((CollapsibleSidePanel)s).OnIsExpandedPropertyChanged()));

        private void OnIsExpandedPropertyChanged()
        {
            foreach(var item in Items)
            {
                item.IsExpanded = IsExpanded;
            }
            if(IsExpanded)
                ItemsColumn.ClearValue(ColumnDefinition.WidthProperty);
            else 
                ItemsColumn.Width = 50;
        }

        public SplitViewPanePlacement PanePlacement
        {
            get { return (SplitViewPanePlacement)GetValue(PanePlacementProperty); }
            set { SetValue(PanePlacementProperty, value); }
        }

        public static readonly DependencyProperty PanePlacementProperty =
            DependencyProperty.Register("PanePlacement", typeof(SplitViewPanePlacement), typeof(CollapsibleSidePanel), new PropertyMetadata(SplitViewPanePlacement.Left, (s, e) => ((CollapsibleSidePanel)s).OnPanePlacementPropertyChanged(true)));

        private void OnPanePlacementPropertyChanged(bool useTransitions = false)
        {
            VisualStateManager.GoToState(this, PanePlacement == SplitViewPanePlacement.Left ? "PlacementLeft" : "PlacementRight", useTransitions);
        }

        public double ContentWidth
        {
            get { return (double)GetValue(ContentWidthProperty); }
            set { SetValue(ContentWidthProperty, value); }
        }

        public static readonly DependencyProperty ContentWidthProperty =
            DependencyProperty.Register("ContentWidth", typeof(double), typeof(CollapsibleSidePanel), new PropertyMetadata(200d));



        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            IsOpen = false;
        }
    }

    [Microsoft.UI.Xaml.Markup.ContentProperty(Name = "FrameContent")]
    public class PanelItem : NavigationViewItem
    {

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(PanelItem), new PropertyMetadata("", (s, e) => ((PanelItem)s).Content = e.NewValue));

        //public IconElement Icon
        //{
        //    get { return (IconElement)GetValue(IconProperty); }
        //    set { SetValue(IconProperty, value); }
        //}

        //public static readonly DependencyProperty IconProperty =
        //    DependencyProperty.Register("Icon", typeof(IconElement), typeof(PanelItem), new PropertyMetadata(null));

        public object FrameContent
        {
            get { return (object)GetValue(FrameContentProperty); }
            set { SetValue(FrameContentProperty, value); }
        }

        public static readonly DependencyProperty FrameContentProperty =
            DependencyProperty.Register("FrameContent", typeof(object), typeof(PanelItem), new PropertyMetadata(null));


    }
}