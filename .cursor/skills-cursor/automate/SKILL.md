---
name: automate
description: Erstellt Cursor Automations. Verwenden, wenn der Nutzer explizit eine neue Cursor Automation anlegen möchte.
environments:
  - local
---
# Automation erstellen (interaktiv)

Verwende diesen Skill, wenn der Nutzer explizit eine neue **Cursor Automation** erstellen, einrichten oder aufsetzen möchte — z. B. „create a Cursor automation“, „open the Automations editor with this draft“ oder „set up a scheduled Cursor agent“.

**Abgrenzung.** „Automation“ im Workspace kann vieles bedeuten (`.github/workflows`, CI-Pipelines, geplante Jobs, Skripte, dbt, Browser-Automation, Shell-Skripte, Workflow-Engines). Nimm **nicht** an, dass generische Formulierungen wie „automate this“, „help me automate my deploys“ oder „make an automation“ **Cursor Automation** meinen. Leite zur genannten Oberfläche, wenn der Nutzer eine nennt; nutze normale Repo-/Produkt-Exploration, wenn der Kontext woanders hinweist; oder stelle eine kurze Klärungsfrage, wenn die Ziel-Oberfläche unklar ist. Starte diese Spine nur, wenn der Nutzer explizit Cursor Automations will oder bestätigt, dass Cursor Automations gemeint ist.

## Ausführungs-Spine (jeder Lauf)

1. **Finish-Path-Check.** Prüfe zuerst, ob der In-App-Automations-Editor-Handoff verfügbar ist (siehe **Finish availability** unten). Wenn weder das Automations-Editor-Tool noch der Resource-Opener verfügbar ist, sofort sagen: „Please use this skill in the Agents Window.“ und stoppen.
2. **Absicht erfassen + proaktive Integration-Discovery.** Nach bestandenem Finish-Path-Check: Wenn im Prompt 2+ von {was startet es, was soll passieren, welches Ergebnis} fehlen, eine kurze Plain-Chat-Nachricht mit Bitte um 1–2 Sätze senden und warten. Überspringen, wenn Trigger + Aktion + Ergebnis schon genannt sind. Proaktiv Discovery für jede vom Nutzer genannte oder implizierte Integration — `gh repo view` auf cwd; Slack-MCP-Channels; PagerDuty-MCP-Services; Linear/Sentry-MCP-Teams/Projekte. Ergebnisse nutzen, um spätere Fragen zu sparen.
3. **Vollständigkeits-Gates.** Trigger-, Tool-, Prompt-, Naming- und PCD-Checks unten durcharbeiten. Nicht zur Zusammenfassung springen, solange picker-gestützte Pflichtfelder unbekannt sind, es sei denn, der Nutzer hat sie explizit an den Automations-Editor delegiert.
4. **Eine konsolidierte Frage.** Inline fragen, was Integration-Discovery nicht auflösen konnte — Trigger-Details, Repo/Channel/Service bei Mehrdeutigkeit, Tools wenn nicht offensichtlich. Default: Plain-Chat inline. Nur zu `AskQuestion` eskalieren für (a) Tools-Multi-Select und (b) Integration-Discovery-Kandidatenlisten mit 3+ Treffern.
5. **Draft-Tabelle → Genehmigung → Finish-Handoff.** Kompakte Markdown-Tabelle (Plain Language, kein YAML) mit Name/Beschreibung, Trigger, Tools, Instructions, aufgelösten Settings und „to finish in editor“. Nutzer genehmigt, dann fragen, ob du den Automations-Editor öffnen sollst für deferred Werte. Bei Ja: Finish-Path aus dem Availability-Check nutzen.

---

## Hausregeln

