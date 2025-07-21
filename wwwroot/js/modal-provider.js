// ===========================================
// JAVASCRIPT DOM REPOSITIONING MODAL SYSTEM  
// Moves modals to document.body level for full viewport coverage
// ===========================================

export function initialize(dotNetRef) {
    const modalProvider = new ModalProvider(dotNetRef);
    console.log('[Modal Provider] JavaScript DOM repositioning system initialized');
    return modalProvider;
}

class ModalProvider {
    constructor(dotNetRef) {
        this.dotNetRef = dotNetRef;
        this.repositionedModals = new Set();
        this.setupKeyboardHandling();
        this.setupScrollLock();
        this.setupFocusManagement();
        this.setupModalRepositioning();
    }

    setupKeyboardHandling() {
        document.addEventListener('keydown', (event) => {
            if (event.key === 'Escape') {
                const activeModal = this.getActiveModal();
                if (activeModal && this.dotNetRef) {
                    this.dotNetRef.invokeMethodAsync('OnKeyDown', event.key);
                }
            }
        });
    }

    setupModalRepositioning() {
        // Watch for modals being added to the DOM
        const observer = new MutationObserver((mutations) => {
            mutations.forEach(mutation => {
                if (mutation.type === 'childList') {
                    // Check for new modal elements
                    mutation.addedNodes.forEach(node => {
                        if (node.nodeType === Node.ELEMENT_NODE) {
                            // Look for modal elements
                            const modals = node.classList?.contains('modal') ? [node] : node.querySelectorAll?.('.modal') || [];
                            modals.forEach(modal => {
                                if (modal.classList.contains('modal--visible')) {
                                    this.repositionModalToBodyLevel(modal);
                                }
                            });
                            
                            // Check if modal-provider becomes active
                            if (node.classList?.contains('modal-provider') && node.classList.contains('active')) {
                                this.processModalProvider(node);
                            }
                        }
                    });
                    
                    // Check for attribute changes on existing modal providers
                    const modalProviders = document.querySelectorAll('.modal-provider.active');
                    modalProviders.forEach(provider => this.processModalProvider(provider));
                }
                
                if (mutation.type === 'attributes' && mutation.attributeName === 'class') {
                    const target = mutation.target;
                    if (target.classList.contains('modal-provider') && target.classList.contains('active')) {
                        this.processModalProvider(target);
                    }
                    if (target.classList.contains('modal') && target.classList.contains('modal--visible')) {
                        this.repositionModalToBodyLevel(target);
                    }
                }
            });
        });

        observer.observe(document.body, { 
            childList: true, 
            subtree: true,
            attributes: true,
            attributeFilter: ['class']
        });

        this.modalObserver = observer;
    }

    processModalProvider(provider) {
        // Move the entire modal provider to body level
        if (provider.parentElement !== document.body) {
            console.log('[Modal Provider] Moving modal provider to document.body for viewport coverage');
            
            // Store original position for restoration
            if (!provider._originalParent) {
                provider._originalParent = provider.parentElement;
                provider._originalNextSibling = provider.nextSibling;
            }
            
            // Move to body level
            document.body.appendChild(provider);
            
            // Apply viewport-level positioning
            this.applyBodyLevelStyles(provider);
        }
        
        // Lock body scroll when modal provider is active
        this.lockBodyScroll();
        
        // Process any modals within the provider
        const modals = provider.querySelectorAll('.modal--visible');
        modals.forEach(modal => {
            this.applyModalStyles(modal);
        });
    }

    repositionModalToBodyLevel(modal) {
        console.log('[Modal Provider] Repositioning modal to body level for full viewport coverage');
        
        // Lock body scroll when any modal becomes visible
        this.lockBodyScroll();
        
        // Check if modal is already at body level
        if (modal.parentElement === document.body) {
            this.applyModalStyles(modal);
            return;
        }

        // Store original parent for cleanup
        if (!modal._originalParent) {
            modal._originalParent = modal.parentElement;
            modal._originalNextSibling = modal.nextSibling;
        }

        // Move modal to body level for proper viewport positioning
        document.body.appendChild(modal);
        this.repositionedModals.add(modal);
        
        // Apply body-level modal styles
        this.applyModalStyles(modal);
    }

    applyBodyLevelStyles(element) {
        // Force maximum positioning for modal provider
        const styles = {
            position: 'fixed',
            top: '0',
            left: '0',
            right: '0',
            bottom: '0',
            width: '100vw',
            height: '100vh',
            zIndex: '1045',
            pointerEvents: 'auto'
        };

        Object.assign(element.style, styles);
        
        // Force with !important to override any CSS
        element.style.setProperty('position', 'fixed', 'important');
        element.style.setProperty('inset', '0', 'important');
        element.style.setProperty('z-index', '1045', 'important');
        element.style.setProperty('pointer-events', 'auto', 'important');
        
        console.log('[Modal Provider] Applied body-level positioning to modal provider');
    }

