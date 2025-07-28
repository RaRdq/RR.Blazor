// Create local debug logger to avoid circular imports
const debugLogger = {
    log: (...args) => {
        if (window.debugLogger?.isDebugMode || 
            localStorage.getItem('debug') === 'true' ||
            window.location.hostname === 'localhost') {
            console.log('[RR.Blazor]', ...args);
        }
    },
    warn: (...args) => {
        if (window.debugLogger?.isDebugMode || 
            localStorage.getItem('debug') === 'true' ||
            window.location.hostname === 'localhost') {
            console.warn('[RR.Blazor]', ...args);
        }
    },
    error: (...args) => {
        console.error('[RR.Blazor]', ...args);
    }
};

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
        try {
            const autosuggestElement = document.querySelector(`[data-autosuggest-id="${elementId}"]`);
            if (!autosuggestElement) {
                debugLogger.warn(`Autosuggest element not found: ${elementId}`);
                return null;
            }

            const triggerElement = autosuggestElement.querySelector('.autosuggest-input input');
            const viewportElement = autosuggestElement.querySelector('.autosuggest-viewport');

            if (!triggerElement || !viewportElement) {
                debugLogger.warn(`Autosuggest elements missing for: ${elementId}`);
                return null;
            }

            // Create portal using the same pattern as RChoice
            const portalId = await window.RRBlazor.Portal.create(viewportElement, {
                id: `autosuggest-${elementId}`,
                type: 'dropdown',
                anchor: triggerElement,
                className: 'autosuggest-portal',
                buffer: 8,
                minWidth: 200,
                zIndex: this.getContextualZIndex(autosuggestElement)
            });

            if (portalId) {
                viewportElement._portalId = portalId;
                autosuggestElement._portalId = portalId;
                
                // Store autosuggest data
                this.activeAutosuggest.set(elementId, {
                    portalId,
                    triggerElement,
                    viewportElement,
                    autosuggestElement,
                    options
                });

                // Observe trigger element for size changes
                if (this.resizeObserver) {
                    triggerElement.dataset.autosuggestId = elementId;
                    this.resizeObserver.observe(triggerElement);
                }

                // Setup event listeners
                this.setupEventListeners(elementId);

                // Position the portal
                window.RRBlazor.Portal.position(portalId);
                
                debugLogger.log(`Autosuggest portal created: ${portalId}`);
            }

            return portalId;
        } catch (error) {
            debugLogger.error(`Failed to create autosuggest portal for ${elementId}:`, error);
            return null;
        }
    }

    async destroyAutosuggestPortal(elementId) {
        const autosuggestData = this.activeAutosuggest.get(elementId);
        if (!autosuggestData) return;

        try {
            // Destroy portal using the same pattern as RChoice
            await window.RRBlazor.Portal.destroy(autosuggestData.portalId);

            // Clean up observers
            if (this.resizeObserver && autosuggestData.triggerElement) {
                this.resizeObserver.unobserve(autosuggestData.triggerElement);
                delete autosuggestData.triggerElement.dataset.autosuggestId;
            }

            // Remove event listeners
            this.removeEventListeners(elementId);

            // Remove from active map
            this.activeAutosuggest.delete(elementId);

            debugLogger.log(`Autosuggest portal destroyed: ${autosuggestData.portalId}`);
        } catch (error) {
            debugLogger.error(`Failed to destroy autosuggest portal for ${elementId}:`, error);
        }
    }

    async updatePosition(elementId) {
        const autosuggestData = this.activeAutosuggest.get(elementId);
        if (!autosuggestData) return;

        try {
            // Get portal manager
            const portalModule = await window.RRBlazor.moduleManager.getModule('portal');
            const portalManager = portalModule.portalManager || window.RRPortalManager;

            // Update portal position
            await portalManager.position(autosuggestData.portalId);
        } catch (error) {
            debugLogger.error(`Failed to update autosuggest position for ${elementId}:`, error);
        }
    }

    getContextualZIndex(element) {
        // Check if inside modal
        const modalAncestor = element.closest('.modal, [data-modal], .rr-modal');
        if (modalAncestor) {
            return 'var(--z-modal-popup)'; // 1100
        }
        return 'var(--z-popup)'; // 900
    }

    setupEventListeners(elementId) {
        const autosuggestData = this.activeAutosuggest.get(elementId);
        if (!autosuggestData) return;

        // Scroll handler for position updates
        const scrollHandler = () => this.updatePosition(elementId);
        window.addEventListener('scroll', scrollHandler, { passive: true });
        window.addEventListener('resize', scrollHandler, { passive: true });

        // Store handlers for cleanup
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

// Register click-outside callback for Blazor component
export function registerClickOutside(elementId, dotNetRef) {
    try {
        const autosuggest = document.querySelector(`[data-autosuggest-id="${elementId}"]`);
        const portalId = autosuggest?._portalId;
        
        if (portalId) {
            window.RRBlazor.Portal.update(portalId, {
                onClickOutside: () => dotNetRef.invokeMethodAsync('OnClickOutside')
            });
        }
    } catch (error) {
        debugLogger.error('[Autosuggest] Click-outside registration failed:', error);
    }
}

// Public API
export async function createAutosuggestPortal(elementId, options) {
    return autosuggestPositioning.createAutosuggestPortal(elementId, options);
}

export async function destroyAutosuggestPortal(portalId) {
    // Find elementId from portal ID for compatibility
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
    // Try direct portal destruction as fallback
    try {
        await window.RRBlazor.Portal.destroy(portalId);
    } catch (error) {
        debugLogger.error('[Autosuggest] Direct portal destruction failed:', error);
    }
}

export async function updateAutosuggestPosition(elementId) {
    return autosuggestPositioning.updatePosition(elementId);
}

export function getAutosuggestDirection(elementId) {
    return autosuggestPositioning.getCurrentDirection(elementId);
}

// Calculate optimal position like Choice does
export function calculateOptimalPosition(triggerElement, options = {}) {
    try {
        if (!triggerElement) return { direction: 'down', position: 'start' };
        
        const triggerRect = triggerElement.getBoundingClientRect();
        const viewportHeight = window.innerHeight;
        const dropdownHeight = options.estimatedHeight || 300;
        const buffer = options.buffer || 8;
        
        const spaceBelow = viewportHeight - triggerRect.bottom - buffer;
        const spaceAbove = triggerRect.top - buffer;
        
        const direction = spaceBelow < dropdownHeight && spaceAbove > dropdownHeight ? 'up' : 'down';
        
        return {
            direction,
            position: 'start',
            spaces: { above: spaceAbove, below: spaceBelow }
        };
    } catch (error) {
        debugLogger.error('[Autosuggest] calculateOptimalPosition error:', error);
        return { direction: 'down', position: 'start' };
    }
}

// Legacy compatibility
window.RRAutosuggest = {
    createPortal: createAutosuggestPortal,
    destroyPortal: destroyAutosuggestPortal,
    updatePosition: updateAutosuggestPosition,
    getDirection: getAutosuggestDirection,
    registerClickOutside: registerClickOutside,
    calculateOptimalPosition: calculateOptimalPosition
};

// Export for module usage
export default {
    createAutosuggestPortal,
    destroyAutosuggestPortal,
    updateAutosuggestPosition,
    getAutosuggestDirection,
    registerClickOutside,
    calculateOptimalPosition
};