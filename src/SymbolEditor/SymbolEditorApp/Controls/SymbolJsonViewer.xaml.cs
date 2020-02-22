using Esri.ArcGISRuntime.Symbology;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SymbolEditorApp.Controls
{
    /// <summary>
    /// Interaction logic for SymbolJsonViewer.xaml
    /// </summary>
    public partial class SymbolJsonViewer : MetroWindow
    {
        public SymbolJsonViewer()
        {
            InitializeComponent();
        }

        private void UpdateJson()
        {
            jsonView.Text = FormatJson(Symbol?.ToJson());
        }

        public Symbol Symbol
        {
            get { return (Symbol)GetValue(SymbolProperty); }
            set { SetValue(SymbolProperty, value); }
        }

        public static readonly DependencyProperty SymbolProperty =
            DependencyProperty.Register("Symbol", typeof(Symbol), typeof(SymbolJsonViewer), new PropertyMetadata(null, OnSymbolPropertyChanged));

        private static void OnSymbolPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SymbolJsonViewer)d).UpdateJson();
        }

        private void CopyToClipboard(string data)
        {
            if (data == null)
                return;
            Clipboard.SetText(data);
        }

        private void CopyJson_Click(object sender, RoutedEventArgs e) => CopyToClipboard(Symbol?.ToJson());

        private void CopyJsonFormatted_Click(object sender, RoutedEventArgs e)
        {
            if (Symbol == null)
                return;
            CopyToClipboard(FormatJson(Symbol.ToJson()));
        }
        private string FormatJson(string json)
        {
            if (json == null)
                return null;
            var doc = System.Text.Json.JsonDocument.Parse(Symbol.ToJson());
            using (var ms = new System.IO.MemoryStream())
            {
                using (var jw = new System.Text.Json.Utf8JsonWriter(ms, new System.Text.Json.JsonWriterOptions() { Indented = true }))
                {
                    doc.WriteTo(jw);
                    jw.Flush();
                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
        }

        private void CopyJsonCSharp_Click(object sender, RoutedEventArgs e)
        {
            if (Symbol == null)
                return;
            CopyToClipboard($"var symbol = Esri.ArcGISRuntime.Symbology.Symbol.FromJson(\"{Symbol.ToJson().Replace("\"", "\\\"")}\");");
        }

        private void CopyJsonVB_Click(object sender, RoutedEventArgs e)
        {
            if (Symbol == null)
                return;
            CopyToClipboard($"Dim symbol = Esri.ArcGISRuntime.Symbology.Symbol.FromJson(\"{Symbol.ToJson().Replace("\"", "\"\"")}\")");
        }
    }
}
