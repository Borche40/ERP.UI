using System.Windows;
using ERP.UI.Commands;
using ERP.UI.Models;
using ERP.UI.ViewModel;

namespace ERP.UI.ViewModel
{
    /// <summary>
    /// Stellt einen Dienst zum Verwalten der Benutzeroberfläche bereit.
    /// </summary>
    internal class OberflächeManager : BaseViewModel
    {
        #region Informationen über die Oberfläche
        public OberflächeManager()
        {
            _IstAngemeldet = false;
            
        }
        /// <summary>
        /// Internes Feld für die Eigenschaft.
        /// </summary>
        private bool? _IstOnline = null!;

        /// <summary>
        /// Ruft den Wahrheitswert ab, der beschreibt, ob eine Internetverbindung vorhanden ist.
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
                        this.OnInternetVerbindungGeändert(System.EventArgs.Empty);

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
        /// Wird ausgelöst, wenn sich die Internetverbindung geändert hat.
        /// </summary>
        public event System.EventHandler? InternetVerbindungGeändert = null!;

        /// <summary>
        /// Löst das Ereignis InternetVerbindungGeändert aus.
        /// </summary>
        protected virtual void OnInternetVerbindungGeändert(System.EventArgs e)
        {
            this.InternetVerbindungGeändert?.Invoke(this, e);
        }

        /// <summary>
        /// Kontrolliert, wie oft die Internetverbindungsprüfung ausgelöst wird.
        /// </summary>
        private static System.Timers.Timer Timer { get; set; } = null!;

