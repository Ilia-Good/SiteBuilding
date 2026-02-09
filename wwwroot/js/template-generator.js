/**
 * HTML Template Generator - JS Integration
 * Интеграция генератора HTML шаблонов в frontend
 */

class TemplateGenerator {
    constructor(apiBaseUrl = '/builder') {
        this.apiBaseUrl = apiBaseUrl;
    }

    /**
     * Генерирует HTML и выводит в окне просмотра
     * @param {string} siteName - Название сайта
     * @param {string} siteDescription - Описание сайта
     */
    async generateAndPreview(siteName, siteDescription) {
        try {
            const html = await this.generateTemplate(siteName, siteDescription);
            this.previewInModal(html);
            return html;
        } catch (error) {
            this.showError('Ошибка при генерировании шаблона: ' + error.message);
            console.error(error);
        }
    }

    /**
     * Генерирует HTML шаблон
     * @param {string} siteName - Название сайта
     * @param {string} siteDescription - Описание сайта
     * @returns {Promise<string>} HTML строка
     */
    async generateTemplate(siteName, siteDescription) {
        const response = await fetch(`${this.apiBaseUrl}/generate-template`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                siteName: siteName || 'Мой сайт',
                siteDescription: siteDescription || 'Добро пожаловать!'
            })
        });

        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.error || 'Ошибка генерирования');
        }

        const data = await response.json();
        return data.html;
    }

    /**
     * Скачивает HTML файл
     * @param {string} siteName - Название сайта
     * @param {string} siteDescription - Описание сайта
     * @param {string} fileName - Имя файла для скачивания
     */
    async downloadTemplate(siteName, siteDescription, fileName = null) {
        try {
            const cleanName = fileName || (siteName || 'site').replace(/\s+/g, '_').toLowerCase();
            
            const response = await fetch(`${this.apiBaseUrl}/download-template`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    siteName: siteName || 'Мой сайт',
                    siteDescription: siteDescription || 'Добро пожаловать!'
                })
            });

            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.error || 'Ошибка скачивания');
            }

            // Скачивание файла
            const blob = await response.blob();
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = `${cleanName}.html`;
            document.body.appendChild(a);
            a.click();
            window.URL.revokeObjectURL(url);
            document.body.removeChild(a);

            this.showSuccess('✅ Файл успешно скачан!');
        } catch (error) {
            this.showError('Ошибка при скачивании: ' + error.message);
            console.error(error);
        }
    }

    /**
     * Открывает сгенерированный HTML в новой вкладке
     * @param {string} html - HTML контент
     */
    openInNewTab(html) {
        const newWindow = window.open();
        newWindow.document.write(html);
        newWindow.document.close();
    }

    /**
     * Показывает превью в модальном окне
     * @param {string} html - HTML контент
     */
    previewInModal(html) {
        // Проверяем есть ли Bootstrap modal в HTML
        const modalHtml = `
            <div class="modal fade" id="previewModal" tabindex="-1">
                <div class="modal-dialog modal-lg">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title">👁️ Превью сайта</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                        </div>
                        <div class="modal-body">
                            <iframe style="width: 100%; height: 500px; border: none; border-radius: 8px;" id="previewFrame"></iframe>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Закрыть</button>
                            <button type="button" class="btn btn-primary" onclick="templateGen.openInNewTab(generatedHtml)">
                                🔗 Открыть в новой вкладке
                            </button>
                            <button type="button" class="btn btn-success" onclick="copyToClipboard()">
                                📋 Копировать HTML
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        `;

        // Добавляем модальное окно если его нет
        if (!document.getElementById('previewModal')) {
            document.body.insertAdjacentHTML('beforeend', modalHtml);
        }

        // Записываем HTML в iframe
        const frame = document.getElementById('previewFrame');
        frame.srcdoc = html;

        // Сохраняем HTML глобально для других функций
        window.generatedHtml = html;

        // Открываем модальное окно
        const modal = new (window.bootstrap?.Modal || bootstrap.Modal)(
            document.getElementById('previewModal')
        );
        modal.show();
    }

    /**
     * Показывает сообщение об ошибке
     * @param {string} message - Текст сообщения
     */
    showError(message) {
        const alertHtml = `
            <div class="alert alert-danger alert-dismissible fade show" role="alert">
                ❌ ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        `;
        this.insertAlert(alertHtml);
    }

    /**
     * Показывает сообщение об успехе
     * @param {string} message - Текст сообщения
     */
    showSuccess(message) {
        const alertHtml = `
            <div class="alert alert-success alert-dismissible fade show" role="alert">
                ✅ ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        `;
        this.insertAlert(alertHtml);
    }

    /**
     * Вставляет alert в начало тела страницы
     * @param {string} html - HTML для вставки
     */
    insertAlert(html) {
        const alertContainer = document.getElementById('alertContainer') || 
            (() => {
                const div = document.createElement('div');
                div.id = 'alertContainer';
                div.style.position = 'fixed';
                div.style.top = '20px';
                div.style.right = '20px';
                div.style.zIndex = '9999';
                document.body.appendChild(div);
                return div;
            })();

        const alertElement = document.createElement('div');
        alertElement.innerHTML = html;
        alertContainer.appendChild(alertElement.firstElementChild);

        // Авто-удаление через 5 сек
        setTimeout(() => {
            const alert = alertContainer.querySelector('.alert');
            if (alert) {
                alert.remove();
            }
        }, 5000);
    }
}

/**
 * Копирует HTML в буфер обмена (глобальная функция)
 */
function copyToClipboard() {
    const html = window.generatedHtml;
    navigator.clipboard.writeText(html).then(() => {
        const alertHtml = `
            <div class="alert alert-info alert-dismissible fade show" role="alert">
                📋 HTML скопирован в буфер обмена!
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        `;
        const tempGen = new TemplateGenerator();
        tempGen.insertAlert(alertHtml);
    });
}

// Инициализация
const templateGen = new TemplateGenerator();

// Экспорт для модульных систем
if (typeof module !== 'undefined' && module.exports) {
    module.exports = TemplateGenerator;
}
