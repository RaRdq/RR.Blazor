// RR.Blazor JavaScript Core - Unified Module System
class DebugLogger {
    constructor(prefix = '[RR.Blazor]') {
        this.isDebugMode = this.detectDebugMode();
        this.logPrefix = prefix;
    }

    detectDebugMode() {
        return (
            window.location.hostname === 'localhost' ||
            window.location.hostname === '127.0.0.1' ||
            window.location.hostname.includes('localhost') ||
            window.location.port === '5001' ||
            window.location.port === '7049' ||
            window.location.href.includes('localhost') ||
            window.location.href.includes('development') ||
            window.location.href.includes('stage') ||
            new URLSearchParams(window.location.search).has('debug') ||
            localStorage.getItem('debug') === 'true' ||
            document.body.classList.contains('debug-mode') ||
            window.location.search.includes('debug') ||
            !(window.location.hostname === 'payrollai.co' || 
              window.location.hostname === 'www.payrollai.co' ||
              window.location.hostname.endsWith('.payrollai.co'))
        );
    }

    log(...args) {
        if (this.isDebugMode) {
            console.log(this.logPrefix, ...args);
        }
    }

    info(...args) {
        if (this.isDebugMode) {
            console.info(this.logPrefix, ...args);
        }
    }

    warn(...args) {
        if (this.isDebugMode) {
            console.warn(this.logPrefix, ...args);
        }
    }

    error(...args) {
        console.error(this.logPrefix, ...args);
    }

    debug(...args) {
        if (this.isDebugMode) {
            console.debug(this.logPrefix, ...args);
        }
    }

    group(label) {
        if (this.isDebugMode) {
            console.group(this.logPrefix + ' ' + label);
        }
    }

    groupEnd() {
        if (this.isDebugMode) {
            console.groupEnd();
        }
    }

    time(label) {
        if (this.isDebugMode) {
            console.time(this.logPrefix + ' ' + label);
        }
    }

    timeEnd(label) {
        if (this.isDebugMode) {
            console.timeEnd(this.logPrefix + ' ' + label);
        }
    }
}

const debugLogger = new DebugLogger();

// Unified Module System for RR.Blazor
class ModuleManager {
    constructor() {
        this.modules = new Map();
        this.moduleUrls = {
            portal: '/_content/RR.Blazor/js/portal.js',
            tooltip: '/_content/RR.Blazor/js/tooltip.js',
            choice: '/_content/RR.Blazor/js/choice.js',
            datepicker: '/_content/RR.Blazor/js/datepicker.js',
            modal: '/_content/RR.Blazor/js/modal.js',
            forms: '/_content/RR.Blazor/js/forms.js',
            utils: '/_content/RR.Blazor/js/utils.js',
            tabs: '/_content/RR.Blazor/js/tabs.js',
            theme: '/_content/RR.Blazor/js/theme.js',
            chart: '/_content/RR.Blazor/js/chart.js'
        };
        this.loadingPromises = new Map();
    }
    
    async getModule(moduleName) {
        // Return cached module if available
        if (this.modules.has(moduleName)) {
            return this.modules.get(moduleName);
        }
        
        // Return existing loading promise if module is being loaded
        if (this.loadingPromises.has(moduleName)) {
            return this.loadingPromises.get(moduleName);
        }
        
        // Start loading module
        const url = this.moduleUrls[moduleName];
        if (!url) {
            throw new Error(`Module '${moduleName}' not found in registry`);
        }
        
        debugLogger.log(`Loading module: ${moduleName}`);
        
        const loadPromise = import(url)
            .then(module => {
                this.modules.set(moduleName, module);
                this.loadingPromises.delete(moduleName);
                debugLogger.log(`Module loaded: ${moduleName}`);
                return module;
            })
            .catch(error => {
                this.loadingPromises.delete(moduleName);
                debugLogger.error(`Failed to load module '${moduleName}':`, error);
                throw error;
            });
            
        this.loadingPromises.set(moduleName, loadPromise);
        return loadPromise;
    }
    
    async preloadModules(...moduleNames) {
        debugLogger.log(`Preloading modules: ${moduleNames.join(', ')}`);
        const promises = moduleNames.map(name => this.getModule(name));
        return Promise.all(promises);
    }
}

const moduleManager = new ModuleManager();

