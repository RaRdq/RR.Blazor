import dropdownManager from './dropdown-manager.js';

const AUTOSUGGEST_CONFIG = {
    DEFAULT_HEIGHT_PX: 320,
    MAX_VIEWPORT_HEIGHT_RATIO: 0.4,
    DEFAULT_OFFSET_PX: 4,
    CLICK_PRIORITY: 10
};

const LAYOUT_STABILITY_TOLERANCE_PX = 1;
const LAYOUT_STABILIZATION_FRAMES = 45;

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

function validateAutosuggestStructure(autosuggestElement, autosuggestId) {
    validateElement(autosuggestElement, `Autosuggest element ${autosuggestId}`);

    if (!autosuggestElement.hasAttribute('data-autosuggest-id')) {
        throw new Error(`Autosuggest element ${autosuggestId} missing required data-autosuggest-id attribute`);
    }

    const viewport = autosuggestElement.querySelector('.autosuggest-viewport');
    if (!viewport) {
        throw new Error(`Autosuggest element ${autosuggestId} missing required .autosuggest-viewport child`);
    }
    validateElement(viewport, `Autosuggest viewport for ${autosuggestId}`);

    const trigger = autosuggestElement.querySelector('.autosuggest-input, input');
    if (!trigger) {
        throw new Error(`Autosuggest element ${autosuggestId} missing required trigger element`);
    }
    validateElement(trigger, `Autosuggest trigger for ${autosuggestId}`);

    return { viewport, trigger };
}

function nextAnimationFrame() {
    return new Promise(resolve => requestAnimationFrame(() => resolve()));
}

async function waitForTriggerExpansion(trigger) {
    if (!trigger) return;

    let previousWidth = trigger.getBoundingClientRect().width;
    for (let attempt = 0; attempt < LAYOUT_STABILIZATION_FRAMES; attempt++) {
        await nextAnimationFrame();
        const currentWidth = trigger.getBoundingClientRect().width;
        if (Math.abs(currentWidth - previousWidth) <= LAYOUT_STABILITY_TOLERANCE_PX) {
            return;
        }
        previousWidth = currentWidth;
    }
}

class AutosuggestManager {
    constructor() {
        this.setupItemClickHandling();
    }

    initialize() {
        document.addEventListener(window.RRBlazor.Events.UI_COMPONENT_CLOSE_REQUEST, (event) => {
            if (event.detail.componentType === window.RRBlazor.ComponentTypes.AUTOSUGGEST && dropdownManager.isOpen(event.detail.componentId)) {
                this.close(event.detail.componentId);
            }
        });

        document.addEventListener(window.RRBlazor.Events.PARENT_CLOSING, (event) => {
            if (event.detail?.reason === 'modal-closing') {
                this.closeAll();
            }
        });
    }

    setupItemClickHandling() {
        window.RRBlazor.ClickManager.registerDelegate(
            'autosuggest-items',
            '.autosuggest-item, .autosuggest-option',
            (event, item) => this.handleItemClick(event, item),
            {
                priority: AUTOSUGGEST_CONFIG.CLICK_PRIORITY,
                excludeSelectors: ['[data-column-manager]', 'input', 'button'],
                stopPropagation: true
            }
        );
    }

    handleItemClick(event, item) {
        if (!item || item.disabled) return;

        event.preventDefault();
        const autosuggestElement = item.closest('[data-autosuggest-id]');
        if (!autosuggestElement) return;

        const autosuggestId = autosuggestElement.dataset.autosuggestId;
        this.selectItem(autosuggestId, item);

        return true;
    }

    cleanup() {
        window.RRBlazor.ClickManager.unregisterDelegate('autosuggest-items');
    }

