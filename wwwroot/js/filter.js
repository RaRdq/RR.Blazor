import dropdownManager from './dropdown-manager.js';
import { TIMEOUTS } from './event-constants.js';

const FILTER_CONFIG = {
    ITEM_HEIGHT_PX: 36,
    DROPDOWN_PADDING_PX: 12,
    MAX_DROPDOWN_HEIGHT_PX: 280,
    MIN_WIDTH_PX: 180,
    CLICK_PRIORITY: 10,
    DEFAULT_OFFSET_PX: 2
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

function validateFilterStructure(filterElement, filterElementId) {
    validateElement(filterElement, `Filter element ${filterElementId}`);

    if (!filterElement.hasAttribute('data-filter-id')) {
        throw new Error(`Filter element ${filterElementId} missing required data-filter-id attribute`);
    }

    const viewport = filterElement.querySelector('.filter-viewport');
    if (!viewport) {
        throw new Error(`Filter element ${filterElementId} missing required .filter-viewport child`);
    }
    validateElement(viewport, `Filter viewport for ${filterElementId}`);

    return viewport;
}

class FilterManager {
    constructor() {
        this.setupItemClickHandling();
    }

    initialize() {
        document.addEventListener(window.RRBlazor.Events.UI_COMPONENT_CLOSE_REQUEST, (event) => {
            if (event.detail.componentType === window.RRBlazor.ComponentTypes.FILTER && dropdownManager.isOpen(event.detail.componentId)) {
                this.close(event.detail.componentId);
            }
        });

        document.addEventListener(window.RRBlazor.Events.PARENT_CLOSING, (event) => {
            if (event.detail?.reason === 'modal-closing') {
                const filterElements = event.target.querySelectorAll('[data-filter-id]');
                filterElements.forEach(element => {
                    const filterId = element.getAttribute('data-filter-id');
                    if (filterId) {
                        this.close(filterId);
                    }
                });
            }
        });
    }

    setupItemClickHandling() {
        window.RRBlazor.ClickManager.registerDelegate(
            'filter-items',
            '.filter-item, .filter-option',
            (event, item) => this.handleItemClick(event, item),
            {
                priority: FILTER_CONFIG.CLICK_PRIORITY,
                excludeSelectors: ['[data-column-manager]', 'input', 'button'],
                stopPropagation: true
            }
        );
    }

    handleItemClick(event, item) {
        if (!item || item.disabled) return;

        event.preventDefault();
        const filterElement = item.closest('[data-filter-id]');
        if (!filterElement) return;

        const filterId = filterElement.dataset.filterId;
        this.selectItem(filterId, item);

        return true;
    }

    cleanup() {
        window.RRBlazor.ClickManager.unregisterDelegate('filter-items');
    }

    async open(filterElementId, options = {}) {
        const filterElement = document.querySelector(`[data-filter-id="${filterElementId}"]`);
        const viewport = validateFilterStructure(filterElement, filterElementId);

        const trigger = filterElement.querySelector('.filter-trigger, .filter-button, .filter-input');
        if (!trigger) {
            throw new Error(`Cannot open filter dropdown - trigger element not found in: ${filterElementId}`);
        }

        const triggerRect = trigger.getBoundingClientRect();
        const itemCount = viewport.querySelectorAll('.filter-item, .filter-option').length;
        const estimatedHeight = Math.min(itemCount * FILTER_CONFIG.ITEM_HEIGHT_PX + FILTER_CONFIG.DROPDOWN_PADDING_PX, FILTER_CONFIG.MAX_DROPDOWN_HEIGHT_PX);

        const targetDimensions = {
            width: Math.max(triggerRect.width, FILTER_CONFIG.MIN_WIDTH_PX),
            height: estimatedHeight
        };

        return await dropdownManager.positionDropdown(filterElement, {
            contentSelector: '.filter-viewport',
            triggerSelector: '.filter-trigger, .filter-button, .filter-input',
            componentType: 'filter',
            componentId: filterElementId,
            dimensions: targetDimensions,
            position: options.position || 'auto',
            offset: options.offset || FILTER_CONFIG.DEFAULT_OFFSET_PX,
            allowMultiple: options.allowMultiple || false,
            excludeSelectors: [
                '.filter-viewport',
                '.filter-content',
                `[data-filter-id="${filterElementId}"]`
            ],
            onOpen: (element, viewport) => {
                viewport.classList.remove('filter-viewport-hidden');
                viewport.classList.add('filter-viewport-positioned');
                this.applyFilterStyles(viewport, targetDimensions);
                this.scrollSelectedIntoView(viewport);
            },
            onClose: (element, viewport) => {
                viewport.classList.remove('filter-viewport-positioned');
                viewport.classList.add('filter-viewport-hidden');
            },
            clickOutsideOptions: {
                callback: () => dropdownManager.closeDropdown(filterElementId)
            }
        });
    }

    async close(filterElementId) {
        return await dropdownManager.closeDropdown(filterElementId);
    }


    applyFilterStyles(viewport, dimensions) {
        viewport.style.minWidth = `${dimensions.width}px`;
        viewport.style.maxHeight = `${dimensions.height}px`;
    }

    scrollSelectedIntoView(viewport) {
        const selectedItem = viewport.querySelector('.filter-item-active, .filter-option-active');
        if (selectedItem) {
            selectedItem.scrollIntoView({ block: 'nearest' });
        }
    }

    selectItem(filterElementId, item) {
        const filterElement = document.querySelector(`[data-filter-id="${filterElementId}"]`);
        if (!filterElement) return;

        const items = filterElement.querySelectorAll('.filter-item, .filter-option');
        items.forEach(i => i.classList.remove('filter-item-active', 'filter-option-active'));
        item.classList.add(item.classList.contains('filter-item') ? 'filter-item-active' : 'filter-option-active');

        const trigger = filterElement.querySelector('.filter-trigger, .filter-button');
        const triggerText = trigger?.querySelector('.filter-text, .filter-label');
        if (triggerText) {
            triggerText.textContent = item.textContent.trim();
        }

        this.close(filterElementId);
    }

    async closeAll() {
        const allFilters = document.querySelectorAll('[data-filter-id]');
        const closePromises = [];

        for (const filterElement of allFilters) {
            const filterId = filterElement.getAttribute('data-filter-id');
            if (dropdownManager.isOpen(filterId)) {
                closePromises.push(dropdownManager.closeDropdown(filterId));
            }
        }

        await Promise.all(closePromises);
        return true;
    }
}

const filterManager = new FilterManager();

const filterAPI = {
    open: (filterId, options) => filterManager.open(filterId, options),
    close: (filterId) => filterManager.close(filterId),
    isOpen: (filterId) => dropdownManager.isOpen(filterId),
    closeAll: () => filterManager.closeAll(),
    initialize: () => filterManager.initialize(),
    cleanup: () => filterManager.cleanup()
};

filterManager.initialize();

if (typeof window !== 'undefined') {
    window.RRBlazor = window.RRBlazor || {};
    window.RRBlazor.Filter = filterAPI;
}

export default filterAPI;