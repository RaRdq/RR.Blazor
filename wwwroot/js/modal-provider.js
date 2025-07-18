// RModalProvider JavaScript module - MudBlazor-style modal management
export function initialize(dotNetRef) {
    const modalProvider = new ModalProvider(dotNetRef);
    return modalProvider;
}

class ModalProvider {
    constructor(dotNetRef) {
        this.dotNetRef = dotNetRef;
        this.setupEventListeners();
    }

    setupEventListeners() {
        // Global keyboard event handler for ESC key
        document.addEventListener('keydown', (event) => {
            if (event.key === 'Escape' && this.dotNetRef) {
                this.dotNetRef.invokeMethodAsync('OnKeyDown', event.key);
            }
        });

        // Prevent body scroll when modal is open
        this.setupScrollLock();

        // Handle focus management for accessibility
        this.setupFocusManagement();
    }

    setupScrollLock() {
        const observer = new MutationObserver((mutations) => {
            const hasModal = document.querySelector('.r-modal-provider .modal');
            if (hasModal) {
                this.lockBodyScroll();
            } else {
                this.unlockBodyScroll();
            }
        });

        observer.observe(document.body, { 
            childList: true, 
            subtree: true 
        });

        this.scrollObserver = observer;
    }

    lockBodyScroll() {
        if (document.body.style.overflow !== 'hidden') {
            // Store original scroll position
            this.originalScrollTop = window.pageYOffset || document.documentElement.scrollTop;
            
            // Calculate scrollbar width to prevent layout shift
            const scrollbarWidth = this.getScrollbarWidth();
            
            // Apply scroll lock
            document.body.style.overflow = 'hidden';
            document.body.style.position = 'fixed';
            document.body.style.top = `-${this.originalScrollTop}px`;
            document.body.style.width = '100%';
            document.body.style.paddingRight = scrollbarWidth + 'px';
            
            // Also lock html element for better browser compatibility
            document.documentElement.style.overflow = 'hidden';
            
            // Add body class for additional styling hooks
            document.body.classList.add('modal-open');
        }
    }

    unlockBodyScroll() {
        if (document.body.style.overflow === 'hidden') {
            // Remove scroll lock
            document.body.style.overflow = '';
            document.body.style.position = '';
            document.body.style.top = '';
            document.body.style.width = '';
            document.body.style.paddingRight = '';
            
            // Unlock html element
            document.documentElement.style.overflow = '';
            
            // Remove body class
            document.body.classList.remove('modal-open');
            
            // Restore scroll position
            if (this.originalScrollTop) {
                window.scrollTo(0, this.originalScrollTop);
                this.originalScrollTop = 0;
            }
        }
    }

    setupFocusManagement() {
        document.addEventListener('focusin', (event) => {
            const modals = document.querySelectorAll('.r-modal-provider .modal');
            if (modals.length === 0) return;

            const topModal = Array.from(modals).pop();
            if (!topModal.contains(event.target)) {
                // Focus escaped modal, bring it back
                const focusableElement = this.getFocusableElements(topModal)[0];
                if (focusableElement) {
                    focusableElement.focus();
                }
            }
        });
    }

    getFocusableElements(container) {
        const focusableSelectors = [
            'button:not([disabled])',
            'input:not([disabled])',
            'select:not([disabled])',
            'textarea:not([disabled])',
            'a[href]',
            '[tabindex]:not([tabindex="-1"])'
        ];

        return container.querySelectorAll(focusableSelectors.join(', '));
    }

    getScrollbarWidth() {
        const div = document.createElement('div');
        div.style.cssText = 'width: 100px; height: 100px; overflow: scroll; position: absolute; top: -9999px;';
        document.body.appendChild(div);
        const scrollbarWidth = div.offsetWidth - div.clientWidth;
        document.body.removeChild(div);
        return scrollbarWidth;
    }

    dispose() {
        if (this.scrollObserver) {
            this.scrollObserver.disconnect();
        }

        // Restore body scroll completely
        this.unlockBodyScroll();
    }
}

export const modalUtils = {
    copyToClipboard(text) {
        return RRBlazor.copyToClipboard(text);
    },

    downloadContent(content, fileName, contentType = 'text/plain') {
        return RRBlazor.downloadContent(content, fileName, contentType);
    },

    setupFormValidation(formElement) {
        if (!formElement) return;

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
    },

    // Setup virtual scrolling for large lists in select modals
    setupVirtualScrolling(listElement, itemHeight = 48) {
        if (!listElement) return;

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

    // Highlight search terms in text
    highlightSearchTerms(element, searchTerm) {
        if (!searchTerm || !element) return;

        const text = element.textContent;
        const regex = new RegExp(`(${this.escapeRegExp(searchTerm)})`, 'gi');
        
        // Clear element first
        element.innerHTML = '';
        
        // Split text and create text nodes with highlights
        let lastIndex = 0;
        let match;
        regex.lastIndex = 0; // Reset regex state
        
        while ((match = regex.exec(text)) !== null) {
            // Add text before match
            if (match.index > lastIndex) {
                const beforeText = text.substring(lastIndex, match.index);
                element.appendChild(document.createTextNode(beforeText));
            }
            
            // Add highlighted match
            const mark = document.createElement('mark');
            mark.textContent = match[1];
            element.appendChild(mark);
            
            lastIndex = match.index + match[1].length;
            
            // Prevent infinite loop
            if (regex.lastIndex === match.index) {
                regex.lastIndex++;
            }
        }
        
        // Add remaining text
        if (lastIndex < text.length) {
            const remainingText = text.substring(lastIndex);
            element.appendChild(document.createTextNode(remainingText));
        }
    },

    escapeRegExp(string) {
        return string.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    }
};