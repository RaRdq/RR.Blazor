using System.Collections.Generic;
using System;
using System.Linq;
using RR.Blazor.Enums;

namespace RR.Blazor.Models;

public record RKanboardBoardState(
    IReadOnlyList<RKanboardColumnModel> Columns,
    IReadOnlyDictionary<string, RKanboardCardModel> Cards)
{
    public RKanboardColumnModel? GetColumn(string columnId) =>
        Columns.FirstOrDefault(column => string.Equals(column.Id, columnId, StringComparison.Ordinal));

    public RKanboardCardModel? GetCard(string cardId) =>
        Cards.TryGetValue(cardId, out var card) ? card : null;
}

public record RKanboardColumnModel
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public IReadOnlyList<string> CardIds { get; init; } = Array.Empty<string>();
    public bool IsHidden { get; init; }
    public KanboardColumnConfig Config { get; init; } = new();
    public string? SwimlaneId { get; init; }
    public string? SwimlaneTitle { get; init; }
    public int SwimlaneOrder { get; init; }
}

public record KanboardColumnConfig
{
    public string? Icon { get; init; }
    public string? ColorClass { get; init; }
    public KanboardColumnWidth Width { get; init; } = KanboardColumnWidth.Auto;
    public int? WorkInProgressLimit { get; init; }
    public bool AllowDrop { get; init; } = true;
    public string? AutoAssignAssigneeId { get; init; }
    public IReadOnlyDictionary<string, object> Metadata { get; init; } = new Dictionary<string, object>();
}

public record RKanboardCardModel
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string Title { get; init; } = string.Empty;
    public string? Subtitle { get; init; }
    public string? Description { get; init; }
    public string? AssigneeId { get; init; }
    public string? AssigneeDisplayName { get; init; }
    public string? AssigneeAvatarUrl { get; init; }
    public string? AssigneeRole { get; init; }
    public IReadOnlyList<RKanboardLabelModel> Labels { get; init; } = Array.Empty<RKanboardLabelModel>();
    public IReadOnlyList<RKanboardBadgeModel> Badges { get; init; } = Array.Empty<RKanboardBadgeModel>();
    public DateTime? DueDate { get; init; }
    public string? DueDateLabel { get; init; }
    public string? PriorityLabel { get; init; }
    public string? PriorityColorClass { get; init; }
    public string? CommentsSummary { get; init; }
    public bool IsBlocked { get; init; }
    public double? ChecklistProgress { get; init; }
    public IReadOnlyList<RKanboardChecklistModel> Checklists { get; init; } = Array.Empty<RKanboardChecklistModel>();
    public IReadOnlyList<RKanboardAttachmentModel> Attachments { get; init; } = Array.Empty<RKanboardAttachmentModel>();
    public IReadOnlyList<RKanboardParticipantModel> Watchers { get; init; } = Array.Empty<RKanboardParticipantModel>();
    public IReadOnlyList<RKanboardParticipantModel> Collaborators { get; init; } = Array.Empty<RKanboardParticipantModel>();
    public IReadOnlyDictionary<string, object> Metadata { get; init; } = new Dictionary<string, object>();
}

public record RKanboardLabelModel(
    string Id,
    string Text,
    string? ColorClass = null,
    string? Icon = null);

public record RKanboardBadgeModel(
    string Text,
    string? ColorClass,
    string? Icon);

public record RKanboardChecklistModel(
    string Id,
    string Title,
    IReadOnlyList<RKanboardChecklistItem> Items);

public record RKanboardChecklistItem(
    string Id,
    string Text,
    bool IsCompleted);

public record RKanboardAttachmentModel(
    string Id,
    string Name,
    string? Url,
    string? Icon = null,
    DateTime? UploadedOn = null);

public record RKanboardParticipantModel(
    string Id,
    string DisplayName,
    string? AvatarUrl,
    string? Role);

public record RKanboardAssigneeOption(
    string Id,
    string DisplayName,
    string? Email,
    string? AvatarUrl,
    string? Role);

public record RKanboardColumnRenderContext(
    RKanboardColumnModel Column,
    IReadOnlyList<RKanboardCardModel> Cards,
    bool IsCollapsed);

public record RKanboardSwimlaneRenderContext(
    string SwimlaneId,
    string? Title,
    IReadOnlyList<RKanboardColumnModel> Columns);

