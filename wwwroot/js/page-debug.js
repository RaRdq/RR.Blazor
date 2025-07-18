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

    // Public API - focused on high-value functions
    return {
        // Critical functions for debugging corruption
        findCorruption,
        verifyCSS,
        analyze,
        
        // Component analysis
        component: analyzeComponent,
        scan,
        
        // Quick health check
        isHealthy: () => analyze().score >= 70,
        
        // Automation support
        getQAReport: () => {
            const report = analyze();
            window.__RR_DEBUG_REPORT = report;
            return report;
        }
    };
})();

// Export and attach to window
window.RRDebug = RRDebugAPI;

// Auto-run corruption detection on debug pages
if (window.location.search.includes('debug=true') || window.location.search.includes('qa=true')) {
    setTimeout(() => {
        console.log('🤖 Auto-running corruption detection...');
        window.RRDebug.findCorruption();
        window.RRDebug.verifyCSS();
    }, 1000);
}

export default RRDebugAPI;