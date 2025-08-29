export function generateSkeletonHTML(containerElement, animated = true) {
    if (!containerElement) {
        console.warn('Skeleton: No container element provided');
        return generateFallbackSkeleton(animated);
    }

    const hiddenContainer = containerElement.querySelector('.skeleton-hidden-container');
    if (!hiddenContainer) {
        console.warn('Skeleton: No hidden container found');
        return generateFallbackSkeleton(animated);
    }
    
    const animationClass = animated ? ' skeleton-pulse' : '';
    
    function cloneAndSkeletonize(element, depth = 0) {
        if (!element || element.nodeType !== Node.ELEMENT_NODE) return null;
        
        const skipTags = ['SCRIPT', 'STYLE', 'META', 'LINK', 'TITLE', 'BR', 'HR'];
        if (skipTags.includes(element.tagName)) return null;
        
        const computedStyle = window.getComputedStyle(element);
        if (computedStyle.display === 'none') return null;
        
        const clone = element.cloneNode(false);
        
        clone.textContent = '';
        
        ['id', 'onclick', 'onmouseover', 'href', 'src', 'alt', 'title', 'data-*'].forEach(attr => {
            if (attr.includes('*')) {
                Array.from(clone.attributes).forEach(a => {
                    if (a.name.startsWith('data-')) clone.removeAttribute(a.name);
                });
            } else {
                clone.removeAttribute(attr);
            }
        });
        
        const hasDirectText = Array.from(element.childNodes).some(node => 
            node.nodeType === Node.TEXT_NODE && node.textContent.trim()
        );
        
        if (hasDirectText || (element.children.length === 0 && element.textContent.trim())) {
            clone.className = getSkeletonClass(element, computedStyle) + animationClass;
            
            const styles = getSkeletonStyles(element, computedStyle);
            if (styles) clone.setAttribute('style', styles);
            
            return clone.outerHTML;
        }
        
        const childrenHTML = Array.from(element.children)
            .map(child => cloneAndSkeletonize(child, depth + 1))
            .filter(html => html)
            .join('');
        
        if (childrenHTML) {
            clone.innerHTML = childrenHTML;
            return clone.outerHTML;
        }
        
        return null;
    }
    
    const skeletonHTML = Array.from(hiddenContainer.children)
        .map(child => cloneAndSkeletonize(child, 0))
        .filter(html => html)
        .join('');
    
    return skeletonHTML || generateFallbackSkeleton(animated);
}

function getSkeletonClass(element, computedStyle) {
    const tagName = element.tagName.toLowerCase();
    const className = element.className.toLowerCase();
    
    if (className.includes('avatar') || className.includes('r-avatar')) {
        return 'skeleton skeleton-avatar';
    }
    if (className.includes('badge') || className.includes('chip') || className.includes('r-chip')) {
        return 'skeleton skeleton-badge';
    }
    if (tagName === 'button' || className.includes('btn') || className.includes('button')) {
        return 'skeleton skeleton-button';
    }
    if (tagName === 'img') {
        return 'skeleton skeleton-image';
    }
    
    if (tagName.match(/^h[1-6]$/) || 
        parseFloat(computedStyle.fontSize) >= 18 || 
        computedStyle.fontWeight === 'bold' || 
        parseInt(computedStyle.fontWeight) >= 600) {
        return 'skeleton skeleton-title';
    }
    
    return 'skeleton skeleton-text';
}

function getSkeletonStyles(element, computedStyle) {
    const styles = [];
    const className = element.className.toLowerCase();
    
    if (className.includes('avatar')) {
        const size = computedStyle.width || computedStyle.height || '3rem';
        styles.push(`width: ${size}`);
        styles.push(`height: ${size}`);
        styles.push('border-radius: 50%');
        styles.push('flex-shrink: 0');
    }
    else if (element.textContent?.trim()) {
        const textLength = element.textContent.trim().length;
        const fontSize = parseFloat(computedStyle.fontSize) || 16;
        
        let width;
        if (textLength < 10) width = '25%';
        else if (textLength < 25) width = '40%';
        else if (textLength < 50) width = '60%';
        else if (textLength < 100) width = '80%';
        else width = '100%';
        
        styles.push(`width: ${width}`);
        styles.push(`height: ${fontSize * 1.2}px`);
    }
    else if (className.includes('badge') || className.includes('chip')) {
        styles.push('height: 1.5rem');
        styles.push('width: auto');
        styles.push('min-width: 3rem');
        styles.push('border-radius: 0.75rem');
        styles.push('display: inline-block');
    }
    
    return styles.join('; ');
}


function generateFallbackSkeleton(animated = true) {
    const animationClass = animated ? ' skeleton-pulse' : '';
    
    return `
        <div class="skeleton skeleton-title${animationClass}" style="width: 40%; height: 1.5rem; margin-bottom: 0.75rem;"></div>
        <div class="skeleton skeleton-text${animationClass}" style="width: 100%; height: 1rem; margin-bottom: 0.5rem;"></div>
        <div class="skeleton skeleton-text${animationClass}" style="width: 85%; height: 1rem; margin-bottom: 0.5rem;"></div>
        <div class="skeleton skeleton-text${animationClass}" style="width: 70%; height: 1rem; margin-bottom: 1rem;"></div>
        <div class="skeleton skeleton-badge${animationClass}" style="width: 4rem; height: 1.5rem; border-radius: 0.75rem;"></div>
    `;
}


export function init() {
    console.log('Skeleton generator loaded');
    return true;
}

export function dispose() {
    console.log('Skeleton generator disposed');
}

export default { generateSkeletonHTML, init, dispose };