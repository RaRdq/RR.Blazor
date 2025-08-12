// modal.js - Modal-specific behavior management
// Following SRP: This module manages modal behavior and state
// Delegates DOM operations to PortalManager and BackdropManager

import PortalManager from './portal.js';
import BackdropManager from './backdrop.js';
import { createFocusTrap, destroyFocusTrap } from './focus-trap.js';
import { lockScroll, unlockScroll, forceUnlockScroll } from './scroll-lock.js';

class ModalManager {
    constructor() {
        this.activeModals = new Map();
        this.stateTransitions = new Map();
        this.eventHandlers = new Map();
        
        // Initialize animation durations from CSS variables
        this.animationDurations = this._initializeAnimationDurations();
        
        // Get singleton instances
        this._portalManager = null;
        this._backdropManager = null;
    }
    
    get portalManager() {
        if (!this._portalManager) {
            this._portalManager = PortalManager.getInstance();
        }
        return this._portalManager;
    }
    
    get backdropManager() {
        if (!this._backdropManager) {
            this._backdropManager = BackdropManager.getInstance();
        }
        return this._backdropManager;
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
            console.log(`[ModalManager] Recreating modal ${modalId} from state: ${currentState}`);
            
            // Destroy existing modal first
            try {
                await this.destroyModal(modalId);
            } catch (error) {
                console.warn(`[ModalManager] Force cleanup modal ${modalId}:`, error.message);
                this._forceCleanup(modalId);
            }
        }
        
        // State: closed → opening
        this.activeModals.set(modalId, {
            element: modalElement,
            state: 'opening',
            options,
            createdAt: Date.now()
        });
        
        // Lock scroll on first modal
        if (this.activeModals.size === 1) {
            lockScroll();
        }
        
        // Create backdrop if requested (delegate to BackdropManager)
        if (options.useBackdrop !== false) {
            const backdropConfig = {
                level: this.activeModals.size - 1,
                className: options.backdropClass || 'modal-backdrop-dark',
                blur: options.backdropBlur || 8,
                animationDuration: this.animationDurations[options.animationSpeed || 'normal'],
                shared: true
            };
            
            const backdrop = this.backdropManager.create(modalId, backdropConfig);
            
            // Setup backdrop click handler
            if (options.closeOnBackdropClick !== false && options.onBackdropClick) {
                const cleanupFn = this.backdropManager.onClick(modalId, (event) => {
                    event.stopPropagation();
                    event.preventDefault();
                    if (this.isTopModal(modalId)) {
                        options.onBackdropClick(event);
                    }
                });
                this._storeEventHandler(modalId, 'backdropClick', cleanupFn);
            }
        }
        
        // Create portal container (delegate to PortalManager)
        const portal = this.portalManager.create({
            id: modalId,
            className: 'modal-portal',
            attributes: {
                'role': 'presentation',
                'data-modal-id': modalId
            }
        });
        
        // Copy theme attributes from document to portal
        this._copyThemeToPortal(portal.element);
        
        // Apply modal-specific centering styles
        this._applyModalContainerStyles(portal.element);
        
        // Move modal element to portal
        portal.element.appendChild(modalElement);
        
        // Apply modal styles and animations
        this._applyModalStyles(modalElement, options);
        
        // Create focus trap
        if (options.trapFocus !== false) {
            await createFocusTrap(modalElement, modalId);
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
        
        // Wait for opening animation
        await this._waitForAnimation(options.animationSpeed || 'normal');
        
        // State: opening → open
        const modal = this.activeModals.get(modalId);
        if (modal) {
            modal.state = 'open';
            modal.portal = portal;
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
        
        // Destroy focus trap
        try {
            await destroyFocusTrap(modalId);
        } catch (error) {
            console.warn(`[ModalManager] Focus trap cleanup failed for ${modalId}:`, error);
        }
        
        // Apply closing animation
        if (modal.element && modal.element.parentNode) {
            this._applyClosingAnimation(modal.element, modal.options);
            await this._waitForAnimation(modal.options?.animationSpeed || 'normal');
        }
        
        // Destroy portal (delegate to PortalManager)
        try {
            this.portalManager.destroy(modalId);
        } catch (error) {
            console.warn(`[ModalManager] Portal cleanup failed for ${modalId}:`, error);
        }
        
        // Destroy backdrop (delegate to BackdropManager)
        if (modal.options?.useBackdrop !== false) {
            try {
                this.backdropManager.destroy(modalId);
            } catch (error) {
                console.warn(`[ModalManager] Backdrop cleanup failed for ${modalId}:`, error);
            }
        }
        
        // State: closing → closed
        modal.state = 'closed';
        this.activeModals.delete(modalId);
        
        // Unlock scroll when all modals are closed
        if (this.activeModals.size === 0) {
            unlockScroll();
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
        forceUnlockScroll();
        
        // Force cleanup portal and backdrop managers
        try {
            this.portalManager.destroyAll();
        } catch (error) {
            console.warn('[ModalManager] Portal force cleanup failed:', error);
        }
        
        try {
            this.backdropManager.destroyAll();
        } catch (error) {
            console.warn('[ModalManager] Backdrop force cleanup failed:', error);
        }
    }
    
    // Private helper methods
    
    _initializeAnimationDurations() {
        try {
            const rootStyles = getComputedStyle(document.documentElement);
            
            const getDurationMs = (cssVar) => {
                const value = rootStyles.getPropertyValue(cssVar).trim();
                if (value.endsWith('ms')) {
                    return parseInt(value, 10);
                } else if (value.endsWith('s')) {
                    return parseFloat(value) * 1000;
                }
                return 200;
            };
            
            return {
                fast: getDurationMs('--duration-fast'),
                normal: getDurationMs('--duration-normal'),
                slow: getDurationMs('--duration-slow')
            };
        } catch (error) {
            console.warn('[ModalManager] Failed to read CSS duration variables:', error);
            return { fast: 100, normal: 200, slow: 300 };
        }
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
        const duration = this.animationDurations[speed] || this.animationDurations.normal;
        return new Promise(resolve => setTimeout(resolve, duration));
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
                try {
                    cleanupFn();
                } catch (error) {
                    console.warn(`[ModalManager] Event cleanup failed:`, error);
                }
            });
            this.eventHandlers.delete(modalId);
        }
    }
    
    _forceCleanup(modalId) {
        this._cleanupEventHandlers(modalId);
        
        try {
            destroyFocusTrap(modalId);
        } catch {}
        
        try {
            this.portalManager.destroy(modalId);
        } catch {}
        
        try {
            this.backdropManager.destroy(modalId);
        } catch {}
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