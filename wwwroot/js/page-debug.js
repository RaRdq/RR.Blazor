/**
 * RR.Blazor Critical Debug Utility
 * High-value functions for identifying layout corruption and CSS compilation issues
 * 
 * CORE FUNCTIONS:
 * - window.RRDebug.findCorruption()      // Find universal CSS rule corruption
 * - window.RRDebug.analyze()             // Page health analysis with actionable insights
 * - window.RRDebug.component('.selector') // Component-specific analysis
 * - window.RRDebug.verifyCSS()           // CSS compilation verification
 */

const RRDebugAPI = (() => {
    const VERSION = '3.0.0';
    
    // Critical corruption patterns that break layouts
    const CRITICAL_ISSUES = {
        forcedHeight: el => {
            const s = getComputedStyle(el);
            return s.minHeight === '44px' && !['BUTTON', 'INPUT', 'A', 'LABEL', 'SELECT', 'TEXTAREA'].includes(el.tagName);
        },
        brokenFlex: el => {
            const hasFlexClass = el.className.includes('flex');
            const actualFlex = getComputedStyle(el).display.includes('flex');
            return hasFlexClass && !actualFlex;
        },
        invalidCSS: el => {
            return Array.from(el.classList).some(cls => cls.startsWith('@') || cls.includes('undefined'));
        }
    };

    // Find the source of universal CSS corruption
    function findCorruption() {
        console.log('🔍 Analyzing CSS Corruption...');
        
        const allElements = document.querySelectorAll('*');
        const corrupted = Array.from(allElements).filter(el => {
            const s = getComputedStyle(el);
            return s.minHeight === '44px' && !['BUTTON', 'INPUT', 'A', 'LABEL', 'SELECT', 'TEXTAREA'].includes(el.tagName);
        });

        console.log(`🚨 CORRUPTION FOUND: ${corrupted.length}/${allElements.length} elements have forced 44px min-height`);
        
        if (corrupted.length > 100) {
            // Universal rule corruption
            const sampleCorrupted = corrupted.slice(0, 5);
            console.log('📋 Sample corrupted elements:', sampleCorrupted.map(el => ({
                tag: el.tagName,
                classes: el.className,
                minHeight: getComputedStyle(el).minHeight
            })));

            // Try to find the CSS rule source
            findCSSRuleSource();
        }

        return {
            totalElements: allElements.length,
            corruptedElements: corrupted.length,
            corruptionRatio: (corrupted.length / allElements.length * 100).toFixed(1) + '%',
            samples: corrupted.slice(0, 10)
        };
    }

    // Find CSS rule causing universal corruption
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
                            
                            // Check if it's a universal selector
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

    // Verify CSS compilation and extends/mixins
    function verifyCSS() {
        console.log('🔧 Verifying CSS Compilation...');
        
        const verification = {
            cssVariables: {},
            extendsCompiled: false,
            mixinsCompiled: false,
            issues: []
        };

        // Check critical CSS variables
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

        // Check if extends are compiled (look for actual CSS classes vs @extend patterns)
        const sampleElements = document.querySelectorAll('.app-shell, .app-header, .form-field');
        const hasCompiledClasses = Array.from(sampleElements).some(el => {
            return !Array.from(el.classList).some(cls => cls.startsWith('@'));
        });
        verification.extendsCompiled = hasCompiledClasses;

        // Check for uncompiled SCSS artifacts
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

    // Streamlined page analysis with actionable insights
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

    // Generate specific actionable insights
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

    // Component analysis with specific issue details
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

    // Quick element scanner
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

    // Enhanced error capture for AI analysis
    function captureConsoleErrors(options = {}) {
        const { maxLines = 3, includeStack = false } = options;
        
        if (!window.__RR_CONSOLE_ERRORS) {
            window.__RR_CONSOLE_ERRORS = [];
            
            // Override console.error to capture errors
            const originalError = console.error;
            console.error = function(...args) {
                const errorEntry = {
                    timestamp: new Date().toISOString(),
                    message: args.map(arg => 
                        typeof arg === 'object' ? JSON.stringify(arg, null, 2) : String(arg)
                    ).join(' '),
                    stack: includeStack ? new Error().stack : null
                };
                
                window.__RR_CONSOLE_ERRORS.push(errorEntry);
                return originalError.apply(console, args);
            };
            
            // Capture unhandled errors
            window.addEventListener('error', (e) => {
                window.__RR_CONSOLE_ERRORS.push({
                    timestamp: new Date().toISOString(),
                    message: `${e.message} at ${e.filename}:${e.lineno}:${e.colno}`,
                    stack: e.error?.stack || null
                });
            });
        }
        
        // Return summarized errors (first 3 lines of each)
        return window.__RR_CONSOLE_ERRORS.map(error => ({
            timestamp: error.timestamp,
            summary: error.message.split('\n').slice(0, maxLines).join('\n'),
            hasMore: error.message.split('\n').length > maxLines
        }));
    }
    
    // Clear console errors for fresh testing
    function clearConsoleErrors() {
        if (window.__RR_CONSOLE_ERRORS) {
            window.__RR_CONSOLE_ERRORS.length = 0;
        }
        console.clear();
        return "Console errors cleared";
    }
    
    // Get comprehensive QA report for AI analysis
    function getAIReport() {
        const pageAnalysis = analyze();
        const cssVerification = verifyCSS();
        const consoleErrors = captureConsoleErrors();
        
        // Run additional analysis for comprehensive report
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
                    text: btn.textContent?.trim() || btn.getAttribute('aria-label') || 'No text',
                    classes: btn.className,
                    disabled: btn.disabled,
                    issues: Object.keys(CRITICAL_ISSUES).filter(type => CRITICAL_ISSUES[type](btn))
                })),
                inputs: Array.from(inputElements).slice(0, 5).map(input => ({
                    type: input.type || input.tagName.toLowerCase(),
                    placeholder: input.placeholder,
                    required: input.required,
                    classes: input.className,
                    issues: Object.keys(CRITICAL_ISSUES).filter(type => CRITICAL_ISSUES[type](input))
                })),
                modals: Array.from(modalElements).map(modal => ({
                    visible: getComputedStyle(modal).display !== 'none',
                    classes: modal.className,
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
                quickFixes: pageAnalysis.issues.invalidCSS > 0 ? ['Run RRDebug.verifyCSS() and rebuild SCSS'] : [],
                recommendations: pageAnalysis.score < 70 ? [
                    'Page health below 70% - investigate CSS corruption',
                    'Check console errors for JavaScript issues',
                    'Verify responsive design and layout integrity'
                ] : ['Page health is acceptable - focus on minor optimizations']
            }
        };
        
        // Store for AI retrieval
        window.__RR_AI_REPORT = report;
        
        console.log('🤖 Comprehensive AI Report Generated:', report);
        console.log('📊 Quick Summary:', {
            health: `${report.health.score}% (${report.health.status})`,
            errors: report.errors.count,
            elements: report.page.elementCounts.total,
            performance: report.performance.loadTime ? `${report.performance.loadTime}ms` : 'N/A'
        });
        
        return report;
    }

    // Additional debug utilities
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

    // 6 Essential Commands for AI QA
    const visualDebug = (selector = 'body') => {
        const element = document.querySelector(selector);
        if (!element) return { error: `Element not found: ${selector}` };
        
        const children = element.querySelectorAll('*');
        const issues = [];
        
        // Visual issues detection
        Array.from(children).forEach(el => {
            const rect = el.getBoundingClientRect();
            const styles = getComputedStyle(el);
            
            if (rect.width === 0 || rect.height === 0) {
                issues.push({ element: el.tagName, issue: 'zero-sized', selector: el.className });
            }
            if (parseFloat(styles.fontSize) < 12 && el.textContent?.trim()) {
                issues.push({ element: el.tagName, issue: 'small-font', size: styles.fontSize });
            }
            if (styles.color === styles.backgroundColor && styles.color !== 'transparent') {
                issues.push({ element: el.tagName, issue: 'invisible-text', color: styles.color });
            }
        });
        
        // Highlight issues
        issues.forEach(issue => {
            const els = document.querySelectorAll(issue.selector ? `.${issue.selector.split(' ')[0]}` : issue.element);
            els.forEach(el => {
                el.style.outline = '2px solid red';
                setTimeout(() => el.style.outline = '', 3000);
            });
        });
        
        return {
            selector,
            totalElements: children.length,
            issues: issues.slice(0, 10),
            summary: `Found ${issues.length} visual issues in ${children.length} elements`
        };
    };

    const checkCSS = () => {
        const corruption = findCorruption();
        const verification = verifyCSS();
        const invalidElements = [];
        
        document.querySelectorAll('*').forEach(el => {
            const classes = Array.from(el.classList);
            const invalidClasses = classes.filter(cls => 
                cls.startsWith('@') || cls.includes('undefined') || cls.includes('NaN')
            );
            if (invalidClasses.length > 0) {
                invalidElements.push({
                    tag: el.tagName,
                    invalidClasses,
                    totalClasses: classes.length
                });
            }
        });
        
        return {
            corruption: corruption.corruptionRatio !== '0.0%' ? corruption : null,
            compilation: verification.issues.length > 0 ? verification : null,
            invalidElements: invalidElements.slice(0, 5),
            status: corruption.corruptionRatio === '0.0%' && verification.issues.length === 0 ? 'healthy' : 'issues-detected'
        };
    };

    const checkHealth = () => {
        const analysis = analyze();
        return {
            score: analysis.score,
            status: analysis.status,
            isHealthy: analysis.score >= 70,
            criticalIssues: analysis.actionable?.filter(a => a.priority === 'CRITICAL') || [],
            elementCount: document.querySelectorAll('*').length,
            summary: `Page health: ${analysis.score}% (${analysis.status})`
        };
    };

    const checkPerformance = () => {
        const timing = performance.timing;
        const memory = performance.memory;
        
        return {
            loadTime: timing ? timing.loadEventEnd - timing.navigationStart : null,
            domReady: timing ? timing.domContentLoadedEventEnd - timing.navigationStart : null,
            renderTime: timing ? timing.domInteractive - timing.navigationStart : null,
            memory: memory ? {
                used: Math.round(memory.usedJSHeapSize / 1024 / 1024) + 'MB',
                total: Math.round(memory.totalJSHeapSize / 1024 / 1024) + 'MB',
                limit: Math.round(memory.jsHeapSizeLimit / 1024 / 1024) + 'MB'
            } : null,
            elements: document.querySelectorAll('*').length,
            images: document.querySelectorAll('img').length,
            status: (timing?.loadEventEnd - timing?.navigationStart) > 3000 ? 'slow' : 'acceptable'
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

    // Public API - 6 Essential Commands
    return {
        // 1. Visual debugging with highlighting
        visual: visualDebug,
        
        // 2. CSS/HTML malformation detection  
        css: checkCSS,
        
        // 3. Page health assessment
        health: checkHealth,
        
        // 4. Performance metrics
        performance: checkPerformance,
        
        // 5. Error logs in efficient format
        logs: getLogs,
        
        // 6. Complete comprehensive report
        report: getAIReport
    };
})();

// Export and attach to window
window.RRDebug = RRDebugAPI;

// Auto-run streamlined analysis on debug pages
if (window.location.search.includes('debug=true') || window.location.search.includes('qa=true')) {
    setTimeout(() => {
        console.log('🤖 Auto-running streamlined QA analysis...');
        
        // Run essential checks
        const health = window.RRDebug.health();
        const logs = window.RRDebug.logs();
        
        console.log('🏥 Health:', health.summary);
        console.log('📝 Logs:', logs.errorSummary);
        
        // Generate full report after interaction
        setTimeout(() => {
            const report = window.RRDebug.report();
            console.log('✅ Complete report ready:', report);
            console.log('📊 Access via: window.__RR_AI_REPORT');
        }, 2000);
    }, 1000);
}

export default RRDebugAPI;