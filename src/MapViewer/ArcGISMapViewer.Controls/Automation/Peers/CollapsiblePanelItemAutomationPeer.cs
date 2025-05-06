using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace ArcGISMapViewer.Controls.Automation.Peers;

public partial class CollapsiblePanelItemAutomationPeer : FrameworkElementAutomationPeer, Microsoft.UI.Xaml.Automation.Provider.ISelectionItemProvider
{
    readonly CollapsiblePanelItem _item;

    public CollapsiblePanelItemAutomationPeer(CollapsiblePanelItem item) : base(item)
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

    protected override string GetNameCore()
    {
        var name = AutomationProperties.GetName(_item);
        if (string.IsNullOrEmpty(name))
            name = _item.Title;
        if (string.IsNullOrEmpty(name))
            name = base.GetNameCore();
        return name;
    }

    protected override object GetPatternCore(PatternInterface patternInterface)
    {
        if (patternInterface == PatternInterface.SelectionItem)
            return this;
        return base.GetPatternCore(patternInterface);
    }

    protected override int GetPositionInSetCore()
    {
        var parent = GetParentCollapsiblePanel();
        var idx = parent?.Items.IndexOf(_item) ?? -1;
        if (idx > -1) return idx + 1;
        idx = parent?.FooterItems.IndexOf(_item) ?? -1;
        if(idx > -1) return idx + (parent?.Items.Count ?? 0) + 1;
        return 0;
    }

    protected override int GetSizeOfSetCore()
    {
        var parent = GetParentCollapsiblePanel();
        return (parent?.Items.Count ?? 0) + (parent?.FooterItems.Count ?? 0);
    }

    protected override int GetLevelCore() => 1;

    protected override string GetClassNameCore() => nameof(CollapsiblePanelItem);

    private CollapsiblePanel? GetParentCollapsiblePanel() => _item.GetCollapsiblePanel();
}