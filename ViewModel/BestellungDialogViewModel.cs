using ERP.Core.Services;
using ERP.Data.Models;
using ERP.UI.Commands;
using ERP.UI.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ERP.UI.ViewModel
{
    /// <summary>
    /// ViewModel für den BestellDialog.
    /// Unterstützt:
    /// - Erstellen und Bearbeiten von Bestellungen
    /// - Laden und Aktualisieren der Bestellpositionen
    /// - Statusänderung
    /// - Stornierung
    /// - Versenden
    /// - Summenberechnung auf Basis der Positionen
    /// </summary>
    public class BestellungDialogViewModel : BaseViewModel
    {
        // ───────── Felder ─────────
        private readonly IDialogService _dialogService;
        private readonly BestellungenService _bestellungenService;
        private bool _istBearbeitenModus;
        private readonly ZahlungsartenService _zahlungsartenService;
        private readonly ZahlungsstatusService zahlungsstatusService;

        private int _bestellungId;
        private int _kundeId;
        private DateTime _bestelldatum;
        private string _status = string.Empty;
        private int _statusCode;
        private int _zahlungsstatusCode;
        private decimal _gesamtbetrag;
        private string _bestellnummer = string.Empty;
        private decimal _nettoSumme;
        private decimal _mwStSumme;
        private decimal _bruttoSumme;
        private string _ausgewählterBestellstatus = "Offen";
        private string _ausgewählterZahlungsstatus = "Offen";

        // ───────── Eigenschaften ─────────

        public int BestellungID
        {
            get => _bestellungId;
            set
            {
                _bestellungId = value;
                OnPropertyChanged();
                AktualisiereCommandStatus();
            }
        }

        public int KundeID
        {
            get => _kundeId;
            set
            {
                _kundeId = value;
                OnPropertyChanged();
                AktualisiereCommandStatus();
            }
        }

        public DateTime Bestelldatum
        {
            get => _bestelldatum;
            set
            {
                _bestelldatum = value;
                OnPropertyChanged();
            }
        }

        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }

        public int StatusCode
        {
            get => _statusCode;
            set
            {
                _statusCode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IstStornierbar));
                OnPropertyChanged(nameof(IstVersendbar));
                AktualisiereCommandStatus();
            }
        }

        public int ZahlungsstatusCode
        {
            get => _zahlungsstatusCode;
            set
            {
                _zahlungsstatusCode = value;
                OnPropertyChanged();
            }
        }

        public decimal Gesamtbetrag
        {
            get => _gesamtbetrag;
            set
            {
                _gesamtbetrag = value;
                OnPropertyChanged();
            }
        }

        public string Bestellnummer
        {
            get => _bestellnummer;
            set
            {
                _bestellnummer = value;
                OnPropertyChanged();
            }
        }

        public decimal NettoSumme
        {
            get => _nettoSumme;
            set
            {
                _nettoSumme = value;
                OnPropertyChanged();
            }
        }

        public decimal MwStSumme
        {
            get => _mwStSumme;
            set
            {
                _mwStSumme = value;
                OnPropertyChanged();
            }
        }

        public decimal BruttoSumme
        {
            get => _bruttoSumme;
            set
            {
                _bruttoSumme = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Anzeige- und Auswahlwerte für den Bestellstatus.
        /// </summary>
        public ObservableCollection<string> BestellstatusListe { get; } =
            new ObservableCollection<string>
            {
                "Offen",
                "Bestätigt",
                "Versendet",
                "Abgeschlossen",
                "Storniert"
            };

        /// <summary>
        /// Anzeige- und Auswahlwerte für den Zahlungsstatus.
        /// </summary>
        public ObservableCollection<string> ZahlungsstatusListe { get; } =
            new ObservableCollection<string>
            {
                "Offen",
                "Teilweise bezahlt",
                "Bezahlt"
            };

        /// <summary>
        /// Ausgewählter Bestellstatus im Dialog.
        /// </summary>
        public string AusgewählterBestellstatus
        {
            get => _ausgewählterBestellstatus;
            set
            {
                _ausgewählterBestellstatus = value;
                Status = value;
                StatusCode = ErmittleStatusCodeAusText(value);
                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Ausgewählter Zahlungsstatus im Dialog.
        /// </summary>
        public string AusgewählterZahlungsstatus
        {
            get => _ausgewählterZahlungsstatus;
            set
            {
                _ausgewählterZahlungsstatus = value;
                ZahlungsstatusCode = ErmittleZahlungsstatusCodeAusText(value);
                OnPropertyChanged();
            }
        }

        public bool IstStornierbar => BestellungID > 0 && StatusCode != 4;
        public bool IstVersendbar => BestellungID > 0 && StatusCode != 2 && StatusCode != 3 && StatusCode != 4;

        /// <summary>
        /// ViewModel für die Positionen der aktuellen Bestellung.
        /// </summary>
        public BestellPositionenViewModel BestellPositionenViewModel { get; }

       


        // ───────── Commands ─────────

        public ICommand NeueCommand { get; }
        public ICommand BearbeitenCommand { get; }
        public ICommand StatusÄndernCommand { get; }
        public ICommand StornierenCommand { get; }
        public ICommand AktualisierenCommand { get; }
        public ICommand VersendenCommand { get; }
        public ICommand SpeichernCommand { get; }
        public ICommand AbbrechenCommand { get; }

        public BestellungDialogViewModel()
        {
            _dialogService = new DialogService();
            _bestellungenService = new BestellungenService();
             BestellPositionenViewModel = new BestellPositionenViewModel();
            SetzeNeueStandardwerte();
        }

        // ───────── Konstruktor ─────────

        public BestellungDialogViewModel(
            IDialogService dialogService,
            BestellungenService bestellungenService,
            Bestellung? bestellung = null)
        {
            _dialogService = dialogService;
            _bestellungenService = bestellungenService;
            _zahlungsartenService = new ZahlungsartenService();
           

            BestellPositionenViewModel = new BestellPositionenViewModel();
            ZahlungsstatusListe = new ObservableCollection<string>
            {
                "Öffen",
                "Teilweise bezahlt",
                "Bezahlt",
                "Überfällig"
            };


            AusgewählterZahlungsstatus = "Öffen";
            if (bestellung != null)
            {
                _istBearbeitenModus = true;
                ÜbernehmeBestellung(bestellung);

                BestellPositionenViewModel.BestellungID = BestellungID;
                _ = LadePositionenUndBerechneSummenAsync();
            }
            else
            {
                _istBearbeitenModus = false;
                SetzeNeueStandardwerte();
            }

            NeueCommand = new Befehl(_ => NeueBestellungVorbereiten());
            BearbeitenCommand = new Befehl(async _ => await BearbeitenNeuLadenAsync(), _ => BestellungID > 0);
            StatusÄndernCommand = new Befehl(async _ => await StatusÄndernAsync(), _ => BestellungID > 0);
            StornierenCommand = new Befehl(async _ => await StornierenAsync(), _ => IstStornierbar);
            AktualisierenCommand = new Befehl(async _ => await AktualisierenAsync(), _ => BestellungID > 0);
            VersendenCommand = new Befehl(async _ => await VersendenAsync(), _ => IstVersendbar);
            SpeichernCommand = new Befehl(async _ => await SpeichernAsync());
            AbbrechenCommand = new Befehl(_ => Abbrechen());
        }

      

        // ───────── Methoden ─────────

        /// <summary>
        /// Setzt den Dialog auf eine neue Bestellung zurück.
        /// </summary>
        private void NeueBestellungVorbereiten()
        {
            _istBearbeitenModus = false;
            SetzeNeueStandardwerte();

            BestellPositionenViewModel.BestellungID = 0;
            BestellPositionenViewModel.BestellPositionen.Clear();

            BerechneSummenAusPositionen();
            AktualisiereCommandStatus();
        }

        /// <summary>
        /// Lädt die aktuelle Bestellung erneut aus der Datenbank.
        /// </summary>
        private async Task BearbeitenNeuLadenAsync()
        {
            try
            {
                if (BestellungID <= 0)
                    return;

                var bestellung = await _bestellungenService.LadeBestellungByIdAsync(BestellungID);

                if (bestellung == null)
                {
                    _dialogService.ShowWarning(
                        "Die Bestellung konnte nicht erneut geladen werden.",
                        "Hinweis");
                    return;
                }

                _istBearbeitenModus = true;
                ÜbernehmeBestellung(bestellung);

                BestellPositionenViewModel.BestellungID = BestellungID;
                await LadePositionenUndBerechneSummenAsync();
            }
            catch (Exception ex)
            {
                _dialogService.ShowError(
                    $"Fehler beim erneuten Laden der Bestellung:\n{ex.Message}",
                    "Fehler");
            }
        }

        /// <summary>
        /// Lädt Bestellung und Positionen neu.
        /// </summary>
        private async Task AktualisierenAsync()
        {
            try
            {
                if (BestellungID > 0)
                {
                    var bestellung = await _bestellungenService.LadeBestellungByIdAsync(BestellungID);
                    if (bestellung != null)
                    {
                        ÜbernehmeBestellung(bestellung);
                    }

                    BestellPositionenViewModel.BestellungID = BestellungID;
                    await LadePositionenUndBerechneSummenAsync();
                }
                else
                {
                    BerechneSummenAusPositionen();
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError(
                    $"Fehler beim Aktualisieren:\n{ex.Message}",
                    "Fehler");
            }
        }

        /// <summary>
        /// Ändert den Status der aktuellen Bestellung stufenweise weiter.
        /// </summary>
        private async Task StatusÄndernAsync()
        {
            try
            {
                if (BestellungID <= 0)
                {
                    _dialogService.ShowWarning(
                        "Bitte speichern Sie die Bestellung zuerst.",
                        "Hinweis");
                    return;
                }

                int neuerStatusCode;
                string neuerStatusText;

                switch (StatusCode)
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

                _bestellungenService.UpdateBestellungStatus(BestellungID, neuerStatusText, neuerStatusCode);

                Status = neuerStatusText;
                StatusCode = neuerStatusCode;
                AusgewählterBestellstatus = neuerStatusText;

                _dialogService.ShowInfo(
                    $"Der Bestellstatus wurde erfolgreich auf '{neuerStatusText}' geändert.",
                    "Erfolg");

                await AktualisierenAsync();
            }
            catch (Exception ex)
            {
                _dialogService.ShowError(
                    $"Fehler beim Ändern des Bestellstatus:\n{ex.Message}",
                    "Fehler");
            }
        }

        /// <summary>
        /// Setzt den Status direkt auf Versendet.
        /// </summary>
        private async Task VersendenAsync()
        {
            try
            {
                if (BestellungID <= 0)
                {
                    _dialogService.ShowWarning(
                        "Bitte speichern Sie die Bestellung zuerst.",
                        "Hinweis");
                    return;
                }

                if (StatusCode == 4)
                {
                    _dialogService.ShowWarning(
                        "Eine stornierte Bestellung kann nicht versendet werden.",
                        "Hinweis");
                    return;
                }

                if (StatusCode == 3)
                {
                    _dialogService.ShowInfo(
                        "Diese Bestellung ist bereits abgeschlossen.",
                        "Information");
                    return;
                }

                _bestellungenService.UpdateBestellungStatus(BestellungID, "Versendet", 2);

                Status = "Versendet";
                StatusCode = 2;
                AusgewählterBestellstatus = "Versendet";

                _dialogService.ShowInfo(
                    "Die Bestellung wurde erfolgreich als versendet markiert.",
                    "Erfolg");

                await AktualisierenAsync();
            }
            catch (Exception ex)
            {
                _dialogService.ShowError(
                    $"Fehler beim Versenden der Bestellung:\n{ex.Message}",
                    "Fehler");
            }
        }

        /// <summary>
        /// Storniert die aktuelle Bestellung.
        /// </summary>
        private async Task StornierenAsync()
        {
            try
            {
                if (BestellungID <= 0)
                {
                    _dialogService.ShowWarning(
                        "Bitte speichern Sie die Bestellung zuerst.",
                        "Hinweis");
                    return;
                }

                bool bestätigen = _dialogService.Confirm(
                    $"Möchten Sie die Bestellung '{Bestellnummer}' wirklich stornieren?",
                    "Stornierung bestätigen");

                if (!bestätigen)
                    return;

                _bestellungenService.StornierenBestellung(BestellungID);

                Status = "Storniert";
                StatusCode = 4;
                AusgewählterBestellstatus = "Storniert";

                _dialogService.ShowInfo(
                    "Die Bestellung wurde erfolgreich storniert.",
                    "Erfolg");

                await AktualisierenAsync();
            }
            catch (Exception ex)
            {
                _dialogService.ShowError(
                    $"Fehler beim Stornieren:\n{ex.Message}",
                    "Fehler");
            }
        }

        /// <summary>
        /// Lädt die Positionen der Bestellung und berechnet anschließend die Summen neu.
        /// </summary>
        private async Task LadePositionenUndBerechneSummenAsync()
        {
            try
            {
                if (BestellungID <= 0)
                    return;

                await BestellPositionenViewModel.LadeBestellPositionenAsync();
                BerechneSummenAusPositionen();
            }
            catch (Exception ex)
            {
                _dialogService.ShowError(
                    $"Fehler beim Laden der Bestellpositionen:\n{ex.Message}",
                    "Fehler");
            }
        }

        /// <summary>
        /// Übernimmt die Summen aus dem BestellPositionenViewModel.
        /// </summary>
        private void BerechneSummenAusPositionen()
        {
            NettoSumme = BestellPositionenViewModel.NettoSumme;
            MwStSumme = BestellPositionenViewModel.MwStSumme;
            BruttoSumme = BestellPositionenViewModel.BruttoSumme;
            Gesamtbetrag = BruttoSumme;
        }

        /// <summary>
        /// Speichert die Bestellung.
        /// </summary>
        private async Task SpeichernAsync()
        {
            try
            {
                if (KundeID <= 0)
                {
                    _dialogService.ShowWarning(
                        "Bitte geben Sie eine gültige KundeID ein.",
                        "Hinweis");
                    return;
                }

                BerechneSummenAusPositionen();

                if (_istBearbeitenModus)
                {
                    var bestellung = ErzeugeBestellobjekt();

                    bool erfolg = await _bestellungenService.BestellungAktualisierenAsync(bestellung);

                    if (erfolg)
                    {
                        _dialogService.ShowInfo(
                            "Bestellung wurde erfolgreich aktualisiert.",
                            "Erfolg");
                        Schließen(true);
                    }
                    else
                    {
                        _dialogService.ShowError(
                            "Die Bestellung konnte nicht aktualisiert werden.",
                            "Fehler");
                    }
                }
                else
                {
                    int neueId = await _bestellungenService.BestellungHinzufügenAsync(KundeID);

                    if (neueId > 0)
                    {
                        BestellungID = neueId;
                        BestellPositionenViewModel.BestellungID = neueId;
                        _istBearbeitenModus = true;

                        _dialogService.ShowInfo(
                            $"Neue Bestellung wurde erfolgreich erstellt.\nBestellungs-ID: {neueId}",
                            "Erfolg");

                        Schließen(true);
                    }
                    else
                    {
                        _dialogService.ShowError(
                            "Die Bestellung konnte nicht erstellt werden.",
                            "Fehler");
                    }
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError(
                    $"Fehler beim Speichern:\n{ex.Message}",
                    "Fehler");
            }
        }

        private Bestellung ErzeugeBestellobjekt()
        {
            return new Bestellung
            {
                BestellungID = BestellungID,
                KundeID = KundeID,
                Bestelldatum = Bestelldatum,
                Gesamtbetrag = Gesamtbetrag,
                Status = Status,
                Bestellnummer = Bestellnummer,
                StatusCode = StatusCode,
                ZahlungsstatusCode = ZahlungsstatusCode,
                NettoSumme = NettoSumme,
                MwStSumme = MwStSumme,
                BruttoSumme = BruttoSumme
            };
        }

        private void Abbrechen()
        {
            Schließen(false);
        }

        private void Schließen(bool? result)
        {
            var window = System.Windows.Application.Current.Windows
                .OfType<System.Windows.Window>()
                .FirstOrDefault(w => w.DataContext == this);

            if (window != null)
                window.DialogResult = result;

            window?.Close();
        }

        /// <summary>
        /// Übernimmt alle Werte aus einer Bestellung in das ViewModel.
        /// </summary>
        private void ÜbernehmeBestellung(Bestellung bestellung)
        {
            BestellungID = bestellung.BestellungID;
            KundeID = bestellung.KundeID;
            Bestelldatum = bestellung.Bestelldatum;
            Gesamtbetrag = bestellung.Gesamtbetrag;
            Status = bestellung.Status ?? "Offen";
            Bestellnummer = bestellung.Bestellnummer ?? string.Empty;
            StatusCode = bestellung.StatusCode;
            ZahlungsstatusCode = bestellung.ZahlungsstatusCode;
            NettoSumme = bestellung.NettoSumme;
            MwStSumme = bestellung.MwStSumme;
            BruttoSumme = bestellung.BruttoSumme;

            AusgewählterBestellstatus = Status;
            AusgewählterZahlungsstatus = ErmittleZahlungsstatusTextAusCode(ZahlungsstatusCode);
        }

        /// <summary>
        /// Setzt Standardwerte für eine neue Bestellung.
        /// </summary>
        private void SetzeNeueStandardwerte()
        {
            BestellungID = 0;
            KundeID = 0;
            Bestelldatum = DateTime.Now;
            Status = "Offen";
            StatusCode = 0;
            ZahlungsstatusCode = 0;
            Gesamtbetrag = 0m;
            NettoSumme = 0m;
            MwStSumme = 0m;
            BruttoSumme = 0m;
            Bestellnummer = $"SO-{DateTime.Now:yyyy}-{DateTime.Now:HHmmss}";

            AusgewählterBestellstatus = "Offen";
            AusgewählterZahlungsstatus = "Offen";
        }

        private int ErmittleStatusCodeAusText(string statusText)
        {
            return statusText switch
            {
                "Offen" => 0,
                "Bestätigt" => 1,
                "Versendet" => 2,
                "Abgeschlossen" => 3,
                "Storniert" => 4,
                _ => 0
            };
        }

        private int ErmittleZahlungsstatusCodeAusText(string zahlungsstatusText)
        {
            return zahlungsstatusText switch
            {
                "Offen" => 0,
                "Teilweise bezahlt" => 1,
                "Bezahlt" => 2,
                _ => 0
            };
        }

        private string ErmittleZahlungsstatusTextAusCode(int code)
        {
            return code switch
            {
                0 => "Offen",
                1 => "Teilweise bezahlt",
                2 => "Bezahlt",
                _ => "Offen"
            };
        }

        /// <summary>
        /// Aktualisiert den CanExecute-Status der Commands.
        /// </summary>
        private void AktualisiereCommandStatus()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}