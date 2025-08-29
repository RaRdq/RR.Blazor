export const RTableScrollManager = {
    
    scrollManagers: new Map(),
    
    initialize: function(tableId) {
        try {
            if (!tableId) {
                return false;
            }
            
            const tableContainer = document.querySelector(`[data-table-id="${tableId}"]`);
            if (!tableContainer) {
                return false;
            }
            
            const scrollContainer = tableContainer.querySelector('.table-content.scroll-container-x');
            if (!scrollContainer) {
                return false;
            }
            
            this.dispose(tableId);
            
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
        this.handleScroll = this.handleScroll.bind(this);
        this.handleResize = this.handleResize.bind(this);
        this.handleHeaderFocus = this.handleHeaderFocus.bind(this);
        
        this.updateScrollShadows();
        
        this.scrollContainer.addEventListener('scroll', this.handleScroll, { passive: true });
        
        if (window.ResizeObserver) {
            this.resizeObserver = new ResizeObserver(this.handleResize);
            this.resizeObserver.observe(this.scrollContainer);
        } else {
            window.addEventListener('resize', this.handleResize, { passive: true });
        }
        
        this.setupHeaderFocusHandling();
    }
    
    handleScroll() {
        if (this.scrollTimeout) return;
        
        this.scrollTimeout = requestAnimationFrame(() => {
            this.updateScrollShadows();
            this.scrollTimeout = null;
        });
    }
    
    handleResize() {
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
            
            this.scrollContainer.classList.remove('scrolled-left', 'scrolled-right', 'scrolled-both');
            
            if (scrollLeft > 10 && scrollRight > 10) {
                this.scrollContainer.classList.add('scrolled-both');
            } else if (scrollLeft > 10) {
                this.scrollContainer.classList.add('scrolled-left');
            } else if (scrollRight > 10) {
                this.scrollContainer.classList.add('scrolled-right');
            }
            
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
            
            if (headerLeft < containerLeft || headerRight > containerRight) {
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
            this.scrollContainer.removeEventListener('scroll', this.handleScroll);
            
            if (this.resizeObserver) {
                this.resizeObserver.disconnect();
                this.resizeObserver = null;
            } else {
                window.removeEventListener('resize', this.handleResize);
            }
            
            const headers = this.scrollContainer.querySelectorAll('.r-table-header-cell[data-column-key]');
            headers.forEach(header => {
                header.removeEventListener('focus', this.handleHeaderFocus);
            });
            
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

window.RTableScrollManager = RTableScrollManager;

export default RTableScrollManager;