class ClickOutsideManager {
    constructor() {
        this.trackedElements = new Map();
        this.portalRelationships = new Map();
        this.clickDelegates = new Map();
        this.globalHandler = null;
        this.initialized = false;
        this.initializationPromise = null;

        this.performanceMetrics = {
            handlerExecutions: 0,
            totalExecutionTime: 0,
            lastExecutionTime: 0
        };

        this.errorCount = 0;
        this.maxErrors = 10;
        this.lastError = null;

        this.debugMode = window.location.hostname === 'localhost' ||
                         window.location.search.includes('debug=true');

        this._initializeWhenReady();
    }

    _initializeWhenReady() {
        if (this.initializationPromise) {
            return this.initializationPromise;
        }

        this.initializationPromise = new Promise((resolve) => {
            const tryInitialize = () => {
                try {
                    if (!window.RRBlazor?.Events) {
                        setTimeout(tryInitialize, 10);
                        return;
                    }

                    this.globalHandler = this._handleGlobalClick.bind(this);

                    document.addEventListener('click', this.globalHandler, true);

                    this._setupPortalTracking();

                    this.initialized = true;

                    resolve(true);
                } catch (error) {
                    this.lastError = error;
                    this.errorCount++;

                    if (!this.globalHandler) {
                        this.globalHandler = this._handleGlobalClick.bind(this);
                        document.addEventListener('click', this.globalHandler, true);
                        this.initialized = true;
                    }

                    resolve(false);
                }
            };

            if (document.readyState === 'loading') {
                document.addEventListener('DOMContentLoaded', tryInitialize);
            } else {
                setTimeout(tryInitialize, 0);
            }
        });

        return this.initializationPromise;
    }

    async initialize() {
        return this.initializationPromise || this._initializeWhenReady();
    }

    async register(elementId, element, options = {}) {
        if (!this.initialized) {
            await this.initialize();
        }

        if (!elementId || !element) {
            return false;
        }

        const targetElement = typeof element === 'string'
            ? document.querySelector(element)
            : element;

        if (!targetElement) {
            return false;
        }

        this.trackedElements.set(elementId, {
            element: targetElement,
            options: options || {},
            excludeSelectors: options.excludeSelectors || [],
            callback: options.callback || null,
            registeredAt: Date.now()
        });

        return true;
    }

    unregister(elementId) {
        if (!elementId) return false;

        const existed = this.trackedElements.has(elementId);
        this.trackedElements.delete(elementId);

        const cleanupKeys = [];
        for (const [key, value] of this.portalRelationships.entries()) {
            if (key === elementId || key.includes(elementId)) {
                cleanupKeys.push(key);
            }
        }
        cleanupKeys.forEach(key => this.portalRelationships.delete(key));

        return existed;
    }

    registerClickDelegate(delegateId, selector, handler, options = {}) {
        if (!delegateId || !selector || !handler) {
            return false;
        }

        this.clickDelegates.set(delegateId, {
            selector,
            handler,
            options: options || {},
            priority: options.priority || 0,
            registeredAt: Date.now()
        });

        return true;
    }

    unregisterClickDelegate(delegateId) {
        if (!delegateId) return false;

        const existed = this.clickDelegates.has(delegateId);
        this.clickDelegates.delete(delegateId);

        return existed;
    }

