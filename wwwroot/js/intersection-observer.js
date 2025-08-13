// Simple Intersection Observer for load-more functionality
// Replaces complex virtualization logic with native browser API

// Use shared debug logger from RR.Blazor main file
const debugLogger = window.debugLogger || new (window.RRDebugLogger || class {
    constructor() { this.logPrefix = '[IntersectionObserver]'; }
    warn(...args) { console.warn(this.logPrefix, ...args); }
})();

let observers = new Map();

export function observe(element, dotNetRef, options = {}) {
    // Clean up existing observer if it exists
    if (observers.has(element)) {
        observers.get(element).disconnect();
    }
    
    const defaultOptions = {
        rootMargin: '50px',
        threshold: 0.1,
        ...options
    };
    
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                // Use requestAnimationFrame to prevent rapid calls
                if (!element._loadMoreScheduled) {
                    element._loadMoreScheduled = true;
                    requestAnimationFrame(() => {
                        delete element._loadMoreScheduled;
                        dotNetRef.invokeMethodAsync('OnIntersectionChanged', true);
                    });
                }
            }
        });
    }, defaultOptions);
    
    observer.observe(element);
    observers.set(element, observer);
    
    return true;
}

export function disconnect(element) {
    const observer = observers.get(element);
    if (observer) {
        observer.disconnect();
        observers.delete(element);
        
        // Clear any pending timeout
        if (element._loadMoreTimeout) {
            clearTimeout(element._loadMoreTimeout);
            delete element._loadMoreTimeout;
        }
    }
}

export function disconnectAll() {
    observers.forEach(observer => observer.disconnect());
    observers.clear();
}

// Fallback for older browsers without IntersectionObserver
if (typeof window !== 'undefined' && !window.IntersectionObserver) {
    debugLogger.warn('IntersectionObserver not supported, falling back to scroll events');
    
    // Simple polyfill using scroll events
    window.IntersectionObserver = class FallbackObserver {
        constructor(callback, options = {}) {
            this.callback = callback;
            this.options = options;
            this.targets = new Set();
            this.handleScroll = this.handleScroll.bind(this);
        }
        
        observe(target) {
            this.targets.add(target);
            if (this.targets.size === 1) {
                window.addEventListener('scroll', this.handleScroll, { passive: true });
            }
        }
        
        disconnect() {
            this.targets.clear();
            window.removeEventListener('scroll', this.handleScroll);
        }
        
        handleScroll() {
            this.targets.forEach(target => {
                const rect = target.getBoundingClientRect();
                const margin = parseInt(this.options.rootMargin) || 50;
                
                if (rect.top <= window.innerHeight + margin) {
                    this.callback([{ 
                        target, 
                        isIntersecting: true 
                    }]);
                }
            });
        }
    };
}