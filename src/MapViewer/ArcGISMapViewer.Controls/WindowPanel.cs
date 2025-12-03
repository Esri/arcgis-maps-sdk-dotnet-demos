using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcGISMapViewer.Controls
{
    [ContentProperty(Name = nameof(Content))]
    public partial class WindowPanel : Control
    {
        public WindowPanel()
        {
            DefaultStyleKey = typeof(WindowPanel);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        public object? Header
        {
            get { return GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(nameof(Header), typeof(object), typeof(WindowPanel), new PropertyMetadata(null));

        public object? Content
        {
            get { return GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register(nameof(Content), typeof(object), typeof(WindowPanel), new PropertyMetadata(null));

        public bool IsCloseButtonVisible
        {
            get { return (bool)GetValue(IsCloseButtonVisibleProperty); }
            set { SetValue(IsCloseButtonVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsCloseButtonVisibleProperty =
            DependencyProperty.Register(nameof(IsCloseButtonVisible), typeof(bool), typeof(WindowPanel), new PropertyMetadata(true));

        public event EventHandler? CloseRequested;

    }
}
