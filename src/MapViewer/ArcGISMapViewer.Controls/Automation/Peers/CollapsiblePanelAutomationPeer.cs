using System;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;

namespace ArcGISMapViewer.Controls.Automation.Peers;

public sealed partial class CollapsiblePanelAutomationPeer : AutomationPeer, ISelectionProvider, IExpandCollapseProvider
{
    private CollapsiblePanel _panel;

    public CollapsiblePanelAutomationPeer(CollapsiblePanel panel)
    {
        _panel = panel;
    }

    protected override object GetPatternCore(PatternInterface patternInterface)
    {
        if (patternInterface == PatternInterface.Selection)
            return this;
        if (patternInterface == PatternInterface.ExpandCollapse)
            return this;
        return base.GetPatternCore(patternInterface);
    }

    #region ISelectionProvider
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
    #endregion ISelectionProvider

    #region IExpandCollapseProvider
    public void Collapse() => _panel.IsPaneExpanded = false;

    public void Expand() => _panel.IsPaneExpanded = true;

    public ExpandCollapseState ExpandCollapseState => this._panel.IsPaneExpanded ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed;
    #endregion IExpandCollapseProvider
}