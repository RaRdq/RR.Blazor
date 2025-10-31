"use strict";

const dashboards = new Map();

const BREAKPOINTS = {
    xs: 0,
    sm: 640,
    md: 768,
    lg: 1024,
    xl: 1280
};

function getOption(options, key) {
    if (!options) {
        return undefined;
    }

    if (Object.prototype.hasOwnProperty.call(options, key)) {
        return options[key];
    }

    const pascalKey = key.charAt(0).toUpperCase() + key.slice(1);
    if (Object.prototype.hasOwnProperty.call(options, pascalKey)) {
        return options[pascalKey];
    }

    return undefined;
}

function determineColumns(options) {
    const width = window.innerWidth || document.documentElement.clientWidth;
    const baseColumns = getOption(options, "columns") || 6;

    if (width >= BREAKPOINTS.xl) {
        return getOption(options, "columnsXl") || baseColumns;
    }

    if (width >= BREAKPOINTS.lg) {
        return getOption(options, "columnsLg") || baseColumns;
    }

    if (width >= BREAKPOINTS.md) {
        return getOption(options, "columnsMd") || Math.min(baseColumns, 4);
    }

    if (width >= BREAKPOINTS.sm) {
        return getOption(options, "columnsSm") || 2;
    }

    return getOption(options, "columnsXs") || 1;
}

function computeColumnWidth(state) {
    const rootRect = state.root.getBoundingClientRect();
    const columns = Math.max(determineColumns(state.options), 1);
    return rootRect.width / columns;
}

function computeRowHeight(state) {
    const computed = getComputedStyle(state.root);
    const value = computed.getPropertyValue("--r-dashboard-row-height");
    const parsed = parseFloat(value);
    if (!parsed || Number.isNaN(parsed)) {
        return 120;
    }
    return parsed;
}

function clamp(value, min, max) {
    if (value < min) return min;
    if (value > max) return max;
    return value;
}

function getOrderedWidgets(state) {
    return Array.from(state.widgets.values())
        .sort((a, b) => a.order - b.order);
}

function updateWidgetDimensions(widget) {
    if (!widget || !widget.element) {
        return;
    }

    widget.element.style.gridColumnEnd = `span ${widget.columnSpan}`;
    widget.element.style.gridRowEnd = `span ${widget.rowSpan}`;
    widget.element.style.setProperty("--r-widget-span-columns", widget.columnSpan);
    widget.element.style.setProperty("--r-widget-span-rows", widget.rowSpan);
    widget.element.dataset.currentColumns = widget.columnSpan;
    widget.element.dataset.currentRows = widget.rowSpan;
}

function clearDragIndicators(state) {
    state.root.querySelectorAll(".r-widget--drop-target").forEach(el => {
        el.classList.remove("r-widget--drop-target");
    });
}

function registerGlobalResizeHandler() {
    if (window.__rrDashboardResizeRegistered) {
        return;
    }

    window.addEventListener("resize", () => {
        dashboards.forEach(state => {
            state.columnWidth = computeColumnWidth(state);
            state.rowHeight = computeRowHeight(state);
        });
    });

    window.__rrDashboardResizeRegistered = true;
}

function bindWidgetInteractions(state, widget) {
    if (!widget || !widget.element) {
        return;
    }

    const dragHandle = widget.element.querySelector(".r-widget__drag-handle");
    const resizeHandle = widget.element.querySelector(".r-widget__resize-handle");

    if (widget.handlers && widget.handlers.cleanup) {
        widget.handlers.cleanup();
    }

    widget.handlers = {
        cleanup() {
            if (dragHandle && widget.handlers?.dragStart) {
                dragHandle.removeEventListener("pointerdown", widget.handlers.dragStart);
            }

            if (resizeHandle && widget.handlers?.resizeStart) {
                resizeHandle.removeEventListener("pointerdown", widget.handlers.resizeStart);
            }
        }
    };

    const isEditing = !!getOption(state.options, "editMode");

    if (isEditing && widget.allowReorder && dragHandle) {
        widget.handlers.dragStart = event => startDrag(event, state, widget);
        dragHandle.addEventListener("pointerdown", widget.handlers.dragStart);
    }

    if (isEditing && widget.allowResize && resizeHandle) {
        widget.handlers.resizeStart = event => startResize(event, state, widget);
        resizeHandle.addEventListener("pointerdown", widget.handlers.resizeStart);
    }
}

