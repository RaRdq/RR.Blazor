using Microsoft.AspNetCore.Components;
using RR.Blazor.Enums;
using System.Collections.ObjectModel;

namespace RR.Blazor.Models;

/// <summary>
/// Base interface for choice items
/// </summary>
public interface IChoiceItem
{
    /// <summary>
    /// Unique identifier for the item
    /// </summary>
    string Id { get; set; }
    
    /// <summary>
    /// Display text for the item
    /// </summary>
    string Label { get; set; }
    
    /// <summary>
    /// Value associated with the item
    /// </summary>
    object Value { get; set; }
    
    /// <summary>
    /// Icon for the item
    /// </summary>
    string Icon { get; set; }
    
    /// <summary>
    /// Whether the item is disabled
    /// </summary>
    bool Disabled { get; set; }
    
    /// <summary>
    /// Whether the item is loading
    /// </summary>
    bool Loading { get; set; }
    
    /// <summary>
    /// Custom CSS classes for the item
    /// </summary>
    string Class { get; set; }
    
    /// <summary>
    /// Tooltip or help text
    /// </summary>
    string Tooltip { get; set; }
    
    /// <summary>
    /// Secondary description text
    /// </summary>
    string Description { get; set; }
    
    /// <summary>
    /// Avatar image URL for the item
    /// </summary>
    string AvatarUrl { get; set; }
    
    /// <summary>
    /// Badge text to display alongside the item
    /// </summary>
    string Badge { get; set; }
    
    /// <summary>
    /// Custom data attributes
    /// </summary>
    Dictionary<string, object> Data { get; set; }
}

/// <summary>
/// Standard choice item implementation
/// </summary>
public class ChoiceItem : IChoiceItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Label { get; set; } = "";
    public object Value { get; set; }
    public string Icon { get; set; } = "";
    public bool Disabled { get; set; }
    public bool Loading { get; set; }
    public string Class { get; set; } = "";
    public string Tooltip { get; set; } = "";
    public Dictionary<string, object> Data { get; set; } = new();
    
    /// <summary>
    /// Badge text to display alongside the item
    /// </summary>
    public string Badge { get; set; } = "";
    
    /// <summary>
    /// Badge variant for styling
    /// </summary>
    public VariantType BadgeVariant { get; set; } = VariantType.Secondary;
    
    /// <summary>
    /// Secondary description text
    /// </summary>
    public string Description { get; set; } = "";
    
    /// <summary>
    /// Avatar image URL for the item
    /// </summary>
    public string AvatarUrl { get; set; } = "";
    
    /// <summary>
    /// Custom template for rendering the item
    /// </summary>
    public RenderFragment<ChoiceItem> Template { get; set; }
}

public class ChoiceItem<T> : ChoiceItem
{
    public ChoiceItem()
    {
    }

    public ChoiceItem(string label, T value)
    {
        Label = label;
        Value = value!;
    }

    public new T Value
    {
        get => base.Value is T typed ? typed : default!;
        set => base.Value = value!;
    }
}

/// <summary>
/// Interface for choice groups
/// </summary>
public interface IChoiceGroup : IChoiceItem
{
    /// <summary>
    /// Items within this group
    /// </summary>
    IEnumerable<IChoiceItem> Items { get; set; }
    
    /// <summary>
    /// Whether the group is expanded
    /// </summary>
    bool IsExpanded { get; set; }
    
    /// <summary>
    /// Whether the group can be collapsed
    /// </summary>
    bool Collapsible { get; set; }
    
    /// <summary>
    /// Count text to display in the group header
    /// </summary>
    string Count { get; set; }
    
    /// <summary>
    /// Whether selecting the group selects all items
    /// </summary>
    bool SelectAll { get; set; }
    
    /// <summary>
    /// Custom template for rendering the group header
    /// </summary>
    RenderFragment<IChoiceGroup> HeaderTemplate { get; set; }
}

