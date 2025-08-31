
class DebugLogger {
    constructor(prefix = '[RR.Blazor]') {
        this.isDebugMode = this.detectDebugMode();
        this.logPrefix = prefix;
    }

    detectDebugMode() {
        const isDevelopmentHost = window.location.hostname === 'localhost' ||
            window.location.hostname === '127.0.0.1' ||
            window.location.hostname.startsWith('192.168.') ||
            window.location.hostname.startsWith('10.');
        
        
        const hasDebugFlag = window.location.search.includes('debug=true') ||
            localStorage.getItem('RRBlazor.DebugMode') === 'true';
        
        const isProductionOverride = window.RRBlazorConfig && window.RRBlazorConfig.forceProduction === true;
        
        return !isProductionOverride && (isDevelopmentHost || hasDebugFlag);
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

    table(data) {
        if (this.isDebugMode) {
            console.table(data);
        }
    }

    time(label) {
        if (this.isDebugMode) {
            console.time(`${this.logPrefix} ${label}`);
        }
    }

    timeEnd(label) {
        if (this.isDebugMode) {
            console.timeEnd(`${this.logPrefix} ${label}`);
        }
    }
}

const debugLogger = new DebugLogger();

class ModuleManager {
    constructor() {
        this.modules = new Map();
        this.loadingPromises = new Map();
        this.moduleExports = new Map();
        
        this.moduleConfigs = {
            'eventConstants': { path: './event-constants.js', preload: true },
            'portal': { path: './portal.js', preload: true },
            'backdrop': { path: './backdrop.js', preload: true },
            'positioning': { path: './positioning.js', preload: true },
            'uiCoordinator': { path: './ui-coordinator.js', preload: true },
            'modal': { path: './modal.js', preload: true },
            'choice': { path: './choice.js', preload: true },
            'clickOutside': { path: './click-outside.js', preload: true },
            'keyboardNavigation': { path: './keyboard-navigation.js', preload: true },
            'modalEvents': { path: './modal-events.js', preload: true },
            'scrollLock': { path: './scroll-lock.js', preload: true },
            'tooltip': { path: './tooltip.js' },
            'focusTrap': { path: './focus-trap.js' },
            'autosuggest': { path: './autosuggest.js' },
            'forms': { path: './forms.js' },
            'tabs': { path: './tabs.js' },
            'datepicker': { path: './datepicker.js' },
            'utils': { path: './utils.js' },
            'theme': { path: './theme.js', preload: true },
            'chart': { path: './chart.js' },
            'table': { path: './table.js' },
            'tableScroll': { path: './table-scroll.js' },
            'fileUpload': { path: './file-upload.js' },
            'clipboard': { path: './clipboard.js' },
            'loader': { path: './loader.js' },
            'toasts': { path: './toasts.js' },
            'appShell': { path: './app-shell.js', preload: true },
            'columnManagement': { path: './column-management.js' },
            'intersectionObserver': { path: './intersection-observer.js' },
            'filter': { path: './filter.js', preload: true },
            'rgrid': { path: './rgrid.js' },
            'skeleton': { path: './skeleton.js', preload: true },
            'pageDebug': { path: './_dev_tools/page-debug.js', preload: true }
        };
    }

    async loadModule(moduleName) {
        if (this.modules.has(moduleName)) {
            return this.modules.get(moduleName);
        }

        if (this.loadingPromises.has(moduleName)) {
            return this.loadingPromises.get(moduleName);
        }

        const config = this.moduleConfigs[moduleName];
        if (!config) {
            throw new Error(`Module '${moduleName}' is not configured`);
        }

        const loadPromise = import(config.path)
            .then(moduleExports => {
                const exportInfo = this.analyzeExports(moduleExports);
                this.moduleExports.set(moduleName, exportInfo);
                this.modules.set(moduleName, moduleExports);
                this.loadingPromises.delete(moduleName);
                debugLogger.log(`✅ Module '${moduleName}' loaded (${exportInfo.type})`);
                return moduleExports;
            })
            .catch(error => {
                this.loadingPromises.delete(moduleName);
                debugLogger.error(`❌ Failed to load module '${moduleName}':`, error);
                throw error;
            });

        this.loadingPromises.set(moduleName, loadPromise);
        return loadPromise;
    }

