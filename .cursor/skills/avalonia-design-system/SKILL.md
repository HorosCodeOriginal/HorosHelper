---
name: avalonia-design-system
description: >-
  HorosCloud Design System für Avalonia Desktop — ResourceDictionary Tokens,
  Horos Slate+Amber Dark Theme, 8px-Grid, 5 Interaktions-States. Verwenden bei
  jeder AXAML-UI-Änderung und vor Mockup-Implementierung.
user-invocable: true
metadata:
  company: HorosCode
  product: HorosCloud
  version: "1.0.0"
---

# Avalonia Design System — HorosCloud Desktop

Design-Tokens und Style-Konventionen für **HorosCode** / **HorosCloud** Desktop (Avalonia). Autoritative Token-Liste: **`docs/design-memory.md`**.

Paralleles Web-System: `@horos-design-system` (React/Tailwind) — semantisch aligned, technisch getrennt.

## Wann nutzen

| Situation | Ja/Nein |
|-----------|---------|
| Neue AXAML-View oder Style | **Ja** |
| Mockup→Code Pass 1 | **Ja** — vor Layout |
| Token aus Figma extrahieren | **Ja** — in design-memory eintragen |
| Web React UI | Nein → `@horos-design-system` |

## Theme-Übersicht

| Aspekt | Wert |
|--------|------|
| Modus | Dark-first |
| Basis | `#0F172A` (slate-900) |
| Oberflächen | slate-800 … slate-600 |
| Akzent | amber-500 / amber-400 |
| Grid | 8px |
| Font | Inter (Fallback: Segoe UI) |
| States | default, pointerover, pressed, disabled, focus |

Vollständige Tabelle: `docs/design-memory.md`

## Datei-Struktur

```text
Styles/
  DesignTokens.axaml      ← Colors, Thickness, CornerRadius, Font resources
  ControlThemes.axaml     ← Button, TextBox, CheckBox, …
  Components.axaml        ← Card, NavItem, HeaderBar, …
```

In `App.axaml` mergen:

```xml
<Application.Styles>
  <FluentTheme />
  <StyleInclude Source="avares://HorosCloud.Desktop/Styles/DesignTokens.axaml" />
  <StyleInclude Source="avares://HorosCloud.Desktop/Styles/ControlThemes.axaml" />
  <StyleInclude Source="avares://HorosCloud.Desktop/Styles/Components.axaml" />
</Application.Styles>
```

## Token-Beispiele (AXAML)

```xml
<!-- DesignTokens.axaml -->
<Color x:Key="ColorBackgroundBase">#0F172A</Color>
<Color x:Key="ColorAccentPrimary">#F59E0B</Color>
<SolidColorBrush x:Key="BrushBackgroundBase" Color="{StaticResource ColorBackgroundBase}" />
<SolidColorBrush x:Key="BrushAccentPrimary" Color="{StaticResource ColorAccentPrimary}" />
<Thickness x:Key="Spacing4">16</Thickness>
<CornerRadius x:Key="RadiusMd">8</CornerRadius>
```

## Verwendung in Views

```xml
<Border Background="{DynamicResource BrushSurfaceDefault}"
        CornerRadius="{DynamicResource RadiusMd}"
        Padding="{DynamicResource Spacing4}">
  <TextBlock Text="{Binding Title}"
             Foreground="{DynamicResource BrushTextPrimary}"
             FontSize="14" />
</Border>
```

**Regel:** Keine `#RRGGBB` inline in Views — Rules `02-pixel-perfect`, `10-anti-patterns`.

## Primary Button (Beispiel)

```xml
<!-- ControlThemes.axaml -->
<Style Selector="Button.primary">
  <Setter Property="Background" Value="{DynamicResource BrushAccentPrimary}" />
  <Setter Property="Foreground" Value="{DynamicResource BrushBackgroundBase}" />
  <Setter Property="CornerRadius" Value="{DynamicResource RadiusMd}" />
  <Setter Property="Padding" Value="12,8" />
  <Setter Property="MinHeight" Value="40" />
</Style>
<Style Selector="Button.primary:pointerover /template/ ContentPresenter">
  <Setter Property="Background" Value="{DynamicResource BrushAccentPrimaryHover}" />
</Style>
```

## 5 Interaktions-States

Jede interaktive Komponente:

1. Default
2. `:pointerover`
3. `:pressed`
4. `:disabled`
5. `:focus` — Focus-Border `ColorBorderFocus` (#F59E0B)

## 8px-Grid

| Token | px |
|-------|-----|
| Spacing2 | 8 |
| Spacing4 | 16 |
| Spacing6 | 24 |

Mockup-exakte Nicht-Grid-Werte als benannte Custom-Tokens in design-memory dokumentieren.

## Neuer Token aus Mockup

1. Wert aus Figma/Screenshot messen
2. Eintrag in `docs/design-memory.md`
3. Resource in `DesignTokens.axaml`
4. In AXAML via `{DynamicResource ...}`

## Sync mit Web

| Semantik | Web (Tailwind) | Desktop (Token) |
|----------|----------------|-----------------|
| App BG | `bg-slate-900` | `BrushBackgroundBase` |
| Card | `bg-slate-800` | `BrushSurfaceDefault` |
| CTA | `bg-amber-500` | `BrushAccentPrimary` |

Bei Design-Updates beide Stacks prüfen.

## Verweise

- design-memory: `docs/design-memory.md`
- Rule: `02-pixel-perfect`, `04-component-discovery`
- Figma-Workflow: `@avalonia-figma-to-code`
