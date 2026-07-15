---
name: implementing-figma-designs-avalonia
description: >-
  HorosCloud Avalonia Pass 1 — Mock-Daten, Komponenten-Inventar, Viewer-Pattern,
  bestehende AXAML wiederverwenden. Nach avalonia-figma-to-code und avalonia-design-system.
user-invocable: true
metadata:
  company: HorosCode
  product: HorosCloud
  version: "1.0.0"
---

# Implementing Figma Designs — Avalonia (HorosCloud Pass 1)

Pass-1-Implementierung: **pixelgenaues Layout** mit **Mock-Daten**, ohne i18n/API. Für HorosCloud Desktop (Avalonia).

**Voraussetzung:** `@avalonia-figma-to-code` (Frame analysiert) + `@avalonia-design-system` (Tokens bereit)

**Agent:** `avalonia-ui`

## Pass 1 Scope

| Enthalten | Nicht enthalten |
|-----------|-----------------|
| AXAML Layout 1:1 | API-Integration |
| ViewModel Mock-Daten | i18n / Lokalisierung |
| Styles / Tokens | Echte Auth / Persistence |
| `*Preview.axaml` | Ganze App auf einmal |
| Screenshot-Review ≥ 98 | Performance-Tuning über Mockup hinaus |

## Komponenten-Inventar

Vor jedem neuen Primitive:

```text
Glob: **/*.axaml
Grep: class Horos, UserControl, Components/
```

### Template-Inventar (bei leerem Projekt starten)

| Komponente | Datei | Zweck |
|------------|-------|-------|
| HorosButton | `Views/Components/HorosButton.axaml` | primary / secondary |
| HorosCard | `Views/Components/HorosCard.axaml` | Panel-Container |
| HorosTextBox | `Views/Components/HorosTextBox.axaml` | Form Input |
| HorosNavItem | `Views/Components/HorosNavItem.axaml` | Sidebar-Eintrag |
| HorosHeaderBar | `Views/Components/HorosHeaderBar.axaml` | App-Header |

Inventar nach jedem Task aktualisieren.

### Mapping-Entscheidung

```
Mockup-Element ≈ bestehende Komponente? → wiederverwenden + Props
Neues Pattern, 2+ Wiederholungen im Mockup? → neues UserControl
Einmaliges Layout? → inline in Bereichs-AXAML (kein Over-Abstract)
```

## Viewer-Pattern (Pflicht)

Jeder Bereich bekommt isolierten Preview:

### Dateien

```text
Views/Previews/HeaderPreview.axaml
Views/Previews/HeaderPreview.axaml.cs
ViewModels/Previews/HeaderPreviewViewModel.cs
```

### HeaderPreview.axaml (Beispiel)

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:components="using:HorosCloud.Desktop.Views.Components"
        xmlns:vm="using:HorosCloud.Desktop.ViewModels.Previews"
        x:Class="HorosCloud.Desktop.Views.Previews.HeaderPreview"
        x:DataType="vm:HeaderPreviewViewModel"
        Width="1440" Height="56"
        Background="{DynamicResource BrushBackgroundBase}">
  <components:HorosHeaderBar DataContext="{Binding}" />
</Window>
```

### Design.PreviewWith (in Component)

```xml
<UserControl ...
             xmlns:previews="using:HorosCloud.Desktop.Views.Previews">
  <Design.PreviewWith>
    <previews:HeaderPreview />
  </Design.PreviewWith>
  ...
</UserControl>
```

### Design-Time ViewModel

```csharp
public partial class HeaderPreviewViewModel : ObservableObject
{
    public static HeaderPreviewViewModel DesignInstance { get; } = new()
    {
        AppTitle = "HorosCloud",
        UserName = "Alex M.",
        NotificationCount = 3
    };

    [ObservableProperty] private string _appTitle = "";
    [ObservableProperty] private string _userName = "";
    [ObservableProperty] private int _notificationCount;
}
```

## Mock-Daten-Konventionen

- Realistische deutsche/englische Platzhalter passend zu HorosCloud
- Listen: 3–5 Items für Layout-Test
- Loading/Empty/Error States nur wenn im Mockup sichtbar
- Keine Lorem-Blöcke wenn Mockup konkreten Text zeigt — **Text aus Mockup übernehmen**

## Implementierungs-Reihenfolge (pro Bereich)

1. ViewModel mit Mock-Properties
2. AXAML — äußerer Container (Höhe/Breite aus Mockup)
3. Kinder — Grid/Stack laut Mockup-Hierarchie
4. Styles — DynamicResource Tokens
5. States — pointerover/disabled wenn im Mockup
6. Preview-Window
7. `dotnet build`
8. Screenshot + Review

## AXAML-Struktur-Tipps

- Mockup-Frame = Root `Border` oder `Grid` mit festen Höhen
- Auto-Layout horizontal → `StackPanel Orientation="Horizontal"` + `Spacing`
- Auto-Layout vertical → `StackPanel` oder `Grid` Rows
- Gleich breite Spalten → `Grid` mit `*` Columns
- Icon + Text → horizontaler Stack, `VerticalAlignment="Center"`

## Nach Pass 1

- Review ≥ 98 → Bereich abgeschlossen
- Nächster Bereich oder Shell-Merge
- Pass 2 (später): Services, echte Daten — **separater Task**

## Verboten

- `@tanstack/react-query`, JSX, Tailwind-Klassen
- Gesamtes MainWindow in einem Schritt
- Preview überspringen
- Hardcodierte Farben

## Verweise

- Orchestrator: `@avalonia-figma-to-code`
- Tokens: `@avalonia-design-system`
- Rules: `04-component-discovery`, `06-ui-preview`, `12-human-approval`
- Checkliste: `docs/review-checklist.md`
