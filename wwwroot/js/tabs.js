export function getTabIndicatorPosition(tabElementId, wrapperElement) {
    // Handle null or invalid elements
    if (!wrapperElement || !wrapperElement.nodeType) {
        return { left: 0, width: 0 };
    }
    
    const element = document.getElementById(tabElementId);
    if (!element) {
        return { left: 0, width: 0 };
    }
    
    element.offsetHeight; // Force reflow
    const tabRect = element.getBoundingClientRect();
    const wrapperRect = wrapperElement.getBoundingClientRect();
    
    const scrollLeft = wrapperElement.scrollLeft || 0;
    const relativeLeft = Math.round(tabRect.left - wrapperRect.left + scrollLeft);
    
    return {
        left: relativeLeft,
        width: Math.round(tabRect.width)
    };
}

export function getTabScrollInfo(wrapperElement, orientation = 'horizontal') {
    // Handle null or invalid element
    if (!wrapperElement || !wrapperElement.nodeType) {
        return {
            isScrollable: false,
            canScrollLeft: false,
            canScrollRight: false,
            canScrollUp: false,
            canScrollDown: false
        };
    }
    
    let scrollPosition, scrollSize, clientSize, isScrollable, canScrollStart, canScrollEnd;
    
    if (orientation === 'vertical') {
        scrollPosition = wrapperElement.scrollTop || 0;
        scrollSize = wrapperElement.scrollHeight || 0;
        clientSize = wrapperElement.clientHeight || 0;
    } else {
        scrollPosition = wrapperElement.scrollLeft || 0;
        scrollSize = wrapperElement.scrollWidth || 0;
        clientSize = wrapperElement.clientWidth || 0;
    }
    
    // More lenient tolerance for better detection
    const tolerance = 2;
    isScrollable = scrollSize > clientSize + tolerance;
    canScrollStart = isScrollable && scrollPosition > tolerance;
    canScrollEnd = isScrollable && scrollPosition < scrollSize - clientSize - tolerance;
    
    if (orientation === 'vertical') {
        return {
            isScrollable: isScrollable,
            canScrollLeft: false,
            canScrollRight: false,
            canScrollUp: canScrollStart,
            canScrollDown: canScrollEnd
        };
    } else {
        return {
            isScrollable: isScrollable,
            canScrollLeft: canScrollStart,
            canScrollRight: canScrollEnd,
            canScrollUp: false,
            canScrollDown: false
        };
    }
}

export function scrollTabsLeft(wrapperElement, orientation = 'horizontal') {
    if (!wrapperElement || !wrapperElement.nodeType) return;
    
    const tabs = wrapperElement.querySelectorAll('[role="tab"]');
    if (!tabs || tabs.length === 0) return;
    
    const containerRect = wrapperElement.getBoundingClientRect();
    
    if (orientation === 'vertical') {
        for (let i = tabs.length - 1; i >= 0; i--) {
            const tabRect = tabs[i].getBoundingClientRect();
            if (tabRect.top < containerRect.top) {
                const targetTop = tabs[i].offsetTop - 16;
                wrapperElement.scrollTo({
                    top: Math.max(0, targetTop),
                    behavior: 'smooth'
                });
                return;
            }
        }
        
        const scrollAmount = wrapperElement.clientHeight * 0.7;
        wrapperElement.scrollBy({
            top: -scrollAmount,
            behavior: 'smooth'
        });
    } else {
        for (let i = tabs.length - 1; i >= 0; i--) {
            const tabRect = tabs[i].getBoundingClientRect();
            if (tabRect.left < containerRect.left) {
                const targetLeft = tabs[i].offsetLeft - 16;
                wrapperElement.scrollTo({
                    left: Math.max(0, targetLeft),
                    behavior: 'smooth'
                });
                return;
            }
        }
        
        const scrollAmount = wrapperElement.clientWidth * 0.7;
        wrapperElement.scrollBy({
            left: -scrollAmount,
            behavior: 'smooth'
        });
    }
}

