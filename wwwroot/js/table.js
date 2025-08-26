/**
 * Unified table functionality for RTableGeneric
 * Provides column resizing, sticky columns, scroll management, and interactions
 */

const debugLogger = window.debugLogger;

export const RTableManager = {
    
    instances: new Map(),
    scrollManagers: new Map(),
    
    // Configuration
    config: {
        minColumnWidth: 50,
        maxColumnWidth: 500,
        handleWidth: 4,
        resizeDelay: 16, // 60fps throttling
        stickyZIndex: 15,
        animationDuration: 200
    },

    /**
     * Initialize table with all features
     */
    initialize(tableId, options = {}) {
        if (this.instances.has(tableId)) {
            this.cleanup(tableId);
        }

        const table = document.querySelector(`[data-table-id="${tableId}"]`);
        if (!table) {
            debugLogger.warn(`Table with id ${tableId} not found`);
            return false;
        }

        // Merge configuration
        const tableConfig = { ...this.config, ...options };
        
        // Create instance
        const instance = {
            tableId,
            element: table,
            config: tableConfig,
            resizing: new Map(),
            sticky: new Map(),
            observers: new Map(),
            scrollManager: null
        };

        // Initialize features
        this.initializeColumnResizing(instance);
        this.initializeStickyColumns(instance);
        this.initializeAdvancedSelection(instance);
        this.initializeKeyboardNavigation(instance);
        
        // Initialize scroll management
        const tableContainer = table.closest('[data-table-id]');
        const scrollContainer = table.querySelector('.table-content.table-content-scroll-container-x');
        if (tableContainer && scrollContainer) {
            instance.scrollManager = new TableScrollInstance(tableId, tableContainer, scrollContainer);
        }

        this.instances.set(tableId, instance);
        
        // Setup cleanup on disconnect
        this.setupCleanup(instance);
        
        return true;
    },

    /**
     * Column resizing functionality
     */
    initializeColumnResizing(instance) {
        const headers = instance.element.querySelectorAll('.table-header-cell');
        
        headers.forEach((header, index) => {
            // Skip if already has resize handle
            if (header.querySelector('.column-resize-handle')) {
                return;
            }

            // Create resize handle
            const handle = this.createResizeHandle(header, index, instance);
            if (handle) {
                header.appendChild(handle);
                header.classList.add('table-header-resizable');
            }
        });
    },

    /**
     * Create resize handle for column
     */
    createResizeHandle(header, columnIndex, instance) {
        const handle = document.createElement('div');
        handle.className = 'column-resize-handle';
        handle.setAttribute('data-column-index', columnIndex);
        
        let isResizing = false;
        let startX = 0;
        let startWidth = 0;
        let resizeIndicator = null;

        // Mouse events
        handle.addEventListener('mousedown', startResize);
        
        function startResize(e) {
            e.preventDefault();
            e.stopPropagation();
            
            isResizing = true;
            startX = e.clientX;
            startWidth = header.offsetWidth;
            
            // Create resize indicator
            resizeIndicator = createResizeIndicator(e.clientX);
            document.body.appendChild(resizeIndicator);
            
            // Add visual feedback
            handle.classList.add('is-resizing');
            header.classList.add('is-resizing');
            document.body.style.cursor = 'col-resize';
            document.body.style.userSelect = 'none';
            
            // Global mouse events
            document.addEventListener('mousemove', handleResize);
            document.addEventListener('mouseup', endResize);
        }

        const handleResize = (e) => {
            if (!isResizing) return;
            
            const deltaX = e.clientX - startX;
            const newWidth = Math.max(
                instance.config.minColumnWidth,
                Math.min(instance.config.maxColumnWidth, startWidth + deltaX)
            );
            
            // Update resize indicator position
            if (resizeIndicator) {
                resizeIndicator.style.left = `${e.clientX}px`;
            }
            
            // Live resize preview
            if (instance.config.liveResize) {
                this.updateColumnWidth(header, newWidth, columnIndex, instance);
            }
        };

        const endResize = (e) => {
            if (!isResizing) return;
            
            isResizing = false;
            
            const deltaX = e.clientX - startX;
            const newWidth = Math.max(
                instance.config.minColumnWidth,
                Math.min(instance.config.maxColumnWidth, startWidth + deltaX)
            );
            
            // Apply final width
            this.updateColumnWidth(header, newWidth, columnIndex, instance);
            
            // Cleanup
            document.removeEventListener('mousemove', handleResize);
            document.removeEventListener('mouseup', endResize);
            
            if (resizeIndicator) {
                document.body.removeChild(resizeIndicator);
                resizeIndicator = null;
            }
            
            handle.classList.remove('is-resizing');
            header.classList.remove('is-resizing');
            document.body.style.cursor = '';
            document.body.style.userSelect = '';
            
            // Persist width if enabled
            if (instance.config.persistWidths) {
                this.persistColumnWidth(instance.tableId, columnIndex, newWidth);
            }
            
            // Notify Blazor component
            this.notifyColumnResize(instance.tableId, columnIndex, startWidth, newWidth);
        };

        function createResizeIndicator(x) {
            const indicator = document.createElement('div');
            indicator.className = 'column-resize-indicator';
            indicator.style.left = `${x}px`;
            return indicator;
        }

        return handle;
    },

    /**
     * Update column width across all rows
     */
    updateColumnWidth(header, width, columnIndex, instance) {
        const table = header.closest('.table');
        if (!table) return;

        // Update header width
        header.style.width = `${width}px`;
        header.style.minWidth = `${width}px`;
        header.style.maxWidth = `${width}px`;

        // Update all cells in this column
        const rows = table.querySelectorAll('tbody tr');
        rows.forEach(row => {
            const cell = row.children[columnIndex];
            if (cell) {
                cell.style.width = `${width}px`;
                cell.style.minWidth = `${width}px`;
                cell.style.maxWidth = `${width}px`;
            }
        });
    },

    /**
     * Sticky columns functionality
     */
    initializeStickyColumns(instance) {
        const table = instance.element;
        const stickyLeft = table.querySelectorAll('.table-column-sticky-left');
        const stickyRight = table.querySelectorAll('.table-column-sticky-right');
        const scrollContainer = table.closest('.table-scroll-container');
        
        if (!scrollContainer) return;

        // Calculate sticky positions
        this.updateStickyPositions(stickyLeft, 'left');
        this.updateStickyPositions(stickyRight, 'right');
        
        // Setup scroll observer for shadows
        this.setupStickyScrollObserver(scrollContainer, table, instance);
        
        // Recalculate on resize
        const resizeObserver = new ResizeObserver(() => {
            this.updateStickyPositions(stickyLeft, 'left');
            this.updateStickyPositions(stickyRight, 'right');
        });
        
        resizeObserver.observe(table);
        instance.observers.set('sticky-resize', resizeObserver);
    },

    /**
     * Update sticky column positions
     */
    updateStickyPositions(columns, side) {
        let offset = 0;
        
        columns.forEach((column, index) => {
            column.style[side] = `${offset}px`;
            offset += column.offsetWidth;
            
            // Add visual separation
            if (index === columns.length - 1) {
                column.classList.add('show-shadow');
            }
        });
    },

    /**
     * Setup scroll observer for sticky column shadows
     */
    setupStickyScrollObserver(scrollContainer, table, instance) {
        let isScrolling = false;
        
        scrollContainer.addEventListener('scroll', () => {
            if (!isScrolling) {
                isScrolling = true;
                requestAnimationFrame(() => {
                    this.updateScrollShadows(scrollContainer, table);
                    isScrolling = false;
                });
            }
        });
    },

    /**
     * Update sticky column shadow visibility based on scroll position
     */
    updateScrollShadows(scrollContainer, table) {
        const { scrollLeft, scrollWidth, clientWidth } = scrollContainer;
        
        const stickyLeft = table.querySelectorAll('.table-column-sticky-left');
        const stickyRight = table.querySelectorAll('.table-column-sticky-right');
        
        // Left shadows - visible when scrolled right
        const showLeftShadow = scrollLeft > 0;
        stickyLeft.forEach(col => {
            col.classList.toggle('has-scroll-shadow', showLeftShadow);
        });
        
        // Right shadows - visible when not at right edge
        const showRightShadow = scrollLeft < (scrollWidth - clientWidth - 1);
        stickyRight.forEach(col => {
            col.classList.toggle('has-scroll-shadow', showRightShadow);
        });
    },

    /**
     * Advanced selection functionality
     */
    initializeAdvancedSelection(instance) {
        const table = instance.element;
        const checkboxes = table.querySelectorAll('.table-checkbox-advanced input[type="checkbox"]');
        const rows = table.querySelectorAll('tbody tr');
        
        // Advanced checkbox behavior
        checkboxes.forEach(checkbox => {
            // Improve visual feedback
            checkbox.addEventListener('change', (e) => {
                const row = e.target.closest('tr');
                if (row) {
                    row.classList.toggle('multi-selected', e.target.checked);
                    
                    // Animate selection
                    if (e.target.checked) {
                        this.animateRowSelection(row, true, instance);
                    } else {
                        this.animateRowSelection(row, false, instance);
                    }
                }
            });
        });
        
        // Row click selection (if enabled)
        if (instance.config.rowClickSelection) {
            rows.forEach(row => {
                row.addEventListener('click', (e) => {
                    // Skip if clicking on input elements
                    if (e.target.matches('input, button, a, select, textarea')) {
                        return;
                    }
                    
                    const checkbox = row.querySelector('input[type="checkbox"], input[type="radio"]');
                    if (checkbox && !checkbox.disabled) {
                        checkbox.checked = !checkbox.checked;
                        checkbox.dispatchEvent(new Event('change', { bubbles: true }));
                    }
                });
                
                row.classList.add('table-row-clickable');
            });
        }
    },

    /**
     * Animate row selection
     */
    animateRowSelection(row, selected, instance) {
        if (!row) return;
        
        // Respect reduced motion preference
        if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) {
            return;
        }
        
        row.style.transform = selected ? 'scale(1.01)' : 'scale(0.99)';
        
        setTimeout(() => {
            row.style.transform = '';
        }, instance.config.animationDuration);
    },

    /**
     * Keyboard navigation functionality
     */
    initializeKeyboardNavigation(instance) {
        const table = instance.element;
        const tbody = table.querySelector('tbody');
        if (!tbody) return;
        
        let currentRow = 0;
        const rows = Array.from(tbody.querySelectorAll('tr'));
        
        // Make table focusable
        table.setAttribute('tabindex', '0');
        table.classList.add('table-keyboard-navigation');
        
        table.addEventListener('keydown', (e) => {
            switch (e.key) {
                case 'ArrowDown':
                    e.preventDefault();
                    currentRow = Math.min(rows.length - 1, currentRow + 1);
                    this.focusRow(rows[currentRow], rows);
                    break;
                    
                case 'ArrowUp':
                    e.preventDefault();
                    currentRow = Math.max(0, currentRow - 1);
                    this.focusRow(rows[currentRow], rows);
                    break;
                    
                case ' ':
                case 'Enter':
                    e.preventDefault();
                    const checkbox = rows[currentRow]?.querySelector('input[type="checkbox"], input[type="radio"]');
                    if (checkbox && !checkbox.disabled) {
                        checkbox.checked = !checkbox.checked;
                        checkbox.dispatchEvent(new Event('change', { bubbles: true }));
                    }
                    break;
                    
                case 'Escape':
                    table.blur();
                    rows.forEach(row => row.classList.remove('keyboard-selected'));
                    currentRow = 0;
                    break;
            }
        });
    },
    
    focusRow(row, allRows) {
        allRows.forEach(r => r.classList.remove('keyboard-selected'));
        if (row) {
            row.classList.add('keyboard-selected');
            row.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
        }
    },

    /**
     * Notify Blazor component of column resize
     */
    notifyColumnResize(tableId, columnIndex, oldWidth, newWidth) {
        if (window.DotNet && window.DotNetCallbacks) {
            const callback = window.DotNetCallbacks[`${tableId}-resize`];
            if (callback) {
                callback.invokeMethodAsync('OnColumnResized', {
                    columnIndex,
                    oldWidth,
                    newWidth,
                    timestamp: new Date().toISOString()
                });
            }
        }
    },

    /**
     * Persist column width to localStorage
     */
    persistColumnWidth(tableId, columnIndex, width) {
        try {
            const key = `table-${tableId}-column-widths`;
            const stored = JSON.parse(localStorage.getItem(key) || '{}');
            stored[columnIndex] = width;
            localStorage.setItem(key, JSON.stringify(stored));
        } catch (error) {
            debugLogger.warn('Failed to persist column width:', error);
        }
    },

    /**
     * Restore persisted column widths
     */
    restoreColumnWidths(tableId) {
        try {
            const key = `table-${tableId}-column-widths`;
            const stored = JSON.parse(localStorage.getItem(key) || '{}');
            
            Object.entries(stored).forEach(([columnIndex, width]) => {
                const header = document.querySelector(`[data-table-id="${tableId}"] .table-header-cell:nth-child(${parseInt(columnIndex) + 1})`);
                if (header) {
                    const instance = this.instances.get(tableId);
                    if (instance) {
                        this.updateColumnWidth(header, width, parseInt(columnIndex), instance);
                    }
                }
            });
        } catch (error) {
            debugLogger.warn('Failed to restore column widths:', error);
        }
    },

    /**
     * Setup cleanup when table is disconnected
     */
    setupCleanup(instance) {
        const observer = new MutationObserver((mutations) => {
            mutations.forEach((mutation) => {
                mutation.removedNodes.forEach((node) => {
                    if (node === instance.element || (node.nodeType === Node.ELEMENT_NODE && node.contains(instance.element))) {
                        this.cleanup(instance.tableId);
                    }
                });
            });
        });
        
        observer.observe(document.body, { childList: true, subtree: true });
        instance.observers.set('cleanup', observer);
    },

    /**
     * Cleanup resources for a table
     */
    cleanup(tableId) {
        const instance = this.instances.get(tableId);
        if (!instance) return;

        // Clear state
        instance.resizing.clear();
        instance.sticky.clear();
        
        // Dispose scroll manager
        if (instance.scrollManager) {
            instance.scrollManager.dispose();
        }
        
        // Disconnect observers
        instance.observers.forEach((observer) => {
            observer.disconnect();
        });
        instance.observers.clear();
            
        // Remove from instances
        this.instances.delete(tableId);
    },

    /**
     * Update table configuration
     */
    updateConfig(tableId, newConfig) {
        const instance = this.instances.get(tableId);
        if (instance) {
            instance.config = { ...instance.config, ...newConfig };
        }
    },

    /**
     * Get current column widths
     */
    getColumnWidths(tableId) {
        const table = document.querySelector(`[data-table-id="${tableId}"]`);
        if (!table) return [];
        
        const headers = table.querySelectorAll('.table-header-cell');
        return Array.from(headers).map(header => header.offsetWidth);
    },

    /**
     * Set column widths
     */
    setColumnWidths(tableId, widths) {
        const instance = this.instances.get(tableId);
        if (!instance) return;
        
        const headers = instance.element.querySelectorAll('.table-header-cell');
        headers.forEach((header, index) => {
            if (widths[index] !== undefined) {
                this.updateColumnWidth(header, widths[index], index, instance);
            }
        });
    },

    // Refresh scroll shadows for a table
    refresh(tableId) {
        const instance = this.instances.get(tableId);
        if (instance && instance.scrollManager) {
            instance.scrollManager.updateScrollShadows();
            return true;
        }
        return false;
    }
};

