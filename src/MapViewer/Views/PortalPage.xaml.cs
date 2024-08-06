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

namespace ArcGISMapViewer.Views
{
    public sealed partial class PortalPage : Page
    {
        public PortalPage()
        {
            this.InitializeComponent();
        }

        public PortalPageViewModel PageVM = PortalPageViewModel.Instance;

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is PortalItem item)
            {
                ApplicationViewModel.Instance.Map = new Map(item);
                ApplicationViewModel.Instance.IsMapVisible = true;
            }
        }
    }
}
