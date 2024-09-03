using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Esri.ArcGISRuntime.Portal;
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

namespace ArcGISMapViewer.Views
{
    public sealed partial class PortalItemDetailView : UserControl
    {
        public PortalItemDetailView()
        {
            this.InitializeComponent();
        }

        public PortalItem Item
        {
            get { return (PortalItem)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register("Item", typeof(PortalItem), typeof(PortalItemDetailView), new PropertyMetadata(null, (s,e) => ((PortalItemDetailView)s).OnItemPropertyChanged(e.NewValue as PortalItem)));

        private async void OnItemPropertyChanged(PortalItem? portalItem)
        {
            if(string.IsNullOrEmpty(Item.Description))
            {
                Description.Visibility = Visibility.Collapsed;
            }
            else
            {
                try
                {
                    await Description.EnsureCoreWebView2Async();
                    Description.NavigateToString(Item.Description);
                }
                catch
                {
                    Description.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            if (Item != null)
                ApplicationViewModel.Instance.Favorites.Add(Item);
        }
    }
}