public record RKanboardCardDropContext(
    string CardId,
    string FromColumnId,
    string ToColumnId,
    int FromIndex,
    int ToIndex,
    RKanboardBoardState BoardState)
{
    public string? SuggestedAssigneeId { get; init; }
    public bool IsSameColumn => string.Equals(FromColumnId, ToColumnId, StringComparison.Ordinal);
    public bool HasPositionChanged => !IsSameColumn || FromIndex != ToIndex;
}

public record RKanboardAssigneeChangedContext(
    string CardId,
    string ColumnId,
    string? PreviousAssigneeId,
    string? NewAssigneeId);

public record RKanboardColumnMutationContext(
    string Title,
    string? Description,
    int InsertIndex,
    KanboardColumnConfig? Config = null,
    string? TemplateId = null,
    string? SwimlaneId = null);

public record RKanboardColumnVisibilityChangeContext(
    string ColumnId,
    bool IsHidden);

public record RKanboardColumnLayoutChangedContext(
    IReadOnlyList<string> ColumnOrder);

public record RKanboardAutoAssignInstruction(
    string? AssigneeId,
    string Reason);

public record RKanboardColumnTemplate(
    string TemplateId,
    string Title,
    string? Description,
    KanboardColumnConfig Config,
    string? SwimlaneId = null,
    string? BadgeText = null,
    string? BadgeClass = null);

public record RKanboardCardMutationContext(
    string CardId,
    string ColumnId,
    KanboardCardMutationType MutationType,
    IReadOnlyDictionary<string, object>? Metadata);

public delegate Task<RKanboardAutoAssignInstruction?> RKanboardAutoAssignRule(RKanboardCardDropContext context);

public record RKanboardDropValidationResult(
    bool IsAllowed,
    string? Reason = null,
    RKanboardAutoAssignInstruction? SuggestedAssignment = null);

public record RKanboardMutationResult(
    bool Succeeded,
    RKanboardBoardState? UpdatedBoard = null,
    string? Message = null);

public record class RKanboardUserPreferences
{
    public string BoardKey { get; init; } = string.Empty;
    public KanboardOrientation Orientation { get; init; } = KanboardOrientation.Vertical;
    public List<string> HiddenColumnIds { get; init; } = new();
    public List<string> ColumnOrder { get; init; } = new();
    public DensityType Density { get; init; } = DensityType.Normal;
    public SizeType Size { get; init; } = SizeType.Medium;
    public VariantType Variant { get; init; } = VariantType.Primary;
    public DesignMode DesignMode { get; init; } = DesignMode.Material;
    public List<RKanboardCustomColumnPreference> CustomColumns { get; init; } = new();
    public Dictionary<string, string> CustomCardAssignments { get; init; } = new(StringComparer.Ordinal);
    public Dictionary<string, List<string>> ColumnCardOrder { get; init; } = new(StringComparer.Ordinal);

    public static RKanboardUserPreferences CreateDefault(string boardKey, KanboardOrientation orientation = KanboardOrientation.Vertical) =>
        new()
        {
            BoardKey = boardKey,
            Orientation = orientation
        };
}

public record RKanboardCustomColumnPreference
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Icon { get; init; }
    public string? ColorClass { get; init; }
    public KanboardColumnWidth Width { get; init; } = KanboardColumnWidth.Auto;
    public string? SwimlaneId { get; init; }
    public bool AllowDrop { get; init; } = true;
    public IReadOnlyDictionary<string, object> Metadata { get; init; } = new Dictionary<string, object>();
}

public record RKanboardCardCreateContext(
    string ColumnId,
    string? SwimlaneId,
    string Title,
    string? Description);

public enum KanboardChecklistMutationType
{
    AddChecklist = 0,
    RenameChecklist = 1,
    DeleteChecklist = 2,
    AddItem = 10,
    UpdateItem = 11,
    ToggleItem = 12,
    DeleteItem = 13
}

public record RKanboardChecklistMutationContext(
    KanboardChecklistMutationType MutationType,
    string CardId,
    string ChecklistId,
    string? ChecklistTitle,
    string? ItemId,
    string? ItemText,
    bool? ItemCompleted);

public record RKanboardCollaboratorEditContext(
    string CardId,
    string ColumnId,
    IReadOnlyList<RKanboardParticipantModel> Collaborators);
