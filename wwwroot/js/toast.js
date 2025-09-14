// Toast Portal Management
window.RRBlazor = window.RRBlazor || {};

// Direct global function for immediate availability
window.moveToastContainerToPortal = function(containerElement) {
    try {
        // Ensure portal-root exists
        let portalRoot = document.getElementById('portal-root');
        if (!portalRoot) {
            portalRoot = document.createElement('div');
            portalRoot.id = 'portal-root';
            portalRoot.className = 'portal-root';
            portalRoot.setAttribute('data-rr-blazor-portal', 'true');
            portalRoot.style.cssText = `
                position: fixed;
                top: 0;
                left: 0;
                width: 0;
                height: 0;
                pointer-events: none;
            `;
            document.body.appendChild(portalRoot);
        }

        // Move the toast container to portal-root with proper z-index
        if (containerElement) {
            containerElement.style.zIndex = 'var(--z-toast, 2700)';
            containerElement.style.pointerEvents = 'auto';
            containerElement.style.position = 'fixed';
            portalRoot.appendChild(containerElement);
        }
    } catch (error) {
        console.error('[RRBlazor] Failed to move toast container to portal:', error);
    }
};