using Esri.ArcGISRuntime.Symbology;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SymbolEditorApp.Controls.RendererEditors
{
    /// <summary>
    /// Interaction logic for UniqueValueRendererEditor.xaml
    /// </summary>
    public partial class UniqueValueRendererEditor : UserControl
    {
        public UniqueValueRendererEditor()
        {
            InitializeComponent();
            DataContext = this;
        }

        public UniqueValueRenderer Renderer
        {
            get { return (UniqueValueRenderer)GetValue(RendererProperty); }
            set { SetValue(RendererProperty, value); }
        }

        public static readonly DependencyProperty RendererProperty =
            DependencyProperty.Register("Renderer", typeof(UniqueValueRenderer), typeof(UniqueValueRendererEditor), new PropertyMetadata(null));


        private void SymbolButton_Click(object sender, RoutedEventArgs e)
        {
            var value = ((Button)sender).DataContext as UniqueValue;
            if (value == null)
                return;
            var editor = new SymbolEditor();
            editor.Symbol = value.Symbol?.Clone() ?? new SimpleMarkerSymbol();
            var result = MetroDialog.ShowDialog("Symbol Editor", editor, this);
            if (result == true)
            {
                value.Symbol = editor.Symbol;
            }
        }
    }

    public class ObjectCollectionToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(!(value is string) && value is IEnumerable ie)
            {
                StringBuilder sb = new StringBuilder();
                foreach(var item in ie)
                {
                    if (sb.Length > 0)
                        sb.Append(',');
                    sb.Append(item.ToString());
                }
                return sb.ToString();
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
