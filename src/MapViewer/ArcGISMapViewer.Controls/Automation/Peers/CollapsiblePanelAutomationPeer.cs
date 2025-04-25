using System;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;

namespace ArcGISMapViewer.Controls.Automation.Peers;

public sealed partial class CollapsiblePanelAutomationPeer : AutomationPeer, Microsoft.UI.Xaml.Automation.Provider.ISelectionProvider
{
    CollapsiblePanel _panel;
    public CollapsiblePanelAutomationPeer(CollapsiblePanel panel)
    {
        _panel = panel;
        _panel.SelectionChanged += (s, e) => RaiseSelectionChangedEvent(); //TODO: Make weak
    }

    private void RaiseSelectionChangedEvent()
    {
        RaiseAutomationEvent(AutomationEvents.SelectionItemPatternOnElementSelected);
    }

    public bool CanSelectMultiple => false;

    public bool IsSelectionRequired => true;

    public IRawElementProviderSimple[] GetSelection()
    {
        if (_panel.SelectedItem != null)
        {
            var peer = ProviderFromPeer(new CollapsiblePanelItemAutomationPeer(_panel.SelectedItem));
            if (peer != null)
            {
                return [peer];
            }
        }
        return Array.Empty<IRawElementProviderSimple>();
    }

    protected override object GetPatternCore(PatternInterface patternInterface)
    {
        if (patternInterface == PatternInterface.Selection)
            return this;
        return base.GetPatternCore(patternInterface);
    }
}