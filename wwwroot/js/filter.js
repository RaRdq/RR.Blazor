let positioningEngine = null;
let PositioningEngine = null;

let activeFilter = null;
let pendingOperations = new Set();
let activePortals = new Map();
let viewportLocations = new Map();

const Filter = {
    async initialize() {
        if (!positioningEngine && window.RRBlazor?.Positioning) {
            const PositioningModule = await window.RRBlazor.Positioning.getPositioningEngine();
            if (PositioningModule && PositioningModule.PositioningEngine) {
                PositioningEngine = PositioningModule.PositioningEngine;
                positioningEngine = new PositioningModule.PositioningEngine();
            } else if (PositioningModule) {
                positioningEngine = PositioningModule;
                PositioningEngine = PositioningModule.constructor || PositioningModule;
            }
        }
        
        if (!this._eventListenerAdded) {
            document.addEventListener(window.RRBlazor.Events.UI_COMPONENT_CLOSE_REQUEST, (event) => {
                if (event.detail.componentType === 'FILTER' && activeFilter === event.detail.componentId) {
                    this.close(event.detail.componentId);
                }
            });
            this._eventListenerAdded = true;
        }
        
        return !!positioningEngine;
    },
    
    findViewport(filterElement, filterElementId) {
        if (!filterElement) {
            return null;
        }
        
        const viewport = filterElement.querySelector('.filter-viewport');
        return viewport;
    },
    
    async open(filterElementId, config = {}) {
        if (pendingOperations.has(filterElementId)) return;
        
        pendingOperations.add(filterElementId);
        try {
            const filterElement = document.querySelector(`[data-filter-id="${filterElementId}"]`);
            if (!filterElement) {
                return;
            }
            
            const trigger = filterElement.querySelector('.filter-trigger') || filterElement.querySelector('.filter-trigger-wrapper');
            if (!trigger) {
                return;
            }
            
            const viewport = this.findViewport(filterElement, filterElementId);
            if (!viewport) {
                return;
            }
            
            if (activeFilter && activeFilter !== filterElementId) {
                await this.close(activeFilter);
            }
            
            window.RRBlazor.EventDispatcher.dispatch(
                window.RRBlazor.Events.UI_COMPONENT_OPENING,
                {
                    componentType: 'FILTER',
                    componentId: filterElementId,
                    priority: window.RRBlazor.EventPriorities.NORMAL
                }
            );

            const portalId = `filter-${filterElementId}`;
            
            const triggerRect = trigger.getBoundingClientRect();
            const dropdownMaxHeight = 280;
            const baseMinWidth = 180;
            const minWidth = Math.max(triggerRect.width, baseMinWidth);
            
            const itemCount = viewport.querySelectorAll('.filter-item').length;
            const estimatedHeight = Math.min(itemCount * 36 + 12, dropdownMaxHeight); // 36px per item (smaller) + padding
            const targetDimensions = {
                width: minWidth,
                height: estimatedHeight
            };
            
            let desiredPosition = 'bottom-start';
            if (positioningEngine && PositioningEngine) {
                if (!config.direction || config.direction === 'auto') {
                    desiredPosition = positioningEngine.detectOptimalPosition(triggerRect, targetDimensions);
                } else {
                    const directionMap = {
                        'bottom': PositioningEngine.POSITIONS.BOTTOM_START,
                        'bottom-start': PositioningEngine.POSITIONS.BOTTOM_START,
                        'bottom-center': PositioningEngine.POSITIONS.BOTTOM_CENTER,
                        'bottom-end': PositioningEngine.POSITIONS.BOTTOM_END,
                        'top': PositioningEngine.POSITIONS.TOP_START,
                        'top-start': PositioningEngine.POSITIONS.TOP_START,
                        'top-center': PositioningEngine.POSITIONS.TOP_CENTER,
                        'top-end': PositioningEngine.POSITIONS.TOP_END
                    };
                    desiredPosition = directionMap[config.direction] || PositioningEngine.POSITIONS.BOTTOM_START;
                }
            }
            
            const containerElement = filterElement.closest('[data-container], .sidebar, .app-sidebar, .modal, .dialog, [class*="sidebar"], [class*="panel"]');
            const containerRect = containerElement ? containerElement.getBoundingClientRect() : null;
            
            let adaptedDimensions = { ...targetDimensions };
            if (containerRect) {
                const maxContainerWidth = containerRect.width - 12;
                if (adaptedDimensions.width > maxContainerWidth) {
                    adaptedDimensions.width = Math.max(maxContainerWidth, triggerRect.width);
                }
            }
            
            let position;
            if (positioningEngine) {
                position = positioningEngine.calculatePosition(
                    triggerRect,
                    adaptedDimensions,
                    {
                        position: desiredPosition,
                        offset: 2,
                        flip: true,
                        constrain: true,
                        container: containerRect
                    }
                );
            } else {
                // Fallback positioning if engine not available
                const spaceBelow = window.innerHeight - triggerRect.bottom;
                const spaceAbove = triggerRect.top;
                const shouldFlip = spaceBelow < adaptedDimensions.height && spaceAbove > spaceBelow;
                
                position = {
                    x: triggerRect.left,
                    y: shouldFlip ? triggerRect.top - adaptedDimensions.height - 2 : triggerRect.bottom + 2
                };
            }

            const portalPromise = this._waitForPortal(filterElementId);
            
            window.RRBlazor.EventDispatcher.dispatch(
                window.RRBlazor.Events.PORTAL_CREATE_REQUEST,
                {
                    requesterId: `filter-${filterElementId}`,
                    config: {
                        id: `filter-portal-${filterElementId}`,
                        className: 'filter-portal'
                    }
                }
            );
            
            const portal = await portalPromise;
            
            viewport._originalParent = viewport.parentNode;
            viewport._originalNextSibling = viewport.nextSibling;
            
            viewport.style.position = '';
            viewport.style.top = '';
            viewport.style.left = '';
            viewport.style.transform = '';
            
            if (portal && portal.element) {
                portal.element.appendChild(viewport);
                viewportLocations.set(filterElementId, portal.element);
                
                viewport.style.position = 'fixed';
                viewport.style.left = `${position.x}px`;
                viewport.style.top = `${position.y}px`;
                viewport.style.width = `${adaptedDimensions.width}px`;
                viewport.style.maxHeight = `${dropdownMaxHeight}px`;
            } else {
                viewport.style.position = 'fixed';
                viewport.style.left = `${position.x}px`;
                viewport.style.top = `${position.y}px`;
                viewport.style.width = `${adaptedDimensions.width}px`;
                viewport.style.maxHeight = `${dropdownMaxHeight}px`;
                viewportLocations.set(filterElementId, viewport.parentElement || document.body);
            }

            this.applyDropdownStyles(viewport, adaptedDimensions.width);
            
            Object.assign(viewport.style, {
                visibility: 'visible',
                display: 'block',
                opacity: '1',
                zIndex: '9998',
                pointerEvents: 'auto'
            });
            
            viewport.classList.remove('filter-viewport-closed');
            viewport.classList.add('filter-viewport-open');
            filterElement.classList.add('filter-dropdown-open');

            activeFilter = filterElementId;
            activePortals.set(filterElementId, portalId);
            
            window.RRBlazor.EventDispatcher.dispatch(
                window.RRBlazor.Events.UI_COMPONENT_OPENED,
                {
                    componentType: 'FILTER',
                    componentId: filterElementId
                }
            );
            
            window.RRBlazor.ClickOutside.register(`filter-${filterElementId}`, filterElement, {
                excludeSelectors: [
                    '.filter-trigger',
                    '.filter-viewport', 
                    '.filter-content',
                    '.filter-portal',
                    '.modal-portal',
                    '.modal-content',
                    '[data-modal-id]',
                    `[data-filter-id="${filterElementId}"]`,
                    `[data-viewport-id="${filterElementId}"]`
                ]
            });

            const clickOutsideHandler = (event) => {
                if (event.detail && event.detail.elementId === `filter-${filterElementId}`) {
                    this.close(filterElementId);
                }
            };
            document.addEventListener(window.RRBlazor.Events.CLICK_OUTSIDE, clickOutsideHandler);
            
            filterElement._clickOutsideHandler = clickOutsideHandler;

            const repositionHandler = () => {
                if (!trigger || !viewport) return;
                
                const triggerRect = trigger.getBoundingClientRect();
                const position = positioningEngine ? positioningEngine.calculatePosition(
                    triggerRect,
                    adaptedDimensions,
                    {
                        position: desiredPosition,
                        offset: 2,
                        flip: true,
                        constrain: true,
                        container: containerRect
                    }
                ) : { x: triggerRect.left, y: triggerRect.bottom + 2 };
                
                viewport.style.left = `${position.x}px`;
                viewport.style.top = `${position.y}px`;
            };
            
            let repositionTimer;
            const throttledReposition = () => {
                clearTimeout(repositionTimer);
                repositionTimer = setTimeout(repositionHandler, 10);
            };
            
            window.addEventListener('scroll', throttledReposition, true);
            window.addEventListener('resize', throttledReposition);
            
            filterElement._scrollHandler = throttledReposition;
            filterElement._resizeHandler = throttledReposition;
            
            return portalId;
        } finally {
            pendingOperations.delete(filterElementId);
        }
    },

    async close(filterElementId) {
        if (!filterElementId || activeFilter !== filterElementId) return false;
        
        window.RRBlazor.EventDispatcher.dispatch(
            window.RRBlazor.Events.UI_COMPONENT_CLOSING,
            {
                componentType: 'FILTER',
                componentId: filterElementId
            }
        );

        const filterElement = document.querySelector(`[data-filter-id="${filterElementId}"]`);
        if (!filterElement) {
            activeFilter = null;
            return false;
        }

        let viewport = null;
        
        if (viewportLocations.has(filterElementId)) {
            const location = viewportLocations.get(filterElementId);
            if (location) {
                viewport = location.querySelector('.filter-viewport');
            }
        }
        
        if (!viewport) {
            viewport = this.findViewport(filterElement, filterElementId);
        }
        
        if (!viewport) {
            activeFilter = null;
            return false;
        }

        viewport.classList.remove('filter-viewport-open');
        viewport.classList.add('filter-viewport-closed');
        filterElement.classList.remove('filter-dropdown-open');

        if (viewport._originalParent) {
            if (viewport._originalNextSibling) {
                viewport._originalParent.insertBefore(viewport, viewport._originalNextSibling);
            } else {
                viewport._originalParent.appendChild(viewport);
            }
            
            viewport.style.position = 'absolute';
            viewport.style.top = '-9999px';
            viewport.style.left = '-9999px';
            viewport.style.visibility = 'hidden';
            
            delete viewport._originalParent;
            delete viewport._originalNextSibling;
        }

        window.RRBlazor.EventDispatcher.dispatch(
            window.RRBlazor.Events.PORTAL_DESTROY_REQUEST,
            { requesterId: `filter-${filterElementId}`, portalId: `filter-portal-${filterElementId}` }
        );

        window.RRBlazor.ClickOutside.unregister(`filter-${filterElementId}`);
        
        if (filterElement && filterElement._clickOutsideHandler) {
            document.removeEventListener(window.RRBlazor.Events.CLICK_OUTSIDE, filterElement._clickOutsideHandler);
            delete filterElement._clickOutsideHandler;
        }
        
        if (filterElement && filterElement._scrollHandler) {
            window.removeEventListener('scroll', filterElement._scrollHandler, true);
            delete filterElement._scrollHandler;
        }
        if (filterElement && filterElement._resizeHandler) {
            window.removeEventListener('resize', filterElement._resizeHandler);
            delete filterElement._resizeHandler;
        }
        
        activeFilter = null;
        activePortals.delete(filterElementId);
        viewportLocations.delete(filterElementId);
        
        window.RRBlazor.EventDispatcher.dispatch(
            window.RRBlazor.Events.UI_COMPONENT_CLOSED,
            {
                componentType: 'FILTER',
                componentId: filterElementId
            }
        );
        
        return true;
    },

    applyDropdownStyles(viewport, triggerWidth) {
        const minWidth = Math.max(triggerWidth, 180);
        
        viewport.style.minWidth = `${minWidth}px`;
        viewport.style.maxHeight = '280px';
        viewport.style.boxShadow = '0 1px 6px rgba(0,0,0,0.08)';
        viewport.style.borderRadius = '6px';
        viewport.style.overflow = 'hidden';
        viewport.style.background = 'var(--color-surface-elevated)';
        viewport.style.border = '1px solid var(--color-border)';

        const content = viewport.querySelector('.filter-content');
        if (content) {
            content.style.maxHeight = '280px';
            content.style.overflowY = 'auto';
        }
    },

    async _waitForPortal(filterElementId, timeout = 1000) {
        return new Promise((resolve, reject) => {
            const requesterId = `filter-${filterElementId}`;
            const timeoutId = setTimeout(() => {
                document.removeEventListener(window.RRBlazor.Events.PORTAL_CREATED, handler);
                reject(new Error(`Portal creation timeout for filter ${filterElementId}`));
            }, timeout);
            
            const handler = (event) => {
                if (event.detail.requesterId === requesterId) {
                    clearTimeout(timeoutId);
                    document.removeEventListener(window.RRBlazor.Events.PORTAL_CREATED, handler);
                    resolve(event.detail.portal);
                }
            };
            
            document.addEventListener(window.RRBlazor.Events.PORTAL_CREATED, handler);
        });
    },

    dispose() {
        if (activeFilter) {
            this.close(activeFilter);
        }
        
        activePortals.clear();
        viewportLocations.clear();
        pendingOperations.clear();
        
        activeFilter = null;
        positioningEngine = null;
    }
};

