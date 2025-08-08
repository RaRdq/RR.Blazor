// Single source of truth for all portal, popup, and positioning needs

class PortalStateMachine {
    static States = {
        IDLE: 'idle',
        CREATING: 'creating',
        OPEN: 'open',
        CLOSING: 'closing',
        DESTROYED: 'destroyed'
    };
    
    static ValidTransitions = {
        [PortalStateMachine.States.IDLE]: [PortalStateMachine.States.CREATING],
        [PortalStateMachine.States.CREATING]: [PortalStateMachine.States.OPEN, PortalStateMachine.States.DESTROYED],
        [PortalStateMachine.States.OPEN]: [PortalStateMachine.States.CLOSING],
        [PortalStateMachine.States.CLOSING]: [PortalStateMachine.States.DESTROYED],
        [PortalStateMachine.States.DESTROYED]: []
    };
    
    constructor(portalId) {
        this.portalId = portalId;
        this.state = PortalStateMachine.States.IDLE;
        this.mutex = new Mutex();
    }
    
    async transition(newState) {
        return await this.mutex.lock(async () => {
            const validTransitions = PortalStateMachine.ValidTransitions[this.state] || [];
            
            if (!validTransitions.includes(newState)) {
                throw new Error(`[PortalStateMachine] Invalid transition from ${this.state} to ${newState} for portal ${this.portalId}`);
            }
            
            const oldState = this.state;
            this.state = newState;
            return { oldState, newState };
        });
    }
    
    async getState() {
        return await this.mutex.lock(async () => this.state);
    }
    
    isInState(state) {
        return this.state === state;
    }
}

// Mutex for ensuring atomic operations
class Mutex {
    constructor() {
        this.locked = false;
        this.queue = [];
    }
    
    async lock(fn) {
        while (this.locked) {
            await new Promise(resolve => this.queue.push(resolve));
        }
        
        this.locked = true;
        try {
            return await fn();
        } finally {
            this.locked = false;
            const resolve = this.queue.shift();
            if (resolve) resolve();
        }
    }
}

// Queue for handling rapid portal operations
class PortalQueue {
    constructor() {
        this.operations = new Map();
        this.processing = new Set();
    }
    
    async enqueue(portalId, operation, fn) {
        // Cancel any pending operations for this portal
        if (this.operations.has(portalId)) {
            const pending = this.operations.get(portalId);
            if (pending.operation !== operation) {
                pending.cancelled = true;
            }
        }
        
        // Create new operation
        const op = {
            portalId,
            operation,
            fn,
            cancelled: false,
            timestamp: Date.now()
        };
        
        this.operations.set(portalId, op);
        
        // Process if not already processing
        if (!this.processing.has(portalId)) {
            await this.process(portalId);
        }
        
        return !op.cancelled;
    }
    
    async process(portalId) {
        this.processing.add(portalId);
        
        try {
            while (this.operations.has(portalId)) {
                const op = this.operations.get(portalId);
                this.operations.delete(portalId);
                
                if (!op.cancelled) {
                    try {
                        await op.fn();
                    } catch (error) {
                        console.error(`[PortalQueue] Operation ${op.operation} failed for portal ${portalId}:`, error);
                    }
                }
            }
        } finally {
            this.processing.delete(portalId);
        }
    }
}

// Backdrop manager for centralized backdrop handling
class BackdropManager {
    constructor(zIndexManager, portalManager) {
        this.backdrops = new Map();
        this.zIndexManager = zIndexManager;
        this.portalManager = portalManager;
        this.mutex = new Mutex();
    }
    
    async create(portalId, clickHandler) {
        return await this.mutex.lock(async () => {
            // Check if backdrop already exists
            if (this.backdrops.has(portalId)) {
                console.warn(`[BackdropManager] Backdrop for portal '${portalId}' already exists`);
                return this.backdrops.get(portalId).element;
            }
            
            // Create backdrop element
            const backdrop = document.createElement('div');
            backdrop.id = `backdrop-${portalId}`;
            backdrop.className = 'rr-portal-backdrop';
            backdrop.setAttribute('data-portal-id', portalId);
            
            // Get z-index
            const zIndex = this.zIndexManager.getNext('backdrop');
            backdrop.style.zIndex = zIndex;
            
            // Add click handler if provided (for modal backdrops)
            if (clickHandler) {
                backdrop.addEventListener('click', (event) => {
                    // Prevent propagation to elements behind
                    event.stopPropagation();
                    event.preventDefault();
                    clickHandler(event);
                });
            }
            
            // Add to DOM
            document.body.appendChild(backdrop);
            
            // Store reference
            this.backdrops.set(portalId, {
                element: backdrop,
                zIndex: zIndex,
                clickHandler: clickHandler
            });
            
            return backdrop;
        });
    }
    
