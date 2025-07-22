// Ultra-lightweight portal system - optimized for performance
const portalCache = new Map();
const bodyTarget = document.body;

export function createPortal(sourceElement, target, portalId, className, style) {
    // Fast path for body target (most common case)
    const targetElement = target === 'body' ? bodyTarget : document.querySelector(target);
    if (!targetElement) return false;

    // Create portal container with minimal DOM operations
    const portalContainer = document.createElement('div');
    const fullPortalId = `portal-${portalId}`;
    
    // CRITICAL: Force full viewport coverage for modals
    portalContainer.id = fullPortalId;
    if (className) portalContainer.className = className;
    
    // Force viewport positioning - OVERRIDE any container constraints
    portalContainer.style.cssText = `
        position: fixed !important;
        top: 0 !important;
        left: 0 !important;
        right: 0 !important;
        bottom: 0 !important;
        width: 100vw !important;
        height: 100vh !important;
        z-index: var(--z-modal-container, 1010) !important;
        pointer-events: auto !important;
        ${style || ''}
    `;
    
    // Efficient content transfer using DocumentFragment for single reflow
    const fragment = document.createDocumentFragment();
    while (sourceElement.firstChild) {
        fragment.appendChild(sourceElement.firstChild);
    }
    portalContainer.appendChild(fragment);
    
    // Single DOM insertion at body level - BYPASS ALL CONTAINERS
    bodyTarget.appendChild(portalContainer);
    
    // Cache for fast cleanup
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