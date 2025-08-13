// choice-events.js - Choice Event Coordination (SRP)
// Responsibility: Bridge choice layer with infrastructure layers via events

class ChoiceEventCoordinator {
    constructor() {
        this.setupInfrastructureEventHandlers();
    }
    
    setupInfrastructureEventHandlers() {
        // Listen for infrastructure responses
        document.addEventListener('portal-created', (event) => {
            const { requesterId, portal } = event.detail;
            if (requesterId.startsWith('choice-')) {
                this.handlePortalCreated(requesterId, portal);
            }
        });
        
        document.addEventListener('portal-destroyed', (event) => {
            const { requesterId, portalId } = event.detail;
            if (requesterId?.startsWith('choice-') || portalId?.startsWith('choice-')) {
                this.handlePortalDestroyed(requesterId || portalId);
            }
        });
        
        // Listen for click-outside events
        document.addEventListener('click-outside', (event) => {
            const { elementId } = event.detail;
            if (elementId.startsWith('choice-')) {
                this.handleClickOutside(elementId);
            }
        });
        
        // Listen for keyboard navigation events
        document.addEventListener('keyboard-select', (event) => {
            const { elementId, item, index } = event.detail;
            if (elementId.startsWith('choice-')) {
                this.handleKeyboardSelect(elementId, item, index);
            }
        });
        
        document.addEventListener('keyboard-escape', (event) => {
            const { elementId } = event.detail;
            if (elementId.startsWith('choice-')) {
                this.handleKeyboardEscape(elementId);
            }
        });
    }
    
    // Request portal creation for choice
    requestPortal(choiceId, config) {
        const portalRequest = new CustomEvent('portal-create-request', {
            detail: {
                requesterId: `choice-${choiceId}`,
                config: {
                    id: `choice-${choiceId}`,
                    className: 'choice-portal',
                    ...config
                }
            },
            bubbles: true
        });
        document.dispatchEvent(portalRequest);
    }
    
    // Request portal destruction for choice
    destroyPortal(choiceId) {
        const destroyRequest = new CustomEvent('portal-destroy-request', {
            detail: {
                requesterId: `choice-${choiceId}`,
                portalId: `choice-${choiceId}`
            },
            bubbles: true
        });
        document.dispatchEvent(destroyRequest);
    }
    
    // Register choice for click-outside detection
    registerClickOutside(choiceId, element, viewportElement) {
        const clickOutsideEvent = new CustomEvent('register-click-outside', {
            detail: {
                elementId: `choice-${choiceId}`,
                element,
                excludeSelectors: [
                    '.choice-trigger',
                    '.choice-viewport', 
                    '.choice-content',
                    '.choice-portal',
                    `[data-choice-id="${choiceId}"]`
                ]
            },
            bubbles: true
        });
        document.dispatchEvent(clickOutsideEvent);
    }
    
    // Unregister choice from click-outside detection
    unregisterClickOutside(choiceId) {
        const unregisterEvent = new CustomEvent('unregister-click-outside', {
            detail: {
                elementId: `choice-${choiceId}`
            },
            bubbles: true
        });
        document.dispatchEvent(unregisterEvent);
    }
    
    // Enable keyboard navigation for choice
    enableKeyboardNavigation(choiceId, viewport) {
        const enableNavEvent = new CustomEvent('enable-keyboard-navigation', {
            detail: {
                elementId: `choice-${choiceId}`,
                container: viewport,
                itemSelector: '.choice-item:not([disabled])',
                highlightClass: 'choice-item-highlighted'
            },
            bubbles: true
        });
        document.dispatchEvent(enableNavEvent);
    }
    
    // Disable keyboard navigation
    disableKeyboardNavigation() {
        const disableNavEvent = new CustomEvent('disable-keyboard-navigation', {
            bubbles: true
        });
        document.dispatchEvent(disableNavEvent);
    }
    
    // Handle infrastructure responses
    handlePortalCreated(choiceId, portal) {
        const choicePortalReadyEvent = new CustomEvent('choice-portal-ready', {
            detail: { choiceId: choiceId.replace('choice-', ''), portal },
            bubbles: true
        });
        document.dispatchEvent(choicePortalReadyEvent);
    }
    
    handlePortalDestroyed(choiceId) {
        const choicePortalDestroyedEvent = new CustomEvent('choice-portal-destroyed', {
            detail: { choiceId: choiceId.replace('choice-', '') },
            bubbles: true
        });
        document.dispatchEvent(choicePortalDestroyedEvent);
    }
    
    handleClickOutside(choiceId) {
        const choiceClickOutsideEvent = new CustomEvent('choice-click-outside', {
            detail: { choiceId: choiceId.replace('choice-', '') },
            bubbles: true
        });
        document.dispatchEvent(choiceClickOutsideEvent);
    }
    
    handleKeyboardSelect(choiceId, item, index) {
        const choiceSelectEvent = new CustomEvent('choice-keyboard-select', {
            detail: { 
                choiceId: choiceId.replace('choice-', ''),
                item,
                index
            },
            bubbles: true
        });
        document.dispatchEvent(choiceSelectEvent);
    }
    
    handleKeyboardEscape(choiceId) {
        const choiceEscapeEvent = new CustomEvent('choice-keyboard-escape', {
            detail: { choiceId: choiceId.replace('choice-', '') },
            bubbles: true
        });
        document.dispatchEvent(choiceEscapeEvent);
    }
}

// Create singleton
const choiceEventCoordinator = new ChoiceEventCoordinator();

export default choiceEventCoordinator;

// Export functions for choice.js to use
export function requestChoicePortal(choiceId, config) {
    return choiceEventCoordinator.requestPortal(choiceId, config);
}

export function destroyChoicePortal(choiceId) {
    return choiceEventCoordinator.destroyPortal(choiceId);
}

export function registerChoiceClickOutside(choiceId, element, viewportElement) {
    return choiceEventCoordinator.registerClickOutside(choiceId, element, viewportElement);
}

export function unregisterChoiceClickOutside(choiceId) {
    return choiceEventCoordinator.unregisterClickOutside(choiceId);
}

export function enableChoiceKeyboardNavigation(choiceId, viewport) {
    return choiceEventCoordinator.enableKeyboardNavigation(choiceId, viewport);
}

export function disableChoiceKeyboardNavigation() {
    return choiceEventCoordinator.disableKeyboardNavigation();
}