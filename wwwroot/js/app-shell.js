const debugLogger = {
    error: (...args) => console.error('[RAppShell]', ...args)
};

export function initialize() {
    setupKeyboardShortcuts();
    setupClickOutside();
    setupResponsive();
    setupAccessibility();
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
        const searchContainer = e.target.closest('.search-container');
        if (!searchContainer) {
            closeSearch();
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
        
        const sidebarSelectors = ['.sidebar', '[class*="sidebar"]', '.app-shell .sidebar', 'aside[role="navigation"]'];
        const sidebar = sidebarSelectors.map(sel => document.querySelector(sel)).find(Boolean);
        const mainContent = document.querySelector('.main-content');
        
        if (isMobile()) {
            sidebar?.classList.add('sidebar-closed', 'mobile-hidden');
            document.documentElement.classList.add('mobile-layout');
            mainContent?.classList.add('mobile-sidebar');
        } else {
            sidebar?.classList.remove('mobile-hidden', 'sidebar-closed');
            document.documentElement.classList.remove('mobile-layout');
            mainContent?.classList.remove('mobile-sidebar');
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
    const searchResults = document.querySelector('.search-results');
    if (searchResults) {
        searchResults.style.display = 'none';
    }
}

function closeAllDropdowns() {
    const dropdowns = document.querySelectorAll('.dropdown__viewport');
    dropdowns.forEach(viewport => {
        const dropdown = viewport.closest('.dropdown');
        const trigger = dropdown?.querySelector('.dropdown__trigger');
        trigger?.click();
    });
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
    
    return {
        loadTime: navigation?.loadEventEnd - navigation?.loadEventStart,
        domContentLoaded: navigation?.domContentLoadedEventEnd - navigation?.domContentLoadedEventStart,
        firstPaint: paint.find(p => p.name === 'first-paint')?.startTime,
        firstContentfulPaint: paint.find(p => p.name === 'first-contentful-paint')?.startTime
    };
}

export function scrollToTop() {
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

export function scrollToElement(selector) {
    const element = document.querySelector(selector);
    element?.scrollIntoView({ behavior: 'smooth', block: 'start' });
}

export function copyToClipboard(text) {
    return RRBlazor.copyToClipboard(text).then(() => {
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
    cleanup
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
    cleanup
};