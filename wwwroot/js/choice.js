import { positioningEngine } from './positioning.js';
import { PortalManager, createPortal } from './portal.js';

export const Choice = {
    activeDropdowns: new Map(),
    activePortals: new Map(),
    
    shouldOpenUpward: function(element) {
        if (!element) throw new Error('[Choice] Element is required');
        
        const rect = element.getBoundingClientRect();
        const viewportHeight = window.innerHeight;
        const estimatedDropdownHeight = 320; // Match max-height from SCSS
        const scrollBuffer = 20;
        
        const spaceBelow = viewportHeight - rect.bottom - scrollBuffer;
        const spaceAbove = rect.top - scrollBuffer;
        
        return spaceBelow < estimatedDropdownHeight && spaceAbove > estimatedDropdownHeight;
    },
    
    calculateOptimalPosition: function(triggerElement, dropdownElement, options = {}) {
        if (!triggerElement) throw new Error('[Choice] Trigger element is required');
        
        const triggerRect = triggerElement.getBoundingClientRect();
        const viewportHeight = window.innerHeight;
        const viewportWidth = window.innerWidth;
        const dropdownHeight = options.estimatedHeight || 320;
        const offset = options.offset || 8;
        const buffer = options.buffer || 8;
        
        // Calculate available space in all directions
        const spaces = {
            above: triggerRect.top - buffer,
            below: viewportHeight - triggerRect.bottom - buffer,
            left: triggerRect.left - buffer,
            right: viewportWidth - triggerRect.right - buffer
        };
        
        // Handle explicit direction from Blazor component
        const explicitDirection = options.dropdownDirection?.toLowerCase();
        
        let direction, x, y, width, maxHeight;
        
        switch (explicitDirection) {
            case 'up':
                return this._positionUp(triggerRect, dropdownHeight, spaces, offset, options);
            case 'down':
                return this._positionDown(triggerRect, dropdownHeight, spaces, offset, options);
            case 'left':
                return this._positionLeft(triggerRect, dropdownHeight, spaces, offset, options);
            case 'right':
                return this._positionRight(triggerRect, dropdownHeight, spaces, offset, options);
            case 'upleft':
                return this._positionUpLeft(triggerRect, dropdownHeight, spaces, offset, options);
            case 'upright':
                return this._positionUpRight(triggerRect, dropdownHeight, spaces, offset, options);
            case 'downleft':
                return this._positionDownLeft(triggerRect, dropdownHeight, spaces, offset, options);
            case 'downright':
                return this._positionDownRight(triggerRect, dropdownHeight, spaces, offset, options);
            case 'auto':
            default:
                // Auto-detection logic - pure geometric calculation
                return this._positionAuto(triggerRect, dropdownHeight, spaces, offset, options);
        }
    },
    
    // Pure geometric positioning functions - container-agnostic
    _positionUp: function(triggerRect, dropdownHeight, spaces, offset, options) {
        const availableHeight = Math.min(dropdownHeight, spaces.above);
        const maxWidth = Math.min(400, window.innerWidth - 16);
        const width = Math.min(Math.max(triggerRect.width, options.minWidth || 200), maxWidth);
        const x = this._constrainHorizontally(triggerRect.left, width, spaces);
        
        return {
            x,
            y: triggerRect.top - availableHeight,
            width,
            maxHeight: Math.max(120, availableHeight),
            direction: 'top-start',
            spaces
        };
    },
    
    _positionDown: function(triggerRect, dropdownHeight, spaces, offset, options) {
        const availableHeight = Math.min(dropdownHeight, spaces.below);
        const maxWidth = Math.min(400, window.innerWidth - 16);
        const width = Math.min(Math.max(triggerRect.width, options.minWidth || 200), maxWidth);
        const x = this._constrainHorizontally(triggerRect.left, width, spaces);
        
        return {
            x,
            y: triggerRect.bottom,
            width,
            maxHeight: Math.max(120, availableHeight),
            direction: 'bottom-start',
            spaces
        };
    },
    
    _positionLeft: function(triggerRect, dropdownHeight, spaces, offset, options) {
        const maxWidth = Math.min(400, window.innerWidth - 16);
        const preferredWidth = Math.min(Math.max(triggerRect.width, options.minWidth || 200), maxWidth);
        const availableWidth = Math.min(preferredWidth, spaces.left - offset);
        const maxHeight = Math.min(dropdownHeight, Math.max(spaces.above, spaces.below));
        
        return {
            x: triggerRect.left - availableWidth - offset,
            y: triggerRect.top,
            width: Math.max(120, availableWidth),
            maxHeight: Math.max(120, maxHeight),
            direction: 'left-start',
            spaces
        };
    },
    
    _positionRight: function(triggerRect, dropdownHeight, spaces, offset, options) {
        const maxWidth = Math.min(400, window.innerWidth - 16);
        const preferredWidth = Math.min(Math.max(triggerRect.width, options.minWidth || 200), maxWidth);
        const availableWidth = Math.min(preferredWidth, spaces.right - offset);
        const maxHeight = Math.min(dropdownHeight, Math.max(spaces.above, spaces.below));
        
        return {
            x: triggerRect.right + offset,
            y: triggerRect.top,
            width: Math.max(120, availableWidth),
            maxHeight: Math.max(120, maxHeight),
            direction: 'right-start',
            spaces
        };
    },
    
    _positionUpLeft: function(triggerRect, dropdownHeight, spaces, offset, options) {
        const availableHeight = Math.min(dropdownHeight, spaces.above - offset);
        const maxWidth = Math.min(400, window.innerWidth - 16);
        const width = Math.min(Math.max(triggerRect.width, options.minWidth || 200), maxWidth);
        
        return {
            x: triggerRect.right - width,
            y: triggerRect.top - availableHeight - offset,
            width,
            maxHeight: Math.max(120, availableHeight),
            direction: 'top-end',
            spaces
        };
    },
    
    _positionUpRight: function(triggerRect, dropdownHeight, spaces, offset, options) {
        const availableHeight = Math.min(dropdownHeight, spaces.above - offset);
        const maxWidth = Math.min(400, window.innerWidth - 16);
        const width = Math.min(Math.max(triggerRect.width, options.minWidth || 200), maxWidth);
        
        return {
            x: triggerRect.left,
            y: triggerRect.top - availableHeight - offset,
            width,
            maxHeight: Math.max(120, availableHeight),
            direction: 'top-start',
            spaces
        };
    },
    
    _positionDownLeft: function(triggerRect, dropdownHeight, spaces, offset, options) {
        const availableHeight = Math.min(dropdownHeight, spaces.below - offset);
        const maxWidth = Math.min(400, window.innerWidth - 16);
        const width = Math.min(Math.max(triggerRect.width, options.minWidth || 200), maxWidth);
        
        return {
            x: triggerRect.right - width,
            y: triggerRect.bottom + offset,
            width,
            maxHeight: Math.max(120, availableHeight),
            direction: 'bottom-end',
            spaces
        };
    },
    
    _positionDownRight: function(triggerRect, dropdownHeight, spaces, offset, options) {
        const availableHeight = Math.min(dropdownHeight, spaces.below - offset);
        const maxWidth = Math.min(400, window.innerWidth - 16);
        const width = Math.min(Math.max(triggerRect.width, options.minWidth || 200), maxWidth);
        
        return {
            x: triggerRect.left,
            y: triggerRect.bottom + offset,
            width,
            maxHeight: Math.max(120, availableHeight),
            direction: 'bottom-start',
            spaces
        };
    },
    
    _positionAuto: function(triggerRect, dropdownHeight, spaces, offset, options) {
        // Smart auto-detection based on available space - no hardcoded context
        
        // Check if there's enough space below (preferred)
        if (spaces.below >= dropdownHeight + offset) {
            return this._positionDown(triggerRect, dropdownHeight, spaces, offset, options);
        }
        
        // Check if there's better space above
        if (spaces.above >= dropdownHeight + offset || spaces.above > spaces.below) {
            return this._positionUp(triggerRect, dropdownHeight, spaces, offset, options);
        }
        
        // Check horizontal options if vertical space is limited
        if (spaces.right >= dropdownHeight && spaces.right > Math.max(spaces.above, spaces.below)) {
            return this._positionRight(triggerRect, dropdownHeight, spaces, offset, options);
        }
        
        if (spaces.left >= dropdownHeight && spaces.left > Math.max(spaces.above, spaces.below)) {
            return this._positionLeft(triggerRect, dropdownHeight, spaces, offset, options);
        }
        
        // Fallback: use the direction with most space
        const maxSpace = Math.max(spaces.above, spaces.below, spaces.left, spaces.right);
        if (maxSpace === spaces.above) {
            return this._positionUp(triggerRect, dropdownHeight, spaces, offset, options);
        } else if (maxSpace === spaces.below) {
            return this._positionDown(triggerRect, dropdownHeight, spaces, offset, options);
        } else if (maxSpace === spaces.right) {
            return this._positionRight(triggerRect, dropdownHeight, spaces, offset, options);
        } else {
            return this._positionLeft(triggerRect, dropdownHeight, spaces, offset, options);
        }
    },
    
    _constrainHorizontally: function(preferredX, width, spaces) {
        // Ensure dropdown stays within viewport horizontally
        const viewportWidth = window.innerWidth;
        const buffer = 8;
        
        let x = preferredX;
        if (x + width > viewportWidth - buffer) {
            x = viewportWidth - width - buffer;
        }
        if (x < buffer) {
            x = buffer;
        }
        
        return x;
    },
    
    createDropdownPortal: function(triggerElement, dropdownElement, options = {}) {
        if (!triggerElement) throw new Error('[Choice] Trigger element is required');
        
        const position = this.calculateOptimalPosition(triggerElement, dropdownElement, options);
        const choiceId = options.choiceId || `choice-${Date.now()}`;
        
        // Use portal system for proper positioning with viewport awareness
        if (dropdownElement && options.portalId) {
            try {
                // Move the dropdown content to the portal
                const portalManager = PortalManager.getInstance();
                const portalContainer = portalManager.getPortal(options.portalId);
                
                if (portalContainer) {
                    if (!dropdownElement._originalParent) {
                        dropdownElement._originalParent = dropdownElement.parentNode;
                        dropdownElement._originalNextSibling = dropdownElement.nextSibling;
                    }
                    
                    // Apply positioning to the portal container
                    portalContainer.style.position = 'fixed';
                    portalContainer.style.left = `${position.x}px`;
                    portalContainer.style.top = `${position.y}px`;
                    portalContainer.style.width = `${position.width}px`;
                    portalContainer.style.maxHeight = `${position.maxHeight}px`;
                    portalContainer.style.zIndex = portalManager.calculateZIndex(options.portalId);
                    portalContainer.style.boxShadow = 'var(--shadow-lg)';
                    portalContainer.style.borderRadius = 'var(--radius-md)';
                    portalContainer.style.backgroundColor = 'var(--color-background-elevated)';
                    portalContainer.style.border = '1px solid var(--color-border-subtle)';
                    portalContainer.style.visibility = 'visible';
                    portalContainer.style.opacity = '1';
                    
                    // Move dropdown element to portal (preserves Blazor event handlers)
                    portalContainer.innerHTML = '';
                    portalContainer.appendChild(dropdownElement);
                    
                    // Hide the original dropdown
                    dropdownElement.style.visibility = 'hidden';
                    dropdownElement.style.opacity = '0';
                    dropdownElement.style.pointerEvents = 'none';
                }
            } catch (error) {
                console.error('[Choice] Portal positioning failed, falling back to direct positioning:', error);
                // Fallback to direct positioning if portal fails
                this.fallbackPositioning(triggerElement, dropdownElement, position);
            }
        } else {
            // Fallback when no portal is available
            this.fallbackPositioning(triggerElement, dropdownElement, position);
        }
        
        return { position };
    },
    
    fallbackPositioning: function(triggerElement, dropdownElement, position) {
        // Portal system failed - use direct CSS positioning as fallback
        if (dropdownElement) {
            dropdownElement.style.position = 'fixed';
            dropdownElement.style.left = `${position.x}px`;
            dropdownElement.style.top = `${position.y}px`;
            dropdownElement.style.width = `${position.width}px`;
            dropdownElement.style.maxHeight = `${position.maxHeight}px`;
            dropdownElement.style.zIndex = '1050';
            dropdownElement.style.boxShadow = 'var(--shadow-lg)';
            dropdownElement.style.borderRadius = 'var(--radius-md)';
            dropdownElement.style.backgroundColor = 'var(--color-background-elevated)';
            dropdownElement.style.border = '1px solid var(--color-border-subtle)';
            dropdownElement.style.visibility = 'visible';
            dropdownElement.style.opacity = '1';
            dropdownElement.style.pointerEvents = 'auto';
        }
        return { position };
    },
    
    applyDynamicPositioning: function(choiceElement, options = {}) {
        if (!choiceElement) throw new Error('[Choice] Choice element is required');
        
        const trigger = choiceElement.querySelector('.choice-trigger');
        const viewport = choiceElement.querySelector('.choice-viewport');
        const choiceId = choiceElement.dataset.choiceId;
        
        if (!trigger) throw new Error('[Choice] Trigger not found');
        if (!viewport) throw new Error('[Choice] Viewport not found');
        if (!choiceId) throw new Error('[Choice] Choice ID is required');
        
        console.log('[Choice] Applying dynamic positioning for:', choiceId, 'options:', options);
        
        const result = this.createDropdownPortal(trigger, viewport, { 
            ...options, 
            choiceId 
        });
        
        console.log('[Choice] Positioning result:', result);
        
        this.activeDropdowns.set(choiceId, {
            element: choiceElement,
            trigger,
            viewport,
            position: result.position,
            portalId: null // No portal used, direct CSS positioning
        });
        
        return result.position;
    },
    
    // Scroll into view for selected item
    scrollSelectedIntoView: function(viewportElement) {
        if (!viewportElement) return;
        
        const selectedItem = viewportElement.querySelector('.choice-item-active');
        if (selectedItem) {
            const viewportRect = viewportElement.getBoundingClientRect();
            const itemRect = selectedItem.getBoundingClientRect();
            
            if (itemRect.top < viewportRect.top || itemRect.bottom > viewportRect.bottom) {
                selectedItem.scrollIntoView({ block: 'nearest', inline: 'nearest' });
            }
        }
    },
    
    // Virtual scrolling support for large lists - Performance target: <100ms for 10k items
    enableVirtualScrolling: function(viewportElement, items, options = {}) {
        if (!viewportElement || !items || items.length <= 100) return null;
        
        const itemHeight = options.itemHeight || 36;
        const containerHeight = options.containerHeight || 320;
        const visibleCount = Math.ceil(containerHeight / itemHeight);
        const bufferCount = Math.min(5, Math.floor(visibleCount * 0.5));
        
        let startIndex = 0;
        let endIndex = Math.min(visibleCount + bufferCount, items.length);
        
        // Create virtual scroll state
        const state = {
            items,
            itemHeight,
            visibleCount,
            bufferCount,
            totalHeight: items.length * itemHeight,
            startIndex,
            endIndex,
            scrollTop: 0,
            isVirtual: true
        };
        
        // Create spacer elements for virtual scrolling
        const topSpacer = document.createElement('div');
        topSpacer.className = 'virtual-scroll-spacer-top';
        topSpacer.style.height = '0px';
        
        const bottomSpacer = document.createElement('div');
        bottomSpacer.className = 'virtual-scroll-spacer-bottom';
        bottomSpacer.style.height = `${state.totalHeight - (endIndex * itemHeight)}px`;
        
        // Container for visible items
        const itemContainer = document.createElement('div');
        itemContainer.className = 'virtual-scroll-items';
        
        // Setup viewport structure
        viewportElement.innerHTML = '';
        viewportElement.appendChild(topSpacer);
        viewportElement.appendChild(itemContainer);
        viewportElement.appendChild(bottomSpacer);
        
        // Render initial items
        this.renderVirtualItems(itemContainer, items, startIndex, endIndex, options);
        
        // Handle scroll events with throttling for performance
        let ticking = false;
        const handleScroll = () => {
            if (!ticking) {
                requestAnimationFrame(() => {
                    this.updateVirtualScroll(viewportElement, state, options);
                    ticking = false;
                });
                ticking = true;
            }
        };
        
        viewportElement.addEventListener('scroll', handleScroll, { passive: true });
        
        // Store state for updates
        viewportElement._virtualState = state;
        
        // Return virtual scroll controller
        return {
            state,
            scrollToItem: (index) => this.scrollToVirtualItem(viewportElement, state, index),
            scrollToTop: () => viewportElement.scrollTop = 0,
            updateItems: (newItems) => this.updateVirtualItems(viewportElement, newItems, options),
            destroy: () => {
                viewportElement.removeEventListener('scroll', handleScroll);
                delete viewportElement._virtualState;
            }
        };
    },
    
    // Render virtual items efficiently
    renderVirtualItems: function(container, items, startIndex, endIndex, options = {}) {
        const fragment = document.createDocumentFragment();
        
        for (let i = startIndex; i < endIndex; i++) {
            const item = items[i];
            if (!item) continue;
            
            const element = document.createElement('div');
            element.className = 'choice-item virtual-item';
            element.dataset.index = i;
            element.dataset.value = item.value || '';
            element.style.height = `${options.itemHeight || 36}px`;
            
            // Use template or render function if provided
            if (options.renderItem) {
                element.innerHTML = options.renderItem(item, i);
            } else {
                element.textContent = item.text || item.label || item.toString();
            }
            
            // Add selection state
            if (item.selected) {
                element.classList.add('selected');
            }
            
            fragment.appendChild(element);
        }
        
        container.innerHTML = '';
        container.appendChild(fragment);
    },
    
    // Update virtual scroll on scroll event
    updateVirtualScroll: function(viewportElement, state, options = {}) {
        const scrollTop = viewportElement.scrollTop;
        const newStartIndex = Math.floor(scrollTop / state.itemHeight);
        const newEndIndex = Math.min(
            newStartIndex + state.visibleCount + (state.bufferCount * 2),
            state.items.length
        );
        
        // Only update if indices changed significantly
        if (Math.abs(newStartIndex - state.startIndex) > state.bufferCount || 
            newEndIndex !== state.endIndex) {
            
            state.startIndex = Math.max(0, newStartIndex - state.bufferCount);
            state.endIndex = newEndIndex;
            state.scrollTop = scrollTop;
            
            // Update spacers
            const topSpacer = viewportElement.querySelector('.virtual-scroll-spacer-top');
            const bottomSpacer = viewportElement.querySelector('.virtual-scroll-spacer-bottom');
            const itemContainer = viewportElement.querySelector('.virtual-scroll-items');
            
            topSpacer.style.height = `${state.startIndex * state.itemHeight}px`;
            bottomSpacer.style.height = `${(state.items.length - state.endIndex) * state.itemHeight}px`;
            
            // Re-render visible items
            this.renderVirtualItems(itemContainer, state.items, state.startIndex, state.endIndex, options);
        }
    },
    
    // Scroll to specific virtual item
    scrollToVirtualItem: function(viewportElement, state, targetIndex) {
        if (targetIndex < 0 || targetIndex >= state.items.length) return;
        
        const targetScrollTop = targetIndex * state.itemHeight;
        viewportElement.scrollTop = targetScrollTop;
        
        // Force update after scroll
        setTimeout(() => {
            this.updateVirtualScroll(viewportElement, state, {});
        }, 16); // Next frame
    },
    
    // Update virtual items data
    updateVirtualItems: function(viewportElement, newItems, options = {}) {
        const state = viewportElement._virtualState;
        if (!state) return;
        
        state.items = newItems;
        state.totalHeight = newItems.length * state.itemHeight;
        state.endIndex = Math.min(state.startIndex + state.visibleCount + state.bufferCount, newItems.length);
        
        // Update spacers and re-render
        this.updateVirtualScroll(viewportElement, state, options);
    }
};

