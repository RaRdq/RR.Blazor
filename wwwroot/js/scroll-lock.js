// scroll-lock.js - Dedicated scroll locking service

class ScrollLockManager {
    constructor() {
        this.lockCount = 0;
        this.originalBodyStyles = {};
        this.isLocked = false;
        this.scrollbarWidth = null;
    }
    
    // Calculate scrollbar width once and cache
    getScrollbarWidth() {
        if (this.scrollbarWidth !== null) return this.scrollbarWidth;
        
        this.scrollbarWidth = window.innerWidth - document.documentElement.clientWidth;
        return this.scrollbarWidth;
    }
    
    // Lock scroll - increment lock count for nested modals
    lock() {
        this.lockCount++;
        
        // Only lock if first call
        if (this.lockCount === 1 && !this.isLocked) {
            this.performLock();
        }
        
        return this.lockCount;
    }
    
    // Unlock scroll - decrement lock count
    unlock() {
        if (this.lockCount === 0) {
            throw new Error('[ScrollLockManager] Cannot unlock - no active locks');
        }
        
        this.lockCount--;
        
        // Only unlock if no more locks
        if (this.lockCount === 0 && this.isLocked) {
            this.performUnlock();
        }
        
        return this.lockCount;
    }
    
    // Force unlock all (for cleanup/error recovery)
    forceUnlock() {
        this.lockCount = 0;
        if (this.isLocked) {
            this.performUnlock();
        }
    }
    
    // Actually perform the scroll lock
    performLock() {
        // Store original body styles
        this.originalBodyStyles = {
            overflow: document.body.style.overflow,
            paddingRight: document.body.style.paddingRight
        };
        
        const scrollbarWidth = this.getScrollbarWidth();
        
        // Apply scroll lock
        document.body.classList.add('modal-open');
        document.body.style.overflow = 'hidden';
        
        // Compensate for scrollbar to prevent layout shift
        if (scrollbarWidth > 0) {
            document.body.style.setProperty('--scrollbar-width', `${scrollbarWidth}px`);
            document.body.style.paddingRight = `${scrollbarWidth}px`;
        }
        
        this.isLocked = true;
    }
    
    // Actually perform the scroll unlock
    performUnlock() {
        // Restore original body styles
        document.body.classList.remove('modal-open');
        document.body.style.overflow = this.originalBodyStyles.overflow || '';
        document.body.style.paddingRight = this.originalBodyStyles.paddingRight || '';
        document.body.style.removeProperty('--scrollbar-width');
        
        this.isLocked = false;
        this.originalBodyStyles = {};
    }
    
    // Get current lock status
    getStatus() {
        return {
            locked: this.isLocked,
            lockCount: this.lockCount,
            scrollbarWidth: this.scrollbarWidth
        };
    }
    
    // Check if currently locked
    isScrollLocked() {
        return this.isLocked;
    }
    
    // Get active lock count
    getLockCount() {
        return this.lockCount;
    }
}

// Create singleton instance
const scrollLockManager = new ScrollLockManager();

// Export for ES6 modules
export { scrollLockManager, ScrollLockManager };
export default scrollLockManager;

// Export individual functions
export function lockScroll() {
    return scrollLockManager.lock();
}

export function unlockScroll() {
    return scrollLockManager.unlock();
}

export function forceUnlockScroll() {
    return scrollLockManager.forceUnlock();
}

export function isScrollLocked() {
    return scrollLockManager.isScrollLocked();
}

export function getScrollLockCount() {
    return scrollLockManager.getLockCount();
}

export function getScrollLockStatus() {
    return scrollLockManager.getStatus();
}

// Required methods for rr-blazor.js proxy system
export function initialize() {
    // Scroll lock system initializes itself, return success
    return true;
}

export function cleanup() {
    forceUnlockScroll();
}