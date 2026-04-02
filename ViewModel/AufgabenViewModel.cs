using DocumentFormat.OpenXml.Drawing;
using ERP.Core.Services;
using ERP.Data;
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
using System.Linq;

namespace ERP.UI.ViewModel
{
    /// <summary>
    /// ViewModel zur Verwaltung der Aufgaben im ERP-System.
    /// Verantwortlich für:
    /// ✔ Laden aller Aufgaben
    /// ✔ Filtern nach Suchtext und Status
    /// ✔ Öffnen des Dialogs zum Erstellen/Bearbeiten
    /// </summary>
    public class AufgabenViewModel : BaseViewModel
    {
      
        // Felder
   
      
        private readonly AufgabenService _aufgabenService;
        private readonly IDialogService _dialogService;
        private ObservableCollection<AufgabenKommentare> _kommentare = new();


        private Aufgabe? _ausgewählteAufgabe;
        private string _suchtext = string.Empty;
        private bool _zeigeNurOffene;
        private bool _istLaden;
        public ObservableCollection<Aufgabe> AufgabenListe { get; } = new();
        private string _ausgewählterStatusFilter = "Alle";
        private string _neuerkommentarText = string.Empty;
        private AufgabenKommentare? _ausgewählterKommentar;


   
        // Öffentliche Eigenschaften
     
