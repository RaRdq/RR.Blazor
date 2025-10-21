import { PositioningEngine } from './positioning.js';
import { createDOMStateManager } from './dom-state-manager.js';
import { createEventHandlerManager } from './event-handler-manager.js';
import { waitForPortal, createCleanupManager } from './utils.js';
import { TIMEOUTS } from './event-constants.js';

const DROPDOWN_CONFIG = {
    REPOSITION_THROTTLE_MS: 10,
    PORTAL_TIMEOUT_MS: 2000,
    VIEWPORT_EDGE_PADDING: 16,
    DEFAULT_WIDTH_PX: 200,
    DEFAULT_HEIGHT_PX: 320,
    DEFAULT_OFFSET_PX: 4,
    CONTAINER_PADDING_PX: 16,
    VISIBLE_OPACITY: '1',
    HIDDEN_OPACITY: '0',
    GPU_ACCELERATION_TRANSFORM: 'translateZ(0)',
    VISIBILITY_BOUNDARY: 0,
    MODAL_SELECTORS: '.modal, .dialog, [role="dialog"]',
    SCROLLABLE_SELECTORS: '.modal-body, .rr-scrollable, .main-content, .page-content, .overflow-auto, .overflow-y-auto'
};

const MIN_TRIGGER_WIDTH_PX = 48;
const LAYOUT_STABILITY_TOLERANCE_PX = 1;

const positioningEngine = new PositioningEngine();
const domStateManager = createDOMStateManager();
const eventHandlerManager = createEventHandlerManager();

function validateElement(element, elementName) {
    if (!element) {
        throw new Error(`${elementName} is null or undefined`);
    }
    if (!(element instanceof Element)) {
        throw new Error(`${elementName} is not a DOM Element`);
    }
    if (!element.parentNode) {
        throw new Error(`${elementName} is not attached to the DOM`);
    }
    return true;
}

function validateDropdownStructure(element, triggerSelector, contentSelector, componentId) {
    validateElement(element, `Dropdown container for ${componentId}`);

    const trigger = element.querySelector(triggerSelector);
    if (!trigger) {
        throw new Error(`Trigger element not found with selector '${triggerSelector}' in ${componentId}`);
    }
    validateElement(trigger, `Trigger element for ${componentId}`);

    const viewport = element.querySelector(contentSelector);
    if (!viewport) {
        throw new Error(`Content viewport not found with selector '${contentSelector}' in ${componentId}`);
    }
    validateElement(viewport, `Content viewport for ${componentId}`);

    return { trigger, viewport };
}

class DropdownManagerBase {
    constructor() {
        this.instances = new Map();
        this.viewportLocations = new Map();
        this.componentEventHandlers = new Map();
        this.pendingOperations = new Set();
        this.conflictResolutionMutex = Promise.resolve();
    }

