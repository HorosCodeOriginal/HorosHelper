# UI-Komponenten-Referenz

Vollständige Referenz für 60 UI-Komponenten mit Best Practices, typischen Layouts und Aliasen.
Quelle: [component.gallery](https://component.gallery), angereichert mit produktionsreifer Guidance.

---

## Inhalt

- [Accordion](#accordion)
- [Alert](#alert)
- [Avatar](#avatar)
- [Badge](#badge)
- [Breadcrumbs](#breadcrumbs)
- [Button](#button)
- [Button group](#button-group)
- [Card](#card)
- [Carousel](#carousel)
- [Checkbox](#checkbox)
- [Color picker](#color-picker)
- [Combobox](#combobox)
- [Date input](#date-input)
- [Datepicker](#datepicker)
- [Drawer](#drawer)
- [Dropdown menu](#dropdown-menu)
- [Empty state](#empty-state)
- [Fieldset](#fieldset)
- [File](#file)
- [File upload](#file-upload)
- [Footer](#footer)
- [Form](#form)
- [Header](#header)
- [Heading](#heading)
- [Hero](#hero)
- [Icon](#icon)
- [Image](#image)
- [Label](#label)
- [Link](#link)
- [List](#list)
- [Modal](#modal)
- [Navigation](#navigation)
- [Pagination](#pagination)
- [Popover](#popover)
- [Progress bar](#progress-bar)
- [Progress indicator](#progress-indicator)
- [Quote](#quote)
- [Radio button](#radio-button)
- [Rating](#rating)
- [Rich text editor](#rich-text-editor)
- [Search input](#search-input)
- [Segmented control](#segmented-control)
- [Select](#select)
- [Separator](#separator)
- [Skeleton](#skeleton)
- [Skip link](#skip-link)
- [Slider](#slider)
- [Spinner](#spinner)
- [Stack](#stack)
- [Stepper](#stepper)
- [Table](#table)
- [Tabs](#tabs)
- [Text input](#text-input)
- [Textarea](#textarea)
- [Toast](#toast)
- [Toggle](#toggle)
- [Tooltip](#tooltip)
- [Tree view](#tree-view)
- [Video](#video)
- [Visually hidden](#visually-hidden)

---

## Accordion

**Auch bekannt als:** Arrow toggle  ·  Collapse  ·  Collapsible sections  ·  Collapsible  ·  Details  ·  Disclosure  ·  Expandable  ·  Expander

Ein vertikal gestapelter Satz aufklappbarer Abschnitte — jede Überschrift schaltet zwischen kurzem Label und dem vollständigen Inhalt darunter.

**Best Practices:**
- Für Long-Form-Content mit progressive disclosure nutzen
- Überschriften knapp und scannbar halten — sie sind die primäre Navigation
- Mehrere Abschnitte gleichzeitig offen erlauben, außer Platz ist kritisch knapp
- Dezentes Expand/Collapse-Icon (Chevron) konsistent rechts ausrichten
- Öffnen/Schließen mit kurzer ease-out-Transition (150–250 ms) animieren
- Tastaturnavigation sicherstellen: Enter/Leertaste toggelt, Pfeiltasten zwischen Headern

**Typische Layouts:**
- FAQ-Seite mit gestapelten Frage/Antwort-Paaren
- Settings-Panel mit gruppierten Präferenz-Abschnitten
- Sidebar-Filtergruppen in E-Commerce oder Dashboards
- Mobile Navigation mit aufklappbaren Menüabschnitten

---

## Alert

**Auch bekannt als:** Notification  ·  Feedback  ·  Message  ·  Banner  ·  Callout

Eine hervorgehobene Meldung für wichtige Informationen oder Statusänderungen.

**Best Practices:**
- Semantische Farbcodierung: Rot für Fehler, Amber für Warnungen, Grün für Erfolg, Blau für Info
- Klare, handlungsorientierte Nachricht — nicht nur ein Status-Label
- Schließen-Aktion für nicht-kritische Alerts
- Inline-Alerts nah am relevanten Content, nicht willkürlich schwebend
- Icon neben Farbe für Barrierefreiheit bei Farbenblindheit
- Alert-Text auf ein bis zwei Sätze begrenzen

**Typische Layouts:**
- Banner oben auf der Seite für systemweite Ankündigungen
- Inline-Validierungsmeldung unter einem Eingabefeld
- Toast-Stack unten rechts
- Kontextuelle Warnung in Card oder Settings-Abschnitt

---

## Avatar

Visuelle Darstellung eines Users — typisch Foto, Illustration oder Initialen.

**Best Practices:**
- Drei Größen: klein (24–32 px), mittel (40–48 px), groß (64–80 px)
- Graceful Fallback: Bild → Initialen → generisches Icon
- Dezenter Ring oder Border zur Abgrenzung vom Hintergrund
- Bei Gruppen Avatare leicht überlappend stapeln mit „+N“-Overflow
- Bild lazy laden mit Placeholder-Shimmer

**Typische Layouts:**
- User-Profil-Header mit Name und Rolle
- Kommentar-Thread mit Avatar neben jeder Nachricht
- Teamliste mit gestapelter Avatar-Gruppe
- User-Menü-Trigger in der Navigationsleiste

---

## Badge

**Auch bekannt als:** Tag  ·  Label  ·  Chip

Ein kompaktes Label innerhalb oder nahe einer größeren Komponente für Status, Kategorie oder Metadaten.

**Best Practices:**
- Badge-Text auf ein bis zwei Wörter — Labels, keine Sätze
- Begrenzte Badge-Farbpalette mit klarer Semantik
- Ausreichender Kontrast zwischen Badge-Text und Hintergrund (WCAG-AA-Minimum)
- Pill-Form (voll abgerundet) für Status-Badges, abgerundete Rechtecke für Tags
- Badges nicht überstrapazieren — wenn alles badged ist, sticht nichts hervor

**Typische Layouts:**
- Status-Indikator in Tabellenzeile (Active, Pending, Archived)
- Tag-Cloud unter Blogpost oder Produkt-Card
- Benachrichtigungszähler am Nav-Icon
- Feature-Label auf Pricing-Tier-Card

---

## Breadcrumbs

**Auch bekannt als:** Breadcrumb trail

Eine Link-Kette, die zeigt, wo die aktuelle Seite in der Navigationshierarchie liegt.

**Best Practices:**
- Vollen Hierarchiepfad zeigen; mittlere Segmente auf Mobile per Ellipsis-Menü kürzen
- Aktuelle Seite als letztes Element, kein Link
- Dezenter Separator (/ oder ›) mit ausreichend Abstand
- Breadcrumbs oben im Content-Bereich, unter dem Header
- Breadcrumb-Text lowercase oder Sentence-Case für Lesbarkeit

**Typische Layouts:**
- E-Commerce Kategorie → Unterkategorie → Produktseite
- Dokumentations-Site Abschnittsnavigation
- Dashboard Drill-down von Übersicht zu Detail
- Pfadanzeige im Dateimanager

---

## Button

Interaktives Steuerelement für Aktionen — Formular absenden, Dialog öffnen, Sichtbarkeit umschalten.

**Best Practices:**
- Klare visuelle Hierarchie: Primary (gefüllt), Secondary (Outline), Tertiary (nur Text)
- Verb-first-Labels: „Änderungen speichern“, „Projekt erstellen“, nicht „Okay“ oder „Absenden“
- Mindest-Touch-Target 44×44 px; Desktop-Buttons mindestens 36 px hoch
- Loading-Spinner im Button bei async Aktionen — deaktivieren gegen Doppelklicks
- Maximal ein Primary-Button pro sichtbarem Viewport-Abschnitt
- Fokus-Ring sichtbar und kontrastreich für Tastaturnutzer

**Typische Layouts:**
- Formular-Footer: Primary rechts, Secondary links
- Hero-CTA zentriert oder links unter der Headline
- Dialog-Footer mit Abbrechen (Secondary) und Bestätigen (Primary)
- FAB unten rechts für mobile Erstellungs-Flows

---

## Button group

**Auch bekannt als:** Toolbar

Container, der verwandte Buttons als eine visuelle Einheit gruppiert.

**Best Practices:**
- Nur verwandte Aktionen gruppieren — unverwandte Buttons trennen
- Buttons visuell verbinden mit gemeinsamer Border oder engem Abstand (1–2 px)
- Active/Selected-State in Toggle-Gruppen klar anzeigen
- Gruppe auf 2–5 Buttons; mehr Optionen → Dropdown oder Overflow-Menü

**Typische Layouts:**
- Texteditor-Toolbar (fett, kursiv, unterstrichen)
- Ansichtswechsler (Raster, Liste)
- Segmentierter Datumsbereich (Tag, Woche, Monat)
- Split-Button mit Primary-Aktion und Dropdown für Alternativen

---

## Card

**Auch bekannt als:** Tile

Eigenständiger Content-Block für eine Entität wie Kontakt, Artikel oder Task.

**Best Practices:**
- Eine klare Hierarchie pro Card: Media → Titel → Meta → Aktion
- Konsistente Card-Höhe im Grid — Line-Clamping bei variablem Text
- Ganze Card klickbar, wenn sie eine navigierbare Entität darstellt
- Dezente Elevation (Schatten) oder Border — nicht beides gleichzeitig
- Card-Inhalt auf Wesentliches begrenzen; Detailseite trägt den Rest

**Typische Layouts:**
- Produkt-Grid mit Bild, Titel, Preis und CTA
- Blog-Feed mit Thumbnail, Headline, Auszug und Datum
- Dashboard-KPI-Cards mit Metrik, Delta und Sparkline
- Team-Verzeichnis mit Avatar, Name und Rolle

---

## Carousel

**Auch bekannt als:** Content slider

Komponente, die mehrere Content-Slides durchläuft — per Swipe, Scroll oder Buttons.

**Best Practices:**
- Sichtbare Navigationspfeile und Pagination-Dots
- Swipe-Gesten auf Touch-Geräten
- Auto-Advance nur ohne User-Interaktion; Pause bei Hover/Focus
- Vorschau des nächsten Slides als Scroll-Hinweis
- Slide-Anzahl überschaubar (3–7) — viele Slides = geringes Engagement
- Barrierefreiheit: jeder Slide per Tastatur erreichbar

**Typische Layouts:**
- Hero-Bild-Slideshow auf Marketing-Homepage
- Produktbild-Galerie auf Detailseite
- Testimonial-Carousel mit Zitat, Autor und Avatar
- Horizontal scrollende Feature-Highlights in Mobile-App

---

## Checkbox

Auswahlsteuerung — in Gruppen für Multi-Select aus einer Liste oder einzeln für eine On/Off-Wahl.

**Best Practices:**
- Checkboxen für Multi-Select, nicht einzelne Toggles (Switch für On/Off)
- Checkbox an erste Zeile des Labels ausrichten, nicht zentriert
- Indeterminate-State für „Alle auswählen“, wenn Kinder teilweise gewählt
- Mindestens 44 px Touch-Target inkl. Label-Bereich
- Verwandte Checkboxen unter Fieldset mit Legend für Barrierefreiheit

**Typische Layouts:**
- Filter-Panel mit Multi-Select-Facetten
- AGB-Einzelcheckbox mit langem Label
- To-do-Liste mit Check/Uncheck pro Item
- Tabellenzeilen-Multi-Select mit Header „Alle auswählen“

---

## Color picker

Steuerung zur Farbauswahl.

**Best Practices:**
- Spektrum-Picker, Hue-Slider und direkte Hex/RGB-Eingabe
- Preset-Swatches für schnelle Auswahl
- Echtzeit-Vorschau der gewählten Farbe
- Copy-Paste von Hex/RGB/HSL-Werten
- Zuletzt genutzte Farben merken

**Typische Layouts:**
- Design-Tool-Farbpicker mit Spektrum, Slidern und Eingabefeldern
- Theme-Customizer mit Preset-Palette und Custom-Override
- Annotations-Tool mit Farbswatch-Zeile
- Brand-Settings mit Primary/Secondary/Accent-Farbpickern

---

## Combobox

**Auch bekannt als:** Autocomplete  ·  Autosuggest

Select-ähnliches Input mit Freitextfeld, das Optionen beim Tippen filtert.

**Best Practices:**
- Vorschläge nach 1–2 Zeichen, um Rauschen zu reduzieren
- Treffertext in Vorschlägen hervorheben
- Tastaturnavigation (Pfeiltasten + Enter) im Dropdown
- „Keine Ergebnisse“ statt leerem Dropdown
- Input debouncen gegen übermäßige API-Calls (200–300 ms)

**Typische Layouts:**
- Suchleiste mit Autocomplete-Vorschlägen
- Adresseingabe mit Ortsvorschlägen
- Tag-Input mit Vorschlägen bestehender Tags
- Assignee-Picker in Projektmanagement-Tool

---

## Date input

Datumseingabe, oft in separate Tag-/Monat-/Jahr-Felder.

**Best Practices:**
- Erwartetes Format klar labeln (DD/MM/YYYY oder MM/DD/YYYY)
- Separate Felder für Tag, Monat, Jahr für eindeutige Eingabe
- Echtzeit-Validierung mit Inline-Fehlern
- Auto-Advance zwischen Feldern bei vollständigem Segment

**Typische Layouts:**
- Geburtsdatum in Registrierungsformular
- Pass-/ID-Ablaufdatum
- Rechnungsdatum in Finanzformular

---

## Datepicker

**Auch bekannt als:** Calendar  ·  Datetime picker

Kalenderbasierte Steuerung zur visuellen Datumsauswahl.

**Best Practices:**
- Manuelle Texteingabe und Kalenderauswahl
- Erwartetes Datumsformat klar angeben (z. B. MM/DD/YYYY)
- Heutiges und gewähltes Datum hervorheben
- Daten außerhalb des gültigen Bereichs deaktivieren
- Tastaturnavigation im Kalender-Grid
- Bei Datumsbereichen Start und Ende in verbundenem Picker

**Typische Layouts:**
- Buchungsflow mit Check-in/Check-out-Bereichspicker
- Formularfeld mit Kalender-Dropdown bei Focus
- Dashboard-Datumsbereichsfilter in Toolbar
- Event-Erstellung mit Startdatum und optionalem Enddatum

---

## Drawer

**Auch bekannt als:** Tray  ·  Flyout  ·  Sheet

Panel, das von einer Bildschirmkante einschiebt für sekundären Content oder Aktionen.

**Best Practices:**
- Drawer für sekundären Content oder fokussierte Subtasks ohne eigene Seite
- Von rechts für Detail-Panels, von links für Navigation
- Klaren Schließen-Button und Escape zum Schließen
- Hintergrund mit halbtransparentem Overlay abdunkeln
- Breite 320–480 px auf Desktop; volle Breite auf Mobile

**Typische Layouts:**
- Mobile Navigationsmenü von links
- Warenkorb-Vorschau von rechts
- Detail/Edit-Panel in Master-Detail-Layout
- Notification Center von rechts

---

## Dropdown menu

**Auch bekannt als:** Select menu

Menü per Button mit Aktions- oder Navigationsoptionen — kein Formular-Input wie Select.

**Best Practices:**
- Verwandte Items mit Separatoren und optionalen Gruppenüberschriften
- Tastatur: Pfeile bewegen, Enter wählen, Escape schließen
- Menü auf 7±2 Items; Submenüs oder Suche bei längeren Listen
- Menü positionieren gegen Viewport-Overflow — nach oben flippen am unteren Rand
- Destruktive Aktionen rot und zuletzt, getrennt

**Typische Layouts:**
- User-Account-Menü oben rechts
- Kontextmenü per Rechtsklick oder Kebab-Icon
- Aktionsmenü in Tabellenzeile (Bearbeiten, Duplizieren, Löschen)
- Sortier-/Filter-Dropdown in Toolbar

---

## Empty state

Platzhalter ohne Daten in einer View, typisch mit hilfreicher Aktion oder Vorschlag.

**Best Practices:**
- Klare Illustration oder Icon gegen leeres Gefühl
- Hilfreiche Headline zum Empty State
- Primary-CTA zum nächsten Schritt
- Keine Schuldzuweisung — positiv formulieren („Noch keine Projekte“, nicht „Du hast keine Projekte“)
- Empty State im Container, nicht als Full-Page-Takeover

**Typische Layouts:**
- Leeres Dashboard mit CTA „Erstes Projekt erstellen“
- Suchergebnisseite mit „Keine Ergebnisse“ und Vorschlägen
- Leerer Posteingang mit Illustration und ermutigender Nachricht
- Leere Tabelle mit Inline-Hinweis zum Hinzufügen

---

## Fieldset

Container für verwandte Formularfelder unter gemeinsamer Legend.

**Best Practices:**
- Fieldsets für verwandte Felder unter beschreibender Legend
- Legend als Abschnittsüberschrift im Formular
- Fieldset von Screenreadern für Kontext ankündigen

**Typische Layouts:**
- Adressabschnitt mit Straße, Stadt, Bundesland, PLZ
- Zahlungsabschnitt mit Kartennummer, Ablauf, CVV
- Persönliche Daten in mehrteiligem Formular

---

## File

**Auch bekannt als:** Attachment  ·  Download

Visuelle Darstellung einer Datei — Anhang oder Download-Dokument.

**Best Practices:**
- Dateityp-Icon, Name und Größe klar zeigen
- Download-Aktion und optional Vorschau
- Upload- oder Änderungsdatum anzeigen
- Fortschrittsanzeige beim Upload

**Typische Layouts:**
- Anhangsliste unter Nachricht oder Formular
- Datei-Card mit Icon, Name, Größe, Download-Button
- Dokument-Grid mit Thumbnails und Metadaten

---

## File upload

**Auch bekannt als:** File input  ·  File uploader  ·  Dropzone

Steuerung zum Auswählen und Hochladen von Dateien vom Gerät.

**Best Practices:**
- Drag-and-Drop mit klar definierter Drop-Zone
- Akzeptierte Typen und Größenlimits vor Upload
- Upload-Fortschritt pro Datei mit Progress Bar
- Abbruch laufender Uploads erlauben
- Vorschau nach Auswahl (Thumbnail für Bilder, Icon + Name für Dokumente)
- Dateityp und -größe clientseitig vor Upload validieren

**Typische Layouts:**
- Profilfoto-Upload mit runder Crop-Vorschau
- Dokument-Anhangsbereich im Formular
- Multi-File-Drop-Zone mit Dateiliste darunter
- Inline-Dateifeld mit Durchsuchen-Button und Dateiname

---

## Footer

Bereich unten auf Seite oder Abschnitt mit Copyright, Legal-Links oder Sekundärnavigation.

**Best Practices:**
- Links in klaren Spalten nach Kategorie
- Wesentliche Legal-Links: Datenschutz, AGB
- Footer visuell abgesetzt, nicht ablenkend — gedämpfter Hintergrund
- Social Links und Newsletter-Anmeldung wenn passend
- Footer barrierefrei, Links per Tastatur

**Typische Layouts:**
- Mehrspaltiger Footer mit Linkgruppen, Logo, Copyright
- Minimaler SaaS-Footer mit Produktlinks und Social-Icons
- E-Commerce-Footer mit Hilfe, Versand, Retouren, Zahlungsicons
- Einzeiliger Footer mit Copyright und Legal-Links

---

## Form

Sammlung von Eingabesteuerungen zum Erfassen und Absenden strukturierter Daten.

**Best Practices:**
- Einspaltiges Layout für die meisten Formulare — schneller zu scannen
- Labels über Inputs für mobile-freundliche Formulare
- Verwandte Felder durch Nähe und optionale Fieldset-Überschriften gruppieren
- Inline-Validierung on blur, nicht bei jedem Tastendruck
- Submit deaktivieren bis Pflichtfelder gültig, oder klare Fehler beim Absenden
- Formulare so kurz wie möglich — nur Nötiges abfragen

**Typische Layouts:**
- Anmeldeformular mit Name, E-Mail, Passwort und CTA
- Mehrstufiger Wizard mit Fortschrittsanzeige
- Settings-Formular mit gruppierten Präferenz-Abschnitten
- Kontaktformular mit Name, E-Mail, Betreff und Nachricht

---

## Header

Persistenter Bereich oben auf der Seite mit Brand, primärer Navigation und wichtigen Aktionen.

**Best Practices:**
- Header-Höhe kompakt (56–72 px) für Content-Platz
- Logo/Brand links, primäre Navigation mittig oder rechts
- Sticky Header auf langen Seiten, Auto-Hide beim Runterscrollen erwägen
- Mobile Header graceful in Hamburger-Menü
- Klare visuelle Trennung vom Content (Border-bottom oder dezenter Schatten)

**Typische Layouts:**
- SaaS-App-Header mit Logo, Nav-Links, Suche und User-Avatar
- Marketing-Site-Header mit Logo, Nav-Links und CTA
- Dashboard-Header mit Breadcrumbs, Seitentitel und Aktionsbuttons
- Minimaler Header mit zentriertem Logo und Hamburger-Menü

---

## Heading

Titel-Element, das einen Content-Abschnitt einführt und benennt.

**Best Practices:**
- Strikte Heading-Hierarchie (h1 → h2 → h3) für Barrierefreiheit und SEO
- Ein h1 pro Seite — Seitentitel
- Überschriften knapp und beschreibend — Gliederung des Contents
- Konsistente Größe, Gewicht und Abstand über Heading-Ebenen

**Typische Layouts:**
- Seitentitel (h1) mit Abschnitts- (h2) und Unterüberschriften (h3)
- Card-Titel als h3 innerhalb eines Seitenabschnitts
- Dashboard-Abschnittsheader zwischen Widget-Gruppen

---

## Hero

**Auch bekannt als:** Jumbotron  ·  Banner

Hervorgehobenes Banner oben auf der Seite, typisch mit Full-Width-Bild oder Illustration und Headline.

**Best Practices:**
- Überzeugende Headline — Klarheit vor Cleverness
- Ein Primary-CTA und optional ein Secondary-CTA
- Hochwertiges Bild oder Illustration, die die Botschaft verstärkt
- Textkontrast zum Hintergrundbild (Overlay oder sichere Textzone)
- Hero-Höhe proportional — zum Scrollen einladen, nicht Viewport dominieren

**Typische Layouts:**
- Split-Hero: Headline + CTA links, Produkt-Screenshot rechts
- Full-Bleed-Hintergrundbild mit zentriertem Text-Overlay
- Minimaler Hero mit großer Headline, Subtext und Inline-E-Mail-Erfassung
- Video-Hintergrund-Hero mit zentrierter Headline und Play-Button

---

## Icon

Kleines Grafiksymbol für Zweck oder Bedeutung eines UI-Elements auf einen Blick.

**Best Practices:**
- Konsistenter Icon-Stil im Produkt (Outline oder Filled, nicht gemischt)
- Icons an angrenzenden Text ausrichten (typisch 16–24 px)
- Icons mit Text-Labels paaren — Icon-only-Buttons brauchen Tooltips
- aria-hidden='true' für dekorative Icons, aria-label für funktionale

**Typische Layouts:**
- Navigationsitem mit Icon + Label
- Aktionsbutton mit Icon + Text („Bericht herunterladen“)
- Status-Icon neben Label (Check, Warning, Error)
- Icon-only-Toolbar mit Tooltips

---

## Image

**Auch bekannt als:** Picture

Komponente zur Einbettung von Bildern in einer Seite.

**Best Practices:**
- Immer sinnvollen Alt-Text für Barrierefreiheit
- Responsive Images (srcset) für passende Größen
- Bilder below the fold lazy laden für Performance
- Platz für Bilder vor dem Laden reservieren gegen Layout Shift
- Moderne Formate (WebP, AVIF) mit Fallbacks

**Typische Layouts:**
- Hero-Banner mit Full-Width-Hintergrundbild
- Produktbild-Galerie mit Thumbnails und Zoom
- Blog-Featured-Image über Titel oder unter Headline
- Avatar oder Profilfoto im runden Rahmen

---

## Label

**Auch bekannt als:** Form label

Text-Element, das ein Formular-Input identifiziert und beschreibt.

**Best Practices:**
- Labels immer mit Inputs verknüpfen (htmlFor / id)
- Labels über Input bei vertikalen Formularen, daneben bei horizontalen
- Pflichtfelder klar markieren (Stern oder „required“)
- Label-Text knapp — Hilfstext für zusätzliche Guidance

**Typische Layouts:**
- Formularfeld mit Label oben und Hilfstext unten
- Inline-Label neben Toggle oder Checkbox
- Floating Label, das bei Focus nach oben wandert

---

## Link

**Auch bekannt als:** Anchor  ·  Hyperlink

Klickbare Referenz auf eine andere Ressource — externe Seite oder Position im aktuellen Dokument.

**Best Practices:**
- Linktext beschreibend — „hier klicken“ oder „mehr erfahren“ nicht isoliert
- Links im Fließtext unterstreichen; Nav-Links können Kontext nutzen
- Farbe vom umgebenden Text unterscheiden (kein reines Blau bei Palette-Konflikt)
- Visited-State bei content-lastigen Seiten
- Externe Links: neuer Tab erkennbar (Icon oder aria-label)

**Typische Layouts:**
- Inline-Textlink im Absatz
- Eigenständiger Link unter Card/Abschnitt als „Weiterlesen“
- Footer-Link-Spalten für Site-Navigation
- Breadcrumb-Links im Hierarchiepfad

---

## List

Komponente, die verwandte Items in geordneter oder ungeordneter Sequenz gruppiert.

**Best Practices:**
- Konsistenter vertikaler Rhythmus — gleicher Abstand zwischen Items
- Bei interaktiven Listen: klare Hover- und Active-States pro Zeile
- Divider in dichten Listen; weglassen in luftigen
- Tastaturnavigation bei interaktiver Liste
- Virtualisierung (Windowing) ab ~100 Items

**Typische Layouts:**
- E-Mail-Posteingang mit Absender, Betreff, Vorschau, Zeitstempel pro Zeile
- Settings-Liste mit Label, Wert/Toggle, optionalem Chevron
- Activity-Feed mit Avatar, Beschreibung, relativem Zeitstempel
- Dateiliste mit Icon, Name, Größe, Datumsspalten

---

## Modal

**Auch bekannt als:** Dialog  ·  Popup  ·  Modal window

Overlay, das Aufmerksamkeit fordert — Interaktion nötig, bevor zum Content darunter zurückgekehrt wird.

**Best Practices:**
- Modals sparsam — nur bei sofortiger Aufmerksamkeit oder fokussierter Eingabe
- Klares Schließen: X-Button, Abbrechen, Escape
- Fokus im Modal fangen für Barrierefreiheit
- Fokus beim Schließen zum Trigger zurückgeben
- Modal-Inhalt knapp — bei Scroll-Bedarf lieber volle Seite
- Halbtransparentes Backdrop zum Abdunkeln

**Typische Layouts:**
- Bestätigungsdialog mit Nachricht und zwei Buttons
- Form-Modal für schnelle Dateneingabe (Erstellen, Bearbeiten)
- Bild-/Media-Vorschau-Lightbox
- Onboarding- oder Ankündigungs-Modal mit Illustration und CTA

---

## Navigation

**Auch bekannt als:** Nav  ·  Menu

Bereich mit Links zum Wechseln zwischen Seiten oder Abschnitten der aktuellen Seite.

**Best Practices:**
- Primäre Navigation auf 5–7 Items; Rest unter „Mehr“ oder Submenüs
- Aktuelle Seite in Navigation klar anzeigen
- Konsistente Iconografie neben Text-Labels
- Auf Mobile zu Hamburger oder Bottom-Tab-Bar
- Alle Nav-Items per Tastatur (Tab + Enter)

**Typische Layouts:**
- Horizontale Top-Nav mit Logo, Links und User-Menü
- Vertikale Sidebar mit Icon + Label und aufklappbaren Gruppen
- Bottom-Tab-Bar für Mobile Apps (Home, Suche, Erstellen, Benachrichtigungen, Profil)
- Mega-Menü-Dropdown mit kategorisierten Link-Spalten

---

## Pagination

Steuerung zum Navigieren zwischen Content-Seiten bei aufgeteilten Daten.

**Best Practices:**
- Erste, letzte und Fenster um aktuelle Seite
- Ellipsis für übersprungene Seiten, nicht Dutzende Seitenzahlen
- Zurück/Weiter zusätzlich zu nummerierten Seiten
- Aktuelle Seite klar als ausgewählt
- Infinite Scroll oder „Mehr laden“ für Feeds erwägen

**Typische Layouts:**
- Tabellen-Footer mit Seitenzahlen, Zeilen-pro-Seite, Gesamtanzahl
- Suchergebnis-Pagination zentriert unter Liste
- Blog-Archiv mit Zurück/Weiter
- API-Doku mit Seitensteuerung oben und unten

---

## Popover

Schwebendes Panel bei Klick nahe am Trigger — im Gegensatz zum Tooltip mit interaktivem Inhalt.

**Best Practices:**
- Per Klick triggern, nicht Hover — Touch und Barrierefreiheit
- Intelligent positionieren gegen Clipping am Viewport-Rand
- Dezenter Pfeil/Caret zum Trigger
- Schließen bei Klick außerhalb oder Escape
- Popover-Inhalt knapp — kein Modal

**Typische Layouts:**
- Farbpicker-Dropdown per Swatch
- User-Profil-Vorschau-Card bei Avatar-Hover/Klick
- Quick-Edit-Popover für Inline-Datenänderung
- Hilfe-Tooltip mit reichem Inhalt (Text + Link)

---

## Progress bar

**Auch bekannt als:** Progress

Horizontaler Indikator für den Fortschritt einer längeren Aufgabe.

**Best Practices:**
- Determinate Bar bei messbarem Fortschritt, indeterminate wenn unbekannt
- Prozent-Label für Barrierefreiheit und Klarheit
- Farbe für Zustand: Blau/Grün normal, Rot Fehler, Amber Warnung
- Sanft animieren — keine ruckartigen Sprünge
- Bar proportional zum Container (nicht zu dünn)

**Typische Layouts:**
- Upload-Fortschritt unter Dateiname
- Onboarding-Fortschrittsbalken in Sidebar oder Header
- Kurs-Fortschritt oben auf Lektionsseite
- Ressourcennutzung in Monitoring-Dashboard

---

## Progress indicator

**Auch bekannt als:** Progress tracker  ·  Stepper  ·  Steps  ·  Timeline  ·  Meter

Visuelle Anzeige des Fortschritts durch einen mehrstufigen Prozess.

**Best Practices:**
- Abgeschlossene, aktuelle und kommende Schritte klar unterscheiden
- Nummerierte oder beschriftete Schritte — nicht nur Dots
- Zurück zu abgeschlossenen Schritten erlauben, wenn Flow es zulässt
- Gesamtzahl der Schritte sichtbar
- Schritte auf Mobile vertikal stapeln

**Typische Layouts:**
- Mehrstufiger Checkout (Warenkorb → Versand → Zahlung → Bestätigung)
- Account-Setup-Wizard mit Profil, Präferenzen, Verifizierung
- Bewerbungsformular mit mehreren Abschnitten
- Projekt-Timeline mit Meilensteinen

---

## Quote

**Auch bekannt als:** Pull quote  ·  Block quote

Gestylter Block für Zitate — Person, externe Quelle oder hervorgehobene Passage.

**Best Practices:**
- Eigenes visuelles Treatment — große Anführungszeichen, linker Border oder Kursiv
- Zitat immer der Quelle zuordnen
- Pull Quotes kurz — Aufmerksamkeitsanker, keine Absätze

**Typische Layouts:**
- Testimonial-Block mit Foto, Zitat, Name, Titel
- Pull Quote im Blogpost zur Auflockerung
- Kundenzitat in Case Study mit Firmenlogo

---

## Radio button

**Auch bekannt als:** Radio  ·  Radio group

Auswahlsteuerung — genau eine Option aus vordefiniertem Set.

**Best Practices:**
- Radio Buttons für sich ausschließende Wahlen
- Sinnvollen Default vorauswählen wenn möglich
- Unter Fieldset mit beschreibender Legend gruppieren
- Vertikal bei mehr als 2 Optionen — horizontal nur bei 2–3 kurzen Labels
- Ausreichend Abstand zwischen Optionen (mind. 8 px)

**Typische Layouts:**
- Versandart (Standard, Express, Overnight)
- Zahlungsmethode mit Radio + Icon + Beschreibung
- Umfragefrage mit Einfachauswahl
- Plan/Tier-Auswahl im Pricing-Formular

---

## Rating

Steuerung für sternbasierte Bewertung eines Produkts oder Items.

**Best Practices:**
- 5-Sterne-Skala als Standard
- Halbe Sterne bei Anzeige; volle Sterne bei Eingabe
- Durchschnittsbewertung und Review-Anzahl zusammen
- Gefüllte/leere Sterne mit ausreichendem Kontrast

**Typische Layouts:**
- Produktbewertung mit Sternen und Review-Anzahl
- Review mit interaktiver Sterneingabe und Textarea
- Bewertungs-Zusammenfassung mit Verteilungsbalken

---

## Rich text editor

**Auch bekannt als:** RTE  ·  WYSIWYG editor

WYSIWYG-Oberfläche zum Erstellen und Formatieren von Rich Text.

**Best Practices:**
- Minimale Standard-Toolbar — erweiterte Formatierung on demand
- Tastaturkürzel für gängige Formatierung (Cmd+B, Cmd+I)
- Eingefügten Content sanitizen gegen layout-brechendes HTML
- Wort-/Zeichenzähler bei Limits

**Typische Layouts:**
- Blog-Editor mit Toolbar und Vorschau
- E-Mail-Composer mit Rich Text und Anhängen
- Kommentar-Editor mit Basisformatierung (fett, kursiv, Link, Liste)

---

## Search input

**Auch bekannt als:** Search

Textfeld für Suchanfragen zum Finden von Content.

**Best Practices:**
- Lupen-Icon im Feld als Zweck-Signal
- Cmd/Ctrl+K als globales Shortcut zum Fokussieren
- Letzte Suchen und Vorschläge im Dropdown
- Input debouncen und Loading bei Server-Queries
- Clear/Reset-Button (×) sobald Text eingegeben

**Typische Layouts:**
- Globale Suche in der Top-Nav
- Command-Palette (Cmd+K) mit kategorisierten Ergebnissen
- Inline-Suche/Filter über Datentabelle
- Full-Page-Suche mit prominentem Input und Ergebnissen darunter

---

## Segmented control

**Auch bekannt als:** Toggle button group

Kompakte Reihe sich ausschließender Optionen — Hybrid aus Button Groups, Radios und Tabs.

**Best Practices:**
- 2–5 Segmente — mehr Optionen → Tabs oder Dropdown
- Segmente mit gleicher Breite
- Auswahl-Indikator zwischen Optionen gleiten lassen
- Ausgewählter Zustand stark kontrastiert
- Sentence Case für Segment-Labels

**Typische Layouts:**
- Karten-/Listen-/Raster-Ansichtswechsler
- Abrechnungszeitraum (Monatlich / Jährlich)
- Hell/Dunkel-Modus in Settings
- Diagrammtyp (Linie, Balken, Kreis)

---

## Select

**Auch bekannt als:** Dropdown  ·  Select input

Formular-Input mit aktueller Auswahl eingeklappt und scrollbarer Optionsliste ausgeklappt.

**Best Practices:**
- Native Select für einfache Fälle (bessere Barrierefreiheit und Mobile-UX)
- Bei Custom Selects: volle Tastaturunterstützung und ARIA
- Placeholder („Option wählen…“) wenn kein Wert
- Lange Listen mit optgroups oder Überschriften
- Bei vielen Optionen Combobox-Verhalten kombinieren

**Typische Layouts:**
- Land/Region im Adressformular
- Sortieren-Dropdown in Produktlisten-Toolbar
- Rollen-Auswahl bei User-Einladung
- Sprach-/Locale-Umschalter

---

## Separator

**Auch bekannt als:** Divider  ·  Horizontal rule  ·  Vertical rule

Visueller Trenner — typisch horizontale oder vertikale Linie zwischen Content-Abschnitten.

**Best Practices:**
- Dezente, kontrastarme Separatoren — führen das Auge, dominieren nicht
- Abstand statt Separator, wenn Gruppierung schon klar
- Horizontale Rules zwischen Abschnitten, vertikale zwischen Spalten

**Typische Layouts:**
- Horizontaler Divider zwischen Listen- oder Content-Abschnitten
- Vertikaler Separator zwischen Sidebar und Hauptcontent
- Abschnitts-Divider mit zentriertem Label („oder“, „verwandter Inhalt“)

---

## Skeleton

**Auch bekannt als:** Skeleton loader

Niedrig aufgelöster Platzhalter in Content-Form während des Ladens, typisch graue Blöcke.

**Best Practices:**
- Skeleton-Form möglichst nah am echten Layout
- Dezentes Shimmer/Pulse statt Spinner
- Keine Skeletons bei sehr schnellem Laden (<300 ms)
- Skeleton sofort bei Navigation; atomisch ersetzen wenn Daten da
- Gedämpfte, kontrastarme Farben für Skeleton-Blöcke

**Typische Layouts:**
- Card-Grid-Skeleton mit Bild-Platzhalter, Titelbalken, Textzeilen
- Listen-/Feed-Skeleton mit wiederholten Zeilenformen
- Profilseiten-Skeleton mit Avatar-Kreis und Textblöcken
- Dashboard-Skeleton mit Chart-Platzhalter und Metrik-Blöcken

---

## Skip link

Versteckte Navigationslinks für Tastaturnutzer direkt zum Hauptcontent.

**Best Practices:**
- Erstes fokussierbares Element im DOM
- Visuell versteckt bis Focus — dann klar sichtbar
- Link zum Hauptcontent mit Label („Zum Hauptinhalt springen“)

**Typische Layouts:**
- Versteckter Link bei Tab-Focus ganz oben auf der Seite

---

## Slider

**Auch bekannt als:** Range input

Ziehbare Steuerung zur Wertauswahl innerhalb eines Bereichs.

**Best Practices:**
- Aktuellen Wert in Tooltip oder angrenzendem Label
- Tick Marks bei diskreten Slidern
- Ziehen und Klicken auf Track zum Setzen
- Mindest-Touch-Target für Thumb (44 px)
- Bei Bedarf mit Text-Input für präzise Eingabe paaren

**Typische Layouts:**
- Preisbereichsfilter mit zwei Thumbs (min/max)
- Lautstärke-/Helligkeits-Slider
- Zoom-Level bei Bild-Crop
- Pricing-Seite Sitz-/Nutzungs-Slider mit dynamischem Preis

---

## Spinner

**Auch bekannt als:** Loader  ·  Loading

Animierter Indikator für laufenden Hintergrundprozess — Interface noch nicht interaktiv.

**Best Practices:**
- Spinner erst nach Verzögerung (~300 ms) gegen Flackern
- Spinner proportional: inline (16 px), Button (20 px), Seite (40+ px)
- Ein einheitliches Spinner-Design in der App
- aria-label oder sr-only für Screenreader („Lädt…“)
- Skeleton Screens statt Spinner bei vorhersagbarem Layout

**Typische Layouts:**
- Zentrierter Full-Page-Spinner beim initialen App-Load
- Inline-Spinner im Button beim Formular-Absenden
- Kleiner Spinner neben Tabellenzelle bei Lazy-Load
- Overlay-Spinner auf Card beim Content-Refresh

---

## Stack

Layout-Utility für einheitlichen Abstand zwischen Kind-Komponenten.

**Best Practices:**
- Konsistente Spacing-Skala (4, 8, 12, 16, 24, 32, 48 px)
- Standard vertikal stapeln; horizontal für Inline-Gruppen
- Stack als Layout-Primitive für konsistenten Abstand

**Typische Layouts:**
- Vertikaler Stack von Formularfeldern mit gleichem Gap
- Horizontaler Stack von Aktionsbuttons mit Gap
- Card-Content als vertikaler Stack aus Titel, Beschreibung, Meta

---

## Stepper

**Auch bekannt als:** Nudger  ·  Quantity  ·  Counter

Numerisches Input mit Plus/Minus zum Anpassen eines Werts.

**Best Practices:**
- Klare +/- Buttons mit ausreichenden Touch-Targets
- Direkte Zahleneingabe zusätzlich zu Buttons
- Sinnvolle min, max und step Werte
- Entsprechenden Button bei min/max deaktivieren

**Typische Layouts:**
- Mengenauswahl im E-Commerce-Warenkorb
- Zahlen-Input für Sitzanzahl im Buchungsflow
- Portionsgröße in Rezept-App

---

## Table

Strukturiertes Raster aus Zeilen und Spalten — Data Table bei Sortierung und Filterung.

**Best Practices:**
- Sticky Header-Zeile bei scrollbaren Tabellen
- Zahlen-Spalten rechtsbündig
- Sortierbare Spaltenheader mit Richtungsindikatoren
- Zebra-Streifen oder horizontale Divider für Lesbarkeit
- Bulk-Select-Checkbox-Spalte bei aktionsfähigen Tabellen
- Tabellen auf Mobile horizontal scrollbar statt Spalten verstecken

**Typische Layouts:**
- Admin-Datentabelle mit Suche, Filtern, Sort, Pagination, Zeilenaktionen
- Pricing-Vergleichstabelle mit Feature-Zeilen und Plan-Spalten
- Finanz-Ledger mit Datum, Beschreibung, Betrag, laufendem Saldo
- Leaderboard mit Rang, Name, Avatar, Score

---

## Tabs

**Auch bekannt als:** Tabbed interface

Auswählbare Labels zum Wechseln zwischen Content-Panels — kompaktes Layout.

**Best Practices:**
- 2–7 Tabs; mehr → scrollbare Tab-Bar oder Dropdown-Overflow
- Aktiven Tab klar: unterer Border, Hintergrund oder Fettdruck
- Kurze, beschreibende Tab-Labels (1–2 Wörter)
- Tab-Content direkt unter Tab-Bar ohne Lücke
- Tastatur: Pfeile zwischen Tabs, Tab zum Content
- Tabs auf schmalen Viewports durch Accordion ersetzen

**Typische Layouts:**
- Produktseite mit Beschreibung, Reviews, Spezifikationen
- Settings mit Allgemein, Sicherheit, Benachrichtigungen
- Profil mit Aktivität, Projekten, Settings
- Dashboard mit Report-Ansichten (Übersicht, Analytics, Logs)

---

## Text input

Einzeiliges Formularfeld für kurze Textwerte.

**Best Practices:**
- Passende input types (email, tel, url, number) für Mobile-Tastatur
- Placeholder nur als Formathinweis, nie als Label-Ersatz
- Zeichenzähler bei längenbegrenzten Feldern
- Inline-Validierungsfehler unter Input mit rotem Border
- Autofill-Attribute für gängige Felder

**Typische Layouts:**
- Login mit E-Mail- und Passwort-Feldern
- Suchleiste mit Icon-Präfix und Clear-Button
- Inline-Edit: Text wird bei Klick zu Input
- Settings-Formular mit beschrifteten Inputs in einer Spalte

---

## Textarea

**Auch bekannt als:** Textbox  ·  Text box

Mehrzeiliges Textfeld für längere Eingaben.

**Best Practices:**
- Vertikales Resize erlauben mit min/max Höhe
- Zeichenzähler bei Limit
- Höhere Standardhöhe (3–5 Zeilen) als Multi-Line-Signal
- Textarea beim Tippen auto-grow für flüssigere UX

**Typische Layouts:**
- Kommentar-/Antwort-Input unter Post
- Feedback-Formular mit großem Nachrichtenfeld
- Notizfeld in CRM oder Projekttool
- Code- oder JSON-Input mit Monospace-Font

---

## Toast

**Auch bekannt als:** Snackbar

Kurze, nicht-blockierende Benachrichtigung in schwebender Schicht über dem Interface.

**Best Practices:**
- Auto-Dismiss nach 4–6 Sekunden bei nicht-kritischen Toasts
- Manuelles Schließen per Button oder Swipe
- Mehrere Toasts stapeln, neueste oben
- Konsistente Ecke — unten rechts am häufigsten auf Desktop
- Aktionslink für rückgängig machbare Ops („Rückgängig“ bei Löschen)
- Eine Textzeile — Toasts für kurze Bestätigungen

**Typische Layouts:**
- Erfolgs-Toast nach Speichern („Änderungen gespeichert“)
- Fehler-Toast mit Retry nach fehlgeschlagenem Request
- Undo-Toast nach Löschen („Element gelöscht. Rückgängig“)
- Benachrichtigungs-Toast mit Avatar und Kurzvorschau

---

## Toggle

**Auch bekannt als:** Switch  ·  Lightswitch  ·  Toggle button

Binärer Schalter zwischen zwei Zuständen — typisch an und aus.

**Best Practices:**
- Für binäre On/Off-Settings mit sofortiger Wirkung
- Toggle mit dem Labelen, was er steuert, nicht „An/Aus“
- Zustand visuell (Farbe, Position) und optional per Text
- Toggle leicht tappbar (44+ px breit)
- Keine Toggles in Formularen mit Speichern — Checkboxen nutzen

**Typische Layouts:**
- Settings-Zeile: Label links, Toggle rechts
- Dark-Mode-Toggle in Header oder Settings
- Feature-Flag-Toggles im Admin-Panel
- Benachrichtigungs-Präferenz-Toggles in Liste

---

## Tooltip

**Auch bekannt als:** Toggletip

Kleines schwebendes Label mit Zusatzinfo zu einem Element, typisch bei Hover.

**Best Practices:**
- Tooltips nur für Zusatzinfo — nie für essentiellen Content
- Hover (Desktop) und Long-Press (Mobile); kein Click-to-show
- Kurze Verzögerung (~300 ms), ausblenden bei Mouse Leave
- Tooltip auf einen Satz oder wenige Wörter
- Positionieren ohne Trigger oder wichtigen Content zu verdecken
- Toggletip (Click) bei interaktivem Inhalt

**Typische Layouts:**
- Icon-Button-Tooltip mit Aktionsname
- Tooltip für abgeschnittenen Text mit vollem String
- Info-Icon-Tooltip für Formularfeld-Zweck
- Chart-Datenpunkt-Tooltip mit exakten Werten

---

## Tree view

Aufklappbare, verschachtelte Hierarchie für strukturierte Daten wie Dateibäume oder Kategorien.

**Best Practices:**
- Einrückung (16–24 px pro Ebene) für Hierarchie
- Expand/Collapse-Toggles (Chevron/Dreieck) für Parent-Nodes
- Tastatur: Pfeile navigieren, Enter wählen, +/- expand/collapse
- Ausgewählten Node hervorheben mit Fokus-Indikator
- Tiefe Kinder lazy laden bei großen Bäumen

**Typische Layouts:**
- Datei-/Ordner-Browser in Editor oder CMS
- Kategoriebaum in E-Commerce-Sidebar
- Organigramm oder Reporting-Hierarchie
- Inhaltsverzeichnis-Navigation für Dokumentation

---

## Video

**Auch bekannt als:** Video player

Media-Komponente für Video mit Steuerung für Wiedergabe, Lautstärke und Vollbild.

**Best Practices:**
- Poster/Thumbnail vor Wiedergabe
- Untertitel für Barrierefreiheit
- Standard-Controls: Play/Pause, Lautstärke, Vollbild, Fortschritt
- Video lazy laden, kein Autoplay mit Ton

**Typische Layouts:**
- Produktdemo-Video zentriert auf Landing Page
- Video-Player mit Titel, Beschreibung, verwandten Videos
- Hintergrund-Video-Hero mit stummem Autoplay
- Tutorial-Video in Dokumentation eingebettet

---

## Visually hidden

**Auch bekannt als:** Screenreader only

Visuell versteckter Content, zugänglich für Screenreader und assistive Technologie.

**Best Practices:**
- Für Screenreader-only-Text mit Kontext, den sehende User nicht brauchen
- Nie display:none oder visibility:hidden — Clip-Rect-Technik
- Für Skip Links, Icon-only-Button-Labels, Formular-Anweisungen

**Typische Layouts:**
- Verstecktes Label für Icon-only-Schließen-Button
- Screenreader-Anweisungen für komplexes Widget

---