/**
 * Table scroll management instance
 */
class TableScrollInstance {
    constructor(tableId, tableContainer, scrollContainer) {
        this.tableId = tableId;
        this.tableContainer = tableContainer;
        this.scrollContainer = scrollContainer;
        this.scrollTimeout = null;
        this.resizeObserver = null;
        
        this.initialize();
    }
    
    initialize() {
        this.handleScroll = this.handleScroll.bind(this);
        this.handleResize = this.handleResize.bind(this);
        this.handleHeaderFocus = this.handleHeaderFocus.bind(this);
        
        this.updateScrollShadows();
        
        this.scrollContainer.addEventListener('scroll', this.handleScroll, { passive: true });
        
        if (window.ResizeObserver) {
            this.resizeObserver = new ResizeObserver(this.handleResize);
            this.resizeObserver.observe(this.scrollContainer);
        } else {
            window.addEventListener('resize', this.handleResize, { passive: true });
        }
        
        this.setupHeaderFocusHandling();
    }
    
    handleScroll() {
        if (this.scrollTimeout) return;
        
        this.scrollTimeout = requestAnimationFrame(() => {
            this.updateScrollShadows();
            this.scrollTimeout = null;
        });
    }
    
    handleResize() {
        if (!this.resizeScheduled) {
            this.resizeScheduled = true;
            requestAnimationFrame(() => {
                this.updateScrollShadows();
                this.resizeScheduled = false;
            });
        }
    }
    
