using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using ERP.Core.Services;
using ERP.Data.Models;
using ERP.UI.Commands;

namespace ERP.UI.ViewModel
{
    /// <summary>
    /// <b>KundenViewModel</b> – steuert die gesamte Kundenverwaltung innerhalb der ERP-Anwendung.
    /// Dieses ViewModel stellt die Verbindung zwischen der Benutzeroberfläche (KundenFenster / KundenView)
    /// und der Logikschicht (<see cref="KundenService"/>) her.
    ///
    /// Es enthält alle Daten, Befehle und Operationen für das Laden, Hinzufügen,
    /// Aktualisieren, Löschen sowie Exportieren von Kunden.
    /// </summary>
    public class KundenViewModel : BaseViewModel
    {
        #region Felder und Services

        /// <summary>
        /// Dienst für den Zugriff auf Kundendaten.
        /// </summary>
        private readonly KundenService _kundenService;

        /// <summary>
        /// Dienst für den PDF-Export der Kundenliste.
        /// </summary>
        private readonly PdfExportService _pdfExportService;

        /// <summary>
        /// Dienst für den Excel-Export der Kundenliste.
        /// </summary>
        private readonly ExcelExportService _excelExportService;

        /// <summary>
        /// Speichert den aktuell ausgewählten Kunden aus der Tabelle
        /// oder den aktuell bearbeiteten Kunden im Eingabebereich.
        /// </summary>
        private Kunde _ausgewählterKunde = new();

        /// <summary>
        /// Speichert den Suchtext für die Live-Filterung der Kundenliste.
        /// </summary>
        private string _suchtext = string.Empty;

        #endregion

        #region Eigenschaften

        /// <summary>
        /// Liste aller Kunden, die im UI angezeigt werden.
        /// </summary>
        public ObservableCollection<Kunde> KundenListe { get; set; } = new();

        /// <summary>
        /// Stellt die gefilterte / sortierte Sicht auf die Kundenliste bereit.
        /// </summary>
        public ICollectionView KundenView { get; private set; } = null!;

        /// <summary>
        /// Bindet den aktuell ausgewählten oder bearbeiteten Kunden an die Oberfläche.
        /// </summary>
        public Kunde AusgewählterKunde
        {
            get => _ausgewählterKunde;
            set
            {
                _ausgewählterKunde = value ?? new Kunde();
                OnPropertyChanged(nameof(AusgewählterKunde));

                // Aktualisiert die Aktivierbarkeit der Buttons.
                CommandManager.InvalidateRequerySuggested();
            }
        }

        /// <summary>
        /// Bindet den Suchtext für die Filterung der Kundenliste.
        /// </summary>
        public string Suchtext
        {
            get => _suchtext;
            set
            {
                if (_suchtext == value)
                    return;

                _suchtext = value;
                OnPropertyChanged(nameof(Suchtext));
                KundenView?.Refresh();
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Befehl zum Laden aller Kunden.
        /// </summary>
        public ICommand LadenCommand { get; }

        /// <summary>
        /// Befehl zum Hinzufügen eines neuen Kunden.
        /// </summary>
        public ICommand HinzufügenCommand { get; }

        /// <summary>
        /// Befehl zum Aktualisieren des ausgewählten Kunden.
        /// </summary>
        public ICommand AktualisierenCommand { get; }

        /// <summary>
        /// Befehl zum Löschen des ausgewählten Kunden.
        /// </summary>
        public ICommand LöschenCommand { get; }

        /// <summary>
        /// Befehl zum Exportieren der Kundenliste nach PDF.
        /// </summary>
        public ICommand ExportierenCommand { get; }

        /// <summary>
        /// Befehl zum Exportieren der Kundenliste nach Excel.
        /// </summary>
        public ICommand ExportExcelCommand { get; }

        #endregion

        #region Konstruktoren

        /// <summary>
        /// Initialisiert das ViewModel mit den benötigten Services.
        /// </summary>
        /// <param name="kundenService">Service für Kundenoperationen.</param>
        public KundenViewModel(KundenService kundenService)
        {
            _kundenService = kundenService ?? new KundenService();
            _pdfExportService = new PdfExportService();
            _excelExportService = new ExcelExportService();

            KundenView = CollectionViewSource.GetDefaultView(KundenListe);
            KundenView.Filter = KundeFilter;

            LadenCommand = new Befehl(async _ => await LadeKundenAsync());
            HinzufügenCommand = new Befehl(async _ => await KundenHinzufügenAsync(), _ => KannHinzufügen());
            AktualisierenCommand = new Befehl(async _ => await KundenAktualisierenAsync(), _ => KannAktualisieren());
            LöschenCommand = new Befehl(async _ => await KundenLöschenAsync(), _ => KannLöschen());
            ExportierenCommand = new Befehl(async _ => await ExportiereKundenNachPdfAsync(), _ => KannExportieren());
            ExportExcelCommand = new Befehl(async _ => await ExportiereKundenExcelAsync(), _ => KannExportieren());

            AusgewählterKunde = new Kunde();

            // Wichtig:
            // Hier NICHT automatisch laden, wenn sich die View an anderer Stelle
            // bereits um das Laden kümmert. So vermeiden wir doppelte Einträge.
            // _ = LadeKundenAsync();
        }

        /// <summary>
        /// Parameterloser Konstruktor für XAML / Design-Time und Standardverwendung.
        /// </summary>
        public KundenViewModel()
            : this(new KundenService())
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                KundenListe.Add(new Kunde
                {
                    KundeID = 1,
                    Name = "Max Mustermann",
                    Email = "max.mustermann@firma.at",
                    Telefonnummer = "+43 660 1234567",
                    ErstelltAm = DateTime.Now.AddDays(-15)
                });

                KundenListe.Add(new Kunde
                {
                    KundeID = 2,
                    Name = "Anna Berger",
                    Email = "anna.berger@firma.at",
                    Telefonnummer = "+43 699 9876543",
                    ErstelltAm = DateTime.Now.AddDays(-7)
                });

                KundenView.Refresh();
            }
        }

