using System.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ArcGISMapViewer.Controls
{
    public sealed class Pager : Control
    {
        public Pager()
        {
            this.DefaultStyleKey = typeof(Pager);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if(GetTemplateChild("PART_PreviousButton") is Button previousButton)
            {
                previousButton.Click += PreviousButton_Click;
            }
            if(GetTemplateChild("PART_NextButton") is Button nextButton)
            {
                nextButton.Click += NextButton_Click;
            }
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            var index = SelectedIndex - 1;
            if (index < 0)
                index = ItemsSource.Count - 1;
            SelectedIndex = index;
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            var index = SelectedIndex + 1;
            if (index >= ItemsSource.Count)
                index = 0;
            SelectedIndex = index;
        }

        public object? SelectedItem
        {
            get { return (object?)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(Pager), new PropertyMetadata(null));

        public IList ItemsSource
        {
            get { return (IList)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IList), typeof(Pager), new PropertyMetadata(null, (s, e) => ((Pager)s).ItemsSourcePropertyChanged(e.NewValue as IList)));

        private void ItemsSourcePropertyChanged(IList? list)
        {
            if (SelectedIndex != 0)
                SelectedIndex = 0;
            else
                SelectedIndexPropertyChanged(0);
            this.Visibility = list is null || list.Count <= 1 ? Visibility.Collapsed : Visibility.Visible;
        }

        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(int), typeof(Pager), new PropertyMetadata(0, (s, e) => ((Pager)s).SelectedIndexPropertyChanged((int)e.NewValue)));

        private void SelectedIndexPropertyChanged(int newValue)
        {
            if (ItemsSource != null && newValue >= 0 && newValue < ItemsSource.Count)
            {
                SelectedItem = ItemsSource[newValue];
            }
            else
                SelectedItem = null;
            if (GetTemplateChild("CurrentText") is TextBlock tb)
            {
                if (ItemsSource != null && ItemsSource.Count > 0)
                    tb.Text = string.Format(CurrentTextFormat, newValue + 1, ItemsSource?.Count);
                else
                    tb.Text = "";
            }
        }

        private const string CurrentTextFormat = "{0} of {1}";
    }
}
