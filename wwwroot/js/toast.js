import { createDOMStateManager } from './dom-state-manager.js';

const domStateManager = createDOMStateManager();

class ToastManager {
    constructor() {
        this.activeToasts = new Map();
        this.toastContainer = null;
        this.setupCloseRequestListener();
    }

    setupCloseRequestListener() {
        document.addEventListener(window.RRBlazor.Events.UI_COMPONENT_CLOSE_REQUEST, (event) => {
            const { componentType, componentId } = event.detail;
            if (componentType === window.RRBlazor.ComponentTypes.TOOLTIP) {
                this.closeAllToasts();
            } else if (componentType === 'toast' && componentId) {
                this.closeToast(componentId);
            }
        });
    }

    async createToastContainer() {
        if (this.toastContainer) return this.toastContainer;

        const containerId = 'toast-container-portal';


        const portalManager = await window.RRBlazor.Portal.getInstance();
        const portal = portalManager.create({
            id: containerId,
            className: 'toast-portal'
        });

        const container = document.createElement('div');
        container.className = 'toast-container';
        container.setAttribute('data-toast-container', 'true');

        const contextZIndex = window.RRBlazor.ZIndexManager.registerElement('toast-container', 'portal');

        container.style.cssText = `
            position: fixed;
            top: 1rem;
            right: 1rem;
            z-index: ${contextZIndex};
            pointer-events: auto;
            display: flex;
            flex-direction: column;
            gap: 0.5rem;
            max-width: 24rem;
        `;

        portal.element.appendChild(container);
        this.toastContainer = {
            element: container,
            portalId: portal.id
        };


        return this.toastContainer;
    }

    async showToast(toastElement, toastId, options = {}) {
        if (!toastElement || !toastId) return false;


        const container = await this.createToastContainer();

        domStateManager.store(toastElement);

        container.element.appendChild(toastElement);

        this.activeToasts.set(toastId, {
            element: toastElement,
            containerId: container.portalId,
            autoClose: options.autoClose !== false,
            duration: options.duration || 5000
        });

        toastElement.style.opacity = '0';
        toastElement.style.transform = 'translateX(100%)';

        requestAnimationFrame(() => {
            toastElement.style.transition = 'opacity 0.3s ease, transform 0.3s ease';
            toastElement.style.opacity = '1';
            toastElement.style.transform = 'translateX(0)';
        });

        const toastData = this.activeToasts.get(toastId);
        if (toastData && toastData.autoClose) {
            setTimeout(() => {
                this.closeToast(toastId);
            }, toastData.duration);
        }

        return true;
    }

    closeToast(toastId) {
        const toastData = this.activeToasts.get(toastId);
        if (!toastData) return;

        const { element } = toastData;

        element.style.transition = 'opacity 0.3s ease, transform 0.3s ease';
        element.style.opacity = '0';
        element.style.transform = 'translateX(100%)';

        setTimeout(() => {
            if (!domStateManager.restore(element)) {
                element.remove();
            }

            this.activeToasts.delete(toastId);

            if (this.activeToasts.size === 0) {
                this.destroyContainer();
            }
        }, 300);
    }

    closeAllToasts() {
        for (const toastId of this.activeToasts.keys()) {
            this.closeToast(toastId);
        }
    }

    async destroyContainer() {
        if (!this.toastContainer) return;

        window.RRBlazor.ZIndexManager.unregisterElement('toast-container');
        const portalManager = await window.RRBlazor.Portal.getInstance();
        portalManager.destroy(this.toastContainer.portalId);

        this.toastContainer = null;
    }
}

const toastManager = new ToastManager();

export async function moveToastContainerToPortal(containerElement) {
    if (!containerElement) return false;

    const toastId = containerElement.getAttribute('data-toast-id') || `toast-${Date.now()}`;
    containerElement.setAttribute('data-toast-id', toastId);

    return await toastManager.showToast(containerElement, toastId);
}

export function closeToast(toastId) {
    toastManager.closeToast(toastId);
}

export function closeAllToasts() {
    toastManager.closeAllToasts();
}

window.RRBlazor = window.RRBlazor || {};
window.RRBlazor.Toast = {
    show: moveToastContainerToPortal,
    close: closeToast,
    closeAll: closeAllToasts
};


export default {
    show: moveToastContainerToPortal,
    close: closeToast,
    closeAll: closeAllToasts
};