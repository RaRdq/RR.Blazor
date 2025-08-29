const textareaCache = new WeakMap();

export function autoResizeTextarea(element) {
    const cached = textareaCache.get(element);
    const currentValue = element.value;
    
    if (cached && cached.value === currentValue && cached.scrollHeight === element.scrollHeight) {
        return;
    }
    
    requestAnimationFrame(() => {
        element.style.height = 'auto';
        const newHeight = element.scrollHeight;
        element.style.height = newHeight + 'px';
        
        textareaCache.set(element, {
            value: currentValue,
            scrollHeight: newHeight,
            timestamp: Date.now()
        });
    });
}

export function initializeFormField(element, options) {
    const input = element.querySelector('input, textarea, select');
    
    const cleanupFunctions = [];
    
    if (input.tagName === 'TEXTAREA' && options.autoResize) {
        let resizeScheduled = false;
        const scheduleResize = () => {
            if (!resizeScheduled) {
                resizeScheduled = true;
                requestAnimationFrame(() => {
                    autoResizeTextarea(input);
                    resizeScheduled = false;
                });
            }
        };
        
        input.addEventListener('input', scheduleResize, { passive: true });
        autoResizeTextarea(input);
        
        cleanupFunctions.push(() => {
            input.removeEventListener('input', scheduleResize);
        });
    }
    
    if (options.showCharacterCount && options.maxLength) {
        const updateCount = () => {
            const counter = element.querySelector('.rr-form-field__character-count');
            const count = input.value.length;
            counter.textContent = `${count} / ${options.maxLength}`;
            
            const percentage = count / options.maxLength;
            
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
        };
        
        input.addEventListener('input', updateCount, { passive: true });
        updateCount();
        
        cleanupFunctions.push(() => {
            input.removeEventListener('input', updateCount);
        });
    }
    
    element._rrCleanup = () => {
        cleanupFunctions.forEach(cleanup => cleanup());
    };
}

export function focusElement(elementId) {
    const element = document.getElementById(elementId);
    element.focus();
    return true;
}

export function copyToClipboard(text) {
    return window.RRBlazor.Clipboard.writeText(text);
}

export function cleanupComponent(elementId) {
    if (!elementId) return;
    
    const element = document.getElementById(elementId);
    if (!element) return;
    
    if (element._rrCleanup) {
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