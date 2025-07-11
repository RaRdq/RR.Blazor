// Enhanced RFileUpload JavaScript Module
// Integrates PayrollAI's battle-tested file upload capabilities with RR.Blazor design system
// Provides chunked uploads, AI analysis, advanced validation, and real-time progress tracking

// Core functionality adapted from PayrollAI.Shared.Client/wwwroot/js/fileUpload.js
export class EnhancedRFileUpload {
    constructor(dotNetHelper, elementId, options = {}) {
        this.dotNetHelper = dotNetHelper;
        this.elementId = elementId;
        this.element = document.getElementById(elementId);
        this.options = {
            // Chunked upload settings
            enableChunkedUpload: true,
            chunkThreshold: 50 * 1024 * 1024, // 50MB
            chunkSize: 5 * 1024 * 1024, // 5MB
            maxRetries: 3,
            
            // AI Analysis settings
            enableAnalysis: false,
            enableCategorization: false,
            
            // Security and validation
            enableAdvancedValidation: true,
            dangerousExtensions: ['.exe', '.bat', '.cmd', '.scr', '.vbs', '.js', '.jar'],
            
            // Standard RFileUpload settings
            allowedTypes: [],
            maxSize: 10 * 1024 * 1024,
            maxFiles: 10,
            enableDragDrop: true,
            showProgress: true,
            showPreview: true,
            generateThumbnails: true,
            
            ...options
        };
        
        this.activeUploads = new Map();
        this.fileQueue = [];
        this.uploadStats = {
            totalFiles: 0,
            completedFiles: 0,
            failedFiles: 0,
            bytesUploaded: 0,
            totalBytes: 0
        };
    }

    // Initialize enhanced file upload
    async initialize() {
        if (!this.element) {
            console.error(`Enhanced file upload element not found: ${this.elementId}`);
            return;
        }

        // Setup drag and drop with enhanced features
        if (this.options.enableDragDrop) {
            this.setupEnhancedDragDrop();
        }

        // Setup file input handling
        this.setupEnhancedFileInput();

        // Setup progress tracking
        this.setupProgressTracking();

        console.log('Enhanced RFileUpload initialized with options:', this.options);
    }

