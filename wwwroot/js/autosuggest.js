class AutosuggestPositioning {
    constructor() {
        this.activeAutosuggest = new Map();
        this.resizeObserver = null;
        this.initializeResizeObserver();
    }

    initializeResizeObserver() {
        if (typeof ResizeObserver !== 'undefined') {
            this.resizeObserver = new ResizeObserver((entries) => {
                for (const entry of entries) {
                    const elementId = entry.target.dataset.autosuggestId;
                    if (elementId && this.activeAutosuggest.has(elementId)) {
                        this.updatePosition(elementId);
                    }
                }
            });
        }
    }

    async createAutosuggestPortal(elementId, options = {}) {
        const autosuggestElement = document.querySelector(`[data-autosuggest-id="${elementId}"]`);
        const triggerElement = autosuggestElement.querySelector('.autosuggest-input input');
        const viewportElement = autosuggestElement.querySelector('.autosuggest-viewport');

            const portalManager = await window.RRBlazor.Portal.getInstance();
            const portalResult = portalManager.create({
                id: `autosuggest-${elementId}`,
                className: 'autosuggest-portal'
            });
            const portalId = portalResult.id;
            const portalContainer = portalResult.element;

            if (portalId && portalContainer) {
                if (!viewportElement._originalParent) {
                    viewportElement._originalParent = viewportElement.parentNode;
                    viewportElement._originalNextSibling = viewportElement.nextSibling;
                }
                
                portalContainer.appendChild(viewportElement);
                this.positionPortal(portalContainer, triggerElement, options);
                
                viewportElement._portalId = portalId;
                autosuggestElement._portalId = portalId;
                
                this.activeAutosuggest.set(elementId, {
                    portalId,
                    portalContainer,
                    triggerElement,
                    viewportElement,
                    autosuggestElement,
                    options
                });

                if (this.resizeObserver) {
                    triggerElement.dataset.autosuggestId = elementId;
                    this.resizeObserver.observe(triggerElement);
                }

                this.setupEventListeners(elementId);
            }

            return portalId;
    }

    async destroyAutosuggestPortal(elementId) {
        const autosuggestData = this.activeAutosuggest.get(elementId);
        if (!autosuggestData) return;

        if (autosuggestData.viewportElement) {
            const viewport = autosuggestData.viewportElement;
            if (viewport._originalParent) {
                if (viewport._originalNextSibling) {
                    viewport._originalParent.insertBefore(viewport, viewport._originalNextSibling);
                } else {
                    viewport._originalParent.appendChild(viewport);
                }
                delete viewport._originalParent;
                delete viewport._originalNextSibling;
            }
        }
        
        const portalManager = await window.RRBlazor.Portal.getInstance();
        if (portalManager.isPortalActive(autosuggestData.portalId)) {
            portalManager.destroy(autosuggestData.portalId);
        }

        if (this.resizeObserver && autosuggestData.triggerElement) {
            this.resizeObserver.unobserve(autosuggestData.triggerElement);
            delete autosuggestData.triggerElement.dataset.autosuggestId;
        }

        this.removeEventListeners(elementId);
        window.RRBlazor.ClickOutside.unregister(`autosuggest-${elementId}`);
        this.activeAutosuggest.delete(elementId);
    }

    async updatePosition(elementId) {
        const autosuggestData = this.activeAutosuggest.get(elementId);
        if (!autosuggestData) return;

        if (autosuggestData.portalContainer && autosuggestData.triggerElement) {
            this.positionPortal(autosuggestData.portalContainer, autosuggestData.triggerElement, autosuggestData.options);
        }
    }

    positionPortal(portalContainer, triggerElement, options = {}) {
        const triggerRect = triggerElement.getBoundingClientRect();
        const viewportHeight = window.innerHeight;
        const viewportWidth = window.innerWidth;
        
        // Calculate optimal position
        const buffer = 8;
        const minWidth = Math.max(200, triggerRect.width);
        const maxWidth = Math.min(400, viewportWidth - 16);
        const width = Math.min(minWidth, maxWidth);
        
        const spaceBelow = viewportHeight - triggerRect.bottom - buffer;
        const spaceAbove = triggerRect.top - buffer;
        const estimatedHeight = Math.min(320, 320);
        
        let x = triggerRect.left;
        let y = triggerRect.bottom + buffer;
        
        if (x + width > viewportWidth - buffer) {
            x = viewportWidth - width - buffer;
        }
        if (x < buffer) {
            x = buffer;
        }
        
        if (spaceBelow < estimatedHeight && spaceAbove > estimatedHeight) {
            y = triggerRect.top - estimatedHeight - buffer;
        }
        
        portalContainer.style.position = 'fixed';
        portalContainer.style.left = `${x}px`;
        portalContainer.style.top = `${y}px`;
        portalContainer.style.width = `${width}px`;
        portalContainer.style.maxHeight = `${Math.min(estimatedHeight, Math.max(spaceAbove, spaceBelow))}px`;
        portalContainer.style.zIndex = this.getContextualZIndex(triggerElement);
        portalContainer.style.visibility = 'visible';
        portalContainer.style.opacity = '1';
        portalContainer.style.pointerEvents = 'auto';
        portalContainer.style.boxShadow = 'var(--shadow-lg)';
        portalContainer.style.borderRadius = 'var(--radius-md)';
        portalContainer.style.backgroundColor = 'var(--color-background-elevated)';
        portalContainer.style.border = '1px solid var(--color-border-subtle)';
    }

    getContextualZIndex(element) {
        if (element.closest('.modal, [data-modal], .rr-modal')) {
            return 'var(--z-modal-popup)';
        }
        
        if (element.closest('.app-header, .header-right')) {
            return 'var(--z-header-popup, var(--z-popup))';
        }
        
        return 'var(--z-popup)';
    }

    setupEventListeners(elementId) {
        const autosuggestData = this.activeAutosuggest.get(elementId);
        if (!autosuggestData) return;

        const scrollHandler = () => this.updatePosition(elementId);
        window.addEventListener('scroll', scrollHandler, { passive: true });
        window.addEventListener('resize', scrollHandler, { passive: true });
        autosuggestData.scrollHandler = scrollHandler;
    }

    removeEventListeners(elementId) {
        const autosuggestData = this.activeAutosuggest.get(elementId);
        if (!autosuggestData || !autosuggestData.scrollHandler) return;

        window.removeEventListener('scroll', autosuggestData.scrollHandler);
        window.removeEventListener('resize', autosuggestData.scrollHandler);
    }

    getCurrentDirection(elementId) {
        const autosuggestData = this.activeAutosuggest.get(elementId);
        if (!autosuggestData) return 'down';

        // Check current portal placement
        const portalElement = document.querySelector(`[data-portal-id="${autosuggestData.portalId}"]`);
        if (!portalElement) return 'down';

        const triggerRect = autosuggestData.triggerElement.getBoundingClientRect();
        const portalRect = portalElement.getBoundingClientRect();

        return portalRect.top < triggerRect.bottom ? 'up' : 'down';
    }
}