        /// <summary>
        /// Kommentare der ausgewählten Aufgabe.
        /// </summary>
        public ObservableCollection<AufgabenKommentare> Kommentare
        {
            get => _kommentare;
          private  set { _kommentare = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Neu hinzuzufügender Kommentar-Text.
        /// </summary>
        public string NeuerKommentarText
        {
            get => _neuerkommentarText;
            set 
            { 
                _neuerkommentarText = value; 
                OnPropertyChanged();
                (KommentarHinzufügenCommand as Befehl)?.RaiseCanExecuteChanged();
            }
        }
        /// <summary>
        /// Vom Benutzer ausgewählte Aufgabe im Grid.
        /// </summary>
        public Aufgabe? AusgewählteAufgabe
        {
            get => _ausgewählteAufgabe;
            set
            {
                if (_ausgewählteAufgabe == value)
                    return;
                _ausgewählteAufgabe = value;
                OnPropertyChanged();

                //Aktualisiert commands
                (KommentarHinzufügenCommand as Befehl)?.RaiseCanExecuteChanged();
                (KommentarLöschenCommand as Befehl)?.RaiseCanExecuteChanged();

                //Lesen die Kommentaren
                _ = LadeKommentareAsync();
            
            }

         
        }


       

        //TDO
        public AufgabenKommentare? AusgewählterKommentar
        {
            get => _ausgewählterKommentar;
            set { _ausgewählterKommentar = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Suchtext zur Filterung nach Titel/Beschreibung.
        /// Bei Änderung wird die Aufgabenliste neu geladen (mit Filter).
        /// </summary>
        public string Suchtext
        {
            get => _suchtext;
            set
            {
                _suchtext = value;
                OnPropertyChanged();


                _= LadeAufgabenAsync();//Liste neu laden mit Filter greifen
            }
        }

        /// <summary>
        /// Status-Filteroptionen für die Aufgabenliste.
        /// </summary>
        public ObservableCollection<string> StatusFilterOptionen { get; } = new ObservableCollection<string>
        {
            "Alle",
            "Offen",
            "In Bearbeitung",
            "Erledigt",
            "Überfällig"
            
        };
        /// <summary>
        /// Ruft die Aufgaben mit dem ausgewählten Status-Filter neu auf.
        /// oder legt den Filter auf "Alle" fest, um alle Aufgaben anzuzeigen.
        /// </summary>
        public string AusgewählterStatusFilter
        {
            get => _ausgewählterStatusFilter;
            set
            {
                if (_ausgewählterStatusFilter == value)
                    return;
                _ausgewählterStatusFilter = value;
                OnPropertyChanged();

                _= LadeAufgabenAsync();
            }
        }
        /// <summary>
        /// Wenn aktiv: nur offene Aufgaben anzeigen (Status != 3).
        /// </summary>
        public bool ZeigeNurOffene
        {
            get => _zeigeNurOffene;
            set
            {
                if(_zeigeNurOffene == value)
                    return;
                _zeigeNurOffene = value;
                OnPropertyChanged();
                _= LadeAufgabenAsync();
            }
        }

        /// <summary>
        /// True = Aufgaben werden geladen → Busy-Indicator im UI.
        /// </summary>
        public bool IstLaden
        {
            get => _istLaden;
            private set { _istLaden = value; OnPropertyChanged(); }
        }



        
        // Commands
       

        public ICommand AktualisierenCommand { get; }
        public ICommand NeueAufgabeCommand { get; }
        public ICommand BearbeitenCommand { get; }
        public ICommand LöschenCommand { get; }
        public ICommand MarkiereAlsErledigtCommand { get; }

        public ICommand KommentarHinzufügenCommand { get; }
        public ICommand KommentarLöschenCommand { get; }

        // Konstruktor

        /// <summary>
        /// Konstruktor mit Dependency Injection für DialogService und AufgabenService.
        /// </summary>
        /// <param name="dialogService">Der DialogService für die Anzeige von Dialogen.</param>
        /// <param name="aufgabenService">Der AufgabenService für die Verwaltung von Aufgaben.</param>
        public AufgabenViewModel(IDialogService dialogService, AufgabenService aufgabenService)
        {
            _dialogService = dialogService;
            _aufgabenService = aufgabenService;

          


            AktualisierenCommand = new Befehl(async _ => await LadeAufgabenAsync());
            NeueAufgabeCommand = new Befehl(async _ => await ÖffneNeueAufgabeDialog());
            BearbeitenCommand = new Befehl(_ => ÖffneBearbeitenDialog(), _ => AusgewählteAufgabe != null);
            LöschenCommand = new Befehl(async _ => await LöscheAusgewählteAufgabe(), _ => AusgewählteAufgabe != null);
            MarkiereAlsErledigtCommand = new Befehl(async _ => await MarkiereAlsErledigt(), _ => AusgewählteAufgabe != null);   
           
            KommentarHinzufügenCommand = new Befehl(async _ => await ErstelleKommentarAsync());
            KommentarLöschenCommand = new Befehl(async _ => await LöscheKommentarAsync(), _ => AusgewählterKommentar != null);

            _= LadeAufgabenAsync();

        }



      
        // Dialoge öffnen
      

        /// <summary>
        /// Öffnet den Dialog zum Erstellen einer neuen Aufgabe.
        /// </summary>
        private async Task  ÖffneNeueAufgabeDialog()
        {
           var dialog = new NeueAufgabeDialog(_dialogService, _aufgabenService, null);

            dialog.Owner = Application.Current.MainWindow;


            if (dialog.ShowDialog() == true)
            {
                await LadeAufgabenAsync();

                if(dialog.DataContext is NeueAufgabeDialogViewModel vm && vm.CreatedAufgabenId > 0)
                {
                    AusgewählteAufgabe = AufgabenListe.FirstOrDefault(a => a.AufgabenID == vm.CreatedAufgabenId);
                }
            }
               


        }


        /// <summary>
        /// Öffnet den Dialog zum Bearbeiten der ausgewählten Aufgabe.
        /// </summary>
        private async void ÖffneBearbeitenDialog()
        {
            if (AusgewählteAufgabe == null)
                return;

            //sobald bearbeiten geklickt wird, In Bearbeitung auf true setzen

            if(AusgewählteAufgabe.Status != 3 && AusgewählteAufgabe.Status != 2)//nur wenn die Aufgabe nicht bereits erledigt oder in Bearbeitung ist, wird der Status geändert
            {
                await _aufgabenService.SetzeInBearbeitungAsync(AusgewählteAufgabe.AufgabenID);//Ändert den Status in der Datenbank

                //lokal setzen damit UI sofort aktualisiert wird
                AusgewählteAufgabe.Status = 2;
            }

            var dialog = new NeueAufgabeDialog(_dialogService, _aufgabenService, AusgewählteAufgabe);//Wenn eine Aufgabe ausgewählt ist,
                                                                                                     //wird sie an den Dialog übergeben,
                                                                                                     //damit die Felder mit den aktuellen Werten
                                                                                                     //gefüllt werden können

            dialog.Owner = Application.Current.MainWindow;//Setzt das Hauptfenster als Besitzer des Dialogs, damit er modal ist

            if (dialog.ShowDialog() == true)
                await LadeAufgabenAsync();
        }



        // ───────────────────────────────────────────────
        // CRUD-Funktionen
        // ───────────────────────────────────────────────

        /// <summary>
        /// Löscht die aktuell ausgewählte Aufgabe (mit Nachfrage).
        /// </summary>
        private async Task LöscheAusgewählteAufgabe()
        {
            if (AusgewählteAufgabe == null)
                return;

            bool bestätigen = _dialogService.Confirm("Soll diese Aufgabe wirklich gelöscht werden?", "Löschen bestätigen");


            if (!bestätigen)
                return;


            await _aufgabenService.LöscheAufgabeAsync(AusgewählteAufgabe.AufgabenID);

            AufgabenListe.Remove(AusgewählteAufgabe);
            AusgewählteAufgabe = null!;
        }



        // ───────────────────────────────────────────────
        // Laden & Filtern
        // ───────────────────────────────────────────────

        /// <summary>
        /// Lädt alle Aufgaben aus der Datenbank und wendet aktive Filter an.
        /// </summary>
        private async Task LadeAufgabenAsync()
        {
            try
            {
                IstLaden = true;


                var alle = await _aufgabenService.LadeAlleAufgabenAsync();//Ruft alle Aufgaben aus der Datenbank ab (unabhängig von Filtern)

                AufgabenListe.Clear();
                foreach (var aufgabe in FilterIntern(alle))
                {
                    AufgabenListe.Add(aufgabe);
                }

                AktualisiereTagTexte(alle);

               
            }
            

            finally
            {
                IstLaden = false;
            }
        }

        /// <summary>
        /// Lädt die Kommentare für die ausgewählte Aufgabe.
        /// </summary>
        /// <returns></returns>
        private async Task LadeKommentareAsync()
        {
            try
            {
                if(AusgewählteAufgabe == null)
                {
                    Kommentare.Clear();
                    return;
                }
                var liste = await _aufgabenService.GetAufgabenKommentareAsync(AusgewählteAufgabe.AufgabenID);

                if (liste == null)
                    return;

                Kommentare.Clear();
                foreach(var kommentar in liste)
                {
                    Kommentare.Add(kommentar);
                }
                Console.WriteLine($"DEBUG:Geladene Kommentare = {Kommentare.Count}");
            }
            catch(Exception ex)
            {
                _dialogService.ShowError("Fehler beim Laden der Kommentare: " + ex.Message, "Fehler");
            }
        }
        /// <summary>
        /// Erstellt einen neuen Kommentar für die ausgewählte Aufgabe.
        /// </summary>
        /// <returns></returns>
        private async Task ErstelleKommentarAsync()
        {
            if (AusgewählteAufgabe == null)
                return;
            if (string.IsNullOrWhiteSpace(NeuerKommentarText))
                return;
            try
            {
                var kommentar = new AufgabenKommentare
                {
                    AufgabeID = AusgewählteAufgabe.AufgabenID,
                    BenutzerID = 1,//TODO später echte benutzerId ersetzen
                    KommentarText = NeuerKommentarText.Trim(),
                    ErstelltAm = DateTime.Now
                };

                int neuerId = await _aufgabenService.ErstelleKommentareAsync(kommentar);

                if (neuerId > 0) kommentar.KommentarID = neuerId;

                //UI Aktualisieren
                Kommentare.Add(kommentar);
                //Löscht textbox
                NeuerKommentarText = string.Empty;

              //  await LadeKommentareAsync();
            }
            catch(Exception ex)
            {
                _dialogService.ShowError("Fehler beim Speichern des Kommentars: " + ex.Message, "Fehler");
                
            }
        }




        /// <summary>
        /// Löscht den ausgewählten Kommentar.
        /// </summary>
        /// <returns></returns>
        private async Task LöscheKommentarAsync()
        {
            if(AusgewählterKommentar == null)
                return;
           bool bestätigen = _dialogService.Confirm("Möchten Sie diesen Kommentar wirklich löschen?", "Kommentar löschen");
            if (!bestätigen)
                return;
            try
            {
                await _aufgabenService.LöscheKommentarAsync(AusgewählterKommentar.KommentarID);
                Kommentare.Remove(AusgewählterKommentar);

                AusgewählterKommentar = null;
                await LadeKommentareAsync();
            }
            catch(Exception ex)
            {
                _dialogService.ShowError("Fehler beim Löschen des Kommentars: " + ex.Message, "Fehler");
            }
        }

        /// <summary>
        /// Aktualisiert die anzuzeigenden Tag-Texte auf Basis der Priorität.
        /// Die Tags werden NICHT aus der Datenbank gelesen, sondern nur für das UI berechnet.
        /// Beispiel:
        /// - PrioritätText = "Niedrig"  -> TagText = "Low"
        /// - PrioritätText = "Mittel"   -> TagText = "Normal"
        /// - PrioritätText = "Hoch"     -> TagText = "High"
        /// - PrioritätText = "Kritisch" -> TagText = "Critical"
        /// </summary>
        /// <param name="aufgaben">Liste der geladenen Aufgaben.</param>
        private void AktualisiereTagTexte(IEnumerable<Aufgabe> aufgaben)
        {
            if (aufgaben == null)
                return;

            foreach (var a in aufgaben)
            {
                // Falls der Text aus der DB kommt, erstmal säubern
                var prioritaet = (a.PrioritätText ?? string.Empty).Trim();

                a.TagText = prioritaet switch
                {
                    "Niedrig" => "Low",
                    "Mittel" => "Normal",
                    "Hoch" => "High",
                    "Kritisch" => "Critical",

                    // Wenn nichts gesetzt: ein neutraler Standard-Tag
                    "" => "Allgemein",

                    // Fallback: nimm einfach den Originaltext als Tag
                    _ => prioritaet
                };
            }
        }


        /// <summary>
        /// Zentrale Filterlogik (Titel, Beschreibung, Status).
        /// </summary>
        private IEnumerable<Aufgabe> FilterIntern(IEnumerable<Aufgabe> daten)
        {
            // Nach Status filtern

            switch(AusgewählterStatusFilter)
            {
                case "Offen":
                    daten = daten.Where(a => a.Status == 1 && !a.IstÜberfällig);//Offen bedeutet hier: Status = 1 und nicht überfällig
                    break;
                case "In Bearbeitung":
                    daten = daten.Where(a => a.Status == 2);//In Bearbeitung bedeutet hier: Status = 2
                    break;
                case "Erledigt":
                    daten = daten.Where(a => a.Status == 3);//Erledigt bedeutet hier: Status = 3
                    break;
                case  "Überfällig":
                    daten = daten.Where(a => a.IstÜberfällig);//Überfällig bedeutet hier: IstÜberfällig = true
                    break;
            }


            // Nur offene Aufgaben?
            if (ZeigeNurOffene)
                daten = daten.Where(a => a.Status != 3);//Nur offene Aufgaben bedeutet hier: Status != 3

            // Nach Suchtext filtern
            if (!string.IsNullOrWhiteSpace(Suchtext))//Wenn ein Suchtext eingegeben wurde, wird nach diesem Text in Titel und Beschreibung gefiltert (Groß-/Kleinschreibung wird ignor
            {
                daten = daten.Where(a =>
                    (!string.IsNullOrEmpty(a.Titel) &&
                     a.Titel.Contains(Suchtext, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(a.Beschreibung) &&
                     a.Beschreibung.Contains(Suchtext, StringComparison.OrdinalIgnoreCase)));
            }

            return daten;
        }
        /// <summary>
        /// Markiert die ausgewählte Aufgabe als erledigt.
        /// </summary>
        /// <returns></returns>
        private async Task MarkiereAlsErledigt()
        {
            if (AusgewählteAufgabe == null)
                return;
            if(!_dialogService.Confirm("Möchten Sie diese Aufgabe als erledigt markieren?", "Aufgabe erledigen"))
                return;

            await _aufgabenService.MarkiereAlsErledigAsync(AusgewählteAufgabe.AufgabenID);//Ändert den Status in der Datenbank

            await LadeAufgabenAsync();


        }
    }
}
