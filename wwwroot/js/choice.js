// Choice dropdown positioning utilities
export const Choice = {
    shouldOpenUpward: function(element) {
        try {
            if (!element) return false;
            
            const rect = element.getBoundingClientRect();
            const viewportHeight = window.innerHeight;
            const estimatedDropdownHeight = 300;
            const scrollBuffer = 20;
            
            const spaceBelow = viewportHeight - rect.bottom - scrollBuffer;
            const spaceAbove = rect.top - scrollBuffer;
            
            return spaceBelow < estimatedDropdownHeight && spaceAbove > estimatedDropdownHeight;
        } catch (error) {
            console.warn('[RR.Blazor] Choice.shouldOpenUpward error:', error);
            return false;
        }
    },
    
    calculateOptimalPosition: function(triggerElement, options = {}) {
        try {
            if (!triggerElement) return { direction: 'down', position: 'center' };
            
            const triggerRect = triggerElement.getBoundingClientRect();
            const viewportWidth = window.innerWidth;
            const viewportHeight = window.innerHeight;
            const dropdownHeight = options.estimatedHeight || 300;
            const dropdownWidth = options.estimatedWidth || Math.max(triggerRect.width, 200);
            const buffer = options.buffer || 20;
            
            const spaces = {
                above: triggerRect.top - buffer,
                below: viewportHeight - triggerRect.bottom - buffer,
                left: triggerRect.left - buffer,
                right: viewportWidth - triggerRect.right - buffer
            };
            
            let verticalDirection = 'down';
            if (spaces.below < dropdownHeight && spaces.above > dropdownHeight) {
                verticalDirection = 'up';
            } else if (spaces.below < dropdownHeight && spaces.above < dropdownHeight) {
                verticalDirection = spaces.above > spaces.below ? 'up' : 'down';
            }
            
            let horizontalPosition = 'start';
            const triggerCenter = triggerRect.left + (triggerRect.width / 2);
            const dropdownHalfWidth = dropdownWidth / 2;
            
            if (triggerCenter - dropdownHalfWidth < buffer) {
                horizontalPosition = 'start';
            } else if (triggerCenter + dropdownHalfWidth > viewportWidth - buffer) {
                horizontalPosition = 'end';
            } else {
                horizontalPosition = 'center';
            }
            
            const isNearLeftEdge = triggerRect.left < viewportWidth * 0.25;
            const isNearRightEdge = triggerRect.right > viewportWidth * 0.75;
            const isNearTopEdge = triggerRect.top < viewportHeight * 0.25;
            const isNearBottomEdge = triggerRect.bottom > viewportHeight * 0.75;
            
            if (isNearLeftEdge && isNearBottomEdge) {
                verticalDirection = 'up';
                horizontalPosition = 'start';
            }
            
            return {
                direction: verticalDirection,
                position: horizontalPosition,
                spaces: spaces,
                debug: {
                    triggerRect,
                    dropdownSize: { width: dropdownWidth, height: dropdownHeight },
                    isNearEdges: { left: isNearLeftEdge, right: isNearRightEdge, top: isNearTopEdge, bottom: isNearBottomEdge }
                }
            };
        } catch (error) {
            console.warn('[RR.Blazor] Choice.calculateOptimalPosition error:', error);
            return { direction: 'down', position: 'center' };
        }
    },
    
    positionDropdown: function(triggerElement, options = {}) {
        try {
            if (!triggerElement) return { left: 0, top: 0, width: 0 };
            
            const triggerRect = triggerElement.getBoundingClientRect();
            const viewportWidth = window.innerWidth;
            const viewportHeight = window.innerHeight;
            const dropdownHeight = options.estimatedHeight || 300;
            const dropdownWidth = options.estimatedWidth || Math.max(triggerRect.width, 200);
            const buffer = options.buffer || 20;
            
            const optimal = this.calculateOptimalPosition(triggerElement, options);
            
            let top, left;
            
            if (optimal.direction === 'up') {
                top = triggerRect.top - dropdownHeight;
            } else {
                top = triggerRect.bottom + 4;
            }
            
            switch (optimal.position) {
                case 'start':
                    left = triggerRect.left;
                    break;
                case 'end':
                    left = triggerRect.right - dropdownWidth;
                    break;
                case 'center':
                default:
                    left = triggerRect.left + (triggerRect.width - dropdownWidth) / 2;
                    break;
            }
            
            top = Math.max(buffer, Math.min(top, viewportHeight - dropdownHeight - buffer));
            left = Math.max(buffer, Math.min(left, viewportWidth - dropdownWidth - buffer));
            
            return {
                left: Math.round(left),
                top: Math.round(top),
                width: Math.round(Math.max(triggerRect.width, dropdownWidth)),
                direction: optimal.direction,
                position: optimal.position,
                optimal: optimal
            };
        } catch (error) {
            console.warn('[RR.Blazor] Choice.positionDropdown error:', error);
            return { left: 0, top: 0, width: 0, direction: 'down', position: 'center' };
        }
    },
    
    applyDynamicPositioning: function(choiceElement) {
        try {
            if (!choiceElement) return;
            
            const trigger = choiceElement.querySelector('.choice-trigger');
            const viewport = choiceElement.querySelector('.choice-viewport');
            
            if (!trigger || !viewport) return;
            
            const positioning = this.positionDropdown(trigger);
            
            choiceElement.classList.remove(
                'choice-top', 'choice-bottom', 
                'choice-left', 'choice-right',
                'choice-topstart', 'choice-topend',
                'choice-bottomstart', 'choice-bottomend'
            );
            
            const directionClass = `choice-${positioning.direction}${positioning.position !== 'center' ? positioning.position : ''}`;
            choiceElement.classList.add(directionClass);
            
            if (window.debugLogger && window.debugLogger.isDebugMode) {
                window.debugLogger.log('Choice positioning:', {
                    element: choiceElement,
                    positioning: positioning,
                    appliedClass: directionClass
                });
            }
            
            return positioning;
        } catch (error) {
            console.warn('[RR.Blazor] Choice.applyDynamicPositioning error:', error);
        }
    }
};

