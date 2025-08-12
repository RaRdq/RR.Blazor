// Form field utilities and interactions

// Cache for textarea dimensions to avoid redundant calculations
const textareaCache = new WeakMap();

export function autoResizeTextarea(element) {
    if (!element) return;
    
    const cached = textareaCache.get(element);
    const currentValue = element.value;
    
    // Skip resize if content hasn't changed significantly
    if (cached && cached.value === currentValue && cached.scrollHeight === element.scrollHeight) {
        return;
    }
    
    // Use requestAnimationFrame to batch style changes
    requestAnimationFrame(() => {
        element.style.height = 'auto';
        const newHeight = element.scrollHeight;
        element.style.height = newHeight + 'px';
        
        // Cache the result
        textareaCache.set(element, {
            value: currentValue,
            scrollHeight: newHeight,
            timestamp: Date.now()
        });
    });
}

export function initializeFormField(element, options) {
    const input = element.querySelector('input, textarea, select');
    if (!input) return;
    
    const cleanupFunctions = [];
    
    if (input.tagName === 'TEXTAREA' && options.autoResize) {
        // Debounce auto-resize for better performance
        let resizeTimeout;
        const debouncedAutoResize = () => {
            if (resizeTimeout) clearTimeout(resizeTimeout);
            resizeTimeout = setTimeout(() => autoResizeTextarea(input), 16);
        };
        
        input.addEventListener('input', debouncedAutoResize, { passive: true });
        autoResizeTextarea(input);
        
        cleanupFunctions.push(() => {
            input.removeEventListener('input', debouncedAutoResize);
            if (resizeTimeout) clearTimeout(resizeTimeout);
        });
    }
    
    if (options.showCharacterCount && options.maxLength) {
        // Debounce character count updates
        let countTimeout;
        const debouncedUpdateCount = () => {
            if (countTimeout) clearTimeout(countTimeout);
            countTimeout = setTimeout(() => {
                const counter = element.querySelector('.rr-form-field__character-count');
                if (counter) {
                    const count = input.value.length;
                    counter.textContent = `${count} / ${options.maxLength}`;
                    
                    const percentage = count / options.maxLength;
                    
                    // Batch class updates
                    requestAnimationFrame(() => {
                        counter.classList.remove('text-error', 'text-warning', 'text-tertiary');
                        
                        if (percentage >= 1.0) {
                            counter.classList.add('text-error');
                        } else if (percentage >= 0.9) {
                            counter.classList.add('text-warning');
                        } else {
                            counter.classList.add('text-tertiary');
                        }
                    });
                }
            }, 100);
        };
        
        input.addEventListener('input', debouncedUpdateCount, { passive: true });
        debouncedUpdateCount();
        
        cleanupFunctions.push(() => {
            input.removeEventListener('input', debouncedUpdateCount);
            if (countTimeout) clearTimeout(countTimeout);
        });
    }
    
    // Store all cleanup functions
    element._rrCleanup = () => {
        cleanupFunctions.forEach(cleanup => cleanup());
    };
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
            if (textArea.parentNode) {
                textArea.remove();
            }
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

export default {
    autoResizeTextarea,
    initializeFormField,
    focusElement,
    copyToClipboard,
    cleanupComponent
};