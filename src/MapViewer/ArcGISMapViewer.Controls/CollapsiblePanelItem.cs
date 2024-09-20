using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace ArcGISMapViewer.Controls
{

    [Microsoft.UI.Xaml.Markup.ContentProperty(Name = nameof(Content))]
    public partial class CollapsiblePanelItem : Control
    {
        // Visual States
        const string c_pressedSelected = "PressedSelected";
        const string c_pointerOverSelected = "PointerOverSelected";
        const string c_selected = "Selected";
        const string c_pressed = "Pressed";
        const string c_pointerOver = "PointerOver";
        const string c_disabled = "Disabled";
        const string c_enabled = "Enabled";
        const string c_normal = "Normal";

        public CollapsiblePanelItem()
        {
            this.DefaultStyleKey = typeof(CollapsiblePanelItem);
            IsEnabledChanged += (s, e) => OnIsEnabledChanged(true);
        }
        protected override void OnPointerEntered(PointerRoutedEventArgs e)
        {
            base.OnPointerEntered(e);
            m_isPointerOver = true;
            UpdateVisualStateForPointer(true);
        }
        protected override void OnPointerExited(PointerRoutedEventArgs e)
        {
            base.OnPointerExited(e);
            m_isPointerOver = false;
            UpdateVisualStateForPointer(true);
        }
        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            base.OnPointerPressed(e);
            m_isPressed = true;
            UpdateVisualStateForPointer(true);
        }
        protected override void OnPointerReleased(PointerRoutedEventArgs e)
        {
            base.OnPointerReleased(e);
            m_isPressed = false;
            UpdateVisualStateForPointer(true);
        }
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            OnIsExpandedPropertyChanged(false);
            OnIsEnabledChanged(false);
        }
        bool m_isPressed;
        bool m_isPointerOver;
        private void UpdateVisualStateForPointer(bool useTransitions)
        {
            var isEnabled = IsEnabled;
            var enabledStateValue = isEnabled ? c_enabled : c_disabled;
            // DisabledStates and CommonStates
            bool isSelected = IsSelected;
            Func<string> selectedStateValue = () =>
            {
                if (isEnabled)
                {
                    if (isSelected)
                    {
                        if (m_isPressed)
                            return c_pressedSelected;
                        else if (m_isPointerOver)
                            return c_pointerOverSelected;
                        else
                            return c_selected;
                    }
                    else if (m_isPointerOver)
                    {
                        if (m_isPressed)
                            return c_pressed;
                        else
                            return c_pointerOver;
                    }
                    else if (m_isPressed)
                        return c_pressed;
                }
                else
                {
                    if (isSelected)
                        return c_selected;
                }
                return c_normal;
            };
            VisualStateManager.GoToState(this, selectedStateValue(), useTransitions);
            VisualStateManager.GoToState(this, IsEnabled ? c_enabled : c_disabled, useTransitions);
        }


        private void OnIsEnabledChanged(bool useTransitions)
        {
            UpdateVisualStateForPointer(useTransitions);
        }

        public string? Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(CollapsiblePanelItem), new PropertyMetadata(""));


        public IconElement? Icon
        {
            get { return (IconElement)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(nameof(Icon), typeof(IconElement), typeof(CollapsiblePanelItem), new PropertyMetadata(null));

        public object? Content
        {
            get { return (object)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register(nameof(Content), typeof(object), typeof(CollapsiblePanelItem), new PropertyMetadata(null));

        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register(nameof(IsExpanded), typeof(bool), typeof(CollapsiblePanelItem), new PropertyMetadata(false, (s, e) => ((CollapsiblePanelItem)s).OnIsExpandedPropertyChanged(true)));

        private void OnIsExpandedPropertyChanged(bool useTransitions)
        {
            VisualStateManager.GoToState(this, IsExpanded ? "Expanded" : "Collapsed", useTransitions);
        }


        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(CollapsiblePanelItem), new PropertyMetadata(false, (s, e) => ((CollapsiblePanelItem)s).OnIsSelectedPropertyChanged(true)));

        private void OnIsSelectedPropertyChanged(bool useTransitions)
        {
            UpdateVisualStateForPointer(useTransitions);
        }

        public string ContentTitle
        {
            get { return (string)GetValue(ContentTitleProperty); }
            set { SetValue(ContentTitleProperty, value); }
        }

        public static readonly DependencyProperty ContentTitleProperty =
            DependencyProperty.Register("ContentTitle", typeof(string), typeof(CollapsiblePanelItem), new PropertyMetadata(string.Empty));
    }
}
