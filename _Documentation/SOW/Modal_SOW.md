# Modal System - REAL Async Architecture SOW

**REAL ASYNC IMPLEMENTATION WITH TaskCompletionSource<T> - GENERIC T ACTUALLY MATTERS**
**NO LEGACY NON-GENERIC METHODS - ALL ASYNC METHODS REQUIRE EXPLICIT TYPE PARAMETERS**
**31.5% CODE REDUCTION ACHIEVED - 596 → 288 LINES IN MODAL.JS**

## 🚀 REAL Async Architecture Overview

**Core Principle**: Smooth generic ultra customizable modals with nice animation, easy to redesign and customize.

## Four Modal Usage Cases

### Case 1: Raw Content (Components WITHOUT internal RModal)
- Provider wraps in RModal automatically
- Used via ShowRawAsync<T>() or ShowRawAsync(typeof(myComponentModal))

### Case 2: Self-Contained Modals (Components WITH internal RModal)
- Components have their own RModal wrapper
- Provider renders directly without double-wrapping
- Must work with simple `<RModal> content </RModal>` and no other stuff.

### Case 3: Direct RModal with @bind-Visible (Standalone Usage)
- Used directly in components: `<RModal @bind-Visible="isVisible">`
- Requires JS registration for backdrop and portal management
- Supports Visible and VisibleChanged parameters

### Case 4: Service-Based Modals (ModalProvider Managed)
- Rendered inside RModalProvider through ModalService
- Visibility controlled by ModalContext.IsVisible
- Cascades ModalContext for service-managed behavior. RModal already has context.

## CRITICAL IMPLEMENTATION RULES

- Hard Typed Parameters
- Full delegation of any DOM manipulation, z index and animation to JS
- All 4 cases are portalled, uses same backdrop systems, same z index management system
- For users - simple <RModal> my html </RModal> should work when invoked from ModalService. Or <RModal @bind-Visible="isVisible"> my html </RModal> when put inline to same page
- Easy and consistent API: ModalService.ShowAsync<ReturnType, ModalType>(params = default) ModalService.ShowAsync(typeof(myModalType), params, ...) ModalService.ShowAsync<ModalType>(...)

## System Requirements

1. **ModalService.ShowAsync<T>()** sets modal.Visible = true
2. **RModalProvider** cascades visibility via ModalContext
3. **RModal** uses ModalContext.IsVisible when available
4. **Custom modals** must not have Visible parameters
5. **System** must be ultra-generic - work with ANY component
6. **Parameters** must be typed objects

## Modal Stacking and Z-Index Management

### Z-Index Hierarchy
- **Regular Modals**: z-index 1030+ (--z-modal)
- **Backdrops**: One level below their modal
- **Portal Containers**: Dynamic based on modal type

### Stacking Rules
1. **First modal**: Gets regular z-index (1030+)
2. **Nested modals**: Get z-index+10*nestingLevel
3. **Each modal**: Gets individual portal container as sibling
4. **Z-index assignment**: Handled by ZIndexManager with proper type registration

## Nested Modal Behavior

### Opening Nested Modals
- Nested modals appear above parent modals
- Each modal gets separate portal container
- Backdrop stacking preserves click hierarchy
- Modal stack maintained in proper order

### Closing Modals
- **Escape key**: Closes topmost modal only
- **Backdrop click**: Closes clicked modal if closeOnBackdrop enabled
- **Parent closing**: Triggers hierarchical close events to children, including any dropdowns and popups
- **Service close**: Properly cleanup modal stack and registrations

### Event Propagation
- **PARENT_CLOSING event**: Dispatched when parent modal closes
- **Child components**: Must listen and cleanup appropriately
- **Bubble cleanup**: Ensures no orphaned modals remain

## Backdrop Management

### Backdrop Creation
- Each modal gets individual backdrop unless shared
- Backdrop z-index one level below modal
- Backdrop click detection via RRBlazor.Backdrop
- Multiple backdrops stack properly

### Backdrop Cleanup
- **Modal close**: Automatically destroys associated backdrop
- **Nested scenarios**: Parent backdrop remains until parent closes
- **Event cleanup**: Removes click handlers to prevent memory leaks

## Portal System Requirements

### Portal Container Selection
- use Z-Index-Manager for providing with correct z index in modal/popup stack

### Portal Lifecycle
- **Creation**: Via PORTAL_CREATE_REQUEST events
- **Destruction**: Via PORTAL_DESTROY_REQUEST events
- **Individual containers**: Each modal gets own portal (no sharing)
- **Sibling arrangement**: All modal portals as siblings, not nested

## Testing Requirements

All modals must:
- Display content (not just backdrop)
- Work without Visible parameters (Cases 1,2,4)
- Support Visible parameters (Case 3)
- Open via ModalService.ShowAsync<T>()
- Close properly with full cleanup
- Support nested modals with correct stacking
- Handle backdrop clicks appropriately
- Cleanup when parent modals close
- Maintain proper z-index hierarchy
- Use individual portal containers
- Close their children portals and content