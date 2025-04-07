using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using ERP.Data.Models;
using ERP.UI.Commands;
using ERP.UI.Views;
using ERP.Data;

namespace ERP.UI.ViewModel
{
    /// <summary>
    /// Verwaltet die Anmelde- und 
    /// Registrierungsdaten von Benutzern.
    /// </summary>
    internal class AuthentifizierungsManager:BaseViewModel
    {
        #region Daten

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private Registrierungsdaten _Registrierung = null!;

        /// <summary>
        /// Ruft die Daten für das Registrierern ab, oder legt diese fest
        /// </summary>
        public Registrierungsdaten Registrierung
        {
            get
            {
                if (this._Registrierung == null)
                {
                    this._Registrierung = new Registrierungsdaten();
                }
                return this._Registrierung;
            }
            set
            {
                this._Registrierung = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private Anmeldungsdaten _Anmeldung = null!;

        /// <summary>
        /// Ruft die Daten für das Anmelden ab, oder legt diese fest
        /// </summary>
        public Anmeldungsdaten Anmeldung
        {
            get
            {
                if (this._Anmeldung == null)
                {
                    this._Anmeldung = new Anmeldungsdaten();
                }
                return this._Anmeldung;
            }
            set
            {
                this._Anmeldung = value;
                this.OnPropertyChanged();
            }
        }

        #endregion Daten

        #region Datenbearbeitung

        /// <summary>
        /// Gibt die Anmeldungs daten zurück, wenn gültige Anmeldeinformationen 
        /// bereitgestellt wurden; andernfalls wird null zurückgegeben.
        /// </summary>
        public ERP.Data.Anmeldung? HoleAnmeldungsDaten()
        {
            this.Kontext.Log.StartMelden();
            this.IstBeschäftigt = true;

            ERP.Data.Anmeldung anmeldung = null!;

            if (string.IsNullOrEmpty(this.Anmeldung.Email))
            {
                // Dem Benutzer mitteilen, dass eine E-Mail angegeben sein muss
                this.Anmeldung.Nachricht = ERP.UI.Views.Texte.EmailFehlt;
            }
            else if (this.IstEmailGültig(this.Anmeldung.Email)
                && !string.IsNullOrEmpty(this.Anmeldung.Passwort))
            {
                if (this.Anmeldung.Passwort.Length == 8)
                {
                    anmeldung = new ERP.Data.Anmeldung
                    {
                        Email = this.Anmeldung.Email,
                        Passwort = this.Anmeldung.Passwort,
                    };
                }
                else
                {
                    this.Anmeldung.Nachricht = ERP.UI.Views.Texte.PasswortFehlt;
                }
            }
            else if (!this.IstEmailGültig(this.Anmeldung.Email))
            {
                this.Anmeldung.Nachricht = ERP.UI.Views.Texte.EmailNichtGültig;
            }
            else if (string.IsNullOrEmpty(this.Anmeldung.Passwort))
            {
                this.Anmeldung.Nachricht = ERP.UI.Views.Texte.PasswortFehlt;
            }
            this.IstBeschäftigt = false;
            this.Kontext.Log.EndeMelden();
            return anmeldung;

        }

        /// <summary>
        /// Gibt die Registrierung daten zurück, wenn gültige Registrierungsinformationen 
        /// bereitgestellt wurden; andernfalls wird null zurückgegeben.
        /// </summary>
        public ERP.Data.Registrierung? HoleRegistrierungsDaten()
        {
            this.Kontext.Log.StartMelden();
            this.IstBeschäftigt = true;

            ERP.Data.Registrierung registrierung = null!;

            if (this.SindDatenVorhanden())
            {
                registrierung = new ERP.Data.Registrierung
                {
                    Email = this.Registrierung.Email,
                    Passwort = this.Registrierung.Passwort,
                    PasswortBestätigung = this.Registrierung.BestätigePasswort,
                    Name = this.Registrierung.Name,
                };
            }
            this.IstBeschäftigt = false;
            this.Kontext.Log.EndeMelden();
            return registrierung;
        }

        /// <summary>
        /// Setzt die Daten zum Anmelden/Registrieren züruck
        /// </summary>
        public void DatenLöschen()
        {
            this.Anmeldung = null!;
            this.Registrierung = null!;
        }

        #endregion Datenbearbeitung

        #region Gültigkeitsabfrage

        /// <summary>
        /// Überprüft, ob eine E-Mail-Adresse 
        /// gültig ist und zu einer der häufigen E-Mail-Domänen gehört.
        /// </summary>
        /// <returns>True, wenn die E-Mail-Adresse gültig ist 
        /// und zu einer erlaubten Domäne gehört; andernfalls false</returns>
        private bool IstEmailGültig(string email)
        {
            bool IstGültig = false;
            // Grundlegende Überprüfung des E-Mail-Formats mithilfe eines regulären Ausdrucks
            string muster = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";
            Regex regex = new Regex(muster);

            if (!regex.IsMatch(email))
            {
                this.Kontext.Log.
              Hinzufügen($"{this} hat überprüft, ob die E-Mail im korrekten Format vorliegt.");
                return IstGültig;
            }

            // Liste der häufigen E-Mail-Domänen zur Validierung
            string[] erlaubteDomänen =
                {
                "gmail.com", "yahoo.com", "outlook.com", "hotmail.com", "aol.com",
                "icloud.com", "protonmail.com", "mail.com", "zoho.com", "yandex.com",
                "inbox.com", "gmx.com", "fastmail.com", "live.com", "me.com",
                "apple.com", "msn.com", "rocketmail.com", "yahoo.co.uk", "yahoo.ca",
                "yahoo.co.in", "yahoo.co.au", "yahoo.co.nz", "yahoo.co.jp", "yahoo.co.za"
                };


            // Extrahieren des Domänenteils der E-Mail
            string[] emailTeile = email.Split('@');
            string domäne = emailTeile[1].ToLower();

            // Überprüfen, ob die Domäne in der erlaubten Liste enthalten ist
            foreach (var erlaubteDomäne in erlaubteDomänen)
            {
                if (domäne.EndsWith(erlaubteDomäne))
                {
                    IstGültig = true; // E-Mail ist gültig
                    return IstGültig;
                }
            }
            this.Kontext.Log.
                Hinzufügen($"{this} hat überprüft, ob die E-Mail im korrekten Format vorliegt.");
            return IstGültig;
        }


        /// <summary>
        /// True falls alle Daten für eine Registrierung vorhanden sind
        /// </summary>
        private bool SindDatenVorhanden()
        {
            #region Hilfsmethode
            bool EnthältNichtBuchstaben(string eingabe)
            {
                // Regulärer Ausdruck, um Nicht-Buchstaben-Zeichen zu erkennen
                string muster = @"[^a-zA-Z]";
                Regex regex = new Regex(muster);

                // Überprüfen, ob die Eingabe Nicht-Buchstaben-Zeichen enthält
                return regex.IsMatch(eingabe);
            }
            #endregion Hilfsmethode

            if (string.IsNullOrEmpty(this.Registrierung.Email))
            {
                this.Registrierung.Nachricht = ERP.UI.Views.Texte.EmailFehlt;
                return false;
            }
            else if (string.IsNullOrEmpty(this.Registrierung.Name)
                || this.Registrierung.Name.Length < 3
                || EnthältNichtBuchstaben(this.Registrierung.Name))
            {

                this.Registrierung.Nachricht = ERP.UI.Views.Texte.FehlerName;
                return false;
            }
            else if (string.IsNullOrEmpty(this.Registrierung.Passwort)
                || this.Registrierung.Passwort.Length != 8)
            {
                this.Registrierung.Nachricht = ERP.UI.Views.Texte.PasswortFehlt;
                return false;
            }
            else if (0 != string.Compare(this.Registrierung.Passwort,
                this.Registrierung.BestätigePasswort, ignoreCase: false))
            {
                this.Registrierung.Nachricht = ERP.UI.Views.Texte.PasswortFehler;
                return false;
            }
            else if (!IstEmailGültig(this.Registrierung.Email))
            {
                this.Registrierung.Nachricht = Views.Texte.EmailNichtGültig;
                return false;
            }
            else if (this.Registrierung.Name.Contains(" "))
            {
                this.Registrierung.Nachricht = Views.Texte.NameFehler;
                return false;
            }

            return true;

        }
        #endregion Gültigkeitsabfrage

    }
}
