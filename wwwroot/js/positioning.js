
export class PositioningEngine {
    static POSITIONS = {
        TOP_START: 'top_start',
        TOP_CENTER: 'top_center',
        TOP_END: 'top_end',
        BOTTOM_START: 'bottom_start',
        BOTTOM_CENTER: 'bottom_center',
        BOTTOM_END: 'bottom_end',
        LEFT_START: 'left_start',
        LEFT_CENTER: 'left_center',
        LEFT_END: 'left_end',
        RIGHT_START: 'right_start',
        RIGHT_CENTER: 'right_center',
        RIGHT_END: 'right_end'
    };

    calculatePosition(triggerRect, targetDimensions, options = {}) {
        if (!triggerRect) {
            throw new Error('PositioningEngine: triggerRect is required');
        }
        if (!targetDimensions || !targetDimensions.width || !targetDimensions.height) {
            throw new Error('PositioningEngine: targetDimensions with width and height required');
        }

        const {
            position = PositioningEngine.POSITIONS.BOTTOM_START,
            offset = 8,
            flip = true,
            constrain = true,
            container = null, // Container bounds for constrained positioning
            viewport = {
                width: window.innerWidth,
                height: window.innerHeight,
                scrollX: window.scrollX,
                scrollY: window.scrollY
            }
        } = options;

        const [placement, alignment] = position.split('_');
        
        let result = this._calculateBasePosition(
            triggerRect,
            targetDimensions,
            placement,
            alignment,
            offset,
            viewport
        );

        if (flip) {
            const collision = this._detectCollision(result, targetDimensions, viewport);
            if (collision) {
                const flippedPlacement = this._getFlippedPlacement(placement, collision);
                const flippedResult = this._calculateBasePosition(
                    triggerRect,
                    targetDimensions,
                    flippedPlacement,
                    alignment,
                    offset,
                    viewport
                );
                
                const flippedCollision = this._detectCollision(flippedResult, targetDimensions, viewport);
                if (!flippedCollision || this._collisionScore(flippedCollision) < this._collisionScore(collision)) {
                    result = { ...flippedResult, flipped: true };
                }
            }
        }

        if (constrain) {
            result = this._constrainToViewport(result, targetDimensions, viewport);
        }

        if (container) {
            result = this._constrainToContainer(result, targetDimensions, container, offset);
        }

        return {
            x: Math.round(result.x),
            y: Math.round(result.y),
            placement: `${result.placement}_${alignment}`,
            flipped: result.flipped || false
        };
    }

    detectOptimalPosition(triggerRect, targetDimensions, viewport = null) {
        if (!triggerRect) {
            throw new Error('PositioningEngine: triggerRect is required for optimal detection');
        }
        if (!targetDimensions || !targetDimensions.width || !targetDimensions.height) {
            throw new Error('PositioningEngine: targetDimensions required for optimal detection');
        }

        viewport = viewport || {
            width: window.innerWidth,
            height: window.innerHeight,
            scrollX: window.scrollX,
            scrollY: window.scrollY
        };

        const space = {
            top: triggerRect.top,
            bottom: viewport.height - triggerRect.bottom,
            left: triggerRect.left,
            right: viewport.width - triggerRect.right
        };
        

        const safetyMargin = 20;
        let placement;
        if (space.bottom >= targetDimensions.height + safetyMargin) {
            placement = 'bottom';
        } else if (space.top >= targetDimensions.height + safetyMargin) {
            placement = 'top';
        } else if (space.right >= targetDimensions.width) {
            placement = 'right';
        } else if (space.left >= targetDimensions.width) {
            placement = 'left';
        } else {
            placement = Object.keys(space).reduce((a, b) => space[a] > space[b] ? a : b);
        }

        let alignment = 'start'; // Default alignment
        if (placement === 'top' || placement === 'bottom') {
            const centerX = triggerRect.left + triggerRect.width / 2;
            if (centerX - targetDimensions.width / 2 >= 0 && 
                centerX + targetDimensions.width / 2 <= viewport.width) {
                alignment = 'center';
            } else if (triggerRect.right < viewport.width / 2) {
                alignment = 'start';
            } else {
                alignment = 'end';
            }
        } else {
            const centerY = triggerRect.top + triggerRect.height / 2;
            if (centerY - targetDimensions.height / 2 >= 0 && 
                centerY + targetDimensions.height / 2 <= viewport.height) {
                alignment = 'center';
            } else if (triggerRect.bottom < viewport.height / 2) {
                alignment = 'start';
            } else {
                alignment = 'end';
            }
        }

        return `${placement}_${alignment}`;
    }

