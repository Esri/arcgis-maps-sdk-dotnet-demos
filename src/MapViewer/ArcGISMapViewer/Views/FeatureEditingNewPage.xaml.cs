using System;
using System.Collections.Generic;
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
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping.FeatureForms;
using CommunityToolkit.Mvvm.Messaging;

namespace ArcGISMapViewer.Views
{
    public sealed partial class FeatureEditingNewPage : Page
    {
        public FeatureEditingNewPage()
        {
            this.InitializeComponent();
        }

        public ViewModels.ApplicationViewModel VM => ApplicationViewModel.Instance;
    }

}
