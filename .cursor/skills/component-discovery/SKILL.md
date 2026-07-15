---
name: component-discovery
description: HorosCode Komponenten-Suche Search→Reuse→Extend→Create — kein Duplikat vor Katalog-Check.
---

# Purpose

Vor jeder neuen AXAML-Komponente das bestehende Inventar prüfen. Verhindert Duplikate und erzwingt konsistente Wiederverwendung im HorosCode Desktop-Stack.

**Firma:** HorosCode · **Produkt:** HorosCloud · **Rule:** `04-component-discovery`

# Workflow — Search → Reuse → Extend → Create

| Stufe | Aktion | Wann stoppen |
|-------|--------|--------------|
| **Search** | `docs/component-catalog.md` + Codebase (`*.axaml`, `Views/`) durchsuchen | — |
| **Reuse** | Bestehende Komponente 1:1 einbinden | Passt visuell + funktional |
| **Extend** | Variante via Properties/Styles erweitern | Basis passt, kleine Abweichung |
| **Create** | Neue Komponente nur wenn 1–3 scheitern | Dokumentieren im Katalog |

## Such-Checkliste

1. `docs/component-catalog.md` — Name, Pfad, Props, States
2. Grep: `HorosButton`, `HorosCard`, `HorosInput` o. ä. Präfixe
3. `Views/Components/` und `Views/Shared/` prüfen
4. ResourceDictionary — existierende Styles (`ButtonPrimary`, `CardSurface`)
5. Mockup-Anforderung mit Katalog-Eintrag abgleichen (Dimensionen, States)

## Extend-Beispiel (Avalonia)

```xml
<!-- Reuse: HorosButton mit Secondary-Style statt neue Klasse -->
<components:HorosButton Style="{StaticResource ButtonSecondary}"
                        Content="Abbrechen" />
```

## Create-Pflichten

Neue Komponente nur mit:
- Eintrag in `docs/component-catalog.md` (Name, Pfad, Props, Mockup-Ref)
- Design-Tokens only (`docs/design-memory.md`)
- Isoliertes `*Preview.axaml` (`@preview-first-development`)
- MVVM-Trennung (`@avalonia-mvvm`)

## Duplikat-Warnsignale

- Zweites `Button`-Control mit ähnlichem Styling
- Copy-Paste aus anderer Region ohne Katalog-Check
- Hardcoded Werte statt bestehender Tokens

# Output

- Entscheidungsnotiz: Reuse / Extend / Create + Begründung
- Bei Create: Katalog-Eintrag + Dateipfad
- Keine doppelten `Button`, `Card`, `HeaderBar` Varianten

# Rule

> **Niemals duplizieren.** Zwei ähnliche Komponenten im selben Projekt = Architektur-Verstoß. Im Zweifel Extend vor Create.

# Verweise

- Docs: `docs/component-catalog.md`
- Works with: `@pixel-perfect-ui`, `@architecture-guardian`
- Rule: `04-component-discovery`, `10-anti-patterns`
- Patterns: `docs/avalonia-patterns.md`