    async positionDropdown(element, config = {}) {
        const {
            contentSelector,
            triggerSelector = '.dropdown-trigger, .trigger',
            componentType,
            componentId,
            dimensions = { width: DROPDOWN_CONFIG.DEFAULT_WIDTH_PX, height: DROPDOWN_CONFIG.DEFAULT_HEIGHT_PX },
            position = 'auto',
            offset = DROPDOWN_CONFIG.DEFAULT_OFFSET_PX,
            allowMultiple = false,
            excludeSelectors = [],
            onOpen = null,
            onClose = null,
            onReposition = null,
            enableKeyboard = false,
            autoCloseOnScroll = true,
            clickOutsideOptions = {},
            customTriggerBounds = null,
            minHeight = null  // Component-specific minimum height
        } = config;

        if (!contentSelector || !componentId) {
            throw new Error(`DropdownManager.positionDropdown: Missing required parameters - contentSelector: ${!!contentSelector}, componentId: ${!!componentId}`);
        }

        if (!componentType || typeof componentType !== 'string') {
            throw new Error(`DropdownManager.positionDropdown: componentType is required and must be a string, got: ${typeof componentType}`);
        }

        const { trigger, viewport } = validateDropdownStructure(element, triggerSelector, contentSelector, componentId);

        if (this.pendingOperations.has(componentId)) {
            throw new Error(`Operation already pending for ${componentId} - fix the caller`);
        }
        this.pendingOperations.add(componentId);

        try {

            if (!allowMultiple) {
                await new Promise(resolve => {
                    this.conflictResolutionMutex = this.conflictResolutionMutex
                        .then(() => this.closeConflictingComponents(componentType, componentId))
                        .then(resolve)
                        .catch(err => {
                            resolve();
                        });
                });
            }

            window.RRBlazor.EventDispatcher.dispatch(
                window.RRBlazor.Events.UI_COMPONENT_OPENING,
                {
                    componentType: componentType.toUpperCase(),
                    componentId: componentId,
                    priority: window.RRBlazor.EventPriorities.NORMAL
                }
            );

            const triggerRect = customTriggerBounds || trigger.getBoundingClientRect();

            if (!window.RRBlazor || !window.RRBlazor.Modal) {
                throw new Error('Modal system not initialized - check module loading order');
            }
            const modalContext = window.RRBlazor.Modal.getModalContext(element);

            let containerRect = null;
            let containerElement = null;
            if (modalContext) {
                containerElement = modalContext.element.querySelector('.modal-body') || modalContext.element;
                containerRect = containerElement.getBoundingClientRect();
            }


            const targetDimensions = {
                width: dimensions.width || viewport.offsetWidth || DROPDOWN_CONFIG.DEFAULT_WIDTH_PX,
                height: dimensions.height || DROPDOWN_CONFIG.DEFAULT_HEIGHT_PX
            };

            const portalId = this._generatePortalId(componentType, componentId);
            const elementType = modalContext ? 'modal' : 'portal';
            const contextZIndex = window.RRBlazor.ZIndexManager.registerElement(portalId, elementType);


            let adaptedDimensions = { ...targetDimensions };
            if (containerRect) {
                const maxContainerWidth = containerRect.width - DROPDOWN_CONFIG.CONTAINER_PADDING_PX;
                if (adaptedDimensions.width > maxContainerWidth) {
                    adaptedDimensions.width = Math.max(maxContainerWidth, triggerRect.width);
                }
            }

            let desiredPosition;
            if (position === 'auto') {
                desiredPosition = positioningEngine.detectOptimalPosition(triggerRect, adaptedDimensions, undefined, containerRect, minHeight);
            } else {
                desiredPosition = this._normalizePosition(position);
            }

            const calculatedPosition = positioningEngine.calculatePosition(
                triggerRect,
                adaptedDimensions,
                {
                    position: desiredPosition,
                    offset: offset,
                    flip: true,
                    minHeight: minHeight,
                    constrain: true,
                    container: containerRect
                }
            );


            domStateManager.store(viewport);

            viewport.style.position = '';
            viewport.style.top = '';
            viewport.style.left = '';
            viewport.style.transform = '';

            if (window.RRBlazor.Portal.isPortalActive(portalId)) {
                window.RRBlazor.Portal.destroy(portalId);
                window.RRBlazor.ZIndexManager.unregisterElement(portalId);
            }

            const portalPromise = waitForPortal(portalId, componentType, DROPDOWN_CONFIG.PORTAL_TIMEOUT_MS);

            window.RRBlazor.EventDispatcher.dispatch(
                window.RRBlazor.Events.PORTAL_CREATE_REQUEST,
                {
                    requesterId: portalId,
                    element: viewport,
                    config: {
                        id: portalId,
                        className: `${componentType}-portal` + (modalContext ? ` ${componentType}-modal-context` : ''),
                        zIndex: contextZIndex,
                        sourceElement: element,
                        attributes: modalContext ? {
                            'data-in-modal': 'true',
                            'data-parent-modal-id': modalContext.modalId,
                            'data-modal-context': modalContext.modalId
                        } : {}
                    }
                }
            );

            const portalResult = await portalPromise;

            if (!portalResult || !portalResult.elementMoved) {
                throw new Error(`Portal creation failed for ${componentType} ${componentId} - fix the portal system, don't add fallbacks`);
            }

            this.viewportLocations.set(componentId, portalResult.element);

            viewport.style.setProperty(`--${componentType}-width`, `${adaptedDimensions.width}px`);
            viewport.style.setProperty(`--${componentType}-max-height`, `${adaptedDimensions.height}px`);
            viewport.style.setProperty('--dropdown-z-index', contextZIndex.toString());

            Object.assign(viewport.style, calculatedPosition.cssStyles);

            viewport.style.width = `${adaptedDimensions.width}px`;
            viewport.style.maxHeight = `${adaptedDimensions.height}px`;
            viewport.style.overflow = ''; // Clear to let CSS overflow-y: auto take over
            viewport.style.display = 'block';
            viewport.style.opacity = DROPDOWN_CONFIG.VISIBLE_OPACITY;
            viewport.style.pointerEvents = 'auto';


            if (modalContext) {
                viewport.style.isolation = 'isolate';
                viewport.style.contain = 'layout style';
                viewport.style.willChange = 'transform';
                viewport.style.transform = DROPDOWN_CONFIG.GPU_ACCELERATION_TRANSFORM;
            } else {
                viewport.style.isolation = 'isolate';
                viewport.style.contain = 'layout style';
            }

            if (onOpen) {
                onOpen(element, viewport);
            }

            this._setupEventHandlers(componentId, trigger, viewport, {
                componentType,
                element,
                adaptedDimensions,
                desiredPosition,
                containerRect,
                offset,
                onReposition,
                autoCloseOnScroll,
                customTriggerBounds
            });

            this._setupClickOutside(componentId, element, {
                excludeSelectors: [
                    triggerSelector,
                    contentSelector,
                    `[data-${componentType}-id="${componentId}"]`,
                    ...excludeSelectors
                ],
                ...clickOutsideOptions
            });


            this.instances.set(componentId, {
                element,
                trigger,
                viewport,
                componentType,
                modalContext,
                contextZIndex,
                containerElement,
                onClose,
                config,
                portalId
            });

            window.RRBlazor.EventDispatcher.dispatch(
                window.RRBlazor.Events.UI_COMPONENT_OPENED,
                {
                    componentType: componentType.toUpperCase(),
                    componentId: componentId
                }
            );

            return true;
        } finally {
            this.pendingOperations.delete(componentId);
        }
    }

