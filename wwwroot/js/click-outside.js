
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
            
            // Check base exclude selectors
            for (const selector of excludeSelectors) {
                if (target.closest(selector)) return;
            }
            
            // Check if target is inside any modal portal (common exclusion)
            if (target.closest('.modal-portal, [data-modal-id], .modal-content')) return;
            
            window.RRBlazor.EventDispatcher.dispatch(
                window.RRBlazor.Events.CLICK_OUTSIDE,
                { elementId, target, originalEvent: event }
            );
            element.dispatchEvent(new Event(window.RRBlazor.Events.CLICK_OUTSIDE));
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

