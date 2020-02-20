using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace MobileStylePicker
{
    public class DropDownButton : Button
    {
        public DropDownButton()
        {
            DefaultStyleKey = typeof(Button);
        }

        protected override void OnClick()
        {
            if(this.ContextMenu != null)
            {
                ContextMenu.IsEnabled = true;
                ContextMenu.PlacementTarget = this;
                ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                ContextMenu.IsOpen = true;
            }
            base.OnClick();
        }
    }
}
