const { test } = require("node:test");
const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");
const vm = require("node:vm");

const source = fs.readFileSync(path.resolve(__dirname, "..", "..", "src", "HelpBoard.Web", "wwwroot", "js", "theme.js"), "utf8");

function createBrowser({ saved = null, dark = false, storageFails = false, options = ["system", "light", "dark", "help"] } = {}) {
    const events = {};
    const warnings = [];
    const status = { textContent: "" };
    const selector = { value: "system", options: options.map(value => ({ value })), matches: () => true };
    const root = { dataset: {} };
    const media = { matches: dark, addEventListener: (_, callback) => { events.system = callback; } };
    let stored = saved;
    const localStorage = {
        getItem: () => { if (storageFails) throw new Error("Storage blocked"); return stored; },
        setItem: (_, value) => { if (storageFails) throw new Error("Storage blocked"); stored = value; }
    };
    const context = {
        window: { matchMedia: () => media, addEventListener: (name, callback) => { events[name] = callback; } },
        document: {
            documentElement: root,
            querySelector: () => selector,
            getElementById: () => status,
            addEventListener: (name, callback) => { events[name] = callback; }
        },
        localStorage,
        console: { warn: (...args) => warnings.push(args), error: (...args) => warnings.push(args) },
        MutationObserver: class { constructor(callback) { events.mutation = callback; } observe() {} }
    };
    vm.runInNewContext(source, context);
    return {
        root, selector, status, warnings,
        stored: () => stored,
        choose(value) { selector.value = value; events.change({ target: selector }); },
        systemDark(value) { media.matches = value; events.system(); },
        recreate() { selector.value = "system"; events.mutation(); },
        storage(value) { events.storage({ key: "helpboard.theme", newValue: value }); }
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
