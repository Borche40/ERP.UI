
using ERP.Core.Services;
using ERP.Data.Models;
using ERP.Data.Controllers;
using ERP.UI.Commands;
using ERP.UI.Services;
using System;
using System.Linq;
using System.Windows.Input;

namespace ERP.UI.ViewModel
{
    /// <summary>
    /// ViewModel für den ProduktDialog – 
    /// Unterstützt sowohl das Erstellen als auch das Bearbeiten von Produkten.
    /// </summary>
    public class ProduktDialogViewModel : BaseViewModel
    {
        private readonly IDialogService _dialogService;
        private readonly ProdukteService _produkteService;
        private bool _istBearbeitenModus;
        private Produkte _produkt;
       
        // ───────── Eigenschaften ─────────
        private int _produktId;
        private string _bildPfad;
        private string _name;
        private string _kategorie;
        private decimal _preis;
        private int _lagerbestand;
        private DateTime? _ablaufdatum;
        private string _aktuellerBenutzerId = "0"; // Platzhalter, in einer echten Anwendung würde dies dynamisch gesetzt werden
        private string _aktuellerBenutzerName = "Unbekannt"; // Platzhalter, in einer echten Anwendung würde dies dynamisch gesetzt werden
        //Originalzustand für Änderungsvergleich
        private string _originalName = string.Empty;
        private string _originalKategorie = string.Empty;
        private decimal _originalPreis;
        private int _originalLagerbestand;
        private DateTime? _originalAblaufdatum;
        private string _originalBildPfad = string.Empty;

