using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace ArcGISMapViewer.Controls
{
    public partial class TextCell : Cell
    {
        private long propertyChangeToken;
        private readonly TextBlock textBlock;
        public TextCell()
        {
            this.Content = textBlock = new TextBlock() { IsTextSelectionEnabled = true };
            this.Content = textBlock = new TextBlock() { IsTextSelectionEnabled = true };
            textBlock.AddHandler(TextBlock.DoubleTappedEvent, new Microsoft.UI.Xaml.Input.DoubleTappedEventHandler(TextBlock_DoubleTapped), true);
        }

        private void TextBlock_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            textBlock.SelectAll();
        }

        public string Text
        {
            get => textBlock.Text; set
            {
                if (value != textBlock.Text)
                {
                    textBlock.Text = value;
                    InvalidateMeasure();
                }
            }
        }
        
        public TextAlignment HorizontalTextAlignment { get => textBlock.HorizontalTextAlignment; set => textBlock.HorizontalTextAlignment = value; }


        private FeatureAttibuteColumn? column;

        public FeatureAttibuteColumn? Column
        {
            get { return column; }
            set
            {
                if (column != null && propertyChangeToken != 0)
                {
                    column.UnregisterPropertyChangedCallback(FeatureAttibuteColumn.WidthProperty, propertyChangeToken);
                    propertyChangeToken = 0;
                }
                column = value;
                if (column is not null)
                {
                    propertyChangeToken = column.RegisterPropertyChangedCallback(FeatureAttibuteColumn.WidthProperty, OnWidthPropertyChanged);
                    this.Width = column.Width;
                }
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            //if(isMaxLengthKnown)
                return base.MeasureOverride(availableSize);
            // var size = base.MeasureOverride(new Size(double.PositiveInfinity, availableSize.Height));
            // Column.MaxWidth = Math.Max(size.Width, Column.MaxWidth);
            //isMaxLengthKnown = true;
            //return size;
        }

        private void OnWidthPropertyChanged(DependencyObject sender, DependencyProperty dp)
        {
            this.Width = column?.Width ?? 0;
            // this.InvalidateMeasure();
        }
    }
}
