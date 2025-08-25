import { getInstance as getPortalManager } from './portal.js';

class PivotManager {
    constructor() {
        this.activePivots = new Map();
        this.dragState = {
            dragging: false,
            draggedField: null,
            draggedElement: null,
            sourceArea: null,
            targetArea: null
        };
        
        this.initializeEventDelegation();
        this.setupDragAndDrop();
    }

    initializeEventDelegation() {
        if (!document.getElementById('pivot-event-root')) {
            const eventRoot = document.createElement('div');
            eventRoot.id = 'pivot-event-root';
            eventRoot.style.cssText = 'position: absolute; pointer-events: none; z-index: -1;';
            document.body.appendChild(eventRoot);
        }

        // Global event delegation for all pivot components
        document.addEventListener('click', this.handleGlobalClick.bind(this), true);
        document.addEventListener('mouseenter', this.handleGlobalMouseEnter.bind(this), true);
        document.addEventListener('mouseleave', this.handleGlobalMouseLeave.bind(this), true);
    }

    setupDragAndDrop() {
        document.addEventListener('dragstart', this.handleDragStart.bind(this));
        document.addEventListener('dragend', this.handleDragEnd.bind(this));
        document.addEventListener('dragover', this.handleDragOver.bind(this));
        document.addEventListener('dragenter', this.handleDragEnter.bind(this));
        document.addEventListener('dragleave', this.handleDragLeave.bind(this));
        document.addEventListener('drop', this.handleDrop.bind(this));
    }

    register(pivotId, config = {}) {
        if (this.activePivots.has(pivotId)) {
            return this.activePivots.get(pivotId);
        }

        const pivotData = {
            id: pivotId,
            element: document.getElementById(pivotId),
            config: {
                enableDragDrop: true,
                enableSorting: true,
                enableDrilldown: true,
                performanceMode: true,
                virtualization: true,
                ...config
            },
            fieldAreas: new Map(),
            sortState: new Map(),
            expandedRows: new Set(),
            virtualizedRows: new Map(),
            lastUpdate: Date.now()
        };

        if (!pivotData.element) {
            throw new Error(`Pivot element with ID '${pivotId}' not found`);
        }

        this.activePivots.set(pivotId, pivotData);
        this.setupPivotOptimizations(pivotData);
        return pivotData;
    }

    unregister(pivotId) {
        const pivotData = this.activePivots.get(pivotId);
        if (pivotData) {
            this.cleanupPivotOptimizations(pivotData);
            this.activePivots.delete(pivotId);
        }
    }

    setupPivotOptimizations(pivotData) {
        if (!pivotData.config.performanceMode) return;

        const tableWrapper = pivotData.element.querySelector('.pivot-table-wrapper');
        if (tableWrapper && pivotData.config.virtualization) {
            this.enableVirtualization(pivotData, tableWrapper);
        }

        // Enable optimistic updates
        this.enableOptimisticUpdates(pivotData);
    }

    enableVirtualization(pivotData, tableWrapper) {
        const virtualConfig = {
            rowHeight: 32,
            bufferRows: 10,
            containerHeight: tableWrapper.clientHeight || 600
        };

        const visibleRows = Math.ceil(virtualConfig.containerHeight / virtualConfig.rowHeight) + virtualConfig.bufferRows;
        
        pivotData.virtualization = {
            config: virtualConfig,
            visibleRows,
            scrollTop: 0,
            startIndex: 0,
            endIndex: visibleRows
        };

        // Setup scroll handler with throttling
        let scrollTimeout;
        tableWrapper.addEventListener('scroll', () => {
            if (scrollTimeout) clearTimeout(scrollTimeout);
            scrollTimeout = setTimeout(() => {
                this.updateVirtualization(pivotData, tableWrapper);
            }, 16); // ~60fps
        });
    }

    updateVirtualization(pivotData, tableWrapper) {
        const { config, visibleRows } = pivotData.virtualization;
        const scrollTop = tableWrapper.scrollTop;
        const startIndex = Math.floor(scrollTop / config.rowHeight);
        const endIndex = Math.min(startIndex + visibleRows, pivotData.totalRows || 1000);

        pivotData.virtualization.scrollTop = scrollTop;
        pivotData.virtualization.startIndex = startIndex;
        pivotData.virtualization.endIndex = endIndex;

        // Dispatch event to Blazor
        this.dispatchPivotEvent('virtualization-update', pivotData.id, {
            startIndex,
            endIndex,
            visibleRows
        });
    }