    async remove(portalId) {
        return await this.mutex.lock(async () => {
            const backdrop = this.backdrops.get(portalId);
            
            if (!backdrop) {
                // Also check DOM for orphaned backdrops
                const orphaned = document.getElementById(`backdrop-${portalId}`);
                if (orphaned && orphaned.parentElement) {
                    orphaned.parentElement.removeChild(orphaned);
                    console.warn(`[BackdropManager] Removed orphaned backdrop for portal ${portalId}`);
                }
                return false;
            }
            
            // Remove from DOM
            if (backdrop.element && backdrop.element.parentElement) {
                backdrop.element.parentElement.removeChild(backdrop.element);
            }
            
            // Release z-index
            if (backdrop.zIndex) {
                this.zIndexManager.release('backdrop', backdrop.zIndex);
            }
            
            // Remove from map
            this.backdrops.delete(portalId);
            
            return true;
        });
    }
    
    async cleanup() {
        return await this.mutex.lock(async () => {
            // Find all backdrop elements in DOM
            const allBackdrops = document.querySelectorAll('.rr-portal-backdrop');
            let cleanedCount = 0;
            
            allBackdrops.forEach(backdrop => {
                const portalId = backdrop.getAttribute('data-portal-id');
                
                // Remove if not tracked OR if the associated portal no longer exists
                // OR if both backdrop and portal exist but portal is not in the active portals map
                const portalContainer = document.getElementById(`portal-${portalId}`);
                const isPortalTracked = this.portalManager ? this.portalManager.portals.has(portalId) : false;
                
                const shouldRemove = !this.backdrops.has(portalId) || 
                                    (portalId && !portalContainer) ||
                                    (portalId && portalContainer && !isPortalTracked);
                
                if (shouldRemove) {
                    try {
                        if (backdrop.parentElement) {
                            backdrop.parentElement.removeChild(backdrop);
                            cleanedCount++;
                            
                            // Release z-index if we can parse it
                            const zIndex = parseInt(backdrop.style.zIndex);
                            if (!isNaN(zIndex)) {
                                this.zIndexManager.release('backdrop', zIndex);
                            }
                            
                            // Remove from backdrops map if it exists
                            if (this.backdrops.has(portalId)) {
                                this.backdrops.delete(portalId);
                            }
                        }
                    } catch (e) {
                        console.warn(`[BackdropManager] Failed to remove orphaned backdrop: ${e.message}`);
                    }
                }
            });
            
            if (cleanedCount > 0) {
                console.warn(`[BackdropManager] Cleaned up ${cleanedCount} orphaned backdrop(s)`);
            }
            
            return cleanedCount;
        });
    }
}

class PortalManager {
    constructor() {
        this.portals = new Map();
        this.stateMachines = new Map();
        this.zIndexManager = new ZIndexManager();
        this.backdropManager = new BackdropManager(this.zIndexManager, this);
        this.positioningEngine = new PositioningEngine();
        this.eventManager = new PortalEventManager();
        this.queue = new PortalQueue();
        this.mutex = new Mutex();
    }
    
