(() => {
    "use strict";

    const storageKey = "helpboard.theme";
    const themes = new Set(["system", "light", "dark", "help"]);
    const systemPreference = window.matchMedia("(prefers-color-scheme: dark)");
    let selection = "system";
    let feedback = "";

    function reportStorageFailure(error) {
        console.warn("HELP-Board theme preference storage is unavailable.", error);
        feedback = "Theme applied for this visit only. Your browser could not save the preference.";
    }

    try {
        const stored = localStorage.getItem(storageKey);
        if (stored !== null && !themes.has(stored)) {
            console.warn("HELP-Board ignored an unsupported saved theme.", stored);
            feedback = "The saved theme is unavailable. Following your system preference.";
        } else {
            selection = stored ?? "system";
        }
    } catch (error) {
        reportStorageFailure(error);
    }

    function synchronizeControls() {
        const selector = document.querySelector("[data-theme-selector]");
        if (selector && !Array.from(selector.options).some(option => option.value === selection)) {
            selection = "system";
            feedback = "The saved theme is unavailable. Following your system preference.";
            document.documentElement.dataset.theme = systemTheme();
        }
        if (selector && selection === "system") document.documentElement.dataset.theme = systemTheme();
        if (selector && selector.value !== selection) selector.value = selection;
        const status = document.getElementById("theme-feedback");
        if (status && status.textContent !== feedback) status.textContent = feedback;
    }

    function systemTheme() {
        const preferred = systemPreference.matches ? "dark" : "light";
        const selector = document.querySelector("[data-theme-selector]");
        if (!selector || Array.from(selector.options).some(option => option.value === preferred)) return preferred;
        return Array.from(selector.options).find(option => option.value !== "system")?.value ?? "light";
    }

    function applyTheme() {
        document.documentElement.dataset.theme = selection === "system"
            ? systemTheme()
            : selection;
        synchronizeControls();
    }

    applyTheme();
    systemPreference.addEventListener("change", applyTheme);
    document.addEventListener("change", event => {
        if (!event.target.matches("[data-theme-selector]")) return;
        if (!themes.has(event.target.value)) {
            console.error("HELP-Board rejected an unsupported theme selection.");
            return;
        }
        selection = event.target.value;
        feedback = "";
        try {
            localStorage.setItem(storageKey, selection);
        } catch (error) {
            reportStorageFailure(error);
        }
        applyTheme();
    });
    window.addEventListener("storage", event => {
        if (event.key !== storageKey && event.key !== null) return;
        if (event.newValue !== null && !themes.has(event.newValue)) {
            console.warn("HELP-Board received an unsupported theme preference.");
            return;
        }
        selection = event.newValue ?? "system";
        feedback = "";
        applyTheme();
    });

    // Blazor inserts and may recreate the selector after the initial document load.
    new MutationObserver(synchronizeControls).observe(document.documentElement, { childList: true, subtree: true });
})();