    enableOptimisticUpdates(pivotData) {
        pivotData.optimisticUpdates = {
            pending: new Set(),
            failed: new Set(),
            rollbackQueue: []
        };
    }

    handleGlobalClick(event) {
        const pivotElement = this.findPivotContainer(event.target);
        if (!pivotElement) return;

        const pivotId = pivotElement.id;
        const pivotData = this.activePivots.get(pivotId);
        if (!pivotData) return;

        // Handle sort clicks
        if (event.target.closest('.pivot-column-header, .pivot-row-header')) {
            this.handleSortClick(event, pivotData);
            return;
        }

        // Handle cell clicks
        if (event.target.closest('.pivot-data-cell')) {
            this.handleCellClick(event, pivotData);
            return;
        }

        // Handle expand/collapse clicks
        if (event.target.closest('.pivot-expand-button')) {
            this.handleExpandClick(event, pivotData);
            return;
        }

        // Handle drill-through clicks
        if (event.target.closest('.pivot-drill-button')) {
            this.handleDrillClick(event, pivotData);
            event.stopPropagation();
            return;
        }
    }

    handleSortClick(event, pivotData) {
        if (!pivotData.config.enableSorting) return;

        const header = event.target.closest('.pivot-column-header, .pivot-row-header');
        const field = header.dataset.field;
        if (!field) return;

        // Optimistic update - immediately update UI
        this.applySortVisualUpdate(header, field, pivotData);

        // Dispatch to Blazor
        this.dispatchPivotEvent('sort-click', pivotData.id, {
            field,
            currentSort: pivotData.sortState.get(field) || 'none'
        });
    }

    applySortVisualUpdate(header, field, pivotData) {
        const currentSort = pivotData.sortState.get(field) || 'none';
        const newSort = currentSort === 'asc' ? 'desc' : currentSort === 'desc' ? 'none' : 'asc';
        
        pivotData.sortState.set(field, newSort);

        // Update visual indicators
        const sortIndicator = header.querySelector('.pivot-sort-indicator');
        if (sortIndicator) {
            sortIndicator.className = `pivot-sort-indicator pivot-sort-${newSort}`;
            const icon = sortIndicator.querySelector('.material-symbols-rounded');
            if (icon) {
                icon.textContent = newSort === 'asc' ? 'arrow_upward' : 
                                  newSort === 'desc' ? 'arrow_downward' : 'unfold_more';
            }
        }

        // Add to pending updates
        if (pivotData.optimisticUpdates) {
            pivotData.optimisticUpdates.pending.add(`sort-${field}`);
        }
    }

    handleCellClick(event, pivotData) {
        const cell = event.target.closest('.pivot-data-cell');
        const rowValue = cell.dataset.row;
        const columnValue = cell.dataset.column;
        const field = cell.dataset.field;

        this.dispatchPivotEvent('cell-click', pivotData.id, {
            row: rowValue,
            column: columnValue,
            field: field,
            element: cell
        });
    }

    handleExpandClick(event, pivotData) {
        if (!pivotData.config.enableDrilldown) return;

        const button = event.target.closest('.pivot-expand-button');
        const row = button.closest('.pivot-data-row');
        const rowValue = row.dataset.value;

        // Optimistic expand/collapse
        const isExpanded = pivotData.expandedRows.has(rowValue);
        if (isExpanded) {
            pivotData.expandedRows.delete(rowValue);
            button.querySelector('.material-symbols-rounded').textContent = 'chevron_right';
        } else {
            pivotData.expandedRows.add(rowValue);
            button.querySelector('.material-symbols-rounded').textContent = 'expand_more';
        }

        this.dispatchPivotEvent('expand-toggle', pivotData.id, {
            row: rowValue,
            expanded: !isExpanded
        });
    }

    handleDrillClick(event, pivotData) {
        const cell = event.target.closest('.pivot-data-cell');
        const rowValue = cell.dataset.row;
        const columnValue = cell.dataset.column;
        const field = cell.dataset.field;

        this.dispatchPivotEvent('drill-through', pivotData.id, {
            row: rowValue,
            column: columnValue,
            field: field
        });
    }

