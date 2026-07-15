---
name: screenshot-capture
description: >-
  Screenshot capture for HorosCode products — setup, auth, scroll test, full-page,
  sticky workaround, container expand. Use with @screenshot-verify after every PNG.
user-invocable: true
metadata:
  company: HorosCode
  product: HorosCloud
---
# Screenshot-Capture / Full-Content

**Gilt für Executors**, wenn Screenshots von **HorosCode**-Produkten (z. B. HorosCloud) erstellt werden.

**Capture und Verify sind untrennbar:** Capture allein reicht nicht. Jede PNG **MUSS** danach mit `Read` geprüft werden (`screenshot-verify.mdc`). **VERBOTEN:** „Screenshot gespeichert" oder „fertig" melden ohne Verify.

**Workspace-Prinzip:** Agents arbeiten im **aktuellen Projekt-Root** (Cursor-Workspace). Pfade sind **relativ zum Workspace** — nicht `D:/HorosHelp` oder ein anderes zentrales Repo voraussetzen.

---

## Agent-Pflicht: Setup im aktuellen Workspace (BLOCKER vor erstem Capture)

Jedes HorosCode-Projekt, das Doc-Screenshots braucht, ist **self-contained** im **aktuellen Workspace-Root**. Der **Agent** (Executor) ist verantwortlich — **nicht** der User.

**VERBOTEN:**
- Dem User sagen, er solle Dateien manuell kopieren, wenn der Agent sie anlegen kann
- Capture starten, ohne vorher Setup geprüft/angelegt zu haben
- Nur dokumentieren, was fehlt — **fehlende Teile MÜSSEN erstellt werden**

**PFLICHT:** Vor **jedem** ersten Capture in einem Workspace (oder wenn Teile fehlen) führt der Agent die Schritte unten **selbst** aus — mit `Read`, `Write`/`StrReplace`, `Glob`, `Shell`.

### Schritt 1 — Prüfen (Agent)

| Prüfpunkt | Pfad (relativ zum Workspace-Root) |
|-----------|-----------------------------------|
| Capture-Skript | `scripts/quality/capture-page.mjs` |
| npm-Script `capture` | `package.json` → `"capture": "node scripts/quality/capture-page.mjs"` |
| Playwright-Dep | `package.json` → `devDependencies.playwright` |
| Rules (optional, empfohlen) | `.cursor/rules/screenshot-capture.mdc`, `.cursor/rules/screenshot-verify.mdc` |
| node_modules + Browser | `node_modules/playwright` vorhanden; Chromium installiert |

### Schritt 2 — Fehlendes anlegen (Agent MUSS)

**`scripts/quality/capture-page.mjs` fehlt:**
1. Agent **liest** die Vorlage (gleicher Workspace oder Referenz-Repo HorosHelp: `scripts/quality/capture-page.mjs`)
2. Agent **schreibt** die Datei ins Zielprojekt unter `scripts/quality/capture-page.mjs` (Verzeichnis ggf. anlegen)

**`package.json` fehlt oder unvollständig:**
- Fehlt `package.json` → Agent legt **minimale** `package.json` an
- Fehlt nur `capture`/`playwright` → Agent **ergänzt** bestehende `package.json` (bestehende Felder nicht überschreiben)

```json
{
  "scripts": {
    "capture": "node scripts/quality/capture-page.mjs"
  },
  "devDependencies": {
    "playwright": "^1.52.0"
  }
}
```

**`.cursor/rules/screenshot-*.mdc` fehlen:**
- Agent **MUSS** beide Rules im Zielprojekt anlegen (Inhalt aus Vorlage/HorosHelp oder identischer Inhalt)

**Dependencies installieren (Agent MUSS per Shell):**

```bash
npm install
npx playwright install chromium
```

Im **Projekt-Root** (Workspace-Root) ausführen. `node_modules/` und Browser-Binaries werden **nicht** committed — nur pro Projekt installiert.

### Schritt 3 — Setup verifizieren (Agent)

- [ ] `scripts/quality/capture-page.mjs` existiert und ist ausführbar
- [ ] `npm run capture -- --help` oder Test mit `--url`/`--out` erfolgreich
- [ ] Bei Auth-Pflicht: `scripts/quality/screenshot-auth.mjs` im Projekt (Agent legt an oder nutzt bestehendes; HorosCloudV5 nur Referenz)

