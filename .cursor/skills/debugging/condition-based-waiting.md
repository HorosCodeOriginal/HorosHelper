# Condition-Based Waiting

## Überblick

Flaky Tests raten oft mit willkürlichen Verzögerungen beim Timing. Das erzeugt Race Conditions — Tests laufen auf schnellen Maschinen, scheitern unter Last oder in CI.

**Kernprinzip:** Warte auf die tatsächliche Bedingung, die dich interessiert, nicht auf eine Schätzung der Dauer.

## Wann nutzen

**Nutzen, wenn:**

- Tests willkürliche Delays haben (`setTimeout`, `sleep`, `time.sleep()`)
- Tests flaky sind (manchmal grün, unter Last rot)
- Tests bei parallelem Lauf timeouten
- Auf async Operationen gewartet wird

**Nicht nutzen, wenn:**

- Tatsächliches Timing-Verhalten getestet wird (Debounce, Throttle-Intervalle)
- Bei willkürlichem Timeout immer dokumentieren WARUM

## Kern-Pattern

```typescript
// BAD: Guessing at timing
await new Promise(r => setTimeout(r, 50));
const result = getResult();
expect(result).toBeDefined();

// GOOD: Waiting for condition
await waitFor(() => getResult() !== undefined);
const result = getResult();
expect(result).toBeDefined();
```

## Schnell-Patterns

| Szenario          | Pattern                                              |
| ----------------- | ---------------------------------------------------- |
| Auf Event warten  | `waitFor(() => events.find(e => e.type === 'DONE'))` |
| Auf State warten  | `waitFor(() => machine.state === 'ready')`           |
| Auf Count warten  | `waitFor(() => items.length >= 5)`                   |
| Auf Datei warten  | `waitFor(() => fs.existsSync(path))`                 |
| Komplexe Bedingung | `waitFor(() => obj.ready && obj.value > 10)`         |

## Implementierung

Generische Polling-Funktion:

```typescript
async function waitFor<T>(
  condition: () => T | undefined | null | false,
  description: string,
  timeoutMs = 5000
): Promise<T> {
  const startTime = Date.now();

  while (true) {
    const result = condition();
    if (result) return result;

    if (Date.now() - startTime > timeoutMs) {
      throw new Error(`Timeout waiting for ${description} after ${timeoutMs}ms`);
    }

    await new Promise(r => setTimeout(r, 10)); // Poll every 10ms
  }
}
```

Siehe `condition-based-waiting-example.ts` in diesem Verzeichnis für die vollständige Implementierung mit domänenspezifischen Helfern (`waitForEvent`, `waitForEventCount`, `waitForEventMatch`).

## Häufige Fehler

**Zu schnelles Polling:** `setTimeout(check, 1)` — verschwendet CPU
**Fix:** Alle 10 ms pollen

**Kein Timeout:** Endlosschleife, wenn Bedingung nie eintritt
**Fix:** Immer Timeout mit klarer Fehlermeldung

**Stale data:** State vor der Schleife cachen
**Fix:** Getter in der Schleife für frische Daten aufrufen

## Wann willkürliches Timeout korrekt ist

```typescript
// Tool ticks every 100ms - need 2 ticks to verify partial output
await waitForEvent(manager, 'TOOL_STARTED'); // First: wait for condition
await new Promise(r => setTimeout(r, 200)); // Then: wait for timed behavior
// 200ms = 2 ticks at 100ms intervals - documented and justified
```

**Anforderungen:**

1. Zuerst auf auslösende Bedingung warten
2. Auf bekanntem Timing basieren (nicht raten)
3. Kommentar mit WARUM

## Real-World-Impact

Aus einer Debug-Session:

- 15 flaky Tests in 3 Dateien behoben
- Pass-Rate: 60 % -> 100 %
- Laufzeit: 40 % schneller
- Keine Race Conditions mehr
