const RRDebugAPI = (() => {
    const VERSION = '1.0.0';
    
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
    
    // CRITICAL ISSUES ONLY - Real problems that need fixing, not normal SPA patterns
    const CRITICAL_ISSUES = {
        // Layout & Display Issues - Only truly broken elements
        brokenRender: el => {
            const s = getComputedStyle(el);
            const rect = el.getBoundingClientRect();
            // Only flag visible elements that should have content but are broken
            return s.display !== 'none' && s.visibility !== 'hidden' && 
                   rect.width === 0 && rect.height === 0 && 
                   el.textContent?.trim() && el.textContent.length > 5;
        },
        
        criticalOverflow: el => {
            const rect = el.getBoundingClientRect();
            const s = getComputedStyle(el);
            // Only flag elements that break layout AND are visible
            return s.display !== 'none' && s.visibility !== 'hidden' && 
                   rect.width > window.innerWidth * 1.5 && s.overflow === 'visible';
        },
        
        brokenForm: el => {
            // Forms that can't be used
            return ['INPUT', 'BUTTON', 'SELECT', 'TEXTAREA'].includes(el.tagName) &&
                   !el.disabled && getComputedStyle(el).display !== 'none' &&
                   (getComputedStyle(el).opacity === '0' || 
                    (el.getBoundingClientRect().width === 0 && el.getBoundingClientRect().height === 0));
        },
        
        brokenInteraction: el => {
            // Interactive elements that can't be clicked
            const rect = el.getBoundingClientRect();
            const s = getComputedStyle(el);
            const isInteractive = ['BUTTON', 'A'].includes(el.tagName) || el.onclick;
            return isInteractive && s.display !== 'none' && 
                   (rect.width < 10 || rect.height < 10 || s.pointerEvents === 'none');
        },
        
        cssCorruption: el => {
            const s = getComputedStyle(el);
            // Only flag truly corrupted CSS rules
            return s.minHeight === '44px' && 
                   !['BUTTON', 'INPUT', 'A', 'LABEL', 'SELECT', 'TEXTAREA'].includes(el.tagName);
        },
        
        brokenLayout: el => {
            const rect = el.getBoundingClientRect();
            const s = getComputedStyle(el);
            // Elements with impossible dimensions
            return rect.height > window.innerHeight * 10 || 
                   (s.display === 'grid' && !s.gridTemplateColumns && !s.gridTemplateRows && el.children.length > 5);
        },
        
        invalidCSS: el => {
            // Only flag truly invalid CSS classes that break rendering
            return Array.from(el.classList).some(cls => 
                cls.includes('undefined') || cls.includes('NaN') || cls.includes('null'));
        },
        
        invisibleText: el => {
            const s = getComputedStyle(el);
            const hasText = el.textContent?.trim();
            // Only flag text that is truly invisible but should be visible
            return hasText && hasText.length > 3 && s.display !== 'none' &&
                   (s.color === s.backgroundColor && s.color !== 'transparent' ||
                    s.color === 'transparent' && s.backgroundColor !== 'transparent');
        },
        
        // Accessibility Issues - Only critical ones
        missingAltText: el => {
            return el.tagName === 'IMG' && !el.alt && !el.getAttribute('aria-label') && 
                   getComputedStyle(el).display !== 'none';
        },
        
        emptyButton: el => {
            return el.tagName === 'BUTTON' && !el.textContent?.trim() && 
                   !el.getAttribute('aria-label') && !el.querySelector('img[alt]') &&
                   getComputedStyle(el).display !== 'none';
        },
        
        // Critical JavaScript Issues
        jsErrors: () => {
            return window.__RR_CONSOLE_ERRORS?.length > 0;
        },
        
        // Critical Framework Issues  
        blazorError: el => {
            return el.getAttribute('b-error') || el.className.includes('blazor-error-boundary');
        },
        
        // Duplicated IDs break functionality
        duplicateId: el => {
            if (!el.id) return false;
            try {
                const escapedId = CSS.escape(el.id);
                return document.querySelectorAll(`#${escapedId}`).length > 1;
            } catch (e) {
                return document.getElementById(el.id) !== el;
            }
        },
        
        // Critical Style Mismatches - Only check CSS class vs computed conflicts
        styleMismatch: el => {
            const s = getComputedStyle(el);
            // Flag serious conflicts between CSS classes and computed styles
            const flexClassButNotFlex = (el.className.includes('flex') || el.className.includes('d-flex')) && 
                                      !s.display.includes('flex') && s.display !== 'none';
            
            const hiddenClassButVisible = (el.className.includes('hidden') || el.className.includes('d-none')) && 
                                        s.display !== 'none' && s.visibility !== 'hidden';
            
            return flexClassButNotFlex || hiddenClassButVisible;
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
            '--header-height', '--sidebar-width', '--color-text', 
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

    // Helper functions for AI agent integration
    function generateSelector(el) {
        if (el.id) return `#${el.id}`;
        
        let selector = el.tagName.toLowerCase();
        if (el.className) {
            const classes = el.className.split(' ').filter(c => c && !c.includes('ng-') && !c.includes('_')).slice(0, 2);
            if (classes.length > 0) selector += '.' + classes.join('.');
        }
        
        // Add position context if needed
        const parent = el.parentElement;
        if (parent && parent.children.length > 1) {
            const index = Array.from(parent.children).indexOf(el);
            selector += `:nth-child(${index + 1})`;
        }
        
        return selector;
    }

    function getRelevantStyles(el, issueType) {
        const s = getComputedStyle(el);
        const relevantStyles = {
            display: s.display,
            visibility: s.visibility,
            opacity: s.opacity,
            position: s.position,
            width: s.width,
            height: s.height
        };

        // Add issue-specific styles
        if (issueType === 'cssCorruption') relevantStyles.minHeight = s.minHeight;
        if (issueType === 'invisibleText') relevantStyles.color = s.color;
        if (issueType === 'brokenLayout') {
            relevantStyles.gridTemplateColumns = s.gridTemplateColumns;
            relevantStyles.gridTemplateRows = s.gridTemplateRows;
        }

        return sanitizeForJSON(relevantStyles);
    }

    function analyze() {
        const allElements = document.querySelectorAll('*');
        const issues = Object.keys(CRITICAL_ISSUES).reduce((acc, key) => {
            acc[key] = 0;
            return acc;
        }, { total: 0 });

        const detectedIssues = []; // Store actual problem elements with DOM references

        Array.from(allElements).forEach(el => {
            Object.entries(CRITICAL_ISSUES).forEach(([type, checkFn]) => {
                if (checkFn(el)) {
                    issues[type]++;
                    issues.total++;
                    
                    // Store reference to problematic element for AI agents
                    detectedIssues.push({
                        type,
                        element: el,
                        selector: generateSelector(el),
                        tagName: el.tagName.toLowerCase(),
                        classes: sanitizeForJSON(el.className),
                        id: el.id,
                        textContent: sanitizeForJSON(el.textContent?.substring(0, 50)),
                        styles: getRelevantStyles(el, type)
                    });
                }
            });
        });

        // More reasonable scoring: Only critical issues matter
        const criticalCount = issues.cssCorruption + issues.brokenForm + issues.brokenInteraction + 
                             issues.blazorError + issues.jsErrors + issues.invalidCSS;
        const score = Math.max(0, 100 - (criticalCount * 5) - (issues.total * 0.5));
        const status = criticalCount > 0 ? 'critical' : 
                      score >= 90 ? 'excellent' : score >= 80 ? 'good' : score >= 70 ? 'fair' : 'needs-attention';

        const report = {
            score: Math.round(score),
            status,
            totalElements: allElements.length,
            issues,
            detectedIssues: detectedIssues.slice(0, 20), // Limit for performance, most critical first
            criticalCount,
            isHealthy: criticalCount === 0 && score >= 80,
            actionable: generateActionableInsights(issues, allElements.length)
        };

        console.log(`🔍 Page Analysis - Score: ${report.score}% (${report.status})`);
        console.log('📊 Issue Counts:', issues);
        if (detectedIssues.length > 0) {
            console.log('🎯 Detected Issues with DOM References:', detectedIssues.slice(0, 5));
        }
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
        
        // Auto-highlighting disabled to prevent unwanted visual debug borders
        // issues.concat(layoutIssues).slice(0, 5).forEach((issue, i) => {
        //     const elements = document.querySelectorAll(issue.selector ? `.${issue.selector.split(' ')[0]}` : issue.element);
        //     const colors = ['red', 'orange', 'yellow', 'purple', 'blue'];
        //     elements.forEach(el => {
        //         el.style.outline = `2px solid ${colors[i % colors.length]}`;
        //         setTimeout(() => el.style.outline = '', 3000);
        //     });
        // });
        
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
        const issues = Object.keys(CRITICAL_ISSUES).reduce((acc, key) => {
            acc[key] = 0;
            return acc;
        }, { total: 0 });

        Array.from(elements).forEach(el => {
            Object.entries(CRITICAL_ISSUES).forEach(([type, checkFn]) => {
                if (checkFn(el)) {
                    issues[type]++;
                    issues.total++;
                }
            });
        });

        const criticalCount = issues.cssCorruption + issues.brokenForm + issues.brokenInteraction + 
                             issues.blazorError + issues.jsErrors + issues.invalidCSS;
        const score = Math.max(0, 100 - (criticalCount * 5) - (issues.total * 0.5));
        const status = criticalCount > 0 ? 'critical' : 
                      score >= 90 ? 'excellent' : score >= 80 ? 'good' : score >= 70 ? 'fair' : 'needs-attention';

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

window.RRDebug = RRDebugAPI;
