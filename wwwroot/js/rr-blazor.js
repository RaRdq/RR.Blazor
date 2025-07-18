// RR.Blazor JavaScript Helpers
// Supporting functions for component interactions

// Unified Debug Logger - Reused across all RR.Blazor JavaScript files
class DebugLogger {
    constructor(prefix = '[RR.Blazor]') {
        this.isDebugMode = this.detectDebugMode();
        this.logPrefix = prefix;
    }

    detectDebugMode() {
        return (
            window.location.hostname === 'localhost' ||
            window.location.hostname === '127.0.0.1' ||
            window.location.hostname.includes('localhost') ||
            window.location.port === '5001' ||
            window.location.port === '7049' ||
            window.location.href.includes('localhost') ||
            window.location.href.includes('development') ||
            window.location.href.includes('stage') ||
            new URLSearchParams(window.location.search).has('debug') ||
            localStorage.getItem('debug') === 'true' ||
            document.body.classList.contains('debug-mode') ||
            window.location.search.includes('debug') ||
            !(window.location.hostname === 'payrollai.co' || 
              window.location.hostname === 'www.payrollai.co' ||
              window.location.hostname.endsWith('.payrollai.co'))
        );
    }

    log(...args) {
        if (this.isDebugMode) {
            console.log(this.logPrefix, ...args);
        }
    }

    info(...args) {
        if (this.isDebugMode) {
            console.info(this.logPrefix, ...args);
        }
    }

    warn(...args) {
        if (this.isDebugMode) {
            console.warn(this.logPrefix, ...args);
        }
    }

    error(...args) {
        console.error(this.logPrefix, ...args);
    }

    debug(...args) {
        if (this.isDebugMode) {
            console.debug(this.logPrefix, ...args);
        }
    }

    group(label) {
        if (this.isDebugMode) {
            console.group(this.logPrefix + ' ' + label);
        }
    }

    groupEnd() {
        if (this.isDebugMode) {
            console.groupEnd();
        }
    }

    time(label) {
        if (this.isDebugMode) {
            console.time(this.logPrefix + ' ' + label);
        }
    }

    timeEnd(label) {
        if (this.isDebugMode) {
            console.timeEnd(this.logPrefix + ' ' + label);
        }
    }
}

// Create global debug logger instance
const debugLogger = new DebugLogger();

// Export debugLogger for other modules
window.RRDebugLogger = DebugLogger;
window.debugLogger = debugLogger;

// Import and initialize theme system
import * as ThemeModule from './theme.js';

// Import and initialize chart system
import * as ChartModule from './chart.js';

// Make theme functions available globally
window.RRTheme = ThemeModule;

// Make chart functions available globally
window.RChart = ChartModule;

