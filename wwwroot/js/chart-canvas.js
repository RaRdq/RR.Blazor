// RR.Blazor Canvas 2D Chart Rendering Engine
// High-performance chart rendering with touch interactions and virtualization

let activeCanvasCharts = new Map();
let canvasObserver = null;

// Core Canvas Chart Manager
class CanvasChart {
    constructor(element, options = {}) {
        this.element = element;
        this.canvas = null;
        this.ctx = null;
        this.options = {
            width: 800,
            height: 400,
            padding: { top: 20, right: 20, bottom: 40, left: 40 },
            backgroundColor: 'transparent',
            enableAnimation: true,
            animationDuration: 300,
            enableTouch: true,
            enableZoom: true,
            enablePan: true,
            virtualizeThreshold: 1000,
            ...options
        };
        
        this.data = [];
        this.animationFrame = null;
        this.touches = new Map();
        this.transform = { x: 0, y: 0, scale: 1 };
        this.isVirtualized = false;
        this.visibleRange = { start: 0, end: 0 };
        
        this.init();
    }

    init() {
        this.createCanvas();
        this.setupEventListeners();
        this.setupObservers();
    }

    createCanvas() {
        this.canvas = document.createElement('canvas');
        this.canvas.style.width = '100%';
        this.canvas.style.height = '100%';
        this.canvas.style.display = 'block';
        this.element.appendChild(this.canvas);
        
        this.ctx = this.canvas.getContext('2d');
        this.updateCanvasSize();
    }

    updateCanvasSize() {
        const rect = this.element.getBoundingClientRect();
        const dpr = window.devicePixelRatio || 1;
        
        this.canvas.width = rect.width * dpr;
        this.canvas.height = rect.height * dpr;
        this.ctx.scale(dpr, dpr);
        
        this.options.width = rect.width;
        this.options.height = rect.height;
        
        this.calculateDrawingArea();
    }

    calculateDrawingArea() {
        const { padding } = this.options;
        this.drawingArea = {
            x: padding.left,
            y: padding.top,
            width: this.options.width - padding.left - padding.right,
            height: this.options.height - padding.top - padding.bottom
        };
    }

    setupEventListeners() {
        // Resize observer
        this.resizeObserver = new ResizeObserver(() => {
            this.updateCanvasSize();
            this.render();
        });
        this.resizeObserver.observe(this.element);

        if (this.options.enableTouch) {
            this.setupTouchEvents();
        }
    }

    setupTouchEvents() {
        // Touch events for pan/zoom
        this.canvas.addEventListener('touchstart', this.handleTouchStart.bind(this));
        this.canvas.addEventListener('touchmove', this.handleTouchMove.bind(this));
        this.canvas.addEventListener('touchend', this.handleTouchEnd.bind(this));
        
        // Mouse events for desktop
        this.canvas.addEventListener('mousedown', this.handleMouseDown.bind(this));
        this.canvas.addEventListener('mousemove', this.handleMouseMove.bind(this));
        this.canvas.addEventListener('mouseup', this.handleMouseUp.bind(this));
        this.canvas.addEventListener('wheel', this.handleWheel.bind(this));
    }

