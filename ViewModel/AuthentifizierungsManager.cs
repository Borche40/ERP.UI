using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using ERP.UI.ViewModel;
using ERP.Data;
using ERP.UI.Views;
using ERP.UI.Commands;
using ERP.Data.Models;

namespace ERP.UI.ViewModel
{
    /// <summary>
    /// AuthentifizierungsManager: Diese Klasse ist für die Verwaltung der Anmeldungs
    /// - und Registrierungsdaten verantwortlich.
    /// </summary>
    internal class AuthentifizierungsManager : BaseViewModel
    {
        /// Diese Klasse verwaltet die Anmeldungs- und Registrierungsdaten,
        private Registrierungsdaten? _Registrierung;
        private Anmeldungsdaten? _Anmeldung;
        /// <summary>
        /// Registrierungsdaten: Diese Eigenschaft speichert die Informationen, 
        /// die für die Registrierung eines neuen Benutzers erforderlich sind, 
        /// wie z. B. E-Mail, Name, Passwort und Bestätigung des Passworts.
        /// </summary>
        public Registrierungsdaten Registrierung
        {
            get => _Registrierung ??= new Registrierungsdaten();
            set => _Registrierung = value;
        }
        /// <summary>
        /// Anmeldungsdaten: Diese Eigenschaft speichert 
        /// die Anmeldungsinformationen des Benutzers,
        /// </summary>
        public Anmeldungsdaten Anmeldung
        {
            get => _Anmeldung ??= new Anmeldungsdaten();
            set => _Anmeldung = value;
        }

