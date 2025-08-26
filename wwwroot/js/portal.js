import { createSingleton, WeakRegistry } from './utils/singleton-factory.js';

const debugLogger = window.debugLogger;

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
            debugLogger.warn(`Portal ${id} not found - already destroyed or never created`);
            return false;
        }
        
        if (window.RRBlazor && window.RRBlazor.EventDispatcher) {
            window.RRBlazor.EventDispatcher.dispatch(
                window.RRBlazor.Events.PORTAL_DESTROYING,
                { portalId: id, portal: portal.element },
                { cancelable: false }
            );
        }
        
        const wasMaxZIndex = portal.zIndex === this._maxZIndex;
        
        this._portals.delete(id);
        this._registry.delete(id);
        
        if (wasMaxZIndex && this._portals.size > 0) {
            let newMax = PortalManagerBase._baseZIndex;
            this._portals.forEach(p => {
                if (p.zIndex > newMax) newMax = p.zIndex;
            });
            this._maxZIndex = newMax;
        } else if (this._portals.size === 0) {
            this._maxZIndex = PortalManagerBase._baseZIndex;
        }
        
        if (!portal.element) {
            throw new Error(`Portal ${id} has no DOM element - corrupted state`);
        }
        portal.element.remove();
        
        this._reindexPortals();
        
        if (window.RRBlazor && window.RRBlazor.EventDispatcher) {
            window.RRBlazor.EventDispatcher.dispatch(
                window.RRBlazor.Events.PORTAL_DESTROYED,
                { portalId: id }
            );
        }
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
            portal.element.style.zIndex = zIndex.toString();
            portal.element.dataset.zIndex = zIndex.toString();
            portal.zIndex = zIndex;
            portal.level = level;
            portal.element.dataset.portalLevel = level.toString();
            level++;
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
        
        this.destroy(id);
        
        return true;
    }
}

export const PortalManager = createSingleton(PortalManagerBase, 'PortalManager');

// Defer event listener registration until RRBlazor.Events is available
function setupPortalEventListeners() {
    if (!window.RRBlazor || !window.RRBlazor.Events) {
        setTimeout(setupPortalEventListeners, 10);
        return;
    }
    
    document.addEventListener(window.RRBlazor.Events.PORTAL_CREATE_REQUEST, (event) => {
        const { requesterId, config } = event.detail;
        const portal = PortalManager.getInstance().create(config);
        
        window.RRBlazor.EventDispatcher.dispatch(
            window.RRBlazor.Events.PORTAL_CREATED,
            { requesterId, portal }
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