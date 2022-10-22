using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using SymbolEditorApp.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SymbolEditorApp
{
    public partial class MainWindow : MahApps.Metro.Controls.MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            ShowSidePanel(null);
        }
        private void TableOfContents_TocItemContextMenuOpening(object sender, Esri.ArcGISRuntime.Toolkit.UI.Controls.TocItemContextMenuEventArgs e)
        {
            if(e.Item.Layer is Layer layer)
            {
                var remove = new MenuItem() { Header = "Remove", Icon = new TextBlock() { Text = "", FontFamily = new FontFamily("Segoe UI Symbol") } };
                remove.Click += (s, a) =>
                {
                    var result = MessageBox.Show("Remove layer " + layer.Name + " ?", "Confirm", MessageBoxButton.OKCancel);
                    if (result == MessageBoxResult.OK)
                    {
                        if (mapView.Map.OperationalLayers.Contains(layer))
                            mapView.Map.OperationalLayers.Remove(layer);
                        else if(e.Item.Parent.LayerContent is GroupLayer gl && gl.Layers.Contains(layer))
                            gl.Layers.Remove(layer);
                    }
                };
                e.MenuItems.Add(remove);
            }
            if(e.Item.Layer is FeatureLayer fl)
            {
                var symbologyMenu = new MenuItem() { Header = "Symbology" };
                e.MenuItems.Add(symbologyMenu);
                symbologyMenu.Click += (s, a) =>
                {
                    var editor = new SymbologyEditor() { Renderer = fl.Renderer.Clone() };
                    ShowSidePanelDialog(editor, () => { fl.Renderer = editor.Renderer.Clone(); e.Item.RefreshLegend(); });
                };
            }

            if(e.Item.Content is LegendInfo li)
            {
                var jsonMenu = new MenuItem() { Header = "View JSON" };
                e.MenuItems.Add(jsonMenu);
                jsonMenu.Click += (s, a) =>
                {
                    var viewer = new SymbolJsonViewer() { Symbol = li.Symbol, Owner = this };
                    viewer.Show();
                };
            }
        }

        private void Settings_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var window = new SettingsWindow() { Owner = this, WindowStartupLocation = WindowStartupLocation.CenterOwner };
            window.ShowDialog();
        }

        private void OnTreeViewItemMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var item = (sender as FrameworkElement).DataContext as Esri.ArcGISRuntime.Toolkit.UI.TocItem;
            if(item?.Content is LegendInfo legendInfo)
            {
                Symbol symbolReference = null;
                Action<Symbol> symbolSetter = null;
                // Find the symbol in the layer that matches the one in the legend
                var layer = item.Layer;
                if (layer is FeatureLayer fl)
                {
                    var renderer = fl.Renderer;
                    if (renderer is SimpleRenderer sr)
                    {
                        symbolReference = sr.Symbol;
                        symbolSetter = (s) => sr.Symbol = s;
                    }
                    else if (renderer is UniqueValueRenderer uvr)
                    {
                        // TODO: Also handle uvr.DefaultSymbol
                        var uv = uvr.UniqueValues.Where(u => u.Label == legendInfo.Name);
                        if (uv.Count() > 1) // In case multiple symbols matches
                        {
                            uv = uv.Where(u => u.Symbol.ToJson() == legendInfo.Symbol.ToJson());
                        }
                        if (uv.Count() == 1)
                        {
                            var u = uv.First();
                            symbolReference = u.Symbol;
                            symbolSetter = (s) => u.Symbol = s;
                        }
                    }
                    else
                    {
                        // TODO
                    }
                }

                if (symbolReference != null && symbolSetter != null)
                {
                    var editor = new SymbolEditor();
                    editor.Symbol = symbolReference?.Clone();
                    ShowSidePanelDialog(editor, () => { symbolSetter(editor.Symbol); item.Parent.RefreshLegend(); });
                }
            }
        }

        public void ShowSidePanel(UIElement panel)
        {
            SidePanel.Content = panel;
            SidePanelContainer.Visibility = panel != null ? Visibility.Visible : Visibility.Collapsed;
            SidePanelResizer.Visibility = panel != null ? Visibility.Visible : Visibility.Collapsed;
            Grid.SetColumnSpan(mapView, panel == null ? 3 : 1);
        }

        public void ShowSidePanelDialog(UIElement panel, Action onComplete)
        {
            Grid g = new Grid();
            g.RowDefinitions.Add(new RowDefinition());
            g.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
            g.Children.Add(panel);
            StackPanel sp = new StackPanel() { Orientation = Orientation.Horizontal, HorizontalAlignment = System.Windows.HorizontalAlignment.Right, Margin = new Thickness(0,10,0,0) };
            Grid.SetRow(sp, 1);
            var cancelButton = new Button() { Content = "Cancel" };
            cancelButton.Click += (s, e) => ShowSidePanel(null);
            var applyButton = new Button() { Content = "Apply" };
            applyButton.Click += (s,e) => onComplete();
            sp.Children.Add(cancelButton);
            sp.Children.Add(applyButton);            
            g.Children.Add(sp);

            ShowSidePanel(g);
        }

        private void CloseSidePanel_Click(object sender, RoutedEventArgs e)
        {
            ShowSidePanel(null);
        }
    }
}
