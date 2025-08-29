
export const RRFileUpload = {
    initialize: function(elementId, options = {}) {
        const element = document.getElementById(elementId);

        const settings = {
            allowedTypes: options.allowedTypes || [],
            maxSize: options.maxSize || 10 * 1024 * 1024,
            maxFiles: options.maxFiles || 10,
            enableDragDrop: options.enableDragDrop !== false,
            showProgress: options.showProgress !== false,
            showPreview: options.showPreview !== false,
            generateThumbnails: options.generateThumbnails !== false,
            ...options
        };

        if (settings.enableDragDrop) {
            this.setupDragDrop(element, settings);
        }

        this.setupFileInput(element, settings);
        element._rrFileUploadSettings = settings;
    },

    setupDragDrop: function(element, settings) {
        const dropZone = element.querySelector('.upload-area, .file-preview-container');

        let dragCounter = 0;
        const dragOverlay = element.querySelector('.upload-drag-overlay');

        const showDragOverlay = () => {
            if (dragOverlay) {
                dragOverlay.classList.remove('hidden');
                dragOverlay.style.animation = 'dragOverlay 0.2s ease-out';
            }
            dropZone.classList.add('upload-zone-dragover');
        };

        const hideDragOverlay = () => {
            if (dragOverlay) {
                dragOverlay.classList.add('hidden');
            }
            dropZone.classList.remove('upload-zone-dragover');
        };

        const updateDragOverlayContent = (isMultiple, hasFiles) => {
            if (!dragOverlay) return;
            
            const icon = dragOverlay.querySelector('.upload-drag-icon');
            const text = dragOverlay.querySelector('.upload-drag-text');
            const hint = dragOverlay.querySelector('.upload-drag-hint');
            
            if (hasFiles) {
                if (isMultiple) {
                    if (icon) icon.textContent = 'add';
                    if (text) text.textContent = 'Add More Files';
                    if (hint) {
                        const remaining = Math.max(0, settings.maxFiles - this.getCurrentFileCount(element));
                        hint.textContent = remaining > 0 ? `You can add up to ${remaining} more files` : 'Maximum files reached';
                    }
                } else {
                    if (icon) icon.textContent = 'swap_horiz';
                    if (text) text.textContent = 'Replace File';
                    if (hint) hint.textContent = 'This will replace the current file';
                }
            } else {
                if (icon) icon.textContent = 'cloud_upload';
                if (text) text.textContent = isMultiple ? 'Drop Files Here' : 'Drop File Here';
                if (hint) hint.textContent = 'Or click to browse';
            }
        };

        ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
            dropZone.addEventListener(eventName, this.preventDefaults, false);
            document.body.addEventListener(eventName, this.preventDefaults, false);
        });

        ['dragenter', 'dragover'].forEach(eventName => {
            dropZone.addEventListener(eventName, (e) => {
                if (eventName === 'dragenter') {
                    dragCounter++;
                    const hasFiles = this.getCurrentFileCount(element) > 0;
                    updateDragOverlayContent(settings.multiple !== false, hasFiles);
                }
                showDragOverlay();
            }, false);
        });

        ['dragleave', 'drop'].forEach(eventName => {
            dropZone.addEventListener(eventName, (e) => {
                if (eventName === 'dragleave') {
                    dragCounter--;
                    if (dragCounter === 0) {
                        hideDragOverlay();
                    }
                } else {
                    dragCounter = 0;
                    hideDragOverlay();
                }
            }, false);
        });

        dropZone.addEventListener('drop', (e) => {
            const files = e.dataTransfer.files;
            this.handleFiles(files, element, settings);
        }, false);

        element._rrDropZoneCleanup = () => {
            ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
                dropZone.removeEventListener(eventName, this.preventDefaults, false);
            });
        };
    },

    setupFileInput: function(element, settings) {
        const fileInput = element.querySelector('input[type="file"]');

        fileInput.addEventListener('change', (e) => {
            this.handleFiles(e.target.files, element, settings);
        });
    },

    preventDefaults: function(e) {
        e.preventDefault();
        e.stopPropagation();
    },

    handleFiles: function(files, element, settings) {
        const fileArray = Array.from(files);
        const validFiles = [];
        const errors = [];

        fileArray.forEach(file => {
            const validation = this.validateFile(file, settings);
            if (validation.valid) {
                validFiles.push(file);
            } else {
                errors.push(`${file.name}: ${validation.error}`);
            }
        });

        if (validFiles.length > 0) {
            this.processFiles(validFiles, element, settings);
        }

        if (errors.length > 0) {
            this.showErrors(errors, element);
        }
    },

    validateFile: function(file, settings) {
        if (file.size > settings.maxSize) {
            return {
                valid: false,
                error: `File size (${this.formatFileSize(file.size)}) exceeds maximum allowed (${this.formatFileSize(settings.maxSize)})`
            };
        }

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

            if (fileInfo.isImage && settings.generateThumbnails) {
                try {
                    fileInfo.thumbnailUrl = await this.generateThumbnail(file);
                } catch (error) {
                }
            }

            fileInfos.push(fileInfo);
        }

        window.RRBlazor.EventDispatcher.dispatch(
            'rr-files-selected',
            { files: fileInfos, originalFiles: files }
        );
        element.dispatchEvent(new Event('rr-files-selected'));
    },

    generateThumbnail: function(file, maxWidth = 150, maxHeight = 150) {
        return new Promise((resolve, reject) => {
            const canvas = document.createElement('canvas');
            const ctx = canvas.getContext('2d');
            const img = new Image();

            img.onload = function() {
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

                ctx.drawImage(img, 0, 0, width, height);
                resolve(canvas.toDataURL('image/jpeg', 0.8));
            };

            img.onerror = reject;
            img.src = URL.createObjectURL(file);
        });
    },

    showErrors: function(errors, element) {
        const errorContainer = element.querySelector('.file-upload-errors');
        if (errorContainer) {
            errorContainer.innerHTML = '';
            
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

    clearErrors: function(element) {
        const errorContainer = element.querySelector('.file-upload-errors');
        if (errorContainer) {
            errorContainer.innerHTML = '';
            errorContainer.style.display = 'none';
        }
    },

    updateProgress: function(elementId, fileId, progress) {
        if (!elementId || !fileId) return;
        
        const element = document.getElementById(elementId);

        const progressBar = element.querySelector(`[data-file-id="${fileId}"] .upload-progress__bar__fill`);
        if (progressBar) {
            progressBar.style.width = `${progress}%`;
        }

        const progressText = element.querySelector(`[data-file-id="${fileId}"] .upload-progress__text`);
        if (progressText) {
            progressText.textContent = `${Math.round(progress)}% uploaded`;
        }
    },

    removeFile: function(elementId, fileId) {
        if (!elementId || !fileId) return;
        
        const element = document.getElementById(elementId);

        const fileElement = element.querySelector(`[data-file-id="${fileId}"]`);
        if (fileElement) {
            fileElement.remove();
        }

        window.RRBlazor.EventDispatcher.dispatch(
            'rr-file-removed',
            { fileId }
        );
        element.dispatchEvent(new Event('rr-file-removed'));
    },

    formatFileSize: function(bytes) {
        if (bytes === 0) return '0 Bytes';
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    },

    generateId: function() {
        return Math.random().toString(36).substring(2, 11);
    },

    escapeHtml: function(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    },

    getFileIcon: function(extension) {
        const iconMap = {
            '.pdf': 'picture_as_pdf',
            '.doc': 'description',
            '.docx': 'description',
            '.txt': 'description',
            '.rtf': 'description',
            '.xls': 'grid_on',
            '.xlsx': 'grid_on',
            '.csv': 'grid_on',
            '.ppt': 'slideshow',
            '.pptx': 'slideshow',
            '.jpg': 'image',
            '.jpeg': 'image',
            '.png': 'image',
            '.gif': 'image',
            '.bmp': 'image',
            '.svg': 'image',
            '.webp': 'image',
            '.mp3': 'audio_file',
            '.wav': 'audio_file',
            '.flac': 'audio_file',
            '.aac': 'audio_file',
            '.mp4': 'video_file',
            '.avi': 'video_file',
            '.mov': 'video_file',
            '.wmv': 'video_file',
            '.zip': 'folder_zip',
            '.rar': 'folder_zip',
            '.7z': 'folder_zip',
            '.tar': 'folder_zip',
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

    triggerFileSelect: function(inputId) {
        const fileInput = document.getElementById(inputId);
        
        fileInput.click();
    },

    getCurrentFileCount: function(element) {
        const fileCards = element.querySelectorAll('.file-preview-card');
        return fileCards.length;
    },

    removeFileWithAnimation: function(elementId, fileId) {
        const element = document.getElementById(elementId);
        if (!element) return;

        const fileCard = element.querySelector(`[data-file-id="${fileId}"]`);
        if (fileCard) {
            fileCard.style.transform = 'scale(0.8)';
            fileCard.style.opacity = '0';
            fileCard.style.transition = 'all 0.2s ease-out';
            
            fileCard.addEventListener('transitionend', function onTransitionEnd() {
                fileCard.remove();
                
                const remainingFiles = this.getCurrentFileCount(element);
                if (remainingFiles === 0) {
                    this.showEmptyState(element);
                }
            }.bind(this), { once: true });
        }

        window.RRBlazor.EventDispatcher.dispatch(
            'rr-file-removed',
            { fileId }
        );
        element.dispatchEvent(new Event('rr-file-removed'));
    },

    showEmptyState: function(element) {
        const previewContainer = element.querySelector('.file-preview-container');
        const uploadArea = element.querySelector('.upload-area');
        
        if (previewContainer && uploadArea) {
            previewContainer.style.opacity = '0';
            previewContainer.addEventListener('transitionend', function onTransitionEnd() {
                previewContainer.style.display = 'none';
                uploadArea.style.display = 'block';
                uploadArea.style.opacity = '0';
                requestAnimationFrame(() => {
                    uploadArea.style.transition = 'opacity 0.3s ease-in';
                    uploadArea.style.opacity = '1';
                });
            }, { once: true });
        }
    },

    showPreviewState: function(element) {
        const previewContainer = element.querySelector('.file-preview-container');
        const uploadArea = element.querySelector('.upload-area');
        
        if (previewContainer && uploadArea && uploadArea.style.display !== 'none') {
            uploadArea.style.opacity = '0';
            uploadArea.addEventListener('transitionend', function onTransitionEnd() {
                uploadArea.style.display = 'none';
                previewContainer.style.display = 'block';
                previewContainer.style.opacity = '0';
                requestAnimationFrame(() => {
                    previewContainer.style.transition = 'opacity 0.3s ease-in';
                    previewContainer.style.opacity = '1';
                });
            }, { once: true });
        }
    },

    setupBlazorEventListeners: function(elementId, dotNetObjectRef) {
        const element = document.getElementById(elementId);

        const filesSelectedHandler = function(e) {
            dotNetObjectRef.invokeMethodAsync('OnFilesSelectedFromJS', e.detail);
        };
        element.addEventListener('rr-files-selected', filesSelectedHandler);

        const fileRemovedHandler = function(e) {
            dotNetObjectRef.invokeMethodAsync('OnFileRemovedFromJS', e.detail.fileId);
        };
        element.addEventListener('rr-file-removed', fileRemovedHandler);

        element._rrBlazorEventCleanup = () => {
            element.removeEventListener('rr-files-selected', filesSelectedHandler);
            element.removeEventListener('rr-file-removed', fileRemovedHandler);
        };
    },

    cleanup: function(elementId) {
        if (!elementId) return;
        
        const element = document.getElementById(elementId);
        if (element) {
            if (element._rrDropZoneCleanup) {
                element._rrDropZoneCleanup();
                delete element._rrDropZoneCleanup;
            }
            if (element._rrBlazorEventCleanup) {
                element._rrBlazorEventCleanup();
                delete element._rrBlazorEventCleanup;
            }
            delete element._rrFileUploadSettings;
        }
    },

    init: function(dotNetObjectRef) {
        this._dotNetObjectRef = dotNetObjectRef;
        return true;
    }
};

window.RRFileUpload = RRFileUpload;

export default RRFileUpload;