// Enhanced portal management with proper positioning
export async function openDropdown(choiceElementId, portalConfig = {}) {
    const choice = document.querySelector(`[data-choice-id="${choiceElementId}"]`);
    if (!choice) throw new Error(`[Choice] Element not found: ${choiceElementId}`);
    
    const viewport = choice.querySelector('.choice-viewport');
    const trigger = choice.querySelector('.choice-trigger');
    const content = viewport?.querySelector('.choice-content');
    
    if (!viewport || !trigger || !content) {
        throw new Error('[Choice] Required elements not found');
    }
    
    try {
        const position = Choice.applyDynamicPositioning(choice, {
            minWidth: 200,
            preferredPosition: 'bottom-start',
            ...portalConfig
        });
        
        
        // Direct CSS positioning approach - show the viewport
        viewport.style.visibility = 'visible';
        viewport.style.opacity = '1';
        viewport.style.pointerEvents = 'auto';
        
        Choice.scrollSelectedIntoView(content);
        
        const items = content.querySelectorAll('.choice-item');
        const virtualScrollInfo = Choice.enableVirtualScrolling(content, Array.from(items));
        if (virtualScrollInfo?.needed) {
            console.log('[Choice] Virtual scrolling recommended:', virtualScrollInfo);
        }
        
        setupKeyboardNavigation(choiceElementId);
        
        return choiceElementId;
        
    } catch (error) {
        console.error('[Choice] Failed to open dropdown:', error);
        throw error;
    }
}

