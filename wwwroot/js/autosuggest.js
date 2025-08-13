// Use RRBlazor proxy instead of direct imports for proper architecture

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

            // Create portal using RRBlazor proxy
            const portalManager = await window.RRBlazor.Portal.getInstance();
            const portalResult = portalManager.create({
                id: `autosuggest-${elementId}`,
                className: 'autosuggest-portal'
            });
            const portalId = portalResult.id;
            const portalContainer = portalResult.element;

            if (portalId && portalContainer) {
                // Store original position for restoration
                if (!viewportElement._originalParent) {
                    viewportElement._originalParent = viewportElement.parentNode;
                    viewportElement._originalNextSibling = viewportElement.nextSibling;
                }
                
                // Move viewport to portal
                portalContainer.appendChild(viewportElement);
                
                // Apply portal positioning
                this.positionPortal(portalContainer, triggerElement, options);
                
                viewportElement._portalId = portalId;
                autosuggestElement._portalId = portalId;
                
                // Store autosuggest data
                this.activeAutosuggest.set(elementId, {
                    portalId,
                    portalContainer,
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
            // Restore viewport to original position
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
            
            // Destroy portal using RRBlazor proxy
            const portalManager = await window.RRBlazor.Portal.getInstance();
            if (portalManager.isPortalActive(autosuggestData.portalId)) {
                portalManager.destroy(autosuggestData.portalId);
            }

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
            // Update portal position using direct positioning
            if (autosuggestData.portalContainer && autosuggestData.triggerElement) {
                this.positionPortal(autosuggestData.portalContainer, autosuggestData.triggerElement, autosuggestData.options);
            }
        } catch (error) {
            debugLogger.error(`Failed to update autosuggest position for ${elementId}:`, error);
        }
    }

    positionPortal(portalContainer, triggerElement, options = {}) {
        if (!portalContainer || !triggerElement) return;
        
        const triggerRect = triggerElement.getBoundingClientRect();
        const viewportHeight = window.innerHeight;
        const viewportWidth = window.innerWidth;
        
        // Calculate optimal position
        const buffer = options.buffer || 8;
        const minWidth = Math.max(options.minWidth || 200, triggerRect.width);
        const maxWidth = Math.min(400, viewportWidth - 16);
        const width = Math.min(minWidth, maxWidth);
        
        // Determine if dropdown should open up or down
        const spaceBelow = viewportHeight - triggerRect.bottom - buffer;
        const spaceAbove = triggerRect.top - buffer;
        const estimatedHeight = Math.min(options.maxHeight || 320, 320);
        
        let x = triggerRect.left;
        let y = triggerRect.bottom + buffer;
        
        // Adjust horizontal position if needed
        if (x + width > viewportWidth - buffer) {
            x = viewportWidth - width - buffer;
        }
        if (x < buffer) {
            x = buffer;
        }
        
        // Adjust vertical position if needed
        if (spaceBelow < estimatedHeight && spaceAbove > estimatedHeight) {
            y = triggerRect.top - estimatedHeight - buffer;
        }
        
        // Apply positioning
        portalContainer.style.position = 'fixed';
        portalContainer.style.left = `${x}px`;
        portalContainer.style.top = `${y}px`;
        portalContainer.style.width = `${width}px`;
        portalContainer.style.maxHeight = `${Math.min(estimatedHeight, Math.max(spaceAbove, spaceBelow))}px`;
        portalContainer.style.zIndex = this.getContextualZIndex(triggerElement);
        portalContainer.style.visibility = 'visible';
        portalContainer.style.opacity = '1';
        portalContainer.style.pointerEvents = 'auto';
        
        // Apply visual styling
        portalContainer.style.boxShadow = 'var(--shadow-lg)';
        portalContainer.style.borderRadius = 'var(--radius-md)';
        portalContainer.style.backgroundColor = 'var(--color-background-elevated)';
        portalContainer.style.border = '1px solid var(--color-border-subtle)';
    }

    getContextualZIndex(element) {
        // Check if inside modal
        const modalAncestor = element.closest('.modal, [data-modal], .rr-modal');
        if (modalAncestor) {
            return 'var(--z-modal-popup)'; // 1100
        }
        
        // Check if inside app shell header
        const appShellHeader = element.closest('.app-header, .header-right');
        if (appShellHeader) {
            return 'var(--z-header-popup, var(--z-popup))'; // 900+
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
            // Click outside handling is managed by Blazor component itself
            // No need for portal-level click outside handling
            debugLogger.log(`Click-outside registered for autosuggest: ${portalId}`);
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
        const portalManager = await window.RRBlazor.Portal.getInstance();
        if (portalManager.isPortalActive(portalId)) {
            portalManager.destroy(portalId);
        }
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
        
        // Check if in app header
        const isInHeader = triggerElement.closest('.app-header, .header-right');
        const headerHeight = isInHeader ? 64 : 0; // Standard header height
        
        const spaceBelow = viewportHeight - triggerRect.bottom - buffer;
        const spaceAbove = triggerRect.top - buffer - headerHeight;
        
        // Smart direction detection even in header
        const direction = (spaceBelow < dropdownHeight && spaceAbove > dropdownHeight) ? 'up' : 'down';
        
        return {
            direction,
            position: 'start',
            spaces: { above: spaceAbove, below: spaceBelow },
            maxHeight: isInHeader ? Math.min(dropdownHeight, spaceBelow - 8) : dropdownHeight
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

// Required methods for rr-blazor.js proxy system
export function initialize(element, dotNetRef) {
    // Autosuggest system initializes itself, return success
    return true;
}

export function cleanup(element) {
    if (element && element.hasAttribute('data-autosuggest-id')) {
        const elementId = element.getAttribute('data-autosuggest-id');
        destroyAutosuggestPortal(elementId).catch(error => {
            debugLogger.error('[Autosuggest] Cleanup failed:', error);
        });
    }
}

// Export for module usage
export default {
    createAutosuggestPortal,
    destroyAutosuggestPortal,
    updateAutosuggestPosition,
    getAutosuggestDirection,
    registerClickOutside,
    calculateOptimalPosition,
    initialize,
    cleanup
};