export function scrollTabsRight(wrapperElement, orientation = 'horizontal') {
    if (!wrapperElement || !wrapperElement.nodeType) return;
    
    const tabs = wrapperElement.querySelectorAll('[role="tab"]');
    if (!tabs || tabs.length === 0) return;
    
    const containerRect = wrapperElement.getBoundingClientRect();
    
    if (orientation === 'vertical') {
        for (let i = 0; i < tabs.length; i++) {
            const tabRect = tabs[i].getBoundingClientRect();
            if (tabRect.bottom > containerRect.bottom) {
                const targetTop = tabs[i].offsetTop - 16;
                wrapperElement.scrollTo({
                    top: targetTop,
                    behavior: 'smooth'
                });
                return;
            }
        }
        
        const scrollAmount = wrapperElement.clientHeight * 0.7;
        wrapperElement.scrollBy({
            top: scrollAmount,
            behavior: 'smooth'
        });
    } else {
        for (let i = 0; i < tabs.length; i++) {
            const tabRect = tabs[i].getBoundingClientRect();
            if (tabRect.right > containerRect.right) {
                const targetLeft = tabs[i].offsetLeft - 16;
                wrapperElement.scrollTo({
                    left: targetLeft,
                    behavior: 'smooth'
                });
                return;
            }
        }
        
        const scrollAmount = wrapperElement.clientWidth * 0.7;
        wrapperElement.scrollBy({
            left: scrollAmount,
            behavior: 'smooth'
        });
    }
}

