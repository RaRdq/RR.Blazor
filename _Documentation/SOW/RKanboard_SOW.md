# RR.Blazor RKanboard Component - Statement of Work

## Executive Summary
Deliver a reusable, extensible kanban experience (`RKanboard`) inside RR.Blazor that matches enterprise-grade workflows. The component must cover interactive drag-and-drop cards, dynamic state management, rich assignment workflows, layout customization (vertical/horizontal), and an adapter architecture that allows downstream projects to plug in arbitrary backends (issue trackers, Jira, banking APIs, etc.).

## Objectives
- Build a composable R* smart component that exposes kanban primitives while staying headless about domain data.
- Mirror Trello-quality UX: column add/remove/hide, card drag between states, inline editing hooks, quick assignment.
- Enable developers to wire business rules (auto-assign, SLA tracking, validation, analytics) without modifying the core component.
- Ship a production-ready reference implementation on the flagship Issues page with view-toggle parity (table ↔ kanboard).

## Core User Experience Requirements
1. **Board Shell**
   - Column-based layout with responsive breakpoints and keyboard/ARIA support.
   - Configurable orientation (columns top-to-bottom or left-to-right).
   - Column collapse/hide toggle, reorder, and virtualization for large boards.
2. **Card Interactions**
   - Drag-and-drop across columns, including auto-scroll and drop validation hooks.
   - Inline quick actions: open details, change priority, add comment (via callbacks).
   - Assignment surface: click avatar/placeholder to open people picker (RChoice or autosuggest).
   - Reorder cards within a column and persist new order via adapter callbacks.
3. **State Management**
   - Dynamic columns defined by consumer. Support ad-hoc addition/removal at runtime.
   - Optional WIP limits, SLA indicators, and auto-assignment rules triggered on drop.
   - Column metadata (color, icon, policy badges).
4. **Customization**
   - Templates for column header, column footer, card body, card footer, and board-level tools.
   - Theme-aware styling using RR.Blazor utility classes (no bespoke CSS leaks).
   - Density and compact modes for embedding boards in tight layouts.
   - Persisted column visibility/order, orientation, and custom column definitions per board via `RKanboardUserPreferences`.
5. **Data & Actions**
   - Immutable input models; all state changes emitted via event callbacks.
   - Built-in optimistic update helper that consumers can opt into or replace.
   - Batched change events for analytics and audit logging.

## Architectural Requirements
### Component Hierarchy
- `RKanboard` main component exposes parameters for data, layout, behavior flags, and adapter registration.
- Child smart components:
  - `RKanboardColumnShell` (internal) for rendering header/action slots.
  - `RKanboardCard` (internal) handling drag logic, keyboard shortcuts, focus trapping.
  - `RKanboardAssigneePicker` thin wrapper over `RChoice`/autosuggest with plugin support.

### Data Contracts
```csharp
public record RKanboardColumnModel(
    string Id,
    string Title,
    string? Description,
    IReadOnlyList<string> CardIds,
    bool IsHidden,
    RKanboardColumnConfig Config);

public record RKanboardCardModel(
    string Id,
    string Title,
    string? Subtitle,
    string? Description,
    string? AssigneeId,
    string? AssigneeDisplayName,
    string? AssigneeRole,
    IReadOnlyList<RKanboardLabelModel> Labels,
    IReadOnlyList<RKanboardBadgeModel> Badges,
    DateTime? DueDate,
    string? DueDateLabel,
    string? PriorityLabel,
    string? PriorityColorClass,
    string? CommentsSummary,
    bool IsBlocked,
    double? ChecklistProgress,
    IReadOnlyList<RKanboardChecklistModel> Checklists,
    IReadOnlyList<RKanboardAttachmentModel> Attachments,
    IReadOnlyList<RKanboardParticipantModel> Watchers,
    IReadOnlyDictionary<string, object> Metadata);

public record RKanboardBoardState(
    IReadOnlyList<RKanboardColumnModel> Columns,
    IReadOnlyDictionary<string, RKanboardCardModel> Cards);
```

Supporting models: `RKanboardLabelModel`, `RKanboardBadgeModel`, `RKanboardChecklistModel`, `RKanboardChecklistItem`, `RKanboardAttachmentModel`, and `RKanboardParticipantModel` capture Trello-style metadata surfaced directly on cards.

### Adapter Pipeline
- Register adapters implementing `IRKanboardDataAdapter` (strategy/transporter pattern) responsible for CRUD with external systems.
- Adapter responsibilities:
  - Fetch initial state.
  - Validate transitions (`CanDropAsync`).
  - Persist mutations (`OnCardMovedAsync`, `OnCardUpdatedAsync`).
  - Provide auxiliary data (assignee directory, tags, SLA metadata).
- Support multiple adapters chained through a composite orchestrator so boards can merge local data with external events.

### Customization Hooks
- Events:
  - `OnCardDropAsync(CardDropContext context)`
  - `OnCardEditRequested(string cardId)`
  - `OnAssigneeChanged(RKanboardAssigneeChangedContext context)`
  - `OnColumnCreateAsync(ColumnMutationContext context)`
  - `OnColumnLayoutChanged(IReadOnlyList<string> columnOrder)`
  - `OnCardBulkUpdateAsync(IEnumerable<CardMutationContext> changes)`
- Render fragments:
  - `ColumnHeader`, `ColumnFooter`, `CardTemplate`, `CardFooter`, `BoardToolbar`, `EmptyState`.
  - `SwimlaneHeader` for horizontal group labels.
