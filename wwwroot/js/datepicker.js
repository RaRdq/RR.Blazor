// Global click-outside tracking for datepickers
let activeDatepickers = new Set();
let clickOutsideHandler = null;

// Smart datepicker positioning with Choice-level sophistication
function calculateOptimalPosition(triggerElement, options = {}) {
    try {
        if (!triggerElement) return { direction: 'down', position: 'start' };
        
        const triggerRect = triggerElement.getBoundingClientRect();
        const viewportWidth = window.innerWidth;
        const viewportHeight = window.innerHeight;
        const popupHeight = options.estimatedHeight || 400;
        const popupWidth = options.estimatedWidth || 320;
        const buffer = options.buffer || 12;
        
        const spaces = {
            above: triggerRect.top - buffer,
            below: viewportHeight - triggerRect.bottom - buffer,
            left: triggerRect.left - buffer,
            right: viewportWidth - triggerRect.right - buffer
        };
        
        // Determine vertical direction
        let verticalDirection = 'down';
        if (spaces.below < popupHeight && spaces.above > popupHeight) {
            verticalDirection = 'up';
        } else if (spaces.below < popupHeight && spaces.above < popupHeight) {
            // Choose direction with more space if both are insufficient
            verticalDirection = spaces.above > spaces.below ? 'up' : 'down';
        }
        
        // Determine horizontal position
        let horizontalPosition = 'start';
        if (triggerRect.left + popupWidth > viewportWidth - buffer) {
            horizontalPosition = 'end';
        }
        
        return {
            direction: verticalDirection,
            position: horizontalPosition,
            spaces: spaces
        };
    } catch (error) {
        console.warn('[RR.Blazor] DatePicker.calculateOptimalPosition error:', error);
        return { direction: 'down', position: 'start' };
    }
}

function getHighestZIndex() {
    // Find the highest z-index on the page to ensure datepicker appears above everything
    let highest = 1000;
    const elements = document.querySelectorAll('*');
    for (let el of elements) {
        const zIndex = parseInt(window.getComputedStyle(el).zIndex);
        if (zIndex && zIndex > highest) {
            highest = zIndex;
        }
    }
    // Add buffer for modal popups
    return Math.max(highest + 10, 1200);
}

