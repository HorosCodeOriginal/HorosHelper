---
name: preview-first-development
description: HorosCode Preview-First — Komponente→Preview→Verify→Integrate, keine Integration ohne Preview.
---

# Purpose

Jede UI-Komponente und jeder Bereich wird **isoliert** gebaut, gerendert und verifiziert, bevor Shell-Integration oder der nächste Schritt erlaubt ist. Ergänzt `@avalonia-preview` mit striktem Create→Verify→Integrate-Zyklus.

**Firma:** HorosCode · **Produkt:** HorosCloud · **Rule:** `06-ui-preview`

# Workflow

```
Create Component → Create Preview → Verify Visual → Integrate (optional)
```

| Schritt | Aktion | Gate |
|---------|--------|------|
| **Create** | `HeaderView.axaml` + `HeaderViewModel.cs` | Tokens only |
| **Preview** | `HeaderPreview.axaml` bei Mockup-Dimensionen | Muss starten |
| **Verify** | Visuell gegen Mockup-Ausschnitt | Keine offensichtlichen Abweichungen |
| **Integrate** | In Shell/MainWindow einbinden | Nur nach Preview-OK + Review |

## Preview-Dateistruktur

```
Views/
  Header/
    HeaderView.axaml
    HeaderViewModel.cs
  Previews/
    HeaderPreview.axaml
    HeaderPreview.axaml.cs
ViewModels/
  Previews/
    HeaderPreviewViewModel.cs
```

## Preview-Fenster (Avalonia)

```xml
<Window Width="1440" Height="56"
        Title="Preview: Header"
        x:DataType="vm:HeaderPreviewViewModel">
  <components:HeaderView DataContext="{Binding}" />
</Window>
```

Mock-Daten im PreviewViewModel — statisch, exakt wie Mockup-Labels.

## Verify-Checkliste

- [ ] Preview startet ohne MainWindow/Shell
- [ ] Fenstergröße = Mockup-Frame des Bereichs
- [ ] Dark Theme (HorosCloud Default)
- [ ] Alle sichtbaren Elemente gerendert (keine leeren Platzhalter)
- [ ] `dotnet build` grün
- [ ] Keine Debug-Borders oder Dev-Overlays

# Output

- Laufendes `*Preview.axaml` pro Komponente/Region
- Screenshot-Bereitschaft: `reviews/review-{region}-v1.png`
- Integrations-Notiz erst nach Verify

# Rule

> **Integration ohne Preview ist verboten.** Niemals direkt in `MainWindow.axaml` einbauen, bevor isoliertes Preview läuft und visuell geprüft wurde.

# Verweise

- Ergänzt: `@avalonia-preview`, `@screenshot-reviewer`
- Rule: `06-ui-preview`, `12-human-approval`
- Works with: `@pixel-perfect-ui`, `@visual-regression`
- Legacy: `@implementing-figma-designs-avalonia`