- Behavior flags:
  - `AllowColumnCreation`, `AllowColumnHide`, `AllowHorizontal`, `UseDenseMode`, `EnableSwimlanes`, `EnableAutoAssign`.

### Security Hooks
- `CanDragEvaluator` delegate per card/column to enforce role-based restrictions.
- Read-only switch to render board interactions without drag while keeping contextual actions.

### Auto-Assignment Rules
- Accept collection of `RKanboardAutoAssignRule` delegates. Each rule receives `CardDropContext` and may return `AssigneeChangeInstruction`.
- Provide built-in helper for "assign to column owner" plus ability to combine rules.
- Consumers can hook into `OnCardDropAsync` to veto or enforce assignments.

### Orientation & Layout
- Orientation parameter `KanboardOrientation` with `Vertical` (columns left-to-right) and `Horizontal` (swimlanes top-to-bottom).
- Virtualized column container when board width exceeds viewport; fallback to horizontal scroll.
- Column width presets: `Auto`, `Compact`, `Wide`, with responsive breakpoints.

### Accessibility & Keyboard
- Implement roving tabindex for cards.
- Support keyboard move (Ctrl+Arrow) to shift card between columns.
- ARIA roles: `role="list"` for column, `role="listitem"` for card.

### Persistence Helpers
- Provide `RKanboardStateManager` service that tracks optimistic UI state, merges adapter responses, and reverts on failure.
- Optional storage provider (local storage) for user-specific board preferences (hidden columns, orientation, density, column order, custom columns, card placements).

## Reference Issues Integration Scope
1. Replace existing Issues table header controls with `TabToolbar` toggle extended to `Kanboard`.
2. When managers/HR switch to board view:
   - Columns reflect issue statuses (Open, In Progress, Escalated, Resolved, Closed, Cancelled).
  - Allow custom columns for company-defined states via dynamic config.
  - Persist custom columns and card placements per manager via preference storage (local today, server-backed in future iteration).
  - Cards display key metadata (employee, category, priority, due date, comments count) plus Trello-style enrichments (labels, attachments, watcher avatars).
  - Inline quick actions (priority change, comment compose, view details) render inside the card without modal context switching.
   - Dragging triggers Issues API assignment/status update.
   - Inline assignee picker hits IssuesService search endpoint.
   - Auto-assign to manager/hr based on column rules (e.g., dropping into `Escalated` assigns HR escalation team).
3. Persist board preferences per user/company (orientation, hidden columns).
4. Provide fallback to table for employees or roles without drag privileges.
5. Maintain shared data source so table and kanboard reflect identical collections and refresh after mutations.

## Deliverables
1. **RR.Blazor**
   - `RKanboard.razor` component + backing partial class.
   - Supporting models/enums in `RR.Blazor.Models`.
   - Adapter abstractions and default in-memory adapter.
   - Storybook/demo page in RR.Blazor (if existing pattern) or docs snippet.
   - Documentation updates: add to `rr-ai-components.json`, create component README, extend `SMART_COMPONENTS_ARCHITECTURE.md`.
2. **Reference Client**
   - Updated `Issues.razor` with view toggle and board integration.
   - Mapper from `EmployeeIssueModel` → `RKanboardCardModel`.
   - State rules (column definitions, auto-assign logic).
   - Assignment picker wired to `IssuesService`.
   - Optimistic update with failure rollback + toast messaging.

## Non-Goals / Constraints
- No database migrations in this iteration.
- Do not reimplement search/filter; reuse existing Issues filters and feed them into board pipeline.
- No offline sync; adapters are online-first.
- Employees without manager privileges remain table-only.

## Implementation Phases
1. **Foundation**
   - Define models, enums, context objects.
   - Build static board layout + templating slots.
   - Implement drag-and-drop skeleton with JS interop (HTML5 DnD or Pointer events).
2. **Behavior**
   - Wire callbacks, optimistic state manager, adapter contract.
   - Add assignment picker and auto-assign rule engine.
   - Implement orientation switch, column hide, user prefs.
3. **Integration**
   - Update Issues page, map data, persist preferences.
   - Add board-specific loaders, empty states, and error handling.
   - Ensure table/board stay in sync on issue CRUD.
4. **Polish**
   - Document component API.
   - Add unit tests for state manager + auto-assign rules.
   - Provide demo usage snippet in docs.

## Testing Strategy
- **Unit Tests (RR.Blazor.Tests)**: Validate state manager transitions, auto-assign engine, adapter execution.
- **Component Smoke Tests**: BUnit tests with mock adapters ensuring drag callback invocations, orientation toggles, column hide states.
- **Sync Verification**: Tests that switch between table and kanboard views to confirm shared query results stay aligned after drag mutations.
- **Reference Client Tests**: Add Issue board integration tests using BUnit/Moq for service calls, verifying table ↔ board toggling and assignment flows.
- **Playwright E2E**: Extend Issues scenario to drag card between columns, assert state change + assignee update.

## Documentation & Handoff
- Update `RR.Blazor/_Documentation/RRAI.md` references and list RKanboard capabilities.
- Produce usage guide describing adapter implementation checklist and reference implementation wiring.
- Record known extension points (swimlanes, analytics overlays) for future roadmap.

## Internal Review Notes
- Verified Trello-level requirements: column hide, card reorder, assignment surface, orientation toggle, auto-assign rules.
- Confirmed adapter pipeline supports external integrations and optimistic sync between board/table views.
- Added security hooks for role-restricted drag and read-only fallback.
- Highlighted testing coverage for cross-view synchronization and drag workflows to avoid regression gaps.
