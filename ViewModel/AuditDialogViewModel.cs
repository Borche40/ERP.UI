using ERP.Core.Services;
using ERP.Data.Models;
using ERP.UI.Commands;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ERP.UI.ViewModel
{
    /// <summary>
    /// ViewModel für den Audit-Dialog.
    /// Lädt Audit-Einträge für Produkte und stellt sie der UI zur Anzeige bereit.
    /// </summary>
    public class AuditDialogViewModel : BaseViewModel
    {
        // ─────────────────────────────── Felder ───────────────────────────────

        /// <summary>
        /// Service zum Laden der Audit-Einträge.
        /// </summary>
        private readonly AuditService _auditService;

        private ObservableCollection<AuditEintrag> _auditEinträge = new();
        private AuditEintrag _ausgewählterEintrag;
        private bool _istLaden;
        private string _ausgewählteAktion = "Alle";
        private string _ausgewählteBenutzer = "Alle";
        private int _ladeVersion = 0;
        private DateTime? _ausgewähltesVonDatum;
        private DateTime? _ausgewähltesBisDatum;




        // ─────────────────────────────── Konstruktor ───────────────────────────────

        /// <summary>
        /// Initialisiert das ViewModel und startet direkt das Laden der Audit-Einträge.
        /// </summary>
        public AuditDialogViewModel()
        {
            _auditService = new AuditService();

            _ = InitialisiereAsync();
        }
        /// <summary>
        /// Initialisiert den Dialog indem zuerst die 
        /// Benutzerfilter und anschließend die Audit-Einträge geladen werden,
        /// </summary>
        /// <returns></returns>
        private async Task InitialisiereAsync()
        {
            await LadeBenutzerFilterAsync();
            await LadeAuditEinträgeAsync();
        }

        // ─────────────────────────────── Eigenschaften ───────────────────────────────

        /// <summary>
        /// Liste aller geladenen Audit-Einträge für Produkte.
        /// Diese Liste wird im Dialog an die UI gebunden.
        /// </summary>
        public ObservableCollection<AuditEintrag> AuditEinträge
        {
            get => _auditEinträge;
            set
            {
                _auditEinträge = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Liste aller verfügbaren Audit-Filteraktionen.
        /// </summary>
        public ObservableCollection<string?> AktionenFilter { get; } = new()
        {
            "Alle",
            "INSERT",
            "UPDATE",
            "DELETE"
        };
        /// <summary>
        /// Liste aller verfügbaren Benutzer für Audit-Filter
        /// </summary>
        public ObservableCollection<string> BenutzerFilter { get; } = new();


        /// <summary>
        /// Der aktuell ausgewählte Benutzer für den Audit-Filter
        /// Beim Ändern wird die Audit-Liste automatisch neue geladen.
        /// </summary>
        public string AusgewählteBenutzer
        {
            get => _ausgewählteBenutzer;
            set
            {
                if(_ausgewählteBenutzer != value)
                {
                    _ausgewählteBenutzer =value;
                    OnPropertyChanged();
                   _= LadeAuditEinträgeAsync();
                }
            }
        }

        public string AusgewählteAktion
        {
            get => _ausgewählteAktion;
            set
            {
                if(_ausgewählteAktion != value)
                {
                    _ausgewählteAktion = value;
                    OnPropertyChanged();
                   _= LadeAuditEinträgeAsync();
                }
            }
        }

        /// <summary>
        /// Der aktuell ausgewählte Audit-Eintrag.
        /// Kann später für eine Detailanzeige verwendet werden.
        /// </summary>
        public AuditEintrag AusgewählterEintrag
        {
            get => _ausgewählterEintrag;
            set
            {
                _ausgewählterEintrag = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Das ausgewählte Startdatum für den Audit-Filter
        /// Wenn sich der Wert ändert,wird die Audit-Liste neue geladen.
        /// </summary>
        public DateTime? AusgewähltesVonDatum
        {
            get => _ausgewähltesVonDatum;
            set
            {
                if(_ausgewähltesVonDatum != value)
                {
                    this._ausgewähltesVonDatum = value;
                    OnPropertyChanged();
                   _= LadeAuditEinträgeAsync();

                }
            }
        }

        /// <summary>
        /// Das ausgewählte Enddatum für den Audit-Filter
        /// Wenn sich der Wert ändert,wird die
        /// Audit-Liste neue geladen.
        /// </summary>
        public DateTime? AusgewähltesBisDatum
        {
            get => _ausgewähltesBisDatum;
            set
            {
                if(_ausgewähltesBisDatum != value)
                {
                    this._ausgewähltesBisDatum = value;
                    this.OnPropertyChanged();
                    _ = LadeAuditEinträgeAsync();
                }
            }
        }




        /// <summary>
        /// Gibt an, ob die Audit-Daten aktuell geladen werden.
        /// Kann später für Ladeanzeigen verwendet werden.
        /// </summary>
        public bool IstLaden
        {
            get => _istLaden;
            set
            {
                _istLaden = value;
                OnPropertyChanged();
            }
        }

        // ─────────────────────────────── Methoden ───────────────────────────────

        /// <summary>
        /// Lädt alle Audit-Einträge für Produkte asynchron aus dem Service
        /// und übernimmt sie in die ObservableCollection für die UI.
        /// </summary>
        public async Task LadeAuditEinträgeAsync()
        {
            int aktuelleVersion = ++_ladeVersion;

            try
            {
                IstLaden = true;

                string? aktionFilter = AusgewählteAktion == "Alle"
                    ? null
                    : AusgewählteAktion;

                string? benutzerFilter = AusgewählteBenutzer == "Alle"
                    ? null 
                    : AusgewählteBenutzer;
                

                System.Diagnostics.Debug.WriteLine(
                    $"[Audit] Version={aktuelleVersion}, Aktion={aktionFilter ?? "NULL"}, Benutzer={benutzerFilter ?? "NULL"}," +
                    $"Von ={AusgewähltesVonDatum?.ToString("dd.MM.yyyy")?? "NULL"}," +
                    $"Bis ={AusgewähltesBisDatum?.ToString("dd.MM.yyyy") ?? "NULL"}");

                var einträge = await _auditService.LadeAuditEinträgeFürProdukteAsync(aktionFilter, benutzerFilter,AusgewähltesVonDatum,AusgewähltesBisDatum);

                // Nur das letzte Laden darf die UI aktualisieren
                if (aktuelleVersion != _ladeVersion)
                    return;

                AuditEinträge = new ObservableCollection<AuditEintrag>(einträge);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AuditDialogViewModel] Fehler beim Laden: {ex.Message}");
            }
            finally
            {
                if (aktuelleVersion == _ladeVersion)
                {
                    IstLaden = false;
                }
            }
        }
        /// <summary>
        /// Lädt alle verfügbaren Benutzer für den Audit-Filter
        /// und übernimmt soe in die ObservableCollection.
        /// </summary>
        /// <returns></returns>
        public async Task LadeBenutzerFilterAsync()
        {
            try
            {
                var benutzer = await _auditService.LadeBenutzerFürProdukteAuditAsync();
                BenutzerFilter.Clear();
                BenutzerFilter.Add("Alle");
                foreach(var benutzerName in benutzer)
                {
                    BenutzerFilter.Add(benutzerName);
                }
                AusgewählteBenutzer = "Alle";
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AuditDialogViewModel] Fehler beim Laden der Benutzer:{ex.Message}");
            }
        }
    }
}