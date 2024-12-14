using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace ArcGISMapViewer.Controls
{
    public partial class Separator : ContentControl
    {
        private double startWidth = double.NaN;

        public Separator()
        {
            ManipulationMode = ManipulationModes.TranslateRailsX;
            ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.SizeWestEast);
            Width = 6;
            Content = new Border() { Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent), HorizontalAlignment = HorizontalAlignment.Stretch, Child = new Border() { Width = 1, Background = new SolidColorBrush(Microsoft.UI.Colors.Black), HorizontalAlignment = HorizontalAlignment.Center } };
            HorizontalAlignment = HorizontalAlignment.Stretch; VerticalAlignment = VerticalAlignment.Stretch;
            HorizontalContentAlignment = HorizontalAlignment.Stretch; VerticalContentAlignment = VerticalAlignment.Stretch;
        }

        private FrameworkElement? GetSibling()
        {
            if (!(Parent is Panel panel))
                return null;
            var idx = panel.Children.IndexOf(this);
            if (idx < 1) return null;
            var sibling = panel.Children[idx - 1] as FrameworkElement;
            return sibling;
        }

        protected override void OnManipulationStarted(ManipulationStartedRoutedEventArgs e)
        {
            base.OnManipulationStarted(e);
            var sibling = GetSibling();
            if (sibling is not null)
            {
                startWidth = double.IsNaN(sibling.Width) ? sibling.ActualWidth : sibling.Width;
            }
        }

        protected override void OnManipulationDelta(ManipulationDeltaRoutedEventArgs e)
        {
            base.OnManipulationDelta(e);
            var sibling = GetSibling();
            if (sibling is not null && !double.IsNaN(startWidth))
                sibling.Width = Math.Max(0, startWidth + e.Cumulative.Translation.X);
        }

        protected override void OnManipulationCompleted(ManipulationCompletedRoutedEventArgs e)
        {
            var sibling = GetSibling();
            if (sibling is not null && !double.IsNaN(startWidth))
                sibling.Width = Math.Max(0, startWidth + e.Cumulative.Translation.X);
            startWidth = double.NaN;
            base.OnManipulationCompleted(e);
        }

        protected override void OnDoubleTapped(DoubleTappedRoutedEventArgs e)
        {
            base.OnDoubleTapped(e);
            var sibling = GetSibling();
            if (sibling is not null)
            {
                sibling.Width = double.NaN;
                double minWidth = 0;
                if (sibling.DataContext is FeatureAttibuteColumn column)
                {
                    minWidth = column.MaxWidth;
                }
                sibling.Measure(new Windows.Foundation.Size(double.PositiveInfinity, double.PositiveInfinity));
                if (!double.IsNaN(sibling.DesiredSize.Width) && sibling.DesiredSize.Width > 0)
                    minWidth = Math.Max(minWidth, sibling.DesiredSize.Width);
                sibling.Width = minWidth;
            }
        }

        protected override void OnPointerEntered(PointerRoutedEventArgs e)
        {
            base.OnPointerEntered(e);
        }

        protected override void OnPointerExited(PointerRoutedEventArgs e)
        {
            base.OnPointerExited(e);
        }
    }
}
