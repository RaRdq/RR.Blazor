// RR.Blazor Choice Component JavaScript Module
// Handles dropdown positioning, viewport calculations, and dynamic positioning

// Choice dropdown component utilities
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
            
            // Calculate available space in all directions
            const spaces = {
                above: triggerRect.top - buffer,
                below: viewportHeight - triggerRect.bottom - buffer,
                left: triggerRect.left - buffer,
                right: viewportWidth - triggerRect.right - buffer
            };
            
            // Determine optimal vertical direction
            let verticalDirection = 'down';
            if (spaces.below < dropdownHeight && spaces.above > dropdownHeight) {
                verticalDirection = 'up';
            } else if (spaces.below < dropdownHeight && spaces.above < dropdownHeight) {
                // Neither fits well, choose the larger space
                verticalDirection = spaces.above > spaces.below ? 'up' : 'down';
            }
            
            // Determine optimal horizontal alignment
            let horizontalPosition = 'start'; // Default to left-aligned
            const triggerCenter = triggerRect.left + (triggerRect.width / 2);
            const dropdownHalfWidth = dropdownWidth / 2;
            
            // Check if centered dropdown would overflow
            if (triggerCenter - dropdownHalfWidth < buffer) {
                horizontalPosition = 'start'; // Left-aligned
            } else if (triggerCenter + dropdownHalfWidth > viewportWidth - buffer) {
                horizontalPosition = 'end'; // Right-aligned
            } else {
                horizontalPosition = 'center'; // Centered
            }
            
            // For special cases like sidebars, prefer opening away from edges
            const isNearLeftEdge = triggerRect.left < viewportWidth * 0.25;
            const isNearRightEdge = triggerRect.right > viewportWidth * 0.75;
            const isNearTopEdge = triggerRect.top < viewportHeight * 0.25;
            const isNearBottomEdge = triggerRect.bottom > viewportHeight * 0.75;
            
            // Special logic for sidebar role switcher (bottom-left corner)
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
            
            // Get optimal positioning
            const optimal = this.calculateOptimalPosition(triggerElement, options);
            
            // Calculate position based on optimal direction
            let top, left;
            
            // Vertical positioning
            if (optimal.direction === 'up') {
                top = triggerRect.top - dropdownHeight - 4;
            } else {
                top = triggerRect.bottom + 4;
            }
            
            // Horizontal positioning
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
            
            // Ensure dropdown stays within viewport bounds
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
            
            // Apply positioning classes
            choiceElement.classList.remove(
                'choice-top', 'choice-bottom', 
                'choice-left', 'choice-right',
                'choice-topstart', 'choice-topend',
                'choice-bottomstart', 'choice-bottomend'
            );
            
            // Add direction and position classes
            const directionClass = `choice-${positioning.direction}${positioning.position !== 'center' ? positioning.position : ''}`;
            choiceElement.classList.add(directionClass);
            
            // Add debug info for development
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

// Legacy dropdown positioning functions for backwards compatibility
export function adjustDropdownPosition(dropdownElement) {
    if (!dropdownElement) return;
    
    const viewport = dropdownElement.querySelector('.dropdown-viewport');
    const content = dropdownElement.querySelector('.dropdown-content');
    
    if (!viewport || !content) return;
    
    // Get dropdown and viewport dimensions
    const dropdownRect = dropdownElement.getBoundingClientRect();
    const viewportHeight = window.innerHeight;
    const viewportWidth = window.innerWidth;
    
    // Calculate space above and below the dropdown trigger
    const spaceAbove = dropdownRect.top;
    const spaceBelow = viewportHeight - dropdownRect.bottom;
    
    // Get content dimensions (temporarily show it to measure)
    const originalDisplay = content.style.display;
    content.style.visibility = 'hidden';
    content.style.display = 'block';
    const contentRect = content.getBoundingClientRect();
    content.style.display = originalDisplay;
    content.style.visibility = '';
    
    const contentHeight = contentRect.height || 250; // Fallback height
    const shouldPositionAbove = spaceBelow < contentHeight && spaceAbove > spaceBelow;
    
    if (shouldPositionAbove) {
        viewport.style.bottom = '100%';
        viewport.style.top = 'auto';
        viewport.style.marginBottom = '8px';
        viewport.style.marginTop = '0';
    } else {
        viewport.style.top = '100%';
        viewport.style.bottom = 'auto';
        viewport.style.marginTop = '8px';
        viewport.style.marginBottom = '0';
    }
    
    if (dropdownRect.right + 320 > viewportWidth) { // 320px typical dropdown width
        viewport.style.right = '0';
        viewport.style.left = 'auto';
    } else {
        viewport.style.left = '0';
        viewport.style.right = 'auto';
    }
    
    const dropdown = dropdownElement;
    dropdown.classList.remove('dropdown--position-above', 'dropdown--position-below');
    dropdown.classList.add(shouldPositionAbove ? 'dropdown--position-above' : 'dropdown--position-below');
}

export function adjustChoicePosition(choiceElement) {
    if (!choiceElement) return;
    
    const viewport = choiceElement.querySelector('.choice-viewport');
    const trigger = choiceElement.querySelector('.choice-trigger');
    
    if (!viewport || !trigger) return;
    
    // Get trigger and viewport dimensions
    const triggerRect = trigger.getBoundingClientRect();
    const viewportHeight = window.innerHeight;
    const viewportWidth = window.innerWidth;
    
    // Calculate space above and below the trigger
    const spaceAbove = triggerRect.top;
    const spaceBelow = viewportHeight - triggerRect.bottom;
    
    // Get viewport content dimensions (temporarily show to measure)
    const originalDisplay = viewport.style.display;
    const originalVisibility = viewport.style.visibility;
    viewport.style.visibility = 'hidden';
    viewport.style.display = 'block';
    const viewportRect = viewport.getBoundingClientRect();
    viewport.style.display = originalDisplay;
    viewport.style.visibility = originalVisibility;
    
    const contentHeight = viewportRect.height || 200; // Fallback height
    const shouldPositionAbove = spaceBelow < contentHeight && spaceAbove > spaceBelow;
    
    // Remove existing positioning classes
    choiceElement.classList.remove('choice-top', 'choice-bottom', 'choice-topend', 'choice-bottomend');
    
    // Check if we need right alignment
    const shouldAlignRight = triggerRect.right + 320 > viewportWidth; // 320px typical choice width
    
    // Apply appropriate positioning class
    if (shouldPositionAbove) {
        choiceElement.classList.add(shouldAlignRight ? 'choice-topend' : 'choice-top');
    } else {
        choiceElement.classList.add(shouldAlignRight ? 'choice-bottomend' : 'choice-bottom');
    }
}

// Export all functions for backwards compatibility
export default {
    Choice,
    adjustDropdownPosition,
    adjustChoicePosition
};