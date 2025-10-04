import dropdownManager from './dropdown-manager.js';
import { TIMEOUTS } from './event-constants.js';


const DATEPICKER_CONFIG = {
    DEFAULT_WIDTH_PX: 320,
    DEFAULT_HEIGHT_PX: 380,
    MIN_HEIGHT_PX: 280,
    DEFAULT_OFFSET_PX: 4,
    CLICK_PRIORITY: 10
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

function validateDatepickerStructure(datepickerElement, datepickerId) {
    validateElement(datepickerElement, `Datepicker element ${datepickerId}`);

    if (!datepickerElement.hasAttribute('data-datepicker-id')) {
        throw new Error(`Datepicker element ${datepickerId} missing required data-datepicker-id attribute`);
    }

    const popup = datepickerElement.querySelector('.rr-datepicker-popup');
    if (!popup) {
        throw new Error(`Datepicker element ${datepickerId} missing required .rr-datepicker-popup child`);
    }
    validateElement(popup, `Datepicker popup for ${datepickerId}`);

    const trigger = datepickerElement.querySelector('.rr-datepicker-trigger');
    if (!trigger) {
        throw new Error(`Datepicker element ${datepickerId} missing required .rr-datepicker-trigger child`);
    }
    validateElement(trigger, `Datepicker trigger for ${datepickerId}`);

    return { popup, trigger };
}

class DatepickerManager {
    constructor() {
    }

    initialize() {
        document.addEventListener(window.RRBlazor.Events.UI_COMPONENT_CLOSE_REQUEST, (event) => {
            if (event.detail.componentType === window.RRBlazor.ComponentTypes.DATEPICKER && dropdownManager.isOpen(event.detail.componentId)) {
                this.close(event.detail.componentId);
            }
        });

        document.addEventListener(window.RRBlazor.Events.PARENT_CLOSING, (event) => {
            if (event.detail?.reason === 'modal-closing') {
                const datepickerElements = event.target.querySelectorAll('[data-datepicker-id]');
                datepickerElements.forEach(element => {
                    const datepickerId = element.getAttribute('data-datepicker-id');
                    if (datepickerId) {
                        this.close(datepickerId);
                    }
                });
            }
        });
    }

    cleanup() {
    }

    async open(datepickerId, options = {}) {
        const datepickerElement = document.querySelector(`[data-datepicker-id="${datepickerId}"]`);
        const { popup, trigger } = validateDatepickerStructure(datepickerElement, datepickerId);

        const triggerRect = trigger.getBoundingClientRect();
        const minWidth = Math.max(triggerRect.width, DATEPICKER_CONFIG.DEFAULT_WIDTH_PX);
        const targetDimensions = {
            width: options.width || minWidth,
            height: options.height || DATEPICKER_CONFIG.DEFAULT_HEIGHT_PX
        };

        return await dropdownManager.positionDropdown(datepickerElement, {
            contentSelector: '.rr-datepicker-popup',
            triggerSelector: '.rr-datepicker-trigger',
            componentType: 'datepicker',
            componentId: datepickerId,
            dimensions: targetDimensions,
            position: options.direction || 'auto',
            offset: DATEPICKER_CONFIG.DEFAULT_OFFSET_PX,
            minHeight: DATEPICKER_CONFIG.MIN_HEIGHT_PX,
            allowMultiple: false,
            excludeSelectors: [
                '.rr-datepicker-trigger',
                '.rr-datepicker-popup',
                `[data-datepicker-id="${datepickerId}"]`
            ],
            onOpen: (element, popup) => {
                popup.classList.remove('rr-datepicker-hidden');
                popup.classList.add('rr-datepicker-positioned');
                popup.classList.add('rr-datepicker-open');
            },
            onClose: (element, popup) => {
                popup.classList.remove('rr-datepicker-positioned');
                popup.classList.add('rr-datepicker-hidden');
                popup.classList.remove('rr-datepicker-open');
            },
            clickOutsideOptions: {
                callback: () => dropdownManager.closeDropdown(datepickerId)
            }
        });
    }

    async close(datepickerId) {
        return await dropdownManager.closeDropdown(datepickerId);
    }

    isOpen(datepickerId) {
        return dropdownManager.isOpen(datepickerId);
    }


    setupDatepickerEvents(element, dotNetRef) {
        element.addEventListener(window.RRBlazor.Events.CLICK_OUTSIDE, function(event) {
            if (dotNetRef) {
                dotNetRef.invokeMethodAsync('HandleClickOutside');
            }
        });

        if (!element._datepickerInitialized) {
            element._datepickerDotNetRef = dotNetRef;
            element._datepickerInitialized = true;
        }
    }

    async closeAll() {
        return await dropdownManager.closeAll();
    }
}

const datepickerManager = new DatepickerManager();

function initialize(element, dotNetRef) {
    return datepickerManager.setupDatepickerEvents(element, dotNetRef);
}

function open(element, config = {}) {
    if (!element) {
        return false;
    }

    const datepickerId = element.dataset.datepickerId || element.id;
    if (!datepickerId) {
        return false;
    }

    return datepickerManager.open(datepickerId, config);
}

function close(element) {
    if (!element) {
        return false;
    }

    const datepickerId = element.dataset.datepickerId || element.id;
    if (!datepickerId) {
        return false;
    }

    return datepickerManager.close(datepickerId);
}

const datepickerAPI = {
    open: (datepickerId, options) => datepickerManager.open(datepickerId, options),
    close: (datepickerId) => datepickerManager.close(datepickerId),
    isOpen: (datepickerId) => dropdownManager.isOpen(datepickerId),
    closeAll: () => datepickerManager.closeAll(),
    initialize: () => datepickerManager.initialize(),
    cleanup: () => datepickerManager.cleanup(),
    openElement: open,
    closeElement: close,
    initializeElement: initialize
};

datepickerManager.initialize();

if (typeof window !== 'undefined') {
    window.RRBlazor = window.RRBlazor || {};
    window.RRBlazor.DatePicker = datepickerAPI;
}

export { initialize, open, close };
export default datepickerAPI;