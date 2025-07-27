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
            type: 'dropdown',
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
    
    console.log('[datepicker] cleanupDatepicker called');
    
    if (datepickerInstances.has(element)) {
        const portalId = datepickerInstances.get(element);
        console.log('[datepicker] Destroying portal:', portalId);
        
        if (window.RRBlazor?.Portal) {
            window.RRBlazor.Portal.destroy(portalId);
        }
        
        datepickerInstances.delete(element);
        console.log('[datepicker] Portal destroyed and removed from registry');
    } else {
        console.log('[datepicker] No portal found in registry for element');
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

export { positionPopup, cleanupDatepicker, setupDatepickerEvents };