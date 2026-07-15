# Bria.ai API-Referenz

## Base URL & Authentifizierung

**Base URL:** `https://engine.prod.bria-api.com`

**Authentifizierung:** Diese Header in allen Requests mitschicken:
```
api_token: YOUR_BRIA_API_KEY
Content-Type: application/json
User-Agent: BriaSkills/<version>
```

> **Pflicht:** Immer den Header `User-Agent: BriaSkills/<version>` mitschicken (wobei `<version>` die aktuelle Skill-Version aus `package.json` ist, z. B. `BriaSkills/1.3.4`) — auch bei Status-Polling.

---

## FIBO - Image Generation

### POST /v2/image/generate

Bilder aus Text-Prompts mit FIBOs strukturiertem Prompt-System generieren.

**Request:**
```json
{
  "prompt": "string (required)",
  "aspect_ratio": "1:1",
  "resolution": "1MP",
  "negative_prompt": "string",
  "num_results": 1,
  "seed": null
}
```

**Parameters:**

| Parameter | Typ | Default | Beschreibung |
|-----------|------|---------|-------------|
| `prompt` | string | required* | Bildbeschreibung (* oder `structured_prompt`) |
| `aspect_ratio` | string | "1:1" | "1:1", "4:3", "16:9", "3:4", "9:16" |
| `resolution` | string | "1MP" | Output image resolution. "1MP" or "4MP". "4MP" improves image details, especially for photorealism, but increases latency by ~30 seconds. |
| `negative_prompt` | string | - | Was ausgeschlossen werden soll |
| `num_results` | int | 1 | Anzahl Bilder (1-4) |
| `seed` | int | random | Für Reproduzierbarkeit |
| `structured_prompt` | string | - | JSON from previous generation (for refinement). Use with `prompt` to refine, or alone with `seed` to recreate. |
| `image_url` | string | - | Reference image (for inspire mode) |

**Input Combination Rules** (mutually exclusive):
- `prompt` — Generate from text
- `image_url` — Generate inspired by a reference image
- `image_url` + `prompt` — Generate inspired by image, guided by text
- `structured_prompt` + `seed` — Recreate a previous image exactly
- `structured_prompt` + `prompt` + `seed` — Refine a previous image with new instructions

All combinations support `aspect_ratio`, `negative_prompt`, `num_results`, and `seed`.

**Response:**
```json
{
  "request_id": "uuid",
  "status_url": "https://engine.prod.bria-api.com/v2/status/uuid"
}
```

**Completed Result:**
```json
{
  "status": "COMPLETED",
  "result": {
    "image_url": "https://...",
    "structured_prompt": "{...}",
    "seed": 12345
  }
}
```

---

## RMBG-2.0 - Background Removal

### POST /v2/image/edit/remove_background

Hintergrund vom Bild entfernen. Gibt PNG mit Transparenz zurück.

**Request:**
```json
{
  "image": "https://publicly-accessible-image-url"
}
```

**Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| `image` | string | Quell-Bild-URL (JPEG, PNG, WEBP) |

**Response:**
```json
{
  "request_id": "uuid",
  "status_url": "https://..."
}
```

**Completed Result:**
```json
{
  "status": "COMPLETED",
  "result": {
    "image_url": "https://...png"
  }
}
```

---

## FIBO-Edit - Image Editing

### POST /v2/image/edit

Bild per natürlichsprachlichen Anweisungen bearbeiten. Keine Maske nötig.

**Request:**
```json
{
  "images": ["https://source-image-url"],
  "instruction": "change the mug color to red"
}
```

**Parameters:**

| Parameter | Typ | Default | Beschreibung |
|-----------|------|---------|-------------|
| `images` | array | required | Array von Bild-URLs oder Base64-Data-URLs |
| `instruction` | string | required | Bearbeitungsanweisung in natürlicher Sprache |

### POST /v2/image/edit/gen_fill

Inhalt in maskierter Region generieren (Inpainting).

**Request:**
```json
{
  "image": "https://source-image-url",
  "mask": "https://mask-image-url",
  "prompt": "what to generate",
  "mask_type": "manual",
  "negative_prompt": "string",
  "num_results": 1
}
```

**Parameters:**

| Parameter | Typ | Default | Beschreibung |
|-----------|------|---------|-------------|
| `image` | string | required | Source image URL |
| `mask` | string | required | Masken-URL (weiß=bearbeiten, schwarz=behalten) |
| `prompt` | string | required | Was in maskiertem Bereich generiert werden soll |
| `mask_type` | string | "manual" | "manual" or "automatic" |
| `negative_prompt` | string | - | Was vermieden werden soll |
| `num_results` | int | 1 | Anzahl Variationen |

**Mask Requirements:**
- White pixels (255) = area to edit
- Black pixels (0) = area to preserve
- Same aspect ratio as source image

### POST /v2/image/edit/erase

