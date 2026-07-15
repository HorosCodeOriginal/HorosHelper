---
name: wide-ultra-logical
version: 1.0.0
description: >-
  Vollständige logische Implementierung — Ende-zu-Ende-Features ohne Stubs,
  mit Edge Cases und Selbst-Verifikation. Aktivieren bei /wide-ultra-logical,
  /wul, /wide, /ultra-logical, /boost:wide oder wenn der Nutzer vollständige
  Feature-Implementierung ohne Platzhalter verlangt.
auto_activates:
  - "/wide-ultra-logical"
  - "/wul"
  - "/wide"
  - "/ultra-logical"
  - "/boost:wide"
  - "wide ultra logical"
  - "vollständige Implementierung"
  - "ohne Stubs"
  - "end-to-end feature"
token_budget: 1800
---

# Wide Ultra-Logical / Vollständige logische Implementierung

## Zweck

Formalisiert `/boost` auf **Feature-Ebene**: nicht nur kompilierbare Snippets, sondern Ende-zu-Ende-fertige, logisch geschlossene Implementierung. Für Executors (Aang, Katara, Momo, Appa, Suki) und jeden Agent, der mit Wide Ultra-Logical beauftragt wird.

Gilt für die aktuelle Aufgabe und Follow-ups, bis `/wide-ultra-logical:off` oder `/wul:off`.

## Wann aktiv

- Nutzer sagt `/wide-ultra-logical`, `/wul`, `/wide`, `/ultra-logical`, `/boost:wide`
- Nutzer verweist auf `@wide-ultra-logical` oder die Regel `.cursor/rules/wide-ultra-logical.mdc`
- Orchestrator erwähnt Wide Ultra-Logical in Delegation oder Executor-Kontext
- Nutzer verlangt explizit vollständige Feature-Implementierung ohne Platzhalter oder Stubs

**Deaktivieren:** `/wide-ultra-logical:off`, `/wul:off`

## Workflow (Pflichtreihenfolge)

### 1. Scope verstehen

- Welches Feature oder welcher Fix ist **Ende-zu-Ende** gefordert?
- Welche Schichten sind betroffen? (UI, State, Services, Typen, Tests, Integration)
- Große Features in **committbare Phasen** teilen — jede Phase muss logisch abgeschlossen sein.

### 2. Bestehende Muster lesen

Vor dem Schreiben relevante Dateien im Projekt suchen und lesen:

| Was | Vorgehen |
|-----|----------|
| Ähnliche Features | Grep/Glob nach gleichem Muster (ViewModel, Service, XAML) |
| Konventionen | Naming, DI-Registrierung, Error-Handling im gleichen Modul |
| Typen & Interfaces | Bestehende Models/Interfaces wiederverwenden |
| Tests | Vorhandene Test-Patterns für das Feature-Bereich |

**Nicht** parallele Abstraktionen erfinden, wenn das Projekt bereits ein Muster hat.

### 3. Vollständig implementieren

Siehe Abschnitte **Completion policy**, **Code quality**, **Logic & edge cases** unten.

### 4. Selbst-Verifikation (vor „erledigt“)

Checkliste abarbeiten — bei Fehlern fixen und erneut prüfen:

- [ ] Typecheck / Build (`dotnet build`, `tsc`, `npm run build` — projektüblich)
- [ ] Relevante Tests ausführen oder sinnvoll ergänzen
- [ ] Lint auf geänderte Dateien (`ReadLints` oder projektübliches Kommando)
- [ ] Keine bekannten Fehler beim Abschluss melden
- [ ] Imports/Exports vollständig; Code direkt kompilierbar
- [ ] **C#/WPF-Stack:** zusätzliche Checkliste im Anhang unten abarbeiten

### 5. Abschluss melden

- Ergebnis und Verifikation kurz benennen (Build/Lint/Test-Status).
- Bei Hard-Blocker: genau sagen, was erledigt ist und was blockiert — kein Skelett als Ersatz.
- Trade-offs dürfen ausführlicher erklärt werden; Code in Diffs trotzdem vollständig.

## Completion policy

- Implementiere angeforderte Features **vollständig**. Nicht bei der Hälfte stoppen.
- Keine Platzhalter, kein `// TODO later`, kein `// ... rest unchanged`.
- Keine ungenutzten `if (false)`-Zweige oder tote Code-Pfade.
- Scope klein pro Schritt ist OK — **jeder** gelieferte Schritt muss logisch abgeschlossen sein.
- Hard-Blocker (fehlende Credentials, unklare API): **genau** benennen, was erledigt ist und was blockiert.

