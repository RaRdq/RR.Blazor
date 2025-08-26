
export class FocusTrap {
    constructor() {
        this.activeTraps = new Map();
        this.previousFocus = null;
    }

    createTrap(modalElement, trapId) {
        if (this.activeTraps.has(trapId)) {
            this.destroyTrap(trapId);
        }

        this.previousFocus = document.activeElement;

        const actualModal = this.findPortaledModal(modalElement, trapId);
        if (!actualModal) {
            return false;
        }

        const focusableElements = this.getFocusableElements(actualModal);
        
        if (focusableElements.length === 0) {
            return false;
        }

        const firstElement = focusableElements[0];
        const lastElement = focusableElements[focusableElements.length - 1];

        const trapHandler = (event) => {
            if (event.key !== 'Tab') return;

            const currentModal = this.findPortaledModal(modalElement, trapId);

            const currentFocusableElements = this.getFocusableElements(currentModal);
            if (currentFocusableElements.length === 0) return;

            const currentFirst = currentFocusableElements[0];
            const currentLast = currentFocusableElements[currentFocusableElements.length - 1];

            const isWithinModal = currentModal.contains(document.activeElement);

            if (event.shiftKey) {
                if (!isWithinModal || document.activeElement === currentFirst) {
                    event.preventDefault();
                    currentLast.focus();
                }
            } else {
  
                if (!isWithinModal || document.activeElement === currentLast) {
                    event.preventDefault();
                    currentFirst.focus();
                }
            }
        };

        this.activeTraps.set(trapId, {
            element: actualModal,
            originalElement: modalElement,
            handler: trapHandler,
            firstElement,
            lastElement
        });

        document.addEventListener('keydown', trapHandler);

        requestAnimationFrame(() => {
            const currentModal = this.findPortaledModal(modalElement, trapId);
            const firstFocusable = this.getFocusableElements(currentModal)[0];
            firstFocusable.focus();
        });

        return true;
    }

    findPortaledModal(originalElement, trapId) {
        let modal = document.querySelector(`#portal-${trapId} [role="dialog"]`);
        
        if (!modal) {
            const modals = document.querySelectorAll('[role="dialog"]');
            modal = Array.from(modals).find(m => {
                if (!m) return false;
                
                const rect = m.getBoundingClientRect();
                return rect.width > 0 && rect.height > 0;
            });
        }
        
        
        return modal;
    }

    destroyTrap(trapId) {
        const trap = this.activeTraps.get(trapId);
        if (!trap) return false;

        document.removeEventListener('keydown', trap.handler);
        
        this.activeTraps.delete(trapId);

        // Find and restore focus from stack
        const focusIndex = this.focusStack.findIndex(f => f.trapId === trapId);
        if (focusIndex !== -1) {
            const focusInfo = this.focusStack[focusIndex];
            this.focusStack.splice(focusIndex, 1);
            
            // Only restore focus if this was the last trap or topmost
            if (this.activeTraps.size === 0 || focusIndex === this.focusStack.length) {
                if (focusInfo.previousFocus && focusInfo.previousFocus.focus) {
                    try {
                        focusInfo.previousFocus.focus();
                    } catch (e) {
                        document.body.focus();
                    }
                }
                
                // Update legacy previousFocus
                if (this.focusStack.length > 0) {
                    this.previousFocus = this.focusStack[this.focusStack.length - 1].previousFocus;
                } else {
                    this.previousFocus = null;
                }
            }
        } else if (this.activeTraps.size === 0 && this.previousFocus) {
            // Fallback to legacy behavior if not in stack
            try {
                this.previousFocus.focus();
            } catch (e) {
                document.body.focus();
            }
            this.previousFocus = null;
        }

        return true;
    }

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

        return elements.filter(element => {
            const style = window.getComputedStyle(element);
            const rect = element.getBoundingClientRect();
            
            const isVisible = style.display !== 'none' &&
                             style.visibility !== 'hidden' &&
                             style.opacity !== '0' &&
                             rect.width > 0 &&
                             rect.height > 0 &&
                             !element.hasAttribute('aria-hidden');

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

    isActive(trapId) {
        return this.activeTraps.has(trapId);
    }

    getActiveTrapCount() {
        return this.activeTraps.size;
    }
}

const globalFocusTrap = new FocusTrap();

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

export { globalFocusTrap };
export default globalFocusTrap;

export function getFocusableElements(container) {
    return globalFocusTrap.getFocusableElements(container);
}

export function isActive(trapId) {
    return globalFocusTrap.isActive(trapId);
}

export function getActiveTrapCount() {
    return globalFocusTrap.getActiveTrapCount();
}

export function destroyAllTraps() {
    return globalFocusTrap.destroyAllTraps();
}

export function initialize(element, dotNetRef) {
    return true;
}

export function cleanup(element) {
    if (element && element.hasAttribute('data-trap-id')) {
        const trapId = element.getAttribute('data-trap-id');
        destroyFocusTrap(trapId);
    } else {
        destroyAllTraps();
    }
}