using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ERP.UI.Models;
using ERP.UI.Views;
using ERP.UI.Commands;
using ERP.Data;

namespace ERP.UI.ViewModel
{
    /// <summary>
    /// Verwaltet die Daten des derzeit angemeldeten Benutzers (Aufgaben/Gruppen)
    /// und reagiert auf Ereignisse aus dem Datenmodell.
    /// WICHTIG: Die Login-Steuerung (Spinner/Navigation) liegt jetzt im Login-ViewModel
    /// über OberflächeManager + AuthentifizierungsManager, nicht mehr hier.
    /// </summary>
    internal class AufgabenManager : BaseViewModel
    {
        #region Datendienst (Model)

        private Models.DatenManager _DatenManager = null!;

        /// <summary>
        /// Dienst zum Arbeiten mit Benutzerdaten aus der Datenquelle.
        /// </summary>
        protected Models.DatenManager DatenManager
        {
            get
            {
                if (this._DatenManager == null)
                {
                    this.Kontext.Log.StartMelden();

                    Verbindungsprüfer.EinstellungenEinrichten();

                    this._DatenManager = this.Kontext.Produziere<Models.DatenManager>();

                    if (this._DatenManager.Benutzer == null)
                        this.IstBeschäftigt = true;

                    // Wenn Gruppen aktualisiert wurden → UI auffrischen
                    this._DatenManager.GruppenAktualisiert += (sender, e) =>
                    {
                        this.IstBeschäftigt = false;
                        this.OnPropertyChanged(nameof(Gruppen));
                        this.OnPropertyChanged(nameof(AktuelleGruppe));
                        this.OnPropertyChanged(nameof(Aufgaben));
                    };

                    // Hier wurde Oberfläche.Angemeldungläuft/IstAngemeldet gesetzt.
                    // Die Login-Steuerung erfolgt jetzt zentral im Login-ViewModel.
                    //           Falls dein DatenManager dieses Ereignis weiterhin feuert,
                    //           ist es hier absichtlich „ohne UI-Umschaltung“, um Doppelsteuerung zu vermeiden.
                    /*
                    this._DatenManager.AuthentifizierungAbgeschlossen += (sender, e) =>
                    {
                        // NICHT MEHR: Oberfläche.Angemeldungläuft/IstAngemeldet setzen.
                        // Das übernimmt Anmeldungsdaten.LoginAsync (Schritt 2/3).
                    };
                    */

                    this._DatenManager.AufgabenInitialisiert += (sender, e) =>
                    {
                        this.IstBeschäftigt = false;
                        this.OnPropertyChanged(nameof(Aufgaben));
                    };

                    this._DatenManager.SpeicherManager.SpeichernAbgeschlossen += (sender, e) =>
                    {
                        this.IstBeschäftigt = false;
                        if (e.Status == 0)
                        {
                            NachrichtBox.Anzeigen(Views.Texte.SchlüsselAbgelaufen, NachrichtTyp.Ok);
                        }
                    };

                    // Fehlerfall: Busy-Zustand sicher beenden
                    this._DatenManager.FehlerAufgetreten += (sender, e) =>
                    {
                        this.IstBeschäftigt = false;
                    };

                    this.Kontext.Log.EndeMelden();
                }
                return this._DatenManager;
            }
        }

        #endregion

        #region OberflächeManager

        private OberflächeManager _Oberfläche = null!;

        /// <summary>
        /// Zugriff auf die Oberfläche-Steuerung (Navigation, Spinner).
        /// </summary>
        public OberflächeManager Oberfläche
        {
            get
            {
                if (this._Oberfläche == null)
                {
                    this._Oberfläche = this.Kontext.Produziere<OberflächeManager>();
                    this.Oberfläche.InternetVerbindungGeändert += (sender, e) =>
                    {
                        if (this.Oberfläche.IstAngemeldet)
                        {
                            this.DatenManager.OnlineAnmelden();
                        }
                    };
                }
                return this._Oberfläche;
            }
        }

        #endregion

        #region Informationen über die Aufgaben des Benutzers

        public string BenutzerName
        {
            get => (this.DatenManager?.Benutzer?.Name)!;
            private set { this.DatenManager.Benutzer.Name = value; this.OnPropertyChanged(); }
        }

        public string BenutzerEmail
        {
            get => (this.DatenManager?.Benutzer?.Email)!;
            private set { this.DatenManager.Benutzer.Email = value; this.OnPropertyChanged(); }
        }