## Code quality

```typescript
// ❌ BAD — partieller Stub
function saveNote(note: Note) {
  // TODO: validation
  api.post('/notes', note);
}

// ✅ GOOD — vollständiger Pfad
function saveNote(note: Note): Promise<Note> {
  if (!note.title?.trim()) {
    throw new ValidationError('title required');
  }
  return api.post<Note>('/notes', note);
}
```

- Keine fehlenden Imports/Exports.
- Discriminated unions / Enums: exhaustive `switch` mit `never` im `default` (TypeScript).
- Keine Type-Suppression (`as any`, `@ts-ignore`, `@ts-expect-error`).
- Keine leeren `catch`-Blöcke.

## Logic & edge cases

| Bereich | Behandeln |
|---------|-----------|
| Null/undefined | Explizite Guards oder sinnvolle Defaults |
| Leere Collections/Strings | Früh validieren oder leerer Zustand in UI |
| Netzwerk/Auth-Fehler | Fehlerpfade mit Nutzer-Feedback oder Logging |
| Async/Race | Cancellation oder debounce wo nötig |
| Persistenz | Load/Save-Fehler nicht verschlucken |

Bestehende Projekt-Muster lesen und wiederverwenden — keine spekulativen Abstraktionen ohne Nutzung.

## Konflikte mit anderen Modi

| Modus | Verhalten bei gleichzeitiger Aktivierung |
|-------|------------------------------------------|
| **`/lean`** | **Code-Tiefe:** Wide Ultra-Logical gewinnt (vollständige Implementierung). **Chat-Kürze:** Lean gewinnt (kein Preamble, knappe Statusmeldungen). Beides möglich: kurzer Chat, tiefer Code. |
| **`/boost`** | Wide Ultra-Logical **umfasst** Boost; bei beiden aktiv reicht Wide Ultra-Logical. |
| **Lean Mode** | Senkt Ausführlichkeit, ersetzt **nicht** Sicherheits- oder Qualitäts-Mindestanforderungen (kein Commit ohne Anfrage, keine Type-Suppression). |

## Model policy

- **`composer-2.5`** für Ausführung und Worker — **niemals** `composer-2.5-fast` als Default.
- Siehe `.cursor/rules/model-policy.mdc` für Orchestrator- und Task-Slug-Regeln.
- Wide Ultra-Logical ändert Modell-Slugs nicht — nur Implementierungstiefe.

## Orchestrator-Hinweis

Wenn der Root-Dispatcher delegiert: Wide Ultra-Logical im Task-Prompt erwähnen und **Selbst-Verifikation** vor Erfolgsmeldung verlangen. Siehe `orchestrator.mdc` → Wide Ultra-Logical.

## Anti-Patterns

| Vermeiden | Stattdessen |
|-----------|-------------|
| `// ... rest of code` in Snippets | Vollständige, kompilierbare Änderungen |
| Feature halbfertig liefern | Phase committbar abschließen oder Blocker benennen |
| Neue Abstraktion ohne Verwendung | Bestehende Patterns erweitern |
| „Build sollte passen“ ohne Prüfung | Build/Lint/Test tatsächlich ausführen |
| `composer-2.5-fast` als Default | Immer `composer-2.5` |

## Beispiele

```
/wide-ultra-logical baue Weather-Widget fertig
/wul /stack react Dashboard-Filter
/ultra-logical ohne Stubs — Settings-Dialog komplett
/boost:wide Settings-Persistenz inkl. Migration
/lean /wul fixe Login — kurzer Chat, vollständiger Fix
```

## C#/WPF — Best Practices (Anhang)

Erweitert **Selbst-Verifikation** und **Code quality** für WPF-Desktop-Projekte (.NET, MVVM, XAML). Allgemeine Regeln oben gelten weiterhin; hier stack-spezifische Muster mit ✅ GOOD / ❌ BAD.

**Quellen:** [WPF Threading](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/threading-model) · [Weak Events](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/events/weak-event-patterns) · [XAML Resources](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/systems/xaml-resources-overview) · [MVVM Toolkit](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/) · [ObservableValidator](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/observablevalidator)

### MVVM *(stabil, seit ~2009)*

