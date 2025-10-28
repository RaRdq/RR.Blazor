using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RR.Blazor.Models;

namespace RR.Blazor.Services;

public interface IRKanboardDataAdapter
{
    Task<RKanboardBoardState?> LoadBoardAsync(CancellationToken ct = default);
    Task<RKanboardDropValidationResult> ValidateDropAsync(RKanboardCardDropContext context, CancellationToken ct = default);
    Task<RKanboardMutationResult?> OnCardMovedAsync(RKanboardCardDropContext context, CancellationToken ct = default);
    Task<RKanboardMutationResult?> OnCardUpdatedAsync(RKanboardCardModel card, CancellationToken ct = default);
    Task<IReadOnlyCollection<RKanboardAssigneeOption>> GetAssigneesAsync(string? query, CancellationToken ct = default);
    Task<IReadOnlyCollection<RKanboardAutoAssignRule>> GetAutoAssignRulesAsync(CancellationToken ct = default);
}

public abstract class RKanboardDataAdapterBase : IRKanboardDataAdapter
{
    public virtual Task<RKanboardBoardState?> LoadBoardAsync(CancellationToken ct = default) =>
        Task.FromResult<RKanboardBoardState?>(null);

    public virtual Task<RKanboardDropValidationResult> ValidateDropAsync(RKanboardCardDropContext context, CancellationToken ct = default) =>
        Task.FromResult(new RKanboardDropValidationResult(true));

    public virtual Task<RKanboardMutationResult?> OnCardMovedAsync(RKanboardCardDropContext context, CancellationToken ct = default) =>
        Task.FromResult<RKanboardMutationResult?>(null);

    public virtual Task<RKanboardMutationResult?> OnCardUpdatedAsync(RKanboardCardModel card, CancellationToken ct = default) =>
        Task.FromResult<RKanboardMutationResult?>(null);

    public virtual Task<IReadOnlyCollection<RKanboardAssigneeOption>> GetAssigneesAsync(string? query, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyCollection<RKanboardAssigneeOption>>(Array.Empty<RKanboardAssigneeOption>());

    public virtual Task<IReadOnlyCollection<RKanboardAutoAssignRule>> GetAutoAssignRulesAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyCollection<RKanboardAutoAssignRule>>(Array.Empty<RKanboardAutoAssignRule>());
}

public class RKanboardAdapterOrchestrator
{
    private readonly List<IRKanboardDataAdapter> adapters = new();
    private readonly ILogger<RKanboardAdapterOrchestrator>? logger;

    public RKanboardAdapterOrchestrator(ILogger<RKanboardAdapterOrchestrator>? logger = null)
    {
        this.logger = logger;
    }

    public RKanboardAdapterOrchestrator RegisterAdapter(IRKanboardDataAdapter adapter)
    {
        if (adapter is null) throw new ArgumentNullException(nameof(adapter));
        adapters.Add(adapter);
        return this;
    }

    public IReadOnlyList<IRKanboardDataAdapter> RegisteredAdapters => adapters;

