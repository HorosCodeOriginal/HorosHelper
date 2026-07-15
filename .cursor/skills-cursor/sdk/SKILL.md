---
name: sdk
description: >-
  Führt beim Bau von Apps, Skripten, CI-Pipelines oder Automations auf Basis
  des Cursor SDK — TypeScript (`@cursor/sdk`) oder Python (`cursor-sdk` /
  `cursor_sdk`). Verwenden bei Integration, Installation oder Code gegen das
  Cursor SDK; bei `Agent.create`, `Agent.prompt`, `Agent.resume`, `agent.send`,
  `run.stream`, `run.messages`, `CursorAgentError`, `@cursor/sdk`, `cursor-sdk`,
  `cursor_sdk`; bei programmatischem Ausführen von Cursor-Agents aus Skript,
  CI/CD, GitHub Action, Backend oder außerhalb der IDE; bei Local vs. Cloud,
  MCP-Konfiguration, Streaming, Cancellation, Errors; oder REST `/v1/agents`.
  Eher diesen Skill laden als aus dem Gedächtnis antworten.
---
# Cursor SDK

Das Cursor SDK führt Cursor-Agents programmatisch aus. Zwei Sprachvarianten, gleiche Konzepte:

- **TypeScript** (`@cursor/sdk`, npm) — Docs: [cursor.com/docs/sdk/typescript](https://cursor.com/docs/sdk/typescript)
- **Python** (`cursor-sdk`, pip) — Docs: [cursor.com/docs/sdk/python](https://cursor.com/docs/sdk/python)

Beide in Public Beta, gleiches `Agent` → `Run`-Modell für Local (auf der Maschine des Callers gegen `cwd`) und Cloud (Cursor-VM gegen geklontes Repo).

Nutze diesen Skill, um jemandem **schnell eine funktionierende Integration** zu geben und **Fallstricke für Neue** zu vermeiden. Die kanonischen Docs sind die Referenz; dieser Skill ergänzt Entscheidungen, Failure-Mode-Prevention und erweiterbare Patterns.

## Sprache wählen

Vor dem ersten Code die Sprache festlegen. Falsche Wahl = ganze Integration umschreiben. Reihenfolge:

1. **Nutzer hat sie benannt.** `@cursor/sdk`, `cursor-sdk`, `cursor_sdk`, `npm install`, `pip install`, `import { Agent } from "@cursor/sdk"`, `from cursor_sdk import` — dem folgen.
2. **Codebase signalisiert.** `pyproject.toml` / `requirements.txt` / `.py` → Python. `package.json` / `tsconfig.json` / `.ts` → TypeScript. Polyglot-Repo: fragen, in welches Subdir die Integration kommt.
3. **Kein Signal.** Eine kurze Frage und warten: *„TypeScript oder Python?“* Nicht für den Nutzer entscheiden.

Der Rest zeigt TypeScript und Python nebeneinander. Konzepte identisch; Syntax unterschiedlich — camelCase vs. snake_case, async-by-default vs. sync-default mit async-Mirror, `await using` vs. `with`.

## Stimme und Haltung

Dieser Skill hilft dem Nutzer, **mit dem SDK zu bauen**. Nicht validieren, gratulieren oder das SDK als Wahl verkaufen. Nutzer-Intent ist Input; deine Aufgabe ist Ausführung.

- **Nutzer nennt SDK explizit** („Cursor SDK“, `@cursor/sdk`, `Agent.create` usw.): annehmen, dass er das SDK kennt und nutzen will. Kein Framing, kein Pep-Talk — direkt Integration liefern.
- **Problem passt zum SDK, aber nicht benannt**: kurz als Frage anbieten: *„Das Cursor SDK wäre hier mein Ansatz — soll ich so designen, oder hast du eine andere Runtime im Sinn?“* Bei Bestätigung weitermachen.
- **In beiden Fällen Intent nicht zurückspiegeln.** Der Nutzer weiß, was er will. Zur Design-Entscheidung oder zum ersten Fakt kommen.

Vermeide diese konkreten Einstiege (und nahe Verwandte):

- „Gute Nachricht: das ist genau das Pattern …“
- „Das SDK ist für diese Form gebaut.“
- „Super, du bist hier richtig.“
- „Das ist fast genau das X, für das das SDK designed ist.“
- Jeder Einstieg, der die Wahl des Nutzers lobt oder sein Ziel schmeichelhaft wiederholt.

Bevorzuge:

- Starte mit der Design-Entscheidung oder dem ersten, was er wissen muss.
- Wenn du eine echte Design-Wahl ansprechen musst (local vs. cloud, prompt vs. send, sync vs. stream, sync Python vs. async Python), benenne sie in einem Satz und erkläre warum.

## Die drei Aufruf-Patterns

Fast jede SDK-Integration lässt sich auf eine dieser drei Formen reduzieren. Wähle die passende — misch sie nicht.

### 1. `Agent.prompt(...)` — One-Shot

Für Fire-and-Forget-Skripte, GitHub-Actions-Steps oder jeden Flow „Prompt senden, Ergebnis holen, beenden“. Kein Streaming, keine Follow-ups, kein Cleanup merken. Wenn du das nimmst und sofort resumest, wolltest du Pattern 2.


**TypeScript:**

```typescript
import { Agent } from "@cursor/sdk";

const result = await Agent.prompt("Refactor src/utils.ts for readability", {
  apiKey: process.env.CURSOR_API_KEY!,
  model: { id: "composer-2.5" },
  local: { cwd: process.cwd() },
});
console.log(result.status, result.result);
```

**Python:**

```python
import os
from cursor_sdk import Agent, AgentOptions, LocalAgentOptions

result = Agent.prompt(
    "Refactor src/utils.py for readability",
    AgentOptions(
        api_key=os.environ["CURSOR_API_KEY"],
        model="composer-2.5",
        local=LocalAgentOptions(cwd=os.getcwd()),
    ),
)
print(result.status, result.result)
```

### 2. `Agent.create(...)` + `agent.send(...)` — dauerhaft mit Follow-ups

Nutzen, wenn du Streaming, Multi-Turn-Conversation oder Lifecycle-Ops (cancel, Status-Listener) brauchst. Das ist die Form der meisten nicht-trivialen Integrationen.

**TypeScript:**

```typescript
import { Agent } from "@cursor/sdk";

await using agent = await Agent.create({
  apiKey: process.env.CURSOR_API_KEY!,
  model: { id: "composer-2.5" },
  local: { cwd: process.cwd() },
});

const run = await agent.send("Find the bug in src/auth.ts");
for await (const event of run.stream()) {
  if (event.type === "assistant") {
    for (const block of event.message.content) {
      if (block.type === "text") process.stdout.write(block.text);
    }
  }
}
await run.wait();

// Follow-up behält vollen Conversation-Kontext.
const run2 = await agent.send("Now write a regression test for it");
await run2.wait();
```

**Python:**

```python
import os
from cursor_sdk import Agent, LocalAgentOptions

with Agent.create(
    model="composer-2.5",
    api_key=os.environ["CURSOR_API_KEY"],
    local=LocalAgentOptions(cwd=os.getcwd()),
) as agent:
    run = agent.send("Find the bug in src/auth.py")
    for message in run.messages():
        if message.type == "assistant":
            for block in message.message.content:
                if block.type == "text":
                    print(block.text, end="")
    run.wait()

    # Follow-up behält vollen Conversation-Kontext.
    run2 = agent.send("Now write a regression test for it")
    run2.wait()
```

Das Python-SDK ist standardmäßig sync. Für Server, Bots und parallele Orchestrierung nutze `AsyncClient.launch_bridge(...)` als async Context Manager und `AsyncAgent`. Es gibt keinen globalen async-Default-Client — instanziiere einen pro Event Loop und misch nie sync und async Clients im selben Code-Pfad.

### 3. `Agent.resume(...)` — bestehenden Agent später fortsetzen

Nutzen über Prozessgrenzen hinweg: Cron, der den Cleanup von gestern fortsetzt, Webhook, der einen User-Agent erweitert, interaktive CLI, die Conversation-State neu lädt. Runtime wird automatisch aus dem ID-Präfix erkannt — `bc-` ist Cloud, alles andere ist local.

**TypeScript:**

```typescript
await using agent = await Agent.resume(previousAgentId, {
  apiKey: process.env.CURSOR_API_KEY!,
});
const run = await agent.send("Also update the changelog");
await run.wait();
```

**Python:**

```python
import os
from cursor_sdk import Agent, AgentOptions

with Agent.resume(
    previous_agent_id,
    AgentOptions(api_key=os.environ["CURSOR_API_KEY"]),
) as agent:
    run = agent.send("Also update the changelog")
    run.wait()
```

**Inline-MCP-Server werden über Resume nicht persistiert** — sie tragen oft Secrets und leben nur im Speicher. Bei Resume in beiden Sprachen erneut übergeben.

## Die fünf häufigsten Fallen

Die treffen fast jede neue Integration. Alle lassen sich vermeiden, wenn du sie kennst.

### 1. Versehentlich die falsche Runtime wählen

`AgentOptions` verlangt weder `local` noch `cloud`; das SDK wählt local, wenn keines gesetzt ist. Die Falle: du wolltest Cloud und hast `cloud` vergessen — du bekommst still local, kein Fehler, nur eine local Agent-ID und einen local Executor.

- **TypeScript:** `cloud: { repos: [...] }` für Cloud, `local: { cwd }` für local — auch wenn local Default ist.
- **Python:** `cloud=CloudAgentOptions(repos=[...])` oder `local=LocalAgentOptions(cwd=...)`.

Setze immer explizit eines von beiden. Ein Zeile Aufwand; eine Stunde nicht bemerken kostet mehr.

### 2. Zwei Fehlerarten, ein Instinkt sie zu vermischen

Ein geworfener `CursorAgentError` heißt: der Run **wurde nie ausgeführt** (Auth, Config, Netzwerk). Ein zurückgegebenes `result.status == "error"` heißt: der Run **lief und scheiterte**. Unterschiedliche Fixes, Exit-Codes, Observability.

**TypeScript:**

```typescript
import { Agent, CursorAgentError } from "@cursor/sdk";

try {
  const run = await agent.send(prompt);
  const result = await run.wait();
  if (result.status === "error") {
    // Run gestartet, aber mid-flight fehlgeschlagen. Transcript, Git-State, Tool-Outputs prüfen.
    console.error("run failed: " + result.id);
    process.exit(2);
  }
} catch (err) {
  if (err instanceof CursorAgentError) {
    // Nicht gestartet. Auth, Config, Netzwerk. Umgebung fixen, retry.
    console.error("startup failed: " + err.message + ", retryable=" + err.isRetryable);
    process.exit(1);
  }
  throw err;
}
```

**Python:**

```python
import sys
from cursor_sdk import CursorAgentError

try:
    run = agent.send(prompt)
    result = run.wait()
    if result.status == "error":
        # Run gestartet, aber mid-flight fehlgeschlagen. Transcript, Git-State, Tool-Outputs prüfen.
        print("run failed: " + result.id, file=sys.stderr)
        sys.exit(2)
except CursorAgentError as err:
    # Nicht gestartet. Auth, Config, Netzwerk. Umgebung fixen, retry.
    print(
        "startup failed: " + err.message + ", retryable=" + str(err.is_retryable),
        file=sys.stderr,
    )
    sys.exit(1)
```

### 3. Vergessen zu disposen leakt Ressourcen

Das SDK hält Handles zu local Executors, persistierten Run-Stores und HTTP-Clients. Ohne Dispose leaken Child-Prozesse, offene DBs und (in Long-Running-Services) Speicher.

- **TypeScript:** `await using agent = await Agent.create(...)` ist der sauberste Weg. Sonst `try/finally` mit `await agent[Symbol.asyncDispose]()`. `Agent.prompt(...)` disposed für dich.
- **Python (sync):** `with Agent.create(...) as agent:` ist der sauberste Weg. Sonst `agent.close()` in `finally`. Long-Running-Prozesse mit Default-Client sollten `close_default_client()` beim Shutdown rufen.
- **Python (async):** zwei async Context Manager verschachteln — `async with await AsyncClient.launch_bridge(...) as client:` dann `async with await client.agents.create(...) as agent:`. `Agent.prompt(...)` disposed für dich.

### 4. Streaming ist optional, aber `wait()` fast immer Pflicht

Der Stream ist Beobachtung; `wait()` liefert das finale Ergebnis. Streaming kannst du weglassen, aber ohne `wait()` weißt du nicht, ob der Run fertig, fehlerhaft oder abgebrochen ist — und du leakest interne Watcher. Immer `wait()` aufrufen. Ohne Live-Output reicht `wait()` allein.

- **TypeScript:** `run.stream()` ist async iterable von `SDKMessage`. `await run.wait()` liefert `RunResult`.
- **Python:** `run.messages()` liefert typisierte SDK-Messages; `run.events()` lower-level Envelopes. `run.wait()` liefert `RunResult`. Async: `async for ...` und `await run.wait()`. Convenience: `run.text()` blockiert auf `wait()` und liefert finalen Assistant-Text; `run.iter_text()` streamt nur Text-Chunks. `run.stream()` ist Alias für `run.messages()`.

### 5. Nicht jede `run`-Operation auf jeder Runtime

`Run` bietet stream/messages, wait, cancel, conversation — detached/rehydrated Runs (Handles von `Agent.getRun(...)` nach Schließen des Live-Event-Stores) unterstützen evtl. nicht alles. Mit `supports(...)` absichern:

```typescript
if (run.supports("cancel")) await run.cancel();
if (run.supports("conversation")) console.log(await run.conversation());
```

```python
if run.supports("cancel"):
    run.cancel()
if run.supports("conversation"):
    print(run.conversation())
```

`run.unsupportedReason(op)` (TypeScript) / `run.unsupported_reason(op)` (Python) sagt dir warum.

## Local vs. Cloud — je ein Satz

- **Local** — läuft auf der Maschine des Callers gegen `cwd`, nutzt dessen Umgebung und Credentials. Gut für Dev-Loops und CI mit Repo-Checkout.
- **Cloud** — läuft auf Cursor-gehosteter VM gegen frisch geklontes Repo. Gut für lange Jobs, Fire-and-Forget-Automation und echte PRs (`autoCreatePR: true` in TypeScript; `auto_create_pr=True` in Python).

## Auth — Minimum

```bash
export CURSOR_API_KEY="cursor_..."  # User-API-Key oder Team-Service-Account-Key
```

Beide SDKs lesen `CURSOR_API_KEY`, wenn kein Key explizit übergeben wird. User-Keys: [Cursor Dashboard → Integrations](https://cursor.com/dashboard/integrations); Team-Service-Account-Keys: Team Settings → Service accounts. Team-Admin-API-Keys noch nicht unterstützt.

Bei 401s: Key mit Whitespace, Key aus anderer Umgebung, oder User ohne Repo-Zugriff für Cloud-Run.

## Modell-Auswahl

Keine unüblichen Model-IDs hardcoden ohne Zugriff des Callers — Modelllisten ändern sich.

**TypeScript:**

```typescript
import { Cursor } from "@cursor/sdk";

const models = await Cursor.models.list({ apiKey: process.env.CURSOR_API_KEY! });
```

**Python:**

```python
from cursor_sdk import Cursor

models = Cursor.models.list()  # fällt zurück auf CURSOR_API_KEY
```

`composer-2.5` ist der aktuelle Default für die meisten Integrationen. `{ id: "auto" }` (TS) / `model="auto"` (Python) lässt den Server wählen. `Cursor.models.list()` liefert gültige IDs, Parameter pro Modell (Reasoning Effort, Max Mode) und Preset-Varianten für das Konto.

Modell ist in beiden SDKs **für local Pflicht**. Für Cloud fällt TypeScript auf Server-Default zurück, wenn weggelassen; Python dokumentiert es als Pflicht für beide Runtimes. Übergib trotzdem eines — für vorhersagbares Verhalten.

## MCP-Server

Beide SDKs nutzen dasselbe Konzept: HTTP-Transport (statische `headers` oder OAuth `auth`) oder stdio (`command` / `args` / `env`). Server inline bei `Agent.create` oder `agent.send` für den häufigsten Fall.

- **Local Agents** können stdio- oder HTTP-Server auf der Maschine des Callers nutzen. Braucht ein local MCP-Server OAuth-Login, kann das SDK gespeicherten Login aus der Cursor-App wiederverwenden, öffnet aber keinen Browser.
- **Cloud Agents** unterstützen HTTP und stdio (stdio in der Cloud-VM). HTTP `headers` und `auth` werden vom Cursor-Backend gehandhabt und vor der VM geschwärzt. Stdio `env` geht in die VM — wie Runtime-Secrets behandeln.
- **Inline-Server ersetzen Creation-Time-Server bei per-send-Override vollständig — nicht gemerged.**
- **Bei Resume mit MCP-Tools Server beim Resume erneut übergeben.** Inline-Server werden nicht persistiert.

Vollständiges Schema und Auth: SDK-Docs pro Sprache und [Cursor MCP](https://cursor.com/docs/mcp).

## Production-Best-Practices

Für unbeaufsichtigte Integrationen:

1. **Immer disposen.** `await using` (TypeScript) oder `with ... as agent:` (Python). Nicht verhandelbar.
2. **Startup-Fehler von Run-Fehlern trennen.** Exit 1 bei geworfenem `CursorAgentError`, Exit 2 bei `result.status == "error"`, Exit 0 nur bei `finished`.
3. **`run.id` und `agent.agentId` / `agent.agent_id` direkt nach `send()` loggen** vor dem Stream. Hängt der Stream, brauchst du die IDs für Dashboard oder `Agent.getRun(...)` / `Agent.get_run(...)`.
4. **`error.isRetryable` / `err.is_retryable` respektieren.** Backend sagt, ob Retry sicher ist. Python-Fehler haben auch `retry_after` — vor exponentiellem Backoff einhalten. Blinde Retries können doppelte Cloud-Runs erzeugen.
5. **Ambient Settings nur laden, wenn gewollt.** TypeScript-Default: `local.settingSources: []`; Python: kein `local.setting_sources`. Beides heißt „nur Inline-Config“. `"all"` lädt Projekt/User/Team/MDM vom Caller — selten gewünscht in Services. Setting Sources wirken nicht auf Cloud — Cloud ehrt team/project/plugins.
6. **Für Cloud in CI `skipReviewerRequest: true` (TypeScript) / `skip_reviewer_request=True` (Python)**, außer ein Mensch soll gepaged werden — unterdrückt Reviewer-Request und hält PR-Notifications leise.
7. **`apiKey` / `api_key` in Shared-Infrastructure explizit übergeben** statt Env-Var. Credential-Abhängigkeit klar; verhindert Cross-Tenant-Fehler.
8. **One-Shot `Agent.prompt(...)` für echte One-Shots** — disposed für dich, schwerer zu leaken.

## Run beobachten, den du nicht gestartet hast

Jeden Agent oder Run später per ID inspizieren. Cloud-IDs mit `bc-` routen automatisch zur Cloud-API; alles andere ist local.

**TypeScript:**

```typescript
const info = await Agent.get("bc-abc123", { apiKey });
const run = await Agent.getRun(runId, { runtime: "cloud", agentId: "bc-abc123", apiKey });

// Local: cwd, wo der Agent erstellt wurde.
const localList = await Agent.list({ runtime: "local", cwd: process.cwd() });
```

**Python:**

```python
from cursor_sdk import CursorClient

with CursorClient.launch_bridge(workspace=".") as client:
    info = client.agents.get("bc-abc123")
    run = client.agents.get_run(run_id)

    # Local: nach cwd filtern.
    local_list = client.agents.list(runtime="local", cwd=".")
```

Eine Cloud-Agent-ID mit `bc-`-Präfix ist **keine** Run-ID. Hast du nur eine Run-ID (aus Log oder Webhook), an `getRun` / `get_run` mit Runtime-Hint übergeben; nicht verwechseln.

## Canvas anbieten

Wenn die Integration Agents überwacht, listet oder visualisiert — Dashboards aktiver Runs, Conversation-Replays, Tool-Call-Timelines — biete ein Cursor Canvas. Bei Zustimmung vollständig an den `canvas`-Skill delegieren.

## Was dieser Skill nicht abdeckt

- Die [Cloud Agents REST API](https://cursor.com/docs/cloud-agent/api/endpoints) (`/v1/agents/*`). Für Sprachen ohne First-Party-SDK oder minimale HTTP-Oberfläche.
- `.cursor/hooks.json`-Hooks. Beide SDKs respektieren sie; keines verwaltet sie. Siehe [Hooks](https://cursor.com/docs/hooks).
- Self-hosted Cloud (private Workers, Self-hosted Pools, my-machines). Siehe [Self-hosted pool](https://cursor.com/docs/cloud-agent/self-hosted-pool) und verwandte Docs.
- SDKs außer TypeScript und Python. Dort ist die REST API die portable Option.
