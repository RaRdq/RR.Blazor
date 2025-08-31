let appShellDotNetRef = null;

export function initialize() {
    setupKeyboardShortcuts();
    setupClickOutside();
    setupResponsive();
    setupAccessibility();
    setupSearchClickOutside();
}

export function setDotNetRef(dotNetRef) {
    appShellDotNetRef = dotNetRef;
}

export function expandSearch() {
    const searchContainer = document.querySelector('[data-search-container]');
    
    if (searchContainer) {
        window.RRBlazor.EventDispatcher.dispatch(
            'search-expanded',
            { searchContainer }
        );
    }
}


export function isMobile() {
    return window.innerWidth < 768;
}

export function isTablet() {
    return window.innerWidth >= 768 && window.innerWidth < 1024;
}

export function isDesktop() {
    return window.innerWidth >= 1024;
}

function setupKeyboardShortcuts() {
    document.addEventListener('keydown', (e) => {
        if (e.ctrlKey || e.metaKey) {
            switch (e.key) {
                case 'k':
                case 'K':
                    e.preventDefault();
                    focusSearch();
                    break;
                case 'b':
                case 'B':
                    e.preventDefault();
                    toggleSidebar();
                    break;
                case ',':
                    e.preventDefault();
                    navigateTo('/settings');
                    break;
            }
        }
        
        if (e.key === 'Escape') {
            closeAllDropdowns();
            closeSearch();
        }
    });
}

function setupClickOutside() {
    document.addEventListener('click', (e) => {
        // More specific targeting for search elements
        const searchContainer = e.target.closest('[data-search-container], .search-autosuggest, .autosuggest-viewport, .autosuggest-dropdown');
        const searchButton = e.target.closest('.search-toggle-button, [aria-label="Open search"]');
        
        // Only close search if clicked completely outside search area AND search results
        if (!searchContainer && !searchButton) {
            // Add delay to prevent interference with search result selection
            setTimeout(() => closeSearch(), 100);
        }
        
        const dropdown = e.target.closest('.dropdown');
        if (!dropdown) {
            closeAllDropdowns();
        }
    });
}

function setupResponsive() {
    let resizeTimeout;
    
    const handleMobileCollapse = () => {
        document.documentElement.style.setProperty('--is-mobile', isMobile() ? '1' : '0');
        document.documentElement.style.setProperty('--is-tablet', isTablet() ? '1' : '0');
        document.documentElement.style.setProperty('--is-desktop', isDesktop() ? '1' : '0');
        
        const sidebar = document.querySelector('.app-sidebar');
        const mainContent = document.querySelector('.app-main');
        
        if (isMobile()) {
            sidebar.classList.add('collapsed');
            document.documentElement.classList.add('mobile-layout');
        } else {
            sidebar.classList.remove('collapsed');
            document.documentElement.classList.remove('mobile-layout');
        }
    };
    
    let resizeScheduled = false;
    window.addEventListener('resize', () => {
        if (!resizeScheduled) {
            resizeScheduled = true;
            requestAnimationFrame(() => {
                handleMobileCollapse();
                resizeScheduled = false;
            });
        }
    });
    
    handleMobileCollapse();
}

function setupAccessibility() {
    document.addEventListener('keydown', (e) => {
        if (e.key === 'Tab') {
            document.body.classList.add('keyboard-navigation');
        }
    });
    
    document.addEventListener('mousedown', () => {
        document.body.classList.remove('keyboard-navigation');
    });
    
    const announcer = document.createElement('div');
    announcer.setAttribute('aria-live', 'polite');
    announcer.setAttribute('aria-atomic', 'true');
    announcer.className = 'sr-only';
    announcer.id = 'app-shell-announcer';
    document.body.appendChild(announcer);
}

function setupSearchClickOutside() {
}

function focusSearch() {
    const searchInput = document.querySelector('input[type="search"], .search-container input');
    if (searchInput) {
        searchInput.focus();
        searchInput.select();
        announce('Search activated');
    }
}

export function focusElement(selector) {
    const element = selector.startsWith('#') 
        ? document.getElementById(selector.substring(1))
        : document.querySelector(selector);
    
    if (element) {
        element.focus();
        return true;
    }
    return false;
}

export function focusSearchInput(searchId) {
    const autosuggestContainer = document.querySelector(`[data-autosuggest-id="${searchId}"]`);
    if (autosuggestContainer) {
        const input = autosuggestContainer.querySelector('input[type="text"], input[type="search"]');
        if (input) {
            input.focus();
            input.select();
            return true;
        }
    }
    
    return focusElement('input[placeholder*="Search"]');
}

