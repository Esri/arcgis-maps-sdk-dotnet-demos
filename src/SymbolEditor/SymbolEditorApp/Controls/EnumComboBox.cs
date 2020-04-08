using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace SymbolEditorApp.Controls
{
    public class EnumComboBox : ComboBox
    {
        public Type EnumType
        {
            get { return (Type)GetValue(EnumTypeProperty); }
            set { SetValue(EnumTypeProperty, value); }
        }

        public static readonly DependencyProperty EnumTypeProperty =
            DependencyProperty.Register("EnumType", typeof(Type), typeof(EnumComboBox), new PropertyMetadata(null, OnEnumTypePropertyChanged));

        private static void OnEnumTypePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is Type t)
            {
                ((EnumComboBox)d).SetItemsSource(t);
            }
        }

        private void SetItemsSource(Type t)
        {
            if (t.IsEnum)
                ItemsSource = Enum.GetValues(t);
        }
    }
}
