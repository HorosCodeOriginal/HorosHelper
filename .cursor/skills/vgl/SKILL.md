---
name: vgl
description: Maximale Kontrolle über KI-Bildgenerierung — strukturiertes VGL-(Visual Generation Language)-JSON schreiben, das jedes visuelle Attribut explizit steuert. Exakte Objektplatzierung, Lichtrichtung, Kamerawinkel, Brennweite, Komposition, Farbschema und künstlerischen Stil als deterministisches JSON statt mehrdeutiger natürlicher Sprache definieren. Verwenden bei reproduzierbarer Bildgenerierung, präziser Szenen-Komposition oder wenn du eine natürlichsprachige Bildanfrage in ein strukturiertes JSON-Schema für Bria-FIBO-Modelle umwandeln willst. Triggert bei strukturierten Prompts, steuerbarer Generierung, VGL-JSON, deterministischen Bildbeschreibungen oder Bria/FIBO-structured_prompt-Format.
license: MIT
metadata:
  author: Bria AI
  version: "1.3.4"
---

# Bria VGL — Volle Kontrolle über Bildgenerierung

Definiere jedes visuelle Attribut als strukturiertes JSON statt zu hoffen, dass natürliche Sprache es trifft. VGL (Visual Generation Language) gibt dir explizite, deterministische Kontrolle über Objekte, Beleuchtung, Kameraeinstellungen, Komposition und Stil für Brias FIBO-Modelle.

> **Verwandter Skill**: Nutze **[bria-ai](../bria-ai/SKILL.md)**, um diese VGL-Prompts über die Bria-API auszuführen. VGL definiert das strukturierte Steuerungsformat; bria-ai übernimmt Generierung, Bearbeitung und Hintergrundentfernung.

## Kernkonzept

VGL ersetzt mehrdeutige natürlichsprachige Prompts durch deterministisches JSON, das jedes visuelle Attribut explizit deklariert: Objekte, Beleuchtung, Kameraeinstellungen, Komposition und Stil. Das sorgt für reproduzierbare, steuerbare Bildgenerierung.

## Betriebsmodi

| Modus | Eingabe | Ausgabe | Anwendungsfall |
|------|-------|--------|----------|
| **Generate** | Text-Prompt | VGL JSON | Neues Bild aus Beschreibung |
| **Edit** | Bild + Anweisung | VGL JSON | Referenzbild bearbeiten |
| **Edit_with_Mask** | Maskiertes Bild + Anweisung | VGL JSON | Graue Maskenregionen füllen |
| **Caption** | Nur Bild | VGL JSON | Bestehendes Bild beschreiben |
| **Refine** | Bestehendes JSON + Edit | Aktualisiertes VGL JSON | Bestehenden Prompt ändern |

## JSON-Schema

Gib ein einzelnes gültiges JSON-Objekt mit diesen Pflichtschlüsseln aus:

### 1. `short_description` (String)
Knappe Zusammenfassung des Bildinhalts, max. 200 Wörter. Wichtige Subjekte, Aktionen, Setting und Stimmung einbeziehen.

### 2. `objects` (Array, max. 5 Items)
Jedes Objekt erfordert:

```json
{
  "description": "Detailed description, max 100 words",
  "location": "center | top-left | bottom-right foreground | etc.",
  "relative_size": "small | medium | large within frame",
  "shape_and_color": "Basic shape and dominant color",
  "texture": "smooth | rough | metallic | furry | fabric | etc.",
  "appearance_details": "Notable visual details",
  "relationship": "Relationship to other objects",
  "orientation": "upright | tilted 45 degrees | facing left | horizontal | etc."
}
```

**Menschliche Subjekte** ergänzen:
```json
{
  "pose": "Body position description",
  "expression": "winking | joyful | serious | surprised | calm",
  "clothing": "Attire description",
  "action": "What the person is doing",
  "gender": "Gender description",
  "skin_tone_and_texture": "Skin appearance"
}
```

**Objekt-Cluster** ergänzen:
```json
{
  "number_of_objects": 3
}
```

**Größen-Hinweis**: Ist eine Person das Hauptsubjekt, nutze `"medium-to-large"` oder `"large within frame"`.

### 3. `background_setting` (String)
Gesamtumgebung, Setting und Hintergrundelemente, die nicht in `objects` stehen.

### 4. `lighting` (Object)
```json
{
  "conditions": "bright daylight | dim indoor | studio lighting | golden hour | blue hour | overcast",
  "direction": "front-lit | backlit | side-lit from left | top-down",
  "shadows": "long, soft shadows | sharp, defined shadows | minimal shadows"
}
```

