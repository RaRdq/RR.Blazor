// modal.js - Modal-specific behavior management
// Following SRP and Dependency Inversion:
// - This module manages modal behavior and state
// - Uses events to communicate with portal and backdrop (not direct imports)
// - Portal and backdrop are core modules that don't know about modals

class ModalManager {
    constructor() {
        this.activeModals = new Map();
        this.stateTransitions = new Map();
        this.eventHandlers = new Map();
        
        // Initialize animation durations from CSS variables
        this.animationDurations = this._initializeAnimationDurations();
        
        // Setup event listeners for portal and backdrop events
        this._setupEventListeners();
    }
    
    _setupEventListeners() {
        // Listen for portal events (Dependency Inversion)
        document.addEventListener('portal-destroyed', (event) => {
            const { portalId } = event.detail;
            // Clean up modal data when portal is destroyed
            const modal = Array.from(this.activeModals.entries())
                .find(([id, data]) => data.portalId === portalId);
            if (modal) {
                const [modalId] = modal;
                this.activeModals.delete(modalId);
            }
        });
    }
    
    /**
     * Creates a modal with proper state management
     * @param {HTMLElement} modalElement - Modal content element
     * @param {Object} options - Modal configuration options
     * @returns {Object} Modal data {id, portal, element}
     */
    async createModal(modalElement, options = {}) {
        const modalId = options.id || `modal-${Date.now()}`;
        
        // Handle existing modal recreation
        if (this.activeModals.has(modalId)) {
            const currentState = this.activeModals.get(modalId).state;
            throw new Error(`Modal ${modalId} already exists in state: ${currentState}. Destroy it first.`);
        }
        
        // State: closed → opening
        this.activeModals.set(modalId, {
            element: modalElement,
            state: 'opening',
            options,
            createdAt: Date.now()
        });
        
        // Lock scroll on first modal via proxy
        if (this.activeModals.size === 1) {
            await window.RRBlazor.ScrollLock.lock();
        }
        
        // Request backdrop creation via events directly
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
        
        // Set up listener BEFORE dispatching request to avoid race condition
        const portalPromise = this._waitForPortal(modalId);
        
        // Now request portal creation via events
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
        
        // Wait for portal to be created
        await portalPromise;
        
        // Get the created portal element
        const portal = document.getElementById(modalId);
        if (!portal) {
            throw new Error(`Portal ${modalId} was not created`);
        }
        
        // Copy theme attributes from document to portal
        this._copyThemeToPortal(portal);
        
        // Apply modal-specific centering styles
        this._applyModalContainerStyles(portal);
        
        // Move modal element to portal
        portal.appendChild(modalElement);
        
        // Apply modal styles and animations
        this._applyModalStyles(modalElement, options);
        
        // Create focus trap via proxy
        if (options.trapFocus !== false) {
            await window.RRBlazor.FocusTrap.create(modalElement, modalId);
        }
        
        // Setup escape key handler
        if (options.closeOnEscape !== false && options.onEscapeKey) {
            const escapeHandler = (event) => {
                if (event.key === 'Escape' && this.isTopModal(modalId)) {
                    event.stopPropagation();
                    event.preventDefault();
                    options.onEscapeKey(event);
                }
            };
            document.addEventListener('keydown', escapeHandler);
            this._storeEventHandler(modalId, 'escape', () => {
                document.removeEventListener('keydown', escapeHandler);
            });
        }
        
        // No waiting - CSS handles animations independently
        
        // State: opening → open
        const modal = this.activeModals.get(modalId);
        if (modal) {
            modal.state = 'open';
            modal.portalId = modalId;
        }
        
        return modalId;
    }
    
