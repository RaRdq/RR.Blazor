using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using RR.Blazor.Models;

namespace RR.Blazor.Components.Data;

/// <summary>
/// Smart table component with automatic type detection and intelligent defaults.
/// Supports any domain (CRM, ERP, eCommerce, etc.) with context-aware behavior.
/// </summary>
public abstract class RTableBase : ComponentBase
{
    #region Core Configuration
    [Parameter] public string Class { get; set; } = "";
    [Parameter] public string AdditionalClass { get; set; } = "";
    [Parameter] public bool Loading { get; set; }
    [Parameter] public string Title { get; set; } = "";
    [Parameter] public string Subtitle { get; set; } = "";
    [Parameter] public string? StartIcon { get; set; }
    [Parameter] public string? EndIcon { get; set; }
    [Parameter] public RenderFragment HeaderContent { get; set; }
    #endregion

    #region Smart Features
    [Parameter] public bool BulkOperationsEnabled { get; set; }
    [Parameter] public bool ExportEnabled { get; set; }
    [Parameter] public RenderFragment BulkOperations { get; set; }
    #endregion

    #region Pagination
    [Parameter] public bool ShowPagination { get; set; } = true;
    [Parameter] public int CurrentPage { get; set; } = 1;
    [Parameter] public int PageSize { get; set; } = 25;
    [Parameter] public int TotalItems { get; set; }
    [Parameter] public EventCallback<int> OnPageChange { get; set; }
    [Parameter] public List<int> PageSizeOptions { get; set; } = new() { 10, 25, 50, 100, 250 };
    #endregion

    #region Selection
    [Parameter] public bool MultiSelection { get; set; }
    [Parameter] public bool SingleSelection { get; set; }
    [Parameter] public EventCallback<object> SelectedItemsChanged { get; set; }
    [Parameter] public EventCallback<object> SelectedItemChanged { get; set; }
    [Parameter] public object SelectedItemsChangedTyped { get; set; }
    [Parameter] public object SelectedItemChangedTyped { get; set; }
    #endregion

    #region Interaction
    [Parameter] public bool RowClickable { get; set; }
    [Parameter] public EventCallback<object> OnRowClick { get; set; }
    [Parameter] public object OnRowClickTyped { get; set; }
    [Parameter] public Func<object, string> RowClassFunc { get; set; }
    [Parameter] public object RowClassFuncTyped { get; set; }
    #endregion

    #region Sorting & Filtering
    [Parameter] public string SortColumn { get; set; } = "";
    [Parameter] public bool SortDescending { get; set; }
    [Parameter] public EventCallback<string> OnSort { get; set; }
    [Parameter] public List<TableSortState> SortStates { get; set; } = new();
    [Parameter] public EventCallback<List<TableSortState>> SortStatesChanged { get; set; }
    [Parameter] public EventCallback<SortEventArgs> OnSortChanged { get; set; }
    [Parameter] public bool MultiColumnSort { get; set; } = true;
    [Parameter] public int MaxSortLevels { get; set; } = 3;
    [Parameter] public Dictionary<string, string> ColumnFilters { get; set; } = new();
    [Parameter] public EventCallback<Dictionary<string, string>> ColumnFiltersChanged { get; set; }
    [Parameter] public EventCallback<ColumnFilterEventArgs> OnColumnFilter { get; set; }
    #endregion

    #region Advanced Features
    [Parameter] public string Height { get; set; } = "";
    [Parameter] public bool Virtualize { get; set; }
    [Parameter] public bool ResizableColumns { get; set; }
    [Parameter] public List<string> StickyColumns { get; set; } = new();
    [Parameter] public Enums.ComponentDensity Density { get; set; } = Enums.ComponentDensity.Normal;
    [Parameter] public bool Striped { get; set; }
    [Parameter] public bool Hover { get; set; } = true;
    [Parameter] public bool Sortable { get; set; } = true;
    [Parameter] public bool AllowUnsorted { get; set; } = true;
    #endregion