    setupObservers() {
        // Intersection observer for performance
        this.intersectionObserver = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    this.startRenderLoop();
                } else {
                    this.stopRenderLoop();
                }
            });
        });
        this.intersectionObserver.observe(this.element);
    }

    setData(data) {
        this.data = data;
        this.isVirtualized = data.length > this.options.virtualizeThreshold;
        
        if (this.isVirtualized) {
            this.calculateVisibleRange();
        }
        
        this.render();
    }

    calculateVisibleRange() {
        // Simple virtualization - show visible data points based on viewport
        const totalPoints = this.data.length;
        const visiblePoints = Math.min(totalPoints, this.options.virtualizeThreshold);
        
        this.visibleRange = {
            start: Math.max(0, Math.floor(totalPoints * 0.1)),
            end: Math.min(totalPoints, Math.floor(totalPoints * 0.9))
        };
    }

    render() {
        if (!this.ctx) return;
        
        this.clear();
        this.applyTransform();
        this.renderChart();
        this.renderOverlays();
    }

    clear() {
        this.ctx.clearRect(0, 0, this.options.width, this.options.height);
        
        if (this.options.backgroundColor !== 'transparent') {
            this.ctx.fillStyle = this.options.backgroundColor;
            this.ctx.fillRect(0, 0, this.options.width, this.options.height);
        }
    }

    applyTransform() {
        this.ctx.save();
        this.ctx.translate(this.transform.x, this.transform.y);
        this.ctx.scale(this.transform.scale, this.transform.scale);
    }

    renderChart() {
        // Override in subclasses
    }

    renderOverlays() {
        this.ctx.restore();
        // Render zoom controls, tooltips, etc.
    }

    // Touch handling
    handleTouchStart(event) {
        event.preventDefault();
        
        Array.from(event.changedTouches).forEach(touch => {
            this.touches.set(touch.identifier, {
                startX: touch.clientX,
                startY: touch.clientY,
                currentX: touch.clientX,
                currentY: touch.clientY
            });
        });

        if (event.touches.length === 2) {
            this.handlePinchStart(event);
        }
    }

    handleTouchMove(event) {
        event.preventDefault();
        
        if (event.touches.length === 1) {
            this.handlePan(event);
        } else if (event.touches.length === 2) {
            this.handlePinch(event);
        }
    }

    handleTouchEnd(event) {
        event.preventDefault();
        
        Array.from(event.changedTouches).forEach(touch => {
            this.touches.delete(touch.identifier);
        });
    }

    handlePan(event) {
        if (!this.options.enablePan) return;
        
        const touch = event.touches[0];
        const stored = this.touches.get(touch.identifier);
        
        if (stored) {
            const deltaX = touch.clientX - stored.currentX;
            const deltaY = touch.clientY - stored.currentY;
            
            this.transform.x += deltaX;
            this.transform.y += deltaY;
            
            stored.currentX = touch.clientX;
            stored.currentY = touch.clientY;
            
            this.render();
        }
    }

    handlePinch(event) {
        if (!this.options.enableZoom) return;
        
        const touch1 = event.touches[0];
        const touch2 = event.touches[1];
        
        const currentDistance = Math.hypot(
            touch1.clientX - touch2.clientX,
            touch1.clientY - touch2.clientY
        );
        
        if (this.lastPinchDistance) {
            const scaleFactor = currentDistance / this.lastPinchDistance;
            this.transform.scale = Math.max(0.1, Math.min(5, this.transform.scale * scaleFactor));
            this.render();
        }
        
        this.lastPinchDistance = currentDistance;
    }

    handlePinchStart(event) {
        const touch1 = event.touches[0];
        const touch2 = event.touches[1];
        
        this.lastPinchDistance = Math.hypot(
            touch1.clientX - touch2.clientX,
            touch1.clientY - touch2.clientY
        );
    }

    // Mouse events for desktop
    handleMouseDown(event) {
        this.isMouseDown = true;
        this.lastMouseX = event.clientX;
        this.lastMouseY = event.clientY;
    }

    handleMouseMove(event) {
        if (this.isMouseDown && this.options.enablePan) {
            const deltaX = event.clientX - this.lastMouseX;
            const deltaY = event.clientY - this.lastMouseY;
            
            this.transform.x += deltaX;
            this.transform.y += deltaY;
            
            this.lastMouseX = event.clientX;
            this.lastMouseY = event.clientY;
            
            this.render();
        }
    }

    handleMouseUp(event) {
        this.isMouseDown = false;
    }

    handleWheel(event) {
        if (!this.options.enableZoom) return;
        
        event.preventDefault();
        const scaleFactor = event.deltaY > 0 ? 0.9 : 1.1;
        this.transform.scale = Math.max(0.1, Math.min(5, this.transform.scale * scaleFactor));
        this.render();
    }

    // Animation system
    startRenderLoop() {
        if (this.animationFrame) return;
        
        const renderLoop = () => {
            this.render();
            this.animationFrame = requestAnimationFrame(renderLoop);
        };
        
        this.animationFrame = requestAnimationFrame(renderLoop);
    }

    stopRenderLoop() {
        if (this.animationFrame) {
            cancelAnimationFrame(this.animationFrame);
            this.animationFrame = null;
        }
    }

    dispose() {
        this.stopRenderLoop();
        
        if (this.resizeObserver) {
            this.resizeObserver.disconnect();
        }
        
        if (this.intersectionObserver) {
            this.intersectionObserver.disconnect();
        }
        
        this.touches.clear();
        
        if (this.canvas && this.canvas.parentNode) {
            this.canvas.parentNode.removeChild(this.canvas);
        }
    }
}

// Column Chart Implementation
class CanvasColumnChart extends CanvasChart {
    renderChart() {
        if (!this.data || !this.data.length) return;
        
        const visibleData = this.isVirtualized ? 
            this.data.slice(this.visibleRange.start, this.visibleRange.end) : 
            this.data;
        
        const maxValue = Math.max(...visibleData.map(d => d.value));
        const barWidth = this.drawingArea.width / visibleData.length * 0.8;
        const barSpacing = this.drawingArea.width / visibleData.length * 0.2;
        
        visibleData.forEach((dataPoint, index) => {
            const barHeight = (dataPoint.value / maxValue) * this.drawingArea.height;
            const x = this.drawingArea.x + index * (barWidth + barSpacing);
            const y = this.drawingArea.y + this.drawingArea.height - barHeight;
            
            // Bar fill
            this.ctx.fillStyle = dataPoint.color || 'var(--color-primary)';
            this.ctx.fillRect(x, y, barWidth, barHeight);
            
            // Bar border
            this.ctx.strokeStyle = 'var(--color-border)';
            this.ctx.strokeRect(x, y, barWidth, barHeight);
            
            // Label
            this.ctx.fillStyle = 'var(--color-text)';
            this.ctx.textAlign = 'center';
            this.ctx.fillText(
                dataPoint.label, 
                x + barWidth / 2, 
                this.drawingArea.y + this.drawingArea.height + 20
            );
        });
    }
}

