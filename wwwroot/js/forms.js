// RR.Blazor Forms Component JavaScript Module
// Handles form field interactions, validation, and auto-resize functionality

export function autoResizeTextarea(element) {
    if (!element) return;
    
    element.style.height = 'auto';
    element.style.height = element.scrollHeight + 'px';
}

export function initializeFormField(element, options) {
    const input = element.querySelector('input, textarea, select');
    if (!input) return;
    
    // Auto-resize textarea
    if (input.tagName === 'TEXTAREA' && options.autoResize) {
        const autoResize = () => autoResizeTextarea(input);
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
    
    // Note: Floating label support removed - using pure CSS approach
}

export function focusElement(elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        element.focus();
    }
}

export function copyToClipboard(text) {
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
            if (window.debugLogger) {
                window.debugLogger.error('Failed to copy to clipboard:', err);
            }
            return Promise.reject(err);
        } finally {
            document.body.removeChild(textArea);
        }
    }
}

export function cleanupComponent(elementId) {
    const element = document.getElementById(elementId);
    if (element && element._rrCleanup) {
        element._rrCleanup();
        delete element._rrCleanup;
    }
}

// Export all form utilities
export default {
    autoResizeTextarea,
    initializeFormField,
    focusElement,
    copyToClipboard,
    cleanupComponent
};