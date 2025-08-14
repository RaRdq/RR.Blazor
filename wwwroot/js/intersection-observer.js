const debugLogger = window.debugLogger;

let observers = new Map();

export function observe(element, dotNetRef, options = {}) {
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

if (!window.IntersectionObserver) {
    throw new Error('IntersectionObserver not supported in this browser');
}