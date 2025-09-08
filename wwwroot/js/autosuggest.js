import { PositioningEngine } from './positioning.js';
import { getInstance as getPortalManager } from './portal.js';

const positioningEngine = new PositioningEngine();

class AutosuggestManager {
    constructor() {
        this.activeDropdowns = new Map();
    }

    async openDropdown(autosuggestId, options = {}) {
        const autosuggestElement = document.querySelector(`[data-autosuggest-id="${autosuggestId}"]`);
        if (!autosuggestElement) return null;

        const viewport = autosuggestElement.querySelector('.autosuggest-viewport');
        if (!viewport) return null;

        if (this.activeDropdowns.has(autosuggestId)) {
            return this.activeDropdowns.get(autosuggestId).portalId;
        }

        const triggerElement = autosuggestElement.querySelector('.autosuggest-input, input');
        if (!triggerElement) return null;

        const portalManager = getPortalManager();
        const portal = portalManager.create({
            id: `autosuggest-portal-${autosuggestId}`,
            className: 'autosuggest-portal'
        });

        viewport._originalParent = viewport.parentNode;
        viewport._originalNextSibling = viewport.nextSibling;
        
        portal.element.appendChild(viewport);
        
        const triggerRect = triggerElement.getBoundingClientRect();
        const dropdownHeight = Math.min(320, window.innerHeight * 0.4);
        const targetDimensions = {
            width: triggerRect.width,
            height: dropdownHeight
        };

        const desiredPosition = options.position || PositioningEngine.POSITIONS.BOTTOM_START;
        
        const position = positioningEngine.calculatePosition(
            triggerRect,
            targetDimensions,
            {
                position: desiredPosition,
                offset: options.offset || 4,
                flip: options.flip !== false,
                constrain: options.constrain !== false
            }
        );

        // Position viewport
        viewport.style.position = 'fixed';
        viewport.style.left = `${position.x}px`;
        viewport.style.top = `${position.y}px`;
        viewport.style.width = `${targetDimensions.width}px`;
        viewport.style.maxHeight = `${dropdownHeight}px`;
        viewport.style.zIndex = portal.zIndex.toString();
        
        // Start with animating-open state (invisible, positioned)
        viewport.classList.remove('autosuggest-viewport-closed');
        viewport.classList.add('autosuggest-viewport-animating-open');
        viewport.style.visibility = 'visible';
        
        // Register dropdown before animation
        this.activeDropdowns.set(autosuggestId, {
            portalId: portal.id,
            viewport,
            triggerElement,
            autosuggestElement
        });
        
        this.setupEventHandlers(autosuggestId);
        
        // Trigger animation to open state after a micro-task
        requestAnimationFrame(() => {
            if (this.activeDropdowns.has(autosuggestId)) {
                viewport.classList.remove('autosuggest-viewport-animating-open');
                viewport.classList.add('autosuggest-viewport-open');
            }
        });
        
        window.RRBlazor.EventDispatcher.dispatch(
            window.RRBlazor.Events.UI_COMPONENT_OPENED,
            {
                componentType: 'autosuggest',
                componentId: autosuggestId
            }
        );
        
        return portal.id;
    }

    closeDropdown(autosuggestId, animated = true) {
        const dropdownData = this.activeDropdowns.get(autosuggestId);
        if (!dropdownData) return;

        const { portalId, viewport } = dropdownData;
        
        this.cleanupEventHandlers(autosuggestId);
        
        if (!animated) {
            // Immediate close (for cleanup/disposal)
            this._finishClose(autosuggestId, viewport, portalId);
            return;
        }
        
        // Start animated close
        viewport.classList.remove('autosuggest-viewport-open');
        viewport.classList.add('autosuggest-viewport-animating-close');
        
        // Wait for animation to complete before final cleanup
        setTimeout(() => {
            if (this.activeDropdowns.has(autosuggestId)) {
                this._finishClose(autosuggestId, viewport, portalId);
            }
        }, 350); // Match CSS --duration-normal
    }
    
    _finishClose(autosuggestId, viewport, portalId) {
        viewport.classList.remove('autosuggest-viewport-open', 'autosuggest-viewport-animating-close');
        viewport.classList.add('autosuggest-viewport-closed');
        
        viewport.style.visibility = 'hidden';
        viewport.style.position = 'absolute';
        viewport.style.top = '-9999px';
        viewport.style.left = '-9999px';
        
        if (viewport._originalParent) {
            if (viewport._originalNextSibling) {
                viewport._originalParent.insertBefore(viewport, viewport._originalNextSibling);
            } else {
                viewport._originalParent.appendChild(viewport);
            }
            delete viewport._originalParent;
            delete viewport._originalNextSibling;
        }
        
        const portalManager = getPortalManager();
        portalManager.destroy(portalId);
        
        this.activeDropdowns.delete(autosuggestId);
        
        window.RRBlazor.EventDispatcher.dispatch(
            window.RRBlazor.Events.UI_COMPONENT_CLOSED,
            {
                componentType: 'autosuggest',
                componentId: autosuggestId
            }
        );
    }

