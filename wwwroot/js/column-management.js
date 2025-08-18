export const RTableColumnManager = {
    
    initialize: function(tableId, dotNetRef) {
        
        window.RRTableManagers = window.RRTableManagers || {};
        window.RRTableManagers[tableId] = {
            dotNetRef: dotNetRef,
            initialized: new Date().toISOString()
        };
    },
    
    loadColumnPreferences: function(tableId) {
        
        const storageKey = `rtable-columns-${tableId}`;
        const stored = localStorage.getItem(storageKey);
        
        return stored ? JSON.parse(stored) : {};
    },
    
    saveColumnPreferences: function(tableId, preferences) {
        
        const storageKey = `rtable-columns-${tableId}`;
        localStorage.setItem(storageKey, JSON.stringify(preferences));
        return true;
    },
    
    clearColumnPreferences: function(tableId) {
        
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