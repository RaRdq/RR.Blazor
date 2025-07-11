// RVirtualList JavaScript Module
// High-performance virtualized list with auto-loading capabilities

class VirtualListManager {
    constructor(container, dotnetRef, autoLoadMore = true, autoLoadDistance = 100) {
        this.container = container;
        this.dotnetRef = dotnetRef;
        this.autoLoadMore = autoLoadMore;
        this.autoLoadDistance = autoLoadDistance;
        
        this.isScrolling = false;
        this.scrollTimeout = null;
        this.lastScrollTop = 0;
        this.raf = null;
        
        this.bindEvents();
    }
    
    bindEvents() {
        this.handleScroll = this.handleScroll.bind(this);
        this.handleResize = this.handleResize.bind(this);
        
        this.container.addEventListener('scroll', this.handleScroll, { passive: true });
        window.addEventListener('resize', this.handleResize, { passive: true });
    }
    
    handleScroll(event) {
        const scrollTop = this.container.scrollTop;
        const scrollHeight = this.container.scrollHeight;
        const clientHeight = this.container.clientHeight;
        
        // Throttle scroll events using RAF
        if (this.raf) {
            cancelAnimationFrame(this.raf);
        }
        
        this.raf = requestAnimationFrame(() => {
            // Update visible items
            this.dotnetRef.invokeMethodAsync('OnScroll', scrollTop);
            
            // Check if we should load more items
            if (this.autoLoadMore) {
                const distanceFromBottom = scrollHeight - (scrollTop + clientHeight);
                if (distanceFromBottom <= this.autoLoadDistance) {
                    this.dotnetRef.invokeMethodAsync('OnLoadMoreVisible');
                }
            }
        });
        
        this.lastScrollTop = scrollTop;
    }
    
    handleResize() {
        // Debounce resize events
        if (this.scrollTimeout) {
            clearTimeout(this.scrollTimeout);
        }
        
        this.scrollTimeout = setTimeout(() => {
            const scrollTop = this.container.scrollTop;
            this.dotnetRef.invokeMethodAsync('OnScroll', scrollTop);
        }, 100);
    }
    
    scrollToItem(index, itemHeight) {
        const scrollTop = index * itemHeight;
        this.container.scrollTo({
            top: scrollTop,
            behavior: 'smooth'
        });
    }
    
    scrollToTop() {
        this.container.scrollTo({
            top: 0,
            behavior: 'smooth'
        });
    }
    
    dispose() {
        if (this.raf) {
            cancelAnimationFrame(this.raf);
        }
        
        if (this.scrollTimeout) {
            clearTimeout(this.scrollTimeout);
        }
        
        this.container.removeEventListener('scroll', this.handleScroll);
        window.removeEventListener('resize', this.handleResize);
        
        this.container = null;
        this.dotnetRef = null;
    }
}

// Module storage for active managers
const managers = new Map();

// Public API
export function initialize(container, dotnetRef, autoLoadMore = true, autoLoadDistance = 100) {
    // Clean up existing manager if it exists
    if (managers.has(container)) {
        managers.get(container).dispose();
    }
    
    // Create new manager
    const manager = new VirtualListManager(container, dotnetRef, autoLoadMore, autoLoadDistance);
    managers.set(container, manager);
    
    return true;
}

export function scrollToItem(container, index, itemHeight) {
    const manager = managers.get(container);
    if (manager) {
        manager.scrollToItem(index, itemHeight);
    }
}

export function scrollToTop(container) {
    const manager = managers.get(container);
    if (manager) {
        manager.scrollToTop();
    }
}

export function dispose(container) {
    const manager = managers.get(container);
    if (manager) {
        manager.dispose();
        managers.delete(container);
    }
}

// Enhanced scroll performance utilities
export function createIntersectionObserver(container, callback, options = {}) {
    const defaultOptions = {
        root: container,
        rootMargin: '100px',
        threshold: 0.1,
        ...options
    };
    
    return new IntersectionObserver(callback, defaultOptions);
}

export function debounce(func, wait, immediate = false) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            timeout = null;
            if (!immediate) func(...args);
        };
        const callNow = immediate && !timeout;
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
        if (callNow) func(...args);
    };
}

export function throttle(func, limit) {
    let inThrottle;
    return function(...args) {
        if (!inThrottle) {
            func.apply(this, args);
            inThrottle = true;
            setTimeout(() => inThrottle = false, limit);
        }
    };
}

// Performance monitoring for development
export function measurePerformance(name, fn) {
    if (typeof performance !== 'undefined' && performance.mark) {
        performance.mark(`${name}-start`);
        const result = fn();
        performance.mark(`${name}-end`);
        performance.measure(name, `${name}-start`, `${name}-end`);
        return result;
    }
    return fn();
}

// Utility for calculating optimal item heights
export function calculateOptimalItemHeight(container, items, maxHeight = 200) {
    const containerHeight = container.clientHeight;
    const itemCount = items.length;
    
    // Calculate ideal height based on container size
    const idealHeight = Math.floor(containerHeight / Math.min(itemCount, 5));
    
    // Clamp to reasonable bounds
    return Math.max(60, Math.min(idealHeight, maxHeight));
}

// Accessibility helpers
export function announceToScreenReader(message) {
    const announcement = document.createElement('div');
    announcement.setAttribute('aria-live', 'polite');
    announcement.setAttribute('aria-atomic', 'true');
    announcement.className = 'sr-only';
    announcement.textContent = message;
    
    document.body.appendChild(announcement);
    
    setTimeout(() => {
        document.body.removeChild(announcement);
    }, 1000);
}

// Export for global access if needed
if (typeof window !== 'undefined') {
    window.RRVirtualList = {
        initialize,
        scrollToItem,
        scrollToTop,
        dispose,
        createIntersectionObserver,
        debounce,
        throttle,
        measurePerformance,
        calculateOptimalItemHeight,
        announceToScreenReader
    };
}