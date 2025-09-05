
class ModalEventCoordinator {
    constructor() {
        this.initialized = false;
        this.setupInfrastructureEventHandlers();
    }
    
    setupInfrastructureEventHandlers() {
        if (!window.RRBlazor || !window.RRBlazor.Events) {
            setTimeout(() => this.setupInfrastructureEventHandlers(), 10);
            return;
        }
        
        document.addEventListener(window.RRBlazor.Events.PORTAL_CREATED, (event) => {
            const { requesterId, portal } = event.detail;
            if (requesterId.startsWith('modal-')) {
                this.handlePortalCreated(requesterId, portal);
            }
        });
        
        document.addEventListener(window.RRBlazor.Events.PORTAL_DESTROYED, (event) => {
            const { requesterId, portalId } = event.detail;
            if ((requesterId && requesterId.startsWith('modal-')) || (portalId && portalId.startsWith('modal-'))) {
                this.handlePortalDestroyed(requesterId || portalId);
            }
        });
        
        document.addEventListener(window.RRBlazor.Events.BACKDROP_CREATED, (event) => {
            const { requesterId } = event.detail;
            if (requesterId.startsWith('modal-')) {
                this.handleBackdropCreated(requesterId);
            }
        });
        
        document.addEventListener(window.RRBlazor.Events.BACKDROP_DESTROYED, (event) => {
            const { requesterId } = event.detail;
            if (requesterId.startsWith('modal-')) {
                this.handleBackdropDestroyed(requesterId);
            }
        });
    }
    
    requestPortal(modalId, config) {
        
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
                    },
                    ...config
                }
            }
        );
    }
    
    destroyPortal(modalId) {
        
        window.RRBlazor.EventDispatcher.dispatch(
            window.RRBlazor.Events.PORTAL_DESTROY_REQUEST,
            {
                requesterId: modalId,
                portalId: modalId
            }
        );
    }
    
    requestBackdrop(modalId, config) {
        
        window.RRBlazor.EventDispatcher.dispatch(
            window.RRBlazor.Events.BACKDROP_CREATE_REQUEST,
            {
                requesterId: modalId,
                config
            }
        );
    }
    
    destroyBackdrop(modalId) {
        
        window.RRBlazor.EventDispatcher.dispatch(
            window.RRBlazor.Events.BACKDROP_DESTROY_REQUEST,
            {
                requesterId: modalId
            }
        );
    }
    
    forceCleanupAll() {
        
        window.RRBlazor.EventDispatcher.dispatch(
            window.RRBlazor.Events.PORTAL_CLEANUP_ALL_REQUEST
        );
        
        window.RRBlazor.EventDispatcher.dispatch(
            window.RRBlazor.Events.BACKDROP_CLEANUP_ALL_REQUEST
        );
    }
    
    handlePortalCreated(modalId, portal) {
        if (!window.RRBlazor || !window.RRBlazor.EventDispatcher) return;
        
        window.RRBlazor.EventDispatcher.dispatch(
            window.RRBlazor.Events.MODAL_PORTAL_READY,
            { modalId, portal }
        );
    }
    
    handlePortalDestroyed(modalId) {
        if (!window.RRBlazor || !window.RRBlazor.EventDispatcher) return;
        
        window.RRBlazor.EventDispatcher.dispatch(
            window.RRBlazor.Events.MODAL_PORTAL_DESTROYED,
            { modalId }
        );
    }
    
    handleBackdropCreated(modalId) {
        if (!window.RRBlazor || !window.RRBlazor.EventDispatcher) return;
        
        window.RRBlazor.EventDispatcher.dispatch(
            window.RRBlazor.Events.MODAL_BACKDROP_READY,
            { modalId }
        );
    }
    
    handleBackdropDestroyed(modalId) {
        if (!window.RRBlazor || !window.RRBlazor.EventDispatcher) return;
        
        window.RRBlazor.EventDispatcher.dispatch(
            window.RRBlazor.Events.MODAL_BACKDROP_DESTROYED,
            { modalId }
        );
    }
}

const modalEventCoordinator = new ModalEventCoordinator();

export default modalEventCoordinator;

export function forceCleanupAllModals() {
    return modalEventCoordinator.forceCleanupAll();
}