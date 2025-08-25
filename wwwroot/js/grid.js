export const RGrid = {
    instances: new Map(),
    
    defaults: {
        virtualItemHeight: 40,
        virtualOverscan: 5,
        resizeDebounceMs: 100,
        scrollDebounceMs: 16
    },

    /**
     * Initialize RGrid instance
     */
    initialize(gridId, configuration) {
        if (this.instances.has(gridId)) {
            this.cleanup(gridId);
        }

        const gridElement = document.querySelector(`[data-grid-id="${gridId}"]`);
        if (!gridElement) {
            return;
        }

        const instance = {
            gridId,
            element: gridElement,
            configuration,
            resizeObserver: null,
            intersectionObserver: null,
            virtualScrollHandler: null,
            columnResizeData: null,
            isDestroyed: false
        };

        this.setupColumnResizing(instance);
        this.setupVirtualization(instance);
        this.setupKeyboardNavigation(instance);
        this.setupAccessibility(instance);

        this.instances.set(gridId, instance);
        
    },

    /**
     * Setup column resizing functionality
     */
    setupColumnResizing(instance) {
        if (!instance.configuration.EnableColumnResizing) return;

        const table = instance.element.querySelector('.rgrid-table');
        if (!table) return;

        let currentResizer = null;
        let startX = 0;
        let startWidth = 0;
        let currentColumn = null;

        const headers = table.querySelectorAll('.rgrid-header-cell.rgrid-resizable');
        headers.forEach(header => {
            const resizer = document.createElement('div');
            resizer.className = 'rgrid-column-resizer';
            header.appendChild(resizer);

            resizer.addEventListener('mousedown', (e) => {
                e.preventDefault();
                currentResizer = resizer;
                currentColumn = header;
                startX = e.clientX;
                startWidth = header.offsetWidth;
                
                resizer.classList.add('rgrid-resizing');
                document.body.style.cursor = 'col-resize';
                document.body.style.userSelect = 'none';

                document.addEventListener('mousemove', handleMouseMove);
                document.addEventListener('mouseup', handleMouseUp);
            });
        });

        const handleMouseMove = this.debounce((e) => {
            if (!currentResizer || !currentColumn) return;

            const diff = e.clientX - startX;
            const newWidth = Math.max(50, startWidth + diff);
            
            currentColumn.style.width = `${newWidth}px`;
            
            const columnIndex = Array.from(currentColumn.parentNode.children).indexOf(currentColumn);
            const rows = table.querySelectorAll('tbody tr');
            rows.forEach(row => {
                const cell = row.children[columnIndex];
                if (cell) {
                    cell.style.width = `${newWidth}px`;
                }
            });
        }, this.defaults.resizeDebounceMs);

        const handleMouseUp = () => {
            if (currentResizer) {
                currentResizer.classList.remove('rgrid-resizing');
            }
            
            document.body.style.cursor = '';
            document.body.style.userSelect = '';
            
            currentResizer = null;
            currentColumn = null;
            
            document.removeEventListener('mousemove', handleMouseMove);
            document.removeEventListener('mouseup', handleMouseUp);
        };
    },

    /**
     * Setup virtualization for large datasets
     */
    setupVirtualization(instance) {
        if (!instance.configuration.EnableVirtualization) return;

        const virtualContainer = instance.element.querySelector('.rgrid-virtualized .rgrid-virtual-container');
        if (!virtualContainer) return;

        let lastScrollTop = 0;
        let isScrolling = false;

        const scrollHandler = this.throttle(() => {
            const scrollTop = virtualContainer.scrollTop;
            const containerHeight = virtualContainer.clientHeight;
            const itemHeight = this.defaults.virtualItemHeight;
            
            const startIndex = Math.floor(scrollTop / itemHeight);
            const endIndex = Math.min(
                startIndex + Math.ceil(containerHeight / itemHeight) + this.defaults.virtualOverscan,
                instance.totalItems || 0
            );

            this.updateVirtualItems(instance, startIndex, endIndex);
            lastScrollTop = scrollTop;
        }, this.defaults.scrollDebounceMs);

        virtualContainer.addEventListener('scroll', scrollHandler);
        instance.virtualScrollHandler = scrollHandler;

        instance.intersectionObserver = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('rgrid-visible');
                } else {
                    entry.target.classList.remove('rgrid-visible');
                }
            });
        }, {
            root: virtualContainer,
            rootMargin: '50px'
        });
    },

    /**
     * Update virtual items in viewport
     */
    updateVirtualItems(instance, startIndex, endIndex) {
        const virtualContent = instance.element.querySelector('.rgrid-virtual-content');
        if (!virtualContent) return;

        // Remove existing items
        virtualContent.innerHTML = '';

        // Create visible items
        for (let i = startIndex; i < endIndex; i++) {
            const item = document.createElement('div');
            item.className = 'rgrid-virtual-row';
            item.style.transform = `translateY(${i * this.defaults.virtualItemHeight}px)`;
            item.style.height = `${this.defaults.virtualItemHeight}px`;
            item.dataset.index = i;

            // This would be populated by Blazor
            item.innerHTML = `<div class="rgrid-virtual-cell">Item ${i}</div>`;

            virtualContent.appendChild(item);

            // Observe for intersection
            if (instance.intersectionObserver) {
                instance.intersectionObserver.observe(item);
            }
        }
    },

    /**
     * Setup keyboard navigation
     */
    setupKeyboardNavigation(instance) {
        if (!instance.configuration.EnableKeyboardNavigation) return;

        const table = instance.element.querySelector('.rgrid-table');
        if (!table) return;

        let currentCell = null;
        let currentRow = 0;
        let currentCol = 0;

        table.addEventListener('keydown', (e) => {
            const rows = table.querySelectorAll('tbody tr:not(.rgrid-master-detail-row)');
            const maxRow = rows.length - 1;
            const maxCol = rows[0]?.children.length - 1 || 0;

            switch (e.key) {
                case 'ArrowDown':
                    e.preventDefault();
                    currentRow = Math.min(currentRow + 1, maxRow);
                    this.focusCell(rows, currentRow, currentCol);
                    break;

                case 'ArrowUp':
                    e.preventDefault();
                    currentRow = Math.max(currentRow - 1, 0);
                    this.focusCell(rows, currentRow, currentCol);
                    break;

                case 'ArrowRight':
                    e.preventDefault();
                    currentCol = Math.min(currentCol + 1, maxCol);
                    this.focusCell(rows, currentRow, currentCol);
                    break;

                case 'ArrowLeft':
                    e.preventDefault();
                    currentCol = Math.max(currentCol - 1, 0);
                    this.focusCell(rows, currentRow, currentCol);
                    break;

                case 'Enter':
                case ' ':
                    e.preventDefault();
                    const cell = rows[currentRow]?.children[currentCol];
                    if (cell) {
                        cell.click();
                    }
                    break;

                case 'Home':
                    e.preventDefault();
                    if (e.ctrlKey) {
                        currentRow = 0;
                        currentCol = 0;
                    } else {
                        currentCol = 0;
                    }
                    this.focusCell(rows, currentRow, currentCol);
                    break;

                case 'End':
                    e.preventDefault();
                    if (e.ctrlKey) {
                        currentRow = maxRow;
                        currentCol = maxCol;
                    } else {
                        currentCol = maxCol;
                    }
                    this.focusCell(rows, currentRow, currentCol);
                    break;
            }
        });

        // Initialize focus
        const firstRow = table.querySelector('tbody tr');
        if (firstRow) {
            this.focusCell(table.querySelectorAll('tbody tr'), 0, 0);
        }
    },

    /**
     * Focus a specific cell
     */
    focusCell(rows, rowIndex, colIndex) {
        // Remove previous focus
        const prevFocused = document.querySelector('.rgrid-cell-focused');
        if (prevFocused) {
            prevFocused.classList.remove('rgrid-cell-focused');
        }

        // Add focus to new cell
        const row = rows[rowIndex];
        const cell = row?.children[colIndex];
        if (cell) {
            cell.classList.add('rgrid-cell-focused');
            cell.scrollIntoView({ block: 'nearest', inline: 'nearest' });
            
            // Set tabindex for screen readers
            cell.setAttribute('tabindex', '0');
            cell.focus();
        }
    },

    /**
     * Setup accessibility features
     */
    setupAccessibility(instance) {
        const table = instance.element.querySelector('.rgrid-table');
        if (!table) return;

        // Set ARIA attributes
        table.setAttribute('role', 'grid');
        table.setAttribute('aria-label', instance.configuration.Title || 'Data Grid');
        
        // Setup row and cell roles
        const rows = table.querySelectorAll('tbody tr');
        rows.forEach((row, rowIndex) => {
            row.setAttribute('role', 'row');
            row.setAttribute('aria-rowindex', rowIndex + 1);

            const cells = row.querySelectorAll('td');
            cells.forEach((cell, cellIndex) => {
                cell.setAttribute('role', 'gridcell');
                cell.setAttribute('aria-colindex', cellIndex + 1);
            });
        });

        // Setup header accessibility
        const headers = table.querySelectorAll('thead th');
        headers.forEach((header, index) => {
            header.setAttribute('role', 'columnheader');
            header.setAttribute('aria-colindex', index + 1);
            
            if (header.classList.contains('rgrid-sortable')) {
                header.setAttribute('aria-sort', 'none');
                header.setAttribute('tabindex', '0');
            }
        });

        // Announce changes to screen readers
        this.setupAriaLive(instance);
    },

    /**
     * Setup ARIA live regions for announcements
     */
    setupAriaLive(instance) {
        const liveRegion = document.createElement('div');
        liveRegion.className = 'rgrid-aria-live';
        liveRegion.setAttribute('aria-live', 'polite');
        liveRegion.setAttribute('aria-atomic', 'false');
        liveRegion.style.position = 'absolute';
        liveRegion.style.left = '-10000px';
        liveRegion.style.width = '1px';
        liveRegion.style.height = '1px';
        liveRegion.style.overflow = 'hidden';

        instance.element.appendChild(liveRegion);
        instance.ariaLiveRegion = liveRegion;
    },

    /**
     * Announce message to screen readers
     */
    announce(gridId, message) {
        const instance = this.instances.get(gridId);
        if (instance && instance.ariaLiveRegion) {
            instance.ariaLiveRegion.textContent = message;
        }
    },


    /**
     * Download file utility for exports - delegates to utils
     */
    downloadFile(filename, base64Data, mimeType) {
        if (window.downloadFile) {
            return window.downloadFile(base64Data, filename, mimeType);
        }
        return false;
    },

    /**
     * Utility: Debounce function
     */
    debounce(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    },

    /**
     * Utility: Throttle function
     */
    throttle(func, limit) {
        let inThrottle;
        return function() {
            const args = arguments;
            const context = this;
            if (!inThrottle) {
                func.apply(context, args);
                inThrottle = true;
                setTimeout(() => inThrottle = false, limit);
            }
        };
    },

    /**
     * Update grid data (called from Blazor)
     */
    updateData(gridId, data) {
        const instance = this.instances.get(gridId);
        if (!instance) return;

        instance.data = data;
        instance.totalItems = data.length;

        // Update virtual container if needed
        if (instance.configuration.EnableVirtualization) {
            const virtualContent = instance.element.querySelector('.rgrid-virtual-content');
            if (virtualContent) {
                virtualContent.style.height = `${data.length * this.defaults.virtualItemHeight}px`;
            }
        }

        // Announce data update
        this.announce(gridId, `Grid updated with ${data.length} items`);
    },

    /**
     * Handle sorting change
     */
    updateSort(gridId, columnKey, direction) {
        const instance = this.instances.get(gridId);
        if (!instance) return;

        const table = instance.element.querySelector('.rgrid-table');
        const headers = table.querySelectorAll('.rgrid-header-cell[data-column="' + columnKey + '"]');
        
        // Update ARIA attributes
        headers.forEach(header => {
            const ariaSort = direction === 'asc' ? 'ascending' : 
                           direction === 'desc' ? 'descending' : 'none';
            header.setAttribute('aria-sort', ariaSort);
        });

        // Announce sort change
        const sortText = direction === 'asc' ? 'ascending' : 
                        direction === 'desc' ? 'descending' : 'removed';
        this.announce(gridId, `Sorted by ${columnKey} ${sortText}`);
    },

    /**
     * Handle selection change
     */
    updateSelection(gridId, selectedCount, totalCount) {
        this.announce(gridId, `${selectedCount} of ${totalCount} items selected`);
    },

    /**
     * Cleanup grid instance
     */
    cleanup(gridId) {
        const instance = this.instances.get(gridId);
        if (!instance) return;

        instance.isDestroyed = true;

        // Cleanup observers
        if (instance.resizeObserver) {
            instance.resizeObserver.disconnect();
        }
        
        if (instance.intersectionObserver) {
            instance.intersectionObserver.disconnect();
        }


        // Remove event listeners
        if (instance.virtualScrollHandler) {
            const virtualContainer = instance.element.querySelector('.rgrid-virtual-container');
            if (virtualContainer) {
                virtualContainer.removeEventListener('scroll', instance.virtualScrollHandler);
            }
        }

        // Cleanup DOM
        if (instance.ariaLiveRegion) {
            instance.ariaLiveRegion.remove();
        }

        this.instances.delete(gridId);
    },

    /**
     * Get instance for debugging
     */
    getInstance(gridId) {
        return this.instances.get(gridId);
    },

    /**
     * Get all instances for debugging
     */
    getAllInstances() {
        return Array.from(this.instances.entries());
    }
};

// Export for module usage
export default RGrid;