// Pie Chart Implementation
class CanvasPieChart extends CanvasChart {
    renderChart() {
        if (!this.data || !this.data.length) return;
        
        const centerX = this.drawingArea.x + this.drawingArea.width / 2;
        const centerY = this.drawingArea.y + this.drawingArea.height / 2;
        const radius = Math.min(this.drawingArea.width, this.drawingArea.height) / 2 * 0.8;
        
        const total = this.data.reduce((sum, d) => sum + d.value, 0);
        let currentAngle = -Math.PI / 2; // Start at top
        
        this.data.forEach(dataPoint => {
            const sliceAngle = (dataPoint.value / total) * 2 * Math.PI;
            
            // Slice
            this.ctx.beginPath();
            this.ctx.moveTo(centerX, centerY);
            this.ctx.arc(centerX, centerY, radius, currentAngle, currentAngle + sliceAngle);
            this.ctx.closePath();
            
            this.ctx.fillStyle = dataPoint.color || 'var(--color-primary)';
            this.ctx.fill();
            
            this.ctx.strokeStyle = 'var(--color-surface)';
            this.ctx.lineWidth = 2;
            this.ctx.stroke();
            
            currentAngle += sliceAngle;
        });
    }
}

// Line Chart Implementation  
class CanvasLineChart extends CanvasChart {
    renderChart() {
        if (!this.data || !this.data.length) return;
        
        const visibleData = this.isVirtualized ? 
            this.data.slice(this.visibleRange.start, this.visibleRange.end) : 
            this.data;
        
        const maxValue = Math.max(...visibleData.map(d => d.value));
        const minValue = Math.min(...visibleData.map(d => d.value));
        const valueRange = maxValue - minValue;
        
        this.ctx.beginPath();
        this.ctx.strokeStyle = 'var(--color-primary)';
        this.ctx.lineWidth = 2;
        
        visibleData.forEach((dataPoint, index) => {
            const x = this.drawingArea.x + (index / (visibleData.length - 1)) * this.drawingArea.width;
            const y = this.drawingArea.y + this.drawingArea.height - 
                ((dataPoint.value - minValue) / valueRange) * this.drawingArea.height;
            
            if (index === 0) {
                this.ctx.moveTo(x, y);
            } else {
                this.ctx.lineTo(x, y);
            }
        });
        
        this.ctx.stroke();
        
        // Data points
        visibleData.forEach((dataPoint, index) => {
            const x = this.drawingArea.x + (index / (visibleData.length - 1)) * this.drawingArea.width;
            const y = this.drawingArea.y + this.drawingArea.height - 
                ((dataPoint.value - minValue) / valueRange) * this.drawingArea.height;
            
            this.ctx.beginPath();
            this.ctx.arc(x, y, 4, 0, 2 * Math.PI);
            this.ctx.fillStyle = 'var(--color-primary)';
            this.ctx.fill();
        });
    }
}

// Chart factory
function createCanvasChart(element, type, options = {}) {
    const chartId = `canvas-chart-${Math.random().toString(36).substr(2, 9)}`;
    
    let chart;
    switch (type) {
        case 'column':
            chart = new CanvasColumnChart(element, options);
            break;
        case 'pie':
            chart = new CanvasPieChart(element, options);
            break;
        case 'line':
            chart = new CanvasLineChart(element, options);
            break;
        default:
            chart = new CanvasChart(element, options);
    }
    
    activeCanvasCharts.set(chartId, chart);
    element.dataset.canvasChartId = chartId;
    
    return chart;
}

function disposeCanvasChart(element) {
    const chartId = element.dataset.canvasChartId;
    if (chartId && activeCanvasCharts.has(chartId)) {
        const chart = activeCanvasCharts.get(chartId);
        chart.dispose();
        activeCanvasCharts.delete(chartId);
        delete element.dataset.canvasChartId;
    }
}

// Global cleanup
window.addEventListener('beforeunload', () => {
    activeCanvasCharts.forEach(chart => chart.dispose());
    activeCanvasCharts.clear();
});

// Global exports for Blazor dynamic import compatibility
if (typeof window !== 'undefined') {
    window.RRBlazor = window.RRBlazor || {};
    window.RRBlazor.CanvasChart = {
        createCanvasChart,
        disposeCanvasChart,
        CanvasChart,
        CanvasColumnChart,  
        CanvasPieChart,
        CanvasLineChart
    };
}

// ES6 Module exports for modern import compatibility
export {
    CanvasChart,
    CanvasColumnChart,
    CanvasPieChart,
    CanvasLineChart,
    createCanvasChart,
    disposeCanvasChart
};