- ViewModel = UI-State + Commands; **keine** `Window`/`Button`-Referenzen im ViewModel
- Code-Behind nur für View-Lifecycle (`Loaded`/`Closing`), nicht für Business-Logik
- Services über Interfaces injizieren; ViewModels als testbare POCOs
- UI-Listen: `ObservableCollection<T>`, nicht `List<T>`

```csharp
// ✅ GOOD — ViewModel ohne UI-Typen
public partial class SettingsViewModel(ISettingsService svc) : ObservableObject { }

// ❌ BAD — ViewModel kennt WPF-Controls
public class BadVm { public void Save(Window w) => w.Close(); }
```

### Data Binding *(stabil)*

- Binding-Mode bewusst wählen: `OneWay` für Anzeige, `TwoWay` nur wo nötig
- `INotifyPropertyChanged` für alle gebundenen Properties (via `[ObservableProperty]` *neu*)
- `UpdateSourceTrigger=PropertyChanged` für Live-Validierung in Formularen
- Binding-Fehler debuggen: `PresentationTraceSources.TraceLevel=High` + VS „WPF Trace → Data Binding ≥ Warning“

```xml
<!-- ✅ GOOD -->
<TextBox Text="{Binding Name, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"/>

<!-- ❌ BAD — Property existiert nicht / falscher DataContext -->
<TextBox Text="{Binding UserName}"/>
```

### Async / UI-Thread / Dispatcher *(stabil)*

- UI-Thread nur für UI-Updates; schwere Arbeit off-thread
- In Event-Handlern: `await` für I/O; `Task.Run` nur für CPU-bound
- **Nie** `.Result`/`.Wait()` auf dem UI-Thread
- `Dispatcher.InvokeAsync` nur für UI-Marshalling — **kein** `Thread.Sleep`/I/O darin

```csharp
// ✅ GOOD
private async Task LoadAsync() {
  var data = await _service.GetAsync();
  StatusText = data; // nach await zurück auf UI-Thread
}

// ❌ BAD — blockiert UI
private void Load() => StatusText = _service.GetAsync().Result;
```

### Dependency Injection *(etabliert; Generic Host + MS.Extensions.DI ~2018+; Toolkit-Ioc *neu*)*

- Constructor Injection in ViewModels/Services; **kein** Service Locator in Business-Logik
- Registrierung zentral (`ServiceCollectionExtensions` / `App.xaml.cs` / Generic Host)
- ViewModels `Transient`, Services nach Lebensdauer (`Singleton`/`Scoped`)
- Designer: `DesignerProperties.GetIsInDesignMode` vor `Ioc.GetRequiredService`

```csharp
// ✅ GOOD
services.AddTransient<MainViewModel>();
services.AddSingleton<ISettingsService, SettingsService>();

// ❌ BAD — versteckte Abhängigkeit
var vm = new MainViewModel { Svc = Ioc.Default.GetService<IService>() };
```

### XAML / Commands *(stabil)*

- User-Aktionen über `ICommand`/`RelayCommand`, nicht `Click`-Handler mit Logik
- `CanExecute` + `[NotifyCanExecuteChangedFor]` für Button-Enable-State
- DataTemplates/Styles in Resource-Dictionaries, nicht inline in jeder View
- Code-Behind für rein visuelle Dinge OK; Datenlogik bleibt im ViewModel

```xml
<!-- ✅ GOOD -->
<Button Command="{Binding SaveCommand}" Content="Speichern"/>

<!-- ❌ BAD -->
<Button Click="Save_Click"/>
```

### Resource Dictionaries / Themes *(stabil)*

- Hierarchie: **Colors → Brushes → Styles → Templates**
- Themes via `MergedDictionaries` in `App.xaml`; Skin zuletzt mergen
- `StaticResource` für feste Theme-Werte; `DynamicResource` für Runtime-Theme-Switch
- Wenige, gut strukturierte Dictionaries — viele Merges kosten Lookup-Performance

```xml
<!-- ✅ GOOD — Theme-Switch -->
<SolidColorBrush x:Key="AccentBrush" Color="{DynamicResource AccentColor}"/>

<!-- ❌ BAD — Hardcoded in jedem Control -->
<Button Background="#FF0078D4"/>
```

### Memory Leaks *(stabil)*

- Lebensdauer prüfen: Subscriber kürzer als Publisher → `-=` in `Unloaded`/`Dispose`
- View → ViewModel-Events: immer abmelden
- `WeakEventManager<TSource,TEventArgs>` wenn Abmeldung unklar *(WPF 4.5+)*
- Nicht pauschal jeden Handler detachen — nur bei Lifetime-Mismatch