- **Nur Plain Language.** Zeige oder sprich niemals MCP-/Tool-/Proto-Namen, Request-Typen, Enum-Werte, Stage-Labels oder rohe CLI-Ausgabe im sichtbaren Chat. Sage „open the editor with the draft“, „the Slack channel“, „the repo and branch“. Ausnahme: Nutzer will explizit Interna sehen.
- **Kein YAML in der Finalisierung.** Draft-Tabelle = kompaktes Markdown nur in Plain Language. YAML/JSON intern bauen und validieren; Wire-Payload nur zeigen, wenn der Nutzer danach fragt.
- **Keine automatischen Fallbacks.** Niemals submiten, URL öffnen, Browser-Prefill-Link einfügen oder Buckets wechseln. Der einzige Finish-Path: reviewed Draft-Tabelle, Nutzer-Genehmigung, Readiness-Bestätigung, dann Automations-Editor öffnen. Wenn weder Automations-Editor-Tool noch Resource-Opener verfügbar: sofort stoppen und Nutzer an Agents Window verweisen.
- **Nur Erstellung.** Dieser Skill bereitet nur neue Automations vor. Keine bestehenden Cursor Automations aus dem Chat listen, getten, inspizieren, updaten oder suchen.
- **Integration-Discovery erlaubt.** Verbundene Integration-MCP read/list/search-Tools für picker-gestützte Werte (Slack-Channels, PagerDuty-Services, Linear-Teams, Sentry-Projekte). Das schließt **nicht** Backend-Automation-Tools ein, die Cursor Automations listen/getten/inspizieren/erstellen/updaten/finishen/prefillen.
- **Repo-Datei-Referenzen.** Nur eine Datei aus dem aktuellen Chat-Repo (Pfad, Auszug oder `@file`) in Draft-Feldern referenzieren — Prompt, Instructions, Name oder Description — wenn **beides** gilt: (1) die Automation läuft im selben Repo (Git-Trigger-Scope oder `workflow.gitConfig.repo` = Chat-Repo) **und** (2) die Datei ist committed (von `git` auf dem Branch getrackt, den die Automation auscheckt, nicht nur im Working Tree). Sonst Absicht paraphrasieren oder Nutzer bitten, zuerst zu committen und zu pushen — Pfad/Inhalt nicht einbetten. Untracked/staged-only/dirty-only, Dateien außerhalb des Automation-Repos und Dateien in anderem Repo als Chat-Checkout qualifizieren nie.

---

## PCD — Portal-Vollständigkeit & Deferral

Nutze diese Checks, damit der Draft vor dem Editor-Öffnen vollständig ist. Zeige die IDs dem Nutzer nicht.

| Id | Scope |
|----|-------|
| **PCD:slack-trigger** | `slackTrigger`-Channel-Auswahl. Fragen, ob Channel(s) jetzt oder in der Automations-UI gewählt werden; Discovery oder explizites UI-Deferral vor der Draft-Tabelle. |
| **PCD:slack-actions** | `slack` / `readSlack`-Aktionen. Ziel oder explizites Editor-Deferral vor der Draft-Tabelle. |
| **PCD:git-scope** | `git` PR/Push/CI-Trigger. Repo/Org/Branch-Scope auflösen oder explizit an Editor delegieren. |
| **PCD:universal** | Jede bewusste Lücke steht in „To finish in editor“ der Draft-Tabelle und wird in der finalen Handoff-Notiz wiederholt. |

### PCD-Matrix

| Scope | Vor Editor-Öffnung, sofern nicht deferred |
|-------|-------------------------------------------|
| **PCD:slack-trigger** | Slack-Channel-IDs aufgelöst oder Nutzer wählt Channels in der Automations-UI. Leere Channels nur bei explizitem UI-Deferral gültig. |
| **PCD:slack-actions** | `slack.channel` / readSlack-Scope aufgelöst oder Nutzer wählt **Select channels** im Editor. IDs müssen `C…` / `G…` / `D…` sein, nie `U…`. |
| **PCD:git-scope** | PR-Trigger haben Repos/Orgs; Push-Trigger Repo + Branch; CI-Trigger Repo-Scope. Scoped Repo-Discovery erst nach Ziel-Identifikation. |

### PCD-Hinweise

- Slack-Trigger und Slack-Action-Ziele sind getrennte Fragen. Slack-Trigger impliziert kein Slack-Action-Ziel und umgekehrt keinen Slack-Trigger-Channel.
- Keine leeren Slack-Channel-Werte prefilling nach „specify now“, außer Discovery lief oder Nutzer wechselt explizit zu Editor-Deferral.
- Bei Slack-Replies „respond in the triggering thread“ getrennt von „send to a specific channel or DM“ anbieten.
- Wenn `mcp` aktiv ist, muss der Server **MCP existence gate** und **MCP auth gate** unten passieren. Bevorzuge exakten Catalog-Server-Namen selbst finden; Nutzer nur fragen bei mehreren passenden Matches oder unklarer Integration. Nicht als Cursor-Plugin bezeichnen. Keine Server-Namen erfinden.

### MCP existence gate

Ein MCP-Server ist nur eligible, wenn der aktuelle MCP/Tool-Catalog des Nutzers beweist, dass er existiert und in dieser Session nutzbar ist. Bevor du einen MCP für Integration-Discovery oder eine `mcp`-Aktion im Draft nutzt, inspiziere den tatsächlichen Catalog der Agent-Session und wähle den richtigen Wert für `mcp.server.name`.

