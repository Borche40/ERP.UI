using ERP.UI.Commands;

namespace ERP.UI.ViewModel
{
    /// <summary>
    /// Stellt Informationen für den Anmelde-Dialog bereit (E-Mail, Passwort, Meldungen)
    /// und enthält den Anmelde-Befehl, der das Backend aufruft.
    /// </summary>
    internal class Anmeldungsdaten : BaseViewModel
    {
        #region Datenfelder

        // Strings bewusst mit Leer-String initialisiert, damit Bindings nie auf null zeigen
        private string _Email = string.Empty;
        private string _Passwort = string.Empty;
        private string _Nachricht = string.Empty;
        private bool _IstNachrichtBereit = false;

        /// <summary>
        /// E-Mail-Adresse des Benutzers
        /// </summary>
        public string Email
        {
            get => _Email;
            set { _Email = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Passwort des Benutzers
        /// </summary>
        public string Passwort
        {
            get => _Passwort;
            set { _Passwort = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Meldung an den Benutzer (z. B. Validierungsfehler)
        /// </summary>
        public string Nachricht
        {
            get => _Nachricht;
            set
            {
                _Nachricht = value ?? string.Empty;
                IstNachrichtBereit = !string.IsNullOrEmpty(_Nachricht);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// True, wenn eine Meldung angezeigt werden soll
        /// </summary>
        public bool IstNachrichtBereit
        {
            get => _IstNachrichtBereit;
            set { _IstNachrichtBereit = value; OnPropertyChanged(); }
        }

        #endregion

        #region Befehl: Anmelden

        private Befehl? _Anmelden;

        /// <summary>
        /// Führt die Anmeldung aus:
        /// - zeigt Spinner an (Angemeldungläuft)
        /// - ruft AuthentifizierungsManager.AuthentifizierenAsync(...)
        /// - schaltet bei Erfolg die UI auf „angemeldet“
        /// </summary>
        public Befehl Anmelden
        {
            get
            {
                if (_Anmelden == null)
                {
                    _Anmelden = new Befehl(async _ =>
                    {
                        // Dienste aus dem Kontext holen
                        var ui = this.Kontext.Produziere<OberflächeManager>();
                        var auth = this.Kontext.Produziere<AuthentifizierungsManager>();

                        // Sicherstellen, dass Auth auf DIESEM ViewModel arbeitet (für Fehlermeldungen)
                        auth.Anmeldung = this;

                        // Spinner ein
                        ui.Angemeldungläuft = true;

                        // Gegen das Backend anmelden
                        var ok = await auth.AuthentifizierenAsync(this.Email ?? string.Empty,
                                                                  this.Passwort ?? string.Empty);

                        // Spinner aus
                        ui.Angemeldungläuft = false;

                        if (ok)
                        {
                            // Erfolgreich → Oberfläche umschalten und Eingaben leeren
                            ui.IstAngemeldet = true;
                            auth.DatenLöschen();
                        }
                        else
                        {
                            // Fehlgeschlagen: Nachricht wurde bereits von AuthentifizierenAsync gesetzt
                            ui.IstAngemeldet = false;
                        }
                    });
                }
                return _Anmelden;
            }
        }

        #endregion
    }
}
