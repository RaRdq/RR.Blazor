// event-manager.js - Centralized event management with delegation patterns

class EventManager {
    constructor() {
        this.globalListeners = new Map();
        this.delegationMap = new Map();
        this.initialized = false;
        this.stats = {
            totalListeners: 0,
            delegatedEvents: 0,
            directListeners: 0
        };
    }
    
    // Initialize global event delegation - only called once
    initialize() {
        if (this.initialized) {
            throw new Error('[EventManager] Already initialized');
        }
        
        // Setup 4 core global listeners using event delegation
        this.setupGlobalClickHandler();
        this.setupGlobalKeyHandler();
        this.setupGlobalFocusHandler();
        this.setupGlobalScrollHandler();
        
        this.initialized = true;
        this.stats.totalListeners = 4; // Keep count for monitoring
    }
    
    // Global click delegation - handles all click events via delegation
    setupGlobalClickHandler() {
        const clickHandler = (event) => {
            const target = event.target;
            
            // Modal backdrop clicks
            if (target.classList.contains('modal-backdrop') || target.classList.contains('portal-backdrop')) {
                this.handleBackdropClick(event);
                return;
            }
            
            // Choice/dropdown clicks
            if (target.closest('.choice-trigger')) {
                this.handleChoiceToggle(event);
                return;
            }
            
            if (target.closest('.choice-item')) {
                this.handleChoiceSelection(event);
                return;
            }
            
            // Button clicks with data attributes
            if (target.closest('[data-action]')) {
                this.handleActionButton(event);
                return;
            }
            
            // Close dropdowns/modals when clicking outside
            if (!target.closest('.choice-viewport, .modal-content, .dropdown-content, .tooltip')) {
                this.handleOutsideClick(event);
            }
        };
        
        document.addEventListener('click', clickHandler, true);
        this.globalListeners.set('click', clickHandler);
        this.stats.delegatedEvents++;
    }
    
    // Global keyboard delegation
    setupGlobalKeyHandler() {
        const keyHandler = (event) => {
            const { key, target } = event;
            
            // Escape key handling
            if (key === 'Escape') {
                this.handleEscapeKey(event);
                return;
            }
            
            // Arrow navigation in dropdowns/choices
            if (['ArrowUp', 'ArrowDown', 'Enter', ' '].includes(key)) {
                if (target.closest('.choice-viewport, .dropdown-content')) {
                    this.handleChoiceNavigation(event);
                    return;
                }
            }
            
            // Tab navigation
            if (key === 'Tab') {
                this.handleTabNavigation(event);
            }
        };
        
        document.addEventListener('keydown', keyHandler, true);
        this.globalListeners.set('keydown', keyHandler);
        this.stats.delegatedEvents++;
    }
    
    // Global focus delegation
    setupGlobalFocusHandler() {
        const focusHandler = (event) => {
            const target = event.target;
            
            // Focus trap handling in modals
            if (target.closest('.modal-content')) {
                this.handleModalFocus(event);
            }
            
            // Choice trigger focus
            if (target.classList.contains('choice-trigger')) {
                this.handleChoiceFocus(event);
            }
        };
        
        document.addEventListener('focusin', focusHandler, true);
        this.globalListeners.set('focusin', focusHandler);
        this.stats.delegatedEvents++;
    }
    
    // Global scroll delegation with throttling
    setupGlobalScrollHandler() {
        let ticking = false;
        const scrollHandler = (event) => {
            if (!ticking) {
                requestAnimationFrame(() => {
                    // Handle virtual scrolling updates
                    const target = event.target;
                    if (target._virtualState) {
                        // Virtual scroll is handled by choice.js directly
                        // This is just for coordination
                    }
                    
                    // Handle sticky headers, parallax, etc.
                    this.handleScrollEffects(event);
                    ticking = false;
                });
                ticking = true;
            }
        };
        
        document.addEventListener('scroll', scrollHandler, { passive: true, capture: true });
        this.globalListeners.set('scroll', scrollHandler);
        this.stats.delegatedEvents++;
    }
    
    // Handle backdrop clicks
    handleBackdropClick(event) {
        const backdrop = event.target;
        const modalId = backdrop.dataset.modalId || backdrop.dataset.backdropId;
        
        if (modalId) {
            // Emit custom event for modal to handle
            const closeEvent = new CustomEvent('backdrop-click', {
                detail: { modalId },
                bubbles: false,
                cancelable: true
            });
            
            backdrop.dispatchEvent(closeEvent);
            
            if (!closeEvent.defaultPrevented) {
                // Default behavior - close modal
                import('./modal.js').then(({ destroyModal }) => {
                    destroyModal(modalId);
                });
            }
        }
    }
    
    // Handle choice toggle
    handleChoiceToggle(event) {
        event.preventDefault();
        const trigger = event.target.closest('.choice-trigger');
        const choiceId = trigger.dataset.choiceId;
        
        if (choiceId) {
            import('./choice.js').then(({ openDropdown, closeDropdown }) => {
                const viewport = trigger.parentElement.querySelector('.choice-viewport');
                const isOpen = viewport.style.visibility === 'visible';
                
                if (isOpen) {
                    closeDropdown(choiceId);
                } else {
                    openDropdown(choiceId);
                }
            });
        }
    }
    
    // Handle choice item selection
    handleChoiceSelection(event) {
        const item = event.target.closest('.choice-item');
        const value = item.dataset.value;
        const index = parseInt(item.dataset.index);
        
        // Emit selection event
        const selectionEvent = new CustomEvent('choice-select', {
            detail: { value, index, item },
            bubbles: true,
            cancelable: false
        });
        
        item.dispatchEvent(selectionEvent);
    }
    