document.addEventListener('click', (event) => {
    const item = event.target.closest('.filter-item');
    if (!item || item.disabled) return;
    
    if (item.querySelector('input, button')) return;
    
    const filterElement = item.closest('[data-filter-id]');
    if (!filterElement) return;
    
    const filterId = filterElement.dataset.filterId;
    
    window.RRBlazor.EventDispatcher.dispatch(
        'filter-item-selected',
        {
            filterId: filterId,
            item: item,
            value: item.dataset.value || item.textContent.trim()
        }
    );
    
    Filter.close(filterId);
});

// Initialize when module loads
if (typeof window !== 'undefined' && window.RRBlazor) {
    // Delay initialization to ensure RRBlazor is fully loaded
    setTimeout(() => {
        Filter.initialize().then(success => {
            if (!success) {
            }
        });
    }, 100);
}

export default {
    open: (filterElementId, config) => Filter.open(filterElementId, config),
    close: (filterElementId) => Filter.close(filterElementId),
    toggle: async (filterElementId, config) => {
        const isOpen = activeFilter === filterElementId;
        return isOpen ? Filter.close(filterElementId) : Filter.open(filterElementId, config);
    },
    isOpen: (filterElementId) => activeFilter === filterElementId,
    closeAll: () => {
        if (activeFilter) {
            return Filter.close(activeFilter);
        }
        activePortals.clear();
        viewportLocations.clear();
        return Promise.resolve(true);
    },
    dispose: () => Filter.dispose(),
    initialize: () => Filter.initialize()
};