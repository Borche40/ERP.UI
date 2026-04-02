using DocumentFormat.OpenXml.Drawing;
using ERP.Core.Services;
using ERP.Data.Controllers;
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
    /// ViewModel für die Verwaltung und Anzeige aller Produkte im ERP-System.
    /// Verantwortlich für das Laden, Hinzufügen, Bearbeiten, Löschen
    /// und Exportieren von Produktdaten.
    /// </summary>
    public class ProdukteViewModel : BaseViewModel
    {
        // ───────── Felder ─────────
        private readonly ProdukteService _produkteService;// Zugriff auf die Produktdaten über den Service
        private readonly IDialogService _dialogService;// Service für die Anzeige von Dialogen (Fehler, Bestätigung, etc.)
        private readonly PdfExportService? _pdfExportService = new();// Service für den PDF-Export, optional, da er nur für den Export benötigt wird
        private bool _istAmLaden;


        private ObservableCollection<Produkte> _produkteListe;// Lokale Kopie der Produktliste, die im DataGrid angezeigt wird
        private Produkte _ausgewähltesProdukt;// Das aktuell ausgewählte Produkt im DataGrid
        private string _suchtext;// Text für die Produktsuche

        // ───────── Eigenschaften ─────────

        /// <summary>
        /// Enthält die aktuelle Liste aller Produkte (für das DataGrid).
        /// </summary>
        public ObservableCollection<Produkte> ProdukteListe 
        {
            get => _produkteListe;// Gibt die aktuelle Produktliste zurück
            set { _produkteListe = value; OnPropertyChanged(); }// Setzt die Produktliste und benachrichtigt
                                                                // die View über die Änderung, damit das DataGrid aktualisiert wird
        }

       
        /// <summary>
        /// Repräsentiert das aktuell ausgewählte Produkt im DataGrid.
        /// </summary>
        public Produkte AusgewähltesProdukt
        {
            get => _ausgewähltesProdukt;
            set { _ausgewähltesProdukt = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Text für die Produktsuche – filtert dynamisch nach Name oder Kategorie.
        /// </summary>
        public string Suchtext
        {
            get => _suchtext;
            set
            {
                _suchtext = value;
                OnPropertyChanged();
                FilterProdukte();
            }
        }


        /// <summary>
        /// Gibt an,ob aktuell ein Ladevorgang für die
        /// Aktualisierung läuft.
        /// 
        /// diese Eigenschaft wird für den Spinner im Aktualisieren-Button 
        /// verwendet.
        /// </summary>
        public bool IstAmLaden
        {
            get => _istAmLaden;
            set
            {
                this._istAmLaden = value;
                this.OnPropertyChanged();

                CommandManager.InvalidateRequerySuggested();
            }
        }

        // ───────── Commands ─────────
        public ICommand NeuesProduktCommand { get; }
        public ICommand LöschenCommand { get; }
        public ICommand AktualisierenCommand { get; }
        public ICommand ExportPdfCommand { get; } 
        public ICommand ExportExcelCommand { get; }
        public ICommand BearbeitenCommand { get; }
        public ICommand AuditCommdand { get; }
      

        // ───────── Konstruktor ─────────
        public ProdukteViewModel(IDialogService dialogService, ProdukteService produkteService)
        {
            _dialogService = dialogService;
            _produkteService = produkteService;
            _pdfExportService = new PdfExportService();

            NeuesProduktCommand = new Befehl(async _ => await NeuesProdukt());// Asynchrone Methode zum Hinzufügen eines neuen Produkts
            LöschenCommand = new Befehl(_ => Löschen(), _ => AusgewähltesProdukt != null);// Methode zum Löschen des ausgewählten Produkts, nur aktiv wenn ein Produkt ausgewählt ist
            AktualisierenCommand = new Befehl(async _ => await AktualisiereProdukteMitSpinnerAsync(), _=> !IstAmLaden);
            ExportPdfCommand = new Befehl( async async_ =>  await ExportProdukteListeAlsPdf(),_=> ProdukteListe != null && ProdukteListe.Count > 0 );
            ExportExcelCommand = new Befehl(_ => ExportExcel());
            BearbeitenCommand = new Befehl(async _=> await Bearbeiten(),
                _=> AusgewähltesProdukt != null);
            AuditCommdand = new Befehl(_ => ÖffneAuditDialog());

          _= LadeProdukte();
        }

        //Methoden
        
       
        /// <summary>
        /// Lädt alle Produkte aus der Datenbank.
        /// </summary>
        private async Task LadeProdukte()
        {
            try
            {
                var liste = await _produkteService.LadeAlleProdukteAsync();// Asynchrone Methode, um alle Produkte aus
                                                                           // der Datenbank zu laden
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ProdukteListe = new ObservableCollection<Produkte>(liste);// Aktualisiert die Produktliste im UI-Thread,
                                                                              // damit das DataGrid die neuen Daten anzeigt
                }); 


                
            }
            catch(Exception ex)
            {
                _dialogService.ShowError($"Fehler beim Laden der Produkte:\n{ex.Message}", "Fehler");
            }
        }

        /// <summary>
        /// Öffnet den Dialog zum Hinzufügen eines neuen Produkts.
        /// </summary>
        private async Task NeuesProdukt()
        {
            try
            {
                var dialog = new ProduktDialog(_dialogService, _produkteService, null);

                bool? result = dialog.ShowDialog();

                if(result == true)
                {
                  await LadeProdukte();
                }
            }
            catch(Exception ex)
            {
                _dialogService.ShowError($"Fehler beim Öffnen das Produktdialogs:\n{ex.Message}","Fehler");
            }
        }
        /// <summary>
        /// Öffnet den Dialog zum Bearbeiten
        /// des ausgewählten Produkts.
        /// </summary>
        /// <returns></returns>
        private async Task Bearbeiten()
        {
            if(AusgewähltesProdukt == null)
            {
                _dialogService.ShowWarning("Bitte" +
                    "wählen Sie ein Produkt aus,das " +
                    "bearbeitet werden soll.", "Hinweis");
                return;
            }

            
            try
            {
                //Eine Kopie erstellen damit der Dialog nicht direkt die Grid-Zeile verändert...
                var produktKopie = new Produkte
                {
                    ProduktID = AusgewähltesProdukt.ProduktID,
                    ProduktName = AusgewähltesProdukt.ProduktName,
                    Kategorie = AusgewähltesProdukt.Kategorie,
                    Preis = AusgewähltesProdukt.Preis,
                    Lagerbestand = AusgewähltesProdukt.Lagerbestand,
                    AblaufDatum = AusgewähltesProdukt.AblaufDatum,
                    BildPfad = AusgewähltesProdukt.BildPfad,
                    Beschreibung = AusgewähltesProdukt.Beschreibung,
                    ErstelltAm = AusgewähltesProdukt.ErstelltAm
                };

                var dialog = new ProduktDialog(_dialogService, _produkteService, produktKopie);
                bool? result = dialog.ShowDialog();
                if(result == true)
                {
                    //Nach erfolgreichem Speicher Liste neu laden
                    await LadeProdukte();
                }
            }
            catch(Exception ex)
            {
                _dialogService.ShowError($"Fehler beim Bearbeiten des Produkts:" +
                    $"\n{ex.Message}", "Fehler");
            }
        }

        /// <summary>
        /// Löscht das ausgewählte Produkt nach Bestätigung.
        /// </summary>
        private async Task Löschen()
        {
            if (AusgewähltesProdukt == null)
            {
                _dialogService.ShowWarning("Bitte wählen Sie ein Produkt aus.", "Hinweis");
                return;
            }

            bool bestätigen = _dialogService.Confirm(
                $"Möchten Sie das Produkt '{AusgewähltesProdukt.ProduktName}' wirklich löschen?",
                "Löschen bestätigen");

            if (!bestätigen)
                return;

            try
            {
                var hauptfensterVm = Application.Current.MainWindow?.DataContext as HauptfensterViewModel;
                var user = hauptfensterVm?.Benutzer?.Authentifizierung?.AktuellerBenutzer;
                string userId = user?.Email ?? "Unbekannt";
                string userName = user?.Name ?? "Unbekannt";



                bool gelöscht = await _produkteService.ProduktLöschenAsync(AusgewähltesProdukt.ProduktID,userId,userName);
                if (gelöscht)
                {
                    _dialogService.ShowError("Produkt wurde erfolgreich gelöscht.", "Erfolg");
                    await LadeProdukte();
                }

                else
                {
                    _dialogService.ShowError("Produkt konnte nicht gelöscht werden.", "Fehler");
                }

               /* _produkteService.DeleteProdukt(AusgewähltesProdukt.ProduktID);
                _dialogService.ShowInfo("Produkt wurde erfolgreich gelöscht.", "Erfolg");//staro pred da se 
                                                                                          //implementira za audit so nego ne go pokazuvase deka e izbrisano...
              await LadeProdukte();*/
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Fehler beim Löschen:\n{ex.Message}", "Fehler");
            }
        }

        /// <summary>
        /// Öffnet den Audit-Dialog für die Anzeige aller 
        /// Audit-Einträge des Produkte-Moduls.
        /// </summary>
        private void ÖffneAuditDialog()
        {
            try
            {
                var dialog = new AuditDialog
                {
                    Owner = Application.Current.MainWindow
                };
                dialog.ShowDialog();
            }
            catch(Exception ex)
            {
                _dialogService.ShowError($"Fehler beim Öffnen des Audit-Dialogs:\n{ex.Message}", "Fehler");
            }
        }
        /// <summary>
        /// Filtert die Produktliste nach dem Suchtext (Name oder Kategorie).
        /// </summary>
        private async void FilterProdukte()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Suchtext))
                {
                  await  LadeProdukte();
                    return;
                }

                var alle = await _produkteService.LadeAlleProdukteAsync();
                var gefiltert = alle.Where(p =>
                    (!string.IsNullOrEmpty(p.ProduktName) && p.ProduktName.Contains(Suchtext, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(p.Kategorie) && p.Kategorie.Contains(Suchtext, StringComparison.OrdinalIgnoreCase))
                );

                ProdukteListe = new ObservableCollection<Produkte>(gefiltert);
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Fehler beim Filtern:\n{ex.Message}", "Fehler");
            }
        }

        /// <summary>
        /// Exportiert die Produktliste als PDF.
        /// (Hier nur Platzhalter – echte Implementierung folgt im Service)
        /// </summary>
        private async Task ExportProdukteListeAlsPdf()
        {
            try
            {
                if (ProdukteListe is null || ProdukteListe.Count == 0)
                {
                    MessageBox.Show("Keine Produkte zum Exportieren vorhanden.",
                                    "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                // Definiere den Speicherort und Dateinamen für die PDF-Datei
                var basis = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var dateiName = $"ProdukteListe_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                var zielPfad = System.IO.Path.Combine(basis, dateiName);

                await Task.Run(() =>
                {
                    _pdfExportService!.ExportProdukteListeAlsPdf(ProdukteListe.ToList(), $"ERP System - Produktverwaltung", zielPfad);
                });

                MessageBox.Show($"✅ Produktliste wurde erfolgreich nach PDF exportiert:\n{zielPfad}",
                                "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Exportieren der Produktliste nach PDF:\n{ex.Message}",
                                "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Exportiert die Produktliste als Excel-Datei.
        /// (Hier nur Platzhalter – echte Implementierung folgt im Service)
        /// </summary>
        private void ExportExcel()
        {
            try
            {
               var excelService = new ExcelExportService();
                excelService.ExportProduktListeAlsExcel(ProdukteListe.ToList(), "Produkte", "Produktübersicht");

                _dialogService.ShowInfo("Excel-Export erfolgreich abgeschlossen.", "Export");
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Fehler beim Excel-Export:\n{ex.Message}", "Fehler");
            }
        }

        /// <summary>
        /// Lädt die Produktliste neu und 
        /// aktiviert dabei den Ladezustand 
        /// für den Aktualisieren-Button.
        /// </summary>
        /// <returns></returns>

        private async Task AktualisiereProdukteMitSpinnerAsync()
        {
            try
            {
                IstAmLaden = true;
                var ladeTask =  LadeProdukte();
                var minDelay = Task.Delay(700);
                await Task.WhenAll(ladeTask, minDelay);
            }
            finally
            {
                IstAmLaden = false;
            }
        }
    }
}