export async function closeDropdown(choiceElementId) {
    const choice = document.querySelector(`[data-choice-id="${choiceElementId}"]`);
    if (!choice) return false;
    
    const viewport = choice.querySelector('.choice-viewport');
    if (!viewport) return false;
    
    // Clean up portal if it was used and restore dropdown element
    const dropdown = Choice.activeDropdowns.get(choiceElementId);
    if (dropdown?.portalId) {
        try {
            const portalManager = PortalManager.getInstance();
            const portalContainer = portalManager.getPortal(dropdown.portalId);
            if (portalContainer) {
                const dropdownElement = portalContainer.querySelector('.choice-viewport');
                if (dropdownElement && dropdownElement._originalParent) {
                    // Restore element to original position
                    if (dropdownElement._originalNextSibling) {
                        dropdownElement._originalParent.insertBefore(dropdownElement, dropdownElement._originalNextSibling);
                    } else {
                        dropdownElement._originalParent.appendChild(dropdownElement);
                    }
                    
                    // Clean up restoration references
                    delete dropdownElement._originalParent;
                    delete dropdownElement._originalNextSibling;
                }
                
                // Clear portal content
                portalContainer.innerHTML = '';
                portalContainer.style.visibility = 'hidden';
                portalContainer.style.opacity = '0';
            }
            if (portalManager.isPortalActive(dropdown.portalId)) {
                portalManager.destroy(dropdown.portalId);
            }
            Choice.activePortals.delete(choiceElementId);
        } catch (error) {
            console.warn('[Choice] Portal cleanup failed:', error);
        }
    }
    
    // Restore original viewport visibility (in case it was hidden for portal)
    viewport.style.visibility = 'hidden';
    viewport.style.opacity = '0';
    viewport.style.pointerEvents = 'none';
    
    cleanupKeyboardNavigation(choiceElementId);
    Choice.activeDropdowns.delete(choiceElementId);
    
    return true;
}

