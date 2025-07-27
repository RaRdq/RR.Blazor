// RR.Blazor Tooltip Module
// Handles tooltip-specific behavior using the unified portal system

export async function createTooltipPortal(popupElement, triggerElement, position, portalId) {
    try {
        if (!popupElement || !triggerElement) {
            console.warn('[Tooltip] Required elements not found');
            return false;
        }
        
        const portal = await window.RRBlazor.Portal.create(popupElement, {
            id: portalId,
            type: 'tooltip',
            anchor: triggerElement,
            className: 'tooltip-portal',
            closeOnClickOutside: false,
            closeOnEscape: false,
            width: popupElement.offsetWidth || 200,
            height: popupElement.offsetHeight || 100
        });
        
        return portal || portalId;
    } catch (error) {
        console.error('[Tooltip] Portal creation failed:', error);
        return false;
    }
}

export async function destroyTooltipPortal(portalId) {
    try {
        return await window.RRBlazor.Portal.destroy(portalId);
    } catch (error) {
        console.error('[Tooltip] Portal cleanup failed:', error);
        return false;
    }
}

export async function updateTooltipPosition(portalId, triggerElement, position) {
    try {
        return await window.RRBlazor.Portal.update(portalId, {
            anchor: triggerElement
        });
    } catch (error) {
        console.error('[Tooltip] Position update failed:', error);
        return false;
    }
}

export function getTooltipPosition(triggerElement, tooltipElement, preferredPosition = 'top') {
    if (!triggerElement || !tooltipElement) return null;
    
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

export default {
    create: createTooltipPortal,
    destroy: destroyTooltipPortal,
    update: updateTooltipPosition,
    getPosition: getTooltipPosition
};