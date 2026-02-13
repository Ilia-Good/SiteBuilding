class TemplateGenerator {
    constructor(apiBaseUrl = '/builder') {
        this.apiBaseUrl = apiBaseUrl;
    }

    async generateAndPreview(siteName, siteDescription) {
        try {
            const html = await this.generateTemplate(siteName, siteDescription);
            this.previewInModal(html);
            return html;
        } catch (error) {
            this.showError('Failed to generate template: ' + error.message);
            console.error(error);
        }
    }

    async generateTemplate(siteName, siteDescription) {
        const response = await fetch(`${this.apiBaseUrl}/generate-template`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                siteName: siteName || 'My site',
                siteDescription: siteDescription || 'Welcome to my site'
            })
        });

        if (!response.ok) {
            const error = await response.json().catch(() => ({}));
            throw new Error(error.error || 'Generation failed');
        }

        const data = await response.json();
        return data.html;
    }

    async downloadTemplate(siteName, siteDescription, fileName = null) {
        try {
            const cleanName = fileName || (siteName || 'site').replace(/\s+/g, '_').toLowerCase();

            const response = await fetch(`${this.apiBaseUrl}/download-template`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    siteName: siteName || 'My site',
                    siteDescription: siteDescription || 'Welcome to my site'
                })
            });

            if (!response.ok) {
                const error = await response.json().catch(() => ({}));
                throw new Error(error.error || 'Download failed');
            }

            const blob = await response.blob();
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = `${cleanName}.html`;
            document.body.appendChild(a);
            a.click();
            window.URL.revokeObjectURL(url);
            document.body.removeChild(a);

            this.showSuccess('File downloaded');
        } catch (error) {
            this.showError('Download error: ' + error.message);
            console.error(error);
        }
    }

    openInNewTab(html) {
        const newWindow = window.open();
        if (!newWindow) {
            this.showError('Popup blocked by browser');
            return;
        }
        newWindow.document.write(html);
        newWindow.document.close();
    }

    previewInModal(html) {
        const modalHtml = `
            <div class="modal fade" id="previewModal" tabindex="-1">
                <div class="modal-dialog modal-lg">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title">Preview</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                        </div>
                        <div class="modal-body">
                            <iframe style="width:100%;height:500px;border:none;border-radius:8px;" id="previewFrame"></iframe>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
                            <button type="button" class="btn btn-primary" onclick="templateGen.openInNewTab(window.generatedHtml)">Open in new tab</button>
                            <button type="button" class="btn btn-success" onclick="copyToClipboard()">Copy HTML</button>
                        </div>
                    </div>
                </div>
            </div>`;

        if (!document.getElementById('previewModal')) {
            document.body.insertAdjacentHTML('beforeend', modalHtml);
        }

        const frame = document.getElementById('previewFrame');
        frame.srcdoc = html;
        window.generatedHtml = html;

        const modal = new (window.bootstrap?.Modal || bootstrap.Modal)(document.getElementById('previewModal'));
        modal.show();
    }

    showError(message) {
        this.insertAlert('danger', message);
    }

    showSuccess(message) {
        this.insertAlert('success', message);
    }

    insertAlert(type, message) {
        const alertContainer = document.getElementById('alertContainer') || (() => {
            const div = document.createElement('div');
            div.id = 'alertContainer';
            div.style.position = 'fixed';
            div.style.top = '20px';
            div.style.right = '20px';
            div.style.zIndex = '9999';
            document.body.appendChild(div);
            return div;
        })();

        const alert = document.createElement('div');
        alert.className = `alert alert-${type} alert-dismissible fade show`;
        alert.role = 'alert';
        alert.innerHTML = `${message}<button type="button" class="btn-close" data-bs-dismiss="alert"></button>`;
        alertContainer.appendChild(alert);

        setTimeout(() => alert.remove(), 5000);
    }
}

function copyToClipboard() {
    const html = window.generatedHtml || '';
    navigator.clipboard.writeText(html).then(() => {
        const tempGen = new TemplateGenerator();
        tempGen.showSuccess('HTML copied to clipboard');
    }).catch(() => {
        const tempGen = new TemplateGenerator();
        tempGen.showError('Copy failed');
    });
}

const templateGen = new TemplateGenerator();

if (typeof module !== 'undefined' && module.exports) {
    module.exports = TemplateGenerator;
}
