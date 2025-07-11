// Modal Container JavaScript functionality
window.modalContainer = {
    dotNetRef: null,
    
    initialize(dotNetRef) {
        this.dotNetRef = dotNetRef;
        this.setupEventListeners();
    },
    
    setupEventListeners() {
        // Global keyboard event handler
        document.addEventListener('keydown', (event) => {
            if (this.dotNetRef) {
                this.dotNetRef.invokeMethodAsync('OnKeyDown', event.key);
            }
        });
        
        // Prevent body scroll when modal is open
        this.setupScrollLock();
    },
    
    setupScrollLock() {
        const observer = new MutationObserver((mutations) => {
            const hasModal = document.querySelector('.modal-container .modal');
            if (hasModal) {
                document.body.style.overflow = 'hidden';
            } else {
                document.body.style.overflow = '';
            }
        });
        
        observer.observe(document.body, { childList: true, subtree: true });
    }
};

// Preview Modal JavaScript functionality
window.previewModal = {
    copyToClipboard(text) {
        // Fallback clipboard functionality for older browsers
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
        } catch (err) {
            console.error('Failed to copy to clipboard:', err);
        }
        
        document.body.removeChild(textArea);
    },
    
    downloadContent(content, fileName, contentType) {
        const blob = new Blob([content], { type: contentType });
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = fileName;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        window.URL.revokeObjectURL(url);
    }
};

// Form Modal JavaScript functionality
window.formModal = {
    setupValidation(formElement) {
        if (!formElement) return;
        
        // Add real-time validation
        const inputs = formElement.querySelectorAll('input, select, textarea');
        inputs.forEach(input => {
            input.addEventListener('blur', () => {
                this.validateField(input);
            });
            
            input.addEventListener('input', () => {
                this.clearFieldErrors(input);
            });
        });
    },
    
    validateField(field) {
        const errorElement = field.parentElement.querySelector('.field-error');
        if (field.hasAttribute('required') && !field.value.trim()) {
            this.showFieldError(field, 'This field is required');
            return false;
        }
        
        if (field.type === 'email' && field.value && !this.isValidEmail(field.value)) {
            this.showFieldError(field, 'Please enter a valid email address');
            return false;
        }
        
        this.clearFieldErrors(field);
        return true;
    },
    
    showFieldError(field, message) {
        this.clearFieldErrors(field);
        const errorElement = document.createElement('div');
        errorElement.className = 'field-error text-sm text--error mt-1';
        errorElement.textContent = message;
        field.parentElement.appendChild(errorElement);
        field.classList.add('field--error');
    },
    
    clearFieldErrors(field) {
        const errorElement = field.parentElement.querySelector('.field-error');
        if (errorElement) {
            errorElement.remove();
        }
        field.classList.remove('field--error');
    },
    
    isValidEmail(email) {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return emailRegex.test(email);
    }
};

// Select Modal JavaScript functionality
window.selectModal = {
    setupVirtualScrolling(listElement, itemHeight = 48) {
        if (!listElement) return;
        
        // Implement virtual scrolling for large lists
        const container = listElement.parentElement;
        const totalItems = listElement.children.length;
        const containerHeight = container.clientHeight;
        const visibleItems = Math.ceil(containerHeight / itemHeight) + 2;
        
        if (totalItems <= visibleItems) return; // No need for virtual scrolling
        
        let scrollTop = 0;
        
        container.addEventListener('scroll', () => {
            scrollTop = container.scrollTop;
            this.updateVisibleItems(listElement, scrollTop, itemHeight, visibleItems);
        });
        
        this.updateVisibleItems(listElement, scrollTop, itemHeight, visibleItems);
    },
    
    updateVisibleItems(listElement, scrollTop, itemHeight, visibleItems) {
        const startIndex = Math.floor(scrollTop / itemHeight);
        const endIndex = Math.min(startIndex + visibleItems, listElement.children.length);
        
        for (let i = 0; i < listElement.children.length; i++) {
            const item = listElement.children[i];
            if (i >= startIndex && i < endIndex) {
                item.style.display = '';
                item.style.transform = `translateY(${i * itemHeight}px)`;
            } else {
                item.style.display = 'none';
            }
        }
    },
    
    highlightSearchTerms(element, searchTerm) {
        if (!searchTerm) return;
        
        const text = element.textContent;
        const regex = new RegExp(`(${searchTerm})`, 'gi');
        const highlightedText = text.replace(regex, '<mark>$1</mark>');
        element.innerHTML = highlightedText;
    }
};