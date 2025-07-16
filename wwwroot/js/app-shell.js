// RAppShell JavaScript module for enhanced functionality

export function initialize() {
    console.log('RAppShell initialized');
    
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
    
    window.addEventListener('resize', () => {
        clearTimeout(resizeTimeout);
        resizeTimeout = setTimeout(() => {
            // Update mobile state
            document.documentElement.style.setProperty('--is-mobile', isMobile() ? '1' : '0');
            document.documentElement.style.setProperty('--is-tablet', isTablet() ? '1' : '0');
            document.documentElement.style.setProperty('--is-desktop', isDesktop() ? '1' : '0');
            
            // Auto-collapse sidebar on mobile
            if (isMobile()) {
                const sidebar = document.querySelector('.sidebar');
                if (sidebar && !sidebar.classList.contains('sidebar--closed')) {
                    sidebar.classList.add('sidebar--closed');
                }
            }
        }, 150);
    });
    
    // Initial setup
    document.documentElement.style.setProperty('--is-mobile', isMobile() ? '1' : '0');
    document.documentElement.style.setProperty('--is-tablet', isTablet() ? '1' : '0');
    document.documentElement.style.setProperty('--is-desktop', isDesktop() ? '1' : '0');
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
    window.location.href = url;
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
    updateUrlWithoutScroll
};