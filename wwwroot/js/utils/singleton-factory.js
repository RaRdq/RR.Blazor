// singleton-factory.js - Universal singleton pattern implementation

export function createSingleton(className, instanceName) {
    const SingletonClass = class extends className {
        static #instances = new Map();
        
        constructor(...args) {
            const key = instanceName || className.name;
            
            if (SingletonClass.#instances.has(key)) {
                throw new Error(`[${className.name}] Singleton already exists`);
            }
            
            super(...args);
            SingletonClass.#instances.set(key, this);
        }
        
        static getInstance(...args) {
            const key = instanceName || className.name;
            
            if (!SingletonClass.#instances.has(key)) {
                new this(...args);
            }
            
            return SingletonClass.#instances.get(key);
        }
        
        static destroyInstance() {
            const key = instanceName || className.name;
            const instance = SingletonClass.#instances.get(key);
            
            if (instance && typeof instance.destroy === 'function') {
                instance.destroy();
            }
            
            SingletonClass.#instances.delete(key);
        }
        
        static hasInstance() {
            const key = instanceName || className.name;
            return SingletonClass.#instances.has(key);
        }
    };
    
    return SingletonClass;
}

// WeakRef registry for automatic memory cleanup
export class WeakRegistry {
    #registry = new Map();
    #cleanupInterval = null;
    
    constructor(cleanupIntervalMs = 30000) {
        this.#cleanupInterval = setInterval(() => this.cleanup(), cleanupIntervalMs);
    }
    
    register(key, object) {
        if (!object || typeof object !== 'object') {
            throw new Error('[WeakRegistry] Object is required');
        }
        
        this.#registry.set(key, new WeakRef(object));
    }
    
    get(key) {
        const ref = this.#registry.get(key);
        if (!ref) return null;
        
        const object = ref.deref();
        if (!object) {
            this.#registry.delete(key);
            return null;
        }
        
        return object;
    }
    
    has(key) {
        return this.get(key) !== null;
    }
    
    delete(key) {
        this.#registry.delete(key);
    }
    
    cleanup() {
        for (const [key, ref] of this.#registry.entries()) {
            if (!ref.deref()) {
                this.#registry.delete(key);
            }
        }
    }
    
    size() {
        this.cleanup();
        return this.#registry.size;
    }
    
    destroy() {
        if (this.#cleanupInterval) {
            clearInterval(this.#cleanupInterval);
            this.#cleanupInterval = null;
        }
        this.#registry.clear();
    }
}

export default { createSingleton, WeakRegistry };