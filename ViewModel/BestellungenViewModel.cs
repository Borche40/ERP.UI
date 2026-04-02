using ERP.Core.Services;
using ERP.Data.Models;
using ERP.UI.Commands;
using ERP.UI.Services;
using ERP.UI.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ERP.UI.ViewModel
{
    /// <summary>
    /// ViewModel für die Verwaltung und Anzeige aller Bestellungen.
    /// Unterstützt:
    /// - Laden aller Bestellungen
    /// - Auswahl einer Bestellung
    /// - Laden der Bestellpositionen
    /// - Aktualisieren
    /// - Neue Bestellung
    /// - Bestellung bearbeiten
    /// - Status ändern
    /// - Bestellung stornieren
    /// - Suche / Filterung
    /// </summary>
    public class BestellungenViewModel : BaseViewModel
    {
        // ───────── Felder ─────────
        private readonly BestellungenService _bestellungenService;
        private readonly IDialogService _dialogService;

        private ObservableCollection<Bestellung> _bestellungen;
        private ObservableCollection<Bestellung> _alleBestellungen;
        private Bestellung? _ausgewählteBestellung;
        private string _suchtext = string.Empty;

        // ───────── Eigenschaften ─────────

        /// <summary>
        /// Liste aller Bestellungen für die Anzeige im DataGrid.
        /// </summary>
        public ObservableCollection<Bestellung> Bestellungen
        {
            get => _bestellungen;
            set
            {
                _bestellungen = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Enthält die vollständige, ungefilterte Bestellliste.
        /// Diese Liste dient als Basis für die Suchfunktion.
        /// </summary>
        public ObservableCollection<Bestellung> AlleBestellungen
        {
            get => _alleBestellungen;
            set
            {
                _alleBestellungen = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Aktuell ausgewählte Bestellung.
        /// Beim Ändern werden automatisch die Positionen geladen.
        /// </summary>
        public Bestellung? AusgewählteBestellung
        {
            get => _ausgewählteBestellung;
            set
            {
                _ausgewählteBestellung = value;
                OnPropertyChanged();

                AktualisiereCommandStatus();
                _ = LadeBestellPositionenZurAusgewähltenBestellung();
            }
        }

        /// <summary>
        /// Suchtext für die Filterung nach Bestellnummer, Kunde oder Status.
        /// </summary>
        public string Suchtext
        {
            get => _suchtext;
            set
            {
                _suchtext = value;
                OnPropertyChanged();
                FilterBestellungen();
            }
        }

        /// <summary>
        /// ViewModel für die Bestellpositionen
        /// der aktuell ausgewählten Bestellung.
        /// </summary>
        public BestellPositionenViewModel BestellPositionenViewModel { get; set; }

        // ───────── Commands ─────────

        /// <summary>
        /// Lädt alle Bestellungen erneut aus der Datenbank.
        /// </summary>
        public ICommand AktualisierenCommand { get; }

        /// <summary>
        /// Öffnet den Dialog zum Anlegen einer neuen Bestellung.
        /// </summary>
        public ICommand NeueCommand { get; }

        /// <summary>
        /// Öffnet den Dialog zum Bearbeiten der ausgewählten Bestellung.
        /// </summary>
        public ICommand BearbeitenCommand { get; }

        /// <summary>
        /// Ändert den Status der ausgewählten Bestellung.
        /// </summary>
        public ICommand StatusÄndernCommand { get; }

        /// <summary>
        /// Storniert die ausgewählte Bestellung.
        /// </summary>
        public ICommand StornierenCommand { get; }

        // ───────── Konstruktor ─────────

        public BestellungenViewModel(IDialogService dialogService, BestellungenService bestellungenService)
        {
            _dialogService = dialogService;
            _bestellungenService = bestellungenService;

            Bestellungen = new ObservableCollection<Bestellung>();
            AlleBestellungen = new ObservableCollection<Bestellung>();
            BestellPositionenViewModel = new BestellPositionenViewModel();

            AktualisierenCommand = new Befehl(async _ => await LadeBestellungenAsync());
            NeueCommand = new Befehl(async _ => await NeueBestellungAsync());
            BearbeitenCommand = new Befehl(async _ => await BearbeitenAsync(), _ => AusgewählteBestellung != null);
            StatusÄndernCommand = new Befehl(async _ => await StatusÄndernAsync(), _ => AusgewählteBestellung != null);
            StornierenCommand = new Befehl(async _ => await StornierenAsync(), _ => AusgewählteBestellung != null);

            _ = LadeBestellungenAsync();
        }

        // ───────── Methoden ─────────

        /// <summary>
        /// Öffnet den Dialog zum Erstellen einer neuen Bestellung
        /// und lädt anschließend die Bestellliste neu.
        /// </summary>
        private async Task NeueBestellungAsync()
        {
            try
            {
                var dialog = new BestellungDialog(_dialogService, _bestellungenService);

                bool? result = dialog.ShowDialog();

                if (result == true)
                {
                    await LadeBestellungenAsync();
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError(
                    $"Fehler beim Öffnen des Bestellungsdialogs:\n{ex.Message}",
                    "Fehler");
            }
        }

        /// <summary>
        /// Öffnet den Dialog zum Bearbeiten der ausgewählten Bestellung
        /// und lädt anschließend die Bestellliste neu.
        /// </summary>
        private async Task BearbeitenAsync()
        {
            try
            {
                if (AusgewählteBestellung == null)
                {
                    _dialogService.ShowWarning(
                        "Bitte wählen Sie eine Bestellung aus, die bearbeitet werden soll.",
                        "Hinweis");
                    return;
                }

                var bestellungKopie = new Bestellung
                {
                    BestellungID = AusgewählteBestellung.BestellungID,
                    KundeID = AusgewählteBestellung.KundeID,
                    Kunde = AusgewählteBestellung.Kunde == null
                        ? null
                        : new Kunde
                        {
                            KundeID = AusgewählteBestellung.Kunde.KundeID,
                            Name = AusgewählteBestellung.Kunde.Name
                        },
                    Bestelldatum = AusgewählteBestellung.Bestelldatum,
                    Gesamtbetrag = AusgewählteBestellung.Gesamtbetrag,
                    Status = AusgewählteBestellung.Status,
                    Bestellnummer = AusgewählteBestellung.Bestellnummer,
                    StatusCode = AusgewählteBestellung.StatusCode,
                    ZahlungsstatusCode = AusgewählteBestellung.ZahlungsstatusCode,
                    NettoSumme = AusgewählteBestellung.NettoSumme,
                    MwStSumme = AusgewählteBestellung.MwStSumme,
                    BruttoSumme = AusgewählteBestellung.BruttoSumme,
                    ErstelltAm = AusgewählteBestellung.ErstelltAm,
                    GeändertAm = AusgewählteBestellung.GeändertAm,
                    RowVersion = AusgewählteBestellung.RowVersion
                };

                var dialog = new BestellungDialog(_dialogService, _bestellungenService, bestellungKopie);

                bool? result = dialog.ShowDialog();

                if (result == true)
                {
                    await LadeBestellungenAsync();
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError(
                    $"Fehler beim Bearbeiten der Bestellung:\n{ex.Message}",
                    "Fehler");
            }
        }

        /// <summary>
        /// Ändert den Status der ausgewählten Bestellung.
        /// Diese Version setzt den Status schrittweise weiter:
        /// Offen -> Bestätigt -> Versendet -> Abgeschlossen.
        /// Stornierte Bestellungen werden nicht weiter geändert.
        /// </summary>
        private async Task StatusÄndernAsync()
        {
            try
            {
                if (AusgewählteBestellung == null)
                {
                    _dialogService.ShowWarning(
                        "Bitte wählen Sie eine Bestellung aus.",
                        "Hinweis");
                    return;
                }

                int neuerStatusCode;
                string neuerStatusText;

                switch (AusgewählteBestellung.StatusCode)
                {
                    case 0:
                        neuerStatusCode = 1;
                        neuerStatusText = "Bestätigt";
                        break;

                    case 1:
                        neuerStatusCode = 2;
                        neuerStatusText = "Versendet";
                        break;

                    case 2:
                        neuerStatusCode = 3;
                        neuerStatusText = "Abgeschlossen";
                        break;

                    case 3:
                        _dialogService.ShowInfo(
                            "Diese Bestellung ist bereits abgeschlossen.",
                            "Information");
                        return;

                    case 4:
                        _dialogService.ShowWarning(
                            "Eine stornierte Bestellung kann nicht weiter geändert werden.",
                            "Hinweis");
                        return;

                    default:
                        neuerStatusCode = 1;
                        neuerStatusText = "Bestätigt";
                        break;
                }

                _bestellungenService.UpdateBestellungStatus(AusgewählteBestellung.BestellungID,neuerStatusText, neuerStatusCode);

                _dialogService.ShowInfo(
                    $"Der Bestellstatus wurde erfolgreich auf '{neuerStatusText}' geändert.",
                    "Erfolg");

                await LadeBestellungenAsync();
            }
            catch (Exception ex)
            {
                _dialogService.ShowError(
                    $"Fehler beim Ändern des Bestellstatus:\n{ex.Message}",
                    "Fehler");
            }
        }

        /// <summary>
        /// Storniert die ausgewählte Bestellung nach Bestätigung
        /// und lädt anschließend die Bestellliste neu.
        /// </summary>
        private async Task StornierenAsync()
        {
            try
            {
                if (AusgewählteBestellung == null)
                {
                    _dialogService.ShowWarning(
                        "Bitte wählen Sie eine Bestellung aus.",
                        "Hinweis");
                    return;
                }

                bool bestätigen = _dialogService.Confirm(
                    $"Möchten Sie die Bestellung '{AusgewählteBestellung.Bestellnummer}' wirklich stornieren?",
                    "Stornierung bestätigen");

                if (!bestätigen)
                    return;

                _bestellungenService.StornierenBestellung(AusgewählteBestellung.BestellungID);

                _dialogService.ShowInfo(
                    "Die Bestellung wurde erfolgreich storniert.",
                    "Erfolg");

                await LadeBestellungenAsync();
            }
            catch (Exception ex)
            {
                _dialogService.ShowError(
                    $"Fehler beim Stornieren der Bestellung:\n{ex.Message}",
                    "Fehler");
            }
        }

        /// <summary>
        /// Lädt alle Bestellungen aus der Datenbank.
        /// Danach wird die vollständige Liste gespeichert und optional gefiltert.
        /// </summary>
        private async Task LadeBestellungenAsync()
        {
            try
            {
                var liste = await _bestellungenService.LadeAlleBestellungenAsync();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    AlleBestellungen = new ObservableCollection<Bestellung>(liste);

                    if (string.IsNullOrWhiteSpace(Suchtext))
                        Bestellungen = new ObservableCollection<Bestellung>(liste);
                    else
                        FilterBestellungen();
                });

                if (AusgewählteBestellung != null)
                {
                    var neuAusgewählt = Bestellungen
                        .FirstOrDefault(b => b.BestellungID == AusgewählteBestellung.BestellungID);

                    AusgewählteBestellung = neuAusgewählt;
                }

                AktualisiereCommandStatus();
            }
            catch (Exception ex)
            {
                _dialogService.ShowError(
                    $"Fehler beim Laden der Bestellungen:\n{ex.Message}",
                    "Fehler");
            }
        }

        /// <summary>
        /// Filtert die Bestellungen nach Bestellnummer, Kundenname oder Status.
        /// </summary>
        private void FilterBestellungen()
        {
            try
            {
                if (AlleBestellungen == null || AlleBestellungen.Count == 0)
                {
                    Bestellungen = new ObservableCollection<Bestellung>();
                    return;
                }

                if (string.IsNullOrWhiteSpace(Suchtext))
                {
                    Bestellungen = new ObservableCollection<Bestellung>(AlleBestellungen);
                    return;
                }

                string suchwert = Suchtext.Trim();

                var gefiltert = AlleBestellungen.Where(b =>
                    (!string.IsNullOrWhiteSpace(b.Bestellnummer) &&
                     b.Bestellnummer.Contains(suchwert, StringComparison.OrdinalIgnoreCase))
                    ||
                    (b.Kunde != null &&
                     !string.IsNullOrWhiteSpace(b.Kunde.Name) &&
                     b.Kunde.Name.Contains(suchwert, StringComparison.OrdinalIgnoreCase))
                    ||
                    (!string.IsNullOrWhiteSpace(b.Status) &&
                     b.Status.Contains(suchwert, StringComparison.OrdinalIgnoreCase))
                );

                Bestellungen = new ObservableCollection<Bestellung>(gefiltert);
            }
            catch (Exception ex)
            {
                _dialogService.ShowError(
                    $"Fehler beim Filtern der Bestellungen:\n{ex.Message}",
                    "Fehler");
            }
        }

        /// <summary>
        /// Lädt die Positionen der aktuell ausgewählten Bestellung.
        /// </summary>
        private async Task LadeBestellPositionenZurAusgewähltenBestellung()
        {
            try
            {
                if (AusgewählteBestellung == null)
                    return;

                BestellPositionenViewModel.BestellungID = AusgewählteBestellung.BestellungID;
                await BestellPositionenViewModel.LadeBestellPositionenAsync();
            }
            catch (Exception ex)
            {
                _dialogService.ShowError(
                    $"Fehler beim Laden der Bestellpositionen:\n{ex.Message}",
                    "Fehler");
            }
        }

        /// <summary>
        /// Aktualisiert den Status der Commands,
        /// damit Buttons korrekt aktiviert/deaktiviert werden.
        /// </summary>
        private void AktualisiereCommandStatus()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}