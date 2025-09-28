
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
    link.remove();
    window.URL.revokeObjectURL(url);
}

export async function downloadFileFromStream(fileName, contentStream) {
    const arrayBuffer = await contentStream.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);
    
    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = fileName;
    anchorElement.click();
    anchorElement.remove();
    
    URL.revokeObjectURL(url);
    return true;
}

export function downloadFile(url, fileName) {
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;
    link.style.display = 'none';
    document.body.appendChild(link);
    link.click();
    link.remove();
}


export function addEventListener(elementId, eventName, dotNetRef, methodName, options = {}) {
    const element = document.getElementById(elementId);

    const passiveEvents = ['scroll', 'wheel', 'touchstart', 'touchmove', 'touchend'];
    const eventOptions = {
        passive: passiveEvents.includes(eventName),
        ...options
    };

    const handler = function(e) {
        if (dotNetRef && typeof dotNetRef.invokeMethodAsync === 'function') {
            const invoke = () => {
                dotNetRef.invokeMethodAsync(methodName, e.detail).catch(err => {
                    if (!err.message.includes('disposed')) {
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

window.RRBlazor = window.RRBlazor || {};
export function createCleanupManager() {
    const cleanupFunctions = [];

    return {
        add: (cleanupFn) => cleanupFunctions.push(cleanupFn),
        addEventListener: (element, eventType, handler, options) => {
            if (typeof element === 'string') {
                element = document.getElementById(element) || document.querySelector(element);
            }
            if (element) {
                element.addEventListener(eventType, handler, options);
                cleanupFunctions.push(() => element.removeEventListener(eventType, handler));
            }
        },
        executeAll: () => {
            cleanupFunctions.forEach(fn => {
                try {
                    fn();
                } catch (error) {
                }
            });
            cleanupFunctions.length = 0;
        }
    };
}
export function waitForPortal(componentId, componentType = null, timeout = 2000) {
    return new Promise((resolve, reject) => {
        // The requesterId should match what's being dispatched - just the componentId
        const requesterId = componentId;

        const timeoutId = setTimeout(() => {
            document.removeEventListener(window.RRBlazor.Events.PORTAL_CREATED, handler);
            const errorMsg = componentType
                ? `Portal creation timeout for ${componentType} ${componentId}`
                : `Portal creation timeout for ${componentId}`;
            reject(new Error(errorMsg));
        }, timeout);

        const handler = (event) => {
            if (event.detail.requesterId === requesterId) {
                clearTimeout(timeoutId);
                document.removeEventListener(window.RRBlazor.Events.PORTAL_CREATED, handler);
                resolve(event.detail);
            }
        };

        document.addEventListener(window.RRBlazor.Events.PORTAL_CREATED, handler);
    });
}

window.RRBlazor.Utils = {
    scrollIntoView,
    getElementDimensions,
    toggleClass,
    updateUrlWithoutScroll,
    downloadContent,
    downloadFileFromStream,
    downloadFile,
    addEventListener,
    createCleanupManager,
    waitForPortal
};

export default {
    scrollIntoView,
    getElementDimensions,
    toggleClass,
    updateUrlWithoutScroll,
    downloadContent,
    downloadFileFromStream,
    downloadFile,
    addEventListener,
    createCleanupManager,
    waitForPortal
};