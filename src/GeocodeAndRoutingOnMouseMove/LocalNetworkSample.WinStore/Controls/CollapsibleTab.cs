using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Windows.ApplicationModel.Store;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Microsoft.VisualBasic.CompilerServices;

namespace LocalNetworkSample.Controls
{
	/// <summary>
	/// Control used for holding a set tabs in an expandable side panel
	/// </summary>
	public sealed class CollapsibleTab : Control
	{
		private ContentPresenter m_Content;
		private TextBlock m_HeaderText;

		public CollapsibleTab()
		{
			this.DefaultStyleKey = typeof(CollapsibleTab);
			Tabs = new TabCollection();
			TabClickCommand = new ClickCommand((item) =>
			{
				m_ignoreTap = true;
				if (item == CurrentItem && IsOpen)
					IsOpen = false;
				else
				{
					if (!IsOpen)
						IsOpen = true;
					CurrentItem = item;
				    CurrentItem.IsCurrentItem = true;
				}
			});
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			var header = GetTemplateChild("HeaderArea") as UIElement;
			if (header != null)
			{
				header.Tapped += header_Tapped;
			}
			m_Content = GetTemplateChild("Content") as ContentPresenter;
			if (m_Content != null)
				m_Content.Content = CurrentItem == null ? null : CurrentItem.Content;

			m_HeaderText = GetTemplateChild("HeaderText") as TextBlock;
			if (m_HeaderText != null)
				m_HeaderText.Text = CurrentItem == null ? "" : CurrentItem.Header;

			var button = GetTemplateChild("CloseButton") as Windows.UI.Xaml.Controls.Primitives.ButtonBase;
			if (button != null)
			{
				button.Click += CloseButton_Click;
			}

			ChangeVisualState(false);
		}

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			if (CurrentItem != null)
			{
			    var tabItem = CurrentItem;
                tabItem.Visibility = Visibility.Collapsed;
                tabItem.IsCurrentItem = false;
			    IsOpen = false;
			    CurrentItem = null;
			}
		}

		public void RemoveTab(TabItem item)
		{
			Tabs.Remove(item);
			IsOpen = false;
			item.RaiseClosed();
		}

		protected override Windows.Foundation.Size ArrangeOverride(Windows.Foundation.Size finalSize)
		{
			var size = base.ArrangeOverride(finalSize);
			var header = GetTemplateChild("HeaderArea") as UIElement;
            if (header != null)
            {
                var p = header.TransformToVisual(this).TransformPoint(new Windows.Foundation.Point(0, header.DesiredSize.Height * .5));
                HeaderBaseline = p.Y - size.Height * .5;
            }
            return size;
		}

		private void Tabs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
				CurrentItem = null;