    _handleGlobalClick(event) {
        if (this.errorCount > this.maxErrors) {
            this.destroy();
            return;
        }

        const startTime = performance.now();

        try {
            const target = event.target;

            const processedByDelegate = this._processClickDelegates(event);

            const clickedElements = new Set();

            for (const [elementId, data] of this.trackedElements.entries()) {
                try {
                    const { element, excludeSelectors, callback, options } = data;

                    if (!document.contains(element)) {
                        this.trackedElements.delete(elementId);
                        continue;
                    }

                    if (this._isTargetContained(element, target, elementId)) {
                        continue;
                    }

                    let excluded = false;
                    for (const selector of excludeSelectors) {
                        if (target.closest(selector)) {
                            excluded = true;
                            break;
                        }
                    }
                    if (excluded) continue;

                    if (options.ignoreModalOverlay && target.closest('.backdrop, .modal-backdrop')) {
                        continue;
                    }

                    clickedElements.add(elementId);

                } catch (error) {
                    this.errorCount++;
                }
            }

            for (const elementId of clickedElements) {
                const data = this.trackedElements.get(elementId);
                if (!data) continue;

                if (data.callback) {
                    try {
                        data.callback(event);
                    } catch (error) {
                    }
                }

                if (window.RRBlazor?.EventDispatcher?.dispatch) {
                    window.RRBlazor.EventDispatcher.dispatch(
                        window.RRBlazor.Events.CLICK_OUTSIDE,
                        { elementId, target, originalEvent: event }
                    );
                }

                data.element.dispatchEvent(new CustomEvent(
                    window.RRBlazor?.Events?.CLICK_OUTSIDE || 'clickoutside',
                    { detail: { elementId, target, originalEvent: event } }
                ));
            }

        } catch (error) {
            this.lastError = error;
            this.errorCount++;
        }

        const executionTime = performance.now() - startTime;
        this.performanceMetrics.handlerExecutions++;
        this.performanceMetrics.totalExecutionTime += executionTime;
        this.performanceMetrics.lastExecutionTime = executionTime;
    }

    _processClickDelegates(event) {
        if (this.clickDelegates.size === 0) return false;

        const target = event.target;

        const sortedDelegates = Array.from(this.clickDelegates.entries())
            .sort(([, a], [, b]) => (b.priority || 0) - (a.priority || 0));

        let processed = false;

        for (const [delegateId, delegate] of sortedDelegates) {
            try {
                const { selector, handler, options } = delegate;

                const matchedElement = target.closest(selector);
                if (matchedElement) {
                    if (options.excludeSelectors) {
                        let excluded = false;
                        for (const excludeSelector of options.excludeSelectors) {
                            if (target.closest(excludeSelector)) {
                                excluded = true;
                                break;
                            }
                        }
                        if (excluded) continue;
                    }

                    const result = handler(event, matchedElement);
                    processed = true;

                    if (result === true || options.stopPropagation) {
                        break;
                    }
                }
            } catch (error) {
            }
        }

        return processed;
    }

