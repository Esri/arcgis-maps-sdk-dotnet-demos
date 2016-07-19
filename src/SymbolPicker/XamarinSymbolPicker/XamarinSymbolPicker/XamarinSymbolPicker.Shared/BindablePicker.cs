using System;
using System.Collections;
using Xamarin.Forms;

namespace XamarinSymbolPicker
{
    public class BindablePicker : Picker
    {
        public BindablePicker()
        {
            this.SelectedIndexChanged += BindablePicker_SelectedIndexChanged;
        }

        private void BindablePicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedIndex < 0 || SelectedIndex > Items.Count - 1)
            {
                SelectedItem = null;
            }
            else
            {
                SelectedItem = Items[SelectedIndex];
            }
        }
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static BindableProperty ItemsSourceProperty =
            BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(BindablePicker), propertyChanged: OnItemsSourceChanged);

        private static void OnItemsSourceChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            var picker = bindable as BindablePicker;
            var newSource = (IEnumerable)newvalue;
            picker.Items.Clear();
            if (newSource != null)
            {
                foreach (var item in newSource)
                {
                    picker.Items.Add((string)item);
                }
            }
        }

        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static BindableProperty SelectedItemProperty =
            BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(BindablePicker), propertyChanged : OnSelectedItemChanged);

        private static void OnSelectedItemChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            var picker = bindable as BindablePicker;
            var newSelection = (string)newvalue;
            picker.SelectedIndex = string.IsNullOrEmpty(newSelection) ? -1 : picker.Items.IndexOf(newSelection);
        }
    }
}