export function registerClickOutsideCallback(choiceElementId, dotNetRef) {
    const choice = document.querySelector(`[data-choice-id="${choiceElementId}"]`);
    if (!choice) throw new Error(`[Choice] Element not found: ${choiceElementId}`);
    
    // Add delay to prevent immediate click-outside detection from the same click that opened the dropdown
    setTimeout(() => {
        // Remove any existing handler first to prevent duplicates
        if (choice._clickOutsideHandler) {
            document.removeEventListener('click', choice._clickOutsideHandler, true);
        }
        
        const handleClickOutside = (e) => {
            const viewport = choice.querySelector('.choice-viewport');
            const trigger = choice.querySelector('.choice-trigger');
            
            if (!viewport || !trigger) return;
            
            // Check if dropdown is actually open
            const isOpen = viewport.style.visibility === 'visible' && viewport.style.opacity !== '0';
            
            if (isOpen && !trigger.contains(e.target) && !viewport.contains(e.target)) {
                try {
                    dotNetRef.invokeMethodAsync('OnClickOutside');
                } catch (error) {
                    console.warn('[Choice] Click outside callback failed:', error);
                }
            }
        };
        
        choice._clickOutsideHandler = handleClickOutside;
        document.addEventListener('click', handleClickOutside, true);
        
        // Store reference for cleanup
        choice._dotNetRef = dotNetRef;
    }, 50); // 50ms delay to ensure click event has fully propagated
}

