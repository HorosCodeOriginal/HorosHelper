---
name: avalonia-mvvm
description: HorosCode Avalonia MVVM-Schichten — View/ViewModel/Service/Repository, Bindings statt Code-Behind.
---

# Purpose

Erzwingt saubere **MVVM-Trennung** im HorosCode Desktop-Stack. Keine Business-Logik in Views, Commands in ViewModels, Daten über Services/Repositories.

**Firma:** HorosCode · **Produkt:** HorosCloud · **Stack:** C# 12 / Avalonia 11+ / CommunityToolkit.Mvvm

# Schichten

```
View (AXAML)  →  ViewModel  →  Service  →  Repository
     ↑              ↑
  Bindings      Commands/State
```

| Schicht | Verantwortung | Verboten |
|---------|---------------|----------|
| **View** | Layout, Styles, Bindings | Business-Logik, API-Calls, `if` in Code-Behind |
| **ViewModel** | State, Commands, Validation | AXAML, direkte DB-Zugriffe |
| **Service** | Use Cases, Orchestrierung | UI-Wissen |
| **Repository** | Datenpersistenz, API | UI-State |

## View — AXAML only

```xml
<UserControl x:DataType="vm:HeaderViewModel">
  <Button Command="{Binding SaveCommand}"
          IsEnabled="{Binding CanSave}" />
</UserControl>
```

Code-Behind: nur `InitializeComponent()` — keine Event-Handler mit Logik.

## ViewModel — Commands + Observable Properties

```csharp
public partial class HeaderViewModel : ObservableObject
{
    private readonly ISettingsService _settings;

    [ObservableProperty] private string _title = "";

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync() => await _settings.SaveTitleAsync(Title);

    private bool CanSave() => !string.IsNullOrWhiteSpace(Title);
}
```

## Service / Repository

```csharp
public interface ISettingsService
{
    Task SaveTitleAsync(string title, CancellationToken ct = default);
}
```

Mock-Daten im ersten UI-Pass — echte Services erst nach Human Approval des Bereichs.

# Bindings over Code-Behind

| Statt | Nutze |
|-------|-------|
| `Click="OnSave"` | `Command="{Binding SaveCommand}"` |
| `textBlock.Text = vm.Title` | `Text="{Binding Title}"` |
| Code-Behind Visibility | `IsVisible="{Binding IsPanelOpen}"` |
| Manuelles INotify | `[ObservableProperty]` (CommunityToolkit) |

# Output

- `*View.axaml` + `*ViewModel.cs` pro Region
- Services nur als Interfaces injiziert (DI)
- Kein Logic-Leak in `.axaml.cs`

# Rule

> **Keine Business-Logik in Views.** Jede User-Aktion = Command im ViewModel. Verstöße werden von `@architecture-guardian` abgelehnt.

# Verweise

- Docs: `docs/avalonia-patterns.md`, `docs/coding-standards.md`, `docs/architecture.md`
- Rule: `05-csharp`, `10-anti-patterns`
- Works with: `@component-discovery`, `@preview-first-development`
