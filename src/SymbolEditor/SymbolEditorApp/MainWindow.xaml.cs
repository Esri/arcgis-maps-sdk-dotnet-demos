using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using SymbolEditorApp.Controls;
using System;
using System.Linq;
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
        }

        private void TableOfContents_TocItemContextMenuOpening(object sender, Esri.ArcGISRuntime.Toolkit.Preview.UI.Controls.TocItemContextMenuEventArgs e)
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
                    var d = new MetroDialog() { Child = editor, SizeToContent = SizeToContent.Manual, Width = 400, Height = 500, ResizeMode = ResizeMode.CanResize, Owner = this, Title = "Symbology Editor" };
                    if (d.ShowDialog() == true)
                    {
                        fl.Renderer = editor.Renderer;
                        e.Item.RefreshLegend();
                    }
                };
            }
        }

        private void Settings_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var window = new SettingsWindow() { Owner = this };
            window.ShowDialog();
        }

        private async void OnTreeViewItemMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var item = (sender as FrameworkElement).DataContext as Esri.ArcGISRuntime.Toolkit.Preview.UI.TocItem;
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
                    var result = MetroDialog.ShowDialog("Symbol Editor", editor, this);
                    if (result == true)
                    {
                        symbolSetter(editor.Symbol);
                        item.Parent.RefreshLegend();
                    }
                }
            }
        }
    }
}
