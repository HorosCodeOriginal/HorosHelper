# Backup & Wiederherstellung

**Sidebar:** Backup · **Mockup:** `assets/images/dashboard-mockup-11/`

## Zweck

Wiederherstellungspunkte verwalten, Backup-Profile mit Zeitplan und optionaler Verschlüsselung.

## Kern-Services

- `IBackupService`, `IRestorePointService`, `IBackupSchedulerService`
- `IBackupEncryptionService` — AES-256, DPAPI-Master-Key
- Headless: `HorosHelp.App --backup-run <profileId>`

## Daten

- Profile: `%AppData%\HorosHelper\backup-profiles.json`
- Manifest: SHA-256 für inkrementelle Backups

## Tests

- `BackupServiceTests`, `RestorePointServiceTests`, `BackupSchedulerServiceTests`