    analyzeExports(moduleExports) {
        const hasDefault = 'default' in moduleExports;
        const namedExports = Object.keys(moduleExports).filter(key => key !== 'default');
        
        let type = 'mixed';
        let primary = null;
        
        if (hasDefault && namedExports.length === 0) {
            type = 'default-only';
            primary = moduleExports.default;
        } else if (!hasDefault && namedExports.length > 0) {
            type = 'named-only';
            primary = moduleExports;
        } else if (hasDefault && namedExports.length > 0) {
            const defaultExport = moduleExports.default;
            if (typeof defaultExport === 'object' && defaultExport !== null) {
                const defaultKeys = Object.keys(defaultExport);
                const hasAllNamed = namedExports.every(name => 
                    defaultKeys.includes(name) || name in defaultExport
                );
                
                if (hasAllNamed) {
                    type = 'default-comprehensive';
                    primary = defaultExport;
                } else {
                    type = 'mixed';
                    primary = moduleExports;
                }
            } else {
                type = 'mixed';
                primary = moduleExports;
            }
        }
        
        return { type, primary, hasDefault, namedExports };
    }

    getModuleExport(moduleName, exportName = null) {
        const moduleExports = this.modules.get(moduleName);
        if (!moduleExports) return null;
        
        const exportInfo = this.moduleExports.get(moduleName);
        if (!exportInfo) return null;
        
        if (!exportName) {
            return exportInfo.primary;
        }
        
        if (exportName in moduleExports) {
            return moduleExports[exportName];
        }
        
        if (exportInfo.hasDefault && typeof moduleExports.default === 'object') {
            if (exportName in moduleExports.default) {
                return moduleExports.default[exportName];
            }
        }
        
        return null;
    }

    async preloadCriticalModules() {
        const preloadModules = Object.entries(this.moduleConfigs)
            .filter(([name, config]) => config.preload)
            .map(([name]) => name);
        
        debugLogger.log(`Preloading critical modules: ${preloadModules.join(', ')}`);
        
        // Load event-constants first as other modules depend on it
        try {
            const eventConstantsModule = await this.loadModule('eventConstants');
            if (eventConstantsModule) {
                // Make events available globally immediately
                window.RRBlazor = window.RRBlazor || {};
                window.RRBlazor.Events = eventConstantsModule.EVENTS || eventConstantsModule.default;
                window.RRBlazor.EventPriorities = eventConstantsModule.EVENT_PRIORITIES;
                window.RRBlazor.ComponentTypes = eventConstantsModule.COMPONENT_TYPES;
                window.RRBlazor.EventDispatcher = eventConstantsModule.EventDispatcher;
                debugLogger.log('Event constants loaded and exposed early');
            }
        } catch (error) {
            debugLogger.error('Failed to preload eventConstants:', error);
        }
        
        const essentialModules = ['portal', 'positioning', 'backdrop', 'uiCoordinator', 'clickOutside', 'scrollLock', 'modalEvents', 'keyboardNavigation', 'modal', 'choice', 'filter', 'theme', 'appShell'];
        
        for (const moduleName of essentialModules) {
            await this.loadModule(moduleName);
        }
        
        const modulesToInit = ['filter', 'choice', 'modal'];
        for (const moduleName of modulesToInit) {
            const module = this.modules.get(moduleName);
            const moduleObject = module?.default || module;
            if (moduleObject?.initialize) {
                await moduleObject.initialize();
                debugLogger.log(`Initialized ${moduleName}`);
            }
        }
        
        const portalModule = this.modules.get('portal');
        if (portalModule) {
            const portalManager = portalModule.PortalManager || portalModule.default;
            if (portalManager?.getInstance) {
                portalManager.getInstance();
                debugLogger.log('Portal manager instantiated');
            }
        }
        
        debugLogger.log('Critical modules preloaded');
    }