**Lies das richtige Feld aus dem Catalog.** Jeder agent-seitige Catalog-Eintrag liegt unter `~/.cursor/mcps/<folder>/SERVER_METADATA.json` mit zwei unterschiedlichen Werten:

- `serverIdentifier` — scoped Folder-Name der Agent-Runtime (z. B. `dashboard-team-1-Linear`, `plugin-pagerduty-pagerduty-mcp`, `cursor-app-control`).
- `serverName` — plain Name, den der Nutzer auf cursor.com konfiguriert hat (z. B. `Linear`, `pagerduty-mcp`, `Databricks SQL`).

Schreibe `serverName` in `workflow.actions[].mcp.server.name` und alle `@[MCP: ...]`-Prompt-Mentions. Niemals `serverIdentifier`/Folder-Name, nie Präfix erfinden oder paraphrasieren (`team-…`, `user-…`, `<orgId>-…`), nie Präfixe vom Folder-Namen hand-strippen — viele `serverName`s enthalten Leerzeichen (z. B. `Databricks SQL`, `statsig read only console`), String-Munging am Identifier ist fragil. Der Automations-Editor matcht auf trim + lowercase, Casing egal, aber exakten `serverName` aus `SERVER_METADATA.json` übergeben. Bei nützlicher URL optional `templateMcpHints: [{ name: <serverName>, url: <serverUrl> }]` für URL-Match bei Name-Drift.

**Eligibility — nur dashboard-backed Server.** Nur dashboard-backed Server erscheinen in `GetAvailableMcpServers` des Editors, mit dem prefilled `mcp`-Aktionen aufgelöst werden. Ihr `serverIdentifier` beginnt immer mit:

- `dashboard-team-<teamId>-` (team-shared)
- `dashboard-` (persönliche User-Server auf cursor.com)
- `plugin-<slug>-` (Marketplace-Plugin-Server)

Alles andere im Agent-Catalog — `cursor-ide-browser`, `cursor-app-control`, `extension-…`, Projekt-`mcp.json`, andere lokale Server — erscheint **nicht** im Dashboard-Catalog. Als ineligible für `mcp` behandeln: nicht in `workflow.actions`, nicht in `@[MCP: ...]`, nicht annehmen, dass sie beim Editor-Öffnen auflösen. Ineligible Prefill landet in „Set up MCP“ und blockiert Save.

Passt ein nutzbarer, dashboard-eligible Catalog-Server klar zur gewünschten Integration, nutze ihn ohne den Nutzer den Server-Namen buchstabieren zu lassen. Name aus Prompt, Screenshot, Skill, Template, Marketplace-Docs oder Firmen-Konvention ist kein Beweis.

Enthält der Catalog keinen nutzbaren, dashboard-eligible Server: MCP nicht aufrufen, keine `mcp`-Aktion, keine `@[MCP: ...]`-Mentions. Fehlend, disabled oder setup-pending = unavailable für Prefill. Nutzer zuerst setup lassen oder MCP aus Draft weglassen und „Set up/select <integration> in the Automations editor“ unter **To finish in editor**. Unbekannte MCP-Server-Namen sind keine gültigen deferred Tool-Rows — prefilled fehlende MCPs blockieren Save.

### MCP auth gate

**Warum das existiert.** Der Automations-Editor kann MCP-OAuth anstoßen, aber der Flow verlässt den Draft und der Nutzer verliert ungespeicherte Änderungen. Authentifiziere MCPs hier im Chat **bevor** du `mcp`-Aktion hinzufügst, Draft-Tabelle zeigst oder den Editor öffnest.

**Wann es gilt.** Immer wenn du `mcp`-Aktion oder `@[MCP: ...]` für einen dashboard-eligible Server planst — auch nach Integration-Discovery für picker-gestützte Werte.

**Nicht authentifizierte Server erkennen** aus dem Session-Catalog (nicht raten). `STATUS.md` wird für Auth und generische Fehler geschrieben — bloße Existenz ist **kein** Auth-Signal — Datei lesen:

- `~/.cursor/mcps/<folder>/STATUS.md` existiert **und** Inhalt sagt, Server braucht Auth (z. B. „needs authentication“ / `mcp_auth`). STATUS.md nur mit generischem Fehler ist **kein** Auth-Signal.
- `GetMcpTools` meldet `serverStatus: "needsAuth"` für `serverIdentifier`.
- Live-Tool-Liste nur `mcp_auth` (noch keine anderen Tools).
- Integration-Discovery scheitert mit Auth-/Authorization-Fehler.

