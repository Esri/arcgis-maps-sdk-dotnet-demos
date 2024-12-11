using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace KmlViewer
{
	public sealed class SidePanel : ItemsControl
	{
		public SidePanel()
		{
			this.DefaultStyleKey = typeof(SidePanel);
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			var ic = GetTemplateChild("sideButtons") as ItemsControl;
			if (ic != null)
			{

				ic.ItemsSource = Items.OfType<SidePanelItem>().Select(t => new IconObjects(this, t) { Icon = t.Icon, IsChecked = t==SelectedItem }).ToList();
			}
		}

		[Bindable]
		private class IconObjects : System.ComponentModel.INotifyPropertyChanged
		{
			SidePanel _parent;
			SidePanelItem _item;
			public IconObjects(SidePanel parent, SidePanelItem item)
			{
				_parent = parent;
				_item = item;
			}
			public IconElement Icon { get; set; }
			private bool m_IsChecked;

			public bool IsChecked
			{
				get { return m_IsChecked; }
				set
				{
					m_IsChecked = value;
					if (value)
						_parent.SelectedItem = _item;
					if (PropertyChanged != null)
						PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs("IsChecked"));
				}
			}
			public SidePanelItem Item { get { return _item; } }

			public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
		}

		protected override void OnItemsChanged(object e)
		{

			base.OnItemsChanged(e);
			if (SelectedIndex < Items.Count)
				SelectedItem = Items[SelectedIndex];
		}

		public int SelectedIndex
		{
			get { return (int)GetValue(SelectedIndexProperty); }
			set { SetValue(SelectedIndexProperty, value); }
		}

		public static readonly DependencyProperty SelectedIndexProperty =
			DependencyProperty.Register("SelectedIndex", typeof(int), typeof(SidePanel), new PropertyMetadata(0, OnSelectedIndexPropertyChanged));

		private static void OnSelectedIndexPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var item = (int)(e.NewValue);
			var ctrl = (SidePanel)d;
			if (item < ctrl.Items.Count)
				ctrl.SelectedItem = ctrl.Items[item];
		}

		public object SelectedItem
		{
			get { return (object)GetValue(SelectedItemProperty); }
			set { SetValue(SelectedItemProperty, value); }
		}

		public static readonly DependencyProperty SelectedItemProperty =
			DependencyProperty.Register("SelectedItem", typeof(object), typeof(SidePanel), new PropertyMetadata(null, OnSelectedItemPropertyChanged));

		private static void OnSelectedItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var ctrl = (SidePanel)d;
			var idx = ctrl.Items.IndexOf(ctrl.SelectedItem);
			ctrl.SelectedIndex = idx;
			var ic = ctrl.GetTemplateChild("sideButtons") as ItemsControl;
			if (ic != null)
			{
				var item = (ic.ItemsSource as IEnumerable<IconObjects>).Where(t=>t.Item == ctrl.SelectedItem).FirstOrDefault();
				if(item != null) item.IsChecked = true;
			}
		}
	
	}

	public class SidePanelItem : ContentControl
	{


		public SymbolIcon Icon
		{
			get { return (SymbolIcon)GetValue(IconProperty); }
			set { SetValue(IconProperty, value); }
		}

		public static readonly DependencyProperty IconProperty =
			DependencyProperty.Register("Icon", typeof(SymbolIcon), typeof(SidePanelItem), new PropertyMetadata(null));



		public string Header
		{
			get { return (string)GetValue(HeaderProperty); }
			set { SetValue(HeaderProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Header.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty HeaderProperty =
			DependencyProperty.Register("Header", typeof(string), typeof(SidePanelItem), new PropertyMetadata(""));

		

	}
}