```csharp
// ✅ GOOD — View entlädt Handler
void OnUnloaded(object s, RoutedEventArgs e) => _vm.Changed -= OnChanged;

// ❌ BAD — ViewModel lebt, View bleibt referenziert
_vm.Changed += (s,e) => Title = e.Message; // nie -=
```

### Validation *(INotifyDataErrorInfo stabil seit .NET 4.5; ObservableValidator *neu*)*

- Neu: `ObservableValidator` + DataAnnotations statt manuellem `IDataErrorInfo`
- Cross-Property: `ValidateProperty()` bei abhängigen Feldern
- UI-Feedback über `Validation.HasError` / `Validation.Errors`
- Async-Validierung: `INotifyDataErrorInfo` + `ErrorsChanged` feuern

```csharp
// ✅ GOOD — Toolkit
public partial class FormVm : ObservableValidator {
  [Required, MinLength(2)]
  [ObservableProperty] private string _name;
}

// ❌ BAD — Setter wirft Exception → bricht Binding
set { if (invalid) throw new Exception(); }
```

### Error Handling *(stabil)*

- Zentral: `Application.DispatcherUnhandledException` — loggen, `Handled=true` nur wenn sicher fortsetzbar
- Async: `try/catch` in `Task`-Methoden; `async void` vermeiden (außer Event-Handler)
- `TaskScheduler.UnobservedTaskException` nur als letztes Sicherheitsnetz + `SetObserved()`
- Background-Thread-Fehler explizit auf UI-Thread marshallen, wenn User informiert werden soll

```csharp
// ✅ GOOD — lokales Handling in Command
[RelayCommand]
private async Task SaveAsync() {
  try { await _svc.SaveAsync(); }
  catch (Exception ex) { ErrorMessage = ex.Message; }
}

// ❌ BAD — Fire-and-forget ohne Beobachtung
_ = Task.Run(() => RiskyWork());
```

### Testing *(stabil)*

- ViewModels black-box testen: öffentliche Properties + Commands
- Dependencies mocken (Moq/NSubstitute); kein WPF-Referenz in Test-Projekt nötig
- `Application.Current.Dispatcher` hinter Interface wrappen für testbare UI-Marshalling
- Arrange-Act-Assert; ViewModel-Factory-Helper für Konstruktor-Änderungen

```csharp
// ✅ GOOD
[Fact]
public async Task Save_CallsService() {
  var svc = Substitute.For<ISettingsService>();
  var vm = new SettingsViewModel(svc);
  await vm.SaveCommand.ExecuteAsync(null);
  await svc.Received(1).SaveAsync(Arg.Any<AppSettings>());
}
```

### Build & WPF-Verifikation *(stabil)*

```bash
dotnet build -c Debug          # 0 Errors, 0 Warnings (Ziel)
dotnet test                    # ViewModel-Unit-Tests
```

**Runtime-Checkliste (WPF):**

- [ ] Keine Binding-Fehler im Output (Trace Level ≥ Warning)
- [ ] UI reagiert während async Ops (kein Freeze)
- [ ] Theme-Switch aktualisiert `DynamicResource`-Keys
- [ ] Fenster schließen/öffnen ohne wachsenden Speicher (Event-Leaks)
- [ ] Commands korrekt disabled (`CanExecute`)
- [ ] UI-Updates nach Background-Work über `Dispatcher`/`await`-Resume

### Stable vs. Neu — Kurzübersicht

| Pattern | Status |
|---------|--------|
| MVVM, ICommand, INPC, Dispatcher, WeakEventManager | **Stabil / etabliert** |
| INotifyDataErrorInfo, Generic Host | **Stabil (.NET 4.5+ / .NET Core 3+)** |
| CommunityToolkit.Mvvm (`[ObservableProperty]`, `[RelayCommand]`, `ObservableValidator`) | **Neu / empfohlen** (Microsoft-maintained) |
| `Microsoft.Extensions.DependencyInjection` in WPF | **Modern standard**, ersetzt handgerollte Singletons |

## Referenzen

- Regel: `.cursor/rules/wide-ultra-logical.mdc`
- Parameter & Konflikttabelle: `.cursor/rules/agent-context-modes.mdc` → Wide Ultra-Logical
- Modelle: `.cursor/rules/model-policy.mdc`
- Orchestrator: `.cursor/rules/orchestrator.mdc` → Wide Ultra-Logical
