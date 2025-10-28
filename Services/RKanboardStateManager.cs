using System;
using System.Collections.Generic;
using System.Linq;
using RR.Blazor.Models;

namespace RR.Blazor.Services;

public class RKanboardStateManager
{
    public RKanboardBoardState MoveCard(RKanboardBoardState board, RKanboardCardDropContext context)
    {
        if (board is null) throw new ArgumentNullException(nameof(board));
        if (context is null) throw new ArgumentNullException(nameof(context));

        if (!board.Cards.ContainsKey(context.CardId)) return board;

        var columnCardMap = board.Columns.ToDictionary(column => column.Id, column => column.CardIds.ToList());

        if (!columnCardMap.TryGetValue(context.FromColumnId, out var originCards)) return board;
        if (!columnCardMap.TryGetValue(context.ToColumnId, out var destinationCards)) return board;

        var fromIndex = context.FromIndex;
        if (fromIndex < 0 || fromIndex >= originCards.Count) return board;

        var cardId = originCards[fromIndex];
        if (!string.Equals(cardId, context.CardId, StringComparison.Ordinal))
        {
            fromIndex = originCards.FindIndex(id => string.Equals(id, context.CardId, StringComparison.Ordinal));
            if (fromIndex < 0) return board;
            cardId = originCards[fromIndex];
        }

        originCards.RemoveAt(fromIndex);

        var targetIndex = context.ToIndex;
        if (targetIndex < 0 || targetIndex > destinationCards.Count)
        {
            targetIndex = destinationCards.Count;
        }

        destinationCards.Insert(targetIndex, cardId);

        var updatedColumns = board.Columns.Select(column =>
        {
            if (!columnCardMap.TryGetValue(column.Id, out var cards))
            {
                return column;
            }

            return column with { CardIds = cards.ToArray() };
        }).ToList();

        return board with { Columns = updatedColumns };
    }

    public RKanboardBoardState UpdateAssignee(RKanboardBoardState board, string cardId, string? assigneeId, string? assigneeName = null, string? avatarUrl = null, string? role = null)
    {
        if (board is null) throw new ArgumentNullException(nameof(board));
        if (string.IsNullOrWhiteSpace(cardId)) return board;

        if (!board.Cards.TryGetValue(cardId, out var card))
            return board;

        var updatedCard = card with
        {
            AssigneeId = assigneeId,
            AssigneeDisplayName = assigneeName,
            AssigneeAvatarUrl = avatarUrl,
            AssigneeRole = role
        };

        var updatedCards = board.Cards.ToDictionary(pair => pair.Key, pair => pair.Value);
        updatedCards[cardId] = updatedCard;

        return board with { Cards = updatedCards };
    }

    public RKanboardBoardState ToggleColumnVisibility(RKanboardBoardState board, string columnId, bool isHidden)
    {
        if (board is null) throw new ArgumentNullException(nameof(board));
        if (string.IsNullOrWhiteSpace(columnId)) return board;

        var updatedColumns = board.Columns
            .Select(column => string.Equals(column.Id, columnId, StringComparison.Ordinal)
                ? column with { IsHidden = isHidden }
                : column)
            .ToList();

        return board with { Columns = updatedColumns };
    }

    public RKanboardBoardState UpsertColumn(RKanboardBoardState board, RKanboardColumnModel column, int insertIndex = -1)
    {
        if (board is null) throw new ArgumentNullException(nameof(board));
        if (column is null) throw new ArgumentNullException(nameof(column));

        var columns = board.Columns.ToList();
        var existingIndex = columns.FindIndex(c => string.Equals(c.Id, column.Id, StringComparison.Ordinal));

        if (existingIndex >= 0)
        {
            columns[existingIndex] = column;
        }
        else
        {
            if (insertIndex < 0 || insertIndex > columns.Count)
            {
                columns.Add(column);
            }
            else
            {
                columns.Insert(insertIndex, column);
            }
        }

        return board with { Columns = columns };
    }

