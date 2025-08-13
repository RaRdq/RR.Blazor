// Tab navigation and scrolling utilities

export function getTabIndicatorPosition(tabElementId, wrapperElement) {
    const element = document.getElementById(tabElementId);
    if (!element || !wrapperElement) {
        return { left: 0, width: 0 };
    }
    
    // Force layout calculation to ensure accurate measurements
    element.offsetHeight;
    
    // Get the position relative to the scrollable wrapper
    const tabRect = element.getBoundingClientRect();
    const wrapperRect = wrapperElement.getBoundingClientRect();
    
    // Calculate position accounting for scroll offset with rounding to prevent sub-pixel issues
    const scrollLeft = wrapperElement.scrollLeft || 0;
    const relativeLeft = Math.round(tabRect.left - wrapperRect.left + scrollLeft);
    
    return {
        left: relativeLeft,
        width: Math.round(tabRect.width)
    };
}

export function getTabScrollInfo(wrapperElement) {
    if (!wrapperElement) {
        return { isScrollable: false, canScrollLeft: false, canScrollRight: false };
    }
    
    const scrollLeft = wrapperElement.scrollLeft;
    const scrollWidth = wrapperElement.scrollWidth;
    const clientWidth = wrapperElement.clientWidth;
    
    const scrollThreshold = 5;
    const isScrollable = scrollWidth > clientWidth + scrollThreshold;
    
    return {
        isScrollable: isScrollable,
        canScrollLeft: isScrollable && scrollLeft > scrollThreshold,
        canScrollRight: isScrollable && scrollLeft < scrollWidth - clientWidth - scrollThreshold
    };
}

export function scrollTabsLeft(wrapperElement) {
    if (!wrapperElement) return;
    
    const tabs = wrapperElement.querySelectorAll('[role="tab"]');
    const containerRect = wrapperElement.getBoundingClientRect();
    
    let targetTab = null;
    for (let i = tabs.length - 1; i >= 0; i--) {
        const tabRect = tabs[i].getBoundingClientRect();
        if (tabRect.left < containerRect.left) {
            targetTab = tabs[i];
            break;
        }
    }
    
    if (targetTab) {
        const targetLeft = targetTab.offsetLeft - 16;
        wrapperElement.scrollTo({
            left: Math.max(0, targetLeft),
            behavior: 'smooth'
        });
    } else {
        const scrollAmount = wrapperElement.clientWidth * 0.7;
        wrapperElement.scrollBy({
            left: -scrollAmount,
            behavior: 'smooth'
        });
    }
}

export function scrollTabsRight(wrapperElement) {
    if (!wrapperElement) return;
    
    const tabs = wrapperElement.querySelectorAll('[role="tab"]');
    const containerRect = wrapperElement.getBoundingClientRect();
    
    let targetTab = null;
    for (let i = 0; i < tabs.length; i++) {
        const tabRect = tabs[i].getBoundingClientRect();
        if (tabRect.right > containerRect.right) {
            targetTab = tabs[i];
            break;
        }
    }
    
    if (targetTab) {
        const targetLeft = targetTab.offsetLeft - 16;
        wrapperElement.scrollTo({
            left: targetLeft,
            behavior: 'smooth'
        });
    } else {
        const scrollAmount = wrapperElement.clientWidth * 0.7;
        wrapperElement.scrollBy({
            left: scrollAmount,
            behavior: 'smooth'
        });
    }
}

export function scrollToTab(wrapperElement, tabElementId) {
    if (!wrapperElement) return;
    
    const tabElement = document.getElementById(tabElementId);
    if (!tabElement) return;
    
    const wrapperRect = wrapperElement.getBoundingClientRect();
    const tabRect = tabElement.getBoundingClientRect();
    
    const isTabVisible = tabRect.left >= wrapperRect.left && tabRect.right <= wrapperRect.right;
    
    if (!isTabVisible) {
        const scrollLeft = wrapperElement.scrollLeft;
        const tabOffsetLeft = tabElement.offsetLeft;
        const wrapperWidth = wrapperElement.clientWidth;
        const tabWidth = tabElement.offsetWidth;
        
        const targetScrollLeft = tabOffsetLeft - (wrapperWidth / 2) + (tabWidth / 2);
        
        wrapperElement.scrollTo({
            left: Math.max(0, targetScrollLeft),
            behavior: 'smooth'
        });
    }
}

