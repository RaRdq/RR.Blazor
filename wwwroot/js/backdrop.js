// backdrop.js - Pure Backdrop Management Only
// Following SRP: This module ONLY manages backdrop elements
// NO portal management, NO event handling logic beyond backdrop clicks

import { createSingleton, WeakRegistry } from './utils/singleton-factory.js';

class BackdropManagerBase {
    static #baseOpacity = 0.5;
    static #opacityIncrement = 0.1;
    static #maxOpacity = 0.9;
    
    #backdrops = new Map();
    #sharedBackdrops = new Map();
    #registry = new WeakRegistry();
    #animationDuration = 200; // Default duration
    
    constructor() {
        // Initialize animation duration from CSS variables
        this.#animationDuration = this.#getAnimationDuration();
    }
    
    /**
     * Creates a backdrop element
     * @param {string} portalId - Associated portal ID
     * @param {Object} config - Backdrop configuration
     * @param {number} config.level - Stacking level for opacity calculation
     * @param {boolean} config.shared - Whether to share backdrop at same level
     * @param {string} config.className - CSS class names to apply
     * @param {number} config.blur - Blur amount in pixels
     * @param {number} config.animationDuration - Animation duration in ms
     * @returns {HTMLElement} Backdrop element
     */
    create(portalId, config = {}) {
        // FAIL FAST: No duplicate backdrops
        if (this.#backdrops.has(portalId)) {
            throw new Error(`Backdrop for portal ${portalId} already exists - programming error`);
        }
        
        const level = config.level || 0;
        const useShared = config.shared !== false;
        
        let backdrop;
        
        if (useShared && this.#sharedBackdrops.has(level)) {
            // Reuse existing shared backdrop
            backdrop = this.#sharedBackdrops.get(level);
            backdrop.refCount++;
        } else {
            // Create new backdrop
            backdrop = this.#createBackdropElement(portalId, level, config);
            
            if (useShared) {
                backdrop.refCount = 1;
                this.#sharedBackdrops.set(level, backdrop);
            }
        }
        
        const backdropData = {
            element: backdrop.element,
            level,
            shared: useShared,
            config
        };
        
        this.#backdrops.set(portalId, backdropData);
        this.#registry.register(portalId, backdropData);
        
        // Animate in
        this.#animateIn(backdrop.element, config.animationDuration);
        
        return backdrop.element;
    }
    
    /**
     * Destroys a backdrop
     * @param {string} portalId - Associated portal ID
     */
    destroy(portalId) {
        const backdrop = this.#backdrops.get(portalId);
        // FAIL FAST: Backdrop must exist when destroying
        if (!backdrop) {
            throw new Error(`Backdrop for portal ${portalId} not found - cannot destroy non-existent backdrop`);
        }
        
        // Remove from tracking immediately
        this.#backdrops.delete(portalId);
        this.#registry.delete(portalId);
        
        if (backdrop.shared) {
            const shared = this.#sharedBackdrops.get(backdrop.level);
            if (shared) {
                shared.refCount = Math.max(0, shared.refCount - 1);
                
                if (shared.refCount === 0) {
                    // Last reference, remove shared backdrop
                    this.#sharedBackdrops.delete(backdrop.level);
                    this.#removeBackdropElement(shared.element, backdrop.config.animationDuration);
                }
            }
        } else {
            // Non-shared backdrop - remove directly
            this.#removeBackdropElement(backdrop.element, backdrop.config.animationDuration);
        }
    }
    