// Create the unified RR.Blazor API
window.RRBlazor = {
    // Module manager instance
    moduleManager,
    debugLogger,
    
    // Portal Management System - Single source of truth
    Portal: {
        async create(element, options = {}) {
            const portal = await moduleManager.getModule('portal');
            // Access the singleton instance
            const portalManager = portal.portalManager || window.RRPortalManager;
            return portalManager.create(element, options);
        },
        
        async destroy(portalId) {
            const portal = await moduleManager.getModule('portal');
            const portalManager = portal.portalManager || window.RRPortalManager;
            return portalManager.destroy(portalId);
        },
        
        async update(portalId, options) {
            const portal = await moduleManager.getModule('portal');
            const portalManager = portal.portalManager || window.RRPortalManager;
            return portalManager.update(portalId, options);
        },
        
        async position(portalId) {
            const portal = await moduleManager.getModule('portal');
            const portalManager = portal.portalManager || window.RRPortalManager;
            return portalManager.position(portalId);
        }
    },
    
    // Tooltip API
    Tooltip: {
        async create(popupElement, triggerElement, position, portalId) {
            const tooltip = await moduleManager.getModule('tooltip');
            return tooltip.createTooltipPortal(popupElement, triggerElement, position, portalId);
        },
        
        async destroy(portalId) {
            const tooltip = await moduleManager.getModule('tooltip');
            return tooltip.destroyTooltipPortal(portalId);
        },
        
        async update(portalId, triggerElement, position) {
            const tooltip = await moduleManager.getModule('tooltip');
            return tooltip.updateTooltipPosition(portalId, triggerElement, position);
        }
    },
    
    // Choice/Dropdown API
    Choice: {
        async createPortal(choiceElementId) {
            const choice = await moduleManager.getModule('choice');
            return choice.createChoicePortal(choiceElementId);
        },
        
        async destroyPortal(portalId) {
            const choice = await moduleManager.getModule('choice');
            return choice.destroyChoicePortal(portalId);
        },
        
        async registerClickOutside(choiceElementId, dotNetRef) {
            const choice = await moduleManager.getModule('choice');
            return choice.registerClickOutside(choiceElementId, dotNetRef);
        },
        
        async calculatePosition(triggerElement, options) {
            const choice = await moduleManager.getModule('choice');
            return choice.Choice.calculateOptimalPosition(triggerElement, options);
        }
    },
    
    // Modal API
    Modal: {
        async lockScroll() {
            const modal = await moduleManager.getModule('modal');
            return modal.lockScroll();
        },
        
        async unlockScroll() {
            const modal = await moduleManager.getModule('modal');
            return modal.unlockScroll();
        },
        
        async register(modalId, options) {
            const modal = await moduleManager.getModule('modal');
            return modal.register(modalId, options);
        },
        
        async unregister(modalId) {
            const modal = await moduleManager.getModule('modal');
            return modal.unregister(modalId);
        }
    },
    
    // Form utilities
    Forms: {
        async autoResizeTextarea(element) {
            const forms = await moduleManager.getModule('forms');
            return forms.autoResizeTextarea(element);
        },
        
        async focusElement(element) {
            const forms = await moduleManager.getModule('forms');
            return forms.focusElement(element);
        },
        
        async copyToClipboard(text) {
            const forms = await moduleManager.getModule('forms');
            return forms.copyToClipboard(text);
        },
        
        async initializeFormField(element, options) {
            const forms = await moduleManager.getModule('forms');
            return forms.initializeFormField(element, options);
        }
    },
    
    // Tab utilities
    Tabs: {
        async getIndicatorPosition(element) {
            const tabs = await moduleManager.getModule('tabs');
            return tabs.getTabIndicatorPosition(element);
        },
        
        async scrollToTab(element) {
            const tabs = await moduleManager.getModule('tabs');
            return tabs.scrollToTab(element);
        },
        
        async initializeTabs(element, options) {
            const tabs = await moduleManager.getModule('tabs');
            return tabs.initializeTabs(element, options);
        }
    },
    
    // DatePicker API
    DatePicker: {
        async init(elementId, options) {
            const datepicker = await moduleManager.getModule('datepicker');
            return datepicker.init(elementId, options);
        },
        
        async positionPopup(elementId) {
            const datepicker = await moduleManager.getModule('datepicker');
            return datepicker.positionPopup(elementId);
        },
        
        async formatDate(date, format) {
            const datepicker = await moduleManager.getModule('datepicker');
            return datepicker.formatDate(date, format);
        },
        
        async parseDate(dateString, format) {
            const datepicker = await moduleManager.getModule('datepicker');
            return datepicker.parseDate(dateString, format);
        }
    },
    
    // Utility functions
    Utils: {
        async scrollIntoView(element, options) {
            const utils = await moduleManager.getModule('utils');
            return utils.scrollIntoView(element, options);
        },
        
        async getElementDimensions(element) {
            const utils = await moduleManager.getModule('utils');
            return utils.getElementDimensions(element);
        },
        
        async toggleClass(element, className) {
            const utils = await moduleManager.getModule('utils');
            return utils.toggleClass(element, className);
        },
        
        async downloadFile(content, filename, contentType) {
            const utils = await moduleManager.getModule('utils');
            return utils.downloadFile(content, filename, contentType);
        },
        
        async updateUrlWithoutScroll(url) {
            const utils = await moduleManager.getModule('utils');
            return utils.updateUrlWithoutScroll(url);
        }
    },
    
    // Theme management
    Theme: {
        async apply(theme) {
            const themeModule = await moduleManager.getModule('theme');
            return themeModule.applyTheme(theme);
        },
        
        async toggle() {
            const themeModule = await moduleManager.getModule('theme');
            return themeModule.toggleTheme();
        },
        
        async getCurrent() {
            const themeModule = await moduleManager.getModule('theme');
            return themeModule.getCurrentTheme();
        }
    },

    // Generic component initialization
    async initializeComponent(componentType, elementId, options = {}) {
        const element = document.getElementById(elementId);
        if (!element) {
            debugLogger.warn(`Element not found: ${elementId}`);
            return;
        }
        
        switch (componentType) {
            case 'tabs':
                return this.Tabs.initializeTabs(element, options);
            case 'form-field':
                return this.Forms.initializeFormField(element, options);
            case 'datepicker':
                return this.DatePicker.init(elementId, options);
            default:
                debugLogger.warn(`Unknown component type: ${componentType}`);
        }
    },
    
    // User menu outside click handler (specific implementation)
    setupUserMenuOutsideClick(userMenuContainerId, toggleCallback) {
        debugLogger.log('Setting up user menu outside click handler for:', userMenuContainerId);
        
        const userMenuContainer = document.querySelector(userMenuContainerId);
        if (!userMenuContainer) {
            debugLogger.warn('User menu container not found:', userMenuContainerId);
            return;
        }

        const outsideClickHandler = function(event) {
            if (!userMenuContainer.contains(event.target)) {
                debugLogger.log('Outside click detected, closing user menu');
                toggleCallback.invokeMethodAsync('CloseUserMenu');
            }
        };

        userMenuContainer._outsideClickHandler = outsideClickHandler;
        document.addEventListener('click', outsideClickHandler);
        debugLogger.log('User menu outside click handler attached');
    },

    removeUserMenuOutsideClick(userMenuContainerId) {
        const userMenuContainer = document.querySelector(userMenuContainerId);
        if (userMenuContainer && userMenuContainer._outsideClickHandler) {
            document.removeEventListener('click', userMenuContainer._outsideClickHandler);
            delete userMenuContainer._outsideClickHandler;
            debugLogger.log('User menu outside click handler removed');
        }
    },
    
    // Preload commonly used modules for performance
    async preloadCore() {
        return moduleManager.preloadModules('portal', 'utils', 'forms');
    }
};

