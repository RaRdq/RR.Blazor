// RR.Blazor - Generic Smart Proxy System
// ZERO manual method definitions - pure dynamic proxy
// Handles all module patterns: default exports, named exports, classes

class DebugLogger {
    constructor(prefix = '[RR.Blazor]') {
        this.isDebugMode = this.detectDebugMode();
        this.logPrefix = prefix;
    }

    detectDebugMode() {
        // Check for common development indicators
        const isDevelopmentHost = 
            window.location.hostname === 'localhost' ||
            window.location.hostname === '127.0.0.1' ||
            window.location.hostname.startsWith('192.168.') ||
            window.location.hostname.startsWith('10.');
        
        // Check for common development ports
        const isDevelopmentPort = 
            ['3000', '3001', '4200', '5000', '5001', '5002', '5173', '7049', '8080', '8081']
                .includes(window.location.port);
        
        // Check for explicit debug flag in URL or localStorage
        const hasDebugFlag = 
            window.location.search.includes('debug=true') ||
            localStorage.getItem('RRBlazor.DebugMode') === 'true';
        
        // Check for environment-specific flag (can be set by the host application)
        const isProductionOverride = 
            window.RRBlazorConfig?.forceProduction === true;
        
        return !isProductionOverride && (isDevelopmentHost || isDevelopmentPort || hasDebugFlag);
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

// Module Manager - Smart module loading with preload support
class ModuleManager {
    constructor() {
        this.modules = new Map();
        this.loadingPromises = new Map();
        this.moduleExports = new Map(); // Cache of actual exports
        
        // Module configurations with preload flags
        this.moduleConfigs = {
            // Critical modules - preload for performance
            'portal': { path: './portal.js', preload: true },
            'backdrop': { path: './backdrop.js', preload: true },
            'positioning': { path: './positioning.js', preload: true },
            'modal': { path: './modal.js', preload: true },
            'choice': { path: './choice.js', preload: true },
            'eventManager': { path: './event-manager.js', preload: true },
            'scrollLock': { path: './scroll-lock.js', preload: true },
            
            // Standard modules - lazy load
            'tooltip': { path: './tooltip.js' },
            'focusTrap': { path: './focus-trap.js' },
            'autosuggest': { path: './autosuggest.js' },
            'forms': { path: './forms.js' },
            'tabs': { path: './tabs.js' },
            'datepicker': { path: './datepicker.js' },
            'utils': { path: './utils.js' },
            'theme': { path: './theme.js' },
            'chart': { path: './chart.js' },
            'table': { path: './table.js' },
            'fileUpload': { path: './file-upload.js' },
            'clipboard': { path: './clipboard.js' },
            'loader': { path: './loader.js' },
            'toasts': { path: './toasts.js' },
            'appShell': { path: './app-shell.js' },
            'columnManagement': { path: './column-management.js' },
            'tableScroll': { path: './table-scroll.js' },
            'intersectionObserver': { path: './intersection-observer.js' }
        };
    }

    async loadModule(moduleName) {
        // Return if already loaded
        if (this.modules.has(moduleName)) {
            return this.modules.get(moduleName);
        }

        // Return existing loading promise
        if (this.loadingPromises.has(moduleName)) {
            return this.loadingPromises.get(moduleName);
        }

        const config = this.moduleConfigs[moduleName];
        if (!config) {
            throw new Error(`Module '${moduleName}' is not configured`);
        }

        // Load the module
        const loadPromise = import(config.path)
            .then(moduleExports => {
                // Analyze and cache the module's export structure
                const exportInfo = this.analyzeExports(moduleExports);
                this.moduleExports.set(moduleName, exportInfo);
                
                // Store the raw module exports
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
        // Determine the module's export pattern
        const hasDefault = 'default' in moduleExports;
        const namedExports = Object.keys(moduleExports).filter(key => key !== 'default');
        
        // Classify the module type
        let type = 'mixed';
        let primary = null;
        
        if (hasDefault && namedExports.length === 0) {
            type = 'default-only';
            primary = moduleExports.default;
        } else if (!hasDefault && namedExports.length > 0) {
            type = 'named-only';
            primary = moduleExports;
        } else if (hasDefault && namedExports.length > 0) {
            // Mixed exports - check if default is a comprehensive object
            const defaultExport = moduleExports.default;
            if (typeof defaultExport === 'object' && defaultExport !== null) {
                // Check if default export contains all named exports
                const defaultKeys = Object.keys(defaultExport);
                const hasAllNamed = namedExports.every(name => 
                    defaultKeys.includes(name) || name in defaultExport
                );
                
                if (hasAllNamed) {
                    type = 'default-comprehensive';
                    primary = defaultExport;
                } else {
                    type = 'mixed';
                    primary = moduleExports; // Use all exports
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
        
        // No specific export requested - return primary
        if (!exportName) {
            return exportInfo.primary;
        }
        
        // Look for specific export
        if (exportName in moduleExports) {
            return moduleExports[exportName];
        }
        
        // Check in default export if it's an object
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
            .map(([name]) => this.loadModule(name));
        
        await Promise.allSettled(preloadModules);
        debugLogger.log('🚀 Critical modules preloaded');
    }

    isModuleLoaded(moduleName) {
        return this.modules.has(moduleName);
    }

    async dispose() {
        // Dispose all loaded modules
        for (const [name, moduleExports] of this.modules.entries()) {
            try {
                // Check for dispose in various places
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

// Universal Smart Proxy - Works with ANY export pattern
function createUniversalProxy(moduleName) {
    // Create a proxy that intelligently handles all call patterns
    return new Proxy(function() {}, {
        // Handle direct function calls: proxy()
        async apply(target, thisArg, args) {
            const module = await moduleManager.loadModule(moduleName);
            const exportInfo = moduleManager.moduleExports.get(moduleName);
            
            // If primary export is a function, call it
            if (typeof exportInfo.primary === 'function') {
                return exportInfo.primary.apply(thisArg, args);
            }
            
            // If default export is a function, call it
            if (module.default && typeof module.default === 'function') {
                return module.default.apply(thisArg, args);
            }
            
            throw new Error(`Module '${moduleName}' is not callable`);
        },
        
        // Handle property access: proxy.method() or proxy.property
        get(target, prop) {
            // Ignore Promise protocol methods
            if (prop === 'then' || prop === 'catch' || prop === 'finally') {
                return undefined;
            }
            
            // Handle toString and other special methods
            if (prop === 'toString' || prop === 'valueOf' || prop === Symbol.toStringTag) {
                return () => `[RRBlazor.${moduleName}]`;
            }
            
            // Return async function that loads module and accesses the property
            return async function(...args) {
                const module = await moduleManager.loadModule(moduleName);
                const exportInfo = moduleManager.moduleExports.get(moduleName);
                
                // Search order for the property/method:
                // 1. Direct named export
                // 2. Property on default export
                // 3. Property on primary export
                
                let target = null;
                let context = null;
                
                // Check named exports first
                if (prop in module) {
                    target = module[prop];
                    context = module;
                }
                // Check default export
                else if (module.default && typeof module.default === 'object' && prop in module.default) {
                    target = module.default[prop];
                    context = module.default;
                }
                // Check primary export (might be different from default)
                else if (exportInfo.primary && typeof exportInfo.primary === 'object' && prop in exportInfo.primary) {
                    target = exportInfo.primary[prop];
                    context = exportInfo.primary;
                }
                
                if (target === undefined) {
                    throw new Error(`Property '${prop}' not found in module '${moduleName}'`);
                }
                
                // If it's a function, call it with proper context
                if (typeof target === 'function') {
                    return target.apply(context, args);
                }
                
                // Otherwise return the value
                return target;
            };
        },
        
        // Make the proxy look like a proper object
        has(target, prop) {
            return moduleManager.loadModule(moduleName).then(module => {
                return prop in module || 
                       (module.default && prop in module.default);
            }).catch(() => false);
        },
        
        ownKeys(target) {
            return moduleManager.loadModule(moduleName).then(module => {
                const keys = new Set(Object.keys(module));
                if (module.default && typeof module.default === 'object') {
                    Object.keys(module.default).forEach(key => keys.add(key));
                }
                return Array.from(keys);
            }).catch(() => []);
        },
        
        getOwnPropertyDescriptor(target, prop) {
            return {
                configurable: true,
                enumerable: true,
                get: () => this.get(target, prop)
            };
        }
    });
}

// Main RRBlazor API - Clean, no manual methods
const RRBlazor = {
    version: '2.0.0',
    debug: debugLogger,
    moduleManager: moduleManager,
    
    // Portal system modules
    Portal: createUniversalProxy('portal'),
    Backdrop: createUniversalProxy('backdrop'),
    Positioning: createUniversalProxy('positioning'),
    EventManager: createUniversalProxy('eventManager'),
    ScrollLock: createUniversalProxy('scrollLock'),
    
    // Component modules
    Modal: createUniversalProxy('modal'),
    Choice: createUniversalProxy('choice'),
    Forms: createUniversalProxy('forms'),
    Tabs: createUniversalProxy('tabs'),
    DatePicker: createUniversalProxy('datepicker'),
    Utils: createUniversalProxy('utils'),
    Theme: createUniversalProxy('theme'),
    Chart: createUniversalProxy('chart'),
    Table: createUniversalProxy('table'),
    Clipboard: createUniversalProxy('clipboard'),
    Loader: createUniversalProxy('loader'),
    Toasts: createUniversalProxy('toasts'),
    Tooltip: createUniversalProxy('tooltip'),
    Autosuggest: createUniversalProxy('autosuggest'),
    FocusTrap: createUniversalProxy('focusTrap'),
    FileUpload: createUniversalProxy('fileUpload'),
    AppShell: createUniversalProxy('appShell'),
    ColumnManagement: createUniversalProxy('columnManagement'),
    TableScroll: createUniversalProxy('tableScroll'),
    IntersectionObserver: createUniversalProxy('intersectionObserver'),
    
    // Dynamic module access
    getModule: function(moduleName) {
        return createUniversalProxy(moduleName);
    },
    
    // Module access via property
    modules: new Proxy({}, {
        get(target, moduleName) {
            return createUniversalProxy(moduleName);
        }
    }),
    
    // Initialize and preload critical modules
    async initialize() {
        debugLogger.log('🚀 RR.Blazor initializing...');
        
        // Preload critical modules for performance
        await moduleManager.preloadCriticalModules();
        
        debugLogger.log('✨ RR.Blazor initialized with universal proxy system');
        return true;
    },
    
    // Cleanup
    async dispose() {
        debugLogger.log('Disposing RR.Blazor...');
        await moduleManager.dispose();
        debugLogger.log('RR.Blazor disposed');
        return true;
    }
};

// Export to window
window.RRBlazor = RRBlazor;
window.RRFileUpload = createUniversalProxy('fileUpload');
window.RRDebugLogger = DebugLogger;
window.debugLogger = debugLogger;

// Debug utilities
window.RRDebug = {
    logger: debugLogger,
    
    async report() {
        if (debugLogger.isDebugMode) {
            const debugModule = await import('./page-debug.js');
            return debugModule.generateReport();
        }
    },
    
    listModules() {
        const loaded = Array.from(moduleManager.modules.keys());
        const available = Object.keys(moduleManager.moduleConfigs);
        const preload = Object.entries(moduleManager.moduleConfigs)
            .filter(([name, config]) => config.preload)
            .map(([name]) => name);
        
        console.table({
            'Loaded': loaded.join(', '),
            'Available': available.join(', '),
            'Preloaded': preload.join(', ')
        });
        
        return { loaded, available, preload };
    },
    
    async testModule(moduleName, methodName = null) {
        try {
            const proxy = createUniversalProxy(moduleName);
            if (methodName) {
                const result = await proxy[methodName]();
                console.log(`✅ ${moduleName}.${methodName}() works:`, result);
                return result;
            } else {
                const module = await moduleManager.loadModule(moduleName);
                const info = moduleManager.moduleExports.get(moduleName);
                console.log(`✅ Module '${moduleName}' loaded:`, info);
                return info;
            }
        } catch (error) {
            console.error(`❌ Test failed for ${moduleName}:`, error);
            throw error;
        }
    }
};

// Initialize on DOM ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => RRBlazor.initialize());
} else {
    RRBlazor.initialize();
}

// Cleanup on unload
window.addEventListener('beforeunload', () => {
    RRBlazor.dispose();
});

// Element existence check for Blazor ElementReference validation
window.RRBlazor.elementExists = function(elementReference) {
    try {
        return elementReference && elementReference.tagName !== undefined;
    } catch (e) {
        return false;
    }
};

// Enhanced element verification for choice components
window.RRBlazor.elementExistsInParent = function(choiceId, selector) {
    try {
        const choice = document.querySelector(`[data-choice-id="${choiceId}"]`);
        if (!choice) return false;
        
        const element = choice.querySelector(selector);
        if (!element) return false;
        
        // Verify element is actually in DOM and has dimensions
        const rect = element.getBoundingClientRect();
        const inDocument = document.body.contains(element);
        const hasParent = element.parentElement !== null;
        
        return inDocument && hasParent;
    } catch (e) {
        debugLogger.error(`elementExistsInParent error for ${choiceId}:`, e);
        return false;
    }
};

// DOM state debugging helper
window.RRBlazor.getChoiceDomInfo = function(choiceId) {
    try {
        const choice = document.querySelector(`[data-choice-id="${choiceId}"]`);
        if (!choice) {
            return `Choice element not found with id: ${choiceId}`;
        }
        
        const info = {
            choiceFound: true,
            choiceClasses: choice.className,
            hasViewport: !!choice.querySelector('.choice-viewport'),
            hasTrigger: !!choice.querySelector('.choice-trigger'),
            hasContent: !!choice.querySelector('.choice-content'),
            childCount: choice.children.length,
            innerHTML: choice.innerHTML.substring(0, 200) + '...',
            computedDisplay: window.getComputedStyle(choice).display,
            isConnected: choice.isConnected
        };
        
        return JSON.stringify(info, null, 2);
    } catch (e) {
        return `Error getting DOM info: ${e.message}`;
    }
};

debugLogger.log('✨ RR.Blazor loaded - Universal proxy active!');

// Export for ES6 modules
export { RRBlazor, debugLogger, moduleManager, createUniversalProxy };
export default RRBlazor;