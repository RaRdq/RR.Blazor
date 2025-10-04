
import dropdownManager from './dropdown-manager.js';

const CHOICE_CONFIG = {
    ITEM_HEIGHT_PX: 40,
    DROPDOWN_PADDING_PX: 16,
    MAX_DROPDOWN_HEIGHT_PX: 480,
    USER_MENU_MIN_WIDTH_PX: 280,
    DEFAULT_MIN_WIDTH_PX: 200,
    CLICK_PRIORITY: 10,
    DEFAULT_OFFSET_PX: 4,
    HIGHLIGHT_INDEX_RESET: -1,
    INDEX_INCREMENT: 1,
    INDEX_DECREMENT: 1,
    MIN_INDEX: 0,
    MIN_HIGHLIGHT_INDEX: 0
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

function validateChoiceStructure(choiceElement, choiceElementId) {
    validateElement(choiceElement, `Choice element ${choiceElementId}`);

    if (!choiceElement.hasAttribute('data-choice-id')) {
        throw new Error(`Choice element ${choiceElementId} missing required data-choice-id attribute`);
    }

    const viewport = choiceElement.querySelector('.choice-viewport');
    if (!viewport) {
        throw new Error(`Choice element ${choiceElementId} missing required .choice-viewport child`);
    }
    validateElement(viewport, `Choice viewport for ${choiceElementId}`);

    return viewport;
}

class ChoiceManager {
    constructor() {
        this.keyboardNavigationEnabled = false;
        this.currentHighlightedIndex = CHOICE_CONFIG.HIGHLIGHT_INDEX_RESET;
        this.setupItemClickHandling();
    }
    initialize() {
        document.addEventListener(window.RRBlazor.Events.UI_COMPONENT_CLOSE_REQUEST, (event) => {
            if (event.detail.componentType === window.RRBlazor.ComponentTypes.CHOICE && dropdownManager.isOpen(event.detail.componentId)) {
                this.close(event.detail.componentId);
            }
        });

        document.addEventListener(window.RRBlazor.Events.PARENT_CLOSING, (event) => {
            if (event.detail?.reason === 'modal-closing') {
                const choiceElements = event.target.querySelectorAll('[data-choice-id]');
                choiceElements.forEach(element => {
                    const choiceId = element.getAttribute('data-choice-id');
                    if (choiceId) {
                        choiceManager.close(choiceId);
                    }
                });
            }
        });

    }

    setupItemClickHandling() {
        window.RRBlazor.ClickManager.registerDelegate(
            'choice-items',
            '.choice-item',
            (event, item) => this.handleItemClick(event, item),
            {
                priority: CHOICE_CONFIG.CLICK_PRIORITY,
                excludeSelectors: ['[data-column-manager]', 'input', 'button', '.choice-trigger-wrapper'],
                stopPropagation: true
            }
        );
    }

    handleItemClick(event, item) {
        if (!item || item.disabled) return;

        event.preventDefault();
        const choiceElement = item.closest('[data-choice-id]');
        if (!choiceElement) return;

        const choiceId = choiceElement.dataset.choiceId;
        this.selectItem(choiceId, item);

        return true;
    }

    cleanup() {
        window.RRBlazor.ClickManager.unregisterDelegate('choice-items');
    }
    
    findViewport(choiceElement, choiceElementId) {
        const viewport = validateChoiceStructure(choiceElement, choiceElementId);
        return viewport;
    }

    async open(choiceElementId, options = {}) {
        const choiceElement = document.querySelector(`[data-choice-id="${choiceElementId}"]`);
        validateChoiceStructure(choiceElement, choiceElementId);

        const trigger = choiceElement.querySelector('.choice-trigger') || choiceElement.querySelector('.choice-trigger-wrapper');
        const viewport = this.findViewport(choiceElement, choiceElementId);

        if (!trigger || !viewport) {
            throw new Error(`Cannot open dropdown - trigger or viewport not found in: ${choiceElementId}`);
        }

        const triggerRect = trigger.getBoundingClientRect();
        const minWidth = Math.max(triggerRect.width, CHOICE_CONFIG.DEFAULT_MIN_WIDTH_PX);

        const targetDimensions = {
            width: minWidth,
            height: CHOICE_CONFIG.MAX_DROPDOWN_HEIGHT_PX
        };

        return await dropdownManager.positionDropdown(choiceElement, {
            contentSelector: '.choice-viewport',
            triggerSelector: '.choice-trigger, .choice-trigger-wrapper',
            componentType: 'choice',
            componentId: choiceElementId,
            dimensions: targetDimensions,
            position: options.direction || 'auto',
            offset: CHOICE_CONFIG.DEFAULT_OFFSET_PX,
            allowMultiple: false,
            customTriggerBounds: triggerRect,
            excludeSelectors: [
                '.choice-trigger',
                '.choice-trigger-wrapper',
                '.choice-viewport',
                '.choice-content',
                `[data-choice-id="${choiceElementId}"]`
            ],
            onOpen: (element, viewport) => {
                if (!element?.classList) {
                    throw new Error(`Choice onOpen: Invalid element passed for ${choiceElementId}`);
                }
                if (!element.hasAttribute('data-choice-id')) {
                    throw new Error(`Choice onOpen: Element missing required data-choice-id attribute for ${choiceElementId}`);
                }
                if (!viewport?.classList) {
                    throw new Error(`Choice onOpen: Invalid viewport passed for ${choiceElementId}`);
                }

                element.classList.add('choice-open');
                viewport.classList.remove('choice-viewport-hidden');
                viewport.classList.add('choice-viewport-positioned');

                if (options.dropdownClass) {
                    const customClasses = options.dropdownClass.split(' ').filter(cls => cls.trim());
                    customClasses.forEach(cls => viewport.classList.add(cls));
                }

                this.enableKeyboardNavigation(choiceElementId);
                this.scrollSelectedIntoView(viewport);

                setTimeout(() => {
                    if (viewport.scrollHeight > viewport.clientHeight) {
                        viewport.classList.add('has-scroll');
                    }
                }, 50);
            },
            onClose: (element, viewport) => {
                element.classList.remove('choice-open');
                viewport.classList.remove('choice-viewport-positioned');
                viewport.classList.remove('has-scroll');
                viewport.classList.add('choice-viewport-hidden');

                if (options.dropdownClass) {
                    const customClasses = options.dropdownClass.split(' ').filter(cls => cls.trim());
                    customClasses.forEach(cls => viewport.classList.remove(cls));
                }

                this.disableKeyboardNavigation();
            },
            clickOutsideOptions: {
                callback: () => this.close(choiceElementId)
            }
        });
    }

    async close(choiceElementId) {
        return await dropdownManager.closeDropdown(choiceElementId);
    }

    getOpenChoiceId() {
        const allChoices = document.querySelectorAll('[data-choice-id]');
        for (const choiceElement of allChoices) {
            const choiceId = choiceElement.getAttribute('data-choice-id');
            if (dropdownManager.isOpen(choiceId)) {
                return choiceId;
            }
        }
        return null;
    }


    enableKeyboardNavigation(choiceElementId) {
        const viewport = dropdownManager.getViewport(choiceElementId);
        if (!viewport) return;

        window.RRBlazor.EventDispatcher.dispatch(
            window.RRBlazor.Events.CHOICE_KEYBOARD_ENABLE,
            { choiceId: choiceElementId, viewport }
        );

        this.keyboardNavigationEnabled = true;
        this.currentHighlightedIndex = -1;
    }

    disableKeyboardNavigation() {
        window.RRBlazor.EventDispatcher.dispatch(
            window.RRBlazor.Events.CHOICE_KEYBOARD_DISABLE
        );
        this.keyboardNavigationEnabled = false;
        this.currentHighlightedIndex = CHOICE_CONFIG.HIGHLIGHT_INDEX_RESET;
    }

    handleKeyDown(event) {
        if (!this.keyboardNavigationEnabled) return;

        const openChoiceId = this.getOpenChoiceId();
        if (!openChoiceId) return;

        const viewport = dropdownManager.getViewport(openChoiceId);
        if (!viewport) return;

        const items = Array.from(viewport.querySelectorAll('.choice-item:not([disabled])'));

        switch (event.key) {
            case 'ArrowDown':
                event.preventDefault();
                this.currentHighlightedIndex = Math.min(this.currentHighlightedIndex + 1, items.length - 1);
                this.highlightItem(items, this.currentHighlightedIndex);
                break;
            case 'ArrowUp':
                event.preventDefault();
                this.currentHighlightedIndex = Math.max(this.currentHighlightedIndex - 1, 0);
                this.highlightItem(items, this.currentHighlightedIndex);
                break;
            case 'Enter':
                event.preventDefault();
                if (this.currentHighlightedIndex >= 0 && items[this.currentHighlightedIndex]) {
                    items[this.currentHighlightedIndex].click();
                }
                break;
            case 'Escape':
                event.preventDefault();
                this.close(openChoiceId);
                break;
        }
    }

    highlightItem(items, index) {
        items.forEach(item => item.classList.remove('choice-item-highlighted'));

        if (index >= 0 && index < items.length) {
            const item = items[index];
            item.classList.add('choice-item-highlighted');
            item.scrollIntoView({ block: 'nearest' });
        }
    }

    scrollSelectedIntoView(viewport) {
        const selectedItem = viewport.querySelector('.choice-item-active');
        if (selectedItem) {
            selectedItem.scrollIntoView({ block: 'nearest' });
        }
    }


    selectItem(choiceElementId, item) {
        const choiceElement = document.querySelector(`[data-choice-id="${choiceElementId}"]`);
        if (!choiceElement) return;

        const items = choiceElement.querySelectorAll('.choice-item');
        items.forEach(i => i.classList.remove('choice-item-active'));
        item.classList.add('choice-item-active');

        const trigger = choiceElement.querySelector('.choice-trigger-wrapper');
        const triggerText = trigger.querySelector('.choice-text');
        if (triggerText) {
            triggerText.textContent = item.textContent.trim();
        }

        this.close(choiceElementId);
    }

    async closeAll() {
        const allChoices = document.querySelectorAll('[data-choice-id]');
        const closePromises = [];

        for (const choiceElement of allChoices) {
            const choiceId = choiceElement.getAttribute('data-choice-id');
            if (dropdownManager.isOpen(choiceId)) {
                closePromises.push(dropdownManager.closeDropdown(choiceId));
            }
        }

        await Promise.all(closePromises);
        return true;
    }
}

const choiceManager = new ChoiceManager();

const choiceAPI = {
    open: (choiceElementId, options) => choiceManager.open(choiceElementId, options),
    close: (choiceElementId) => choiceManager.close(choiceElementId),
    isOpen: (choiceElementId) => dropdownManager.isOpen(choiceElementId),
    closeAll: () => choiceManager.closeAll(),
    initialize: () => choiceManager.initialize(),
    cleanup: () => choiceManager.cleanup(),
    getViewportElement: (choiceElementId) => {
        const choiceElement = document.querySelector(`[data-choice-id="${choiceElementId}"]`);
        if (!choiceElement) return null;
        return choiceManager.findViewport(choiceElement, choiceElementId);
    }
};

choiceManager.initialize();

if (typeof window !== 'undefined') {
    window.RRBlazor = window.RRBlazor || {};
    window.RRBlazor.Choice = choiceAPI;
}

export default choiceAPI;