using ERP.Core.Services;
using ERP.Data.Models;
using ERP.UI.Commands;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ERP.UI.ViewModel
{
    /// <summary>
    /// ViewModel für die Verwaltung von Bestellpositionen
    /// innerhalb einer Bestellung.
    /// </summary>
    public class BestellPositionenViewModel : BaseViewModel
    {
        // ───────── Felder ─────────
        private readonly BestellPositionenService _bestellPositionenService;

        private ObservableCollection<BestellPosition> _bestellPositionen;
        private BestellPosition? _ausgewähltePosition;
        private int _bestellungId;
        private int _produktId;
        private int _menge;
        private decimal _preis;
        private decimal _nettoSumme;
        private decimal _mwStSumme;
        private decimal _bruttoSumme;

        // ───────── Eigenschaften ─────────

        /// <summary>
        /// Sammlung aller Positionen der aktuellen Bestellung.
        /// </summary>
        public ObservableCollection<BestellPosition> BestellPositionen
        {
            get => _bestellPositionen;
            set
            {
                _bestellPositionen = value;
                OnPropertyChanged();
                BerechneSummen();
            }
        }

        /// <summary>
        /// Aktuell ausgewählte Position in der UI.
        /// </summary>
        public BestellPosition? AusgewähltePosition
        {
            get => _ausgewähltePosition;
            set
            {
                _ausgewähltePosition = value;
                OnPropertyChanged();
                AktualisiereCommandStatus();
            }
        }

        /// <summary>
        /// ID der Bestellung, zu der Positionen geladen werden.
        /// </summary>
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

        /// <summary>
        /// Produkt-ID für eine neue Position.
        /// </summary>
        public int ProduktID
        {
            get => _produktId;
            set
            {
                _produktId = value;
                OnPropertyChanged();
                AktualisiereCommandStatus();
            }
        }

        /// <summary>
        /// Menge des Produkts.
        /// </summary>
        public int Menge
        {
            get => _menge;
            set
            {
                _menge = value;
                OnPropertyChanged();
                AktualisiereCommandStatus();
            }
        }

        /// <summary>
        /// Preis des Produkts.
        /// </summary>
        public decimal Preis
        {
            get => _preis;
            set
            {
                _preis = value;
                OnPropertyChanged();
                AktualisiereCommandStatus();
            }
        }

        /// <summary>
        /// Netto-Summe aller aktuell geladenen Positionen.
        /// </summary>
        public decimal NettoSumme
        {
            get => _nettoSumme;
            set
            {
                _nettoSumme = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// MwSt-Summe der Bestellung.
        /// Standardmäßig mit 20 % berechnet.
        /// </summary>
        public decimal MwStSumme
        {
            get => _mwStSumme;
            set
            {
                _mwStSumme = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Brutto-Summe der Bestellung.
        /// </summary>
        public decimal BruttoSumme
        {
            get => _bruttoSumme;
            set
            {
                _bruttoSumme = value;
                OnPropertyChanged();
            }
        }

        // ───────── Commands ─────────
        public ICommand LadenCommand { get; }
        public ICommand HinzufügenCommand { get; }
        public ICommand LöschenCommand { get; }

        // ───────── Konstruktor ─────────
        public BestellPositionenViewModel()
        {
            _bestellPositionenService = new BestellPositionenService();
            BestellPositionen = new ObservableCollection<BestellPosition>();

            LadenCommand = new Befehl(
                async _ => await LadeBestellPositionenAsync(),
                _ => BestellungID > 0);

            HinzufügenCommand = new Befehl(
                async _ => await NeueBestellPositionHinzufügenAsync(),
                _ => BestellungID > 0 && ProduktID > 0 && Menge > 0 && Preis > 0);

            LöschenCommand = new Befehl(
                async _ => await BestellPositionLöschenAsync(),
                _ => AusgewähltePosition != null);
        }

        // ───────── Methoden ─────────

        /// <summary>
        /// Lädt alle Positionen der aktuellen Bestellung asynchron.
        /// </summary>
        public async Task LadeBestellPositionenAsync()
        {
            try
            {
                if (BestellungID <= 0)
                    return;

                var daten = await _bestellPositionenService.LadeBestellPositionenNachBestellungAsync(BestellungID);

                BestellPositionen.Clear();

                foreach (var position in daten)
                {
                    BestellPositionen.Add(position);
                }

                BerechneSummen();
                AktualisiereCommandStatus();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Fehler] LadeBestellPositionenAsync: {ex.Message}");
            }
        }

        /// <summary>
        /// Fügt eine neue Position zur aktuellen Bestellung hinzu
        /// und lädt anschließend die Liste neu.
        /// </summary>
        public async Task NeueBestellPositionHinzufügenAsync()
        {
            try
            {
                if (BestellungID <= 0 || ProduktID <= 0 || Menge <= 0 || Preis <= 0)
                    return;

                bool erfolg = await _bestellPositionenService.BestellPositionHinzufügenAsync(
                    BestellungID,
                    ProduktID,
                    Menge,
                    Preis);

                if (erfolg)
                {
                    await LadeBestellPositionenAsync();

                    ProduktID = 0;
                    Menge = 0;
                    Preis = 0m;

                    BerechneSummen();
                    AktualisiereCommandStatus();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Fehler] NeueBestellPositionHinzufügenAsync: {ex.Message}");
            }
        }

        /// <summary>
        /// Löscht die aktuell ausgewählte Position
        /// und lädt anschließend die Liste neu.
        /// </summary>
        public async Task BestellPositionLöschenAsync()
        {
            try
            {
                if (AusgewähltePosition == null)
                    return;

                bool erfolg = await _bestellPositionenService.BestellPositionLöschenAsync(AusgewähltePosition.ID);

                if (erfolg)
                {
                    await LadeBestellPositionenAsync();
                    BerechneSummen();
                    AktualisiereCommandStatus();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Fehler] BestellPositionLöschenAsync: {ex.Message}");
            }
        }

        /// <summary>
        /// Berechnet die Summen aus allen geladenen Positionen.
        /// Netto = Summe der Gesamtpreise
        /// MwSt = 20 % vom Netto
        /// Brutto = Netto + MwSt
        /// </summary>
        private void BerechneSummen()
        {
            try
            {
                if (BestellPositionen == null || BestellPositionen.Count == 0)
                {
                    NettoSumme = 0m;
                    MwStSumme = 0m;
                    BruttoSumme = 0m;
                    return;
                }

                decimal netto = BestellPositionen.Sum(p => p.Gesamtpreis);
                decimal mwst = Math.Round(netto * 0.20m, 2);
                decimal brutto = netto + mwst;

                NettoSumme = netto;
                MwStSumme = mwst;
                BruttoSumme = brutto;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Fehler] BerechneSummen: {ex.Message}");
            }
        }

        /// <summary>
        /// Erzwingt die Aktualisierung des Command-Zustands,
        /// damit Buttons korrekt aktiviert oder deaktiviert werden.
        /// </summary>
        private void AktualisiereCommandStatus()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}