    // Handle action buttons with data-action attribute
    handleActionButton(event) {
        const button = event.target.closest('[data-action]');
        const action = button.dataset.action;
        const params = button.dataset.params ? JSON.parse(button.dataset.params) : {};
        
        // Emit action event
        const actionEvent = new CustomEvent('button-action', {
            detail: { action, params, button },
            bubbles: true,
            cancelable: true
        });
        
        button.dispatchEvent(actionEvent);
        
        if (!actionEvent.defaultPrevented) {
            // Handle common actions
            switch (action) {
                case 'close-modal':
                    import('./modal.js').then(({ destroyModal }) => {
                        destroyModal(params.modalId);
                    });
                    break;
                case 'toggle-dropdown':
                    this.handleChoiceToggle(event);
                    break;
            }
        }
    }
    
    // Handle outside clicks to close dropdowns
    handleOutsideClick(event) {
        // Defer outside click detection to next event cycle to avoid race condition
        // with async choice.js imports in handleChoiceToggle
        setTimeout(() => {
            // Find all open dropdowns and close them
            const openDropdowns = document.querySelectorAll('.choice-viewport[style*="visible"]');
            openDropdowns.forEach(viewport => {
                const trigger = viewport.parentElement.querySelector('.choice-trigger');
                const choiceId = trigger?.dataset.choiceId;
                
                if (choiceId) {
                    import('./choice.js').then(({ closeDropdown }) => {
                        closeDropdown(choiceId);
                    });
                }
            });
        }, 0);
    }
    
    // Handle Escape key
    handleEscapeKey(event) {
        // Close top-most modal first
        import('./modal.js').then(({ destroyModal }) => {
            const activeModals = document.querySelectorAll('.modal-portal');
            if (activeModals.length > 0) {
                const topModal = activeModals[activeModals.length - 1];
                const modalId = topModal.dataset.modalId;
                if (modalId) {
                    destroyModal(modalId);
                    event.preventDefault();
                    return;
                }
            }
            
            // Then close dropdowns
            this.handleOutsideClick(event);
        });
    }
    
    // Handle choice navigation with arrow keys
    handleChoiceNavigation(event) {
        const { key } = event;
        const viewport = event.target.closest('.choice-viewport');
        const items = viewport.querySelectorAll('.choice-item');
        const selectedItem = viewport.querySelector('.choice-item.selected, .choice-item.active');
        
        let nextIndex = 0;
        
        if (selectedItem) {
            const currentIndex = Array.from(items).indexOf(selectedItem);
            
            switch (key) {
                case 'ArrowUp':
                    nextIndex = Math.max(0, currentIndex - 1);
                    event.preventDefault();
                    break;
                case 'ArrowDown':
                    nextIndex = Math.min(items.length - 1, currentIndex + 1);
                    event.preventDefault();
                    break;
                case 'Enter':
                case ' ':
                    selectedItem.click();
                    event.preventDefault();
                    return;
            }
        }
        
        // Update selection
        items.forEach((item, index) => {
            item.classList.toggle('active', index === nextIndex);
        });
        
        // Scroll into view
        if (items[nextIndex]) {
            items[nextIndex].scrollIntoView({ block: 'nearest' });
        }
    }
    
    // Handle tab navigation
    handleTabNavigation(event) {
        // Focus trap logic for modals is handled by focus-trap.js
        // This is just for coordination
    }
    
    // Handle modal focus
    handleModalFocus(event) {
        // Coordination with focus-trap.js
    }
    
    // Handle choice focus
    handleChoiceFocus(event) {
        const trigger = event.target;
        trigger.classList.add('focused');
        
        // Remove on blur
        const removeFocus = () => {
            trigger.classList.remove('focused');
            trigger.removeEventListener('blur', removeFocus);
        };
        
        trigger.addEventListener('blur', removeFocus);
    }
    
    // Handle scroll effects
    handleScrollEffects(event) {
        // Placeholder for scroll-based effects like sticky headers, parallax
        // Can be extended as needed
    }
    
    // Add a direct event listener (avoid if possible)
    addDirectListener(element, event, handler, options = {}) {
        if (!this.initialized) {
            throw new Error('[EventManager] Must initialize before adding direct listeners');
        }
        
        element.addEventListener(event, handler, options);
        this.stats.directListeners++;
        
        console.warn(`[EventManager] Direct listener added. Total: ${this.stats.directListeners}. Consider delegation instead.`);
    }
    
    // Get statistics
    getStats() {
        return {
            ...this.stats,
            initialized: this.initialized,
            globalListeners: this.globalListeners.size
        };
    }
    
    // Cleanup
    destroy() {
        this.globalListeners.forEach((handler, eventType) => {
            document.removeEventListener(eventType, handler, true);
        });
        
        this.globalListeners.clear();
        this.delegationMap.clear();
        this.initialized = false;
        this.stats = { totalListeners: 0, delegatedEvents: 0, directListeners: 0 };
    }
}

// Create singleton instance
const eventManager = new EventManager();

// Auto-initialize when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        eventManager.initialize();
    });
} else {
    eventManager.initialize();
}

// Export for ES6 modules
export { eventManager, EventManager };
export default eventManager;

// Export utility functions
export function addDirectListener(element, event, handler, options) {
    return eventManager.addDirectListener(element, event, handler, options);
}

export function getEventStats() {
    return eventManager.getStats();
}

export function isInitialized() {
    return eventManager.initialized;
}

// Required methods for rr-blazor.js proxy system
export function initialize() {
    return eventManager.initialize();
}

export function cleanup() {
    eventManager.destroy();
}