    #region Professional Styling Parameters  
    [Parameter] public Enums.TableVariant TableVariant { get; set; } = Enums.TableVariant.Standard;
    [Parameter] public Enums.TableDensity TableDensity { get; set; } = Enums.TableDensity.Normal;
    [Parameter] public int CustomRowHeight { get; set; } = 48;
    [Parameter] public int TableElevation { get; set; } = 2;
    [Parameter] public bool EnableGlassmorphism { get; set; }
    [Parameter] public bool ShowBorders { get; set; } = true;
    [Parameter] public bool EnableHoverEffects { get; set; } = true;
    [Parameter] public bool EnableAnimations { get; set; } = true;
    [Parameter] public string TableTheme { get; set; } = "auto";
    [Parameter] public Dictionary<string, string> CustomCssVariables { get; set; } = new();
    #endregion

    #region Column Management
    [Parameter] public bool ShowColumnManager { get; set; } = true;
    [Parameter] public Dictionary<string, ColumnPreferences> ColumnPreferences { get; set; } = new();
    [Parameter] public EventCallback<Dictionary<string, ColumnPreferences>> ColumnPreferencesChanged { get; set; }
    [Parameter] public EventCallback<ColumnManagementEventArgs> OnColumnManagement { get; set; }
    [Parameter] public EventCallback<ColumnResizeEventArgs> OnColumnResize { get; set; }
    [Parameter] public bool EnableColumnReordering { get; set; }
    [Parameter] public bool PersistColumnPreferences { get; set; } = true;
    [Parameter] public string TableId { get; set; } = "";
    #endregion

    #region Content Areas
    [Parameter] public RenderFragment ColumnsContent { get; set; }
    [Parameter] public string EmptyMessage { get; set; }
    [Parameter] public string LoadingMessage { get; set; }
    [Parameter] public RenderFragment FooterContent { get; set; }
    #endregion

    #region Export Configuration
    [Parameter] public string ExportFormats { get; set; } = "csv,excel,json";
    [Parameter] public string ExportFileName { get; set; } = "";
    [Parameter] public Dictionary<string, string> ExportMetadata { get; set; } = new();
    #endregion

