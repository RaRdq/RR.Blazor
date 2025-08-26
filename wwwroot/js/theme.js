
let dotNetHelper = null;
let systemThemeListener = null;

const debugLogger = window.debugLogger;

export function applyTheme(themeData) {
    const root = document.documentElement;
    
    root.setAttribute('data-theme', themeData.mode);
        if (themeData.colors) {
            Object.entries(themeData.colors).forEach(([key, value]) => {
                if (value) {
                    root.style.setProperty(`--color-${key.replace(/([A-Z])/g, '-$1').toLowerCase()}`, value);
                }
            });
        }
        
        if (themeData.customVariables) {
            Object.entries(themeData.customVariables).forEach(([key, value]) => {
                if (value) {
                    root.style.setProperty(key, value);
                }
            });
        }
        
        if (themeData.animations !== undefined) {
            root.style.setProperty('--theme-animations', themeData.animations ? 'enabled' : 'disabled');
            if (!themeData.animations) {
                root.style.setProperty('--theme-transition-duration', '0s');
            } else {
                root.style.setProperty('--theme-transition-duration', '0.3s');
            }
        }
        
        if (themeData.accessibility) {
            root.classList.add('theme-accessibility');
        } else {
            root.classList.remove('theme-accessibility');
        }
        
        if (themeData.highContrast) {
            root.classList.add('theme-high-contrast');
        } else {
            root.classList.remove('theme-high-contrast');
        }
        
        root.style.display = 'none';
        root.offsetHeight;
        root.style.display = '';
        
        const bodyEl = document.body;
        if (bodyEl) {
            bodyEl.style.opacity = '0.999';
            requestAnimationFrame(() => {
                bodyEl.style.opacity = '';
            });
        }
        
        notifyThemeChange(themeData);
        
        return true;
}

export function getSystemDarkMode() {
    return window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
}

export function getSystemHighContrast() {
    return window.matchMedia && window.matchMedia('(prefers-contrast: more)').matches;
}

export function registerSystemThemeListener(dotNetRef) {
    dotNetHelper = dotNetRef;
    
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

function notifyThemeChange(themeData) {
    if (window.themeManager && typeof window.themeManager.onThemeChanged === 'function') {
        window.themeManager.onThemeChanged(themeData);
    }
    
    if (window.RRAppShell && typeof window.RRAppShell.onThemeChanged === 'function') {
        window.RRAppShell.onThemeChanged(themeData);
    }
    
    window.RRBlazor.EventDispatcher.dispatch(
        window.RRBlazor.Events.THEME_CHANGED,
        themeData
    );
}

export function setCSSVariable(property, value) {
    document.documentElement.style.setProperty(property, value);
}

export function getCSSVariable(property) {
    return getComputedStyle(document.documentElement).getPropertyValue(property).trim();
}

export function validateTheme(themeData) {
    if (!themeData) throw new Error('Theme data required');
    if (!themeData.mode) throw new Error('Theme mode required');
    if (!['light', 'dark', 'system'].includes(themeData.mode)) {
        throw new Error(`Invalid theme mode: ${themeData.mode}`);
    }
    return true;
}

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

(function() {
    let themeMode = 'system';
    const stored = localStorage.getItem('rr-blazor-theme');
    if (stored) {
        const config = JSON.parse(stored);
        const mode = config.Mode;
        if (typeof mode === 'number') {
            themeMode = mode === 0 ? 'system' : mode === 1 ? 'light' : mode === 2 ? 'dark' : 'system';
        } else if (typeof mode === 'string') {
            themeMode = mode.toLowerCase();
        }
    }
    
    if (themeMode === 'system' || themeMode === 0) {
        const prefersDark = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
        themeMode = prefersDark ? 'dark' : 'light';
    }
    
    document.documentElement.setAttribute('data-theme', themeMode);
})();

function initialize() {
    return true;
}

function cleanup() {
    disposeSystemThemeListener();
}

export default {
    apply: applyTheme,
    applyTheme,
    setTheme,
    registerSystemThemeListener,
    disposeSystemThemeListener,
    isCurrentlyDark,
    getCurrentTheme,
    getEffectiveTheme,
    validateTheme,
    getThemeInfo,
    initialize,
    cleanup
};