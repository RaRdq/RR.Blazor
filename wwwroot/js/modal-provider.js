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
        document.addEventListener('keydown', (event) => {
            if (event.key === 'Escape' && this.dotNetRef) {
                this.dotNetRef.invokeMethodAsync('OnKeyDown', event.key);
            }
        });

        this.setupScrollLock();
        this.setupFocusManagement();
    }

    getVisibleModal() {
        const modals = document.querySelectorAll('.modal[role="dialog"]');
        for (const modal of modals) {
            const computedStyle = window.getComputedStyle(modal);
            const isVisible = computedStyle.display !== 'none' && 
                             computedStyle.visibility !== 'hidden' && 
                             computedStyle.opacity !== '0' &&
                             modal.offsetHeight > 0 && 
                             modal.offsetWidth > 0;
            if (isVisible) return modal;
        }
        return null;
    }

    setupScrollLock() {
        const observer = new MutationObserver(() => {
            requestAnimationFrame(() => {
                const visibleModal = this.getVisibleModal();
                if (visibleModal) {
                    this.repositionModalToBodyLevel(visibleModal);
                    this.lockBodyScroll();
                } else {
                    this.unlockBodyScroll();
                }
            });
        });

        observer.observe(document.body, { 
            childList: true, 
            subtree: true,
            attributes: true,
            attributeFilter: ['class', 'style']
        });

        this.scrollObserver = observer;
        
        const visibleModal = this.getVisibleModal();
        if (visibleModal) {
            this.repositionModalToBodyLevel(visibleModal);
            this.lockBodyScroll();
        } else {
            this.unlockBodyScroll();
        }
    }

    repositionModalToBodyLevel(modal) {
        if (modal.parentElement === document.body) return;

        if (!modal._originalParent) {
            modal._originalParent = modal.parentElement;
            modal._originalNextSibling = modal.nextSibling;
        }

        document.body.appendChild(modal);
        modal.style.position = 'fixed';
        modal.style.inset = '0';
        modal.style.zIndex = getComputedStyle(document.documentElement).getPropertyValue('--z-modal') || '1050';
    }

    restoreModalPosition(modal) {
        if (modal._originalParent && modal._originalParent !== document.body) {
            if (modal._originalNextSibling) {
                modal._originalParent.insertBefore(modal, modal._originalNextSibling);
            } else {
                modal._originalParent.appendChild(modal);
            }
            
            delete modal._originalParent;
            delete modal._originalNextSibling;
        }
    }

    lockBodyScroll() {
        if (document.body.classList.contains('modal-open')) return;
        
        this.originalScrollTop = window.pageYOffset || document.documentElement.scrollTop;
        
        document.body.style.overflow = 'hidden';
        document.body.style.position = 'fixed';
        document.body.style.top = `-${this.originalScrollTop}px`;
        document.body.style.left = '0';
        document.body.style.right = '0';
        document.body.style.width = '100%';
        
        document.body.classList.add('modal-open');
        
        this.preventScrollEvents();
    }

    unlockBodyScroll() {
        const visibleModal = this.getVisibleModal();
        if (visibleModal) return;
        
        const bodyModals = document.querySelectorAll('body > .modal[role="dialog"]');
        bodyModals.forEach(modal => this.restoreModalPosition(modal));
        
        document.body.style.overflow = '';
        document.body.style.position = '';
        document.body.style.top = '';
        document.body.style.left = '';
        document.body.style.right = '';
        document.body.style.width = '';
        
        document.body.classList.remove('modal-open');
        
        this.allowScrollEvents();
        
        if (this.originalScrollTop) {
            window.scrollTo(0, this.originalScrollTop);
            this.originalScrollTop = 0;
        }
    }

    preventScrollEvents() {
        this.wheelListener = (e) => {
            if (!this.isScrollableArea(e.target)) {
                e.preventDefault();
                e.stopPropagation();
            }
        };
        
        this.touchMoveListener = (e) => {
            if (!this.isScrollableArea(e.target)) {
                e.preventDefault();
                e.stopPropagation();
            }
        };
        
        this.keydownListener = (e) => {
            const scrollKeys = ['ArrowUp', 'ArrowDown', 'PageUp', 'PageDown', 'Home', 'End', ' '];
            if (scrollKeys.includes(e.key) && !this.isScrollableArea(e.target)) {
                e.preventDefault();
                e.stopPropagation();
            }
        };
        
        document.addEventListener('wheel', this.wheelListener, { passive: false });
        document.addEventListener('touchmove', this.touchMoveListener, { passive: false });
        document.addEventListener('keydown', this.keydownListener, { passive: false });
    }

    isScrollableArea(element) {
        while (element && element !== document.body) {
            if (element.matches('.modal-body, .modal-content, .select-modal-list, [data-scroll="true"]')) {
                const computedStyle = window.getComputedStyle(element);
                return computedStyle.overflowY === 'auto' || 
                       computedStyle.overflowY === 'scroll' ||
                       computedStyle.overflow === 'auto' ||
                       computedStyle.overflow === 'scroll';
            }
            element = element.parentElement;
        }
        return false;
    }

    allowScrollEvents() {
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

    dispose() {
        if (this.scrollObserver) {
            this.scrollObserver.disconnect();
        }

        this.allowScrollEvents();
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
    }
};