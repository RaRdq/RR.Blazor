const datepickerInstances = new Map();

async function positionPopup(element, immediate = false) {
    if (!element) return;
    
    if (!window.RRBlazor?.Portal) {
        throw new Error('Portal system not available');
    }
    
    const popup = element.querySelector('.rr-datepicker-popup');
    const trigger = element.querySelector('.rr-datepicker-trigger');
    
    if (!popup || !trigger) {
        throw new Error('Popup or trigger element not found');
    }
    
    if (!datepickerInstances.has(element)) {
        const portalId = `datepicker-${element.id || Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
        const datepickerId = element.dataset.datepickerId || element.id || portalId;
        
        window.RRBlazor.ClickOutside.register(datepickerId, element, {
            excludeSelectors: ['.rr-datepicker-popup', '.datepicker-portal']
        });
        
        const result = window.RRBlazor.Portal.create(popup, {
            id: portalId,
            type: 'datepicker',
            anchor: trigger,
            className: 'datepicker-portal',
            width: 320,
            height: 400
        });
        
        if (result) {
            datepickerInstances.set(element, { portalId, datepickerId });
        }
    } else {
        const { portalId } = datepickerInstances.get(element);
        window.RRBlazor.Portal.position(portalId);
    }
}

function cleanupDatepicker(element) {
    if (!element) return;
    
    if (datepickerInstances.has(element)) {
        const { portalId, datepickerId } = datepickerInstances.get(element);
        
        window.RRBlazor.ClickOutside.unregister(datepickerId);
        
        if (window.RRBlazor?.Portal) {
            window.RRBlazor.Portal.destroy(portalId);
        }
        
        datepickerInstances.delete(element);
    }
}

function setupDatepickerEvents(element, dotNetRef) {
    if (!element) return;
    
    element.addEventListener('click-outside', function(event) {
        if (dotNetRef) {
            dotNetRef.invokeMethodAsync('HandleClickOutside');
        }
    });
}

async function initializeDatepicker(element, dotNetRef) {
    if (!element || !dotNetRef) return;
    
    setupDatepickerEvents(element, dotNetRef);
    
    if (!element._datepickerInitialized) {
        element._datepickerDotNetRef = dotNetRef;
        element._datepickerInitialized = true;
    }
}

async function openDatepicker(element) {
    if (!element) return;
    
    await positionPopup(element, true);
}

async function closeDatepicker(element) {
    if (!element) return;
    
    cleanupDatepicker(element);
}

function initialize(element, dotNetRef) {
    return initializeDatepicker(element, dotNetRef);
}

function cleanup(element) {
    cleanupDatepicker(element);
}

export { 
    positionPopup, 
    cleanupDatepicker, 
    setupDatepickerEvents, 
    initializeDatepicker, 
    openDatepicker, 
    closeDatepicker,
    initialize,
    cleanup
};