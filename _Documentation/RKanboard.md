# RKanboard Component

## Overview

`<RKanboard>` delivers a reusable RR.Blazor kanban shell with fully managed drag-and-drop, column visibility controls, inline assignee picker, and orientation switching. Cards expose Trello-style enrichments (labels, attachments, watcher avatars, checklist progress) while remaining container-agnostic via `RKanboardBoardState`.

## Key Features

- Vertical or horizontal column layouts with runtime toggling and optional swimlane grouping.
- Column hide/show affordances plus drop enablement per column, including lane-aware column reorder.
- Card drag-and-drop across columns and within a column, emitting rich context through `RKanboardCardDropContext` and `RKanboardCardMutationContext`.
- Inline autosuggest picker (via `AssigneeOptions`/`AssigneeSearch`) with optional unassign button.
- Inline quick editors for priority and comment workflows rendered inside the card (no modal context switching required).
- Trello-style metadata surface: labels, badges, checklist progress, attachment pills, and watcher avatars.
- Size/variant/density knobs align the board with RR visual language while recoloring drop states through accent variants.
- Auto-scroll containers while dragging near edges plus batched analytics via `OnCardBulkUpdate`, with opt-in column creation shell, column templates, and column visibility overrides.

## Basic Usage

```razor
@using RR.Blazor.Components.Workflow
@using RR.Blazor.Models
@using RR.Blazor.Enums

<RKanboard Board="@boardState"
           Orientation="@orientation"
           AllowHorizontal="true"
           AllowOrientationToggle="true"
           AllowColumnHide="true"
           AllowUnassign="true"
           EnableSwimlanes="false"
           Size="SizeType.Medium"
           Variant="VariantType.Primary"
           Density="DensityType.Normal"
           AssigneeOptions="@assignees"
           AssigneeSearch="SearchDirectoryAsync"
           AutoAssignRules="@autoAssignRules"
           OnCardDrop="HandleDropAsync"
           OnAssigneeChanged="HandleAssigneeChangedAsync"
           OnCardEditRequested="HandleEditAsync"
           OnCardPriorityRequested="HandlePriorityAsync"
           OnCardCommentRequested="HandleCommentAsync"
           OnCardBulkUpdate="TrackMutationsAsync"
           OrientationChanged="orientation => orientationPreference = orientation" />
```

## Essential Models

- `RKanboardBoardState` – wraps `IReadOnlyList<RKanboardColumnModel>` plus `IReadOnlyDictionary<string, RKanboardCardModel>`.
- `RKanboardColumnModel` – defines column metadata, description, and ordered `CardIds`.
- `RKanboardCardModel` – provides card display text, labels, attachments, watchers, checklist progress, due date, assignment info, and metadata.
- `RKanboardLabelModel` / `RKanboardBadgeModel` / `RKanboardChecklistModel` / `RKanboardAttachmentModel` / `RKanboardParticipantModel` – supporting records that deliver Trello/Notion parity primitives.
- `RKanboardAssigneeOption` – directory entry for inline autosuggest picker.
- `RKanboardCardDropContext` / `RKanboardAssigneeChangedContext` – emitted from drag/assignment events.
- `RKanboardSwimlaneRenderContext` – grouping context passed to the swimlane header template.
- `RKanboardCardMutationContext` + `KanboardCardMutationType` – describe batched card mutations emitted through `OnCardBulkUpdate`.

## Callback Expectations

| Callback | Purpose |
|----------|---------|
| `OnCardDrop` | Persist status/order change. Use `SuggestedAssigneeId` for auto-assign rules. |
| `OnCardBulkUpdate` | Capture aggregated mutations (move, assignment, quick actions) for analytics/audit pipelines. |
| `OnAssigneeChanged` | Persist manual assignment changes emitted from inline picker. |
| `OnCardEditRequested` | Launch edit surface or modal when inline edit quick action is invoked. |
| `OnCardPriorityRequested` | Trigger priority change workflow when the flag quick action is used. |
| `OnCardCommentRequested` | Open comment composer when the chat quick action is pressed. |
| `OrientationChanged` | Store user preference or trigger responsive adjustments. |
| `OnCardOpen` | Launch detail modal or navigation when quick-open action triggered. |

## Layout & Styling

- Top-level element honours `Size`, `Variant`, and `Density` so boards align with application-wide theming.
- Accent-driven variables (`--rkanboard-accent`) recolor drop indicators, focus rings, and call-to-action buttons per variant.
- Columns use RR utility classes (glass/light surfaces) and respect `KanboardColumnConfig.ColorClass`, while swimlane wrappers add optional headers.
- Drop zones and cards ship with sensible defaults – extend via SCSS overrides or slot templates when needed.

## Customization Highlights

- **Orientation & Swimlanes** – set `EnableSwimlanes` plus optional `SwimlaneHeaderTemplate` for Trello-like lanes.
- **Quick Actions** – toggle `ShowEditAction`, `ShowPriorityAction`, `ShowCommentAction` and wire respective callbacks; use `CardTemplate` to layer inline editors or dashboards beneath the stock layout.
- **Analytics** – subscribe to `OnCardBulkUpdate` for consolidated mutation telemetry across drag/assign/action flows.
- **Security** – supply `CanDragEvaluator` to enforce role-based interaction rules per card/column pair.
- **Templates & Shortcuts** – feed `ColumnTemplates` for one-click column creation and press `Ctrl`+`Shift`+`N` to open the creation drawer from the keyboard. Persist custom columns and board preferences via `IRKanboardPreferenceStore` for user-specific layouts.

## Integration Tips

1. Build `RKanboardBoardState` from your domain models (statuses → columns, records → cards).
2. Wire drag callback to your persistence layer; optionally use `RKanboardStateManager` for optimistic updates.
3. Feed `AssigneeOptions` with directory data; implement `AssigneeSearch` for large datasets.
4. Use column `AllowDrop` and `KanboardColumnConfig.Metadata` to enforce state restrictions, SLA indicators, or analytics overlays.
5. Persist and hydrate custom columns through `IRKanboardPreferenceStore`; the PayrollAI client supplies a `ServerKanboardPreferenceStore` that calls the new `KanboardPreferencesController`.
6. Combine with `ModalService` or navigation to surface details when `OnCardOpen` fires.
7. For prototypes or storybook demos, register `RKanboardInMemoryAdapter` with `RKanboardAdapterOrchestrator` to exercise drag/drop and quick actions without wiring a backend.

## Related Helpers

- `RKanboardStateManager` (RR.Blazor.Services) – mutation helpers for optimistic UI flows.
- `RKanboardAutoAssignRule` – delegate used to suggest assignee IDs during drag operations.