    async open(autosuggestId, options = {}) {
        const autosuggestElement = document.querySelector(`[data-autosuggest-id="${autosuggestId}"]`);
        const { viewport, trigger } = validateAutosuggestStructure(autosuggestElement, autosuggestId);

        await waitForTriggerExpansion(trigger);
        const triggerRect = trigger.getBoundingClientRect();
        const dropdownHeight = Math.min(AUTOSUGGEST_CONFIG.DEFAULT_HEIGHT_PX, window.innerHeight * AUTOSUGGEST_CONFIG.MAX_VIEWPORT_HEIGHT_RATIO);

        const {
            position = 'bottom-start',
            offset = AUTOSUGGEST_CONFIG.DEFAULT_OFFSET_PX,
            allowMultiple = false,
            excludeSelectors = [],
            autoCloseOnScroll = true,
            clickOutsideOptions
        } = options;

        const wrapperElement = autosuggestElement.closest('.search-input-wrapper');
        const wrapperRect = wrapperElement ? wrapperElement.getBoundingClientRect() : null;
        const targetWidth = wrapperRect ? Math.max(wrapperRect.width, triggerRect.width) : triggerRect.width;
        const triggerBounds = wrapperRect ?? triggerRect;

        return await dropdownManager.positionDropdown(autosuggestElement, {
            contentSelector: '.autosuggest-viewport',
            triggerSelector: '.autosuggest-input, input',
            componentType: 'autosuggest',
            componentId: autosuggestId,
            dimensions: {
                width: targetWidth,
                height: dropdownHeight
            },
            position,
            offset,
            allowMultiple,
            excludeSelectors: [
                '.autosuggest-viewport',
                '.autosuggest-dropdown',
                `[data-autosuggest-id="${autosuggestId}"]`,
                ...excludeSelectors
            ],
            customTriggerBounds: triggerBounds,
            onOpen: (element, viewport) => {
                viewport.classList.remove('autosuggest-viewport-closed');
                viewport.classList.remove('autosuggest-viewport-animating-close');
                viewport.classList.remove('autosuggest-viewport-animating-open');
                viewport.classList.add('autosuggest-viewport-open');
            },
            onClose: (element, viewport) => {
                viewport.classList.remove('autosuggest-viewport-animating-open');
                viewport.classList.remove('autosuggest-viewport-open');
                viewport.classList.remove('autosuggest-viewport-animating-close');
                viewport.classList.add('autosuggest-viewport-closed');
            },
            autoCloseOnScroll,
            clickOutsideOptions: clickOutsideOptions || {
                callback: () => dropdownManager.closeDropdown(autosuggestId)
            }
        });
    }

    async close(autosuggestId) {
        return await dropdownManager.closeDropdown(autosuggestId);
    }


    async closeAll() {
        const allAutosuggest = document.querySelectorAll('[data-autosuggest-id]');
        const closePromises = [];

        for (const autosuggestElement of allAutosuggest) {
            const autosuggestId = autosuggestElement.getAttribute('data-autosuggest-id');
            if (dropdownManager.isOpen(autosuggestId)) {
                closePromises.push(dropdownManager.closeDropdown(autosuggestId));
            }
        }

        await Promise.all(closePromises);
        return true;
    }

    selectItem(autosuggestId, item) {
        const autosuggestElement = document.querySelector(`[data-autosuggest-id="${autosuggestId}"]`);
        if (!autosuggestElement) return;

        const items = autosuggestElement.querySelectorAll('.autosuggest-item, .autosuggest-option');
        items.forEach(i => i.classList.remove('autosuggest-item-active', 'autosuggest-option-active'));
        item.classList.add(item.classList.contains('autosuggest-item') ? 'autosuggest-item-active' : 'autosuggest-option-active');

        const trigger = autosuggestElement.querySelector('.autosuggest-input, input');
        if (trigger) {
            trigger.value = item.textContent.trim();
        }

        this.close(autosuggestId);
    }
}

const autosuggestManager = new AutosuggestManager();

function normalizeOptions(options = {}) {
    return {
        position: options.placement || options.position || 'bottom-start',
        offset: typeof options.offset === 'number' ? options.offset : AUTOSUGGEST_CONFIG.DEFAULT_OFFSET_PX,
        allowMultiple: options.allowMultiple ?? false,
        excludeSelectors: Array.isArray(options.excludeSelectors) ? options.excludeSelectors : [],
        autoCloseOnScroll: options.autoCloseOnScroll ?? true,
        clickOutsideOptions: options.clickOutsideOptions
    };
}

export async function open(autosuggestId, options = {}) {
    const normalizedOptions = normalizeOptions(options);
    await autosuggestManager.open(autosuggestId, normalizedOptions);
}

export async function createPortal(autosuggestId, options = {}) {
    await open(autosuggestId, options);
}

export async function close(autosuggestId) {
    await autosuggestManager.close(autosuggestId);
}

export async function closeDropdown(autosuggestId) {
    await close(autosuggestId);
}

const autosuggestAPI = {
    open: (autosuggestId, options) => open(autosuggestId, options),
    close: (autosuggestId) => close(autosuggestId),
    createPortal: (autosuggestId, options) => createPortal(autosuggestId, options),
    closeDropdown: (autosuggestId) => closeDropdown(autosuggestId),
    isOpen: (autosuggestId) => dropdownManager.isOpen(autosuggestId),
    closeAll: () => autosuggestManager.closeAll(),
    initialize: () => autosuggestManager.initialize(),
    cleanup: () => autosuggestManager.cleanup()
};

autosuggestManager.initialize();

if (typeof window !== 'undefined') {
    window.RRBlazor = window.RRBlazor || {};
    window.RRBlazor.Autosuggest = {
        ...autosuggestAPI
    };
}

export default autosuggestAPI;
