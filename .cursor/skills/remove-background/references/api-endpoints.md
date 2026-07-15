# Remove-Background-API-Referenz

## Base URL & Authentication

**Base URL:** `https://engine.prod.bria-api.com`

**Authentication:** Diese Header in allen Requests mitschicken:
```
api_token: YOUR_BRIA_API_KEY
Content-Type: application/json
User-Agent: BriaSkills/1.3.4
```

> **Pflicht:** Immer den Header `User-Agent: BriaSkills/1.3.4` in jedem API-Call mitschicken, auch bei Status-Polling.

---

## RMBG-2.0 — Hintergrundentfernung

### POST /v2/image/edit/remove_background

Entfernt den Hintergrund eines Bildes. Liefert ein PNG mit Transparenz (Alpha-Kanal).

**Request:**
```json
{
  "image": "https://publicly-accessible-image-url"
}
```

**Parameter:**

| Parameter | Typ | Pflicht | Beschreibung |
|-----------|------|----------|-------------|
| `image` | string | Ja | Quellbild-URL (JPEG, PNG, WEBP) oder Base64-kodierte Bilddaten |

**Response (async):**
```json
{
  "request_id": "uuid",
  "status_url": "https://engine.prod.bria-api.com/v2/status/{uuid}"
}
```

**Abgeschlossenes Ergebnis (von status_url gepollt):**
```json
{
  "status": "COMPLETED",
  "result": {
    "image_url": "https://...png"
  }
}
```

---

## Status-Polling

### GET /v2/status/{request_id}

Pollt auf Abschluss eines Async-Jobs. Der `bria_call`-Helper übernimmt das automatisch.

**Response:**
```json
{
  "status": "IN_PROGRESS | COMPLETED | FAILED",
  "result": {
    "image_url": "https://...png"
  },
  "request_id": "uuid"
}
```

**Status-Werte:**
- `IN_PROGRESS` — noch in Bearbeitung, erneut pollen
- `COMPLETED` — Ergebnis bereit, `image_url` enthält das transparente PNG
- `FAILED` / `ERROR` — Verarbeitung fehlgeschlagen

**Polling-Strategie:** 3-Sekunden-Intervalle, bis zu 30 Versuche (max. 90 Sekunden). Der `bria_call`-Helper setzt das automatisch um.