    handleDragStart(event) {
        const fieldElement = event.target.closest('.pivot-available-field, .pivot-field-chip');
        if (!fieldElement) return;

        const pivotContainer = this.findPivotContainer(fieldElement);
        if (!pivotContainer) return;

        this.dragState.dragging = true;
        this.dragState.draggedElement = fieldElement;
        this.dragState.draggedField = fieldElement.dataset.field || fieldElement.textContent.trim();
        this.dragState.sourceArea = this.findFieldArea(fieldElement);

        // Visual feedback
        fieldElement.classList.add('pivot-field-dragging');
        document.body.classList.add('pivot-drag-active');

        event.dataTransfer.effectAllowed = 'move';
        event.dataTransfer.setData('text/plain', this.dragState.draggedField);
    }

    handleDragEnd(event) {
        if (!this.dragState.dragging) return;

        // Cleanup visual feedback
        if (this.dragState.draggedElement) {
            this.dragState.draggedElement.classList.remove('pivot-field-dragging');
        }
        document.body.classList.remove('pivot-drag-active');
        document.querySelectorAll('.pivot-drop-zone-active').forEach(zone => {
            zone.classList.remove('pivot-drop-zone-active');
        });

        // Reset drag state
        this.dragState = {
            dragging: false,
            draggedField: null,
            draggedElement: null,
            sourceArea: null,
            targetArea: null
        };
    }

    handleDragOver(event) {
        if (!this.dragState.dragging) return;

        const dropZone = event.target.closest('.pivot-field-drop-zone');
        if (dropZone) {
            event.preventDefault();
            event.dataTransfer.dropEffect = 'move';
        }
    }

    handleDragEnter(event) {
        if (!this.dragState.dragging) return;

        const dropZone = event.target.closest('.pivot-field-drop-zone');
        if (dropZone) {
            dropZone.classList.add('pivot-drop-zone-active');
            this.dragState.targetArea = this.getAreaFromDropZone(dropZone);
        }
    }

    handleDragLeave(event) {
        if (!this.dragState.dragging) return;

        const dropZone = event.target.closest('.pivot-field-drop-zone');
        if (dropZone && !dropZone.contains(event.relatedTarget)) {
            dropZone.classList.remove('pivot-drop-zone-active');
        }
    }

    handleDrop(event) {
        if (!this.dragState.dragging) return;

        const dropZone = event.target.closest('.pivot-field-drop-zone');
        if (!dropZone) return;

        event.preventDefault();
        
        const pivotContainer = this.findPivotContainer(dropZone);
        if (!pivotContainer) return;

        const pivotData = this.activePivots.get(pivotContainer.id);
        if (!pivotData) return;

        const targetArea = this.getAreaFromDropZone(dropZone);

        // Optimistic update - immediately move the field visually
        this.optimisticallyMoveField(this.dragState, targetArea, dropZone);

        // Dispatch to Blazor for server-side processing
        this.dispatchPivotEvent('field-drop', pivotContainer.id, {
            field: this.dragState.draggedField,
            sourceArea: this.dragState.sourceArea,
            targetArea: targetArea
        });
    }

    optimisticallyMoveField(dragState, targetArea, dropZone) {
        if (!dragState.draggedElement) return;

        // Clone the dragged element for immediate feedback
        const clone = dragState.draggedElement.cloneNode(true);
        clone.classList.remove('pivot-field-dragging');
        clone.classList.add('pivot-field-optimistic');

        // Add to drop zone
        const placeholder = dropZone.querySelector('.pivot-drop-placeholder');
        if (placeholder) {
            placeholder.style.display = 'none';
        }
        
        dropZone.appendChild(clone);

        // Remove from source if moving between areas
        if (dragState.sourceArea !== targetArea) {
            dragState.draggedElement.style.opacity = '0.5';
            dragState.draggedElement.classList.add('pivot-field-pending-removal');
        }
    }

    rollbackOptimisticUpdate(pivotId, operation, data) {
        const pivotData = this.activePivots.get(pivotId);
        if (!pivotData || !pivotData.optimisticUpdates) return;

        // Remove from pending
        pivotData.optimisticUpdates.pending.delete(operation);
        pivotData.optimisticUpdates.failed.add(operation);

        // Specific rollback logic
        switch (operation.split('-')[0]) {
            case 'sort':
                this.rollbackSortUpdate(pivotData, data);
                break;
            case 'field':
                this.rollbackFieldMove(pivotData, data);
                break;
        }
    }

