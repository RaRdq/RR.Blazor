const debugLogger = window.debugLogger;

class ModalManager {
    constructor() {
        this.activeModals = new Map();
        this.modalStack = [];
        this.eventHandlers = new Map();
        this.modalCounter = 0;
        this.closingModals = new Set();
        
        this.zIndexManager = window.RRBlazor.ZIndexManager;
        
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
                this.closingModals.delete(modalId);
            }
        });

        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && this.modalStack.length > 0) {
                const topModalId = this.modalStack[this.modalStack.length - 1];
                const topModal = this.activeModals.get(topModalId);
                
                if (topModal && topModal.config.closeOnEscape) {
                    e.preventDefault();
                    e.stopPropagation();
                    this.hide(topModalId);
                }
            }
        });
    }
    
    async show(contentSelector, options = {}) {
        const contentElement = typeof contentSelector === 'string' 
            ? document.querySelector(contentSelector)
            : contentSelector;
        
        if (!contentElement) {
            throw new Error(`Modal content not found: ${contentSelector}`);
        }
        
        const existingModalId = contentElement.getAttribute('data-modal-id');
        if (existingModalId && this.activeModals.has(existingModalId)) {
            const existingModal = this.activeModals.get(existingModalId);
            if (existingModal.state !== 'closed') {
                return existingModalId;
            }
        }
        
        const modalId = options.modalId || existingModalId || this._generateModalId();
        
        const modal = this._createModalConfig(modalId, contentElement, options);
        this.activeModals.set(modalId, modal);
        
        await this._showModalInstance(modal);
        
        return modalId;
    }
    
    async hide(modalId) {
        if (this.closingModals.has(modalId)) {
            return;
        }
        
        const modal = this.activeModals.get(modalId);
        if (!modal) {
            return;
        }
        
        if (modal.state === 'closed') {
            this.activeModals.delete(modalId);
            return;
        }
        
        if (modal.id !== modalId) {
            console.error(`Modal ID mismatch: requested ${modalId}, found ${modal.id}`);
            return;
        }
        
        this.closingModals.add(modalId);
        
        try {
            await this._hideModalInstance(modal);
        } finally {
            this.activeModals.delete(modalId);
            this.closingModals.delete(modalId);
        }
    }
    
    async hideAll() {
        const modalsToHide = Array.from(this.activeModals.keys()).reverse();
        
        for (const modalId of modalsToHide) {
            await this.hide(modalId);
        }
        
        this.modalStack = [];
        this.eventHandlers.clear();
        window.RRBlazor.ScrollLock.unlock();
        
        if (window.RRBlazor.BackdropSingleton?.getInstance) {
            window.RRBlazor.BackdropSingleton.getInstance().destroyAll();
        }
    }
    
    isActive(modalId) {
        const modal = this.activeModals.get(modalId);
        return modal && modal.state !== 'closed';
    }
    
    getActiveCount() {
        return this.activeModals.size;
    }
    
    setProviderRef(dotNetRef) {
        this.providerRef = dotNetRef;
    }
    
    _generateModalId() {
        return `modal-${++this.modalCounter}-${Date.now()}`;
    }
    
    _createModalConfig(modalId, contentElement, options) {
        return {
            id: modalId,
            config: {
                closeOnBackdrop: options.closeOnBackdrop ?? true,
                closeOnEscape: options.closeOnEscape ?? true,
                backdrop: options.backdrop ?? true,
                blur: options.blur ?? 8,
                className: options.className ?? 'modal-backdrop-dark',
                onClose: options.onClose,
                dotNetRef: options.dotNetRef,
                modalId: options.modalId
            },
            contentElement,
            portalElement: null,
            originalParent: contentElement.parentNode,
            originalNextSibling: contentElement.nextSibling,
            eventHandlers: new Map(),
            state: 'opening'
        };
    }
    
    async _showModalInstance(modal) {
        const isAlreadyInPortal = !!modal.contentElement.closest('#modal-portal-container');
        modal.isAlreadyInPortal = isAlreadyInPortal;
        
        modal.zIndex = this.zIndexManager.registerElement(modal.id, 'modal');
        modal.backdropZIndex = this.zIndexManager.registerElement(`${modal.id}-backdrop`, 'backdrop');
        
        this.modalStack.push(modal.id);
        
        if (!isAlreadyInPortal) {
            const portalPromise = this._waitForPortal(modal.id);
            
            window.RRBlazor.EventDispatcher.dispatch(
                window.RRBlazor.Events.PORTAL_CREATE_REQUEST,
                {
                    requesterId: modal.id,
                    config: {
                        id: modal.id,
                        className: 'modal-portal',
                        zIndex: modal.zIndex,
                        attributes: {
                            'role': 'dialog',
                            'aria-modal': 'true',
                            'data-portal-type': 'modal'
                        }
                    }
                }
            );
            
            await portalPromise;
            modal.portalElement = document.getElementById(modal.id);
            if (modal.portalElement) {
                modal.portalElement.style.setProperty('z-index', modal.zIndex.toString(), 'important');
            }
        } else {
            modal.portalElement = modal.contentElement.closest('#modal-portal-container');
        }
        
        if (modal.contentElement) {
            const actualModal = modal.contentElement.classList.contains('modal') 
                ? modal.contentElement 
                : modal.contentElement.querySelector('.modal');
            
            if (actualModal) {
                actualModal.style.setProperty('z-index', modal.zIndex.toString(), 'important');
            } else {
                modal.contentElement.style.setProperty('z-index', modal.zIndex.toString(), 'important');
            }
        }
        
        if (modal.config.backdrop) {
            window.RRBlazor.EventDispatcher.dispatch(
                window.RRBlazor.Events.BACKDROP_CREATE_REQUEST,
                {
                    requesterId: modal.id,
                    config: {
                        level: this.modalStack.length,
                        className: modal.config.className,
                        blur: modal.config.blur,
                        shared: false,
                        zIndex: modal.backdropZIndex
                    }
                }
            );
        }
        
        window.RRBlazor.UICoordinator.notifyOpening('modal', modal.id, window.RRBlazor.EventPriorities.HIGH);
        
        if (!modal.isAlreadyInPortal && modal.portalElement && modal.contentElement) {
            window.RRBlazor.Portal.moveToPortal(modal.id, modal.contentElement);
        }
        
        this._setupModalEventHandlers(modal);
        
        if (this.activeModals.size === 1) {
            window.RRBlazor.ScrollLock.lock();
        }
        
        modal.state = 'open';
        window.RRBlazor.UICoordinator.notifyOpened('modal', modal.id);
    }
    
    async _hideModalInstance(modal) {
        if (modal.state === 'closing' || modal.state === 'closed') {
            return;
        }
        
        modal.state = 'closing';
        window.RRBlazor.UICoordinator.notifyClosing('modal', modal.id);
        
        this._cleanupModalEventHandlers(modal);
        
        if (!modal.isAlreadyInPortal && modal.originalParent && document.body.contains(modal.originalParent)) {
            if (modal.originalNextSibling) {
                modal.originalParent.insertBefore(modal.contentElement, modal.originalNextSibling);
            } else {
                modal.originalParent.appendChild(modal.contentElement);
            }
        }
        
        if (modal.config.backdrop) {
            window.RRBlazor.EventDispatcher.dispatch(
                window.RRBlazor.Events.BACKDROP_DESTROY_REQUEST,
                { requesterId: modal.id }
            );
        }
        
        if (!modal.isAlreadyInPortal) {
            window.RRBlazor.EventDispatcher.dispatch(
                window.RRBlazor.Events.PORTAL_DESTROY_REQUEST,
                { 
                    requesterId: modal.id,
                    portalId: modal.id
                }
            );
        }
        
        const stackIndex = this.modalStack.indexOf(modal.id);
        if (stackIndex > -1) {
            this.modalStack.splice(stackIndex, 1);
        }
        
        // Clean up z-index registrations
        this.zIndexManager.unregisterElement(modal.id);
        this.zIndexManager.unregisterElement(`${modal.id}-backdrop`);
        
        if (this.modalStack.length === 0) {
            window.RRBlazor.ScrollLock.unlock();
        }
        
        if (modal.config.onClose) {
            modal.config.onClose(modal.id);
        }
        
        if (modal.config.dotNetRef && modal.config.modalId === modal.id) {
            try {
                await modal.config.dotNetRef.invokeMethodAsync('OnModalClosedFromJS');
            } catch (e) {
                console.warn(`Failed to notify Blazor component for modal ${modal.id}:`, e);
            }
        }
        
        if (this.providerRef && !modal.config.dotNetRef) {
            try {
                await this.providerRef.invokeMethodAsync('OnModalClosedFromJS', modal.id);
            } catch (e) {
                console.warn('Failed to notify modal provider:', e);
            }
        }
        
        modal.state = 'closed';
        window.RRBlazor.UICoordinator.notifyClosed('modal', modal.id);
    }
    
    _setupModalEventHandlers(modal) {
        if (modal.config.closeOnBackdrop) {
            const manager = window.RRBlazor.BackdropSingleton.getInstance();
            const removeHandler = manager.onClick(modal.id, () => {
                if (this._isTopModal(modal.id)) {
                    this.hide(modal.id);
                }
            });
            modal.eventHandlers.set('backdrop', removeHandler);
        }
    }
    
    _cleanupModalEventHandlers(modal) {
        modal.eventHandlers.forEach(cleanup => cleanup());
        modal.eventHandlers.clear();
    }
    
    _isTopModal(modalId) {
        if (this.modalStack.length === 0) return false;
        return this.modalStack[this.modalStack.length - 1] === modalId;
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

window.modalManager = modalManager;

window.RRBlazor = window.RRBlazor || {};
window.RRBlazor.Modal = {
    show: (contentSelector, options) => modalManager.show(contentSelector, options),
    hide: (modalId) => modalManager.hide(modalId),
    hideAll: () => modalManager.hideAll(),
    isActive: (modalId) => modalManager.isActive(modalId),
    getActiveCount: () => modalManager.getActiveCount(),
    setProviderRef: (dotNetRef) => modalManager.setProviderRef(dotNetRef)
};

export default modalManager;