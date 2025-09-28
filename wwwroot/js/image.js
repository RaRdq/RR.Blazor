const imageLoadingCache = new Map();

const config = {
    defaultPlaceholderColor: '#f0f0f0',
    errorRetryAttempts: 2,
    errorRetryDelay: 1000,
    preloadDistance: '200px',
    blurRadius: 20
};

export function preloadImage(src) {
    if (!src) return Promise.reject('No source provided');
    
    if (imageLoadingCache.has(src)) {
        return imageLoadingCache.get(src);
    }
    
    const promise = new Promise((resolve, reject) => {
        const img = new Image();
        
        img.onload = () => {
            imageLoadingCache.delete(src);
            resolve({
                src,
                width: img.naturalWidth,
                height: img.naturalHeight,
                aspectRatio: img.naturalWidth / img.naturalHeight
            });
        };
        
        img.onerror = () => {
            imageLoadingCache.delete(src);
            reject(new Error(`Failed to load image: ${src}`));
        };
        
        img.src = src;
    });
    
    imageLoadingCache.set(src, promise);
    return promise;
}

export function initProgressiveImage(element, dotNetRef, options = {}) {
    if (!element) return false;
    
    const img = element.querySelector('img');
    if (!img) return false;
    
    const {
        src,
        placeholderSrc,
        errorSrc,
        retryAttempts = config.errorRetryAttempts
    } = options;
    
    let attempts = 0;
    
    const loadImage = async () => {
        try {
            if (placeholderSrc && placeholderSrc !== src) {
                img.src = placeholderSrc;
                img.classList.add('placeholder-loaded');
            }
            
            await preloadImage(src);
            
            img.src = src;
            img.classList.remove('placeholder-loaded');
            img.classList.add('main-loaded');
            
            if (dotNetRef) {
                dotNetRef.invokeMethodAsync('OnImageLoaded', true);
            }
        } catch (error) {
            attempts++;
            
            if (attempts < retryAttempts) {
                setTimeout(loadImage, config.errorRetryDelay * attempts);
            } else if (errorSrc) {
                img.src = errorSrc;
                img.classList.add('error-loaded');
                
                if (dotNetRef) {
                    dotNetRef.invokeMethodAsync('OnImageError', src);
                }
            }
        }
    };
    
    loadImage();
    return true;
}

export async function createBlurDataUrl(src, width = 40, height = 30) {
    try {
        const img = await preloadImage(src);
        
        const canvas = document.createElement('canvas');
        canvas.width = width;
        canvas.height = height;
        
        const ctx = canvas.getContext('2d');
        ctx.filter = `blur(${config.blurRadius}px)`;
        
        const imgElement = new Image();
        imgElement.src = src;
        
        await new Promise(resolve => {
            imgElement.onload = resolve;
        });
        
        ctx.drawImage(imgElement, 0, 0, width, height);
        
        return canvas.toDataURL('image/jpeg', 0.5);
    } catch (error) {
        return null;
    }
}

export function initImageZoom(element, options = {}) {
    if (!element) return false;
    
    const img = element.querySelector('img');
    if (!img) return false;
    
    const {
        zoomLevel = 2,
        zoomType = 'hover'
    } = options;
    
    if (zoomType === 'hover') {
        let zoomContainer = null;
        
        element.addEventListener('mouseenter', () => {
            if (!zoomContainer) {
                zoomContainer = createZoomContainer(img, zoomLevel);
                element.appendChild(zoomContainer);
            }
            zoomContainer.style.display = 'block';
        });
        
        element.addEventListener('mousemove', (e) => {
            if (zoomContainer) {
                updateZoomPosition(e, element, img, zoomContainer, zoomLevel);
            }
        });
        
        element.addEventListener('mouseleave', () => {
            if (zoomContainer) {
                zoomContainer.style.display = 'none';
            }
        });
    } else if (zoomType === 'click') {
        img.addEventListener('click', () => {
            openImageModal(img.src);
        });
    }
    
    return true;
}

function createZoomContainer(img, zoomLevel) {
    const container = document.createElement('div');
    container.className = 'image-zoom-container';
    const zIndex = window.RRBlazor.ZIndexManager.registerElement('image-zoom-container', 'portal');
    container.style.cssText = `
        position: absolute;
        top: 0;
        left: 100%;
        margin-left: 10px;
        width: 400px;
        height: 400px;
        border: 2px solid var(--border-color);
        border-radius: var(--radius-md);
        overflow: hidden;
        display: none;
        z-index: ${zIndex};
        background-image: url('${img.src}');
        background-repeat: no-repeat;
        background-size: ${img.naturalWidth * zoomLevel}px ${img.naturalHeight * zoomLevel}px;
    `;

    return container;
}

function updateZoomPosition(e, element, img, zoomContainer, zoomLevel) {
    const rect = element.getBoundingClientRect();
    const x = e.clientX - rect.left;
    const y = e.clientY - rect.top;
    
    const xPercent = x / rect.width;
    const yPercent = y / rect.height;
    
    const bgX = -(xPercent * img.naturalWidth * zoomLevel - 200);
    const bgY = -(yPercent * img.naturalHeight * zoomLevel - 200);
    
    zoomContainer.style.backgroundPosition = `${bgX}px ${bgY}px`;
}

function openImageModal(src) {
    const event = new CustomEvent('RRBlazor:OpenImageModal', {
        detail: { src }
    });
    window.dispatchEvent(event);
}

export function cleanup(element) {
    if (!element) return;
    
    const img = element.querySelector('img');
    if (img) {
        img.onload = null;
        img.onerror = null;
    }
    
    const zoomContainer = element.querySelector('.image-zoom-container');
    if (zoomContainer) {
        window.RRBlazor.ZIndexManager.unregisterElement('image-zoom-container');
        zoomContainer.remove();
    }
}

export function getOptimalFormat() {
    const canvas = document.createElement('canvas');
    canvas.width = canvas.height = 1;
    
    if (canvas.toDataURL('image/webp').indexOf('image/webp') === 5) {
        return 'webp';
    }
    
    if (canvas.toDataURL('image/avif').indexOf('image/avif') === 5) {
        return 'avif';
    }
    
    return 'jpeg';
}

export function generateSrcset(baseSrc, sizes = [320, 640, 768, 1024, 1280, 1920]) {
    if (!baseSrc) return '';
    
    return sizes
        .map(size => {
            const url = baseSrc.replace(/\{width\}/g, size);
            return `${url} ${size}w`;
        })
        .join(', ');
}

export function calculateSizes(breakpoints = {}) {
    const defaultBreakpoints = {
        '640px': '100vw',
        '768px': '50vw',
        '1024px': '33vw',
        'default': '25vw'
    };
    
    const merged = { ...defaultBreakpoints, ...breakpoints };
    
    return Object.entries(merged)
        .filter(([key]) => key !== 'default')
        .map(([breakpoint, size]) => `(max-width: ${breakpoint}) ${size}`)
        .concat(merged.default || '100vw')
        .join(', ');
}

if (window.debugLogger?.isDebugMode) {
    window.RRBlazor = window.RRBlazor || {};
    window.RRBlazor.ImageModule = {
        preloadImage,
        initProgressiveImage,
        createBlurDataUrl,
        initImageZoom,
        getOptimalFormat,
        generateSrcset,
        calculateSizes,
        imageLoadingCache
    };
}