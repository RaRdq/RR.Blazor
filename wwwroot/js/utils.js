// General DOM and utility functions

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
    if (!element) {
        return { width: 0, height: 0 };
    }
    
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
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
}

export async function downloadFileFromStream(fileName, contentStream) {
    try {
        const arrayBuffer = await contentStream.arrayBuffer();
        const blob = new Blob([arrayBuffer]);
        const url = URL.createObjectURL(blob);
        
        const anchorElement = document.createElement('a');
        anchorElement.href = url;
        anchorElement.download = fileName ?? '';
        anchorElement.click();
        anchorElement.remove();
        
        URL.revokeObjectURL(url);
        return true;
    } catch (error) {
        if (window.debugLogger) {
            window.debugLogger.error('Download failed:', error);
        }
        return false;
    }
}

export function downloadFile(url, fileName) {
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName || 'download';
    link.style.display = 'none';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

export function setupOutsideClickHandler(containerId, callback) {
    const container = document.querySelector(containerId);
    if (!container) {
        console.warn('[RR.Blazor] Container not found:', containerId);
        return;
    }

    const outsideClickHandler = function(event) {
        if (!container.contains(event.target)) {
            callback();
        }
    };

    container._outsideClickHandler = outsideClickHandler;
    document.addEventListener('click', outsideClickHandler);
    
    if (window.debugLogger) {
        window.debugLogger.log('Outside click handler attached for:', containerId);
    }
}

export function removeOutsideClickHandler(containerId) {
    const container = document.querySelector(containerId);
    if (container && container._outsideClickHandler) {
        document.removeEventListener('click', container._outsideClickHandler);
        delete container._outsideClickHandler;
        
        if (window.debugLogger) {
            window.debugLogger.log('Outside click handler removed for:', containerId);
        }
    }
}

export function addEventListener(elementId, eventName, dotNetRef, methodName) {
    const element = document.getElementById(elementId);
    if (!element || !dotNetRef) return;

    const handler = function(e) {
        try {
            if (dotNetRef && typeof dotNetRef.invokeMethodAsync === 'function') {
                dotNetRef.invokeMethodAsync(methodName, e.detail).catch(err => {
                    if (!err.message?.includes('disposed')) {
                        console.error('JSInterop error:', err);
                    }
                });
            }
        } catch (error) {
            if (!error.message?.includes('disposed')) {
                console.error('Event handler error:', error);
            }
        }
    };

    element.addEventListener(eventName, handler);
    
    if (!element._rrEventCleanups) {
        element._rrEventCleanups = [];
    }
    element._rrEventCleanups.push(() => {
        element.removeEventListener(eventName, handler);
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