    public RKanboardBoardState ReorderColumns(RKanboardBoardState board, IReadOnlyList<string> columnOrder)
    {
        if (board is null) throw new ArgumentNullException(nameof(board));
        if (columnOrder is null || columnOrder.Count == 0) return board;

        var columnMap = board.Columns.ToDictionary(column => column.Id, column => column, StringComparer.Ordinal);
        var reordered = new List<RKanboardColumnModel>();

        foreach (var columnId in columnOrder)
        {
            if (columnMap.TryGetValue(columnId, out var column))
            {
                reordered.Add(column);
            }
        }

        foreach (var column in board.Columns)
        {
            if (!reordered.Any(existing => string.Equals(existing.Id, column.Id, StringComparison.Ordinal)))
            {
                reordered.Add(column);
            }
        }

        return board with { Columns = reordered };
    }

    public RKanboardBoardState UpdateCollaborators(RKanboardBoardState board, string cardId, IReadOnlyList<RKanboardParticipantModel> collaborators)
    {
        if (board is null) throw new ArgumentNullException(nameof(board));
        if (string.IsNullOrWhiteSpace(cardId)) return board;

        if (!board.Cards.TryGetValue(cardId, out var card))
        {
            return board;
        }

        var updatedCard = card with { Collaborators = collaborators ?? Array.Empty<RKanboardParticipantModel>() };
        var updatedCards = board.Cards.ToDictionary(pair => pair.Key, pair => pair.Value);
        updatedCards[cardId] = updatedCard;

        return board with { Cards = updatedCards };
    }

