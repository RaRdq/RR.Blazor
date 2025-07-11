// RDatePicker JavaScript Helper
// Provides advanced calendar functionality and keyboard navigation

window.RDatePicker = {
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

            const calendar = instance.element.querySelector('.rdatepicker__calendar');
            if (!calendar) return;

            const days = calendar.querySelectorAll('.rdatepicker__day:not([disabled])');
            const currentFocused = calendar.querySelector('.rdatepicker__day--focused');
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

            const popup = instance.element.querySelector('.rdatepicker__popup');
            const trigger = instance.element.querySelector('.rdatepicker__trigger');

            if (popup && !popup.contains(e.target) && 
                trigger && !trigger.contains(e.target)) {
                this.close(instance);
            }
        });
    },

    // Setup touch support for mobile devices
    setupTouchSupport: function (instance) {
        const popup = instance.element.querySelector('.rdatepicker__popup');
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
        const previousFocused = instance.element.querySelector('.rdatepicker__day--focused');
        if (previousFocused) {
            previousFocused.classList.remove('rdatepicker__day--focused');
        }

        // Add focus to new day
        dayElement.classList.add('rdatepicker__day--focused');
        dayElement.focus();
        
        instance.focusedDate = dayElement.textContent;
    },

    // Navigate to different month
    navigateMonth: function (instance, direction) {
        const navButton = direction > 0 
            ? instance.element.querySelector('.rdatepicker__nav-btn:last-child')
            : instance.element.querySelector('.rdatepicker__nav-btn:first-child');
        
        if (navButton) {
            navButton.click();
        }
    },

    // Open calendar popup
    open: function (instance) {
        instance.isOpen = true;
        const popup = instance.element.querySelector('.rdatepicker__popup');
        if (popup) {
            this.positionPopup(instance);
            
            // Focus first available day
            setTimeout(() => {
                const firstDay = popup.querySelector('.rdatepicker__day:not([disabled])');
                if (firstDay) {
                    this.focusDay(firstDay, instance);
                }
            }, 100);
        }
    },

    // Close calendar popup
    close: function (instance) {
        instance.isOpen = false;
        const trigger = instance.element.querySelector('.rdatepicker__trigger input');
        if (trigger) {
            trigger.focus();
        }
    },

    // Smart popup positioning
    positionPopup: function (instance) {
        const popup = instance.element.querySelector('.rdatepicker__popup');
        const trigger = instance.element.querySelector('.rdatepicker__trigger');
        
        if (!popup || !trigger) return;

        const triggerRect = trigger.getBoundingClientRect();
        const popupRect = popup.getBoundingClientRect();
        const viewportHeight = window.innerHeight;
        const viewportWidth = window.innerWidth;

        // Reset positioning
        popup.style.top = '';
        popup.style.bottom = '';
        popup.style.left = '';
        popup.style.right = '';

        // Vertical positioning
        const spaceBelow = viewportHeight - triggerRect.bottom;
        const spaceAbove = triggerRect.top;

        if (spaceBelow >= popupRect.height + 10) {
            // Position below
            popup.style.top = '100%';
            popup.style.marginTop = '0.5rem';
        } else if (spaceAbove >= popupRect.height + 10) {
            // Position above
            popup.style.bottom = '100%';
            popup.style.marginBottom = '0.5rem';
        } else {
            // Position below with max height
            popup.style.top = '100%';
            popup.style.marginTop = '0.5rem';
            popup.style.maxHeight = `${spaceBelow - 20}px`;
            popup.style.overflowY = 'auto';
        }

        // Horizontal positioning
        const spaceRight = viewportWidth - triggerRect.left;
        const spaceLeft = triggerRect.right;

        if (spaceRight >= popupRect.width) {
            popup.style.left = '0';
        } else if (spaceLeft >= popupRect.width) {
            popup.style.right = '0';
        } else {
            // Center horizontally if no space on either side
            popup.style.left = '50%';
            popup.style.transform = 'translateX(-50%)';
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
    const datePickers = document.querySelectorAll('[data-rdatepicker]');
    datePickers.forEach(element => {
        const options = JSON.parse(element.dataset.rdatepicker || '{}');
        window.RDatePicker.init(element.id, options);
    });
});

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = window.RDatePicker;
}