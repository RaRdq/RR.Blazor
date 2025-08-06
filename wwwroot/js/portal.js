// RR.Blazor Unified Portal System
// Single source of truth for all portal, popup, and positioning needs

class PortalManager {
    constructor() {
        this.portals = new Map();
        this.zIndexManager = new ZIndexManager();
        this.positioningEngine = new PositioningEngine();
        this.eventManager = new PortalEventManager();
    }
    
    // Create a portal with unified options
    create(element, options = {}) {
        if (!element) {
            console.warn('[PortalManager] No element provided');
            return null;
        }
        
        const portalId = options.id || `portal-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
        const targetContainer = this.resolveTarget(options.target || 'body');
        
        if (!targetContainer) {
            console.warn('[PortalManager] Target container not found');
            return null;
        }
        
        // Store original position for restoration
        const originalState = {
            parent: element.parentElement,
            nextSibling: element.nextSibling,
            display: element.style.display,
            position: element.style.position
        };
        
        // Create portal container
        const portalContainer = this.createPortalContainer(portalId, options);
        
        // Handle different portal types
        const portalType = options.type || 'generic';
        switch (portalType) {
            case 'tooltip':
                // Clone content for tooltips to preserve styling
                const clonedContent = element.cloneNode(true);
                portalContainer.appendChild(clonedContent);
                element.style.display = 'none'; // Hide original
                break;
            case 'modal':
                // Modals get special backdrop handling
                if (options.backdrop !== false) {
                    this.createBackdrop(portalId);
                }
                portalContainer.appendChild(element);
                break;
            default:
                // Standard portal - move element
                portalContainer.appendChild(element);
        }
        
        // Apply z-index
        const zIndex = this.zIndexManager.getNext(portalType);
        portalContainer.style.zIndex = zIndex;
        
        // Append to target
        targetContainer.appendChild(portalContainer);
        
        // Create portal instance
        const portal = {
            id: portalId,
            element,
            container: portalContainer,
            type: portalType,
            originalState,
            zIndex,
            options,
            anchor: options.anchor,
            isOpen: true,
            isDestroying: false,
            backdropElement: null,
            backdropZIndex: null
        };
        
        this.portals.set(portalId, portal);
        
        // Setup positioning if anchor provided (but NOT for modals)
        if (options.anchor && portalType !== 'modal') {
            this.position(portalId);
            
            // Track scroll events to reposition
            const scrollHandler = () => {
                this.position(portalId);
            };
            
            // Track scroll on window and all parent elements
            window.addEventListener('scroll', scrollHandler, true);
            portal.scrollHandler = scrollHandler;
            
            // Also track resize events
            const resizeHandler = () => {
                this.position(portalId);
            };
            
            window.addEventListener('resize', resizeHandler);
            portal.resizeHandler = resizeHandler;
        }
        
        // Setup event handlers
        if (options.onClickOutside || options.closeOnClickOutside) {
            const callbackMethod = options.backdropCallbackMethod || 'HandleClickOutside';
            this.eventManager.setupClickOutside(portal, options.onClickOutside, options.dotNetRef, callbackMethod);
        }
        
        if (options.onEscape || options.closeOnEscape) {
            const callbackMethod = options.escapeCallbackMethod || 'HandleEscape';
            this.eventManager.setupEscape(portal, options.onEscape, options.dotNetRef, callbackMethod);
        }
        
        return portalId;
    }
    
    // Update portal position
    position(portalId) {
        const portal = this.portals.get(portalId);
        if (!portal || !portal.anchor) return;
        
        const elementToMeasure = portal.element || portal.container;
        
        const position = this.positioningEngine.calculate(
            portal.anchor, 
            elementToMeasure,
            portal.options
        );
        
        this.positioningEngine.apply(portal.container, position);
        
        // Add position classes for CSS hooks
        const classes = this.positioningEngine.getPositionClasses(position);
        portal.container.className = `rr-portal rr-portal-${portal.type} ${classes} ${portal.options.className || ''}`.trim();
    }
    
    // Destroy portal and restore element with comprehensive error handling
    destroy(portalId) {
        const portal = this.portals.get(portalId);
        if (!portal) {
            // Don't log for modal-auto-* portals as these are common during cleanup
            if (!portalId.startsWith('modal-auto-')) {
                console.warn(`[PortalManager] Portal '${portalId}' not found`);
            }
            return false;
        }
        
        // Prevent race conditions during destruction
        if (portal.isDestroying) {
            console.warn(`[PortalManager] Portal '${portalId}' is already being destroyed`);
            return false;
        }
        portal.isDestroying = true;
        portal.isOpen = false;
        
        try {
            // Clean up event handlers
            this.eventManager.cleanup(portal);
            
            // Clean up scroll and resize handlers
            if (portal.scrollHandler) {
                try {
                    window.removeEventListener('scroll', portal.scrollHandler, true);
                } catch (e) {
                    console.warn(`[PortalManager] Failed to remove scroll handler: ${e.message}`);
                }
            }
            if (portal.resizeHandler) {
                try {
                    window.removeEventListener('resize', portal.resizeHandler);
                } catch (e) {
                    console.warn(`[PortalManager] Failed to remove resize handler: ${e.message}`);
                }
            }
            
            // Remove backdrop if exists
            this.removeBackdrop(portalId);
            
            // Restore element to original position
            if (portal.originalState.parent && portal.element) {
                try {
                    if (portal.type === 'tooltip') {
                        // For tooltips, just show the original element
                        portal.element.style.display = portal.originalState.display;
                    } else {
                        // Move element back
                        if (portal.originalState.nextSibling) {
                            portal.originalState.parent.insertBefore(portal.element, portal.originalState.nextSibling);
                        } else {
                            portal.originalState.parent.appendChild(portal.element);
                        }
                        
                        // Restore styles
                        portal.element.style.display = portal.originalState.display;
                        portal.element.style.position = portal.originalState.position;
                    }
                } catch (e) {
                    console.warn(`[PortalManager] Failed to restore element position: ${e.message}`);
                }
            }
            
            // Remove portal container
            if (portal.container && portal.container.parentElement) {
                try {
                    portal.container.parentElement.removeChild(portal.container);
                } catch (e) {
                    console.warn(`[PortalManager] Failed to remove container: ${e.message}`);
                }
            }
            
            // Release z-index
            this.zIndexManager.release(portal.type, portal.zIndex);
            
        } finally {
            // Always clean up references even if other operations fail
            this.portals.delete(portalId);
            
            // Run orphaned element cleanup as a safety net
            try {
                this.cleanupOrphanedBackdrops();
            } catch (e) {
                console.warn(`[PortalManager] Failed to run orphaned cleanup: ${e.message}`);
            }
        }
        
        return true;
    }
    
    // Update portal options
    update(portalId, options) {
        const portal = this.portals.get(portalId);
        if (!portal) return false;
        
        portal.options = { ...portal.options, ...options };
        
        if (options.anchor && portal.type !== 'modal') {
            portal.anchor = options.anchor;
            this.position(portalId);
        }
        
        // Update event handlers if provided
        if (options.onClickOutside) {
            const callbackMethod = options.backdropCallbackMethod || 'HandleClickOutside';
            this.eventManager.setupClickOutside(portal, options.onClickOutside, options.dotNetRef, callbackMethod);
        }
        
        if (options.onEscape) {
            const callbackMethod = options.escapeCallbackMethod || 'HandleEscape';
            this.eventManager.setupEscape(portal, options.onEscape, options.dotNetRef, callbackMethod);
        }
        
        return true;
    }
    
    // Helper methods
    resolveTarget(target) {
        if (typeof target === 'string') {
            return document.querySelector(target);
        }
        return target;
    }
    
    createPortalContainer(portalId, options) {
        const container = document.createElement('div');
        container.id = `portal-${portalId}`;
        container.className = `rr-portal rr-portal-${options.type || 'generic'} ${options.className || ''}`.trim();
        
        if (options.style) {
            container.setAttribute('style', options.style);
        }
        
        // Transfer data attributes
        if (options.attributes) {
            Object.entries(options.attributes).forEach(([key, value]) => {
                container.setAttribute(`data-${key}`, value);
            });
        }
        
        return container;
    }
    
    createBackdrop(portalId) {
        // Check if backdrop already exists
        const existingBackdrop = document.getElementById(`backdrop-${portalId}`);
        if (existingBackdrop) {
            console.warn(`[PortalManager] Backdrop for portal '${portalId}' already exists`);
            return existingBackdrop;
        }
        
        const backdrop = document.createElement('div');
        backdrop.id = `backdrop-${portalId}`;
        backdrop.className = 'rr-portal-backdrop';
        backdrop.setAttribute('data-portal-id', portalId);
        const zIndex = this.zIndexManager.getNext('backdrop');
        backdrop.style.zIndex = zIndex;
        document.body.appendChild(backdrop);
        
        // Store backdrop reference in portal object
        const portal = this.portals.get(portalId);
        if (portal) {
            portal.backdropElement = backdrop;
            portal.backdropZIndex = zIndex;
        }
        
        return backdrop;
    }
    
    removeBackdrop(portalId) {
        const portal = this.portals.get(portalId);
        let backdropRemoved = false;
        
        // Strategy 1: Try to remove using stored reference
        if (portal && portal.backdropElement && portal.backdropElement.parentElement) {
            try {
                portal.backdropElement.parentElement.removeChild(portal.backdropElement);
                // Release z-index
                if (portal.backdropZIndex) {
                    this.zIndexManager.release('backdrop', portal.backdropZIndex);
                }
                portal.backdropElement = null;
                portal.backdropZIndex = null;
                backdropRemoved = true;
            } catch (e) {
                console.warn(`[PortalManager] Failed to remove backdrop using reference: ${e.message}`);
            }
        }
        
        // Strategy 2: Try to remove by ID (run regardless of Strategy 1 success)
        const backdrop = document.getElementById(`backdrop-${portalId}`);
        if (backdrop && backdrop.parentElement) {
            try {
                backdrop.parentElement.removeChild(backdrop);
                // Release z-index if we can parse it
                const zIndex = parseInt(backdrop.style.zIndex);
                if (!isNaN(zIndex)) {
                    this.zIndexManager.release('backdrop', zIndex);
                }
                backdropRemoved = true;
            } catch (e) {
                console.warn(`[PortalManager] Failed to remove backdrop by ID: ${e.message}`);
            }
        }
        
        // Strategy 3: Remove all backdrops with matching data attribute (always run)
        const orphanedBackdrops = document.querySelectorAll(`[data-portal-id="${portalId}"]`);
        orphanedBackdrops.forEach(backdrop => {
            try {
                if (backdrop.parentElement) {
                    backdrop.parentElement.removeChild(backdrop);
                    backdropRemoved = true;
                }
            } catch (e) {
                console.warn(`[PortalManager] Failed to remove orphaned backdrop: ${e.message}`);
            }
        });
        
        if (!backdropRemoved) {
            console.warn(`[PortalManager] No backdrop found or removed for portal: ${portalId}`);
        }
    }
    
    // Get all portals of a specific type
    getByType(type) {
        return Array.from(this.portals.values()).filter(p => p.type === type);
    }
    
    // Check if element is in a portal
    isInPortal(element) {
        return element?.closest('.rr-portal') !== null;
    }
    
    // Check if there are open portals within a container (e.g., modal)
    hasOpenPortalsInContainer(containerSelector) {
        const container = document.querySelector(containerSelector);
        if (!container) return false;
        
        // Check for any open portals that are children of this container
        for (const [portalId, portal] of this.portals.entries()) {
            if (portal.isOpen && container.contains(portal.container)) {
                return true;
            }
        }
        return false;
    }
    
    // Cleanup method to remove all orphaned backdrops and modal elements
    cleanupOrphanedBackdrops() {
        // Clean up orphaned backdrops
        const allBackdrops = document.querySelectorAll('.rr-portal-backdrop');
        
        allBackdrops.forEach(backdrop => {
            const portalId = backdrop.getAttribute('data-portal-id');
            
            // If no portal ID or portal doesn't exist, remove the backdrop
            if (!portalId || !this.portals.has(portalId)) {
                try {
                    if (backdrop.parentElement) {
                        backdrop.parentElement.removeChild(backdrop);
                    }
                    console.warn(`[PortalManager] Removed orphaned backdrop: ${backdrop.id}`);
                } catch (e) {
                    console.warn(`[PortalManager] Failed to remove orphaned backdrop: ${e.message}`);
                }
            }
        });
        
        // Clean up orphaned portal containers
        const allPortals = document.querySelectorAll('.rr-portal');
        allPortals.forEach(portal => {
            const portalId = portal.id.replace('portal-', '');
            
            if (!this.portals.has(portalId)) {
                try {
                    if (portal.parentElement) {
                        portal.parentElement.removeChild(portal);
                    }
                    // Only log for non-modal-auto portals to reduce noise
                    if (!portal.id.includes('modal-auto-')) {
                        console.warn(`[PortalManager] Removed orphaned portal: ${portal.id}`);
                    }
                } catch (e) {
                    console.warn(`[PortalManager] Failed to remove orphaned portal: ${e.message}`);
                }
            }
        });
        
        // Clean up any visible modal elements that don't belong to active portals
        const orphanedModals = document.querySelectorAll('.modal-content:not([style*="display: none"])');
        orphanedModals.forEach(modal => {
            const portalContainer = modal.closest('.rr-portal');
            if (!portalContainer) {
                // Modal not in a portal container - likely orphaned
                try {
                    if (modal.parentElement) {
                        modal.parentElement.removeChild(modal);
                    }
                    console.warn(`[PortalManager] Removed orphaned modal: ${modal.className}`);
                } catch (e) {
                    console.warn(`[PortalManager] Failed to remove orphaned modal: ${e.message}`);
                }
            }
        });
    }
}

// Z-Index management with CSS variable integration and recycling
class ZIndexManager {
    constructor() {
        this.counters = new Map();
        this.freeIndices = new Map(); // Track free indices for recycling
        this.usedIndices = new Map(); // Track which indices are in use
        this.bases = {
            backdrop: 1000,
            dropdown: 1100,
            tooltip: 1200,
            modal: 1300,
            notification: 1400,
            generic: 1000
        };
        
        // Maximum indices per type before recycling
        this.maxPerType = 100;
        
        // Try to read from CSS variables
        this.loadFromCSS();
    }
    
    loadFromCSS() {
        const root = document.documentElement;
        const getVar = (name, fallback) => {
            const value = getComputedStyle(root).getPropertyValue(name).trim();
            return value ? parseInt(value) : fallback;
        };
        
        this.bases.dropdown = getVar('--z-dropdown', this.bases.dropdown);
        this.bases.tooltip = getVar('--z-tooltip', this.bases.tooltip);
        this.bases.modal = getVar('--z-modal', this.bases.modal);
        this.bases.notification = getVar('--z-notification', this.bases.notification);
    }
    
    getNext(type) {
        const base = this.bases[type] || this.bases.generic;
        
        // Check for free indices to recycle
        if (!this.freeIndices.has(type)) {
            this.freeIndices.set(type, []);
        }
        const freeList = this.freeIndices.get(type);
        
        if (freeList.length > 0) {
            // Recycle a free index
            const recycledIndex = freeList.shift();
            this.markAsUsed(type, recycledIndex);
            return recycledIndex;
        }
        
        // No free indices, generate new one
        const count = this.counters.get(type) || 0;
        
        // Reset counter if we've reached max to prevent overflow
        if (count >= this.maxPerType) {
            this.resetType(type);
            return base;
        }
        
        this.counters.set(type, count + 1);
        const newIndex = base + count;
        this.markAsUsed(type, newIndex);
        return newIndex;
    }
    
    release(type, zIndex) {
        if (!this.freeIndices.has(type)) {
            this.freeIndices.set(type, []);
        }
        
        // Mark index as free for recycling
        const freeList = this.freeIndices.get(type);
        if (!freeList.includes(zIndex)) {
            freeList.push(zIndex);
            // Sort to prefer lower indices
            freeList.sort((a, b) => a - b);
        }
        
        // Remove from used indices
        this.markAsUnused(type, zIndex);
        
        // If no indices are in use, reset the counter
        const usedForType = this.usedIndices.get(type) || new Set();
        if (usedForType.size === 0) {
            this.counters.set(type, 0);
            this.freeIndices.set(type, []);
        }
    }
    
    markAsUsed(type, index) {
        if (!this.usedIndices.has(type)) {
            this.usedIndices.set(type, new Set());
        }
        this.usedIndices.get(type).add(index);
    }
    
    markAsUnused(type, index) {
        if (this.usedIndices.has(type)) {
            this.usedIndices.get(type).delete(index);
        }
    }
    
    resetType(type) {
        this.counters.set(type, 0);
        this.freeIndices.set(type, []);
        this.usedIndices.set(type, new Set());
    }
}

// Positioning engine that consolidates logic from popup-manager
class PositioningEngine {
    calculate(anchor, popup, options = {}) {
        if (!anchor || !popup) return { top: 0, left: 0 };
        
        const anchorRect = anchor.getBoundingClientRect();
        const viewportWidth = window.innerWidth;
        const viewportHeight = window.innerHeight;
        const buffer = options.buffer || 8;
        
        // Get popup dimensions
        let popupWidth = options.width || popup.offsetWidth || 250;
        let popupHeight = options.height || this.getMeasuredHeight(popup) || 300;
        
        // For dropdowns, match trigger width
        if (options.type === 'dropdown' && !options.width) {
            popupWidth = Math.max(anchorRect.width, options.minWidth || 200);
        }
        
        // Calculate available spaces
        const spaces = {
            above: anchorRect.top - buffer,
            below: viewportHeight - anchorRect.bottom - buffer,
            left: anchorRect.left - buffer,
            right: viewportWidth - anchorRect.right - buffer
        };
        
        // Determine optimal vertical position
        const vertical = spaces.below >= popupHeight ? 'below' : 
                        spaces.above >= popupHeight ? 'above' :
                        spaces.above > spaces.below ? 'above' : 'below';
        
        // Determine horizontal position
        const anchorCenter = anchorRect.left + anchorRect.width / 2;
        let horizontal = 'start';
        
        if (anchorCenter - popupWidth / 2 >= buffer && 
            anchorCenter + popupWidth / 2 <= viewportWidth - buffer) {
            horizontal = 'center';
        } else if (anchorRect.right > viewportWidth * 0.7) {
            horizontal = 'end';
        } else if (spaces.right < popupWidth && spaces.left > popupWidth) {
            horizontal = 'end';
        }
        
        // Calculate position
        const top = vertical === 'above' ? 
            Math.max(buffer, anchorRect.top - popupHeight - 4) :
            Math.min(anchorRect.bottom + 4, viewportHeight - popupHeight - buffer);
            
        const left = horizontal === 'center' ? anchorCenter - popupWidth / 2 :
                    horizontal === 'end' ? anchorRect.right - popupWidth :
                    anchorRect.left;
        
        return {
            top: Math.round(Math.max(buffer, Math.min(top, viewportHeight - popupHeight - buffer))),
            left: Math.round(Math.max(buffer, Math.min(left, viewportWidth - popupWidth - buffer))),
            vertical,
            horizontal,
            width: popupWidth,
            height: popupHeight
        };
    }
    
    getMeasuredHeight(popup) {
        let height = popup.offsetHeight || popup.clientHeight;
        
        // Force measurement for hidden elements
        if (!height) {
            const { display, visibility, position } = popup.style;
            popup.style.display = 'block';
            popup.style.visibility = 'hidden';
            popup.style.position = 'absolute';
            
            height = popup.offsetHeight || popup.clientHeight || popup.scrollHeight;
            
            popup.style.display = display;
            popup.style.visibility = visibility;
            popup.style.position = position;
        }
        
        return height;
    }
    
    apply(element, position) {
        if (!element) return;
        
        element.style.position = 'fixed';
        element.style.top = `${position.top}px`;
        element.style.left = `${position.left}px`;
        
        if (position.width) {
            element.style.width = `${position.width}px`;
        }
        
        if (position.maxHeight) {
            element.style.maxHeight = `${position.maxHeight}px`;
        }
    }
    
    getPositionClasses(position) {
        return `portal-${position.vertical} portal-${position.horizontal}`;
    }
}

// Event management
class PortalEventManager {
    constructor() {
        this.handlers = new Map();
        this.globalClickHandler = null;
        this.globalEscapeHandler = null;
        this.setupGlobalHandlers();
    }
    
    setupGlobalHandlers() {
        // Global click handler
        this.globalClickHandler = (event) => {
            this.handlers.forEach((handler, portalId) => {
                if (handler.clickOutside && !handler.isRecentlyCreated) {
                    const portal = handler.portal;
                    if (!portal.container.contains(event.target) && 
                        (!portal.anchor || !portal.anchor.contains(event.target))) {
                        handler.clickOutside(event);
                    }
                }
            });
        };
        
        // Global escape handler
        this.globalEscapeHandler = (event) => {
            if (event.key === 'Escape') {
                // Find topmost portal
                let topPortal = null;
                let topZIndex = -1;
                
                this.handlers.forEach((handler, portalId) => {
                    if (handler.escape && handler.portal.isOpen) {
                        const zIndex = parseInt(handler.portal.container.style.zIndex || '0');
                        if (zIndex > topZIndex) {
                            topZIndex = zIndex;
                            topPortal = handler;
                        }
                    }
                });
                
                if (topPortal) {
                    topPortal.escape(event);
                }
            }
        };
        
        document.addEventListener('click', this.globalClickHandler, true);
        document.addEventListener('keydown', this.globalEscapeHandler, true);
    }
    
    setupClickOutside(portal, callback, dotNetRef, callbackMethod) {
        const handler = this.handlers.get(portal.id) || {};
        handler.portal = portal;
        
        // If dotNetRef is provided, create a callback that invokes the method
        if (dotNetRef && callbackMethod) {
            handler.clickOutside = async () => {
                try {
                    await dotNetRef.invokeMethodAsync(callbackMethod);
                } catch (error) {
                    console.warn(`[PortalEventManager] Failed to invoke ${callbackMethod}:`, error);
                }
            };
        } else {
            handler.clickOutside = callback || (() => {
                // Default: trigger custom event
                const event = new CustomEvent('portalclickoutside', {
                    detail: { portalId: portal.id },
                    bubbles: true
                });
                portal.element.dispatchEvent(event);
            });
        }
        
        handler.isRecentlyCreated = true;
        const protectionDelay = 50;
        setTimeout(() => {
            if (this.handlers.has(portal.id)) {
                handler.isRecentlyCreated = false;
            }
        }, protectionDelay);
        
        this.handlers.set(portal.id, handler);
    }
    
    setupEscape(portal, callback, dotNetRef, callbackMethod) {
        const handler = this.handlers.get(portal.id) || {};
        handler.portal = portal;
        
        // If dotNetRef is provided, create a callback that invokes the method
        if (dotNetRef && callbackMethod) {
            handler.escape = async () => {
                try {
                    await dotNetRef.invokeMethodAsync(callbackMethod);
                } catch (error) {
                    console.warn(`[PortalEventManager] Failed to invoke ${callbackMethod}:`, error);
                }
            };
        } else {
            handler.escape = callback || (() => {
                // Default: trigger custom event
                const event = new CustomEvent('portalescape', {
                    detail: { portalId: portal.id },
                    bubbles: true
                });
                portal.element.dispatchEvent(event);
            });
        }
        this.handlers.set(portal.id, handler);
    }
    
    cleanup(portal) {
        this.handlers.delete(portal.id);
    }
    
    dispose() {
        try {
            if (this.globalClickHandler) {
                document.removeEventListener('click', this.globalClickHandler, true);
            }
            if (this.globalEscapeHandler) {
                document.removeEventListener('keydown', this.globalEscapeHandler, true);
            }
        } finally {
            // Always clear handlers even if removal fails
            this.handlers.clear();
            this.globalClickHandler = null;
            this.globalEscapeHandler = null;
        }
    }
}

// Create singleton instance
const portalManager = new PortalManager();

// Export for ES6 modules
export { portalManager, PortalManager, ZIndexManager, PositioningEngine, PortalEventManager };

// Export functions for simplified API
export function createPortal(element, target, id, className, style, options) {
    return portalManager.create(element, {
        target,
        id,
        className,
        style,
        ...options
    });
}

export function cleanupPortal(portalId) {
    return portalManager.destroy(portalId);
}

export function updatePortal(portalId, options) {
    return portalManager.update(portalId, options);
}

export function positionPortal(portalId, anchor, options) {
    return portalManager.update(portalId, { anchor, ...options });
}

// Window exports for RRBlazor integration
window.RRPortalManager = portalManager;
window.RRPortal = {
    create: (element, options) => portalManager.create(element, options),
    destroy: (portalId) => portalManager.destroy(portalId),
    update: (portalId, options) => portalManager.update(portalId, options),
    position: (portalId) => portalManager.position(portalId),
    isInPortal: (element) => portalManager.isInPortal(element),
    hasOpenPortalsInContainer: (containerSelector) => portalManager.hasOpenPortalsInContainer(containerSelector),
    cleanupOrphanedBackdrops: () => portalManager.cleanupOrphanedBackdrops()
};

// Also initialize RRBlazor.Portal if it doesn't exist (for direct script loading)
if (!window.RRBlazor) {
    window.RRBlazor = {};
}
if (!window.RRBlazor.Portal) {
    window.RRBlazor.Portal = window.RRPortal;
}