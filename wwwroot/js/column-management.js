// RTableColumnManager JavaScript utilities for column preferences and management
export const RTableColumnManager = {
    
    initialize: function(tableId, dotNetRef) {
        try {
            if (!tableId || !dotNetRef) {
                console.warn('[RTableColumnManager] Missing required parameters for initialization');
                return;
            }
            
            // Store reference for cleanup
            window.RRTableManagers = window.RRTableManagers || {};
            window.RRTableManagers[tableId] = {
                dotNetRef: dotNetRef,
                initialized: new Date().toISOString()
            };
            
            if (window.debugLogger?.isDebugMode) {
                window.debugLogger.log('[RTableColumnManager] Initialized:', { tableId });
            }
            
        } catch (error) {
            console.error('[RTableColumnManager] Initialization failed:', error);
        }
    },
    
    loadColumnPreferences: function(tableId) {
        try {
            if (!tableId) {
                console.warn('[RTableColumnManager] No table ID provided for loading preferences');
                return {};
            }
            
            const storageKey = `rtable-columns-${tableId}`;
            const stored = localStorage.getItem(storageKey);
            
            if (!stored) {
                if (window.debugLogger?.isDebugMode) {
                    window.debugLogger.log('[RTableColumnManager] No stored preferences found for:', tableId);
                }
                return {};
            }
            
            const preferences = JSON.parse(stored);
            
            if (window.debugLogger?.isDebugMode) {
                window.debugLogger.log('[RTableColumnManager] Loaded preferences:', { tableId, preferences });
            }
            
            return preferences;
            
        } catch (error) {
            console.error('[RTableColumnManager] Failed to load preferences:', error);
            return {};
        }
    },
    
    saveColumnPreferences: function(tableId, preferences) {
        try {
            if (!tableId || !preferences) {
                console.warn('[RTableColumnManager] Missing parameters for saving preferences');
                return false;
            }
            
            const storageKey = `rtable-columns-${tableId}`;
            localStorage.setItem(storageKey, JSON.stringify(preferences));
            
            if (window.debugLogger?.isDebugMode) {
                window.debugLogger.log('[RTableColumnManager] Saved preferences:', { tableId, preferences });
            }
            
            return true;
            
        } catch (error) {
            console.error('[RTableColumnManager] Failed to save preferences:', error);
            return false;
        }
    },
    
    clearColumnPreferences: function(tableId) {
        try {
            if (!tableId) {
                console.warn('[RTableColumnManager] No table ID provided for clearing preferences');
                return false;
            }
            
            const storageKey = `rtable-columns-${tableId}`;
            localStorage.removeItem(storageKey);
            
            if (window.debugLogger?.isDebugMode) {
                window.debugLogger.log('[RTableColumnManager] Cleared preferences for:', tableId);
            }
            
            return true;
            
        } catch (error) {
            console.error('[RTableColumnManager] Failed to clear preferences:', error);
            return false;
        }
    },
    
    dispose: function(tableId) {
        try {
            if (window.RRTableManagers && window.RRTableManagers[tableId]) {
                delete window.RRTableManagers[tableId];
                
                if (window.debugLogger?.isDebugMode) {
                    window.debugLogger.log('[RTableColumnManager] Disposed:', tableId);
                }
            }
        } catch (error) {
            console.error('[RTableColumnManager] Dispose failed:', error);
        }
    }
};

// Export for global access
window.RTableColumnManager = RTableColumnManager;

export default RTableColumnManager;