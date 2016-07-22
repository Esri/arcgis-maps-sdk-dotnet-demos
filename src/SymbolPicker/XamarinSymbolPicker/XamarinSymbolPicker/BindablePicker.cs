namespace XamarinSymbolPicker
{
    using System;
    using System.Collections;
    using Xamarin.Forms;

    public class BindablePicker : Picker
    {
        public BindablePicker()
        {
            this.SelectedIndexChanged += this.BindablePicker_SelectedIndexChanged;
        }

        private void BindablePicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.SelectedIndex < 0 || this.SelectedIndex > this.Items.Count - 1)
            {
                this.SelectedItem = null;
            }
            else
            {
                this.SelectedItem = this.Items[this.SelectedIndex];
            }
        }

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)this.GetValue(ItemsSourceProperty); }
            set { this.SetValue(ItemsSourceProperty, value); }
        }

#pragma warning disable SA1401 // Fields must be private
        public static BindableProperty ItemsSourceProperty =
#pragma warning restore SA1401 // Fields must be private
#pragma warning disable CS0618 // Type or member is obsolete
            BindableProperty.Create<BindablePicker, IEnumerable>(p => p.ItemsSource, default(IEnumerable), propertyChanged: OnItemsSourceChanged);
#pragma warning restore CS0618 // Type or member is obsolete

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
            get { return (object)this.GetValue(SelectedItemProperty); }
            set { this.SetValue(SelectedItemProperty, value); }
        }

#pragma warning disable SA1401 // Fields must be private
        public static BindableProperty SelectedItemProperty =
#pragma warning restore SA1401 // Fields must be private
#pragma warning disable CS0618 // Type or member is obsolete
            BindableProperty.Create<BindablePicker, object>(p => p.SelectedItem, default(object), propertyChanged: OnSelectedItemChanged);
#pragma warning restore CS0618 // Type or member is obsolete

        private static void OnSelectedItemChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            var picker = bindable as BindablePicker;
            var newSelection = (string)newvalue;
            picker.SelectedIndex = string.IsNullOrEmpty(newSelection) ? -1 : picker.Items.IndexOf(newSelection);
        }
    }
}