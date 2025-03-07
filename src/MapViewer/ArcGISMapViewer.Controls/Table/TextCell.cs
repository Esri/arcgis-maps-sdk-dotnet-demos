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

        public FeatureAttibuteColumn? Column { get; set; }

        internal double TextWidth => textBlock.DesiredSize.Width;

    }
}
