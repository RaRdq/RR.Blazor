// Focus Trap System - WCAG 2.4.3 Compliance
// Ensures keyboard navigation stays within modal boundaries

export class FocusTrap {
    constructor() {
        this.activeTraps = new Map();
        this.previousFocus = null;
    }

    // Create focus trap for modal
    createTrap(modalElement, trapId) {
        if (this.activeTraps.has(trapId)) {
            this.destroyTrap(trapId);
        }

        // Store previously focused element
        this.previousFocus = document.activeElement;

        // For portaled modals, find the actual modal in the DOM by role
        const actualModal = this.findPortaledModal(modalElement, trapId);
        if (!actualModal) {
            console.warn(`[FocusTrap-${trapId}] Could not find portaled modal`);
            return false;
        }

        // Get all focusable elements within the actual modal
        const focusableElements = this.getFocusableElements(actualModal);
        
        if (focusableElements.length === 0) {
            console.warn(`[FocusTrap-${trapId}] No focusable elements found in modal`);
            return false;
        }

        const firstElement = focusableElements[0];
        const lastElement = focusableElements[focusableElements.length - 1];

        // Focus trap handler
        const trapHandler = (event) => {
            // Only handle Tab key
            if (event.key !== 'Tab') return;

            // Re-find modal in case DOM changed
            const currentModal = this.findPortaledModal(modalElement, trapId);
            if (!currentModal) return;

            const currentFocusableElements = this.getFocusableElements(currentModal);
            if (currentFocusableElements.length === 0) return;

            const currentFirst = currentFocusableElements[0];
            const currentLast = currentFocusableElements[currentFocusableElements.length - 1];

            // Check if focus is within the modal
            const isWithinModal = currentModal.contains(document.activeElement);

            if (event.shiftKey) {
                // Shift + Tab (backwards)
                if (!isWithinModal || document.activeElement === currentFirst) {
                    event.preventDefault();
                    currentLast?.focus();
                }
            } else {
                // Tab (forwards)  
                if (!isWithinModal || document.activeElement === currentLast) {
                    event.preventDefault();
                    currentFirst?.focus();
                }
            }
        };

        // Store trap data
        this.activeTraps.set(trapId, {
            element: actualModal,
            originalElement: modalElement,
            handler: trapHandler,
            firstElement,
            lastElement
        });

        // Add event listener
        document.addEventListener('keydown', trapHandler);

        // Focus first element after a delay to ensure rendering
        setTimeout(() => {
            const currentModal = this.findPortaledModal(modalElement, trapId);
            if (currentModal) {
                const firstFocusable = this.getFocusableElements(currentModal)[0];
                if (firstFocusable) {
                    firstFocusable.focus();
                }
            }
        }, 150);

        return true;
    }

    // Find the actual portaled modal in the DOM
    findPortaledModal(originalElement, trapId) {
        // Look for portaled modal by trapId first
        let modal = document.querySelector(`#portal-${trapId} [role="dialog"]`);
        
        // Fallback: look for any visible modal dialog
        if (!modal) {
            const modals = document.querySelectorAll('[role="dialog"]');
            modal = Array.from(modals).find(m => {
                const rect = m.getBoundingClientRect();
                return rect.width > 0 && rect.height > 0; // Visible modal
            });
        }
        
        return modal;
    }

    // Destroy focus trap
    destroyTrap(trapId) {
        const trap = this.activeTraps.get(trapId);
        if (!trap) return false;

        // Remove event listener
        document.removeEventListener('keydown', trap.handler);
        
        // Clear from active traps
        this.activeTraps.delete(trapId);

        // Restore previous focus if no other traps are active
        if (this.activeTraps.size === 0 && this.previousFocus) {
            try {
                this.previousFocus.focus();
            } catch (e) {
                // Element may no longer exist
                document.body.focus();
            }
            this.previousFocus = null;
        }

        return true;
    }

    // Get all focusable elements within container
    getFocusableElements(container) {
        const focusableSelectors = [
            'button:not([disabled])',
            'input:not([disabled])',
            'select:not([disabled])',
            'textarea:not([disabled])',
            'a[href]',
            '[tabindex]:not([tabindex="-1"])',
            '[contenteditable="true"]',
            'summary',
            'details[open] summary',
            '[role="button"]:not([disabled])',  // RCard with Clickable=true
            '[role="tab"]:not([disabled])',
            '[role="menuitem"]:not([disabled])'
        ].join(',');

        const elements = Array.from(container.querySelectorAll(focusableSelectors));

        // Filter out hidden elements and ensure they're actually focusable
        return elements.filter(element => {
            const style = window.getComputedStyle(element);
            const rect = element.getBoundingClientRect();
            
            // Check if element is visible
            const isVisible = style.display !== 'none' &&
                             style.visibility !== 'hidden' &&
                             style.opacity !== '0' &&
                             rect.width > 0 &&
                             rect.height > 0 &&
                             !element.hasAttribute('aria-hidden');

            // Check if element is actually focusable
            const isFocusable = element.tabIndex >= 0 || 
                               element.tagName === 'BUTTON' ||
                               element.tagName === 'INPUT' ||
                               element.tagName === 'SELECT' ||
                               element.tagName === 'TEXTAREA' ||
                               element.tagName === 'A' ||
                               element.hasAttribute('contenteditable');
            
            return isVisible && isFocusable;
        });
    }

    // Check if trap is active
    isActive(trapId) {
        return this.activeTraps.has(trapId);
    }

    // Get active trap count
    getActiveTrapCount() {
        return this.activeTraps.size;
    }
}

// Global focus trap instance
const globalFocusTrap = new FocusTrap();

// Export functions for Blazor integration
export function createFocusTrap(modalElement, trapId) {
    return globalFocusTrap.createTrap(modalElement, trapId);
}

export function destroyFocusTrap(trapId) {
    return globalFocusTrap.destroyTrap(trapId);
}

export function isFocusTrapActive(trapId) {
    return globalFocusTrap.isActive(trapId);
}

export function getActiveFocusTrapCount() {
    return globalFocusTrap.getActiveTrapCount();
}