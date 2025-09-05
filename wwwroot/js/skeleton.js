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
    
    const animationClass = animated ? ' skeleton-shimmer' : '';
    
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
            clone.setAttribute('aria-hidden', 'true');
            clone.setAttribute('role', 'presentation');
            
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
    const baseClass = 'skeleton';
    
    // Add shimmer effect by default
    const shimmerClass = 'skeleton-shimmer';
    
    if (className.includes('avatar') || className.includes('r-avatar')) {
        return `${baseClass} skeleton-avatar ${shimmerClass}`;
    }
    if (className.includes('badge') || className.includes('chip') || className.includes('r-chip')) {
        return `${baseClass} skeleton-badge ${shimmerClass}`;
    }
    if (tagName === 'button' || className.includes('btn') || className.includes('button') || className.includes('r-button')) {
        return `${baseClass} skeleton-button ${shimmerClass}`;
    }
    if (tagName === 'img' || className.includes('image')) {
        return `${baseClass} skeleton-image ${shimmerClass}`;
    }
    if (tagName === 'input' || tagName === 'textarea' || tagName === 'select') {
        return `${baseClass} skeleton-input ${shimmerClass}`;
    }
    if (className.includes('card') || className.includes('r-card')) {
        return `${baseClass} skeleton-card ${shimmerClass}`;
    }
    
    // Enhanced heading detection
    const isHeading = tagName.match(/^h[1-6]$/) || 
        className.includes('text-h') || 
        className.includes('title') ||
        className.includes('heading') ||
        parseFloat(computedStyle.fontSize) >= 18 || 
        computedStyle.fontWeight === 'bold' || 
        parseInt(computedStyle.fontWeight) >= 600;
    
    if (isHeading) {
        return `${baseClass} skeleton-title ${shimmerClass}`;
    }
    
    return `${baseClass} skeleton-text ${shimmerClass}`;
}

function getSkeletonStyles(element, computedStyle) {
    const styles = [];
    const className = element.className.toLowerCase();
    const tagName = element.tagName.toLowerCase();
    
    // Get actual computed dimensions
    const rect = element.getBoundingClientRect();
    const actualWidth = rect.width;
    const actualHeight = rect.height;
    
    // Add subtle animation delays for staggered effect
    const animationDelay = Math.random() * 0.3;
    styles.push(`animation-delay: ${animationDelay}s`);
    
    if (className.includes('avatar') || className.includes('r-avatar')) {
        const size = actualWidth > 0 ? `${actualWidth}px` : computedStyle.width || '3rem';
        styles.push(`width: ${size}`);
        styles.push(`height: ${size}`);
        styles.push('border-radius: 50%');
        styles.push('flex-shrink: 0');
        styles.push('position: relative');
        styles.push('overflow: hidden');
    }
    else if (className.includes('badge') || className.includes('chip') || className.includes('r-chip')) {
        const heightVal = actualHeight > 0 ? actualHeight : 22;
        const widthVal = actualWidth > 0 ? actualWidth : 56;
        styles.push(`height: ${heightVal}px`);
        styles.push(`width: ${widthVal}px`);
        styles.push('border-radius: 1rem');
        styles.push('display: inline-block');
        styles.push('opacity: 0.8');
    }
    else if (tagName === 'button' || className.includes('btn') || className.includes('button') || className.includes('r-button')) {
        styles.push(`width: ${actualWidth > 0 ? actualWidth + 'px' : '5.5rem'}`);
        styles.push(`height: ${actualHeight > 0 ? actualHeight + 'px' : '2.25rem'}`);
        styles.push('border-radius: 0.375rem');
        styles.push('position: relative');
    }
    else if (tagName === 'input' || tagName === 'textarea' || tagName === 'select') {
        styles.push(`width: ${actualWidth > 0 ? actualWidth + 'px' : '100%'}`);
        styles.push(`height: ${actualHeight > 0 ? actualHeight + 'px' : '2.375rem'}`);
        styles.push('border-radius: 0.375rem');
        styles.push('border: 1px solid rgba(0, 0, 0, 0.05)');
    }
    else if (element.textContent?.trim()) {
        const textLength = element.textContent.trim().length;
        const fontSize = parseFloat(computedStyle.fontSize) || 14;
        const isHeading = tagName.match(/^h[1-6]$/) || parseFloat(computedStyle.fontSize) >= 18;
        
        let width;
        if (actualWidth > 0) {
            // Create more natural width variations
            const baseVariation = isHeading ? 0.4 : 0.8;
            const variation = Math.random() * 0.2 + baseVariation;
            width = `${Math.min(actualWidth * variation, actualWidth)}px`;
        } else {
            // Smart width based on content type
            if (isHeading) {
                width = textLength < 20 ? '30%' : '45%';
            } else {
                const widthOptions = ['92%', '85%', '76%', '68%', '100%'];
                width = widthOptions[Math.floor(Math.random() * widthOptions.length)];
            }
        }
        
        const heightVal = actualHeight > 0 ? actualHeight : (isHeading ? fontSize * 1.4 : fontSize * 1.2);
        styles.push(`width: ${width}`);
        styles.push(`height: ${heightVal}px`);
        styles.push(`opacity: ${isHeading ? '0.9' : '0.85'}`);
        
        // Preserve spacing
        const marginBottom = computedStyle.marginBottom;
        if (marginBottom && marginBottom !== '0px') {
            styles.push(`margin-bottom: ${marginBottom}`);
        }
    }
    else {
        // Container styling
        if (actualWidth > 0) styles.push(`width: ${actualWidth}px`);
        if (actualHeight > 0) styles.push(`min-height: ${actualHeight}px`);
        styles.push('position: relative');
    }
    
    return styles.join('; ');
}


function generateFallbackSkeleton(animated = true) {
    const animationClass = animated ? ' skeleton-shimmer' : '';
    const randomWidths = [95, 88, 73, 62];
    const textLines = randomWidths.map((width, i) => {
        const delay = i * 0.1;
        return `<div class="skeleton skeleton-text${animationClass}" style="width: ${width}%; height: 0.875rem; margin-bottom: 0.5rem; animation-delay: ${delay}s; opacity: ${1 - i * 0.15};" aria-hidden="true" role="presentation"></div>`;
    }).join('');
    
    return `
        <div class="skeleton-fallback-container" style="padding: 1rem;">
            <div class="skeleton skeleton-title${animationClass}" style="width: 35%; height: 1.75rem; margin-bottom: 1rem; opacity: 0.9;" aria-hidden="true" role="presentation"></div>
            ${textLines}
            <div class="skeleton-badge-group" style="display: flex; gap: 0.5rem; margin-top: 1rem;">
                <div class="skeleton skeleton-badge${animationClass}" style="width: 3.5rem; height: 1.375rem; border-radius: 1rem; opacity: 0.7;" aria-hidden="true" role="presentation"></div>
                <div class="skeleton skeleton-badge${animationClass}" style="width: 4rem; height: 1.375rem; border-radius: 1rem; animation-delay: 0.2s; opacity: 0.6;" aria-hidden="true" role="presentation"></div>
            </div>
        </div>
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