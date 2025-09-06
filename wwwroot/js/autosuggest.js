class AutosuggestPortalManager {
    constructor() {
        this.activePortals = new Map();
        this.positioningEngine = window.RRBlazor?.Positioning?.getInstance?.() || null;
        this.viewportLocations = new Map();
    }

    async createPortal(autosuggestId, options = {}) {
        const autosuggestElement = document.querySelector(`[data-autosuggest-id="${autosuggestId}"]`);
        if (!autosuggestElement) {
            console.warn(`Autosuggest element not found: ${autosuggestId}`);
            return null;
        }

        const viewport = autosuggestElement.querySelector('.autosuggest-viewport');
        if (!viewport) {
            console.warn(`Viewport not found for autosuggest: ${autosuggestId}`);
            return null;
        }

        if (this.activePortals.has(autosuggestId)) {
            console.warn(`Portal already exists for: ${autosuggestId}`);
            return this.activePortals.get(autosuggestId).portalId;
        }

        try {
            const portalId = await new Promise((resolve) => {
                const requesterId = `autosuggest-${autosuggestId}`;
                
                const handlePortalCreated = (event) => {
                    if (event.detail.requesterId === requesterId) {
                        document.removeEventListener(window.RRBlazor.Events.PORTAL_CREATED, handlePortalCreated);
                        resolve(event.detail.portal.id);
                    }
                };
                
                document.addEventListener(window.RRBlazor.Events.PORTAL_CREATED, handlePortalCreated);
                
                window.RRBlazor.EventDispatcher.dispatch(
                    window.RRBlazor.Events.PORTAL_CREATE_REQUEST,
                    {
                        requesterId,
                        config: {
                            id: `autosuggest-portal-${autosuggestId}`,
                            className: 'autosuggest-portal'
                        }
                    }
                );
                
                setTimeout(() => resolve(null), 1000);
            });

            if (!portalId) {
                console.error('Portal creation timeout');
                return null;
            }

            const portal = document.getElementById(portalId);
            if (!portal) {
                console.error('Portal element not found:', portalId);
                return null;
            }
            
            viewport._originalParent = viewport.parentNode;
            viewport._originalNextSibling = viewport.nextSibling;
            
            viewport.style.position = '';
            viewport.style.top = '';
            viewport.style.left = '';
            viewport.style.visibility = '';
            
            portal.appendChild(viewport);
            this.viewportLocations.set(autosuggestId, portal);
            
            const triggerElement = autosuggestElement.querySelector('.autosuggest-input');
            const triggerRect = triggerElement.getBoundingClientRect();
            
            const position = this.calculatePosition(triggerRect, viewport, options);
            
            viewport.style.position = 'fixed';
            viewport.style.left = `${position.x}px`;
            viewport.style.top = `${position.y}px`;
            viewport.style.width = `${position.width}px`;
            viewport.style.maxHeight = `${position.maxHeight}px`;
            viewport.style.visibility = 'visible';
            viewport.style.opacity = '1';
            viewport.style.pointerEvents = 'auto';
            
            viewport.classList.remove('autosuggest-viewport-closed');
            viewport.classList.add('autosuggest-viewport-open');
            
            this.activePortals.set(autosuggestId, {
                portalId,
                viewport,
                triggerElement,
                autosuggestElement,
                options
            });
            
            this.setupEventHandlers(autosuggestId);
            
            return portalId;
        } catch (error) {
            console.error('Failed to create autosuggest portal:', error);
            return null;
        }
    }

    closeDropdown(autosuggestId) {
        const portalData = this.activePortals.get(autosuggestId);
        if (!portalData) return;

        const { portalId, viewport } = portalData;
        
        this.cleanupEventHandlers(autosuggestId);
        
        viewport.classList.remove('autosuggest-viewport-open');
        viewport.classList.add('autosuggest-viewport-closed');
        
        setTimeout(() => {
            viewport.style.visibility = 'hidden';
            viewport.style.position = 'absolute';
            viewport.style.top = '-9999px';
            viewport.style.left = '-9999px';
            
            if (viewport._originalParent) {
                if (viewport._originalNextSibling) {
                    viewport._originalParent.insertBefore(viewport, viewport._originalNextSibling);
                } else {
                    viewport._originalParent.appendChild(viewport);
                }
                delete viewport._originalParent;
                delete viewport._originalNextSibling;
            }
            
            window.RRBlazor.EventDispatcher.dispatch(
                window.RRBlazor.Events.PORTAL_DESTROY_REQUEST,
                { 
                    requesterId: `autosuggest-${autosuggestId}`,
                    portalId 
                }
            );
            
            this.viewportLocations.delete(autosuggestId);
            this.activePortals.delete(autosuggestId);
        }, 150);
    }

    calculatePosition(triggerRect, viewport, options = {}) {
        const viewportHeight = window.innerHeight;
        const viewportWidth = window.innerWidth;
        
        const dropdownHeight = Math.min(options.maxHeight || 320, viewportHeight * 0.4);
        const dropdownWidth = options.width || triggerRect.width;
        
        const spaceBelow = viewportHeight - triggerRect.bottom - 8;
        const spaceAbove = triggerRect.top - 8;
        
        let y, placement;
        if (spaceBelow >= dropdownHeight || spaceBelow > spaceAbove) {
            y = triggerRect.bottom + (options.offset || 4);
            placement = 'bottom';
        } else {
            y = triggerRect.top - dropdownHeight - (options.offset || 4);
            placement = 'top';
        }
        
        let x = triggerRect.left;
        if (x + dropdownWidth > viewportWidth - 8) {
            x = Math.max(8, viewportWidth - dropdownWidth - 8);
        }
        
        return {
            x,
            y,
            width: dropdownWidth,
            maxHeight: dropdownHeight,
            placement
        };
    }

    setupEventHandlers(autosuggestId) {
        const portalData = this.activePortals.get(autosuggestId);
        if (!portalData) return;

        const clickOutsideHandler = (event) => {
            if (event.detail?.elementId === `autosuggest-clickoutside-${autosuggestId}`) {
                this.closeDropdown(autosuggestId);
            }
        };

        const scrollHandler = () => {
            this.reposition(autosuggestId);
        };

        const resizeHandler = () => {
            this.reposition(autosuggestId);
        };

        portalData.clickOutsideHandler = clickOutsideHandler;
        portalData.scrollHandler = scrollHandler;
        portalData.resizeHandler = resizeHandler;

        document.addEventListener(window.RRBlazor.Events.CLICK_OUTSIDE, clickOutsideHandler);
        window.addEventListener('scroll', scrollHandler, { passive: true });
        window.addEventListener('resize', resizeHandler, { passive: true });

        window.RRBlazor.ClickOutside.register(
            `autosuggest-clickoutside-${autosuggestId}`,
            portalData.autosuggestElement,
            {
                excludeSelectors: [
                    '.autosuggest-viewport',
                    '.autosuggest-dropdown',
                    '.portal',
                    '[data-portal-positioned="true"]'
                ]
            }
        );
    }

    cleanupEventHandlers(autosuggestId) {
        const portalData = this.activePortals.get(autosuggestId);
        if (!portalData) return;

        if (portalData.clickOutsideHandler) {
            document.removeEventListener(window.RRBlazor.Events.CLICK_OUTSIDE, portalData.clickOutsideHandler);
        }
        if (portalData.scrollHandler) {
            window.removeEventListener('scroll', portalData.scrollHandler);
        }
        if (portalData.resizeHandler) {
            window.removeEventListener('resize', portalData.resizeHandler);
        }

        window.RRBlazor.ClickOutside.unregister(`autosuggest-clickoutside-${autosuggestId}`);
    }

    reposition(autosuggestId) {
        const portalData = this.activePortals.get(autosuggestId);
        if (!portalData) return;

        const { viewport, triggerElement, options } = portalData;
        const triggerRect = triggerElement.getBoundingClientRect();
        
        if (triggerRect.width === 0) {
            this.closeDropdown(autosuggestId);
            return;
        }

        const position = this.calculatePosition(triggerRect, viewport, options);
        
        viewport.style.left = `${position.x}px`;
        viewport.style.top = `${position.y}px`;
        viewport.style.width = `${position.width}px`;
    }

    destroyAll() {
        for (const autosuggestId of this.activePortals.keys()) {
            this.closeDropdown(autosuggestId);
        }
    }
}

const autosuggestManager = new AutosuggestPortalManager();

export function createPortal(autosuggestId, options) {
    return autosuggestManager.createPortal(autosuggestId, options);
}

export function closeDropdown(autosuggestId) {
    autosuggestManager.closeDropdown(autosuggestId);
}

export function repositionDropdown(autosuggestId) {
    autosuggestManager.reposition(autosuggestId);
}

export function destroyAllPortals() {
    autosuggestManager.destroyAll();
}

window.RRBlazor = window.RRBlazor || {};
window.RRBlazor.Autosuggest = {
    createPortal,
    closeDropdown,
    repositionDropdown,
    destroyAllPortals
};

export default {
    createPortal,
    closeDropdown,
    repositionDropdown,
    destroyAllPortals
};