    /**
     * Destroys all backdrops
     */
    destroyAll() {
        const elementsToRemove = [];
        
        // Collect non-shared backdrops
        this.#backdrops.forEach((backdrop) => {
            if (!backdrop.shared && backdrop.element) {
                elementsToRemove.push(backdrop.element);
            }
        });
        
        // Collect shared backdrops
        this.#sharedBackdrops.forEach(shared => {
            if (shared.element) {
                elementsToRemove.push(shared.element);
            }
        });
        
        // Clear tracking immediately
        this.#backdrops.clear();
        this.#sharedBackdrops.clear();
        this.#registry.destroy();
        
        // Remove all elements without animation
        elementsToRemove.forEach(element => {
            if (element && element.parentNode) {
                element.remove();
            }
        });
    }
    
    /**
     * Updates backdrop opacity
     * @param {string} portalId - Associated portal ID
     * @param {number} opacity - New opacity value
     */
    updateOpacity(portalId, opacity) {
        const backdrop = this.#backdrops.get(portalId);
        if (!backdrop) {
            throw new Error(`[BackdropManager] Backdrop for portal ${portalId} not found`);
        }
        
        backdrop.element.style.setProperty('--backdrop-opacity', opacity);
    }
    
    /**
     * Adds click handler to backdrop
     * @param {string} portalId - Associated portal ID
     * @param {Function} handler - Click handler function
     * @returns {Function} Cleanup function to remove handler
     */
    onClick(portalId, handler) {
        const backdrop = this.#backdrops.get(portalId);
        if (!backdrop) {
            throw new Error(`[BackdropManager] Backdrop for portal ${portalId} not found`);
        }
        
        const clickHandler = (event) => {
            if (event.target === backdrop.element) {
                handler(event);
            }
        };
        
        backdrop.element.addEventListener('click', clickHandler);
        backdrop.element.dataset.clickHandler = 'true';
        
        // Return cleanup function
        return () => {
            backdrop.element.removeEventListener('click', clickHandler);
            delete backdrop.element.dataset.clickHandler;
        };
    }
    
    /**
     * Checks if a backdrop exists
     * @param {string} portalId - Associated portal ID
     * @returns {boolean} True if backdrop exists
     */
    hasBackdrop(portalId) {
        return this.#backdrops.has(portalId);
    }
    
    /**
     * Gets backdrop data
     * @param {string} portalId - Associated portal ID
     * @returns {Object} Backdrop data
     */
    getBackdrop(portalId) {
        const backdrop = this.#backdrops.get(portalId);
        if (!backdrop) {
            throw new Error(`[BackdropManager] Backdrop for portal ${portalId} not found`);
        }
        return backdrop;
    }
    
    // Private helper methods
    
    #getAnimationDuration() {
        try {
            const rootStyles = getComputedStyle(document.documentElement);
            const durationValue = rootStyles.getPropertyValue('--duration-normal').trim();
            
            if (durationValue.endsWith('ms')) {
                return parseInt(durationValue, 10);
            } else if (durationValue.endsWith('s')) {
                return parseFloat(durationValue) * 1000;
            }
        } catch (error) {
            console.warn('[BackdropManager] Failed to read CSS duration variable, using default:', error);
        }
        return 200; // Fallback
    }
    
    #createBackdropElement(portalId, level, config) {
        const backdrop = document.createElement('div');
        backdrop.className = `portal-backdrop ${config.className || ''}`.trim();
        backdrop.dataset.portalId = portalId;
        backdrop.dataset.backdropLevel = level.toString();
        
        const opacity = this.#calculateOpacity(level);
        backdrop.style.setProperty('--backdrop-opacity', opacity);
        backdrop.style.opacity = '0'; // Start invisible for animation
        
        if (config.blur) {
            backdrop.style.setProperty('--backdrop-blur', `${config.blur}px`);
        }
        
        // Get or create portal root container
        let container = document.getElementById('portal-root');
        if (!container) {
            container = document.createElement('div');
            container.id = 'portal-root';
            container.className = 'portal-root';
            container.setAttribute('data-rr-blazor-portal', 'true');
            container.style.cssText = `
                position: fixed;
                top: 0;
                left: 0;
                width: 100vw;
                height: 100vh;
                z-index: 9999;
                pointer-events: none;
                overflow: hidden;
            `;
            document.body.appendChild(container);
        }
        
        // Insert backdrop before the portal if it exists
        const portal = document.getElementById(portalId);
        if (portal && portal.parentNode === container) {
            container.insertBefore(backdrop, portal);
        } else {
            container.appendChild(backdrop);
        }
        
        return { element: backdrop, refCount: 0 };
    }
    
    #calculateOpacity(level) {
        const opacity = Math.min(
            BackdropManagerBase.#baseOpacity + (level * BackdropManagerBase.#opacityIncrement),
            BackdropManagerBase.#maxOpacity
        );
        return opacity;
    }
    
    #animateIn(element, duration = this.#animationDuration) {
        element.style.transition = `opacity ${duration}ms ease-out`;
        
        requestAnimationFrame(() => {
            element.style.opacity = 'var(--backdrop-opacity, 0.5)';
        });
    }
    
    #removeBackdropElement(element, duration = this.#animationDuration) {
        if (!element || !element.parentNode) {
            return;
        }
        
        // Mark element for removal to prevent double-execution
        if (element.dataset.removing === 'true') {
            return;
        }
        
        element.dataset.removing = 'true';
        
        // Handle animation and removal
        if (duration > 0) {
            element.style.transition = `opacity ${duration}ms ease-in`;
            element.style.opacity = '0';
            
            // Let CSS handle animation completion
            element.addEventListener('transitionend', function onTransitionEnd() {
                if (element.parentNode) {
                    element.remove();
                }
            }, { once: true });
        } else {
            // No animation, remove immediately
            element.remove();
        }
    }
}

// Create singleton using factory
export const BackdropManager = createSingleton(BackdropManagerBase, 'BackdropManager');

// Generic event-driven backdrop integration (application-agnostic)
document.addEventListener('backdrop-create-request', (event) => {
    const { requesterId, config } = event.detail;
    const backdrop = BackdropManager.getInstance().create(requesterId, config);
    
    const responseEvent = new CustomEvent('backdrop-created', {
        detail: { requesterId, backdrop },
        bubbles: true
    });
    document.dispatchEvent(responseEvent);
});

document.addEventListener('backdrop-destroy-request', (event) => {
    const { requesterId } = event.detail;
    if (BackdropManager.getInstance().hasBackdrop(requesterId)) {
        BackdropManager.getInstance().destroy(requesterId);
        
        const responseEvent = new CustomEvent('backdrop-destroyed', {
            detail: { requesterId },
            bubbles: true
        });
        document.dispatchEvent(responseEvent);
    }
});

document.addEventListener('backdrop-cleanup-all-request', () => {
    BackdropManager.getInstance().destroyAll();
    
    const responseEvent = new CustomEvent('backdrop-all-destroyed', {
        bubbles: true
    });
    document.dispatchEvent(responseEvent);
});

// Auto-cleanup on page unload
window.addEventListener('beforeunload', () => {
    if (BackdropManager.hasInstance()) {
        BackdropManager.getInstance().destroyAll();
        BackdropManager.destroyInstance();
    }
});

// Export for ES6 modules
export default BackdropManager;

// Export pure backdrop management functions
export function createBackdrop(portalId, config) {
    return BackdropManager.getInstance().create(portalId, config);
}

export function destroyBackdrop(portalId) {
    return BackdropManager.getInstance().destroy(portalId);
}

export function hasBackdrop(portalId) {
    return BackdropManager.getInstance().hasBackdrop(portalId);
}

export function updateBackdropOpacity(portalId, opacity) {
    return BackdropManager.getInstance().updateOpacity(portalId, opacity);
}

export function onBackdropClick(portalId, handler) {
    return BackdropManager.getInstance().onClick(portalId, handler);
}