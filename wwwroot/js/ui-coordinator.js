class UICoordinator {
    constructor() {
        this.componentStates = new Map();
        this.eventListeners = new Map();
        this.initializeEventHandlers();
    }
    
    initializeEventHandlers() {
        const EVENTS = window.RRBlazor.Events;
        
        document.addEventListener(EVENTS.UI_COMPONENT_OPENING, this.handleComponentOpening.bind(this));
        document.addEventListener(EVENTS.UI_COMPONENT_OPENED, this.handleComponentOpened.bind(this));
        document.addEventListener(EVENTS.UI_COMPONENT_CLOSING, this.handleComponentClosing.bind(this));
        document.addEventListener(EVENTS.UI_COMPONENT_CLOSED, this.handleComponentClosed.bind(this));
    }
    
    handleComponentOpening(event) {
        const { componentType, componentId, priority = 0 } = event.detail;
        
        const lowerPriorityComponents = Array.from(this.componentStates.entries())
            .filter(([id, state]) => 
                state.status === 'open' && 
                state.priority < priority &&
                state.componentType !== componentType
            );
        
        lowerPriorityComponents.forEach(([id, state]) => {
            this.requestComponentClose(state.componentType, id);
        });
        
        this.componentStates.set(componentId, {
            componentType,
            status: 'opening',
            priority,
            openedAt: Date.now()
        });
    }
    
    handleComponentOpened(event) {
        const { componentId } = event.detail;
        const state = this.componentStates.get(componentId);
        if (state) {
            state.status = 'open';
        }
    }
    
    handleComponentClosing(event) {
        const { componentId } = event.detail;
        const state = this.componentStates.get(componentId);
        if (state) {
            state.status = 'closing';
        }
    }
    
    handleComponentClosed(event) {
        const { componentId } = event.detail;
        this.componentStates.delete(componentId);
    }
    
    requestComponentClose(componentType, componentId) {
        window.RRBlazor.EventDispatcher.dispatch(
            window.RRBlazor.Events.UI_COMPONENT_CLOSE_REQUEST,
            { componentType, componentId }
        );
    }
    
    notifyComponentOpening(componentType, componentId, priority = window.RRBlazor.EventPriorities.NORMAL) {
        window.RRBlazor.EventDispatcher.dispatch(
            window.RRBlazor.Events.UI_COMPONENT_OPENING,
            { componentType, componentId, priority }
        );
    }
    
    notifyComponentOpened(componentType, componentId) {
        window.RRBlazor.EventDispatcher.dispatch(
            window.RRBlazor.Events.UI_COMPONENT_OPENED,
            { componentType, componentId }
        );
    }
    
    notifyComponentClosing(componentType, componentId) {
        window.RRBlazor.EventDispatcher.dispatch(
            window.RRBlazor.Events.UI_COMPONENT_CLOSING,
            { componentType, componentId }
        );
    }
    
    notifyComponentClosed(componentType, componentId) {
        window.RRBlazor.EventDispatcher.dispatch(
            window.RRBlazor.Events.UI_COMPONENT_CLOSED,
            { componentType, componentId }
        );
    }
    
    isComponentOpen(componentId) {
        const state = this.componentStates.get(componentId);
        return state && (state.status === 'open' || state.status === 'opening');
    }
    
    getOpenComponents(componentType = null) {
        return Array.from(this.componentStates.entries())
            .filter(([id, state]) => {
                const typeMatch = !componentType || state.componentType === componentType;
                const isOpen = state.status === 'open' || state.status === 'opening';
                return typeMatch && isOpen;
            })
            .map(([id, state]) => ({ id, ...state }));
    }
    
    closeAllComponents(componentType = null) {
        const components = this.getOpenComponents(componentType);
        components.forEach(component => {
            this.requestComponentClose(component.componentType, component.id);
        });
    }
}

const uiCoordinator = new UICoordinator();

window.RRBlazor = window.RRBlazor || {};
window.RRBlazor.UICoordinator = {
    notifyOpening: (type, id, priority) => uiCoordinator.notifyComponentOpening(type, id, priority),
    notifyOpened: (type, id) => uiCoordinator.notifyComponentOpened(type, id),
    notifyClosing: (type, id) => uiCoordinator.notifyComponentClosing(type, id),
    notifyClosed: (type, id) => uiCoordinator.notifyComponentClosed(type, id),
    isOpen: (id) => uiCoordinator.isComponentOpen(id),
    getOpen: (type) => uiCoordinator.getOpenComponents(type),
    closeAll: (type) => uiCoordinator.closeAllComponents(type)
};

export default uiCoordinator;