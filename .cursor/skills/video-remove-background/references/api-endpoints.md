# Video-Remove-Background-API-Referenz

## Base-URL & Authentifizierung

**Base URL:** `https://engine.prod.bria-api.com`

**Authentication:** Diese Header in allen Requests mitschicken:
```
api_token: YOUR_BRIA_API_KEY
Content-Type: application/json
User-Agent: BriaSkills/1.3.4
```

> **Pflicht:** Immer den Header `User-Agent: BriaSkills/1.3.4` in jedem API-Call mitschicken, auch bei Status-Polling.

---

## Video-Hintergrundentfernung

### POST /v2/video/edit/remove_background

Startet einen asynchronen Job zur Hintergrundentfernung für ein Video. Liefert HTTP 202 mit `request_id` und `status_url` zum Pollen, bis ein terminaler Status zurückkommt.

**Request:**
```json
{
  "video": "https://publicly-accessible-video-url.mp4",
  "background_color": "Transparent",
  "output_container_and_codec": "webm_vp9",
  "preserve_audio": true
}
```

**Parameter:**

| Parameter | Typ | Pflicht | Default | Beschreibung |
|-----------|------|----------|---------|-------------|
| `video` | string | Ja | — | Öffentlich erreichbare URL des Eingabevideos. Eingabeauflösung bis 16000x16000 (16K) unterstützt |
| `background_color` | string | Nein | `Transparent` | Nur vordefinierte Strings — einer von: `Transparent`, `Black`, `White`, `Gray`, `Red`, `Green`, `Blue`, `Yellow`, `Cyan`, `Magenta`, `Orange`. Hex-Werte werden nicht unterstützt |
| `output_container_and_codec` | string | Nein | `webm_vp9` (beobachtet) | Output-Preset — einer von: `mp4_h264`, `mp4_h265`, `webm_vp9`, `mov_h265`, `mov_proresks`, `mkv_h264`, `mkv_h265`, `mkv_vp9`, `gif` |
| `preserve_audio` | boolean | Nein | — | Audio-Spur im Output behalten, falls im Input vorhanden |
| `webhook_url` | string | Nein | — | Optionale URL für Ergebnis per Webhook, wenn der Async-Job fertig ist |

**Response (HTTP 202):**
```json
{
  "request_id": "uuid",
  "status_url": "https://engine.prod.bria-api.com/v2/status/{uuid}"
}
```

**Fehler-Response (HTTP 400 / 422):**
```json
{
  "error": {
    "code": 123,
    "message": "...",
    "details": "..."
  },
  "request_id": "uuid"
}
```

### Eingabe-Constraints

- **Max. Eingabedauer:** 60 Sekunden
- **Eingabe-Container:** `.mp4`, `.mov`, `.webm`, `.avi`, `.gif`
- **Eingabe-Codecs:** H.264, H.265 (HEVC), VP9, AV1, PhotoJPEG
- **Auflösung:** bis 16000x16000 (16K). Größere Eingaben liefern `413 Payload Too Large`

### Im Output erhaltene Attribute

- Seitenverhältnis und Auflösung (Output entspricht Input)
- Framerate
- Audio, falls vorhanden (mit `preserve_audio`)

### Transparenz / Alpha-Unterstützung

Ist `background_color` `Transparent` (Default), muss das gewählte Output-Preset **Alpha unterstützen**, sonst antwortet der Server mit `422 Unprocessable Entity`.

| Alpha unterstützt (server-erzwungen) | Alpha NICHT unterstützt |
|-----------------------------------|---------------------|
| `webm_vp9`, `mkv_vp9`, `mov_proresks` | `mp4_h264`, `mp4_h265`, `mkv_h264`, `mkv_h265`, `gif`, `mov_h265` |

> Die öffentlichen Docs listen auch `gif` und `mov_h265` (HEVC mit Alpha) als alpha-fähig, aber die 422-Antwort des Servers akzeptiert nur `webm_vp9`, `mkv_vp9`, `mov_proresks` (verifiziert Juni 2026).