    updateScrollShadows() {
        try {
            const scrollLeft = this.scrollContainer.scrollLeft;
            const scrollWidth = this.scrollContainer.scrollWidth;
            const clientWidth = this.scrollContainer.clientWidth;
            const scrollRight = scrollWidth - clientWidth - scrollLeft;
            
            this.scrollContainer.classList.remove('table-content-scroll-container-x-scrolled-left', 'table-content-scroll-container-x-scrolled-right', 'table-content-scroll-container-x-scrolled-both');
            
            if (scrollLeft > 10 && scrollRight > 10) {
                this.scrollContainer.classList.add('table-content-scroll-container-x-scrolled-both');
            } else if (scrollLeft > 10) {
                this.scrollContainer.classList.add('table-content-scroll-container-x-scrolled-left');
            } else if (scrollRight > 10) {
                this.scrollContainer.classList.add('table-content-scroll-container-x-scrolled-right');
            }
            
            if (this.tableContainer.classList.contains('table-container-mobile-scroll')) {
                if (scrollLeft > 10) {
                    this.tableContainer.classList.add('scrolled');
                } else {
                    this.tableContainer.classList.remove('scrolled');
                }
            }
            
        } catch (error) {
            debugLogger.error('[TableScrollInstance] Update scroll shadows failed:', error);
        }
    }
    
