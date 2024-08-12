using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace ArcGISMapViewer.Controls
{
    public sealed class CollapsiblePanel : Control
    {
        private ContentPresenter? PART_ContentPresenter;
        public CollapsiblePanel()
        {
            this.DefaultStyleKey = typeof(CollapsiblePanel);

            ((INotifyCollectionChanged)_items).CollectionChanged += CollapsiblePanel_CollectionChanged;
            ((INotifyCollectionChanged)_footerItems).CollectionChanged += CollapsiblePanel_CollectionChanged;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if(GetTemplateChild("PART_ContentPresenter") is ContentPresenter contentPresenter)
            {
                PART_ContentPresenter = contentPresenter;
                foreach (var item in Items?.OfType<CollapsiblePanelItem>() ?? Enumerable.Empty<CollapsiblePanelItem>())
                {
                    if (item.IsSelected)
                    {
                        PART_ContentPresenter.Content = item.Content;
                        break;
                    }
                }
            }
            if(GetTemplateChild("PART_MenuPanel") is ItemsControl menuPanel)
            {
                menuPanel.ItemsSource = Items;
            }
            if (GetTemplateChild("PART_FooterMenuPanel") is ItemsControl footerPanel)
            {
                footerPanel.ItemsSource = FooterItems;
            }
            if (GetTemplateChild("PART_CloseButton") is ButtonBase button)
            {
                button.Click += (s, e) => IsOpen = false;
            }
            if (GetTemplateChild("PART_ExpandButton") is ButtonBase expandButton)
            {
                expandButton.Click += (s, e) => IsPaneExpanded = !IsPaneExpanded;
            }
            
            OnIsOpenPropertyChanged(false);
            OnIsPaneExpandedPropertyChanged(false);
            OnPanePlacementPropertyChanged(false);
        }

        private void CollapsiblePanel_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var item in e.OldItems?.OfType<CollapsiblePanelItem>() ?? Enumerable.Empty<CollapsiblePanelItem>())
            {
                item.Tapped -= ItemTapped;
            }
            foreach (var item in e.NewItems?.OfType<CollapsiblePanelItem>() ?? Enumerable.Empty<CollapsiblePanelItem>())
            {
                item.IsExpanded = IsPaneExpanded;
                item.Tapped += ItemTapped;
                if (item.IsSelected)
                    SelectedItem = item;
            }
        }

        private void ItemTapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is CollapsiblePanelItem item && item.IsEnabled)
            {
                if (item == SelectedItem)
                    IsOpen = !IsOpen;
                else
                {
                    IsOpen = true;
                    SelectedItem = item;
                }
            }
        }

        private readonly IList<CollapsiblePanelItem> _items = new ObservableCollection<CollapsiblePanelItem>();

        public IList<CollapsiblePanelItem> Items => _items;

        private readonly IList<CollapsiblePanelItem> _footerItems = new ObservableCollection<CollapsiblePanelItem>();

        public IList<CollapsiblePanelItem> FooterItems => _footerItems;

        public CollapsiblePanelItem? SelectedItem
        {
            get { return (CollapsiblePanelItem?)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(nameof(SelectedItem), typeof(CollapsiblePanelItem), typeof(CollapsiblePanel), new PropertyMetadata(null, (s, e) => ((CollapsiblePanel)s).OnSelectedItemPropertyChanged(true)));

        private void OnSelectedItemPropertyChanged(bool useTransitions)
        {
            var selectedItem = SelectedItem;
            if (PART_ContentPresenter is not null && selectedItem != null)
                PART_ContentPresenter.Content = selectedItem.Content;
            if(GetTemplateChild("PART_Title") is TextBlock tb)
                tb.Text = selectedItem?.Title;
            foreach (var item in Items)
            {
                item.IsSelected = item == selectedItem;
            }
            foreach (var item in FooterItems)
            {
                item.IsSelected = item == selectedItem;
            }
        }

        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register("IsOpen", typeof(bool), typeof(CollapsiblePanel), new PropertyMetadata(false, (s, e) => ((CollapsiblePanel)s).OnIsOpenPropertyChanged(true)));

        private void OnIsOpenPropertyChanged(bool useTransitions)
        {
            VisualStateManager.GoToState(this, IsOpen ? "Open" : "Closed", useTransitions);
        }

        public bool IsPaneExpanded
        {
            get { return (bool)GetValue(IsPaneExpandedProperty); }
            set { SetValue(IsPaneExpandedProperty, value); }
        }

        public static readonly DependencyProperty IsPaneExpandedProperty =
            DependencyProperty.Register("IsPaneExpanded", typeof(bool), typeof(CollapsiblePanel), new PropertyMetadata(false, (s, e) => ((CollapsiblePanel)s).OnIsPaneExpandedPropertyChanged(true)));

        private void OnIsPaneExpandedPropertyChanged(bool useTransitions)
        {
            foreach (var item in Items)
            {
                item.IsExpanded = IsPaneExpanded;
            }
            VisualStateManager.GoToState(this, (IsPaneExpanded ? "PaneExpanded" : "PaneCollapsed") + PanePlacement.ToString(), useTransitions);
        }

        public SplitViewPanePlacement PanePlacement
        {
            get { return (SplitViewPanePlacement)GetValue(PanePlacementProperty); }
            set { SetValue(PanePlacementProperty, value); }
        }

        public static readonly DependencyProperty PanePlacementProperty =
            DependencyProperty.Register("PanePlacement", typeof(SplitViewPanePlacement), typeof(CollapsiblePanel), new PropertyMetadata(SplitViewPanePlacement.Left, (s, e) => ((CollapsiblePanel)s).OnPanePlacementPropertyChanged(true)));

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
            DependencyProperty.Register(nameof(ContentWidth), typeof(double), typeof(CollapsiblePanel), new PropertyMetadata(200d));

        public Visibility CloseButtonVisibility
        {
            get { return (Visibility)GetValue(CloseButtonVisibilityProperty); }
            set { SetValue(CloseButtonVisibilityProperty, value); }
        }

        public static readonly DependencyProperty CloseButtonVisibilityProperty =
            DependencyProperty.Register(nameof(CloseButtonVisibility), typeof(Visibility), typeof(CollapsiblePanel), new PropertyMetadata(Visibility.Visible));

        public Visibility ExpandButtonVisibility
        {
            get { return (Visibility)GetValue(ExpandButtonVisibilityProperty); }
            set { SetValue(ExpandButtonVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ExpandButtonVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExpandButtonVisibilityProperty =
            DependencyProperty.Register(nameof(ExpandButtonVisibility), typeof(Visibility), typeof(CollapsiblePanel), new PropertyMetadata(true));


    }
}
