// RR.Blazor JavaScript Helpers
// Supporting functions for component interactions

// Unified Debug Logger - Reused across all RR.Blazor JavaScript files
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

// Create global debug logger instance
const debugLogger = new DebugLogger();

// Export debugLogger for other modules
window.RRDebugLogger = DebugLogger;
window.debugLogger = debugLogger;

// Import and initialize theme system
import * as ThemeModule from './theme.js';

// Import and initialize chart system
import * as ChartModule from './chart.js';

// Import modular components
import * as ChoiceModule from './choice.js';
import * as TabsModule from './tabs.js';
import * as FormsModule from './forms.js';
import * as UtilsModule from './utils.js';

// Make modules available globally
window.RRTheme = ThemeModule;
window.RChart = ChartModule;
window.RRChoice = ChoiceModule;
window.RRTabs = TabsModule;
window.RRForms = FormsModule;
window.RRUtils = UtilsModule;

window.RRBlazor = {
    // Tab management functions (delegated to TabsModule)
    getTabIndicatorPosition: TabsModule.getTabIndicatorPosition,
    getTabScrollInfo: TabsModule.getTabScrollInfo,
    scrollTabsLeft: TabsModule.scrollTabsLeft,
    
    scrollTabsRight: TabsModule.scrollTabsRight,
    
    scrollToTab: TabsModule.scrollToTab,
    
    // Form utilities (delegated to FormsModule)
    autoResizeTextarea: FormsModule.autoResizeTextarea,
    
    focusElement: FormsModule.focusElement,
    
    // Utility functions (delegated to UtilsModule)
    scrollIntoView: UtilsModule.scrollIntoView,
    
    copyToClipboard: FormsModule.copyToClipboard,
    
    
    getElementDimensions: UtilsModule.getElementDimensions,
    
    toggleClass: UtilsModule.toggleClass,
    
    // Component initialization (delegated to respective modules)
    initializeComponent: function(componentType, elementId, options = {}) {
        const element = document.getElementById(elementId);
        if (!element) return;
        
        switch (componentType) {
            case 'tabs':
                TabsModule.initializeTabs(element, options);
                break;
            case 'form-field':
                FormsModule.initializeFormField(element, options);
                break;
        }
    },
    
    initializeTabs: TabsModule.initializeTabs,
    initializeFormField: FormsModule.initializeFormField,
    cleanupComponent: FormsModule.cleanupComponent,
    
    updateUrlWithoutScroll: UtilsModule.updateUrlWithoutScroll,
    
    // Choice/dropdown positioning (delegated to ChoiceModule)
    adjustDropdownPosition: ChoiceModule.adjustDropdownPosition,
    
    adjustChoicePosition: ChoiceModule.adjustChoicePosition,

    // Choice positioning utilities (delegated to ChoiceModule)
    Choice: ChoiceModule.Choice,

    // User menu dropdown management
    setupUserMenuOutsideClick: function(userMenuContainerId, toggleCallback) {
        debugLogger.log('Setting up user menu outside click handler for:', userMenuContainerId);
        
        const userMenuContainer = document.querySelector(userMenuContainerId);
        if (!userMenuContainer) {
            debugLogger.warn('User menu container not found:', userMenuContainerId);
            return;
        }

        const outsideClickHandler = function(event) {
            // Check if click is outside the user menu container
            if (!userMenuContainer.contains(event.target)) {
                debugLogger.log('Outside click detected, closing user menu');
                toggleCallback.invokeMethodAsync('CloseUserMenu');
            }
        };

        // Store handler reference for cleanup
        userMenuContainer._outsideClickHandler = outsideClickHandler;
        
        // Add event listener to document
        document.addEventListener('click', outsideClickHandler);
        
        debugLogger.log('User menu outside click handler attached');
    },

    removeUserMenuOutsideClick: function(userMenuContainerId) {
        const userMenuContainer = document.querySelector(userMenuContainerId);
        if (userMenuContainer && userMenuContainer._outsideClickHandler) {
            document.removeEventListener('click', userMenuContainer._outsideClickHandler);
            delete userMenuContainer._outsideClickHandler;
            debugLogger.log('User menu outside click handler removed');
        }
    }
};

// Global helper for adding event listeners from Blazor
window.addEventListener = UtilsModule.addEventListener;

// Global functions for Blazor interop
// Floating label functions removed - using pure CSS approach

window.updateUrlWithoutScroll = UtilsModule.updateUrlWithoutScroll;


document.addEventListener('DOMContentLoaded', function() {
    // Initialize any components that need it
    document.querySelectorAll('[data-rr-component]').forEach(element => {
        const componentType = element.getAttribute('data-rr-component');
        const options = element.getAttribute('data-rr-options');
        RRBlazor.initializeComponent(componentType, element.id, options ? JSON.parse(options) : {});
    });
    
    // Auto-initialization for floating labels removed - using pure CSS approach
});

// Delegate download functions to UtilsModule
window.RRBlazor.downloadContent = UtilsModule.downloadContent;

window.downloadFileFromStream = UtilsModule.downloadFileFromStream;

// Simple file download from URL
window.downloadFile = UtilsModule.downloadFile;

// ===== DEVELOPMENT DEBUG UTILITIES =====
// Load debug utilities in development environments only
if (debugLogger.isDebugMode) {
    // Import and initialize debug module
    import('./page-debug.js')
        .then(debugModule => {
            // The debug module should export its functionality as window.RRDebug
            debugLogger.log('🔧 Debug utilities loaded from module for development environment');
        })
        .catch(error => {
            debugLogger.warn('Failed to load debug utilities:', error);
        });
}

