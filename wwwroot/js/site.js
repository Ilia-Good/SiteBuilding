(function () {
  const key = "ui_settings_v1";
  const defaults = {
    language: "ru",
    theme: "aurora",
    effects: "none",
    scale: 1,
    compact: false
  };

  const i18n = {
    ru: {
      nav_home: "Главная",
      nav_builder: "Конструктор",
      nav_public: "Мини-сайты",
      nav_my_sites: "Мои сайты",
      btn_logout: "Выйти",
      btn_login_google: "Войти через Google",
      btn_ui_settings: "Вид",
      footer_privacy: "Конфиденциальность",
      footer_terms: "Условия",
      footer_public_sites: "Публичные сайты",
      settings_title: "Настройки интерфейса",
      settings_language: "Язык",
      settings_theme: "Фон",
      settings_effects: "Эффекты",
      settings_text_size: "Размер текста",
      settings_compact: "Компактный режим",
      settings_surprise: "Удиви меня",
      settings_reset: "Сбросить"
    },
    en: {
      nav_home: "Home",
      nav_builder: "Builder",
      nav_public: "Mini Sites",
      nav_my_sites: "My Sites",
      btn_logout: "Logout",
      btn_login_google: "Login with Google",
      btn_ui_settings: "Style",
      footer_privacy: "Privacy",
      footer_terms: "Terms",
      footer_public_sites: "Public Sites",
      settings_title: "Interface Settings",
      settings_language: "Language",
      settings_theme: "Background",
      settings_effects: "Effects",
      settings_text_size: "Text Size",
      settings_compact: "Compact Mode",
      settings_surprise: "Surprise me",
      settings_reset: "Reset"
    },
    es: {
      nav_home: "Inicio",
      nav_builder: "Constructor",
      nav_public: "Mini Sitios",
      nav_my_sites: "Mis Sitios",
      btn_logout: "Salir",
      btn_login_google: "Entrar con Google",
      btn_ui_settings: "Estilo",
      footer_privacy: "Privacidad",
      footer_terms: "Términos",
      footer_public_sites: "Sitios Públicos",
      settings_title: "Ajustes de interfaz",
      settings_language: "Idioma",
      settings_theme: "Fondo",
      settings_effects: "Efectos",
      settings_text_size: "Tamaño del texto",
      settings_compact: "Modo compacto",
      settings_surprise: "Sorpréndeme",
      settings_reset: "Restablecer"
    }
  };

  function parseJson(raw) {
    try {
      return JSON.parse(raw);
    } catch {
      return null;
    }
  }

  function loadSettings() {
    const stored = parseJson(localStorage.getItem(key) || "");
    return { ...defaults, ...(stored || {}) };
  }

  function saveSettings(state) {
    localStorage.setItem(key, JSON.stringify(state));
  }

  function applyLanguage(language) {
    const dict = i18n[language] || i18n.ru;
    document.documentElement.lang = language;
    document.querySelectorAll("[data-i18n]").forEach((el) => {
      const k = el.getAttribute("data-i18n");
      if (!k) return;
      if (dict[k]) el.textContent = dict[k];
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
  }

  function applyCompact(compact) {
    document.body.classList.toggle("ui-compact", !!compact);
  }

  function applyAll(state) {
    applyLanguage(state.language);
    applyTheme(state.theme);
    applyEffects(state.effects);
    applyScale(state.scale);
    applyCompact(state.compact);
  }

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
      state = {
        language: language.value || "ru",
        theme: theme.value || "aurora",
        effects: effects.value || "none",
        scale: Number(scale.value || 1),
        compact: !!compact.checked
      };
      applyAll(state);
      saveSettings(state);
    }

    toggle.addEventListener("click", (e) => {
      e.preventDefault();
      e.stopPropagation();
      menu.classList.toggle("d-none");
    });

    document.addEventListener("click", (e) => {
      if (!menu.classList.contains("d-none") && !menu.contains(e.target) && !toggle.contains(e.target)) {
        menu.classList.add("d-none");
      }
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
      scale.value = String((0.96 + Math.random() * 0.14).toFixed(2));
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

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", initSettingsUi);
  } else {
    initSettingsUi();
  }
})();
