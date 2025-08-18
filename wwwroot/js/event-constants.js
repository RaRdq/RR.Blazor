const EVENTS = Object.freeze({
    // Portal Events
    PORTAL_CREATE_REQUEST: 'portal-create-request',
    PORTAL_CREATED: 'portal-created',
    PORTAL_DESTROY_REQUEST: 'portal-destroy-request',
    PORTAL_DESTROYING: 'portal-destroying',
    PORTAL_DESTROYED: 'portal-destroyed',
    PORTAL_CLEANUP_ALL_REQUEST: 'portal-cleanup-all-request',
    PORTAL_ALL_DESTROYED: 'portal-all-destroyed',
    PORTAL_ELEMENT_MOVED: 'portal-element-moved',
    PORTAL_ELEMENT_RESTORED: 'portal-element-restored',
    
    // Backdrop Events
    BACKDROP_CREATE_REQUEST: 'backdrop-create-request',
    BACKDROP_CREATED: 'backdrop-created',
    BACKDROP_DESTROY_REQUEST: 'backdrop-destroy-request',
    BACKDROP_DESTROYED: 'backdrop-destroyed',
    BACKDROP_CLEANUP_ALL_REQUEST: 'backdrop-cleanup-all-request',
    BACKDROP_ALL_DESTROYED: 'backdrop-all-destroyed',
    BACKDROP_CLICK: 'backdrop-click',
    
    // UI Component Coordination Events
    UI_COMPONENT_OPENING: 'ui-component-opening',
    UI_COMPONENT_OPENED: 'ui-component-opened',
    UI_COMPONENT_CLOSING: 'ui-component-closing',
    UI_COMPONENT_CLOSED: 'ui-component-closed',
    UI_COMPONENT_CLOSE_REQUEST: 'ui-component-close-request',
    
    // Modal Events
    MODAL_PORTAL_READY: 'modal-portal-ready',
    MODAL_PORTAL_DESTROYED: 'modal-portal-destroyed',
    MODAL_BACKDROP_READY: 'modal-backdrop-ready',
    MODAL_BACKDROP_DESTROYED: 'modal-backdrop-destroyed',
    
    // Choice/Dropdown Events
    CHOICE_KEYBOARD_ENABLE: 'choice-keyboard-enable',
    CHOICE_KEYBOARD_DISABLE: 'choice-keyboard-disable',
    
    // Click Outside Events
    CLICK_OUTSIDE: 'click-outside',
    
    // Focus Events
    FOCUS_TRAP_CREATED: 'focus-trap-created',
    FOCUS_TRAP_DESTROYED: 'focus-trap-destroyed',
    
    // Keyboard Navigation Events
    KEYBOARD_NAV_ENABLED: 'keyboard-nav-enabled',
    KEYBOARD_NAV_DISABLED: 'keyboard-nav-disabled',
    KEYBOARD_NAV_ITEM_SELECTED: 'keyboard-nav-item-selected',
    
    // Form Events
    FORM_VALIDATION_REQUEST: 'form-validation-request',
    FORM_VALIDATION_RESULT: 'form-validation-result',
    FORM_SUBMIT_REQUEST: 'form-submit-request',
    FORM_RESET_REQUEST: 'form-reset-request',
    
    // Table Events
    TABLE_SCROLL_SYNC: 'table-scroll-sync',
    TABLE_COLUMN_RESIZE: 'table-column-resize',
    TABLE_COLUMN_REORDER: 'table-column-reorder',
    
    // Theme Events
    THEME_CHANGED: 'theme-changed',
    DENSITY_CHANGED: 'density-changed',
    
    // Tooltip Events
    TOOLTIP_SHOW_REQUEST: 'tooltip-show-request',
    TOOLTIP_HIDE_REQUEST: 'tooltip-hide-request',
    
    // File Upload Events
    FILE_UPLOAD_START: 'file-upload-start',
    FILE_UPLOAD_PROGRESS: 'file-upload-progress',
    FILE_UPLOAD_COMPLETE: 'file-upload-complete',
    FILE_UPLOAD_ERROR: 'file-upload-error',
    
    // App Shell Events
    SIDEBAR_TOGGLE: 'sidebar-toggle',
    SIDEBAR_COLLAPSED: 'sidebar-collapsed',
    SIDEBAR_EXPANDED: 'sidebar-expanded',
    
    // Intersection Observer Events
    ELEMENT_VISIBLE: 'element-visible',
    ELEMENT_HIDDEN: 'element-hidden',
    
    // Column Management Events
    COLUMNS_CHANGED: 'columns-changed',
    COLUMNS_RESET: 'columns-reset',
    
    // Tabs Events
    TAB_ACTIVATED: 'tab-activated',
    TAB_DEACTIVATED: 'tab-deactivated',
    
    // Datepicker Events
    DATE_SELECTED: 'date-selected',
    DATE_RANGE_SELECTED: 'date-range-selected',
    
    // Autosuggest Events
    SUGGESTION_SELECTED: 'suggestion-selected',
    SUGGESTIONS_SHOWN: 'suggestions-shown',
    SUGGESTIONS_HIDDEN: 'suggestions-hidden'
});

const EVENT_PRIORITIES = Object.freeze({
    LOW: 0,
    NORMAL: 50,
    HIGH: 100,
    CRITICAL: 200
});

const COMPONENT_TYPES = Object.freeze({
    MODAL: 'modal',
    CHOICE: 'choice',
    DROPDOWN: 'dropdown',
    TOOLTIP: 'tooltip',
    DATEPICKER: 'datepicker',
    AUTOSUGGEST: 'autosuggest',
    SIDEBAR: 'sidebar',
    FORM: 'form',
    TABLE: 'table'
});

class EventDispatcher {
    static dispatch(eventName, detail = {}, options = {}) {
        const event = new CustomEvent(eventName, {
            detail,
            bubbles: options.bubbles !== false,
            cancelable: options.cancelable || false,
            composed: options.composed || false
        });
        
        const target = options.target || document;
        return target.dispatchEvent(event);
    }
    
    static listen(eventName, handler, options = {}) {
        const target = options.target || document;
        target.addEventListener(eventName, handler, options);
        
        return () => target.removeEventListener(eventName, handler, options);
    }
    
    static once(eventName, handler, options = {}) {
        return this.listen(eventName, handler, { ...options, once: true });
    }
    
    static async waitFor(eventName, options = {}) {
        return new Promise((resolve, reject) => {
            const timeout = options.timeout || 5000;
            const target = options.target || document;
            
            let timeoutId;
            if (timeout > 0) {
                timeoutId = setTimeout(() => {
                    target.removeEventListener(eventName, handler);
                    reject(new Error(`Timeout waiting for event: ${eventName}`));
                }, timeout);
            }
            
            const handler = (event) => {
                if (options.filter && !options.filter(event)) {
                    return;
                }
                
                if (timeoutId) clearTimeout(timeoutId);
                target.removeEventListener(eventName, handler);
                resolve(event);
            };
            
            target.addEventListener(eventName, handler);
        });
    }
}

if (typeof window !== 'undefined') {
    window.RRBlazor = window.RRBlazor || {};
    window.RRBlazor.Events = EVENTS;
    window.RRBlazor.EventPriorities = EVENT_PRIORITIES;
    window.RRBlazor.ComponentTypes = COMPONENT_TYPES;
    window.RRBlazor.EventDispatcher = EventDispatcher;
}

export { EVENTS, EVENT_PRIORITIES, COMPONENT_TYPES, EventDispatcher };
export default EVENTS;