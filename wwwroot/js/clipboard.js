
class ClipboardManager {
    async writeText(text) {
        if (!navigator.clipboard) {
            return Promise.reject(new Error('Clipboard access requires HTTPS or localhost'));
        }

        try {
            await navigator.clipboard.writeText(text);
            return Promise.resolve();
        } catch (err) {
            if (err.name === 'NotAllowedError') {
                return Promise.reject(new Error('Clipboard access denied. Please allow clipboard permissions.'));
            }
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

    isAvailable() {
        return Boolean(navigator.clipboard && window.isSecureContext);
    }
}

const clipboardManager = new ClipboardManager();

export function copyToClipboard(text) {
    return clipboardManager.writeText(text);
}

export function readFromClipboard() {
    return clipboardManager.readText();
}

window.RRBlazor = window.RRBlazor || {};
window.RRBlazor.Clipboard = {
    writeText: (text) => clipboardManager.writeText(text),
    readText: () => clipboardManager.readText(),
    copyToClipboard: (text) => clipboardManager.writeText(text),
    isAvailable: () => clipboardManager.isAvailable()
};

export default clipboardManager;