---
name: bria-ai
description: >-
  KI-Bildgenerierung, -bearbeitung und Hintergrundentfernung via Bria.ai API —
  transparente PNGs, Text-zu-Bild, natürlichsprachige Edits, Produktfotos,
  Lifestyle-Shots, Hintergrund ersetzen/bluren, Upscaling, Restyle, Batch-Assets.
  Verwenden bei Freistellen, transparenten PNGs, Generieren, Bearbeiten oder
  Transformieren von Bildern — Hero, Banner, Social, Produktfotos, Illustrationen,
  Icons, Thumbnails, Ads, Marketing. Auch Cutout, Inpainting, Outpainting,
  Objekt-Entfernung/-Hinzufügung, Restoration, Style Transfer, Relight, Reseason,
  Sketch-to-Photo. Kommerziell sicher, lizenzfrei. 20+ Endpoints für E-Commerce,
  Webdesign und Content-Pipelines.
license: MIT
metadata:
  author: Bria AI
  version: "1.3.4"
---

# Bria — KI-Bildgenerierung, -bearbeitung & Hintergrundentfernung

Kommerziell sichere, lizenzfreie Bildgenerierung und -bearbeitung über 20+ API-Endpoints. Generieren aus Text, Bearbeiten per natürlicher Sprache, Hintergründe entfernen, Produktshots erstellen und automatisierte Bild-Pipelines bauen.