// Singleton instance
const autosuggestPositioning = new AutosuggestPositioning();


// Public API
export async function createAutosuggestPortal(elementId, options) {
    return autosuggestPositioning.createAutosuggestPortal(elementId, options);
}

export async function destroyAutosuggestPortal(portalId) {
    let elementId = null;
    for (const [key, data] of autosuggestPositioning.activeAutosuggest) {
        if (data.portalId === portalId) {
            elementId = key;
            break;
        }
    }
    if (elementId) {
        return autosuggestPositioning.destroyAutosuggestPortal(elementId);
    }
    
    const portalManager = await window.RRBlazor.Portal.getInstance();
    if (portalManager.isPortalActive(portalId)) {
        portalManager.destroy(portalId);
    }
}

export async function updateAutosuggestPosition(elementId) {
    return autosuggestPositioning.updatePosition(elementId);
}

export function getAutosuggestDirection(elementId) {
    return autosuggestPositioning.getCurrentDirection(elementId);
}

export function calculateOptimalPosition(triggerElement, options = {}) {
    const triggerRect = triggerElement.getBoundingClientRect();
    const viewportHeight = window.innerHeight;
    const dropdownHeight = options.estimatedHeight || 300;
    const buffer = options.buffer || 8;
    
    const isInHeader = triggerElement.closest('.app-header, .header-right');
    const headerHeight = isInHeader ? 64 : 0;
    
    const spaceBelow = viewportHeight - triggerRect.bottom - buffer;
    const spaceAbove = triggerRect.top - buffer - headerHeight;
    
    const direction = (spaceBelow < dropdownHeight && spaceAbove > dropdownHeight) ? 'up' : 'down';
    
    return {
        direction,
        position: 'start',
        spaces: { above: spaceAbove, below: spaceBelow },
        maxHeight: isInHeader ? Math.min(dropdownHeight, spaceBelow - 8) : dropdownHeight
    };
}

window.RRAutosuggest = {
    createPortal: createAutosuggestPortal,
    destroyPortal: destroyAutosuggestPortal,
    updatePosition: updateAutosuggestPosition,
    getDirection: getAutosuggestDirection,
    calculateOptimalPosition: calculateOptimalPosition
};

export function initialize(element, dotNetRef) {
    return true;
}

export function cleanup(element) {
    if (element && element.hasAttribute('data-autosuggest-id')) {
        const elementId = element.getAttribute('data-autosuggest-id');
        destroyAutosuggestPortal(elementId);
    }
}

export default {
    createAutosuggestPortal,
    destroyAutosuggestPortal,
    updateAutosuggestPosition,
    getAutosuggestDirection,
    calculateOptimalPosition,
    initialize,
    cleanup
};