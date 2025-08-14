
class ScrollLockManager {
    constructor() {
        this.lockCount = 0;
        this.originalBodyStyles = {};
        this.isLocked = false;
        this.scrollbarWidth = null;
    }
    
    getScrollbarWidth() {
        if (this.scrollbarWidth !== null) return this.scrollbarWidth;
        
        this.scrollbarWidth = window.innerWidth - document.documentElement.clientWidth;
        return this.scrollbarWidth;
    }
    
    lock() {
        this.lockCount++;
        
        if (this.lockCount === 1 && !this.isLocked) {
            this.performLock();
        }
        
        return this.lockCount;
    }
    
    unlock() {
        if (this.lockCount === 0) {
            throw new Error('Cannot unlock - no active locks');
        }
        
        this.lockCount--;
        
        if (this.lockCount === 0 && this.isLocked) {
            this.performUnlock();
        }
        
        return this.lockCount;
    }
    
    forceUnlock() {
        this.lockCount = 0;
        if (this.isLocked) {
            this.performUnlock();
        }
    }
    
    performLock() {
        this.originalBodyStyles = {
            overflow: document.body.style.overflow,
            paddingRight: document.body.style.paddingRight
        };
        
        const scrollbarWidth = this.getScrollbarWidth();
        
        document.body.classList.add('modal-open');
        document.body.style.overflow = 'hidden';
        
        if (scrollbarWidth > 0) {
            document.body.style.setProperty('--scrollbar-width', `${scrollbarWidth}px`);
            document.body.style.paddingRight = `${scrollbarWidth}px`;
        }
        
        this.isLocked = true;
    }
    
    performUnlock() {
        document.body.classList.remove('modal-open');
        document.body.style.overflow = this.originalBodyStyles.overflow || '';
        document.body.style.paddingRight = this.originalBodyStyles.paddingRight || '';
        document.body.style.removeProperty('--scrollbar-width');
        
        this.isLocked = false;
        this.originalBodyStyles = {};
    }
    
    getStatus() {
        return {
            locked: this.isLocked,
            lockCount: this.lockCount,
            scrollbarWidth: this.scrollbarWidth
        };
    }
    
    isScrollLocked() {
        return this.isLocked;
    }
    
    getLockCount() {
        return this.lockCount;
    }
}

const scrollLockManager = new ScrollLockManager();

export { scrollLockManager, ScrollLockManager };
export default scrollLockManager;
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

export function initialize() {
    return true;
}

export function cleanup() {
    forceUnlockScroll();
}