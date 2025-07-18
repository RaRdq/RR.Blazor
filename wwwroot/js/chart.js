// RR.Blazor Chart System - Modern chart rendering and interaction management
// Supports pie charts, column charts, and interactive features

let activeCharts = new Map();
let chartObserver = null;
let dotNetHelper = null;

// Chart Animation System (simplified - animations now handled by SCSS)
export function animatePieChart(element) {
    if (!element) return;
    
    const slices = element.querySelectorAll('.pie-slice');
    if (!slices.length) return;
    
    slices.forEach(slice => {
        slice.classList.add('hover-lift');
        
        slice.addEventListener('click', () => {
            slice.classList.toggle('active');
        });
    });
}

export function animateColumnChart(element) {
    if (!element) return;
    
    const bars = element.querySelectorAll('.column-chart-bar');
    if (!bars.length) return;
    
    
    bars.forEach(bar => {
        bar.classList.add('hover-lift');
        
        bar.addEventListener('click', () => {
            bar.classList.toggle('active');
        });
    });
}

// Chart Tooltip System
export function initializeTooltip(element, options = {}) {
    if (!element) return;
    
    const tooltip = document.createElement('div');
    tooltip.className = 'chart-tooltip';
    tooltip.style.position = 'absolute';
    tooltip.style.pointerEvents = 'none';
    tooltip.style.zIndex = '1000';
    document.body.appendChild(tooltip);
    
    const showTooltip = (event, content) => {
        tooltip.textContent = content; // Use textContent for security
        tooltip.classList.add('visible');
        
        const rect = element.getBoundingClientRect();
        const tooltipRect = tooltip.getBoundingClientRect();
        
        let left = event.clientX - tooltipRect.width / 2;
        let top = event.clientY - tooltipRect.height - 10;
        
        // Boundary checks
        if (left < 0) left = 0;
        if (left + tooltipRect.width > window.innerWidth) {
            left = window.innerWidth - tooltipRect.width;
        }
        if (top < 0) top = event.clientY + 10;
        
        tooltip.style.left = `${left}px`;
        tooltip.style.top = `${top}px`;
    };
    
    const hideTooltip = () => {
        tooltip.classList.remove('visible');
    };
    
    // Chart-specific tooltip handling
    if (element.querySelector('.pie-slice')) {
        element.querySelectorAll('.pie-slice').forEach(slice => {
            slice.addEventListener('mouseenter', (e) => {
                const label = slice.getAttribute('aria-label');
                if (label) {
                    showTooltip(e, `<div class="chart-tooltip-content">${label}</div>`);
                }
            });
            slice.addEventListener('mouseleave', hideTooltip);
        });
    }
    
    if (element.querySelector('.column-chart-bar')) {
        element.querySelectorAll('.column-chart-bar').forEach(bar => {
            bar.addEventListener('mouseenter', (e) => {
                const label = bar.getAttribute('aria-label');
                if (label) {
                    showTooltip(e, `<div class="chart-tooltip-content">${label}</div>`);
                }
            });
            bar.addEventListener('mouseleave', hideTooltip);
        });
    }
    
    return {
        dispose: () => {
            tooltip.remove();
        }
    };
}

// Chart Data Export
export function exportChartData(element, format = 'csv') {
    if (!element) return null;
    
    const data = extractChartData(element);
    if (!data || !data.length) return null;
    
    switch (format.toLowerCase()) {
        case 'csv':
            return exportToCSV(data);
        case 'json':
            return JSON.stringify(data, null, 2);
        case 'tsv':
            return exportToTSV(data);
        default:
            return null;
    }
}

function extractChartData(element) {
    const data = [];
    
    // Extract from pie chart
    const pieSlices = element.querySelectorAll('.pie-slice');
    pieSlices.forEach(slice => {
        const label = slice.getAttribute('aria-label');
        if (label) {
            // Parse aria-label: "Category: Value (Percentage)"
            const matches = label.match(/(.+?):\s*(.+?)\s*\((.+?)\)/);
            if (matches) {
                data.push({
                    label: matches[1].trim(),
                    value: matches[2].trim(),
                    percentage: matches[3].trim()
                });
            }
        }
    });
    
    // Extract from column chart
    const columnBars = element.querySelectorAll('.column-chart-bar');
    columnBars.forEach(bar => {
        const label = bar.getAttribute('aria-label');
        if (label) {
            // Parse aria-label: "Category: Value"
            const matches = label.match(/(.+?):\s*(.+)/);
            if (matches) {
                data.push({
                    label: matches[1].trim(),
                    value: matches[2].trim()
                });
            }
        }
    });
    
    return data;
}

