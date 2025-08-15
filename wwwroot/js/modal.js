
class ModalManager {
    constructor() {
        this.activeModals = new Map();
        this.stateTransitions = new Map();
        this.eventHandlers = new Map();
        
        this.animationDurations = this._initializeAnimationDurations();
        
        this._setupEventListeners();
    }
    
    _setupEventListeners() {
        document.addEventListener('portal-destroyed', (event) => {
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
        
        if (this.activeModals.size === 1) {
            await window.RRBlazor.ScrollLock.lock();
        }
        
        if (options.useBackdrop !== false) {
            const backdropRequest = new CustomEvent('backdrop-create-request', {
                detail: {
                    requesterId: modalId,
                    config: {
                        level: this.activeModals.size - 1,
                        className: options.backdropClass || 'modal-backdrop-dark',
                        blur: options.backdropBlur || 8,
                        animationDuration: this.animationDurations[options.animationSpeed || 'normal'],
                        shared: true
                    }
                },
                bubbles: true
            });
            document.dispatchEvent(backdropRequest);
        }
        
        const portalPromise = this._waitForPortal(modalId);
        
        const portalRequest = new CustomEvent('portal-create-request', {
            detail: {
                requesterId: modalId,
                config: {
                    id: modalId,
                    className: 'modal-portal',
                    attributes: {
                        'role': 'presentation',
                        'data-modal-id': modalId
                    }
                }
            },
            bubbles: true
        });
        document.dispatchEvent(portalRequest);
        
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
        
        this._cleanupEventHandlers(modalId);
        
        await window.RRBlazor.FocusTrap.destroy(modalId);
        
        if (modal.element && modal.element.parentNode) {
            this._applyClosingAnimation(modal.element, modal.options);
        }
        
        const destroyPortalRequest = new CustomEvent('portal-destroy-request', {
            detail: { requesterId: modalId, portalId: modalId },
            bubbles: true
        });
        document.dispatchEvent(destroyPortalRequest);
        
        if (modal.options?.useBackdrop !== false) {
            const destroyBackdropRequest = new CustomEvent('backdrop-destroy-request', {
                detail: { requesterId: modalId },
                bubbles: true
            });
            document.dispatchEvent(destroyBackdropRequest);
        }
        
        modal.state = 'closed';
        this.activeModals.delete(modalId);
        
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
            this._forceCleanup(modalId);
        });
        
        this.activeModals.clear();
        this.eventHandlers.clear();
        window.RRBlazor.ScrollLock.forceUnlock();
        
        document.dispatchEvent(new CustomEvent('portal-cleanup-all-request', { bubbles: true }));
        document.dispatchEvent(new CustomEvent('backdrop-cleanup-all-request', { bubbles: true }));
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
        const animation = options.animation || 'scale';
        const speed = options.animationSpeed || 'normal';
        
        element.classList.add('modal-animating', `modal-${animation}`, `modal-speed-${speed}`);
        
        element.style.position = 'relative';
        element.style.margin = 'auto';
        
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
    
    _forceCleanup(modalId) {
        this._cleanupEventHandlers(modalId);
        window.RRBlazor.FocusTrap.destroy(modalId);
        
        document.dispatchEvent(new CustomEvent('portal-destroy-request', { 
            detail: { requesterId: modalId, portalId: modalId },
            bubbles: true 
        }));
        document.dispatchEvent(new CustomEvent('backdrop-destroy-request', { 
            detail: { requesterId: modalId },
            bubbles: true 
        }));
    }
    
    async _waitForPortal(modalId, timeout = 1000) {
        return new Promise((resolve, reject) => {
            const timeoutId = setTimeout(() => {
                document.removeEventListener('portal-created', handler);
                reject(new Error(`Portal creation timeout for modal ${modalId}`));
            }, timeout);
            
            const handler = (event) => {
                if (event.detail.requesterId === modalId) {
                    clearTimeout(timeoutId);
                    document.removeEventListener('portal-created', handler);
                    resolve(event.detail.portal);
                }
            };
            
            document.addEventListener('portal-created', handler);
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
    
    register: (modalId) => {
        throw new Error(`ModalStackService.register is obsolete - use JavaScript modal management`);
    },
    unregister: (modalId) => {
        throw new Error(`ModalStackService.unregister is obsolete - use JavaScript modal management`);
    }
};

export default modalManager;