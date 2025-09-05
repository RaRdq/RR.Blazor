// RGrid JavaScript Module - Responsive mode switching and advanced functionality
window.RGridModule = (() => {
    'use strict';

    const debugLogger = window.debugLogger;

    const BREAKPOINTS = {
        xs: 0,
        sm: 640,
        md: 768,
        lg: 1024,
        xl: 1280
    };

    const instances = new Map();
    let resizeObserver;
    let mediaQueryLists = {};

    // Initialize media query listeners
    function initializeMediaQueries() {
        if (Object.keys(mediaQueryLists).length > 0) return;

        Object.entries(BREAKPOINTS).forEach(([breakpoint, minWidth]) => {
            if (breakpoint === 'xs') return; // xs doesn't need a media query
            
            const mediaQuery = window.matchMedia(`(min-width: ${minWidth}px)`);
            mediaQueryLists[breakpoint] = mediaQuery;
            
            mediaQuery.addEventListener('change', handleBreakpointChange);
        });
    }

    // Get current breakpoint
    function getCurrentBreakpoint() {
        const width = window.innerWidth;
        
        if (width >= BREAKPOINTS.xl) return 'xl';
        if (width >= BREAKPOINTS.lg) return 'lg';
        if (width >= BREAKPOINTS.md) return 'md';
        if (width >= BREAKPOINTS.sm) return 'sm';
        return 'xs';
    }

    // Handle breakpoint changes
    function handleBreakpointChange() {
        const currentBreakpoint = getCurrentBreakpoint();
        
        instances.forEach((instance, elementId) => {
            updateGridMode(elementId, instance, currentBreakpoint);
        });
    }

    // Update grid mode based on breakpoint
    function updateGridMode(elementId, instance, breakpoint) {
        const element = document.getElementById(elementId);
        if (!element) return;

        // Get responsive mode for current breakpoint
        const modeProperty = `mode${breakpoint.charAt(0).toUpperCase()}${breakpoint.slice(1)}`;
        const newMode = instance.responsiveModes[modeProperty] || instance.responsiveModes.mode || 'auto';
        
        if (newMode !== instance.currentMode) {
            instance.currentMode = newMode;
            
            // Update CSS classes
            updateGridClasses(element, newMode, breakpoint);
            
            // Notify Blazor component if needed
            if (instance.dotNetRef && instance.dotNetRef.invokeMethodAsync) {
                instance.dotNetRef.invokeMethodAsync('OnJSModeChanged', newMode, breakpoint);
            }
        }
    }

    // Update grid CSS classes
    function updateGridClasses(element, mode, breakpoint) {
        // Remove old mode classes
        const oldClasses = Array.from(element.classList).filter(cls => 
            cls.startsWith('rgrid-mode-') || cls.startsWith('rgrid-breakpoint-')
        );
        element.classList.remove(...oldClasses);
        
        // Add new classes
        element.classList.add(`rgrid-mode-${mode}`);
        element.classList.add(`rgrid-breakpoint-${breakpoint}`);
        
        // Update CSS custom properties for responsive columns
        const instance = instances.get(element.id);
        if (instance && instance.responsiveColumns) {
            const columnsProperty = `columns${breakpoint.charAt(0).toUpperCase()}${breakpoint.slice(1)}`;
            const columns = instance.responsiveColumns[columnsProperty] || instance.responsiveColumns.columns || 'auto';
            
            if (columns !== 'auto') {
                element.style.setProperty('--rgrid-columns', columns);
            }
        }
    }

    // Initialize virtualization intersection observer
    function initializeVirtualization(elementId, options = {}) {
        const element = document.getElementById(elementId);
        if (!element) return;

        const instance = instances.get(elementId);
        if (!instance) return;

        // Create intersection observer for virtualization
        if (!instance.intersectionObserver) {
            instance.intersectionObserver = new IntersectionObserver((entries) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        // Load more items if needed
                        if (instance.dotNetRef && instance.hasMoreItems) {
                            instance.dotNetRef.invokeMethodAsync('LoadMoreItems');
                        }
                    }
                });
            }, {
                root: element,
                rootMargin: '100px',
                threshold: 0.1
            });
        }

        // Observe load more trigger
        const loadMoreTrigger = element.querySelector('.rgrid-load-more-trigger');
        if (loadMoreTrigger) {
            instance.intersectionObserver.observe(loadMoreTrigger);
        }
    }

    // Performance optimization: Debounced resize handler
    function debounce(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }

    // Public API
    return {
        // Initialize RGrid instance
        initialize(elementId, options = {}) {
            const element = document.getElementById(elementId);
            if (!element) {
                debugLogger.warn(`RGrid: Element with ID '${elementId}' not found`);
                return;
            }

            // Initialize media queries on first use
            initializeMediaQueries();

            // Store instance configuration
            const instance = {
                elementId,
                dotNetRef: options.dotNetRef || null,
                responsiveModes: {
                    mode: options.mode || 'auto',
                    modeXs: options.modeXs,
                    modeSm: options.modeSm,
                    modeMd: options.modeMd,
                    modeLg: options.modeLg,
                    modeXl: options.modeXl
                },
                responsiveColumns: {
                    columns: options.columns || 'auto',
                    columnsXs: options.columnsXs,
                    columnsSm: options.columnsSm,
                    columnsMd: options.columnsMd,
                    columnsLg: options.columnsLg,
                    columnsXl: options.columnsXl
                },
                currentMode: null,
                enableVirtualization: options.enableVirtualization || false,
                hasMoreItems: options.hasMoreItems || false,
                intersectionObserver: null
            };

            instances.set(elementId, instance);

            // Set initial mode
            const currentBreakpoint = getCurrentBreakpoint();
            updateGridMode(elementId, instance, currentBreakpoint);

            // Initialize virtualization if enabled
            if (instance.enableVirtualization) {
                initializeVirtualization(elementId, options);
            }

            debugLogger.log(`RGrid initialized: ${elementId} at breakpoint ${currentBreakpoint}`);
        },

        // Update instance configuration
        update(elementId, options = {}) {
            const instance = instances.get(elementId);
            if (!instance) {
                debugLogger.warn(`RGrid: Instance '${elementId}' not found for update`);
                return;
            }

            // Update configuration
            Object.assign(instance.responsiveModes, {
                mode: options.mode || instance.responsiveModes.mode,
                modeXs: options.modeXs !== undefined ? options.modeXs : instance.responsiveModes.modeXs,
                modeSm: options.modeSm !== undefined ? options.modeSm : instance.responsiveModes.modeSm,
                modeMd: options.modeMd !== undefined ? options.modeMd : instance.responsiveModes.modeMd,
                modeLg: options.modeLg !== undefined ? options.modeLg : instance.responsiveModes.modeLg,
                modeXl: options.modeXl !== undefined ? options.modeXl : instance.responsiveModes.modeXl
            });

            Object.assign(instance.responsiveColumns, {
                columns: options.columns || instance.responsiveColumns.columns,
                columnsXs: options.columnsXs !== undefined ? options.columnsXs : instance.responsiveColumns.columnsXs,
                columnsSm: options.columnsSm !== undefined ? options.columnsSm : instance.responsiveColumns.columnsSm,
                columnsMd: options.columnsMd !== undefined ? options.columnsMd : instance.responsiveColumns.columnsMd,
                columnsLg: options.columnsLg !== undefined ? options.columnsLg : instance.responsiveColumns.columnsLg,
                columnsXl: options.columnsXl !== undefined ? options.columnsXl : instance.responsiveColumns.columnsXl
            });

            // Trigger update
            const currentBreakpoint = getCurrentBreakpoint();
            updateGridMode(elementId, instance, currentBreakpoint);
        },

        // Dispose instance
        dispose(elementId) {
            const instance = instances.get(elementId);
            if (instance) {
                // Clean up intersection observer
                if (instance.intersectionObserver) {
                    instance.intersectionObserver.disconnect();
                }
                
                instances.delete(elementId);
                debugLogger.log(`RGrid disposed: ${elementId}`);
            }
        },

        // Get current state
        getState(elementId) {
            const instance = instances.get(elementId);
            if (!instance) return null;

            return {
                currentMode: instance.currentMode,
                currentBreakpoint: getCurrentBreakpoint(),
                itemCount: instance.itemCount || 0,
                isVirtualized: instance.enableVirtualization
            };
        },

        // Force refresh
        refresh(elementId) {
            const instance = instances.get(elementId);
            if (instance) {
                const currentBreakpoint = getCurrentBreakpoint();
                updateGridMode(elementId, instance, currentBreakpoint);
            }
        },

        // Utility functions
        getCurrentBreakpoint,
        
        // Debug information
        getDebugInfo() {
            return {
                instances: instances.size,
                currentBreakpoint: getCurrentBreakpoint(),
                mediaQueries: Object.keys(mediaQueryLists)
            };
        }
    };
})();

// Auto-dispose instances when page unloads
window.addEventListener('beforeunload', () => {
    if (window.RGridModule) {
        const instances = window.RGridModule.getDebugInfo().instances;
        debugLogger.log(`Auto-disposing ${instances} RGrid instances`);
    }
});