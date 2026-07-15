---
name: image-utils
description: >-
  Klassische Bildbearbeitung mit Python Pillow — Resize, Crop, Composite,
  Format-Konvertierung, Wasserzeichen, Helligkeit/Kontrast und Web-Optimierung.
  Verwenden bei Nachbearbeitung KI-generierter Bilder, Web-Delivery,
  Batch-Verarbeitung, responsiven Varianten oder deterministischen Pixel-Ops.
  Standalone oder mit bria-ai für generierte Bilder.
license: MIT
metadata:
  author: Bria AI
  version: "1.3.4"
---

# Bild-Utilities

Pillow-basierte Utilities für deterministische Pixel-Operationen: Resize, Crop, Composite, Format-Konvertierung, Wasserzeichen und Standard-Bildverarbeitung.

## Wann diesen Skill nutzen

- **Nachbearbeitung KI-Bilder**: Resize, Crop, Web-Optimierung nach Generierung
- **Format-Konvertierung**: PNG ↔ JPEG ↔ WEBP mit Qualitätskontrolle
- **Compositing**: Bilder überlagern, Subjekte auf Hintergründe legen
- **Batch-Verarbeitung**: Mehrere Größen, Wasserzeichen
- **Web-Optimierung**: Komprimieren und resizen für schnelle Auslieferung
- **Social-Media-Vorbereitung**: Crop auf plattformspezifische Aspect Ratios

## Wann NICHT diesen Skill — stattdessen `bria-ai`

Dieser Skill behandelt **nur deterministische Pixel-Operationen**. Für **generative oder KI-gestützte** Bildarbeit nutze `bria-ai`:

- **Bilder aus Text-Prompts generieren** → `bria-ai` nutzen
- **KI-Hintergrundentfernung oder -ersetzung** → `bria-ai` nutzen
- **KI-Bildbearbeitung (Inpainting, Objekt-Entfernung/-Hinzufügung)** → `bria-ai` nutzen
- **Style Transfer oder KI-gestützte visuelle Effekte** → `bria-ai` nutzen
- **Produkt-Lifestyle-Shots mit KI** → `bria-ai` nutzen
- **Bild-Upscaling mit KI-Super-Resolution** → `bria-ai` nutzen

**Faustregel**: Braucht die Aufgabe *neuen visuellen Inhalt* oder *Bild-Semantik*, nutze `bria-ai`. Braucht sie *Transformation bestehender Pixel* (Resize, Crop, Format, Wasserzeichen), nutze diesen Skill.

Ist `bria-ai` nicht verfügbar, installieren mit:
```bash
npx skills add bria-ai/bria-skill
```

## Kurzreferenz

| Operation | Methode | Beschreibung |
|-----------|--------|-------------|
| **Laden** | `load(source)` | Von URL, Pfad, Bytes oder Base64 laden |
| | `load_from_url(url)` | Bild von URL herunterladen |
| **Speichern** | `save(image, path)` | Speichern mit Format-Auto-Detection |
| | `to_bytes(image, format)` | In Bytes konvertieren |
| | `to_base64(image, format)` | In Base64-String konvertieren |
| **Resizing** | `resize(image, width, height)` | Auf exakte Abmessungen resizen |
| | `scale(image, factor)` | Mit Faktor skalieren (0.5 = halb) |
| | `thumbnail(image, size)` | In Größe einpassen, Aspect Ratio behalten |
| **Cropping** | `crop(image, left, top, right, bottom)` | Auf Region croppen |
| | `crop_center(image, width, height)` | Von Mitte croppen |
| | `crop_to_aspect(image, ratio)` | Auf Aspect Ratio croppen |
| **Compositing** | `paste(bg, fg, position)` | An Koordinaten überlagern |
| | `composite(bg, fg, mask)` | Alpha-Composite |
| | `fit_to_canvas(image, w, h)` | Auf Canvas-Größe einpassen |
| **Borders** | `add_border(image, width, color)` | Soliden Border hinzufügen |
| | `add_padding(image, padding)` | Whitespace-Padding hinzufügen |
| **Transforms** | `rotate(image, angle)` | Um Grad rotieren |
| | `flip_horizontal(image)` | Horizontal spiegeln |
| | `flip_vertical(image)` | Vertikal spiegeln |
| **Watermarks** | `add_text_watermark(image, text)` | Text-Overlay hinzufügen |
| | `add_image_watermark(image, logo)` | Logo-Wasserzeichen hinzufügen |
| **Adjustments** | `adjust_brightness(image, factor)` | Aufhellen/abdunkeln |
| | `adjust_contrast(image, factor)` | Kontrast anpassen |
| | `adjust_saturation(image, factor)` | Farbsättigung anpassen |
| | `blur(image, radius)` | Gaußschen Blur anwenden |
| **Web** | `optimize_for_web(image, max_size)` | Für Auslieferung optimieren |
| **Info** | `get_info(image)` | Abmessungen, Format, Mode abrufen |