        /// <summary>
        /// Startet den Timer für die Internetverbindungsprüfung.
        /// </summary>
        public void Starten()
        {
            OberflächeManager.Timer = new System.Timers.Timer(5000);
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
        /// Internes Feld für die Eigenschaft.
        /// </summary>
        private bool _IstAngemeldet = false;

        /// <summary>
        /// Ruft einen Wahrheitswert ab, der bestimmt, ob ein Benutzer angemeldet ist oder nicht,
        /// und legt diesen fest.
        /// </summary>
        public bool IstAngemeldet
        {
            get => this._IstAngemeldet;
            set
            {
                if (this._IstAngemeldet != value)
                {
                    this._IstAngemeldet = value;
                    this.OnPropertyChanged(nameof(IstAngemeldet));
                    this.InitialisiereOberfläche();
                }
            }
        }

        /// <summary>
        /// Internes Feld für die Eigenschaft.
        /// </summary>
        private bool _Angemeldungläuft = false;

        /// <summary>
        /// Ruft einen Wahrheitswert ab, der bestimmt, ob eine Anmeldung durchgeführt wird oder nicht,
        /// und legt diesen fest.
        /// </summary>
        public bool Angemeldungläuft
        {
            get => this._Angemeldungläuft;
            set
            {
                if (this._Angemeldungläuft != value)// Nur bei Statuswechsel reagieren
                {
                    this._Angemeldungläuft = value;

                    // Bei Statuswechsel Oberfläche aktualisieren
                    this.InitialisiereOberfläche();
                    this.OnPropertyChanged();

                    // Wird die Anmeldung beendet, die Ladeanimation beenden
                    if (value == false)
                    {
                        this.Ladenläuft = false;
                    }
                }
            }
        }

        /// <summary>
        /// Internes Feld für die Eigenschaft.
        /// </summary>
        private bool _Ladenläuft = false;

        /// <summary>
        /// Steuert, ob die Ladeanimation durchgeführt wird.
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

        #endregion Informationen über die Oberfläche

        #region Zum Anzeigen der Benutzeroberfläche

        /// <summary>
        /// Internes Feld für die Benutzeroberfläche.
        /// </summary>
        private System.Windows.Controls.UserControl _Benutzeroberfläche = null!;

        /// <summary>
        /// Ruft die View zum Anzeigen der aktuellen Benutzeroberfläche ab oder legt diese fest.
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
        /// Baut die passende View (UserControl) und setzt ihren DataContext korrekt,
        /// damit Bindings in allen Oberflächen (Login, Dashboard usw.) funktionieren.
        /// 
        /// - LoginView  -> DataContext = HauptfensterViewModel (damit Benutzer.* erreichbar ist)
        /// - DashboardView -> DataContext = Dashboard-ViewModel (für Online-Status, Zeit, Diagramm)
        /// - BenutzerDatenView/Schmal -> DataContext = AufgabenManager (= HauptfensterVM.Benutzer)
        /// - BeschäftigtView -> egal (setzt HauptfensterVM)
        /// </summary>
        public virtual void InitialisiereOberfläche()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                System.Windows.Controls.UserControl oberfläche;

             
                //  Beschäftigt-Ansicht beim Anmeldevorgang
                
                if (this.Angemeldungläuft == true)
                {
                    oberfläche = new ERP.UI.Views.BeschäftigtView();
                }

            
                // Login-Ansicht, wenn Benutzer nicht angemeldet ist
                
                else if (this.IstAngemeldet == false)
                {
                    oberfläche = new ERP.UI.Views.LoginView();
                }

              
                // Dashboard, wenn Benutzer angemeldet ist und Fenster groß genug
                
                else if (this.AufgabenOberfläche.HauptfensterBreite > this.AufgabenOberfläche.MinimaleFensterBreite)
                {
                    var dashboardView = new ERP.UI.Views.DashboardView();

                    // WICHTIG:
                    // Setzt explizit den DataContext, damit Bindings wie Online-Status, Zeit und Chart funktionieren
                    dashboardView.DataContext = new ERP.UI.ViewModel.Dashboard();

                    oberfläche = dashboardView;
                    this.AufgabenOberfläche.SindHilfsTastenAngezeigt = false;
                }

                // Schmale Benutzeransicht bei kleiner Fensterbreite
               
                else
                {
                    oberfläche = new ERP.UI.Views.BenutzerDatenSchmal();
                    this.AufgabenOberfläche.SindHilfsTastenAngezeigt = true;
                }

                
                //  DataContext korrekt zuordnen
               
                var fensterDc = System.Windows.Application.Current.MainWindow?.DataContext;

                // Hauptfenster-ViewModel holen
                var hfvm = fensterDc as ERP.UI.ViewModel.HauptfensterViewModel;

                // Benutzer (AufgabenManager) holen
                var benutzerVm = hfvm?.Benutzer;

                if (oberfläche is ERP.UI.Views.LoginView lv)
                {
                    // Login braucht Zugriff auf Benutzer.* → HauptfensterVM als DC
                    lv.DataContext = hfvm;
                }
                else if (oberfläche is ERP.UI.Views.DashboardView bdv)
                {
                    // DashboardView behält hier seinen eigenen DataContext (Dashboard-VM)
                    // → keine Änderung notwendig
                }
                else if (oberfläche is ERP.UI.Views.BenutzerDatenSchmal bds)
                {
                    // Schmale Benutzeransicht direkt an AufgabenManager binden
                    bds.DataContext = benutzerVm;
                }
                else
                {
                    // Fallback: mindestens HauptfensterVM setzen
                    if (oberfläche is System.Windows.FrameworkElement fe && hfvm != null)
                        fe.DataContext = hfvm;
                }

                
                //  Oberfläche aktivieren und Layout aktualisieren
                
                this.Benutzeroberfläche = oberfläche;

                // Ladeanimation erst nach dem Laden starten
                this.Benutzeroberfläche.Loaded += (s, e) => this.Ladenläuft = true;

                // Layout-Update für flüssige Anzeige
                this.Benutzeroberfläche.UpdateLayout();
            });
        }

        #endregion Zum Anzeigen der Benutzeroberfläche

        #region Zum Steuern der Anmeldung/Registrierung Oberfläche

        /// <summary>
        /// Internes Feld für die Eigenschaft.
        /// </summary>
        private ERP.UI.Commands.Befehl _AnmeldungAnzeigen = null!;

        /// <summary>
        /// Befehl zum expliziten Anzeigen der Anmeldungs-View (z. B. aus Menüs).
        /// Setzt den DataContext korrekt auf das gemeinsame Anmeldungs-ViewModel.
        /// </summary>
        public ERP.UI.Commands.Befehl AnmeldungAnzeigen
        {
            get
            {
                if (this._AnmeldungAnzeigen == null)
                {
                    this._AnmeldungAnzeigen = new ERP.UI.Commands.Befehl(d =>
                    {
                        var view = new Views.LoginView();

                        var anmeldungsVm = this.Kontext.Produziere<Anmeldungsdaten>();
                        view.DataContext = anmeldungsVm;

                        var auth = this.Kontext.Produziere<AuthentifizierungsManager>();
                        auth.Anmeldung = anmeldungsVm;

                        this.Benutzeroberfläche = view;
                    });
                }
                return this._AnmeldungAnzeigen;
            }
        }

        /// <summary>
        /// Internes Feld für die Eigenschaft.
        /// </summary>
        private ERP.UI.Commands.Befehl _RegistrierungAnzeigen = null!;

        /// <summary>
        /// Befehl zum Anzeigen der Registrierungs-View.
        /// Setzt den DataContext auf das gemeinsame Registrierungs-ViewModel.
        /// </summary>
        public ERP.UI.Commands.Befehl RegistrierungAnzeigen
        {
            get
            {
                if (this._RegistrierungAnzeigen == null)
                {
                    this._RegistrierungAnzeigen = new ERP.UI.Commands.Befehl(d =>
                    {
                        var view = new Views.RegisterView();

                        // Gemeinsames Registrierungs-ViewModel aus dem Kontext
                        var registrierungsVm = this.Kontext.Produziere<Registrierungsdaten>();
                        view.DataContext = registrierungsVm;

                        // Optional: Auch im AuthentifizierungsManager spiegeln
                        var auth = this.Kontext.Produziere<AuthentifizierungsManager>();
                        auth.Registrierung = registrierungsVm;

                        this.Benutzeroberfläche = view;
                    });
                }
                return this._RegistrierungAnzeigen;
            }
        }

        #endregion Zum Steuern der Anmeldung/Registrierung Oberfläche

        #region AufgabenOberfläche

        /// <summary>
        /// Internes Feld für die Aufgabenoberfläche.
        /// </summary>
        private AufgabenOberflächeManager _AufgabenOberfläche = null!;

        /// <summary>
        /// Zugriff auf die Aufgabenoberfläche (aus dem gemeinsamen Kontext erzeugt).
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
                if (this.Benutzeroberfläche is not Views.DashboardView)
                {
                    this.Benutzeroberfläche = new Views.DashboardView();
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
