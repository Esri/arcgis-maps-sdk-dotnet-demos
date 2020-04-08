using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SymbolEditorApp.Controls
{
    /// <summary>
    /// Interaction logic for CollectionPropertyGrid.xaml
    /// </summary>
    public partial class CollectionPropertyGrid : UserControl
    {
        public CollectionPropertyGrid()
        {
            InitializeComponent();
        }

        public object Values
        {
            get { return (object)GetValue(ValuesProperty); }
            set { SetValue(ValuesProperty, value); }
        }

        public static readonly DependencyProperty ValuesProperty =
            DependencyProperty.Register("Values", typeof(object), typeof(CollectionPropertyGrid), new PropertyMetadata(null));

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var value = (sender as Button).DataContext;
            MetroDialog.ShowDialog(value.GetType().Name, new AutoPropertyGrid() { Value = value, MaxWidth = 400, MinWidth = 300 }, this, showCancel: false);
        }
    }
    public class TypeNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Regex.Replace(value?.GetType().Name ?? "", "(\\B[A-Z])", " $1");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