    isModuleLoaded(moduleName) {
        return this.modules.has(moduleName);
    }

    async dispose() {
        for (const [name, moduleExports] of this.modules.entries()) {
            try {
                if (typeof moduleExports.dispose === 'function') {
                    await moduleExports.dispose();
                } else if (moduleExports.default && typeof moduleExports.default.dispose === 'function') {
                    await moduleExports.default.dispose();
                }
            } catch (error) {
                debugLogger.warn(`Failed to dispose module '${name}':`, error);
            }
        }
        
        this.modules.clear();
        this.loadingPromises.clear();
        this.moduleExports.clear();
        debugLogger.log('All modules disposed');
    }
}

const moduleManager = new ModuleManager();

// Universal proxy for dynamic module loading
function createUniversalProxy(moduleName) {
    const propertyCache = new Map();
    
    return new Proxy(function() {}, {
        async apply(target, thisArg, args) {
            const module = await moduleManager.loadModule(moduleName);
            const exportInfo = moduleManager.moduleExports.get(moduleName);
            
            if (typeof exportInfo.primary === 'function') {
                return exportInfo.primary.apply(thisArg, args);
            }
            
            if (module.default && typeof module.default === 'function') {
                return module.default.apply(thisArg, args);
            }
            
            throw new Error(`Module '${moduleName}' is not callable`);
        },
        
        get(target, prop) {
            if (prop === 'then' || prop === 'catch' || prop === 'finally') {
                return undefined;
            }
            
            if (prop === 'toString' || prop === 'valueOf' || prop === Symbol.toStringTag || prop === Symbol.toPrimitive) {
                if (prop === 'toString') return `[RRBlazor.${moduleName}]`;
                if (prop === 'valueOf') return `[RRBlazor.${moduleName}]`;
                if (prop === Symbol.toStringTag) return `RRBlazor.${moduleName}`;
                if (prop === Symbol.toPrimitive) return () => `[RRBlazor.${moduleName}]`;
            }
            
            if (prop === 'prototype') {
                return undefined;
            }
            
            if (propertyCache.has(prop) && moduleManager.isModuleLoaded(moduleName)) {
                const cached = propertyCache.get(prop);
                return function(...args) {
                    if (typeof cached.target === 'function') {
                        return cached.target.apply(cached.context, args);
                    }
                    return cached.target;
                };
            }
            
            return async function(...args) {
                const module = await moduleManager.loadModule(moduleName);
                const exportInfo = moduleManager.moduleExports.get(moduleName);
                
                let target = null;
                let context = null;
                
                if (prop in module) {
                    target = module[prop];
                    context = module;
                }
                else if (module.default && typeof module.default === 'object' && prop in module.default) {
                    target = module.default[prop];
                    context = module.default;
                }
                else if (exportInfo.primary && typeof exportInfo.primary === 'object' && prop in exportInfo.primary) {
                    target = exportInfo.primary[prop];
                    context = exportInfo.primary;
                }
                
                if (target === undefined) {
                    throw new Error(`Property '${prop}' not found in module '${moduleName}'`);
                }
                
                propertyCache.set(prop, { target, context });
                
                if (typeof target === 'function') {
                    return target.apply(context, args);
                }
                
                return target;
            };
        },
        
        has(target, prop) {
            return moduleManager.loadModule(moduleName).then(module => {
                return prop in module || 
                       (module.default && prop in module.default);
            }).catch(() => false);
        },
        
        ownKeys(target) {
            return ['prototype', 'length', 'name', 'constructor'];
        },
        
        getOwnPropertyDescriptor(target, prop) {
            if (['prototype', 'length', 'name', 'constructor'].includes(prop)) {
                return {
                    configurable: true,
                    enumerable: false,
                    value: undefined
                };
            }
            return {
                configurable: true,
                enumerable: true,
                value: undefined
            };
        }
    });
}

const RRBlazor = {
    version: '2.0.0',
    debug: debugLogger,
    moduleManager: moduleManager,
    
    EventConstants: createUniversalProxy('eventConstants'),
    Portal: createUniversalProxy('portal'),
    Backdrop: createUniversalProxy('backdrop'),
    Positioning: createUniversalProxy('positioning'),
    UICoordinator: createUniversalProxy('uiCoordinator'),
    ClickOutside: createUniversalProxy('clickOutside'),
    KeyboardNavigation: createUniversalProxy('keyboardNavigation'),
    ModalEvents: createUniversalProxy('modalEvents'),
    ScrollLock: createUniversalProxy('scrollLock'),
    
    Modal: createUniversalProxy('modal'),
    Choice: createUniversalProxy('choice'),
    Filter: createUniversalProxy('filter'),
    Forms: createUniversalProxy('forms'),
    Tabs: createUniversalProxy('tabs'),
    DatePicker: createUniversalProxy('datepicker'),
    Utils: createUniversalProxy('utils'),
    Theme: createUniversalProxy('theme'),
    Chart: createUniversalProxy('chart'),
    Table: createUniversalProxy('table'),
    TableScroll: createUniversalProxy('tableScroll'),
    Grid: createUniversalProxy('grid'),
    Clipboard: createUniversalProxy('clipboard'),
    Loader: createUniversalProxy('loader'),
    Toasts: createUniversalProxy('toasts'),
    Tooltip: createUniversalProxy('tooltip'),
    Autosuggest: createUniversalProxy('autosuggest'),
    FocusTrap: createUniversalProxy('focusTrap'),
    FileUpload: createUniversalProxy('fileUpload'),
    AppShell: createUniversalProxy('appShell'),
    ColumnManagement: createUniversalProxy('columnManagement'),
    IntersectionObserver: createUniversalProxy('intersectionObserver'),
    
    getModule: function(moduleName) {
        return createUniversalProxy(moduleName);
    },
    
    async getModuleReference(moduleName) {
        const module = await moduleManager.loadModule(moduleName);
        return module.default || module;
    },
    
    modules: new Proxy({}, {
        get(target, moduleName) {
            return createUniversalProxy(moduleName);
        }
    }),
    
    async initialize() {
        debugLogger.log('RR.Blazor initializing...');
        
        // Check if we're in a browser context and DOM is ready
        if (typeof window === 'undefined' || typeof document === 'undefined') {
            debugLogger.warn('Not in browser context - initialization deferred');
            return false;
        }
        
        if (document.readyState === 'loading') {
            debugLogger.log('DOM still loading - initialization deferred');
            return false;
        }
        
        try {
            await moduleManager.preloadCriticalModules();
            
            const portalModule = moduleManager.modules.get('portal');
            if (portalModule) {
                debugLogger.log('Portal module event listeners registered');
            } else {
                debugLogger.warn('Portal module not loaded - some features may be limited');
            }
            
            debugLogger.log('RR.Blazor initialized with universal proxy system');
            return true;
        } catch (error) {
            debugLogger.error('Failed to initialize RR.Blazor:', error);
            return false;
        }
    },
    
    async loadModule(moduleName) {
        return await moduleManager.loadModule(moduleName);
    },
    
    async dispose() {
        debugLogger.log('Disposing RR.Blazor...');
        await moduleManager.dispose();
        debugLogger.log('RR.Blazor disposed');
        return true;
    }
};

window.RRBlazor = RRBlazor;
window.moduleManager = moduleManager;

window.RRBlazor.isSafeForInterop = function() {
    if (typeof window === 'undefined' || typeof document === 'undefined') {
        return false;
    }
    
    if (document.readyState === 'loading') {
        return false;
    }
    
    if (window.Blazor && window.Blazor._internal) {
        if (window.Blazor._internal.navigationManager) {
            return true;
        }
        return false;
    }
    
    if (window.Blazor && !window.Blazor._internal) {
        return true;
    }
    
    return true;
};

window.RRBlazor.safeInvoke = function(method, ...args) {
    if (!window.RRBlazor.isSafeForInterop()) {
        return Promise.resolve(false);
    }
    
    try {
        const parts = method.split('.');
        let target = window;
        
        for (let i = 0; i < parts.length - 1; i++) {
            target = target[parts[i]];
            if (!target) return Promise.resolve(false);
        }
        
        const func = target[parts[parts.length - 1]];
        if (typeof func !== 'function') return Promise.resolve(false);
        
        const result = func.apply(target, args);
        return result instanceof Promise ? result : Promise.resolve(result);
    } catch {
        return Promise.resolve(false);
    }
};
window.RRFileUpload = createUniversalProxy('fileUpload');
window.RRDebugLogger = DebugLogger;
window.debugLogger = debugLogger;

// Universal initialization that works with both Server and WebAssembly
function initializeWhenReady() {
    if (typeof window === 'undefined' || typeof document === 'undefined') {
        // Not in browser context - likely server-side rendering
        return;
    }
    
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => {
            debugLogger.log('DOM loaded - attempting initialization');
            RRBlazor.initialize();
        });
    } else {
        // DOM already loaded
        debugLogger.log('DOM already loaded - attempting immediate initialization');
        RRBlazor.initialize();
    }
    
    // For Blazor Server - retry initialization when Blazor is fully connected
    // Check if Blazor has already started to avoid double initialization
    if (window.Blazor && window.Blazor.start && !window.Blazor._internal) {
        window.Blazor.start().then(() => {
            debugLogger.log('Blazor Server connected - retrying initialization');
            RRBlazor.initialize();
        }).catch((error) => {
            // Blazor has already started, which is fine
            if (error.message && error.message.includes('already started')) {
                debugLogger.log('Blazor already started - skipping duplicate initialization');
            } else {
                debugLogger.error('Blazor start error:', error);
            }
        });
    }
    
    // Listen for Blazor circuit reconnection
    if (window.Blazor && window.Blazor.reconnect) {
        document.addEventListener('DOMContentLoaded', () => {
            window.Blazor.defaultReconnectionHandler = {
                onConnectionDown: () => debugLogger.warn('Blazor Server connection lost'),
                onConnectionUp: () => {
                    debugLogger.log('Blazor Server reconnected - retrying initialization');
                    RRBlazor.initialize();
                }
            };
        });
    }
}

