export class EventHandlerManager {
    constructor() {
        this.handlers = new Map();
    }

    addEventListener(element, eventType, handler, options = {}) {
        if (typeof element === 'string') {
            element = document.getElementById(element) || document.querySelector(element);
        }

        if (!element) {
            return null;
        }

        const handlerId = `${element.id || 'anonymous'}-${eventType}-${Date.now()}`;

        element.addEventListener(eventType, handler, options);

        this.handlers.set(handlerId, {
            element,
            eventType,
            handler,
            options
        });

        return handlerId;
    }

    removeEventListener(handlerId) {
        const handlerInfo = this.handlers.get(handlerId);
        if (handlerInfo) {
            handlerInfo.element.removeEventListener(handlerInfo.eventType, handlerInfo.handler);
            this.handlers.delete(handlerId);
            return true;
        }
        return false;
    }

    removeAllEventListeners() {
        this.handlers.forEach((handlerInfo, handlerId) => {
            handlerInfo.element.removeEventListener(handlerInfo.eventType, handlerInfo.handler);
        });
        this.handlers.clear();
    }

    getCleanupFunction(handlerId) {
        return () => this.removeEventListener(handlerId);
    }

    getAllCleanupFunction() {
        return () => this.removeAllEventListeners();
    }
}

export function createEventHandlerManager() {
    return new EventHandlerManager();
}

export default EventHandlerManager;