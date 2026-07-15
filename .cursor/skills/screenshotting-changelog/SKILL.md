---
name: screenshotting-changelog
description: HorosCode Before/After Screenshots — visuelle Änderungshistorie pro UI-Bereich dokumentieren.
---

# Purpose

Jede visuelle Korrektur mit **Before/After-Screenshots** nachweisen — Audit-Trail für Reviews und Human Approval.

**Firma:** HorosCode · **Produkt:** HorosCloud

# Workflow

```
Before capture → Change → After capture → Changelog entry
```

## 1. Before Capture

Vor jeder Korrektur-Runde:

```
reviews/before-{region}-v{n}.png    ← IST vor Fix
reviews/mockup-{region}.png         ← SOLL (unverändert)
```

## 2. Change

Diff-Liste aus `@visual-regression` abarbeiten. Ein Fix-Typ pro Commit wenn möglich (Spacing, dann Colors …).

## 3. After Capture

```
reviews/after-{region}-v{n}.png   ← IST nach Fix
reviews/review-{region}-v{n+1}.png ← offizieller Review-Shot
```

## 4. Changelog Entry

In Task-Kommentar oder `docs/changelog.md`:

```markdown
### Header — Korrektur v1→v2 (94→98)
- **Before:** reviews/before-header-v1.png
- **After:** reviews/after-header-v1.png
- **Fixes:** padding-left 16→24px (Spacing6), nav font 14→13px
- **Score:** 94 → 98
```

# Rules

- Nie After ohne Before bei Korrektur-Runden
- Dateinamen versioniert (`v1`, `v2`, …)
- Mockup-Referenz nie überschreiben
- Screenshots bei Human Approval mitschicken

# Output

Versionierte PNG-Paare + kurzer Changelog-Eintrag pro Korrektur-Runde.

# Verweise

- `@visual-regression`, `@avalonia-preview`
- Docs: `docs/visual-regression.md`, `docs/changelog.md`