function exportToCSV(data) {
    if (!data.length) return '';
    
    const headers = Object.keys(data[0]);
    const csvContent = [
        headers.join(','),
        ...data.map(row => headers.map(header => `"${row[header] || ''}"`).join(','))
    ].join('\n');
    
    return csvContent;
}

function exportToTSV(data) {
    if (!data.length) return '';
    
    const headers = Object.keys(data[0]);
    const tsvContent = [
        headers.join('\t'),
        ...data.map(row => headers.map(header => row[header] || '').join('\t'))
    ].join('\n');
    
    return tsvContent;
}

// Chart Responsiveness
export function initializeResponsiveChart(element) {
    if (!element) return;
    
    const observer = new ResizeObserver(entries => {
        entries.forEach(entry => {
            const { width, height } = entry.contentRect;
            updateChartSize(element, width, height);
        });
    });
    
    observer.observe(element);
    
    return {
        dispose: () => {
            observer.disconnect();
        }
    };
}

function updateChartSize(element, width, height) {
    // Update SVG viewBox for pie charts
    const svg = element.querySelector('svg');
    if (svg) {
        const aspectRatio = width / height;
        const viewBoxSize = Math.min(width, height);
        svg.setAttribute('viewBox', `0 0 ${viewBoxSize} ${viewBoxSize}`);
    }
    
    // Update column chart layout
    const columnContainer = element.querySelector('.column-chart-container');
    if (columnContainer) {
        const isSmall = width < 400;
        columnContainer.classList.toggle('chart-small-layout', isSmall);
        
        // Adjust bar spacing for small screens
        const bars = element.querySelectorAll('.column-chart-bar');
        bars.forEach(bar => {
            bar.style.flexBasis = isSmall ? '100%' : 'auto';
        });
    }
}

// Chart Accessibility
export function enhanceChartAccessibility(element, options = {}) {
    if (!element) return;
    
    // Add ARIA roles and properties
    element.setAttribute('role', 'img');
    element.setAttribute('tabindex', '0');
    
    // Add keyboard navigation
    element.addEventListener('keydown', (e) => {
        if (e.key === 'Enter' || e.key === ' ') {
            e.preventDefault();
            const firstInteractive = element.querySelector('.pie-slice, .column-chart-bar');
            if (firstInteractive) {
                firstInteractive.click();
            }
        }
    });
    
    // Generate alternative text
    const dataTable = element.querySelector('.chart-accessibility-table table');
    if (dataTable && options.generateSummary) {
        const summary = generateChartSummary(dataTable);
        element.setAttribute('aria-label', summary);
    }
    
    // Add focus management
    const interactiveElements = element.querySelectorAll('.pie-slice, .column-chart-bar');
    interactiveElements.forEach((el, index) => {
        el.setAttribute('tabindex', '0');
        el.addEventListener('focus', () => {
            el.style.outline = '2px solid var(--color-interactive-focus)';
        });
        el.addEventListener('blur', () => {
            el.style.outline = 'none';
        });
    });
}

function generateChartSummary(dataTable) {
    const rows = dataTable.querySelectorAll('tbody tr');
    if (!rows.length) return 'Chart with no data';
    
    const total = rows.length;
    const firstRow = rows[0];
    const lastRow = rows[rows.length - 1];
    
    const firstLabel = firstRow.querySelector('td:first-child')?.textContent || '';
    const lastLabel = lastRow.querySelector('td:first-child')?.textContent || '';
    
    return `Chart with ${total} data points, ranging from ${firstLabel} to ${lastLabel}`;
}

// Chart Theme Integration (simplified)
export function applyChartTheme(element, themeData) {
    if (!element) return;
    
    const isDark = themeData.mode === 'dark';
    
    element.setAttribute('data-theme', isDark ? 'dark' : 'light');
    
}

