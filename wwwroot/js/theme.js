// RR.Blazor Theme System - Unified theme management for all layouts
// Coordinates between RR.Blazor theme system and project-specific theme implementations

let dotNetHelper = null;
let systemThemeListener = null;

export function applyTheme(themeData) {
    const root = document.documentElement;
    
    try {
        // Set theme mode as data attribute
        root.setAttribute('data-theme', themeData.mode);
        console.log('RR.Blazor Theme: Set data-theme attribute to', themeData.mode);
        
        // Apply color variables
        if (themeData.colors) {
            Object.entries(themeData.colors).forEach(([key, value]) => {
                if (value) {
                    root.style.setProperty(`--color-${key.replace(/([A-Z])/g, '-$1').toLowerCase()}`, value);
                }
            });
        }
        
        // Apply custom variables
        if (themeData.customVariables) {
            Object.entries(themeData.customVariables).forEach(([key, value]) => {
                if (value) {
                    root.style.setProperty(key, value);
                }
            });
        }
        
        // Apply animation settings
        if (themeData.animations !== undefined) {
            root.style.setProperty('--theme-animations', themeData.animations ? 'enabled' : 'disabled');
            if (!themeData.animations) {
                root.style.setProperty('--theme-transition-duration', '0s');
            } else {
                root.style.setProperty('--theme-transition-duration', '0.3s');
            }
        }
        
        // Apply accessibility settings
        if (themeData.accessibility) {
            root.classList.add('theme-accessibility');
        } else {
            root.classList.remove('theme-accessibility');
        }
        
        // Apply high contrast mode
        if (themeData.highContrast) {
            root.classList.add('theme-high-contrast');
        } else {
            root.classList.remove('theme-high-contrast');
        }
        
        // Force style recalculation and theme variable updates
        root.style.display = 'none';
        root.offsetHeight; // Trigger reflow
        root.style.display = '';
        
        // Force background redraw for gradient updates
        const bodyEl = document.body;
        if (bodyEl) {
            bodyEl.style.opacity = '0.999';
            setTimeout(() => {
                bodyEl.style.opacity = '';
            }, 1);
        }
        
        // Notify other systems of theme change
        notifyThemeChange(themeData);
        
        return true;
    } catch (error) {
        console.error('Error applying theme:', error);
        return false;
    }
}

export function getSystemDarkMode() {
    return window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
}

export function getSystemHighContrast() {
    return window.matchMedia && window.matchMedia('(prefers-contrast: more)').matches;
}

export function registerSystemThemeListener(dotNetRef) {
    dotNetHelper = dotNetRef;
    
    // Listen for system theme changes
    const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
    const contrastQuery = window.matchMedia('(prefers-contrast: more)');
    
    function handleThemeChange() {
        if (dotNetHelper) {
            dotNetHelper.invokeMethodAsync('OnSystemThemeChanged', mediaQuery.matches);
        }
    }
    
    function handleContrastChange() {
        if (dotNetHelper) {
            dotNetHelper.invokeMethodAsync('OnSystemContrastChanged', contrastQuery.matches);
        }
    }
    
    mediaQuery.addEventListener('change', handleThemeChange);
    contrastQuery.addEventListener('change', handleContrastChange);
    
    // Initial calls
    handleThemeChange();
    handleContrastChange();
    
    systemThemeListener = {
        dispose: () => {
            mediaQuery.removeEventListener('change', handleThemeChange);
            contrastQuery.removeEventListener('change', handleContrastChange);
            dotNetHelper = null;
        }
    };
    
    return systemThemeListener;
}

export function disposeSystemThemeListener() {
    if (systemThemeListener) {
        systemThemeListener.dispose();
        systemThemeListener = null;
    }
}

// Enhanced setTheme export for modules
export function setTheme(theme) {
    if (theme && typeof theme === 'string') {
        const normalizedTheme = theme.toLowerCase();
        const effectiveTheme = normalizedTheme === 'system' ? getEffectiveTheme(normalizedTheme) : normalizedTheme;
        
        const themeData = {
            mode: effectiveTheme,
            colors: {},
            customVariables: {},
            animations: true,
            accessibility: false,
            highContrast: getSystemHighContrast()
        };
        
        return applyTheme(themeData);
    }
    return false;
}

// Utility functions for theme management
export function getEffectiveTheme(themeMode) {
    switch (themeMode) {
        case 'system':
            return getSystemDarkMode() ? 'dark' : 'light';
        case 'dark':
            return 'dark';
        case 'light':
        default:
            return 'light';
    }
}

export function isCurrentlyDark() {
    const theme = document.documentElement.getAttribute('data-theme');
    return theme === 'dark' || (theme === 'system' && getSystemDarkMode());
}

export function getCurrentTheme() {
    return document.documentElement.getAttribute('data-theme') || 'light';
}

// Integration with existing theme systems
function notifyThemeChange(themeData) {
    // Notify MainLayout theme manager if it exists
    if (window.themeManager && typeof window.themeManager.onThemeChanged === 'function') {
        window.themeManager.onThemeChanged(themeData);
    }
    
    // Notify RAppShell if it exists
    if (window.RRAppShell && typeof window.RRAppShell.onThemeChanged === 'function') {
        window.RRAppShell.onThemeChanged(themeData);
    }
    
    // Dispatch custom event for other components
    const event = new CustomEvent('themeChanged', { 
        detail: themeData,
        bubbles: true 
    });
    document.dispatchEvent(event);
}

// CSS Custom Properties helper
export function setCSSVariable(property, value) {
    document.documentElement.style.setProperty(property, value);
}

export function getCSSVariable(property) {
    return getComputedStyle(document.documentElement).getPropertyValue(property).trim();
}

// Theme validation
export function validateTheme(themeData) {
    const requiredProperties = ['mode'];
    const validModes = ['light', 'dark', 'system'];
    
    if (!themeData) return false;
    
    for (const prop of requiredProperties) {
        if (!themeData.hasOwnProperty(prop)) {
            console.warn(`Theme validation failed: missing property '${prop}'`);
            return false;
        }
    }
    
    if (!validModes.includes(themeData.mode)) {
        console.warn(`Theme validation failed: invalid mode '${themeData.mode}'`);
        return false;
    }
    
    return true;
}

// Export theme info for compatibility
export function getThemeInfo() {
    const currentTheme = getCurrentTheme();
    const systemDark = getSystemDarkMode();
    const highContrast = getSystemHighContrast();
    
    return {
        current: currentTheme,
        systemDark: Boolean(systemDark),
        highContrast: Boolean(highContrast),
        effectiveTheme: currentTheme
    };
}

// Global theme utilities
window.RRTheme = {
    applyTheme,
    getSystemDarkMode,
    getSystemHighContrast,
    getEffectiveTheme,
    isCurrentlyDark,
    getCurrentTheme,
    setCSSVariable,
    getCSSVariable,
    validateTheme,
    getThemeInfo
};

// Initialize theme immediately (before DOM load)
(function() {
    // Get stored theme or default to 'system'
    let themeMode = 'system';
    try {
        const stored = localStorage.getItem('rr-blazor-theme');
        if (stored) {
            const config = JSON.parse(stored);
            themeMode = config.Mode || 'system';
        }
    } catch (e) {
        // Ignore errors, use default
    }
    
    // Apply system preference if mode is system
    if (themeMode === 'system' || themeMode === '0') {
        themeMode = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
    } else if (themeMode === '1') {
        themeMode = 'light';
    } else if (themeMode === '2') {
        themeMode = 'dark';
    }
    
    // Apply theme immediately
    document.documentElement.setAttribute('data-theme', themeMode);
})();