// Enhanced datepicker positioning with modal support and smart positioning
function positionPopup(element, immediate = false) {
    if (!element) return;
    
    const popup = element.querySelector('.rr-datepicker-popup');
    const trigger = element.querySelector('.rr-datepicker-trigger');
    
    if (!popup || !trigger) {
        return;
    }
    
    // Register this datepicker for click-outside detection
    activeDatepickers.add(element);
    setupClickOutsideHandler();
    
    // Check if we're inside a modal or positioned container
    const modal = element.closest('.modal, .rr-modal, [role="dialog"]');
    const positionedContainer = element.closest('[style*="position: relative"], [style*="position: absolute"], [style*="position: fixed"]');
    const isInModal = modal !== null;
    const isInPositionedContainer = positionedContainer !== null;
    
    const triggerRect = trigger.getBoundingClientRect();
    const popupWidth = 320;
    const popupHeight = 400;
    const buffer = 12;
    
    // Calculate optimal positioning
    const optimal = calculateOptimalPosition(trigger, {
        estimatedHeight: popupHeight,
        estimatedWidth: popupWidth,
        buffer: buffer
    });
    
    let top, left;
    
    if (isInPositionedContainer && !isInModal) {
        // For positioned containers (but not modals), use absolute positioning
        const containerRect = positionedContainer.getBoundingClientRect();
        const triggerRelativeToContainer = {
            top: triggerRect.top - containerRect.top,
            bottom: triggerRect.bottom - containerRect.top,
            left: triggerRect.left - containerRect.left,
            right: triggerRect.right - containerRect.left
        };
        
        if (optimal.direction === 'up') {
            top = triggerRelativeToContainer.top - popupHeight - buffer;
        } else {
            top = triggerRelativeToContainer.bottom + buffer;
        }
        
        if (optimal.position === 'end') {
            left = triggerRelativeToContainer.right - popupWidth;
        } else {
            left = triggerRelativeToContainer.left;
        }
        
        // Ensure popup stays within container bounds
        top = Math.max(buffer, Math.min(top, positionedContainer.clientHeight - popupHeight - buffer));
        left = Math.max(buffer, Math.min(left, positionedContainer.clientWidth - popupWidth - buffer));
        
        popup.style.position = 'absolute';
        popup.style.zIndex = getHighestZIndex() + '';
    } else {
        // For modals or viewport positioning, use fixed positioning
        if (optimal.direction === 'up') {
            top = triggerRect.top - popupHeight - buffer;
        } else {
            top = triggerRect.bottom + buffer;
        }
        
        if (optimal.position === 'end') {
            left = triggerRect.right - popupWidth;
        } else {
            left = triggerRect.left;
        }
        
        // Ensure popup stays within viewport bounds
        top = Math.max(buffer, Math.min(top, window.innerHeight - popupHeight - buffer));
        left = Math.max(buffer, Math.min(left, window.innerWidth - popupWidth - buffer));
        
        popup.style.position = 'fixed';
        popup.style.zIndex = getHighestZIndex() + '';
    }
    
    // Apply positioning
    popup.style.top = `${Math.round(top)}px`;
    popup.style.left = `${Math.round(left)}px`;
    popup.style.width = `${popupWidth}px`;
    popup.style.maxHeight = `${popupHeight}px`;
    
    // Add direction classes for CSS styling
    element.classList.remove('rr-datepicker-up', 'rr-datepicker-down', 'rr-datepicker-start', 'rr-datepicker-end');
    element.classList.add(`rr-datepicker-${optimal.direction}`);
    element.classList.add(`rr-datepicker-${optimal.position}`);
}

// Setup click-outside handler for all active datepickers
function setupClickOutsideHandler() {
    if (clickOutsideHandler) return; // Already setup
    
    clickOutsideHandler = function(event) {
        if (activeDatepickers.size === 0) return;
        
        // Check if click is outside any active datepicker
        for (let datepicker of activeDatepickers) {
            const popup = datepicker.querySelector('.rr-datepicker-popup');
            const trigger = datepicker.querySelector('.rr-datepicker-trigger');
            
            if (!popup || !trigger) continue;
            
            // If click is inside the datepicker (popup or trigger), don't close
            if (datepicker.contains(event.target)) {
                continue;
            }
            
            // Click is outside this datepicker - trigger close
            const closeEvent = new CustomEvent('datePickerClickOutside', {
                detail: { datepicker: datepicker }
            });
            datepicker.dispatchEvent(closeEvent);
        }
    };
    
    // Add the handler with capture to get it before other handlers
    document.addEventListener('click', clickOutsideHandler, true);
}

// Remove click-outside handler when no datepickers are active
function cleanupClickOutsideHandler() {
    if (activeDatepickers.size === 0 && clickOutsideHandler) {
        document.removeEventListener('click', clickOutsideHandler, true);
        clickOutsideHandler = null;
    }
}

// Cleanup function for when datepicker is closed
function cleanupDatepicker(element) {
    if (element) {
        activeDatepickers.delete(element);
        cleanupClickOutsideHandler();
    }
}

// Setup click-outside event listener for a specific datepicker
function setupDatepickerEvents(element, dotNetRef) {
    if (!element) return;
    
    // Add custom event listener for click outside
    element.addEventListener('datePickerClickOutside', function(event) {
        if (dotNetRef) {
            dotNetRef.invokeMethodAsync('HandleClickOutside');
        }
    });
    
    // Register for global click-outside detection
    activeDatepickers.add(element);
    setupClickOutsideHandler();
}

export { positionPopup, cleanupDatepicker, setupDatepickerEvents };