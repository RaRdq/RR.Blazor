

import dropdownManager from './dropdown-manager.js';
import { TIMEOUTS } from './event-constants.js';

const TOOLTIP_CONFIG = {
    DEFAULT_WIDTH_PX: 200,
    DEFAULT_HEIGHT_PX: 40,
    DEFAULT_OFFSET_PX: 8
};

function validateElement(element, elementName) {
    if (!element) {
        throw new Error(`${elementName} is null or undefined`);
    }
    if (!(element instanceof Element)) {
        throw new Error(`${elementName} is not a DOM Element`);
    }
    if (!element.parentNode) {
        throw new Error(`${elementName} is not attached to the DOM`);
    }
    return true;
}

function validateTooltipStructure(tooltipElement, tooltipElementId) {
    validateElement(tooltipElement, `Tooltip element ${tooltipElementId}`);

    if (!tooltipElement.hasAttribute('data-tooltip-id')) {
        throw new Error(`Tooltip element ${tooltipElementId} missing required data-tooltip-id attribute`);
    }

    const content = tooltipElement.querySelector('.tooltip-content');
    if (!content) {
        throw new Error(`Tooltip element ${tooltipElementId} missing required .tooltip-content child`);
    }
    validateElement(content, `Tooltip content for ${tooltipElementId}`);

    return content;
}

const Tooltip = {
    dimensionsCache: new Map(),

    initialize() {
        document.addEventListener(window.RRBlazor.Events.UI_COMPONENT_CLOSE_REQUEST, (event) => {
            if (event.detail.componentType === window.RRBlazor.ComponentTypes.TOOLTIP) {
                this.hideTooltip(event.detail.componentId);
            }
        });

        document.addEventListener(window.RRBlazor.Events.PARENT_CLOSING, (event) => {
            if (event.detail?.reason === 'modal-closing') {
                const tooltipElements = event.target.querySelectorAll('[data-tooltip-id]');
                tooltipElements.forEach(element => {
                    const tooltipId = element.getAttribute('data-tooltip-id');
                    if (tooltipId) {
                        this.hideTooltip(tooltipId);
                    }
                });
            }
        });
    },

    async showTooltip(tooltipElementId, options = {}) {
        const tooltipElement = document.querySelector(`[data-tooltip-id="${tooltipElementId}"]`);
        const content = validateTooltipStructure(tooltipElement, tooltipElementId);

        const position = options.position || 'top';

        let targetDimensions = this.dimensionsCache.get(tooltipElementId);

        if (!targetDimensions) {
            const wasVisible = content.classList.contains('tooltip-visible');
            const originalDisplay = content.style.display;
            const originalVisibility = content.style.visibility;
            const originalPosition = content.style.position;

            content.style.visibility = 'hidden';
            content.style.position = 'absolute';
            content.style.display = 'block';
            content.classList.remove('tooltip-hidden');
            content.classList.add('tooltip-visible');

            targetDimensions = {
                width: content.offsetWidth || content.scrollWidth || TOOLTIP_CONFIG.DEFAULT_WIDTH_PX,
                height: content.offsetHeight || content.scrollHeight || TOOLTIP_CONFIG.DEFAULT_HEIGHT_PX
            };

            if (!wasVisible) {
                content.classList.remove('tooltip-visible');
                content.classList.add('tooltip-hidden');
            }
            content.style.display = originalDisplay;
            content.style.visibility = originalVisibility;
            content.style.position = originalPosition;

            this.dimensionsCache.set(tooltipElementId, targetDimensions);
        }

        return await dropdownManager.positionDropdown(tooltipElement, {
            contentSelector: '.tooltip-content',
            triggerSelector: '.tooltip-trigger',
            componentType: 'tooltip',
            componentId: tooltipElementId,
            dimensions: targetDimensions,
            position: position,
            offset: TOOLTIP_CONFIG.DEFAULT_OFFSET_PX,
            allowMultiple: false,
            excludeSelectors: [
                '.tooltip-trigger',
                '.tooltip-content',
                `[data-tooltip-id="${tooltipElementId}"]`
            ],
            onOpen: (element, content) => {
                this._updateArrowPosition(content, position);
                content.classList.remove('tooltip-hidden');
                content.classList.add('tooltip-visible');
            },
            onClose: (element, content) => {
                content.classList.remove('tooltip-visible');
                content.classList.add('tooltip-hidden');
            },
            clickOutsideOptions: {
                disabled: true
            },
            autoCloseOnScroll: false
        });
    },

    _updateArrowPosition(content, position) {
        content.classList.remove('tooltip-top', 'tooltip-bottom', 'tooltip-left', 'tooltip-right');
        content.classList.add(`tooltip-${position}`);
    },

    async hideTooltip(tooltipElementId) {
        if (!tooltipElementId) return false;

        const isOpen = dropdownManager.isOpen(tooltipElementId);
        if (!isOpen) return false;

        return await dropdownManager.closeDropdown(tooltipElementId);
    }
};

Tooltip.initialize();

export async function createTooltipPortal(popupElement, triggerElement, position, portalId, config = {}) {
    // Business requirement: Blazor must pass valid element references
    validateElement(popupElement, 'Tooltip popup element');
    validateElement(triggerElement, 'Tooltip trigger element');

    // Business requirement: Tooltip DOM element must exist before JS interaction
    const tooltipElement = document.querySelector(`[data-tooltip-id="${portalId}"]`);
    validateTooltipStructure(tooltipElement, portalId);

    return await Tooltip.showTooltip(portalId, { position, ...config });
}

export async function destroyTooltipPortal(portalId) {
    Tooltip.dimensionsCache.delete(portalId);
    return await Tooltip.hideTooltip(portalId);
}

export async function updateTooltipPosition(portalId, triggerElement, position) {
    if (dropdownManager.isOpen(portalId)) {
        await Tooltip.hideTooltip(portalId);
        return await Tooltip.showTooltip(portalId, {
            position: position || 'top'
        });
    }
    return false;
}


export function cleanup(element) {
    if (element && element.hasAttribute('data-tooltip-id')) {
        const tooltipId = element.getAttribute('data-tooltip-id');
        Tooltip.dimensionsCache.delete(tooltipId);
        Tooltip.hideTooltip(tooltipId);
    }
}

window.RRBlazor = window.RRBlazor || {};
window.RRBlazor.Tooltip = {
    create: createTooltipPortal,
    destroy: destroyTooltipPortal,
    update: updateTooltipPosition,
    initialize: Tooltip.initialize.bind(Tooltip),
    cleanup,
    showTooltip: Tooltip.showTooltip.bind(Tooltip),
    hideTooltip: Tooltip.hideTooltip.bind(Tooltip)
};

export default {
    create: createTooltipPortal,
    destroy: destroyTooltipPortal,
    update: updateTooltipPosition,
    initialize: Tooltip.initialize.bind(Tooltip),
    cleanup
};