    applyModalStyles(modal) {
        // Force viewport-level positioning for modals
        const styles = {
            position: 'fixed',
            top: '0',
            left: '0',
            right: '0',
            bottom: '0',
            width: '100vw',
            height: '100vh',
            zIndex: '1050',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            opacity: '1',
            visibility: 'visible'
        };

        Object.assign(modal.style, styles);
        
        // Force with !important to override CSS animations that cause blinking
        modal.style.setProperty('position', 'fixed', 'important');
        modal.style.setProperty('inset', '0', 'important');
        modal.style.setProperty('z-index', '1050', 'important');
        modal.style.setProperty('opacity', '1', 'important');
        modal.style.setProperty('visibility', 'visible', 'important');
        modal.style.setProperty('pointer-events', 'auto', 'important');
        
        console.log('[Modal Provider] Applied viewport-level positioning to modal');
    }

    setupScrollLock() {
        // Watch for modal provider state changes
        const observer = new MutationObserver(() => {
            const provider = document.querySelector('.modal-provider.active');
            if (provider) {
                this.lockBodyScroll();
            } else {
                this.unlockBodyScroll();
            }
        });

        observer.observe(document.body, { 
            childList: true, 
            subtree: true, 
            attributes: true, 
            attributeFilter: ['class'] 
        });

        this.scrollObserver = observer;
    }

    setupFocusManagement() {
        document.addEventListener('focusin', (event) => {
            const activeModal = this.getActiveModal();
            if (!activeModal) return;

            // If focus goes outside modal, bring it back
            if (!activeModal.contains(event.target)) {
                const focusableElement = this.getFocusableElements(activeModal)[0];
                if (focusableElement) {
                    focusableElement.focus();
                }
            }
        });
    }

    getActiveModal() {
        return document.querySelector('.modal--visible');
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

    lockBodyScroll() {
        if (document.body.classList.contains('modal-open')) return;
        
        // Store current scroll position
        this.scrollPosition = window.pageYOffset;
        
        // Lock scroll
        document.body.style.overflow = 'hidden';
        document.body.style.position = 'fixed';
        document.body.style.top = `-${this.scrollPosition}px`;
        document.body.style.width = '100%';
        
        document.body.classList.add('modal-open');
        
        console.log('[Modal Provider] Body scroll locked');
    }

    unlockBodyScroll() {
        // Only unlock if no visible modals exist
        const visibleModals = document.querySelectorAll('.modal--visible');
        if (visibleModals.length > 0) {
            console.log('[Modal Provider] Scroll unlock prevented - modals still visible:', visibleModals.length);
            return;
        }
        
        if (!document.body.classList.contains('modal-open')) return;
        
        // Restore all repositioned modals first
        this.restoreModalPositions();
        
        // Restore scroll position and unlock
        document.body.style.overflow = '';
        document.body.style.position = '';
        document.body.style.top = '';
        document.body.style.width = '';
        
        document.body.classList.remove('modal-open');
        
        // Restore scroll position
        if (this.scrollPosition) {
            window.scrollTo(0, this.scrollPosition);
            this.scrollPosition = 0;
        }
        
        console.log('[Modal Provider] Body scroll unlocked');
    }

    restoreModalPositions() {
        // Restore modal providers to original positions
        const modalProviders = document.querySelectorAll('.modal-provider');
        modalProviders.forEach(provider => {
            if (provider._originalParent && provider._originalParent !== document.body) {
                console.log('[Modal Provider] Restoring modal provider to original position');
                
                if (provider._originalNextSibling) {
                    provider._originalParent.insertBefore(provider, provider._originalNextSibling);
                } else {
                    provider._originalParent.appendChild(provider);
                }
                
                delete provider._originalParent;
                delete provider._originalNextSibling;
            }
        });
        
        // Restore repositioned modals
        this.repositionedModals.forEach(modal => {
            if (modal._originalParent && modal._originalParent !== document.body) {
                console.log('[Modal Provider] Restoring modal to original position');
                
                if (modal._originalNextSibling) {
                    modal._originalParent.insertBefore(modal, modal._originalNextSibling);
                } else {
                    modal._originalParent.appendChild(modal);
                }
                
                delete modal._originalParent;
                delete modal._originalNextSibling;
            }
        });
        
        this.repositionedModals.clear();
    }

    dispose() {
        this.modalObserver?.disconnect();
        this.scrollObserver?.disconnect();
        this.restoreModalPositions();
        this.unlockBodyScroll();
        
        console.log('[Modal Provider] JavaScript DOM repositioning system disposed');
    }
}

// Export for backwards compatibility
export const modalUtils = {
    copyToClipboard(text) {
        return navigator.clipboard?.writeText(text) || Promise.reject('Clipboard not available');
    },

    downloadContent(content, fileName, contentType = 'text/plain') {
        const blob = new Blob([content], { type: contentType });
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = fileName;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(url);
    }
};