**Verifiziertes Verhalten (Juni 2026):**
- Default-Output (ohne `output_container_and_codec`) ist ein transparentes `.webm` (`webm_vp9`) mit `ALPHA_MODE=1`.
- `mkv_vp9` hat einen echten Alpha-Kanal (dekodiert zu RGBA mit libvpx).
- `mov_proresks` liefert ProRes 4444, aber **ohne Alpha-Ebene** — vor Nutzung verifizieren.
- `gif` scheitert serverseitig mit `500 "list index out of range"`, auch mit solidem Hintergrund — konvertiere stattdessen lokal ein WebM-Ergebnis zu GIF.
- VP9-Alpha liegt in einem WebM-Side-Channel: `ffprobe` zeigt `pix_fmt=yuv420p`; prüfe den `ALPHA_MODE`-Stream-Tag oder dekodiere mit `-c:v libvpx-vp9`.

---

## Lokaler Video-Upload-Service

Der `video`-Parameter braucht eine öffentlich erreichbare URL. Für lokale Dateien zuerst hochladen, um eine temporäre URL zu bekommen.

### POST /v2/video/upload

Fordert eine presigned Upload-URL an.

**Request:**
```json
{ "media_type": "video/mp4" }
```

| Parameter | Typ | Pflicht | Beschreibung |
|-----------|------|----------|-------------|
| `media_type` | string | Nein | MIME-Typ (z. B. `video/mp4`). Default `video/`, wenn weggelassen |

**Response:**
```json
{
  "result": {
    "upload_url": "https://...",
    "upload_fields": { "key": "...", "policy": "...", "signature": "..." },
    "file_url": "https://..."
  }
}
```

| Feld | Gültigkeit | Zweck |
|-------|----------|---------|
| `upload_url` | 1 Stunde | Presigned-POST-Endpoint für den Datei-Upload |
| `upload_fields` | 1 Stunde | Formularfelder, die mit dem Upload mitgeschickt werden müssen |
| `file_url` | 1 Tag | URL für `video` an Editing-Endpoints |

### Datei hochladen

POST an `upload_url` als `multipart/form-data`. **Alle `upload_fields` müssen vor dem Dateifeld stehen** — Validierung ist sequenziell, die Datei muss zuletzt kommen. Erfolg liefert HTTP 204 No Content.

**Limits:** nur Videodateien, max. 1 GB. Behandle `upload_url` und `file_url` als Secrets — sie sind unauthentifiziert.

---

## Status-Polling

### GET /v2/status/{request_id}

Pollt auf Abschluss eines Async-Jobs. Der `bria_video_call`-Helper übernimmt das automatisch.

**Response:**
```json
{
  "status": "IN_PROGRESS | COMPLETED | ERROR",
  "result": {
    "video_url": "https://...webm"
  },
  "request_id": "uuid"
}
```

**Status-Werte:**
- `IN_PROGRESS` — noch in Bearbeitung, erneut pollen
- `COMPLETED` — Ergebnis bereit, `video_url` enthält das verarbeitete Video
- `ERROR` / `FAILED` — Verarbeitung fehlgeschlagen, ein Error-Objekt wird geliefert
- `UNKNOWN` — unerwarteter interner Fehler (entspricht HTTP 500)

**HTTP-Codes:** `200` Standard-Response (unabhängig vom Job-Erfolg), `404` Request-ID existiert nicht oder abgelaufen, `5XX` interner Fehler des Status-Service.

**Polling-Strategie:** 5-Sekunden-Intervalle, bis zu 120 Versuche (max. 10 Minuten — Video-Jobs dauern länger als Bild-Jobs; kurze Clips fertig in ~30–60s). Der `bria_video_call`-Helper setzt das automatisch um; überschreibbar mit den Umgebungsvariablen `BRIA_POLL_INTERVAL` und `BRIA_POLL_ATTEMPTS`.
