using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
    /// Interaction logic for SymbolPicker.xaml
    /// </summary>
    public partial class MetroDialog : MahApps.Metro.Controls.MetroWindow
    {
        public MetroDialog()
        {
            InitializeComponent();
        }

        public static bool? ShowDialog(string title, UIElement content, UIElement parent)
        {
            var window = parent as Window;
            while(window is null && parent != null)
            {
                parent = VisualTreeHelper.GetParent(parent) as UIElement;
                window = parent as Window;
            }
            var d = new MetroDialog() { Child = content, Title = title, Owner = window };
            var result = d.ShowDialog();
            d.Child = null;
            return result;
        }

        public UIElement Child
        {
            get { return (UIElement)GetValue(ChildProperty); }
            set { SetValue(ChildProperty, value); }
        }

        public static readonly DependencyProperty ChildProperty =
            DependencyProperty.Register("Child", typeof(UIElement), typeof(MetroDialog), new PropertyMetadata(null));

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