    protected void ForwardBaseParameters(RenderTreeBuilder builder)
    {
        builder.AddAttribute(1, nameof(AdditionalClass), AdditionalClass);
        builder.AddAttribute(2, nameof(Loading), Loading);
        builder.AddAttribute(3, nameof(Title), Title);
        builder.AddAttribute(4, nameof(Subtitle), Subtitle);
        builder.AddAttribute(5, nameof(StartIcon), StartIcon);
        builder.AddAttribute(6, nameof(EndIcon), EndIcon);
        builder.AddAttribute(7, nameof(HeaderContent), HeaderContent);
        
        builder.AddAttribute(8, nameof(BulkOperationsEnabled), BulkOperationsEnabled);
        builder.AddAttribute(9, nameof(ExportEnabled), ExportEnabled);
        builder.AddAttribute(10, nameof(BulkOperations), BulkOperations);
        
        builder.AddAttribute(11, nameof(ShowPagination), ShowPagination);
        builder.AddAttribute(12, nameof(CurrentPage), CurrentPage);
        builder.AddAttribute(13, nameof(PageSize), PageSize);
        builder.AddAttribute(14, nameof(TotalItems), TotalItems);
        builder.AddAttribute(15, nameof(OnPageChange), OnPageChange);
        builder.AddAttribute(16, nameof(PageSizeOptions), PageSizeOptions);
        
        builder.AddAttribute(17, nameof(MultiSelection), MultiSelection);
        builder.AddAttribute(18, nameof(SingleSelection), SingleSelection);
        builder.AddAttribute(19, nameof(SelectedItemsChanged), SelectedItemsChanged);
        builder.AddAttribute(20, nameof(SelectedItemChanged), SelectedItemChanged);
        
        builder.AddAttribute(21, nameof(RowClickable), RowClickable);
        builder.AddAttribute(22, nameof(OnRowClick), OnRowClick);
        builder.AddAttribute(23, nameof(RowClassFunc), RowClassFunc);
        
        builder.AddAttribute(28, nameof(SortColumn), SortColumn);
        builder.AddAttribute(29, nameof(SortDescending), SortDescending);
        builder.AddAttribute(30, nameof(OnSort), OnSort);
        builder.AddAttribute(31, nameof(SortStates), SortStates);
        builder.AddAttribute(32, nameof(SortStatesChanged), SortStatesChanged);
        builder.AddAttribute(33, nameof(OnSortChanged), OnSortChanged);
        builder.AddAttribute(34, nameof(MultiColumnSort), MultiColumnSort);
        builder.AddAttribute(35, nameof(MaxSortLevels), MaxSortLevels);
        builder.AddAttribute(36, nameof(ColumnFilters), ColumnFilters);
        builder.AddAttribute(37, nameof(ColumnFiltersChanged), ColumnFiltersChanged);
        builder.AddAttribute(38, nameof(OnColumnFilter), OnColumnFilter);
        
        builder.AddAttribute(39, nameof(Height), Height);
        builder.AddAttribute(40, nameof(Virtualize), Virtualize);
        builder.AddAttribute(41, nameof(ResizableColumns), ResizableColumns);
        builder.AddAttribute(42, nameof(StickyColumns), StickyColumns);
        
        builder.AddAttribute(43, nameof(ColumnsContent), ColumnsContent);
        builder.AddAttribute(44, nameof(EmptyMessage), EmptyMessage);
        builder.AddAttribute(45, nameof(LoadingMessage), LoadingMessage);
        builder.AddAttribute(46, nameof(FooterContent), FooterContent);
        
        builder.AddAttribute(47, nameof(ExportFormats), ExportFormats);
        builder.AddAttribute(48, nameof(ExportFileName), ExportFileName);
        builder.AddAttribute(49, nameof(ExportMetadata), ExportMetadata);
        builder.AddAttribute(50, nameof(Density), Density);
        builder.AddAttribute(51, nameof(Striped), Striped);
        builder.AddAttribute(52, nameof(Hover), Hover);
        builder.AddAttribute(53, nameof(Sortable), Sortable);
        builder.AddAttribute(54, nameof(AllowUnsorted), AllowUnsorted);
        
        // Column management parameters
        builder.AddAttribute(55, nameof(ShowColumnManager), ShowColumnManager);
        builder.AddAttribute(56, nameof(ColumnPreferences), ColumnPreferences);
        builder.AddAttribute(57, nameof(ColumnPreferencesChanged), ColumnPreferencesChanged);
        builder.AddAttribute(58, nameof(OnColumnManagement), OnColumnManagement);
        builder.AddAttribute(59, nameof(OnColumnResize), OnColumnResize);
        builder.AddAttribute(60, nameof(EnableColumnReordering), EnableColumnReordering);
        builder.AddAttribute(61, nameof(PersistColumnPreferences), PersistColumnPreferences);
        builder.AddAttribute(62, nameof(TableId), TableId);
        
        // Professional styling parameters
        builder.AddAttribute(63, nameof(TableVariant), TableVariant);
        builder.AddAttribute(64, nameof(TableDensity), TableDensity);
        builder.AddAttribute(65, nameof(CustomRowHeight), CustomRowHeight);
        builder.AddAttribute(66, nameof(TableElevation), TableElevation);
        builder.AddAttribute(67, nameof(EnableGlassmorphism), EnableGlassmorphism);
        builder.AddAttribute(68, nameof(ShowBorders), ShowBorders);
        builder.AddAttribute(69, nameof(EnableHoverEffects), EnableHoverEffects);
        builder.AddAttribute(70, nameof(EnableAnimations), EnableAnimations);
        builder.AddAttribute(71, nameof(TableTheme), TableTheme);
        builder.AddAttribute(72, nameof(CustomCssVariables), CustomCssVariables);
    }