    _calculateBasePosition(triggerRect, targetDimensions, placement, alignment, offset, viewport) {
        let x, y;

        switch (placement) {
            case 'top':
                y = triggerRect.top - targetDimensions.height - offset;
                break;
            case 'bottom':
                y = triggerRect.bottom + offset;
                break;
            case 'left':
                x = triggerRect.left - targetDimensions.width - offset;
                break;
            case 'right':
                x = triggerRect.right + offset;
                break;
            default:
                throw new Error(`PositioningEngine: Invalid placement "${placement}"`);
        }

        if (placement === 'top' || placement === 'bottom') {
            switch (alignment) {
                case 'start':
                    x = triggerRect.left;
                    break;
                case 'center':
                    x = triggerRect.left + (triggerRect.width - targetDimensions.width) / 2;
                    break;
                case 'end':
                    x = triggerRect.right - targetDimensions.width;
                    break;
                default:
                    throw new Error(`PositioningEngine: Invalid alignment "${alignment}"`);
            }
        } else {
            switch (alignment) {
                case 'start':
                    y = triggerRect.top;
                    break;
                case 'center':
                    y = triggerRect.top + (triggerRect.height - targetDimensions.height) / 2;
                    break;
                case 'end':
                    y = triggerRect.bottom - targetDimensions.height;
                    break;
                default:
                    throw new Error(`PositioningEngine: Invalid alignment "${alignment}"`);
            }
        }

        return { x, y, placement };
    }

    _detectCollision(position, targetDimensions, viewport) {
        const collision = {
            top: position.y < 0,
            bottom: position.y + targetDimensions.height > viewport.height,
            left: position.x < 0,
            right: position.x + targetDimensions.width > viewport.width
        };

        const hasCollision = Object.values(collision).some(v => v);
        return hasCollision ? collision : null;
    }

    _getFlippedPlacement(placement, collision) {
        switch (placement) {
            case 'top':
                return collision.top ? 'bottom' : placement;
            case 'bottom':
                return collision.bottom ? 'top' : placement;
            case 'left':
                return collision.left ? 'right' : placement;
            case 'right':
                return collision.right ? 'left' : placement;
            default:
                return placement;
        }
    }

    _collisionScore(collision) {
        if (!collision) return 0;
        return Object.values(collision).filter(v => v).length;
    }

    _constrainToViewport(position, targetDimensions, viewport) {
        const constrained = { ...position };

        constrained.x = Math.max(
            0,
            Math.min(
                constrained.x,
                viewport.width - targetDimensions.width
            )
        );

        constrained.y = Math.max(
            0,
            Math.min(
                constrained.y,
                viewport.height - targetDimensions.height
            )
        );

        return constrained;
    }

    _constrainToContainer(position, targetDimensions, container, offset = 8) {
        const constrained = { ...position };

        if (constrained.x < container.left + offset) {
            constrained.x = container.left + offset;
        } else if (constrained.x + targetDimensions.width > container.right - offset) {
            constrained.x = container.right - targetDimensions.width - offset;
        }

  
        if (constrained.y < container.top + offset) {
            constrained.y = container.top + offset;
        } else if (constrained.y + targetDimensions.height > container.bottom - offset) {
            constrained.y = container.bottom - targetDimensions.height - offset;
        }

        return constrained;
    }

    calculateArrowPosition(placement, triggerRect, targetPosition) {
        if (!placement || !triggerRect || !targetPosition) {
            throw new Error('PositioningEngine: All parameters required for arrow calculation');
        }

        const [side, alignment] = placement.split('_');
        const arrowSize = 8; // Standard arrow size
        let x, y, arrowSide;

        switch (side) {
            case 'top':
                arrowSide = 'bottom';
                y = '100%';
                x = triggerRect.left + triggerRect.width / 2 - targetPosition.x;
                break;
            case 'bottom':
                arrowSide = 'top';
                y = `-${arrowSize}px`;
                x = triggerRect.left + triggerRect.width / 2 - targetPosition.x;
                break;
            case 'left':
                arrowSide = 'right';
                x = '100%';
                y = triggerRect.top + triggerRect.height / 2 - targetPosition.y;
                break;
            case 'right':
                arrowSide = 'left';
                x = `-${arrowSize}px`;
                y = triggerRect.top + triggerRect.height / 2 - targetPosition.y;
                break;
            default:
                throw new Error(`PositioningEngine: Invalid side "${side}" for arrow`);
        }

        return {
            x: typeof x === 'number' ? `${x}px` : x,
            y: typeof y === 'number' ? `${y}px` : y,
            side: arrowSide
        };
    }

    updatePosition(currentPosition, triggerRect, targetDimensions, options = {}) {
        if (!currentPosition || !triggerRect || !targetDimensions) {
            throw new Error('PositioningEngine: All parameters required for position update');
        }

        const newPosition = this.calculatePosition(triggerRect, targetDimensions, {
            ...options,
            position: currentPosition.placement
        });

        if (newPosition.x === currentPosition.x && 
            newPosition.y === currentPosition.y &&
            newPosition.placement === currentPosition.placement) {
            return null; // No change needed
        }

        return newPosition;
    }
}

const positioningEngine = new PositioningEngine();

export default {
    PositioningEngine,
    getInstance: () => positioningEngine,
    getPositioningEngine: () => ({ PositioningEngine }),
    ...positioningEngine
};