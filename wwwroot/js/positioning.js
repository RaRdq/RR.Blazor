// positioning.js - Position Calculation Only

export class PositioningEngine {
    static Positions = {
        TOP_START: 'top-start',
        TOP: 'top',
        TOP_END: 'top-end',
        RIGHT_START: 'right-start',
        RIGHT: 'right',
        RIGHT_END: 'right-end',
        BOTTOM_START: 'bottom-start',
        BOTTOM: 'bottom',
        BOTTOM_END: 'bottom-end',
        LEFT_START: 'left-start',
        LEFT: 'left',
        LEFT_END: 'left-end'
    };
    
    static #flipMap = {
        'top': 'bottom',
        'bottom': 'top',
        'left': 'right',
        'right': 'left',
        'start': 'end',
        'end': 'start'
    };
    
    calculate(trigger, portal, position = PositioningEngine.Positions.BOTTOM, config = {}) {
        const triggerRect = trigger.getBoundingClientRect();
        const portalRect = portal.getBoundingClientRect();
        
        if (triggerRect.width === 0 || triggerRect.height === 0) {
            throw new Error('[PositioningEngine] Trigger element has no dimensions');
        }
        
        const viewport = this.#getViewport();
        const offset = config.offset || 0;
        const padding = config.padding || 8;
        
        let coords = this.#calculateInitialPosition(triggerRect, portalRect, position, offset);
        
        if (config.flip !== false) {
            coords = this.#applyFlipping(coords, portalRect, viewport, position, triggerRect, offset, padding);
        }
        
        if (config.constrain !== false) {
            coords = this.#constrainToViewport(coords, portalRect, viewport, padding);
        }
        
        return coords;
    }
    
    autoPosition(trigger, portal, config = {}) {
        const preferredPositions = config.preferredPositions || [
            PositioningEngine.Positions.BOTTOM,
            PositioningEngine.Positions.TOP,
            PositioningEngine.Positions.RIGHT,
            PositioningEngine.Positions.LEFT
        ];
        
        const triggerRect = trigger.getBoundingClientRect();
        const portalRect = portal.getBoundingClientRect();
        const viewport = this.#getViewport();
        
        let bestPosition = null;
        let bestScore = -1;
        
        for (const position of preferredPositions) {
            const coords = this.#calculateInitialPosition(triggerRect, portalRect, position, config.offset || 0);
            const score = this.#scorePosition(coords, portalRect, viewport);
            
            if (score > bestScore) {
                bestScore = score;
                bestPosition = { position, coords };
            }
        }
        
        if (!bestPosition) {
            throw new Error('[PositioningEngine] Unable to find suitable position');
        }
        
        return bestPosition.coords;
    }
    
    updateOnScroll(trigger, portal, position, config = {}) {
        const coords = this.calculate(trigger, portal, position, config);
        
        portal.style.position = 'fixed';
        portal.style.left = `${coords.x}px`;
        portal.style.top = `${coords.y}px`;
        
        return coords;
    }
    
    getViewportPosition(element) {
        const rect = element.getBoundingClientRect();
        const viewport = this.#getViewport();
        
        return {
            visible: rect.top >= 0 && 
                     rect.left >= 0 && 
                     rect.bottom <= viewport.height && 
                     rect.right <= viewport.width,
            partiallyVisible: rect.bottom > 0 && 
                              rect.right > 0 && 
                              rect.top < viewport.height && 
                              rect.left < viewport.width,
            position: {
                top: rect.top < viewport.height / 2 ? 'top' : 'bottom',
                left: rect.left < viewport.width / 2 ? 'left' : 'right'
            }
        };
    }
    
    #calculateInitialPosition(triggerRect, portalRect, position, offset) {
        const [primary, alignment] = position.split('-');
        let x = 0;
        let y = 0;
        
        switch (primary) {
            case 'top':
                x = this.#getAlignmentX(triggerRect, portalRect, alignment);
                y = triggerRect.top - portalRect.height - offset;
                break;
                
            case 'bottom':
                x = this.#getAlignmentX(triggerRect, portalRect, alignment);
                y = triggerRect.bottom + offset;
                break;
                
            case 'left':
                x = triggerRect.left - portalRect.width - offset;
                y = this.#getAlignmentY(triggerRect, portalRect, alignment);
                break;
                
            case 'right':
                x = triggerRect.right + offset;
                y = this.#getAlignmentY(triggerRect, portalRect, alignment);
                break;
                
            default:
                throw new Error(`[PositioningEngine] Invalid position: ${position}`);
        }
        
        return { x, y, position };
    }
    
    #getAlignmentX(triggerRect, portalRect, alignment) {
        switch (alignment) {
            case 'start':
                return triggerRect.left;
            case 'end':
                return triggerRect.right - portalRect.width;
            default:
                return triggerRect.left + (triggerRect.width - portalRect.width) / 2;
        }
    }
    
    #getAlignmentY(triggerRect, portalRect, alignment) {
        switch (alignment) {
            case 'start':
                return triggerRect.top;
            case 'end':
                return triggerRect.bottom - portalRect.height;
            default:
                return triggerRect.top + (triggerRect.height - portalRect.height) / 2;
        }
    }
    
    #applyFlipping(coords, portalRect, viewport, position, triggerRect, offset, padding) {
        const overflow = {
            top: coords.y < padding,
            bottom: coords.y + portalRect.height > viewport.height - padding,
            left: coords.x < padding,
            right: coords.x + portalRect.width > viewport.width - padding
        };
        
        if (overflow.top || overflow.bottom || overflow.left || overflow.right) {
            const flippedPosition = this.#flipPosition(position);
            const flippedCoords = this.#calculateInitialPosition(
                triggerRect, 
                portalRect, 
                flippedPosition, 
                offset
            );
            
            const flippedOverflow = {
                top: flippedCoords.y < padding,
                bottom: flippedCoords.y + portalRect.height > viewport.height - padding,
                left: flippedCoords.x < padding,
                right: flippedCoords.x + portalRect.width > viewport.width - padding
            };
            
            const originalOverflowCount = Object.values(overflow).filter(v => v).length;
            const flippedOverflowCount = Object.values(flippedOverflow).filter(v => v).length;
            
            if (flippedOverflowCount < originalOverflowCount) {
                return flippedCoords;
            }
        }
        
        return coords;
    }
    
    #flipPosition(position) {
        const parts = position.split('-');
        const flipped = parts.map(part => PositioningEngine.#flipMap[part] || part);
        return flipped.join('-');
    }
    
    #constrainToViewport(coords, portalRect, viewport, padding) {
        const constrained = { ...coords };
        
        constrained.x = Math.max(padding, Math.min(coords.x, viewport.width - portalRect.width - padding));
        constrained.y = Math.max(padding, Math.min(coords.y, viewport.height - portalRect.height - padding));
        
        return constrained;
    }
    
    #scorePosition(coords, portalRect, viewport) {
        const visibleWidth = Math.min(portalRect.width, viewport.width - coords.x) - Math.max(0, -coords.x);
        const visibleHeight = Math.min(portalRect.height, viewport.height - coords.y) - Math.max(0, -coords.y);
        
        const visibleArea = visibleWidth * visibleHeight;
        const totalArea = portalRect.width * portalRect.height;
        
        return totalArea > 0 ? visibleArea / totalArea : 0;
    }
    
    #getViewport() {
        return {
            width: window.innerWidth || document.documentElement.clientWidth,
            height: window.innerHeight || document.documentElement.clientHeight,
            scrollX: window.pageXOffset || document.documentElement.scrollLeft,
            scrollY: window.pageYOffset || document.documentElement.scrollTop
        };
    }
}

export const positioningEngine = new PositioningEngine();
export default PositioningEngine;

// Required methods for rr-blazor.js proxy system
export function initialize() {
    return true;
}

export function cleanup() {
    return true;
}