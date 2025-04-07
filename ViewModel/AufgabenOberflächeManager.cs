using ERP.UI.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.UI.ViewModel
{
    /// <summary>
    /// Stellt einen Dienst zum verwalten der Aufgabenoberfläche bereit
    /// </summary>
    internal class AufgabenOberflächeManager:BaseViewModel
    {
        #region Daten

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private bool _SindHilfsTastenAngezeigt = false;

        /// <summary>
        /// Ruft den Warheitswert ab der 
        /// beschreibt ob die Tasten zum wechseln 
        /// zwischen Aufgaben und Gruppen view angezeigt werden
        /// </summary>
        public bool SindHilfsTastenAngezeigt
        {
            get => this._SindHilfsTastenAngezeigt;
            set
            {
                this._SindHilfsTastenAngezeigt = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private bool _SindGruppenAngezeigt = false;

        /// <summary>
        /// Ruft den Warheitswert ab der 
        /// beschreibt ob die Gruppen angezeigt werden
        /// </summary>
        public bool SindGruppenAngezeigt
        {
            get => this._SindGruppenAngezeigt;
            set
            {
                this._SindGruppenAngezeigt = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Ruft die Breite des Hauptfensters ab
        /// </summary>
        public double HauptfensterBreite
        {
            get => ERP.UI.Properties.Settings.Default.HauptfensterBreite;
        }

        /// <summary>
        /// Ruft die minimale Breite des Hauptfensters ab, ab welcher die View sich ändert.
        /// </summary>
        public double MinimaleFensterBreite
        {
            get => ERP.UI.Properties.Settings.Default.MinimaleFensterBreite;
        }


        /// <summary>
        /// Ruft die minimale Breite der Aufgaben View ab, ab welcher die View sich ändert.
        /// </summary>
        public double MinimaleAufgabenBreite
        {
            get => ERP.UI.Properties.Settings.Default.MinimaleAufgabenBreite;
        }

        #endregion Daten

        #region Aufgabenstatus-logik

        /// <summary>
        /// Ruft den Warheitswert ab der 
        /// beschreibt ob eine alle Aufgaben angezeigt werden
        /// </summary>
        public bool AlleAnzeigen
        {
            get => ERP.UI.Properties.Settings.Default.AlleAnzeigen;
            set
            {
                ERP.UI.Properties.Settings.Default.AlleAnzeigen = value;
                if (value == true)
                {
                    this.SindAbgeschlosseneAngezeigt = false;
                }
                this.OnPropertyChanged();
            }
        }


        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private char _AnzeigeSymbol = (char)ERP.UI.Properties.Settings.Default.Unten;

        /// <summary>
        /// Ruft den Tipp für die Anzeige von Abgeschlossenen Aufgaben ab
        /// oder legt diesen fest
        /// </summary>
        public char AnzeigeSymbol
        {
            get => this._AnzeigeSymbol;
            set
            {
                this._AnzeigeSymbol = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private bool _SindAbgeschlosseneAngezeigt = false;

        /// <summary>
        /// Ruft den Warheitswert ab der 
        /// beschreibt ob die Abgeschlossenen Aufgaben angezeigt werden.
        /// </summary>
        public bool SindAbgeschlosseneAngezeigt
        {
            get => this._SindAbgeschlosseneAngezeigt;
            set
            {
                this._SindAbgeschlosseneAngezeigt = value;
                if (value == false)
                {
                    this.AnzeigeSymbol = (char)ERP.UI.Properties.Settings.Default.Unten;
                }
                else
                {
                    this.AnzeigeSymbol = (char)ERP.UI.Properties.Settings.Default.Oben;
                }
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Wechselt den wert SindAbgeschlosseneAngezeigt auf falls oder true.
        /// </summary>
        private void AufgabeAnzeigeWechseln()
        {
            if (this.SindAbgeschlosseneAngezeigt)
            {
                this.SindAbgeschlosseneAngezeigt = false;
            }
            else
            {
                this.SindAbgeschlosseneAngezeigt = true;
            }
        }

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private ERP.UI.Commands.Befehl _AbgeschlosseneAngezeigen = null!;

        /// <summary>
        /// Ruft den Befehl zum anzeigen der Abgeschlossenen Aufgaben ab.
        /// </summary>
        public ERP.UI.Commands.Befehl AbgeschlosseneAngezeigen
        {
            get
            {
                if (this._AbgeschlosseneAngezeigen == null)
                {
                    this._AbgeschlosseneAngezeigen = new ERP.UI.Commands.Befehl(d => this.AufgabeAnzeigeWechseln(), d => !this.AlleAnzeigen);
                }
                return this._AbgeschlosseneAngezeigen;
            }
        }

        #endregion Aufgabenstatus-logik

        #region Oberfläche-logik

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private System.Windows.Controls.UserControl _AufgabenEinstellungen = null!;

        /// <summary>
        /// Ruft die View zum Anzeigen der Aufgabeneinstellungen,
        /// oder legt diese fest
        /// </summary>
        public System.Windows.Controls.UserControl AufgabenEinstellungen
        {
            get
            {
                if (this._AufgabenEinstellungen == null)
                {
                    var breite = ERP.UI.Properties.Settings.Default.HauptfensterBreite;

                    if (breite == 0 || breite > this.MinimaleAufgabenBreite)
                    {
                        this._AufgabenEinstellungen = new Views.AufgabenEinstellungenBreit();
                    }
                    else
                    {
                        this._AufgabenEinstellungen = new Views.AufgabenEinstellungenSchmal();
                    }
                }
                return this._AufgabenEinstellungen;
            }
            set
            {
                this._AufgabenEinstellungen = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private ERP.UI.Commands.Befehl _GruppenAnzeigen = null!;

        /// <summary>
        /// Ruft den Befehl ab, der die Gruppen anzeigt, wenn sie versteckt sind,
        /// und versteckt, wenn sie angezeigt sind.
        /// </summary>
        public ERP.UI.Commands.Befehl GruppenAnzeigen
        {
            get
            {
                if (this._GruppenAnzeigen == null)
                {
                    this._GruppenAnzeigen = new ERP.UI.Commands.Befehl(d =>
                    {
                        if (this.SindGruppenAngezeigt)
                        {
                            this.SindGruppenAngezeigt = false;
                        }
                        else
                        {
                            this.SindGruppenAngezeigt = true;
                        }
                    });
                }
                return this._GruppenAnzeigen;
            }
        }


        #endregion Oberfläche-logik

    }
}