### Schritt 4 — Dann erst Capture

Capture-Befehle **immer vom Projekt-Root** (CWD = Workspace-Root). `--out` ist relativ zum CWD.

```bash
# Variante A — npm (empfohlen)
npm run capture -- --url http://localhost:5173/ziel-route \
  --out docs/screenshots/features/<slug>/01-hauptansicht.png \
  --mode png --viewport 1280x720 --wait-for "[data-testid='ziel-root']"

# Variante B — direkt
node scripts/quality/capture-page.mjs --url http://localhost:5173/ziel-route \
  --out docs/screenshots/features/<slug>/01-hauptansicht.png
```

### Schritt 5 — Verify + Aufräumen

Wie `screenshot-verify.mdc`: jede PNG mit `Read` prüfen, Browser schließen — **BLOCKER**.

### Auth (projektspezifisch — Agent-Pflicht bei geschützten Routen)

Geschützte Routen: Agent legt bei Bedarf `scripts/quality/screenshot-auth.mjs` an (Login, Cookies, Token aus Env) und führt es **vor** `capture-page.mjs` aus. Das Capture-Skript setzt keine Tokens.

- HorosCloud-Produkt **HorosCloudV5** als Referenz: `HorosCloudV5/scripts/quality/screenshot-auth.mjs` (`HC_SCREENSHOT_PASSWORD`, `users.json`, `refreshJti`) — nur Beispiel
- Fehlt Auth bei geschützter Route → **nicht** capturen und Erfolg melden (`screenshot-verify.mdc`)

### Vorlage-Referenz (nur wenn Zielprojekt sie nicht hat)

Wenn der Agent aus einem **anderen** Repo arbeitet und keine lokale Vorlage findet: Inhalt aus HorosHelp-Workspace lesen (`scripts/quality/capture-page.mjs`, `.cursor/rules/screenshot-*.mdc`) und ins **Zielprojekt schreiben** — nicht vom User verlangen.

**Default:** Setup liegt im **Zielprojekt** (self-contained). HorosHelp ist Vorlage, kein Laufzeit-Hardcode.

### Ausnahme: Zentraler HorosHelp-Hub (selten)

Nur wenn der Workspace **selbst** HorosHelp ist und mehrere App-Repos bedient: Agent **darf** das zentrale Skript nutzen — **bevorzugt** bleibt Setup im App-Repo (Schritte 1–4 oben).

```bash
node scripts/quality/capture-page.mjs --url http://localhost:5173/... \
  --out docs/screenshots/...
```

Auth bleibt **projektspezifisch** im App-Repo.

### Agent-Checkliste: Automatisch vor jedem Capture-Lauf

1. [ ] **Prüfen:** `capture-page.mjs`, `package.json` (`capture` + `playwright`), Rules
2. [ ] **Anlegen:** fehlende Dateien schreiben (nicht User bitten)
3. [ ] **Installieren:** `npm install` + `npx playwright install chromium`
4. [ ] **Auth** (falls Route geschützt): `screenshot-auth.mjs` anlegen/ausführen
5. [ ] **Capture:** `npm run capture -- …` oder `node scripts/quality/capture-page.mjs`
6. [ ] **Verify:** jede PNG mit `Read` (`screenshot-verify.mdc`)
7. [ ] **Schließen:** Browser/App beenden

---

## Grundprinzip

Screenshots **MÜSSEN** die **gesamte Anwendungsoberfläche** des Zielbereichs zeigen — **nicht** nur den sichtbaren Viewport, wenn der Inhalt scrollt oder größer als der Bildschirm ist.

**`fullPage: true` allein reicht oft nicht.** Vor jedem Capture: Scroll-Test, Warm-up-Scroll, Warten auf stabile UI, dann Capture-Methode nach Entscheidungsbaum wählen.

**Full-Content bei Scroll ist PFLICHT, nicht optional.**

---

## Tool-Reihenfolge (Agents)