        public ERP.Data.AufgabenGruppe? AktuelleGruppe
        {
            get => this.DatenManager.AktuelleGruppe;
            set { this.DatenManager.AktuelleGruppe = value; this.OnPropertyChanged(); this.OnPropertyChanged(nameof(Aufgaben)); }
        }

        public ERP.Data.AufgabenGruppen? Gruppen
        {
            get => this.DatenManager.Gruppen;
            set { this.DatenManager.Gruppen = value!; this.OnPropertyChanged(); }
        }

        
        #endregion

        #region Zum Arbeiten mit Benutzerdaten (Eingaben für neue Aufgaben/Gruppen)

        private BenutzerDaten _Daten = null!;
        public BenutzerDaten Daten
        {
            get { return this._Daten ??= new BenutzerDaten(); }
        }

        #endregion

        #region Authentifizierung (Services referenzieren)

        private AuthentifizierungsManager _Authentifizierung = null!;

        /// <summary>
        /// Dienst zum Verwalten von Anmelde-/Registrierungsdaten und API-Login.
        /// </summary>
        public AuthentifizierungsManager Authentifizierung
        {
            get
            {
                if (this._Authentifizierung == null)
                    this._Authentifizierung = this.Kontext.Produziere<AuthentifizierungsManager>();
                return this._Authentifizierung;
            }
        }

        #endregion

        #region Benutzerverwaltung (Registrieren/Abmelden)

        private ERP.UI.Commands.Befehl _BenutzerRegistrieren = null!;
        /// <summary>
        /// Befehl zum Registrieren eines Benutzers.
        /// </summary>
        public ERP.UI.Commands.Befehl BenutzerRegistrieren
        {
            get
            {
                if (this._BenutzerRegistrieren == null)
                {
                    this._BenutzerRegistrieren = new ERP.UI.Commands.Befehl(async _ => await this.Registrieren());
                }
                return this._BenutzerRegistrieren;
            }
        }

        /// <summary>
        /// Registriert einen neuen Benutzer falls die Daten vorhanden sind
        /// und meldet ihn optional direkt an (über den AuthentifizierungsManager!).
        /// </summary>
        private async Task Registrieren()
        {
            this.IstBeschäftigt = true;

            // Online-Status: aktuell immer true (Kürze). Bei Bedarf hier echte Prüfung einbauen.
            bool istOnline = true;
            //ERP.UI.Properties.Settings.Default.Offline = false;

            if (!istOnline)
            {
                NachrichtBox.Anzeigen(Views.Texte.RegistrierenWarnung, NachrichtTyp.Ok);
                this.IstBeschäftigt = false;
                return;
            }

            // Validierte Registrierungsdaten holen
            var registrierung = this.Authentifizierung.HoleRegistrierungsDaten();
            if (registrierung != null)
            {
                // Kurze Busy-Anzeige
                this.Oberfläche.Angemeldungläuft = true;

                var r = await this.DatenManager.Registrieren(registrierung);

                this.Oberfläche.Angemeldungläuft = false;

                if (r == 1)
                {
                    // Erfolgreich → Nachfrage, ob direkt anmelden
                    var result = NachrichtBox.Anzeigen(Views.Texte.RegistrierungErfolgreich, NachrichtTyp.JaNein);

                    if (result == MessageBoxResult.Yes)
                    {
                        this.Oberfläche.Angemeldungläuft = true;

                        // *** NEU: Anmeldung über AuthentifizierungsManager,
                        // damit die gleiche Login-Pipeline genutzt wird (Spinner/Navigation).
                        bool ok = await this.Authentifizierung.AuthentifizierenAsync(registrierung.Email, registrierung.Passwort);

                        if (ok)
                        {
                            this.Oberfläche.IstAngemeldet = true;
                            this.Authentifizierung.DatenLöschen();
                        }
                        else
                        {
                            this.Oberfläche.IstAngemeldet = false;
                            this.Authentifizierung.Anmeldung.Nachricht = Views.Texte.AnmeldungFehler.ToString();
                        }

                        this.Oberfläche.Angemeldungläuft = false;
                    }
                }
                else
                {
                    NachrichtBox.Anzeigen(Views.Texte.RegistrierungFehler, NachrichtTyp.Ok);
                }
            }

            this.IstBeschäftigt = false;
        }

