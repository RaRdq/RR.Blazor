export class DOMStateManager {
    constructor() {
        this.stateMap = new WeakMap();
    }

    store(element) {
        if (!element || this.stateMap.has(element)) return;

        this.stateMap.set(element, {
            parent: element.parentNode,
            nextSibling: element.nextSibling,
            timestamp: Date.now()
        });
    }

    restore(element) {
        if (!element || !this.stateMap.has(element)) return false;

        const originalState = this.stateMap.get(element);

        if (!originalState.parent || !document.body.contains(originalState.parent)) {
            this.stateMap.delete(element);
            return false;
        }

        if (originalState.nextSibling && originalState.nextSibling.parentNode === originalState.parent) {
            originalState.parent.insertBefore(element, originalState.nextSibling);
        } else {
            originalState.parent.appendChild(element);
        }

        this.stateMap.delete(element);
        return true;
    }

    hasState(element) {
        return element && this.stateMap.has(element);
    }

    clear(element) {
        if (element && this.stateMap.has(element)) {
            this.stateMap.delete(element);
            return true;
        }
        return false;
    }

    getState(element) {
        return element ? this.stateMap.get(element) : null;
    }
}

export function createDOMStateManager() {
    return new DOMStateManager();
}

export default DOMStateManager;