## Voraussetzungen

```bash
pip install Pillow requests
```

## Basis-Nutzung

```python
from image_utils import ImageUtils

# Load from URL
image = ImageUtils.load_from_url("https://example.com/image.jpg")

# Or load from various sources
image = ImageUtils.load("/path/to/image.png")         # File path
image = ImageUtils.load(image_bytes)                  # Bytes
image = ImageUtils.load("data:image/png;base64,...")  # Base64

# Resize and save
resized = ImageUtils.resize(image, width=800, height=600)
ImageUtils.save(resized, "output.webp", quality=90)

# Get image info
info = ImageUtils.get_info(image)
print(f"{info['width']}x{info['height']} {info['mode']}")
```

## Resizing & Skalierung

```python
# Resize to exact dimensions
resized = ImageUtils.resize(image, width=800, height=600)

# Resize maintaining aspect ratio (fit within bounds)
fitted = ImageUtils.resize(image, width=800, height=600, maintain_aspect=True)

# Resize by width only (height auto-calculated)
resized = ImageUtils.resize(image, width=800)

# Scale by factor
half = ImageUtils.scale(image, 0.5)    # 50% size
double = ImageUtils.scale(image, 2.0)  # 200% size

# Create thumbnail
thumb = ImageUtils.thumbnail(image, (150, 150))
```

## Cropping

```python
# Crop to specific region
cropped = ImageUtils.crop(image, left=100, top=50, right=500, bottom=350)

# Crop from center
center = ImageUtils.crop_center(image, width=400, height=400)

# Crop to aspect ratio (for social media)
square = ImageUtils.crop_to_aspect(image, "1:1")      # Instagram
wide = ImageUtils.crop_to_aspect(image, "16:9")       # YouTube thumbnail
story = ImageUtils.crop_to_aspect(image, "9:16")      # Stories/Reels

# Control crop anchor
top_crop = ImageUtils.crop_to_aspect(image, "16:9", anchor="top")
bottom_crop = ImageUtils.crop_to_aspect(image, "16:9", anchor="bottom")
```

## Compositing

```python
# Paste foreground onto background
result = ImageUtils.paste(background, foreground, position=(100, 50))

# Alpha composite (foreground must have transparency)
result = ImageUtils.composite(background, foreground)

# Fit image onto canvas with letterboxing
canvas = ImageUtils.fit_to_canvas(
    image,
    width=1200,
    height=800,
    background_color=(255, 255, 255, 255),  # White
    position="center"  # or "top", "bottom"
)
```

## Format-Konvertierung

```python
# Convert to different formats
png_bytes = ImageUtils.to_bytes(image, "PNG")
jpeg_bytes = ImageUtils.to_bytes(image, "JPEG", quality=85)
webp_bytes = ImageUtils.to_bytes(image, "WEBP", quality=90)

# Get base64 for data URLs
base64_str = ImageUtils.to_base64(image, "PNG")
data_url = ImageUtils.to_base64(image, "PNG", include_data_url=True)
# Returns: "data:image/png;base64,..."

# Save with format auto-detected from extension
ImageUtils.save(image, "output.png")
ImageUtils.save(image, "output.jpg", quality=85)
ImageUtils.save(image, "output.webp", quality=90)
```

## Watermarks

```python
# Text watermark
watermarked = ImageUtils.add_text_watermark(
    image,
    text="© 2024 My Company",
    position="bottom-right",  # bottom-left, top-right, top-left, center
    font_size=24,
    color=(255, 255, 255, 128),  # Semi-transparent white
    margin=20
)

# Logo/image watermark
logo = ImageUtils.load("logo.png")
watermarked = ImageUtils.add_image_watermark(
    image,
    watermark=logo,
    position="bottom-right",
    opacity=0.5,
    scale=0.15,  # 15% of image width
    margin=20
)
```

