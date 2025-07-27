using System;
using System.Collections;
using Microsoft.UI.Xaml;

namespace ArcGISMapViewer.Controls;

public sealed partial class ThemeResources : ResourceDictionary
{
    public ThemeResources()
    {
        InitializeComponent();
    }

    public static Visibility EmptyToCollapsed(string value) => string.IsNullOrEmpty(value) ? Visibility.Collapsed : Visibility.Visible;
}