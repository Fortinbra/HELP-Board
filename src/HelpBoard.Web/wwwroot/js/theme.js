(() => {
    "use strict";

    const storageKey = "helpboard.theme";
    const motionStorageKey = "helpboard.geocitiesMotion";
    const themes = new Set(["system", "light", "dark", "help", "geocities", "minimalist", "western"]);
    const standardThemes = new Set(["light", "dark", "help"]);
    const systemPreference = window.matchMedia("(prefers-color-scheme: dark)");
    let selection = "system";
    let motionEnabled = true;
    let feedback = "";

    function reportStorageFailure(error) {
        console.warn("HELP-Board theme preference storage is unavailable.", error);
        feedback = "Theme or animation preference applied for this visit only. Your browser could not save the preference.";
    }

    try {
        const stored = localStorage.getItem(storageKey);
        if (stored !== null && !themes.has(stored)) {
            console.warn("HELP-Board ignored an unsupported saved theme.", stored);
            feedback = "The saved theme is unavailable. Following your system preference.";
        } else {
            selection = stored ?? "system";
        }
        const storedMotion = localStorage.getItem(motionStorageKey);
        if (storedMotion !== null && storedMotion !== "on" && storedMotion !== "off") {
            console.warn("HELP-Board ignored an unsupported Geocities motion preference.", storedMotion);
            feedback = "The saved animation preference is unavailable. Animations are enabled.";
        } else {
            motionEnabled = storedMotion !== "off";
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
        const motionControl = document.querySelector("[data-geocities-motion-control]");
        if (motionControl && motionControl.checked !== motionEnabled) motionControl.checked = motionEnabled;
        const status = document.getElementById("theme-feedback");
        if (status && status.textContent !== feedback) status.textContent = feedback;
    }

    function systemTheme() {
        const preferred = systemPreference.matches ? "dark" : "light";
        const selector = document.querySelector("[data-theme-selector]");
        if (!selector || Array.from(selector.options).some(option => option.value === preferred)) return preferred;
        return Array.from(selector.options).find(option => standardThemes.has(option.value))?.value ?? "light";
    }

    function applyTheme() {
        document.documentElement.dataset.theme = selection === "system"
            ? systemTheme()
            : selection;
        document.documentElement.dataset.geocitiesMotion = motionEnabled ? "on" : "off";
        synchronizeControls();
    }

    applyTheme();
    systemPreference.addEventListener("change", applyTheme);
    document.addEventListener("change", event => {
        if (event.target.matches("[data-geocities-motion-control]")) {
            motionEnabled = event.target.checked;
            feedback = "";
            try {
                localStorage.setItem(motionStorageKey, motionEnabled ? "on" : "off");
            } catch (error) {
                reportStorageFailure(error);
            }
            applyTheme();
            return;
        }
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
        if (event.key === motionStorageKey) {
            if (event.newValue !== null && event.newValue !== "on" && event.newValue !== "off") {
                console.warn("HELP-Board received an unsupported Geocities motion preference.");
                return;
            }
            motionEnabled = event.newValue !== "off";
            feedback = "";
            applyTheme();
            return;
        }
        if (event.key !== storageKey && event.key !== null) return;
        if (event.newValue !== null && !themes.has(event.newValue)) {
            console.warn("HELP-Board received an unsupported theme preference.");
            return;
        }
        if (event.key === null) motionEnabled = true;
        selection = event.newValue ?? "system";
        feedback = "";
        applyTheme();
    });

    // Blazor inserts and may recreate the selector after the initial document load.
    new MutationObserver(synchronizeControls).observe(document.documentElement, { childList: true, subtree: true });
})();