### 5. `aesthetics` (Object)
```json
{
  "composition": "rule of thirds | symmetrical | centered | leading lines | medium shot | close-up",
  "color_scheme": "monochromatic blue | warm complementary | high contrast | pastel",
  "mood_atmosphere": "serene | energetic | mysterious | joyful | dramatic | peaceful"
}
```
Bei Menschen als Hauptsubjekt Shot-Typ in composition angeben: `"medium shot"`, `"close-up"`, `"portrait composition"`.

### 6. `photographic_characteristics` (Object)
```json
{
  "depth_of_field": "shallow | deep | bokeh background",
  "focus": "sharp focus on subject | soft focus | motion blur",
  "camera_angle": "eye-level | low angle | high angle | dutch angle | bird's-eye",
  "lens_focal_length": "wide-angle | 50mm standard | 85mm portrait | telephoto | macro"
}
```
**Bei Menschen**: Bevorzuge `"standard lens (35mm-50mm)"` oder `"portrait lens (50mm-85mm)"`. Kein Weitwinkel, außer explizit gewünscht.

### 7. `style_medium` (String)
`"photograph"` | `"oil painting"` | `"watercolor"` | `"3D render"` | `"digital illustration"` | `"pencil sketch"`

Standard: `"photograph"`, sofern nicht anders gewünscht.

### 8. `artistic_style` (String)
Wenn kein Foto: Eigenschaften in max. 3 Wörtern: `"impressionistic, vibrant, textured"`

Bei Fotos: `"realistic"` oder ähnlich.

### 9. `context` (String)
Bildtyp/Zweck beschreiben:
- `"High-fashion editorial photograph for magazine spread"`
- `"Concept art for fantasy video game"`
- `"Commercial product photography for e-commerce"`

### 10. `text_render` (Array)
**Standard: leeres Array `[]`**

Nur befüllen, wenn der User exakten Text liefert:
```json
{
  "text": "Exact text from user (never placeholder)",
  "location": "center | top-left | bottom",
  "size": "small | medium | large",
  "color": "white | red | blue",
  "font": "serif typeface | sans-serif | handwritten | bold impact",
  "appearance_details": "Metallic finish | 3D effect | etc."
}
```
Ausnahme: Universeller Text, der zu Objekten gehört (z. B. „STOP“ auf Stoppschild).

### 11. `edit_instruction` (String)
Einzelner Imperativ-Befehl, der Edit/Generierung beschreibt.

## Edit-Instruction-Formate

### Standard-Edits (ohne Maske)
Mit Aktionsverb beginnen, Änderungen beschreiben, nie „original image“ referenzieren:

| Kategorie | Umformulierte Anweisung |
|----------|----------------------|
| Stiländerung | `Turn the image into the cartoon style.` |
| Objektattribut | `Change the dog's color to black and white.` |
| Element hinzufügen | `Add a wide-brimmed felt hat to the subject.` |
| Objekt entfernen | `Remove the book from the subject's hands.` |
| Objekt ersetzen | `Change the rose to a bright yellow sunflower.` |
| Beleuchtung | `Change the lighting from dark and moody to bright and vibrant.` |
| Komposition | `Change the perspective to a wider shot.` |
| Textänderung | `Change the text "Happy Anniversary" to "Hello".` |
| Qualität | `Refine the image to obtain increased clarity and sharpness.` |

### Maskierte Bereichs-Edits
„masked regions“ oder „masked area“ als Ziel referenzieren:

| Intent | Umformulierte Anweisung |
|--------|----------------------|
| Objektgenerierung | `Generate a white rose with a blue center in the masked region.` |
| Erweiterung | `Extend the image into the masked region to create a scene featuring...` |
| Hintergrund füllen | `Create the following background in the masked region: A vast ocean extending to horizon.` |
| Atmosphäre füllen | `Fill the background masked area with a clear, bright blue sky with wispy clouds.` |
| Subjekt wiederherstellen | `Restore the area in the mask with a young woman.` |
| Umgebung infill | `Create inside the masked area: a greenhouse with rows of plants under glass ceiling.` |

## Fidelity-Regeln

### Standard-Edit-Modus
ALLE visuellen Eigenschaften beibehalten, außer die Anweisung ändert sie explizit:
- Subjekt-Identität, Pose, Erscheinung
- Objekt-Existenz, Position, Größe, Orientierung
- Komposition, Kamerawinkel, Objektivmerkmale
- Stil/Medium

Nur ändern, was der Edit strikt erfordert.

