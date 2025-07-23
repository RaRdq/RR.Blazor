// RDatePicker JavaScript Helper
// Provides advanced calendar functionality and keyboard navigation

const RDatePicker = {
    // Initialize date picker functionality
    init: function (elementId, options = {}) {
        const element = document.getElementById(elementId);
        if (!element) return null;

        const instance = {
            element: element,
            options: {
                autoClose: true,
                keyboard: true,
                outsideClick: true,
                ...options
            },
            isOpen: false,
            focusedDate: null
        };

        this.setupKeyboardNavigation(instance);
        this.setupOutsideClickDetection(instance);
        this.setupTouchSupport(instance);

        return instance;
    },

    // Setup keyboard navigation for calendar
    setupKeyboardNavigation: function (instance) {
        if (!instance.options.keyboard) return;

        instance.element.addEventListener('keydown', (e) => {
            if (!instance.isOpen) return;

            const calendar = instance.element.querySelector('.datepicker-calendar');
            if (!calendar) return;

            const days = calendar.querySelectorAll('.datepicker-day:not([disabled])');
            const currentFocused = calendar.querySelector('.datepicker-day-focused');
            let currentIndex = currentFocused ? Array.from(days).indexOf(currentFocused) : -1;

            switch (e.key) {
                case 'ArrowLeft':
                    e.preventDefault();
                    if (currentIndex > 0) {
                        this.focusDay(days[currentIndex - 1], instance);
                    }
                    break;

                case 'ArrowRight':
                    e.preventDefault();
                    if (currentIndex < days.length - 1) {
                        this.focusDay(days[currentIndex + 1], instance);
                    }
                    break;

                case 'ArrowUp':
                    e.preventDefault();
                    if (currentIndex >= 7) {
                        this.focusDay(days[currentIndex - 7], instance);
                    }
                    break;

                case 'ArrowDown':
                    e.preventDefault();
                    if (currentIndex + 7 < days.length) {
                        this.focusDay(days[currentIndex + 7], instance);
                    }
                    break;

                case 'Home':
                    e.preventDefault();
                    const firstWeekDay = Math.floor(currentIndex / 7) * 7;
                    this.focusDay(days[firstWeekDay], instance);
                    break;

                case 'End':
                    e.preventDefault();
                    const lastWeekDay = Math.min(Math.floor(currentIndex / 7) * 7 + 6, days.length - 1);
                    this.focusDay(days[lastWeekDay], instance);
                    break;

                case 'PageUp':
                    e.preventDefault();
                    this.navigateMonth(instance, -1);
                    break;

                case 'PageDown':
                    e.preventDefault();
                    this.navigateMonth(instance, 1);
                    break;

                case 'Enter':
                case ' ':
                    e.preventDefault();
                    if (currentFocused) {
                        currentFocused.click();
                    }
                    break;

                case 'Escape':
                    e.preventDefault();
                    this.close(instance);
                    break;
            }
        });
    },

    // Setup outside click detection
    setupOutsideClickDetection: function (instance) {
        if (!instance.options.outsideClick) return;

        document.addEventListener('click', (e) => {
            if (!instance.isOpen) return;

            const popup = instance.element.querySelector('.datepicker-popup');
            const trigger = instance.element.querySelector('.datepicker-trigger');

            if (popup && !popup.contains(e.target) && 
                trigger && !trigger.contains(e.target)) {
                this.close(instance);
            }
        });
    },

    // Setup touch support for mobile devices
    setupTouchSupport: function (instance) {
        const popup = instance.element.querySelector('.datepicker-popup');
        if (!popup) return;

        let startY = 0;
        let startX = 0;

        popup.addEventListener('touchstart', (e) => {
            startY = e.touches[0].clientY;
            startX = e.touches[0].clientX;
        }, { passive: true });

        popup.addEventListener('touchmove', (e) => {
            const currentY = e.touches[0].clientY;
            const currentX = e.touches[0].clientX;
            const deltaY = Math.abs(currentY - startY);
            const deltaX = Math.abs(currentX - startX);

            // Prevent scrolling if moving within calendar
            if (deltaY < 50 && deltaX < 50) {
                e.preventDefault();
            }
        });
    },

    // Focus a specific day in calendar
    focusDay: function (dayElement, instance) {
        if (!dayElement) return;

        // Remove previous focus
        const previousFocused = instance.element.querySelector('.datepicker-day-focused');
        if (previousFocused) {
            previousFocused.classList.remove('datepicker-day-focused');
        }

        // Add focus to new day
        dayElement.classList.add('datepicker-day-focused');
        dayElement.focus();
        
        instance.focusedDate = dayElement.textContent;
    },

    // Navigate to different month
    navigateMonth: function (instance, direction) {
        const navButton = direction > 0 
            ? instance.element.querySelector('.datepicker-nav-btn:last-child')
            : instance.element.querySelector('.datepicker-nav-btn:first-child');
        
        if (navButton) {
            navButton.click();
        }
    },

    // Open calendar popup
    open: function (instance) {
        instance.isOpen = true;
        
        // Add open class to trigger CSS transitions
        instance.element.classList.add('datepicker-open');
        
        const popup = instance.element.querySelector('.datepicker__popup, .datepicker-popup');
        if (popup) {
            this.positionPopup(instance);
            
            // Focus first available day after positioning
            requestAnimationFrame(() => {
                const firstDay = popup.querySelector('.datepicker-day:not([disabled])');
                if (firstDay) {
                    this.focusDay(firstDay, instance);
                }
            });
        }
    },

    // Close calendar popup
    close: function (instance) {
        instance.isOpen = false;
        
        // Remove open class to trigger CSS transitions
        instance.element.classList.remove('datepicker-open');
        
        // CRITICAL FIX: Restore modal overflow when datepicker closes
        const trigger = instance.element.querySelector('.datepicker-trigger') || 
                       instance.element.querySelector('.datepicker-trigger input');
        if (trigger) {
            const modalElement = trigger.closest('.modal-content, .modal-body, .modal');
            if (modalElement) {
                // Restore original overflow settings
                modalElement.style.overflow = '';
                const modalBody = modalElement.closest('.modal-body') || modalElement.querySelector('.modal-body');
                if (modalBody) {
                    modalBody.style.overflow = '';
                }
            }
            
            // Focus the input if it exists
            const input = instance.element.querySelector('.datepicker-trigger input');
            if (input) {
                input.focus();
            }
        }
    },

    // Calculate optimal popup positioning (adapted from RChoice)
    calculateOptimalPosition: function (triggerElement, options = {}) {
        try {
            if (!triggerElement) return { direction: 'down', position: 'start' };
            
            const triggerRect = triggerElement.getBoundingClientRect();
            const viewportWidth = window.innerWidth;
            const viewportHeight = window.innerHeight;
            const popupHeight = options.estimatedHeight || 400;
            const popupWidth = options.estimatedWidth || Math.max(triggerRect.width, 320);
            const buffer = options.buffer || 20;
            
            const spaces = {
                above: triggerRect.top - buffer,
                below: viewportHeight - triggerRect.bottom - buffer,
                left: triggerRect.left - buffer,
                right: viewportWidth - triggerRect.right - buffer
            };
            
            let verticalDirection = 'down';
            if (spaces.below < popupHeight && spaces.above > popupHeight) {
                verticalDirection = 'up';
            } else if (spaces.below < popupHeight && spaces.above < popupHeight) {
                verticalDirection = spaces.above > spaces.below ? 'up' : 'down';
            }
            
            let horizontalPosition = 'start';
            const triggerCenter = triggerRect.left + (triggerRect.width / 2);
            const popupHalfWidth = popupWidth / 2;
            
            if (triggerCenter - popupHalfWidth < buffer) {
                horizontalPosition = 'start';
            } else if (triggerCenter + popupHalfWidth > viewportWidth - buffer) {
                horizontalPosition = 'end';
            } else {
                horizontalPosition = 'center';
            }
            
            const isNearLeftEdge = triggerRect.left < viewportWidth * 0.25;
            const isNearRightEdge = triggerRect.right > viewportWidth * 0.75;
            const isNearTopEdge = triggerRect.top < viewportHeight * 0.25;
            const isNearBottomEdge = triggerRect.bottom > viewportHeight * 0.75;
            
            if (isNearLeftEdge && isNearBottomEdge) {
                verticalDirection = 'up';
                horizontalPosition = 'start';
            }
            
            return {
                direction: verticalDirection,
                position: horizontalPosition,
                spaces: spaces,
                debug: {
                    triggerRect,
                    popupSize: { width: popupWidth, height: popupHeight },
                    isNearEdges: { left: isNearLeftEdge, right: isNearRightEdge, top: isNearTopEdge, bottom: isNearBottomEdge }
                }
            };
        } catch (error) {
            console.warn('[RR.Blazor] RDatePicker.calculateOptimalPosition error:', error);
            return { direction: 'down', position: 'start' };
        }
    },

    // RChoice's proven positionDropdown function with viewport clamping
    positionDropdown: function(triggerElement, options = {}) {
        try {
            if (!triggerElement) return { left: 0, top: 0, width: 0 };
            
            const triggerRect = triggerElement.getBoundingClientRect();
            const viewportWidth = window.innerWidth;
            const viewportHeight = window.innerHeight;
            const dropdownHeight = options.estimatedHeight || 300;
            const dropdownWidth = options.estimatedWidth || Math.max(triggerRect.width, 200);
            const buffer = options.buffer || 20;
            
            const optimal = this.calculateOptimalPosition(triggerElement, options);
            
            let top, left;
            
            if (optimal.direction === 'up') {
                top = triggerRect.top - dropdownHeight - 4;
            } else {
                top = triggerRect.bottom + 4;
            }
            
            switch (optimal.position) {
                case 'start':
                    left = triggerRect.left;
                    break;
                case 'end':
                    left = triggerRect.right - dropdownWidth;
                    break;
                case 'center':
                default:
                    left = triggerRect.left + (triggerRect.width - dropdownWidth) / 2;
                    break;
            }
            
            // CRITICAL: RChoice's exact viewport clamping that works perfectly
            top = Math.max(buffer, Math.min(top, viewportHeight - dropdownHeight - buffer));
            left = Math.max(buffer, Math.min(left, viewportWidth - dropdownWidth - buffer));
            
            return {
                left: Math.round(left),
                top: Math.round(top),
                width: Math.round(Math.max(triggerRect.width, dropdownWidth)),
                direction: optimal.direction,
                position: optimal.position,
                optimal: optimal
            };
        } catch (error) {
            console.warn('[RR.Blazor] RDatePicker.positionDropdown error:', error);
            return { left: 0, top: 0, width: 0, direction: 'down', position: 'center' };
        }
    },

    // Simplified popup positioning using proven RChoice approach
    positionPopup: function (instance) {
        // Handle both instance object and direct element reference
        let element;
        if (instance && instance.element) {
            element = instance.element;
        } else if (instance && instance.querySelector) {
            element = instance;
        } else {
            return;
        }
        
        const popup = element.querySelector('.datepicker-popup');
        const trigger = element.querySelector('.datepicker-trigger');
        
        console.log('RDatePicker.positionPopup - Debug:', {
            element: element,
            popup: popup,
            trigger: trigger,
            popupClasses: popup ? popup.className : 'null',
            triggerClasses: trigger ? trigger.className : 'null'
        });
        
        if (!popup || !trigger) return;

        // Calculate available space for dynamic height
        const triggerRect = trigger.getBoundingClientRect();
        const viewportHeight = window.innerHeight;
        const buffer = 20;
        
        // CRITICAL FIX: Check if we're inside a modal and adjust accordingly
        const modalElement = trigger.closest('.modal-content, .modal-body, .modal');
        const isInModal = modalElement !== null;
        
        // Calculate maximum available height based on position
        const spaceBelow = viewportHeight - triggerRect.bottom - buffer;
        const spaceAbove = triggerRect.top - buffer;
        
        // If in modal, use viewport space rather than modal constraints
        const maxHeight = isInModal 
            ? Math.max(spaceBelow, spaceAbove, 300) // Larger minimum in modal
            : Math.max(spaceBelow, spaceAbove, 200); // Standard minimum
        
        // Use dynamic height - prefer full calendar height in modal
        const dynamicHeight = isInModal 
            ? Math.min(400, Math.max(350, maxHeight)) // Prefer larger calendar in modal
            : Math.min(400, maxHeight);

        // Use RChoice's proven positioning approach with dynamic height
        const positioning = this.positionDropdown(trigger, {
            estimatedHeight: dynamicHeight,
            estimatedWidth: 320,
            buffer: buffer
        });

        // Apply positioning classes
        this.applyDynamicPositioning(element, positioning.optimal);

        // Apply calculated positioning directly - let CSS handle z-index completely
        popup.style.position = 'fixed';
        popup.style.top = `${positioning.top}px`;
        popup.style.left = `${positioning.left}px`;
        popup.style.width = `${positioning.width}px`;
        
        // CRITICAL FIX: Use calculated dynamic height instead of fixed 400px
        popup.style.maxHeight = `${dynamicHeight}px`;
        popup.style.height = 'auto';
        popup.style.overflowY = dynamicHeight < 400 ? 'auto' : 'visible';
        
        popup.style.visibility = 'visible';
        popup.style.opacity = '1';
        popup.style.pointerEvents = 'auto';
        
        // Enhanced z-index for modal context
        popup.style.zIndex = isInModal ? '10000' : '9999';
        
        // Clear conflicting styles
        popup.style.bottom = 'auto';
        popup.style.right = 'auto';
        popup.style.transform = '';
        
        // CRITICAL FIX: Ensure modal allows overflow when datepicker is open
        if (isInModal && modalElement) {
            modalElement.style.overflow = 'visible';
            // Also check for modal-body parent
            const modalBody = modalElement.closest('.modal-body') || modalElement.querySelector('.modal-body');
            if (modalBody) {
                modalBody.style.overflow = 'visible';
            }
        }
        
        console.log('RDatePicker positioning applied:', {
            dynamicHeight: dynamicHeight,
            spaceBelow: spaceBelow,
            spaceAbove: spaceAbove,
            positioning: positioning
        });
    },

    // Position date picker using RChoice's proven logic
    positionDatePicker: function(triggerElement, options = {}) {
        try {
            if (!triggerElement) return { left: 0, top: 0, width: 0 };
            
            const triggerRect = triggerElement.getBoundingClientRect();
            const viewportWidth = window.innerWidth;
            const viewportHeight = window.innerHeight;
            const popupHeight = options.estimatedHeight || 400;
            const popupWidth = options.estimatedWidth || Math.max(triggerRect.width, 320);
            const buffer = options.buffer || 20;
            
            const optimal = this.calculateOptimalPosition(triggerElement, options);
            
            let top, left;
            
            if (optimal.direction === 'up') {
                top = triggerRect.top - popupHeight - 4;
            } else {
                top = triggerRect.bottom + 4;
            }
            
            switch (optimal.position) {
                case 'start':
                    left = triggerRect.left;
                    break;
                case 'end':
                    left = triggerRect.right - popupWidth;
                    break;
                case 'center':
                default:
                    left = triggerRect.left + (triggerRect.width - popupWidth) / 2;
                    break;
            }
            
            // Critical: Use RChoice's proven viewport clamping
            top = Math.max(buffer, Math.min(top, viewportHeight - popupHeight - buffer));
            left = Math.max(buffer, Math.min(left, viewportWidth - popupWidth - buffer));
            
            return {
                left: Math.round(left),
                top: Math.round(top),
                width: Math.round(Math.max(triggerRect.width, popupWidth)),
                direction: optimal.direction,
                position: optimal.position,
                optimal: optimal
            };
        } catch (error) {
            console.warn('[RR.Blazor] RDatePicker.positionDatePicker error:', error);
            return { left: 0, top: 0, width: 320, direction: 'down', position: 'center' };
        }
    },

    // Apply dynamic positioning classes (adapted from RChoice)
    applyDynamicPositioning: function (datePickerElement, positioning) {
        try {
            if (!datePickerElement) return;
            
            // Remove all positioning classes
            datePickerElement.classList.remove(
                'datepicker-top', 'datepicker-bottom', 
                'datepicker-left', 'datepicker-right',
                'datepicker-topstart', 'datepicker-topend',
                'datepicker-bottomstart', 'datepicker-bottomend',
                'datepicker-center'
            );
            
            // Add appropriate positioning class
            const directionClass = positioning.position === 'center' 
                ? `datepicker-${positioning.direction}` 
                : `datepicker-${positioning.direction}${positioning.position}`;
            
            datePickerElement.classList.add(directionClass);
            
            // Add center class for centering transforms
            if (positioning.position === 'center') {
                datePickerElement.classList.add('datepicker-center');
            }
            
            if (window.debugLogger && window.debugLogger.isDebugMode) {
                window.debugLogger.log('RDatePicker positioning:', {
                    element: datePickerElement,
                    positioning: positioning,
                    appliedClass: directionClass
                });
            }
            
            return positioning;
        } catch (error) {
            console.warn('[RR.Blazor] RDatePicker.applyDynamicPositioning error:', error);
        }
    },

    // Format date for display
    formatDate: function (date, format = 'dd/MM/yyyy') {
        if (!date) return '';

        const d = new Date(date);
        if (isNaN(d.getTime())) return '';

        const formatMap = {
            'dd': d.getDate().toString().padStart(2, '0'),
            'd': d.getDate().toString(),
            'MM': (d.getMonth() + 1).toString().padStart(2, '0'),
            'M': (d.getMonth() + 1).toString(),
            'yyyy': d.getFullYear().toString(),
            'yy': d.getFullYear().toString().slice(-2),
            'HH': d.getHours().toString().padStart(2, '0'),
            'H': d.getHours().toString(),
            'hh': (d.getHours() % 12 || 12).toString().padStart(2, '0'),
            'h': (d.getHours() % 12 || 12).toString(),
            'mm': d.getMinutes().toString().padStart(2, '0'),
            'm': d.getMinutes().toString(),
            'ss': d.getSeconds().toString().padStart(2, '0'),
            's': d.getSeconds().toString(),
            'tt': d.getHours() >= 12 ? 'PM' : 'AM',
            't': d.getHours() >= 12 ? 'P' : 'A'
        };

        return format.replace(/dd|d|MM|M|yyyy|yy|HH|H|hh|h|mm|m|ss|s|tt|t/g, match => formatMap[match] || match);
    },

    // Parse date from string
    parseDate: function (dateString, format = 'dd/MM/yyyy') {
        if (!dateString) return null;

        try {
            // Simple parsing for common formats
            if (format === 'dd/MM/yyyy') {
                const parts = dateString.split('/');
                if (parts.length === 3) {
                    const day = parseInt(parts[0], 10);
                    const month = parseInt(parts[1], 10) - 1; // Months are 0-based
                    const year = parseInt(parts[2], 10);
                    return new Date(year, month, day);
                }
            }

            // Fallback to native parsing
            return new Date(dateString);
        } catch {
            return null;
        }
    },

    // Validate date range
    isValidRange: function (startDate, endDate) {
        if (!startDate || !endDate) return true;
        return new Date(startDate) <= new Date(endDate);
    },

    // Check if date is in disabled list
    isDateDisabled: function (date, disabledDates = [], disabledDaysOfWeek = []) {
        const d = new Date(date);
        
        // Check disabled days of week
        if (disabledDaysOfWeek.includes(d.getDay())) {
            return true;
        }

        // Check disabled specific dates
        const dateString = this.formatDate(d, 'yyyy-MM-dd');
        return disabledDates.some(disabled => {
            const disabledString = this.formatDate(new Date(disabled), 'yyyy-MM-dd');
            return dateString === disabledString;
        });
    },

    // Get week number
    getWeekNumber: function (date) {
        const d = new Date(date);
        d.setHours(0, 0, 0, 0);
        d.setDate(d.getDate() + 3 - (d.getDay() + 6) % 7);
        const week1 = new Date(d.getFullYear(), 0, 4);
        return 1 + Math.round(((d.getTime() - week1.getTime()) / 86400000 - 3 + (week1.getDay() + 6) % 7) / 7);
    },

    // Cleanup instance
    destroy: function (instance) {
        if (!instance) return;
        
        // Remove event listeners
        document.removeEventListener('click', instance.outsideClickHandler);
        instance.element.removeEventListener('keydown', instance.keyboardHandler);
        
        // Clear references
        instance.element = null;
        instance.options = null;
    }
};

// Auto-initialize date pickers with data attributes
document.addEventListener('DOMContentLoaded', function() {
    const datePickers = document.querySelectorAll('[data-datepicker]');
    datePickers.forEach(element => {
        const options = JSON.parse(element.dataset.datepicker || '{}');
        window.RDatePicker.init(element.id, options);
    });
});

// Expose globally for compatibility
window.RDatePicker = RDatePicker;

// ES6 module exports
export const init = RDatePicker.init.bind(RDatePicker);
export const open = RDatePicker.open.bind(RDatePicker);
export const close = RDatePicker.close.bind(RDatePicker);
export const formatDate = RDatePicker.formatDate.bind(RDatePicker);
export const parseDate = RDatePicker.parseDate.bind(RDatePicker);
export const destroy = RDatePicker.destroy.bind(RDatePicker);
export const positionPopup = RDatePicker.positionPopup.bind(RDatePicker);

// Export the entire object as default
export default RDatePicker;