/// <summary>
/// Choice group implementation with hierarchical support
/// </summary>
public class ChoiceGroup : ChoiceItem, IChoiceGroup
{
    private ObservableCollection<IChoiceItem> _items = new();
    
    public IEnumerable<IChoiceItem> Items 
    { 
        get => _items; 
        set 
        {
            _items.Clear();
            if (value != null)
            {
                foreach (var item in value)
                {
                    _items.Add(item);
                    if (item is ChoiceTreeItem treeItem)
                    {
                        treeItem.Parent = this;
                    }
                }
            }
        }
    }
    
    public bool IsExpanded { get; set; } = true;
    public bool Collapsible { get; set; } = true;
    public string Count { get; set; } = "";
    public bool SelectAll { get; set; } = false;
    public RenderFragment<IChoiceGroup> HeaderTemplate { get; set; }
    
    /// <summary>
    /// Group styling variant
    /// </summary>
    public VariantType Variant { get; set; } = VariantType.Secondary;
    
    /// <summary>
    /// Group density for spacing
    /// </summary>
    public DensityType Density { get; set; } = DensityType.Normal;
    
    /// <summary>
    /// Add an item to the group
    /// </summary>
    public void AddItem(IChoiceItem item)
    {
        _items.Add(item);
        if (item is ChoiceTreeItem treeItem)
        {
            treeItem.Parent = this;
        }
        UpdateCount();
    }
    
    /// <summary>
    /// Remove an item from the group
    /// </summary>
    public bool RemoveItem(IChoiceItem item)
    {
        var removed = _items.Remove(item);
        if (removed && item is ChoiceTreeItem treeItem)
        {
            treeItem.Parent = null;
        }
        UpdateCount();
        return removed;
    }
    
    /// <summary>
    /// Get all items recursively (including nested groups)
    /// </summary>
    public IEnumerable<IChoiceItem> GetAllItems()
    {
        foreach (var item in Items)
        {
            yield return item;
            if (item is ChoiceGroup group)
            {
                foreach (var nestedItem in group.GetAllItems())
                {
                    yield return nestedItem;
                }
            }
        }
    }
    
    /// <summary>
    /// Update the count display based on items
    /// </summary>
    private void UpdateCount()
    {
        if (string.IsNullOrEmpty(Count))
        {
            var totalCount = GetAllItems().Count(i => !(i is IChoiceGroup));
            Count = totalCount > 0 ? $"({totalCount})" : "";
        }
    }
}

/// <summary>
/// Interface for tree choice items with parent-child relationships
/// </summary>
public interface IChoiceTreeItem : IChoiceItem
{
    /// <summary>
    /// Parent item in the tree
    /// </summary>
    IChoiceItem Parent { get; set; }
    
    /// <summary>
    /// Child items in the tree
    /// </summary>
    IEnumerable<IChoiceTreeItem> Children { get; set; }
    
    /// <summary>
    /// Whether the tree node is expanded
    /// </summary>
    bool IsExpanded { get; set; }
    
    /// <summary>
    /// Whether the item is selected
    /// </summary>
    bool IsSelected { get; set; }
    
    /// <summary>
    /// Whether the item is partially selected (some children selected)
    /// </summary>
    bool IsPartiallySelected { get; set; }
    
    /// <summary>
    /// Tree depth level (0 = root)
    /// </summary>
    int Level { get; set; }
    
    /// <summary>
    /// Whether the item has children
    /// </summary>
    bool HasChildren { get; }
    
    /// <summary>
    /// Whether the item is a leaf node
    /// </summary>
    bool IsLeaf { get; }
    
    /// <summary>
    /// Custom expand icon
    /// </summary>
    string ExpandIcon { get; set; }
    
    /// <summary>
    /// Custom collapse icon
    /// </summary>
    string CollapseIcon { get; set; }
}

/// <summary>
/// Tree choice item implementation with cascade selection support
/// </summary>
public class ChoiceTreeItem : ChoiceItem, IChoiceTreeItem
{
    private ObservableCollection<IChoiceTreeItem> _children = new();
    private bool _isSelected;
    
