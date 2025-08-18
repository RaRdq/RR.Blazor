
export function getTabIndicatorPosition(tabElementId, wrapperElement) {
    const element = document.getElementById(tabElementId);
    
    element.offsetHeight;
    const tabRect = element.getBoundingClientRect();
    const wrapperRect = wrapperElement.getBoundingClientRect();
    
    const scrollLeft = wrapperElement.scrollLeft || 0;
    const relativeLeft = Math.round(tabRect.left - wrapperRect.left + scrollLeft);
    
    return {
        left: relativeLeft,
        width: Math.round(tabRect.width)
    };
}

export function getTabScrollInfo(wrapperElement) {
    
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
    const tabElement = document.getElementById(tabElementId);
    
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
    const isTouchDevice = 'ontouchstart' in window || navigator.maxTouchPoints > 0;
    const isSmallViewport = () => window.innerWidth <= 768;
    const isLandscape = () => window.innerWidth > window.innerHeight;
    
    const tabs = element.querySelectorAll('[role="tab"]');
    tabs.forEach(tab => {
        tab.addEventListener('keydown', () => {});
    });
    
    const updateIndicator = () => {
        const activeTab = element.querySelector('[role="tab"][aria-selected="true"]');
        if (activeTab) {
            if (window.RRBlazor && window.RRBlazor.EventDispatcher) {
                window.RRBlazor.EventDispatcher.dispatch(
                    'rr-tab-indicator-update',
                    { tabId: activeTab.id }
                );
            }
            element.dispatchEvent(new Event('rr-tab-indicator-update'));
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
    
    const updateTabSizing = () => {
        const viewport = {
            width: window.innerWidth,
            height: window.innerHeight,
            aspectRatio: window.innerWidth / window.innerHeight,
            orientation: isLandscape() ? 'landscape' : 'portrait',
            devicePixelRatio: window.devicePixelRatio || 1
        };
        
        element.classList.toggle('tabs-touch', isTouchDevice);
        element.classList.toggle('tabs-mobile', isSmallViewport());
        element.classList.toggle('tabs-landscape', isLandscape());
        element.classList.toggle('tabs-ultra-wide', viewport.aspectRatio > 2.1);
        element.classList.toggle('tabs-square', viewport.aspectRatio < 1.3 && viewport.aspectRatio > 0.77);
        
        if (window.RRBlazor && window.RRBlazor.EventDispatcher) {
            window.RRBlazor.EventDispatcher.dispatch(
                'rr-tabs-viewport-change',
                viewport
            );
        }
        element.dispatchEvent(new Event('rr-tabs-viewport-change'));
    };
    
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
    
    const handleOrientationChange = () => {
        requestAnimationFrame(() => {
            updateTabSizing();
            updateIndicator();
            updateScrollState();
            
            const activeTab = element.querySelector('[role="tab"][aria-selected="true"]');
            if (activeTab) {
                scrollToTab(navWrapper, activeTab.id);
            }
        });
    };
    
    window.addEventListener('resize', handleResize);
    window.addEventListener('orientationchange', handleOrientationChange);
    
    if (navWrapper) {
        let scrollTimeout;
        navWrapper.addEventListener('scroll', () => {
            updateScrollState();
            
            clearTimeout(scrollTimeout);
            
            if (!scrollTimeout) {
                scrollTimeout = requestAnimationFrame(() => {
                    updateIndicator();
                    scrollTimeout = null;
                });
            }
        });
    }
    
    const resizeObserver = new ResizeObserver(() => {
        updateScrollState();
    });
    
    if (navContainer) {
        resizeObserver.observe(navContainer);
    }
    
    requestAnimationFrame(() => {
        updateTabSizing();
        updateScrollState();
        updateIndicator();
    });
    
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

export function initialize(element, dotNetRef) {
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