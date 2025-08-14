export const RTableColumnManager = {
    
    initialize: function(tableId, dotNetRef) {
        if (!tableId || !dotNetRef) {
            throw new Error('Missing required parameters for initialization');
        }
        
        window.RRTableManagers = window.RRTableManagers || {};
        window.RRTableManagers[tableId] = {
            dotNetRef: dotNetRef,
            initialized: new Date().toISOString()
        };
    },
    
    loadColumnPreferences: function(tableId) {
        if (!tableId) {
            throw new Error('No table ID provided');
        }
        
        const storageKey = `rtable-columns-${tableId}`;
        const stored = localStorage.getItem(storageKey);
        
        return stored ? JSON.parse(stored) : {};
    },
    
    saveColumnPreferences: function(tableId, preferences) {
        if (!tableId || !preferences) {
            throw new Error('Missing parameters for saving preferences');
        }
        
        const storageKey = `rtable-columns-${tableId}`;
        localStorage.setItem(storageKey, JSON.stringify(preferences));
        return true;
    },
    
    clearColumnPreferences: function(tableId) {
        if (!tableId) {
            throw new Error('No table ID provided');
        }
        
        const storageKey = `rtable-columns-${tableId}`;
        localStorage.removeItem(storageKey);
        return true;
    },
    
    dispose: function(tableId) {
        if (window.RRTableManagers && window.RRTableManagers[tableId]) {
            delete window.RRTableManagers[tableId];
        }
    }
};

window.RTableColumnManager = RTableColumnManager;

export default RTableColumnManager;