using ERP.Core.Services;
using ERP.Data;
using ERP.UI.Commands;
using ERP.UI.Services;
using ERP.UI.Models;
using ERP.UI.Views;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.Generic;

namespace ERP.UI.ViewModel
{
    /// <summary>
    /// ViewModel für den NeueAufgabeDialog.
    /// Verantwortlich für:
    /// ✔ Erstellen neuer Aufgaben
    /// ✔ Bearbeiten bestehender Aufgaben
    /// ✔ Validierung & Dialogsteuerung
    /// ✔ Übergabe an AufgabenService (INSERT/UPDATE)
    /// </summary>
    public class NeueAufgabeDialogViewModel : BaseViewModel
    {
        private readonly IDialogService _dialogService;
        private readonly AufgabenService _aufgabenService;
        private readonly NeueAufgabeDialog _dialog;

        /// <summary>
        /// True = Bearbeitungsmodus.
        /// False = Neue Aufgabe wird erstellt.
        /// </summary>
        private readonly bool _istBearbeitenModus;
        /// Originalstatus der Aufgabe (nur im Bearbeitungsmodus relevant, z.B. für Validierung oder Rücksetzen).
        private readonly int _originalStatus;
        
        

        // ───────── Eigenschaften (vom Dialog gebunden) ─────────

        private int _aufgabeId;
        private string _titel = string.Empty;
        private string _beschreibung = string.Empty;
        private PrioritätsLevel _priorität;
        private DateTime? _fälligkeitsDatum;
        private int _kategorie = 1;
        private string _ticketID = string.Empty;
        private Aufgabe _ausgewählteAufgabe;
        private int _createdAufgabenId;


        /// <summary>
        /// 
        /// </summary>
        public Aufgabe AusgewählteAufgabe
        {
            get => _ausgewählteAufgabe;
            set
            {
                this._ausgewählteAufgabe = value;
                this.OnPropertyChanged();

                //Fortschritt Anzeige neu berechnen 
                this.OnPropertyChanged(nameof(Fortschritt));
                this.OnPropertyChanged(nameof(FortschrittText));
            }
        }
        //Private Feld zur Speicherung des Fortschrittwertes 0-100;
        private int _fortschritt;

        /// <summary>
        /// Prozentualer Fortschritt der aktuell ausgewählten Aufgabe 0-100
        /// Diese wert wird im UI z.B. als ProgressBar dargestellt.
        /// </summary>
        public int Fortschritt
        {
            get => _fortschritt;
            set
            {
                //Begrenzung des wertes auf den Bereich von 0-100
                var clamped = Math.Clamp(value, 0, 100);

                //Wenn sich der wert nicht geändert hat,keine Aktualisierung notwendig.
                if (_fortschritt == clamped)
                    return;
                _fortschritt = clamped;

                //Benachrichtig das UI über die Änderung des Frotschrittswertes
                OnPropertyChanged(nameof(Fortschritt));

                //Aktualisiert den Text im UI z.B. Fortschritt:60%
                OnPropertyChanged(nameof(FortschrittText));
            }
        }
        /// <summary>
        /// Formatierter Text für die Anzeige 
        /// im UI z.B Fortschritt:60%
        /// </summary>
        public string FortschrittText => $"Fortschritt:{Fortschritt}%";