    public RKanboardBoardState ApplyChecklistMutation(RKanboardBoardState board, RKanboardChecklistMutationContext context)
    {
        if (board is null) throw new ArgumentNullException(nameof(board));
        if (context is null) throw new ArgumentNullException(nameof(context));

        if (!board.Cards.TryGetValue(context.CardId, out var card))
        {
            return board;
        }

        var checklists = card.Checklists?.ToList() ?? new List<RKanboardChecklistModel>();

        switch (context.MutationType)
        {
            case KanboardChecklistMutationType.AddChecklist:
            {
                var checklistId = string.IsNullOrWhiteSpace(context.ChecklistId) ? Guid.NewGuid().ToString("N") : context.ChecklistId;
                var newChecklist = new RKanboardChecklistModel(
                    checklistId,
                    context.ChecklistTitle ?? "Checklist",
                    Array.Empty<RKanboardChecklistItem>());
                checklists.Add(newChecklist);
                break;
            }
            case KanboardChecklistMutationType.RenameChecklist:
            {
                var index = checklists.FindIndex(list => string.Equals(list.Id, context.ChecklistId, StringComparison.Ordinal));
                if (index >= 0)
                {
                    var existing = checklists[index];
                    checklists[index] = existing with { Title = context.ChecklistTitle ?? existing.Title };
                }
                break;
            }
            case KanboardChecklistMutationType.DeleteChecklist:
            {
                checklists.RemoveAll(list => string.Equals(list.Id, context.ChecklistId, StringComparison.Ordinal));
                break;
            }
            case KanboardChecklistMutationType.AddItem:
            {
                var index = checklists.FindIndex(list => string.Equals(list.Id, context.ChecklistId, StringComparison.Ordinal));
                if (index >= 0)
                {
                    var existing = checklists[index];
                    var items = existing.Items?.ToList() ?? new List<RKanboardChecklistItem>();
                    var itemId = string.IsNullOrWhiteSpace(context.ItemId) ? Guid.NewGuid().ToString("N") : context.ItemId;
                    items.Add(new RKanboardChecklistItem(itemId, context.ItemText ?? string.Empty, false));
                    checklists[index] = existing with { Items = items };
                }
                break;
            }
            case KanboardChecklistMutationType.UpdateItem:
            {
                var checklistIndex = checklists.FindIndex(list => string.Equals(list.Id, context.ChecklistId, StringComparison.Ordinal));
                if (checklistIndex >= 0)
                {
                    var existing = checklists[checklistIndex];
                    var items = existing.Items?.ToList() ?? new List<RKanboardChecklistItem>();
                    var itemIndex = items.FindIndex(item => string.Equals(item.Id, context.ItemId, StringComparison.Ordinal));
                    if (itemIndex >= 0)
                    {
                        var item = items[itemIndex];
                        items[itemIndex] = item with { Text = context.ItemText ?? item.Text };
                        checklists[checklistIndex] = existing with { Items = items };
                    }
                }
                break;
            }
            case KanboardChecklistMutationType.ToggleItem:
            {
                var checklistIndex = checklists.FindIndex(list => string.Equals(list.Id, context.ChecklistId, StringComparison.Ordinal));
                if (checklistIndex >= 0)
                {
                    var existing = checklists[checklistIndex];
                    var items = existing.Items?.ToList() ?? new List<RKanboardChecklistItem>();
                    var itemIndex = items.FindIndex(item => string.Equals(item.Id, context.ItemId, StringComparison.Ordinal));
                    if (itemIndex >= 0)
                    {
                        var item = items[itemIndex];
                        items[itemIndex] = item with { IsCompleted = context.ItemCompleted ?? item.IsCompleted };
                        checklists[checklistIndex] = existing with { Items = items };
                    }
                }
                break;
            }
            case KanboardChecklistMutationType.DeleteItem:
            {
                var checklistIndex = checklists.FindIndex(list => string.Equals(list.Id, context.ChecklistId, StringComparison.Ordinal));
                if (checklistIndex >= 0)
                {
                    var existing = checklists[checklistIndex];
                    var items = existing.Items?.ToList() ?? new List<RKanboardChecklistItem>();
                    items.RemoveAll(item => string.Equals(item.Id, context.ItemId, StringComparison.Ordinal));
                    checklists[checklistIndex] = existing with { Items = items };
                }
                break;
            }
        }

        var updatedCard = card with { Checklists = checklists };
        var updatedCards = board.Cards.ToDictionary(pair => pair.Key, pair => pair.Value);
        updatedCards[context.CardId] = updatedCard;

        return board with { Cards = updatedCards };
    }

    public RKanboardBoardState InsertCard(RKanboardBoardState board, string columnId, RKanboardCardModel card, int insertIndex = -1)
    {
        if (board is null) throw new ArgumentNullException(nameof(board));
        if (card is null) throw new ArgumentNullException(nameof(card));
        if (string.IsNullOrWhiteSpace(columnId)) return board;

        var columns = board.Columns.ToDictionary(column => column.Id, column => column, StringComparer.Ordinal);
        if (!columns.TryGetValue(columnId, out var targetColumn))
        {
            return board;
        }

        var cards = targetColumn.CardIds?.ToList() ?? new List<string>();
        if (insertIndex < 0 || insertIndex > cards.Count)
        {
            cards.Add(card.Id);
        }
        else
        {
            cards.Insert(insertIndex, card.Id);
        }

        columns[columnId] = targetColumn with { CardIds = cards };

        var updatedColumns = board.Columns.Select(column => columns.TryGetValue(column.Id, out var updated) ? updated : column).ToList();
        var updatedCards = board.Cards.ToDictionary(pair => pair.Key, pair => pair.Value);
        updatedCards[card.Id] = card;

        return board with { Columns = updatedColumns, Cards = updatedCards };
    }

    public Dictionary<string, List<string>> GetColumnCardOrder(RKanboardBoardState board)
    {
        if (board is null) throw new ArgumentNullException(nameof(board));

        return board.Columns
            .ToDictionary(
                column => column.Id,
                column => column.CardIds?.ToList() ?? new List<string>(),
                StringComparer.Ordinal);
    }
}
