using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RR.Blazor.Models;

namespace RR.Blazor.Services;

/// <summary>
/// Simple in-memory adapter that fulfils <see cref="IRKanboardDataAdapter"/> contracts for local prototyping.
/// Can be supplied to <see cref="RKanboardAdapterOrchestrator"/> without wiring a backend.
/// </summary>
public class RKanboardInMemoryAdapter : RKanboardDataAdapterBase
{
    private readonly object gate = new();
    private readonly RKanboardStateManager stateManager = new();
    private RKanboardBoardState board;
    private IReadOnlyCollection<RKanboardAssigneeOption> directory = Array.Empty<RKanboardAssigneeOption>();
    private IReadOnlyCollection<RKanboardAutoAssignRule> autoAssignRules = Array.Empty<RKanboardAutoAssignRule>();

    public RKanboardInMemoryAdapter(RKanboardBoardState? initialState = null)
    {
        board = initialState ?? new RKanboardBoardState(
            Array.Empty<RKanboardColumnModel>(),
            new Dictionary<string, RKanboardCardModel>(StringComparer.Ordinal));
    }

    /// <summary>
    /// Replaces the in-memory board snapshot with the provided state.
    /// </summary>
    public void Reset(RKanboardBoardState newState)
    {
        if (newState is null) throw new ArgumentNullException(nameof(newState));

        lock (gate)
        {
            board = newState;
        }
    }

    /// <summary>
    /// Configures the assignee directory returned by <see cref="GetAssigneesAsync"/>.
    /// </summary>
    public void SetAssignees(IEnumerable<RKanboardAssigneeOption>? options)
    {
        var normalized = options?
            .Where(option => option is not null && !string.IsNullOrWhiteSpace(option.Id))
            .GroupBy(option => option.Id!, StringComparer.Ordinal)
            .Select(group => group.First())
            .ToList();

        directory = normalized is { Count: > 0 }
            ? normalized
            : Array.Empty<RKanboardAssigneeOption>();
    }

    /// <summary>
    /// Registers auto-assign delegates that will be surfaced through <see cref="GetAutoAssignRulesAsync"/>.
    /// </summary>
    public void SetAutoAssignRules(IEnumerable<RKanboardAutoAssignRule>? rules)
    {
        var normalized = rules?
            .Where(rule => rule is not null)
            .ToList();

        autoAssignRules = normalized is { Count: > 0 }
            ? normalized!
            : Array.Empty<RKanboardAutoAssignRule>();
    }

    public override Task<RKanboardBoardState?> LoadBoardAsync(CancellationToken ct = default)
    {
        lock (gate)
        {
            return Task.FromResult<RKanboardBoardState?>(board);
        }
    }

    public override Task<RKanboardMutationResult?> OnCardMovedAsync(RKanboardCardDropContext context, CancellationToken ct = default)
    {
        if (context is null) throw new ArgumentNullException(nameof(context));

        RKanboardBoardState updated;
        lock (gate)
        {
            board = stateManager.MoveCard(board, context);
            updated = board;
        }

        return Task.FromResult<RKanboardMutationResult?>(new RKanboardMutationResult(true, updated));
    }

    public override Task<RKanboardMutationResult?> OnCardUpdatedAsync(RKanboardCardModel card, CancellationToken ct = default)
    {
        if (card is null) throw new ArgumentNullException(nameof(card));

        RKanboardBoardState updated;
        lock (gate)
        {
            var cards = board.Cards.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
            cards[card.Id] = card;
            updated = board = board with { Cards = cards };
        }

        return Task.FromResult<RKanboardMutationResult?>(new RKanboardMutationResult(true, updated));
    }

    public override Task<IReadOnlyCollection<RKanboardAssigneeOption>> GetAssigneesAsync(string? query, CancellationToken ct = default)
    {
        if (directory.Count == 0)
        {
            return Task.FromResult<IReadOnlyCollection<RKanboardAssigneeOption>>(Array.Empty<RKanboardAssigneeOption>());
        }

        if (string.IsNullOrWhiteSpace(query))
        {
            return Task.FromResult(directory);
        }

        var matches = directory
            .Where(option => option.DisplayName?.Contains(query, StringComparison.OrdinalIgnoreCase) == true)
            .ToList();

        return Task.FromResult<IReadOnlyCollection<RKanboardAssigneeOption>>(matches);
    }

    public override Task<IReadOnlyCollection<RKanboardAutoAssignRule>> GetAutoAssignRulesAsync(CancellationToken ct = default) =>
        Task.FromResult(autoAssignRules);
}
