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
    
    function cloneStructureWithSkeletons(element, depth = 0) {
        if (!element || element.nodeType !== Node.ELEMENT_NODE) return null;
        if (depth > 15) return null; // Prevent infinite recursion
        
        const skipTags = ['SCRIPT', 'STYLE', 'META', 'LINK', 'TITLE', 'BR', 'HR', 'BLAZOR-ERROR-UI'];
        if (skipTags.includes(element.tagName)) return null;
        
        const computedStyle = window.getComputedStyle(element);
        if (computedStyle.display === 'none' || computedStyle.visibility === 'hidden') return null;
        
        const className = (element.className || '').toLowerCase();
        const tagName = element.tagName.toLowerCase();
        
        // Determine if this element is a content element or a container
        const elementType = detectElementType(element, computedStyle);
        
        // If it's a content element (leaf node), create a skeleton
        if (elementType.isContent) {
            return createSkeletonElement(elementType.type, element, computedStyle, animationClass);
        }
        
        // It's a container - preserve its structure
        const clone = document.createElement(element.tagName.toLowerCase());
        
        // Copy essential attributes and classes that affect layout
        copyEssentialAttributes(element, clone);
        
        // Process children
        let hasContent = false;
        Array.from(element.childNodes).forEach(child => {
            if (child.nodeType === Node.ELEMENT_NODE) {
                const childSkeleton = cloneStructureWithSkeletons(child, depth + 1);
                if (childSkeleton) {
                    if (typeof childSkeleton === 'string') {
                        const temp = document.createElement('div');
                        temp.innerHTML = childSkeleton;
                        while (temp.firstChild) {
                            clone.appendChild(temp.firstChild);
                        }
                    } else {
                        clone.appendChild(childSkeleton);
                    }
                    hasContent = true;
                }
            } else if (child.nodeType === Node.TEXT_NODE) {
                const text = child.textContent.trim();
                if (text) {
                    // Replace text nodes with skeleton text
                    const textSkeleton = createTextSkeleton(text, element, computedStyle, animationClass);
                    const temp = document.createElement('div');
                    temp.innerHTML = textSkeleton;
                    clone.appendChild(temp.firstChild);
                    hasContent = true;
                }
            }
        });
        
        // If container has no processable children, return null
        if (!hasContent && !element.childNodes.length) {
            return null;
        }
        
        return clone;
    }
    
    function detectElementType(element, computedStyle) {
        const className = (element.className || '').toLowerCase();
        const tagName = element.tagName.toLowerCase();
        const hasChildren = element.children.length > 0;
        const hasText = Array.from(element.childNodes).some(
            node => node.nodeType === Node.TEXT_NODE && node.textContent.trim()
        );
        
        // Avatar detection
        if (className.includes('avatar') || className.includes('r-avatar')) {
            return { type: 'avatar', isContent: true };
        }
        
        // Badge/Chip detection - these are content elements
        if (className.includes('chip') || className.includes('badge') || className.includes('r-chip')) {
            // Check if it's the actual chip element or a container
            if (hasChildren && className.includes('chip-text')) {
                return { type: 'container', isContent: false };
            }
            return { type: 'badge', isContent: true };
        }
        
        // Button detection
        if (tagName === 'button' || className.includes('button') || className.includes('btn')) {
            return { type: 'button', isContent: true };
        }
        
        // Icon detection
        if (tagName === 'i' || className.includes('icon') || className.includes('material-icons')) {
            return { type: 'icon', isContent: true };
        }
        
        // Circular element detection (like icon containers)
        const borderRadius = computedStyle.borderRadius;
        if (borderRadius && (borderRadius.includes('50%') || borderRadius === '9999px')) {
            const rect = element.getBoundingClientRect();
            if (rect.width > 20 && rect.width < 100 && Math.abs(rect.width - rect.height) < 5) {
                // Check if it has an icon child
                const hasIcon = Array.from(element.children).some(child => 
                    child.tagName.toLowerCase() === 'i' || child.className.includes('icon')
                );
                if (hasIcon) {
                    return { type: 'container', isContent: false }; // Container for icon
                }
                return { type: 'avatar', isContent: true };
            }
        }
        
        // Text elements - only if they have no children
        if (!hasChildren && hasText) {
            // Check text type
            if (tagName.match(/^h[1-6]$/) || className.includes('text-h') || className.includes('title')) {
                return { type: 'title', isContent: true };
            }
            if (className.includes('caption') || className.includes('muted') || className.includes('text-sm')) {
                return { type: 'caption', isContent: true };
            }
            const fontWeight = computedStyle.fontWeight;
            if (fontWeight === 'bold' || parseInt(fontWeight) >= 600) {
                return { type: 'title', isContent: true };
            }
            return { type: 'text', isContent: true };
        }
        
        // Cards and other containers - preserve structure
        if (className.includes('card') || className.includes('glass') || className.includes('rounded-lg')) {
            return { type: 'container', isContent: false };
        }
        
        // Default: if has children, it's a container
        if (hasChildren) {
            return { type: 'container', isContent: false };
        }
        
        // Empty elements
        return { type: 'empty', isContent: true };
    }
    
    function createSkeletonElement(type, originalElement, computedStyle, animationClass) {
        const rect = originalElement.getBoundingClientRect();
        const styles = [];
        
        switch(type) {
            case 'avatar':
                const size = rect.width > 0 ? `${rect.width}px` : '48px';
                return `<div class="skeleton skeleton-avatar${animationClass}" style="width: ${size}; height: ${size}; border-radius: 50%; flex-shrink: 0;" aria-hidden="true"></div>`;
                
            case 'badge':
                const badgeWidth = rect.width > 0 ? `${rect.width}px` : '60px';
                const badgeHeight = rect.height > 0 ? `${rect.height}px` : '24px';
                return `<div class="skeleton skeleton-badge${animationClass}" style="width: ${badgeWidth}; height: ${badgeHeight}; border-radius: 12px; display: inline-block;" aria-hidden="true"></div>`;
                
            case 'button':
                const btnWidth = rect.width > 0 ? `${rect.width}px` : '80px';
                const btnHeight = rect.height > 0 ? `${rect.height}px` : '36px';
                return `<div class="skeleton skeleton-button${animationClass}" style="width: ${btnWidth}; height: ${btnHeight}; border-radius: 6px;" aria-hidden="true"></div>`;
                
            case 'icon':
                const iconSize = rect.width > 0 ? `${rect.width}px` : '24px';
                return `<div class="skeleton skeleton-icon${animationClass}" style="width: ${iconSize}; height: ${iconSize}; border-radius: 4px;" aria-hidden="true"></div>`;
                
            case 'title':
                const titleWidth = rect.width > 0 ? `${Math.min(rect.width * 0.6, 250)}px` : '150px';
                const titleHeight = rect.height > 0 ? `${rect.height}px` : '20px';
                return `<div class="skeleton skeleton-title${animationClass}" style="width: ${titleWidth}; height: ${titleHeight}; border-radius: 4px;" aria-hidden="true"></div>`;
                
            case 'caption':
                const captionWidth = rect.width > 0 ? `${Math.min(rect.width * 0.7, 100)}px` : '80px';
                return `<div class="skeleton skeleton-caption${animationClass}" style="width: ${captionWidth}; height: 12px; border-radius: 3px;" aria-hidden="true"></div>`;
                
            case 'text':
                const textWidth = rect.width > 0 ? `${rect.width * (0.65 + Math.random() * 0.3)}px` : '100%';
                const textHeight = rect.height > 0 ? `${Math.max(rect.height, 14)}px` : '14px';
                return `<div class="skeleton skeleton-text${animationClass}" style="width: ${textWidth}; height: ${textHeight}; border-radius: 3px;" aria-hidden="true"></div>`;
                
            case 'empty':
                return null;
                
            default:
                return null;
        }
    }
    
    function createTextSkeleton(text, parentElement, computedStyle, animationClass) {
        const fontSize = parseFloat(computedStyle.fontSize);
        const fontWeight = computedStyle.fontWeight;
        const className = (parentElement.className || '').toLowerCase();
        
        // Determine text type
        let type = 'text';
        let width = '80%';
        let height = '14px';
        
        if (fontWeight === 'bold' || parseInt(fontWeight) >= 600 || fontSize >= 18) {
            type = 'title';
            width = text.length > 20 ? '60%' : '40%';
            height = `${Math.max(fontSize, 20)}px`;
        } else if (className.includes('caption') || className.includes('muted') || fontSize <= 12) {
            type = 'caption';
            width = text.length > 15 ? '70%' : '50%';
            height = '12px';
        } else {
            // Regular text - vary width based on length
            const ratio = Math.min(text.length / 50, 1);
            width = `${60 + ratio * 35}%`;
            height = `${Math.max(fontSize, 14)}px`;
        }
        
        return `<div class="skeleton skeleton-${type}${animationClass}" style="width: ${width}; height: ${height}; border-radius: 3px; display: inline-block;" aria-hidden="true"></div>`;
    }
    
    function copyEssentialAttributes(source, target) {
        // Copy classes that affect layout but not appearance
        const classes = (source.className || '').split(' ').filter(cls => {
            const lower = cls.toLowerCase();
            return (
                lower.includes('flex') ||
                lower.includes('grid') ||
                lower.includes('gap') ||
                lower.includes('items-') ||
                lower.includes('justify-') ||
                lower.includes('align-') ||
                lower.includes('space-') ||
                lower.includes('col-') ||
                lower.includes('row-') ||
                lower.includes('container') ||
                lower.includes('wrapper') ||
                lower.includes('rounded') ||
                lower.includes('glass') ||
                lower.includes('card') ||
                lower.match(/^(mt|mb|ml|mr|mx|my|pt|pb|pl|pr|px|py|pa)-/) ||
                (lower.match(/^(w|h)-\d+$/) && !lower.includes('text'))
            );
        });
        
        if (classes.length > 0) {
            target.className = classes.join(' ');
        }
        
        // Copy inline styles that affect layout
        const computedStyle = window.getComputedStyle(source);
        const layoutStyles = [];
        
        // Important layout properties to preserve
        const layoutProps = ['display', 'flex-direction', 'justify-content', 'align-items', 'gap', 'grid-template-columns', 'grid-template-rows'];
        layoutProps.forEach(prop => {
            const value = computedStyle.getPropertyValue(prop);
            if (value && value !== 'normal' && value !== 'auto') {
                if (prop === 'display' && (value === 'flex' || value === 'grid' || value === 'inline-flex' || value === 'inline-grid')) {
                    layoutStyles.push(`${prop}: ${value}`);
                } else if (prop !== 'display') {
                    layoutStyles.push(`${prop}: ${value}`);
                }
            }
        });
        
        // Preserve padding for containers (but not background/color)
        const padding = computedStyle.padding;
        if (padding && padding !== '0px') {
            layoutStyles.push(`padding: ${padding}`);
        }
        
        if (layoutStyles.length > 0) {
            target.setAttribute('style', layoutStyles.join('; '));
        }
        
        // Preserve data attributes that might be important
        Array.from(source.attributes).forEach(attr => {
            if (attr.name.startsWith('data-') || attr.name === 'role') {
                target.setAttribute(attr.name, attr.value);
            }
        });
    }
    
    // Process the hidden container
    const result = cloneStructureWithSkeletons(hiddenContainer.firstElementChild, 0);
    
    if (result) {
        // Convert DOM element to HTML string
        if (typeof result === 'string') {
            return result;
        }
        const temp = document.createElement('div');
        temp.appendChild(result);
        return temp.innerHTML;
    }
    
    return generateFallbackSkeleton(animated);
}

function generateFallbackSkeleton(animated = true) {
    const animationClass = animated ? ' skeleton-shimmer' : '';
    
    return `
        <div class="flex gap-4 pa-4 glass-light rounded-lg">
            <div class="skeleton skeleton-avatar${animationClass}" style="width: 48px; height: 48px; border-radius: 50%;" aria-hidden="true"></div>
            <div class="flex-1">
                <div class="skeleton skeleton-title${animationClass}" style="width: 150px; height: 20px; margin-bottom: 8px;" aria-hidden="true"></div>
                <div class="skeleton skeleton-text${animationClass}" style="width: 200px; height: 14px; margin-bottom: 12px;" aria-hidden="true"></div>
                <div class="flex gap-2">
                    <div class="skeleton skeleton-badge${animationClass}" style="width: 60px; height: 24px; border-radius: 12px;" aria-hidden="true"></div>
                    <div class="skeleton skeleton-badge${animationClass}" style="width: 45px; height: 24px; border-radius: 12px;" aria-hidden="true"></div>
                </div>
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