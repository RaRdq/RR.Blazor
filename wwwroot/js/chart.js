
let activeCharts = new Map();
let chartObserver = null;

function animatePieChart(element) {
    if (!element) return;
    
    const slices = element.querySelectorAll('.pie-slice');
    if (!slices.length) return;
    
    slices.forEach(slice => {
        slice.classList.add('hover-lift');
        
        slice.addEventListener('click', () => {
            slice.classList.toggle('chart-item-selected');
        });
    });
}

function animateColumnChart(element) {
    if (!element) return;
    
    const bars = element.querySelectorAll('.column-chart-bar');
    if (!bars.length) return;
    
    
    bars.forEach(bar => {
        bar.classList.add('hover-lift');
        
        bar.addEventListener('click', () => {
            bar.classList.toggle('chart-item-selected');
        });
    });
}

function initializeTooltip(element, options = {}) {
    if (!element) return;
    
    const tooltip = document.createElement('div');
    tooltip.className = 'chart-tooltip';
    tooltip.style.position = 'absolute';
    tooltip.style.pointerEvents = 'none';
    tooltip.style.zIndex = 'var(--z-tooltip)';
    document.body.appendChild(tooltip);
    
    const showTooltip = (event, content, targetElement = null) => {
        tooltip.textContent = content;
        tooltip.classList.add('visible');
        
        tooltip.style.visibility = 'visible';
        tooltip.style.opacity = '0';
        const tooltipRect = tooltip.getBoundingClientRect();
        
        let left, top;
        
        if (targetElement) {
            const targetRect = targetElement.getBoundingClientRect();
            left = targetRect.left + targetRect.width / 2 - tooltipRect.width / 2;
            top = targetRect.top - tooltipRect.height - 10;
        } else {
            left = event.clientX - tooltipRect.width / 2;
            top = event.clientY - tooltipRect.height - 15;
        }
        
        if (left < 5) left = 5;
        if (left + tooltipRect.width > window.innerWidth) {
            left = window.innerWidth - tooltipRect.width - 5;
        }
        if (top < 5) {
            top = targetElement 
                ? targetElement.getBoundingClientRect().bottom + 10
                : event.clientY + 15;
        }
        
        tooltip.style.left = `${left}px`;
        tooltip.style.top = `${top}px`;
        tooltip.style.visibility = 'visible';
        tooltip.style.opacity = '1';
    };
    
    const hideTooltip = () => {
        tooltip.classList.remove('visible');
    };
    
    if (element.querySelector('.pie-slice')) {
        element.querySelectorAll('.pie-slice').forEach(slice => {
            slice.addEventListener('mouseenter', (e) => {
                const label = slice.getAttribute('aria-label');
                if (label) {
                    showTooltip(e, label, slice);
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
                    const fillElement = bar.querySelector('.column-chart-bar-fill');
                    showTooltip(e, label, fillElement || bar);
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

function exportChartData(element, format = 'csv') {
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
    
    const pieSlices = element.querySelectorAll('.pie-slice');
    pieSlices.forEach(slice => {
        const label = slice.getAttribute('aria-label');
        if (label) {
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
    
    const columnBars = element.querySelectorAll('.column-chart-bar');
    columnBars.forEach(bar => {
        const label = bar.getAttribute('aria-label');
        if (label) {
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
        ...data.map(row => headers.map(header => `"${row[header]}"`).join(','))
    ].join('\n');
    
    return csvContent;
}

function exportToTSV(data) {
    if (!data.length) return '';
    
    const headers = Object.keys(data[0]);
    const tsvContent = [
        headers.join('\t'),
        ...data.map(row => headers.map(header => row[header]).join('\t'))
    ].join('\n');
    
    return tsvContent;
}

function getContainerDimensions(element) {
    if (!element) return { width: 0, height: 0 };
    
    const rect = element.getBoundingClientRect();
    return {
        width: rect.width,
        height: rect.height
    };
}


function updateChartLayout(container, width) {
    const dataCount = container.querySelectorAll('.column-chart-bar, .pie-slice').length;
    const barWidth = dataCount > 0 ? width / dataCount : width;
    
    container.classList.toggle('chart-xs', width < 300);
    container.classList.toggle('chart-sm', width >= 300 && width < 600);
    container.classList.toggle('chart-md', width >= 600 && width < 900);
    container.classList.toggle('chart-lg', width >= 900);
    
    container.classList.toggle('chart-dense', barWidth < 20);
    container.classList.toggle('chart-compact', barWidth >= 20 && barWidth < 40);
    container.classList.toggle('chart-normal', barWidth >= 40 && barWidth < 80);
    container.classList.toggle('chart-spacious', barWidth >= 80);
}

function enhanceChartAccessibility(element, options = {}) {
    if (!element) return;
    
    element.setAttribute('role', 'img');
    element.setAttribute('tabindex', '0');
    
    element.addEventListener('keydown', (e) => {
        if (e.key === 'Enter' || e.key === ' ') {
            e.preventDefault();
            const firstInteractive = element.querySelector('.pie-slice, .column-chart-bar');
            firstInteractive.click();
        }
    });
    
    const dataTable = element.querySelector('.chart-accessibility-table table');
    if (dataTable && options.generateSummary) {
        const summary = generateChartSummary(dataTable);
        element.setAttribute('aria-label', summary);
    }
    
    const interactiveElements = element.querySelectorAll('.pie-slice, .column-chart-bar');
    interactiveElements.forEach((el) => {
        el.setAttribute('tabindex', '0');
    });
}

function generateChartSummary(dataTable) {
    const rows = dataTable.querySelectorAll('tbody tr');
    if (!rows.length) return 'Chart with no data';
    
    const total = rows.length;
    const firstRow = rows[0];
    const lastRow = rows[rows.length - 1];
    
    const firstLabel = firstRow.querySelector('td:first-child').textContent;
    const lastLabel = lastRow.querySelector('td:first-child').textContent;
    
    return `Chart with ${total} data points, ranging from ${firstLabel} to ${lastLabel}`;
}

function applyChartTheme(element, themeData) {
    if (!element) return;
    
    const isDark = themeData.mode === 'dark';
    
    element.setAttribute('data-theme', isDark ? 'dark' : 'light');
    
}

function optimizeChartPerformance(element) {
    if (!element) return;
    
    element.style.willChange = 'transform';
    element.style.transform = 'translateZ(0)';
    
    const svg = element.querySelector('svg');
    if (svg) {
        svg.style.shapeRendering = 'geometricPrecision';
        svg.style.textRendering = 'geometricPrecision';
    }
}

function initializeChart(element, options = {}) {
    if (!element) return null;
    
    const chartId = `chart-${Math.random().toString(36).substr(2, 9)}`;
    element.setAttribute('data-chart-id', chartId);
    
    const container = element.querySelector('.column-chart-container, .pie-chart-container');
    if (container) {
        // Ensure DOM is ready before layout calculations
        requestAnimationFrame(() => {
            const updateLayout = () => {
                if (!element.offsetWidth) return; // Skip if element not visible
                const availableWidth = container.offsetWidth || element.offsetWidth;
                if (availableWidth > 0) {
                    updateChartLayout(container, availableWidth);
                }
            };
            
            updateLayout();
            
            // Use ResizeObserver with requestAnimationFrame
            let resizeScheduled = false;
            const resizeObserver = new ResizeObserver(entries => {
                if (!resizeScheduled && entries[0].contentRect.width > 0) {
                    resizeScheduled = true;
                    requestAnimationFrame(() => {
                        updateLayout();
                        resizeScheduled = false;
                    });
                }
            });
            
            resizeObserver.observe(container);
            
            container._resizeObserver = resizeObserver;
        });
    }
    
    // Initialize animations
    if (options.animation !== false) {
        if (element.querySelector('.pie-slice')) {
            animatePieChart(element);
        } else if (element.querySelector('.column-chart-bar')) {
            animateColumnChart(element);
        }
    }
    
    const chartInstance = {
        id: chartId,
        element: element,
        dispose: () => {
            if (container && container._resizeObserver) {
                container._resizeObserver.disconnect();
                clearTimeout(container._resizeTimeout);
            }
            activeCharts.delete(chartId);
        }
    };
    
    activeCharts.set(chartId, chartInstance);
    return chartInstance;
}

function getChart(element) {
    if (!element) return null;
    
    const chartId = element.getAttribute('data-chart-id');
    return chartId ? activeCharts.get(chartId) : null;
}

function disposeChart(element) {
    const chart = getChart(element);
    if (chart) {
        chart.dispose();
        return true;
    }
    return false;
}

function disposeAllCharts() {
    activeCharts.forEach(chart => chart.dispose());
    activeCharts.clear();
}

function handleThemeChange(themeData) {
    activeCharts.forEach(chart => {
        applyChartTheme(chart.element, themeData);
    });
}

document.addEventListener('themeChanged', (e) => {
    handleThemeChange(e.detail);
});

export {
    animatePieChart,
    animateColumnChart,
    initializeTooltip,
    exportChartData,
    getContainerDimensions,
    enhanceChartAccessibility,
    applyChartTheme,
    handleThemeChange,
    optimizeChartPerformance,
    initializeChart,
    getChart,
    disposeChart,
    disposeAllCharts,
    extractChartData
};

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
        
        document.querySelectorAll('[data-chart-type]').forEach(element => {
            chartObserver.observe(element);
        });
        
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

function initialize(element, dotNetRef) {
    if (element) {
        const chart = initializeChart(element);
        if (chart && dotNetRef) {
            chart.dotNetRef = dotNetRef;
        }
        return chart;
    }
    return true;
}

function cleanup(element) {
    if (element) {
        return disposeChart(element);
    }
    disposeAllCharts();
    if (chartObserver) {
        chartObserver.disconnect();
    }
    return true;
}

window.addEventListener('beforeunload', () => {
    disposeAllCharts();
    chartObserver.disconnect();
});