    protected void ForwardBaseParametersExceptChildContent(RenderTreeBuilder builder)
    {
        builder.AddAttribute(1, nameof(AdditionalClass), AdditionalClass);
        builder.AddAttribute(2, nameof(Loading), Loading);
        builder.AddAttribute(3, nameof(Title), Title);
        builder.AddAttribute(4, nameof(Subtitle), Subtitle);
        builder.AddAttribute(5, nameof(StartIcon), StartIcon);
        builder.AddAttribute(6, nameof(EndIcon), EndIcon);
        builder.AddAttribute(7, nameof(HeaderContent), HeaderContent);
        
        builder.AddAttribute(8, nameof(BulkOperationsEnabled), BulkOperationsEnabled);
        builder.AddAttribute(9, nameof(ExportEnabled), ExportEnabled);
        builder.AddAttribute(10, nameof(BulkOperations), BulkOperations);
        
        builder.AddAttribute(11, nameof(ShowPagination), ShowPagination);
        builder.AddAttribute(12, nameof(CurrentPage), CurrentPage);
        builder.AddAttribute(13, nameof(PageSize), PageSize);
        builder.AddAttribute(14, nameof(TotalItems), TotalItems);
        builder.AddAttribute(15, nameof(OnPageChange), OnPageChange);
        builder.AddAttribute(16, nameof(PageSizeOptions), PageSizeOptions);
        
        builder.AddAttribute(17, nameof(MultiSelection), MultiSelection);
        builder.AddAttribute(18, nameof(SingleSelection), SingleSelection);
        builder.AddAttribute(19, nameof(SelectedItemsChanged), SelectedItemsChanged);
        builder.AddAttribute(20, nameof(SelectedItemChanged), SelectedItemChanged);
        
        builder.AddAttribute(21, nameof(RowClickable), RowClickable);
        builder.AddAttribute(22, nameof(OnRowClick), OnRowClick);
        builder.AddAttribute(23, nameof(RowClassFunc), RowClassFunc);
        
        builder.AddAttribute(28, nameof(SortColumn), SortColumn);
        builder.AddAttribute(29, nameof(SortDescending), SortDescending);
        builder.AddAttribute(30, nameof(OnSort), OnSort);
        builder.AddAttribute(31, nameof(SortStates), SortStates);
        builder.AddAttribute(32, nameof(SortStatesChanged), SortStatesChanged);
        builder.AddAttribute(33, nameof(OnSortChanged), OnSortChanged);
        builder.AddAttribute(34, nameof(MultiColumnSort), MultiColumnSort);
        builder.AddAttribute(35, nameof(MaxSortLevels), MaxSortLevels);
        builder.AddAttribute(36, nameof(ColumnFilters), ColumnFilters);
        builder.AddAttribute(37, nameof(ColumnFiltersChanged), ColumnFiltersChanged);
        builder.AddAttribute(38, nameof(OnColumnFilter), OnColumnFilter);
        
        builder.AddAttribute(39, nameof(Height), Height);
        builder.AddAttribute(40, nameof(Virtualize), Virtualize);
        builder.AddAttribute(41, nameof(ResizableColumns), ResizableColumns);
        builder.AddAttribute(42, nameof(StickyColumns), StickyColumns);
        
        builder.AddAttribute(43, nameof(ColumnsContent), ColumnsContent);
        // NOTE: Intentionally NOT forwarding ChildContent - RTableGeneric doesn't support it
        builder.AddAttribute(44, nameof(EmptyMessage), EmptyMessage);
        builder.AddAttribute(45, nameof(LoadingMessage), LoadingMessage);
        builder.AddAttribute(46, nameof(FooterContent), FooterContent);
        
        builder.AddAttribute(47, nameof(ExportFormats), ExportFormats);
        builder.AddAttribute(48, nameof(ExportFileName), ExportFileName);
        builder.AddAttribute(49, nameof(ExportMetadata), ExportMetadata);
        builder.AddAttribute(50, nameof(Density), Density);
        builder.AddAttribute(51, nameof(Striped), Striped);
        builder.AddAttribute(52, nameof(Hover), Hover);
        builder.AddAttribute(53, nameof(Sortable), Sortable);
        builder.AddAttribute(54, nameof(AllowUnsorted), AllowUnsorted);
        
        // Column management parameters
        builder.AddAttribute(55, nameof(ShowColumnManager), ShowColumnManager);
        builder.AddAttribute(56, nameof(ColumnPreferences), ColumnPreferences);
        builder.AddAttribute(57, nameof(ColumnPreferencesChanged), ColumnPreferencesChanged);
        builder.AddAttribute(58, nameof(OnColumnManagement), OnColumnManagement);
        builder.AddAttribute(59, nameof(OnColumnResize), OnColumnResize);
        builder.AddAttribute(60, nameof(EnableColumnReordering), EnableColumnReordering);
        builder.AddAttribute(61, nameof(PersistColumnPreferences), PersistColumnPreferences);
        builder.AddAttribute(62, nameof(TableId), TableId);
        
        // Professional styling parameters
        builder.AddAttribute(63, nameof(TableVariant), TableVariant);
        builder.AddAttribute(64, nameof(TableDensity), TableDensity);
        builder.AddAttribute(65, nameof(CustomRowHeight), CustomRowHeight);
        builder.AddAttribute(66, nameof(TableElevation), TableElevation);
        builder.AddAttribute(67, nameof(EnableGlassmorphism), EnableGlassmorphism);
        builder.AddAttribute(68, nameof(ShowBorders), ShowBorders);
        builder.AddAttribute(69, nameof(EnableHoverEffects), EnableHoverEffects);
        builder.AddAttribute(70, nameof(EnableAnimations), EnableAnimations);
        builder.AddAttribute(71, nameof(TableTheme), TableTheme);
        builder.AddAttribute(72, nameof(CustomCssVariables), CustomCssVariables);
    }
}

