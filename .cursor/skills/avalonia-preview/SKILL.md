---
name: avalonia-preview
description: HorosCode isolierte Avalonia Preview-Fenster — *Preview.axaml, Mock-Daten, Design.PreviewWith.
---

# Requirement

Jede UI-Komponente und jeder Bereich **muss** isoliert previewbar sein, bevor Shell-Integration oder Review.

**Firma:** HorosCode · **Rule:** `06-ui-preview`

# Create

## 1. Preview Window (`*Preview.axaml`)

Pro Bereich eine Preview-Datei neben der Produktions-View:

```
Views/
  Header/
    HeaderView.axaml
    HeaderViewModel.cs
    HeaderPreview.axaml      ← isoliert
```

```xml
<!-- HeaderPreview.axaml -->
<Window xmlns="https://github.com/avaloniaui"
        Width="1440" Height="56"
        Background="{StaticResource ColorSurfacePrimary}">
  <local:HeaderView DataContext="{Binding HeaderViewModel, Mode=OneTime}" />
</Window>
```

## 2. Mock Data

Preview-ViewModel mit statischen Werten aus dem Mockup — keine API, kein DI-Graph:

```csharp
public class HeaderPreviewViewModel : HeaderViewModel
{
    public HeaderPreviewViewModel() : base(null!)
    {
        Title = "HorosCloud";           // exakt wie Mockup
        NavItems = new[] { "Dashboard", "Settings" };
    }
}
```

## 3. Test Navigation

- `Design.PreviewWith` in IDE für schnelle Iteration
- Optional: separates `*.Preview.csproj` für CI-Screenshots
- Viewport = exakte Mockup-Dimensionen (z. B. `1440×56`)

# Validation

- [ ] Preview startet ohne Shell/MainWindow
- [ ] Fenstergröße = Mockup-Frame
- [ ] Dark Theme (HorosCloud Default)
- [ ] Keine Debug-Borders, keine Dev-Overlays
- [ ] Mock-Inhalt sichtbar (Labels, Icons, States)
- [ ] `dotnet build` grün

Screenshot-Pfad: `reviews/review-{region}-v{n}.png`

# Verweise

- Rule: `06-ui-preview`
- Workflow: `@incremental-ui`
- Review: `@visual-regression`
- Legacy: `@implementing-figma-designs-avalonia` (Viewer-Pattern)
