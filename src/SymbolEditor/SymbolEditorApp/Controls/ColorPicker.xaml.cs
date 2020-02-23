using System;
using System.Collections.Generic;
using System.Drawing;
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

namespace SymbolEditorApp.Controls
{
    /// <summary>
    /// Interaction logic for ColorPicker.xaml
    /// </summary>
    public partial class ColorPicker : UserControl
    {
        public ColorPicker()
        {
            this.MinWidth = 100;
            InitializeComponent();
            var values = Enum.GetValues(typeof(KnownColor));
            List<Brush> colors = new List<Brush>();
            var props = typeof(System.Windows.Media.Colors).GetProperties(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            foreach(var value in props)
            {
                colors.Add(new SolidColorBrush((System.Windows.Media.Color)value.GetValue(null)));
            }
            list.ItemsSource = colors;
        }



        public System.Drawing.Color Color
        {
            get { return (System.Drawing.Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(System.Drawing.Color), typeof(ColorPicker), new PropertyMetadata(System.Drawing.Color.Transparent, (d,e) => ((ColorPicker)d).OnColorPropertyChanged(e)));

        private void OnColorPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            var color = Color;
            Selection.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
            ColorChanged?.Invoke(this, EventArgs.Empty);
        }

        private void DropdownClick(object sender, RoutedEventArgs e)
        {
            dropdown.IsOpen = !dropdown.IsOpen;
        }

        private void list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dropdown.IsOpen = false;
            if (list.SelectedItem != null)
            {
                var c = (list.SelectedItem as SolidColorBrush).Color;
                Color = System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B);
                list.SelectedItem = null;
            }
        }

        public event EventHandler ColorChanged;
    }
}