        /// <summary>
        /// 
        /// </summary>
        public int CreatedAufgabenId
        {
            get => _createdAufgabenId;
          private  set
            {
                _createdAufgabenId = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Primärschlüssel der Aufgabe</summary>
        public int AufgabeID
        {
            get => _aufgabeId;
            set { _aufgabeId = value; OnPropertyChanged(); }
        }

        /// <summary>Titel der Aufgabe</summary>
        public string Titel
        {
            get => _titel;
            set { _titel = value; OnPropertyChanged(); }
        }

        /// <summary>Beschreibung der Aufgabe</summary>
        public string Beschreibung
        {
            get => _beschreibung;
            set { _beschreibung = value; OnPropertyChanged(); }
        }

        /// <summary>Priorität der Aufgabe (Enum)</summary>
        public PrioritätsLevel Priorität
        {
            get => _priorität;
            set { _priorität = value; OnPropertyChanged(); }
        }

        /// <summary>Fälligkeitsdatum der Aufgabe</summary>
        public DateTime? FälligkeitsDatum
        {
            get => _fälligkeitsDatum;
            set { _fälligkeitsDatum = value; OnPropertyChanged(); }
        }
        /// <summary>
        /// Kategorie der Aufgabe (z.B. 1 = "Allgemein", 2 = "Arbeit", 3 = "Privat").
        /// </summary>
        public int Kategorie
        {
            get => _kategorie;
            set { _kategorie = value; OnPropertyChanged(); }
        }
        /// <summary>
        /// TicketID der Aufgabe (optional, z.B. für Verknüpfung mit externen Systemen oder Referenzen).
        /// </summary>
        public string TicketID
        {
            get => _ticketID;
            set
            {
                if (_ticketID == value)
                    return;
                _ticketID = value;
                this.OnPropertyChanged();

                if(AusgewählteAufgabe != null)
                {
                    AusgewählteAufgabe.TicketID = _ticketID;
                }
            }
        }

        /// <summary>
        /// Liste aller möglichen Prioritäten (für ComboBox).
        /// </summary>
        public Array PrioritätenListe => Enum.GetValues(typeof(PrioritätsLevel));
        /// <summary>
        /// Repräsentiert die verfügbaren Kategorien für Aufgaben.
        /// </summary>
        public List<KatagorieItem> KategorienListe { get; set; } = new List<KatagorieItem>
        {
            new KatagorieItem { Id = 1, Name = "Allgemein" },
            new KatagorieItem { Id = 2, Name = "IT" },
            new KatagorieItem { Id = 3, Name = "Personal" },
            new KatagorieItem { Id = 4, Name = "Finanzen" },
            new KatagorieItem { Id = 5, Name = "Marketing" },
            new KatagorieItem { Id = 6, Name = "Unbekannt" }
        };


        // ───────── Commands ─────────

        public ICommand SpeichernCommand { get; }
        public ICommand AbbrechenCommand { get; }


        // ───────── Konstruktor ─────────
        /// <summary>
        /// Initialisiert das ViewModel.
        /// Wenn bestehendeAufgabe != null → Bearbeiten-Modus.
        /// </summary>
        public NeueAufgabeDialogViewModel(IDialogService dialogService,
                                          AufgabenService aufgabenService,
                                       Aufgabe? bestehendeAufgabe ,NeueAufgabeDialog dialog)
        {
            _dialogService = dialogService;
            _aufgabenService = aufgabenService;
      
             _dialog = dialog;
            if (bestehendeAufgabe != null)
            {
                // ─── Bearbeiten ───
                _istBearbeitenModus = true;

                AufgabeID = bestehendeAufgabe.AufgabenID;
                Titel = bestehendeAufgabe.Titel;
                Beschreibung = bestehendeAufgabe.Beschreibung ?? string.Empty;
                Priorität = (PrioritätsLevel)bestehendeAufgabe.Priorität;
                FälligkeitsDatum = bestehendeAufgabe.FälligkeitsDatum;
                Kategorie = bestehendeAufgabe.Kategorie;
                TicketID = bestehendeAufgabe.TicketID ?? string.Empty;

                _originalStatus = bestehendeAufgabe.Status;
                AusgewählteAufgabe = bestehendeAufgabe;
            }

            else
            {
                // ─── Neue Aufgabe ───
                _istBearbeitenModus = false;
                Priorität = PrioritätsLevel.Normal;
                FälligkeitsDatum = DateTime.Now;
                Kategorie = 1; // Standardkategorie "Allgemein"
                _originalStatus = 1; // Standardstatus "Offen"
                TicketID = string.Empty;     //TicketID mit Zeitstempel

                AusgewählteAufgabe = new Aufgabe();
               
            }

            // Commands binden
            SpeichernCommand = new Befehl(async _ => await Speichern());
            AbbrechenCommand = new Befehl(_ => Schließen(false));
        }


        // ───────── Speicher-Logik ─────────

        /// <summary>
        /// Speichert die Aufgabe (INSERT oder UPDATE).
        /// </summary>
        private async Task Speichern()
        {
            try
            {
                //Validierung
                if (string.IsNullOrWhiteSpace(Titel))
                {
                    _dialogService.ShowError("Bitte geben Sie einen" +
                        "Titel für die Aufgabe ein.",
                        "Validierung");
                    return;
                }
                var aufgabe = new Aufgabe
                {
                    
                    AufgabenID = AufgabeID,
                    Titel = Titel.Trim(),
                    Beschreibung = string.IsNullOrWhiteSpace(Beschreibung) ? null : Beschreibung.Trim(),
                    Status = _istBearbeitenModus ? _originalStatus : 1, // Status nur im Bearbeitungsmodus übernehmen, sonst auf "Offen" setzen
                    Priorität = (int)Priorität,
                    FälligkeitsDatum = FälligkeitsDatum ?? DateTime.Now,
                    BenutzerID = 1,
                    ErledigtAm = null,
                    GruppeID =3,
                    Kategorie = Kategorie,
                    TicketID = TicketID
                };
                if (_istBearbeitenModus)
                {
                    await _aufgabenService.AktualisiereAufgabeAsync(aufgabe);
                    _dialogService.ShowInfo("Aufgabe erfolgreich aktualisiert.", "Erfolg");
                    Schließen(true);
                }
                else
                {
                    int neueId = await _aufgabenService.ErstelleAufgabeAsync(aufgabe);
                  
                    if(neueId > 0)
                    {
                        CreatedAufgabenId = neueId;
                        _dialogService.ShowInfo("Neue Aufgabe erfolgreich erstellt!", "Erfolg");
                        Schließen(true); return;
                    }
                   
                }
            }
            catch(Exception ex)
            {
                _dialogService.ShowError($"Fehler beim Speichern:\n{ex.Message}", "Fehler");
            }
        }

        /// <summary>Schließt den Dialog (mit Ergebnis)</summary>
        private void Schließen(bool? result)
        {
            var window = System.Windows.Application.Current.Windows
                .OfType<System.Windows.Window>()
                .FirstOrDefault(w => w.DataContext == this);

            if (window != null)
                window.DialogResult = result;

            window?.Close();
        }
    }



    /// <summary>
    /// Enum für Prioritätsstufen einer Aufgabe.
    /// Wird direkt im UI angezeigt (ComboBox).
    /// </summary>
    public enum PrioritätsLevel
    {
        Niedrig = 0,
        Normal = 1,
        Hoch = 2
        
    }


}