        /// <summary>
        /// AktuellerBenutzer: Diese Eigenschaft speichert die Informationen über den aktuell 
        /// angemeldeten Benutzer,und Audit-Informationen wie Anmeldezeitpunk.
        /// </summary>
        public Benutzer? AktuellerBenutzer { get;private set; }
        /// <summary>
        /// Es wird eine reguläre Ausdrucksmuster definiert, 
        /// um die Struktur einer gültigen E-Mail-Adresse zu überprüfen.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        private static bool IstEmailGültig(string email)
        {
            const string muster = @"^[a-zA-Z0-9_.+\-]+@[a-zA-Z0-9\-]+\.[a-zA-Z0-9\-.]+$";
            return Regex.IsMatch(email ?? string.Empty, muster);
        }
        /// <summary>
        /// Diese Methode überprüft, ob die Registrierungsdaten gültig sind,
        /// </summary>
        /// <returns></returns>
        private bool SindDatenVorhanden()
        {
            bool EnthältNichtBuchstaben(string eingabe)
                => Regex.IsMatch(eingabe ?? string.Empty, @"[^a-zA-Z]");

            if (string.IsNullOrEmpty(this.Registrierung.Email))//prüft, ob die E-Mail-Adresse leer ist oder nicht eingegeben wurde.
            {
                this.Registrierung.Nachricht = Texte.EmailFehlt;//Wenn die E-Mail-Adresse ungültig ist,
                                                                //wird eine Fehlermeldung angezeigt.
                return false;
            }
            if (string.IsNullOrEmpty(this.Registrierung.Name)//prüft, ob der Name leer ist oder nicht eingegeben wurde,
                                                             //oder ob er weniger als 3 Zeichen enthält, oder ob er Nicht-Buchstaben enthält.
                || this.Registrierung.Name.Length < 3
                || EnthältNichtBuchstaben(this.Registrierung.Name))//Wenn der Name ungültig ist, wird eine Fehlermeldung angezeigt.
            {
                this.Registrierung.Nachricht = Texte.FehlerName;
                return false;
            }
            if (string.IsNullOrEmpty(this.Registrierung.Passwort)//prüft, ob das Passwort leer ist oder nicht eingegeben wurde,
                || this.Registrierung.Passwort.Length != 8)
            {
                this.Registrierung.Nachricht = Texte.PasswortFehlt;//Wenn das Passwort ungültig ist, wird eine Fehlermeldung angezeigt.
                return false;
            }
            if (0 != string.Compare(this.Registrierung.Passwort,
                                    this.Registrierung.BestätigePasswort, ignoreCase: false))//prüft, ob das Passwort und die Bestätigung
                                                                                             //des Passworts übereinstimmen.
            {
                this.Registrierung.Nachricht = Texte.PasswortFehler;
                return false;
            }
            if (!IstEmailGültig(this.Registrierung.Email))
            {
                this.Registrierung.Nachricht = Texte.EmailNichtGültig;
                return false;
            }
            if (this.Registrierung.Name.Contains(" "))
            {
                this.Registrierung.Nachricht = Texte.NameFehler;
                return false;
            }
            return true;
        }
        /// <summary>
        /// Anhand der Anmeldungsdaten überprüft diese Methode,
        /// ob die E-Mail-Adresse gültig ist 
        /// und ob ein Passwort eingegeben wurde.
        /// </summary>
        /// <returns></returns>
        public Anmeldung? HoleAnmeldungsDaten()
        {
            Anmeldung anmeldung = null!;

            if (string.IsNullOrEmpty(this.Anmeldung.Email))//prüft, ob die E-Mail-Adresse leer ist oder nicht eingegeben wurde.
            {
                this.Anmeldung.Nachricht = Texte.EmailFehlt;
            }
            else if (IstEmailGültig(this.Anmeldung.Email) &&
                     !string.IsNullOrEmpty(this.Anmeldung.Passwort))
            {
                if (this.Anmeldung.Passwort.Length >= 8)//prüft, ob das Passwort mindestens 8 Zeichen lang ist.
                {
                    anmeldung = new Anmeldung
                    {
                        Email = this.Anmeldung.Email,
                        Passwort = this.Anmeldung.Passwort
                    };
                }
                else//Wenn das Passwort ungültig ist, wird eine Fehlermeldung angezeigt.
                {
                    this.Anmeldung.Nachricht = Texte.PasswortFehlt;
                }
            }
            else if (!IstEmailGültig(this.Anmeldung.Email))//Wenn die E-Mail-Adresse ungültig ist, wird eine Fehlermeldung angezeigt.
            {
                this.Anmeldung.Nachricht = Texte.EmailNichtGültig;
            }
            else if (string.IsNullOrEmpty(this.Anmeldung.Passwort))
            {
                this.Anmeldung.Nachricht = Texte.PasswortFehlt;
            }

            return anmeldung;
        }
        /// <summary>
        /// Diese Methode überprüft, ob die Registrierungsdaten gültig sind, 
        /// und wenn ja, erstellt sie ein neues Objekt vom Typ "Registrierung" 
        /// mit den entsprechenden Werten.
        /// </summary>
        /// <returns></returns>
        public Registrierung? HoleRegistrierungsDaten()
        {
            Registrierung registrierung = null!;

            if (this.SindDatenVorhanden())//Wenn die Daten gültig sind, wird ein neues Objekt
                                          //"Registrierung" erstellt und mit den entsprechenden Werten gefüllt.
            {
                registrierung = new Registrierung
                {
                    Email = this.Registrierung.Email,
                    Passwort = this.Registrierung.Passwort,
                    PasswortBestätigung = this.Registrierung.BestätigePasswort,
                    Name = this.Registrierung.Name
                };
            }
            return registrierung;
        }
        /// <summary>
        /// Diese Methode setzt die Anmeldungs- und Registrierungsdaten zurück,
        /// indem sie neue Instanzen der entsprechenden Klassen erstellt. 
        /// Dadurch werden alle vorherigen Werte gelöscht 
        /// und die Felder auf ihre Standardwerte zurückgesetzt.
        /// </summary>
        public void DatenLöschen()
        {
            this.Anmeldung = new Anmeldungsdaten();
            this.Registrierung = new Registrierungsdaten();
        }
        /// <summary>
        /// HttpClient für die Kommunikation mit dem Backend. 
        /// Es wird eine Basisadresse und ein Timeout festgelegt.
        /// </summary>
        private static readonly HttpClient _http = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7181/"), 
            
            Timeout = TimeSpan.FromSeconds(30)
        };
        /// <summary>
        /// Authentifiziert den Benutzer mit den angegebenen Anmeldedaten gegen das Backend.
        /// </summary>
        public async Task<bool> AuthentifizierenAsync(string email, string passwort, CancellationToken ct = default)
        {
            (this.Anmeldung ??= new Anmeldungsdaten()).Email = email;
            this.Anmeldung.Passwort = passwort;

            var anmeldung = this.HoleAnmeldungsDaten();
            if (anmeldung == null)
                return false;

            try
            {
                var payload = new { email = anmeldung.Email, passwort = anmeldung.Passwort };
                using var resp = await _http.PostAsJsonAsync("erpui/anmelden", payload, ct);




                if (!resp.IsSuccessStatusCode)
                {
                    this.Anmeldung.Nachricht = "Login fehlgeschlagen – bitte E-Mail/Passwort prüfen."; 
                    return false;
                }
                
                var user = await resp.Content.ReadFromJsonAsync<Benutzer>();

                if(user != null)
                {
                    this.AktuellerBenutzer = user;
                }



                return true;
            }
            catch (TaskCanceledException)
            {
                this.Anmeldung.Nachricht = "Zeitüberschreitung beim Anmelden.";
                return false;
            }
            catch (Exception)
            {
                this.Anmeldung.Nachricht = "Unerwarteter Fehler beim Anmelden.";
                return false;
            }
        }
    }
}