Server, der existence gate passiert aber ein Auth-Signal oben matcht, ist **nicht authentifiziert** — getrennt von „missing“, „not set up“ oder generischem Fehler.

**Hard stop until authed.** Wenn Ziel-MCP nicht authentifiziert:

1. **Automation-Drafting-Spine stoppen.** Keine `mcp`-Aktion, keine `@[MCP: ...]`, keine Draft-Tabelle, Editor nicht öffnen.
2. **Dem Nutzer klar sagen**, welche Integration noch verbunden werden muss (`serverName`, nie interne IDs). Jetzt verbinden hält den Draft sicher; Auth-Deferral zum Editor kann Arbeit verwerfen.
3. **Inline-Auth anbieten**, wenn verfügbar. Skill läuft im Agents Window mit interaktiver MCP-Auth. Bei `mcp_auth` fragen, ob Verbindungsflow jetzt starten soll. Bei Zustimmung **ein Server nach dem anderen** via `mcp_auth` für `serverIdentifier` (leere Args oder Session-`McpAuth` mit `server_identifier`). Auf Erfolg warten, Auth re-checken, Drafting fortsetzen.
4. **Wenn Inline-Auth unavailable** (kein `mcp_auth`): Integration in Cursor Settings → MCP verbinden, hier zurückkommen und bestätigen. Editor nicht öffnen, solange MCP unauthentifiziert.

**MCP-OAuth nie an den Automations-Editor delegieren.** Kein „Authenticate/connect <integration> in the Automations editor“ in **To finish in editor** für MCP, den du prefilling willst. Unauthentifizierte prefilled MCP-Rows blockieren Save; Editor-Auth-Redirect verliert Draft-State.

**Nach erfolgreicher Auth** Auth-Gate erneut, dann Integration-Discovery und Drafting. `mcp` nur im Draft, wenn Server authentifiziert und nutzbar.

---

## Ablauf

### Stage 0 — Finish availability (vor Intent-Capture)

**Finish availability** (einmal pro Lauf; agent-interne Entscheidung — Tool-Namen dem Nutzer nie zitieren). Check dem Nutzer nicht erwähnen. Nicht sagen: „I'll first check whether the Automations editor handoff is available“. Keine Cursor-Backend-Automation-Tools inspizieren.

| Bucket | Signal | Default finish |
|--------|--------|----------------|
| **Automations editor** | `cursor-app-control.open_automation` gelistet | Automations-Editor mit reviewed Draft öffnen |
| **Agents Window required** | Weder `cursor-app-control.open_automation` noch `cursor-app-control.open_resource` gelistet | Sofort stoppen — „Please use this skill in the Agents Window.“ |

Automations-Editor-Pfad nutzt `open_automation` direkt mit reviewed Draft. Keine Backend-Finish-Tools, kein Browser-Prefill-URL, kein `open_resource`, kein `cursor://`-Deeplink.

Wenn weder `open_automation` noch `open_resource` verfügbar: Automation-Drafting nicht fortsetzen, keine Browser-Prefill-URL, keine Follow-up-Fragen. Sofort: „Please use this skill in the Agents Window.“

### Stage 1 — Intent erfassen (Plain Chat, kein AskQuestion)

Nur nach bestandenem Finish-Availability-Check. Bei dünnem Prompt eine kurze Plain-Chat-Nachricht und **warten**:

> "Before we dive in, what do you want this automation to do? What kicks it off, what should happen, and what's the outcome? A sentence or two is plenty. Let me know if you want some examples of what you can build."

Überspringen, wenn Trigger + Aktion + Ergebnis schon abgedeckt oder Nutzer „walk me through it“ sagt. Antwort freiform — Frage nie in `AskQuestion` wrappen. Keine Repo-Discovery vor Stage 1 oder bewusstem Skip.

### Stage 2 — Authoring funnel

Bestehende Automation-Edits in diesem Flow nicht unterstützt. Keine bestehenden Automations via Backend-Tools listen/getten/inspizieren/updaten/suchen. Nicht nach Name/Beschreibung suchen. Änderung bestehender Automation: direkt in Automations UI oder neue Ersatz-Automation. Nicht behaupten, Änderungen aus Chat gespeichert wurden.

Reihenfolge wie Automations UI: trigger → tools → prompt → name/description → draft table. Lücken aus vorherigen Messages füllen. Nicht direkt zu YAML springen, außer Prompt deckt Felder ab und PCD-Gates sind erfüllt oder explizit deferred.

#### Trigger

