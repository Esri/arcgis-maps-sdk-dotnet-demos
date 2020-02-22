using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;

namespace SymbolEditorApp.Controls
{
    public class AutoPropertyGrid : KeyValuePanel
    {
        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(AutoPropertyGrid), new PropertyMetadata(null, OnValuePropertyChanged));

        private static void OnValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AutoPropertyGrid)d).BuildPanel();
        }

        protected virtual void BuildPanel()
        {
            this.Children.Clear();
            if (!(Value is null))
            {
                foreach (var property in Value.GetType().GetProperties().OrderBy(t=>t.Name))
                {
                    bool isCollection = property.PropertyType.Name == "IList`1";
                    if (!property.CanRead || !property.CanWrite && !isCollection)
                    {
                        continue;
                    }

                    string name = property.Name;
                    FrameworkElement editor = null;
                    if(isCollection)
                    {
                        var d = new CollectionPropertyGrid();
                        d.SetBinding(CollectionPropertyGrid.ValuesProperty, new Binding() { Path = new PropertyPath(name), Source = Value, Mode = property.CanWrite ? BindingMode.TwoWay : BindingMode.OneTime });
                        editor = d;
                    }
                    else if (property.PropertyType == typeof(double))
                    {
                        var d = new DoubleUpDown();
                        d.SetBinding(DoubleUpDown.ValueProperty, new Binding() { Path = new PropertyPath(name), Source = Value, Mode = BindingMode.TwoWay });
                        editor = d;
                    }
                    else if (property.PropertyType.IsEnum)
                    {
                        var d = new EnumComboBox() { EnumType = property.PropertyType };
                        d.SetBinding(ComboBox.SelectedValueProperty, new Binding() { Path = new PropertyPath(name), Source = Value, Mode = BindingMode.TwoWay });
                        editor = d;
                    }
                    else if (property.PropertyType == typeof(string))
                    {
                        var d = new TextBox();
                        d.SetBinding(TextBox.TextProperty, new Binding() { Path = new PropertyPath(name), Source = Value, Mode = BindingMode.TwoWay });
                        editor = d;
                    }
                    else if (property.PropertyType == typeof(bool))
                    {
                        var d = new CheckBox();
                        d.SetBinding(CheckBox.IsCheckedProperty, new Binding() { Path = new PropertyPath(name), Source = Value, Mode = BindingMode.TwoWay });
                        editor = d;
                    }
                    else if (property.PropertyType == typeof(System.Drawing.Color))
                    {
                        var d = new ColorPicker();
                        d.SetBinding(ColorPicker.SelectedColorProperty, new Binding() { Path = new PropertyPath(name), Source = Value, Mode = BindingMode.TwoWay, Converter = new Converters.ColorConverter() });
                        editor = d;
                    }
                    else if (!property.PropertyType.IsValueType && (property.PropertyType.GetConstructor(new Type[] { }) != null || property.PropertyType.IsAbstract))
                    {
                        var d = new ObjectPropertyGrid() { Type = property.PropertyType };
                        d.SetBinding(ObjectPropertyGrid.ValueProperty, new Binding() { Path = new PropertyPath(name), Source = Value, Mode = BindingMode.TwoWay });
                        editor = new Border() { BorderThickness = new Thickness(1), BorderBrush = Application.Current.Resources[MahApps.Metro.Theme.ThemeShowcaseBrushKey] as Brush, Child = d };
                    }
                    else
                    {
                    }

                    if (editor != null)
                    {
                        Children.Add(new TextBlock() { Text = name, VerticalAlignment = VerticalAlignment.Center });
                        editor.Margin = new Thickness(5, 2, 0, 2);
                        Children.Add(editor);
                    }
                }
            }
        }
    }
}