window.RRBlazor = {
    getTabIndicatorPosition: function(tabElementId, wrapperElement) {
        const element = document.getElementById(tabElementId);
        if (!element) {
            return { left: 0, width: 0 };
        }
        
        const rect = element.getBoundingClientRect();
        const wrapperRect = wrapperElement ? wrapperElement.getBoundingClientRect() : element.parentElement.getBoundingClientRect();
        
        return {
            left: rect.left - wrapperRect.left,
            width: rect.width
        };
    },
    
    // Enhanced tab scrolling functions
    getTabScrollInfo: function(wrapperElement) {
        if (!wrapperElement) {
            return { isScrollable: false, canScrollLeft: false, canScrollRight: false };
        }
        
        const scrollLeft = wrapperElement.scrollLeft;
        const scrollWidth = wrapperElement.scrollWidth;
        const clientWidth = wrapperElement.clientWidth;
        
        // Add 5px threshold to avoid false positives from rounding errors
        const scrollThreshold = 5;
        const isScrollable = scrollWidth > clientWidth + scrollThreshold;
        
        return {
            isScrollable: isScrollable,
            canScrollLeft: isScrollable && scrollLeft > scrollThreshold,
            canScrollRight: isScrollable && scrollLeft < scrollWidth - clientWidth - scrollThreshold
        };
    },
    
    scrollTabsLeft: function(wrapperElement) {
        if (!wrapperElement) return;
        
        // Enhanced scroll behavior - snap to tabs for professional feel
        const tabs = wrapperElement.querySelectorAll('[role="tab"]');
        const containerRect = wrapperElement.getBoundingClientRect();
        
        // Find the first fully visible tab from the left
        let targetTab = null;
        for (let i = tabs.length - 1; i >= 0; i--) {
            const tabRect = tabs[i].getBoundingClientRect();
            if (tabRect.left < containerRect.left) {
                targetTab = tabs[i];
                break;
            }
        }
        
        if (targetTab) {
            // Scroll to show the target tab with some padding
            const targetLeft = targetTab.offsetLeft - 16; // 16px padding
            wrapperElement.scrollTo({
                left: Math.max(0, targetLeft),
                behavior: 'smooth'
            });
        } else {
            // Fallback to percentage-based scrolling
            const scrollAmount = wrapperElement.clientWidth * 0.7;
            wrapperElement.scrollBy({
                left: -scrollAmount,
                behavior: 'smooth'
            });
        }
    },
    
    scrollTabsRight: function(wrapperElement) {
        if (!wrapperElement) return;
        
        // Enhanced scroll behavior - snap to tabs for professional feel
        const tabs = wrapperElement.querySelectorAll('[role="tab"]');
        const containerRect = wrapperElement.getBoundingClientRect();
        
        // Find the first partially hidden tab from the right
        let targetTab = null;
        for (let i = 0; i < tabs.length; i++) {
            const tabRect = tabs[i].getBoundingClientRect();
            if (tabRect.right > containerRect.right) {
                targetTab = tabs[i];
                break;
            }
        }
        
        if (targetTab) {
            // Scroll to show the target tab with some padding
            const targetLeft = targetTab.offsetLeft - 16; // 16px padding
            wrapperElement.scrollTo({
                left: targetLeft,
                behavior: 'smooth'
            });
        } else {
            // Fallback to percentage-based scrolling
            const scrollAmount = wrapperElement.clientWidth * 0.7;
            wrapperElement.scrollBy({
                left: scrollAmount,
                behavior: 'smooth'
            });
        }
    },
    
    scrollToTab: function(wrapperElement, tabElementId) {
        if (!wrapperElement) return;
        
        const tabElement = document.getElementById(tabElementId);
        if (!tabElement) return;
        
        const wrapperRect = wrapperElement.getBoundingClientRect();
        const tabRect = tabElement.getBoundingClientRect();
        
        // Calculate if tab is outside visible area
        const isTabVisible = tabRect.left >= wrapperRect.left && tabRect.right <= wrapperRect.right;
        
        if (!isTabVisible) {
            // Calculate scroll position to center the tab
            const scrollLeft = wrapperElement.scrollLeft;
            const tabOffsetLeft = tabElement.offsetLeft;
            const wrapperWidth = wrapperElement.clientWidth;
            const tabWidth = tabElement.offsetWidth;
            
            const targetScrollLeft = tabOffsetLeft - (wrapperWidth / 2) + (tabWidth / 2);
            
            wrapperElement.scrollTo({
                left: Math.max(0, targetScrollLeft),
                behavior: 'smooth'
            });
        }
    },
    
    // Auto-resize textarea
    autoResizeTextarea: function(element) {
        if (!element) return;
        
        element.style.height = 'auto';
        element.style.height = element.scrollHeight + 'px';
    },
    
    // Focus element
    focusElement: function(elementId) {
        const element = document.getElementById(elementId);
        if (element) {
            element.focus();
        }
    },
    
    // Scroll element into view
    scrollIntoView: function(elementId, options = {}) {
        const element = document.getElementById(elementId);
        if (element) {
            element.scrollIntoView({
                behavior: 'smooth',
                block: 'nearest',
                ...options
            });
        }
    },
    
    copyToClipboard: function(text) {
        if (navigator.clipboard && window.isSecureContext) {
            return navigator.clipboard.writeText(text);
        } else {
            // Fallback for older browsers or non-HTTPS contexts
            const textArea = document.createElement('textarea');
            textArea.value = text;
            textArea.style.position = 'fixed';
            textArea.style.left = '-999999px';
            textArea.style.top = '-999999px';
            document.body.appendChild(textArea);
            textArea.focus();
            textArea.select();

            try {
                document.execCommand('copy');
                return Promise.resolve();
            } catch (err) {
                debugLogger.error('Failed to copy to clipboard:', err);
                return Promise.reject(err);
            } finally {
                document.body.removeChild(textArea);
            }
        }
    },
    
    // Get element dimensions
    getElementDimensions: function(elementId) {
        const element = document.getElementById(elementId);
        if (!element) {
            return { width: 0, height: 0 };
        }
        
        const rect = element.getBoundingClientRect();
        return {
            width: rect.width,
            height: rect.height,
            top: rect.top,
            left: rect.left
        };
    },
    
    // Add/remove CSS classes
    toggleClass: function(elementId, className, add) {
        const element = document.getElementById(elementId);
        if (element) {
            if (add) {
                element.classList.add(className);
            } else {
                element.classList.remove(className);
            }
        }
    },
    
    // Initialize component (called when component is rendered)
    initializeComponent: function(componentType, elementId, options = {}) {
        const element = document.getElementById(elementId);
        if (!element) return;
        
        switch (componentType) {
            case 'tabs':
                this.initializeTabs(element, options);
                break;
            case 'form-field':
                this.initializeFormField(element, options);
                break;
        }
    },
    
    // Initialize tabs component with enhanced scrolling
    initializeTabs: function(element, navContainer, navWrapper) {
        // Add keyboard navigation
        const tabs = element.querySelectorAll('[role="tab"]');
        tabs.forEach(tab => {
            tab.addEventListener('keydown', (e) => {
                // Handled in Blazor component
            });
        });
        
        // Update indicator position on window resize
        const updateIndicator = () => {
            const activeTab = element.querySelector('[role="tab"][aria-selected="true"]');
            if (activeTab) {
                // Trigger update in Blazor component
                const event = new CustomEvent('rr-tab-indicator-update', {
                    detail: { tabId: activeTab.id }
                });
                element.dispatchEvent(event);
            }
        };
        
        // Enhanced scroll state management
        const updateScrollState = () => {
            if (navWrapper) {
                const scrollInfo = this.getTabScrollInfo(navWrapper);
                const navElement = element.querySelector('.tabs__nav');
                if (navElement) {
                    navElement.classList.toggle('tabs__nav--scrollable', scrollInfo.isScrollable);
                }
                
                // Update arrow visibility
                const leftArrow = element.querySelector('.tabs__nav-arrow--left');
                const rightArrow = element.querySelector('.tabs__nav-arrow--right');
                
                if (leftArrow) {
                    leftArrow.classList.toggle('tabs__nav-arrow--visible', scrollInfo.canScrollLeft);
                }
                if (rightArrow) {
                    rightArrow.classList.toggle('tabs__nav-arrow--visible', scrollInfo.canScrollRight);
                }
            }
        };
        
        // Event listeners
        window.addEventListener('resize', updateIndicator);
        window.addEventListener('resize', updateScrollState);
        
        // Scroll event listener for real-time arrow updates
        if (navWrapper) {
            navWrapper.addEventListener('scroll', updateScrollState);
        }
        
        // Initial setup
        updateScrollState();
        
        // Store cleanup function
        element._rrCleanup = () => {
            window.removeEventListener('resize', updateIndicator);
            window.removeEventListener('resize', updateScrollState);
            if (navWrapper) {
                navWrapper.removeEventListener('scroll', updateScrollState);
            }
        };
    },
    
    // Initialize form field component  
    initializeFormField: function(element, options) {
        const input = element.querySelector('input, textarea, select');
        if (!input) return;
        
        // Auto-resize textarea
        if (input.tagName === 'TEXTAREA' && options.autoResize) {
            const autoResize = () => this.autoResizeTextarea(input);
            input.addEventListener('input', autoResize);
            autoResize(); // Initial resize
            
            element._rrCleanup = () => {
                input.removeEventListener('input', autoResize);
            };
        }
        
        // Character count updates
        if (options.showCharacterCount && options.maxLength) {
            const updateCount = () => {
                const counter = element.querySelector('.rr-form-field__character-count');
                if (counter) {
                    const count = input.value.length;
                    counter.textContent = `${count} / ${options.maxLength}`;
                    
                    // Update color based on percentage
                    const percentage = count / options.maxLength;
                    counter.classList.remove('text-error', 'text-warning', 'text-tertiary');
                    
                    if (percentage >= 1.0) {
                        counter.classList.add('text-error');
                    } else if (percentage >= 0.9) {
                        counter.classList.add('text-warning');
                    } else {
                        counter.classList.add('text-tertiary');
                    }
                }
            };
            
            input.addEventListener('input', updateCount);
            updateCount(); // Initial update
        }
        
        // Floating label support
        if (options.isFloatingLabel || element.classList.contains('form-field__wrapper--floating-label')) {
            this.initializeFloatingLabel(input, element);
        }
    },
    
    // Floating label functionality
    updateFloatingLabelClasses: function(wrapperElement, classString) {
        if (!wrapperElement) return;
        
        // Get the base classes (everything except state classes)
        const existingClasses = wrapperElement.className.split(' ');
        const newClasses = classString ? classString.split(' ') : [];
        
        // Remove all floating-label state classes
        const filteredClasses = existingClasses.filter(cls => 
            !cls.startsWith('form-field__wrapper--has-value') &&
            !cls.startsWith('form-field__wrapper--floating') &&
            !cls.startsWith('form-field__wrapper--error') &&
            !cls.startsWith('form-field__wrapper--disabled')
        );
        
        // Combine with new state classes
        const finalClasses = [...filteredClasses, ...newClasses].filter(cls => cls.length > 0);
        
        // Apply classes atomically
        wrapperElement.className = finalClasses.join(' ');
    },
    
    initializeFloatingLabel: function(inputElement, wrapperElement) {
        if (!inputElement || !wrapperElement) return;
        
        const updateState = () => {
            const hasValue = inputElement.value && inputElement.value.trim().length > 0;
            const isFocused = document.activeElement === inputElement;
            
            // Remove all state classes first
            wrapperElement.classList.remove('form-field__wrapper--has-value', 'form-field__wrapper--floating');
            
            // Add appropriate classes
            if (hasValue) {
                wrapperElement.classList.add('form-field__wrapper--has-value');
            }
            if (isFocused || hasValue) {
                wrapperElement.classList.add('form-field__wrapper--floating');
            }
        };
        
        // Initialize state immediately
        setTimeout(updateState, 0);
        
        // Event handlers with debouncing for input
        let inputTimeout;
        const debouncedInputHandler = () => {
            clearTimeout(inputTimeout);
            inputTimeout = setTimeout(updateState, 10);
        };
        
        const focusHandler = () => {
            clearTimeout(inputTimeout);
            wrapperElement.classList.add('form-field__wrapper--floating');
            updateState();
        };
        
        const blurHandler = () => {
            clearTimeout(inputTimeout);
            setTimeout(() => {
                if (!inputElement.value || inputElement.value.trim().length === 0) {
                    wrapperElement.classList.remove('form-field__wrapper--floating');
                }
                updateState();
            }, 50);
        };
        
        // Attach event listeners
        inputElement.addEventListener('focus', focusHandler);
        inputElement.addEventListener('blur', blurHandler);
        inputElement.addEventListener('input', debouncedInputHandler);
        inputElement.addEventListener('change', updateState);
        
        // Store cleanup function
        const existingCleanup = wrapperElement._rrCleanup;
        wrapperElement._rrCleanup = () => {
            if (existingCleanup) existingCleanup();
            clearTimeout(inputTimeout);
            inputElement.removeEventListener('focus', focusHandler);
            inputElement.removeEventListener('blur', blurHandler);
            inputElement.removeEventListener('input', debouncedInputHandler);
            inputElement.removeEventListener('change', updateState);
        };
        
        return updateState;
    },
    
    // Cleanup component
    cleanupComponent: function(elementId) {
        const element = document.getElementById(elementId);
        if (element && element._rrCleanup) {
            element._rrCleanup();
            delete element._rrCleanup;
        }
    },
    
    // Update URL without causing page scroll
    updateUrlWithoutScroll: function(newUrl) {
        if (window.history && window.history.pushState) {
            window.history.pushState({ path: newUrl }, '', newUrl);
        }
    },
    
    // Adjust dropdown position based on viewport constraints
    adjustDropdownPosition: function(dropdownElement) {
        if (!dropdownElement) return;
        
        const viewport = dropdownElement.querySelector('.dropdown__viewport');
        const content = dropdownElement.querySelector('.dropdown__content');
        
        if (!viewport || !content) return;
        
        // Get dropdown and viewport dimensions
        const dropdownRect = dropdownElement.getBoundingClientRect();
        const viewportHeight = window.innerHeight;
        const viewportWidth = window.innerWidth;
        
        // Calculate space above and below the dropdown trigger
        const spaceAbove = dropdownRect.top;
        const spaceBelow = viewportHeight - dropdownRect.bottom;
        
        // Get content dimensions (temporarily show it to measure)
        const originalDisplay = content.style.display;
        content.style.visibility = 'hidden';
        content.style.display = 'block';
        const contentRect = content.getBoundingClientRect();
        content.style.display = originalDisplay;
        content.style.visibility = '';
        
        const contentHeight = contentRect.height || 250; // Fallback height
        const shouldPositionAbove = spaceBelow < contentHeight && spaceAbove > spaceBelow;
        
        if (shouldPositionAbove) {
            viewport.style.bottom = '100%';
            viewport.style.top = 'auto';
            viewport.style.marginBottom = '8px';
            viewport.style.marginTop = '0';
        } else {
            viewport.style.top = '100%';
            viewport.style.bottom = 'auto';
            viewport.style.marginTop = '8px';
            viewport.style.marginBottom = '0';
        }
        
        if (dropdownRect.right + 320 > viewportWidth) { // 320px typical dropdown width
            viewport.style.right = '0';
            viewport.style.left = 'auto';
        } else {
            viewport.style.left = '0';
            viewport.style.right = 'auto';
        }
        
        const dropdown = dropdownElement;
        dropdown.classList.remove('dropdown--position-above', 'dropdown--position-below');
        dropdown.classList.add(shouldPositionAbove ? 'dropdown--position-above' : 'dropdown--position-below');
    }
};

