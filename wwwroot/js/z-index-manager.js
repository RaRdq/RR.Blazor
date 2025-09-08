
class ZIndexManager {
    constructor() {
        const rootStyles = getComputedStyle(document.documentElement);
        this.modalBase = parseInt(rootStyles.getPropertyValue('--z-modal').trim()) || 1030;
        this.backdropBase = parseInt(rootStyles.getPropertyValue('--z-modal-backdrop').trim()) || 1040;
        this.portalBase = parseInt(rootStyles.getPropertyValue('--z-popup').trim()) || 910;
        this.increment = 10;
        
        this.currentLevel = 0;
        this.activeElements = new Map();
    }
    
    registerElement(id, type = 'modal') {
        if (!this.activeElements.has(id)) {
            const level = this.currentLevel++;
            this.activeElements.set(id, { type, level });
            return this.getZIndexForLevel(type, level);
        }
        return this.getZIndexForElement(id);
    }
    
    unregisterElement(id) {
        if (this.activeElements.has(id)) {
            this.activeElements.delete(id);
            if (this.activeElements.size === 0) {
                this.currentLevel = 0;
            }
        }
    }
    
    getZIndexForElement(id) {
        const element = this.activeElements.get(id);
        if (element) {
            return this.getZIndexForLevel(element.type, element.level);
        }
        return null;
    }
    
    getZIndexForLevel(type, level) {
        const base = this.getBaseForType(type);
        return base + (level * this.increment);
    }
    
    getBaseForType(type) {
        switch(type) {
            case 'backdrop': return this.backdropBase;
            case 'portal': return this.portalBase;
            case 'modal': 
            default: return this.modalBase;
        }
    }
    
    getNextZIndex(type = 'modal') {
        return this.getBaseForType(type) + (this.currentLevel * this.increment);
    }
}

export default class ZIndexManagerSingleton {
    static instance = null;
    
    static getInstance() {
        if (!ZIndexManagerSingleton.instance) {
            ZIndexManagerSingleton.instance = new ZIndexManager();
        }
        return ZIndexManagerSingleton.instance;
    }
}

window.RRBlazor = window.RRBlazor || {};
window.RRBlazor.ZIndexManager = ZIndexManagerSingleton.getInstance();