Per Maske definierte Objekte entfernen.

**Request:**
```json
{
  "image": "https://source-image-url",
  "mask": "https://mask-image-url"
}
```

### POST /v2/image/edit/erase_foreground

Hauptsubjekt entfernen und mit Hintergrund füllen.

**Request:**
```json
{
  "image": "https://source-image-url"
}
```

### POST /v2/image/edit/replace_background

Hintergrund durch KI-generierten Inhalt ersetzen.

**Request:**
```json
{
  "image": "https://source-image-url",
  "prompt": "new background description"
}
```

### POST /v2/image/edit/blur_background

Blur-Effekt auf Bildhintergrund anwenden.

**Request:**
```json
{
  "image": "https://source-image-url"
}
```

### POST /v2/image/edit/expand

Bild erweitern/outpainten, um Grenzen zu erweitern.

**Request:**
```json
{
  "image": "base64-string-or-url",
  "aspect_ratio": "16:9",
  "prompt": "optional description for new content"
}
```

**Parameters:**

| Parameter | Typ | Default | Beschreibung |
|-----------|------|---------|-------------|
| `image` | string | required | Source image URL or base64 string |
| `aspect_ratio` | string | required | Target ratio: "1:1", "4:3", "16:9", "3:4", "9:16" |
| `prompt` | string | - | Optional - describe content to generate |

### POST /v2/image/edit/enhance

Bildqualität verbessern (Beleuchtung, Farben, Details).

**Request:**
```json
{
  "image": "https://source-image-url"
}
```

### POST /v2/image/edit/increase_resolution

Bildauflösung hochskalieren.

**Request:**
```json
{
  "image": "https://source-image-url",
  "desired_increase": 4,
  "preserve_alpha": true
}
```

**Parameters:**

| Parameter | Typ | Default | Beschreibung |
|-----------|------|---------|-------------|
| `image` | string | required | Source image URL |
| `desired_increase` | int | 2 | Upscale factor, range 2–4 |
| `preserve_alpha` | bool | false | Preserve transparency. Set `true` when input has an alpha channel — the API upscales and recombines the alpha server-side, so you don't need to handle it client-side. |

### POST /v1/product/lifestyle_shot_by_text

Produkt per Textbeschreibung in Lifestyle-Szene platzieren.

**Request:**
```json
{
  "file": "BASE64_ENCODED_IMAGE",
  "scene_description": "modern kitchen countertop, natural lighting",
  "placement_type": "automatic"
}
```

### POST /image/edit/product/integrate

Integrate and embed one or more products into a predefined scene at precise user-defined coordinates. The product is automatically matched to the scene's lighting, perspective, and aesthetics. Products are automatically cut out from their background as part of the pipeline.

**Request:**
```json
{
  "scene": "https://scene-image-url",
  "products": [
    {
      "image": "https://product-image-url",
      "coordinates": {
        "x": 100,
        "y": 200,
        "width": 300,
        "height": 400
      }
    }
  ],
  "seed": 42
}
```

**Parameters:**

| Parameter | Typ | Default | Beschreibung |
|-----------|------|---------|-------------|
| `scene` | string | required | Scene image URL or base64. Accepted formats: jpeg, jpg, png, webp |
| `products` | array | required | Array of product objects (1 to N products) |
| `products[].image` | string | required | Product image URL or base64. If it has an alpha channel, no cutout is applied; otherwise automatic cutout is applied |
| `products[].coordinates` | object | required | Placement and scaling of the product within the scene |
| `products[].coordinates.x` | int | required | X-coordinate of the product's top-left corner (pixels) |
| `products[].coordinates.y` | int | required | Y-coordinate of the product's top-left corner (pixels) |
| `products[].coordinates.width` | int | required | Desired product width in pixels (must not exceed scene dimensions) |
| `products[].coordinates.height` | int | required | Desired product height in pixels (must not exceed scene dimensions) |
| `seed` | int | random | Seed for deterministic generation |

**Response:**
```json
{
  "request_id": "uuid",
  "result": {
    "image_url": "https://..."
  }
}
```

**Async Response (202):**
```json
{
  "request_id": "uuid",
  "status_url": "https://..."
}
```

---

## Text-Based Object Editing

### POST /v2/image/edit/add_object_by_text

Neues Objekt per natürlicher Sprache zum Bild hinzufügen.

**Request:**
```json
{
  "image": "base64-or-url",
  "instruction": "Place a red vase with flowers on the table"
}
```

### POST /v2/image/edit/replace_object_by_text

Bestehendes Objekt durch neues ersetzen.

**Request:**
```json
{
  "image": "base64-or-url",
  "instruction": "Replace the red apple with a green pear"
}
```

### POST /v2/image/edit/erase_by_text

Spezifisches Objekt per Name entfernen.

**Request:**
```json
{
  "image": "base64-or-url",
  "object_name": "table"
}
```

---

## Image Transformation

