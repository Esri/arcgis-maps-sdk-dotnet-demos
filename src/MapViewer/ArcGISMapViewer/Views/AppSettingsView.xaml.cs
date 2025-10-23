using System;
using System.Linq;
using Microsoft.UI.Xaml.Controls;

namespace ArcGISMapViewer.Views;

public sealed partial class AppSettingsView : UserControl
{
    public AppSettingsView()
    {
        InitializeComponent();
        styleSelector.ItemsSource = Enum.GetValues<AppStyleResources.Theme>().ToList();
        styleSelector.SelectedItem = ApplicationViewModel.Instance.AppSettings.StyleTheme;
        styleSelector.SelectionChanged += StyleSelector_SelectionChanged;
        themeSelector.ItemsSource = Enum.GetValues<ElementTheme>().Select(s => s.ToString()).ToList();
        themeSelector.SelectedItem = ApplicationViewModel.Instance.AppSettings.Theme.ToString();
        themeSelector.SelectionChanged += ThemeSelector_SelectionChanged;
    }

    private void StyleSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ApplicationViewModel.Instance.AppSettings.StyleTheme = e.AddedItems.FirstOrDefault() as AppStyleResources.Theme? ?? AppStyleResources.Theme.Calcite;
    }

    private void ThemeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var name = e.AddedItems.FirstOrDefault();
        ApplicationViewModel.Instance.AppSettings.Theme = name switch
        {
            "Light" => ElementTheme.Light,
            "Dark" => ElementTheme.Dark,
            _ => ElementTheme.Default,
        };
    }
}