// Portal management for choice dropdowns
export async function createChoicePortal(choiceElementId) {
    try {
        const choice = document.querySelector(`[data-choice-id="${choiceElementId}"]`);
        const viewport = choice?.querySelector('.choice-viewport');
        const trigger = choice?.querySelector('.choice-trigger');
        
        if (!viewport || !trigger) return false;
        
        const portalId = await window.RRBlazor.Portal.create(viewport, {
            id: `choice-${choiceElementId}`,
            type: 'dropdown',
            anchor: trigger,
            className: 'choice-portal',
            closeOnClickOutside: true,
            onClickOutside: () => {
                const event = new CustomEvent('choiceclickoutside', {
                    detail: { choiceId: choiceElementId },
                    bubbles: true
                });
                choice.dispatchEvent(event);
            }
        });
        
        if (portalId) {
            viewport._portalId = portalId;
            choice._portalId = portalId;
            window.RRBlazor.Portal.position(portalId);
        }
        
        return portalId;
    } catch (error) {
        console.error('[Choice] Portal creation failed:', error);
        return false;
    }
}

export async function destroyChoicePortal(portalId) {
    try {
        return await window.RRBlazor.Portal.destroy(portalId);
    } catch (error) {
        console.error('[Choice] Portal cleanup failed:', error);
        return false;
    }
}

// Register click-outside callback for Blazor component
export function registerClickOutside(choiceElementId, dotNetRef) {
    try {
        const choice = document.querySelector(`[data-choice-id="${choiceElementId}"]`);
        const portalId = choice?._portalId;
        
        if (portalId) {
            window.RRBlazor.Portal.update(portalId, {
                onClickOutside: () => dotNetRef.invokeMethodAsync('OnClickOutside')
            });
        }
    } catch (error) {
        console.error('[Choice] Click-outside registration failed:', error);
    }
}

// Export for module usage
window.RRChoice = {
    createPortal: createChoicePortal,
    destroyPortal: destroyChoicePortal,
    registerClickOutside: registerClickOutside,
    shouldOpenUpward: Choice.shouldOpenUpward,
    calculateOptimalPosition: Choice.calculateOptimalPosition
};

export default {
    Choice,
    createChoicePortal,
    destroyChoicePortal,
    registerClickOutside
};