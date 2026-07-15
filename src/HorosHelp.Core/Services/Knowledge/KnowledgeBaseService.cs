using HorosHelp.Core.Models.Knowledge;
using Microsoft.Extensions.Logging;

namespace HorosHelp.Core.Services.Knowledge;

public sealed class KnowledgeBaseService : IKnowledgeBaseService
{
    private readonly ILogger<KnowledgeBaseService> _logger;
    private static readonly IReadOnlyList<KnowledgeCategory> Categories =
    [
        new() { Id = "system", Name = "System", IconGlyph = "▣" },
        new() { Id = "netzwerk", Name = "Netzwerk", IconGlyph = "⊕" },
        new() { Id = "sicherheit", Name = "Sicherheit", IconGlyph = "⊗" },
        new() { Id = "apps", Name = "Apps", IconGlyph = "⊞" },
    ];

    private static readonly IReadOnlyList<KnowledgeArticle> Articles = BuildArticles();

    public KnowledgeBaseService(ILogger<KnowledgeBaseService> logger)
    {
        _logger = logger;
    }

    public IReadOnlyList<KnowledgeCategory> GetCategories() => Categories;

    public IReadOnlyList<KnowledgeArticle> GetArticles(string? categoryId = null, string? searchQuery = null)
    {
        IEnumerable<KnowledgeArticle> query = Articles;

        if (!string.IsNullOrWhiteSpace(categoryId))
            query = query.Where(a => a.CategoryId.Equals(categoryId, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            var tokens = searchQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            query = query.Where(a => tokens.All(t => Matches(a, t)));
        }

        var result = query.ToList();
        _logger.LogDebug("Knowledge search returned {Count} articles (category={Category}, query={Query})",
            result.Count, categoryId ?? "*", searchQuery ?? "");

        return result;
    }

    public KnowledgeArticle? GetArticle(string articleId) =>
        Articles.FirstOrDefault(a => a.Id.Equals(articleId, StringComparison.OrdinalIgnoreCase));

    private static bool Matches(KnowledgeArticle article, string token)
    {
        var comparison = StringComparison.OrdinalIgnoreCase;
        return article.Title.Contains(token, comparison)
               || article.Subtitle.Contains(token, comparison)
               || article.Description.Contains(token, comparison);
    }

    private static IReadOnlyList<KnowledgeArticle> BuildArticles() =>
    [
        // Netzwerk
        new()
        {
            Id = "wlan-settings",
            CategoryId = "netzwerk",
            Title = "WLAN-Einstellungen öffnen",
            Subtitle = "Verbinden und Verwalten von WLAN-Netzwerken",
            Description = "Öffnen Sie die WLAN-Einstellungen, um verfügbare Netzwerke anzuzeigen, Verbindungen herzustellen oder zu verwalten.",
            DeepLink = "ms-settings:network-wifi",
            IsFavorite = true,
            Steps =
            [
                new() { Number = 1, Text = "Klicken Sie auf das Startmenü (Windows-Symbol)." },
                new() { Number = 2, Text = "Wählen Sie „Einstellungen“ (Zahnrad-Symbol)." },
                new() { Number = 3, Text = "Klicken Sie auf „Netzwerk & Internet“." },
                new() { Number = 4, Text = "Wählen Sie „WLAN“ im linken Menü." },
                new() { Number = 5, Text = "Schalten Sie WLAN ein, falls es deaktiviert ist." },
                new() { Number = 6, Text = "Wählen Sie ein verfügbares Netzwerk aus der Liste." },
                new() { Number = 7, Text = "Klicken Sie auf „Verbinden“ und geben Sie das Passwort ein, falls erforderlich." },
            ],
            Tip = "Vergessenes Netzwerk? Klicken Sie auf das Netzwerk und wählen Sie „Vergessen“, um es zu entfernen.",
        },
        new()
        {
            Id = "dns-settings",
            CategoryId = "netzwerk",
            Title = "DNS-Server ändern",
            Subtitle = "Eigene DNS-Server konfigurieren",
            Description = "Konfigurieren Sie benutzerdefinierte DNS-Server für eine stabilere Namensauflösung.",
            DeepLink = "ms-settings:network-ethernet",
            IsFavorite = true,
            Steps =
            [
                new() { Number = 1, Text = "Öffnen Sie „Einstellungen“ → „Netzwerk & Internet“." },
                new() { Number = 2, Text = "Wählen Sie Ihre aktive Verbindung (WLAN oder Ethernet)." },
                new() { Number = 3, Text = "Klicken Sie auf „DNS-Serverzuweisung“." },
                new() { Number = 4, Text = "Wählen Sie „Manuell“ und tragen Sie die DNS-Adressen ein." },
            ],
            Tip = "Öffentliche DNS-Server wie 1.1.1.1 oder 8.8.8.8 können bei Ausfällen helfen.",
        },
        new()
        {
            Id = "firewall-settings",
            CategoryId = "netzwerk",
            Title = "Firewall konfigurieren",
            Subtitle = "Windows Defender Firewall anpassen",
            Description = "Steuern Sie, welche Apps über die Firewall kommunizieren dürfen.",
            DeepLink = "ms-settings:windowsdefender-firewall",
            Steps =
            [
                new() { Number = 1, Text = "Öffnen Sie „Einstellungen“ → „Datenschutz & Sicherheit“." },
                new() { Number = 2, Text = "Wählen Sie „Windows-Sicherheit“ → „Firewall & Netzwerkschutz“." },
                new() { Number = 3, Text = "Passen Sie Profile für Domäne, privat und öffentlich an." },
            ],
            Tip = "Lassen Sie die Firewall aktiviert, auch in privaten Netzwerken.",
        },
        new()
        {
            Id = "network-profile",
            CategoryId = "netzwerk",
            Title = "Netzwerkprofil ändern",
            Subtitle = "Öffentliches oder privates Netzwerk",
            Description = "Legen Sie fest, ob Windows Ihr Netzwerk als öffentlich oder privat behandelt.",
            DeepLink = "ms-settings:network-status",
            Steps =
            [
                new() { Number = 1, Text = "Öffnen Sie „Einstellungen“ → „Netzwerk & Internet“." },
                new() { Number = 2, Text = "Klicken Sie auf „Eigenschaften“ Ihrer Verbindung." },
                new() { Number = 3, Text = "Wählen Sie „Öffentlich“ oder „Privat“." },
            ],
            Tip = "In öffentlichen Netzwerken sollte die Erkennung ausgeschaltet bleiben.",
        },
        new()
        {
            Id = "network-reset",
            CategoryId = "netzwerk",
            Title = "Netzwerkverbindung zurücksetzen",
            Subtitle = "Zurücksetzen aller Netzwerkkomponenten",
            Description = "Setzt Netzwerkadapter und Protokolle auf Werkseinstellungen zurück.",
            DeepLink = "ms-settings:network-status",
            Steps =
            [
                new() { Number = 1, Text = "Öffnen Sie „Einstellungen“ → „Netzwerk & Internet“ → „Erweiterte Netzwerkeinstellungen“." },
                new() { Number = 2, Text = "Wählen Sie „Netzwerk zurücksetzen“." },
                new() { Number = 3, Text = "Bestätigen Sie den Neustart des PCs." },
            ],
            Tip = "Speichern Sie VPN-Profile vor dem Zurücksetzen.",
        },
        new()
        {
            Id = "proxy-settings",
            CategoryId = "netzwerk",
            Title = "Proxy-Einstellungen",
            Subtitle = "Proxy-Server manuell einrichten",
            Description = "Konfigurieren Sie einen Proxy für Firmen- oder Schulnetzwerke.",
            DeepLink = "ms-settings:network-proxy",
            Steps =
            [
                new() { Number = 1, Text = "Öffnen Sie „Einstellungen“ → „Netzwerk & Internet“ → „Proxy“." },
                new() { Number = 2, Text = "Aktivieren Sie „Proxyserver manuell einrichten“." },
                new() { Number = 3, Text = "Tragen Sie Adresse und Port ein." },
            ],
            Tip = "Automatische Erkennung funktioniert in vielen Unternehmensnetzen.",
        },
        new()
        {
            Id = "adapter-driver",
            CategoryId = "netzwerk",
            Title = "Netzwerkadapter aktualisieren",
            Subtitle = "Treiber für Netzwerkadapter aktualisieren",
            Description = "Aktualisieren Sie den Netzwerktreiber über den Geräte-Manager.",
            DeepLink = "ms-settings:devices",
            Steps =
            [
                new() { Number = 1, Text = "Rechtsklick auf Start → „Geräte-Manager“." },
                new() { Number = 2, Text = "Erweitern Sie „Netzwerkadapter“." },
                new() { Number = 3, Text = "Rechtsklick auf den Adapter → „Treiber aktualisieren“." },
            ],
            Tip = "Laden Sie Treiber vom Hersteller herunter, wenn Windows keine Updates findet.",
        },
        // System
        new()
        {
            Id = "display-settings",
            CategoryId = "system",
            Title = "Anzeige-Einstellungen",
            Subtitle = "Auflösung und Skalierung anpassen",
            Description = "Passen Sie Bildschirmauflösung, HDR und Skalierung an.",
            DeepLink = "ms-settings:display",
            Steps =
            [
                new() { Number = 1, Text = "Öffnen Sie „Einstellungen“ → „System“ → „Anzeige“." },
                new() { Number = 2, Text = "Wählen Sie die gewünschte Auflösung." },
                new() { Number = 3, Text = "Stellen Sie die Skalierung für Lesbarkeit ein." },
            ],
            Tip = "Empfohlene Einstellungen nutzen meist die native Panel-Auflösung.",
        },
        new()
        {
            Id = "sound-settings",
            CategoryId = "system",
            Title = "Sound-Einstellungen",
            Subtitle = "Ausgabegerät und Lautstärke",
            Description = "Wählen Sie Lautsprecher, Kopfhörer und Standardgeräte.",
            DeepLink = "ms-settings:sound",
            Steps =
            [
                new() { Number = 1, Text = "Öffnen Sie „Einstellungen“ → „System“ → „Sound“." },
                new() { Number = 2, Text = "Wählen Sie das Ausgabegerät." },
                new() { Number = 3, Text = "Testen Sie die Wiedergabe mit „Gerät testen“." },
            ],
            Tip = "Bluetooth-Geräte müssen zuerst gekoppelt sein.",
        },
        new()
        {
            Id = "power-settings",
            CategoryId = "system",
            Title = "Energieoptionen",
            Subtitle = "Energiesparmodus und Bildschirmzeit",
            Description = "Steuern Sie Ruhezustand, Bildschirmabschaltung und Energiepläne.",
            DeepLink = "ms-settings:powersleep",
            Steps =
            [
                new() { Number = 1, Text = "Öffnen Sie „Einstellungen“ → „System“ → „Energie & Akku“." },
                new() { Number = 2, Text = "Wählen Sie einen Energiesparmodus." },
                new() { Number = 3, Text = "Passen Sie Bildschirm- und Ruhezeiten an." },
            ],
            Tip = "„Beste Leistung“ erhöht den Stromverbrauch auf Laptops.",
        },
        new()
        {
            Id = "storage-settings",
            CategoryId = "system",
            Title = "Speicher verwalten",
            Subtitle = "Festplattennutzung und Bereinigung",
            Description = "Sehen Sie Speicherverbrauch nach Kategorien und starten Sie die Speicherbereinigung.",
            DeepLink = "ms-settings:storagesense",
            Steps =
            [
                new() { Number = 1, Text = "Öffnen Sie „Einstellungen“ → „System“ → „Speicher“." },
                new() { Number = 2, Text = "Prüfen Sie die Kategorien (Apps, Temporär, …)." },
                new() { Number = 3, Text = "Aktivieren Sie optional „Speicheroptimierung“." },
            ],
            Tip = "Temporäre Dateien können oft mehrere GB freigeben.",
        },
        // Sicherheit
        new()
        {
            Id = "windows-security",
            CategoryId = "sicherheit",
            Title = "Windows-Sicherheit öffnen",
            Subtitle = "Virenschutz und Bedrohungsschutz",
            Description = "Prüfen Sie Schutzstatus, Scans und Sicherheitsupdates.",
            DeepLink = "ms-settings:windowsdefender",
            Steps =
            [
                new() { Number = 1, Text = "Öffnen Sie „Einstellungen“ → „Datenschutz & Sicherheit“." },
                new() { Number = 2, Text = "Wählen Sie „Windows-Sicherheit“." },
                new() { Number = 3, Text = "Prüfen Sie „Viren- & Bedrohungsschutz“." },
            ],
            Tip = "Führen Sie bei Verdacht einen vollständigen Scan aus.",
        },
        new()
        {
            Id = "bitlocker",
            CategoryId = "sicherheit",
            Title = "Geräteverschlüsselung",
            Subtitle = "BitLocker und Festplattenverschlüsselung",
            Description = "Schützen Sie Daten bei Verlust oder Diebstahl des Geräts.",
            DeepLink = "ms-settings:deviceencryption",
            Steps =
            [
                new() { Number = 1, Text = "Öffnen Sie „Einstellungen“ → „Datenschutz & Sicherheit“ → „Geräteverschlüsselung“." },
                new() { Number = 2, Text = "Aktivieren Sie die Verschlüsselung, falls verfügbar." },
                new() { Number = 3, Text = "Sichern Sie den Wiederherstellungsschlüssel." },
            ],
            Tip = "Bewahren Sie den Wiederherstellungsschlüssel sicher auf.",
        },
        new()
        {
            Id = "privacy-settings",
            CategoryId = "sicherheit",
            Title = "Datenschutz-Einstellungen",
            Subtitle = "App-Berechtigungen und Telemetrie",
            Description = "Steuern Sie Kamera, Mikrofon, Standort und Diagnosedaten.",
            DeepLink = "ms-settings:privacy",
            Steps =
            [
                new() { Number = 1, Text = "Öffnen Sie „Einstellungen“ → „Datenschutz & Sicherheit“." },
                new() { Number = 2, Text = "Wählen Sie die gewünschte Berechtigungskategorie." },
                new() { Number = 3, Text = "Schalten Sie Zugriff pro App ein oder aus." },
            ],
            Tip = "Prüfen Sie regelmäßig Hintergrund-App-Berechtigungen.",
        },
        // Apps
        new()
        {
            Id = "installed-apps",
            CategoryId = "apps",
            Title = "Installierte Apps verwalten",
            Subtitle = "Apps deinstallieren oder reparieren",
            Description = "Entfernen Sie nicht benötigte Anwendungen und sparen Sie Speicher.",
            DeepLink = "ms-settings:appsfeatures",
            Steps =
            [
                new() { Number = 1, Text = "Öffnen Sie „Einstellungen“ → „Apps“ → „Installierte Apps“." },
                new() { Number = 2, Text = "Suchen Sie die App in der Liste." },
                new() { Number = 3, Text = "Wählen Sie „Deinstallieren“ oder „Erweiterte Optionen“." },
            ],
            Tip = "Einige System-Apps lassen sich nicht entfernen.",
        },
        new()
        {
            Id = "default-apps",
            CategoryId = "apps",
            Title = "Standard-Apps festlegen",
            Subtitle = "Browser, E-Mail und Medienplayer",
            Description = "Legen Sie fest, welche Apps Standardaufgaben übernehmen.",
            DeepLink = "ms-settings:defaultapps",
            Steps =
            [
                new() { Number = 1, Text = "Öffnen Sie „Einstellungen“ → „Apps“ → „Standard-Apps“." },
                new() { Number = 2, Text = "Wählen Sie die Kategorie (Browser, E-Mail, …)." },
                new() { Number = 3, Text = "Setzen Sie die gewünschte Standard-App." },
            ],
            Tip = "„Nach Dateityp“ erlaubt feinere Zuordnungen.",
        },
        new()
        {
            Id = "startup-apps",
            CategoryId = "apps",
            Title = "Autostart-Apps verwalten",
            Subtitle = "Programme beim Windows-Start",
            Description = "Reduzieren Sie Autostart-Programme für schnelleren Systemstart.",
            DeepLink = "ms-settings:startupapps",
            Steps =
            [
                new() { Number = 1, Text = "Öffnen Sie „Einstellungen“ → „Apps“ → „Autostart“." },
                new() { Number = 2, Text = "Deaktivieren Sie nicht benötigte Apps." },
                new() { Number = 3, Text = "Starten Sie den PC neu und prüfen Sie die Startzeit." },
            ],
            Tip = "Weniger Autostart-Einträge bedeuten oft spürbar schnelleren Boot.",
        },
    ];
}