### Maskierter Edit-Modus
- Alle sichtbaren (nicht maskierten) Teile exakt erhalten
- Graue Maskenregionen nahtlos mit unmaskierten Bereichen füllen
- Bestehenden Stil, Licht und Motiv treffen
- Graue Masken nie beschreiben — Inhalt beschreiben, der sie füllt

## Beispiel-Ausgabe

```json
{
  "short_description": "A professional businesswoman in a navy blazer stands confidently in a modern glass office, holding a tablet. Natural daylight streams through floor-to-ceiling windows, creating a warm, productive atmosphere.",
  "objects": [
    {
      "description": "A confident businesswoman in her 30s with shoulder-length dark hair, wearing a tailored navy blazer over a white blouse. She holds a tablet in her left hand while gesturing naturally with her right.",
      "location": "center-right",
      "relative_size": "large within frame",
      "shape_and_color": "Human figure, navy and white clothing",
      "texture": "smooth fabric, professional attire",
      "appearance_details": "Minimal jewelry, well-groomed professional appearance",
      "relationship": "Main subject, interacting with tablet",
      "orientation": "facing slightly left, three-quarter view",
      "pose": "Standing upright, relaxed professional stance",
      "expression": "confident, approachable smile",
      "clothing": "Tailored navy blazer, white silk blouse, dark trousers",
      "action": "Presenting or reviewing information on tablet",
      "gender": "female",
      "skin_tone_and_texture": "Medium warm skin tone, healthy smooth complexion"
    },
    {
      "description": "A modern tablet device with a bright display showing charts and graphs",
      "location": "center, held by subject",
      "relative_size": "small",
      "shape_and_color": "Rectangular, silver frame with illuminated screen",
      "texture": "smooth glass and metal",
      "appearance_details": "Thin profile, business application visible on screen",
      "relationship": "Held by businesswoman, focus of her attention",
      "orientation": "vertical, screen facing viewer at slight angle",
      "pose": null,
      "expression": null,
      "clothing": null,
      "action": null,
      "gender": null,
      "skin_tone_and_texture": null,
      "number_of_objects": null
    }
  ],
  "background_setting": "Modern corporate office interior with floor-to-ceiling windows overlooking a city skyline. Minimalist furniture in neutral tones, potted plants adding touches of green.",
  "lighting": {
    "conditions": "bright natural daylight",
    "direction": "side-lit from left through windows",
    "shadows": "soft, natural shadows"
  },
  "aesthetics": {
    "composition": "rule of thirds, medium shot",
    "color_scheme": "professional blues and neutral whites with warm accents",
    "mood_atmosphere": "confident, professional, welcoming"
  },
  "photographic_characteristics": {
    "depth_of_field": "shallow, background slightly soft",
    "focus": "sharp focus on subject's face and upper body",
    "camera_angle": "eye-level",
    "lens_focal_length": "portrait lens (85mm)"
  },
  "style_medium": "photograph",
  "artistic_style": "realistic",
  "context": "Corporate portrait photography for company website or LinkedIn professional profile.",
  "text_render": [],
  "edit_instruction": "Generate a professional businesswoman in a modern office environment holding a tablet."
}
```

## Häufige Fehler

1. **Keinen Text erfinden** — `text_render` leer lassen, außer der User liefert exakten Text
2. **Nicht überbeschreiben** — Max. 5 Objekte, Wichtigstes priorisieren
3. **Modus treffen** — Richtiges `edit_instruction`-Format für maskiert vs. Standard
4. **Fidelity wahren** — Nur explizit Gewünschtes ändern
5. **Spezifisch sein** — Konkrete Werte („85mm portrait lens“), keine vagen Begriffe („nice camera“)
6. **Null bei Irrelevantem** — Mensch-spezifische Felder bei Nicht-Menschen `null`


### curl-Beispiel

```bash
curl -X POST "https://engine.prod.bria-api.com/v2/image/generate" \
  -H "api_token: $BRIA_API_KEY" \
  -H "Content-Type: application/json" \
  -H "User-Agent: BriaSkills/1.3.4" \
  -d '{
    "structured_prompt": "{\"short_description\": \"...\", ...}",
    "prompt": "Generate this scene",
    "aspect_ratio": "16:9"
  }'
```

---

## Referenzen

- **[Schema-Referenz](references/schema-reference.md)** — Vollständiges JSON-Schema mit allen Parameterwerten
- **[bria-ai](../bria-ai/SKILL.md)** — API-Client und Endpoint-Dokumentation zur Ausführung von VGL-Prompts
