// RR.Blazor JavaScript Helpers
// Supporting functions for component interactions

window.RRBlazor = {
    getTabIndicatorPosition: function(tabElementId) {
        const element = document.getElementById(tabElementId);
        if (!element) {
            return { left: 0, width: 0 };
        }
        
        const rect = element.getBoundingClientRect();
        const parentRect = element.parentElement.getBoundingClientRect();
        
        return {
            left: rect.left - parentRect.left,
            width: rect.width
        };
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
                console.error('Failed to copy to clipboard:', err);
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
    
    // Initialize tabs component
    initializeTabs: function(element, options) {
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
        
        window.addEventListener('resize', updateIndicator);
        
        // Enhanced scroll detection for mobile indicators
        const navElement = element.querySelector('.tabs__nav');
        if (navElement) {
            const checkScrollable = () => {
                const isScrollable = navElement.scrollWidth > navElement.clientWidth;
                navElement.classList.toggle('scrollable', isScrollable);
            };
            
            // Check on init and resize
            checkScrollable();
            window.addEventListener('resize', checkScrollable);
            
            // Store enhanced cleanup
            element._rrCleanup = () => {
                window.removeEventListener('resize', updateIndicator);
                window.removeEventListener('resize', checkScrollable);
            };
        } else {
            // Store cleanup function
            element._rrCleanup = () => {
                window.removeEventListener('resize', updateIndicator);
            };
        }
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
        console.error('Download failed:', error);
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