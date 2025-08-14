
class KeyboardNavigationManager {
    constructor() {
        this.activeNavigation = null;
        this.globalHandler = this.handleGlobalKeydown.bind(this);
        this.initialized = false;
    }
    
    initialize() {
        if (this.initialized) return;
        
        document.addEventListener('keydown', this.globalHandler, true);
        this.initialized = true;
    }
    
    enableNavigation(elementId, container, options = {}) {
        if (!this.initialized) {
            throw new Error('Must initialize before enabling navigation');
        }
        
        this.activeNavigation = {
            elementId,
            container,
            itemSelector: options.itemSelector || '.nav-item',
            selectedClass: options.selectedClass || 'nav-selected',
            highlightClass: options.highlightClass || 'nav-highlighted',
            currentIndex: options.startIndex || -1,
            wrapAround: options.wrapAround !== false,
            options
        };
    }
    
    disableNavigation() {
        if (!this.activeNavigation) return;
        
        this.clearHighlight();
        this.activeNavigation = null;
    }
    
    handleGlobalKeydown(event) {
        if (!this.activeNavigation) return;
        
        const { key } = event;
        
        switch (key) {
            case 'ArrowUp':
                event.preventDefault();
                this.navigateUp();
                break;
            case 'ArrowDown':
                event.preventDefault();
                this.navigateDown();
                break;
            case 'Enter':
            case ' ':
                event.preventDefault();
                this.selectCurrent();
                break;
            case 'Escape':
                event.preventDefault();
                this.emitEscape();
                break;
            case 'Home':
                event.preventDefault();
                this.navigateToFirst();
                break;
            case 'End':
                event.preventDefault();
                this.navigateToLast();
                break;
        }
    }
    
    navigateUp() {
        const items = this.getNavigableItems();
        if (items.length === 0) return;
        
        this.activeNavigation.currentIndex = this.activeNavigation.wrapAround
            ? (this.activeNavigation.currentIndex - 1 + items.length) % items.length
            : Math.max(0, this.activeNavigation.currentIndex - 1);
            
        this.updateHighlight(items);
    }
    
    navigateDown() {
        const items = this.getNavigableItems();
        if (items.length === 0) return;
        
        const nextIndex = this.activeNavigation.currentIndex + 1;
        this.activeNavigation.currentIndex = this.activeNavigation.wrapAround
            ? nextIndex % items.length
            : Math.min(items.length - 1, nextIndex);
            
        this.updateHighlight(items);
    }
    
    navigateToFirst() {
        const items = this.getNavigableItems();
        if (items.length === 0) return;
        
        this.activeNavigation.currentIndex = 0;
        this.updateHighlight(items);
    }
    
    navigateToLast() {
        const items = this.getNavigableItems();
        if (items.length === 0) return;
        
        this.activeNavigation.currentIndex = items.length - 1;
        this.updateHighlight(items);
    }
    
    selectCurrent() {
        const items = this.getNavigableItems();
        const currentItem = items[this.activeNavigation.currentIndex];
        
        if (!currentItem) throw new Error('No current item to select');
        
        const selectEvent = new CustomEvent('keyboard-select', {
            detail: { 
                elementId: this.activeNavigation.elementId,
                item: currentItem,
                index: this.activeNavigation.currentIndex
            },
            bubbles: true
        });
        currentItem.dispatchEvent(selectEvent);
    }
    
    emitEscape() {
        const escapeEvent = new CustomEvent('keyboard-escape', {
            detail: { elementId: this.activeNavigation.elementId },
            bubbles: true
        });
        this.activeNavigation.container.dispatchEvent(escapeEvent);
    }
    
    getNavigableItems() {
        if (!this.activeNavigation) return [];
        
        return Array.from(
            this.activeNavigation.container.querySelectorAll(
                `${this.activeNavigation.itemSelector}:not([disabled])`
            )
        );
    }
    
    updateHighlight(items) {
        items.forEach(item => {
            item.classList.remove(this.activeNavigation.highlightClass);
        });
        
        const currentItem = items[this.activeNavigation.currentIndex];
        if (!currentItem) throw new Error('Invalid highlight index');
        
        currentItem.classList.add(this.activeNavigation.highlightClass);
        currentItem.scrollIntoView({ block: 'nearest' });
    }
    
    clearHighlight() {
        if (!this.activeNavigation) return;
        
        const items = this.getNavigableItems();
        items.forEach(item => {
            item.classList.remove(this.activeNavigation.highlightClass);
        });
    }
    
    destroy() {
        if (this.initialized) {
            document.removeEventListener('keydown', this.globalHandler, true);
        }
        this.disableNavigation();
        this.initialized = false;
    }
}

const keyboardNavigationManager = new KeyboardNavigationManager();

// Auto-initialize
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => keyboardNavigationManager.initialize());
} else {
    keyboardNavigationManager.initialize();
}

export default keyboardNavigationManager;

export function enableKeyboardNavigation(elementId, container, options) {
    return keyboardNavigationManager.enableNavigation(elementId, container, options);
}

export function disableKeyboardNavigation() {
    return keyboardNavigationManager.disableNavigation();
}