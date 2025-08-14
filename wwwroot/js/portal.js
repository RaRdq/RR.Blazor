import { createSingleton, WeakRegistry } from './utils/singleton-factory.js';

class PortalManagerBase {
    static _baseZIndex = 1000;
    static _zIndexIncrement = 10;
    
    constructor() {
        this._portals = new Map();
        this._registry = new WeakRegistry();
        this._container = null;
        this._nextId = 1;
        this._container = document.getElementById('portal-root') || this._createContainer();
    }
    
    _createContainer() {
        const container = document.createElement('div');
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
        
        return container;
    }
    
    create(config = {}) {
        const id = config.id || `portal-${this._nextId++}`;
        
        if (this._portals.has(id)) {
            throw new Error(`Portal ${id} already exists - this is a programming error`);
        }
        
        const portal = document.createElement('div');
        portal.id = id;
        portal.className = `portal ${config.className || ''}`.trim();
        portal.dataset.portalId = id;
        portal.dataset.portalLevel = this._portals.size.toString();
        
        const zIndex = this._calculateZIndex();
        portal.style.zIndex = zIndex.toString();
        portal.dataset.zIndex = zIndex.toString();
        
        portal.style.pointerEvents = 'auto';
        
        if (config.position) {
            this._applyPosition(portal, config.position);
        }
        
        if (config.attributes) {
            Object.entries(config.attributes).forEach(([key, value]) => {
                portal.setAttribute(key, value);
            });
        }
        
        this._container.appendChild(portal);
        
        const portalData = {
            element: portal,
            zIndex,
            level: this._portals.size,
            created: Date.now(),
            config
        };
        
        this._portals.set(id, portalData);
        this._registry.register(id, portalData);
        
        return {
            id,
            element: portal,
            zIndex
        };
    }
    
    destroy(id) {
        const portal = this._portals.get(id);
        if (!portal) {
            throw new Error(`Portal ${id} not found - cannot destroy non-existent portal`);
        }
        
        // Emit event before destroying (Dependency Inversion)
        const destroyEvent = new CustomEvent('portal-destroying', {
            detail: { portalId: id, portal: portal.element },
            bubbles: true,
            cancelable: false
        });
        document.dispatchEvent(destroyEvent);
        
        // Track if we're removing the highest z-index portal
        const wasMaxZIndex = portal.zIndex === this._maxZIndex;
        
        this._portals.delete(id);
        this._registry.delete(id);
        
        // Update max z-index efficiently
        if (wasMaxZIndex && this._portals.size > 0) {
            // Only recalculate if we removed the highest portal
            let newMax = PortalManagerBase._baseZIndex;
            this._portals.forEach(p => {
                if (p.zIndex > newMax) newMax = p.zIndex;
            });
            this._maxZIndex = newMax;
        } else if (this._portals.size === 0) {
            // Reset to base if no portals left
            this._maxZIndex = PortalManagerBase._baseZIndex;
        }
        
        if (!portal.element) {
            throw new Error(`Portal ${id} has no DOM element - corrupted state`);
        }
        portal.element.remove();
        
        this._reindexPortals();
        
        // Emit event after destroying
        const destroyedEvent = new CustomEvent('portal-destroyed', {
            detail: { portalId: id },
            bubbles: true
        });
        document.dispatchEvent(destroyedEvent);
    }
    
    destroyAll() {
        const portalsToClean = new Map(this._portals);
        
        this._portals.clear();
        this._registry.destroy();
        
        portalsToClean.forEach((portal, id) => {
            if (!portal.element) {
                throw new Error(`Portal ${id} has no DOM element during cleanup`);
            }
            portal.element.remove();
        });
    }
    
    getPortal(id) {
        const portal = this._portals.get(id);
        if (!portal) {
            throw new Error(`Portal ${id} not found - invalid portal ID`);
        }
        return portal;
    }
    
    getTopPortal() {
        if (this._portals.size === 0) {
            return null;
        }
        
        let topPortal = null;
        let maxZIndex = -1;
        
        this._portals.forEach(portal => {
            if (portal.zIndex > maxZIndex) {
                maxZIndex = portal.zIndex;
                topPortal = portal;
            }
        });
        
        return topPortal;
    }
    
    updateZIndex(id, zIndex) {
        const portal = this._portals.get(id);
        if (!portal) {
            throw new Error(`Portal ${id} not found - cannot update non-existent portal`);
        }
        
        portal.element.style.zIndex = zIndex.toString();
        portal.element.dataset.zIndex = zIndex.toString();
        portal.zIndex = zIndex;
    }
    
    updatePosition(id, position) {
        const portal = this._portals.get(id);
        if (!portal) {
            throw new Error(`Portal ${id} not found - cannot position non-existent portal`);
        }
        
        this._applyPosition(portal.element, position);
        return true;
    }
    
    isPortalActive(id) {
        return this._portals.has(id);
    }
    
    getActivePortalCount() {
        return this._portals.size;
    }
    
    getAllPortals() {
        return Array.from(this._portals.values());
    }
    
    getContainer() {
        return this._container;
    }
    
