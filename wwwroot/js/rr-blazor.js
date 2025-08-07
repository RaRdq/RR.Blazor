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

// NO PROTECTIVE CODING - All errors must bubble up for proper debugging

// Unified Module System for RR.Blazor
class ModuleManager {
    constructor() {
        this.modules = new Map();
        this.moduleUrls = {
            portal: '/_content/RR.Blazor/js/portal.js',
            tooltip: '/_content/RR.Blazor/js/tooltip.js',
            choice: '/_content/RR.Blazor/js/choice.js',
            autosuggest: '/_content/RR.Blazor/js/autosuggest.js',
            datepicker: '/_content/RR.Blazor/js/datepicker.js',
            modal: '/_content/RR.Blazor/js/modal.js',
            forms: '/_content/RR.Blazor/js/forms.js',
            utils: '/_content/RR.Blazor/js/utils.js',
            tabs: '/_content/RR.Blazor/js/tabs.js',
            theme: '/_content/RR.Blazor/js/theme.js',
            chart: '/_content/RR.Blazor/js/chart.js',
            table: '/_content/RR.Blazor/js/table-scroll.js',
            fileUpload: '/_content/RR.Blazor/js/file-upload.js'
        };
        this.loadingPromises = new Map();
    }
    
    async getModule(moduleName) {
        if (this.modules.has(moduleName)) {
            return this.modules.get(moduleName);
        }
        
        if (this.loadingPromises.has(moduleName)) {
            return this.loadingPromises.get(moduleName);
        }
        
        const url = this.moduleUrls[moduleName];
        if (!url) {
            throw new Error(`Module '${moduleName}' not found in registry`);
        }
        
        debugLogger.log(`Loading module: ${moduleName}`);
        
        const loadPromise = import(url).then(module => {
            this.modules.set(moduleName, module);
            this.loadingPromises.delete(moduleName);
            debugLogger.log(`Module loaded: ${moduleName}`);
            return module;
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
    moduleManager,
    debugLogger,
    
    // Core preload function for essential modules
    async preloadCore() {
        debugLogger.log('Preloading core RR.Blazor modules...');
        try {
            // Preload essential modules that are commonly needed
            await moduleManager.preloadModules('utils', 'theme', 'forms', 'modal');
            debugLogger.log('Core modules preloaded successfully');
        } catch (error) {
            debugLogger.error('Failed to preload core modules:', error);
        }
    },
    
    Portal: {
        async create(element, options = {}) {
            const portal = await moduleManager.getModule('portal');
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
    
    Choice: {
        async createPortal(choiceElementId) {
            const choice = await moduleManager.getModule('choice');
            return choice.createChoicePortal(choiceElementId);
        },
        
        async destroyPortal(portalId) {
            const choice = await moduleManager.getModule('choice');
            return choice.destroyChoicePortal(portalId);
        }
    },
    
    Modal: {
        async lockScroll() {
            const modal = await moduleManager.getModule('modal');
            return modal.lockScroll();
        },
        
        async unlockScroll() {
            const modal = await moduleManager.getModule('modal');
            return modal.unlockScroll();
        }
    },
    
    async initializeComponent(componentType, elementId, options = {}) {
        const element = document.getElementById(elementId);
        // Let this throw if element doesn't exist - FAIL FAST
        switch (componentType) {
            case 'tabs':
                return this.Tabs.initializeTabs(element, options);
            default:
                throw new Error(`Unknown component type: ${componentType}`);
        }
    }
};

document.addEventListener('DOMContentLoaded', function() {
    document.querySelectorAll('[data-rr-component]').forEach(element => {
        const componentType = element.getAttribute('data-rr-component');
        const options = element.getAttribute('data-rr-options');
        RRBlazor.initializeComponent(componentType, element.id, options ? JSON.parse(options) : {});
    });
    
    RRBlazor.preloadCore();
});

// Direct tab methods for RTabs component
RRBlazor.initializeTabs = async function(element, navContainer, navWrapper) {
    const tabs = await moduleManager.getModule('tabs');
    return tabs.initializeTabs(element, navContainer, navWrapper);
};

RRBlazor.getTabIndicatorPosition = async function(tabElementId, wrapperElement) {
    const tabs = await moduleManager.getModule('tabs');
    return tabs.getTabIndicatorPosition(tabElementId, wrapperElement);
};

window.RRBlazor = window.RRBlazor || RRBlazor;
window.RRDebugLogger = DebugLogger;
window.debugLogger = debugLogger;

window.RRDebug = {
    logger: debugLogger,
    report: async () => {
        if (debugLogger.isDebugMode) {
            const debugModule = await import('./page-debug.js');
            return debugModule.generateReport();
        }
    }
};

if (debugLogger.isDebugMode) {
    import('./page-debug.js').then(debugModule => {
        window.RRDebug.pageDebug = debugModule;
        debugLogger.log('🔧 Debug utilities loaded for development environment');
    });
}