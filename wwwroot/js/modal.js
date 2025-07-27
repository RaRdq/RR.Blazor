// RR.Blazor Modal Utilities
// Handles scroll locking and modal-specific behavior

class ModalManager {
    constructor() {
        this.scrollLocked = false;
        this.originalBodyStyles = {};
        this.activeModals = new Set();
    }
    
    // Lock scroll when modal opens
    lockScroll() {
        if (this.scrollLocked) return;
        
        // Store original body styles
        this.originalBodyStyles = {
            overflow: document.body.style.overflow,
            paddingRight: document.body.style.paddingRight
        };
        
        // Calculate scrollbar width
        const scrollbarWidth = window.innerWidth - document.documentElement.clientWidth;
        
        // Apply scroll lock
        document.body.classList.add('modal-open');
        document.body.style.overflow = 'hidden';
        
        // Compensate for scrollbar to prevent layout shift
        if (scrollbarWidth > 0) {
            document.body.style.setProperty('--scrollbar-width', `${scrollbarWidth}px`);
            document.body.style.paddingRight = `${scrollbarWidth}px`;
        }
        
        this.scrollLocked = true;
    }
    
    // Unlock scroll when all modals close
    unlockScroll() {
        if (!this.scrollLocked) return;
        
        // Restore original body styles
        document.body.classList.remove('modal-open');
        document.body.style.overflow = this.originalBodyStyles.overflow || '';
        document.body.style.paddingRight = this.originalBodyStyles.paddingRight || '';
        document.body.style.removeProperty('--scrollbar-width');
        
        this.scrollLocked = false;
    }
    
    // Register a modal
    register(modalId, options = {}) {
        this.activeModals.add(modalId);
        
        // Lock scroll on first modal
        if (this.activeModals.size === 1) {
            this.lockScroll();
        }
        
        return modalId;
    }
    
    // Unregister a modal
    unregister(modalId) {
        this.activeModals.delete(modalId);
        
        // Unlock scroll when all modals are closed
        if (this.activeModals.size === 0) {
            this.unlockScroll();
        }
    }
    
    // Get count of active modals
    getActiveCount() {
        return this.activeModals.size;
    }
    
    // Check if specific modal is active
    isActive(modalId) {
        return this.activeModals.has(modalId);
    }
    
    // Force unlock (for cleanup)
    forceUnlock() {
        this.activeModals.clear();
        this.unlockScroll();
    }
}

// Create singleton instance
const modalManager = new ModalManager();

// Export for ES6 modules
export { modalManager, ModalManager };

// Export individual functions
export function lockScroll() {
    return modalManager.lockScroll();
}

export function unlockScroll() {
    return modalManager.unlockScroll();
}

export function register(modalId, options) {
    return modalManager.register(modalId, options);
}

export function unregister(modalId) {
    return modalManager.unregister(modalId);
}

// Window exports for non-module usage
window.RRModalManager = modalManager;

// Also export as RRBlazor.Modal for consistency with other modules
window.RRBlazor = window.RRBlazor || {};
window.RRBlazor.Modal = {
    register: (modalId, options) => modalManager.register(modalId, options),
    unregister: (modalId) => modalManager.unregister(modalId),
    lockScroll: () => modalManager.lockScroll(),
    unlockScroll: () => modalManager.unlockScroll(),
    getActiveCount: () => modalManager.getActiveCount(),
    isActive: (modalId) => modalManager.isActive(modalId),
    forceUnlock: () => modalManager.forceUnlock()
};