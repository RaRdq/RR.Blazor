// RFileUpload JavaScript Module
// Advanced file upload functionality with drag/drop, previews, and validation

// Use shared debug logger from RR.Blazor main file
const debugLogger = window.debugLogger || new (window.RRDebugLogger || class {
    constructor() { this.logPrefix = '[RFileUpload]'; }
    warn(...args) { console.warn(this.logPrefix, ...args); }
})();

export const RRFileUpload = {
    // Initialize file upload component
    initialize: function(elementId, options = {}) {
        const element = document.getElementById(elementId);
        if (!element) return;

        const settings = {
            allowedTypes: options.allowedTypes || [],
            maxSize: options.maxSize || 10 * 1024 * 1024, // 10MB
            maxFiles: options.maxFiles || 10,
            enableDragDrop: options.enableDragDrop !== false,
            showProgress: options.showProgress !== false,
            showPreview: options.showPreview !== false,
            generateThumbnails: options.generateThumbnails !== false,
            ...options
        };

        // Setup drag and drop
        if (settings.enableDragDrop) {
            this.setupDragDrop(element, settings);
        }

        // Setup file input handling
        this.setupFileInput(element, settings);

        // Store settings on element
        element._rrFileUploadSettings = settings;
    },

    // Setup drag and drop functionality
    setupDragDrop: function(element, settings) {
        const dropZone = element.querySelector('.file-upload-area, .upload-zone');
        if (!dropZone) return;

        let dragCounter = 0;

        const addDragoverClass = () => {
            dropZone.classList.add('upload-zone--dragover');
        };

        const removeDragoverClass = () => {
            dropZone.classList.remove('upload-zone--dragover');
        };

        // Prevent default drag behaviors
        ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
            dropZone.addEventListener(eventName, this.preventDefaults, false);
            document.body.addEventListener(eventName, this.preventDefaults, false);
        });

        // Highlight drop zone when dragging over it
        ['dragenter', 'dragover'].forEach(eventName => {
            dropZone.addEventListener(eventName, () => {
                addDragoverClass();
                if (eventName === 'dragenter') dragCounter++;
            }, false);
        });

        ['dragleave', 'drop'].forEach(eventName => {
            dropZone.addEventListener(eventName, () => {
                if (eventName === 'dragleave') {
                    dragCounter--;
                    if (dragCounter === 0) {
                        removeDragoverClass();
                    }
                } else {
                    dragCounter = 0;
                    removeDragoverClass();
                }
            }, false);
        });

        // Handle dropped files
        dropZone.addEventListener('drop', (e) => {
            const files = e.dataTransfer.files;
            this.handleFiles(files, element, settings);
        }, false);

        // Store cleanup function
        element._rrDropZoneCleanup = () => {
            ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
                dropZone.removeEventListener(eventName, this.preventDefaults, false);
            });
        };
    },

    // Setup file input handling
    setupFileInput: function(element, settings) {
        const fileInput = element.querySelector('input[type="file"]');
        if (!fileInput) return;

        fileInput.addEventListener('change', (e) => {
            this.handleFiles(e.target.files, element, settings);
        });
    },

    // Prevent default drag behaviors
    preventDefaults: function(e) {
        e.preventDefault();
        e.stopPropagation();
    },

    // Handle file selection/drop
    handleFiles: function(files, element, settings) {
        const fileArray = Array.from(files);
        const validFiles = [];
        const errors = [];

        // Validate files
        fileArray.forEach(file => {
            const validation = this.validateFile(file, settings);
            if (validation.valid) {
                validFiles.push(file);
            } else {
                errors.push(`${file.name}: ${validation.error}`);
            }
        });

        // Process valid files
        if (validFiles.length > 0) {
            this.processFiles(validFiles, element, settings);
        }

        // Report errors
        if (errors.length > 0) {
            this.showErrors(errors, element);
        }
    },

    // Validate individual file
    validateFile: function(file, settings) {
        // Check file size
        if (file.size > settings.maxSize) {
            return {
                valid: false,
                error: `File size (${this.formatFileSize(file.size)}) exceeds maximum allowed (${this.formatFileSize(settings.maxSize)})`
            };
        }

        // Check file type
        if (settings.allowedTypes && settings.allowedTypes.length > 0) {
            const extension = '.' + file.name.split('.').pop().toLowerCase();
            const isAllowed = settings.allowedTypes.some(type => 
                type.toLowerCase() === extension || 
                type.toLowerCase() === file.type.toLowerCase()
            );
            
            if (!isAllowed) {
                return {
                    valid: false,
                    error: `File type not allowed. Allowed types: ${settings.allowedTypes.join(', ')}`
                };
            }
        }

        return { valid: true };
    },

    // Process valid files
    processFiles: async function(files, element, settings) {
        const fileInfos = [];

        for (const file of files) {
            const fileInfo = {
                id: this.generateId(),
                name: file.name,
                extension: '.' + file.name.split('.').pop().toLowerCase(),
                size: file.size,
                contentType: file.type,
                lastModified: new Date(file.lastModified),
                isImage: file.type.startsWith('image/'),
                status: 'Pending'
            };

            // Generate thumbnail for images
            if (fileInfo.isImage && settings.generateThumbnails) {
                try {
                    fileInfo.thumbnailUrl = await this.generateThumbnail(file);
                } catch (error) {
                    debugLogger.warn('Failed to generate thumbnail:', error);
                }
            }

            fileInfos.push(fileInfo);
        }

        // Trigger Blazor callback
        const event = new CustomEvent('rr-files-selected', {
            detail: { files: fileInfos, originalFiles: files }
        });
        element.dispatchEvent(event);
    },

    // Generate thumbnail for image files
    generateThumbnail: function(file, maxWidth = 150, maxHeight = 150) {
        return new Promise((resolve, reject) => {
            const canvas = document.createElement('canvas');
            const ctx = canvas.getContext('2d');
            const img = new Image();

            img.onload = function() {
                // Calculate dimensions
                let { width, height } = img;
                
                if (width > height) {
                    if (width > maxWidth) {
                        height = (height * maxWidth) / width;
                        width = maxWidth;
                    }
                } else {
                    if (height > maxHeight) {
                        width = (width * maxHeight) / height;
                        height = maxHeight;
                    }
                }

                canvas.width = width;
                canvas.height = height;

                // Draw and export
                ctx.drawImage(img, 0, 0, width, height);
                resolve(canvas.toDataURL('image/jpeg', 0.8));
            };

            img.onerror = reject;
            img.src = URL.createObjectURL(file);
        });
    },

    // Show validation errors
    showErrors: function(errors, element) {
        const errorContainer = element.querySelector('.file-upload-errors');
        if (errorContainer) {
            // Clear existing content
            errorContainer.innerHTML = '';
            
            // Create error elements safely
            errors.forEach(error => {
                const errorDiv = document.createElement('div');
                errorDiv.className = 'd-flex align-center text-xs text-error mb-1';
                
                const icon = document.createElement('i');
                icon.className = 'material-symbols-rounded text-xs mr-1';
                icon.textContent = 'error';
                
                const errorText = document.createTextNode(error);
                
                errorDiv.appendChild(icon);
                errorDiv.appendChild(errorText);
                errorContainer.appendChild(errorDiv);
            });
            
            errorContainer.style.display = 'block';
        }
    },

    // Clear errors
    clearErrors: function(element) {
        const errorContainer = element.querySelector('.file-upload-errors');
        if (errorContainer) {
            errorContainer.innerHTML = '';
            errorContainer.style.display = 'none';
        }
    },

    // Update upload progress
    updateProgress: function(elementId, fileId, progress) {
        const element = document.getElementById(elementId);
        if (!element) return;

        const progressBar = element.querySelector(`[data-file-id="${fileId}"] .upload-progress__bar__fill`);
        if (progressBar) {
            progressBar.style.width = `${progress}%`;
        }

        const progressText = element.querySelector(`[data-file-id="${fileId}"] .upload-progress__text`);
        if (progressText) {
            progressText.textContent = `${Math.round(progress)}% uploaded`;
        }
    },

    // Remove file from list
    removeFile: function(elementId, fileId) {
        const element = document.getElementById(elementId);
        if (!element) return;

        const fileElement = element.querySelector(`[data-file-id="${fileId}"]`);
        if (fileElement) {
            fileElement.remove();
        }

        // Trigger removal event
        const event = new CustomEvent('rr-file-removed', {
            detail: { fileId }
        });
        element.dispatchEvent(event);
    },

    // Utility functions
    formatFileSize: function(bytes) {
        if (bytes === 0) return '0 Bytes';
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    },

    generateId: function() {
        return Math.random().toString(36).substr(2, 9);
    },

    escapeHtml: function(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    },

    // Get file icon based on extension
    getFileIcon: function(extension) {
        const iconMap = {
            // Documents
            '.pdf': 'picture_as_pdf',
            '.doc': 'description',
            '.docx': 'description',
            '.txt': 'description',
            '.rtf': 'description',
            
            // Spreadsheets
            '.xls': 'grid_on',
            '.xlsx': 'grid_on',
            '.csv': 'grid_on',
            
            // Presentations
            '.ppt': 'slideshow',
            '.pptx': 'slideshow',
            
            // Images
            '.jpg': 'image',
            '.jpeg': 'image',
            '.png': 'image',
            '.gif': 'image',
            '.bmp': 'image',
            '.svg': 'image',
            '.webp': 'image',
            
            // Audio
            '.mp3': 'audio_file',
            '.wav': 'audio_file',
            '.flac': 'audio_file',
            '.aac': 'audio_file',
            
            // Video
            '.mp4': 'video_file',
            '.avi': 'video_file',
            '.mov': 'video_file',
            '.wmv': 'video_file',
            
            // Archives
            '.zip': 'folder_zip',
            '.rar': 'folder_zip',
            '.7z': 'folder_zip',
            '.tar': 'folder_zip',
            
            // Code
            '.html': 'code',
            '.css': 'code',
            '.js': 'code',
            '.json': 'code',
            '.xml': 'code',
            '.cs': 'code',
            '.py': 'code',
            '.sql': 'code'
        };

        return iconMap[extension.toLowerCase()] || 'draft';
    },

    // Cleanup component
    cleanup: function(elementId) {
        const element = document.getElementById(elementId);
        if (element) {
            if (element._rrDropZoneCleanup) {
                element._rrDropZoneCleanup();
                delete element._rrDropZoneCleanup;
            }
            delete element._rrFileUploadSettings;
        }
    }
};

// Also expose on window for non-module usage
window.RRFileUpload = RRFileUpload;