export function scrollToTab(wrapperElement, tabElementId, orientation = 'horizontal') {
    if (!wrapperElement || !wrapperElement.nodeType) return;
    
    const tabElement = document.getElementById(tabElementId);
    if (!tabElement) return;
    
    const wrapperRect = wrapperElement.getBoundingClientRect();
    const tabRect = tabElement.getBoundingClientRect();
    
    let isTabVisible;
    
    if (orientation === 'vertical') {
        isTabVisible = tabRect.top >= wrapperRect.top && tabRect.bottom <= wrapperRect.bottom;
        
        if (!isTabVisible) {
            const tabOffsetTop = tabElement.offsetTop;
            const wrapperHeight = wrapperElement.clientHeight;
            const tabHeight = tabElement.offsetHeight;
            
            const targetScrollTop = tabOffsetTop - (wrapperHeight / 2) + (tabHeight / 2);
            
            wrapperElement.scrollTo({
                top: Math.max(0, targetScrollTop),
                behavior: 'smooth'
            });
        }
    } else {
        isTabVisible = tabRect.left >= wrapperRect.left && tabRect.right <= wrapperRect.right;
        
        if (!isTabVisible) {
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
}

function updateScrollState(element, navWrapper, orientation = 'horizontal') {
    if (!element || !element.nodeType || !navWrapper || !navWrapper.nodeType) {
        return;
    }
    
    try {
        const scrollInfo = getTabScrollInfo(navWrapper, orientation);
        
        if (orientation === 'vertical') {
            const upArrow = element.querySelector('.tabs-nav-arrow-up');
            const downArrow = element.querySelector('.tabs-nav-arrow-down');
            
            if (upArrow) {
                upArrow.classList.toggle('tabs-nav-arrow-visible', scrollInfo.canScrollUp);
            }
            if (downArrow) {
                downArrow.classList.toggle('tabs-nav-arrow-visible', scrollInfo.canScrollDown);
            }
        } else {
            const leftArrow = element.querySelector('.tabs-nav-arrow-left');
            const rightArrow = element.querySelector('.tabs-nav-arrow-right');
            
            if (leftArrow) {
                leftArrow.classList.toggle('tabs-nav-arrow-visible', scrollInfo.canScrollLeft);
            }
            if (rightArrow) {
                rightArrow.classList.toggle('tabs-nav-arrow-visible', scrollInfo.canScrollRight);
            }
        }
    } catch (error) {
    }
}

export function initializeTabs(element, navContainer, navWrapper, orientation = 'horizontal') {
    // Handle null or invalid elements
    if (!element || !element.nodeType) {
        // Invalid element provided
        return;
    }
    if (!navWrapper || !navWrapper.nodeType) {
        // Invalid navWrapper provided
        return;
    }
    
    try {
        const isTouchDevice = 'ontouchstart' in window;
        
        if (isTouchDevice && navWrapper) {
            let touchStartPos = 0;
            let touchStartScrollPos = 0;
            
            navWrapper.addEventListener('touchstart', (e) => {
                if (orientation === 'vertical') {
                    touchStartPos = e.touches[0].clientY;
                    touchStartScrollPos = navWrapper.scrollTop;
                } else {
                    touchStartPos = e.touches[0].clientX;
                    touchStartScrollPos = navWrapper.scrollLeft;
                }
            }, { passive: true });
            
            navWrapper.addEventListener('touchmove', (e) => {
                if (orientation === 'vertical') {
                    const touchY = e.touches[0].clientY;
                    const diff = touchStartPos - touchY;
                    navWrapper.scrollTop = touchStartScrollPos + diff;
                } else {
                    const touchX = e.touches[0].clientX;
                    const diff = touchStartPos - touchX;
                    navWrapper.scrollLeft = touchStartScrollPos + diff;
                }
            }, { passive: true });
            
            navWrapper.addEventListener('touchend', () => {
                updateScrollState(element, navWrapper, orientation);
            }, { passive: true });
        }
        
        const handleResize = () => {
            requestAnimationFrame(() => updateScrollState(element, navWrapper, orientation));
        };
        
        window.addEventListener('resize', handleResize);
        
        if (navWrapper) {
            navWrapper.addEventListener('scroll', () => {
                requestAnimationFrame(() => updateScrollState(element, navWrapper, orientation));
            });
        }
        
        const resizeObserver = new ResizeObserver(() => {
            updateScrollState(element, navWrapper, orientation);
        });
        
        if (navContainer) resizeObserver.observe(navContainer);
        if (navWrapper) resizeObserver.observe(navWrapper);
        
        const mutationObserver = new MutationObserver((mutations) => {
            const shouldUpdate = mutations.some(mutation => 
                mutation.type === 'childList' && 
                (mutation.addedNodes.length > 0 || mutation.removedNodes.length > 0)
            );
            
            if (shouldUpdate) {
                updateScrollState(element, navWrapper, orientation);
            }
        });
        
        if (navWrapper) {
            mutationObserver.observe(navWrapper, {
                childList: true,
                subtree: true
            });
        }
        
        // Initial scroll state update with delay to ensure DOM is ready
        requestAnimationFrame(() => {
            updateScrollState(element, navWrapper, orientation);
        });
        
        element._rrCleanup = () => {
            window.removeEventListener('resize', handleResize);
            resizeObserver.disconnect();
            mutationObserver.disconnect();
        };
    } catch (error) {
    }
}

export function initialize(element) {
    initializeTabs(element);
    return true;
}

export function cleanup(element) {
    if (element._rrCleanup) {
        element._rrCleanup();
        delete element._rrCleanup;
    }
    return true;
}

// Add vertical scrolling functions
export function scrollTabsUp(wrapperElement) {
    return scrollTabsLeft(wrapperElement, 'vertical');
}

export function scrollTabsDown(wrapperElement) {
    return scrollTabsRight(wrapperElement, 'vertical');
}

if (typeof window !== 'undefined') {
    if (!window.RRBlazor) window.RRBlazor = {};
    if (!window.RRBlazor.Tabs) {
        window.RRBlazor.Tabs = {};
    }
    
    // Only add functions if they don't already exist to prevent redefinition errors
    if (!window.RRBlazor.Tabs.getTabIndicatorPosition) {
        Object.assign(window.RRBlazor.Tabs, {
            getTabIndicatorPosition,
            getTabScrollInfo,
            scrollTabsLeft,
            scrollTabsRight,
            scrollTabsUp,
            scrollTabsDown,
            scrollToTab,
            initializeTabs,
            initialize,
            cleanup
        });
    }
}

export default {
    getTabIndicatorPosition,
    getTabScrollInfo,
    scrollTabsLeft,
    scrollTabsRight,
    scrollTabsUp,
    scrollTabsDown,
    scrollToTab,
    initializeTabs,
    initialize,
    cleanup
};