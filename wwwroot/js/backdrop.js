
import { createSingleton, WeakRegistry } from './utils/singleton-factory.js';

class BackdropManagerBase {
    static #baseOpacity = 0.5;
    static #opacityIncrement = 0.1;
    static #maxOpacity = 0.9;
    
    #backdrops = new Map();
    #sharedBackdrops = new Map();
    #registry = new WeakRegistry();
    #animationDuration = 200;
    
    constructor() {
        this.#animationDuration = this.#getAnimationDuration();
    }
    
    create(portalId, config = {}) {
        if (this.#backdrops.has(portalId)) {
            throw new Error(`Backdrop for portal ${portalId} already exists`);
        }
        
        const level = config.level || 0;
        const useShared = config.shared !== false;
        
        let backdrop;
        
        if (useShared && this.#sharedBackdrops.has(level)) {
            backdrop = this.#sharedBackdrops.get(level);
            backdrop.refCount++;
        } else {
            backdrop = this.#createBackdropElement(portalId, level, config);
            
            if (useShared) {
                backdrop.refCount = 1;
                this.#sharedBackdrops.set(level, backdrop);
            }
        }
        
        const backdropData = {
            element: backdrop.element,
            level,
            shared: useShared,
            config
        };
        
        this.#backdrops.set(portalId, backdropData);
        this.#registry.register(portalId, backdropData);
        
        this.#animateIn(backdrop.element, config.animationDuration);
        
        return backdrop.element;
    }
    
    destroy(portalId) {
        const backdrop = this.#backdrops.get(portalId);
        if (!backdrop) {
            throw new Error(`Backdrop for portal ${portalId} not found`);
        }
        
        this.#backdrops.delete(portalId);
        this.#registry.delete(portalId);
        
        if (backdrop.shared) {
            const shared = this.#sharedBackdrops.get(backdrop.level);
            if (shared) {
                shared.refCount = Math.max(0, shared.refCount - 1);
                
                if (shared.refCount === 0) {
                    this.#sharedBackdrops.delete(backdrop.level);
                    this.#removeBackdropElement(shared.element, backdrop.config.animationDuration);
                }
            }
        } else {
            this.#removeBackdropElement(backdrop.element, backdrop.config.animationDuration);
        }
    }
    
    destroyAll() {
        const elementsToRemove = [];
        
        this.#backdrops.forEach((backdrop) => {
            if (!backdrop.shared && backdrop.element) {
                elementsToRemove.push(backdrop.element);
            }
        });
        
        this.#sharedBackdrops.forEach(shared => {
            if (shared.element) {
                elementsToRemove.push(shared.element);
            }
        });
        
        this.#backdrops.clear();
        this.#sharedBackdrops.clear();
        this.#registry.destroy();
        
        elementsToRemove.forEach(element => {
            if (element && element.parentNode) {
                element.remove();
            }
        });
    }
    
    updateOpacity(portalId, opacity) {
        const backdrop = this.#backdrops.get(portalId);
        if (!backdrop) {
            throw new Error(`[BackdropManager] Backdrop for portal ${portalId} not found`);
        }
        
        backdrop.element.style.setProperty('--backdrop-opacity', opacity);
    }
    
    onClick(portalId, handler) {
        const backdrop = this.#backdrops.get(portalId);
        if (!backdrop) {
            throw new Error(`[BackdropManager] Backdrop for portal ${portalId} not found`);
        }
        
        const clickHandler = (event) => {
            if (event.target === backdrop.element) {
                handler(event);
            }
        };
        
        backdrop.element.addEventListener('click', clickHandler);
        backdrop.element.dataset.clickHandler = 'true';
        
        return () => {
            backdrop.element.removeEventListener('click', clickHandler);
            delete backdrop.element.dataset.clickHandler;
        };
    }
    
    hasBackdrop(portalId) {
        return this.#backdrops.has(portalId);
    }
    
    getBackdrop(portalId) {
        const backdrop = this.#backdrops.get(portalId);
        if (!backdrop) {
            throw new Error(`[BackdropManager] Backdrop for portal ${portalId} not found`);
        }
        return backdrop;
    }
    
    
    #getAnimationDuration() {
        try {
            const rootStyles = getComputedStyle(document.documentElement);
            const durationValue = rootStyles.getPropertyValue('--duration-normal').trim();
            
            if (durationValue.endsWith('ms')) {
                return parseInt(durationValue, 10);
            } else if (durationValue.endsWith('s')) {
                return parseFloat(durationValue) * 1000;
            }
        } catch (error) {
            throw error;
        }
        return 200;
    }
    
    #createBackdropElement(portalId, level, config) {
        const backdrop = document.createElement('div');
        backdrop.className = `portal-backdrop ${config.className || ''}`.trim();
        backdrop.dataset.portalId = portalId;
        backdrop.dataset.backdropLevel = level.toString();
        
        const opacity = this.#calculateOpacity(level);
        backdrop.style.setProperty('--backdrop-opacity', opacity);
        backdrop.style.opacity = '0'; // Start invisible for animation
        
        if (config.blur) {
            backdrop.style.setProperty('--backdrop-blur', `${config.blur}px`);
        }
        
        let container = document.getElementById('portal-root');
        if (!container) {
            container = document.createElement('div');
            container.id = 'portal-root';
            container.className = 'portal-root';
            container.setAttribute('data-rr-blazor-portal', 'true');
            container.style.cssText = `
                position: fixed;
                top: 0;
                left: 0;
                width: 100vw;
                height: 100vh;
                z-index: 9999;
                pointer-events: none;
                overflow: hidden;
            `;
            document.body.appendChild(container);
        }
        
        const portal = document.getElementById(portalId);
        if (portal && portal.parentNode === container) {
            container.insertBefore(backdrop, portal);
        } else {
            container.appendChild(backdrop);
        }
        
        return { element: backdrop, refCount: 0 };
    }
    
    #calculateOpacity(level) {
        const opacity = Math.min(
            BackdropManagerBase.#baseOpacity + (level * BackdropManagerBase.#opacityIncrement),
            BackdropManagerBase.#maxOpacity
        );
        return opacity;
    }
    
    #animateIn(element, duration = this.#animationDuration) {
        element.style.transition = `opacity ${duration}ms ease-out`;
        
        requestAnimationFrame(() => {
            element.style.opacity = 'var(--backdrop-opacity, 0.5)';
        });
    }
    
    #removeBackdropElement(element, duration = this.#animationDuration) {
        if (!element.parentNode) {
            throw new Error('[BackdropManager] Element has no parent node');
        }
        
        if (element.dataset.removing === 'true') {
            return;
        }
        
        element.dataset.removing = 'true';
        
        if (duration > 0) {
            element.style.transition = `opacity ${duration}ms ease-in`;
            element.style.opacity = '0';
            
            element.addEventListener('transitionend', function onTransitionEnd() {
                if (element.parentNode) {
                    element.remove();
                }
            }, { once: true });
        } else {
            element.remove();
        }
    }
}

