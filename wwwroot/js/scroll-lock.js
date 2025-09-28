
class ScrollLockManager {
    constructor() {
        this.lockCount = 0;
        this.scrollPosition = 0;
    }
    
    lock() {
        this.lockCount++;
        
        if (this.lockCount === 1) {
            this.scrollPosition = window.pageYOffset;
            document.body.style.top = `-${this.scrollPosition}px`;
            document.body.classList.add('modal-open');
        }
        
        return this.lockCount;
    }
    
    unlock() {
        if (this.lockCount === 0) return 0;
        
        this.lockCount--;
        
        if (this.lockCount === 0) {
            document.body.classList.remove('modal-open');
            document.body.style.top = '';
            window.scrollTo(0, this.scrollPosition);
        }
        
        return this.lockCount;
    }
    
    forceUnlock() {
        this.lockCount = 0;
        document.body.classList.remove('modal-open');
        document.body.style.top = '';
        window.scrollTo(0, this.scrollPosition);
    }
    
    getStatus() {
        return {
            locked: this.lockCount > 0,
            lockCount: this.lockCount
        };
    }
    
    isScrollLocked() {
        return this.lockCount > 0;
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


export function cleanup() {
    forceUnlockScroll();
}