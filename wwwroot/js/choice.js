
import { PositioningEngine } from './positioning.js';

const positioningEngine = new PositioningEngine();

let activeDropdown = null;
let keyboardNavigationEnabled = false;
let currentHighlightedIndex = -1;
let pendingOperations = new Set();
let activePortals = new Map();
let viewportLocations = new Map();

const Choice = {
    initialize() {
        document.addEventListener(window.RRBlazor.Events.UI_COMPONENT_CLOSE_REQUEST, (event) => {
            if (event.detail.componentType === window.RRBlazor.ComponentTypes.CHOICE && activeDropdown === event.detail.componentId) {
                this.closeDropdown(event.detail.componentId);
            }
        });
    },
    
    /**
     * Find viewport element for a choice component - always a child
     */
    findViewport(choiceElement, choiceElementId) {
        if (!choiceElement) {
            console.error(`Choice element not found for id: ${choiceElementId}`);
            return null;
        }
        
        const viewport = choiceElement.querySelector('.choice-viewport');
        if (!viewport) {
            console.error(`Viewport not found as child of choice element: ${choiceElementId}`);
        }
        return viewport;
    },
    
    async openDropdown(choiceElementId, options = {}) {
        if (pendingOperations.has(choiceElementId)) return;
        
        pendingOperations.add(choiceElementId);
        try {
            const choiceElement = document.querySelector(`[data-choice-id="${choiceElementId}"]`);
            if (!choiceElement) {
                console.warn(`Choice element not found: ${choiceElementId}`);
                return;
            }
            
            const trigger = choiceElement.querySelector('.choice-trigger') || choiceElement.querySelector('.choice-trigger-wrapper');
            if (!trigger) {
                console.warn(`Trigger element not found for choice: ${choiceElementId}`);
                return;
            }
            
            const viewport = this.findViewport(choiceElement, choiceElementId);
            if (!viewport) {
                console.warn(`Viewport element not found for choice: ${choiceElementId}`);
                return;
            }
            
            if (activeDropdown && activeDropdown !== choiceElementId) {
                await this.closeDropdown(activeDropdown);
            }
            
            window.RRBlazor.EventDispatcher.dispatch(
                window.RRBlazor.Events.UI_COMPONENT_OPENING,
                {
                    componentType: window.RRBlazor.ComponentTypes.CHOICE,
                    componentId: choiceElementId,
                    priority: window.RRBlazor.EventPriorities.NORMAL
                }
            );

            const portalId = `choice-${choiceElementId}`;
            

            const triggerRect = trigger.getBoundingClientRect();
            const dropdownMaxHeight = 320;
            const isUserMenu = choiceElement.closest('[class*="role-switcher"], [class*="user-menu"], .choice-dropdown[data-choice-id*="user"]');
            const baseMinWidth = isUserMenu ? 280 : 200;
            const minWidth = Math.max(triggerRect.width, baseMinWidth);
            
            const itemCount = viewport.querySelectorAll('.choice-item').length;
            const estimatedHeight = Math.min(itemCount * 40 + 16, dropdownMaxHeight); // 40px per item + padding
            const targetDimensions = {
                width: minWidth,
                height: estimatedHeight
            };
            
            
            let desiredPosition;
            if (!options.direction || options.direction === 'auto') {
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
                    'top-end': PositioningEngine.POSITIONS.TOP_END,
                    'left': PositioningEngine.POSITIONS.LEFT_START,
                    'left-start': PositioningEngine.POSITIONS.LEFT_START,
                    'left-center': PositioningEngine.POSITIONS.LEFT_CENTER,
                    'left-end': PositioningEngine.POSITIONS.LEFT_END,
                    'right': PositioningEngine.POSITIONS.RIGHT_START,
                    'right-start': PositioningEngine.POSITIONS.RIGHT_START,
                    'right-center': PositioningEngine.POSITIONS.RIGHT_CENTER,
                    'right-end': PositioningEngine.POSITIONS.RIGHT_END
                };
                desiredPosition = directionMap[options.direction] || PositioningEngine.POSITIONS.BOTTOM_START;
            }
            
            const containerElement = choiceElement.closest('[data-container], .sidebar, .app-sidebar, .modal, .dialog, [class*="sidebar"], [class*="panel"]');
            const containerRect = containerElement ? containerElement.getBoundingClientRect() : null;
            
            let adaptedDimensions = { ...targetDimensions };
            if (containerRect) {
                const maxContainerWidth = containerRect.width - 16; // Leave 16px total margin
                if (adaptedDimensions.width > maxContainerWidth) {
                    adaptedDimensions.width = Math.max(maxContainerWidth, triggerRect.width);
                }
            }
            
            const position = positioningEngine.calculatePosition(
                triggerRect,
                adaptedDimensions,
                {
                    position: desiredPosition,
                    offset: 4, // Reduced from 8 to 4 since we removed SCSS transform offset
                    flip: true,
                    constrain: true,
                    container: containerRect
                }
            );

            const portalPromise = this._waitForPortal(choiceElementId);
            
            window.RRBlazor.EventDispatcher.dispatch(
                window.RRBlazor.Events.PORTAL_CREATE_REQUEST,
                {
                    requesterId: `choice-${choiceElementId}`,
                    config: {
                        id: `choice-portal-${choiceElementId}`,
                        className: 'choice-portal'
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
                viewportLocations.set(choiceElementId, portal.element);
                
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
                viewportLocations.set(choiceElementId, viewport.parentElement || document.body);
            }

            this.applyDropdownStyles(viewport, adaptedDimensions.width);
            
            Object.assign(viewport.style, {
                visibility: 'visible',
                display: 'block',
                opacity: '1',
                zIndex: '9999',
                pointerEvents: 'auto'
            });
            viewport.style.zIndex = '9999';
            
            viewport.classList.remove('choice-viewport-closed');
            viewport.classList.add('choice-viewport-open');
            choiceElement.classList.add('choice-open');

            activeDropdown = choiceElementId;
            activePortals.set(choiceElementId, portalId);
            
            window.RRBlazor.EventDispatcher.dispatch(
                window.RRBlazor.Events.UI_COMPONENT_OPENED,
                {
                    componentType: window.RRBlazor.ComponentTypes.CHOICE,
                    componentId: choiceElementId
                }
            );
            
            window.RRBlazor.ClickOutside.register(`choice-${choiceElementId}`, choiceElement, {
                excludeSelectors: [
                    '.choice-trigger',
                    '.choice-viewport', 
                    '.choice-content',
                    '.choice-portal',
                    '.modal-portal',
                    '.modal-content',
                    '[data-modal-id]',
                    `[data-choice-id="${choiceElementId}"]`,
                    `[data-viewport-id="${choiceElementId}"]`
                ]
            });

            // Add click-outside listener - listen for the EventDispatcher's CustomEvent on document
            const clickOutsideHandler = (event) => {
                if (event.detail && event.detail.elementId === `choice-${choiceElementId}`) {
                    this.closeDropdown(choiceElementId);
                }
            };
            document.addEventListener(window.RRBlazor.Events.CLICK_OUTSIDE, clickOutsideHandler);
            
            // Store handler for cleanup
            choiceElement._clickOutsideHandler = clickOutsideHandler;

            // Add scroll and resize handlers to reposition dropdown
            const repositionHandler = () => {
                if (!trigger || !viewport) return;
                
                const triggerRect = trigger.getBoundingClientRect();
                const position = positioningEngine.calculatePosition(
                    triggerRect,
                    adaptedDimensions,
                    {
                        position: desiredPosition,
                        offset: 4,
                        flip: true,
                        constrain: true,
                        container: containerRect
                    }
                );
                
                viewport.style.left = `${position.x}px`;
                viewport.style.top = `${position.y}px`;
            };
            
            // Throttle scroll/resize events
            let repositionTimer;
            const throttledReposition = () => {
                clearTimeout(repositionTimer);
                repositionTimer = setTimeout(repositionHandler, 10);
            };
            
            window.addEventListener('scroll', throttledReposition, true);
            window.addEventListener('resize', throttledReposition);
            
            // Store handlers for cleanup
            choiceElement._scrollHandler = throttledReposition;
            choiceElement._resizeHandler = throttledReposition;

            this.enableKeyboardNavigation(choiceElementId);
            this.scrollSelectedIntoView(viewport);
            
            return portalId;
        } finally {
            pendingOperations.delete(choiceElementId);
        }
    },

    async closeDropdown(choiceElementId) {
        if (!choiceElementId || activeDropdown !== choiceElementId) return false;
        
        window.RRBlazor.EventDispatcher.dispatch(
            window.RRBlazor.Events.UI_COMPONENT_CLOSING,
            {
                componentType: window.RRBlazor.ComponentTypes.CHOICE,
                componentId: choiceElementId
            }
        );

        const choiceElement = document.querySelector(`[data-choice-id="${choiceElementId}"]`);
        if (!choiceElement) {
            activeDropdown = null;
            return false;
        }

        let viewport = null;
        
        if (viewportLocations.has(choiceElementId)) {
            const location = viewportLocations.get(choiceElementId);
            if (location) {
                viewport = location.querySelector('.choice-viewport');
            }
        }
        
        if (!viewport) {
            viewport = this.findViewport(choiceElement, choiceElementId);
        }
        
        if (!viewport) {
            activeDropdown = null;
            return false;
        }

        viewport.classList.remove('choice-viewport-open');
        viewport.classList.add('choice-viewport-closed');
        choiceElement.classList.remove('choice-open');

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

        // Request portal cleanup via events (Dependency Inversion)
        window.RRBlazor.EventDispatcher.dispatch(
            window.RRBlazor.Events.PORTAL_DESTROY_REQUEST,
            { requesterId: `choice-${choiceElementId}`, portalId: `choice-portal-${choiceElementId}` }
        );

        this.disableKeyboardNavigation();
        window.RRBlazor.ClickOutside.unregister(`choice-${choiceElementId}`);
        
        // Clean up click-outside handler if it exists
        if (choiceElement && choiceElement._clickOutsideHandler) {
            document.removeEventListener(window.RRBlazor.Events.CLICK_OUTSIDE, choiceElement._clickOutsideHandler);
            delete choiceElement._clickOutsideHandler;
        }
        
        // Clean up scroll and resize handlers
        if (choiceElement && choiceElement._scrollHandler) {
            window.removeEventListener('scroll', choiceElement._scrollHandler, true);
            delete choiceElement._scrollHandler;
        }
        if (choiceElement && choiceElement._resizeHandler) {
            window.removeEventListener('resize', choiceElement._resizeHandler);
            delete choiceElement._resizeHandler;
        }
        
        activeDropdown = null;
        activePortals.delete(choiceElementId);
        viewportLocations.delete(choiceElementId);
        
        window.RRBlazor.EventDispatcher.dispatch(
            window.RRBlazor.Events.UI_COMPONENT_CLOSED,
            {
                componentType: window.RRBlazor.ComponentTypes.CHOICE,
                componentId: choiceElementId
            }
        );
        
        return true;
    },


    applyDropdownStyles(viewport, triggerWidth) {
        const minWidth = Math.max(triggerWidth, 200); // From plan
        
        viewport.style.minWidth = `${minWidth}px`;
        viewport.style.maxHeight = '320px'; // From plan
        viewport.style.boxShadow = '0 2px 8px rgba(0,0,0,0.1)'; // From plan
        viewport.style.borderRadius = '8px';
        viewport.style.overflow = 'hidden';
        viewport.style.background = 'var(--color-surface-elevated)';
        viewport.style.border = '1px solid var(--color-border)';

        const content = viewport.querySelector('.choice-content');
        if (content) {
            content.style.maxHeight = '320px';
            content.style.overflowY = 'auto';
        }
    },

    enableKeyboardNavigation(choiceElementId) {
        const choiceElement = document.querySelector(`[data-choice-id="${choiceElementId}"]`);
        if (!choiceElement) return;
        
        let viewport = null;
        if (viewportLocations.has(choiceElementId)) {
            const location = viewportLocations.get(choiceElementId);
            if (location) {
                viewport = location.querySelector('.choice-viewport');
            }
        }
        
        if (!viewport) {
            viewport = this.findViewport(choiceElement, choiceElementId);
        }
        
        if (viewport) {
            window.RRBlazor.EventDispatcher.dispatch(
                window.RRBlazor.Events.CHOICE_KEYBOARD_ENABLE,
                { choiceId: choiceElementId, viewport }
            );
        }
        
        keyboardNavigationEnabled = true;
        currentHighlightedIndex = -1;
    },

    disableKeyboardNavigation() {
        window.RRBlazor.EventDispatcher.dispatch(
            window.RRBlazor.Events.CHOICE_KEYBOARD_DISABLE
        );
        keyboardNavigationEnabled = false;
        currentHighlightedIndex = -1;
    },

    handleKeyDown(event) {
        if (!keyboardNavigationEnabled || !activeDropdown) return;

        const choiceElement = document.querySelector(`[data-choice-id="${activeDropdown}"]`);
        if (!choiceElement) return;

        let viewport = null;
        
        // First check if viewport is in a portal (when dropdown is open)
        if (viewportLocations.has(activeDropdown)) {
            const location = viewportLocations.get(activeDropdown);
            if (location) {
                viewport = location.querySelector('.choice-viewport');
            }
        }
        
        // If not in portal, use standard lookup
        if (!viewport) {
            viewport = this.findViewport(choiceElement, activeDropdown);
        }
        
        if (!viewport) return;
        
        const items = Array.from(viewport.querySelectorAll('.choice-item:not([disabled])'));

        switch (event.key) {
            case 'ArrowDown':
                event.preventDefault();
                currentHighlightedIndex = Math.min(currentHighlightedIndex + 1, items.length - 1);
                Choice.highlightItem(items, currentHighlightedIndex);
                break;
            case 'ArrowUp':
                event.preventDefault();
                currentHighlightedIndex = Math.max(currentHighlightedIndex - 1, 0);
                Choice.highlightItem(items, currentHighlightedIndex);
                break;
            case 'Enter':
                event.preventDefault();
                if (currentHighlightedIndex >= 0 && items[currentHighlightedIndex]) {
                    items[currentHighlightedIndex].click();
                }
                break;
            case 'Escape':
                event.preventDefault();
                Choice.closeDropdown(activeDropdown);
                break;
        }
    },

    highlightItem(items, index) {
        items.forEach(item => item.classList.remove('choice-item-highlighted'));

        if (index >= 0 && index < items.length) {
            const item = items[index];
            item.classList.add('choice-item-highlighted');
            item.scrollIntoView({ block: 'nearest' });
        }
    },

    scrollSelectedIntoView(viewport) {
        const selectedItem = viewport.querySelector('.choice-item-active');
        if (selectedItem) {
            selectedItem.scrollIntoView({ block: 'nearest' });
        }
    },


    selectItem(choiceElementId, item) {
        const choiceElement = document.querySelector(`[data-choice-id="${choiceElementId}"]`);
        if (!choiceElement) return;

        const items = choiceElement.querySelectorAll('.choice-item');
        items.forEach(i => i.classList.remove('choice-item-active'));
        item.classList.add('choice-item-active');

        const trigger = choiceElement.querySelector('.choice-trigger');
        const triggerText = trigger.querySelector('.choice-text');
        if (triggerText) {
            triggerText.textContent = item.textContent.trim();
        }

        this.closeDropdown(choiceElementId);
    },
    

    async _waitForPortal(choiceElementId, timeout = 1000) {
        return new Promise((resolve, reject) => {
            const requesterId = `choice-${choiceElementId}`;
            const timeoutId = setTimeout(() => {
                document.removeEventListener(window.RRBlazor.Events.PORTAL_CREATED, handler);
                reject(new Error(`Portal creation timeout for choice ${choiceElementId}`));
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
    }
};

document.addEventListener('click', (event) => {
    // Skip if inside any interactive element
    if (event.target.closest('[data-column-manager]')) return;
    if (event.target.matches('input, button')) return;
    
    const item = event.target.closest('.choice-item');
    if (!item || item.disabled) return;
    
    // Skip if item contains interactive content
    if (item.querySelector('[data-column-manager], input, button')) return;
    
    event.preventDefault();
    const choiceElement = item.closest('[data-choice-id]');
    if (!choiceElement) return;
    
    const choiceId = choiceElement.dataset.choiceId;
    Choice.selectItem(choiceId, item);
});

Choice.initialize();

export default {
    openDropdown: (choiceElementId, options) => Choice.openDropdown(choiceElementId, options),
    closeDropdown: (choiceElementId) => Choice.closeDropdown(choiceElementId),
    isDropdownOpen: (choiceElementId) => activeDropdown === choiceElementId,
    closeAllDropdowns: () => {
        if (activeDropdown) {
            return Choice.closeDropdown(activeDropdown);
        }
        activePortals.clear();
        viewportLocations.clear();
        return Promise.resolve(true);
    },
    initialize: () => Choice.initialize()
};