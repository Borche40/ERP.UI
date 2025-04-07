


using System.Windows;
using ERP.UI.Commands;
using ERP.UI.Models;
using ERP.UI.ViewModel;

namespace ERP.UI.ViewModel
{
    /// <summary>
    /// Stellt einen Dienst zum verwalten der Benutzeroberfläche bereit
    /// </summary>
    internal class OberflächeManager :BaseViewModel
    {
        #region Infomationen über die Oberfläche

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private bool? _IstOnline = null!;

        /// <summary>
        /// Ruft den Warheitswert ab der 
        /// beschreibt ob eine Internetverbindung vorhanden ist
        /// </summary>
        public bool? IstOnline
        {
            get
            {
                if (this._IstOnline == null)
                {
                    this._IstOnline = !ERP.UI.Properties.Settings.Default.Offline;
                    this.VerwendeInternetVerbindungsprüfer();
                    this.Starten();
                }
                return this._IstOnline;

            }
            set
            {
                if (this._IstOnline != value)
                {
                    if (value != null)
                    {
                        this._IstOnline = value;
                        this.OnInternetVerbindungGeändert(EventArgs.Empty);

                        if (value == false)
                        {
                            ERP.UI.Properties.Settings.Default.WarOffline = true;
                        }
                    }
                    this.OnPropertyChanged();
                }

            }
        }

        /// <summary>
        /// Wird ausgelöst, wenn die Internet geändert wurde.
        /// </summary>
        public event System.EventHandler? InternetVerbindungGeändert = null!;

        /// <summary>
        /// Löst das Ereignis InternetVerbindungGeändert aus
        /// </summary>
        /// <param name="e">Ereignisdaten</param>
        protected virtual void OnInternetVerbindungGeändert(System.EventArgs e)
        {
            this.InternetVerbindungGeändert?.Invoke(this, e);
        }

        /// <summary>
        /// Kontrolliert wie oft die Internetverbindungsprüfung ausgelöst wird.
        /// </summary>
        private static System.Timers.Timer Timer { get; set; } = null!;

        /// <summary>
        /// Startet den Timer für die Internetverbindungsprüfung.
        /// </summary>
        public void Starten()
        {
            OberflächeManager.Timer = new System.Timers.Timer(3000);
            OberflächeManager.Timer.Elapsed += (sender, e) => this.VerwendeInternetVerbindungsprüfer();
            OberflächeManager.Timer.AutoReset = true;
            OberflächeManager.Timer.Enabled = true;

        }


        /// <summary>
        /// Verwendet den Internetverbindungsprüfer, um die Internetverbindung zu überprüfen.
        /// </summary>
        private async void VerwendeInternetVerbindungsprüfer()
        {
            // Einmalige Überprüfung der Internetverbindung
            this.IstOnline = await Verbindungsprüfer.PrüfeServerVerbindung();
        }

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private bool _IstAngemeldet = false;

        /// <summary>
        /// Ruft einen Wahrheitswert ab,
        /// der bestimmt, ob ein Benutzer
        /// angemeldet ist
        /// oder nicht, und legt dieses fest
        /// </summary>
        public bool IstAngemeldet
        {
            get => this._IstAngemeldet;
            set
            {
                this._IstAngemeldet = value;
                this.InitialisiereOberfläche();
            }
        }
        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private bool _Angemeldungläuft = false;

        /// <summary>
        /// Ruft einen Wahrheitswert ab,
        /// der bestimmt, ob eine Anmeldung
        /// durchgefürt wird
        /// oder nicht, und legt dieses fest
        /// </summary>
        public bool Angemeldungläuft
        {
            get => this._Angemeldungläuft;
            set
            {
                if (this._Angemeldungläuft != value)
                {
                    this._Angemeldungläuft = value;

                    this.InitialisiereOberfläche();
                    this.OnPropertyChanged();
                    if (value == false)
                    {
                        this.Ladenläuft = false;
                    }

                }

            }
        }

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private bool _Ladenläuft = false;

        /// <summary>
        /// Ruft einen Wahrheitswert ab,
        /// der bestimmt, ob die Laden Animation
        /// durchgefürt wird
        /// oder nicht, und legt dieses fest
        /// </summary>
        public bool Ladenläuft
        {
            get => this._Ladenläuft;
            set
            {
                if (this._Ladenläuft != value)
                {
                    this._Ladenläuft = value;
                    this.OnPropertyChanged();
                }

            }
        }
        #endregion Infomationen über die Oberfläche

        #region Zum Anzeigen der Benutzeroberfläche

        /// <summary>
        /// Internes Feld für die Benutzeroberfläche
        /// </summary>
        private System.Windows.Controls.UserControl _Benutzeroberfläche = null!;

