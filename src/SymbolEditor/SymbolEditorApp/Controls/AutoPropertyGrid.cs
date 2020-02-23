using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

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
                foreach (var property in Value.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).OrderBy(t=>t.Name))
                {
                    bool isCollection = property.PropertyType.Name == "IList`1";
                    if (!property.CanRead || !property.CanWrite && !isCollection)
                    {
                        continue;
                    }

                    string name = property.Name;
                    FrameworkElement editor = null;
                    if (isCollection)
                    {
                        var d = new CollectionPropertyGrid();
                        d.SetBinding(CollectionPropertyGrid.ValuesProperty, new Binding() { Path = new PropertyPath(name), Source = Value, Mode = property.CanWrite ? BindingMode.TwoWay : BindingMode.OneTime });
                        editor = d;
                    }
                    else if (property.PropertyType == typeof(double))
                    {
                        var d = new UniversalWPF.NumberBox() { SpinButtonPlacementMode = UniversalWPF.NumberBoxSpinButtonPlacementMode.Inline, AcceptsExpression = true };

                        if (name.Contains("Angle"))
                        {
                            d.Minimum = 0; d.Maximum = 360; d.IsWrapEnabled = true;
                        }
                        if (name.Contains("Size") || name.Contains("Width"))
                        {
                            d.Minimum = 1;
                        }
                        //var d = new DoubleUpDown() { Foreground = Application.Current.Resources["MahApps.Brushes.Text"] as Brush };
                        d.SetBinding(UniversalWPF.NumberBox.ValueProperty, new Binding() { Path = new PropertyPath(name), Source = Value, Mode = BindingMode.TwoWay });
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
                        d.SetBinding(ColorPicker.ColorProperty, new Binding() { Path = new PropertyPath(name), Source = Value, Mode = BindingMode.TwoWay });
                        editor = d;
                    }
                    else if (property.PropertyType == typeof(Esri.ArcGISRuntime.Geometry.Geometry))
                    {
                        var d = new TextBlock();
                        d.Text = (property.GetValue(Value) as Esri.ArcGISRuntime.Geometry.Geometry)?.ToJson();
                        //d.SetBinding(ColorPicker.ColorProperty, new Binding() { Path = new PropertyPath(name), Source = Value, Mode = BindingMode.TwoWay });
                        editor = d;
                    }
                    else if (!property.PropertyType.IsValueType &&
                        (property.GetValue(Value) != null && property.PropertyType.GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).Where(t=>t.CanRead && t.CanWrite).Any() 
                        || (property.PropertyType.GetConstructor(new Type[] { }) != null)))
                    {
                        var d = new ObjectPropertyGrid() { Type = property.PropertyType };
                        d.SetBinding(ObjectPropertyGrid.ValueProperty, new Binding() { Path = new PropertyPath(name), Source = Value, Mode = BindingMode.TwoWay });
                        editor = new Border() { Padding = new Thickness(2), BorderThickness = new Thickness(1), BorderBrush = Application.Current.Resources[MahApps.Metro.Theme.ThemeShowcaseBrushKey] as Brush, Child = d };
                    }
                    else
                    {
                    }

                    if (editor != null)
                    {
                        Children.Add(new TextBlock() { Text = Regex.Replace(name, "(\\B[A-Z])", " $1"), VerticalAlignment = VerticalAlignment.Center });
                        editor.Margin = new Thickness(5, 2, 0, 2);
                        Children.Add(editor);
                    }
                }
            }
        }
    }
}