## Anpassungen

```python
# Brightness (1.0 = original, <1 darker, >1 lighter)
bright = ImageUtils.adjust_brightness(image, 1.3)
dark = ImageUtils.adjust_brightness(image, 0.7)

# Contrast (1.0 = original)
high_contrast = ImageUtils.adjust_contrast(image, 1.5)

# Saturation (0 = grayscale, 1.0 = original, >1 more vivid)
vivid = ImageUtils.adjust_saturation(image, 1.3)
grayscale = ImageUtils.adjust_saturation(image, 0)

# Sharpness
sharp = ImageUtils.adjust_sharpness(image, 2.0)

# Blur
blurred = ImageUtils.blur(image, radius=5)
```

## Transformationen

```python
# Rotate (counter-clockwise, degrees)
rotated = ImageUtils.rotate(image, 45)
rotated = ImageUtils.rotate(image, 90, expand=False)  # Don't expand canvas

# Flip
mirrored = ImageUtils.flip_horizontal(image)
flipped = ImageUtils.flip_vertical(image)
```

## Borders & Padding

```python
# Add solid border
bordered = ImageUtils.add_border(image, width=5, color=(0, 0, 0))

# Add padding (whitespace)
padded = ImageUtils.add_padding(image, padding=20)  # Uniform
padded = ImageUtils.add_padding(image, padding=(10, 20, 10, 20))  # left, top, right, bottom
```

## Web-Optimierung CLI (HorosCode)

For agent-driven post-processing after AI generation or when preparing existing assets:

```bash
python .cursor/skills/image-utils/scripts/optimize_web_image.py --help
```

See **@horos-web-image-optimize** for HorosCloud paths and srcset patterns. The CLI imports `ImageUtils` from `references/code-examples/image_utils.py` — do not duplicate logic in agent scripts.

## Web-Optimierung

```python
# Optimize for web delivery
optimized_bytes = ImageUtils.optimize_for_web(
    image,
    max_dimension=1920,  # Resize if larger
    format="WEBP",       # Best compression
    quality=85
)

# Save optimized
with open("optimized.webp", "wb") as f:
    f.write(optimized_bytes)
```

## Integration mit Bria AI

Use alongside the **[bria-ai skill](https://clawhub.ai/galbria/bria-ai)** to post-process AI-generated images. Generate or edit images with Bria's API, then use image-utils for resizing, cropping, watermarking, and web optimization.

```python
import requests
from image_utils import ImageUtils

# Generate with Bria AI (see bria-ai skill for full API reference)
response = requests.post(
    "https://engine.prod.bria-api.com/v2/image/generate",
    headers={"api_token": BRIA_API_KEY, "Content-Type": "application/json"},
    json={"prompt": "product photo of headphones", "aspect_ratio": "1:1", "sync": True}
)
image_url = response.json()["result"]["image_url"]

# Download and post-process
image = ImageUtils.load_from_url(image_url)

# Create multiple sizes for responsive images
sizes = {
    "large": ImageUtils.resize(image, width=1200),
    "medium": ImageUtils.resize(image, width=600),
    "thumb": ImageUtils.thumbnail(image, (150, 150))
}

# Save all as optimized WebP
for name, img in sizes.items():
    ImageUtils.save(img, f"product_{name}.webp", quality=85)
```

## Batch-Verarbeitungs-Beispiel

```python
from pathlib import Path
from image_utils import ImageUtils

def process_catalog(input_dir, output_dir):
    """Process all images in a directory."""
    output_path = Path(output_dir)
    output_path.mkdir(exist_ok=True)

    for image_file in Path(input_dir).glob("*.{jpg,png,webp}"):
        image = ImageUtils.load(image_file)

        # Crop to square
        square = ImageUtils.crop_to_aspect(image, "1:1")

        # Resize to standard size
        resized = ImageUtils.resize(square, width=800, height=800)

        # Add watermark
        final = ImageUtils.add_text_watermark(resized, "© My Brand")

        # Save optimized
        output_file = output_path / f"{image_file.stem}.webp"
        ImageUtils.save(final, output_file, quality=85)

process_catalog("./raw_images", "./processed")
```

## API-Referenz

See [image_utils.py](./references/code-examples/image_utils.py) for complete implementation with docstrings.
