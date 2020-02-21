using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using SymbolEditorApp.Controls;
using System;
using System.Linq;
using System.Windows.Controls;

namespace SymbolEditorApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MahApps.Metro.Controls.MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TableOfContents_TableOfContentContextMenuOpening(object sender, Esri.ArcGISRuntime.Toolkit.Preview.UI.Controls.TableOfContentsContextMenuEventArgs e)
        {
            Symbol symbolReference = null;
            Action<Symbol> symbolSetter = null;
            if(e.Content is LegendInfo legendInfo)
            {
                // Find the symbol in the layer that matches the one in the legend
                var layer = e.TableOfContentItem.Parent.Content as Layer;
                if (layer is FeatureLayer fl)
                {
                    var renderer = fl.Renderer;
                    if(renderer is SimpleRenderer sr)
                    {
                        symbolReference = sr.Symbol;
                        symbolSetter = (s) => sr.Symbol = symbolReference;
                    }
                    else if(renderer is UniqueValueRenderer uvr)
                    {
                        // TODO: Also handle uvr.DefaultSymbol
                        var uv = uvr.UniqueValues.Where(u => u.Label == legendInfo.Name);
                        if(uv.Count() > 1) // In case multiple symbols matches
                        {
                            uv = uv.Where(u => u.Symbol.ToJson() == legendInfo.Symbol.ToJson());
                        }
                        if(uv.Count() == 1)
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
            }

            if (symbolReference != null && symbolSetter != null)
            {
                e.MenuItems.Add(new MenuItem() { Header = "Edit symbol... " });
                ((MenuItem)e.MenuItems[0]).Click += (s,e) =>
                {
                    var editor = new SymbolEditor();
                    editor.Symbol = symbolReference;
                    var window = new MahApps.Metro.Controls.MetroWindow()
                    {
                        Width = 600,
                        Height = 500,
                        Owner = this,
                        Content = editor,
                        Title = "Symbol Editor"
                    };
                    if (window.ShowDialog() == true)
                    {
                        symbolSetter(editor.Symbol);
                    }
                };
            }
        }

        private void Settings_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var window = new SettingsWindow() { Owner = this };
            window.ShowDialog();
        }
    }
}