/// <summary>
/// Smart table wrapper with automatic type detection.
/// Usage: &lt;RTable Items="@products" Title="Product Catalog" Domain="TableDomain.Ecommerce" /&gt;
/// </summary>
public class RTable : RTableBase
{
    [Parameter] public object Items { get; set; }
    [Parameter] public object SelectedItems { get; set; }
    [Parameter] public object SelectedItem { get; set; }
    [Parameter] public object Columns { get; set; }
    [Parameter] public RenderFragment ChildContent { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (Items == null)
        {
            // Render empty state
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "table-empty-state pa-6 text-center");
            builder.OpenElement(2, "div");
            builder.AddAttribute(3, "class", "text-secondary");
            builder.AddContent(4, EmptyMessage ?? "No data available");
            builder.CloseElement();
            builder.CloseElement();
            return;
        }

        // Auto-detect item type from Items collection
        var itemsType = Items.GetType();
        Type itemType;

        if (itemsType.IsGenericType && itemsType.GetGenericTypeDefinition() == typeof(List<>))
        {
            itemType = itemsType.GetGenericArguments()[0];
        }
        else if (itemsType.IsArray)
        {
            itemType = itemsType.GetElementType();
        }
        else if (itemsType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
        {
            itemType = itemsType.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                .GetGenericArguments()[0];
        }
        else
        {
            itemType = typeof(object);
        }

        // Create table context for child columns
        var tableContext = new TableContext(itemType, $"smart-table-{GetHashCode()}", true);

        // Use ChildContent as ColumnsContent if provided (for compatibility)
        var effectiveColumnsContent = ColumnsContent ?? ChildContent;

        // Provide context to child components
        builder.OpenComponent<CascadingValue<TableContext>>(0);
        builder.AddAttribute(1, "Value", tableContext);
        builder.AddAttribute(2, "ChildContent", (RenderFragment)(childBuilder =>
        {
            // Create RTableGeneric<T> dynamically
            var genericTableType = typeof(RTableGeneric<>).MakeGenericType(itemType);

            childBuilder.OpenComponent(0, genericTableType);
            
            // Forward all base parameters EXCEPT ChildContent (RTableGeneric doesn't support it)
            ForwardBaseParametersExceptChildContent(childBuilder);
            
            // Override ColumnsContent with effective content
            childBuilder.AddAttribute(38, nameof(ColumnsContent), effectiveColumnsContent);
            
            // Forward specific parameters with type conversion
            childBuilder.AddAttribute(50, "Items", Items);
            childBuilder.AddAttribute(51, "SelectedItems", SelectedItems);
            childBuilder.AddAttribute(52, "SelectedItem", SelectedItem);
            childBuilder.AddAttribute(53, "Columns", Columns);
            
            childBuilder.CloseComponent();
        }));
        builder.CloseComponent();
    }
}