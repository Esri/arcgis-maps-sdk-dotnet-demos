using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace HydrographicsSample
{
    /// <summary>
    ///  Custom control that displays all the settable properties of an object instance
    /// </summary>
    public class PropertyGrid : StackPanel
    {
        public PropertyGrid()
        {
            Overrides = new OverrideList();
            Initialize();
        }

        /// <summary>
        /// The object instance to show properties for
        /// </summary>
        public object Instance
        {
            get { return (object)GetValue(InstanceProperty); }
            set { SetValue(InstanceProperty, value); }
        }

        public static readonly DependencyProperty InstanceProperty =
            DependencyProperty.Register("Instance", typeof(object), typeof(PropertyGrid), new PropertyMetadata(null, OnInstancePropertyChanged));

        private static void OnInstancePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PropertyGrid)d).Initialize();
        }

        private void Initialize()
        {
            Children.Clear();
            if (Instance == null)
                return;
            //DataContext = Instance;

            if (Instance == null)
                return;
            var props = Instance.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.GetProperty);


            foreach (var item in props)
            {
                string displayName = item.Name;
                IPropertyOverride propOverride = null;
                if (Overrides != null)
                {
                    propOverride = Overrides.Where(o => o.PropertyName == displayName).FirstOrDefault();
                }
                BindingMode bindingMode = item.CanWrite ? BindingMode.TwoWay : BindingMode.OneWay;

                displayName = System.Text.RegularExpressions.Regex.Replace(displayName, "(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z])", " $1", RegexOptions.Compiled).Trim();
                TextBlock text = new TextBlock() { Text = displayName, Opacity = .5, Margin = new Thickness(0,5,0,2) };
                if (item.PropertyType != typeof(bool))
                    Children.Add(text);
                if (item.PropertyType == typeof(bool))
                {
                    CheckBox cb = new CheckBox();
                    cb.Content = displayName;
                    cb.DataContext = Instance;
                    cb.SetBinding(CheckBox.IsCheckedProperty, new Binding(item.Name) { Mode = bindingMode });
                    cb.IsEnabled = item.CanWrite;
                    Children.Add(cb);
                }
                else if (item.PropertyType == typeof(double) ||
                    item.PropertyType == typeof(string) ||
                    item.PropertyType == typeof(float) ||
                    item.PropertyType == typeof(int) ||
                    item.PropertyType == typeof(long))
                {
                    if (propOverride is SliderOverride)
                    {
                        var po = (SliderOverride)propOverride;
                        Slider slider = new Slider() { HorizontalAlignment = HorizontalAlignment.Stretch };
                        slider.Minimum = po.MinValue;
                        slider.Maximum = po.MaxValue;
                        if (po.StepValue > 0)
                        {
                            slider.SmallChange = po.StepValue;
                        }
                        else if(item.PropertyType == typeof(int) || item.PropertyType == typeof(long))
                        {
                            slider.SmallChange = 1;
                        }
                        var binding = new Binding(item.Name) { Mode = bindingMode };
                        if (po.Delay > 0)
                            binding.Delay = binding.Delay;
                        slider.DataContext = Instance;
                        slider.IsEnabled = item.CanWrite;
                        slider.SetBinding(Slider.ValueProperty, binding);
                        Children.Remove(text);
                        Grid g = new Grid();
                        g.Children.Add(text);

                        TextBlock tb = new TextBlock() { HorizontalAlignment = HorizontalAlignment.Right, FontSize = 9 };
                        tb.DataContext = Instance;
                        tb.SetBinding(TextBlock.TextProperty, new Binding(item.Name) { Mode = BindingMode.OneWay, StringFormat="0.0" });
                        g.Children.Add(tb);
                        Children.Add(g);

                        Children.Add(slider);
                    }
                    else
                    {
                        TextBox tb = new TextBox() { HorizontalAlignment = HorizontalAlignment.Stretch };
                        tb.IsEnabled = item.CanWrite;
                        tb.DataContext = Instance;
                        tb.SetBinding(TextBox.TextProperty, new Binding(item.Name) { Mode = bindingMode });
                        Children.Add(tb);
                    }
                }
                else if (item.PropertyType == typeof(string) || item.PropertyType == typeof(Uri))
                {
                    TextBox tb = new TextBox() { HorizontalAlignment = HorizontalAlignment.Stretch };
                    tb.IsEnabled = item.CanWrite;
                    tb.DataContext = Instance;
                    tb.SetBinding(TextBox.TextProperty, new Binding(item.Name) { Mode = bindingMode });
                    Children.Add(tb);
                }
                else if (item.PropertyType.IsEnum)
                {
                    ComboBox cb = new ComboBox() { HorizontalAlignment = HorizontalAlignment.Stretch };
                    cb.IsEnabled = item.CanWrite;
                    cb.ItemsSource = Enum.GetValues(item.PropertyType);
                    cb.DataContext = Instance;
                    cb.SetBinding(ComboBox.SelectedItemProperty, new Binding(item.Name) { Mode = bindingMode });
                    Children.Add(cb);
                }
                else if(item.PropertyType.IsClass)
                {
                    PropertyGrid pg = new PropertyGrid();
                    //pg.SetBinding(FrameworkElement.DataContextProperty, new PropertyItem)
                    pg.DataContext = Instance;
                    pg.SetBinding(PropertyGrid.InstanceProperty, new Binding(item.Name) { Mode = BindingMode.OneWay });
                    Children.Add(pg);
                }
                else
                {
                    TextBox tb = new TextBox() { HorizontalAlignment = HorizontalAlignment.Stretch };
                    tb.DataContext = Instance;
                    tb.SetBinding(TextBox.TextProperty, new Binding(item.Name) { Mode = bindingMode });
                    tb.IsReadOnly = true;
                    tb.Foreground = System.Windows.Media.Brushes.Gray;
                    Children.Add(tb);
                }
            }
        }

        public OverrideList Overrides
        {
            get { return (OverrideList)GetValue(OverridesProperty); }
            set { SetValue(OverridesProperty, value); }
        }

        public static readonly DependencyProperty OverridesProperty =
            DependencyProperty.Register("Overrides", typeof(OverrideList), typeof(PropertyGrid), new PropertyMetadata(null));
    }

    public interface IPropertyOverride
    {
        string PropertyName { get; }
    }

    public class OverrideList : List<IPropertyOverride> { }
    /// <summary>
    /// Used by <see cref="PropertyGrid.Overrides"/>
    /// </summary>
    public class SliderOverride : IPropertyOverride
    {
        public string PropertyName { get; set; }
        public double MinValue { get; set; } = 0;
        public double MaxValue { get; set; } = 100;

        public double StepValue { get; set; } = 0;

        public int Delay { get; set; } = 0;
    }
}
