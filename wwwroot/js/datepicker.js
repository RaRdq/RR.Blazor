// Track portaled elements for cleanup
const portaledPopups = new WeakSet();

function positionPopup(element) {
    if (!element) return;
    
    const popup = element.querySelector('.rr-datepicker-popup');
    const trigger = element.querySelector('.rr-datepicker-trigger');
    
    if (!popup || !trigger) return;
    
    // Portal solution: Move popup to document.body to escape stacking context
    if (popup.parentElement !== document.body) {
        popup.dataset.originalParent = element.tagName.toLowerCase();
        document.body.appendChild(popup);
        portaledPopups.add(popup);
    }
    
    const isInModal = trigger.closest('.modal, .modal-content, .modal-body, .modal-provider, .modal-backdrop-container');
    const triggerRect = trigger.getBoundingClientRect();
    const viewportWidth = window.innerWidth;
    const viewportHeight = window.innerHeight;
    const popupWidth = 320;
    const popupHeight = 400;
    const buffer = 16;
    
    let constraintRect = { top: 0, left: 0, width: viewportWidth, height: viewportHeight };
    
    if (isInModal) {
        const modalContent = isInModal.querySelector('.modal-content') || isInModal;
        if (modalContent) {
            const modalRect = modalContent.getBoundingClientRect();
            constraintRect = {
                top: Math.max(0, modalRect.top + 8),
                left: Math.max(0, modalRect.left + 8),
                width: Math.min(viewportWidth, modalRect.width - 16),
                height: Math.min(viewportHeight, modalRect.height - 16)
            };
        }
    }
    
    const spaceBelow = constraintRect.top + constraintRect.height - triggerRect.bottom - buffer;
    const spaceAbove = triggerRect.top - constraintRect.top - buffer;
    const spaceLeft = triggerRect.left - constraintRect.left - buffer;
    const spaceRight = constraintRect.left + constraintRect.width - triggerRect.right - buffer;
    
    let top, left;
    
    if (spaceBelow >= popupHeight || spaceBelow > spaceAbove) {
        top = triggerRect.bottom + 4;
    } else {
        top = triggerRect.top - popupHeight - 4;
    }
    
    if (spaceRight >= popupWidth) {
        left = triggerRect.left;
    } else if (spaceLeft >= popupWidth) {
        left = triggerRect.right - popupWidth;
    } else {
        left = triggerRect.left + (triggerRect.width - popupWidth) / 2;
    }
    
    top = Math.max(constraintRect.top + buffer, Math.min(top, constraintRect.top + constraintRect.height - popupHeight - buffer));
    left = Math.max(constraintRect.left + buffer, Math.min(left, constraintRect.left + constraintRect.width - popupWidth - buffer));
    
    popup.style.position = 'fixed';
    popup.style.top = `${Math.round(top)}px`;
    popup.style.left = `${Math.round(left)}px`;
    popup.style.width = `${popupWidth}px`;
    popup.style.maxHeight = `${popupHeight}px`;
    popup.style.visibility = 'visible';
    popup.style.opacity = '1';
    popup.style.pointerEvents = 'auto';
    
    // Set appropriate z-index based on modal context
    popup.style.zIndex = isInModal ? '1100' : '900';
}

function cleanupPortaledPopups() {
    // Clean up any orphaned popup elements in document.body
    document.querySelectorAll('body > .rr-datepicker-popup').forEach(popup => {
        if (popup.style.visibility === 'hidden' || popup.style.opacity === '0') {
            popup.remove();
        }
    });
}

export { positionPopup, cleanupPortaledPopups };