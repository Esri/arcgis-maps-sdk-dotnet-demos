using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.Symbology;
using Windows.Security.DataProtection;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ArcGISMapViewer.Controls;

public sealed partial class TemplatePicker : UserControl
{
    public TemplatePicker()
    {
        InitializeComponent();
    }


    private async void UpdateTemplates()
    {
        List<TemplateItem> items = new();
        await ProcessLayers(items, Layers);
    }
    private async Task ProcessLayers(List<TemplateItem> items, IEnumerable<Layer>? layers)
    {
        if (layers is not null)
        {
            foreach (var layer in layers)
            {
                if (layer is GroupLayer groupLayer)
                {
                    await ProcessLayers(items, groupLayer.Layers);
                    continue;
                }
                if (layer is not FeatureLayer fl)
                    continue;
                try
                {
                    await fl.LoadAsync();
                }
                catch { continue; }
                if (fl.FeatureTable?.IsEditable != true)
                    continue;
                if (fl.FeatureTable is ArcGISFeatureTable atable)
                {
                    foreach (var t in atable.FeatureTemplates)
                    {
                        var symbol = fl.Renderer?.GetSymbol(new Graphic(attributes: t.PrototypeAttributes));
                        items.Add(new TemplateItem()
                        {
                            Name = t.Name,
                            Template = t,
                            Symbol = symbol,
                            Description = t.Description,
                            Tool = t.DrawingTool,
                            Prototype = t.PrototypeAttributes
                        });
                    }
                }
                else
                {
                    // Generate prototypes based on renderers if possible
                    if (fl.Renderer is SimpleRenderer s)
                        items.Add(new TemplateItem() { Name = fl.Name, Symbol = s.Symbol }); // TODO: Use renderer to generate prototypes
                    else if (fl.Renderer is UniqueValueRenderer uvr)
                    {
                        foreach(var u in uvr.UniqueValues)
                        {
                            List<KeyValuePair<string, object?>> attr = new();
                            for (int i = 0; i < uvr.FieldNames.Count; i++)
                            {
                                attr.Add(new KeyValuePair<string, object?>(uvr.FieldNames[i], u.Values[i]));
                            }
                            items.Add(new TemplateItem()
                            {
                                Name = u.Label,
                                Symbol = u.Symbol,
                                Description = u.Description,
                                Prototype = attr
                            });
                        }
                        if (uvr.DefaultSymbol != null)
                        {
                            items.Add(new TemplateItem()
                            {
                                Name = uvr.DefaultLabel,
                                Symbol = uvr.DefaultSymbol,
                            });
                        }
                    }
                    else if (fl.Renderer is ClassBreaksRenderer cbr)
                    {
                        items.Add(new TemplateItem()
                        {
                            Name = cbr.DefaultLabel,
                            Symbol = cbr.DefaultSymbol,
                        });
                    }
                }
            }
        }
        TemplateList.ItemsSource = items;
    }

    public LayerCollection Layers
    {
        get { return (LayerCollection)GetValue(LayersProperty); }
        set { SetValue(LayersProperty, value); }
    }

    public static readonly DependencyProperty LayersProperty =
        DependencyProperty.Register("Layers", typeof(LayerCollection), typeof(TemplatePicker), new PropertyMetadata(null, static (s, e) => ((TemplatePicker)s).OnLayerCollectionPropertyChanged(e.OldValue as LayerCollection, e.NewValue as LayerCollection)));

    private void OnLayerCollectionPropertyChanged(LayerCollection? oldLayers, LayerCollection? newLayers)
    {
        if(oldLayers is not null)
        oldLayers.CollectionChanged -= Layers_CollectionChanged;
        if (newLayers is not null)
            newLayers.CollectionChanged += Layers_CollectionChanged;
        UpdateTemplates();
    }

    private void Layers_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        UpdateTemplates();
    }

    private void TemplateList_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is TemplateItem ti)
            TemplateSelected?.Invoke(this, ti);
    }

    public event EventHandler<TemplateItem>? TemplateSelected;
}

public class TemplateItem
{
    public string Name { get; set; } = string.Empty;
    public Esri.ArcGISRuntime.Symbology.Symbol? Symbol { get; set; } = null!;
    public FeatureTemplate? Template { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public DrawingTool Tool { get; set; } = DrawingTool.None;
    public IEnumerable<KeyValuePair<string, object?>>? Prototype { get; internal set; }
}
