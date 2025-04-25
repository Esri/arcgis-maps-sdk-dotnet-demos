using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace ArcGISMapViewer.Controls.Automation.Peers;

public partial class CollapsiblePanelItemAutomationPeer : AutomationPeer, Microsoft.UI.Xaml.Automation.Provider.ISelectionItemProvider
{
    readonly CollapsiblePanelItem _item;

    public CollapsiblePanelItemAutomationPeer(CollapsiblePanelItem item)
    {
        _item = item;
    }

    public bool IsSelected => _item.IsSelected;

    public IRawElementProviderSimple? SelectionContainer
    {
        get
        {
            var panel = GetParentCollapsiblePanel();
            if (panel != null && FrameworkElementAutomationPeer.CreatePeerForElement(panel) is CollapsiblePanelAutomationPeer panelPeer)
            {
                return ProviderFromPeer(panelPeer);
            }
            return null;
        }
    }

    public void AddToSelection()
    {
        _item.IsSelected = true;
    }

    public void RemoveFromSelection()
    {
        _item.IsSelected = false;
    }

    public void Select()
    {
        _item.IsSelected = true;
    }

    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.ListItem;

    protected override string GetNameCore() => _item.Name;

    protected override object GetPatternCore(PatternInterface patternInterface)
    {
        if (patternInterface == PatternInterface.SelectionItem)
            return this;
        return base.GetPatternCore(patternInterface);
    }

    protected override int GetPositionInSetCore() => GetParentCollapsiblePanel()?.Items.IndexOf(_item) ?? 0;

    protected override int GetSizeOfSetCore() => GetParentCollapsiblePanel()?.Items.Count ?? 0;

    protected override int GetLevelCore() => 1;

    protected override string GetClassNameCore() => nameof(CollapsiblePanelItem);

    private CollapsiblePanel? GetParentCollapsiblePanel() => _item.GetCollapsiblePanel();
}