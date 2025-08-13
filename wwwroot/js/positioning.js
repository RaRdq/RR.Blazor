/**
 * PositioningEngine - Pure position calculation engine
 * SRP: Calculate optimal positions ONLY - no DOM manipulation
 * Fail-fast: Throws on invalid inputs, no protective coding
 * 
 * @module positioning
 */

export class PositioningEngine {
    /**
     * Position variants (12 total)
     * Format: {placement}_{alignment}
     */
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

    /**
     * Calculate optimal position for target element relative to trigger
     * 
     * @param {DOMRect} triggerRect - Bounding rect of trigger element
     * @param {Object} targetDimensions - Target element dimensions { width, height }
     * @param {Object} options - Positioning options
     * @param {string} options.position - Desired position from POSITIONS
     * @param {number} options.offset - Distance from trigger (default: 8)
     * @param {boolean} options.flip - Auto-flip on viewport collision (default: true)
     * @param {boolean} options.constrain - Constrain to viewport (default: true)
     * @param {Object} options.viewport - Viewport dimensions (default: window)
     * @returns {Object} Calculated position { x, y, placement, flipped }
     */
    calculatePosition(triggerRect, targetDimensions, options = {}) {
        // Fail fast - no protective coding
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

        // Parse position into placement and alignment
        const [placement, alignment] = position.split('_');
        
        // Calculate base position
        let result = this._calculateBasePosition(
            triggerRect,
            targetDimensions,
            placement,
            alignment,
            offset,
            viewport
        );

        // Check if position needs flipping
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
                
                // Only use flipped position if it's better
                const flippedCollision = this._detectCollision(flippedResult, targetDimensions, viewport);
                if (!flippedCollision || this._collisionScore(flippedCollision) < this._collisionScore(collision)) {
                    result = { ...flippedResult, flipped: true };
                }
            }
        }

        // Constrain to viewport if needed
        if (constrain) {
            result = this._constrainToViewport(result, targetDimensions, viewport);
        }

        // Ultra-generic container constraint handling (all directions)
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

    /**
     * Detect optimal position based on available space
     * 
     * @param {DOMRect} triggerRect - Trigger element bounding rect
     * @param {Object} targetDimensions - Target dimensions
     * @param {Object} viewport - Viewport dimensions
     * @returns {string} Optimal position from POSITIONS
     */
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

        // Calculate available space in each direction
        // For fixed positioning, use viewport-relative coordinates directly
        const space = {
            top: triggerRect.top,
            bottom: viewport.height - triggerRect.bottom,
            left: triggerRect.left,
            right: viewport.width - triggerRect.right
        };
        
        console.log('🎯 DEBUG space calculation:', {
            triggerTop: triggerRect.top,
            triggerBottom: triggerRect.bottom,
            viewportHeight: viewport.height,
            spaceTop: space.top,
            spaceBottom: space.bottom,
            targetHeight: targetDimensions.height
        });

        // Determine best placement based on available space (with safety margin)
        const safetyMargin = 20; // Extra margin to prevent cut-off
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
            // Use placement with most space
            placement = Object.keys(space).reduce((a, b) => space[a] > space[b] ? a : b);
        }

        // Determine best alignment
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

    /**
     * Calculate base position without collision detection
     * @private
     */
    _calculateBasePosition(triggerRect, targetDimensions, placement, alignment, offset, viewport) {
        let x, y;

        // Calculate position based on placement
        switch (placement) {
            case 'top':
                y = triggerRect.top - targetDimensions.height - offset;
                console.log('🎯 DEBUG TOP calculation:', {
                    triggerTop: triggerRect.top,
                    targetHeight: targetDimensions.height,
                    offset: offset,
                    calculatedY: y,
                    formula: `${triggerRect.top} - ${targetDimensions.height} - ${offset} = ${y}`
                });
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

        // Calculate alignment
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

    /**
     * Detect collision with viewport boundaries
     * @private
     */
    _detectCollision(position, targetDimensions, viewport) {
        // For fixed positioning, check against viewport boundaries directly
        const collision = {
            top: position.y < 0,
            bottom: position.y + targetDimensions.height > viewport.height,
            left: position.x < 0,
            right: position.x + targetDimensions.width > viewport.width
        };

        // Return null if no collision
        const hasCollision = Object.values(collision).some(v => v);
        return hasCollision ? collision : null;
    }

    /**
     * Get flipped placement based on collision
     * @private
     */
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

    /**
     * Calculate collision severity score
     * @private
     */
    _collisionScore(collision) {
        if (!collision) return 0;
        return Object.values(collision).filter(v => v).length;
    }

    /**
     * Constrain position to viewport boundaries
     * @private
     */
    _constrainToViewport(position, targetDimensions, viewport) {
        const constrained = { ...position };

        // Constrain X axis for fixed positioning (viewport boundaries)
        constrained.x = Math.max(
            0,
            Math.min(
                constrained.x,
                viewport.width - targetDimensions.width
            )
        );

        // Constrain Y axis for fixed positioning (viewport boundaries)
        constrained.y = Math.max(
            0,
            Math.min(
                constrained.y,
                viewport.height - targetDimensions.height
            )
        );

        return constrained;
    }

    /**
     * Ultra-generic container constraint handling (all directions with auto-adaptation)
     * Automatically handles left, right, top, bottom boundaries with offset
     * 
     * @param {Object} position - Current position { x, y, placement }
     * @param {Object} targetDimensions - Target dimensions { width, height }
     * @param {DOMRect} container - Container bounds
     * @param {number} offset - Minimum distance from container edges
     * @returns {Object} Constrained position
     */
    _constrainToContainer(position, targetDimensions, container, offset = 8) {
        const constrained = { ...position };

        // Ultra-generic X-axis constraint (left/right adaptive)
        if (constrained.x < container.left + offset) {
            constrained.x = container.left + offset;
        } else if (constrained.x + targetDimensions.width > container.right - offset) {
            constrained.x = container.right - targetDimensions.width - offset;
        }

        // Ultra-generic Y-axis constraint (top/bottom adaptive)  
        if (constrained.y < container.top + offset) {
            constrained.y = container.top + offset;
        } else if (constrained.y + targetDimensions.height > container.bottom - offset) {
            constrained.y = container.bottom - targetDimensions.height - offset;
        }

        return constrained;
    }

    /**
     * Calculate arrow position for tooltips/popovers
     * 
     * @param {string} placement - Current placement
     * @param {DOMRect} triggerRect - Trigger element rect
     * @param {Object} targetPosition - Calculated target position
     * @returns {Object} Arrow position { x, y, side }
     */
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

    /**
     * Update position on scroll/resize
     * Returns new position or null if no change needed
     * 
     * @param {Object} currentPosition - Current position
     * @param {DOMRect} triggerRect - Updated trigger rect
     * @param {Object} targetDimensions - Target dimensions
     * @param {Object} options - Positioning options
     * @returns {Object|null} New position or null
     */
    updatePosition(currentPosition, triggerRect, targetDimensions, options = {}) {
        if (!currentPosition || !triggerRect || !targetDimensions) {
            throw new Error('PositioningEngine: All parameters required for position update');
        }

        const newPosition = this.calculatePosition(triggerRect, targetDimensions, {
            ...options,
            position: currentPosition.placement
        });

        // Check if position actually changed
        if (newPosition.x === currentPosition.x && 
            newPosition.y === currentPosition.y &&
            newPosition.placement === currentPosition.placement) {
            return null; // No change needed
        }

        return newPosition;
    }
}

// Export singleton instance for convenience
export const positioningEngine = new PositioningEngine();

// Default export
export default PositioningEngine;