        /// <summary>
        /// Ruckt die ProduktID zurück oder legt sie fest.
        /// </summary>
        public int ProduktID
        {
            get => _produktId;
            set { _produktId = value; OnPropertyChanged(); }
        }
        /// <summary>
        /// Bildpfad des Produkts, der im Dialog angezeigt wird.
        /// </summary>
        public string BildPfad
        {
            get => _bildPfad;
            set {
                _bildPfad = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(IstBildVorhanden)); 
                OnPropertyChanged(nameof(HatÄnderungen));
                OnPropertyChanged(nameof(IstEingabeGültig));
            }
        }
        /// <summary>
        /// Name des Produkts, der im Dialog angezeigt und bearbeitet wird.
        /// </summary>
        public string Name
        {
            get => _name;
            set { 
                _name = value;
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(HatÄnderungen));
                OnPropertyChanged(nameof(IstEingabeGültig));
                (SpeichernCommand as Befehl)?.RaiseCanExecuteChanged();
            }
        }
        /// <summary>
        /// Kategorie des Produkts, die im Dialog angezeigt und bearbeitet wird.
        /// </summary>
        public string Kategorie
        {
            get => _kategorie;
            set { 
                _kategorie = value; 
                OnPropertyChanged();
                OnPropertyChanged(nameof(HatÄnderungen));
                OnPropertyChanged(nameof(IstEingabeGültig));
                (SpeichernCommand as Befehl)?.RaiseCanExecuteChanged();

            }
        }

        /// <summary>
        /// Preis des Produkts, der im Dialog angezeigt und bearbeitet wird.
        /// </summary>
        public decimal Preis
        {
            get => _preis;
            set { 
                _preis = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(HatÄnderungen));
                OnPropertyChanged(nameof(IstEingabeGültig));
                (SpeichernCommand as Befehl)?.RaiseCanExecuteChanged();
            }
        }
        /// <summary>
        /// Lagerbestand des Produkts, der im Dialog angezeigt und bearbeitet wird.
        /// </summary>
        public int Lagerbestand
        {
            get => _lagerbestand;
            set { 
                _lagerbestand = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HatÄnderungen));
                OnPropertyChanged(nameof(IstEingabeGültig));
                (SpeichernCommand as Befehl)?.RaiseCanExecuteChanged();
            }
        }
        /// <summary>
        /// Das Ablaufdatum des Produkts, das im Dialog angezeigt und bearbeitet wird.
        /// </summary>
        public DateTime? Ablaufdatum
        {
            get => _ablaufdatum;
            set { 
                
                _ablaufdatum = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(HatÄnderungen));
                OnPropertyChanged(nameof(IstEingabeGültig));
                (SpeichernCommand as Befehl)?.RaiseCanExecuteChanged();


            }
            
        }
        /// <summary>
        /// ID des aktuell angemeldeten Benutzers, die für Audit-Zwecke verwendet wird.
        /// </summary>
        public string AktuellerBenutzerId
        {
            get => _aktuellerBenutzerId;
            set
            {
                _aktuellerBenutzerId = value;
                OnPropertyChanged();
            }
        }


        /// <summary>
        /// Anzeigename des aktuell angemeldeten 
        /// Benutzers, der für Audit-Zwecke verwendet wird.
        /// </summary>
        public string AktuellerBenutzerName
        {
            get => _aktuellerBenutzerName;
            set
            {
                _aktuellerBenutzerName = value;
                OnPropertyChanged();
            }
        }


        /// <summary>
        /// Gibt zurück, ob ein Bild für das Produkt vorhanden ist.
        /// </summary>
        public bool IstBildVorhanden => !string.IsNullOrEmpty(BildPfad);
        /// <summary>
        /// Gibt an, ob der aktuelle Zustand des Dialogs
        /// vom ursprünglich geladenen Zustand abweicht.
        /// Diese Eigenschaft wird später verwendet,
        /// um den Speichern-Button nur bei echten Änderungen zu aktivieren.
        /// </summary>
        public bool HatÄnderungen =>
            (Name ?? string.Empty) != _originalName ||
            (Kategorie ?? string.Empty) != _originalKategorie ||
            Preis != _originalPreis ||
            Lagerbestand != _originalLagerbestand ||
            Ablaufdatum != _originalAblaufdatum ||
            (BildPfad ?? string.Empty) != _originalBildPfad;


        /// <summary>
        /// Gibt an,ob die aktuellen Eingaben grundsätzlich gültig sind, um das Produkt zu speichern.
        /// Diese Eigenschaft wird verwendet für die Aktivierung
        /// des Speichern-Buttons, damit der Benutzer erst dann speichern kann,
        /// wenn alle Eingaben gültig sind.
        /// </summary>
        public bool IstEingabeGültig =>
            !string.IsNullOrWhiteSpace(Name) &&
            !string.IsNullOrWhiteSpace(Kategorie) &&
            Preis > 0 &&
            Lagerbestand >= 0 && Ablaufdatum != null;


        // ───────── Commands ─────────
        public ICommand BildAuswählenCommand { get; }
        public ICommand SpeichernCommand { get; }
        public ICommand AbbrechenCommand { get; }

        // ───────── Konstruktor ─────────
        public ProduktDialogViewModel(IDialogService dialogService, ProdukteService 
            produkteService, Produkte? produkt = null)
        {
            _dialogService = dialogService;
            _produkteService = produkteService;

            // Prüfen, ob Bearbeitungsmodus oder Neuerstellung
            if (produkt != null)
            {
                _istBearbeitenModus = true;
                ProduktID = produkt.ProduktID;
                Name = produkt.ProduktName!;
                Kategorie = produkt.Kategorie;
                Preis = produkt.Preis ?? 0;
                Lagerbestand = produkt.Lagerbestand;
                Ablaufdatum = produkt.AblaufDatum;
                BildPfad = produkt.BildPfad!;
            }
            else
            {
                _istBearbeitenModus = false;
                Preis = 0.0m;
            }

            BildAuswählenCommand = new Befehl(_ => BildAuswählen());
            SpeichernCommand = new Befehl(async _ => await Speichern(), _=> HatÄnderungen);
            AbbrechenCommand = new Befehl( _=> Abbrechen());
            OriginalzustandSpeichern();
        }

        // ───────── Methoden ─────────
        /// <summary>
        /// Produkt, das im Dialog bearbeitet oder erstellt wird.
        /// </summary>
        public Produkte Produkt
        {
            get => _produkt;
            set
            {
                this._produkt = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Öffnet einen Datei-Dialog zum Auswählen eines Produktbildes.
        /// </summary>
        private void BildAuswählen()
        {
            var pfad = _dialogService.PickImageFile();
            if (pfad != null)
                BildPfad = pfad;
        }


        /// <summary>
        /// Speichert den ursprünglichen Zustand des Dialogs.
        /// Dieser Zustand wird später verwendet, um festzustellen,
        /// ob der Benutzer tatsächlich Änderungen vorgenommen hat.
        /// </summary>
        private void OriginalzustandSpeichern()
        {
            _originalName = Name ?? string.Empty;
            _originalKategorie = Kategorie ?? string.Empty;
            _originalPreis = Preis;
            _originalLagerbestand = Lagerbestand;
            _originalAblaufdatum = Ablaufdatum;
            _originalBildPfad = BildPfad ?? string.Empty;
        }

        /// <summary>
        /// Prüft die Benutzereingaben zentral vor dem Speichern,
        /// Gibt <c>true</c> zurück wenn alle Eingabe gültig sind, andernfalls <c>false</c>.
        /// </summary>
        /// <returns></returns>
        private bool ValidiereEingaben()
        {
            if(string.IsNullOrWhiteSpace(Name))
            {
                _dialogService.ShowError("Der Name des Produkts darf nicht leer sein.", "Ungültige Eingabe");//Validierung: Name darf nicht leer sein
                return false;
            }
            if(string.IsNullOrWhiteSpace(Kategorie))
            {
                _dialogService.ShowError("Die Kategorie des Produkts darf nicht leer sein.", "Ungültige Eingabe");//Validierung: Kategorie darf nicht leer sein
                return false;
            }
            if(Preis <= 0)
            {
                _dialogService.ShowError("Der Preis des Produkts muss größer als 0 sein.", "Ungültige Eingabe");//Validierung: Preis muss größer als 0 sein
                return false;
            }
            if(Lagerbestand < 0)
            {
                _dialogService.ShowError("Der Lagerbestand des Produkts darf nicht negativ sein.", "Ungültige Eingabe");//Validierung: Lagerbestand darf nicht negativ sein
                return false;
            }
            return true;
        }
        /// <summary>
        /// Speichert oder aktualisiert das Produkt in der Datenbank.
        /// Im Neuanlage-Modus wird die asynchrone Insert-Methode verwendet,
        /// damit auch der Audit-Eintrag korrekt geschrieben wird.
        /// </summary>
        private async Task Speichern()
        {
            try
            {
                if(!ValidiereEingaben())
                    return;

                var produkt = new Produkte
                {
                    ProduktID = ProduktID,
                    ProduktName = Name,
                    Kategorie = Kategorie,
                    Preis = Preis,
                    Lagerbestand = Lagerbestand,
                    AblaufDatum = Ablaufdatum,
                    BildPfad = BildPfad,
                    ErstelltAm = DateTime.Now
                };

                if (_istBearbeitenModus)
                {
                    bool aktualisiert = await _produkteService.ProduktAktualisierenAsync(produkt,AktuellerBenutzerId,AktuellerBenutzerName);

                    if (aktualisiert)
                    {
                        _dialogService.ShowInfo("Produkt wurde erfolgreich aktualisiert!", "Erfolg");
                        Schließen(true);
                    }
                    else
                    {
                        string fehlerText = _produkteService.LetzteValidierungsfehler.Any()
                            ? string.Join(Environment.NewLine, _produkteService.LetzteValidierungsfehler) :
                            "Produkt konnte nicht aktualisiert werden.";




                        _dialogService.ShowError(fehlerText,"Validierungsfehler");
                    }
                }
                else
                {
                    int neueProduktId = await _produkteService.ProduktHinzufügenAsync(produkt,AktuellerBenutzerId,AktuellerBenutzerName);

                    if (neueProduktId > 0)
                    {
                        _dialogService.ShowInfo("Neues Produkt wurde erfolgreich hinzugefügt!", "Erfolg");
                        Schließen(true);
                    }
                    else
                    {
                        string fehlerText = _produkteService.LetzteValidierungsfehler.Any() ?
                            string.Join(Environment.NewLine, _produkteService.LetzteValidierungsfehler) :
                            "Produkt konnte nicht gespeichert werden,";




                        _dialogService.ShowError(fehlerText,"Validierungsfehler");
                    }
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Fehler beim Speichern:\n{ex.Message}", "Fehler");
            }
        }
       

        /// <summary>
        /// Schließt den Dialog ohne Änderungen.
        /// </summary>
        private void Abbrechen() => Schließen(false);
        /// <summary>
        /// Schließt den Dialog und gibt an, ob die Änderungen gespeichert wurden (true) oder nicht (false).
        /// </summary>
        /// <param name="result">Gibt an, ob die Änderungen gespeichert wurden (true) oder nicht (false).</param>
        private void Schließen(bool? result)
        {
            var window = System.Windows.Application.Current.Windows
                .OfType<System.Windows.Window>()
                .FirstOrDefault(w => w.DataContext == this);

            if (window != null)
                window.DialogResult = result;
            window.Close();
        }
    }
}