// Global helper for adding event listeners from Blazor
window.addEventListener = function(elementId, eventName, dotNetRef, methodName) {
    const element = document.getElementById(elementId);
    if (!element || !dotNetRef) return;

    const handler = function(e) {
        dotNetRef.invokeMethodAsync(methodName, e.detail);
    };

    element.addEventListener(eventName, handler);
    
    // Store cleanup function
    if (!element._rrEventCleanups) {
        element._rrEventCleanups = [];
    }
    element._rrEventCleanups.push(() => {
        element.removeEventListener(eventName, handler);
    });
};

// Global functions for Blazor interop
window.updateFloatingLabelClasses = function(wrapperElement, classString) {
    return RRBlazor.updateFloatingLabelClasses(wrapperElement, classString);
};

window.initializeFloatingLabel = function(inputElement, wrapperElement) {
    return RRBlazor.initializeFloatingLabel(inputElement, wrapperElement);
};

window.updateUrlWithoutScroll = function(newUrl) {
    return RRBlazor.updateUrlWithoutScroll(newUrl);
};


document.addEventListener('DOMContentLoaded', function() {
    // Initialize any components that need it
    document.querySelectorAll('[data-rr-component]').forEach(element => {
        const componentType = element.getAttribute('data-rr-component');
        const options = element.getAttribute('data-rr-options');
        RRBlazor.initializeComponent(componentType, element.id, options ? JSON.parse(options) : {});
    });
    
    // Auto-initialize floating labels
    document.querySelectorAll('.form-field__wrapper--floating-label').forEach(wrapper => {
        const input = wrapper.querySelector('input, textarea, select');
        if (input && !input.dataset.floatingLabelInitialized) {
            RRBlazor.initializeFloatingLabel(input, wrapper);
            input.dataset.floatingLabelInitialized = 'true';
        }
    });
});

window.RRBlazor.downloadContent = function(content, fileName, contentType = 'text/plain') {
    const blob = new Blob([content], { type: contentType });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
};

window.downloadFileFromStream = async function(fileName, contentStream) {
    try {
        const arrayBuffer = await contentStream.arrayBuffer();
        const blob = new Blob([arrayBuffer]);
        const url = URL.createObjectURL(blob);
        
        const anchorElement = document.createElement('a');
        anchorElement.href = url;
        anchorElement.download = fileName ?? '';
        anchorElement.click();
        anchorElement.remove();
        
        URL.revokeObjectURL(url);
        return true;
    } catch (error) {
        debugLogger.error('Download failed:', error);
        return false;
    }
};

// Simple file download from URL
window.downloadFile = function(url, fileName) {
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName || 'download';
    link.style.display = 'none';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};