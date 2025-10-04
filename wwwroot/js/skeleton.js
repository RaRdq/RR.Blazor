export function generateSkeletonHTML(containerElement, animated = true) {
    if (!containerElement) {
        return '';
    }

    const hiddenContainer = containerElement.querySelector('.skeleton-hidden-container');
    if (!hiddenContainer) {
        return '';
    }

    const animationClass = animated ? ' skeleton-shimmer' : '';
    const parentRect = containerElement.getBoundingClientRect();
    const maxContainerWidth = parentRect.width || 500;
    
    function detectElementType(element) {
        if (!element) return null;

        const tag = element.tagName?.toLowerCase() || '';
        const className = element.className?.toString() || '';
        const classList = element.classList || [];
        const style = element.getAttribute('style') || '';

        if (style.includes('height:') && style.includes('%') &&
            (className.includes('bg-primary') || className.includes('bg-'))) {
            return 'chart-bar';
        }

        if (classList.contains('chip') || classList.contains('badge') ||
            className.includes('chip') || className.includes('badge')) {
            return 'chip';
        }

        if (classList.contains('avatar') || classList.contains('r-avatar') ||
            className.includes('avatar') || className.includes('rounded-full')) {
            return 'avatar';
        }

        if (tag === 'button' || classList.contains('btn') ||
            className.includes('button') || className.includes('btn')) {
            return 'button';
        }

        if ((tag === 'i' || tag === 'svg' || tag === 'span') &&
            (className.includes('icon') || className.includes('material-icons') ||
             className.includes('fa-') || className.includes('bi-'))) {
            return 'icon';
        }

        if (/^h[1-6]$/.test(tag)) {
            return 'title';
        }

        const hasDirectText = Array.from(element.childNodes || [])
            .some(node => node.nodeType === Node.TEXT_NODE && node.textContent?.trim());

        if (hasDirectText || tag === 'p' || tag === 'span' ||
            className.includes('text') || className.includes('caption')) {
            return 'text';
        }

        return null;
    }
    
    function getElementDimensions(element, type) {
        const tag = element.tagName?.toLowerCase() || '';
        const text = element.textContent?.trim() || '';
        const className = element.className?.toString() || '';
        const style = element.getAttribute('style') || '';

        const dimensions = {
            width: 100,
            height: 16
        };

        switch(type) {
            case 'chart-bar':
                const heightMatch = style.match(/height:\s*(\d+)/);
                if (heightMatch) {
                    dimensions.height = parseInt(heightMatch[1]);
                } else {
                    dimensions.height = 50;
                }
                dimensions.width = 16;
                break;

            case 'chip':
            case 'badge':
                const badgeRect = element.getBoundingClientRect();
                if (badgeRect.width > 0) {
                    dimensions.width = Math.min(Math.round(badgeRect.width), maxContainerWidth * 0.9);
                } else {
                    dimensions.width = Math.min(text.length * 7 + 20, 120, maxContainerWidth * 0.9);
                }
                dimensions.height = 24;
                break;

            case 'avatar':
                const rect = element.getBoundingClientRect();
                if (rect.width > 0 && rect.height > 0) {
                    dimensions.width = Math.round(rect.width);
                    dimensions.height = Math.round(rect.height);
                } else if (element.className?.includes('w-8')) {
                    dimensions.width = 32;
                    dimensions.height = 32;
                } else {
                    dimensions.width = 40;
                    dimensions.height = 40;
                }
                break;

            case 'button':
                dimensions.width = Math.min(text.length * 8 + 30, 150);
                dimensions.height = 36;
                break;

            case 'icon':
                dimensions.width = 24;
                dimensions.height = 24;
                break;

            case 'title':
                dimensions.width = Math.min(text.length * 10, maxContainerWidth * 0.8);
                dimensions.height = tag === 'h1' ? 32 : tag === 'h2' ? 28 : 24;
                break;

            case 'text':
                if (className.includes('text-h4') || className.includes('font-bold')) {
                    dimensions.width = Math.min(text.length * 12, maxContainerWidth * 0.5);
                    dimensions.height = 32;
                } else if (className.includes('caption')) {
                    dimensions.width = Math.min(text.length * 6, maxContainerWidth * 0.7);
                    dimensions.height = 14;
                } else {
                    dimensions.width = Math.min(text.length * 7, maxContainerWidth * 0.9);
                    dimensions.height = 16;
                }
                break;
        }

        return dimensions;
    }
    
    function createSkeletonElement(type, dimensions) {
        const skeleton = document.createElement('div');
        skeleton.className = `skeleton skeleton-${type}${animationClass}`;

        const width = type === 'chart-bar' ? dimensions.width : Math.min(dimensions.width, maxContainerWidth * 0.95);

        const styles = {
            width: `${width}px`,
            height: `${dimensions.height}px`,
            display: (type === 'chip' || type === 'badge' || type === 'button') ? 'inline-block' : 'block'
        };

        if (type === 'chart-bar') {
            styles.borderRadius = '4px 4px 0 0';
            styles.alignSelf = 'flex-end';
        } else if (type === 'avatar' || type === 'avatar-inner' || type === 'icon-circle') {
            styles.borderRadius = '50%';
        } else if (type === 'chip' || type === 'badge') {
            styles.borderRadius = '12px';
        } else if (type === 'button') {
            styles.borderRadius = '6px';
        } else {
            styles.borderRadius = '4px';
        }

        if (type === 'chip' || type === 'badge') {
            styles.marginRight = '8px';
        }

        skeleton.style.cssText = Object.entries(styles)
            .map(([key, value]) => `${key.replace(/([A-Z])/g, '-$1').toLowerCase()}: ${value}`)
            .join('; ');

        skeleton.setAttribute('aria-hidden', 'true');
        return skeleton;
    }
    
    function processElement(element, depth = 0) {
        if (!element || element.nodeType !== Node.ELEMENT_NODE || depth > 15) {
            if (depth > 15) {
                console.warn('RSkeleton: Max depth (15) exceeded, skipping nested content');
            }
            return null;
        }

        const tag = element.tagName;
        if (['SCRIPT', 'STYLE', 'META', 'LINK', 'BR', 'HR'].includes(tag)) {
            return null;
        }

        const type = detectElementType(element);
        const hasStructuralChildren = element.children && element.children.length > 0;

        // Special handling for avatars - preserve container, replace inner content
        if (type === 'avatar') {
            const avatarContainer = element.cloneNode(false);
            avatarContainer.removeAttribute('id');
            avatarContainer.removeAttribute('_bl');

            const dimensions = getElementDimensions(element, type);
            const skeleton = createSkeletonElement('avatar-inner', dimensions);
            avatarContainer.appendChild(skeleton);
            return avatarContainer;
        }

        // Badges, chips, buttons - always replace entirely (even with children)
        if (type === 'chip' || type === 'badge' || type === 'button' || type === 'icon') {
            const dimensions = getElementDimensions(element, type);
            return createSkeletonElement(type, dimensions);
        }

        // Other typed elements - only replace if no structural children
        if (type && !hasStructuralChildren) {
            const dimensions = getElementDimensions(element, type);
            return createSkeletonElement(type, dimensions);
        }

        const container = element.cloneNode(false);
        container.removeAttribute('id');
        container.removeAttribute('_bl');

        let hasContent = false;
        const processedChildren = [];

        for (const child of element.children) {
            const processed = processElement(child, depth + 1);
            if (processed) {
                processedChildren.push(processed);
                hasContent = true;
            }
        }

        if (processedChildren.length > 0) {
            processedChildren.forEach(child => container.appendChild(child));
        } else {
            const directTextNodes = Array.from(element.childNodes || [])
                .filter(node => node.nodeType === Node.TEXT_NODE && node.textContent?.trim());

            if (directTextNodes.length > 0) {
                const textContent = element.textContent?.trim() || '';
                if (textContent) {
                    const dimensions = getElementDimensions(element, 'text');
                    const skeleton = createSkeletonElement('text', dimensions);
                    container.appendChild(skeleton);
                    hasContent = true;
                }
            }
        }

        return hasContent ? container : null;
    }
    
    const results = [];

    try {
        for (const child of hiddenContainer.children) {
            const processed = processElement(child, 0);
            if (processed) {
                results.push(processed.outerHTML);
            }
        }
    } catch (error) {
    }
    
    const html = results.join('');
    return html;
}

export function init() {
    return true;
}

export function dispose() {
}

export default { generateSkeletonHTML, init, dispose };