// Datepicker instances registry
const datepickerInstances = new Map();

async function positionPopup(element, immediate = false) {
    if (!element) return;
    
    // Ensure portal system is available
    if (!window.RRBlazor?.Portal) {
        console.warn('[datepicker] Portal system not available');
        return;
    }
    
    const popup = element.querySelector('.rr-datepicker-popup');
    const trigger = element.querySelector('.rr-datepicker-trigger');
    
    if (!popup || !trigger) {
        console.warn('[datepicker] Popup or trigger element not found');
        return;
    }
    
    if (!datepickerInstances.has(element)) {
        const portalId = `datepicker-${element.id || Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
        
        const result = window.RRBlazor.Portal.create(popup, {
            id: portalId,
            type: 'datepicker',
            anchor: trigger,
            className: 'datepicker-portal',
            width: 320,
            height: 400,
            onClickOutside: () => {
                element.dispatchEvent(new CustomEvent('datePickerClickOutside', {
                    detail: { datepicker: element }
                }));
            }
        });
        
        if (result) {
            datepickerInstances.set(element, portalId);
        }
    } else {
        const portalId = datepickerInstances.get(element);
        window.RRBlazor.Portal.position(portalId);
    }
}

// Clean up datepicker portal
function cleanupDatepicker(element) {
    if (!element) return;
    
    if (datepickerInstances.has(element)) {
        const portalId = datepickerInstances.get(element);
        
        if (window.RRBlazor?.Portal) {
            window.RRBlazor.Portal.destroy(portalId);
        }
        
        datepickerInstances.delete(element);
    }
}

// Setup datepicker events
function setupDatepickerEvents(element, dotNetRef) {
    if (!element) return;
    
    element.addEventListener('datePickerClickOutside', function(event) {
        if (dotNetRef) {
            dotNetRef.invokeMethodAsync('HandleClickOutside');
        }
    });
}

// Initialize datepicker with proper API calls
async function initializeDatepicker(element, dotNetRef) {
    if (!element || !dotNetRef) return;
    
    // Setup event listeners
    setupDatepickerEvents(element, dotNetRef);
    
    // Store reference for cleanup
    if (!element._datepickerInitialized) {
        element._datepickerDotNetRef = dotNetRef;
        element._datepickerInitialized = true;
    }
}

// Open datepicker popup
async function openDatepicker(element) {
    if (!element) return;
    
    await positionPopup(element, true);
}

// Close datepicker popup
async function closeDatepicker(element) {
    if (!element) return;
    
    cleanupDatepicker(element);
}

// Required methods for rr-blazor.js proxy system
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