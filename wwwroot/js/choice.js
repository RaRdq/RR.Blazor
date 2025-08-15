
import { PositioningEngine } from './positioning.js';

const positioningEngine = new PositioningEngine();

let activeDropdown = null;
let keyboardNavigationEnabled = false;
let currentHighlightedIndex = -1;
let pendingOperations = new Set();
let activePortals = new Map();
let viewportLocations = new Map();

const Choice = {
    async openDropdown(choiceElementId, options = {}) {
        if (pendingOperations.has(choiceElementId)) return;
        
        pendingOperations.add(choiceElementId);
        try {
            const choiceElement = document.querySelector(`[data-choice-id="${choiceElementId}"]`);
            if (!choiceElement) throw new Error(`Choice element not found: ${choiceElementId}`);
            
            const trigger = choiceElement.querySelector('.choice-trigger');
            if (!trigger) throw new Error(`Trigger not found: ${choiceElementId}`);
            
            const viewport = choiceElement.parentElement?.querySelector('.choice-viewport');
            if (!viewport) throw new Error(`[Choice] Viewport not found for ${choiceElementId}`);
            
            if (activeDropdown && activeDropdown !== choiceElementId) {
                await this.closeDropdown(activeDropdown);
            }

            const portalId = `choice-${choiceElementId}`;
            
            document.dispatchEvent(new CustomEvent('portal-destroy-request', {
                detail: { requesterId: `choice-${choiceElementId}`, portalId: `choice-portal-${choiceElementId}` },
                bubbles: true
            }));

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
            const containerRect = containerElement?.getBoundingClientRect();
            
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
            
            document.dispatchEvent(new CustomEvent('portal-create-request', {
                detail: {
                    requesterId: `choice-${choiceElementId}`,
                    config: {
                        id: `choice-portal-${choiceElementId}`,
                        className: 'choice-portal'
                    }
                },
                bubbles: true
            }));
            
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
            
            window.RRBlazor.ClickOutside.register(`choice-${choiceElementId}`, choiceElement, {
                excludeSelectors: [
                    '.choice-trigger',
                    '.choice-viewport', 
                    '.choice-content',
                    '.choice-portal',
                    `[data-choice-id="${choiceElementId}"]`
                ]
            });

            // Add click-outside listener
            choiceElement.addEventListener('click-outside', (event) => {
                if (event.detail.elementId === `choice-${choiceElementId}`) {
                    this.closeDropdown(choiceElementId);
                }
            });

            this.enableKeyboardNavigation(choiceElementId);
            this.scrollSelectedIntoView(viewport);
            
            return portalId;
        } finally {
            pendingOperations.delete(choiceElementId);
        }
    },

    async closeDropdown(choiceElementId) {
        if (!choiceElementId || activeDropdown !== choiceElementId) return false;

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
            viewport = choiceElement.querySelector('.choice-viewport') || 
                      choiceElement.parentElement?.querySelector('.choice-viewport');
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
        document.dispatchEvent(new CustomEvent('portal-destroy-request', {
                detail: { requesterId: `choice-${choiceElementId}`, portalId: `choice-portal-${choiceElementId}` },
                bubbles: true
            }));

        this.disableKeyboardNavigation();
        window.RRBlazor.ClickOutside.unregister(`choice-${choiceElementId}`);
        activeDropdown = null;
        activePortals.delete(choiceElementId);
        viewportLocations.delete(choiceElementId);
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
            viewport = choiceElement.querySelector('.choice-viewport') || 
                      choiceElement.parentElement?.querySelector('.choice-viewport');
        }
        
        if (viewport) {
            document.dispatchEvent(new CustomEvent('choice-keyboard-enable', {
                detail: { choiceId: choiceElementId, viewport },
                bubbles: true
            }));
        }
        
        keyboardNavigationEnabled = true;
        currentHighlightedIndex = -1;
    },

    disableKeyboardNavigation() {
        document.dispatchEvent(new CustomEvent('choice-keyboard-disable', {
            bubbles: true
        }));
        keyboardNavigationEnabled = false;
        currentHighlightedIndex = -1;
    },

    handleKeyDown(event) {
        if (!keyboardNavigationEnabled || !activeDropdown) return;

        const choiceElement = document.querySelector(`[data-choice-id="${activeDropdown}"]`);
        if (!choiceElement) return;

        let viewport = null;
        
        if (viewportLocations.has(activeDropdown)) {
            const location = viewportLocations.get(activeDropdown);
            if (location) {
                viewport = location.querySelector('.choice-viewport');
            }
        }
        
        if (!viewport) {
            const portalId = activePortals.get(activeDropdown);
            if (portalId) {
                const portal = document.getElementById(portalId);
                if (portal) {
                    viewport = portal.querySelector('.choice-viewport');
                }
            }
        }
        
        if (!viewport) {
            viewport = choiceElement.querySelector('.choice-viewport') || 
                      choiceElement.parentElement?.querySelector('.choice-viewport');
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
        const triggerText = trigger?.querySelector('.choice-text');
        if (triggerText) {
            triggerText.textContent = item.textContent.trim();
        }

        this.closeDropdown(choiceElementId);
    },
    

    async _waitForPortal(choiceElementId, timeout = 1000) {
        return new Promise((resolve, reject) => {
            const requesterId = `choice-${choiceElementId}`;
            const timeoutId = setTimeout(() => {
                document.removeEventListener('portal-created', handler);
                reject(new Error(`Portal creation timeout for choice ${choiceElementId}`));
            }, timeout);
            
            const handler = (event) => {
                if (event.detail.requesterId === requesterId) {
                    clearTimeout(timeoutId);
                    document.removeEventListener('portal-created', handler);
                    resolve(event.detail.portal);
                }
            };
            
            document.addEventListener('portal-created', handler);
        });
    }
};

document.addEventListener('click', (event) => {
    const item = event.target.closest('.choice-item');
    if (item && !item.disabled) {
        event.preventDefault();
        const choiceElement = item.closest('[data-choice-id]');
        if (choiceElement) {
            const choiceId = choiceElement.dataset.choiceId;
            Choice.selectItem(choiceId, item);
        }
        return;
    }
});

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
    }
};