
export function scrollIntoView(elementId, options = {}) {
    const element = document.getElementById(elementId);
    if (element) {
        element.scrollIntoView({
            behavior: 'smooth',
            block: 'nearest',
            ...options
        });
    }
}

export function getElementDimensions(elementId) {
    const element = document.getElementById(elementId);
    if (!element) throw new Error(`Element not found: ${elementId}`);
    
    const rect = element.getBoundingClientRect();
    return {
        width: rect.width,
        height: rect.height,
        top: rect.top,
        left: rect.left
    };
}

export function toggleClass(elementId, className, add) {
    const element = document.getElementById(elementId);
    if (element) {
        if (add) {
            element.classList.add(className);
        } else {
            element.classList.remove(className);
        }
    }
}

export function updateUrlWithoutScroll(newUrl) {
    if (window.history && window.history.pushState) {
        window.history.pushState({ path: newUrl }, '', newUrl);
    }
}

export function downloadContent(content, fileName, contentType = 'text/plain') {
    const blob = new Blob([content], { type: contentType });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    link?.remove();
    window.URL.revokeObjectURL(url);
}

export async function downloadFileFromStream(fileName, contentStream) {
    const arrayBuffer = await contentStream.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);
    
    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = fileName ?? '';
    anchorElement.click();
    anchorElement?.remove();
    
    URL.revokeObjectURL(url);
    return true;
}

export function downloadFile(url, fileName) {
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName || 'download';
    link.style.display = 'none';
    document.body.appendChild(link);
    link.click();
    link?.remove();
}

export function setupOutsideClickHandler(containerId, callback) {
    const container = document.querySelector(containerId);
    if (!container) throw new Error(`Container not found: ${containerId}`);

    const outsideClickHandler = function(event) {
        if (!container.contains(event.target)) {
            const now = Date.now();
            if (!outsideClickHandler._lastClick || now - outsideClickHandler._lastClick > 50) {
                outsideClickHandler._lastClick = now;
                callback();
            }
        }
    };

    container._outsideClickHandler = outsideClickHandler;
    document.addEventListener('click', outsideClickHandler, { passive: true });
}

export function removeOutsideClickHandler(containerId) {
    const container = document.querySelector(containerId);
    if (container && container._outsideClickHandler) {
        document.removeEventListener('click', container._outsideClickHandler);
        delete container._outsideClickHandler;
    }
}

export function addEventListener(elementId, eventName, dotNetRef, methodName, options = {}) {
    const element = document.getElementById(elementId);
    if (!element) throw new Error(`Element not found: ${elementId}`);
    if (!dotNetRef) throw new Error('DotNet reference required');

    const passiveEvents = ['scroll', 'wheel', 'touchstart', 'touchmove', 'touchend'];
    const eventOptions = {
        passive: passiveEvents.includes(eventName),
        ...options
    };

    const handler = function(e) {
        if (dotNetRef && typeof dotNetRef.invokeMethodAsync === 'function') {
            const invoke = () => {
                dotNetRef.invokeMethodAsync(methodName, e.detail).catch(err => {
                    if (!err.message?.includes('disposed')) {
                        console.error('JSInterop error:', err);
                    }
                });
            };

            if (window.requestIdleCallback && !['click', 'keydown', 'keyup'].includes(eventName)) {
                requestIdleCallback(invoke, { timeout: 100 });
            } else {
                invoke();
            }
        }
    };

    element.addEventListener(eventName, handler, eventOptions);
    
    if (!element._rrEventCleanups) {
        element._rrEventCleanups = [];
    }
    element._rrEventCleanups.push(() => {
        element.removeEventListener(eventName, handler, eventOptions);
    });
}

export default {
    scrollIntoView,
    getElementDimensions,
    toggleClass,
    updateUrlWithoutScroll,
    downloadContent,
    downloadFileFromStream,
    downloadFile,
    setupOutsideClickHandler,
    removeOutsideClickHandler,
    addEventListener
};