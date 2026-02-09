using System;
using System.Text;

namespace SiteBuilder.Services
{
    public class HtmlTemplateGenerator
    {
        /// <summary>
        /// Генерирует HTML шаблон мини-сайта с формой обратной связи через Formspree
        /// </summary>
        /// <param name="userEmail">Email пользователя из Google аккаунта</param>
        /// <param name="siteName">Название сайта</param>
        /// <param name="siteDescription">Описание сайта</param>
        /// <returns>HTML шаблон как строка</returns>
        public string GenerateTemplate(string userEmail, string siteName = "Мой сайт", string siteDescription = "Добро пожаловать на мой сайт")
        {
            if (string.IsNullOrEmpty(userEmail))
                throw new ArgumentException("Email не может быть пустым", nameof(userEmail));

            var template = new StringBuilder();

            template.AppendLine("<!DOCTYPE html>");
            template.AppendLine("<html lang=\"ru\">");
            template.AppendLine("<head>");
            template.AppendLine("    <meta charset=\"UTF-8\">");
            template.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            template.AppendLine($"    <title>{EscapeHtml(siteName)}</title>");
            template.AppendLine("    <link href=\"https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css\" rel=\"stylesheet\">");
            template.AppendLine("    <style>");
            template.AppendLine("        :root { --primary-color: #4f46e5; }");
            template.AppendLine("        body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; color: #333; }");
            template.AppendLine("        .navbar { background: linear-gradient(135deg, var(--primary-color) 0%, #6366f1 100%); box-shadow: 0 2px 10px rgba(0,0,0,0.1); }");
            template.AppendLine("        .hero-section { background: linear-gradient(135deg, var(--primary-color) 0%, #6366f1 100%); color: white; padding: 80px 0; text-align: center; }");
            template.AppendLine("        .hero-section h1 { font-size: 3rem; font-weight: 700; margin-bottom: 20px; }");
            template.AppendLine("        .hero-section p { font-size: 1.25rem; opacity: 0.95; }");
            template.AppendLine("        .image-placeholder { background: linear-gradient(135deg, #e5e7eb 0%, #f3f4f6 100%); border-radius: 12px; padding: 60px 20px; text-align: center; margin: 40px 0; border: 2px dashed #d1d5db; }");
            template.AppendLine("        .features-section { padding: 60px 0; }");
            template.AppendLine("        .feature-card { border: none; border-radius: 12px; padding: 30px; text-align: center; transition: transform 0.3s, box-shadow 0.3s; }");
            template.AppendLine("        .feature-card:hover { transform: translateY(-10px); box-shadow: 0 10px 30px rgba(0,0,0,0.1); }");
            template.AppendLine("        .feature-icon { font-size: 2.5rem; margin-bottom: 15px; color: var(--primary-color); }");
            template.AppendLine("        .form-section { background: #f9fafb; padding: 60px 0; }");
            template.AppendLine("        .form-title { font-size: 2rem; font-weight: 700; margin-bottom: 10px; color: #111; }");
            template.AppendLine("        .form-subtitle { color: #666; margin-bottom: 30px; font-size: 1.1rem; }");
            template.AppendLine("        .form-group label { font-weight: 600; color: #374151; margin-bottom: 8px; }");
            template.AppendLine("        .form-control { border-radius: 8px; border: 2px solid #e5e7eb; padding: 12px 15px; transition: border-color 0.3s; }");
            template.AppendLine("        .form-control:focus { border-color: var(--primary-color); box-shadow: 0 0 0 3px rgba(79, 70, 229, 0.1); }");
            template.AppendLine("        .btn-submit { background: var(--primary-color); border: none; border-radius: 8px; padding: 12px 30px; font-weight: 600; transition: all 0.3s; }");
            template.AppendLine("        .btn-submit:hover { background: #4338ca; transform: translateY(-2px); box-shadow: 0 8px 20px rgba(79, 70, 229, 0.3); }");
            template.AppendLine("        .btn-submit:active { transform: translateY(0); }");
            template.AppendLine("        .instruction-box { background: #eff6ff; border-left: 4px solid var(--primary-color); padding: 15px; border-radius: 8px; margin: 20px 0; }");
            template.AppendLine("        .instruction-title { font-weight: 600; color: var(--primary-color); margin-bottom: 5px; }");
            template.AppendLine("        .instruction-text { color: #333; font-size: 0.95rem; margin: 0; }");
            template.AppendLine("        .footer { background: #1f2937; color: white; padding: 40px 0; text-align: center; margin-top: 60px; }");
            template.AppendLine("        .copy-btn { position: relative; padding: 6px 12px; font-size: 0.85rem; }");
            template.AppendLine("        .copy-btn.copied { background-color: #10b981; }");
            template.AppendLine("    </style>");
            template.AppendLine("</head>");
            template.AppendLine("<body>");
            
            // Навигация
            template.AppendLine("    <nav class=\"navbar navbar-dark sticky-top\">");
            template.AppendLine("        <div class=\"container-fluid\">");
            template.AppendLine($"            <span class=\"navbar-brand mb-0 h1\">🚀 {EscapeHtml(siteName)}</span>");
            template.AppendLine("        </div>");
            template.AppendLine("    </nav>");

            // Hero секция
            template.AppendLine("    <section class=\"hero-section\">");
            template.AppendLine("        <div class=\"container\">");
            template.AppendLine($"            <h1>{EscapeHtml(siteName)}</h1>");
            template.AppendLine($"            <p>{EscapeHtml(siteDescription)}</p>");
            template.AppendLine("        </div>");
            template.AppendLine("    </section>");

            // Placeholder для изображения
            template.AppendLine("    <div class=\"container\">");
            template.AppendLine("        <div class=\"image-placeholder\">");
            template.AppendLine("            <div style=\"font-size: 4rem; margin-bottom: 15px;\">🖼️</div>");
            template.AppendLine("            <p style=\"margin: 0; color: #666;\"><strong>Ваше изображение</strong></p>");
            template.AppendLine("            <small style=\"color: #999;\">Замените src своим изображением</small>");
            template.AppendLine("            <div style=\"margin-top: 20px;\">");
            template.AppendLine("                <code>&lt;img src=\"URL_ВАШЕГО_ИЗОБРАЖЕНИЯ\" alt=\"\"&gt;</code>");
            template.AppendLine("            </div>");
            template.AppendLine("        </div>");
            template.AppendLine("    </div>");

            // Секция с особенностями
            template.AppendLine("    <section class=\"features-section\">");
            template.AppendLine("        <div class=\"container\">");
            template.AppendLine("            <div class=\"row\">");
            template.AppendLine("                <div class=\"col-md-4 mb-4\">");
            template.AppendLine("                    <div class=\"feature-card shadow-sm\">");
            template.AppendLine("                        <div class=\"feature-icon\">⚡</div>");
            template.AppendLine("                        <h5>Быстрый</h5>");
            template.AppendLine("                        <p>Загружается мгновенно на всех устройствах</p>");
            template.AppendLine("                    </div>");
            template.AppendLine("                </div>");
            template.AppendLine("                <div class=\"col-md-4 mb-4\">");
            template.AppendLine("                    <div class=\"feature-card shadow-sm\">");
            template.AppendLine("                        <div class=\"feature-icon\">📱</div>");
            template.AppendLine("                        <h5>Адаптивный</h5>");
            template.AppendLine("                        <p>Идеально выглядит на компьютере и телефоне</p>");
            template.AppendLine("                    </div>");
            template.AppendLine("                </div>");
            template.AppendLine("                <div class=\"col-md-4 mb-4\">");
            template.AppendLine("                    <div class=\"feature-card shadow-sm\">");
            template.AppendLine("                        <div class=\"feature-icon\">🔒</div>");
            template.AppendLine("                        <h5>Защищённый</h5>");
            template.AppendLine("                        <p>Ваши данные в безопасности</p>");
            template.AppendLine("                    </div>");
            template.AppendLine("                </div>");
            template.AppendLine("            </div>");
            template.AppendLine("        </div>");
            template.AppendLine("    </section>");

            // Форма обратной связи с инструкциями
            template.AppendLine("    <section class=\"form-section\">");
            template.AppendLine("        <div class=\"container\">");
            template.AppendLine("            <div class=\"row justify-content-center\">");
            template.AppendLine("                <div class=\"col-md-8\">");
            template.AppendLine("                    <h2 class=\"form-title\">📬 Напишите нам</h2>");
            template.AppendLine("                    <p class=\"form-subtitle\">Любые вопросы и предложения приветствуются!</p>");

            // Инструкция по настройке
            template.AppendLine("                    <div class=\"instruction-box\">");
            template.AppendLine("                        <div class=\"instruction-title\">👉 Важно! Перед использованием формы:</div>");
            template.AppendLine("                        <p class=\"instruction-text\" style=\"margin-bottom: 8px;\">");
            template.AppendLine("                            1. Перейдите на <strong><a href=\"https://formspree.io\" target=\"_blank\">formspree.io</a></strong>");
            template.AppendLine("                        </p>");
            template.AppendLine("                        <p class=\"instruction-text\" style=\"margin-bottom: 8px;\">");
            template.AppendLine("                            2. Создайте форму и подтвердите этот email:");
            template.AppendLine("                        </p>");
            template.AppendLine($"                        <p class=\"instruction-text\" style=\"background: white; padding: 10px; border-radius: 5px; font-family: monospace; margin-bottom: 8px;\"><strong>{EscapeHtml(userEmail)}</strong>");
            template.AppendLine("                            <button class=\"copy-btn btn btn-sm btn-outline-primary ms-2\" onclick=\"copyEmail(this)\" type=\"button\">📋 Копировать</button>");
            template.AppendLine("                        </p>");
            template.AppendLine("                        <p class=\"instruction-text\">");
            template.AppendLine("                            3. Замените YOUR_FORM_ID на полученный ID формы");
            template.AppendLine("                        </p>");
            template.AppendLine("                    </div>");

            // Сама форма
            template.AppendLine("                    <form action=\"https://formspree.io/f/YOUR_FORM_ID\" method=\"POST\" class=\"mt-4\">");
            template.AppendLine("                        <div class=\"form-group mb-3\">");
            template.AppendLine("                            <label for=\"name\">Ваше имя</label>");
            template.AppendLine("                            <input type=\"text\" class=\"form-control\" id=\"name\" name=\"name\" placeholder=\"Введите ваше имя\" required>");
            template.AppendLine("                        </div>");
            template.AppendLine("                        <div class=\"form-group mb-3\">");
            template.AppendLine("                            <label for=\"email\">Ваш email</label>");
            template.AppendLine("                            <input type=\"email\" class=\"form-control\" id=\"email\" name=\"email\" placeholder=\"ваш@email.com\" required>");
            template.AppendLine("                        </div>");
            template.AppendLine("                        <div class=\"form-group mb-3\">");
            template.AppendLine("                            <label for=\"subject\">Тема</label>");
            template.AppendLine("                            <input type=\"text\" class=\"form-control\" id=\"subject\" name=\"subject\" placeholder=\"О чём ваше сообщение?\" required>");
            template.AppendLine("                        </div>");
            template.AppendLine("                        <div class=\"form-group mb-3\">");
            template.AppendLine("                            <label for=\"message\">Сообщение</label>");
            template.AppendLine("                            <textarea class=\"form-control\" id=\"message\" name=\"message\" rows=\"5\" placeholder=\"Напишите ваше сообщение...\" required></textarea>");
            template.AppendLine("                        </div>");
            template.AppendLine("                        <button type=\"submit\" class=\"btn btn-submit btn-lg w-100 text-white\">✉️ Отправить сообщение</button>");
            template.AppendLine("                    </form>");
            template.AppendLine("                </div>");
            template.AppendLine("            </div>");
            template.AppendLine("        </div>");
            template.AppendLine("    </section>");

            // Кнопка для добавления дополнительной формы
            template.AppendLine("    <section style=\"padding: 40px 0; background: white;\">");
            template.AppendLine("        <div class=\"container text-center\">");
            template.AppendLine("                    <h3 style=\"margin-bottom: 20px;\">⚙️ Дополнительные функции</h3>");
            template.AppendLine("                    <button class=\"btn btn-outline-primary btn-lg me-2 mb-2\" onclick=\"addContactForm()\" type=\"button\">➕ Добавить форму контактов</button>");
            template.AppendLine("                    <button class=\"btn btn-outline-info btn-lg mb-2\" onclick=\"toggleTheme()\" type=\"button\">🎨 Переключить тему</button>");
            template.AppendLine("        </div>");
            template.AppendLine("    </section>");

            // Footer
            template.AppendLine("    <footer class=\"footer\">");
            template.AppendLine($"        <p>&copy; 2026 {EscapeHtml(siteName)}. Создано с помощью SiteBuilder</p>");
            template.AppendLine("        <small>Email для обратной связи: " + EscapeHtml(userEmail) + "</small>");
            template.AppendLine("    </footer>");

            // JavaScript
            template.AppendLine("    <script src=\"https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js\"></script>");
            template.AppendLine("    <script>");
            template.AppendLine("        function copyEmail(button) {");
            template.AppendLine("            const email = button.previousElementSibling;");
            template.AppendLine("            navigator.clipboard.writeText(email.textContent.trim());");
            template.AppendLine("            button.textContent = '✅ Скопировано!';");
            template.AppendLine("            button.classList.add('copied');");
            template.AppendLine("            setTimeout(() => {");
            template.AppendLine("                button.textContent = '📋 Копировать';");
            template.AppendLine("                button.classList.remove('copied');");
            template.AppendLine("            }, 2000);");
            template.AppendLine("        }");
            template.AppendLine("        function addContactForm() {");
            template.AppendLine("            alert('📝 Форма контактов добавлена!\\n\\nНастройте её в настройках сайта.');");
            template.AppendLine("        }");
            template.AppendLine("        function toggleTheme() {");
            template.AppendLine("            const root = document.documentElement;");
            template.AppendLine("            const isDark = root.style.getPropertyValue('--primary-color') === '#6366f1';");
            template.AppendLine("            root.style.setProperty('--primary-color', isDark ? '#8b5cf6' : '#6366f1');");
            template.AppendLine("            alert('🎨 Тема изменена!');");
            template.AppendLine("        }");
            template.AppendLine("    </script>");
            template.AppendLine("</body>");
            template.AppendLine("</html>");

            return template.ToString();
        }

        /// <summary>
        /// Экранирует HTML спецсимволы для безопасности
        /// </summary>
        private string EscapeHtml(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return text
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#39;");
        }
    }
}