**Appendix — Trigger selection tables** für Trigger und Follow-ups. Picker-gestützte Werte via Integration-Discovery vor Fragen auflösen; nur noch unbeantwortete Felder fragen. Cron ohne aufgelösten Schedule ist für direktes Save invalid; Webhook-Trigger kommen nach Save für URL/Auth zurück zum Editor.

#### Scheduled times

Cron speichert einen Ausdruck, kein separates Timezone-Feld. Mappt die Nutzer-Angabe sauber auf Cron (z. B. „every weekday at 3am“, „daily at 9am“, „Mondays at 9am“): Cron-Trigger im Editor-Prefill. „my timezone“ / „local time“ als Display-Time-Intent, wenn Stunde/Tag als Cron ausdrückbar. Schedule nicht nur im Prompt lassen bei leerem `workflow.triggers`.

Lokale Zeiten, die der Editor nicht exakt anzeigen kann: kein rohes Cron plus Timezone-Hint. Eine weitere Schedule-Frage vor Editor-Öffnung; keine scheduled Automation ohne Trigger. Kein `cron: {}` prefilling — invalid.

Valid cron examples:

```yaml
# Every hour
cron: { cron: "0 * * * *" }

# Every day at 9:00
cron: { cron: "0 9 * * *" }

# Every Monday at 9:00
cron: { cron: "0 9 * * 1" }

# Weekdays at 9:00
cron: { cron: "0 9 * * 1-5" }
```

#### Tools

Bei nicht offensichtlichen Tools strukturiertes Multi-Select fragen:

| Label | YAML |
|-------|------|
| Comment on PRs | `prComment` |
| Post to Slack | `slack` |
| Read Slack | `readSlack` |
| Request reviewers | `requestReviewers` |
| Manage check runs | `manageCheckRun` |
| Use MCP server | `mcp` |

Bei `slack` / `readSlack`: Channel via Slack-MCP-Discovery vor Draft oder UI-Deferral dokumentieren. Bei `mcp`: zuerst MCP existence gate und MCP auth gate; nur exakte, authentifizierte, nutzbare Catalog-Matches in `workflow.actions`; `mcp.server.name` MUSS `serverName` aus `SERVER_METADATA.json` sein — nie Folder/`serverIdentifier`. Fehlender/nicht eingerichteter MCP: nicht prefilling — Setup oder Editor-Deferral. Existiert aber unauthentifiziert: Stop per MCP auth gate — nicht prefilling, OAuth nicht an Editor delegieren.

#### Prompt + name

