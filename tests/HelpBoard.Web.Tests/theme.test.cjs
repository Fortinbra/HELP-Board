const { test } = require("node:test");
const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");
const vm = require("node:vm");

const source = fs.readFileSync(path.resolve(__dirname, "..", "..", "src", "HelpBoard.Web", "wwwroot", "js", "theme.js"), "utf8");
const themeStyles = fs.readFileSync(path.resolve(__dirname, "..", "..", "src", "HelpBoard.Web", "wwwroot", "css", "baseline.css"), "utf8");

function tokens(selector) {
    const block = themeStyles.match(new RegExp(`${selector}\\s*\\{([^}]*)\\}`));
    assert.ok(block, `Missing theme selector ${selector}`);
    return Object.fromEntries([...block[1].matchAll(/--baseline-([\w-]+):\s*(#[0-9a-f]{6})\s*;/gi)]
        .map(([, name, value]) => [name, value]));
}

function luminance(hex) {
    const channels = hex.slice(1).match(/../g).map(value => parseInt(value, 16) / 255)
        .map(value => value <= 0.04045 ? value / 12.92 : ((value + 0.055) / 1.055) ** 2.4);
    return 0.2126 * channels[0] + 0.7152 * channels[1] + 0.0722 * channels[2];
}

function createBrowser({ saved = null, savedMotion = null, dark = false, storageFails = false, options = ["system", "light", "dark", "help"] } = {}) {
    const events = {};
    const warnings = [];
    const status = { textContent: "" };
    const selector = { value: "system", options: options.map(value => ({ value })), matches: query => query === "[data-theme-selector]" };
    const motion = { checked: false, matches: query => query === "[data-geocities-motion-control]" };
    const root = { dataset: {} };
    const media = { matches: dark, addEventListener: (_, callback) => { events.system = callback; } };
    const stored = { "helpboard.theme": saved, "helpboard.geocitiesMotion": savedMotion };
    const localStorage = {
        getItem: key => { if (storageFails) throw new Error("Storage blocked"); return stored[key] ?? null; },
        setItem: (key, value) => { if (storageFails) throw new Error("Storage blocked"); stored[key] = value; }
    };
    const context = {
        window: { matchMedia: () => media, addEventListener: (name, callback) => { events[name] = callback; } },
        document: {
            documentElement: root,
            querySelector: query => {
                if (query === "[data-theme-selector]") return selector;
                if (query === "[data-geocities-motion-control]") return motion;
                if (query === "[data-geocities-motion]" && root.dataset.geocitiesMotion) return root;
                return null;
            },
            getElementById: () => status,
            addEventListener: (name, callback) => { events[name] = callback; }
        },
        localStorage,
        console: { warn: (...args) => warnings.push(args), error: (...args) => warnings.push(args) },
        MutationObserver: class { constructor(callback) { events.mutation = callback; } observe() {} }
    };
    vm.runInNewContext(source, context);
    return {
        root, selector, motion, status, warnings,
        stored: (key = "helpboard.theme") => stored[key],
        choose(value) { selector.value = value; events.change({ target: selector }); },
        chooseMotion(value) { motion.checked = value; events.change({ target: motion }); },
        systemDark(value) { media.matches = value; events.system(); },
        recreate() { selector.value = "system"; motion.checked = false; events.mutation(); },
        storage(value, key = "helpboard.theme") { events.storage({ key, newValue: value }); }
    };
}

test("initial theme follows system changes until explicitly selected", () => {
    const browser = createBrowser();
    assert.equal(browser.root.dataset.theme, "light");
    browser.systemDark(true);
    assert.equal(browser.root.dataset.theme, "dark");
    browser.choose("help");
    browser.systemDark(false);
    assert.equal(browser.root.dataset.theme, "help");
    assert.equal(browser.stored(), "help");
    browser.choose("system");
    assert.equal(browser.root.dataset.theme, "light");
});

test("saved explicit choice restores and survives recreated Blazor controls", () => {
    const browser = createBrowser({ saved: "dark" });
    assert.equal(browser.root.dataset.theme, "dark");
    browser.recreate();
    assert.equal(browser.selector.value, "dark");
    browser.storage("help");
    assert.equal(browser.root.dataset.theme, "help");
    browser.storage(null);
    assert.equal(browser.root.dataset.theme, "light");
});

test("blocked storage keeps theme usable and reports non-persistence", () => {
    const browser = createBrowser({ storageFails: true });
    browser.choose("dark");
    assert.equal(browser.root.dataset.theme, "dark");
    assert.match(browser.status.textContent, /could not save/);
    assert.equal(browser.warnings.length, 2);
});

test("unknown or disabled saved themes fall back with an explanation", () => {
    for (const saved of ["western", "help"]) {
        const browser = createBrowser({ saved, options: ["system", "light", "dark"] });
        assert.equal(browser.root.dataset.theme, "light");
        assert.equal(browser.selector.value, "system");
        assert.match(browser.status.textContent, /unavailable/);
    }
});

test("system preference chooses an available standard when its preferred theme is disabled", () => {
    const browser = createBrowser({ dark: true, options: ["system", "light"] });
    assert.equal(browser.root.dataset.theme, "light");
});

test("optional themes can be selected, restored, and synchronized across tabs", () => {
    const options = ["system", "light", "dark", "help", "geocities", "minimalist", "western"];
    for (const theme of ["geocities", "minimalist", "western"]) {
        const browser = createBrowser({ options });
        browser.choose(theme);
        assert.equal(browser.root.dataset.theme, theme);
        assert.equal(browser.stored(), theme);

        const restored = createBrowser({ saved: theme, options });
        assert.equal(restored.root.dataset.theme, theme);
        restored.storage("dark");
        assert.equal(restored.root.dataset.theme, "dark");
    }
});

test("system preference does not select an optional theme when standard themes are disabled", () => {
    const browser = createBrowser({ dark: true, options: ["system", "geocities"] });
    assert.equal(browser.root.dataset.theme, "light");
});

test("disabled optional themes received from another tab fall back visibly", () => {
    const browser = createBrowser({ options: ["system", "light", "dark", "minimalist"] });
    browser.storage("western");
    assert.equal(browser.root.dataset.theme, "light");
    assert.equal(browser.selector.value, "system");
    assert.match(browser.status.textContent, /unavailable/);
});

test("geocities motion preference persists and synchronizes recreated controls and other tabs", () => {
    const browser = createBrowser({ saved: "geocities", options: ["system", "light", "geocities"] });
    assert.equal(browser.root.dataset.geocitiesMotion, "on");
    assert.equal(browser.motion.checked, true);

    browser.chooseMotion(false);
    assert.equal(browser.root.dataset.geocitiesMotion, "off");
    assert.equal(browser.stored("helpboard.geocitiesMotion"), "off");
    const restored = createBrowser({ savedMotion: "off" });
    assert.equal(restored.motion.checked, false);
    restored.storage("on", "helpboard.geocitiesMotion");
    assert.equal(restored.root.dataset.geocitiesMotion, "on");
    restored.recreate();
    assert.equal(restored.motion.checked, true);
    restored.storage(null, null);
    assert.equal(restored.root.dataset.geocitiesMotion, "on");
});

test("motion control remains usable and reports blocked storage", () => {
    const browser = createBrowser({ storageFails: true });
    browser.chooseMotion(false);
    assert.equal(browser.root.dataset.geocitiesMotion, "off");
    assert.match(browser.status.textContent, /could not save/);
});

test("unsupported saved motion preference is reported and uses the default", () => {
    const browser = createBrowser({ savedMotion: "unexpected" });
    assert.equal(browser.root.dataset.geocitiesMotion, "on");
    assert.match(browser.status.textContent, /animation preference is unavailable/);
    assert.equal(browser.warnings.length, 1);
});

test("geocities uses dark surfaces with bright yellow and green text", () => {
    const palette = tokens(':root\\[data-theme="geocities"\\]');
    const channels = color => color.slice(1).match(/../g).map(value => parseInt(value, 16));
    const [red, green, blue] = channels(palette.text);
    const [mutedRed, mutedGreen, mutedBlue] = channels(palette.muted);

    assert.ok(luminance(palette.bg) < 0.05 && luminance(palette.panel) < 0.05);
    assert.ok(red >= 200 && green >= 200 && blue < 100);
    assert.ok(mutedGreen >= 200 && mutedGreen > 2 * mutedRed && mutedGreen > 2 * mutedBlue);
});

test("theme tokens meet text, focus, and control contrast thresholds", () => {
    const defaults = tokens(":root");
    for (const theme of ["light", "dark", "help", "geocities", "minimalist", "western"]) {
        const palette = theme === "light"
            ? defaults
            : { ...defaults, ...tokens(`:root\\[data-theme="${theme}"\\]`) };
        const checks = [
            ["text", "bg", 4.5], ["text", "panel", 4.5], ["text", "header", 4.5],
            ["muted", "bg", 4.5], ["muted", "panel", 4.5], ["muted", "header", 4.5],
            ["accent", "bg", 4.5], ["accent", "panel", 4.5], ["action-text", "accent", 4.5],
            ["focus", "bg", 3], ["focus", "panel", 3], ["focus", "header", 3],
            ["border", "bg", 3], ["border", "panel", 3], ["border", "header", 3]
        ];
        if (theme === "geocities") {
            checks.push(["heading", "bg", 4.5], ["heading", "panel", 4.5], ["heading", "header", 4.5]);
        }
        for (const [foreground, background, minimum] of checks) {
            const first = luminance(palette[foreground]);
            const second = luminance(palette[background]);
            const contrast = (Math.max(first, second) + 0.05) / (Math.min(first, second) + 0.05);
            assert.ok(contrast >= minimum, `${theme}: ${foreground} on ${background} is ${contrast.toFixed(2)}, expected ${minimum}`);
        }
    }
});

test("geocities animated colors retain contrast throughout each transition", () => {
    const palette = tokens(':root\\[data-theme="geocities"\\]');
    for (const [animation, minimum] of [["text", 4.5], ["border", 3]]) {
        const keyframes = themeStyles.match(new RegExp(`@keyframes geocities-rainbow-${animation}\\s*\\{([\\s\\S]*?)\\n\\}`));
        assert.ok(keyframes, `Missing ${animation} animation`);
        const colors = [...keyframes[1].matchAll(/var\(--baseline-([\w-]+)\)/g)].map(([, name]) => palette[name]);
        assert.ok(colors.length > 1);
        colors.push(colors[0]);
        for (let index = 0; index < colors.length - 1; index++) {
            const start = colors[index].slice(1).match(/../g).map(value => parseInt(value, 16));
            const end = colors[index + 1].slice(1).match(/../g).map(value => parseInt(value, 16));
            for (let step = 0; step <= 100; step++) {
                const color = "#" + start.map((value, channel) =>
                    Math.round(value + (end[channel] - value) * step / 100).toString(16).padStart(2, "0")).join("");
                for (const background of ["bg", "panel", "header"]) {
                    const first = luminance(color);
                    const second = luminance(palette[background]);
                    const contrast = (Math.max(first, second) + 0.05) / (Math.min(first, second) + 0.05);
                    assert.ok(contrast >= minimum, `${animation}: ${color} on ${background} has contrast ${contrast.toFixed(2)}`);
                }
            }
        }
    }
});
