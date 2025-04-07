using ERP.Data.Models;
using ERP.UI.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using ERP.UI.Views;
using ERP.UI.ViewModel;


namespace ERP.UI.ViewModel
{
    /// <summary>
    /// Stellt Information über
    /// ein Anmeldungs prozess
    /// </summary>
    internal class Anmeldungsdaten:BaseViewModel
    {
        #region Daten
        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private bool _IstNachrichtBereit = false;

        /// <summary>
        /// True falls es eine Nachricht zum Anzeigen gibt
        /// </summary>
        public bool IstNachrichtBereit
        {
            get => this._IstNachrichtBereit;
            set
            {
                this._IstNachrichtBereit = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private string _Nachricht = null!;

        /// <summary>
        /// Ruft die Mitteilung ab, die für das Melden von nicht korrekten
        /// oder nicht angegebenen Daten verwendet wird, oder legt diese fest
        /// </summary>
        public string Nachricht
        {
            get => this._Nachricht;
            set
            {
                this._Nachricht = value;
                if (string.IsNullOrEmpty(this._Nachricht))
                {
                    this.IstNachrichtBereit = false;
                }
                else
                {
                    this.IstNachrichtBereit = true;
                }
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private string _Email = null!;

        /// <summary>
        /// Ruft die E-Mail-Adresse des Benutzers für 
        /// das Anmelden ab oder legt diese fest
        /// </summary>
        public string Email
        {
            get => this._Email;
            set
            {
                this._Email = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private string _Passwort = null!;

        /// <summary>
        /// Ruft das Kennwort des Benutzers 
        /// für das Anmelden ab oder legt dieses fest
        /// </summary>
        public string Passwort
        {
            get => this._Passwort;
            set
            {
                this._Passwort = value;
                this.OnPropertyChanged();
            }
        }
        #endregion Daten

    }

}