export function initializeTabs(element, navContainer, navWrapper) {
    if (!element || !navWrapper) return;
    
    // Device detection
    const isTouchDevice = 'ontouchstart' in window || navigator.maxTouchPoints > 0;
    const isSmallViewport = () => window.innerWidth <= 768;
    const isLandscape = () => window.innerWidth > window.innerHeight;
    
    const tabs = element.querySelectorAll('[role="tab"]');
    tabs.forEach(tab => {
        tab.addEventListener('keydown', (e) => {
            // Keyboard navigation handled by Blazor
        });
    });
    
    const updateIndicator = () => {
        const activeTab = element.querySelector('[role="tab"][aria-selected="true"]');
        if (activeTab) {
            const event = new CustomEvent('rr-tab-indicator-update', {
                detail: { tabId: activeTab.id }
            });
            element.dispatchEvent(event);
        }
    };
    
    const updateScrollState = () => {
        if (navWrapper) {
            const scrollInfo = getTabScrollInfo(navWrapper);
            const navElement = element.querySelector('.tabs-nav');
            if (navElement) {
                navElement.classList.toggle('tabs-nav-scrollable', scrollInfo.isScrollable);
            }
            
            const leftArrow = element.querySelector('.tabs-nav-arrow-left');
            const rightArrow = element.querySelector('.tabs-nav-arrow-right');
            
            if (leftArrow) {
                leftArrow.classList.toggle('tabs-nav-arrow-visible', scrollInfo.canScrollLeft);
            }
            if (rightArrow) {
                rightArrow.classList.toggle('tabs-nav-arrow-visible', scrollInfo.canScrollRight);
            }
        }
    };
    
    // Adaptive tab sizing based on viewport
    const updateTabSizing = () => {
        const viewport = {
            width: window.innerWidth,
            height: window.innerHeight,
            aspectRatio: window.innerWidth / window.innerHeight,
            orientation: isLandscape() ? 'landscape' : 'portrait',
            devicePixelRatio: window.devicePixelRatio || 1
        };
        
        // Add viewport classes for CSS hooks
        element.classList.toggle('tabs-touch', isTouchDevice);
        element.classList.toggle('tabs-mobile', isSmallViewport());
        element.classList.toggle('tabs-landscape', isLandscape());
        element.classList.toggle('tabs-ultra-wide', viewport.aspectRatio > 2.1);
        element.classList.toggle('tabs-square', viewport.aspectRatio < 1.3 && viewport.aspectRatio > 0.77);
        
        // Dispatch viewport info for Blazor
        const event = new CustomEvent('rr-tabs-viewport-change', {
            detail: viewport
        });
        element.dispatchEvent(event);
    };
    
    // Touch gesture support
    if (isTouchDevice && navWrapper) {
        let touchStartX = 0;
        let touchStartScrollLeft = 0;
        let isScrolling = false;
        
        navWrapper.addEventListener('touchstart', (e) => {
            touchStartX = e.touches[0].clientX;
            touchStartScrollLeft = navWrapper.scrollLeft;
            isScrolling = false;
        }, { passive: true });
        
        navWrapper.addEventListener('touchmove', (e) => {
            if (!isScrolling) {
                isScrolling = true;
                navWrapper.style.scrollSnapType = 'none';
            }
            const touchX = e.touches[0].clientX;
            const diff = touchStartX - touchX;
            navWrapper.scrollLeft = touchStartScrollLeft + diff;
        }, { passive: true });
        
        navWrapper.addEventListener('touchend', () => {
            navWrapper.style.scrollSnapType = 'x mandatory';
            updateScrollState();
        }, { passive: true });
    }
    
    // Enhanced resize handling with requestAnimationFrame
    let resizeScheduled = false;
    const handleResize = () => {
        if (!resizeScheduled) {
            resizeScheduled = true;
            requestAnimationFrame(() => {
                updateTabSizing();
                updateIndicator();
                updateScrollState();
                resizeScheduled = false;
            });
        }
    };
    
    // Orientation change handling
    const handleOrientationChange = () => {
        // Use requestAnimationFrame for orientation change
        requestAnimationFrame(() => {
            updateTabSizing();
            updateIndicator();
            updateScrollState();
            
            // Re-center active tab after orientation change
            const activeTab = element.querySelector('[role="tab"][aria-selected="true"]');
            if (activeTab) {
                scrollToTab(navWrapper, activeTab.id);
            }
        });
    };
    
    window.addEventListener('resize', handleResize);
    window.addEventListener('orientationchange', handleOrientationChange);
    
    // Update on scroll
    if (navWrapper) {
        let scrollTimeout;
        navWrapper.addEventListener('scroll', () => {
            updateScrollState();
            
            // Clear existing timeout
            clearTimeout(scrollTimeout);
            
            // Update indicator after scroll ends
            // Use requestAnimationFrame after scroll
            if (!scrollTimeout) {
                scrollTimeout = requestAnimationFrame(() => {
                    updateIndicator();
                    scrollTimeout = null;
                });
            }
        });
    }
    
    // ResizeObserver for dynamic content changes
    const resizeObserver = new ResizeObserver(() => {
        updateScrollState();
    });
    
    if (navContainer) {
        resizeObserver.observe(navContainer);
    }
    
    // Initial update
    requestAnimationFrame(() => {
        updateTabSizing();
        updateScrollState();
        updateIndicator();
    });
    
    // Cleanup function
    element._rrCleanup = () => {
        window.removeEventListener('resize', handleResize);
        window.removeEventListener('orientationchange', handleOrientationChange);
        if (navWrapper) {
            navWrapper.removeEventListener('scroll', updateScrollState);
        }
        if (resizeObserver) {
            resizeObserver.disconnect();
        }
    };
}

// Required methods for rr-blazor.js proxy system
export function initialize(element, dotNetRef) {
    if (element) {
        initializeTabs(element);
        return true;
    }
    return true;
}

export function cleanup(element) {
    if (element && element._rrCleanup) {
        element._rrCleanup();
        delete element._rrCleanup;
        return true;
    }
    return true;
}

export default {
    getTabIndicatorPosition,
    getTabScrollInfo,
    scrollTabsLeft,
    scrollTabsRight,
    scrollToTab,
    initializeTabs,
    initialize,
    cleanup
};