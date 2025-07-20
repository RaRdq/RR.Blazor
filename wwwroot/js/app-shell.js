// RAppShell JavaScript module for enhanced functionality

// Use shared debug logger from RR.Blazor main file
const debugLogger = window.debugLogger || new (window.RRDebugLogger || class {
    constructor() { this.logPrefix = '[RAppShell]'; }
    log(...args) { console.log(this.logPrefix, ...args); }
    error(...args) { console.error(this.logPrefix, ...args); }
})();

export function initialize() {
        debugLogger.log('RAppShell initialized');
    
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
        // Global shortcuts
        if (e.ctrlKey || e.metaKey) {
            switch (e.key) {
                case 'k':
                case 'K':
                    // Focus search (Ctrl/Cmd + K)
                    e.preventDefault();
                    focusSearch();
                    break;
                case 'b':
                case 'B':
                    // Toggle sidebar (Ctrl/Cmd + B)
                    e.preventDefault();
                    toggleSidebar();
                    break;
                case ',':
                    // Open settings (Ctrl/Cmd + ,)
                    e.preventDefault();
                    navigateTo('/settings');
                    break;
            }
        }
        
        // Escape key handling
        if (e.key === 'Escape') {
            closeAllDropdowns();
            closeSearch();
        }
    });
}

function setupClickOutside() {
    document.addEventListener('click', (e) => {
        // Close search results if clicking outside
        const searchContainer = e.target.closest('.search-container');
        if (!searchContainer) {
            closeSearch();
        }
        
        // Close dropdowns if clicking outside
        const dropdown = e.target.closest('.dropdown');
        if (!dropdown) {
            closeAllDropdowns();
        }
    });
}

function setupResponsive() {
    let resizeTimeout;
    
    const handleMobileCollapse = () => {
        // Update mobile state
        document.documentElement.style.setProperty('--is-mobile', isMobile() ? '1' : '0');
        document.documentElement.style.setProperty('--is-tablet', isTablet() ? '1' : '0');
        document.documentElement.style.setProperty('--is-desktop', isDesktop() ? '1' : '0');
        
        // Force mobile sidebar behavior - try multiple selector patterns
        if (isMobile()) {
            // Multiple selector patterns to ensure we catch the sidebar
            const sidebarSelectors = ['.sidebar', '[class*="sidebar"]', '.app-shell .sidebar', 'aside[role="navigation"]'];
            let sidebar = null;
            
            for (const selector of sidebarSelectors) {
                sidebar = document.querySelector(selector);
                if (sidebar) break;
            }
            
            if (sidebar) {
                // Use CSS classes instead of direct style manipulation
                sidebar.classList.add('sidebar-closed', 'mobile-hidden');
                
                // Add mobile classes to body/html for CSS targeting
                document.body.classList.add('mobile-layout');
                document.documentElement.classList.add('mobile-layout');
                
                // Ensure main content gets proper mobile class
                const mainContent = document.querySelector('.main-content');
                if (mainContent) {
                    mainContent.classList.add('mobile-sidebar');
                }
            }
        } else {
            // Desktop behavior - restore sidebar
            const sidebarSelectors = ['.sidebar', '[class*="sidebar"]', '.app-shell .sidebar', 'aside[role="navigation"]'];
            let sidebar = null;
            
            for (const selector of sidebarSelectors) {
                sidebar = document.querySelector(selector);
                if (sidebar) break;
            }
            
            if (sidebar) {
                sidebar.classList.remove('mobile-hidden', 'sidebar-closed');
                
                document.body.classList.remove('mobile-layout');
                document.documentElement.classList.remove('mobile-layout');
                
                // Remove mobile class from main content
                const mainContent = document.querySelector('.main-content');
                if (mainContent) {
                    mainContent.classList.remove('mobile-sidebar');
                }
            }
        }
    };
    
    window.addEventListener('resize', () => {
        clearTimeout(resizeTimeout);
        resizeTimeout = setTimeout(handleMobileCollapse, 150);
    });
    
    // Initial setup - use setTimeout to ensure DOM is ready
    setTimeout(handleMobileCollapse, 100);
}

function setupAccessibility() {
    // Add focus indicators for keyboard navigation
    document.addEventListener('keydown', (e) => {
        if (e.key === 'Tab') {
            document.body.classList.add('keyboard-navigation');
        }
    });
    
    document.addEventListener('mousedown', () => {
        document.body.classList.remove('keyboard-navigation');
    });
    
    // Screen reader announcements
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
    try {
        let element;
        if (selector.startsWith('#')) {
            element = document.getElementById(selector.substring(1));
        } else if (selector.startsWith('[') && selector.endsWith(']')) {
            element = document.querySelector(selector);
        } else {
            element = document.querySelector(selector);
        }
        
        if (element) {
            element.focus();
            return true;
        }
        return false;
    } catch (error) {
        console.warn('Focus element failed:', error);
        return false;
    }
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
        if (dropdown) {
            const trigger = dropdown.querySelector('.dropdown__trigger');
            if (trigger) {
                trigger.click(); // This will trigger the Blazor close logic
            }
        }
    });
}

function navigateTo(url) {
    if (isValidUrl(url)) {
        window.location.href = url;
    }
}

function isValidUrl(url) {
    return url && 
           (url.startsWith('/') || 
            url.startsWith('./') || 
            url.startsWith('../') ||
            (url.startsWith('http://') || url.startsWith('https://')));
}

function announce(message) {
    const announcer = document.getElementById('app-shell-announcer');
    if (announcer) {
        announcer.textContent = message;
        setTimeout(() => {
            announcer.textContent = '';
        }, 1000);
    }
}

// Theme system integration
export function registerSystemThemeListener(dotNetRef) {
    const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
    
    function handleThemeChange(e) {
        dotNetRef.invokeMethodAsync('OnSystemThemeChanged', e.matches, false);
    }
    
    mediaQuery.addEventListener('change', handleThemeChange);
    
    // Initial call
    handleThemeChange(mediaQuery);
    
    return {
        dispose: () => {
            mediaQuery.removeEventListener('change', handleThemeChange);
        }
    };
}

// Performance monitoring
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

// Utility functions for component interaction
export function scrollToTop() {
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

export function scrollToElement(selector) {
    const element = document.querySelector(selector);
    if (element) {
        element.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
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
    focusElement
};