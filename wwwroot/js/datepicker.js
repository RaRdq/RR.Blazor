function positionPopup(element) {
    if (!element) return;
    
    const popup = element.querySelector('.rr-datepicker-popup');
    const trigger = element.querySelector('.rr-datepicker-trigger');
    
    if (!popup || !trigger) return;
    
    const triggerRect = trigger.getBoundingClientRect();
    const viewportWidth = window.innerWidth;
    const viewportHeight = window.innerHeight;
    const popupWidth = 320;
    const popupHeight = 400;
    const buffer = 16;
    
    const spaceBelow = viewportHeight - triggerRect.bottom - buffer;
    const spaceAbove = triggerRect.top - buffer;
    const spaceLeft = triggerRect.left - buffer;
    const spaceRight = viewportWidth - triggerRect.right - buffer;
    
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
    
    top = Math.max(buffer, Math.min(top, viewportHeight - popupHeight - buffer));
    left = Math.max(buffer, Math.min(left, viewportWidth - popupWidth - buffer));
    
    popup.style.position = 'fixed';
    popup.style.top = `${Math.round(top)}px`;
    popup.style.left = `${Math.round(left)}px`;
    popup.style.width = `${popupWidth}px`;
    popup.style.maxHeight = `${popupHeight}px`;
    popup.style.zIndex = getZIndex(trigger);
    popup.style.visibility = 'visible';
    popup.style.opacity = '1';
    popup.style.pointerEvents = 'auto';
}

function getZIndex(trigger) {
    const modal = trigger.closest('.modal, .modal-content, .modal-body');
    return modal ? '9999' : '9990';
}

export { positionPopup };