// Keyboard navigation support
function setupKeyboardNavigation(choiceElementId) {
    const choice = document.querySelector(`[data-choice-id="${choiceElementId}"]`);
    if (!choice) return;
    
    const content = choice.querySelector('.choice-content');
    if (!content) return;
    
    let currentIndex = -1;
    const items = Array.from(content.querySelectorAll('.choice-item:not([disabled])'));
    const activeItem = content.querySelector('.choice-item-active');
    
    if (activeItem) {
        currentIndex = items.indexOf(activeItem);
    }
    
    const handleKeyDown = (e) => {
        switch (e.key) {
            case 'ArrowDown':
                e.preventDefault();
                currentIndex = Math.min(currentIndex + 1, items.length - 1);
                highlightItem(items[currentIndex]);
                break;
            case 'ArrowUp':
                e.preventDefault();
                currentIndex = Math.max(currentIndex - 1, 0);
                highlightItem(items[currentIndex]);
                break;
            case 'Enter':
                e.preventDefault();
                if (currentIndex >= 0 && items[currentIndex]) {
                    items[currentIndex].click();
                }
                break;
            case 'Escape':
                e.preventDefault();
                closeDropdown(choiceElementId);
                break;
        }
    };
    
    const highlightItem = (item) => {
        if (!item) return;
        
        // Remove previous highlight
        items.forEach(i => i.classList.remove('choice-item-highlighted'));
        
        // Add highlight to current item
        item.classList.add('choice-item-highlighted');
        
        // Scroll into view
        const contentRect = content.getBoundingClientRect();
        const itemRect = item.getBoundingClientRect();
        
        if (itemRect.top < contentRect.top || itemRect.bottom > contentRect.bottom) {
            item.scrollIntoView({ block: 'nearest', inline: 'nearest' });
        }
    };
    
    choice._keyboardHandler = handleKeyDown;
    document.addEventListener('keydown', handleKeyDown);
}