        /// <summary>
        /// Ruft die View zum Anzeigen der Aktuellen Benutzeroberfläche,
        /// oder legt diese fest
        /// </summary>
        public System.Windows.Controls.UserControl Benutzeroberfläche
        {
            get
            {
                if (this._Benutzeroberfläche == null)
                {
                    this.InitialisiereOberfläche();
                }
                return this._Benutzeroberfläche!;
            }
            set
            {
                this._Benutzeroberfläche = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Stellt die View für die Benutzeroberfläche
        /// Usercontrol bereit, die sich ändert
        /// entschprechend dem ob der Benutzer Angemeldet ist
        /// </summary>
        protected virtual void InitialisiereOberfläche()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                System.Windows.Controls.UserControl Oberfläche = null!;

                if (this.Angemeldungläuft == true)
                {
                    Oberfläche = new Views.BeschäftigtView();
                }
                else if (this.IstAngemeldet == false)
                {
                    Oberfläche = new Views.LoginView();
                }
                else if (this.IstAngemeldet == true &&
                this.AufgabenOberfläche.HauptfensterBreite > this.AufgabenOberfläche.MinimaleFensterBreite)
                {
                    Oberfläche = new Views.BenutzerDatenView();
                }
                else
                {
                    Oberfläche = new Views.BenutzerDatenSchmal();
                    this.AufgabenOberfläche.SindHilfsTastenAngezeigt = true;
                }
                this.Benutzeroberfläche = Oberfläche;

                this.Benutzeroberfläche.Loaded += (sender, e) =>
                {
                    this.Ladenläuft = true;
                }; // Erst wenn die View geladen ist, die Laden Animation durchführen

                this.Benutzeroberfläche.UpdateLayout();

                // Ohne UpdateLayout() sieht man den Fortschritts-Spinner nicht,
                // eventuell sieht man ihn, aber er dreht sich nicht.
            });
        }



        #endregion Zum Anzeigen der Benutzeroberfläche

        #region Zum Steuren der Anmeldung/Registrierung Oberfläche

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private ERP.UI.Commands.Befehl _AnmeldungAnzeigen = null!;

        /// <summary>
        /// Ruft den Befehl zum Anzeigen der AnmeldungsView
        /// </summary>
        public ERP.UI.Commands.Befehl AnmeldungAnzeigen
        {
            get
            {
                if (this._AnmeldungAnzeigen == null)
                {
                    this._AnmeldungAnzeigen =
                        new ERP.UI.Commands.Befehl(d =>
                        this.Benutzeroberfläche = new Views.LoginView());
                }
                return this._AnmeldungAnzeigen;
            }
        }

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private ERP.UI.Commands.Befehl _RegistrierungAnzeigen = null!;

        /// <summary>
        /// Ruft den Befehl zum Anzeigen der RegistrierungsView
        /// </summary>
        public ERP.UI.Commands.Befehl RegistrierungAnzeigen
        {
            get
            {
                if (this._RegistrierungAnzeigen == null)
                {
                    this._RegistrierungAnzeigen =
                        new ERP.UI.Commands.Befehl(d => this.Benutzeroberfläche = new Views.RegisterView());
                }
                return this._RegistrierungAnzeigen;
            }
        }
        #endregion Zum Steuren der Anmeldung/Registrierung Oberfläche

        #region AufgabenOberfläche

        /// <summary>
        /// Internes Feld für die Benutzeroberfläche
        /// </summary>
        private AufgabenOberflächeManager _AufgabenOberfläche = null!;

        /// <summary>
        /// Ruft die View zum Anzeigen der Aktuellen Aufgabenoberfläche,
        /// oder legt diese fest
        /// </summary>
        public AufgabenOberflächeManager AufgabenOberfläche
        {
            get
            {
                if (this._AufgabenOberfläche == null)
                {
                    this._AufgabenOberfläche = this.Kontext.Produziere<AufgabenOberflächeManager>();
                }
                return this._AufgabenOberfläche;
            }
        }

        #endregion AufgabenOberfläche

        #region FensterAktualisieren
        /// <summary>
        /// Aktualisiert das Hauptfenster basierend auf der angegebenen Breite.
        /// Ändert die Ansicht der Aufgaben- und Benutzeroberfläche abhängig von der Fensterbreite.
        /// </summary>
        /// <param name="breite">Die aktuelle Breite des Fensters.</param>
        public void HauptfensterAktualisieren(double breite)
        {
            if (breite > this.AufgabenOberfläche.MinimaleAufgabenBreite)
            {
                if (this.AufgabenOberfläche.AufgabenEinstellungen is not Views.AufgabenEinstellungenBreit)
                {
                    this.AufgabenOberfläche.AufgabenEinstellungen = new Views.AufgabenEinstellungenBreit();
                }
            }
            else
            {
                if (this.AufgabenOberfläche.AufgabenEinstellungen is not Views.AufgabenEinstellungenSchmal)
                {
                    this.AufgabenOberfläche.AufgabenEinstellungen = new Views.AufgabenEinstellungenSchmal();
                }
            }
            if (breite > this.AufgabenOberfläche.MinimaleFensterBreite && this.IstAngemeldet && !this.Angemeldungläuft)
            {
                if (this.Benutzeroberfläche is not Views.BenutzerDatenView)
                {
                    this.Benutzeroberfläche = new Views.BenutzerDatenView();
                    this.AufgabenOberfläche.SindHilfsTastenAngezeigt = false;

                }
            }
            else if (this.IstAngemeldet && !this.Angemeldungläuft)
            {
                if (this.Benutzeroberfläche is not Views.BenutzerDatenSchmal)
                {
                    this.Benutzeroberfläche = new Views.BenutzerDatenSchmal();
                    this.AufgabenOberfläche.SindHilfsTastenAngezeigt = true;

                }
            }
        }
        #endregion FensterAktualisieren


    }
}