			if (e.OldItems != null)
			{
				foreach (var item in e.OldItems.OfType<TabItem>())
				{
					if (CurrentItem == item)
					{
						CurrentItem = null;
						if (e.OldStartingIndex > 0)
						{
							int i = e.OldStartingIndex;
							while (i > 0)
								CurrentItem = Tabs[--i];
						}
						else if (Tabs.Count > 0)
							CurrentItem = VisibleTabs.FirstOrDefault();
					}
					item.Parent = null;
                    UnregisterTabItem(item);
				}
			}
			if (e.NewItems != null)
			{			    
				foreach (var item in e.NewItems.OfType<TabItem>())
				{
					item.Parent = this;					        
                    if(CurrentItem == null && item.Visibility == Visibility.Visible)
					    CurrentItem = item;				    
                    RegisterTabItem(item);
				}			    
			}
            UpdateVisibilityState();
		}

		private double m_HeaderBaseline;

		public double HeaderBaseline
		{
			get { return m_HeaderBaseline; }
			private set
			{
				if (m_HeaderBaseline != value)
				{
					m_HeaderBaseline = value;
					UpdateOffset();
				}
			}
		}

		private void UpdateOffset()
		{
			var groups = VisualStateManager.GetVisualStateGroups(VisualTreeHelper.GetChild(this, 0) as FrameworkElement);
            if (groups == null)
                return;
			var group = groups.Where(g => g.Name == "ExpansionState").FirstOrDefault();
			if (group != null)
			{
				foreach (var state in group.States.Where(s=>s!=null && s.Storyboard != null))
				{
					foreach (var anim in state.Storyboard.Children)
					{
						if (anim is Windows.UI.Xaml.Media.Animation.SplitOpenThemeAnimation)
						{
							(anim as Windows.UI.Xaml.Media.Animation.SplitOpenThemeAnimation).OffsetFromCenter = HeaderBaseline;
							(anim as Windows.UI.Xaml.Media.Animation.SplitOpenThemeAnimation).OpenedLength = DesiredSize.Height;
						}
						else if (anim is Windows.UI.Xaml.Media.Animation.SplitCloseThemeAnimation)
						{
							(anim as Windows.UI.Xaml.Media.Animation.SplitCloseThemeAnimation).OffsetFromCenter = HeaderBaseline;
							(anim as Windows.UI.Xaml.Media.Animation.SplitCloseThemeAnimation).OpenedLength = DesiredSize.Height;
						}
					}
				}
			}
		}

		private bool m_ignoreTap;
	
		private void header_Tapped(object sender, TappedRoutedEventArgs e)
		{
			if (!m_ignoreTap)
				IsOpen = !IsOpen;
			else 
                m_ignoreTap = false;
		}

        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register("IsOpen", typeof(bool), typeof(CollapsibleTab), new PropertyMetadata(true, OnIsOpenPropertyChanged));

        private static void OnIsOpenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (CollapsibleTab)d;
            ctrl.ChangeVisualState(true);
            if ((bool)e.NewValue && ctrl.Opened != null)
                ctrl.Opened(ctrl, EventArgs.Empty);
            else if (!(bool)e.NewValue && ctrl.Closed != null)
                ctrl.Closed(ctrl, EventArgs.Empty);
		}

		private void ChangeVisualState(bool useTransitions)
		{
			if (IsOpen)
				GoToState(useTransitions, "Normal");
			else
				GoToState(useTransitions, "Collapsed");
		}

		private bool GoToState(bool useTransitions, string stateName)
		{
			return VisualStateManager.GoToState(this, stateName, useTransitions);
		}

		public ICommand TabClickCommand
		{
			get { return (ICommand)GetValue(TabClickCommandProperty); }
			set { SetValue(TabClickCommandProperty, value); }
		}

		public static readonly DependencyProperty TabClickCommandProperty =
			DependencyProperty.Register("TabClickCommand", typeof(ICommand), typeof(CollapsibleTab), null);

		private class ClickCommand : ICommand
		{
			Action<TabItem> m_action;
			public ClickCommand(Action<TabItem> a)
			{
				m_action = a;
			}
			public bool CanExecute(object parameter)
			{
				return parameter is TabItem;
			}

			public event EventHandler CanExecuteChanged;

			public void Execute(object parameter)
			{
				m_action(parameter as TabItem);
			}
		}

		public int CurrentIndex
		{
			get { return (int)GetValue(CurrentIndexProperty); }
			set { SetValue(CurrentIndexProperty, value); }
		}
		public static readonly DependencyProperty CurrentIndexProperty =
			DependencyProperty.Register("CurrentIndex", typeof(int), typeof(CollapsibleTab), new PropertyMetadata(null, OnCurrentIndexPropertyChanged));

		private static void OnCurrentIndexPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var sidetab = d as CollapsibleTab;
			int index = (int)e.NewValue;
			if (index >= sidetab.Tabs.Count)
				throw new ArgumentOutOfRangeException();
			if (index < 0)
				sidetab.CurrentItem = null;
			else
				sidetab.CurrentItem = sidetab.Tabs[index];
		}

		public TabItem CurrentItem
		{
			get { return (TabItem)GetValue(CurrentItemProperty); }
			set { SetValue(CurrentItemProperty, value); }
		}

		public static readonly DependencyProperty CurrentItemProperty =
			DependencyProperty.Register("CurrentItem", typeof(TabItem), typeof(CollapsibleTab), new PropertyMetadata(null, OnCurrentItemPropertyChanged));

		private static void OnCurrentItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var sidetab = d as CollapsibleTab;
			if (e.OldValue != null)
			{
				(e.OldValue as TabItem).IsCurrentItem = false;
			}
			if (e.NewValue != null)
				(e.NewValue as TabItem).IsCurrentItem = true;
			if (sidetab.m_Content != null)
			{
				sidetab.m_Content.Content = sidetab.CurrentItem == null ? null : sidetab.CurrentItem.Content;
			}
			if (sidetab.m_HeaderText != null)
			{
				sidetab.m_HeaderText.Text = sidetab.CurrentItem == null ? "" : sidetab.CurrentItem.Header;
			}

			if (sidetab.CurrentItemChanged != null)
				sidetab.CurrentItemChanged(sidetab, e.NewValue as TabItem);
			if (e.NewValue is TabItem)
				sidetab.CurrentIndex = sidetab.Tabs.IndexOf(e.NewValue as TabItem);
			else
				sidetab.CurrentIndex = -1;
		}

		public TabCollection Tabs
		{
			get { return (TabCollection)GetValue(TabsProperty); }
			set { SetValue(TabsProperty, value); }
		}

		private IEnumerable<TabItem> VisibleTabs
		{
			get
			{
				if (Tabs != null)
				{
					foreach (var t in Tabs)
						if (t.Visibility == Windows.UI.Xaml.Visibility.Visible)
							yield return t;
				}
			}
		}

		public static readonly DependencyProperty TabsProperty =
			DependencyProperty.Register("Tabs", typeof(TabCollection), typeof(CollapsibleTab), new PropertyMetadata(null, OnTabsPropertyChanged));

        private static void OnTabsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as CollapsibleTab;
            if(e.OldValue is TabCollection)
            {
                ((TabCollection)e.OldValue).CollectionChanged -= ctrl.Tabs_CollectionChanged;
                foreach (var item in ((TabCollection)e.OldValue))
                    ctrl.UnregisterTabItem(item);
            }
            if (e.NewValue is TabCollection)
            {
                ((TabCollection)e.NewValue).CollectionChanged += ctrl.Tabs_CollectionChanged;
                foreach (var item in ((TabCollection)e.NewValue))
                    ctrl.RegisterTabItem(item);
            }
            ctrl.UpdateVisibilityState();
        }

        private void UnregisterTabItem(TabItem item)
        {
            item.PropertyChanged -= item_PropertyChanged;
        }

        private void RegisterTabItem(TabItem item)
        {
            item.PropertyChanged += item_PropertyChanged;
        }
        private void item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Visibility")
                UpdateVisibilityState();
        }

        private void UpdateVisibilityState()
        {
            if (VisibleTabs.Any())
                Visibility = Visibility.Visible;
            else
                Visibility = Visibility.Collapsed;
        }

		public TransitionCollection ContentTransitions
		{
			get { return (TransitionCollection)GetValue(ContentTransitionsProperty); }
			set { SetValue(ContentTransitionsProperty, value); }
		}

		public static readonly DependencyProperty ContentTransitionsProperty =
			DependencyProperty.Register("ContentTransitions", typeof(TransitionCollection), typeof(CollapsibleTab), null);		

		public event EventHandler<TabItem> CurrentItemChanged;

        public event EventHandler Opened;
		
        public event EventHandler Closed;
	}

	public sealed class TabCollection : ObservableCollection<TabItem> { }

	[Windows.UI.Xaml.Markup.ContentProperty(Name = "Content")]
	public class TabItem : DependencyObject, INotifyPropertyChanged
	{	  
		public CollapsibleTab Parent { get; set; }
		
		public object Content
		{
			get { return (object)GetValue(ContentProperty); }
			set { SetValue(ContentProperty, value); }
		}

		public static readonly DependencyProperty ContentProperty =
			DependencyProperty.Register("Content", typeof(object), typeof(TabItem), null);

		public string Header
		{
			get { return (string)GetValue(HeaderProperty); }
			set { SetValue(HeaderProperty, value); }
		}

		public static readonly DependencyProperty HeaderProperty =
			DependencyProperty.Register("Header", typeof(string), typeof(TabItem), new PropertyMetadata("TabItem"));

		public Visibility Visibility
		{
			get { return (Visibility)GetValue(VisibilityProperty); }
			set { SetValue(VisibilityProperty, value); }
		}

		public static readonly DependencyProperty VisibilityProperty =
			DependencyProperty.Register("Visibility", typeof(Visibility), typeof(TabItem), new PropertyMetadata(Visibility.Visible, OnVisibilityPropertyChanged));

        private static void OnVisibilityPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var item = (TabItem)d;
            if (item.PropertyChanged != null)
                item.PropertyChanged(item, new PropertyChangedEventArgs("Visibility"));
            if ((Windows.UI.Xaml.Visibility)e.NewValue == Windows.UI.Xaml.Visibility.Visible &&
                item.Parent != null && item.Parent.CurrentItem == null)
            {
                item.Parent.CurrentItem = item;
            }
			else if ((Windows.UI.Xaml.Visibility)e.NewValue == Windows.UI.Xaml.Visibility.Collapsed &&
				item.Parent != null && item.Parent.CurrentItem == item)
			{
				//Current item got collapsed. Pick a new item
				TabItem newCurrentItem = null;
				var index = item.Parent.Tabs.IndexOf(item);
				if (index == 0) //Select the first visible tab
				{
					newCurrentItem = item.Parent.Tabs.FirstOrDefault(t => t.Visibility == Visibility.Visible);					
				}
				else
				{
					while(--index>=0)
					{
						var i = item.Parent.Tabs[index];
						if(i.Visibility == Visibility.Visible)
						{
							newCurrentItem = i;
							break;
						}
					}
				}
				item.Parent.CurrentItem = newCurrentItem;
			}
		}

        public bool IsCurrentItem
        {
            get { return (bool)GetValue(IsCurrentItemProperty); }
            set { SetValue(IsCurrentItemProperty, value); }
        }

        public static readonly DependencyProperty IsCurrentItemProperty =
            DependencyProperty.Register("IsCurrentItem", typeof(bool), typeof(TabItem), new PropertyMetadata(false, OnIsCurrentItemPropertyChanged));


        private static void OnIsCurrentItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	    {
	        var tabItem = ((TabItem)d);

	        if ((bool)e.NewValue && tabItem.Parent != null)
	        {	            
	            foreach (TabItem tab in tabItem.Parent.Tabs)
	            {
	                if (tab == tabItem)
	                    continue;
	                tab.IsCurrentItem = false;
	            }
	            tabItem.Parent.CurrentItem = tabItem;	              
	        }
	    }

		public IconElement Icon
		{
			get { return (IconElement)GetValue(IconProperty); }
			set { SetValue(IconProperty, value); }
		}

		public static readonly DependencyProperty IconProperty =
			DependencyProperty.Register("Icon", typeof(IconElement), typeof(TabItem), new PropertyMetadata(null));

		public bool ShowCloseButton
		{
			get { return (bool)GetValue(ShowCloseButtonProperty); }
			set { SetValue(ShowCloseButtonProperty, value); }
		}

		public static readonly DependencyProperty ShowCloseButtonProperty =
			DependencyProperty.Register("ShowCloseButton", typeof(bool), typeof(TabItem), new PropertyMetadata(false));

		public event PropertyChangedEventHandler PropertyChanged;

		public event EventHandler Closed;
	
		internal void RaiseClosed()
		{
			if (Closed != null)
				Closed(this, EventArgs.Empty);
		}

		public void Close()
		{
			if (Parent != null)
			{
				Parent.RemoveTab(this);
			}
		}
	}
}