    public async Task<RKanboardBoardState> LoadBoardAsync(CancellationToken ct = default)
    {
        RKanboardBoardState? board = null;
        foreach (var adapter in adapters)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                var next = await adapter.LoadBoardAsync(ct);
                if (next is null) continue;
                board = board is null ? next : MergeBoardState(board, next);
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "Kanboard adapter {Adapter} failed during LoadBoardAsync", adapter.GetType().Name);
            }
        }

        return board ?? new RKanboardBoardState(Array.Empty<RKanboardColumnModel>(), new Dictionary<string, RKanboardCardModel>());
    }

    public async Task<RKanboardDropValidationResult> ValidateDropAsync(RKanboardCardDropContext context, CancellationToken ct = default)
    {
        var combinedResult = new RKanboardDropValidationResult(true);
        foreach (var adapter in adapters)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                var result = await adapter.ValidateDropAsync(context, ct);
                if (!result.IsAllowed)
                {
                    return result;
                }

                if (combinedResult.SuggestedAssignment is null && result.SuggestedAssignment is not null)
                {
                    combinedResult = combinedResult with { SuggestedAssignment = result.SuggestedAssignment };
                }
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "Kanboard adapter {Adapter} failed during ValidateDropAsync", adapter.GetType().Name);
            }
        }

        return combinedResult;
    }

    public async Task<RKanboardMutationResult> OnCardMovedAsync(RKanboardCardDropContext context, CancellationToken ct = default)
    {
        var currentBoard = context.BoardState;
        foreach (var adapter in adapters)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                var result = await adapter.OnCardMovedAsync(context, ct);
                if (result?.UpdatedBoard is not null)
                {
                    currentBoard = MergeBoardState(currentBoard, result.UpdatedBoard);
                }

                if (result is { Succeeded: false })
                {
                    return new RKanboardMutationResult(false, currentBoard, result.Message);
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Kanboard adapter {Adapter} threw during OnCardMovedAsync", adapter.GetType().Name);
                return new RKanboardMutationResult(false, currentBoard, ex.Message);
            }
        }

        return new RKanboardMutationResult(true, currentBoard);
    }

    public async Task<RKanboardMutationResult> OnCardUpdatedAsync(RKanboardCardModel card, CancellationToken ct = default)
    {
        RKanboardBoardState? updatedBoard = null;
        foreach (var adapter in adapters)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                var result = await adapter.OnCardUpdatedAsync(card, ct);
                if (result?.UpdatedBoard is not null)
                {
                    updatedBoard = updatedBoard is null ? result.UpdatedBoard : MergeBoardState(updatedBoard, result.UpdatedBoard);
                }

                if (result is { Succeeded: false })
                {
                    return new RKanboardMutationResult(false, updatedBoard, result.Message);
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Kanboard adapter {Adapter} threw during OnCardUpdatedAsync", adapter.GetType().Name);
                return new RKanboardMutationResult(false, updatedBoard, ex.Message);
            }
        }

        return new RKanboardMutationResult(true, updatedBoard);
    }

    public async Task<IReadOnlyList<RKanboardAssigneeOption>> GetAssigneesAsync(string? query, CancellationToken ct = default)
    {
        var options = new List<RKanboardAssigneeOption>();
        foreach (var adapter in adapters)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                var results = await adapter.GetAssigneesAsync(query, ct);
                if (results?.Count > 0)
                {
                    options.AddRange(results);
                }
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "Kanboard adapter {Adapter} failed during GetAssigneesAsync", adapter.GetType().Name);
            }
        }

        return options
            .GroupBy(option => option.Id ?? string.Empty, StringComparer.Ordinal)
            .Select(group => group.First())
            .ToList();
    }

    public async Task<IReadOnlyList<RKanboardAutoAssignRule>> GetAutoAssignRulesAsync(CancellationToken ct = default)
    {
        var rules = new List<RKanboardAutoAssignRule>();
        foreach (var adapter in adapters)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                var result = await adapter.GetAutoAssignRulesAsync(ct);
                if (result?.Count > 0)
                {
                    rules.AddRange(result);
                }
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "Kanboard adapter {Adapter} failed during GetAutoAssignRulesAsync", adapter.GetType().Name);
            }
        }

        return rules;
    }

    private static RKanboardBoardState MergeBoardState(RKanboardBoardState primary, RKanboardBoardState secondary)
    {
        var columnMap = primary.Columns.ToDictionary(column => column.Id, column => column);
        foreach (var column in secondary.Columns)
        {
            columnMap[column.Id] = column;
        }

        var cardMap = primary.Cards.ToDictionary(pair => pair.Key, pair => pair.Value);
        foreach (var (cardId, card) in secondary.Cards)
        {
            cardMap[cardId] = card;
        }

        return new RKanboardBoardState(columnMap.Values.ToList(), cardMap);
    }
}