    public IChoiceItem Parent { get; set; }
    
    public IEnumerable<IChoiceTreeItem> Children
    {
        get => _children;
        set
        {
            _children.Clear();
            if (value != null)
            {
                foreach (var child in value)
                {
                    _children.Add(child);
                    child.Parent = this;
                    child.Level = Level + 1;
                }
            }
            UpdateSelectionState();
        }
    }
    
    public bool IsExpanded { get; set; } = false;
    
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnSelectionChanged();
            }
        }
    }
    
    public bool IsPartiallySelected { get; set; }
    public int Level { get; set; }
    
    public bool HasChildren => _children.Any();
    public bool IsLeaf => !HasChildren;
    
    /// <summary>
    /// Whether to enable cascade selection (selecting parent selects all children)
    /// </summary>
    public bool CascadeSelection { get; set; } = true;
    
    /// <summary>
    /// Custom expand/collapse icon
    /// </summary>
    public string ExpandIcon { get; set; } = "expand_more";
    
    /// <summary>
    /// Custom collapse icon
    /// </summary>
    public string CollapseIcon { get; set; } = "expand_less";
    
    /// <summary>
    /// Event triggered when selection changes
    /// </summary>
    public event Action<IChoiceTreeItem> SelectionChanged;
    
    /// <summary>
    /// Add a child item to the tree
    /// </summary>
    public void AddChild(IChoiceTreeItem child)
    {
        _children.Add(child);
        child.Parent = this;
        child.Level = Level + 1;
        UpdateSelectionState();
    }
    
    /// <summary>
    /// Remove a child item from the tree
    /// </summary>
    public bool RemoveChild(IChoiceTreeItem child)
    {
        var removed = _children.Remove(child);
        if (removed)
        {
            child.Parent = null;
            UpdateSelectionState();
        }
        return removed;
    }
    
    /// <summary>
    /// Get all descendant items
    /// </summary>
    public IEnumerable<IChoiceTreeItem> GetAllDescendants()
    {
        foreach (var child in Children)
        {
            yield return child;
            if (child is ChoiceTreeItem childTree)
            {
                foreach (var descendant in childTree.GetAllDescendants())
                {
                    yield return descendant;
                }
            }
        }
    }
    
    /// <summary>
    /// Get all ancestor items up to root
    /// </summary>
    public IEnumerable<IChoiceItem> GetAncestors()
    {
        var current = Parent;
        while (current != null)
        {
            yield return current;
            current = current is IChoiceTreeItem treeParent ? treeParent.Parent : null;
        }
    }
    
    /// <summary>
    /// Toggle expanded state
    /// </summary>
    public void ToggleExpanded()
    {
        IsExpanded = !IsExpanded;
    }
    
    /// <summary>
    /// Select this item and optionally cascade to children
    /// </summary>
    public void Select(bool cascade = true)
    {
        IsSelected = true;
        if (cascade && CascadeSelection)
        {
            foreach (var child in Children)
            {
                if (child is ChoiceTreeItem childTree)
                {
                    childTree.Select(true);
                }
            }
        }
    }
    
    /// <summary>
    /// Deselect this item and optionally cascade to children
    /// </summary>
    public void Deselect(bool cascade = true)
    {
        IsSelected = false;
        if (cascade && CascadeSelection)
        {
            foreach (var child in Children)
            {
                if (child is ChoiceTreeItem childTree)
                {
                    childTree.Deselect(true);
                }
            }
        }
    }
    
    /// <summary>
    /// Update selection state based on children
    /// </summary>
    private void UpdateSelectionState()
    {
        if (!HasChildren) return;
        
        var selectedChildren = Children.Count(c => c.IsSelected);
        var totalChildren = Children.Count();
        
        if (selectedChildren == totalChildren)
        {
            IsPartiallySelected = false;
            if (!_isSelected)
            {
                _isSelected = true;
                OnSelectionChanged();
            }
        }
        else if (selectedChildren > 0)
        {
            IsPartiallySelected = true;
            if (_isSelected)
            {
                _isSelected = false;
                OnSelectionChanged();
            }
        }
        else
        {
            IsPartiallySelected = false;
            if (_isSelected)
            {
                _isSelected = false;
                OnSelectionChanged();
            }
        }
        
        // Update parent selection state
        if (Parent is ChoiceTreeItem parentTree)
        {
            parentTree.UpdateSelectionState();
        }
    }
    
    /// <summary>
    /// Handle selection change events
    /// </summary>
    private void OnSelectionChanged()
    {
        SelectionChanged?.Invoke(this);
        
        // Update children if cascade selection is enabled
        if (CascadeSelection)
        {
            foreach (var child in Children)
            {
                child.IsSelected = _isSelected;
            }
        }
        
        // Update parent selection state
        if (Parent is ChoiceTreeItem parentTree)
        {
            parentTree.UpdateSelectionState();
        }
    }
}