Fragen: „What should the agent do when [trigger]?“ Default ein enger Absatz; Nutzer-Länge matchen, wenn mehr gegeben. Cloud Compute im [Cloud Agent dashboard](https://cursor.com/dashboard?tab=cloud-agents). Name + 1–2 Sätze Beschreibung aus vorherigen Antworten vorschlagen.

#### Fast-path

Bei hoher Confidence und vorhandenen Pflichtfeldern direkt zur Draft-Tabelle. Fast-path überspringt nicht: Slack-Channels, Git Repo/Branch, `mcp.server.name`, unaufgelösten Schedule, MCP auth gate — **Hard stop until authed** gilt immer; Draft-Tabelle nicht bei unauthentifiziertem prefilled MCP. Bei Unsicherheit eine fokussierte Frage statt vollem Fragebogen.

### Stage 3 — Draft-Tabelle, Validierung, Finish

Draft als kompakte Markdown-Tabelle in Plain Language recap. Kein Planungsdokument, keine Checkliste, keine „steps I'll take“:

| Draft field | What will open in the editor |
|-------------|------------------------------|
| Name / description | Short plain-language value |
| Trigger | What starts the automation |
| Tools | Enabled capabilities |
| Instructions | The prompt behavior, summarized |
| Resolved settings | Repo / branch, Slack channel, service / project, schedule, and other picker-backed values |
| To finish in editor | Settings deferred to the Automations UI; write "None" if nothing is deferred |

Mit „Does this look correct?“ enden. Kein YAML/JSON-Block anhängen. Nach Genehmigung internen **Validation check**, dann Finish-Handoff-Bestätigung. Plain „yes“ genehmigt Draft, ersetzt nicht die finale Readiness-Bestätigung.

#### Glass finish path (Compliance)

Wenn `cursor-app-control.open_automation` verfügbar, genau ein Finish-Path:

1. Markdown-Draft-Tabelle zeigen.
2. Auf Nutzer-Genehmigung warten.
3. Welche Werte, Setup oder Webhook/Auth-Details im Automations-Editor fertig werden müssen. Direkte Readiness-Frage, z. B. „Are you ready for me to open it for you?“
4. Nach Bestätigung Automations-Editor mit reviewed Draft öffnen.

Kein Save, Browser, Paste-Link, Skip oder Fallback anbieten.

### Post-finish actions (agent-intern)

Vor Editor-Öffnen oder Wegführung aus Chat: kurze finale Handoff-Notiz mit allen deferred Feldern und Caveats. Alle „To finish in editor“-Zeilen, Integration-Setup, Webhook/Auth, Slack DM/Channel-Picker, Cloud-Compute, absichtlich im Editor gelassene Schedules. Mit Readiness-Frage enden. Erinnerungen nicht nach dem Open — Nutzer sieht Chat evtl. nicht mehr.

- **Automations-Editor mit Draft öffnen.** Nur neue Automations. `cursor-app-control.open_automation` mit reviewed WorkflowData JSON als `prefillWorkflowData`. Keine Backend-Automation-Tools, kein `open_resource`, kein Browser-URL-Builder, kein `cursor://` für diesen Bucket. Bei `open_automation`-Fehler plain erklären und stoppen.
- **Agents Window required.** Wenn weder `open_automation` noch `open_resource`: „Please use this skill in the Agents Window.“ Nicht draften, submiten, Browser-Prefill oder Pfade automatisch wechseln.

---

## Referenz

### Discover before ask

Bevor du picker-gestützte Werte fragst (Repo, Slack Channel/DM, GitHub/GitLab PR/Comment-Scope, PagerDuty Service, Linear Team, Sentry Project, …), proaktiv prüfen, ob Integration-MCP oder CLI verfügbar und authentifiziert ist, dann Records holen. Bei MCP-Integrationen muss der Check der tatsächliche MCP/Tool-Catalog des Nutzers sein. Ergebnis für nächste Frage: **1 Match → inline bestätigen; 2 → inline either/or; 3+ → `AskQuestion` Single-Select.** Freiform erst nach erschöpfter scoped Discovery oder Editor-Deferral.

**Auth boundary.** Integration list/search/read, `gh`, `glab` wenn verbunden und authentifiziert. Dieser Pfad nicht zum Listen/Getten/Inspizieren/Erstellen/Updaten/Finishen/Prefillen von Cursor Automations.

- **MCP für `mcp`-Automation-Aktion** folgt **MCP auth gate** — stoppen, erklären, im Chat authentifizieren vor Prefill. Inline `mcp_auth` im Agents Window wenn gelistet.
- **Andere Integrationen** (Slack, PagerDuty, `gh`/`glab`): bei fehlend/unavailable fragen, ob Setup vor Fortsetzung. Bei Ja Setup führen und nach Bestätigung Discovery retry. Bei Nein Draft fortsetzen und Hinweis, Setup später im Editor. Für diese nicht-`mcp`-Integrationen kein `mcp_auth` ohne explizites **Retry after setup**.

#### GitHub / GitLab Repo-Scope

Natürliche Repo-Nicknames akzeptieren; nicht zuerst `owner/repo` verlangen; Nutzer nicht bitten, Repos zu listen, bevor scoped CLI-Discovery versucht wird. Reihenfolge:

1. **Exaktes oder aktuelles Repo.** Bei exaktem `owner/repo`, „this repo“ oder offensichtlichem Checkout: `gh repo view` mit JSON für Repo + Default-Branch. GitLab: äquivalentes `glab repo view`. Auth-Status nur bei Auth-Fehler beim Lookup.
2. **Mehrere benannte Kandidaten.** Keine unscoped Suche. Exakte `owner/repo` per Lookup oder scoped Discovery bei gemeinsamem Owner/Org/Namespace, Checkout oder Produktkontext. Ohne scoped Lookup: `AskQuestion` über benannte Repos plus **Pick in Automations UI** vor CLI.
3. **Nutzer muss wählen.** Bei scoped Suche Kandidaten proaktiv holen: `gh search repos`, scoped `gh repo list`, äquivalent `glab`. Auch wenn der nächste Schritt Nutzer-Pick ist.
4. **Kandidaten präsentieren.** 1 Match → inline; 2 → either/or; 3+ → `AskQuestion` plus **Pick in Automations UI**. Bei Mehrdeutigkeit nach Search/List: `AskQuestion` über Partial Matches + UI-Deferral.
5. **Branch bestätigen.** Default-Branch aus Lookup. Braucht Automation spezifischen Branch und Discovery löste ihn nicht: Branch fragen oder Default explizit anbieten.
6. **Discovery fehlgeschlagen.** Fehlendes/unauthentifiziertes `gh`/`glab` oder Lookup-Fehler: Repo/Branch fragen oder Pick in Automations UI. Nicht auf CLI-Setup blockieren außer **Retry after setup**.

Guardrails: keine breite private Repo-Inventur oder unscoped Org-Sweeps. `gh repo list`/`glab repo list` scoped. `git remote` nicht als einzige Wahrheit.

#### Slack

Slack-MCP-Discovery vor Channel-Frage, immer bei `slackTrigger`/`slack`/`readSlack`. **Specify now** = Agent führt Discovery zuerst aus — nicht „Nutzer nach IDs fragen“. 1 Channel → inline; 2 → either/or; 3+ → `AskQuestion` (+ **Pick in Automations UI**). **Kein Prefill mit leeren Channels nach Specify now ohne Discovery.** Blockiert → **Retry after setup** / **Continue without MCP** / **Pick in Automations UI**.

Slack `channel` nur `C…`/`G…`/`D…` — nie `U…`. Replies: **respond in the triggering thread** getrennt von **send to a specific channel or DM**. Leere `{}`-Actions gültig bei **Select channels** im Editor; Deferral in Draft-Tabelle.

#### PagerDuty / Linear / Sentry

PagerDuty MCP list services vor `serviceIds`: 1/2 inline; 3+ → `AskQuestion`. **Optional `serviceIds`** — sonst UI-Deferral. Linear/Sentry gleiches Pattern: discover wenn MCP verbunden; sonst Editor.

### YAML-Output-Shape (agent-intern)

Wire-Format = reviewed Automations-Draft an `open_automation` als `prefillWorkflowData` — kanonisches Proto-JSON mit vollen Enum-Namen. PR-Scope auf `git.pullRequest`; `workflow.gitConfig` für `repo`+`branch` bei non-`git`-Triggers mit Checkout. `ignoreDraftPrs`, nicht `ignoreDraftPr`. Slack-IDs: `C…`/`G…`/`D…`.

Skeleton:

```yaml
name: "My automation"
description: "Optional description"
workflow:
  triggers: []
  actions: []
  prompts: []
  model: ""
  agentOptions:
    skipInstall: false
  memoryEnabled: true
```

Prompts nutzen `|`-Block-Scalar (`>-` bricht Bullets). Leere `{}`-Actions gültig bei UI-only-Feldern. `mcp.server.name` Pflicht bei `mcp`; Name = `serverName` aus `SERVER_METADATA.json` — nicht Folder/`serverIdentifier`. Siehe MCP existence gate.

**Trigger-oneof-Keys sind exhaustiv.** Jeder Eintrag in `workflow.triggers` nutzt genau einen Proto-Key: `cron`, `git`, `slackTrigger`, `slackReactionAdded`, `slackChannelCreated`, `microsoftTeamsTrigger`, `microsoftTeamsChannelCreated`, `linear`, `webhook`, `pagerduty`, `sentry`. Niemals erfinden oder paraphrasieren — Editor decodiert mit `ignoreUnknownFields: true`, droppt unbekannte Keys, rendert unkonfigurierbare „Configure trigger“-Karte, blockiert Save. Leere `{}`-Trigger gleicher Failure-Mode; nie prefilling ohne vollständigen Namen.

### Validation check (agent-intern)

Nach Draft-Genehmigung: YAML vs. Checklist + Proto validieren (PR-Enums, `ignoreDraftPrs`, Slack-ID-Präfixe, `gitConfig` wenn nötig, `mcp.server.name` bei `mcp`, MCP-Actions mit authentifizierten Catalog-Matches, Description ohne `__securitybot_metadata__`/`customInstruction`). **Keine inline JSON-Schema-Validatoren** oder Shell-Snippets für Automation-YAML. Bei Fehlschlag plain erklären; volles YAML nur auf Nutzerwunsch. **Keine Backend-Automation-Tools, kein Shell an Repo-Skripten** — nur `open_automation` für Editor-Handoff.

---

## Appendix — Trigger-Auswahl-Tabellen

Diese Labels nur agent-intern — IDs Nutzern nie zeigen. Bei strukturiertem Picker Zeilen vor Option-Cap splitten.

### Trigger-Kategorie

**Prompt:** „When should this automation run?“

| Option label | Option id | Proto / YAML |
|--------------|-----------|----------------|
| On a schedule | `cron` | `cron` |
| On a GitHub / GitLab event | `git` | `git` → specific event |
| On a Slack event | `slack` | specific event: `slackTrigger` vs `slackChannelCreated` |
| On a Linear event | `linear` | `linear` → specific event |
| On a PagerDuty incident event | `pagerduty` | `pagerduty` → specific event |
| On a Sentry issue event | `sentry` | `sentry` → specific event |
| On an incoming HTTP webhook | `webhook` | `webhook` |

### Spezifisches Event (pro Kategorie)

**`cron`** — Prompt: "Which schedule shape?"

| Option label | Option id | Notes |
|--------------|-----------|-------|
| Every hour | `cron_every_hour` | UI preset |
| Every day | `cron_every_day` | preset |
| Every week | `cron_every_week` | preset |
| Custom cron expression | `cron_custom` | user supplies full cron |

**`git`** — Prompt: "Which Git event?"

| Option label | Option id | Maps to |
|--------------|-----------|---------|
| Draft pull request opened | `git_draft_opened` | `DRAFT_OPENED` |
| Pull request opened | `git_pr_opened` | `OPENED` |
| Code pushed to a pull request | `git_pr_pushed` | `PUSHED` |
| Pull request merged | `git_pr_merged` | `MERGED` |
| Comment added on pull request | `git_pr_commented` | `COMMENTED` |
| Label change | `git_label` | label trigger |
| New push to branch | `git_push` | push |
| Checks completed | `git_ci` | `ciCompleted` |

**`slack`** — Prompt: "Which Slack trigger?"

| Option label | Option id | YAML |
|--------------|-----------|------|
| New message in channel | `slack_message` | `slackTrigger` |
| Reaction added to message | `slack_reaction_added` | `slackReactionAdded` |
| Channel created | `slack_channel_created` | `slackChannelCreated` |

**`slackReactionAdded` Payload** — `{ channels: ["C…"], emojiName: "<name>" }`. `emojiName` ist der Slack-Kurzname **ohne** umgebende Doppelpunkte (z. B. `thumbsup`, nicht `:thumbsup:`); Server normalisiert Unicode-Emoji zum passenden Alias beim Save. Completion-Reactions auf `slackReactionAdded`-Triggers nicht unterstützt (Rekursion) und werden still gedroppt.

**Completion-Reaction auf Slack-Message-Trigger.** „React with `:foo:` when the agent finishes“ ist Completion-Reaction-Option auf `slackTrigger`, kein separater Trigger. `slackCompletionReactionMode: SLACK_COMPLETION_REACTION_MODE_CUSTOM` und `slackCompletionReactionCustomEmoji: ":foo:"` (mit Doppelpunkten) auf demselben `slackTrigger`. Keinen `slackReactionAdded`-Trigger für Completion-Verhalten.

**„react with …“ disambiguieren.** „react with X to trigger“ → `slackReactionAdded` (`emojiName: "x"`, ohne Doppelpunkte). „react with X when done“ / „upon completion“ / „after success“ → `slackTrigger` mit Completion-Reaction-Feldern oben. Bei Mehrdeutigkeit eine fokussierte Frage statt raten.

**`linear`** — Prompt: "Which Linear event?"

| Option label | Option id | Proto JSON |
|--------------|-----------|------------|
| Issue created | `linear_created` | `linear.issueCreated` |
| Issue status changed | `linear_status` | `linear.statusChanged` |
| End of cycle | `linear_cycle` | `linear.endOfCycle` |

**`pagerduty`** — Prompt: "Which PagerDuty incident event?"

| Option label | Option id | Proto JSON |
|--------------|-----------|------------|
| Incident triggered | `pagerduty_triggered` | `incidentTriggered: {}` |
| Incident acknowledged | `pagerduty_ack` | `incidentAcknowledged: {}` |
| Incident resolved | `pagerduty_resolved` | `incidentResolved: {}` |
| Any incident event | `pagerduty_any` | `incidentAny: {}` |

Optional `serviceIds`. Proto kann `incidentEscalated` enthalten — nur auf Nutzerwunsch.

**`sentry`** — Prompt: "Which Sentry issue event?"

| Option label | Option id | Proto JSON |
|--------------|-----------|------------|
| Issue created | `sentry_created` | `issueCreated: {}` |
| Issue resolved | `sentry_resolved` | `issueResolved: {}` |
| Issue assigned | `sentry_assigned` | `issueAssigned: {}` |
| Issue archived | `sentry_archived` | `issueArchived: {}` |
| Issue unresolved | `sentry_unresolved` | `issueUnresolved: {}` |
| Any issue event | `sentry_any` | `issueAny: {}` |

Optional `projectIds`.

**`webhook`** — specific-event überspringen; `webhook: {}`; Nutzer erhält URL/Auth nach Save.

---
