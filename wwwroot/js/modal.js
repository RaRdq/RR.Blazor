
class ModalManager {
    constructor() {
        this.activeModals = new Map();
        this.stateTransitions = new Map();
        this.eventHandlers = new Map();
        
        this.animationDurations = this._initializeAnimationDurations();
        
        this._setupEventListeners();
    }
    
    _setupEventListeners() {
        document.addEventListener(window.RRBlazor.Events.PORTAL_DESTROYED, (event) => {
            const { portalId } = event.detail;
            const modal = Array.from(this.activeModals.entries())
                .find(([id, data]) => data.portalId === portalId);
            if (modal) {
                const [modalId] = modal;
                this.activeModals.delete(modalId);
            }
        });
    }
    
    async createModal(modalElement, options = {}) {
        const modalId = options.id || `modal-${Date.now()}`;
        
        if (this.activeModals.has(modalId)) {
            const currentState = this.activeModals.get(modalId).state;
            throw new Error(`Modal ${modalId} already exists in state: ${currentState}. Destroy it first.`);
        }
        
        this.activeModals.set(modalId, {
            element: modalElement,
            state: 'opening',
            options,
            createdAt: Date.now()
        });
        
        this._setupCloseRequestListener(modalId);
        
        if (this.activeModals.size === 1) {
            await window.RRBlazor.ScrollLock.lock();
            
            window.RRBlazor.EventDispatcher.dispatch(
                window.RRBlazor.Events.UI_COMPONENT_OPENING,
                {
                    componentType: window.RRBlazor.ComponentTypes.MODAL,
                    componentId: modalId,
                    priority: window.RRBlazor.EventPriorities.HIGH
                }
            );
        }
        
        if (options.useBackdrop !== false) {
            window.RRBlazor.EventDispatcher.dispatch(
                window.RRBlazor.Events.BACKDROP_CREATE_REQUEST,
                {
                    requesterId: modalId,
                    config: {
                        level: this.activeModals.size - 1,
                        className: options.backdropClass || 'modal-backdrop-dark',
                        blur: options.backdropBlur || 8,
                        animationDuration: this.animationDurations[options.animationSpeed || 'normal'],
                        shared: true
                    }
                }
            );
        }
        
        const portalPromise = this._waitForPortal(modalId);
        
        window.RRBlazor.EventDispatcher.dispatch(
            window.RRBlazor.Events.PORTAL_CREATE_REQUEST,
            {
                requesterId: modalId,
                config: {
                    id: modalId,
                    className: 'modal-portal',
                    attributes: {
                        'role': 'presentation',
                        'data-modal-id': modalId
                    }
                }
            }
        );
        
        await portalPromise;
        
        const portal = document.getElementById(modalId);
        if (!portal) {
            throw new Error(`Portal ${modalId} was not created`);
        }
        
        this._copyThemeToPortal(portal);
        
        this._applyModalContainerStyles(portal);
        
        portal.appendChild(modalElement);
        
        this._applyModalStyles(modalElement, options);
        
        if (options.trapFocus !== false) {
            await window.RRBlazor.FocusTrap.create(modalElement, modalId);
        }
        
        if (options.closeOnEscape !== false) {
            const escapeHandler = (event) => {
                if (event.key === 'Escape' && this.isTopModal(modalId)) {
                    event.stopPropagation();
                    event.preventDefault();
                    this.destroyModal(modalId);
                }
            };
            document.addEventListener('keydown', escapeHandler);
            this._storeEventHandler(modalId, 'escape', () => {
                document.removeEventListener('keydown', escapeHandler);
            });
        }
        
        if (options.closeOnBackdropClick !== false) {
            try {
                window.RRBlazor.Backdrop.onClick(modalId, (event) => {
                    if (this.isTopModal(modalId)) {
                        event.stopPropagation();
                        this.destroyModal(modalId);
                    }
                });
            } catch (error) {
                console.warn(`Failed to register backdrop click handler for modal ${modalId}:`, error);
            }
        }
        
        const modal = this.activeModals.get(modalId);
        if (modal) {
            modal.state = 'open';
            modal.portalId = modalId;
            
            window.RRBlazor.EventDispatcher.dispatch(
                window.RRBlazor.Events.UI_COMPONENT_OPENED,
                {
                    componentType: window.RRBlazor.ComponentTypes.MODAL,
                    componentId: modalId
                }
            );
        }
        
        return modalId;
    }
    
