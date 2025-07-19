const RRDebugAPI = (() => {
    const VERSION = '3.0.0';
    
    function sanitizeForJSON(obj) {
        if (obj === null || obj === undefined) return obj;
        if (typeof obj === 'string') {
            try {
                // AGGRESSIVE Unicode cleaning to prevent Claude Code API errors
                let clean = obj
                    .replace(/[\u0000-\u001F\u007F-\u009F]/g, '') // Control characters
                    .replace(/[\uD800-\uDFFF]/g, '') // ALL surrogates - prevent "no low surrogate" errors
                    .replace(/[\uFEFF\uFFFE\uFFFF]/g, '') // BOM and invalid chars
                    .replace(/[\u200B-\u200F\u2028-\u202F]/g, '') // Zero-width and line separators
                    .replace(/[^\x09\x0A\x0D\x20-\x7E\u00A0-\u017F\u0400-\u04FF]/g, '') // Only basic Latin + common extended
                    .trim();
                
                // Reasonable size limit while preserving useful data
                if (clean.length > 1000) {
                    clean = clean.substring(0, 1000) + '...';
                }
                
                return clean;
            } catch (e) {
                return '[INVALID_STRING]';
            }
        }
        if (typeof obj === 'number') {
            if (isNaN(obj) || !isFinite(obj)) return 0;
            return Math.round(obj * 100) / 100; // Round to 2 decimals
        }
        if (typeof obj === 'boolean') return obj;
        if (Array.isArray(obj)) {
            try {
                // Keep reasonable amount of data while preventing Unicode issues
                return obj.slice(0, 15).map(sanitizeForJSON).filter(item => item !== undefined);
            } catch (e) {
                return [];
            }
        }
        if (typeof obj === 'object') {
            try {
                const clean = {};
                let count = 0;
                let totalSize = 0;
                for (const [key, value] of Object.entries(obj)) {
                    if (count++ >= 50) break; // Reasonable limit
                    if (totalSize > 20000) break; // Reasonable payload size
                    
                    const cleanKey = sanitizeForJSON(key);
                    if (cleanKey && typeof cleanKey === 'string' && cleanKey.length > 0 && cleanKey.length < 200) {
                        const cleanValue = sanitizeForJSON(value);
                        if (cleanValue !== undefined) {
                            const valueSize = typeof cleanValue === 'string' ? cleanValue.length : 50;
                            totalSize += valueSize;
                            clean[cleanKey] = cleanValue;
                        }
                    }
                }
                return clean;
            } catch (e) {
                return { error: 'OBJECT_SANITIZATION_FAILED' };
            }
        }
        return obj;
    }
    
    function safeStringify(obj) {
        try {
            const sanitized = sanitizeForJSON(obj);
            const jsonString = JSON.stringify(sanitized);
            
            // Additional validation - try to parse it back
            JSON.parse(jsonString);
            return jsonString;
        } catch (e) {
            try {
                // Fallback: create minimal safe object
                const fallback = {
                    error: 'JSON_STRINGIFY_FAILED',
                    message: String(e.message || 'Unknown error').substring(0, 100),
                    timestamp: new Date().toISOString()
                };
                return JSON.stringify(fallback);
            } catch (fallbackError) {
                // Ultimate fallback
                return '{"error":"COMPLETE_JSON_FAILURE","timestamp":"' + new Date().toISOString() + '"}';
            }
        }
    }
    
    // Comprehensive issue detection patterns for all web development problems
    const CRITICAL_ISSUES = {
        // Layout & Display Issues
        invisibleElement: el => {
            const s = getComputedStyle(el);
            const rect = el.getBoundingClientRect();
            return (s.display === 'none' && el.textContent?.trim()) ||
                   (s.visibility === 'hidden' && el.textContent?.trim()) ||
                   (s.opacity === '0' && el.textContent?.trim()) ||
                   (rect.width === 0 && rect.height === 0 && el.textContent?.trim());
        },
        
        overflowingElement: el => {
            const rect = el.getBoundingClientRect();
            return rect.width > window.innerWidth * 1.2 || rect.height > window.innerHeight * 2;
        },
        
        offscreenElement: el => {
            const rect = el.getBoundingClientRect();
            const s = getComputedStyle(el);
            return s.position !== 'fixed' && (rect.right < -50 || rect.bottom < -50 || 
                   rect.left > window.innerWidth + 50 || rect.top > window.innerHeight + 50);
        },
        
        zeroSizedWithContent: el => {
            const rect = el.getBoundingClientRect();
            return (rect.width === 0 || rect.height === 0) && 
                   (el.textContent?.trim() || el.children.length > 0);
        },
        
        brokenFlexbox: el => {
            const s = getComputedStyle(el);
            return (el.className.includes('flex') && !s.display.includes('flex')) ||
                   (s.display === 'flex' && s.flexWrap === 'nowrap' && el.children.length > 8);
        },
        
        brokenVerticalRhythm: el => {
            const s = getComputedStyle(el);
            const lineHeight = parseFloat(s.lineHeight) || 0;
            const fontSize = parseFloat(s.fontSize) || 0;
            
            // Flag elements with broken line-height ratios
            if (fontSize > 0 && lineHeight > 0) {
                const ratio = lineHeight / fontSize;
                return ratio < 0.8 || ratio > 3.0; // Outside reasonable range
            }
            
            return false;
        },
        
        extremeHeight: el => {
            const rect = el.getBoundingClientRect();
            const s = getComputedStyle(el);
            
            // Detect suspiciously extreme heights that indicate corruption
            const hasFixedExtremeHeight = s.height && s.height.includes('px') && 
                                        parseFloat(s.height) > window.innerHeight * 3;
            
            const hasMinHeightIssue = s.minHeight && s.minHeight.includes('px') && 
                                    parseFloat(s.minHeight) > window.innerHeight;
            
            const hasRenderExtremeHeight = rect.height > window.innerHeight * 10;
            
            return hasFixedExtremeHeight || hasMinHeightIssue || hasRenderExtremeHeight;
        },
        
        brokenGrid: el => {
            const s = getComputedStyle(el);
            return (el.className.includes('grid') && s.display !== 'grid') ||
                   (s.display === 'grid' && !s.gridTemplateColumns && !s.gridTemplateRows && el.children.length > 0);
        },
        
        // CSS & Styling Issues
        forcedHeight: el => {
            const s = getComputedStyle(el);
            return s.minHeight === '44px' && !['BUTTON', 'INPUT', 'A', 'LABEL', 'SELECT', 'TEXTAREA'].includes(el.tagName);
        },
        
        invalidCSS: el => {
            return Array.from(el.classList).some(cls => 
                cls.startsWith('@') || cls.includes('undefined') || cls.includes('NaN') || 
                cls.includes('null') || cls.includes('extend-') || cls.includes('mixin-'));
        },
        
        missingCriticalStyles: el => {
            const s = getComputedStyle(el);
            const tag = el.tagName.toLowerCase();
            return (tag === 'button' && s.cursor !== 'pointer') ||
                   (tag === 'a' && s.cursor !== 'pointer' && el.href) ||
                   (tag === 'input' && s.padding === '0px') ||
                   (el.className.includes('btn') && s.cursor !== 'pointer');
        },
        
        textColorIssues: el => {
            const s = getComputedStyle(el);
            const hasText = el.textContent?.trim();
            return hasText && (
                s.color === s.backgroundColor ||
                s.color === 'transparent' ||
                (s.color === 'rgb(255, 255, 255)' && s.backgroundColor === 'rgb(255, 255, 255)') ||
                (s.color === 'rgb(0, 0, 0)' && s.backgroundColor === 'rgb(0, 0, 0)')
            );
        },
        
        // Typography Issues
        tinyText: el => {
            const s = getComputedStyle(el);
            return el.textContent?.trim() && parseFloat(s.fontSize) < 10;
        },
        
        hugeText: el => {
            const s = getComputedStyle(el);
            return el.textContent?.trim() && parseFloat(s.fontSize) > 72;
        },
        
        // Accessibility Issues
        missingAltText: el => {
            return el.tagName === 'IMG' && !el.alt && !el.getAttribute('aria-label');
        },
        
        emptyButtonOrLink: el => {
            return (el.tagName === 'BUTTON' || el.tagName === 'A') && 
                   !el.textContent?.trim() && !el.getAttribute('aria-label') && 
                   !el.querySelector('img[alt]');
        },
        
        smallTouchTarget: el => {
            const rect = el.getBoundingClientRect();
            const isInteractive = ['BUTTON', 'A', 'INPUT', 'SELECT', 'TEXTAREA'].includes(el.tagName) ||
                                 el.onclick || el.getAttribute('role') === 'button';
            return isInteractive && (rect.width < 44 || rect.height < 44) && rect.width > 0 && rect.height > 0;
        },
        
        // Performance Issues
        excessiveNesting: el => {
            let depth = 0;
            let current = el;
            while (current.parentElement) {
                depth++;
                current = current.parentElement;
                if (depth > 20) break; // Prevent infinite loops
            }
            return depth > 15;
        },
        
        tooManyChildren: el => {
            return el.children.length > 30;
        },
        
        // Position & Z-index Issues
        excessiveZIndex: el => {
            const s = getComputedStyle(el);
            const zIndex = parseInt(s.zIndex);
            return !isNaN(zIndex) && zIndex > 1000;
        },
        
        positionedWithoutCoords: el => {
            const s = getComputedStyle(el);
            return (s.position === 'absolute' || s.position === 'fixed') &&
                   s.top === 'auto' && s.right === 'auto' && 
                   s.bottom === 'auto' && s.left === 'auto';
        },
        
        // Form Issues
        inputWithoutLabels: el => {
            return ['INPUT', 'TEXTAREA', 'SELECT'].includes(el.tagName) &&
                   el.type !== 'hidden' && !el.labels?.length && 
                   !el.getAttribute('aria-label') && !el.getAttribute('placeholder');
        },
        
        // Mobile & Responsive Issues
        notMobileFriendly: el => {
            const rect = el.getBoundingClientRect();
            return rect.width > window.innerWidth && !['HTML', 'BODY'].includes(el.tagName);
        },
        
        fixedWidthBreakpoint: el => {
            const s = getComputedStyle(el);
            return s.width && s.width.includes('px') && parseInt(s.width) > 768 && 
                   !['IMG', 'VIDEO', 'IFRAME'].includes(el.tagName);
        },
        
        // Animation & Transition Issues
        missingTransitions: el => {
            const s = getComputedStyle(el);
            const isInteractive = ['BUTTON', 'A'].includes(el.tagName) || el.onclick ||
                                 el.className.includes('hover') || el.className.includes('btn');
            return isInteractive && s.transition === 'all 0s ease 0s';
        },
        
        // Color & Contrast Issues
        lowContrast: el => {
            const s = getComputedStyle(el);
            if (!el.textContent?.trim()) return false;
            
            const color = s.color;
            const bgColor = s.backgroundColor;
            return (color === 'rgb(128, 128, 128)' && bgColor === 'rgb(255, 255, 255)') ||
                   (color === 'rgb(255, 255, 255)' && bgColor === 'rgb(240, 240, 240)');
        },
        
        // Layout & Spacing Issues
        negativeMargins: el => {
            const s = getComputedStyle(el);
            return s.marginLeft.includes('-') || s.marginRight.includes('-') ||
                   s.marginTop.includes('-') || s.marginBottom.includes('-');
        },
        
        excessiveSpacing: el => {
            const s = getComputedStyle(el);
            const viewportHeight = window.innerHeight;
            const viewportWidth = window.innerWidth;
            
            // Check for excessive margins/padding relative to viewport
            const margin = Math.max(
                parseFloat(s.marginTop) || 0,
                parseFloat(s.marginBottom) || 0,
                parseFloat(s.marginLeft) || 0,
                parseFloat(s.marginRight) || 0
            );
            
            const padding = Math.max(
                parseFloat(s.paddingTop) || 0,
                parseFloat(s.paddingBottom) || 0,
                parseFloat(s.paddingLeft) || 0,
                parseFloat(s.paddingRight) || 0
            );
            
            // Flag if any spacing exceeds reasonable thresholds
            return margin > viewportHeight * 0.3 || padding > viewportWidth * 0.2 ||
                   margin > 300 || padding > 200;
        },
        
        disproportionateSpacing: el => {
            const s = getComputedStyle(el);
            const rect = el.getBoundingClientRect();
            
            // Skip if element has no content or size
            if (rect.width === 0 || rect.height === 0) return false;
            
            const totalPadding = (parseFloat(s.paddingTop) || 0) + 
                               (parseFloat(s.paddingBottom) || 0) + 
                               (parseFloat(s.paddingLeft) || 0) + 
                               (parseFloat(s.paddingRight) || 0);
            
            const totalMargin = (parseFloat(s.marginTop) || 0) + 
                              (parseFloat(s.marginBottom) || 0) + 
                              (parseFloat(s.marginLeft) || 0) + 
                              (parseFloat(s.marginRight) || 0);
            
            // Flag if spacing is larger than the content itself
            return totalPadding > rect.width || totalMargin > rect.height * 2;
        },
        
        inconsistentSpacing: el => {
            const s = getComputedStyle(el);
            
            // Check for wildly inconsistent margin/padding values
            const margins = [
                parseFloat(s.marginTop) || 0,
                parseFloat(s.marginRight) || 0,
                parseFloat(s.marginBottom) || 0,
                parseFloat(s.marginLeft) || 0
            ];
            
            const paddings = [
                parseFloat(s.paddingTop) || 0,
                parseFloat(s.paddingRight) || 0,
                parseFloat(s.paddingBottom) || 0,
                parseFloat(s.paddingLeft) || 0
            ];
            
            // Calculate variance to detect inconsistency
            const marginMax = Math.max(...margins);
            const marginMin = Math.min(...margins.filter(m => m > 0));
            const paddingMax = Math.max(...paddings);
            const paddingMin = Math.min(...paddings.filter(p => p > 0));
            
            // Flag if there's extreme inconsistency (more than 10x difference)
            return (marginMax > 0 && marginMin > 0 && marginMax / marginMin > 10) ||
                   (paddingMax > 0 && paddingMin > 0 && paddingMax / paddingMin > 10);
        },
        
        brokenGutters: el => {
            const s = getComputedStyle(el);
            
            // Check for broken grid/flex gutters
            if (s.display === 'grid') {
                const gap = parseFloat(s.gap) || parseFloat(s.gridGap) || 0;
                const columnGap = parseFloat(s.columnGap) || parseFloat(s.gridColumnGap) || 0;
                const rowGap = parseFloat(s.rowGap) || parseFloat(s.gridRowGap) || 0;
                
                // Flag excessive gaps in grid layouts
                return gap > 100 || columnGap > 150 || rowGap > 150;
            }
            
            if (s.display === 'flex') {
                const gap = parseFloat(s.gap) || 0;
                return gap > 100;
            }
            
            return false;
        },
        
        malformedLayout: el => {
            const rect = el.getBoundingClientRect();
            const s = getComputedStyle(el);
            
            // Detect severely malformed elements
            const hasExtremeAspectRatio = rect.width > 0 && rect.height > 0 && 
                                        (rect.width / rect.height > 50 || rect.height / rect.width > 50);
            
            const hasExtremeSize = rect.width > window.innerWidth * 5 || 
                                 rect.height > window.innerHeight * 5;
            
            const hasConfictingPositions = s.position === 'absolute' && 
                                         s.top !== 'auto' && s.bottom !== 'auto' &&
                                         s.left !== 'auto' && s.right !== 'auto';
            
            return hasExtremeAspectRatio || hasExtremeSize || hasConfictingPositions;
        },
        
        overlapppingElements: el => {
            const rect = el.getBoundingClientRect();
            const siblings = Array.from(el.parentElement?.children || []);
            return siblings.some(sibling => {
                if (sibling === el) return false;
                const sibRect = sibling.getBoundingClientRect();
                return rect.left < sibRect.right && rect.right > sibRect.left &&
                       rect.top < sibRect.bottom && rect.bottom > sibRect.top;
            });
        },
        
        // Content Issues
        emptyContainer: el => {
            return el.children.length === 0 && !el.textContent?.trim() && 
                   !['IMG', 'INPUT', 'HR', 'BR'].includes(el.tagName) &&
                   el.getBoundingClientRect().height > 20;
        },
        
        textOverflow: el => {
            const s = getComputedStyle(el);
            return s.overflow === 'visible' && el.scrollWidth > el.clientWidth + 5;
        },
        
        // Image Issues
        unsizedImage: el => {
            if (el.tagName !== 'IMG') return false;
            const s = getComputedStyle(el);
            return !s.width || !s.height || s.width === 'auto' || s.height === 'auto';
        },
        
        // JavaScript Issues
        consoleErrors: () => {
            return window.__rrDebugErrors?.length > 0;
        },
        
        // Theme Issues
        hardcodedColors: el => {
            const s = getComputedStyle(el);
            const hasHardcoded = s.color?.includes('#') || s.backgroundColor?.includes('#') ||
                               s.borderColor?.includes('#');
            return hasHardcoded && !el.style.color && !el.style.backgroundColor;
        },
        
        // Performance & Memory Issues
        heavyDOMNode: el => {
            return el.children.length > 50 || el.innerHTML.length > 10000;
        },
        
        duplicateIds: el => {
            if (!el.id) return false;
            return document.querySelectorAll(`#${el.id}`).length > 1;
        },
        
        // Loading & State Issues
        emptyLoadingState: el => {
            return el.className.includes('loading') && !el.textContent?.trim() && 
                   !el.querySelector('.spinner, .skeleton, .placeholder');
        },
        
        // Navigation Issues
        brokenLinks: el => {
            return el.tagName === 'A' && el.href && 
                   (el.href === window.location.href + '#' || el.href.endsWith('undefined'));
        },
        
        // Security Issues
        unsafeInlineEvents: el => {
            return ['onclick', 'onmouseover', 'onerror', 'onload'].some(event => el.getAttribute(event));
        },
        
        // SEO Issues
        missingHeadingHierarchy: el => {
            if (!['H1', 'H2', 'H3', 'H4', 'H5', 'H6'].includes(el.tagName)) return false;
            const level = parseInt(el.tagName[1]);
            const prevHeading = document.querySelector(`h${level - 1}`);
            return level > 1 && !prevHeading;
        },
        
        // Utility Issues
        redundantClasses: el => {
            const classes = Array.from(el.classList);
            return classes.length !== new Set(classes).size; // Has duplicate classes
        },
        
        // Framework-specific Issues
        blazorComponentError: el => {
            return el.getAttribute('b-error') || el.className.includes('blazor-error-boundary');
        },
        
        // JavaScript vs CSS Conflicts
        jsOverridingCSS: el => {
            const computed = getComputedStyle(el);
            const inline = el.style;
            
            // Detect when inline styles override CSS with !important indicators
            const hasInlineConflicts = ['display', 'position', 'width', 'height', 'top', 'left'].some(prop => {
                return inline[prop] && computed[prop] !== inline[prop];
            });
            
            // Check for programmatic style conflicts
            const hasDataConflicts = el.dataset.originalStyle || el.dataset.jsModified;
            
            return hasInlineConflicts || hasDataConflicts;
        },
        
        dynamicStyleConflict: el => {
            const s = getComputedStyle(el);
            
            // Detect elements with CSS classes that are being overridden by JS
            const hasTransformConflict = s.transform !== 'none' && el.style.transform && 
                                       s.transform !== el.style.transform;
            
            const hasDisplayConflict = el.classList.contains('hidden') && s.display !== 'none';
            
            const hasVisibilityConflict = el.classList.contains('invisible') && s.visibility !== 'hidden';
            
            return hasTransformConflict || hasDisplayConflict || hasVisibilityConflict;
        },
        
        inlineStyleOveruse: el => {
            // Detect excessive inline styles that should be CSS classes
            const inlineStyleCount = el.style.length;
            const hasMultipleInlineStyles = inlineStyleCount > 5;
            
            // Check for complex inline styles that indicate JS/CSS conflicts
            const hasComplexInlineStyles = el.style.cssText && 
                el.style.cssText.includes('!important');
            
            return hasMultipleInlineStyles || hasComplexInlineStyles;
        },
        
        animationConflict: el => {
            const s = getComputedStyle(el);
            
            // Detect CSS animations being overridden by JS
            const hasCSSAnimation = s.animation !== 'none';
            const hasCSSTransition = s.transition !== 'all 0s ease 0s';
            
            // Check for JS animation libraries that might conflict
            const hasJSAnimation = el.style.transition || el.style.animation ||
                                 el.getAttribute('data-animate') ||
                                 el.classList.toString().includes('animate-');
            
            return (hasCSSAnimation || hasCSSTransition) && hasJSAnimation;
        },
        
        computedStyleMismatch: el => {
            const s = getComputedStyle(el);
            
            // Detect when computed styles don't match expected class behavior
            const flexClassButNotFlex = (el.className.includes('flex') || el.className.includes('d-flex')) && 
                                      !s.display.includes('flex');
            
            const gridClassButNotGrid = (el.className.includes('grid') || el.className.includes('d-grid')) && 
                                      s.display !== 'grid';
            
            const hiddenClassButVisible = (el.className.includes('hidden') || el.className.includes('d-none')) && 
                                        s.display !== 'none';
            
            return flexClassButNotFlex || gridClassButNotGrid || hiddenClassButVisible;
        },
        
        eventListenerStyling: el => {
            // Detect elements that likely have JS event listeners affecting styling
            const hasClickEvents = el.onclick || el.addEventListener;
            const hasHoverStyling = el.style.cursor === 'pointer' || 
                                  getComputedStyle(el).cursor === 'pointer';
            
            // Check for elements that should be CSS-only but have JS styling
            const isStaticElement = ['DIV', 'SPAN', 'P', 'H1', 'H2', 'H3', 'H4', 'H5', 'H6'].includes(el.tagName);
            
            return isStaticElement && hasClickEvents && hasHoverStyling && 
                   !el.getAttribute('role') && !el.tabIndex;
        },
        
        cssVariableOverride: el => {
            const s = getComputedStyle(el);
            
            // Detect CSS custom properties being overridden inconsistently
            const cssVarProps = ['--primary-color', '--secondary-color', '--background-color', '--text-color'];
            const hasOverriddenVars = cssVarProps.some(prop => {
                const rootValue = getComputedStyle(document.documentElement).getPropertyValue(prop);
                const elValue = s.getPropertyValue(prop);
                return rootValue && elValue && rootValue !== elValue;
            });
            
            return hasOverriddenVars;
        },
        
        styleRecalculation: el => {
            // Detect elements that cause expensive style recalculations
            const rect = el.getBoundingClientRect();
            const s = getComputedStyle(el);
            
            // Elements with complex selectors or dynamic properties
            const hasComplexTransforms = s.transform !== 'none' && s.transform.includes('matrix');
            const hasComplexFilters = s.filter !== 'none';
            const hasComplexClipping = s.clipPath !== 'none';
            
            return hasComplexTransforms || hasComplexFilters || hasComplexClipping;
        },
        
        // Animation & Transition Tracking
        brokenTransitions: el => {
            const s = getComputedStyle(el);
            const isInteractive = ['BUTTON', 'A', 'INPUT', 'SELECT'].includes(el.tagName) ||
                                 el.onclick || el.className.includes('btn') || el.className.includes('hover');
            
            if (!isInteractive) return false;
            
            // Check for broken transition properties
            const transition = s.transition;
            const hasTransition = transition && transition !== 'all 0s ease 0s';
            
            if (hasTransition) {
                // Detect invalid transition values
                const hasInvalidDuration = transition.includes('0s') && transition.includes('ease');
                const hasInvalidProperty = transition.includes('undefined') || transition.includes('NaN');
                return hasInvalidDuration || hasInvalidProperty;
            }
            
            return false;
        },
        
        invisibleAnimation: el => {
            const s = getComputedStyle(el);
            
            // Detect animations that are effectively invisible
            const hasAnimation = s.animation !== 'none';
            
            if (hasAnimation) {
                // Check for animations with transparent or same-color transitions
                const opacity = parseFloat(s.opacity);
                const color = s.color;
                const backgroundColor = s.backgroundColor;
                
                // Flag animations on invisible elements
                const isInvisible = opacity === 0 || s.visibility === 'hidden' || s.display === 'none';
                
                // Flag animations with no visual change
                const hasNoVisualChange = color === backgroundColor && color === 'rgba(0, 0, 0, 0)';
                
                return isInvisible || hasNoVisualChange;
            }
            
            return false;
        },
        
        motionWithoutPurpose: el => {
            const s = getComputedStyle(el);
            
            // Detect motion that serves no UX purpose
            const hasTransform = s.transform !== 'none';
            const hasAnimation = s.animation !== 'none';
            const hasTransition = s.transition !== 'all 0s ease 0s';
            
            if (hasTransform || hasAnimation || hasTransition) {
                // Check if element is interactive or provides feedback
                const isInteractive = ['BUTTON', 'A', 'INPUT'].includes(el.tagName) ||
                                     el.onclick || el.onmouseover || el.getAttribute('role');
                
                const providesVisualFeedback = el.className.includes('loading') ||
                                             el.className.includes('spinner') ||
                                             el.className.includes('progress');
                
                // Flag motion on non-interactive, non-feedback elements
                return !isInteractive && !providesVisualFeedback;
            }
            
            return false;
        },
        
        performanceKillingAnimation: el => {
            const s = getComputedStyle(el);
            
            // Detect animations that cause performance issues
            const hasExpensiveAnimation = s.animation !== 'none' && (
                s.animation.includes('infinite') ||
                s.animationIterationCount === 'infinite'
            );
            
            const hasExpensiveTransform = s.transform !== 'none' && (
                s.transform.includes('matrix3d') ||
                s.transform.includes('perspective')
            );
            
            const hasLayoutTriggering = s.transition && (
                s.transition.includes('width') ||
                s.transition.includes('height') ||
                s.transition.includes('top') ||
                s.transition.includes('left')
            );
            
            return hasExpensiveAnimation || hasExpensiveTransform || hasLayoutTriggering;
        },
        
        hoverStateIssues: el => {
            const s = getComputedStyle(el);
            const isInteractive = ['BUTTON', 'A'].includes(el.tagName) || el.onclick;
            
            if (!isInteractive) return false;
            
            // Simulate hover to test visibility
            const originalTransition = el.style.transition;
            el.style.transition = 'none'; // Temporarily disable transitions for testing
            
            // Check hover styles by adding hover class if it exists
            const hoverClass = Array.from(el.classList).find(cls => cls.includes('hover'));
            if (hoverClass) {
                const hoverS = getComputedStyle(el);
                
                // Check for invisible hover states
                const hoverColor = hoverS.color;
                const hoverBg = hoverS.backgroundColor;
                
                const isInvisibleHover = hoverColor === 'rgba(0, 0, 0, 0)' ||
                                       hoverBg === 'rgba(0, 0, 0, 0)' ||
                                       hoverColor === '#ffffff' && hoverBg === '#ffffff' ||
                                       hoverColor === 'transparent';
                
                // Restore original transition
                el.style.transition = originalTransition;
                
                return isInvisibleHover;
            }
            
            el.style.transition = originalTransition;
            return false;
        },
        
        motionReducedIgnored: el => {
            const s = getComputedStyle(el);
            
            // Check if animations ignore prefers-reduced-motion
            const hasMotion = s.animation !== 'none' || s.transition !== 'all 0s ease 0s';
            
            if (hasMotion) {
                // Test if motion is disabled when user prefers reduced motion
                const prefersReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
                
                if (prefersReducedMotion) {
                    // Motion should be disabled or significantly reduced
                    const stillHasMotion = s.animation !== 'none' ||
                                         (s.transition !== 'all 0s ease 0s' && !s.transition.includes('0.15s'));
                    
                    return stillHasMotion;
                }
            }
            
            return false;
        },
        
        activeStateTracking: el => {
            const s = getComputedStyle(el);
            const isInteractive = ['BUTTON', 'A', 'INPUT'].includes(el.tagName) || el.onclick;
            
            if (!isInteractive) return false;
            
            // Test if active states are properly defined
            const hasActiveTransition = s.transition && s.transition !== 'all 0s ease 0s';
            const hasTransform = s.transform !== 'none';
            
            // Check for missing active state feedback
            const lacksActiveFeedback = !hasActiveTransition && !hasTransform &&
                                      s.cursor === 'pointer' && !el.disabled;
            
            return lacksActiveFeedback;
        },
        
        loadingAnimationIssues: el => {
            const s = getComputedStyle(el);
            const isLoadingElement = el.className.includes('loading') ||
                                   el.className.includes('spinner') ||
                                   el.className.includes('skeleton');
            
            if (!isLoadingElement) return false;
            
            // Check if loading animations are working
            const hasAnimation = s.animation !== 'none';
            const hasRotation = s.transform && s.transform.includes('rotate');
            const hasOpacityChange = s.opacity !== '1';
            
            // Loading elements should have some form of animation
            const lacksAnimation = !hasAnimation && !hasRotation && !hasOpacityChange;
            
            return lacksAnimation;
        }
    };

    // Removed external styles dependency - debug script should be self-contained
    
    // Simple built-in expectations for common CSS classes
    function getBuiltInExpectations() {
        return {
            // Form elements - critical for functionality
            'input': { padding: '!= 0px', border: '!= none', fontSize: '>= 14px' },
            'input-outlined': { border: 'includes solid', borderRadius: '!= 0px' },
            'input-filled': { backgroundColor: '!= rgba(0, 0, 0, 0)' },
            'input-lg': { fontSize: '>= 16px', minHeight: '>= 44px' },
            'input-sm': { fontSize: '<= 14px' },
            
            // Buttons - critical for interaction
            'button': { padding: '!= 0px', cursor: '== pointer' },
            'button-primary': { backgroundColor: '!= rgba(0, 0, 0, 0)' },
            'button-lg': { minHeight: '>= 44px' },
            'btn': { padding: '!= 0px', cursor: '== pointer' },
            
            // Layout utilities
            'w-full': { width: 'includes 100%' },
            'h-full': { height: 'includes 100%' },
            'flex': { display: '== flex' },
            'grid': { display: '== grid' },
            'block': { display: '== block' },
            'hidden': { display: '== none' },
            'relative': { position: '== relative' },
            'absolute': { position: '== absolute' },
            'fixed': { position: '== fixed' },
            
            // Spacing
            'p-0': { padding: '== 0px' },
            'm-0': { margin: '== 0px' },
            'm-auto': { margin: 'includes auto' },
            
            // Typography
            'text-lg': { fontSize: '>= 18px' },
            'text-sm': { fontSize: '<= 14px' },
            'font-bold': { fontWeight: '>= 700' },
            
            // Interactive states
            'cursor-pointer': { cursor: '== pointer' },
            'cursor-not-allowed': { cursor: '== not-allowed' },
            'opacity-50': { opacity: '== 0.5' },
            'transition-all': { transition: '!= all 0s' }
        };
    }

    function verifyClassStyles(element) {
        const computedStyle = getComputedStyle(element);
        const classList = Array.from(element.classList);
        const missingRules = [];
        
        const coreExpectations = getBuiltInExpectations();
        
        classList.forEach(className => {
            const expectation = coreExpectations[className];
            if (expectation) {
                Object.entries(expectation).forEach(([prop, expected]) => {
                    const actual = computedStyle[prop];
                    let isValid = true;
                    let expectedValue;
                    
                    if (expected.startsWith('!=')) {
                        expectedValue = expected.substring(3).trim();
                        isValid = actual !== expectedValue;
                    } else if (expected.startsWith('==')) {
                        expectedValue = expected.substring(3).trim();
                        isValid = actual === expectedValue;
                    } else if (expected.startsWith('>=')) {
                        expectedValue = parseFloat(expected.substring(3));
                        const actualValue = parseFloat(actual);
                        isValid = actualValue >= expectedValue;
                    } else if (expected.startsWith('<=')) {
                        expectedValue = parseFloat(expected.substring(3));
                        const actualValue = parseFloat(actual);
                        isValid = actualValue <= expectedValue;
                    } else if (expected.startsWith('includes')) {
                        expectedValue = expected.substring(9).trim();
                        isValid = actual.includes(expectedValue);
                    }
                    
                    if (!isValid) {
                        missingRules.push({
                            className,
                            property: prop,
                            expected: expectedValue,
                            actual,
                            severity: determineSeverity(className, prop),
                            element: element.tagName.toLowerCase()
                        });
                    }
                });
            }
        });
        
        return missingRules;
    }
    
    function determineSeverity(className, property) {
        if (className.startsWith('input') && ['padding', 'border', 'fontSize'].includes(property)) return 'critical';
        if (className.startsWith('button') && ['padding', 'backgroundColor', 'cursor'].includes(property)) return 'critical';
        if (['display', 'position', 'width', 'height'].includes(property)) return 'high';
        if (['color', 'backgroundColor', 'borderRadius'].includes(property)) return 'medium';
        return 'low';
    }

    function findCorruption() {
        console.log('🔍 Analyzing CSS Corruption and Missing Rules...');
        
        const allElements = document.querySelectorAll('*');
        const corrupted = Array.from(allElements).filter(el => {
            const s = getComputedStyle(el);
            return s.minHeight === '44px' && !['BUTTON', 'INPUT', 'A', 'LABEL', 'SELECT', 'TEXTAREA'].includes(el.tagName);
        });

        console.log(`🚨 CORRUPTION FOUND: ${corrupted.length}/${allElements.length} elements have forced 44px min-height`);
        
        const formElements = document.querySelectorAll('input, button, .input, .button');
        const elementsWithMissingRules = [];
        
        formElements.forEach(el => {
            try {
                const missingRules = verifyClassStyles(el);
                if (missingRules.length > 0) {
                    elementsWithMissingRules.push({
                        element: el,
                        tag: el.tagName,
                        classes: sanitizeForJSON(el.className),
                        missingRules: sanitizeForJSON(missingRules)
                    });
                }
            } catch (e) {
                console.warn('Error verifying class styles:', e);
            }
        });
        
        if (elementsWithMissingRules.length > 0) {
            console.log(`🎨 CSS RULES MISSING: ${elementsWithMissingRules.length} elements have classes but missing expected styles`);
            elementsWithMissingRules.forEach(item => {
                console.log(`📋 Element ${item.tag} with classes "${item.classes}":`, item.missingRules);
            });
        }
        
        if (corrupted.length > 100) {
            const sampleCorrupted = corrupted.slice(0, 5);
            console.log('📋 Sample corrupted elements:', sampleCorrupted.map(el => ({
                tag: el.tagName,
                classes: el.className,
                minHeight: getComputedStyle(el).minHeight
            })));

            findCSSRuleSource();
        }

        return {
            totalElements: allElements.length,
            corruptedElements: corrupted.length,
            corruptionRatio: (corrupted.length / allElements.length * 100).toFixed(1) + '%',
            samples: corrupted.slice(0, 10)
        };
    }

    function findCSSRuleSource() {
        console.log('🔎 Searching for corrupting CSS rules...');
        
        try {
            for (let i = 0; i < document.styleSheets.length; i++) {
                const sheet = document.styleSheets[i];
                try {
                    const rules = sheet.cssRules || sheet.rules;
                    for (let j = 0; j < rules.length; j++) {
                        const rule = rules[j];
                        if (rule.style && rule.style.minHeight === '44px') {
                            console.log(`🎯 FOUND CORRUPTING RULE: ${rule.selectorText} { min-height: 44px }`);
                            console.log(`📄 From stylesheet: ${sheet.href || 'inline'}`);
                            
                            if (rule.selectorText.includes('*') || rule.selectorText === '*') {
                                console.log('💥 UNIVERSAL SELECTOR CORRUPTION DETECTED!');
                            }
                        }
                    }
                } catch (e) {
                    console.log(`⚠️ Cannot access stylesheet ${i} (CORS): ${sheet.href}`);
                }
            }
        } catch (e) {
            console.log('❌ Error scanning stylesheets:', e.message);
        }
    }

    function verifyCSS() {
        console.log('🔧 Verifying CSS Compilation...');
        
        const verification = {
            cssVariables: {},
            extendsCompiled: false,
            mixinsCompiled: false,
            issues: []
        };

        const root = getComputedStyle(document.documentElement);
        const criticalVars = [
            '--header-height', '--sidebar-width', '--color-text-primary', 
            '--space-4', '--radius-lg', '--shadow-md'
        ];

        criticalVars.forEach(varName => {
            const value = root.getPropertyValue(varName);
            verification.cssVariables[varName] = value || 'MISSING';
            if (!value) verification.issues.push(`Missing CSS variable: ${varName}`);
        });

        const sampleElements = document.querySelectorAll('.app-shell, .app-header, .form-field');
        const hasCompiledClasses = Array.from(sampleElements).some(el => {
            return !Array.from(el.classList).some(cls => cls.startsWith('@'));
        });
        verification.extendsCompiled = hasCompiledClasses;

        const invalidClasses = [];
        document.querySelectorAll('*').forEach(el => {
            Array.from(el.classList).forEach(cls => {
                if (cls.startsWith('@') || cls.includes('extend-') || cls.includes('mixin-')) {
                    invalidClasses.push(cls);
                }
            });
        });

        if (invalidClasses.length > 0) {
            verification.issues.push(`Uncompiled SCSS classes found: ${[...new Set(invalidClasses)].join(', ')}`);
        }

        console.log('✅ CSS Verification Results:', verification);
        return verification;
    }

    function analyze() {
        const allElements = document.querySelectorAll('*');
        const issues = {
            forcedHeight: 0,
            brokenFlex: 0,
            invalidCSS: 0,
            total: 0
        };

        Array.from(allElements).forEach(el => {
            Object.entries(CRITICAL_ISSUES).forEach(([type, checkFn]) => {
                if (checkFn(el)) {
                    issues[type]++;
                    issues.total++;
                }
            });
        });

        const score = Math.max(0, 100 - (issues.total / allElements.length * 200));
        const status = score >= 85 ? 'excellent' : score >= 70 ? 'good' : score >= 50 ? 'fair' : 'critical';

        const report = {
            score: Math.round(score),
            status,
            totalElements: allElements.length,
            issues,
            actionable: generateActionableInsights(issues, allElements.length)
        };

        console.log(`🔍 Page Analysis - Score: ${report.score}% (${report.status})`);
        console.log('📊 Critical Issues:', issues);
        console.log('🎯 Actionable Insights:', report.actionable);

        return report;
    }

    function generateActionableInsights(issues, totalElements) {
        const insights = [];

        if (issues.forcedHeight > totalElements * 0.5) {
            insights.push({
                priority: 'CRITICAL',
                issue: 'Universal min-height corruption',
                action: 'Run RRDebug.findCorruption() to identify the CSS rule causing mass corruption',
                impact: `${issues.forcedHeight} elements affected`
            });
        }

        if (issues.brokenFlex > 10) {
            insights.push({
                priority: 'HIGH',
                issue: 'Broken flex utilities',
                action: 'Check if responsive utilities are forcing display:block over flex',
                impact: `${issues.brokenFlex} flex elements broken`
            });
        }

        if (issues.invalidCSS > 0) {
            insights.push({
                priority: 'HIGH',
                issue: 'CSS compilation failure',
                action: 'Run RRDebug.verifyCSS() and rebuild SCSS files',
                impact: `${issues.invalidCSS} elements with invalid classes`
            });
        }

        return insights;
    }

    function analyzeComponent(selector) {
        const element = document.querySelector(selector);
        if (!element) {
            console.warn(`Component not found: ${selector}`);
            return null;
        }

        const children = element.querySelectorAll('*');
        const componentIssues = Object.entries(CRITICAL_ISSUES).reduce((acc, [type, checkFn]) => {
            acc[type] = checkFn(element) ? 1 : 0;
            return acc;
        }, {});

        const childIssues = Array.from(children).reduce((acc, child) => {
            Object.entries(CRITICAL_ISSUES).forEach(([type, checkFn]) => {
                if (checkFn(child)) acc[type] = (acc[type] || 0) + 1;
            });
            return acc;
        }, {});

        const totalIssues = Object.values({...componentIssues, ...childIssues}).reduce((a, b) => a + b, 0);
        const health = Math.max(0, 100 - (totalIssues * 10));

        const result = {
            selector,
            health: Math.round(health),
            totalElements: children.length + 1,
            componentIssues,
            childIssues,
            status: health >= 85 ? 'excellent' : health >= 70 ? 'good' : health >= 50 ? 'fair' : 'critical'
        };

        console.log(`🔍 Component: ${selector} - Health: ${result.health}% (${result.status})`);
        if (totalIssues > 0) {
            console.log('🚨 Issues found:', {...componentIssues, ...childIssues});
        }

        return result;
    }

    function scan(selector = '*', options = {}) {
        const { limit = 10 } = options;
        const elements = document.querySelectorAll(selector);
        
        const results = Array.from(elements).slice(0, limit).map(el => {
            const issues = Object.keys(CRITICAL_ISSUES).filter(type => CRITICAL_ISSUES[type](el));
            return {
                element: `${el.tagName.toLowerCase()}${el.id ? '#' + el.id : ''}${el.className ? '.' + el.className.split(' ')[0] : ''}`,
                issues: issues.length > 0 ? issues.join(', ') : 'none',
                minHeight: getComputedStyle(el).minHeight,
                display: getComputedStyle(el).display
            };
        });

        console.log(`🔍 Scanned ${elements.length} elements matching "${selector}" (showing ${Math.min(limit, elements.length)})`);
        console.table(results);
        return results;
    }

    function captureConsoleErrors(options = {}) {
        const { maxLines = 3, includeStack = false } = options;
        
        if (!window.__RR_CONSOLE_ERRORS) {
            window.__RR_CONSOLE_ERRORS = [];
            
            const originalError = console.error;
            console.error = function(...args) {
                const message = args.map(arg => 
                    typeof arg === 'object' ? JSON.stringify(arg, null, 2) : String(arg)
                ).join(' ');
                
                // Skip Serilog and other problematic console messages
                if (message.includes('%cserilog') || 
                    message.includes('color:white;background') ||
                    message.includes('�') ||
                    message.includes('[DEBUG]') ||
                    message.includes('[INFO]')) {
                    return originalError.apply(console, args);
                }
                
                const errorEntry = {
                    timestamp: new Date().toISOString(),
                    message: sanitizeForJSON(message),
                    stack: includeStack ? new Error().stack : null
                };
                
                window.__RR_CONSOLE_ERRORS.push(errorEntry);
                return originalError.apply(console, args);
            };
            
            window.addEventListener('error', (e) => {
                const message = `${e.message} at ${e.filename}:${e.lineno}:${e.colno}`;
                if (!message.includes('serilog') && !message.includes('�')) {
                    window.__RR_CONSOLE_ERRORS.push({
                        timestamp: new Date().toISOString(),
                        message: sanitizeForJSON(message),
                        stack: e.error?.stack || null
                    });
                }
            });
        }
        
        return window.__RR_CONSOLE_ERRORS.map(error => ({
            timestamp: error.timestamp,
            summary: sanitizeForJSON(error.message.split('\n').slice(0, maxLines).join('\n')),
            hasMore: error.message.split('\n').length > maxLines
        }));
    }
    
    function clearConsoleErrors() {
        if (window.__RR_CONSOLE_ERRORS) {
            window.__RR_CONSOLE_ERRORS.length = 0;
        }
        console.clear();
        return "Console errors cleared";
    }
    

    function getAIReport() {
        const pageAnalysis = analyze();
        const cssVerification = verifyCSS();
        const consoleErrors = captureConsoleErrors();
        const responsiveResults = testResponsiveLayout();
        
        const allElements = document.querySelectorAll('*');
        const buttonElements = document.querySelectorAll('button, .btn, [role="button"]');
        const inputElements = document.querySelectorAll('input, textarea, select');
        const modalElements = document.querySelectorAll('.modal, [role="dialog"]');
        
        const report = {
            timestamp: new Date().toISOString(),
            page: {
                url: window.location.href,
                title: document.title,
                elementCounts: {
                    total: allElements.length,
                    buttons: buttonElements.length,
                    inputs: inputElements.length,
                    modals: modalElements.length
                }
            },
            health: {
                score: pageAnalysis.score,
                status: pageAnalysis.status,
                issues: pageAnalysis.issues,
                insights: pageAnalysis.actionable,
                isHealthy: pageAnalysis.score >= 70
            },
            css: {
                compilation: cssVerification,
                corruption: pageAnalysis.issues.forcedHeight > 100 ? findCorruption() : null,
                variables: debugUtils.css(),
                customProperties: Object.keys(debugUtils.css()).length
            },
            responsive: {
                tested: responsiveResults.summary.totalBreakpoints,
                issues: responsiveResults.summary.totalIssues,
                brokenBreakpoints: responsiveResults.summary.brokenBreakpoints,
                score: responsiveResults.responsive.score,
                status: responsiveResults.responsive.status,
                criticalBreakpoints: responsiveResults.responsive.criticalBreakpoints,
                worstDevice: responsiveResults.summary.worstBreakpoint?.device,
                bestDevice: responsiveResults.summary.bestBreakpoint?.device,
                summary: `${responsiveResults.summary.totalIssues} responsive issues across ${responsiveResults.summary.totalBreakpoints} breakpoints`
            },
            errors: {
                count: consoleErrors.length,
                recent: consoleErrors.slice(-5),
                hasErrors: consoleErrors.length > 0,
                summary: consoleErrors.length > 0 ? 
                    consoleErrors.map(e => e.summary).join(' | ') : 'No errors detected'
            },
            performance: {
                loadTime: performance.timing ? 
                    performance.timing.loadEventEnd - performance.timing.navigationStart : null,
                domElements: allElements.length,
                renderStart: performance.timing ? performance.timing.domContentLoadedEventStart : null,
                memoryUsage: performance.memory ? {
                    used: Math.round(performance.memory.usedJSHeapSize / 1024 / 1024) + 'MB',
                    total: Math.round(performance.memory.totalJSHeapSize / 1024 / 1024) + 'MB'
                } : null
            },
            components: {
                buttons: Array.from(buttonElements).slice(0, 5).map(btn => ({
                    text: sanitizeForJSON(btn.textContent?.trim() || btn.getAttribute('aria-label') || 'No text'),
                    classes: sanitizeForJSON(btn.className),
                    disabled: btn.disabled,
                    issues: Object.keys(CRITICAL_ISSUES).filter(type => CRITICAL_ISSUES[type](btn))
                })),
                inputs: Array.from(inputElements).slice(0, 5).map(input => ({
                    type: sanitizeForJSON(input.type || input.tagName.toLowerCase()),
                    placeholder: sanitizeForJSON(input.placeholder),
                    required: input.required,
                    classes: sanitizeForJSON(input.className),
                    issues: Object.keys(CRITICAL_ISSUES).filter(type => CRITICAL_ISSUES[type](input))
                })),
                modals: Array.from(modalElements).map(modal => ({
                    visible: getComputedStyle(modal).display !== 'none',
                    classes: sanitizeForJSON(modal.className),
                    issues: Object.keys(CRITICAL_ISSUES).filter(type => CRITICAL_ISSUES[type](modal))
                }))
            },
            layout: {
                viewport: {
                    width: window.innerWidth,
                    height: window.innerHeight
                },
                scrollable: document.body.scrollHeight > window.innerHeight,
                zeroSizedElements: Array.from(allElements).filter(el => {
                    const rect = el.getBoundingClientRect();
                    return rect.width === 0 || rect.height === 0;
                }).length,
                highZIndexElements: Array.from(allElements).filter(el => {
                    const zIndex = parseInt(getComputedStyle(el).zIndex);
                    return zIndex > 1000;
                }).length
            },
            actionable: {
                criticalIssues: pageAnalysis.actionable?.filter(insight => insight.priority === 'CRITICAL') || [],
                quickFixes: [
                    ...(pageAnalysis.issues.invalidCSS > 0 ? ['Run RRDebug.verifyCSS() and rebuild SCSS'] : []),
                    ...(responsiveResults.summary.totalIssues > 0 ? ['Run RRDebug.responsive() for detailed breakpoint analysis'] : [])
                ],
                recommendations: [
                    ...(pageAnalysis.score < 70 ? [
                        'Page health below 70% - investigate CSS corruption',
                        'Check console errors for JavaScript issues'
                    ] : []),
                    ...(responsiveResults.summary.totalIssues > 10 ? [
                        `Critical: ${responsiveResults.summary.totalIssues} responsive issues found`,
                        `Fix ${responsiveResults.summary.brokenBreakpoints} broken breakpoints`,
                        `Worst device: ${responsiveResults.summary.worstBreakpoint?.device} (${responsiveResults.summary.worstBreakpoint?.issues?.length} issues)`
                    ] : responsiveResults.summary.totalIssues > 0 ? [
                        `Minor responsive issues on ${responsiveResults.summary.brokenBreakpoints} breakpoints`
                    ] : []),
                    ...(pageAnalysis.score >= 70 && responsiveResults.summary.totalIssues === 0 ? 
                        ['Page health and responsive design are excellent'] : [])
                ]
            }
        };
        
        window.__RR_AI_REPORT = sanitizeForJSON(report);
        
        console.log('🤖 Comprehensive AI Report Generated:', report);
        console.log('📊 Quick Summary:', {
            health: `${report.health.score}% (${report.health.status})`,
            errors: report.errors.count,
            elements: report.page.elementCounts.total,
            performance: report.performance.loadTime ? `${report.performance.loadTime}ms` : 'N/A'
        });
        
        return sanitizeForJSON(report);
    }

    const debugMode = { enabled: false };
    
    const debugUtils = {
        enableDebugMode: () => { debugMode.enabled = true; localStorage.setItem('debug', 'true'); },
        log: (msg) => debugMode.enabled && console.log(`🐛 ${msg}`),
        time: (label) => console.time(label),
        timeEnd: (label) => console.timeEnd(label),
        group: (label) => console.group(label),
        groupEnd: () => console.groupEnd(),
        inspect: (selector) => {
            const el = document.querySelector(selector);
            if (!el) return null;
            const styles = getComputedStyle(el);
            return {
                element: el,
                css: { display: styles.display, position: styles.position, width: styles.width, height: styles.height },
                boxModel: { margin: styles.margin, padding: styles.padding, border: styles.border },
                computed: styles
            };
        },
        highlight: (selector, color = '#ff0000') => {
            const el = document.querySelector(selector);
            if (!el) return;
            const original = el.style.outline;
            el.style.outline = `2px solid ${color}`;
            setTimeout(() => el.style.outline = original, 3000);
        },
        layout: (selector) => {
            const el = document.querySelector(selector);
            if (!el) return null;
            const rect = el.getBoundingClientRect();
            const styles = getComputedStyle(el);
            return {
                size: { width: rect.width, height: rect.height },
                position: { x: rect.x, y: rect.y },
                issues: {
                    zeroSized: rect.width === 0 || rect.height === 0,
                    smallFont: parseFloat(styles.fontSize) < 12,
                    highZIndex: parseInt(styles.zIndex) > 1000
                }
            };
        },
        perf: (label, fn) => {
            const start = performance.now();
            const result = fn();
            const end = performance.now();
            console.log(`⚡ ${label}: ${(end - start).toFixed(2)}ms`);
            return result;
        },
        watch: () => {
            const observer = new MutationObserver(mutations => {
                mutations.forEach(m => console.log('🔄 DOM changed:', m.type, m.target));
            });
            observer.observe(document.body, { childList: true, subtree: true });
            return observer;
        },
        css: (el = document.documentElement) => {
            const styles = getComputedStyle(el);
            const vars = {};
            for (let prop of Array.from(styles)) {
                if (prop.startsWith('--')) vars[prop] = styles.getPropertyValue(prop);
            }
            return vars;
        }
    };

    const visualDebug = (selector = 'body') => {
        const element = document.querySelector(selector);
        if (!element) return { error: `Element not found: ${selector}` };
        
        const children = element.querySelectorAll('*');
        const issues = [];
        const layoutIssues = [];
        const accessibilityIssues = [];
        const performanceIssues = [];
        
        Array.from(children).forEach(el => {
            const rect = el.getBoundingClientRect();
            const styles = getComputedStyle(el);
            const tag = el.tagName.toLowerCase();
            
            if (rect.width === 0 || rect.height === 0) {
                issues.push({ type: 'zero-sized', element: tag, selector: el.className });
            }
            if (parseFloat(styles.fontSize) < 11 && el.textContent?.trim()) {
                const text = el.textContent || '';
                const safeText = text.replace(/[\uD800-\uDFFF]/g, '').substring(0, 20);
                issues.push({ type: 'tiny-font', element: tag, size: styles.fontSize, text: sanitizeForJSON(safeText) });
            }
            if (styles.color === styles.backgroundColor && styles.color !== 'transparent' && styles.color !== 'rgba(0, 0, 0, 0)') {
                issues.push({ type: 'invisible-text', element: tag, color: styles.color });
            }
            
            if (rect.width > window.innerWidth * 1.1 && styles.overflow === 'visible') {
                layoutIssues.push({ type: 'horizontal-overflow', element: tag, width: Math.round(rect.width) });
            }
            if (styles.position === 'fixed' && rect.top < 0) {
                layoutIssues.push({ type: 'fixed-offscreen', element: tag, top: Math.round(rect.top) });
            }
            if (parseInt(styles.zIndex) > 9999) {
                layoutIssues.push({ type: 'excessive-z-index', element: tag, zIndex: styles.zIndex });
            }
            
            if (tag === 'button' && !el.textContent?.trim() && !el.getAttribute('aria-label')) {
                accessibilityIssues.push({ type: 'button-no-text', element: tag, id: el.id });
            }
            if (tag === 'img' && !el.getAttribute('alt')) {
                accessibilityIssues.push({ type: 'img-no-alt', element: tag, src: el.src?.substring(0, 30) });
            }
            if (parseFloat(styles.fontSize) < 16 && ['button', 'a', 'input'].includes(tag)) {
                accessibilityIssues.push({ type: 'touch-target-small', element: tag, fontSize: styles.fontSize });
            }
              
            if (el.children.length > 100) {
                performanceIssues.push({ type: 'deep-nesting', element: tag, children: el.children.length });
            }
            if (styles.boxShadow.split(',').length > 3) {
                performanceIssues.push({ type: 'multiple-shadows', element: tag, shadows: styles.boxShadow.split(',').length });
            }
        });
        
        issues.concat(layoutIssues).slice(0, 5).forEach((issue, i) => {
            const elements = document.querySelectorAll(issue.selector ? `.${issue.selector.split(' ')[0]}` : issue.element);
            const colors = ['red', 'orange', 'yellow', 'purple', 'blue'];
            elements.forEach(el => {
                el.style.outline = `2px solid ${colors[i % colors.length]}`;
                setTimeout(() => el.style.outline = '', 3000);
            });
        });
        
        const totalIssues = issues.length + layoutIssues.length + accessibilityIssues.length + performanceIssues.length;
        const severity = totalIssues > 10 ? 'critical' : totalIssues > 5 ? 'high' : totalIssues > 2 ? 'medium' : 'low';
        
        return {
            selector,
            totalElements: children.length,
            severity,
            score: Math.max(0, 100 - (totalIssues * 5)),
            visual: { count: issues.length, issues: issues.slice(0, 5) },
            layout: { count: layoutIssues.length, issues: layoutIssues.slice(0, 5) },
            accessibility: { count: accessibilityIssues.length, issues: accessibilityIssues.slice(0, 5) },
            performance: { count: performanceIssues.length, issues: performanceIssues.slice(0, 5) },
            summary: `Visual Analysis: ${totalIssues} issues found (${severity} severity) - ${issues.length} visual, ${layoutIssues.length} layout, ${accessibilityIssues.length} a11y, ${performanceIssues.length} perf`,
            actionable: totalIssues > 0 ? [
                issues.length > 0 ? `Fix ${issues.length} visual rendering issues` : null,
                layoutIssues.length > 0 ? `Resolve ${layoutIssues.length} layout problems` : null,
                accessibilityIssues.length > 0 ? `Address ${accessibilityIssues.length} accessibility violations` : null,
                performanceIssues.length > 0 ? `Optimize ${performanceIssues.length} performance bottlenecks` : null
            ].filter(Boolean) : ['No visual issues detected']
        };
    };

    const checkCSS = (selector = '*') => {
        const allElements = document.querySelectorAll(selector);
        const corruption = [];
        const missingRules = [];
        const invalidElements = [];
        let corruptedCount = 0;
        
        // Limit elements for performance, but allow full analysis of specific selectors
        const elementsToCheck = selector === '*' ? Array.from(allElements).slice(0, 100) : Array.from(allElements);
        
        for (const el of elementsToCheck) {
            const styles = getComputedStyle(el);
            const classes = Array.from(el.classList);
            
            if (styles.minHeight === '44px' && !['BUTTON', 'INPUT', 'A', 'LABEL', 'SELECT', 'TEXTAREA'].includes(el.tagName)) {
                corruptedCount++;
                if (corruption.length < 3) {
                    corruption.push({ tag: el.tagName, classes: sanitizeForJSON(el.className) });
                }
            }
            
            try {
                const elementMissingRules = verifyClassStyles(el);
                if (elementMissingRules.length > 0) {
                    missingRules.push({
                        element: sanitizeForJSON(`${el.tagName.toLowerCase()}${el.id ? '#' + el.id : ''}`),
                        classes: sanitizeForJSON(el.className),
                        missing: sanitizeForJSON(elementMissingRules.slice(0, 3))
                    });
                }
            } catch (e) {
                console.warn('Error verifying class styles in checkCSS:', e);
            }
            
            const invalidClasses = classes.filter(cls => 
                cls.startsWith('@') || cls.includes('undefined') || cls.includes('NaN') || cls.includes('extend-')
            );
            if (invalidClasses.length > 0) {
                invalidElements.push({
                    tag: el.tagName,
                    invalidClasses: sanitizeForJSON(invalidClasses),
                    selector: sanitizeForJSON(el.className)
                });
            }
        }
        
        const corruptionRatio = (corruptedCount / allElements.length * 100).toFixed(1) + '%';
        const hasIssues = corruptedCount > 0 || missingRules.length > 0 || invalidElements.length > 0;
        
        return {
            selector: selector,
            status: hasIssues ? 'issues-detected' : 'healthy',
            corruption: {
                ratio: corruptionRatio,
                count: corruptedCount,
                samples: corruption,
                isCritical: corruptedCount > allElements.length * 0.1
            },
            missingRules: {
                count: missingRules.length,
                elements: missingRules.slice(0, 5),
                criticalCount: missingRules.filter(mr => mr.missing.some(rule => rule.severity === 'critical')).length
            },
            invalidClasses: {
                count: invalidElements.length,
                elements: invalidElements.slice(0, 3)
            },
            summary: `CSS Health for '${selector}': ${hasIssues ? 'Issues Found' : 'Healthy'} - ${corruptedCount} corrupted, ${missingRules.length} missing rules, ${invalidElements.length} invalid classes`
        };
    };

    const checkHealth = (selector = '*') => {
        const elements = document.querySelectorAll(selector);
        const analysis = analyzeElements(elements);
        return {
            selector: selector,
            score: analysis.score,
            status: analysis.status,
            isHealthy: analysis.score >= 70,
            criticalIssues: analysis.actionable?.filter(a => a.priority === 'CRITICAL') || [],
            elementCount: elements.length,
            summary: `Health for '${selector}': ${analysis.score}% (${analysis.status})`
        };
    };

    // Helper function for element-specific analysis
    const analyzeElements = (elements) => {
        const issues = {
            forcedHeight: 0,
            brokenFlex: 0,
            invalidCSS: 0,
            total: 0
        };

        Array.from(elements).forEach(el => {
            Object.entries(CRITICAL_ISSUES).forEach(([type, checkFn]) => {
                if (checkFn(el)) {
                    issues[type]++;
                    issues.total++;
                }
            });
        });

        const score = Math.max(0, 100 - (issues.total / elements.length * 200));
        const status = score >= 85 ? 'excellent' : score >= 70 ? 'good' : score >= 50 ? 'fair' : 'critical';

        return {
            score: Math.round(score),
            status,
            totalElements: elements.length,
            issues,
            actionable: generateActionableInsights(issues, elements.length)
        };
    };

    // Comprehensive device testing matrix for maximum coverage
    const responsiveBreakpoints = {
        'mobile-xs': { width: 320, height: 568, name: 'iPhone 5/SE' },
        'mobile-portrait': { width: 375, height: 667, name: 'iPhone 6/7/8' },
        'mobile-large': { width: 414, height: 896, name: 'iPhone 11/12/13' },
        'mobile-landscape': { width: 667, height: 375, name: 'iPhone Landscape' },
        'mobile-large-landscape': { width: 896, height: 414, name: 'iPhone Large Landscape' },
        'tablet-small': { width: 600, height: 960, name: 'Small Tablet' },
        'tablet-portrait': { width: 768, height: 1024, name: 'iPad Portrait' },
        'tablet-landscape': { width: 1024, height: 768, name: 'iPad Landscape' },
        'tablet-large': { width: 834, height: 1194, name: 'iPad Air' },
        'laptop-small': { width: 1280, height: 720, name: 'Small Laptop' },
        'laptop': { width: 1366, height: 768, name: 'Standard Laptop' },
        'desktop': { width: 1920, height: 1080, name: 'Full HD Desktop' },
        'desktop-large': { width: 2560, height: 1440, name: 'QHD Desktop' },
        'desktop-ultrawide': { width: 3440, height: 1440, name: 'Ultrawide Monitor' },
        'desktop-4k': { width: 3840, height: 2160, name: '4K Desktop' }
    };

    function testResponsiveLayout() {
        const originalViewport = { width: window.innerWidth, height: window.innerHeight };
        const results = [];
        
        Object.entries(responsiveBreakpoints).forEach(([breakpoint, dimensions]) => {
            const mediaQuery = `(max-width: ${dimensions.width}px)`;
            const matchesQuery = window.matchMedia(mediaQuery).matches;
            const layoutIssues = analyzeLayoutAtBreakpoint(dimensions, breakpoint);
            
            results.push({
                breakpoint,
                device: dimensions.name,
                dimensions,
                matchesCurrentQuery: matchesQuery,
                issues: layoutIssues,
                score: Math.max(0, 100 - (layoutIssues.length * 10)),
                status: layoutIssues.length === 0 ? 'perfect' : 
                        layoutIssues.length <= 2 ? 'good' : 
                        layoutIssues.length <= 5 ? 'issues' : 'broken'
            });
        });
        
        const criticalIssues = results.filter(r => r.status === 'broken');
        const totalIssues = results.reduce((sum, r) => sum + r.issues.length, 0);
        
        return {
            currentViewport: originalViewport,
            breakpointResults: results,
            summary: {
                totalBreakpoints: results.length,
                brokenBreakpoints: criticalIssues.length,
                totalIssues,
                worstBreakpoint: results.reduce((worst, current) => 
                    current.issues.length > worst.issues.length ? current : worst
                ),
                bestBreakpoint: results.reduce((best, current) => 
                    current.issues.length < best.issues.length ? current : best
                )
            },
            responsive: {
                score: Math.max(0, 100 - (totalIssues * 2)),
                status: totalIssues === 0 ? 'excellent' : 
                        totalIssues <= 5 ? 'good' : 
                        totalIssues <= 15 ? 'needs-work' : 'critical',
                criticalBreakpoints: criticalIssues.map(r => r.breakpoint)
            }
        };
    }

    function analyzeLayoutAtBreakpoint(dimensions, breakpoint) {
        const issues = [];
        const elements = document.querySelectorAll('*');
        
        elements.forEach(el => {
            const rect = el.getBoundingClientRect();
            const styles = getComputedStyle(el);
            
            if (rect.width > dimensions.width + 20) {
                issues.push({
                    type: 'horizontal-overflow',
                    element: el.tagName.toLowerCase(),
                    width: Math.round(rect.width),
                    viewport: dimensions.width,
                    severity: breakpoint.includes('mobile') ? 'critical' : 'high'
                });
            }
            
            if (breakpoint.includes('mobile') && parseFloat(styles.fontSize) < 14 && el.textContent?.trim()) {
                issues.push({
                    type: 'text-too-small-mobile',
                    element: el.tagName.toLowerCase(),
                    fontSize: styles.fontSize,
                    severity: 'medium'
                });
            }
            
            if (breakpoint.includes('mobile') && ['button', 'a', 'input'].includes(el.tagName.toLowerCase())) {
                if (rect.width < 44 || rect.height < 44) {
                    issues.push({
                        type: 'touch-target-small',
                        element: el.tagName.toLowerCase(),
                        size: `${Math.round(rect.width)}x${Math.round(rect.height)}`,
                        severity: 'high'
                    });
                }
            }
            
            if (breakpoint.includes('mobile') && styles.position === 'fixed') {
                if (rect.width > dimensions.width || rect.height > dimensions.height) {
                    issues.push({
                        type: 'fixed-element-overflow',
                        element: el.tagName.toLowerCase(),
                        severity: 'high'
                    });
                }
            }
            
            if (el.tagName.toLowerCase() === 'table' && breakpoint.includes('mobile')) {
                if (rect.width > dimensions.width) {
                    issues.push({
                        type: 'table-not-responsive',
                        element: 'table',
                        width: Math.round(rect.width),
                        severity: 'medium'
                    });
                }
            }
        });
        
        const responsiveClasses = ['sm:', 'md:', 'lg:', 'xl:', '2xl:', 'mobile-', 'tablet-', 'desktop-'];
        elements.forEach(el => {
            const classes = Array.from(el.classList);
            const hasResponsiveClasses = classes.some(cls => 
                responsiveClasses.some(prefix => cls.includes(prefix))
            );
            
            if (hasResponsiveClasses) {
                const computedDisplay = getComputedStyle(el).display;
                if (computedDisplay === 'none' && breakpoint.includes('mobile')) {
                    issues.push({
                        type: 'responsive-hidden',
                        element: el.tagName.toLowerCase(),
                        classes: classes.filter(cls => responsiveClasses.some(prefix => cls.includes(prefix))).join(' '),
                        severity: 'low'
                    });
                }
            }
        });
        
        return issues.slice(0, 10);
    }

    const checkPerformance = (selector = '*') => {
        const timing = performance.timing;
        const memory = performance.memory;
        const responsiveResults = testResponsiveLayout();
        const elements = document.querySelectorAll(selector);
        const images = document.querySelectorAll(`${selector} img, img${selector.startsWith('.') || selector.startsWith('#') ? selector : ''}`);
        
        return {
            selector: selector,
            loadTime: timing ? timing.loadEventEnd - timing.navigationStart : null,
            domReady: timing ? timing.domContentLoadedEventEnd - timing.navigationStart : null,
            renderTime: timing ? timing.domInteractive - timing.navigationStart : null,
            memory: memory ? {
                used: Math.round(memory.usedJSHeapSize / 1024 / 1024) + 'MB',
                total: Math.round(memory.totalJSHeapSize / 1024 / 1024) + 'MB',
                limit: Math.round(memory.jsHeapSizeLimit / 1024 / 1024) + 'MB'
            } : null,
            elements: elements.length,
            images: images.length,
            status: (timing?.loadEventEnd - timing?.navigationStart) > 3000 ? 'slow' : 'acceptable',
            responsive: responsiveResults,
            summary: `Performance for '${selector}': ${(timing?.loadEventEnd - timing?.navigationStart) || 'N/A'}ms load, ${elements.length} elements, ${images.length} images, ${responsiveResults.summary.totalIssues} responsive issues`
        };
    };

    const getLogs = () => {
        const errors = captureConsoleErrors({ maxLines: 2 });
        return {
            errorCount: errors.length,
            recentErrors: errors.slice(-3),
            errorSummary: errors.length > 0 ? errors.map(e => e.summary.split('\n')[0]).join(' | ') : 'No errors',
            hasErrors: errors.length > 0,
            timestamp: new Date().toISOString()
        };
    };

    const testResponsive = async (options = {}) => {
        console.log('📱 Testing responsive layout across all screen sizes...');
        const results = testResponsiveLayout();
        
        results.breakpointResults.forEach(result => {
            const status = result.status === 'perfect' ? '✅' : 
                          result.status === 'good' ? '⚠️' : 
                          result.status === 'issues' ? '🔶' : '❌';
            
            console.log(`${status} ${result.device} (${result.dimensions.width}x${result.dimensions.height}): ${result.issues.length} issues`);
            
            if (result.issues.length > 0) {
                result.issues.forEach(issue => {
                    console.log(`  • ${issue.type}: ${issue.element} (${issue.severity})`);
                });
            }
        });
        
        console.log(`\n📊 Responsive Summary:`);
        console.log(`• ${results.summary.totalBreakpoints} breakpoints tested`);
        console.log(`• ${results.summary.totalIssues} total issues found`);
        console.log(`• ${results.summary.brokenBreakpoints} broken breakpoints`);
        console.log(`• Worst: ${results.summary.worstBreakpoint.device} (${results.summary.worstBreakpoint.issues.length} issues)`);
        console.log(`• Best: ${results.summary.bestBreakpoint.device} (${results.summary.bestBreakpoint.issues.length} issues)`);
        
        return results;
    };

    return {
        visual: visualDebug,
        css: checkCSS,
        health: checkHealth,
        performance: checkPerformance,
        logs: getLogs,
        report: getAIReport,
        responsive: testResponsive,
        sanitizeForJSON: sanitizeForJSON,
        safeStringify: safeStringify
    };
})();

window.RRDebug = RRDebugAPI;

if (window.location.search.includes('debug=true') || window.location.search.includes('qa=true')) {
    setTimeout(async () => {
        console.log('🤖 Auto-running streamlined QA analysis...');
        
        try {
            const health = window.RRDebug.health();
            const logs = window.RRDebug.logs();
            
            console.log('🏥 Health:', health.summary);
            console.log('📝 Logs:', logs.errorSummary);
            
            setTimeout(() => {
                try {
                    const report = window.RRDebug.report();
                    console.log('✅ Complete report ready:', report);
                    console.log('📊 Access via: window.__RR_AI_REPORT');
                } catch (e) {
                    console.warn('Report generation failed:', e);
                    window.__RR_AI_REPORT = { error: 'REPORT_GENERATION_FAILED', message: e.message };
                }
            }, 2000);
        } catch (e) {
            console.warn('QA analysis failed:', e);
        }
    }, 1000);
}

export default RRDebugAPI;