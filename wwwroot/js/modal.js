class ModalManager {
    constructor() {
        this.activeModals = new Map();
        this.modalStack = [];
        this.eventHandlers = new Map();
        this.modalCounter = 0;
        this.closingModals = new Set();
        this.registrationMutex = new Map();

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
        const modalId = options.modalId;
        if (!modalId) {
            throw new Error('Modal ID must be provided');
            return null;
        }

        const mutexTimeout = 5000;
        const mutexStart = Date.now();

        if (this.registrationMutex.has(modalId)) {
            const mutexAge = mutexStart - this.registrationMutex.get(modalId);
            if (mutexAge > mutexTimeout) {
                this.registrationMutex.delete(modalId);
            } else {
                return modalId;
            }
        }

        this.registrationMutex.set(modalId, mutexStart);

        try {
            if (typeof contentSelector === 'string') {
                const allMatches = document.querySelectorAll(contentSelector);
                if (allMatches.length > 1) {
                    for (let i = 1; i < allMatches.length; i++) {
                        allMatches[i].remove();
                    }
                }
            }

            const contentElement = typeof contentSelector === 'string'
                ? document.querySelector(contentSelector)
                : contentSelector;

            if (!contentElement) {
                throw new Error(`Modal content not found: ${contentSelector}`);
                return null;
            }

            if (this.activeModals.has(modalId)) {
                const existingModal = this.activeModals.get(modalId);
                if (existingModal.state !== 'closed') {
                    return modalId;
                }
            }

            const modal = this._createModalConfig(modalId, contentElement, options);
            this.activeModals.set(modalId, modal);

            await this._showModalInstance(modal);

            return modalId;
        } finally {
            this.registrationMutex.delete(modalId);
        }
    }
    
    async hide(modalId) {
        this.registrationMutex.delete(modalId);

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
            throw new Error(`Modal ID mismatch: requested ${modalId}, found ${modal.id}`);
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
        
        window.RRBlazor.Backdrop.getInstance().destroyAll();
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

    dispatchParentClosing(modalId) {
        const modal = this.activeModals.get(modalId);
        if (!modal) return;

        if (window.RRBlazor && window.RRBlazor.EventDispatcher) {
            window.RRBlazor.EventDispatcher.dispatchParentClosing(
                modal.contentElement,
                'modal',
                modalId
            );
        }
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
        const isAlreadyInPortal = !!modal.contentElement.closest('#portal-root');
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
                modal.portalElement.style.zIndex = modal.zIndex.toString();
            }
        } else {
            modal.portalElement = modal.contentElement.parentElement;
        }
        
        if (modal.contentElement) {
            const actualModal = modal.contentElement.classList.contains('modal')
                ? modal.contentElement
                : modal.contentElement.querySelector('.modal');

            if (actualModal) {
                actualModal.style.zIndex = modal.zIndex.toString();
                actualModal.style.removeProperty('display');
            } else {
                modal.contentElement.style.zIndex = modal.zIndex.toString();
                modal.contentElement.style.removeProperty('display');
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
        
        if (!modal.isAlreadyInPortal && modal.portalElement && modal.contentElement) {
            window.RRBlazor.Portal.moveToPortal(modal.id, modal.contentElement);
        }
        
        this._setupModalEventHandlers(modal);
        
        if (this.activeModals.size === 1) {
            window.RRBlazor.ScrollLock.lock();
        }
        
        modal.state = 'open';
    }
    
    async _hideModalInstance(modal) {
        if (modal.state === 'closing' || modal.state === 'closed') {
            return;
        }

        modal.state = 'closing';

        this._cleanupModalEventHandlers(modal);

        if (!modal.isAlreadyInPortal && modal.contentElement._portalPositioned) {
            window.RRBlazor.Portal.restoreFromPortal(modal.id, modal.contentElement);
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

        this.zIndexManager.unregisterElement(modal.id);
        this.zIndexManager.unregisterElement(`${modal.id}-backdrop`);
        
        if (this.modalStack.length === 0) {
            window.RRBlazor.ScrollLock.unlock();
        }
        
        if (modal.config.onClose) {
            modal.config.onClose(modal.id);
        }
        
        if (modal.config.isServiceModal && this.providerRef) {
            await this.providerRef.invokeMethodAsync('OnModalClosedFromJS', modal.id);
        } else if (modal.config.dotNetRef) {
            await modal.config.dotNetRef.invokeMethodAsync('OnModalClosedFromJS');
        }
        
        modal.state = 'closed';
    }
    
    _setupModalEventHandlers(modal) {
        if (modal.config.closeOnBackdrop) {
            const manager = window.RRBlazor.Backdrop.getInstance();
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

    getModalContext(element) {
        for (const [modalId, modal] of this.activeModals) {
            if (modal.state !== 'closed' &&
                (modal.contentElement.contains(element) || modal.contentElement === element)) {
                return {
                    modalId: modalId,
                    element: modal.contentElement,
                    zIndex: modal.zIndex,
                    portalElement: modal.portalElement
                };
            }
        }
        return null;
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
    setProviderRef: (dotNetRef) => modalManager.setProviderRef(dotNetRef),
    dispatchParentClosing: (modalId) => modalManager.dispatchParentClosing(modalId),
    getModalContext: (element) => modalManager.getModalContext(element)
};

export default modalManager;