using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ArcGISMapViewer.Controls
{
    public partial class ActionCell : Cell
    {
        private readonly IconSourceElement iconSourceElement;

        public ActionCell()
        {
            iconSourceElement = new IconSourceElement();
            Content = iconSourceElement;
            ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand);
            HorizontalAlignment = HorizontalAlignment.Center;
            Margin = new Thickness(2, 0, 0, 0);
            this.PointerPressed += ActionCell_PointerPressed;
        }

        public EventHandler? Invoked;

        private void ActionCell_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Invoked?.Invoke(this, EventArgs.Empty);
        }

        public IconSource? Icon
        {
            get { return (IconSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(IconSource), typeof(ActionCell), new PropertyMetadata(null, (s, e) => ((ActionCell)s).OnIconPropertyChanged(e.NewValue as IconSource)));

        private void OnIconPropertyChanged(IconSource? icon)
        {
            iconSourceElement.IconSource = icon;
        }

        public string? Tooltip
        {
            get { return (string)GetValue(TooltipProperty); }
            set { SetValue(TooltipProperty, value); }
        }

        public static readonly DependencyProperty TooltipProperty =
            DependencyProperty.Register(nameof(Tooltip), typeof(string), typeof(ActionCell), new PropertyMetadata("", (s, e) => ((ActionCell)s).OnTooltipPropertyChanged(e.NewValue as string)));

        private void OnTooltipPropertyChanged(string? text)
        {
            ToolTipService.SetToolTip(this, text);
        }
    }
}
