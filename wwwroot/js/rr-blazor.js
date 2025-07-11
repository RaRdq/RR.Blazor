// RR.Blazor JavaScript Helpers
// Supporting functions for component interactions

window.RRBlazor = {
    // Tab indicator positioning
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
    
    // Copy text to clipboard
    copyToClipboard: async function(text) {
        try {
            await navigator.clipboard.writeText(text);
            return true;
        } catch (err) {
            // Fallback for older browsers
            const textArea = document.createElement('textarea');
            textArea.value = text;
            document.body.appendChild(textArea);
            textArea.select();
            const success = document.execCommand('copy');
            document.body.removeChild(textArea);
            return success;
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
        
        // Store cleanup function
        element._rrCleanup = () => {
            window.removeEventListener('resize', updateIndicator);
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
    },
    
    // Cleanup component
    cleanupComponent: function(elementId) {
        const element = document.getElementById(elementId);
        if (element && element._rrCleanup) {
            element._rrCleanup();
            delete element._rrCleanup;
        }
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

// Auto-initialize components on DOM load
document.addEventListener('DOMContentLoaded', function() {
    // Initialize any components that need it
    document.querySelectorAll('[data-rr-component]').forEach(element => {
        const componentType = element.getAttribute('data-rr-component');
        const options = element.getAttribute('data-rr-options');
        RRBlazor.initializeComponent(componentType, element.id, options ? JSON.parse(options) : {});
    });
});