        /// <summary>
        /// Optional: Anmeldung mit gespeicherten Einstellungen (bestehendes Verhalten belassen).
        /// </summary>
        public void AnmeldenMitEinstellungen()
        {
            var email = ERP.UI.Properties.Settings.Default.Email;
            var passwort = ERP.UI.Properties.Settings.Default.Passwort;

            if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(passwort))
            {
                var anmeldung = new ERP.Data.Anmeldung { Email = email, Passwort = passwort };
                this.Oberfläche.Angemeldungläuft = true;
                this.DatenManager.Anmelden(anmeldung); // Falls du willst, kannst du auch hier AuthentifizierenAsync verwenden.
            }
        }
        // ================== AufgabenManager.cs – im Abschnitt „Benutzerverwaltung“ ==================

        /// <summary>
        /// Internes Feld für den Anmelde-Befehl.
        /// </summary>
        private ERP.UI.Commands.Befehl _BenutzerAnmelden = null!;


        /// <summary>
        /// Befehl zum Anmelden des Benutzers.
        /// - liest E-Mail/Passwort aus dem Login-ViewModel,
        /// - zeigt währenddessen den Spinner (Angemeldungläuft),
        /// - schaltet bei Erfolg die Oberfläche auf „angemeldet“.
        /// </summary>
        public ERP.UI.Commands.Befehl BenutzerAnmelden
        {
            get
            {
                if (_BenutzerAnmelden == null)
                {
                    _BenutzerAnmelden = new ERP.UI.Commands.Befehl(async _ =>
                    {
                        // 1) Doppelklicks vermeiden: wenn gerade angemeldet wird, nichts tun
                        if (this.Oberfläche.Angemeldungläuft) return;

                        // 2) Spinner EIN
                        this.Oberfläche.Angemeldungläuft = true;
                        this.Authentifizierung.Anmeldung.Nachricht = "Anmeldung läuft...";

                        try
                        {
                            // 3) Anmeldedaten aus dem Login-ViewModel holen
                            var email = this.Authentifizierung?.Anmeldung?.Email ?? string.Empty;
                            var pass = this.Authentifizierung?.Anmeldung?.Passwort ?? string.Empty;

                            // 4) Gegen Backend authentifizieren (setzt Fehlermeldung bei Bedarf selbst)
                            var ok = await this.Authentifizierung!.AuthentifizierenAsync(email, pass);

                            if (ok)
                            {
                                // 5) Erfolg → UI auf „angemeldet“ umschalten
                                this.Oberfläche.IstAngemeldet = true;

                                // Erfolgsmeldung
                                this.Authentifizierung.Anmeldung.Nachricht = "✅ Anmeldung erfolgreich!";

                                // 6) Fallback: falls Setter nicht sofort umschaltet, View explizit setzen
                                if (this.Oberfläche.Benutzeroberfläche is not ERP.UI.Views.DashboardView)
                                    this.Oberfläche.Benutzeroberfläche = new ERP.UI.Views.DashboardView();

                                // 7) Eingabefelder leeren
                                this.Authentifizierung.DatenLöschen();
                            }
                            else
                            {
                                // Fehlschlag → Meldung steht bereits in Anmeldung.Nachricht
                                this.Oberfläche.IstAngemeldet = false;
                            }
                            
                        }
                        finally
                        {
                            // 8) Spinner AUS (immer)
                            this.Oberfläche.Angemeldungläuft = false;
                        }
                    }
                    );
                }
                return _BenutzerAnmelden;
            }
        }
        /// <summary>
        /// Ruft die Aufgaben der Aktuellen Gruppe ab
        /// oder legt diese fest
        /// </summary>
        public ERP.Data.Aufgaben? Aufgaben
        {
            get
            {
                if (this.AktuelleGruppe != null)
                {
                    if (this.AktuelleGruppe.Aufgaben == null)
                    {
                        this.IstBeschäftigt = true;
                        this.DatenManager.InitialisiereAufgaben();

                        this.AktuelleGruppe.Aufgaben = new ERP.Data.Aufgaben();
                    }
                }
                return this.AktuelleGruppe?.Aufgaben;
            }
            set
            {
                this.AktuelleGruppe!.Aufgaben = value!;
                this.OnPropertyChanged();
            }
        }