Weitere Endpoint-Details: [Bria API reference for agents](https://docs.bria.ai/llms.txt).

## Wann diesen Skill nutzen

Nutze diesen Skill, wenn der Nutzer möchte:
- **Bilder generieren** — „erstelle ein Bild von…“, „mach mir ein Banner“, „generiere ein Hero-Bild“, „ich brauche ein Produktfoto“
- **Bilder bearbeiten** — „ändere den Hintergrund“, „mach es winterlich“, „füge eine Vase auf den Tisch“, „entferne die Person“
- **Hintergrund entfernen/ersetzen** — „mache den Hintergrund transparent“, „schneide das Produkt aus“, „ersetze mit Studio-Hintergrund“
- **Produktfotografie** — „erstelle einen Lifestyle-Shot“, „platziere das Produkt in einer Küche“, „E-Commerce-Packshot“
- **Verbessern/transformieren** — „skaliere dieses Bild hoch“, „höhere Auflösung“, „restyle als Ölgemälde“, „ändere die Beleuchtung“
- **Batch/Pipeline** — „generiere 10 Produktbilder“, „verarbeite alle diese Bilder“, „Hintergründe in Bulk entfernen“

Dieser Skill deckt das volle Spektrum KI-Bildoperationen ab. Erwähnt der Nutzer Bilder, Fotos, Visuals oder visuelle Inhalte — diesen Skill nutzen.

---

## Was du bauen kannst

- **E-Commerce-Produktkatalog** — Produktfotos generieren, Hintergründe für transparente PNGs entfernen, Produkte in Lifestyle-Szenen (Küche, Büro, Outdoor), Packshots im konsistenten Stil
- **Landing-Page-Visuals** — Hero-Bilder, abstrakte Tech-Hintergründe, Team-Fotos, Sektions-Illustrationen — passend zur Markenästhetik
- **Social-Media-Content** — Instagram-Posts (1:1), Stories/Reels (9:16), LinkedIn-Banner (16:9), Ad-Creatives — Varianten für A/B-Tests batch-generieren
- **Marketing-Kampagnen-Assets** — Saisonale Transformationen (Sommer→Winter), Produktshots für andere Märkte restylen, lokalisierte Visuals in Scale
- **Foto-Restaurierungs-Pipeline** — Alte beschädigte Fotos restaurieren, S/W colorisieren, Low-Res auf 4x hochskalieren, Qualität automatisch verbessern
- **Brand-Asset-Toolkit** — Hintergründe von Logos entfernen, Artwork auf Produkte (T-Shirts, Tassen) blenden, konsistente Produktfotografie im gesamten Katalog
- **KI-Design-Workflows** — Operationen ketten: generieren→bearbeiten→Hintergrund entfernen→in Szene platzieren→hochskalieren — alles per API-Pipelines automatisiert

---

## Setup — Authentifizierung

Vor jedem API-Call brauchst du ein gültiges Bria-Access-Token.

### Schritt 1: Bestehende Credentials prüfen

```bash
if [ -f ~/.bria/credentials ]; then
  BRIA_ACCESS_TOKEN=$(grep '^access_token=' "$HOME/.bria/credentials" | cut -d= -f2-)
  BRIA_API_KEY=$(grep '^api_token=' "$HOME/.bria/credentials" | cut -d= -f2-)
fi
if [ -z "$BRIA_ACCESS_TOKEN" ]; then
  echo "NO_CREDENTIALS"
elif [ -n "$BRIA_API_KEY" ]; then
  echo "READY"
else
  echo "CREDENTIALS_FOUND"
fi
```

Ist die Ausgabe `READY`, direkt API-Calls — keine Introspection nötig.
Ist die Ausgabe `CREDENTIALS_FOUND`, zu Schritt 3 springen.
Ist die Ausgabe `NO_CREDENTIALS`, Schritt 2.

### Schritt 2: Authentifizierung per Device Authorization

Device-Authorization-Flow starten:

**2a. Device-Code anfordern:**

```bash
DEVICE_RESPONSE=$(curl -s -X POST "https://engine.prod.bria-api.com/v2/auth/device/authorize" \
  -H "Content-Type: application/json")
echo "$DEVICE_RESPONSE"
```

Response-Felder parsen:
- `device_code` — zum Token-Polling (behalten, Nutzer nicht zeigen)
- `user_code` — Code, den der Nutzer eingeben muss (z. B. `BRIA-XXXX`)
- `interval` — Sekunden zwischen Poll-Versuchen

**2b. Dem Nutzer einen einzigen Sign-in-Link zeigen.** Genau das sagen — nicht mehr:

> **Bria-Konto verbinden:** [Hier klicken zum Anmelden](https://platform.bria.ai/device/verify?user_code={user_code})
> Dein Code ist **{user_code}** — er ist bereits vorausgefüllt.

Keine zwei Links. Keine rohe URL separat. Nicht `verification_uri` aus der API-Response nutzen. Ein klickbarer Link reicht.

**2c. Auf Token pollen.** Nach Anzeige des Codes sofort pollen. Bis zu 60 Mal mit dem angegebenen Intervall (Default 5 Sekunden):

```bash
for i in $(seq 1 60); do
  TOKEN_RESPONSE=$(curl -s -X POST "https://engine.prod.bria-api.com/v2/auth/token" \
    -d "grant_type=urn:ietf:params:oauth:grant-type:device_code" \
    -d "device_code=$DEVICE_CODE")
  ACCESS_TOKEN=$(printf '%s' "$TOKEN_RESPONSE" | sed -n 's/.*"access_token" *: *"\([^"]*\)".*/\1/p')
  if [ -n "$ACCESS_TOKEN" ]; then
    BRIA_ACCESS_TOKEN="$ACCESS_TOKEN"
    REFRESH_TOKEN=$(printf '%s' "$TOKEN_RESPONSE" | sed -n 's/.*"refresh_token" *: *"\([^"]*\)".*/\1/p')
    mkdir -p ~/.bria
    printf 'access_token=%s\nrefresh_token=%s\n' "$BRIA_ACCESS_TOKEN" "$REFRESH_TOKEN" > "$HOME/.bria/credentials"
    echo "AUTHENTICATED"
    break
  fi
  sleep 5
done
```

Enthält die Ausgabe `AUTHENTICATED`, Schritt 3. Sonst Code abgelaufen — von Schritt 2a neu starten.

**Keinen API-Call starten, bis Authentifizierung bestätigt ist.**

### Schritt 3: Billing-Status prüfen und API-Key auflösen

Bearer-Token introspektieren für Billing-Status und echten API-Key für Bria-Calls:

```bash
INTROSPECT=$(curl -s -X POST "https://engine.prod.bria-api.com/v2/auth/token/introspect" \
  -d "token=$BRIA_ACCESS_TOKEN")
BILLING_STATUS=$(printf '%s' "$INTROSPECT" | sed -n 's/.*"billing_status" *: *"\([^"]*\)".*/\1/p')
if [ "$BILLING_STATUS" = "blocked" ]; then
  BILLING_MSG=$(printf '%s' "$INTROSPECT" | sed -n 's/.*"billing_message" *: *"\([^"]*\)".*/\1/p')
  echo "BILLING_ERROR: $BILLING_MSG"
fi
ACTIVE=$(printf '%s' "$INTROSPECT" | sed -n 's/.*"active" *: *\([^,}]*\).*/\1/p' | tr -d ' ')
if [ "$ACTIVE" = "false" ]; then
  # Clear stale tokens so re-auth starts fresh (credentials file is re-created in Step 2c)
  printf '' > "$HOME/.bria/credentials"
  echo "TOKEN_EXPIRED"
fi
BRIA_API_KEY=$(printf '%s' "$INTROSPECT" | sed -n 's/.*"api_token" *: *"\([^"]*\)".*/\1/p')
if [ -n "$BRIA_API_KEY" ]; then
  grep -v '^api_token=' "$HOME/.bria/credentials" > "$HOME/.bria/credentials.tmp" 2>/dev/null || true
  printf 'api_token=%s\n' "$BRIA_API_KEY" >> "$HOME/.bria/credentials.tmp"
  mv "$HOME/.bria/credentials.tmp" "$HOME/.bria/credentials"
fi
```

Ausgabe interpretieren:
- Druckt `BILLING_ERROR: ...` — Meldung dem Nutzer **wörtlich** weitergeben und **stoppen**. Keine API-Calls.
- Druckt `TOKEN_EXPIRED` — Session ungültig. Nutzer informieren, dass die Session abgelaufen ist, von Schritt 2 neu starten.
- Sonst enthält `BRIA_API_KEY` den echten API-Key, gecacht für spätere Calls. Weiter zum nächsten Abschnitt.

---

## Kernfähigkeiten

| Bedarf | Fähigkeit | Use Case |
|------|------------|----------|
| Bilder aus Text generieren | FIBO Generate | Hero-Bilder, Produktshots, Illustrationen, Social-Media, Banner |
| Bild per Textanweisung bearbeiten | FIBO-Edit | Farben ändern, Objekte modifizieren, Szenen transformieren |
| Bildregion mit Maske bearbeiten | GenFill/Erase | Präzises Inpainting, Regionen hinzufügen/ersetzen |
| Objekte hinzufügen/ersetzen/entfernen | Text-basierte Bearbeitung | Vase hinzufügen, Apfel durch Birne ersetzen, Tisch entfernen |
| Hintergrund entfernen (transparentes PNG) | RMBG-2.0 | Subjekte für Overlays, Logos, Cutouts |
| Hintergrund ersetzen/bluren/löschen | Background-Ops | Hintergrund ändern, bluren oder entfernen |
| Bilder erweitern/outpainten | Outpainting | Grenzen erweitern, Aspect Ratios ändern |
| Auflösung hochskalieren | Super Resolution | Auflösung 2x oder 4x erhöhen |
| Bildqualität verbessern | Enhancement | Beleuchtung, Farben, Details |
| Bilder restylen | Restyle | Ölgemälde, Anime, Cartoon, 3D-Render |
| Beleuchtung ändern | Relight | Golden Hour, Spotlight, dramatisches Licht |
| Jahreszeit ändern | Reseason | Frühling, Sommer, Herbst, Winter |
| Bilder compositen/blenden | Image Blending | Texturen, Logos, Bilder mergen |
| Alte Fotos restaurieren | Restoration | Alte/beschädigte Fotos reparieren |
| Bilder colorisieren | Colorization | Farbe zu S/W oder zu S/W konvertieren |
| Skizze zu Foto | Sketch2Image | Zeichnungen in realistische Fotos |
| Produkt-Lifestyle-Shots | Lifestyle Shot | Produkte in Szenen für E-Commerce |
| Produkte in Szenen integrieren | Product Integrate | Produkte an exakten Koordinaten einbetten |

## Beliebigen Endpoint aufrufen

Nutze `bria_call` für alle API-Calls. Handhabt URL-Passthrough, lokale Datei-Base64, JSON, API-Call und async Polling in einem Aufruf. API-Key wird automatisch aus `~/.bria/credentials` geladen.

**Zuerst** Helper-Skript unter `references/code-examples/bria_client.sh` sourcen (relativ zum Skill-Verzeichnis).

```bash
source <SKILL_DIR>/references/code-examples/bria_client.sh

# Generate (no image input — pass empty string)
RESULT=$(bria_call /v2/image/generate "" '"prompt": "your description", "aspect_ratio": "16:9", "sync": true')

# Remove background
RESULT=$(bria_call /v2/image/edit/remove_background "/path/to/local/image.png")

# Replace background
RESULT=$(bria_call /v2/image/edit/replace_background "https://example.com/img.jpg" '"prompt": "sunset beach"')

# Edit image (uses images array — pass --key images)
RESULT=$(bria_call /v2/image/edit "/path/to/image.png" --key images '"instruction": "make it look warmer"')

# Upscale (use `desired_increase`, range 2-4. Add `"preserve_alpha": true` for transparent inputs)
RESULT=$(bria_call /v2/image/edit/increase_resolution "https://example.com/img.jpg" '"desired_increase": 4')

# Lifestyle shot
RESULT=$(bria_call /v1/product/lifestyle_shot_by_text "/path/to/product.png" '"scene_description": "modern kitchen countertop"')

echo "$RESULT"
```

**Aufruf-Konvention:** `bria_call <endpoint> <image_or_empty> [--key <json_key>] [extra JSON fields...]`
- URL, lokalen Dateipfad oder `""` (leer) für Endpoints ohne Bild-Input
- `--key images`, wenn der Endpoint ein `images`-Array statt `image` erwartet
- Extra JSON-Felder als Key-Value: `'"key": "value"'`
- Gibt bei Erfolg die Ergebnis-Bild-URL zurück, sonst Fehler auf stderr

**Generierungs-Optionen:** Aspect Ratios `1:1`, `16:9`, `4:3`, `9:16`, `3:4`. Auflösung `1MP` (Default) oder `4MP` (mehr Detail, +30s). `"sync": true` für Einzelbilder.

> **Advanced**: Für präzise Generierungskontrolle den **vgl**-Skill für strukturierte VGL-JSON-Prompts statt natürlicher Sprache.

Siehe **[API-Endpoints-Referenz](references/api-endpoints.md)** für vollständige Parameter-Doku aller 20+ Endpoints.

---

## Prompt-Engineering-Tipps

- **Stil**: „professionelle Produktfotografie“ vs. „lockerer Schnappschuss“, „Flat-Design-Illustration“ vs. „3D-Render“
- **Beleuchtung**: „weiches natürliches Licht“, „Studiobeleuchtung“, „dramatische Schatten“
- **Hintergrund**: „weißes Studio“, „Verlauf“, „unscharfes Büro“, „transparent“
- **Komposition**: „zentriert“, „Drittel-Regel“, „Negativraum links für Text“
- **Qualitäts-Keywords**: „hohe Qualität“, „professionell“, „commercial grade“, „4K“, „scharfer Fokus“
- **Negative Prompts**: „unscharf, niedrige Qualität, pixelig“, „Text, Wasserzeichen, Logo“

### Rezepte nach Use Case

**Hero-Banner (16:9):** `"Modern tech startup workspace with developers collaborating, bright natural lighting, clean minimal aesthetic"` — „clean background“ oder „minimal“ für Text-Overlay-Platz

**Produktfoto (1:1):** `"Professional product photo of [item] on white studio background, soft shadows, commercial photography lighting"` — dann Hintergrund entfernen für transparentes PNG

**Präsentations-Visual (16:9):** `"Abstract visualization of data analytics, blue and purple gradient, modern corporate style, clean composition with space for text"` — typische Themen: „abstract technology“, „business collaboration“, „minimalist geometric patterns“

**Instagram-Post (1:1):** `"Lifestyle photo of coffee and laptop on wooden desk, morning light, cozy atmosphere"`

**Story/Reel (9:16):** `"Vertical product showcase of smartphone, floating in gradient background, tech aesthetic"`

---

## Weitere Ressourcen

- **[API-Endpoints-Referenz](references/api-endpoints.md)** — Vollständige Endpoint-Doku mit Request/Response für alle 20+ Endpoints
- **[Shell-Client (bria_client.sh)](references/code-examples/bria_client.sh)** — Single-Function-Helper: `bria_call` für Auth, Base64, JSON, Polling
- **[Vollständige API-Docs für Agents (llms.txt)](https://docs.bria.ai/llms.txt)** — Agent-ready Bria-API-Referenz; wenn diese Skill-Zusammenfassung nicht reicht

## Verwandte Skills

- **vgl** — Strukturierte VGL-JSON-Prompts für präzise, deterministische Kontrolle bei FIBO-Bildgenerierung
- **image-utils** — Klassische Bildbearbeitung (Resize, Crop, Composite, Wasserzeichen) für Nachbearbeitung
- **@horos-image-manifest** — **Pflicht** nach jedem Bria-Output: detailliertes `.manifest.md` (Prompt, Endpoint, Hex, Alpha, Einbindung); Rule `@image-manifest`