function startResize(event, state, widget) {
    event.preventDefault();
    event.stopPropagation();

    const handle = event.currentTarget;
    if (!handle.setPointerCapture) {
        return;
    }

    handle.setPointerCapture(event.pointerId);

    const columnWidth = state.columnWidth || computeColumnWidth(state);
    const rowHeight = state.rowHeight || computeRowHeight(state);

    const start = {
        x: event.clientX,
        y: event.clientY,
        columns: widget.columnSpan,
        rows: widget.rowSpan
    };

    widget.element.classList.add("r-widget--resizing");

    const onPointerMove = moveEvent => {
        const deltaX = moveEvent.clientX - start.x;
        const deltaY = moveEvent.clientY - start.y;

        let nextColumns = start.columns + Math.round(deltaX / columnWidth);
        let nextRows = start.rows + Math.round(deltaY / rowHeight);

        nextColumns = clamp(nextColumns, widget.minColumns, widget.maxColumns);
        nextRows = clamp(nextRows, widget.minRows, widget.maxRows);

        if (nextColumns !== widget.columnSpan || nextRows !== widget.rowSpan) {
            widget.columnSpan = nextColumns;
            widget.rowSpan = nextRows;
            updateWidgetDimensions(widget);
        }
    };

    const finalize = endEvent => {
        handle.releasePointerCapture(event.pointerId);
        window.removeEventListener("pointermove", onPointerMove);
        window.removeEventListener("pointerup", finalize);
        window.removeEventListener("pointercancel", finalize);
        widget.element.classList.remove("r-widget--resizing");

        if (widget.columnSpan !== start.columns || widget.rowSpan !== start.rows) {
            if (state.dotNetRef) {
                state.dotNetRef.invokeMethodAsync("OnWidgetResized", widget.id, widget.columnSpan, widget.rowSpan);
            }
        } else {
            updateWidgetDimensions(widget);
        }
    };

    window.addEventListener("pointermove", onPointerMove, { passive: true });
    window.addEventListener("pointerup", finalize, { passive: true });
    window.addEventListener("pointercancel", finalize, { passive: true });
}

function determineDropTarget(state, draggingId, pointerX, pointerY) {
    const ordered = getOrderedWidgets(state);
    const others = ordered.filter(entry => entry.id !== draggingId);

    let insertIndex = others.length;
    let targetElement = null;

    for (let index = 0; index < others.length; index++) {
        const entry = others[index];
        const rect = entry.element.getBoundingClientRect();
        const midpointY = rect.top + rect.height / 2;

        if (pointerY < midpointY) {
            insertIndex = index;
            targetElement = entry.element;
            break;
        }
    }

    if (!targetElement && others.length > 0) {
        targetElement = others[others.length - 1].element;
    }

    return {
        index: insertIndex,
        element: targetElement
    };
}

function startDrag(event, state, widget) {
    event.preventDefault();
    event.stopPropagation();

    const handle = event.currentTarget;
    if (!handle.setPointerCapture) {
        return;
    }

    handle.setPointerCapture(event.pointerId);

    const widgetRect = widget.element.getBoundingClientRect();
    const start = {
        x: event.clientX,
        y: event.clientY,
        order: widget.order,
        offsetX: event.clientX - widgetRect.left,
        offsetY: event.clientY - widgetRect.top
    };

    widget.element.classList.add("r-widget--dragging");
    widget.element.style.willChange = "transform";

    let currentDropIndex = widget.order;
    let currentDropElement = null;

    const onPointerMove = moveEvent => {
        const translateX = moveEvent.clientX - start.x;
        const translateY = moveEvent.clientY - start.y;

        widget.element.style.transform = `translate(${translateX}px, ${translateY}px)`;

        const dropTarget = determineDropTarget(state, widget.id, moveEvent.clientX, moveEvent.clientY);

        if (dropTarget.index !== currentDropIndex) {
            clearDragIndicators(state);

            currentDropIndex = dropTarget.index;
            currentDropElement = dropTarget.element;

            if (currentDropElement) {
                currentDropElement.classList.add("r-widget--drop-target");
            }
        }
    };

    const finalize = () => {
        handle.releasePointerCapture(event.pointerId);
        window.removeEventListener("pointermove", onPointerMove);
        window.removeEventListener("pointerup", finalize);
        window.removeEventListener("pointercancel", finalize);

        widget.element.classList.remove("r-widget--dragging");
        widget.element.classList.remove("r-widget--drop-target");
        widget.element.style.transform = "";
        widget.element.style.willChange = "";
        clearDragIndicators(state);

        if (currentDropElement) {
            currentDropElement.classList.remove("r-widget--drop-target");
        }

        if (currentDropIndex !== widget.order && state.dotNetRef) {
            state.dotNetRef.invokeMethodAsync("OnWidgetReordered", widget.id, currentDropIndex);
        }
    };

    window.addEventListener("pointermove", onPointerMove, { passive: true });
    window.addEventListener("pointerup", finalize, { passive: true });
    window.addEventListener("pointercancel", finalize, { passive: true });
}