    // Enhanced drag and drop with better UX
    setupEnhancedDragDrop() {
        const dropZone = this.element.querySelector('.file-upload-area, .upload-zone');
        if (!dropZone) return;

        let dragCounter = 0;
        let dragTimer;

        const addDragEffect = () => {
            dropZone.classList.add('upload-zone--dragover', 'upload-zone--enhanced');
            clearTimeout(dragTimer);
        };

        const removeDragEffect = () => {
            dragTimer = setTimeout(() => {
                dropZone.classList.remove('upload-zone--dragover', 'upload-zone--enhanced');
            }, 100);
        };

        // Prevent default behaviors
        ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
            dropZone.addEventListener(eventName, this.preventDefaults, false);
            document.body.addEventListener(eventName, this.preventDefaults, false);
        });

        // Enhanced drag feedback
        ['dragenter', 'dragover'].forEach(eventName => {
            dropZone.addEventListener(eventName, (e) => {
                addDragEffect();
                if (eventName === 'dragenter') dragCounter++;
                
                // Show file count preview
                if (e.dataTransfer?.items) {
                    const fileCount = Array.from(e.dataTransfer.items).filter(item => item.kind === 'file').length;
                    this.showDragPreview(fileCount);
                }
            }, false);
        });

        ['dragleave', 'drop'].forEach(eventName => {
            dropZone.addEventListener(eventName, (e) => {
                if (eventName === 'dragleave') {
                    dragCounter--;
                    if (dragCounter === 0) {
                        removeDragEffect();
                        this.hideDragPreview();
                    }
                } else {
                    dragCounter = 0;
                    removeDragEffect();
                    this.hideDragPreview();
                    this.handleEnhancedFileSelection(e.dataTransfer.files);
                }
            }, false);
        });
    }

    // Setup enhanced file input handling
    setupEnhancedFileInput() {
        const fileInput = this.element.querySelector('input[type="file"]');
        if (!fileInput) return;

        fileInput.addEventListener('change', (e) => {
            this.handleEnhancedFileSelection(e.target.files);
        });
    }

    // Setup progress tracking for multiple files and chunks
    setupProgressTracking() {
        this.progressUpdateInterval = setInterval(() => {
            this.updateOverallProgress();
        }, 250);
    }

    // Enhanced file selection with validation and analysis
    async handleEnhancedFileSelection(files) {
        if (!files || files.length === 0) return;

        const fileArray = Array.from(files);
        const validatedFiles = [];
        const errors = [];

        // Enhanced validation for each file
        for (const file of fileArray) {
            const validation = await this.validateFileEnhanced(file);
            if (validation.isValid) {
                validatedFiles.push({
                    file,
                    validation: validation.fileInfo,
                    id: this.generateFileId()
                });
            } else {
                errors.push({
                    fileName: file.name,
                    errors: validation.errors,
                    warnings: validation.warnings
                });
            }
        }

        // Process valid files
        if (validatedFiles.length > 0) {
            await this.processValidatedFiles(validatedFiles);
        }

        // Report validation errors
        if (errors.length > 0) {
            await this.reportValidationErrors(errors);
        }
    }

    // Enhanced file validation with security checks
    async validateFileEnhanced(file) {
        const errors = [];
        const warnings = [];

        // Basic size validation
        if (file.size > this.options.maxSize) {
            errors.push(`File size (${this.formatFileSize(file.size)}) exceeds maximum allowed size (${this.formatFileSize(this.options.maxSize)})`);
        }

        if (file.size === 0) {
            errors.push('File is empty');
        }

        // Enhanced type validation
        const extension = this.getFileExtension(file.name);
        if (this.options.allowedTypes?.length > 0) {
            const isAllowed = this.options.allowedTypes.some(type => 
                type.toLowerCase() === extension || 
                type.toLowerCase() === file.type.toLowerCase()
            );
            
            if (!isAllowed) {
                errors.push(`File type '${extension}' is not allowed. Allowed types: ${this.options.allowedTypes.join(', ')}`);
            }
        }

        // Security validation
        if (this.options.enableAdvancedValidation) {
            if (this.options.dangerousExtensions.some(ext => file.name.toLowerCase().endsWith(ext))) {
                errors.push('File type is not allowed for security reasons');
            }

            // File name validation
            if (file.name.length > 255) {
                errors.push('File name is too long (max 255 characters)');
            }

            if (!/^[a-zA-Z0-9._\-\s]+$/.test(file.name)) {
                warnings.push('File name contains special characters that may cause issues');
            }
        }

        // Extract comprehensive metadata
        const fileInfo = await this.extractFileMetadata(file);

        return {
            isValid: errors.length === 0,
            errors,
            warnings,
            fileInfo
        };
    }

    // Process validated files with optional AI analysis
    async processValidatedFiles(validatedFiles) {
        const fileInfos = [];

        for (const validatedFile of validatedFiles) {
            const { file, validation, id } = validatedFile;
            
            const fileInfo = {
                id,
                name: file.name,
                extension: this.getFileExtension(file.name),
                size: file.size,
                contentType: file.type,
                lastModified: new Date(file.lastModified),
                isImage: file.type.startsWith('image/'),
                status: 'Pending',
                uploadProgress: 0,
                metadata: validation,
                analysis: null
            };

            // Generate thumbnail for images
            if (fileInfo.isImage && this.options.generateThumbnails) {
                try {
                    fileInfo.thumbnailUrl = await this.generateThumbnail(file);
                } catch (error) {
                    console.warn('Failed to generate thumbnail:', error);
                }
            }

            // AI content analysis if enabled
            if (this.options.enableAnalysis) {
                try {
                    fileInfo.analysis = await this.analyzeFileContent(file);
                    await this.dotNetHelper.invokeMethodAsync('OnFileAnalyzed', fileInfo.id, fileInfo.analysis);
                } catch (error) {
                    console.warn('File analysis failed:', error);
                }
            }

            fileInfos.push(fileInfo);
        }

        // Add to upload queue
        this.fileQueue.push(...fileInfos);
        this.uploadStats.totalFiles += fileInfos.length;
        this.uploadStats.totalBytes += fileInfos.reduce((sum, f) => sum + f.size, 0);

        // Notify Blazor component
        await this.dotNetHelper.invokeMethodAsync('OnFilesValidated', fileInfos);
    }

    // AI-powered content analysis
    async analyzeFileContent(file) {
        try {
            const metadata = await this.extractFileMetadata(file);
            const contentType = this.detectContentType(file);
            const processingTime = this.estimateProcessingTime(file);
            
            let contentPreview = null;
            let suggestedCategory = 'Other';
            let confidence = 0;

            // Text-based analysis
            if (file.type === 'text/plain' || file.name.endsWith('.txt')) {
                contentPreview = await this.readFileAsText(file);
                const analysis = this.analyzeTextContent(contentPreview);
                suggestedCategory = analysis.category;
                confidence = analysis.confidence;
            } else if (file.type === 'application/pdf') {
                suggestedCategory = 'Document';
                confidence = 0.7;
            } else if (file.type.startsWith('image/')) {
                suggestedCategory = 'Identity';
                confidence = 0.6;
            } else if (file.type.includes('spreadsheet') || file.name.match(/\.(xlsx?|csv)$/i)) {
                suggestedCategory = 'Report';
                confidence = 0.8;
            }

            return {
                metadata,
                contentType,
                processingTime,
                contentPreview: contentPreview ? contentPreview.substring(0, 500) : null,
                suggestedCategory,
                confidence,
                timestamp: new Date().toISOString()
            };
        } catch (error) {
            console.error('File analysis failed:', error);
            return {
                metadata: await this.extractFileMetadata(file),
                contentType: 'unknown',
                processingTime: 0,
                contentPreview: null,
                suggestedCategory: 'Other',
                confidence: 0,
                error: error.message
            };
        }
    }

    // Enhanced file upload with chunking support
    async uploadFile(fileInfo, uploadUrl, options = {}) {
        const file = fileInfo.file || this.fileQueue.find(f => f.id === fileInfo.id)?.file;
        if (!file) {
            throw new Error('File not found for upload');
        }

        // Determine upload method
        const useChunking = this.options.enableChunkedUpload && 
                          file.size > this.options.chunkThreshold;

        try {
            fileInfo.status = 'Uploading';
            await this.dotNetHelper.invokeMethodAsync('OnUploadStarted', fileInfo.id);

            let result;
            if (useChunking) {
                result = await this.chunkUpload(file, uploadUrl, fileInfo);
            } else {
                result = await this.standardUpload(file, uploadUrl, fileInfo);
            }

            fileInfo.status = 'Completed';
            fileInfo.uploadProgress = 100;
            this.uploadStats.completedFiles++;
            
            await this.dotNetHelper.invokeMethodAsync('OnUploadCompleted', fileInfo.id, result);
            return result;

        } catch (error) {
            fileInfo.status = 'Failed';
            fileInfo.errorMessage = error.message;
            this.uploadStats.failedFiles++;
            
            await this.dotNetHelper.invokeMethodAsync('OnUploadFailed', fileInfo.id, error.message);
            throw error;
        }
    }

    // Chunked upload for large files
    async chunkUpload(file, uploadUrl, fileInfo) {
        const chunkSize = this.options.chunkSize;
        const totalChunks = Math.ceil(file.size / chunkSize);
        let uploadedChunks = 0;
        
        const uploadId = this.generateUploadId();
        this.activeUploads.set(fileInfo.id, { uploadId, cancelled: false });

        for (let chunkIndex = 0; chunkIndex < totalChunks; chunkIndex++) {
            // Check for cancellation
            if (this.activeUploads.get(fileInfo.id)?.cancelled) {
                throw new Error('Upload cancelled');
            }

            const start = chunkIndex * chunkSize;
            const end = Math.min(start + chunkSize, file.size);
            const chunk = file.slice(start, end);
            
            let retryCount = 0;
            let chunkUploaded = false;

            while (!chunkUploaded && retryCount < this.options.maxRetries) {
                try {
                    await this.uploadChunk(chunk, chunkIndex, totalChunks, uploadId, uploadUrl, fileInfo);
                    chunkUploaded = true;
                    uploadedChunks++;
                    
                    const progress = (uploadedChunks / totalChunks) * 100;
                    fileInfo.uploadProgress = progress;
                    this.uploadStats.bytesUploaded += chunk.size;
                    
                    await this.dotNetHelper.invokeMethodAsync('OnChunkUploaded', fileInfo.id, chunkIndex, totalChunks, progress);
                } catch (error) {
                    retryCount++;
                    if (retryCount >= this.options.maxRetries) {
                        throw new Error(`Chunk ${chunkIndex} failed after ${this.options.maxRetries} retries: ${error.message}`);
                    }
                    
                    // Exponential backoff
                    await this.delay(1000 * Math.pow(2, retryCount - 1));
                }
            }
        }

        // Complete the chunked upload
        return this.completeChunkedUpload(uploadId, uploadUrl, fileInfo);
    }

    // Upload individual chunk
    async uploadChunk(chunk, chunkIndex, totalChunks, uploadId, uploadUrl, fileInfo) {
        const formData = new FormData();
        formData.append('chunk', chunk);
        formData.append('chunkIndex', chunkIndex.toString());
        formData.append('totalChunks', totalChunks.toString());
        formData.append('uploadId', uploadId);
        formData.append('fileName', fileInfo.name);
        formData.append('fileSize', fileInfo.size.toString());

        const response = await fetch(`${uploadUrl}/chunk`, {
            method: 'POST',
            body: formData
        });

        if (!response.ok) {
            throw new Error(`Chunk upload failed: HTTP ${response.status}`);
        }

        return response.json();
    }

    // Complete chunked upload
    async completeChunkedUpload(uploadId, uploadUrl, fileInfo) {
        const response = await fetch(`${uploadUrl}/complete`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                uploadId,
                fileName: fileInfo.name,
                fileSize: fileInfo.size
            })
        });

        if (!response.ok) {
            throw new Error(`Failed to complete chunked upload: HTTP ${response.status}`);
        }

        return response.json();
    }

    // Standard upload for smaller files
    async standardUpload(file, uploadUrl, fileInfo) {
        return new Promise((resolve, reject) => {
            const formData = new FormData();
            formData.append('file', file);
            
            const xhr = new XMLHttpRequest();
            
            xhr.upload.addEventListener('progress', (e) => {
                if (e.lengthComputable) {
                    const progress = (e.loaded / e.total) * 100;
                    fileInfo.uploadProgress = progress;
                    this.uploadStats.bytesUploaded = e.loaded;
                    this.dotNetHelper.invokeMethodAsync('OnProgressChanged', fileInfo.id, progress);
                }
            });
            
            xhr.addEventListener('load', () => {
                if (xhr.status === 200) {
                    try {
                        resolve(JSON.parse(xhr.responseText));
                    } catch {
                        resolve(xhr.responseText);
                    }
                } else {
                    reject(new Error(`Upload failed: HTTP ${xhr.status}`));
                }
            });
            
            xhr.addEventListener('error', () => {
                reject(new Error('Upload failed due to network error'));
            });
            
            xhr.open('POST', uploadUrl);
            xhr.send(formData);
            
            // Store xhr for cancellation
            this.activeUploads.set(fileInfo.id, { xhr, cancelled: false });
        });
    }

    // Cancel upload
    async cancelUpload(fileId) {
        const upload = this.activeUploads.get(fileId);
        if (!upload) return;

        upload.cancelled = true;
        
        if (upload.xhr) {
            upload.xhr.abort();
        }
        
        this.activeUploads.delete(fileId);
        await this.dotNetHelper.invokeMethodAsync('OnUploadCancelled', fileId);
    }

    // Utility: Generate file thumbnail
    generateThumbnail(file, maxWidth = 150, maxHeight = 150) {
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
    }

    // Utility: Extract comprehensive file metadata
    async extractFileMetadata(file) {
        return {
            name: file.name,
            size: file.size,
            type: file.type,
            lastModified: new Date(file.lastModified),
            extension: this.getFileExtension(file.name),
            sizeFormatted: this.formatFileSize(file.size),
            contentType: this.detectContentType(file),
            estimatedProcessingTime: this.estimateProcessingTime(file),
            webkitRelativePath: file.webkitRelativePath || '',
            isImage: file.type.startsWith('image/'),
            isDocument: ['application/pdf', 'application/msword', 'application/vnd.openxmlformats-officedocument.wordprocessingml.document'].includes(file.type),
            isSpreadsheet: file.type.includes('spreadsheet') || file.name.match(/\.(xlsx?|csv)$/i),
            isArchive: ['application/zip', 'application/x-rar-compressed', 'application/x-7z-compressed'].includes(file.type)
        };
    }

    // Utility: Analyze text content for categorization
    analyzeTextContent(text) {
        const keywords = {
            'Policy': ['policy', 'procedure', 'guidelines', 'rules', 'regulations', 'compliance'],
            'Contract': ['agreement', 'contract', 'terms', 'conditions', 'obligations', 'parties'],
            'Invoice': ['invoice', 'bill', 'payment', 'amount', 'due', 'total', 'tax'],
            'Report': ['report', 'summary', 'analysis', 'findings', 'results', 'data'],
            'Training': ['training', 'course', 'learning', 'education', 'certification', 'skills'],
            'Certificate': ['certificate', 'certification', 'diploma', 'achievement', 'completion'],
            'Identity': ['passport', 'license', 'identification', 'id', 'social security', 'birth certificate']
        };
        
        const textLower = text.toLowerCase();
        const scores = {};
        
        for (const [category, words] of Object.entries(keywords)) {
            scores[category] = words.reduce((score, word) => {
                const regex = new RegExp(`\\b${word}\\b`, 'gi');
                const matches = textLower.match(regex);
                return score + (matches ? matches.length : 0);
            }, 0);
        }
        
        const bestCategory = Object.entries(scores).reduce((best, [category, score]) => {
            return score > best.score ? { category, score } : best;
        }, { category: 'Other', score: 0 });
        
        const totalWords = text.split(/\s+/).length;
        const confidence = Math.min(bestCategory.score / Math.max(totalWords * 0.1, 1), 1);
        
        return {
            category: bestCategory.category,
            confidence: Math.round(confidence * 100) / 100,
            scores
        };
    }

    // Utility functions
    readFileAsText(file, encoding = 'utf-8') {
        return new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.onload = (e) => resolve(e.target.result);
            reader.onerror = () => reject(new Error('Failed to read file as text'));
            reader.readAsText(file, encoding);
        });
    }

    detectContentType(file) {
        const extension = this.getFileExtension(file.name);
        const typeMap = {
            '.pdf': 'document', '.doc': 'document', '.docx': 'document', '.txt': 'text',
            '.csv': 'spreadsheet', '.xlsx': 'spreadsheet', '.xls': 'spreadsheet',
            '.jpg': 'image', '.jpeg': 'image', '.png': 'image', '.gif': 'image', '.bmp': 'image',
            '.zip': 'archive', '.rar': 'archive', '.7z': 'archive',
            '.mp4': 'video', '.avi': 'video', '.mov': 'video',
            '.mp3': 'audio', '.wav': 'audio'
        };
        return typeMap[extension] || 'unknown';
    }

    estimateProcessingTime(file) {
        const baseTime = 2;
        const sizeMultiplier = file.size / (1024 * 1024);
        const typeMultipliers = {
            'application/pdf': 1.5, 'image/jpeg': 1.2, 'image/png': 1.2,
            'application/msword': 1.3, 'text/plain': 0.5, 'text/csv': 0.8
        };
        const multiplier = typeMultipliers[file.type] || 1.0;
        return Math.ceil(baseTime + (sizeMultiplier * multiplier));
    }

    getFileExtension(fileName) {
        return fileName.toLowerCase().substring(fileName.lastIndexOf('.'));
    }

    formatFileSize(bytes) {
        if (bytes === 0) return '0 Bytes';
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    }

    generateFileId() {
        return `file-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
    }

    generateUploadId() {
        return `upload-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
    }

    delay(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }

    preventDefaults(e) {
        e.preventDefault();
        e.stopPropagation();
    }

    // UI feedback methods
    showDragPreview(fileCount) {
        const hint = this.element.querySelector('.file-upload-area__hint');
        if (hint) {
            hint.textContent = `Drop ${fileCount} file${fileCount !== 1 ? 's' : ''} to upload`;
        }
    }

    hideDragPreview() {
        const hint = this.element.querySelector('.file-upload-area__hint');
        if (hint && this.options.originalHintText) {
            hint.textContent = this.options.originalHintText;
        }
    }

    async reportValidationErrors(errors) {
        await this.dotNetHelper.invokeMethodAsync('OnValidationErrors', errors);
    }

    updateOverallProgress() {
        if (this.uploadStats.totalFiles === 0) return;

        const overallProgress = {
            totalFiles: this.uploadStats.totalFiles,
            completedFiles: this.uploadStats.completedFiles,
            failedFiles: this.uploadStats.failedFiles,
            bytesUploaded: this.uploadStats.bytesUploaded,
            totalBytes: this.uploadStats.totalBytes,
            percentage: this.uploadStats.totalFiles > 0 ? 
                       (this.uploadStats.completedFiles / this.uploadStats.totalFiles) * 100 : 0
        };

        this.dotNetHelper.invokeMethodAsync('OnOverallProgressChanged', overallProgress);
    }

    // Cleanup
    dispose() {
        if (this.progressUpdateInterval) {
            clearInterval(this.progressUpdateInterval);
        }

        // Cancel all active uploads
        for (const [fileId] of this.activeUploads) {
            this.cancelUpload(fileId);
        }

        this.activeUploads.clear();
        this.fileQueue = [];
        this.uploadStats = { totalFiles: 0, completedFiles: 0, failedFiles: 0, bytesUploaded: 0, totalBytes: 0 };
    }
}