/// <summary>
/// Factory class for creating choice models
/// </summary>
public static class ChoiceFactory
{
    /// <summary>
    /// Create a simple choice item
    /// </summary>
    public static ChoiceItem CreateItem(string label, object value, string icon = "", bool disabled = false)
    {
        return new ChoiceItem
        {
            Label = label,
            Value = value,
            Icon = icon,
            Disabled = disabled
        };
    }

    public static ChoiceItem<T> CreateItem<T>(string label, T value, string icon = "", bool disabled = false)
    {
        return new ChoiceItem<T>
        {
            Label = label,
            Value = value,
            Icon = icon,
            Disabled = disabled
        };
    }
    
    /// <summary>
    /// Create a choice group
    /// </summary>
    public static ChoiceGroup CreateGroup(string label, IEnumerable<IChoiceItem> items = null, string icon = "", bool expanded = true)
    {
        return new ChoiceGroup
        {
            Label = label,
            Icon = icon,
            IsExpanded = expanded,
            Items = items ?? Enumerable.Empty<IChoiceItem>()
        };
    }
    
    /// <summary>
    /// Create a tree choice item
    /// </summary>
    public static ChoiceTreeItem CreateTreeItem(string label, object value, IEnumerable<IChoiceTreeItem> children = null, string icon = "", bool expanded = false)
    {
        return new ChoiceTreeItem
        {
            Label = label,
            Value = value,
            Icon = icon,
            IsExpanded = expanded,
            Children = children ?? Enumerable.Empty<IChoiceTreeItem>()
        };
    }
    
    /// <summary>
    /// Create a hierarchical tree from flat data with parent-child relationships
    /// </summary>
    public static IEnumerable<IChoiceTreeItem> CreateTreeFromFlat<T>(
        IEnumerable<T> items,
        Func<T, object> idSelector,
        Func<T, object> parentIdSelector,
        Func<T, string> labelSelector,
        Func<T, object> valueSelector,
        Func<T, string> iconSelector = null)
    {
        var itemMap = new Dictionary<object, ChoiceTreeItem>();
        var rootItems = new List<IChoiceTreeItem>();
        
        // First pass: create all items
        foreach (var item in items)
        {
            var id = idSelector(item);
            var treeItem = new ChoiceTreeItem
            {
                Id = id?.ToString() ?? Guid.NewGuid().ToString(),
                Label = labelSelector(item),
                Value = valueSelector(item),
                Icon = iconSelector?.Invoke(item) ?? ""
            };
            itemMap[id] = treeItem;
        }
        
        // Second pass: build hierarchy
        foreach (var item in items)
        {
            var id = idSelector(item);
            var parentId = parentIdSelector(item);
            var currentItem = itemMap[id];
            
            if (parentId != null && itemMap.TryGetValue(parentId, out var parentItem))
            {
                parentItem.AddChild(currentItem);
            }
            else
            {
                // Root item
                rootItems.Add(currentItem);
            }
        }
        
        return rootItems;
    }
}