    async destroyModal(modalId) {
        const modal = this.activeModals.get(modalId);
        if (!modal) {
            return true;
        }
        
        if (modal.state === 'closing' || modal.state === 'closed') {
            return true;
        }
        
        modal.state = 'closing';
        modal.destroyStartedAt = Date.now();
        
        window.RRBlazor.EventDispatcher.dispatch(
            window.RRBlazor.Events.UI_COMPONENT_CLOSING,
            {
                componentType: window.RRBlazor.ComponentTypes.MODAL,
                componentId: modalId
            }
        );
        
        this._cleanupEventHandlers(modalId);
        
        await window.RRBlazor.FocusTrap.destroy(modalId);
        
        if (modal.element && modal.element.parentNode) {
            this._applyClosingAnimation(modal.element, modal.options);
        }
        
        window.RRBlazor.EventDispatcher.dispatch(
            window.RRBlazor.Events.PORTAL_DESTROY_REQUEST,
            { requesterId: modalId, portalId: modalId }
        );
        
        if (modal.options.useBackdrop !== false) {
            window.RRBlazor.EventDispatcher.dispatch(
                window.RRBlazor.Events.BACKDROP_DESTROY_REQUEST,
                { requesterId: modalId }
            );
        }
        
        modal.state = 'closed';
        this.activeModals.delete(modalId);
        
        window.RRBlazor.EventDispatcher.dispatch(
            window.RRBlazor.Events.UI_COMPONENT_CLOSED,
            {
                componentType: window.RRBlazor.ComponentTypes.MODAL,
                componentId: modalId
            }
        );
        
        if (this.activeModals.size === 0) {
            await window.RRBlazor.ScrollLock.unlock();
        }
        
        return true;
    }
    
    getModalState(modalId) {
        const modal = this.activeModals.get(modalId);
        return modal ? modal.state : 'closed';
    }
    
    getActiveCount() {
        return this.activeModals.size;
    }
    
    isActive(modalId) {
        const modal = this.activeModals.get(modalId);
        return modal && modal.state !== 'closed';
    }
    
    isTopModal(modalId) {
        const activeModalIds = Array.from(this.activeModals.entries())
            .filter(([id, modal]) => modal.state === 'open' || modal.state === 'opening')
            .map(([id]) => id)
            .sort((a, b) => this.activeModals.get(a).createdAt - this.activeModals.get(b).createdAt);
        
        return activeModalIds.length > 0 && activeModalIds[activeModalIds.length - 1] === modalId;
    }
    
    forceUnlock() {
        const modalIds = Array.from(this.activeModals.keys());
        
        modalIds.forEach(modalId => {
            this._cleanupEventHandlers(modalId);
            window.RRBlazor.FocusTrap.destroy(modalId);
        });
        
        this.activeModals.clear();
        this.eventHandlers.clear();
        window.RRBlazor.ScrollLock.forceUnlock();
        
        window.RRBlazor.EventDispatcher.dispatch(window.RRBlazor.Events.PORTAL_CLEANUP_ALL_REQUEST);
        window.RRBlazor.EventDispatcher.dispatch(window.RRBlazor.Events.BACKDROP_CLEANUP_ALL_REQUEST);
    }
    
    
    _initializeAnimationDurations() {
        const rootStyles = getComputedStyle(document.documentElement);
        
        const getDurationMs = (cssVar, defaultValue = '200ms') => {
            const value = rootStyles.getPropertyValue(cssVar).trim();
            if (!value) {
                return defaultValue.endsWith('ms') ? parseInt(defaultValue, 10) : parseFloat(defaultValue) * 1000;
            }
            if (value.endsWith('ms')) {
                return parseInt(value, 10);
            } else if (value.endsWith('s')) {
                return parseFloat(value) * 1000;
            }
            throw new Error(`Invalid duration format: ${value}`);
        };
        
        return {
            fast: getDurationMs('--duration-fast'),
            normal: getDurationMs('--duration-normal'),
            slow: getDurationMs('--duration-slow')
        };
    }
    
    _copyThemeToPortal(portalElement) {
        const rootElement = document.documentElement;
        const theme = rootElement.dataset.theme;
        const density = rootElement.dataset.density;
        
        if (theme) {
            portalElement.dataset.theme = theme;
        }
        if (density) {
            portalElement.dataset.density = density;
        }
    }
    
    _applyModalContainerStyles(element) {
        element.style.cssText = `
            position: fixed;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: var(--space-4);
            pointer-events: auto;
        `;
    }
    
    _applyModalStyles(element, options) {
        element.style.position = 'relative';
        element.style.margin = 'auto';
        element.style.transition = 'all var(--duration-normal) var(--ease-out)';
        
        requestAnimationFrame(() => {
            element.classList.add('modal-in');
        });
    }
    
    _applyClosingAnimation(element, options) {
        element.classList.remove('modal-in');
        element.classList.add('modal-out');
    }
    
    async _waitForAnimation(speed) {
        return Promise.resolve();
    }
    
    _storeEventHandler(modalId, type, cleanupFn) {
        if (!this.eventHandlers.has(modalId)) {
            this.eventHandlers.set(modalId, new Map());
        }
        this.eventHandlers.get(modalId).set(type, cleanupFn);
    }
    