    setupHeaderFocusHandling() {
        try {
            const headers = this.scrollContainer.querySelectorAll('.table-header-cell[data-column-key]');
            
            headers.forEach(header => {
                header.addEventListener('focus', this.handleHeaderFocus, { passive: true });
            });
            
        } catch (error) {
            debugLogger.error('[TableScrollInstance] Header focus setup failed:', error);
        }
    }
    
    handleHeaderFocus(event) {
        try {
            const header = event.target;
            const headerLeft = header.offsetLeft;
            const headerRight = headerLeft + header.offsetWidth;
            const containerLeft = this.scrollContainer.scrollLeft;
            const containerRight = containerLeft + this.scrollContainer.clientWidth;
            
            if (headerLeft < containerLeft || headerRight > containerRight) {
                const targetScrollLeft = Math.max(0, headerLeft - 20);
                
                this.scrollContainer.scrollTo({
                    left: targetScrollLeft,
                    behavior: 'smooth'
                });
            }
            
        } catch (error) {
            debugLogger.error('[TableScrollInstance] Header focus handling failed:', error);
        }
    }
    
    dispose() {
        try {
            this.scrollContainer.removeEventListener('scroll', this.handleScroll);
            
            if (this.resizeObserver) {
                this.resizeObserver.disconnect();
                this.resizeObserver = null;
            } else {
                window.removeEventListener('resize', this.handleResize);
            }
            
            const headers = this.scrollContainer.querySelectorAll('.table-header-cell[data-column-key]');
            headers.forEach(header => {
                header.removeEventListener('focus', this.handleHeaderFocus);
            });
            
            if (this.scrollTimeout) {
                cancelAnimationFrame(this.scrollTimeout);
                this.scrollTimeout = null;
            }
            
        } catch (error) {
            debugLogger.error('[TableScrollInstance] Dispose failed:', error);
        }
    }
}

// Auto-initialize on DOM ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        // Auto-detect tables that need advanced features
        document.querySelectorAll('[data-table-id]').forEach(table => {
            const tableId = table.getAttribute('data-table-id');
            if (tableId && table.classList.contains('table-advanced')) {
                RTableManager.initialize(tableId);
            }
        });
    });
} else {
    // DOM is already ready
    document.querySelectorAll('[data-table-id]').forEach(table => {
        const tableId = table.getAttribute('data-table-id');
        if (tableId && table.classList.contains('table-advanced')) {
            RTableManager.initialize(tableId);
        }
    });
}

// Export for use in other modules
export default RTableManager;

// Legacy compatibility
window.RTableAdvanced = RTableManager;
window.RTableScrollManager = RTableManager;