| Priorität | Tool | Wann |
|-----------|------|------|
| **1** | **Playwright-Skript** — `scripts/quality/capture-page.mjs` im **aktuellen Workspace** (`npm run capture`) | Docs-Screenshots, scrollbare UI, reproduzierbare Captures |
| **2** | **Browser MCP** (`cursor-ide-browser`) | Schnelle visuelle Prüfung; nur mit Warm-up-Scroll + `fullPage: true` |
| **3** | **Puppeteer** | Nur wenn Playwright nicht verfügbar — gleiche Patterns wie Playwright |

**Browser MCP allein ohne Warm-up-Scroll liefert häufig Viewport-only** — auch mit `fullPage: true`, wenn Lazy-Load oder innerer Scroll-Container nicht getriggert wurde.

**Bekannte Browser-MCP-Einschränkungen:**
- `fullPage` und `element`/`ref` **nicht kombinierbar** (Fehler von Playwright MCP) — [PR #704](https://github.com/microsoft/playwright-mcp/pull/704)
- `cursor-ide-browser` kann **gecachte PNGs** liefern, die nicht zum aktuellen DOM passen — vor erneutem Capture: navigieren, scrollen, kurz warten — [Cursor Forum #160422](https://forum.cursor.com/t/browser-take-screenshot-cursor-ide-browser-mcp-serves-cached-pngs-that-disagree-with-browser-snapshot-on-the-same-tab/160422)
- Für **offizielle Doc-Screenshots** unter `docs/screenshots/`: Playwright bevorzugen, nicht nur MCP

---

## Reihenfolge (Pflicht-Workflow)

1. **Setup sicherstellen** — Agent-Pflicht oben: Skript, `package.json`, Rules, `npm install`, Playwright-Browser
2. **Auth** — falls Route geschützt: `scripts/quality/screenshot-auth.mjs` im **aktuellen Projekt** (Agent legt an; HorosCloudV5 nur Beispiel).
3. **Viewport setzen** — vor Navigation/Capture (z. B. `1280×720` oder Projekt-Standard).
4. **Navigieren** — richtige Route/Tab; Modals/Toasts schließen.
5. **Warten** — stabiles UI-Element sichtbar (kein Spinner); **nicht** blind `networkidle` (s. unten).
6. **Scroll-Test** — PFLICHT (siehe Abschnitt „Scroll-Test").
7. **Warm-up-Scroll** — bei scrollbarem Inhalt PFLICHT vor Capture.
8. **Full-Content erfassen** — Entscheidungsbaum + Code-Snippets.
9. **Speichern** unter `docs/screenshots/<kontext>/…` (kebab-case).
10. **Verifizieren** — jede PNG mit `Read` (`screenshot-verify.mdc`). **BLOCKER**.
11. **Aufräumen** — Browser/App schließen (letzter Schritt).

---

## Entscheidungsbaum: Welche Capture-Methode?

```
Scroll-Test durchführen
│
├─ document.documentElement.scrollHeight > window.innerHeight ?
│  └─ JA → Dokument scrollt
│     1. Warm-up-Scroll (lazy load triggern)
│     2. Optional: sticky/fixed Header temporär neutralisieren
│     3. page.screenshot({ fullPage: true, animations: 'disabled' })
│     4. Bei Sticky-Duplikaten trotz Fix → PDF-Modus (s. unten) oder Tile+Stitch
│     5. Bei > ~16.000px Höhe oder fehlendem Content: Tile+Stitch (s. unten)
│
├─ Nur innerer Container scrollt (overflow: auto/scroll) ?
│  └─ JA → locator.screenshot() allein reicht NICHT
│     1. Container per Selector finden (scrollHeight > clientHeight)
│     2. CSS expandieren (overflow/max-height/height) via page.evaluate
│     3. locator.screenshot() ODER fullPage nach Expand
│     4. Styles wiederherstellen
│
├─ Dokument UND innerer Container scrollen ?
│  └─ Beides erfassen:
│     A) Zuerst inneren Container expandieren + screenshot
│     ODER B) fullPage nach Container-Expand (wenn Layout es erlaubt)
│     Bei fixed App-Shell (Sidebar+Header): Container-Methode bevorzugen
│
└─ Kein Scroll → normales Viewport-Screenshot (fullPage: false)
```

**Quellen:** [Playwright Screenshots](https://playwright.dev/docs/screenshots), [Locator.screenshot — nur sichtbarer Container-Inhalt](https://playwright.dev/docs/api/class-locator#locator-screenshot), [Issue #38699](https://github.com/microsoft/playwright/issues/38699), [Issue #24458](https://github.com/microsoft/playwright/issues/24458)

---

## Scroll-Test (PFLICHT vor Capture)

**Vor jedem Screenshot MUSS** geprüft werden, ob scrollbarer Inhalt existiert:

```js
// Playwright / page.evaluate — in Browser MCP via browser_cdp Runtime.evaluate
const scrollInfo = await page.evaluate(() => ({
  documentScrolls: document.documentElement.scrollHeight > window.innerHeight,
  bodyScrolls: document.body.scrollHeight > window.innerHeight,
  docScrollHeight: document.documentElement.scrollHeight,
  viewportHeight: window.innerHeight,
  // Scrollbare innere Container finden
  scrollContainers: [...document.querySelectorAll('*')].filter(el => {
    const s = getComputedStyle(el);
    return (s.overflowY === 'auto' || s.overflowY === 'scroll')
      && el.scrollHeight > el.clientHeight + 2;
  }).map(el => ({
    tag: el.tagName,
    id: el.id,
    className: el.className?.toString?.().slice(0, 80),
    scrollHeight: el.scrollHeight,
    clientHeight: el.clientHeight,
  })),
}));
```

| Prüfung | Methode |
|---------|---------|
| **Document scrollt** | `document.documentElement.scrollHeight > window.innerHeight` |
| **Innerer Container** | `element.scrollHeight > element.clientHeight` oder `scrollInfo.scrollContainers` |
| **Visuell** | Scrollbar am Rand, Footer/Content unten abgeschnitten |

**Wenn scrollable → Full-Page/Full-Content-Pfad. VERBOTEN:** Viewport-only speichern.

**Verify-Hinweis:** PNG-Höhe in Pixeln sollte deutlich über Viewport-Höhe liegen, wenn `docScrollHeight` groß ist (~1080px bei langem Feature = Fehler → `screenshot-verify.mdc`).

---

## Vor Capture: Viewport, Warten, Modals

### Viewport setzen (PFLICHT)

```js
await page.setViewportSize({ width: 1280, height: 720 });
// Breite bestimmt Screenshot-Breite; fullPage erweitert nur die Höhe
```

### Warten — was funktioniert

| Strategie | Empfehlung |
|-----------|------------|
| `waitForSelector('.ziel-element')` | **Bevorzugt** — wartet auf sichtbares UI |
| `waitForResponse(/\/api\//)` + Selector | SPA mit API-Daten |
| `waitForLoadState('load')` | Nach `goto` als Minimum |
| `waitForLoadState('networkidle')` | **NICHT** als Default — hängt bei WebSockets/Analytics — [Playwright #22809](https://github.com/microsoft/playwright/issues/22809) |

```js
await page.goto(url, { waitUntil: 'domcontentloaded' });
await page.locator('[data-testid="feature-root"]').waitFor({ state: 'visible', timeout: 30000 });
// Optional: API abwarten
await page.waitForResponse(resp => resp.url().includes('/api/') && resp.ok());
```

### Modals / Overlays

Vor Capture: Cookie-Banner, Onboarding, Dialoge schließen (`Escape`, Close-Button klicken).

---

## Warm-up-Scroll (PFLICHT bei scrollbarem Content)

`fullPage: true` rendert den DOM-Stand — **lazy-loaded Bilder/Sections fehlen**, wenn sie nie in den Viewport kamen. **Immer** vor `fullPage` scrollen:

```js
/** Lazy-Load und Scroll-Reveal triggern — vor fullPage aufrufen */
async function warmupScroll(page) {
  await page.evaluate(async () => {
    const delay = (ms) => new Promise(r => setTimeout(r, ms));
    const step = Math.max(200, Math.floor(window.innerHeight * 0.8));
    let lastY = -1;
    for (let i = 0; i < 50; i++) {
      window.scrollBy(0, step);
      await delay(150);
      const y = window.scrollY;
      if (y === lastY) break; // Boden erreicht
      lastY = y;
    }
    window.scrollTo(0, 0); // zurück nach oben für konsistenten Start
    await delay(300);
  });
}
```

**Lazy Images explizit laden** (optional, bei `loading="lazy"`):

```js
const lazyImages = page.locator('img[loading="lazy"]:visible');
for (const img of await lazyImages.all()) {
  await img.scrollIntoViewIfNeeded();
  await page.waitForFunction(
    el => el.naturalWidth > 0,
    await img.elementHandle(),
    { timeout: 5000 }
  ).catch(() => {}); // einzelnes Bild darf fehlschlagen
}
```

Quellen: [Playwright #19861](https://github.com/microsoft/playwright/issues/19861), [ScreenshotOne Blog](https://screenshotone.com/blog/playwright-python-full-page-website-screenshots/), [Stack Overflow #77365685](https://stackoverflow.com/questions/77365685/how-to-take-fullpage-screenshot-using-playwright)

---

## Pflicht-Snippets: Document Full-Page

```js
import { chromium } from 'playwright';

const VIEWPORT = { width: 1280, height: 720 };
const OUT = 'docs/screenshots/features/<slug>/01-hauptansicht.png';

const browser = await chromium.launch();
const context = await browser.newContext({ viewport: VIEWPORT });
const page = await context.newPage();

await page.goto('http://localhost:5173/ziel-route', { waitUntil: 'domcontentloaded' });
await page.locator('[data-testid="ziel-root"]').waitFor({ state: 'visible' });

// 1. Scroll-Test
const { documentScrolls, docScrollHeight } = await page.evaluate(() => ({
  documentScrolls: document.documentElement.scrollHeight > window.innerHeight,
  docScrollHeight: document.documentElement.scrollHeight,
}));

// 2. Warm-up wenn nötig
if (documentScrolls) await warmupScroll(page);

// 3. Sticky Header neutralisieren (verhindert Duplikate im Stitch)
if (documentScrolls) {
  await page.addStyleTag({
    content: `
      [style*="position: fixed"], [style*="position: sticky"],
      .sticky, .fixed, header[class*="sticky"] {
        position: relative !important;
      }
    `,
  });
}

// 4. Capture
await page.screenshot({
  path: OUT,
  fullPage: documentScrolls,
  animations: 'disabled',
});

await context.close();
await browser.close();
// 5. Read verify (screenshot-verify.mdc) — BLOCKER
```

**Puppeteer-Äquivalent:** `page.screenshot({ path, fullPage: true })` — gleiche Warm-up-/Sticky-Workarounds nötig — [DEV: Puppeteer vs Playwright](https://dev.to/grabbit/puppeteer-vs-playwright-for-screenshots-which-should-you-use-1o92)

---

## Pflicht-Snippets: Innerer Scroll-Container (`overflow: auto`)

**Wichtig:** `locator.screenshot()` erfasst laut Playwright-Docs **nur den aktuell sichtbaren** Container-Inhalt — **nicht** den vollen `scrollHeight`. Workaround: CSS expandieren.

```js
/** Scrollbaren Container vollständig sichtbar machen, dann screenshot */
async function captureScrollableContainer(page, selector, outPath) {
  const locator = page.locator(selector);
  await locator.waitFor({ state: 'visible' });

  const needsExpand = await locator.evaluate(el =>
    el.scrollHeight > el.clientHeight + 2
  );
  if (!needsExpand) {
    await locator.screenshot({ path: outPath, animations: 'disabled' });
    return;
  }

  // Styles merken und expandieren
  await locator.evaluate(el => {
    el.dataset._pwOrigOverflow = el.style.overflow;
    el.dataset._pwOrigMaxH = el.style.maxHeight;
    el.dataset._pwOrigHeight = el.style.height;
    el.style.overflow = 'visible';
    el.style.maxHeight = 'none';
    el.style.height = `${el.scrollHeight}px`;
  });

  // Kurz warten bis Reflow
  await page.waitForTimeout(200);

  await locator.screenshot({ path: outPath, animations: 'disabled' });

  // Styles wiederherstellen
  await locator.evaluate(el => {
    el.style.overflow = el.dataset._pwOrigOverflow || '';
    el.style.maxHeight = el.dataset._pwOrigMaxH || '';
    el.style.height = el.dataset._pwOrigHeight || '';
  });
}

// Aufruf:
await captureScrollableContainer(page, '[data-testid="main-scroll-area"]', OUT);
```

**Alternative:** Viewport vergrößern, wenn Bounding-Box des Elements größer als Viewport — [Playwright #13486](https://github.com/microsoft/playwright/issues/13486):

```js
async function resizeViewportForLocator(locator) {
  const page = locator.page();
  const box = await locator.boundingBox();
  const orig = page.viewportSize();
  if (box && (box.width > orig.width || box.height > orig.height)) {
    await page.setViewportSize({
      width: Math.ceil(Math.max(orig.width, box.width)),
      height: Math.ceil(Math.max(orig.height, box.height)),
    });
  }
  return orig;
}
```

---

## Browser MCP — konkreter Ablauf

```
1. browser_navigate → Ziel-URL
2. browser_lock (wenn Tab existiert)
3. browser_snapshot → richtige Seite/Tab?
4. Modals schließen (browser_click / Escape)
5. browser_cdp → Runtime.evaluate: Scroll-Test-Snippet (oben)
6. browser_scroll oder Runtime.evaluate → Warm-up-Scroll
7. browser_take_screenshot({ fullPage: true, filename: "docs/screenshots/..." })
   — NICHT fullPage + element/ref kombinieren
8. Read verify (screenshot-verify.mdc)
9. browser_lock unlock + browser_close
```

**Wenn Schritt 7 trotzdem Viewport-only liefert:** Playwright-Skript mit Container-Expand (oben) — nicht mit abgeschnittenem Bild abschließen.

---

## Bekannte Fehler & Workarounds

| Problem | Ursache | Lösung |
|---------|---------|--------|
| Nur Viewport trotz `fullPage: true` | Lazy-Load nicht getriggert; falscher Scroll-Root | Warm-up-Scroll; Container-Expand |
| Sticky Header mehrfach im Bild | `fullPage` stitched mit `position: fixed/sticky` | computed `fixed/sticky` → `absolute` (Snippet unten); oder PDF-Modus |
| Doppelte Listeneinträge / Nav im Bild | Sticky + Scroll-Stitch | Sticky-Fix **vor** Capture; bei SPA-Shell: Container-Expand |
| PDF nur erste Seite / abgeschnitten | Innerer `overflow`-Container, Print-Layout | Container-Expand; `emulateMedia({ media: 'screen' })`; ggf. PNG |
| Leere/Platzhalter-Bilder | `loading="lazy"` | `scrollIntoViewIfNeeded` + `naturalWidth > 0` |
| Innerer Container abgeschnitten | `locator.screenshot()` Limitierung | CSS expandieren (Snippet oben) |
| Seite > ~16.384px | CDP/Chrome Höhenlimit | Tile-Capture (8k-Stücke) + Stitch — [full-page-screenshot](https://github.com/LewisLiu007/full-page-screenshot) |
| Infinite Scroll | Kein festes Ende | Max. Scroll-Iterationen (z. B. 50), dann Capture |
| Canvas/WebGL | Nicht zuverlässig per DOM-Scroll | Mehrere Viewport-Shots + dokumentieren — Ausnahme |
| MCP liefert altes Bild | PNG-Cache | Neu navigieren/scrollen; Playwright für Docs |

---

## Alternative: PDF statt PNG (bei Stitch-Artefakten)

**Wann PDF sinnvoll ist:** Sticky-Header, Sidebars oder Toolbars erscheinen im langen PNG **mehrfach** (Scroll-Stitch-Artefakt). Chromiums Print-Renderer paginiert den Inhalt — fixed/sticky Elemente werden oft **nur einmal** pro Druckseite gezeichnet, nicht in jedem Viewport-Chunk wiederholt.

**Wann PNG bleiben sollte:** Pixel-genaue UI-Docs, Einbettung in Markdown/HTML (`<img>`), visuelle Regression. PNG = exakt das, was der Screen-Layout-Engine rendert (nach Sticky-Fix).

**PDF-Einschränkungen (nicht magisch besser):**

| Aspekt | PNG `fullPage` | PDF `page.pdf()` |
|--------|----------------|------------------|
| Sticky-Duplikate | Häufig ohne Fix | Oft weniger — Print-Layout |
| Lazy-Load | Warm-up-Scroll nötig | Ebenfalls Warm-up nötig |
| Innerer Scroll-Container | Container-Expand nötig | **Gleiches Problem** — nur sichtbarer Bereich |
| Aussehen | Screen-CSS | Default: `@media print` — **`emulateMedia({ media: 'screen' })` vor `pdf()`** |
| Farben/Hintergrund | Wie im Browser | `printBackground: true` + `-webkit-print-color-adjust: exact` |
| Doc-Einbindung | `<img src="…png">` | Link oder Konvertierung nötig |

**Manuell im Browser (Chrome):** Drucken → „Als PDF speichern“ — gleiche Trade-offs wie Playwright-PDF, nicht reproduzierbar für Agents. Für **offizielle HorosCode-Docs** Playwright bevorzugen.

### Capture-Skript (PNG / PDF)

Vom **Projekt-Root** (nach Agent-Setup oben):

```bash
# PNG — Sticky-Fix + Warm-up eingebaut
npm run capture -- \
  --url http://localhost:5173/settings/widgets \
  --out docs/screenshots/features/settings-widgets/01-hauptansicht.png \
  --mode png --viewport 1280x720 --wait-for "[data-testid='ziel-root']"

# PDF-Alternative bei Duplikat-Artefakten
npm run capture -- \
  --url http://localhost:5173/settings/widgets \
  --out docs/screenshots/features/settings-widgets/01-hauptansicht.pdf \
  --mode pdf
```

`--out` relativ zum CWD (typisch `docs/screenshots/…`). Auth vorher per projekt-eigenem `scripts/quality/screenshot-auth.mjs` — Capture setzt keine Tokens.

### PDF-Snippet (Playwright, inline)

```js
if (documentScrolls) await warmupScroll(page);

// Optional: wie bei PNG — Sticky neutralisieren wenn Print trotzdem dupliziert
await page.evaluate(() => {
  for (const el of document.querySelectorAll('*')) {
    const pos = getComputedStyle(el).position;
    if (pos === 'fixed' || pos === 'sticky') el.style.position = 'absolute';
  }
});

await page.emulateMedia({ media: 'screen' });
await page.pdf({
  path: 'docs/screenshots/features/<slug>/01-hauptansicht.pdf',
  printBackground: true,
  width: '1280px',
  margin: { top: '0', right: '0', bottom: '0', left: '0' },
});
// PDF visuell prüfen (Viewer) — Read-Verify wie bei PNG nicht möglich
```

**Empfehlungsreihenfolge bei „langgezogen + doppelte Einträge":**

1. Warm-up-Scroll (falls noch nicht)
2. Sticky-Fix via computed style (`fixed`/`sticky` → `absolute`)
3. Erneut PNG — meist reicht das
4. Immer noch Duplikate → PDF-Modus testen
5. Innerer Scroll-Container → Container-Expand (PNG **und** PDF)
6. Letzter Ausweg → Tile+Stitch

Quellen: [Playwright page.pdf()](https://playwright.dev/docs/api/class-page#page-page-pdf), [emulateMedia screen](https://github.com/microsoft/playwright/issues/3656), [Sticky in fullPage](https://stackoverflow.com/questions/77896738/sticky-navbar-appears-in-middle-of-full-page-screenshot-using-puppeteer)

---

## Multi-Shot + Stitch (Fallback)

Nur wenn `fullPage` und Container-Expand scheitern:

1. Viewport-Höhe `H`, Seite in Schritten von `0.8 * H` scrollen
2. Pro Schritt `page.screenshot({ fullPage: false })` ohne Overlap-Verlust
3. Mit `sharp`/`jimp`/PIL vertikal zusammenfügen
4. Alle Teile + Ergebnis mit `Read` verifizieren

---

## Desktop / WPF / Tauri / Mobile

- Projekt-spezifische Quality-Skripte (`scripts/quality/`), falls vorhanden.
- Sonst: Fenster maximieren, Multi-Shot+Stitch, Begründung dokumentieren.
- Mobile (HorosCloud Backup): Scope **Suki**.

---

## Viewport-only (seltene Ausnahme)

**Nur** wenn Full-Content nach Scroll-Test und allen Alternativen **technisch unmöglich** (Canvas-GL, DRM):

- **Warum** dokumentieren.
- **Mehrere Shots** mit Überlappung.
- Verify prüft alle Teile (`screenshot-verify.mdc`).

---

## Anwendung schließen (PFLICHT — letzter Schritt)

| Plattform | Aktion |
|-----------|--------|
| **Browser MCP** | `browser_lock` unlock → `browser_close` |
| **Playwright** | `context.close()` / `browser.close()` |
| **Desktop-App** | Fenster/Prozess beenden |

**Auch bei Fehler nach Retry:** Ressourcen aufräumen.

---

## Ablage & Naming

```
docs/screenshots/features/<feature-slug>/01-hauptansicht.png
docs/screenshots/<bereich>/<beschreibung>.png
```

Konsistent mit `/doc-epic`. Keine Screenshots in `src/` ohne Grund.

---

## Skripte im Projekt

| Skript | Ort (relativ zum Workspace) | Zweck |
|--------|----------------------------|-------|
| `capture-page.mjs` | `scripts/quality/` | PNG/PDF — `--url`, `--out`, `--mode png\|pdf` |
| `screenshot-auth.mjs` | `scripts/quality/` | Optional — Auth für geschützte Routen (projektspezifisch) |
| Batch-Capture | z. B. `scripts/quality/capture-live-screenshots.mjs` | Mehrere Routen in einem Lauf |

**Setup (Agent):** `npm install` + `npx playwright install chromium` im Projekt-Root, wenn noch nicht geschehen. Pro Capture nur URL/`--out` anpassen.

---

## Referenzen

| Thema | URL |
|-------|-----|
| Playwright Screenshots (fullPage, Element) | https://playwright.dev/docs/screenshots |
| Locator.screenshot API | https://playwright.dev/docs/api/class-locator#locator-screenshot |
| Lazy-Load Workarounds | https://github.com/microsoft/playwright/issues/19861 |
| Element scrollbar — kein natives fullPage | https://github.com/microsoft/playwright/issues/38699 |
| Fixed Header + innerer Scroll | https://github.com/microsoft/playwright/issues/24458 |
| Viewport-Resize Workaround | https://github.com/microsoft/playwright/issues/13486 |
| Browser MCP fullPage | https://github.com/microsoft/playwright-mcp/pull/704 |
| MCP PNG-Cache Bug | https://forum.cursor.com/t/browser-take-screenshot-cursor-ide-browser-mcp-serves-cached-pngs-that-disagree-with-browser-snapshot-on-the-same-tab/160422 |
| SPA Wait-Strategien | https://screenshotrun.com/blog/how-to-screenshot-single-page-applications-spa |
| Sticky + Lazy Scroll Pattern | https://screenshotone.com/blog/playwright-python-full-page-website-screenshots/ |
| Playwright PDF API | https://playwright.dev/docs/api/class-page#page-page-pdf |
| PDF mit Screen-CSS | https://github.com/microsoft/playwright/issues/3656 |

---

## Kurz-Checkliste (Executor)

- [ ] **Setup geprüft und fehlendes angelegt** (Agent-Pflicht oben — nicht User)
- [ ] `npm install` + `npx playwright install chromium` ausgeführt (falls nötig)
- [ ] Auth geprüft (falls geschützte Route)
- [ ] Viewport gesetzt
- [ ] Seite geladen, Modals geschlossen, stabiles Element sichtbar
- [ ] **Scroll-Test** durchgeführt (`scrollHeight > innerHeight` / Container-Check)
- [ ] **Warm-up-Scroll** bei scrollbarem Content
- [ ] Richtige Methode: fullPage / PDF / Container-Expand / Stitch
- [ ] Bei Sticky-Duplikaten: computed-style-Fix oder PDF getestet
- [ ] PNG gespeichert unter `docs/screenshots/…`
- [ ] **Jede PNG mit `Read` verifiziert** (`screenshot-verify.mdc`) — Höhe plausibel?
- [ ] **Browser-Tab / Fenster / App geschlossen**
