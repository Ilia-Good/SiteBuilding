(function () {
  const key = "ui_settings_v2";
  const defaults = { language: "ru", theme: "aurora", effects: "none", scale: 1, compact: false };

  const i18n = {
    ru: {
      nav_home: "Главная", nav_builder: "Конструктор", nav_all_sites: "Все сайты", nav_my_sites: "Мои сайты", hello: "Привет",
      btn_logout: "Выйти", btn_login_google: "Войти через Google", btn_ui_settings: "Настройки вида",
      footer_privacy: "Конфиденциальность", footer_terms: "Условия", footer_about: "О проекте", footer_faq: "FAQ", footer_guides: "Гайды", footer_contact: "Контакты", footer_all_sites: "Все сайты",
      settings_title: "Настройки интерфейса", settings_language: "Язык", settings_theme: "Фон", settings_effects: "Эффекты",
      settings_effects_hint: "Стекло: мягкие полупрозрачные панели. Свечение: яркий акцент.", settings_text_size: "Размер текста", settings_compact: "Компактный режим", settings_surprise: "Удиви меня", settings_reset: "Сбросить",
      lang_ru: "Русский", lang_en: "English", lang_es: "Español",
      theme_aurora: "Aurora", theme_sunset: "Sunset", theme_ocean: "Ocean", theme_mono: "Mono",
      fx_none: "Нет", fx_glass: "Стекло", fx_glow: "Свечение",
      all_sites_title: "Все сайты", all_sites_subtitle: "Все опубликованные сайты сообщества.", all_sites_create: "Создать свой", all_sites_empty: "Пока нет опубликованных сайтов.", all_sites_open: "Открыть сайт",
      all_sites_published: "Опубликован",
      about_kicker: "Почему SiteBuilding", about_title: "Сделать сайт должно быть так же просто, как собрать плейлист", about_lead: "SiteBuilding — это конструктор для людей, которым нужен понятный результат без долгих курсов и сложных панелей.",
      about_step1_title: "1. Идея", about_step1_text: "Вы выбираете структуру, добавляете тексты и изображения, а сервис собирает чистую страницу.",
      about_step2_title: "2. Публикация", about_step2_text: "Один клик — и сайт доступен по ссылке. Без отдельного DevOps и ручных деплоев.",
      about_step3_title: "3. Обратная связь", about_step3_text: "Форма сообщений уже встроена: обращения приходят владельцу сайта в личный кабинет.",
      guides_title: "Гайды", guide_1_title: "Лендинг за 15 минут", guide_1_text: "Структура: заголовок, блок ценности, CTA-кнопка, контактная форма.",
      guide_2_title: "Портфолио фрилансера", guide_2_text: "Добавьте 3 ключевых проекта, короткий био-текст и кнопку «Связаться».",
      guide_3_title: "Сайт локального бизнеса", guide_3_text: "Основные блоки: оффер, услуги, преимущества, контакты.",
      guide_4_title: "Страница мероприятия", guide_4_text: "Дата, место, программа, кнопка регистрации и форма вопросов.",
      faq_1_q: "Нужны ли навыки программирования?", faq_1_a: "Нет. Конструктор рассчитан на тех, кто хочет быстро запустить сайт без кода.",
      faq_2_q: "Можно ли редактировать сайт после публикации?", faq_2_a: "Да. Открываете свой сайт в списке, меняете блоки и публикуете снова.",
      faq_3_q: "Куда приходят сообщения из формы?", faq_3_a: "В раздел «Сообщения» у владельца сайта. Там видны дата, имя, email, текст и IP.",
      faq_4_q: "Почему форма может не отправляться?", faq_4_a: "Чаще всего из-за антиспам-лимитов или некорректных полей.",
      contact_title: "Контакты", contact_email_note: "Ответ обычно в течение 24 часов.", contact_telegram_link: "Написать в Telegram", contact_telegram_note: "Для быстрых вопросов по запуску сайта.",
      home_lead: "Конструктор одностраничных сайтов из блоков: заголовки, текст, изображения, кнопки и секции.",
      home_open_builder: "Открыть конструктор", home_rules_title: "Правила и ограничения", home_how_title: "Как это работает",
      home_rule_1: "Максимум 3 сайта на пользователя. Новый сайт можно создать только после удаления одного из своих.",
      home_rule_2: "Имя сайта должно быть уникальным для всех пользователей. Если имя занято — публикация запрещена.",
      home_rule_3: "До 3 изменений в день для каждого сайта.",
      home_rule_4: "Пробный период: 48 часов для каждого нового сайта.",
      home_how_1: "Задайте название, шапку и футер.",
      home_how_2: "Добавьте блоки: заголовок, текст, изображение, кнопка, секция.",
      home_how_3: "Редактируйте слева и сразу смотрите результат справа.",
      home_how_4: "Нажмите «Опубликовать сайт» и получите ссылку.",
      home_best_sites: "Лучшие сайты", home_view_all: "Смотреть все", home_open: "Открыть",
      home_published: "Опубликован",
      home_publish_label: "Публикация:", home_publish_text: "сайт автоматически размещается через GitHub Pages. Подождите пару минут после публикации.",
      my_sites_title: "Мои сайты", my_sites_limit: "Лимит", my_sites_used: "Использовано", my_sites_left: "Осталось",
      my_sites_create: "Создать сайт", my_sites_empty: "Пока нет сайтов. Создайте первый в конструкторе.",
      my_sites_created: "Создан", my_sites_trial: "Триал до", my_sites_paid: "Оплачен",
      my_sites_edits_today: "Правок сегодня", my_sites_edits_left: "Осталось",
      my_sites_github: "GitHub", common_yes: "Да", common_no: "Нет",
      my_sites_edit: "Изменить", my_sites_copy: "Копия", my_sites_messages: "Сообщения", my_sites_delete: "Удалить"
    },
    en: {
      nav_home: "Home", nav_builder: "Builder", nav_all_sites: "All Sites", nav_my_sites: "My Sites", hello: "Hi",
      btn_logout: "Logout", btn_login_google: "Login with Google", btn_ui_settings: "View Settings",
      footer_privacy: "Privacy", footer_terms: "Terms", footer_about: "About", footer_faq: "FAQ", footer_guides: "Guides", footer_contact: "Contact", footer_all_sites: "All Sites",
      settings_title: "Interface Settings", settings_language: "Language", settings_theme: "Background", settings_effects: "Effects",
      settings_effects_hint: "Glass: soft translucent panels. Glow: bright accent highlights.", settings_text_size: "Text Size", settings_compact: "Compact Mode", settings_surprise: "Surprise me", settings_reset: "Reset",
      lang_ru: "Russian", lang_en: "English", lang_es: "Spanish",
      theme_aurora: "Aurora", theme_sunset: "Sunset", theme_ocean: "Ocean", theme_mono: "Mono",
      fx_none: "None", fx_glass: "Glass", fx_glow: "Glow",
      all_sites_title: "All Sites", all_sites_subtitle: "All published community websites.", all_sites_create: "Create yours", all_sites_empty: "No published websites yet.", all_sites_open: "Open site",
      all_sites_published: "Published",
      about_kicker: "Why SiteBuilding", about_title: "Building a website should feel as easy as making a playlist", about_lead: "SiteBuilding is a builder for people who want a clear result without long courses and complex dashboards.",
      about_step1_title: "1. Idea", about_step1_text: "Pick a structure, add text and images, and the service assembles a clean page.",
      about_step2_title: "2. Publish", about_step2_text: "One click and your site is live. No separate DevOps flow needed.",
      about_step3_title: "3. Feedback", about_step3_text: "Contact form is built in: messages go directly to the site owner dashboard.",
      guides_title: "Guides", guide_1_title: "Landing page in 15 minutes", guide_1_text: "Structure: headline, value block, CTA button, contact form.",
      guide_2_title: "Freelancer portfolio", guide_2_text: "Add 3 key projects, a short bio, and a contact CTA.",
      guide_3_title: "Local business page", guide_3_text: "Core blocks: offer, services, benefits, contacts.",
      guide_4_title: "Event page", guide_4_text: "Date, place, program, registration button and questions form.",
      faq_1_q: "Do I need coding skills?", faq_1_a: "No. The builder is designed for quick launch without code.",
      faq_2_q: "Can I edit after publishing?", faq_2_a: "Yes. Open your site, update blocks, and publish again.",
      faq_3_q: "Where do form messages go?", faq_3_a: "To the owner's Messages section with date, name, email, text and IP.",
      faq_4_q: "Why can form sending fail?", faq_4_a: "Most often due to anti-spam limits or invalid fields.",
      contact_title: "Contact", contact_email_note: "Usually we reply within 24 hours.", contact_telegram_link: "Message in Telegram", contact_telegram_note: "For fast launch questions.",
      home_lead: "One-page website builder with blocks: headings, text, images, buttons and sections.",
      home_open_builder: "Open builder", home_rules_title: "Rules and limits", home_how_title: "How it works",
      home_rule_1: "Maximum 3 sites per user. Create a new one only after deleting one of yours.",
      home_rule_2: "Site name must be unique across all users. If occupied, publishing is blocked.",
      home_rule_3: "Up to 3 edits per day for each site.",
      home_rule_4: "Trial period: 48 hours for each new site.",
      home_how_1: "Set site name, header and footer.",
      home_how_2: "Add blocks: heading, text, image, button, section.",
      home_how_3: "Edit on the left and see the result on the right instantly.",
      home_how_4: "Click “Publish site” and get a link.",
      home_best_sites: "Top sites", home_view_all: "View all", home_open: "Open",
      home_published: "Published",
      home_publish_label: "Publishing:", home_publish_text: "the site is deployed via GitHub Pages automatically. Wait a couple of minutes after publishing.",
      my_sites_title: "My Sites", my_sites_limit: "Limit", my_sites_used: "Used", my_sites_left: "Left",
      my_sites_create: "Create site", my_sites_empty: "No sites yet. Create your first one in the builder.",
      my_sites_created: "Created", my_sites_trial: "Trial till", my_sites_paid: "Paid",
      my_sites_edits_today: "Edits today", my_sites_edits_left: "Left",
      my_sites_github: "GitHub", common_yes: "Yes", common_no: "No",
      my_sites_edit: "Edit", my_sites_copy: "Copy", my_sites_messages: "Messages", my_sites_delete: "Delete"
    },
    es: {
      nav_home: "Inicio", nav_builder: "Constructor", nav_all_sites: "Todos los sitios", nav_my_sites: "Mis sitios", hello: "Hola",
      btn_logout: "Salir", btn_login_google: "Entrar con Google", btn_ui_settings: "Ajustes visuales",
      footer_privacy: "Privacidad", footer_terms: "Términos", footer_about: "Sobre el proyecto", footer_faq: "FAQ", footer_guides: "Guías", footer_contact: "Contacto", footer_all_sites: "Todos los sitios",
      settings_title: "Ajustes de interfaz", settings_language: "Idioma", settings_theme: "Fondo", settings_effects: "Efectos",
      settings_effects_hint: "Glass: paneles translúcidos. Glow: acentos brillantes.", settings_text_size: "Tamaño del texto", settings_compact: "Modo compacto", settings_surprise: "Sorpréndeme", settings_reset: "Restablecer",
      lang_ru: "Ruso", lang_en: "Inglés", lang_es: "Español",
      theme_aurora: "Aurora", theme_sunset: "Atardecer", theme_ocean: "Océano", theme_mono: "Mono",
      fx_none: "Ninguno", fx_glass: "Cristal", fx_glow: "Brillo",
      all_sites_title: "Todos los sitios", all_sites_subtitle: "Todos los sitios publicados de la comunidad.", all_sites_create: "Crear el tuyo", all_sites_empty: "Aún no hay sitios publicados.", all_sites_open: "Abrir sitio",
      all_sites_published: "Publicado",
      about_kicker: "Por qué SiteBuilding", about_title: "Crear un sitio debe ser tan fácil como armar una playlist", about_lead: "SiteBuilding es para quienes quieren un resultado claro sin cursos largos ni paneles complejos.",
      about_step1_title: "1. Idea", about_step1_text: "Eliges estructura, agregas textos e imágenes y el servicio arma una página limpia.",
      about_step2_title: "2. Publicación", about_step2_text: "Un clic y el sitio está en línea. Sin flujo DevOps separado.",
      about_step3_title: "3. Respuestas", about_step3_text: "El formulario está integrado: los mensajes llegan al panel del propietario.",
      guides_title: "Guías", guide_1_title: "Landing en 15 minutos", guide_1_text: "Estructura: titular, valor, botón CTA y formulario.",
      guide_2_title: "Portafolio freelance", guide_2_text: "Agrega 3 proyectos clave, bio corta y botón de contacto.",
      guide_3_title: "Sitio de negocio local", guide_3_text: "Bloques base: oferta, servicios, ventajas y contacto.",
      guide_4_title: "Página de evento", guide_4_text: "Fecha, lugar, programa, botón de registro y formulario.",
      faq_1_q: "¿Necesito programar?", faq_1_a: "No. El constructor está hecho para lanzar rápido sin código.",
      faq_2_q: "¿Puedo editar después?", faq_2_a: "Sí. Abres tu sitio, cambias bloques y publicas de nuevo.",
      faq_3_q: "¿Dónde llegan los mensajes?", faq_3_a: "A la sección Mensajes del propietario con fecha, nombre, email, texto e IP.",
      faq_4_q: "¿Por qué falla el envío?", faq_4_a: "Normalmente por límites anti-spam o campos inválidos.",
      contact_title: "Contacto", contact_email_note: "Respondemos normalmente en 24 horas.", contact_telegram_link: "Escribir en Telegram", contact_telegram_note: "Para preguntas rápidas de lanzamiento.",
      home_lead: "Constructor de sitios de una página con bloques: títulos, texto, imágenes, botones y secciones.",
      home_open_builder: "Abrir constructor", home_rules_title: "Reglas y límites", home_how_title: "Cómo funciona",
      home_rule_1: "Máximo 3 sitios por usuario. Solo puedes crear uno nuevo tras borrar uno tuyo.",
      home_rule_2: "El nombre del sitio debe ser único entre todos los usuarios. Si está ocupado, no se publica.",
      home_rule_3: "Hasta 3 ediciones por día para cada sitio.",
      home_rule_4: "Periodo de prueba: 48 horas para cada sitio nuevo.",
      home_how_1: "Define nombre, encabezado y pie del sitio.",
      home_how_2: "Agrega bloques: título, texto, imagen, botón y sección.",
      home_how_3: "Edita a la izquierda y ve el resultado a la derecha al instante.",
      home_how_4: "Pulsa “Publicar sitio” y obtén el enlace.",
      home_best_sites: "Mejores sitios", home_view_all: "Ver todos", home_open: "Abrir",
      home_published: "Publicado",
      home_publish_label: "Publicación:", home_publish_text: "el sitio se publica automáticamente en GitHub Pages. Espera un par de minutos.",
      my_sites_title: "Mis sitios", my_sites_limit: "Límite", my_sites_used: "Usados", my_sites_left: "Restan",
      my_sites_create: "Crear sitio", my_sites_empty: "Aún no hay sitios. Crea el primero en el constructor.",
      my_sites_created: "Creado", my_sites_trial: "Prueba hasta", my_sites_paid: "Pagado",
      my_sites_edits_today: "Ediciones hoy", my_sites_edits_left: "Restan",
      my_sites_github: "GitHub", common_yes: "Sí", common_no: "No",
      my_sites_edit: "Editar", my_sites_copy: "Copia", my_sites_messages: "Mensajes", my_sites_delete: "Eliminar"
    }
  };

  function parseJson(raw) { try { return JSON.parse(raw); } catch { return null; } }
  function loadSettings() { const s = parseJson(localStorage.getItem(key) || ""); return { ...defaults, ...(s || {}) }; }
  function saveSettings(state) { localStorage.setItem(key, JSON.stringify(state)); }

  function applyLanguage(language) {
    const dict = i18n[language] || i18n.ru;
    document.documentElement.lang = language;
    document.querySelectorAll("[data-i18n]").forEach((el) => {
      const k = el.getAttribute("data-i18n");
      if (k && dict[k]) el.textContent = dict[k];
    });
  }

  function applyTheme(theme) {
    document.body.classList.remove("ui-theme-aurora", "ui-theme-sunset", "ui-theme-ocean", "ui-theme-mono");
    document.body.classList.add(`ui-theme-${theme}`);
  }

  function applyEffects(effects) {
    document.body.classList.remove("ui-fx-glass", "ui-fx-glow");
    if (effects === "glass") document.body.classList.add("ui-fx-glass");
    if (effects === "glow") document.body.classList.add("ui-fx-glow");
  }

  function applyScale(scale) {
    document.documentElement.style.setProperty("--ui-scale", String(scale || 1));
    const label = document.getElementById("uiScaleValue");
    if (label) label.textContent = `${Math.round((scale || 1) * 100)}%`;
  }

  function applyCompact(compact) { document.body.classList.toggle("ui-compact", !!compact); }
  function applyAll(state) { applyLanguage(state.language); applyTheme(state.theme); applyEffects(state.effects); applyScale(state.scale); applyCompact(state.compact); }

  function initSettingsUi() {
    const menu = document.getElementById("uiSettingsMenu");
    const toggle = document.getElementById("uiSettingsToggle");
    const language = document.getElementById("uiLanguage");
    const theme = document.getElementById("uiTheme");
    const effects = document.getElementById("uiEffects");
    const scale = document.getElementById("uiScale");
    const compact = document.getElementById("uiCompact");
    const surprise = document.getElementById("uiSurprise");
    const reset = document.getElementById("uiReset");
    if (!menu || !toggle || !language || !theme || !effects || !scale || !compact || !surprise || !reset) return;

    let state = loadSettings();
    language.value = state.language;
    theme.value = state.theme;
    effects.value = state.effects;
    scale.value = String(state.scale);
    compact.checked = !!state.compact;
    applyAll(state);

    function sync() {
      state = { language: language.value || "ru", theme: theme.value || "aurora", effects: effects.value || "none", scale: Number(scale.value || 1), compact: !!compact.checked };
      applyAll(state);
      saveSettings(state);
    }

    toggle.addEventListener("click", (e) => { e.preventDefault(); e.stopPropagation(); menu.classList.toggle("d-none"); });
    document.addEventListener("click", (e) => {
      if (!menu.classList.contains("d-none") && !menu.contains(e.target) && !toggle.contains(e.target)) menu.classList.add("d-none");
    });

    language.addEventListener("change", sync);
    theme.addEventListener("change", sync);
    effects.addEventListener("change", sync);
    scale.addEventListener("input", sync);
    compact.addEventListener("change", sync);

    surprise.addEventListener("click", () => {
      const themes = ["aurora", "sunset", "ocean", "mono"];
      const fx = ["none", "glass", "glow"];
      theme.value = themes[Math.floor(Math.random() * themes.length)];
      effects.value = fx[Math.floor(Math.random() * fx.length)];
      scale.value = String((0.85 + Math.random() * 0.45).toFixed(2));
      compact.checked = Math.random() > 0.5;
      sync();
    });

    reset.addEventListener("click", () => {
      localStorage.removeItem(key);
      state = { ...defaults };
      language.value = state.language;
      theme.value = state.theme;
      effects.value = state.effects;
      scale.value = String(state.scale);
      compact.checked = false;
      applyAll(state);
    });
  }

  if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", initSettingsUi);
  else initSettingsUi();
})();
