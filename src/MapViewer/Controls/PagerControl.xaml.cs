using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ArcGISMapViewer.Controls
{
    public sealed partial class PagerControl : UserControl
    {
        public PagerControl()
        {
            this.InitializeComponent();
        }



        public string SelectionText
        {
            get { return (string)GetValue(SelectionTextProperty); }
            set { SetValue(SelectionTextProperty, value); }
        }

        public static readonly DependencyProperty SelectionTextProperty =
            DependencyProperty.Register("SelectionText", typeof(string), typeof(PagerControl), new PropertyMetadata(""));

        public object? SelectedItem
        {
            get { return (object?)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(PagerControl), new PropertyMetadata(null));

        public IList ItemsSource
        {
            get { return (IList)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IList), typeof(PagerControl), new PropertyMetadata(null, (s,e) => ((PagerControl)s).ItemsSourcePropertyChanged(e.NewValue as IList)));

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
            DependencyProperty.Register("SelectedIndex", typeof(int), typeof(PagerControl), new PropertyMetadata(0, (s, e) => ((PagerControl)s).SelectedIndexPropertyChanged((int)e.NewValue)));

        private void SelectedIndexPropertyChanged(int newValue)
        {
            if(ItemsSource != null && newValue >= 0 && newValue < ItemsSource.Count)
            {
                SelectedItem = ItemsSource[newValue];
            }
            else 
                SelectedItem = null;
            if (ItemsSource != null && ItemsSource.Count > 0)
                SelectionText = $"{newValue + 1} of {ItemsSource?.Count}";
            else
                SelectionText = "";
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
    }
}
