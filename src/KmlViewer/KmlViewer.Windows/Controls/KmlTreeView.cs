using Esri.ArcGISRuntime.Layers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace KmlViewer.Controls
{
	public sealed class KmlTreeView : Control, INotifyPropertyChanged
	{
		public KmlTreeView()
		{
			DefaultStyleKey = typeof(KmlTreeView);
		}

		public KmlFeature Parent
		{
			get { return (KmlFeature)GetValue(ParentProperty); }
			set { SetValue(ParentProperty, value); }
		}

		public static readonly DependencyProperty ParentProperty =
			DependencyProperty.Register("Parent", typeof(KmlFeature), typeof(KmlTreeView), new PropertyMetadata(null, OnParentPropertyChanged));

		private static void OnParentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((KmlTreeView)d).UpdateVisibilityBoxes();
		}

		

		public KmlFeature KmlFeature
		{
			get { return (KmlFeature)GetValue(KmlFeatureProperty); }
			set { SetValue(KmlFeatureProperty, value); }
		}

		public static readonly DependencyProperty KmlFeatureProperty =
			DependencyProperty.Register("KmlFeature", typeof(KmlFeature), typeof(KmlTreeView), new PropertyMetadata(null, OnKmlFeaturePropertyChanged));

		private static void OnKmlFeaturePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((KmlTreeView)d).UpdateShowChildren(e.OldValue as KmlFeature, e.NewValue as KmlFeature);
			((KmlTreeView)d).UpdateVisibilityBoxes();
			
		}

		private void UpdateVisibilityBoxes()
		{
			//if this node is in a radio button list, switch checkbox over to radio button
			var e1 = GetTemplateChild("VisibilityCheckBox") as FrameworkElement;
			var e2 = GetTemplateChild("VisibilityRadioButton") as FrameworkElement;
			if (Parent is KmlContainer && ((KmlContainer)Parent).ListType == KmlListType.RadioFolder)
			{
				if (e1 != null) e1.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
				if (e2 != null) e2.Visibility = Windows.UI.Xaml.Visibility.Visible;
			}
			else
			{
				if (e1 != null) e1.Visibility = Windows.UI.Xaml.Visibility.Visible;
				if (e2 != null) e2.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
			}
		}
		private void UpdateShowChildren(KmlFeature oldNode, KmlFeature newNode)
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

		void elm_DoubleTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
		{
			var parent = Windows.UI.Xaml.Media.VisualTreeHelper.GetParent(this);
			KmlTreeView topTree = null;
			while (parent != null)
			{
				if (parent is KmlTreeView)
				{
					topTree = (KmlTreeView)parent;
					if (topTree.FeatureDoubleTapped != null)
						topTree.FeatureDoubleTapped(this, KmlFeature);
				}
				parent = Windows.UI.Xaml.Media.VisualTreeHelper.GetParent(parent);
			}
		}

		private void elm_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
		{
			//walk the tree and raise on top-most treeview or anyone else listening on the way up
			var parent = Windows.UI.Xaml.Media.VisualTreeHelper.GetParent(this);
			KmlTreeView topTree = null;
			while(parent != null)
			{
				if (parent is KmlTreeView)
				{
					topTree = (KmlTreeView)parent;
					if (topTree.FeatureTapped != null)
						topTree.FeatureTapped(this, KmlFeature);
				}
				parent = Windows.UI.Xaml.Media.VisualTreeHelper.GetParent(parent);
			}
		}
		public event EventHandler<KmlFeature> FeatureTapped;
		public event EventHandler<KmlFeature> FeatureDoubleTapped;

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
				return KmlFeature is KmlNetworkLink &&
				   ((KmlNetworkLink)KmlFeature).ListType != KmlListType.CheckHideChildren && //don't show if the list type tells you to hide it
					KmlFeature.ChildFeatures != null && KmlFeature.ChildFeatures.Any() || //only if there are children
					KmlFeature is KmlContainer &&
					((KmlContainer)KmlFeature).ListType != KmlListType.CheckHideChildren &&
					KmlFeature.ChildFeatures != null && KmlFeature.ChildFeatures.Any();
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
			var feature = value as KmlFeature;
			if (feature != null)
			{
				if (feature is KmlNetworkLink && ((KmlNetworkLink)feature).ListType != KmlListType.CheckHideChildren ||
				   feature is KmlContainer && ((KmlContainer)feature).ListType != KmlListType.CheckHideChildren)
					return feature.ChildFeatures;
			}
			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}