export function optimizeChartPerformance(element) {
    if (!element) return;
    
    element.style.willChange = 'transform';
    element.style.transform = 'translateZ(0)';
    
    const svg = element.querySelector('svg');
    if (svg) {
        svg.style.shapeRendering = 'geometricPrecision';
        svg.style.textRendering = 'geometricPrecision';
    }
    
}

export function initializeChart(element, options = {}) {
    if (!element) return null;
    
    const chartId = `chart-${Math.random().toString(36).substr(2, 9)}`;
    element.setAttribute('data-chart-id', chartId);
    
    const chartInstance = {
        id: chartId,
        element: element,
        options: options,
        tooltip: null,
        resizeObserver: null,
        dispose: () => {
            if (chartInstance.tooltip) {
                chartInstance.tooltip.dispose();
            }
            if (chartInstance.resizeObserver) {
                chartInstance.resizeObserver.dispose();
            }
            activeCharts.delete(chartId);
        }
    };
    
    // Initialize features
    if (options.tooltip !== false) {
        chartInstance.tooltip = initializeTooltip(element, options.tooltip);
    }
    
    if (options.responsive !== false) {
        chartInstance.resizeObserver = initializeResponsiveChart(element);
    }
    
    if (options.accessibility !== false) {
        enhanceChartAccessibility(element, options.accessibility);
    }
    
    if (options.animations !== false) {
        optimizeChartPerformance(element);
    }
    
    activeCharts.set(chartId, chartInstance);
    
    return chartInstance;
}

// Chart Management
export function getChart(element) {
    if (!element) return null;
    
    const chartId = element.getAttribute('data-chart-id');
    return chartId ? activeCharts.get(chartId) : null;
}

export function disposeChart(element) {
    const chart = getChart(element);
    if (chart) {
        chart.dispose();
        return true;
    }
    return false;
}

export function disposeAllCharts() {
    activeCharts.forEach(chart => chart.dispose());
    activeCharts.clear();
}

// Theme change handler
export function handleThemeChange(themeData) {
    activeCharts.forEach(chart => {
        applyChartTheme(chart.element, themeData);
    });
}

// Register with theme system
document.addEventListener('themeChanged', (e) => {
    handleThemeChange(e.detail);
});

// Global chart utilities
window.RChart = {
    // Animation functions
    animatePieChart,
    animateColumnChart,
    
    // Tooltip functions
    initializeTooltip,
    
    // Export functions
    exportChartData,
    
    // Responsive functions
    initializeResponsiveChart,
    
    // Accessibility functions
    enhanceChartAccessibility,
    
    // Theme functions
    applyChartTheme,
    handleThemeChange,
    
    // Performance functions
    optimizeChartPerformance,
    
    // Management functions
    initializeChart,
    getChart,
    disposeChart,
    disposeAllCharts,
    
    // Utilities
    extractChartData,
    generateChartSummary
};

// Initialize intersection observer for lazy loading
(function() {
    if ('IntersectionObserver' in window) {
        chartObserver = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const element = entry.target;
                    const chartType = element.getAttribute('data-chart-type');
                    
                    if (chartType === 'pie' && !element.hasAttribute('data-animated')) {
                        animatePieChart(element);
                        element.setAttribute('data-animated', 'true');
                    } else if (chartType === 'column' && !element.hasAttribute('data-animated')) {
                        animateColumnChart(element);
                        element.setAttribute('data-animated', 'true');
                    }
                }
            });
        }, {
            threshold: 0.1
        });
        
        // Observe all chart elements
        document.querySelectorAll('[data-chart-type]').forEach(element => {
            chartObserver.observe(element);
        });
        
        // Also observe dynamically added chart elements
        const mutationObserver = new MutationObserver(mutations => {
            mutations.forEach(mutation => {
                mutation.addedNodes.forEach(node => {
                    if (node.nodeType === Node.ELEMENT_NODE) {
                        const chartElements = node.querySelectorAll ? node.querySelectorAll('[data-chart-type]') : [];
                        chartElements.forEach(element => {
                            chartObserver.observe(element);
                        });
                    }
                });
            });
        });
        
        mutationObserver.observe(document.body, {
            childList: true,
            subtree: true
        });
    }
})();

// Cleanup on page unload
window.addEventListener('beforeunload', () => {
    disposeAllCharts();
    if (chartObserver) {
        chartObserver.disconnect();
    }
});
