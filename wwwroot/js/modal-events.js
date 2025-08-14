
class ModalEventCoordinator {
    constructor() {
        this.initialized = false;
        this.setupInfrastructureEventHandlers();
    }
    
    setupInfrastructureEventHandlers() {
        document.addEventListener('portal-created', (event) => {
            const { requesterId, portal } = event.detail;
            if (requesterId.startsWith('modal-')) {
                this.handlePortalCreated(requesterId, portal);
            }
        });
        
        document.addEventListener('portal-destroyed', (event) => {
            const { requesterId, portalId } = event.detail;
            if (requesterId?.startsWith('modal-') || portalId?.startsWith('modal-')) {
                this.handlePortalDestroyed(requesterId || portalId);
            }
        });
        
        document.addEventListener('backdrop-created', (event) => {
            const { requesterId } = event.detail;
            if (requesterId?.startsWith('modal-')) {
                this.handleBackdropCreated(requesterId);
            }
        });
        
        document.addEventListener('backdrop-destroyed', (event) => {
            const { requesterId } = event.detail;
            if (requesterId?.startsWith('modal-')) {
                this.handleBackdropDestroyed(requesterId);
            }
        });
    }
    
    requestPortal(modalId, config) {
        const portalRequest = new CustomEvent('portal-create-request', {
            detail: {
                requesterId: modalId,
                config: {
                    id: modalId,
                    className: 'modal-portal',
                    attributes: {
                        'role': 'presentation',
                        'data-modal-id': modalId
                    },
                    ...config
                }
            },
            bubbles: true
        });
        document.dispatchEvent(portalRequest);
    }
    
    destroyPortal(modalId) {
        const destroyRequest = new CustomEvent('portal-destroy-request', {
            detail: {
                requesterId: modalId,
                portalId: modalId
            },
            bubbles: true
        });
        document.dispatchEvent(destroyRequest);
    }
    
    requestBackdrop(modalId, config) {
        const backdropRequest = new CustomEvent('backdrop-create-request', {
            detail: {
                requesterId: modalId,
                config
            },
            bubbles: true
        });
        document.dispatchEvent(backdropRequest);
    }
    
    destroyBackdrop(modalId) {
        const destroyRequest = new CustomEvent('backdrop-destroy-request', {
            detail: {
                requesterId: modalId
            },
            bubbles: true
        });
        document.dispatchEvent(destroyRequest);
    }
    
    forceCleanupAll() {
        const cleanupRequest = new CustomEvent('portal-cleanup-all-request', {
            bubbles: true
        });
        document.dispatchEvent(cleanupRequest);
        
        const backdropCleanupRequest = new CustomEvent('backdrop-cleanup-all-request', {
            bubbles: true
        });
        document.dispatchEvent(backdropCleanupRequest);
    }
    
    handlePortalCreated(modalId, portal) {
        const modalCreatedEvent = new CustomEvent('modal-portal-ready', {
            detail: { modalId, portal },
            bubbles: true
        });
        document.dispatchEvent(modalCreatedEvent);
    }
    
    handlePortalDestroyed(modalId) {
        const modalPortalDestroyedEvent = new CustomEvent('modal-portal-destroyed', {
            detail: { modalId },
            bubbles: true
        });
        document.dispatchEvent(modalPortalDestroyedEvent);
    }
    
    handleBackdropCreated(modalId) {
        const backdropReadyEvent = new CustomEvent('modal-backdrop-ready', {
            detail: { modalId },
            bubbles: true
        });
        document.dispatchEvent(backdropReadyEvent);
    }
    
    handleBackdropDestroyed(modalId) {
        const backdropDestroyedEvent = new CustomEvent('modal-backdrop-destroyed', {
            detail: { modalId },
            bubbles: true
        });
        document.dispatchEvent(backdropDestroyedEvent);
    }
}

const modalEventCoordinator = new ModalEventCoordinator();

export default modalEventCoordinator;
export function requestModalPortal(modalId, config) {
    return modalEventCoordinator.requestPortal(modalId, config);
}

export function destroyModalPortal(modalId) {
    return modalEventCoordinator.destroyPortal(modalId);
}

export function requestModalBackdrop(modalId, config) {
    return modalEventCoordinator.requestBackdrop(modalId, config);
}

export function destroyModalBackdrop(modalId) {
    return modalEventCoordinator.destroyBackdrop(modalId);
}

export function forceCleanupAllModals() {
    return modalEventCoordinator.forceCleanupAll();
}