    /**
     * Destroys a modal with proper cleanup
     * @param {string} modalId - Modal identifier
     * @returns {boolean} Success status
     */
    async destroyModal(modalId) {
        const modal = this.activeModals.get(modalId);
        if (!modal) {
            console.warn(`[ModalManager] Modal ${modalId} not found for destruction`);
            return true;
        }
        
        // Check state transition validity
        if (modal.state === 'closing' || modal.state === 'closed') {
            console.log(`[ModalManager] Modal ${modalId} already closing/closed`);
            return true;
        }
        
        // State: → closing
        modal.state = 'closing';
        modal.destroyStartedAt = Date.now();
        
        // Cleanup event handlers
        this._cleanupEventHandlers(modalId);
        
        // Destroy focus trap via proxy - let it throw if it fails
        await window.RRBlazor.FocusTrap.destroy(modalId);
        
        // Apply closing animation - CSS handles timing
        if (modal.element && modal.element.parentNode) {
            this._applyClosingAnimation(modal.element, modal.options);
        }
        
        // Request portal destruction via events directly
        const destroyPortalRequest = new CustomEvent('portal-destroy-request', {
            detail: { requesterId: modalId, portalId: modalId },
            bubbles: true
        });
        document.dispatchEvent(destroyPortalRequest);
        
        // Request backdrop destruction via events if it was created
        if (modal.options?.useBackdrop !== false) {
            const destroyBackdropRequest = new CustomEvent('backdrop-destroy-request', {
                detail: { requesterId: modalId },
                bubbles: true
            });
            document.dispatchEvent(destroyBackdropRequest);
        }
        
        // State: closing → closed
        modal.state = 'closed';
        this.activeModals.delete(modalId);
        
        // Unlock scroll when all modals are closed via proxy
        if (this.activeModals.size === 0) {
            await window.RRBlazor.ScrollLock.unlock();
        }
        
        return true;
    }
    
    /**
     * Gets modal state
     * @param {string} modalId - Modal identifier
     * @returns {string} Modal state
     */
    getModalState(modalId) {
        const modal = this.activeModals.get(modalId);
        return modal ? modal.state : 'closed';
    }
    
    /**
     * Gets count of active modals
     * @returns {number} Active modal count
     */
    getActiveCount() {
        return this.activeModals.size;
    }
    
    /**
     * Checks if specific modal is active
     * @param {string} modalId - Modal identifier
     * @returns {boolean} True if modal is active
     */
    isActive(modalId) {
        const modal = this.activeModals.get(modalId);
        return modal && modal.state !== 'closed';
    }
    
    /**
     * Checks if modal is top-most
     * @param {string} modalId - Modal identifier
     * @returns {boolean} True if modal is on top
     */
    isTopModal(modalId) {
        const activeModalIds = Array.from(this.activeModals.entries())
            .filter(([id, modal]) => modal.state === 'open' || modal.state === 'opening')
            .map(([id]) => id)
            .sort((a, b) => this.activeModals.get(a).createdAt - this.activeModals.get(b).createdAt);
        
        return activeModalIds.length > 0 && activeModalIds[activeModalIds.length - 1] === modalId;
    }
    
    /**
     * Force cleanup for all modals
     */
    forceUnlock() {
        const modalIds = Array.from(this.activeModals.keys());
        
        modalIds.forEach(modalId => {
            this._forceCleanup(modalId);
        });
        
        this.activeModals.clear();
        this.eventHandlers.clear();
        window.RRBlazor.ScrollLock.forceUnlock();
        
        // Request force cleanup via events
        document.dispatchEvent(new CustomEvent('portal-cleanup-all-request', { bubbles: true }));
        document.dispatchEvent(new CustomEvent('backdrop-cleanup-all-request', { bubbles: true }));
    }
    
    // Private helper methods
    
    _initializeAnimationDurations() {
        const rootStyles = getComputedStyle(document.documentElement);
        
        const getDurationMs = (cssVar, defaultValue = '200ms') => {
            const value = rootStyles.getPropertyValue(cssVar).trim();
            if (!value) {
                // Use default value if CSS variable not yet loaded
                console.warn(`CSS variable ${cssVar} not found, using default: ${defaultValue}`);
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
        element.style.maxWidth = '90vw';
        element.style.maxHeight = '90vh';
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
        // No waiting - CSS handles animations, we proceed immediately
        // Fail-fast principle: if CSS transitions aren't ready, that's a CSS issue
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
        
        // Request cleanup via events
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

// Create singleton instance
const modalManager = new ModalManager();

// Export API for RRBlazor
window.RRBlazor = window.RRBlazor || {};
window.RRBlazor.Modal = {
    create: (elementOrSelector, options) => {
        // Handle both element and selector
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
    
    // No legacy compatibility - fail fast
    register: (modalId) => {
        throw new Error(`ModalStackService.register is obsolete - use JavaScript modal management`);
    },
    unregister: (modalId) => {
        throw new Error(`ModalStackService.unregister is obsolete - use JavaScript modal management`);
    }
};

// Export for ES6 modules
export default modalManager;