    _applyPosition(element, position) {
        element.style.position = 'absolute';
        element.style.left = `${position.x}px`;
        element.style.top = `${position.y}px`;
        
        if (position.width) {
            element.style.width = `${position.width}px`;
        }
        if (position.maxHeight) {
            element.style.maxHeight = `${position.maxHeight}px`;
        }
    }
    
    _calculateZIndex() {
        // Find the highest z-index among existing portals
        let maxZIndex = PortalManagerBase._baseZIndex;
        this._portals.forEach(portal => {
            if (portal.zIndex >= maxZIndex) {
                maxZIndex = portal.zIndex + PortalManagerBase._zIndexIncrement;
            }
        });
        
        if (maxZIndex > 2147483647) {
            throw new Error('[PortalManager] Z-index limit exceeded');
        }
        
        return maxZIndex;
    }
    
    _reindexPortals() {
        let level = 0;
        this._portals.forEach((portal, id) => {
            const zIndex = PortalManagerBase._baseZIndex + (level * PortalManagerBase._zIndexIncrement);
            // Update z-index directly to avoid error throwing
            portal.element.style.zIndex = zIndex.toString();
            portal.element.dataset.zIndex = zIndex.toString();
            portal.zIndex = zIndex;
            portal.level = level;
            portal.element.dataset.portalLevel = level.toString();
            level++;
        });
    }
    
    // Move element into portal and preserve original position
    moveToPortal(id, element) {
        const portal = this.getPortal(id);
        if (!portal) {
            throw new Error(`Portal ${id} not found`);
        }
        
        // Store original position for restoration
        element._originalParent = element.parentElement;
        element._originalNextSibling = element.nextElementSibling;
        
        // Move element into portal
        portal.element.appendChild(element);
        element.setAttribute('data-portal-positioned', 'true');
        element._portalPositioned = true;
        
        // Emit event for other modules to react (Dependency Inversion)
        const moveEvent = new CustomEvent('portal-element-moved', {
            detail: { portalId: id, element },
            bubbles: true
        });
        document.dispatchEvent(moveEvent);
        
        return portal.element;
    }
    
    // Restore element from portal to original position
    restoreFromPortal(id, element) {
        if (!element._portalPositioned) {
            return false;
        }
        
        // Restore to original position
        if (element._originalParent) {
            if (element._originalNextSibling) {
                element._originalParent.insertBefore(element, element._originalNextSibling);
            } else {
                element._originalParent.appendChild(element);
            }
            delete element._originalParent;
            delete element._originalNextSibling;
        }
        
        // Clean up attributes
        element.removeAttribute('data-portal-positioned');
        delete element._portalPositioned;
        
        // Emit event for cleanup (Dependency Inversion)
        const restoreEvent = new CustomEvent('portal-element-restored', {
            detail: { portalId: id, element },
            bubbles: true
        });
        document.dispatchEvent(restoreEvent);
        
        // Destroy the portal itself
        this.destroy(id);
        
        return true;
    }
}

export const PortalManager = createSingleton(PortalManagerBase, 'PortalManager');

document.addEventListener('portal-create-request', (event) => {
    const { requesterId, config } = event.detail;
    const portal = PortalManager.getInstance().create(config);
    
    const responseEvent = new CustomEvent('portal-created', {
        detail: { requesterId, portal },
        bubbles: true
    });
    document.dispatchEvent(responseEvent);
});

document.addEventListener('portal-destroy-request', (event) => {
    const { requesterId, portalId } = event.detail;
    if (PortalManager.getInstance().isPortalActive(portalId)) {
        PortalManager.getInstance().destroy(portalId);
        
        const responseEvent = new CustomEvent('portal-destroyed', {
            detail: { requesterId, portalId },
            bubbles: true
        });
        document.dispatchEvent(responseEvent);
    }
});

document.addEventListener('portal-cleanup-all-request', () => {
    PortalManager.getInstance().destroyAll();
    
    const responseEvent = new CustomEvent('portal-all-destroyed', {
        bubbles: true
    });
    document.dispatchEvent(responseEvent);
});

// Auto-cleanup on page unload
window.addEventListener('beforeunload', () => {
    if (PortalManager.hasInstance()) {
        PortalManager.getInstance().destroyAll();
        PortalManager.destroyInstance();
    }
});

// Export for ES6 modules
export default PortalManager;

// Export pure DOM manipulation functions
export function getInstance() {
    return PortalManager.getInstance();
}

export function createPortal(config) {
    return PortalManager.getInstance().create(config);
}

export function destroyPortal(id) {
    return PortalManager.getInstance().destroy(id);
}

export function getPortal(id) {
    return PortalManager.getInstance().getPortal(id);
}

export function getTopPortal() {
    return PortalManager.getInstance().getTopPortal();
}

export function updatePortalPosition(id, position) {
    return PortalManager.getInstance().updatePosition(id, position);
}

export function updatePortalZIndex(id, zIndex) {
    return PortalManager.getInstance().updateZIndex(id, zIndex);
}

export function moveToPortal(id, element) {
    return PortalManager.getInstance().moveToPortal(id, element);
}

export function restoreFromPortal(id, element) {
    return PortalManager.getInstance().restoreFromPortal(id, element);
}