        #endregion

        #region Filterlogik

        /// <summary>
        /// Filtert die Kundenliste anhand des eingegebenen Suchtextes.
        /// Es wird nach Name, E-Mail, Telefonnummer und ID gesucht.
        /// </summary>
        /// <param name="obj">Aktuelles Objekt aus der CollectionView.</param>
        /// <returns>True, wenn der Kunde angezeigt werden soll, sonst False.</returns>
        private bool KundeFilter(object obj)
        {
            if (obj is not Kunde kunde)
                return false;

            if (string.IsNullOrWhiteSpace(Suchtext))
                return true;

            string q = Suchtext.Trim().ToLower();

            return (kunde.Name?.ToLower().Contains(q) ?? false)
                   || (kunde.Email?.ToLower().Contains(q) ?? false)
                   || (kunde.Telefonnummer?.ToLower().Contains(q) ?? false)
                   || kunde.KundeID.ToString().Contains(q);
        }

        #endregion

        #region Methoden – Datenoperationen

        /// <summary>
        /// Lädt alle Kunden aus der Datenbank und aktualisiert die Liste.
        /// </summary>
        public async Task LadeKundenAsync()
        {
            try
            {
                KundenListe.Clear();

                var kunden = await _kundenService.LadeAlleKundenAsync();

                if (kunden != null && kunden.Count > 0)
                {
                    foreach (var kunde in kunden)
                    {
                        KundenListe.Add(kunde);
                    }
                }
                else
                {
                    MessageBox.Show("Keine Kunden gefunden.",
                                    "Information",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                }

                KundenView.Refresh();
                CommandManager.InvalidateRequerySuggested();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Laden der Kunden:\n{ex.Message}",
                                "Fehler",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Fügt einen neuen Kunden hinzu.
        /// </summary>
        private async Task KundenHinzufügenAsync()
        {
            try
            {
                var neuerKunde = AusgewählterKunde;

                if (neuerKunde == null)
                {
                    MessageBox.Show("Es sind keine Kundendaten vorhanden.",
                                    "Hinweis",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                    return;
                }

                int result = await _kundenService.KundenHinzufügenAsync(neuerKunde);

                if (result <= 0)
                {
                    MessageBox.Show("Kunde konnte nicht hinzugefügt werden.",
                                    "Warnung",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                    return;
                }

                MessageBox.Show("Neuer Kunde wurde erfolgreich hinzugefügt.",
                                "Erfolg",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

                await LadeKundenAsync();

                // Nach erfolgreichem Hinzufügen Formular zurücksetzen.
                AusgewählterKunde = new Kunde();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Hinzufügen des Kunden:\n{ex.Message}",
                                "Fehler",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Aktualisiert den aktuell ausgewählten Kunden.
        /// </summary>
        private async Task KundenAktualisierenAsync()
        {
            try
            {
                if (AusgewählterKunde == null || AusgewählterKunde.KundeID <= 0)
                {
                    MessageBox.Show("Bitte wählen Sie einen gültigen Kunden zum Aktualisieren aus.",
                                    "Hinweis",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                    return;
                }

                bool erfolgreich = await _kundenService.KundenAktualisierenAsync(AusgewählterKunde);

                if (erfolgreich)
                {
                    MessageBox.Show("Kunde wurde erfolgreich aktualisiert.",
                                    "Erfolg",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);

                    await LadeKundenAsync();
                }
                else
                {
                    MessageBox.Show("Kunde konnte nicht aktualisiert werden.",
                                    "Warnung",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Aktualisieren des Kunden:\n{ex.Message}",
                                "Fehler",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Löscht den aktuell ausgewählten Kunden nach Bestätigung.
        /// </summary>
        private async Task KundenLöschenAsync()
        {
            if (AusgewählterKunde == null || AusgewählterKunde.KundeID <= 0)
            {
                MessageBox.Show("Bitte wählen Sie einen gültigen Kunden zum Löschen aus.",
                                "Hinweis",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Möchten Sie den Kunden '{AusgewählterKunde.Name}' wirklich löschen?",
                "Bestätigung",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                bool erfolgreich = await _kundenService.KundenLöschenAsync(AusgewählterKunde.KundeID);

                if (erfolgreich)
                {
                    MessageBox.Show("Kunde wurde erfolgreich gelöscht.",
                                    "Erfolg",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);

                    await LadeKundenAsync();
                    AusgewählterKunde = new Kunde();
                }
                else
                {
                    MessageBox.Show("Kunde konnte nicht gelöscht werden.",
                                    "Warnung",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Löschen des Kunden:\n{ex.Message}",
                                "Fehler",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        #endregion

        #region Methoden – Export

        /// <summary>
        /// Exportiert die aktuelle Kundenliste nach Excel.
        /// </summary>
        private async Task ExportiereKundenExcelAsync()
        {
            try
            {
                if (KundenListe == null || KundenListe.Count == 0)
                {
                    MessageBox.Show("Keine Kunden zum Exportieren vorhanden.",
                                    "Information",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                    return;
                }

                string basisOrdner = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string dateiName = $"Kundenliste_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                string zielPfad = System.IO.Path.Combine(basisOrdner, dateiName);

                await Task.Run(() =>
                {
                    _excelExportService.ExportKundenListeAlsExcel(
                        KundenListe.ToList(),
                        "ERP System - Kundenverwaltung",
                        zielPfad);
                });

                MessageBox.Show($"Kundenliste wurde erfolgreich nach Excel exportiert:\n{zielPfad}",
                                "Erfolg",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Exportieren der Kundenliste nach Excel:\n{ex.Message}",
                                "Fehler",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Exportiert die aktuelle Kundenliste nach PDF.
        /// </summary>
        public async Task ExportiereKundenNachPdfAsync()
        {
            try
            {
                if (KundenListe == null || KundenListe.Count == 0)
                {
                    MessageBox.Show("Keine Kunden zum Exportieren vorhanden.",
                                    "Information",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                    return;
                }

                string basisOrdner = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string dateiName = $"Kundenliste_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                string zielPfad = System.IO.Path.Combine(basisOrdner, dateiName);

                await Task.Run(() =>
                {
                    _pdfExportService.ExportKundenListeAlsPdf(
                        KundenListe,
                        "ERP System - Kundenverwaltung",
                        zielPfad);
                });

                MessageBox.Show($"Kundenliste wurde erfolgreich nach PDF exportiert:\n{zielPfad}",
                                "Erfolg",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Exportieren der Kundenliste nach PDF:\n{ex.Message}",
                                "Fehler",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        #endregion

        #region Validierungslogik

        /// <summary>
        /// Prüft, ob ein Kunde hinzugefügt werden kann.
        /// </summary>
        private bool KannHinzufügen()
        {
            return !string.IsNullOrWhiteSpace(AusgewählterKunde?.Name)
                   && !string.IsNullOrWhiteSpace(AusgewählterKunde?.Email);
        }

        /// <summary>
        /// Prüft, ob der ausgewählte Kunde aktualisiert werden kann.
        /// </summary>
        private bool KannAktualisieren()
        {
            return AusgewählterKunde != null && AusgewählterKunde.KundeID > 0;
        }

        /// <summary>
        /// Prüft, ob der ausgewählte Kunde gelöscht werden kann.
        /// </summary>
        private bool KannLöschen()
        {
            return AusgewählterKunde != null && AusgewählterKunde.KundeID > 0;
        }

        /// <summary>
        /// Prüft, ob ein Export möglich ist.
        /// </summary>
        private bool KannExportieren()
        {
            return KundenListe != null && KundenListe.Count > 0;
        }

        #endregion
    }
}