function toggleSidebar() {
    const sidebar = document.querySelector('.sidebar');
    const toggleButton = document.querySelector('.header__toggle, [aria-label="Toggle sidebar"]');
    
    if (sidebar && toggleButton) {
        toggleButton.click();
    }
}

function closeSearch() {
    const searchContainer = document.querySelector('[data-search-container]');
    const searchIconContainer = searchContainer?.querySelector('.search-icon-container');
    if (searchIconContainer && searchIconContainer.classList.contains('search-expanded')) {
        // Only close search if user explicitly clicked outside - not on every event
        const activeElement = document.activeElement;
        const isSearchInput = activeElement?.closest('.search-autosuggest') || 
                             activeElement?.tagName === 'INPUT' && activeElement?.placeholder?.toLowerCase().includes('search');
        
        // Don't auto-close if user is still typing in search
        if (!isSearchInput && appShellDotNetRef) {
            // Use throttled approach to prevent rapid calls
            clearTimeout(window._searchCollapseTimeout);
            window._searchCollapseTimeout = setTimeout(() => {
                appShellDotNetRef.invokeMethodAsync('OnSearchCollapsed');
            }, 200);
        }
    }
}

function closeAllDropdowns() {
    window.RRBlazor.Choice.closeAllDropdowns();
}


function navigateTo(url) {
    if (url && (url.startsWith('/') || url.startsWith('./') || url.startsWith('../') || url.startsWith('http'))) {
        window.location.href = url;
    }
}

function announce(message) {
    const announcer = document.getElementById('app-shell-announcer');
    if (announcer) {
        announcer.textContent = message;
        requestAnimationFrame(() => {
            requestAnimationFrame(() => {
                announcer.textContent = '';
            });
        });
    }
}

export function registerSystemThemeListener(dotNetRef) {
    const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
    
    const handleThemeChange = (e) => {
        dotNetRef.invokeMethodAsync('OnSystemThemeChanged', e.matches, false);
    };
    
    mediaQuery.addEventListener('change', handleThemeChange);
    handleThemeChange(mediaQuery);
    
    return {
        dispose: () => {
            mediaQuery.removeEventListener('change', handleThemeChange);
        }
    };
}

export function getPerformanceMetrics() {
    if (!window.performance) return null;
    
    const navigation = performance.getEntriesByType('navigation')[0];
    const paint = performance.getEntriesByType('paint');
    const firstPaint = paint.find(p => p.name === 'first-paint');
    const firstContentfulPaint = paint.find(p => p.name === 'first-contentful-paint');
    
    return {
        loadTime: navigation.loadEventEnd - navigation.loadEventStart,
        domContentLoaded: navigation.domContentLoadedEventEnd - navigation.domContentLoadedEventStart,
        firstPaint: firstPaint.startTime,
        firstContentfulPaint: firstContentfulPaint.startTime
    };
}

export function scrollToTop() {
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

export function scrollToElement(selector) {
    const element = document.querySelector(selector);
    element.scrollIntoView({ behavior: 'smooth', block: 'start' });
}

export function copyToClipboard(text) {
    return window.RRBlazor.Clipboard.writeText(text).then(() => {
        announce('Copied to clipboard');
        return true;
    }).catch(() => {
        announce('Failed to copy to clipboard');
        return false;
    });
}

export function updateUrlWithoutScroll(newUrl) {
    return RRBlazor.updateUrlWithoutScroll(newUrl);
}

export function dispose() {
}

window.RRAppShell = {
    isMobile,
    isTablet,
    isDesktop,
    focusSearch,
    toggleSidebar,
    scrollToTop,
    scrollToElement,
    copyToClipboard,
    getPerformanceMetrics,
    updateUrlWithoutScroll,
    focusElement,
    initialize,
    setDotNetRef,
    expandSearch,
    focusSearchInput,
    registerSystemThemeListener,
    dispose
};

export default {
    isMobile,
    isTablet,
    isDesktop,
    focusSearch,
    toggleSidebar,
    scrollToTop,
    scrollToElement,
    copyToClipboard,
    getPerformanceMetrics,
    updateUrlWithoutScroll,
    focusElement,
    initialize,
    setDotNetRef,
    expandSearch,
    focusSearchInput,
    registerSystemThemeListener,
    dispose
};