export const BackdropManager = createSingleton(BackdropManagerBase, 'BackdropManager');
// Defer event listener registration until RRBlazor.Events is available
function setupBackdropEventListeners() {
    if (!window.RRBlazor || !window.RRBlazor.Events) {
        setTimeout(setupBackdropEventListeners, 10);
        return;
    }
    
    document.addEventListener(window.RRBlazor.Events.BACKDROP_CREATE_REQUEST, (event) => {
        const { requesterId, config } = event.detail;
        const backdrop = BackdropManager.getInstance().create(requesterId, config);
        
        window.RRBlazor.EventDispatcher.dispatch(
            window.RRBlazor.Events.BACKDROP_CREATED,
            { requesterId, backdrop }
        );
    });
    
    document.addEventListener(window.RRBlazor.Events.BACKDROP_DESTROY_REQUEST, (event) => {
        const { requesterId } = event.detail;
        if (BackdropManager.getInstance().hasBackdrop(requesterId)) {
            BackdropManager.getInstance().destroy(requesterId);
            
            window.RRBlazor.EventDispatcher.dispatch(
                window.RRBlazor.Events.BACKDROP_DESTROYED,
                { requesterId }
            );
        }
    });
    
    document.addEventListener(window.RRBlazor.Events.BACKDROP_CLEANUP_ALL_REQUEST, () => {
        BackdropManager.getInstance().destroyAll();
        
        window.RRBlazor.EventDispatcher.dispatch(
            window.RRBlazor.Events.BACKDROP_ALL_DESTROYED
        );
    });
}

setupBackdropEventListeners();



window.addEventListener('beforeunload', () => {
    if (BackdropManager.hasInstance()) {
        BackdropManager.getInstance().destroyAll();
        BackdropManager.destroyInstance();
    }
});

export default BackdropManager;
export function createBackdrop(portalId, config) {
    return BackdropManager.getInstance().create(portalId, config);
}

export function destroyBackdrop(portalId) {
    return BackdropManager.getInstance().destroy(portalId);
}

export function hasBackdrop(portalId) {
    return BackdropManager.getInstance().hasBackdrop(portalId);
}

export function updateBackdropOpacity(portalId, opacity) {
    return BackdropManager.getInstance().updateOpacity(portalId, opacity);
}

export function onBackdropClick(portalId, handler) {
    return BackdropManager.getInstance().onClick(portalId, handler);
}