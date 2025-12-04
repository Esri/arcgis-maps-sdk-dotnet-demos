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
            if (GetTemplateChild("CollapseButton") is Button collapseButton)
            {
                collapseButton.Click += (s, e) =>
                {
                    IsCollapsed = !IsCollapsed;
                };
            }
            if (GetTemplateChild("CloseButton") is Button closeButton)
            {
                closeButton.Click += (s, e) =>
                {
                    CloseRequested?.Invoke(this, EventArgs.Empty);
                };
            }
            if (GetTemplateChild("ResizeThumb") is UIElement elm)
            {
                elm.ManipulationDelta += ResizeThumb_ManipulationDelta;
            }
            OnIsCollapsedPropertyChanged(false);
            OnStatePropertyChanged(false);
        }

        private void ResizeThumb_ManipulationDelta(object sender, Microsoft.UI.Xaml.Input.ManipulationDeltaRoutedEventArgs e)
        {
            if(e.Delta.Translation.Y != 0)
            {
                var parent = GetTemplateChild("LayoutRoot") as FrameworkElement;
                if (parent is not null)
                {
                    var newHeight = parent.ActualHeight - e.Delta.Translation.Y;
                    var totalHeight = parent.ActualHeight / HeightRatio;
                    HeightRatio = Math.Min(1, Math.Max(0, newHeight / totalHeight));
                }
            }
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

        public bool IsCollapsed
        {
            get { return (bool)GetValue(IsCollapsedProperty); }
            set { SetValue(IsCollapsedProperty, value); }
        }

        public static readonly DependencyProperty IsCollapsedProperty =
            DependencyProperty.Register(nameof(IsCollapsed), typeof(bool), typeof(WindowPanel), new PropertyMetadata(false, (s, e) => ((WindowPanel)s).OnIsCollapsedPropertyChanged()));

        private void OnIsCollapsedPropertyChanged(bool useTransitions = true)
        {
            VisualStateManager.GoToState(this, IsCollapsed ? "Collapsed" : "Expanded", useTransitions);
            var bottomRow = GetTemplateChild("BottomRow") as RowDefinition;
            if(bottomRow is not null)
            {
                if (IsCollapsed)
                    bottomRow.Height = new GridLength(1, GridUnitType.Auto);
                else
                    OnStatePropertyChanged();
            }
        }

        public double HeightRatio
        {
            get { return (double)GetValue(HeightRatioProperty); }
            set { SetValue(HeightRatioProperty, value); }
        }

        public static readonly DependencyProperty HeightRatioProperty =
            DependencyProperty.Register(nameof(HeightRatio), typeof(double), typeof(WindowPanel), new PropertyMetadata(.5d, (s, e) => ((WindowPanel)s).OnStatePropertyChanged()));

        private void OnStatePropertyChanged(bool useTransitions = true)
        {
            var topRow = GetTemplateChild("TopRow") as RowDefinition;
            var bottomRow = GetTemplateChild("BottomRow") as RowDefinition;
            var ratio = Math.Min(1, Math.Max(0, HeightRatio));
            if (topRow is not null)
                topRow.Height = new GridLength((1 - HeightRatio)*100, GridUnitType.Star);
            if (bottomRow is not null)
                bottomRow.Height = new GridLength(HeightRatio*100, GridUnitType.Star);
        }
    }
}
