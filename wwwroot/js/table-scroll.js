// RTableScrollManager - Professional table scroll management and mobile enhancements
export const RTableScrollManager = {
    
    scrollManagers: new Map(),
    
    initialize: function(tableId) {
        try {
            if (!tableId) {
                console.warn('[RTableScrollManager] Missing table ID for initialization');
                return false;
            }
            
            const tableContainer = document.querySelector(`[data-table-id="${tableId}"]`);
            if (!tableContainer) {
                console.warn('[RTableScrollManager] Table container not found:', tableId);
                return false;
            }
            
            const scrollContainer = tableContainer.querySelector('.table-content.scroll-container-x');
            if (!scrollContainer) {
                console.warn('[RTableScrollManager] Scroll container not found for table:', tableId);
                return false;
            }
            
            // Clean up existing manager if present
            this.dispose(tableId);
            
            // Create manager instance
            const manager = new TableScrollInstance(tableId, tableContainer, scrollContainer);
            this.scrollManagers.set(tableId, manager);
            
            if (window.debugLogger?.isDebugMode) {
                window.debugLogger.log('[RTableScrollManager] Initialized:', { tableId });
            }
            
            return true;
            
        } catch (error) {
            console.error('[RTableScrollManager] Initialization failed:', error);
            return false;
        }
    },
    
    dispose: function(tableId) {
        try {
            if (this.scrollManagers.has(tableId)) {
                const manager = this.scrollManagers.get(tableId);
                manager.dispose();
                this.scrollManagers.delete(tableId);
                
                if (window.debugLogger?.isDebugMode) {
                    window.debugLogger.log('[RTableScrollManager] Disposed:', tableId);
                }
            }
        } catch (error) {
            console.error('[RTableScrollManager] Dispose failed:', error);
        }
    },
    
    refresh: function(tableId) {
        try {
            if (this.scrollManagers.has(tableId)) {
                const manager = this.scrollManagers.get(tableId);
                manager.updateScrollShadows();
                return true;
            }
            return false;
        } catch (error) {
            console.error('[RTableScrollManager] Refresh failed:', error);
            return false;
        }
    }
};

class TableScrollInstance {
    constructor(tableId, tableContainer, scrollContainer) {
        this.tableId = tableId;
        this.tableContainer = tableContainer;
        this.scrollContainer = scrollContainer;
        this.scrollTimeout = null;
        this.resizeObserver = null;
        
        this.initialize();
    }
    
    initialize() {
        // Bind methods to preserve context
        this.handleScroll = this.handleScroll.bind(this);
        this.handleResize = this.handleResize.bind(this);
        this.handleHeaderFocus = this.handleHeaderFocus.bind(this);
        
        // Initialize scroll shadows
        this.updateScrollShadows();
        
        // Add scroll event listener with passive mode for performance
        this.scrollContainer.addEventListener('scroll', this.handleScroll, { passive: true });
        
        // Use ResizeObserver for better performance than window resize events
        if (window.ResizeObserver) {
            this.resizeObserver = new ResizeObserver(this.handleResize);
            this.resizeObserver.observe(this.scrollContainer);
        } else {
            // Fallback for older browsers
            window.addEventListener('resize', this.handleResize, { passive: true });
        }
        
        // Enhanced accessibility - smooth scroll to focused columns
        this.setupHeaderFocusHandling();
    }
    
    handleScroll() {
        // Use requestAnimationFrame for smooth performance
        if (this.scrollTimeout) return;
        
        this.scrollTimeout = requestAnimationFrame(() => {
            this.updateScrollShadows();
            this.scrollTimeout = null;
        });
    }
    
    handleResize() {
        // Use requestAnimationFrame for resize handling
        if (!this.resizeScheduled) {
            this.resizeScheduled = true;
            requestAnimationFrame(() => {
                this.updateScrollShadows();
                this.resizeScheduled = false;
            });
        }
    }
    
    updateScrollShadows() {
        try {
            const scrollLeft = this.scrollContainer.scrollLeft;
            const scrollWidth = this.scrollContainer.scrollWidth;
            const clientWidth = this.scrollContainer.clientWidth;
            const scrollRight = scrollWidth - clientWidth - scrollLeft;
            
            // Remove all scroll state classes
            this.scrollContainer.classList.remove('scrolled-left', 'scrolled-right', 'scrolled-both');
            
            // Add appropriate classes based on scroll position (10px threshold for smooth UX)
            if (scrollLeft > 10 && scrollRight > 10) {
                this.scrollContainer.classList.add('scrolled-both');
            } else if (scrollLeft > 10) {
                this.scrollContainer.classList.add('scrolled-left');
            } else if (scrollRight > 10) {
                this.scrollContainer.classList.add('scrolled-right');
            }
            
            // Legacy mobile scroll class for backward compatibility
            if (this.tableContainer.classList.contains('table-mobile-scroll')) {
                if (scrollLeft > 10) {
                    this.tableContainer.classList.add('scrolled');
                } else {
                    this.tableContainer.classList.remove('scrolled');
                }
            }
            
        } catch (error) {
            console.error('[TableScrollInstance] Update scroll shadows failed:', error);
        }
    }
    
    setupHeaderFocusHandling() {
        try {
            const headers = this.scrollContainer.querySelectorAll('.r-table-header-cell[data-column-key]');
            
            headers.forEach(header => {
                header.addEventListener('focus', this.handleHeaderFocus, { passive: true });
            });
            
        } catch (error) {
            console.error('[TableScrollInstance] Header focus setup failed:', error);
        }
    }
    
    handleHeaderFocus(event) {
        try {
            const header = event.target;
            const headerLeft = header.offsetLeft;
            const headerRight = headerLeft + header.offsetWidth;
            const containerLeft = this.scrollContainer.scrollLeft;
            const containerRight = containerLeft + this.scrollContainer.clientWidth;
            
            // Check if header is outside visible area
            if (headerLeft < containerLeft || headerRight > containerRight) {
                // Calculate optimal scroll position with padding
                const targetScrollLeft = Math.max(0, headerLeft - 20);
                
                this.scrollContainer.scrollTo({
                    left: targetScrollLeft,
                    behavior: 'smooth'
                });
            }
            
        } catch (error) {
            console.error('[TableScrollInstance] Header focus handling failed:', error);
        }
    }
    
    dispose() {
        try {
            // Remove scroll event listener
            this.scrollContainer.removeEventListener('scroll', this.handleScroll);
            
            // Clean up resize observer
            if (this.resizeObserver) {
                this.resizeObserver.disconnect();
                this.resizeObserver = null;
            } else {
                window.removeEventListener('resize', this.handleResize);
            }
            
            // Clean up header focus handlers
            const headers = this.scrollContainer.querySelectorAll('.r-table-header-cell[data-column-key]');
            headers.forEach(header => {
                header.removeEventListener('focus', this.handleHeaderFocus);
            });
            
            // Clear any pending timeouts
            if (this.scrollTimeout) {
                cancelAnimationFrame(this.scrollTimeout);
                this.scrollTimeout = null;
            }
            
            if (this.resizeTimeout) {
                clearTimeout(this.resizeTimeout);
                this.resizeTimeout = null;
            }
            
        } catch (error) {
            console.error('[TableScrollInstance] Dispose failed:', error);
        }
    }
}

// Export for global access
window.RTableScrollManager = RTableScrollManager;

export default RTableScrollManager;