function refreshWidgets(state) {
    if (!state || !state.root) {
        return;
    }

    const widgetOptions = getOption(state.options, "widgets") || [];

    widgetOptions.forEach(widgetInfo => {
        const element = state.root.querySelector(`[data-widget-id="${widgetInfo.id}"]`);
        if (!element) {
            return;
        }

        const widgetState = {
            id: widgetInfo.id,
            element,
            allowResize: widgetInfo.allowResize,
            allowReorder: widgetInfo.allowReorder,
            minColumns: widgetInfo.minColumnSpan,
            maxColumns: widgetInfo.maxColumnSpan,
            minRows: widgetInfo.minRowSpan,
            maxRows: widgetInfo.maxRowSpan,
            responsiveColumns: widgetInfo.responsiveColumns || {},
            responsiveRows: widgetInfo.responsiveRows || {},
            columnSpan: widgetInfo.columnSpan,
            rowSpan: widgetInfo.rowSpan,
            order: widgetInfo.order
        };

        state.widgets.set(widgetInfo.id, widgetState);

        element.dataset.currentColumns = widgetInfo.columnSpan;
        element.dataset.currentRows = widgetInfo.rowSpan;
        element.style.order = widgetInfo.order;
        updateWidgetDimensions(widgetState);

        bindWidgetInteractions(state, widgetState);
    });

    const widgetIds = widgetOptions.map(w => w.id);
    Array.from(state.widgets.keys()).forEach(id =>
    {
        if (!widgetIds.includes(id)) {
            const stale = state.widgets.get(id);
            if (stale?.handlers?.cleanup) {
                stale.handlers.cleanup();
            }
            state.widgets.delete(id);
        }
    });
}

function ensureState(dashboardId) {
    const state = dashboards.get(dashboardId);
    if (!state) {
        throw new Error(`Dashboard '${dashboardId}' is not initialized`);
    }
    return state;
}

export function initialize(dashboardId, dotNetRef, options) {
    const root = document.getElementById(dashboardId);
    if (!root) {
        return false;
    }

    const state = {
        root,
        dotNetRef,
        options: options || {},
        widgets: new Map(),
        columnWidth: 0,
        rowHeight: 0
    };

    dashboards.set(dashboardId, state);
    registerGlobalResizeHandler();

    state.columnWidth = computeColumnWidth(state);
    state.rowHeight = computeRowHeight(state);
    refreshWidgets(state);
    return true;
}

export function update(dashboardId, dotNetRef, options) {
    const state = ensureState(dashboardId);
    state.dotNetRef = dotNetRef || state.dotNetRef;
    state.options = Object.assign({}, state.options, options || {});
    state.columnWidth = computeColumnWidth(state);
    state.rowHeight = computeRowHeight(state);
    refreshWidgets(state);
    return true;
}

export function dispose(dashboardId) {
    const state = dashboards.get(dashboardId);
    if (!state) {
        return;
    }

    state.widgets.forEach(widget => {
        if (widget.handlers?.cleanup) {
            widget.handlers.cleanup();
        }
    });

    dashboards.delete(dashboardId);
}

export default {
    initialize,
    update,
    dispose
};