        private ERP.UI.Commands.Befehl _BenutzerAbmelden = null!;
        /// <summary>
        /// Befehl zum Abmelden des aktuell angemeldeten Benutzers.
        /// </summary>
        public ERP.UI.Commands.Befehl BenutzerAbmelden
        {
            get
            {
                if (this._BenutzerAbmelden == null)
                {
                    this._BenutzerAbmelden = new ERP.UI.Commands.Befehl(_ =>
                    {
                        this.Oberfläche.IstAngemeldet = false;

                        if (this.Gruppen != null && this.AktuelleGruppe != null)
                        {
                            this.Aufgaben = null!;
                            this.Gruppen = null!;
                            this.AktuelleGruppe = null!;
                            this.BenutzerEmail = null!;
                            this.BenutzerName = null!;
                            this.DatenManager!.Benutzer = null!;
                        }

                        ERP.UI.Properties.Settings.Default.Passwort = null;
                        ERP.UI.Properties.Settings.Default.Email = null;
                        ERP.UI.Properties.Settings.Default.AngemeldetBleiben = false;
                    });
                }
                return this._BenutzerAbmelden;
            }
        }

        #endregion

        #region Bearbeiten der Aufgaben und Gruppen

        private ERP.UI.Commands.Befehl _GruppeHinzufügen = null!;
        public ERP.UI.Commands.Befehl GruppeHinzufügen
        {
            get
            {
                if (this._GruppeHinzufügen == null)
                {
                    this._GruppeHinzufügen = new ERP.UI.Commands.Befehl(_ =>
                    {
                        var gruppe = this.Daten.HoleNeueGruppe();
                        if (gruppe != null)
                        {
                            this.DatenManager.GruppeHinzufügen(gruppe);
                        }
                        this.Daten.Gruppe = null!;
                    });
                }
                return this._GruppeHinzufügen;
            }
        }

        


        private ERP.UI.Commands.Befehl _LöscheAufgabe = null!;
        public ERP.UI.Commands.Befehl LöscheAufgabe
        {
            get
            {
                if (this._LöscheAufgabe == null)
                {
                    this._LöscheAufgabe = new ERP.UI.Commands.Befehl(d =>
                    {
                        if (d is ERP.Data.Aufgabe aufgabe)
                        {
                            this.DatenManager?.AufgabeLöschen(aufgabe);
                            // TODO: ggf. Sicherheitsabfrage einbauen
                        }
                    });
                }
                return this._LöscheAufgabe;
            }
        }

        private ERP.UI.Commands.Befehl _LöscheGruppe = null!;
        public ERP.UI.Commands.Befehl LöscheGruppe
        {
            get
            {
                if (this._LöscheGruppe == null)
                {
                    this._LöscheGruppe = new ERP.UI.Commands.Befehl(d =>
                    {
                        if (d is ERP.Data.AufgabenGruppe gruppe)
                        {
                            if (0 == string.Compare(this.AktuelleGruppe?.Name, gruppe.Name))
                            {
                                var gruppeIndex = this.Gruppen?.IndexOf(gruppe);
                                if (gruppeIndex != null)
                                {
                                    if (gruppeIndex > 0)
                                        this.AktuelleGruppe = this.Gruppen?[(int)gruppeIndex - 1];
                                    else if (this.Gruppen?.Count > 1)
                                        this.AktuelleGruppe = this.Gruppen?[(int)gruppeIndex + 1];
                                }
                            }
                            this.DatenManager?.GruppeLöschen(gruppe);
                        }
                    });
                }
                return this._LöscheGruppe;
            }
        }

        #endregion

        #region Anmeldeeinstellung

        /// <summary>
        /// Steuert, ob die Anwendung Benutzerdaten merken soll.
        /// </summary>
        public bool SollAngemeldetSein
        {
            get => ERP.UI.Properties.Settings.Default.AngemeldetBleiben;
            set
            {
                if (value == false)
                {
                    ERP.UI.Properties.Settings.Default.Email = null!;
                    ERP.UI.Properties.Settings.Default.Passwort = null!;
                }
                else
                {
                    ERP.UI.Properties.Settings.Default.Email = this._DatenManager?.Benutzer?.Email;
                    ERP.UI.Properties.Settings.Default.Passwort = this._DatenManager?.Benutzer?.Passwort;
                }
                ERP.UI.Properties.Settings.Default.AngemeldetBleiben = value;
                this.OnPropertyChanged();
            }
        }

        #endregion
    }
}