    async closeDropdown(componentId) {
        if (!componentId) return false;

        const instance = this.instances.get(componentId);
        if (!instance) {
            return false;
        }

        window.RRBlazor.EventDispatcher.dispatch(
            window.RRBlazor.Events.UI_COMPONENT_CLOSING,
            {
                componentType: instance.componentType.toUpperCase(),
                componentId: componentId
            }
        );

        const { viewport, element, onClose, componentType, portalId } = instance;

        if (!portalId || portalId.includes('undefined')) {
            throw new Error(`Portal ID is invalid for component ${componentId}: "${portalId}" - fix the componentType parameter`);
        }

        if (onClose) {
            onClose(element, viewport);
        }

        this._resetStyles(viewport, componentType);

        domStateManager.restore(viewport);

        window.RRBlazor.EventDispatcher.dispatch(
            window.RRBlazor.Events.PORTAL_DESTROY_REQUEST,
            { requesterId: portalId, portalId: portalId }
        );

        this._cleanupEventHandlers(componentId);
        this._cleanupClickOutside(componentId);

        if (viewport) {
            window.RRBlazor.EventDispatcher.dispatchParentClosing(viewport, componentType, componentId);
        }

        window.RRBlazor.ZIndexManager.unregisterElement(portalId);

        this.instances.delete(componentId);
        this.viewportLocations.delete(componentId);

        window.RRBlazor.EventDispatcher.dispatch(
            window.RRBlazor.Events.UI_COMPONENT_CLOSED,
            {
                componentType: instance.componentType.toUpperCase(),
                componentId: componentId
            }
        );

        return true;
    }

    async closeConflictingComponents(componentType, excludeComponentId = null) {
        const COEXISTENCE_RULES = {
            tooltip: [],
            choice: ['choice', 'filter', 'autosuggest', 'datepicker'],
            filter: ['choice', 'filter', 'autosuggest', 'datepicker'],
            autosuggest: ['choice', 'filter', 'autosuggest', 'datepicker'],
            datepicker: ['choice', 'filter', 'autosuggest', 'datepicker']
        };

        const conflictingTypes = COEXISTENCE_RULES[componentType] || [];
        if (conflictingTypes.length === 0) return;

        const closePromises = [];
        for (const [componentId, instance] of this.instances.entries()) {
            if (componentId === excludeComponentId) continue;

            if (conflictingTypes.includes(instance.componentType)) {
                closePromises.push(this.closeDropdown(componentId));
            }
        }
        await Promise.all(closePromises);
    }