    // Create a portal with unified options and atomic operations
    async create(element, options = {}) {
        if (!element) {
            console.warn('[PortalManager] No element provided');
            return null;
        }
        
        const portalId = options.id || `portal-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
        
        // Create state machine for this portal
        const stateMachine = new PortalStateMachine(portalId);
        this.stateMachines.set(portalId, stateMachine);
        
        // Queue the creation operation
        const result = await this.queue.enqueue(portalId, 'create', async () => {
            return await this._createPortal(element, options, portalId, stateMachine);
        });
        
        return result ? portalId : null;
    }
    
    async _createPortal(element, options, portalId, stateMachine) {
        try {
            // Transition to CREATING state
            await stateMachine.transition(PortalStateMachine.States.CREATING);
            
            const targetContainer = this.resolveTarget(options.target || 'body');
            
            if (!targetContainer) {
                console.warn('[PortalManager] Target container not found');
                await stateMachine.transition(PortalStateMachine.States.DESTROYED);
                return false;
            }
            
            // Store original position for restoration
            const originalState = {
                parent: element.parentElement,
                nextSibling: element.nextSibling,
                display: element.style.display,
                position: element.style.position
            };
            
            // Create portal container
            const portalContainer = this.createPortalContainer(portalId, options);
            
            // Handle different portal types
            const portalType = options.type || 'generic';
            let backdropElement = null;
            
            switch (portalType) {
                case 'tooltip':
                    // Clone content for tooltips to preserve styling
                    const clonedContent = element.cloneNode(true);
                    portalContainer.appendChild(clonedContent);
                    element.style.display = 'none'; // Hide original
                    break;
                case 'modal':
                    // Modals get special backdrop handling
                    if (options.backdrop !== false) {
                        // Create click handler for modal backdrop
                        const backdropClickHandler = options.dotNetRef && options.backdropCallbackMethod ? 
                            async () => {
                                try {
                                    await options.dotNetRef.invokeMethodAsync(options.backdropCallbackMethod);
                                } catch (error) {
                                    console.warn('[PortalManager] Backdrop click callback failed:', error);
                                }
                            } : null;
                        
                        backdropElement = await this.backdropManager.create(portalId, backdropClickHandler);
                    }
                    portalContainer.appendChild(element);
                    break;
                default:
                    // Standard portal - move element
                    portalContainer.appendChild(element);
            }
            
            // Apply z-index with context awareness
            const isInsideModal = !!element.closest('.modal-content, [role="dialog"]');
            const context = isInsideModal ? 'modal' : null;
            const zIndex = this.zIndexManager.getNext(portalType, context);
            portalContainer.style.zIndex = zIndex;
            
            // Append to target
            targetContainer.appendChild(portalContainer);
            
            // Create portal instance
            const portal = {
                id: portalId,
                element,
                container: portalContainer,
                type: portalType,
                originalState,
                zIndex,
                options,
                anchor: options.anchor,
                stateMachine,
                backdropElement
            };
            
            this.portals.set(portalId, portal);
            
            // Setup positioning if anchor provided (but NOT for modals)
            if (options.anchor && portalType !== 'modal') {
                this.position(portalId);
                
                // Track scroll events to reposition
                const scrollHandler = () => {
                    this.position(portalId);
                };
                
                // Track scroll on window and all parent elements
                window.addEventListener('scroll', scrollHandler, true);
                portal.scrollHandler = scrollHandler;
                
                // Also track resize events
                const resizeHandler = () => {
                    this.position(portalId);
                };
                
                window.addEventListener('resize', resizeHandler);
                portal.resizeHandler = resizeHandler;
            }
            
            // Setup event handlers
            if (options.onClickOutside || options.closeOnClickOutside) {
                const callbackMethod = options.backdropCallbackMethod || 'HandleClickOutside';
                this.eventManager.setupClickOutside(portal, options.onClickOutside, options.dotNetRef, callbackMethod);
            }
            
            if (options.onEscape || options.closeOnEscape) {
                const callbackMethod = options.escapeCallbackMethod || 'HandleEscape';
                this.eventManager.setupEscape(portal, options.onEscape, options.dotNetRef, callbackMethod);
            }
            
            // Transition to OPEN state
            await stateMachine.transition(PortalStateMachine.States.OPEN);
            
            return true;
        } catch (error) {
            console.error(`[PortalManager] Failed to create portal ${portalId}:`, error);
            
            // Try to transition to DESTROYED state
            try {
                await stateMachine.transition(PortalStateMachine.States.DESTROYED);
            } catch (e) {
                // Ignore transition errors during cleanup
            }
            
            return false;
        }
    }
    
    // Update portal position
    position(portalId) {
        const portal = this.portals.get(portalId);
        if (!portal || !portal.anchor) return;
        
        const elementToMeasure = portal.element || portal.container;
        
        const position = this.positioningEngine.calculate(
            portal.anchor, 
            elementToMeasure,
            portal.options
        );
        
        this.positioningEngine.apply(portal.container, position);
        
        // Add position classes for CSS hooks
        const classes = this.positioningEngine.getPositionClasses(position);
        portal.container.className = `rr-portal rr-portal-${portal.type} ${classes} ${portal.options.className || ''}`.trim();
    }
    
    // Destroy portal and restore element with atomic operations
    async destroy(portalId) {
        const portal = this.portals.get(portalId);
        if (!portal) {
            // Don't log for modal-auto-* portals as these are common during cleanup
            if (!portalId.startsWith('modal-auto-')) {
                console.warn(`[PortalManager] Portal '${portalId}' not found`);
            }
            return false;
        }
        
        const stateMachine = portal.stateMachine || this.stateMachines.get(portalId);
        if (!stateMachine) {
            console.warn(`[PortalManager] No state machine for portal '${portalId}'`);
            return false;
        }
        
        // Queue the destruction operation
        return await this.queue.enqueue(portalId, 'destroy', async () => {
            return await this._destroyPortal(portal, portalId, stateMachine);
        });
    }
    
    async _destroyPortal(portal, portalId, stateMachine) {
        try {
            // Check current state
            const currentState = await stateMachine.getState();
            if (currentState === PortalStateMachine.States.DESTROYED) {
                return false;
            }
            
            if (currentState === PortalStateMachine.States.CLOSING) {
                // Already closing, wait for it to complete
                return false;
            }
            
            // Transition to CLOSING state
            await stateMachine.transition(PortalStateMachine.States.CLOSING);
            
            // Clean up event handlers
            this.eventManager.cleanup(portal);
            
            // Clean up scroll and resize handlers
            if (portal.scrollHandler) {
                try {
                    window.removeEventListener('scroll', portal.scrollHandler, true);
                } catch (e) {
                    console.warn(`[PortalManager] Failed to remove scroll handler: ${e.message}`);
                }
            }
            if (portal.resizeHandler) {
                try {
                    window.removeEventListener('resize', portal.resizeHandler);
                } catch (e) {
                    console.warn(`[PortalManager] Failed to remove resize handler: ${e.message}`);
                }
            }
            
            // Remove backdrop if exists
            await this.backdropManager.remove(portalId);
            
            // Restore element to original position
            if (portal.originalState.parent && portal.element) {
                try {
                    if (portal.type === 'tooltip') {
                        // For tooltips, just show the original element
                        portal.element.style.display = portal.originalState.display;
                    } else {
                        // Move element back
                        if (portal.originalState.nextSibling) {
                            portal.originalState.parent.insertBefore(portal.element, portal.originalState.nextSibling);
                        } else {
                            portal.originalState.parent.appendChild(portal.element);
                        }
                        
                        // Restore styles
                        portal.element.style.display = portal.originalState.display;
                        portal.element.style.position = portal.originalState.position;
                    }
                } catch (e) {
                    console.warn(`[PortalManager] Failed to restore element position: ${e.message}`);
                }
            }
            
            // Remove portal container
            if (portal.container && portal.container.parentElement) {
                try {
                    portal.container.parentElement.removeChild(portal.container);
                } catch (e) {
                    console.warn(`[PortalManager] Failed to remove container: ${e.message}`);
                }
            }
            
            // Release z-index with context
            const isInsideModal = portal.stateMachine ? 
                !!portal.element?.closest('.modal-content, [role="dialog"]') : false;
            const context = isInsideModal ? 'modal' : null;
            this.zIndexManager.release(portal.type, portal.zIndex, context);
            
            // Transition to DESTROYED state
            await stateMachine.transition(PortalStateMachine.States.DESTROYED);
            
            return true;
        } catch (error) {
            console.error(`[PortalManager] Failed to destroy portal ${portalId}:`, error);
            return false;
        } finally {
            // Always clean up references even if other operations fail
            this.portals.delete(portalId);
            this.stateMachines.delete(portalId);
            
            // Run orphaned element cleanup as a safety net
            await this.cleanupOrphanedElements();
        }
    }
    
    // Update portal options
    update(portalId, options) {
        const portal = this.portals.get(portalId);
        if (!portal) return false;
        
        portal.options = { ...portal.options, ...options };
        
        if (options.anchor && portal.type !== 'modal') {
            portal.anchor = options.anchor;
            this.position(portalId);
        }
        
        // Update event handlers if provided
        if (options.onClickOutside) {
            const callbackMethod = options.backdropCallbackMethod || 'HandleClickOutside';
            this.eventManager.setupClickOutside(portal, options.onClickOutside, options.dotNetRef, callbackMethod);
        }
        
        if (options.onEscape) {
            const callbackMethod = options.escapeCallbackMethod || 'HandleEscape';
            this.eventManager.setupEscape(portal, options.onEscape, options.dotNetRef, callbackMethod);
        }
        
        return true;
    }
    
    // Helper methods
    resolveTarget(target) {
        if (typeof target === 'string') {
            return document.querySelector(target);
        }
        return target;
    }
    
    createPortalContainer(portalId, options) {
        const container = document.createElement('div');
        container.id = `portal-${portalId}`;
        container.className = `rr-portal rr-portal-${options.type || 'generic'} ${options.className || ''}`.trim();
        
        if (options.style) {
            container.setAttribute('style', options.style);
        }
        
        // Transfer data attributes
        if (options.attributes) {
            Object.entries(options.attributes).forEach(([key, value]) => {
                container.setAttribute(`data-${key}`, value);
            });
        }
        
        return container;
    }
    
    // Legacy methods for backward compatibility - now delegate to BackdropManager
    async createBackdrop(portalId, clickHandler) {
        return await this.backdropManager.create(portalId, clickHandler);
    }
    
    async removeBackdrop(portalId) {
        return await this.backdropManager.remove(portalId);
    }
    
    // Get all portals of a specific type
    getByType(type) {
        return Array.from(this.portals.values()).filter(p => p.type === type);
    }
    
    // Check if element is in a portal
    isInPortal(element) {
        return element?.closest('.rr-portal') !== null;
    }
    
    // Check if there are open portals within a container (e.g., modal)
    hasOpenPortalsInContainer(containerSelector) {
        const container = document.querySelector(containerSelector);
        if (!container) return false;
        
        // Check for any open portals that are children of this container
        for (const [portalId, portal] of this.portals.entries()) {
            const stateMachine = this.stateMachines.get(portalId);
            const isOpen = stateMachine ? 
                stateMachine.state === PortalStateMachine.States.OPEN : 
                (portal.isOpen !== false);
            
            if (isOpen && container.contains(portal.container)) {
                return true;
            }
        }
        
        // Also check for any portal containers in DOM
        const portalElements = container.querySelectorAll('.rr-portal:not(.rr-portal-modal)');
        return portalElements.length > 0;
    }
    
    // Cleanup method to remove all orphaned elements
    async cleanupOrphanedElements() {
        // Clean up orphaned backdrops
        const backdropCleanupCount = await this.backdropManager.cleanup();
        
        // Clean up orphaned portal containers
        const allPortals = document.querySelectorAll('.rr-portal');
        allPortals.forEach(portal => {
            const portalId = portal.id.replace('portal-', '');
            
            if (!this.portals.has(portalId)) {
                try {
                    if (portal.parentElement) {
                        portal.parentElement.removeChild(portal);
                    }
                    // Only log for non-modal-auto portals to reduce noise
                    if (!portal.id.includes('modal-auto-')) {
                        console.warn(`[PortalManager] Removed orphaned portal: ${portal.id}`);
                    }
                } catch (e) {
                    console.warn(`[PortalManager] Failed to remove orphaned portal: ${e.message}`);
                }
            }
        });
        
        // Clean up any visible modal elements that don't belong to active portals
        const orphanedModals = document.querySelectorAll('.modal-content:not([style*="display: none"])');
        orphanedModals.forEach(modal => {
            const portalContainer = modal.closest('.rr-portal');
            if (!portalContainer) {
                // Modal not in a portal container - likely orphaned
                try {
                    if (modal.parentElement) {
                        modal.parentElement.removeChild(modal);
                    }
                    console.warn(`[PortalManager] Removed orphaned modal: ${modal.className}`);
                } catch (e) {
                    console.warn(`[PortalManager] Failed to remove orphaned modal: ${e.message}`);
                }
            }
        });
    }
    
    // Legacy method for backward compatibility
    cleanupOrphanedBackdrops() {
        return this.cleanupOrphanedElements();
    }
}

// Z-Index management with CSS variable integration and recycling
class ZIndexManager {
    constructor() {
        this.counters = new Map();
        this.freeIndices = new Map(); // Track free indices for recycling
        this.usedIndices = new Map(); // Track which indices are in use
        this.bases = {};
        this.modalContextBases = {};
        
        // Maximum indices per type before recycling
        this.maxPerType = 100;
        
        // Load z-index values from CSS variables
        this.loadFromCSS();
    }
    
    loadFromCSS() {
        const root = document.documentElement;
        const getVar = (name, fallback) => {
            const value = getComputedStyle(root).getPropertyValue(name).trim();
            return value ? parseInt(value) : fallback;
        };
        
        // Load base z-index values from CSS variables
        // From _variables.scss:
        // --z-popup: 900
        // --z-modal-backdrop: 1000
        // --z-modal-content: 1040
        // --z-modal-popup: 1100 (for dropdowns inside modals)
        // --z-tooltip: 1220
        // --z-notification: 1230
        
        this.bases.backdrop = getVar('--z-modal-backdrop', 1000);
        this.bases.dropdown = getVar('--z-popup', 900);
        this.bases.tooltip = getVar('--z-tooltip', 1220);
        this.bases.modal = getVar('--z-modal-content', 1040);
        this.bases.notification = getVar('--z-notification', 1230);
        this.bases.generic = getVar('--z-popup', 900);
        this.bases.datepicker = getVar('--z-popup', 900);
        this.bases.choice = getVar('--z-popup', 900);
        
        // Context-aware z-index for portals inside modals
        this.modalContextBases.dropdown = getVar('--z-modal-popup', 1100);
        this.modalContextBases.tooltip = getVar('--z-tooltip', 1220);
        this.modalContextBases.datepicker = getVar('--z-modal-popup', 1100);
        this.modalContextBases.choice = getVar('--z-modal-popup', 1100);
        this.modalContextBases.generic = getVar('--z-modal-popup', 1100);
    }
    
    getNext(type, context = null) {
        // Check if this portal is inside a modal
        let base = this.bases[type] || this.bases.generic;
        
        if (context === 'modal' && this.modalContextBases[type]) {
            // Use higher base for portals inside modals
            base = this.modalContextBases[type];
        }
        
        // Check for free indices to recycle
        const contextKey = context ? `${type}_${context}` : type;
        if (!this.freeIndices.has(contextKey)) {
            this.freeIndices.set(contextKey, []);
        }
        const freeList = this.freeIndices.get(contextKey);
        
        if (freeList.length > 0) {
            // Recycle a free index
            const recycledIndex = freeList.shift();
            this.markAsUsed(contextKey, recycledIndex);
            return recycledIndex;
        }
        
        // No free indices, generate new one
        const count = this.counters.get(contextKey) || 0;
        
        // Reset counter if we've reached max to prevent overflow
        if (count >= this.maxPerType) {
            this.resetType(contextKey);
            return base;
        }
        
        this.counters.set(contextKey, count + 1);
        const newIndex = base + count;
        this.markAsUsed(contextKey, newIndex);
        return newIndex;
    }
    
    release(type, zIndex, context = null) {
        const contextKey = context ? `${type}_${context}` : type;
        
        if (!this.freeIndices.has(contextKey)) {
            this.freeIndices.set(contextKey, []);
        }
        
        // Mark index as free for recycling
        const freeList = this.freeIndices.get(contextKey);
        if (!freeList.includes(zIndex)) {
            freeList.push(zIndex);
            // Sort to prefer lower indices
            freeList.sort((a, b) => a - b);
        }
        
        // Remove from used indices
        this.markAsUnused(contextKey, zIndex);
        
        // If no indices are in use, reset the counter
        const usedForType = this.usedIndices.get(contextKey) || new Set();
        if (usedForType.size === 0) {
            this.counters.set(contextKey, 0);
            this.freeIndices.set(contextKey, []);
        }
    }
    
    markAsUsed(type, index) {
        if (!this.usedIndices.has(type)) {
            this.usedIndices.set(type, new Set());
        }
        this.usedIndices.get(type).add(index);
    }
    
    markAsUnused(type, index) {
        if (this.usedIndices.has(type)) {
            this.usedIndices.get(type).delete(index);
        }
    }
    
    resetType(type) {
        this.counters.set(type, 0);
        this.freeIndices.set(type, []);
        this.usedIndices.set(type, new Set());
    }
}

// Positioning engine that consolidates logic from popup-manager
class PositioningEngine {
    calculate(anchor, popup, options = {}) {
        if (!anchor || !popup) return { top: 0, left: 0 };
        
        const anchorRect = anchor.getBoundingClientRect();
        const viewportWidth = window.innerWidth;
        const viewportHeight = window.innerHeight;
        const buffer = options.buffer || 8;
        
        // Get popup dimensions
        let popupWidth = options.width || popup.offsetWidth || 250;
        let popupHeight = options.height || this.getMeasuredHeight(popup) || 300;
        
        // For dropdowns, match trigger width
        if (options.type === 'dropdown' && !options.width) {
            popupWidth = Math.max(anchorRect.width, options.minWidth || 200);
        }
        
        // Calculate available spaces
        const spaces = {
            above: anchorRect.top - buffer,
            below: viewportHeight - anchorRect.bottom - buffer,
            left: anchorRect.left - buffer,
            right: viewportWidth - anchorRect.right - buffer
        };
        
        // Determine optimal vertical position
        const vertical = spaces.below >= popupHeight ? 'below' : 
                        spaces.above >= popupHeight ? 'above' :
                        spaces.above > spaces.below ? 'above' : 'below';
        
        // Determine horizontal position
        const anchorCenter = anchorRect.left + anchorRect.width / 2;
        let horizontal = 'start';
        
        if (anchorCenter - popupWidth / 2 >= buffer && 
            anchorCenter + popupWidth / 2 <= viewportWidth - buffer) {
            horizontal = 'center';
        } else if (anchorRect.right > viewportWidth * 0.7) {
            horizontal = 'end';
        } else if (spaces.right < popupWidth && spaces.left > popupWidth) {
            horizontal = 'end';
        }
        
        // Calculate position
        const top = vertical === 'above' ? 
            Math.max(buffer, anchorRect.top - popupHeight - 4) :
            Math.min(anchorRect.bottom + 4, viewportHeight - popupHeight - buffer);
            
        const left = horizontal === 'center' ? anchorCenter - popupWidth / 2 :
                    horizontal === 'end' ? anchorRect.right - popupWidth :
                    anchorRect.left;
        
        return {
            top: Math.round(Math.max(buffer, Math.min(top, viewportHeight - popupHeight - buffer))),
            left: Math.round(Math.max(buffer, Math.min(left, viewportWidth - popupWidth - buffer))),
            vertical,
            horizontal,
            width: popupWidth,
            height: popupHeight
        };
    }
    
    getMeasuredHeight(popup) {
        let height = popup.offsetHeight || popup.clientHeight;
        
        // Force measurement for hidden elements
        if (!height) {
            const { display, visibility, position } = popup.style;
            popup.style.display = 'block';
            popup.style.visibility = 'hidden';
            popup.style.position = 'absolute';
            
            height = popup.offsetHeight || popup.clientHeight || popup.scrollHeight;
            
            popup.style.display = display;
            popup.style.visibility = visibility;
            popup.style.position = position;
        }
        
        return height;
    }
    
    apply(element, position) {
        if (!element) return;
        
        element.style.position = 'fixed';
        element.style.top = `${position.top}px`;
        element.style.left = `${position.left}px`;
        
        if (position.width) {
            element.style.width = `${position.width}px`;
        }
        
        if (position.maxHeight) {
            element.style.maxHeight = `${position.maxHeight}px`;
        }
    }
    
    getPositionClasses(position) {
        return `portal-${position.vertical} portal-${position.horizontal}`;
    }
}

// Event management
class PortalEventManager {
    constructor() {
        this.handlers = new Map();
        this.globalClickHandler = null;
        this.globalEscapeHandler = null;
        this.setupGlobalHandlers();
    }
    
    setupGlobalHandlers() {
        // Global click handler
        this.globalClickHandler = (event) => {
            this.handlers.forEach((handler, portalId) => {
                if (handler.clickOutside && !handler.isRecentlyCreated) {
                    const portal = handler.portal;
                    if (!portal.container.contains(event.target) && 
                        (!portal.anchor || !portal.anchor.contains(event.target))) {
                        handler.clickOutside(event);
                    }
                }
            });
        };
        
        // Global escape handler
        this.globalEscapeHandler = (event) => {
            if (event.key === 'Escape') {
                // Collect all portals with escape handlers
                const activePortals = [];
                
                this.handlers.forEach((handler, portalId) => {
                    if (handler.escape && handler.portal.isOpen) {
                        const zIndex = parseInt(handler.portal.container.style.zIndex || '0');
                        activePortals.push({ handler, zIndex, portalId, type: handler.portal.type });
                    }
                });
                
                // Sort by z-index descending (highest first)
                activePortals.sort((a, b) => b.zIndex - a.zIndex);
                
                // Close the topmost non-modal portal first, or the topmost modal if no other portals
                for (const portal of activePortals) {
                    // Skip modals if there are non-modal portals above them
                    if (portal.type === 'modal' && 
                        activePortals.some(p => p.zIndex > portal.zIndex && p.type !== 'modal')) {
                        continue;
                    }
                    
                    // Close this portal and stop
                    portal.handler.escape(event);
                    event.preventDefault();
                    event.stopPropagation();
                    break;
                }
            }
        };
        
        document.addEventListener('click', this.globalClickHandler, true);
        document.addEventListener('keydown', this.globalEscapeHandler, true);
    }
    
    setupClickOutside(portal, callback, dotNetRef, callbackMethod) {
        const handler = this.handlers.get(portal.id) || {};
        handler.portal = portal;
        
        // If dotNetRef is provided, create a callback that invokes the method
        if (dotNetRef && callbackMethod) {
            handler.clickOutside = async () => {
                try {
                    await dotNetRef.invokeMethodAsync(callbackMethod);
                } catch (error) {
                    console.warn(`[PortalEventManager] Failed to invoke ${callbackMethod}:`, error);
                }
            };
        } else {
            handler.clickOutside = callback || (() => {
                // Default: trigger custom event
                const event = new CustomEvent('portalclickoutside', {
                    detail: { portalId: portal.id },
                    bubbles: true
                });
                portal.element.dispatchEvent(event);
            });
        }
        
        handler.isRecentlyCreated = true;
        const protectionDelay = 50;
        setTimeout(() => {
            if (this.handlers.has(portal.id)) {
                handler.isRecentlyCreated = false;
            }
        }, protectionDelay);
        
        this.handlers.set(portal.id, handler);
    }
    
    setupEscape(portal, callback, dotNetRef, callbackMethod) {
        const handler = this.handlers.get(portal.id) || {};
        handler.portal = portal;
        
        // If dotNetRef is provided, create a callback that invokes the method
        if (dotNetRef && callbackMethod) {
            handler.escape = async () => {
                try {
                    await dotNetRef.invokeMethodAsync(callbackMethod);
                } catch (error) {
                    console.warn(`[PortalEventManager] Failed to invoke ${callbackMethod}:`, error);
                }
            };
        } else {
            handler.escape = callback || (() => {
                // Default: trigger custom event
                const event = new CustomEvent('portalescape', {
                    detail: { portalId: portal.id },
                    bubbles: true
                });
                portal.element.dispatchEvent(event);
            });
        }
        this.handlers.set(portal.id, handler);
    }
    
    cleanup(portal) {
        this.handlers.delete(portal.id);
    }
    
    dispose() {
        try {
            if (this.globalClickHandler) {
                document.removeEventListener('click', this.globalClickHandler, true);
            }
            if (this.globalEscapeHandler) {
                document.removeEventListener('keydown', this.globalEscapeHandler, true);
            }
        } finally {
            // Always clear handlers even if removal fails
            this.handlers.clear();
            this.globalClickHandler = null;
            this.globalEscapeHandler = null;
        }
    }
}

// Create singleton instance
const portalManager = new PortalManager();

// Export for ES6 modules
export { portalManager, PortalManager, ZIndexManager, PositioningEngine, PortalEventManager };

// Export functions for simplified API
export function createPortal(element, target, id, className, style, options) {
    return portalManager.create(element, {
        target,
        id,
        className,
        style,
        ...options
    });
}

export function cleanupPortal(portalId) {
    return portalManager.destroy(portalId);
}

export function updatePortal(portalId, options) {
    return portalManager.update(portalId, options);
}

export function positionPortal(portalId, anchor, options) {
    return portalManager.update(portalId, { anchor, ...options });
}

// Window exports for RRBlazor integration
window.RRPortalManager = portalManager;
window.RRPortal = {
    create: (element, options) => portalManager.create(element, options),
    destroy: (portalId) => portalManager.destroy(portalId),
    update: (portalId, options) => portalManager.update(portalId, options),
    position: (portalId) => portalManager.position(portalId),
    isInPortal: (element) => portalManager.isInPortal(element),
    hasOpenPortalsInContainer: (containerSelector) => portalManager.hasOpenPortalsInContainer(containerSelector),
    cleanupOrphanedBackdrops: () => portalManager.cleanupOrphanedBackdrops(),
    cleanupOrphanedElements: () => portalManager.cleanupOrphanedElements()
};

// Also initialize RRBlazor.Portal if it doesn't exist (for direct script loading)
if (!window.RRBlazor) {
    window.RRBlazor = {};
}
if (!window.RRBlazor.Portal) {
    window.RRBlazor.Portal = window.RRPortal;
}

// Add FocusTrap API for modal focus management
window.RRBlazor.FocusTrap = {
    create: (element, trapId) => {
        try {
            if (!element) return false;
            
            // Store the currently focused element
            const previouslyFocused = document.activeElement;
            
            // Focus the modal element
            if (element.focus) {
                element.focus();
            }
            
            // Find all focusable elements
            const focusableSelector = 'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])';
            const focusableElements = element.querySelectorAll(focusableSelector);
            
            if (focusableElements.length > 0) {
                // Focus first focusable element
                focusableElements[0].focus();
            }
            
            return true;
        } catch (e) {
            console.warn(`[FocusTrap] Failed to create focus trap: ${e.message}`);
            return false;
        }
    },
    
    destroy: (trapId) => {
        try {
            // Focus trap cleanup
            return true;
        } catch (e) {
            console.warn(`[FocusTrap] Failed to destroy focus trap: ${e.message}`);
            return false;
        }
    }
};