    rollbackSortUpdate(pivotData, data) {
        const header = pivotData.element.querySelector(`[data-field="${data.field}"]`);
        if (header) {
            const sortIndicator = header.querySelector('.pivot-sort-indicator');
            if (sortIndicator) {
                // Restore previous state
                const previousSort = data.previousSort || 'none';
                sortIndicator.className = `pivot-sort-indicator pivot-sort-${previousSort}`;
            }
        }
    }

    rollbackFieldMove(pivotData, data) {
        // Remove optimistic elements
        pivotData.element.querySelectorAll('.pivot-field-optimistic').forEach(el => el.remove());
        
        // Restore pending removal elements
        pivotData.element.querySelectorAll('.pivot-field-pending-removal').forEach(el => {
            el.style.opacity = '';
            el.classList.remove('pivot-field-pending-removal');
        });
    }

    findPivotContainer(element) {
        return element.closest('.pivot-grid');
    }

    findFieldArea(element) {
        const area = element.closest('.pivot-field-area');
        return area ? area.dataset.area : null;
    }

    getAreaFromDropZone(dropZone) {
        const area = dropZone.closest('.pivot-field-area');
        return area ? area.dataset.area : null;
    }

    dispatchPivotEvent(eventType, pivotId, data) {
        if (!window.RRBlazor || !window.RRBlazor.EventDispatcher) return;

        window.RRBlazor.EventDispatcher.dispatch(`pivot-${eventType}`, {
            pivotId,
            timestamp: Date.now(),
            ...data
        });
    }

    handleGlobalMouseEnter(event) {
        // Add hover states for better UX
        const field = event.target.closest('.pivot-available-field, .pivot-field-chip');
        if (field) {
            field.classList.add('pivot-field-hover');
        }

        const cell = event.target.closest('.pivot-data-cell');
        if (cell) {
            this.highlightCellContext(cell);
        }
    }

    handleGlobalMouseLeave(event) {
        const field = event.target.closest('.pivot-available-field, .pivot-field-chip');
        if (field) {
            field.classList.remove('pivot-field-hover');
        }

        const cell = event.target.closest('.pivot-data-cell');
        if (cell) {
            this.clearCellContextHighlight(cell);
        }
    }

    highlightCellContext(cell) {
        const table = cell.closest('.pivot-table');
        if (!table) return;

        const rowValue = cell.dataset.row;
        const columnValue = cell.dataset.column;

        // Highlight row and column
        table.querySelectorAll(`[data-row="${rowValue}"]`).forEach(el => {
            el.classList.add('pivot-row-highlight');
        });
        
        table.querySelectorAll(`[data-column="${columnValue}"]`).forEach(el => {
            el.classList.add('pivot-column-highlight');
        });
    }

    clearCellContextHighlight(cell) {
        const table = cell.closest('.pivot-table');
        if (!table) return;

        table.querySelectorAll('.pivot-row-highlight, .pivot-column-highlight').forEach(el => {
            el.classList.remove('pivot-row-highlight', 'pivot-column-highlight');
        });
    }

    // Performance monitoring
    measurePerformance(pivotId, operation, fn) {
        const start = performance.now();
        const result = fn();
        const end = performance.now();
        
        const pivotData = this.activePivots.get(pivotId);
        if (pivotData) {
            pivotData.performanceMetrics = pivotData.performanceMetrics || {};
            pivotData.performanceMetrics[operation] = {
                duration: end - start,
                timestamp: Date.now()
            };
        }
        
        return result;
    }

    getPerformanceStats(pivotId) {
        const pivotData = this.activePivots.get(pivotId);
        return pivotData?.performanceMetrics || {};
    }
}

// Create singleton instance
const pivotManager = new PivotManager();

// Expose to global namespace
window.RRBlazor = window.RRBlazor || {};
window.RRBlazor.Pivot = {
    register: (id, config) => pivotManager.register(id, config),
    unregister: (id) => pivotManager.unregister(id),
    rollback: (id, operation, data) => pivotManager.rollbackOptimisticUpdate(id, operation, data),
    getStats: (id) => pivotManager.getPerformanceStats(id),
    measurePerformance: (id, op, fn) => pivotManager.measurePerformance(id, op, fn)
};

export default pivotManager;