    async closeAll() {
        const closePromises = [];
        for (const componentId of this.instances.keys()) {
            closePromises.push(this.closeDropdown(componentId));
        }
        await Promise.all(closePromises);

        this.instances.clear();
        this.viewportLocations.clear();
        this.componentEventHandlers.clear();

        return true;
    }

    getViewport(componentId) {
        const instance = this.instances.get(componentId);
        if (!instance) return null;

        if (this.viewportLocations.has(componentId)) {
            const location = this.viewportLocations.get(componentId);
            if (location) {
                return location.querySelector('.viewport, .popup, .dropdown');
            }
        }

        return instance.viewport;
    }

    isOpen(componentId) {
        return this.instances.has(componentId);
    }

    cleanup(componentId) {
        return this.closeDropdown(componentId);
    }

    _normalizePosition(position) {
        const directionMap = {
            'bottom': PositioningEngine.POSITIONS.BOTTOM_START,
            'bottom-start': PositioningEngine.POSITIONS.BOTTOM_START,
            'bottom-center': PositioningEngine.POSITIONS.BOTTOM_CENTER,
            'bottom-end': PositioningEngine.POSITIONS.BOTTOM_END,
            'top': PositioningEngine.POSITIONS.TOP_START,
            'top-start': PositioningEngine.POSITIONS.TOP_START,
            'top-center': PositioningEngine.POSITIONS.TOP_CENTER,
            'top-end': PositioningEngine.POSITIONS.TOP_END,
            'left': PositioningEngine.POSITIONS.LEFT_START,
            'left-start': PositioningEngine.POSITIONS.LEFT_START,
            'left-center': PositioningEngine.POSITIONS.LEFT_CENTER,
            'left-end': PositioningEngine.POSITIONS.LEFT_END,
            'right': PositioningEngine.POSITIONS.RIGHT_START,
            'right-start': PositioningEngine.POSITIONS.RIGHT_START,
            'right-center': PositioningEngine.POSITIONS.RIGHT_CENTER,
            'right-end': PositioningEngine.POSITIONS.RIGHT_END
        };
        return directionMap[position] || PositioningEngine.POSITIONS.BOTTOM_START;
    }

    _generatePortalId(componentType, componentId) {
        const portalId = `${componentType}-portal-${componentId}`;
        return portalId;
    }

