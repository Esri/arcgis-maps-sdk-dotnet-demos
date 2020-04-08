using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace SymbolEditorApp.Controls
{
    public class ObjectPropertyGrid : AutoPropertyGrid
    {
        public Type Type
        {
            get { return (Type)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register("Type", typeof(Type), typeof(ObjectPropertyGrid), new PropertyMetadata(null, OnTypePropertyChanged));

        private static void OnTypePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ObjectPropertyGrid)d).BuildPanel();
        }

        protected override void BuildPanel()
        {
            if(Value == null && Type != null)
            {
                Children.Clear();
                var button = new Button() { Content = "New" };
                button.Click += (s, e) =>
                {
                    Value = Activator.CreateInstance(Type);
                };
                Grid.SetColumnSpan(button, 2);
                Children.Add(button);
                return;
            }
            base.BuildPanel();
        }
    }
}