### POST /v2/image/edit/blend

Bilder blenden/mergen oder Texturen anwenden.

**Request:**
```json
{
  "image": "base64-or-url",
  "overlay": "texture-or-art-url",
  "instruction": "Place the art on the shirt, keep the art exactly the same"
}
```

### POST /v2/image/edit/reseason

Jahreszeit oder Wetter eines Bildes ändern.

**Request:**
```json
{
  "image": "base64-or-url",
  "season": "winter"
}
```

**Seasons:** `spring`, `summer`, `autumn`, `winter`

### POST /v2/image/edit/restyle

Künstlerischen Stil eines Bildes transformieren.

**Request:**
```json
{
  "image": "base64-or-url",
  "style": "oil_painting"
}
```

**Style IDs:** `render_3d`, `cubism`, `oil_painting`, `anime`, `cartoon`, `coloring_book`, `retro_ad`, `pop_art_halftone`, `vector_art`, `story_board`, `art_nouveau`, `cross_etching`, `wood_cut`

### POST /v2/image/edit/relight

Beleuchtung eines Bildes ändern.

**Request:**
```json
{
  "image": "base64-or-url",
  "light_type": "sunrise light",
  "light_direction": "front"
}
```

**Parameters:**

| Parameter | Typ | Default | Beschreibung |
|-----------|------|---------|-------------|
| `image` | string | required | Source image URL or base64 |
| `light_type` | string | required | Lighting preset (see values below) |
| `light_direction` | string | required | `front`, `side`, `bottom`, `top-down` |

**Light Types:** `midday`, `blue hour light`, `low-angle sunlight`, `sunrise light`, `spotlight on subject`, `overcast light`, `soft overcast daylight lighting`, `cloud-filtered lighting`, `fog-diffused lighting`, `side lighting`, `moonlight lighting`, `starlight nighttime`, `soft bokeh lighting`, `harsh studio lighting`

---

## Image Restoration & Conversion

### POST /v2/image/edit/sketch_to_colored_image

Skizze oder Line Drawing in fotorealistisches Bild konvertieren.

**Request:**
```json
{
  "image": "sketch-base64-or-url",
  "prompt": "optional description"
}
```

### POST /v2/image/edit/restore

Alte/beschädigte Fotos restaurieren durch Entfernen von Rauschen, Kratzern und Unschärfe.

**Request:**
```json
{
  "image": "base64-or-url"
}
```

### POST /v2/image/edit/colorize

Farbe zu S/W-Fotos hinzufügen oder zu S/W konvertieren.

**Request:**
```json
{
  "image": "base64-or-url",
  "color": "contemporary color"
}
```

**Colors:** `contemporary color`, `vivid color`, `black and white colors`, `sepia vintage`

### POST /v2/image/edit/crop_foreground

Hintergrund entfernen und eng um Vordergrund croppen.

**Request:**
```json
{
  "image": "base64-or-url"
}
```

---

## Structured Instructions

### POST /v2/structured_instruction/generate

Strukturierte JSON-Anweisung aus natürlicher Sprache generieren (kein Bild).

**Request:**
```json
{
  "images": ["base64-or-url"],
  "instruction": "change to golden hour lighting",
  "mask": "optional-mask-url"
}
```

**Returns:** `structured_instruction` JSON that can be passed to `/v2/image/edit`

---

## Status Polling

### GET /v2/status/{request_id}

Async-Request-Status prüfen.

**Response:**
```json
{
  "status": "IN_PROGRESS | COMPLETED | FAILED",
  "result": {
    "image_url": "https://..."
  },
  "request_id": "uuid"
}
```

**Status Values:**
- `IN_PROGRESS` - Noch in Bearbeitung
- `COMPLETED` - Erfolg, Ergebnis verfügbar
- `FAILED` - Fehler aufgetreten

**Polling Pattern:**
```python
import requests, time

def poll(status_url, api_key, timeout=120):
    headers = {"api_token": api_key, "User-Agent": "BriaSkills/1.3.4"}
    for _ in range(timeout // 2):
        r = requests.get(status_url, headers=headers)
        data = r.json()
        if data["status"] == "COMPLETED":
            return data["result"]["image_url"]
        if data["status"] == "FAILED":
            raise Exception(data.get("error"))
        time.sleep(2)
    raise TimeoutError()
```

---

## Error Handling

### HTTP Status Codes

| Code | Beschreibung |
|------|-------------|
| 200 | Erfolg |
| 400 | Bad Request |
| 401 | Unauthorized — ungültiger API-Key |
| 415 | Unsupported Media Type |
| 422 | Validierung fehlgeschlagen / Content Moderation blockiert |
| 429 | Rate Limited |
| 500 | Server-Fehler |

### Supported Image Formats

- **Input:** JPEG, JPG, PNG, WEBP (RGB, RGBA, CMYK)
- **Output:** PNG (with transparency where applicable)