initializeWhenReady();

// Load debug tools in development only
if (debugLogger.isDebugMode) {
    moduleManager.loadModule('pageDebug').catch(error => {
        debugLogger.error('Failed to load debug tools:', error);
    });
}

window.addEventListener('beforeunload', () => {
    RRBlazor.dispose();
});

window.RRBlazor.elementExists = function(elementReference) {
    try {
        return elementReference && elementReference.tagName !== undefined;
    } catch (e) {
        return false;
    }
};

window.RRBlazor.elementExistsInParent = function(choiceId, selector) {
    try {
        const choice = document.querySelector(`[data-choice-id="${choiceId}"]`);
        if (!choice) return false;
        
        const element = choice.querySelector(selector);
        if (!element) return false;
        
        const inDocument = document.body.contains(element);
        const hasParent = element.parentElement !== null;
        
        return inDocument && hasParent;
    } catch (e) {
        debugLogger.error(`elementExistsInParent error for ${choiceId}:`, e);
        return false;
    }
};

// File download utility - delegates to utils module
window.downloadFile = function(base64Data, filename, mimeType) {
    if (window.RRBlazor && window.RRBlazor.Utils) {
        // Convert base64 to content and use downloadContent
        try {
            const byteCharacters = atob(base64Data);
            return window.RRBlazor.Utils.downloadContent(byteCharacters, filename, mimeType || 'application/octet-stream');
        } catch (error) {
            console.error('[RR.Blazor] Failed to process base64 data:', error);
            return false;
        }
    }
    console.warn('[RR.Blazor] Utils module not loaded - download failed');
    return false;
};

debugLogger.log('RR.Blazor loaded!');

export { RRBlazor, debugLogger, moduleManager, createUniversalProxy };
export default RRBlazor;