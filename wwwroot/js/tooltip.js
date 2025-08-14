
export async function createTooltipPortal(popupElement, triggerElement, position, portalId) {
    if (!popupElement) throw new Error('Popup element required');
    if (!triggerElement) throw new Error('Trigger element required');
        
        const portalManager = await window.RRBlazor.Portal.getInstance();
        const portalResult = portalManager.create({
            id: portalId,
            className: 'tooltip-portal'
        });
        
        const portalContainer = portalResult.element;
        
        if (!popupElement._originalParent) {
            popupElement._originalParent = popupElement.parentNode;
            popupElement._originalNextSibling = popupElement.nextSibling;
        }
        
        portalContainer.appendChild(popupElement);
        const triggerRect = triggerElement.getBoundingClientRect();
        const finalPosition = getTooltipPosition(triggerElement, popupElement, position || 'top');
        
        let x, y;
        const buffer = 8;
        const tooltipWidth = popupElement.offsetWidth || 200;
        const tooltipHeight = popupElement.offsetHeight || 100;
        
        switch (finalPosition) {
            case 'top':
                x = triggerRect.left + (triggerRect.width / 2) - (tooltipWidth / 2);
                y = triggerRect.top - tooltipHeight - buffer;
                break;
            case 'bottom':
                x = triggerRect.left + (triggerRect.width / 2) - (tooltipWidth / 2);
                y = triggerRect.bottom + buffer;
                break;
            case 'left':
                x = triggerRect.left - tooltipWidth - buffer;
                y = triggerRect.top + (triggerRect.height / 2) - (tooltipHeight / 2);
                break;
            case 'right':
                x = triggerRect.right + buffer;
                y = triggerRect.top + (triggerRect.height / 2) - (tooltipHeight / 2);
                break;
            default:
                x = triggerRect.left;
                y = triggerRect.top - tooltipHeight - buffer;
        }
        
        x = Math.max(8, Math.min(x, window.innerWidth - tooltipWidth - 8));
        y = Math.max(8, Math.min(y, window.innerHeight - tooltipHeight - 8));
        portalContainer.style.position = 'fixed';
        portalContainer.style.left = `${x}px`;
        portalContainer.style.top = `${y}px`;
        portalContainer.style.zIndex = portalResult.zIndex;
        portalContainer.style.visibility = 'visible';
        portalContainer.style.opacity = '1';
        portalContainer.style.pointerEvents = 'auto';
        
        return portalResult.id;
}

export async function destroyTooltipPortal(portalId) {
    const portalManager = await window.RRBlazor.Portal.getInstance();
    if (portalManager.isPortalActive(portalId)) {
        const portal = portalManager.getPortal(portalId);
        
        const tooltipElement = portal.element.querySelector('.tooltip, [role="tooltip"]');
        if (tooltipElement && tooltipElement._originalParent) {
            if (tooltipElement._originalNextSibling) {
                tooltipElement._originalParent.insertBefore(tooltipElement, tooltipElement._originalNextSibling);
            } else {
                tooltipElement._originalParent.appendChild(tooltipElement);
            }
            delete tooltipElement._originalParent;
            delete tooltipElement._originalNextSibling;
        }
        
        portalManager.destroy(portalId);
    }
    return true;
}

export async function updateTooltipPosition(portalId, triggerElement, position) {
    const portalManager = await window.RRBlazor.Portal.getInstance();
    if (!portalManager.isPortalActive(portalId)) return true;
    
    const portal = portalManager.getPortal(portalId);
    const portalContainer = portal.element;
    const tooltipElement = portalContainer.querySelector('.tooltip, [role="tooltip"]');
    
    if (!tooltipElement || !triggerElement) return true;
    
    const triggerRect = triggerElement.getBoundingClientRect();
    const finalPosition = getTooltipPosition(triggerElement, tooltipElement, position || 'top');
    
    let x, y;
    const buffer = 8;
    const tooltipWidth = tooltipElement.offsetWidth || 200;
    const tooltipHeight = tooltipElement.offsetHeight || 100;
    
    switch (finalPosition) {
        case 'top':
            x = triggerRect.left + (triggerRect.width / 2) - (tooltipWidth / 2);
            y = triggerRect.top - tooltipHeight - buffer;
            break;
        case 'bottom':
            x = triggerRect.left + (triggerRect.width / 2) - (tooltipWidth / 2);
            y = triggerRect.bottom + buffer;
            break;
        case 'left':
            x = triggerRect.left - tooltipWidth - buffer;
            y = triggerRect.top + (triggerRect.height / 2) - (tooltipHeight / 2);
            break;
        case 'right':
            x = triggerRect.right + buffer;
            y = triggerRect.top + (triggerRect.height / 2) - (tooltipHeight / 2);
            break;
        default:
            x = triggerRect.left;
            y = triggerRect.top - tooltipHeight - buffer;
    }
    
    x = Math.max(8, Math.min(x, window.innerWidth - tooltipWidth - 8));
    y = Math.max(8, Math.min(y, window.innerHeight - tooltipHeight - 8));
    
    portalContainer.style.left = `${x}px`;
    portalContainer.style.top = `${y}px`;
    return true;
}

export function getTooltipPosition(triggerElement, tooltipElement, preferredPosition = 'top') {
    if (!triggerElement) throw new Error('Trigger element required');
    if (!tooltipElement) throw new Error('Tooltip element required');
    
    const triggerRect = triggerElement.getBoundingClientRect();
    const tooltipWidth = tooltipElement.offsetWidth || 200;
    const tooltipHeight = tooltipElement.offsetHeight || 100;
    const buffer = 8;
    
    const viewportWidth = window.innerWidth;
    const viewportHeight = window.innerHeight;
    
    const space = {
        top: triggerRect.top - buffer,
        bottom: viewportHeight - triggerRect.bottom - buffer,
        left: triggerRect.left - buffer,
        right: viewportWidth - triggerRect.right - buffer
    };
    
    let finalPosition = preferredPosition;
    
    switch (preferredPosition) {
        case 'top':
            if (space.top < tooltipHeight) finalPosition = 'bottom';
            break;
        case 'bottom':
            if (space.bottom < tooltipHeight) finalPosition = 'top';
            break;
        case 'left':
            if (space.left < tooltipWidth) finalPosition = 'right';
            break;
        case 'right':
            if (space.right < tooltipWidth) finalPosition = 'left';
            break;
    }
    
    return finalPosition;
}

export function initialize(element, dotNetRef) {
    return true;
}

export function cleanup(element) {
    if (element && element.hasAttribute('data-tooltip-id')) {
        const tooltipId = element.getAttribute('data-tooltip-id');
        destroyTooltipPortal(tooltipId);
    }
}

export default {
    create: createTooltipPortal,
    destroy: destroyTooltipPortal,
    update: updateTooltipPosition,
    getPosition: getTooltipPosition,
    initialize,
    cleanup
};