    setupEventHandlers(autosuggestId) {
        const dropdownData = this.activeDropdowns.get(autosuggestId);
        if (!dropdownData) return;

        const scrollHandler = () => this.reposition(autosuggestId);
        const resizeHandler = () => this.reposition(autosuggestId);

        dropdownData.scrollHandler = scrollHandler;
        dropdownData.resizeHandler = resizeHandler;

        window.addEventListener('scroll', scrollHandler, { passive: true });
        window.addEventListener('resize', resizeHandler, { passive: true });

        // Create click-outside handler that triggers animated close
        const clickOutsideHandler = (event) => {
            // Only handle events for this specific autosuggest instance
            if (event.detail && event.detail.elementId === `autosuggest-clickoutside-${autosuggestId}`) {
                this.closeDropdown(autosuggestId, true); // Use animated close
            }
        };
        
        dropdownData.clickOutsideHandler = clickOutsideHandler;
        document.addEventListener(window.RRBlazor.Events.CLICK_OUTSIDE, clickOutsideHandler);
        
        window.RRBlazor.ClickOutside.register(
            `autosuggest-clickoutside-${autosuggestId}`,
            dropdownData.autosuggestElement,
            {
                excludeSelectors: [
                    '.autosuggest-viewport',
                    '.autosuggest-dropdown',
                    '.autosuggest-portal',
                    '.portal',
                    '[data-portal-positioned="true"]'
                ]
            }
        );
    }

    cleanupEventHandlers(autosuggestId) {
        const dropdownData = this.activeDropdowns.get(autosuggestId);
        if (!dropdownData) return;

        if (dropdownData.scrollHandler) {
            window.removeEventListener('scroll', dropdownData.scrollHandler);
        }
        if (dropdownData.resizeHandler) {
            window.removeEventListener('resize', dropdownData.resizeHandler);
        }
        if (dropdownData.clickOutsideHandler) {
            document.removeEventListener(window.RRBlazor.Events.CLICK_OUTSIDE, dropdownData.clickOutsideHandler);
        }

        window.RRBlazor.ClickOutside.unregister(`autosuggest-clickoutside-${autosuggestId}`);
    }

    reposition(autosuggestId) {
        const dropdownData = this.activeDropdowns.get(autosuggestId);
        if (!dropdownData) return;

        const { viewport, triggerElement } = dropdownData;
        const triggerRect = triggerElement.getBoundingClientRect();
        
        if (triggerRect.width === 0) {
            this.closeDropdown(autosuggestId);
            return;
        }

        const targetDimensions = {
            width: triggerRect.width,
            height: parseFloat(viewport.style.maxHeight) || 320
        };

        const position = positioningEngine.calculatePosition(
            triggerRect,
            targetDimensions,
            {
                position: PositioningEngine.POSITIONS.BOTTOM_START,
                offset: 4,
                flip: true,
                constrain: true
            }
        );
        
        viewport.style.left = `${position.x}px`;
        viewport.style.top = `${position.y}px`;
        viewport.style.width = `${targetDimensions.width}px`;
    }

    destroyAll() {
        for (const autosuggestId of this.activeDropdowns.keys()) {
            this.closeDropdown(autosuggestId, false); // Immediate close for cleanup
        }
    }
}

const autosuggestManager = new AutosuggestManager();

export function createPortal(autosuggestId, options) {
    return autosuggestManager.openDropdown(autosuggestId, options);
}

export function closeDropdown(autosuggestId) {
    autosuggestManager.closeDropdown(autosuggestId);
}

export function repositionDropdown(autosuggestId) {
    autosuggestManager.reposition(autosuggestId);
}

export function destroyAllPortals() {
    autosuggestManager.destroyAll();
}

window.RRBlazor = window.RRBlazor || {};
window.RRBlazor.Autosuggest = {
    createPortal,
    closeDropdown,
    repositionDropdown,
    destroyAllPortals
};

export default {
    createPortal,
    closeDropdown,
    repositionDropdown,
    destroyAllPortals
};