function cleanupKeyboardNavigation(choiceElementId) {
    const choice = document.querySelector(`[data-choice-id="${choiceElementId}"]`);
    if (!choice) return;
    
    if (choice._keyboardHandler) {
        document.removeEventListener('keydown', choice._keyboardHandler);
        delete choice._keyboardHandler;
    }
    
    // Clean up click-outside handler
    if (choice._clickOutsideHandler) {
        document.removeEventListener('click', choice._clickOutsideHandler, true);
        delete choice._clickOutsideHandler;
    }
    
    if (choice._dotNetRef) {
        delete choice._dotNetRef;
    }
}

// Export utility functions for external access
export function positionDropdown(trigger, dropdown, options) {
    return Choice.positionDropdown(trigger, dropdown, options);
}

export function scrollSelectedIntoView(viewport) {
    return Choice.scrollSelectedIntoView(viewport);
}

export function shouldOpenUpward(element) {
    return Choice.shouldOpenUpward(element);
}

export function calculateOptimalPosition(trigger, dropdown, options) {
    return Choice.calculateOptimalPosition(trigger, dropdown, options);
}

export function enableVirtualScrolling(viewportElement, items, options) {
    return Choice.enableVirtualScrolling(viewportElement, items, options);
}

// Required methods for rr-blazor.js proxy system
export function initialize(element, dotNetRef) {
    // Choice system initializes itself, return success
    return true;
}

export function cleanup(element) {
    if (element && element.hasAttribute('data-choice-id')) {
        const choiceId = element.getAttribute('data-choice-id');
        closeDropdown(choiceId);
        cleanupKeyboardNavigation(choiceId);
        
        const portalId = Choice.activePortals.get(choiceId);
        if (portalId) {
            try {
                const portalManager = PortalManager.getInstance();
                if (portalManager.isPortalActive(portalId)) {
                    portalManager.destroy(portalId);
                }
            } catch (error) {
                console.warn('[Choice] Portal cleanup failed during component cleanup:', error);
            }
            Choice.activePortals.delete(choiceId);
        }
    }
}

// Export for module usage
export default {
    Choice,
    openDropdown,
    closeDropdown,
    registerClickOutsideCallback,
    initialize,
    cleanup
};