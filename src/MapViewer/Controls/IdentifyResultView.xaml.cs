using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Esri.ArcGISRuntime.Data;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace ArcGISMapViewer.Controls
{
    public sealed partial class IdentifyResultView : UserControl
    {
        public IdentifyResultView()
        {
            this.InitializeComponent();
        }

        public IReadOnlyList<IdentifyLayerResult> IdentifyResult
        {
            get { return (IReadOnlyList<IdentifyLayerResult>)GetValue(IdentifyResultProperty); }
            set { SetValue(IdentifyResultProperty, value); }
        }

        public static readonly DependencyProperty IdentifyResultProperty =
            DependencyProperty.Register(nameof(IdentifyResult), typeof(IReadOnlyList<IdentifyLayerResult>), typeof(IdentifyResultView), new PropertyMetadata(null));

    }
}