// Enhanced RFileUpload integration for Blazor
export const EnhancedRRFileUpload = {
    instances: new Map(),

    async initialize(elementId, dotNetHelper, options = {}) {
        try {
            const instance = new EnhancedRFileUpload(dotNetHelper, elementId, options);
            await instance.initialize();
            this.instances.set(elementId, instance);
            console.log(`Enhanced RFileUpload initialized: ${elementId}`);
        } catch (error) {
            console.error('Enhanced RFileUpload initialization failed:', error);
            throw error;
        }
    },

    async uploadFile(elementId, fileInfo, uploadUrl, options = {}) {
        const instance = this.instances.get(elementId);
        if (!instance) {
            throw new Error(`Enhanced RFileUpload instance not found: ${elementId}`);
        }
        return instance.uploadFile(fileInfo, uploadUrl, options);
    },

    async cancelUpload(elementId, fileId) {
        const instance = this.instances.get(elementId);
        if (instance) {
            await instance.cancelUpload(fileId);
        }
    },

    cleanup(elementId) {
        const instance = this.instances.get(elementId);
        if (instance) {
            instance.dispose();
            this.instances.delete(elementId);
        }
    }
};

// Expose on window for non-module usage
window.EnhancedRRFileUpload = EnhancedRRFileUpload;