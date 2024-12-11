using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Ogc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;

namespace KmlViewer.Controls
{
	public sealed class KmlTreeView : Control, INotifyPropertyChanged
	{
		public KmlTreeView()
		{
			DefaultStyleKey = typeof(KmlTreeView);
		}

		public KmlNode ParentNode
		{
			get { return (KmlNode)GetValue(ParentProperty); }
			set { SetValue(ParentProperty, value); }
		}

		public static readonly DependencyProperty ParentProperty =
			DependencyProperty.Register("Parent", typeof(KmlNode), typeof(KmlTreeView), new PropertyMetadata(null, OnParentPropertyChanged));

		private static void OnParentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((KmlTreeView)d).UpdateVisibilityBoxes();
		}

		public KmlNode KmlFeature
		{
			get { return (KmlNode)GetValue(KmlFeatureProperty); }
			set { SetValue(KmlFeatureProperty, value); }
		}

		public static readonly DependencyProperty KmlFeatureProperty =
			DependencyProperty.Register("KmlFeature", typeof(KmlNode), typeof(KmlTreeView), new PropertyMetadata(null, OnKmlFeaturePropertyChanged));

		private static void OnKmlFeaturePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((KmlTreeView)d).UpdateShowChildren(e.OldValue as KmlNode, e.NewValue as KmlNode);
			((KmlTreeView)d).UpdateVisibilityBoxes();
			
		}

		private void UpdateVisibilityBoxes()
		{
			//if this node is in a radio button list, switch checkbox over to radio button
			var e1 = GetTemplateChild("VisibilityCheckBox") as FrameworkElement;
			var e2 = GetTemplateChild("VisibilityRadioButton") as FrameworkElement;
			if (ParentNode is KmlContainer && ((KmlContainer)ParentNode).ListItemType == KmlListItemType.RadioFolder)
			{
				if (e1 != null) e1.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
				if (e2 != null) e2.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
			}
			else
			{
				if (e1 != null) e1.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
				if (e2 != null) e2.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
			}
		}
		private void UpdateShowChildren(KmlNode oldNode, KmlNode newNode)
		{ 
			if(oldNode != null)
			{
				oldNode.PropertyChanged -= KmlFeature_PropertyChanged;
			}
			if(newNode != null)
			{
				newNode.PropertyChanged += KmlFeature_PropertyChanged;
			}
			UpdateShowChildren();
		}
		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			UpdateShowChildren();
			UpdateVisibilityBoxes();
			var elm = GetTemplateChild("FeatureNameText") as FrameworkElement;
			if(elm != null)
			{
				elm.Tapped += elm_Tapped;
				elm.DoubleTapped += elm_DoubleTapped;
			}
		}

		void elm_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
		{
			var parent = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(this);
			KmlTreeView topTree = null;
			while (parent != null)
			{
				if (parent is KmlTreeView)
				{
					topTree = (KmlTreeView)parent;
					if (topTree.FeatureDoubleTapped != null)
						topTree.FeatureDoubleTapped(this, KmlFeature);
				}
				parent = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(parent);
			}
		}

		private void elm_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
		{
			//walk the tree and raise on top-most treeview or anyone else listening on the way up
			var parent = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(this);
			KmlTreeView topTree = null;
			while(parent != null)
			{
				if (parent is KmlTreeView)
				{
					topTree = (KmlTreeView)parent;
					if (topTree.FeatureTapped != null)
						topTree.FeatureTapped(this, KmlFeature);
				}
				parent = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(parent);
			}
		}
		public event EventHandler<KmlNode> FeatureTapped;
		public event EventHandler<KmlNode> FeatureDoubleTapped;

		private void KmlFeature_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == "ChildFeatures")
			{
				UpdateShowChildren();
			}
		}

		private void UpdateShowChildren()
		{
			var elm = GetTemplateChild("ChildListToggle") as UIElement;
			if(elm != null)
			{
				elm.Visibility = HasVisibleChildren ? Visibility.Visible : Visibility.Collapsed;
			}
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs("HasVisibleChildren"));
		}

		public bool HasVisibleChildren
		{
			get
			{
				//Network links and containers can have children
				return KmlFeature is KmlNetworkLink link &&
				   ((KmlNetworkLink)KmlFeature).ListItemType != KmlListItemType.CheckHideChildren && //don't show if the list type tells you to hide it
					link.ChildNodes != null && link.ChildNodes.Any() || //only if there are children
					KmlFeature is KmlContainer cont &&
					((KmlContainer)KmlFeature).ListItemType != KmlListItemType.CheckHideChildren &&
                    cont.ChildNodes != null && cont.ChildNodes.Any();
			}
		}

		//public DataTemplate NetworkLinkTemplate
		//{
		//	get { return (DataTemplate)GetValue(NetworkLinkTemplateProperty); }
		//	set { SetValue(NetworkLinkTemplateProperty, value); }
		//}
		//
		//public static readonly DependencyProperty NetworkLinkTemplateProperty =
		//	DependencyProperty.Register("NetworkLinkTemplate", typeof(DataTemplate), typeof(KmlTreeView), new PropertyMetadata(null));

		public event PropertyChangedEventHandler PropertyChanged;		

	}

	/// <summary>
	/// This converter only returns the child collection of a feature if they aren't meant to be hidden.
	/// This prevent touching the ChildFeatures property if it's not needed, and thus not creating a large
	/// object graph in memory until needed.
	/// </summary>
	public class BindChildrenIfNotHiddenConverter : IValueConverter
	{
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var feature = value as KmlNode;
            if (feature != null)
            {
                var link = feature as KmlNetworkLink;
                var cont = feature as KmlContainer;

                if (link != null && link.ListItemType != KmlListItemType.CheckHideChildren ||
                   cont != null && cont.ListItemType != KmlListItemType.CheckHideChildren)
                {
                    if (link != null)
                        return link.ChildNodes;
                    else if (cont != null)
                        return cont.ChildNodes;
                }
            }
            return null;
        }

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}
