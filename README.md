# ERP UI System (C# WPF / MVVM / SQL Server)

## Überblick
ERP-nahe Desktop-Anwendung, entwickelt mit **C#/.NET**, **WPF (MVVM)** und **SQL Server**.  
Fokus: saubere Struktur, wartbarer Code und typische Business-Workflows (CRUD, Validierung, Logging).

## Tech Stack
- **C# / .NET**
- **WPF, XAML, MVVM**
- **SQL Server / SSMS**
- **REST API + Swagger** (Integration/Tests)

## Funktionen (Auszug)
- Login / Registrierung
- Module: Benutzer, Kunden, Aufgaben, Bestellungen (CRUD)
- Dashboard mit KPIs und Diagrammen
- Validierung, Fehlerbehandlung & Logging

## Screenshots
> (Bilder liegen unter `docs/screenshots/`)

![Dashboard](docs/screenshots/01_Dashboard 1.png)

## Projektstruktur (Kurz)
- `Views/` – UI Views (WPF)
- `ViewModel/` – MVVM ViewModels
- `Models/` – Datenmodelle
- `Commands/` – Commands / UI Aktionen
- `Styles/` / `Themes/` – UI Styles & Themes
- `docs/screenshots/` – Screenshots für README

## How to Run (lokal)
1. Repository klonen
2. Solution in **Visual Studio** öffnen
3. SQL Server Verbindung konfigurieren (lokale DB)
4. Anwendung starten

## Link
GitHub: https://github.com/Borche40/ERP.UI
