# Region-Manifest — {{REGION_LABEL_DE}}

> **HorosCode** · Mini-Manifest für Region `{{REGION_SLUG}}` in Asset **`{{ASSET_SLUG}}`**
> Parent: `{{ASSET_MANIFEST_PATH}}` · regions.json: `regions/regions.json`

---

## Identität

| Feld | Wert |
|------|------|
| **Region-ID** | `{{REGION_ID}}` |
| **Slug** | `{{REGION_SLUG}}` |
| **Label (DE)** | `{{REGION_LABEL_DE}}` |
| **Label (EN)** | `{{REGION_LABEL_EN}}` |
| **Asset-Slug** | `{{ASSET_SLUG}}` |
| **Export-Pfad** | `{{EXPORT_PATH}}` |
| **Erstellt (UTC)** | `{{CREATED_AT_ISO}}` |

---

## Bounds (relativ zu full)

| Feld | Wert |
|------|------|
| **x** | `{{BOUNDS_X}}` px |
| **y** | `{{BOUNDS_Y}}` px |
| **width** | `{{BOUNDS_WIDTH}}` px |
| **height** | `{{BOUNDS_HEIGHT}}` px |
| **Parent full** | `{{PARENT_FULL_PATH}}` |
| **Full-Größe** | `{{FULL_WIDTH}}` × `{{FULL_HEIGHT}}` px |

### Relative Position (für CSS)

| Feld | Wert |
|------|------|
| **left %** | `{{LEFT_PCT}}%` |
| **top %** | `{{TOP_PCT}}%` |
| **width %** | `{{WIDTH_PCT}}%` |
| **height %** | `{{HEIGHT_PCT}}%` |

---

## Farben

| Rang | Hex | Verwendung |
|------|-----|------------|
| 1 | `#{{DOMINANT_1}}` | `{{USAGE_1}}` |
| 2 | `#{{DOMINANT_2}}` | `{{USAGE_2}}` |

| Feld | Wert |
|------|------|
| **Transparent** | `{{TRANSPARENT}}` |
| **zIndex / Layer** | `{{Z_INDEX}}` |
| **usage** | `{{USAGE}}` (`icon` \| `bg` \| `overlay` \| `text-safe` \| `decorative`) |

---

## Zweck / Verwendung

`{{PURPOSE_DE}}`

### Einbindung 1:1 (Beispiel)

```tsx
<img
  src="/{{PUBLIC_URL_PATH}}"
  alt="{{ALT_OR_EMPTY}}"
  aria-hidden={{ARIA_HIDDEN}}
  className="{{TAILWIND_CLASSES}}"
  width={{{BOUNDS_WIDTH}}}
  height={{{BOUNDS_HEIGHT}}}
/>
```

### Layout-Hinweise

| Feld | Wert |
|------|------|
| **object-fit** | `{{OBJECT_FIT}}` |
| **Blend-Modus** | `{{BLEND_MODE_OR_NONE}}` |
| **Notizen** | `{{NOTES}}` |

---

*© HorosCode · Region-Manifest für HorosCloud 1:1-Layer-Parity.*
