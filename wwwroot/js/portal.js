// Unified portal system for all popup types
const portalCache = new Map();
const bodyTarget = document.body;

// Portal types with their default configurations
const PortalTypes = {
    MODAL: { zIndex: 'var(--z-modal-container, 1010)', positioning: 'viewport', pointerEvents: 'auto' },
    TOOLTIP: { zIndex: 'var(--z-tooltip, 1220)', positioning: 'absolute', pointerEvents: 'none' },
    DROPDOWN: { zIndex: 'var(--z-modal-popup, 1100)', positioning: 'absolute', pointerEvents: 'auto' },
    POPUP: { zIndex: 'var(--z-popover, 1210)', positioning: 'absolute', pointerEvents: 'auto' }
};

export function createPortal(sourceElement, target, portalId, className, style, options = {}) {
    const targetElement = target === 'body' ? bodyTarget : document.querySelector(target);
    if (!targetElement) return false;

    const portalContainer = document.createElement('div');
    const fullPortalId = `portal-${portalId}`;
    const { type = 'MODAL', customZIndex } = options;
    const config = PortalTypes[type] || PortalTypes.MODAL;
    
    portalContainer.id = fullPortalId;
    if (className) portalContainer.className = className;
    
    // Type-specific positioning with unified approach
    const baseStyles = config.positioning === 'viewport' 
        ? 'position: fixed !important; top: 0 !important; left: 0 !important; right: 0 !important; bottom: 0 !important; width: 100vw !important; height: 100vh !important;'
        : 'position: fixed !important;';
        
    portalContainer.style.cssText = `
        ${baseStyles}
        z-index: ${customZIndex || config.zIndex} !important;
        pointer-events: ${config.pointerEvents} !important;
        ${style || ''}
    `;
    
    // Efficient content transfer
    const fragment = document.createDocumentFragment();
    while (sourceElement.firstChild) {
        fragment.appendChild(sourceElement.firstChild);
    }
    portalContainer.appendChild(fragment);
    
    bodyTarget.appendChild(portalContainer);
    portalCache.set(portalId, portalContainer);
    return true;
}

export function cleanupPortal(portalId) {
    // Ultra-fast cleanup using cache
    const portalContainer = portalCache.get(portalId);
    if (portalContainer) {
        portalContainer.remove();
        portalCache.delete(portalId);
        return true;
    }
    return false;
}

export function updatePortal(portalId, className, style) {
    // Ultra-fast update using cache
    const portalContainer = portalCache.get(portalId);
    if (portalContainer) {
        // Batch property updates for single reflow
        if (className !== undefined) portalContainer.className = className;
        if (style !== undefined) portalContainer.style.cssText = style;
        return true;
    }
    return false;
}