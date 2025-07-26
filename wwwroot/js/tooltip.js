// Tooltip Portal System - Uses unified portal.js
import { createPortal, cleanupPortal, updatePortal } from './portal.js';

const tooltipCache = new Map();

export function createTooltipPortal(tooltipElement, triggerElement, position = 'top') {
    const tooltipId = generateTooltipId();
    const optimalPosition = calculatePosition(triggerElement, tooltipElement, position);
    
    // Use unified portal system with TOOLTIP type
    const success = createPortal(
        tooltipElement,
        'body',
        `tooltip-${tooltipId}`,
        'tooltip-portal',
        getTooltipStyles(triggerElement, tooltipElement, optimalPosition),
        { type: 'TOOLTIP' }
    );
    
    if (success) {
        tooltipCache.set(tooltipId, { triggerElement, tooltipElement, position: optimalPosition });
        updateArrowPosition(tooltipElement, optimalPosition);
    }
    
    return success;
}

export function updateTooltipPosition(tooltipId) {
    const cached = tooltipCache.get(tooltipId);
    if (!cached) return false;
    
    const { triggerElement, tooltipElement } = cached;
    const newPosition = calculatePosition(triggerElement, tooltipElement, cached.position);
    
    return updatePortal(
        `tooltip-${tooltipId}`,
        'tooltip-portal',
        getTooltipStyles(triggerElement, tooltipElement, newPosition)
    );
}

export function destroyTooltipPortal(tooltipId) {
    const success = cleanupPortal(`tooltip-${tooltipId}`);
    if (success) tooltipCache.delete(tooltipId);
    return success;
}

function calculatePosition(trigger, tooltip, preferredPosition) {
    const triggerRect = trigger.getBoundingClientRect();
    const tooltipRect = tooltip.getBoundingClientRect();
    const viewport = {
        width: window.innerWidth,
        height: window.innerHeight
    };
    
    // Calculate available space in each direction
    const spaceTop = triggerRect.top;
    const spaceBottom = viewport.height - triggerRect.bottom;
    const spaceLeft = triggerRect.left;
    const spaceRight = viewport.width - triggerRect.right;
    
    // Check if preferred position fits
    const fits = {
        top: spaceTop >= tooltipRect.height + TOOLTIP_OFFSET + VIEWPORT_PADDING,
        bottom: spaceBottom >= tooltipRect.height + TOOLTIP_OFFSET + VIEWPORT_PADDING,
        left: spaceLeft >= tooltipRect.width + TOOLTIP_OFFSET + VIEWPORT_PADDING,
        right: spaceRight >= tooltipRect.width + TOOLTIP_OFFSET + VIEWPORT_PADDING
    };
    
    // Use preferred position if it fits
    if (fits[preferredPosition]) {
        return preferredPosition;
    }
    
    // Fallback priority based on available space
    const positions = [
        { name: 'bottom', space: spaceBottom },
        { name: 'top', space: spaceTop },
        { name: 'right', space: spaceRight },
        { name: 'left', space: spaceLeft }
    ].sort((a, b) => b.space - a.space);
    
    // Return position with most space that fits
    for (const pos of positions) {
        if (fits[pos.name]) {
            return pos.name;
        }
    }
    
    // Fallback to position with most space
    return positions[0].name;
}

function getTooltipStyles(trigger, tooltip, position) {
    const triggerRect = trigger.getBoundingClientRect();
    const tooltipRect = tooltip.getBoundingClientRect();
    const offset = 8;
    const padding = 16;
    
    let top, left;
    
    switch (position) {
        case 'top':
            top = triggerRect.top - tooltipRect.height - offset;
            left = triggerRect.left + (triggerRect.width - tooltipRect.width) / 2;
            break;
        case 'bottom':
            top = triggerRect.bottom + offset;
            left = triggerRect.left + (triggerRect.width - tooltipRect.width) / 2;
            break;
        case 'left':
            top = triggerRect.top + (triggerRect.height - tooltipRect.height) / 2;
            left = triggerRect.left - tooltipRect.width - offset;
            break;
        case 'right':
            top = triggerRect.top + (triggerRect.height - tooltipRect.height) / 2;
            left = triggerRect.right + offset;
            break;
    }
    
    // Keep tooltip in viewport
    const { innerWidth: vw, innerHeight: vh } = window;
    top = Math.max(padding, Math.min(top, vh - tooltipRect.height - padding));
    left = Math.max(padding, Math.min(left, vw - tooltipRect.width - padding));
    
    return `
        top: ${top}px !important;
        left: ${left}px !important;
        opacity: 1 !important;
        visibility: visible !important;
    `;
}

function updateArrowPosition(tooltip, position) {
    const arrow = tooltip.querySelector('.tooltip-arrow, .tooltip-content::before');
    if (!arrow) return;
    
    // Reset arrow positioning
    arrow.style.cssText = '';
    arrow.classList.remove('tooltip-arrow-top', 'tooltip-arrow-bottom', 'tooltip-arrow-left', 'tooltip-arrow-right');
    arrow.classList.add(`tooltip-arrow-${position}`);
}

function generateTooltipId() {
    return `tooltip-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
}

// Auto-update positions on scroll/resize
let updateTimeout;
function schedulePositionUpdate() {
    if (updateTimeout) clearTimeout(updateTimeout);
    updateTimeout = setTimeout(() => {
        for (const [tooltipId] of tooltipCache) {
            updateTooltipPosition(tooltipId);
        }
    }, 16); // 60fps
}

window.addEventListener('scroll', schedulePositionUpdate, { passive: true });
window.addEventListener('resize', schedulePositionUpdate, { passive: true });

// Cleanup on page unload
window.addEventListener('beforeunload', () => {
    for (const [tooltipId] of tooltipCache) {
        destroyTooltipPortal(tooltipId);
    }
});