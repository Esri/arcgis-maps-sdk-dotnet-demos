using System;
using System.Collections;
using Microsoft.UI.Xaml;

namespace ArcGISMapViewer;

public sealed partial class AppStyleResources : ResourceDictionary
{
    private AppStyleResources()
    {
        InitializeComponent();
    }

    public static ResourceDictionary? GetStyle(Theme theme)
    {
        AppStyleResources resources = new AppStyleResources();
        var key = theme.ToString() + "Theme";
        ResourceDictionary? r = null;
        if (resources.ContainsKey(key))
        {
            r = resources[key] as ResourceDictionary;
            resources.Remove(key);
        }
        return r;
    }

    public enum Theme : int
    {
        Amber,
        Calcite,
        Crimson,
        Emerald,
        Magenta,
        Purple,
        Tangerine,
        Teal
    }
}