    _cleanupEventHandlers(modalId) {
        const handlers = this.eventHandlers.get(modalId);
        if (handlers) {
            handlers.forEach(cleanupFn => {
                cleanupFn();
            });
            this.eventHandlers.delete(modalId);
        }
    }
    
    
    _setupCloseRequestListener(modalId) {
        const handler = (event) => {
            if (event.detail.componentId === modalId && event.detail.componentType === window.RRBlazor.ComponentTypes.MODAL) {
                this.destroyModal(modalId);
                document.removeEventListener(window.RRBlazor.Events.UI_COMPONENT_CLOSE_REQUEST, handler);
            }
        };
        document.addEventListener(window.RRBlazor.Events.UI_COMPONENT_CLOSE_REQUEST, handler);
        this._storeEventHandler(modalId, 'close-request', () => {
            document.removeEventListener(window.RRBlazor.Events.UI_COMPONENT_CLOSE_REQUEST, handler);
        });
    }
    
    async _waitForPortal(modalId, timeout = 1000) {
        return new Promise((resolve, reject) => {
            const timeoutId = setTimeout(() => {
                document.removeEventListener(window.RRBlazor.Events.PORTAL_CREATED, handler);
                reject(new Error(`Portal creation timeout for modal ${modalId}`));
            }, timeout);
            
            const handler = (event) => {
                if (event.detail.requesterId === modalId) {
                    clearTimeout(timeoutId);
                    document.removeEventListener(window.RRBlazor.Events.PORTAL_CREATED, handler);
                    resolve(event.detail.portal);
                }
            };
            
            document.addEventListener(window.RRBlazor.Events.PORTAL_CREATED, handler);
        });
    }
}

const modalManager = new ModalManager();

window.RRBlazor = window.RRBlazor || {};
window.RRBlazor.Modal = {
    create: (elementOrSelector, options) => {
        let element = elementOrSelector;
        if (typeof elementOrSelector === 'string') {
            element = document.querySelector(elementOrSelector);
            if (!element) {
                throw new Error(`Modal element not found for selector: ${elementOrSelector}`);
            }
        }
        return modalManager.createModal(element, options);
    },
    destroy: (modalId) => modalManager.destroyModal(modalId),
    isActive: (modalId) => modalManager.isActive(modalId),
    isTopModal: (modalId) => modalManager.isTopModal(modalId),
    getState: (modalId) => modalManager.getModalState(modalId),
    forceUnlock: () => modalManager.forceUnlock(),
    getActiveCount: () => modalManager.getActiveCount(),
    
    // Find existing Blazor-rendered modal component and create modal portal
    createAndShow: (modalId, modalTypeName, parameters, options) => {
        // Wait for Blazor to render the component, then find it
        const findAndCreateModal = () => {
            const modalElement = document.getElementById(modalId) || 
                                document.querySelector(`[data-modal-id="${modalId}"]`);
            
            if (modalElement) {
                modalManager.createModal(modalElement, options);
            } else {
                // Keep checking for the component for a short time
                setTimeout(findAndCreateModal, 10);
            }
        };
        
        // Start looking for the modal
        findAndCreateModal();
    },

    _getVariantClass: (variant) => {
        switch (variant) {
            case 'Destructive': 
            case 'Danger': return 'modal-destructive';
            case 'Warning': return 'modal-warning';
            default: return 'modal-default';
        }
    },

    // Helper functions to create modal elements without eval (kept for compatibility)
    createElement: (modalId, className) => {
        const wrapper = document.createElement('div');
        wrapper.id = `modal-wrapper-${modalId}`;
        wrapper.className = className || 'modal-wrapper';
        wrapper.setAttribute('data-modal-id', modalId);
        return wrapper;
    },
    
    createConfirmationElement: (modalId, title, message, confirmText, cancelText, variantClass) => {
        const modal = document.createElement('div');
        modal.className = `rmodal rmodal-confirmation ${variantClass}`;
        modal.setAttribute('data-modal-id', modalId);
        
        modal.innerHTML = `
            <div class="modal-content">
                <div class="modal-header">
                    <h2 class="modal-title">${title}</h2>
                </div>
                <div class="modal-body">
                    <p class="modal-message">${message}</p>
                </div>
                <div class="modal-footer">
                    <button class="btn btn-secondary modal-cancel" data-action="cancel">${cancelText}</button>
                    <button class="btn btn-primary modal-confirm" data-action="confirm">${confirmText}</button>
                </div>
            </div>
        `;
        
        // Add event listeners
        modal.querySelector('[data-action="cancel"]').addEventListener('click', () => {
            DotNet.invokeMethodAsync('RR.Blazor', 'HandleModalAction', modalId, 'cancel');
        });
        
        modal.querySelector('[data-action="confirm"]').addEventListener('click', () => {
            DotNet.invokeMethodAsync('RR.Blazor', 'HandleModalAction', modalId, 'confirm');
        });
        
        return modal;
    },
    
};

export default modalManager;