window.RKanboardModule = (() => {
    'use strict';

    const instances = new Map();

    function initialize(boardId, options) {
        if (!boardId) return false;

        const element = document.getElementById(boardId);
        if (!element) return false;

        dispose(boardId);

        const settings = {
            autoScroll: true,
            edgeThreshold: 48,
            scrollSpeed: 18,
            ...options
        };

        const instance = {
            id: boardId,
            element,
            options: settings,
            pointer: null,
            raf: null,
            handlers: {}
        };

        attachEvents(instance);
        instances.set(boardId, instance);
        return true;
    }

    function attachEvents(instance) {
        const element = instance.element;

        const onDragOver = event => {
            if (!instance.options.autoScroll) return;
            instance.pointer = { x: event.clientX, y: event.clientY };
            if (!instance.raf) {
                instance.raf = requestAnimationFrame(() => step(instance));
            }
        };

        const clearPointer = () => {
            instance.pointer = null;
            stopLoop(instance);
        };

        const onDragLeave = event => {
            if (!event.relatedTarget || !instance.element.contains(event.relatedTarget)) {
                clearPointer();
            }
        };

        element.addEventListener('dragover', onDragOver);
        element.addEventListener('dragleave', onDragLeave);
        element.addEventListener('drop', clearPointer);
        element.addEventListener('dragend', clearPointer);

        instance.handlers.onDragOver = onDragOver;
        instance.handlers.onDragLeave = onDragLeave;
        instance.handlers.onDragEnd = clearPointer;
        instance.handlers.onDrop = clearPointer;
    }

    function step(instance) {
        if (!instance.pointer) {
            stopLoop(instance);
            return;
        }

        const containers = getScrollableContainers(instance.element);
        if (containers.length === 0) {
            stopLoop(instance);
            return;
        }

        const { x, y } = instance.pointer;
        const threshold = Math.max(8, Number(instance.options.edgeThreshold) || 48);
        const speed = Math.max(2, Number(instance.options.scrollSpeed) || 12);
        let scrolled = false;

        containers.forEach(container => {
            const rect = container.getBoundingClientRect();
            if (x < rect.left || x > rect.right || y < rect.top || y > rect.bottom) {
                return;
            }

            if (container.classList.contains('is-horizontal')) {
                if (y < rect.top + threshold) {
                    container.scrollTop -= speed;
                    scrolled = true;
                } else if (y > rect.bottom - threshold) {
                    container.scrollTop += speed;
                    scrolled = true;
                }
            } else {
                if (x < rect.left + threshold) {
                    container.scrollLeft -= speed;
                    scrolled = true;
                } else if (x > rect.right - threshold) {
                    container.scrollLeft += speed;
                    scrolled = true;
                }
            }
        });

        if (scrolled) {
            instance.raf = requestAnimationFrame(() => step(instance));
        } else {
            stopLoop(instance);
        }
    }

    function stopLoop(instance) {
        if (instance.raf) {
            cancelAnimationFrame(instance.raf);
            instance.raf = null;
        }
    }

    function getScrollableContainers(root) {
        const containers = Array.from(root.querySelectorAll('.rkanboard-body, .rkanboard-swimlane-body'));
        return containers.filter(container => container && (container.scrollWidth > container.clientWidth || container.scrollHeight > container.clientHeight));
    }

    function dispose(boardId) {
        if (!boardId) return;

        const instance = instances.get(boardId);
        if (!instance) return;

        const element = instance.element;
        if (element && instance.handlers) {
            element.removeEventListener('dragover', instance.handlers.onDragOver);
            element.removeEventListener('dragleave', instance.handlers.onDragLeave);
            element.removeEventListener('drop', instance.handlers.onDrop);
            element.removeEventListener('dragend', instance.handlers.onDragEnd);
        }

        stopLoop(instance);
        instances.delete(boardId);
    }

    function refresh(boardId, options) {
        const instance = instances.get(boardId);
        if (!instance) {
            initialize(boardId, options);
            return true;
        }

        instance.options = {
            ...instance.options,
            ...(options || {})
        };

        return true;
    }

    return {
        initialize,
        dispose,
        refresh
    };
})();
export default window.RKanboardModule;
