using ERP.UI.Commands;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input.Manipulations;
using System.Windows.Navigation;

namespace ERP.UI.ViewModel
{
    /// <summary>
    /// Stellt Informationen über eine neue Aufgabe und Gruppe bereit
    /// </summary>
    internal class BenutzerDaten:BaseViewModel
    {
        #region Gruppe

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private string _Gruppe = null!;

        /// <summary>
        /// Ruft den Namen der neuen AufgabenGruppe ab, oder legt diese fest
        /// </summary>
        public string? Gruppe
        {
            get => this._Gruppe;
            set
            {
                this._Gruppe = value!;
                this.OnPropertyChanged();
            }
        }
        #endregion Gruppe

        #region Aufgabe

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private string _Inhalt = null!;

        /// <summary>
        /// Ruft den Inhalt der neuen Aufgabe ab, oder legt diesen fest
        /// </summary>
        public string? Inhalt
        {
            get => this._Inhalt;

            set
            {
                this._Inhalt = value!;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private string _Zeit = null!;

        /// <summary>
        /// Ruft das Datum wann die Aufgabe erfüllt werden soll, oder 
        /// legt dieses fest
        /// </summary>
        public string? Zeit
        {
            get => this._Zeit;

            set
            {
                if (value != null)
                {
                    string format = "M/d/yyyy h:mm:ss tt";

                    if (DateTime.TryParseExact(value, format, 
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out DateTime parsedDate))
                    {

                        this._Zeit = parsedDate.ToString("dd.MM.yyyy");
                    }
                }
                else
                {
                    this._Zeit = null!;
                }
                this.OnPropertyChanged();
            }
        }

        #endregion Aufgabe

        #region Datenbearbeitung

        /// <summary>
        /// Gibt ein Aufgabe-Objekt aus den vorgegebenen Daten zurück, 
        /// oder null, falls die Daten fehlen.
        /// </summary>
        public ERP.Data.Aufgabe? HoleNeueAufgabe()
        {
            ERP.Data.Aufgabe aufgabe = null!;

            if (!string.IsNullOrEmpty(this.Inhalt) && !string.IsNullOrEmpty(this.Zeit))
            {
                aufgabe = new ERP.Data.Aufgabe
                {
                    Inhalt = this.Inhalt,
                    Zeit = this.Zeit,
                    IstFertig = false,

                };
            }
            else
            {
                NachrichtBox.Anzeigen(Views.Texte.NeueAufgabeFehler, NachrichtTyp.Ok);
            }
            return aufgabe;
        }

        /// <summary>
        /// Gibt ein Gruppe-Objekt aus den vorgegebenen Daten zurück, 
        /// oder null, falls die Daten fehlen.
        /// </summary>
        public ERP.Data.AufgabenGruppe HoleNeueGruppe()
        {
            ERP.Data.AufgabenGruppe gruppe = null!;

            if (!string.IsNullOrEmpty(this.Gruppe))
            {
                gruppe = new ERP.Data.AufgabenGruppe
                {
                    Name = this.Gruppe
                };
            }
            else
            {
                NachrichtBox.Anzeigen(Views.Texte.NeueGruppeFehler, NachrichtTyp.Ok);
            }
            return gruppe;
        }
        #endregion Datenbearbeitung

    }
}
