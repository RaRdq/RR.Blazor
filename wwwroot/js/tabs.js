// RR.Blazor Tabs Component JavaScript Module
// Handles tab navigation, scrolling, and indicator positioning

export function getTabIndicatorPosition(tabElementId, wrapperElement) {
    const element = document.getElementById(tabElementId);
    if (!element) {
        return { left: 0, width: 0 };
    }
    
    const rect = element.getBoundingClientRect();
    const wrapperRect = wrapperElement ? wrapperElement.getBoundingClientRect() : element.parentElement.getBoundingClientRect();
    
    return {
        left: rect.left - wrapperRect.left,
        width: rect.width
    };
}

export function getTabScrollInfo(wrapperElement) {
    if (!wrapperElement) {
        return { isScrollable: false, canScrollLeft: false, canScrollRight: false };
    }
    
    const scrollLeft = wrapperElement.scrollLeft;
    const scrollWidth = wrapperElement.scrollWidth;
    const clientWidth = wrapperElement.clientWidth;
    
    // Add 5px threshold to avoid false positives from rounding errors
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
    
    // Enhanced scroll behavior - snap to tabs for professional feel
    const tabs = wrapperElement.querySelectorAll('[role="tab"]');
    const containerRect = wrapperElement.getBoundingClientRect();
    
    // Find the first fully visible tab from the left
    let targetTab = null;
    for (let i = tabs.length - 1; i >= 0; i--) {
        const tabRect = tabs[i].getBoundingClientRect();
        if (tabRect.left < containerRect.left) {
            targetTab = tabs[i];
            break;
        }
    }
    
    if (targetTab) {
        // Scroll to show the target tab with some padding
        const targetLeft = targetTab.offsetLeft - 16; // 16px padding
        wrapperElement.scrollTo({
            left: Math.max(0, targetLeft),
            behavior: 'smooth'
        });
    } else {
        // Fallback to percentage-based scrolling
        const scrollAmount = wrapperElement.clientWidth * 0.7;
        wrapperElement.scrollBy({
            left: -scrollAmount,
            behavior: 'smooth'
        });
    }
}

export function scrollTabsRight(wrapperElement) {
    if (!wrapperElement) return;
    
    // Enhanced scroll behavior - snap to tabs for professional feel
    const tabs = wrapperElement.querySelectorAll('[role="tab"]');
    const containerRect = wrapperElement.getBoundingClientRect();
    
    // Find the first partially hidden tab from the right
    let targetTab = null;
    for (let i = 0; i < tabs.length; i++) {
        const tabRect = tabs[i].getBoundingClientRect();
        if (tabRect.right > containerRect.right) {
            targetTab = tabs[i];
            break;
        }
    }
    
    if (targetTab) {
        // Scroll to show the target tab with some padding
        const targetLeft = targetTab.offsetLeft - 16; // 16px padding
        wrapperElement.scrollTo({
            left: targetLeft,
            behavior: 'smooth'
        });
    } else {
        // Fallback to percentage-based scrolling
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
    
    // Calculate if tab is outside visible area
    const isTabVisible = tabRect.left >= wrapperRect.left && tabRect.right <= wrapperRect.right;
    
    if (!isTabVisible) {
        // Calculate scroll position to center the tab
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
    // Add keyboard navigation
    const tabs = element.querySelectorAll('[role="tab"]');
    tabs.forEach(tab => {
        tab.addEventListener('keydown', (e) => {
            // Handled in Blazor component
        });
    });
    
    // Update indicator position on window resize
    const updateIndicator = () => {
        const activeTab = element.querySelector('[role="tab"][aria-selected="true"]');
        if (activeTab) {
            // Trigger update in Blazor component
            const event = new CustomEvent('rr-tab-indicator-update', {
                detail: { tabId: activeTab.id }
            });
            element.dispatchEvent(event);
        }
    };
    
    // Enhanced scroll state management
    const updateScrollState = () => {
        if (navWrapper) {
            const scrollInfo = getTabScrollInfo(navWrapper);
            const navElement = element.querySelector('.tabs__nav');
            if (navElement) {
                navElement.classList.toggle('tabs__nav--scrollable', scrollInfo.isScrollable);
            }
            
            // Update arrow visibility
            const leftArrow = element.querySelector('.tabs__nav-arrow--left');
            const rightArrow = element.querySelector('.tabs__nav-arrow--right');
            
            if (leftArrow) {
                leftArrow.classList.toggle('tabs__nav-arrow--visible', scrollInfo.canScrollLeft);
            }
            if (rightArrow) {
                rightArrow.classList.toggle('tabs__nav-arrow--visible', scrollInfo.canScrollRight);
            }
        }
    };
    
    // Event listeners
    window.addEventListener('resize', updateIndicator);
    window.addEventListener('resize', updateScrollState);
    
    // Scroll event listener for real-time arrow updates
    if (navWrapper) {
        navWrapper.addEventListener('scroll', updateScrollState);
    }
    
    // Initial setup
    updateScrollState();
    
    // Store cleanup function
    element._rrCleanup = () => {
        window.removeEventListener('resize', updateIndicator);
        window.removeEventListener('resize', updateScrollState);
        if (navWrapper) {
            navWrapper.removeEventListener('scroll', updateScrollState);
        }
    };
}

// Export all tab functions
export default {
    getTabIndicatorPosition,
    getTabScrollInfo,
    scrollTabsLeft,
    scrollTabsRight,
    scrollToTab,
    initializeTabs
};