    _isTargetContained(element, target, elementId) {
        if (element.contains(target)) return true;

        if (this.portalRelationships.has(elementId)) {
            const portalContainers = this.portalRelationships.get(elementId);
            for (const portal of portalContainers) {
                if (portal && document.contains(portal) && portal.contains(target)) {
                    return true;
                }
            }
        }

        const elementIdAttr = element.getAttribute('data-choice-id') ||
                             element.getAttribute('data-modal-id') ||
                             element.id;

        if (elementIdAttr) {
            const escapedId = CSS.escape(elementIdAttr);
            const portalSelector = `[data-portal-for="${elementIdAttr}"], #choice-${escapedId}, #datepicker-${escapedId}, #autosuggest-${escapedId}, #filter-${escapedId}, #tooltip-${escapedId}`;
            const portal = document.querySelector(portalSelector);
            if (portal && portal.contains(target)) {
                return true;
            }

            const targetPortal = target.closest('[data-portal-positioned]');
            if (targetPortal) {
                const portalParent = targetPortal.closest('.portal, [class*="-portal"]');
                if (portalParent) {
                    const portalId = portalParent.id || portalParent.getAttribute('data-portal-id');
                    if (portalId && portalId.includes(elementIdAttr)) {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    _setupPortalTracking() {
        if (!window.RRBlazor?.Events) {
            return;
        }

        const events = window.RRBlazor.Events;

        document.addEventListener(events.PORTAL_ELEMENT_MOVED, (event) => {
            this._trackPortalRelationship(event.detail);
        });

        document.addEventListener(events.PORTAL_ELEMENT_RESTORED, (event) => {
            this._untrackPortalRelationship(event.detail);
        });

        document.addEventListener(events.PORTAL_DESTROYED, (event) => {
            this._cleanupPortalRelationships(event.detail?.portalId);
        });
    }

    _trackPortalRelationship(detail) {
        if (!detail?.element || !detail?.portalId) return;

        let elementId = detail.sourceElementId;
        if (!elementId && detail.portalId) {
            const patterns = [
                /^choice-(.+)$/,
                /^datepicker-(.+)$/,
                /^autosuggest-(.+)$/,
                /^filter-(.+)$/,
                /^tooltip-(.+)$/
            ];

            for (const pattern of patterns) {
                const match = detail.portalId.match(pattern);
                if (match) {
                    elementId = match[1];
                    break;
                }
            }
        }

        if (!elementId) {
            elementId = detail.portalId;
        }

        const portalContainer = document.getElementById(detail.portalId) ||
                               detail.element.closest('.portal');

        if (portalContainer) {
            if (!this.portalRelationships.has(elementId)) {
                this.portalRelationships.set(elementId, new Set());
            }
            this.portalRelationships.get(elementId).add(portalContainer);
        }
    }

    _untrackPortalRelationship(detail) {
        if (!detail?.portalId) return;

        for (const [elementId, portals] of this.portalRelationships.entries()) {
            const portalContainer = document.getElementById(detail.portalId);
            if (portalContainer) {
                portals.delete(portalContainer);
                if (portals.size === 0) {
                    this.portalRelationships.delete(elementId);
                }
            }
        }
    }

    _cleanupPortalRelationships(portalId) {
        if (!portalId) return;

        const cleanupKeys = [];
        for (const [key, portals] of this.portalRelationships.entries()) {
            const portalContainer = document.getElementById(portalId);
            if (portalContainer) {
                portals.delete(portalContainer);
                if (portals.size === 0) {
                    cleanupKeys.push(key);
                }
            }
        }

        cleanupKeys.forEach(key => this.portalRelationships.delete(key));
    }

    getStatus() {
        return {
            initialized: this.initialized,
            trackedElements: this.trackedElements.size,
            portalRelationships: this.portalRelationships.size,
            clickDelegates: this.clickDelegates.size,
            errorCount: this.errorCount,
            performanceMetrics: { ...this.performanceMetrics },
            lastError: this.lastError
        };
    }

    _logStatus() {
        if (!this.debugMode) return;
    }

    destroy() {
        if (this.globalHandler) {
            document.removeEventListener('click', this.globalHandler, true);
            this.globalHandler = null;
        }

        this.trackedElements.clear();
        this.portalRelationships.clear();
        this.clickDelegates.clear();
        this.initialized = false;
        this.initializationPromise = null;
    }

    resetErrors() {
        this.errorCount = 0;
        this.lastError = null;
    }
}

const clickOutsideManager = new ClickOutsideManager();

clickOutsideManager.initialize();

export default clickOutsideManager;

if (typeof window !== 'undefined') {
    window.RRBlazor = window.RRBlazor || {};
    window.RRBlazor.ClickOutside = clickOutsideManager;

    window.RRBlazor.ClickManager = {
        register: (elementId, element, options) => clickOutsideManager.register(elementId, element, options),
        unregister: (elementId) => clickOutsideManager.unregister(elementId),
        registerDelegate: (delegateId, selector, handler, options) =>
            clickOutsideManager.registerClickDelegate(delegateId, selector, handler, options),
        unregisterDelegate: (delegateId) => clickOutsideManager.unregisterClickDelegate(delegateId),
        getStatus: () => clickOutsideManager.getStatus(),
        resetErrors: () => clickOutsideManager.resetErrors(),
        cleanup: () => clickOutsideManager.destroy()
    };

}