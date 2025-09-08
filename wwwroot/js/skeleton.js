export function generateSkeletonHTML(containerElement, animated = true) {
    if (!containerElement) {
        console.warn('Skeleton: No container element provided');
        return '';
    }

    const hiddenContainer = containerElement.querySelector('.skeleton-hidden-container');
    if (!hiddenContainer) {
        console.warn('Skeleton: No hidden container found');
        return '';
    }
    
    const animationClass = animated ? ' skeleton-shimmer' : '';
    
    // Temporarily make container visible to get dimensions
    const originalStyles = {
        visibility: hiddenContainer.style.visibility,
        opacity: hiddenContainer.style.opacity,
        position: hiddenContainer.style.position,
        height: hiddenContainer.style.height
    };
    
    // Make visible but off-screen to measure
    hiddenContainer.style.visibility = 'visible';
    hiddenContainer.style.opacity = '0';
    hiddenContainer.style.position = 'absolute';
    hiddenContainer.style.height = 'auto';
    hiddenContainer.style.left = '-9999px';
    
    // Process elements to create skeleton versions
    function processElement(element, depth = 0) {
        if (!element || element.nodeType !== Node.ELEMENT_NODE) return null;
        if (depth > 20) return null; // Prevent infinite recursion
        
        // Skip script, style, and other non-visual elements
        const skipTags = ['SCRIPT', 'STYLE', 'META', 'LINK', 'TITLE', 'BR', 'HR', 'BLAZOR-ERROR-UI'];
        if (skipTags.includes(element.tagName)) return null;
        
        // Check if element is visible
        const computedStyle = window.getComputedStyle(element);
        if (computedStyle.display === 'none') return null;
        
        // Clone the element (preserving structure)
        const clone = element.cloneNode(false);
        
        // Preserve important classes but remove IDs
        clone.removeAttribute('id');
        clone.removeAttribute('_bl');
        
        // Get element info for skeleton processing
        const rect = element.getBoundingClientRect();
        const className = (element.className || '').toLowerCase();
        
        // Handle avatar elements specially - put skeleton inside, keep container
        if (className.includes('r-avatar') || className.includes('avatar')) {
            // Keep the avatar container but replace its content with circular skeleton
            const skeleton = document.createElement('div');
            skeleton.className = `skeleton skeleton-avatar${animationClass}`;
            skeleton.setAttribute('aria-hidden', 'true');
            
            if (rect.width > 0 && rect.height > 0) {
                const size = Math.min(rect.width, rect.height);
                skeleton.style.cssText = `
                    width: ${size}px;
                    height: ${size}px;
                    border-radius: 50%;
                    display: block;
                `;
            } else {
                // Fallback size
                skeleton.style.cssText = `
                    width: 40px;
                    height: 40px;
                    border-radius: 50%;
                    display: block;
                `;
            }
            
            clone.innerHTML = '';
            clone.appendChild(skeleton);
            return clone;
        }
        
        // Handle icon elements - replace with skeleton block
        if (element.tagName === 'I' && className.includes('icon')) {
            const skeleton = document.createElement('div');
            skeleton.className = `skeleton skeleton-icon${animationClass}`;
            skeleton.setAttribute('aria-hidden', 'true');
            
            const size = Math.max(rect.width, rect.height) || 24;
            skeleton.style.cssText = `
                width: ${size}px;
                height: ${size}px;
                border-radius: 4px;
                display: inline-block;
            `;
            
            return skeleton;
        }
        
        // Check for direct text content
        let hasDirectText = false;
        let directTextContent = '';
        
        for (const childNode of element.childNodes) {
            if (childNode.nodeType === Node.TEXT_NODE) {
                const text = childNode.textContent.trim();
                if (text) {
                    hasDirectText = true;
                    directTextContent = text;
                    break;
                }
            }
        }
        
        // If element has direct text content, replace with text skeleton
        if (hasDirectText) {
            const skeleton = document.createElement('span');
            skeleton.className = `skeleton skeleton-text${animationClass}`;
            skeleton.setAttribute('aria-hidden', 'true');
            
            // Calculate text skeleton dimensions
            let width = rect.width;
            const height = rect.height || 16;
            
            // Ensure text skeleton doesn't exceed parent container
            if (element.parentElement) {
                const parentRect = element.parentElement.getBoundingClientRect();
                if (parentRect.width > 0) {
                    width = Math.min(width, parentRect.width * 0.8); // 80% of parent max
                }
            }
            
            // Use consistent width based on content
            if (width > 0) {
                width = Math.floor(width * 0.7); // 70% of original
            } else {
                width = Math.min(directTextContent.length * 8, 150);
            }
            
            // Detect heading/title styles
            const fontSize = parseFloat(computedStyle.fontSize);
            const fontWeight = computedStyle.fontWeight;
            const tagName = element.tagName.toLowerCase();
            
            if (tagName.match(/^h[1-6]$/) || fontWeight === 'bold' || 
                parseInt(fontWeight) >= 600 || fontSize >= 20) {
                skeleton.className = `skeleton skeleton-title${animationClass}`;
            }
            
            skeleton.style.cssText = `
                width: ${width}px;
                height: ${height}px;
                border-radius: 4px;
                display: inline-block;
            `;
            
            // Replace all content with skeleton
            clone.innerHTML = '';
            clone.appendChild(skeleton);
            return clone;
        }
        
        // Process children recursively
        let hasProcessedChildren = false;
        
        for (const child of element.children) {
            const processedChild = processElement(child, depth + 1);
            if (processedChild) {
                clone.appendChild(processedChild);
                hasProcessedChildren = true;
            }
        }
        
        // Return clone if it has content or should be preserved
        const shouldPreserve = hasProcessedChildren || 
            className.includes('card') || 
            className.includes('container') ||
            className.includes('wrapper') ||
            className.includes('glass') ||
            className.includes('flex') ||
            className.includes('grid') ||
            element.tagName === 'DIV';
            
        return shouldPreserve ? clone : null;
    }
    
    // Process all root children
    const results = [];
    // Processing root children - logging removed for performance
    
    for (const child of hiddenContainer.children) {
        const processed = processElement(child, 0);
        if (processed) {
            results.push(processed.outerHTML);
        }
    }
    
    // Restore original styles
    hiddenContainer.style.visibility = originalStyles.visibility || '';
    hiddenContainer.style.opacity = originalStyles.opacity || '';
    hiddenContainer.style.position = originalStyles.position || '';
    hiddenContainer.style.height = originalStyles.height || '';
    hiddenContainer.style.left = '';
    
    const result = results.join('');
    // HTML generation complete - logging removed for performance
    
    return result;
}

export function init() {
    // Skeleton generator initialized
    return true;
}

export function dispose() {
    // Skeleton generator disposed
}

export default { generateSkeletonHTML, init, dispose };