    _setupEventHandlers(componentId, trigger, viewport, options) {
        const {
            componentType,
            element,
            adaptedDimensions,
            desiredPosition,
            containerRect,
            offset,
            onReposition,
            autoCloseOnScroll,
            customTriggerBounds
        } = options;

        let lastKnownBounds = customTriggerBounds || null;

        const resolveTriggerRect = () => {
            if (!trigger) {
                return lastKnownBounds;
            }

            const rect = trigger.getBoundingClientRect();

            if (!lastKnownBounds) {
                if (rect.width >= MIN_TRIGGER_WIDTH_PX) {
                    lastKnownBounds = rect;
                }
                return lastKnownBounds ?? rect;
            }

            if (rect.width < MIN_TRIGGER_WIDTH_PX) {
                return lastKnownBounds;
            }

            const widthDelta = rect.width - lastKnownBounds.width;
            const leftDelta = Math.abs(rect.left - lastKnownBounds.left);

            if (widthDelta < -LAYOUT_STABILITY_TOLERANCE_PX) {
                return lastKnownBounds;
            }

            if (leftDelta > LAYOUT_STABILITY_TOLERANCE_PX && widthDelta < LAYOUT_STABILITY_TOLERANCE_PX) {
                return lastKnownBounds;
            }

            lastKnownBounds = rect;
            return rect;
        };

        const repositionHandler = () => {
            if (!trigger || !viewport) return;

            const triggerRect = resolveTriggerRect();
            if (!triggerRect) return;

            if (autoCloseOnScroll) {
                const isVisible = triggerRect.bottom > DROPDOWN_CONFIG.VISIBILITY_BOUNDARY &&
                                triggerRect.top < window.innerHeight &&
                                triggerRect.right > DROPDOWN_CONFIG.VISIBILITY_BOUNDARY &&
                                triggerRect.left < window.innerWidth;

                if (!isVisible) {
                    this.closeDropdown(componentId);
                    return;
                }
            }

            const position = positioningEngine.calculatePosition(
                triggerRect,
                adaptedDimensions,
                {
                    position: desiredPosition,
                    offset: offset,
                    flip: true,
                    constrain: true,
                    container: containerRect
                }
            );

            Object.assign(viewport.style, position.cssStyles);

            if (onReposition) {
                onReposition(position, viewport);
            }
        };

        let repositionTimeout;
        const throttledReposition = () => {
            clearTimeout(repositionTimeout);
            repositionTimeout = setTimeout(repositionHandler, DROPDOWN_CONFIG.REPOSITION_THROTTLE_MS);
        };

        if (!trigger) {
            throw new Error(`Trigger element is null for component ${componentId}`);
        }

        const scrollContainer = this._findScrollableContainer(trigger) || element.closest(DROPDOWN_CONFIG.SCROLLABLE_SELECTORS) || document.documentElement;


        const scrollHandlerId = eventHandlerManager.addEventListener(scrollContainer, 'scroll', throttledReposition, true);
        const resizeHandlerId = eventHandlerManager.addEventListener(window, 'resize', throttledReposition);

        if (!this.componentEventHandlers.has(componentId)) {
            this.componentEventHandlers.set(componentId, []);
        }
        this.componentEventHandlers.get(componentId).push(scrollHandlerId, resizeHandlerId);
    }

    _findScrollableContainer(element) {
        if (!element || !element.parentElement) {
            throw new Error('_findScrollableContainer: element is null or not attached to DOM');
        }

        let parent = element.parentElement;
        while (parent && parent !== document.documentElement) {
            const style = window.getComputedStyle(parent);
            if (style.overflow === 'auto' || style.overflow === 'scroll' ||
                style.overflowY === 'auto' || style.overflowY === 'scroll' ||
                style.overflowX === 'auto' || style.overflowX === 'scroll') {
                return parent;
            }
            parent = parent.parentElement;
        }
        return null;
    }

    _setupClickOutside(componentId, element, options) {
        window.RRBlazor.ClickOutside.register(`dropdown-${componentId}`, element, {
            excludeSelectors: options.excludeSelectors || [],
            callback: (event) => {
                if (!options.disabled) {
                    this.closeDropdown(componentId);
                }
            },
            ...options
        });
    }

    _cleanupEventHandlers(componentId) {
        if (this.componentEventHandlers.has(componentId)) {
            const handlerIds = this.componentEventHandlers.get(componentId);
            handlerIds.forEach(handlerId => {
                if (handlerId) {
                    eventHandlerManager.removeEventListener(handlerId);
                }
            });
            this.componentEventHandlers.delete(componentId);
        }


        if (this.pendingOperations.has(componentId)) {
            this.pendingOperations.delete(componentId);
        }
    }

    _cleanupClickOutside(componentId) {
        window.RRBlazor.ClickOutside.unregister(`dropdown-${componentId}`);
    }



    _resetStyles(viewport, componentType) {
        viewport.style.removeProperty(`--${componentType}-width`);
        viewport.style.removeProperty(`--${componentType}-max-height`);
        viewport.style.removeProperty('--dropdown-z-index');

        viewport.style.position = '';
        viewport.style.top = '';
        viewport.style.bottom = '';
        viewport.style.left = '';
        viewport.style.right = '';
        viewport.style.display = 'none';
        viewport.style.opacity = '';
        viewport.style.pointerEvents = '';
        viewport.style.transformOrigin = '';

        viewport.style.transform = '';
        viewport.style.width = '';
        viewport.style.maxHeight = '';
        viewport.style.isolation = '';
        viewport.style.contain = '';
        viewport.style.willChange = '';
    }
}

const dropdownManager = new DropdownManagerBase();

export default dropdownManager;
