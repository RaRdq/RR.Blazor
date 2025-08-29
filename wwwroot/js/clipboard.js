/**
 * Clipboard utilities module
 * Uses modern Clipboard API with permission handling
 */

class ClipboardManager {
    async writeText(text) {
        // Modern Clipboard API requires HTTPS or localhost
        if (!navigator.clipboard) {
            // For non-secure contexts, show user-friendly error
            return Promise.reject(new Error('Clipboard access requires HTTPS or localhost'));
        }

        try {
            await navigator.clipboard.writeText(text);
            return Promise.resolve();
        } catch (err) {
            // Handle permission denied or other errors
            if (err.name === 'NotAllowedError') {
                return Promise.reject(new Error('Clipboard access denied. Please allow clipboard permissions.'));
            }
            console.error('Failed to copy to clipboard:', err);
            return Promise.reject(err);
        }
    }

    async readText() {
        if (!navigator.clipboard) {
            throw new Error('Clipboard access requires HTTPS or localhost');
        }

        try {
            return await navigator.clipboard.readText();
        } catch (err) {
            if (err.name === 'NotAllowedError') {
                throw new Error('Clipboard read access denied. Please allow clipboard permissions.');
            }
            throw err;
        }
    }

    // Check if clipboard API is available
    isAvailable() {
        return Boolean(navigator.clipboard && window.isSecureContext);
    }
}

const clipboardManager = new ClipboardManager();

// Export for ES6 modules
export function copyToClipboard(text) {
    return clipboardManager.writeText(text);
}

export function readFromClipboard() {
    return clipboardManager.readText();
}

// Register with RRBlazor global
window.RRBlazor = window.RRBlazor || {};
window.RRBlazor.Clipboard = {
    writeText: (text) => clipboardManager.writeText(text),
    readText: () => clipboardManager.readText(),
    copyToClipboard: (text) => clipboardManager.writeText(text),
    isAvailable: () => clipboardManager.isAvailable()
};

export default clipboardManager;