document.addEventListener('DOMContentLoaded', function() {
    document.querySelectorAll('[data-rr-component]').forEach(element => {
        const componentType = element.getAttribute('data-rr-component');
        const options = element.getAttribute('data-rr-options');
        RRBlazor.initializeComponent(componentType, element.id, options ? JSON.parse(options) : {});
    });
    
    // Preload core modules for better performance
    RRBlazor.preloadCore().catch(error => {
        debugLogger.warn('Failed to preload core modules:', error);
    });
});

// Ensure RRBlazor is available globally before any module loads
window.RRBlazor = window.RRBlazor || RRBlazor;

// Export debug utilities
window.RRDebugLogger = DebugLogger;
window.debugLogger = debugLogger;

// Legacy compatibility - these will be removed in future versions
window.RRDebug = {
    logger: debugLogger,
    report: async () => {
        if (debugLogger.isDebugMode) {
            const debugModule = await import('./page-debug.js');
            return debugModule.generateReport();
        }
    }
};

// Load debug utilities in development mode
if (debugLogger.isDebugMode) {
    import('./page-debug.js')
        .then(debugModule => {
            window.RRDebug.pageDebug = debugModule;
            debugLogger.log('🔧 Debug utilities loaded for development environment');
        })
        .catch(error => {
            debugLogger.warn('Failed to load debug utilities:', error);
        });
}

