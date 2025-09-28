import { createSingleton, WeakRegistry } from './utils/singleton-factory.js';

class PortalManagerBase {
    
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
        
        const containerZIndex = window.RRBlazor.ZIndexManager.registerElement('portal-container', 'portal');
        container.style.cssText = `
            position: fixed;
            top: 0;
            left: 0;
            width: 100vw;
            height: 100vh;
            z-index: ${containerZIndex};
            pointer-events: none;
            overflow: hidden;
        `;
        
        document.body.appendChild(container);
        
        return container;
    }
    
    create(config = {}) {
        const id = config.id || `portal-${this._nextId++}`;

        if (this._portals.has(id)) {
            throw new Error(`Portal ${id} already exists - fix the duplicate creation`);
        }
        
        const portal = document.createElement('div');
        portal.id = id;
        portal.className = `portal ${config.className || ''}`.trim();
        portal.dataset.portalId = id;
        portal.dataset.portalLevel = this._portals.size.toString();
        
        if (!config.zIndex) {
            throw new Error(`Portal ${id} requires zIndex to be provided in config - use ZIndexManager.registerElement()`);
        }
        const zIndex = config.zIndex;
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
            return false;
        }
        
        window.RRBlazor.EventDispatcher.dispatch(
            window.RRBlazor.Events.PORTAL_DESTROYING,
            { portalId: id, portal: portal.element },
            { cancelable: false }
        );
        
        this._portals.delete(id);
        this._registry.delete(id);

        window.RRBlazor.ZIndexManager.unregisterElement(id);
        
        portal.element.remove();
        
        this._reindexPortals();
        
        window.RRBlazor.EventDispatcher.dispatch(
            window.RRBlazor.Events.PORTAL_DESTROYED,
            { portalId: id }
        );
    }
    
    destroyAll() {
        const portalsToClean = new Map(this._portals);
        
        this._portals.clear();
        this._registry.destroy();
        
        portalsToClean.forEach((portal, id) => {
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
        const exists = this._portals.has(id);
        const portal = this._portals.get(id);

        // Fix Map corruption: if key exists but value is undefined, clean it up
        if (exists && !portal) {
            this._portals.delete(id);
            this._registry.delete(id);
            return false;
        }

        return exists && portal;
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
    
    
    _reindexPortals() {
        this._portals.forEach((portal, id) => {
            const zIndex = window.RRBlazor.ZIndexManager.getZIndexForElement(id) || 
                          window.RRBlazor.ZIndexManager.registerElement(id, 'portal');
            portal.element.style.zIndex = zIndex.toString();
            portal.element.dataset.zIndex = zIndex.toString();
            portal.zIndex = zIndex;
        });
    }
    
    moveToPortal(id, element) {
        const portal = this.getPortal(id);
        if (!portal) {
            throw new Error(`Portal ${id} not found`);
        }
        
        element._originalParent = element.parentElement;
        element._originalNextSibling = element.nextElementSibling;
        
        portal.element.appendChild(element);
        element.setAttribute('data-portal-positioned', 'true');
        element._portalPositioned = true;
        
        if (window.RRBlazor && window.RRBlazor.EventDispatcher) {
            window.RRBlazor.EventDispatcher.dispatch(
                window.RRBlazor.Events.PORTAL_ELEMENT_MOVED,
                { portalId: id, element }
            );
        }
        
        return portal.element;
    }
    
    restoreFromPortal(id, element) {
        if (!element._portalPositioned) {
            return false;
        }

        if (element._originalParent) {
            if (element._originalNextSibling) {
                element._originalParent.insertBefore(element, element._originalNextSibling);
            } else {
                element._originalParent.appendChild(element);
            }
            delete element._originalParent;
            delete element._originalNextSibling;
        }

        element.removeAttribute('data-portal-positioned');
        delete element._portalPositioned;

        if (window.RRBlazor && window.RRBlazor.EventDispatcher) {
            window.RRBlazor.EventDispatcher.dispatch(
                window.RRBlazor.Events.PORTAL_ELEMENT_RESTORED,
                { portalId: id, element }
            );
        }

        return true;
    }
}

export const PortalManager = createSingleton(PortalManagerBase, 'PortalManager');

// Setup portal event listeners - fail fast if dependencies missing
function setupPortalEventListeners() {
    if (!window.RRBlazor?.Events) {
        throw new Error('RRBlazor.Events not available - fix initialization order');
    }
    
    document.addEventListener(window.RRBlazor.Events.PORTAL_CREATE_REQUEST, (event) => {
        const { requesterId, element, config } = event.detail;
        const portal = PortalManager.getInstance().create(config);

        let elementMoved = false;
        if (element) {
            try {
                PortalManager.getInstance().moveToPortal(config.id, element);
                elementMoved = true;
            } catch (error) {
                throw error;
            }
        }

        window.RRBlazor.EventDispatcher.dispatch(
            window.RRBlazor.Events.PORTAL_CREATED,
            { requesterId, portal, elementMoved, element: portal.element }
        );
    });
    
    document.addEventListener(window.RRBlazor.Events.PORTAL_DESTROY_REQUEST, (event) => {
        const { requesterId, portalId } = event.detail;
        if (PortalManager.getInstance().isPortalActive(portalId)) {
            PortalManager.getInstance().destroy(portalId);
            
            window.RRBlazor.EventDispatcher.dispatch(
                window.RRBlazor.Events.PORTAL_DESTROYED,
                { requesterId, portalId }
            );
        }
    });
    
    document.addEventListener(window.RRBlazor.Events.PORTAL_CLEANUP_ALL_REQUEST, () => {
        PortalManager.getInstance().destroyAll();
        
        window.RRBlazor.EventDispatcher.dispatch(
            window.RRBlazor.Events.PORTAL_ALL_DESTROYED
        );
    });
}

setupPortalEventListeners();



window.addEventListener('beforeunload', () => {
    if (PortalManager.hasInstance()) {
        PortalManager.getInstance().destroyAll();
        PortalManager.destroyInstance();
    }
});

export default PortalManager;

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

export function ensureContainer() {
    return PortalManager.getInstance().getContainer() || PortalManager.getInstance()._createContainer();
}

export function getContainer() {
    return PortalManager.getInstance().getContainer();
}