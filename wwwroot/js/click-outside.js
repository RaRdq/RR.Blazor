
class ClickOutsideManager {
    constructor() {
        this.trackedElements = new Map();
        this.globalHandler = this.handleGlobalClick.bind(this);
        this.initialized = false;
    }
    
    initialize() {
        if (this.initialized) return;
        document.addEventListener('click', this.globalHandler, true);
        this.initialized = true;
    }
    
    register(elementId, element, options = {}) {
        if (!this.initialized) {
            throw new Error('Must initialize before registering elements');
        }
        this.trackedElements.set(elementId, {
            element,
            options,
            excludeSelectors: options.excludeSelectors || []
        });
    }
    
    unregister(elementId) {
        this.trackedElements.delete(elementId);
    }
    
    handleGlobalClick(event) {
        const target = event.target;
        
        this.trackedElements.forEach((data, elementId) => {
            const { element, excludeSelectors } = data;
            
            if (element.contains(target)) return;
            
            for (const selector of excludeSelectors) {
                if (target.closest(selector)) return;
            }
            
            const outsideClickEvent = new CustomEvent('click-outside', {
                detail: { elementId, target, originalEvent: event },
                bubbles: true
            });
            element.dispatchEvent(outsideClickEvent);
        });
    }
    
    destroy() {
        if (this.initialized) {
            document.removeEventListener('click', this.globalHandler, true);
        }
        this.trackedElements.clear();
        this.initialized = false;
    }
}

const clickOutsideManager = new ClickOutsideManager();

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => clickOutsideManager.initialize());
} else {
    clickOutsideManager.initialize();
}

export default clickOutsideManager;

export function registerClickOutside(elementId, element, options) {
    return clickOutsideManager.register(elementId, element, options);
}

export function unregisterClickOutside(elementId) {
    return clickOutsideManager.unregister(elementId);
}