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
            // Use requestAnimationFrame to ensure DOM is fully updated
            requestAnimationFrame(() => {
                // Look for any modal elements in the DOM, not just inside modal-provider
                const hasModal = document.querySelector('.modal[role="dialog"]');
                
                if (hasModal) {
                    // FRAMEWORK INTEGRATION FIX: Move modal to body level for proper positioning
                    this.repositionModalToBodyLevel(hasModal);
                    this.lockBodyScroll();
                } else {
                    this.unlockBodyScroll();
                }
            });
        });

        // Observe the document body for any modal additions/removals
        observer.observe(document.body, { 
            childList: true, 
            subtree: true,
            attributes: true,
            attributeFilter: ['class', 'style']
        });

        this.scrollObserver = observer;
        
        // Initial check
        this.checkModalState();
    }

    repositionModalToBodyLevel(modal) {
        // Check if modal is already at body level
        if (modal.parentElement === document.body) {
            return; // Already positioned correctly
        }

        // Store original parent for cleanup
        if (!modal._originalParent) {
            modal._originalParent = modal.parentElement;
            modal._originalNextSibling = modal.nextSibling;
        }

        // Move modal to body level for proper viewport positioning
        document.body.appendChild(modal);
        
        // Ensure modal has proper body-level positioning
        modal.style.position = 'fixed';
        modal.style.inset = '0';
        modal.style.zIndex = getComputedStyle(document.documentElement).getPropertyValue('--z-modal') || '1050';
        
        console.log('[Modal Provider] Repositioned modal to body level for framework integration');
    }

    restoreModalPosition(modal) {
        // Restore modal to original position when closed
        if (modal._originalParent && modal._originalParent !== document.body) {
            if (modal._originalNextSibling) {
                modal._originalParent.insertBefore(modal, modal._originalNextSibling);
            } else {
                modal._originalParent.appendChild(modal);
            }
            
            // Clean up tracking properties
            delete modal._originalParent;
            delete modal._originalNextSibling;
            
            console.log('[Modal Provider] Restored modal to original position');
        }
    }

    checkModalState() {
        // Look for any modal elements in the DOM, not just inside modal-provider
        const hasModal = document.querySelector('.modal[role="dialog"]');
        
        if (hasModal) {
            this.repositionModalToBodyLevel(hasModal);
            this.lockBodyScroll();
        } else {
            this.unlockBodyScroll();
        }
    }

    lockBodyScroll() {
        if (!document.body.classList.contains('modal-open')) {
            // Store original scroll position
            this.originalScrollTop = window.pageYOffset || document.documentElement.scrollTop;
            
            // Calculate scrollbar width to prevent layout shift
            const scrollbarWidth = this.getScrollbarWidth();
            
            // Apply scroll lock with enhanced compatibility
            document.body.style.overflow = 'hidden';
            document.body.style.position = 'fixed';
            document.body.style.top = `-${this.originalScrollTop}px`;
            document.body.style.left = '0';
            document.body.style.right = '0';
            document.body.style.width = '100%';
            document.body.style.paddingRight = scrollbarWidth + 'px';
            
            // Also lock html element for better browser compatibility
            document.documentElement.style.overflow = 'hidden';
            document.documentElement.style.position = 'relative';
            
            // Prevent touch scrolling on mobile devices
            document.body.style.touchAction = 'none';
            document.body.style.webkitOverflowScrolling = 'touch';
            
            // Add body class for additional styling hooks
            document.body.classList.add('modal-open');
            
            // Prevent wheel and touch events that cause visual scrolling
            this.preventScrollEvents();
            
            // Prevent scroll restoration during popstate
            if ('scrollRestoration' in history) {
                this.originalScrollRestoration = history.scrollRestoration;
                history.scrollRestoration = 'manual';
            }
        }
    }

    unlockBodyScroll() {
        // Double-check no modals exist before unlocking
        const hasModal = document.querySelector('.modal[role="dialog"]');
        
        if (!hasModal && document.body.classList.contains('modal-open')) {
            // Restore any modals that were moved to body level
            const bodyModals = document.querySelectorAll('body > .modal[role="dialog"]');
            bodyModals.forEach(modal => this.restoreModalPosition(modal));
            
            // Remove scroll lock
            document.body.style.overflow = '';
            document.body.style.position = '';
            document.body.style.top = '';
            document.body.style.left = '';
            document.body.style.right = '';
            document.body.style.width = '';
            document.body.style.paddingRight = '';
            document.body.style.touchAction = '';
            document.body.style.webkitOverflowScrolling = '';
            
            // Unlock html element
            document.documentElement.style.overflow = '';
            document.documentElement.style.position = '';
            
            // Remove body class
            document.body.classList.remove('modal-open');
            
            // Remove scroll event prevention
            this.allowScrollEvents();
            
            // Restore scroll restoration
            if (this.originalScrollRestoration) {
                history.scrollRestoration = this.originalScrollRestoration;
                this.originalScrollRestoration = null;
            }
            
            // Restore scroll position
            if (this.originalScrollTop) {
                window.scrollTo(0, this.originalScrollTop);
                this.originalScrollTop = 0;
            }
        }
    }

    preventScrollEvents() {
        // Prevent wheel events (mouse scroll)
        this.wheelListener = (e) => {
            e.preventDefault();
            e.stopPropagation();
        };
        
        // Prevent touch events (mobile scroll)
        this.touchMoveListener = (e) => {
            e.preventDefault();
            e.stopPropagation();
        };
        
        // Prevent keyboard scroll (arrow keys, space, page up/down)
        this.keydownListener = (e) => {
            const scrollKeys = ['ArrowUp', 'ArrowDown', 'PageUp', 'PageDown', 'Home', 'End', ' '];
            if (scrollKeys.includes(e.key)) {
                e.preventDefault();
                e.stopPropagation();
            }
        };
        
        // Add listeners with passive: false to ensure preventDefault works
        document.addEventListener('wheel', this.wheelListener, { passive: false });
        document.addEventListener('touchmove', this.touchMoveListener, { passive: false });
        document.addEventListener('keydown', this.keydownListener, { passive: false });
    }

    allowScrollEvents() {
        // Remove all scroll prevention listeners
        if (this.wheelListener) {
            document.removeEventListener('wheel', this.wheelListener);
            this.wheelListener = null;
        }
        if (this.touchMoveListener) {
            document.removeEventListener('touchmove', this.touchMoveListener);
            this.touchMoveListener = null;
        }
        if (this.keydownListener) {
            document.removeEventListener('keydown', this.keydownListener);
            this.keydownListener = null;
        }
    }

    setupFocusManagement() {
        document.addEventListener('focusin', (event) => {
            const modals = document.querySelectorAll('.modal[role="dialog"]');
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

        // Remove scroll event prevention
        this.allowScrollEvents();

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