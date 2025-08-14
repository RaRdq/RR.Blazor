// RR.Blazor - Universal module proxy system
// Dynamic imports with smart export pattern detection

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
        
        const isProductionOverride = window.RRBlazorConfig?.forceProduction === true;
        
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
            // Critical modules (preloaded)
            'portal': { path: './portal.js', preload: true },
            'backdrop': { path: './backdrop.js', preload: true },
            'positioning': { path: './positioning.js', preload: true },
            'modal': { path: './modal.js', preload: true },
            'choice': { path: './choice.js', preload: true },
            'clickOutside': { path: './click-outside.js', preload: true },
            'keyboardNavigation': { path: './keyboard-navigation.js', preload: true },
            'modalEvents': { path: './modal-events.js', preload: true },
            'choiceEvents': { path: './choice-events.js', preload: true },
            'scrollLock': { path: './scroll-lock.js', preload: true },
            
            // Standard modules (lazy loaded)
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
        
        const results = await Promise.allSettled(
            preloadModules.map(name => this.loadModule(name))
        );
        
        results.forEach((result, index) => {
            if (result.status === 'rejected') {
                debugLogger.error(`Failed to preload ${preloadModules[index]}: ${result.reason}`);
            }
        });
        
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
            
            if (prop === 'toString' || prop === 'valueOf' || prop === Symbol.toStringTag) {
                return () => `[RRBlazor.${moduleName}]`;
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
    
    // Portal system modules
    Portal: createUniversalProxy('portal'),
    Backdrop: createUniversalProxy('backdrop'),
    Positioning: createUniversalProxy('positioning'),
    ClickOutside: createUniversalProxy('clickOutside'),
    KeyboardNavigation: createUniversalProxy('keyboardNavigation'),
    ModalEvents: createUniversalProxy('modalEvents'),
    ChoiceEvents: createUniversalProxy('choiceEvents'),
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
    
    getModule: function(moduleName) {
        return createUniversalProxy(moduleName);
    },
    
    modules: new Proxy({}, {
        get(target, moduleName) {
            return createUniversalProxy(moduleName);
        }
    }),
    
    async initialize() {
        debugLogger.log('RR.Blazor initializing...');
        await moduleManager.preloadCriticalModules();
        
        const portalModule = await moduleManager.loadModule('portal');
        if (portalModule) {
            debugLogger.log('Portal module event listeners registered');
        } else {
            debugLogger.error('Failed to load portal module - dropdowns and modals will not work!');
        }
        
        debugLogger.log('RR.Blazor initialized with universal proxy system');
        return true;
    },
    
    async dispose() {
        debugLogger.log('Disposing RR.Blazor...');
        await moduleManager.dispose();
        debugLogger.log('RR.Blazor disposed');
        return true;
    }
};

window.RRBlazor = RRBlazor;
window.RRFileUpload = createUniversalProxy('fileUpload');
window.RRDebugLogger = DebugLogger;
window.debugLogger = debugLogger;

// Debug utilities for development
window.RRDebug = {
    logger: debugLogger,
    
    async report() {
        if (debugLogger.isDebugMode) {
            const debugModule = await import('./_dev_tools/page-debug.js');
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
    }
};

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => RRBlazor.initialize());
} else {
    RRBlazor.initialize();
}

window.addEventListener('beforeunload', () => {
    RRBlazor.dispose();
});

// Blazor ElementReference validation
window.RRBlazor.elementExists = function(elementReference) {
    try {
        return elementReference && elementReference.tagName !== undefined;
    } catch (e) {
        return false;
    }
};

// Element verification for choice components
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

debugLogger.log('RR.Blazor loaded!');

export { RRBlazor, debugLogger, moduleManager, createUniversalProxy };
export default RRBlazor;