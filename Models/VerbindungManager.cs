

using ERP.Data;

namespace ERP.UI.Models
{

    /// <summary>
    /// Delegat für Ereignisse, die Aufgaben betreffen.
    /// </summary>
    /// <param name="gruppe">Die Aufgaben-Gruppe.</param>
    /// <param name="email">Die E-Mail-Adresse des Benutzers.</param>
    public delegate void AufgabenEventHandler(string gruppe, string email);

    /// <summary>
    /// Delegat für Ereignisse, die Gruppen betreffen.
    /// </summary>
    /// <param name="email">Die E-Mail-Adresse des Benutzers.</param>
    public delegate void GruppenEventHandler(string email);


    /// <summary>
    /// Stellt die Verbindung zum SignalR-Hub-Dienst für die Kommunikation zwischen den Clients bereit.
    /// </summary>
    internal class VerbindungManager : Anwendung.AppObjekt
    {
        #region Controller

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private IVerbindungController _LokalerController = null!;

        /// <summary>
        /// Ruft den Dienst zum senden von Nachrichten durch die OfflineHub klasse
        /// </summary>
        private IVerbindungController LokalerController
        {
            get
            {
                if (this._LokalerController == null)
                {
                    this._LokalerController
                        = this.Kontext
                            .Produziere<LokalerVerbindungController>();
                }
                return this._LokalerController;
            }
        }

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private IVerbindungController _OnlineController = null!;

        /// <summary>
        /// Ruft den Dienst zum senden von Nachrichten durch die OfflineHub klasse
        /// </summary>
        private IVerbindungController OnlineController
        {
            get
            {
                if (this._OnlineController == null)
                {
                    this._OnlineController
                        = this.Kontext
                            .Produziere<OnlineVerbindungController>();
                }
                return this._OnlineController;
            }
        }

        #endregion Controller

        #region Verbindungsmanagement

        /// <summary>
        /// Ruft einen Wahrheitswert ab,
        /// der bestimmt, ob der Benutzer der Sender der Daten ist
        /// oder nicht und legt dieses fest
        /// </summary>
        public bool IstSender { get; set; } = false;


        /// <summary>
        /// Stellt eine Verbindung mit dem Hub service für den Benutzer bereit
        /// </summary>        
        /// <param name="email">Die Email-adresse des Benutzers</param>
        public void VerbindungHerstellen(string email)
        {

            AufgabenEventHandler aufgaben = (gruppe, email) =>
            {

                this.OnAufgabenAktualisiert(new HubEventArgs(gruppe));


            };

            GruppenEventHandler gruppen = (email) =>
            {
                this.OnGruppenAktualisiert(EventArgs.Empty);

            };

            if (ERP.UI.Properties.Settings.Default.Offline)
            {
                this.LokalerController.VerbindungHerstellen(email, aufgaben, gruppen);
            }
            else
            {
                this.OnlineController.VerbindungHerstellen(email, aufgaben, gruppen);
            }

        }

        /// <summary>
        /// Trennt die Verbindung mit dem Hub service
        /// </summary>
        /// <param name="email">Die Email-adresse des Benutzers</param>
        public void VerbindungAbmelden(string email)
        {
            if (ERP.UI.Properties.Settings.Default.Offline)
            {
                this.LokalerController.VerbindungAbmelden(email);
            }
            else
            {
                this.OnlineController.VerbindungAbmelden(email);
            }
        }

        #endregion Verbindungsmanagement

        #region Aufgabenmeldung

        /// <summary>
        /// Wird ausgelöst, wenn die Initialisierung der Aufgaben abgeschlossen ist.
        /// </summary>
        public event EventHandler<HubEventArgs> AufgabenAktualisiert = null!;

        /// <summary>
        /// Löst das Ereignis "AufgabenAktualisiert" aus.
        /// </summary>
        /// <param name="e">Ereignisdaten</param>
        protected virtual void OnAufgabenAktualisiert(HubEventArgs e)
        {
            this.AufgabenAktualisiert?.Invoke(this, e);
        }

        /// <summary>
        /// Meldet dem Server-Hub, welche Gruppe aktualisiert werden muss,
        /// indem eine Aktualisierungsnachricht an den Hub des Servers gesendet wird, 
        /// damit andere Clients im selben Konto die Daten abrufen können.
        /// </summary>
        /// <param name="gruppe">Die AufgabenGruppe, die aktualisiert werden soll.</param>
        /// <param name="email">Die E-Mail-Adresse des Benutzers.</param>
        public void AufgabenAktualisieren(string gruppe, string email)
        {
            this.IstSender = true;

            if (ERP.UI.Properties.Settings.Default.Offline)
            {
                this.LokalerController.AufgabenAktualisieren(gruppe, email);
            }
            else
            {
                this.OnlineController.AufgabenAktualisieren(gruppe, email);
            }
        }
        #endregion Aufgabenmeldung

        #region Gruppenmeldung

        /// <summary>
        /// Wird ausgelöst, wenn die Initialisierung der Gruppen abgeschlossen ist.
        /// </summary>
        public event EventHandler? GruppenAktualisiert = null!;

        /// <summary>
        /// Löst das Ereignis "GruppenAktualisiert" aus.
        /// </summary>
        /// <param name="e">Ereignisdaten</param>
        protected virtual void OnGruppenAktualisiert(EventArgs e)
        {
            this.GruppenAktualisiert?.Invoke(this, e);
        }

        /// <summary>
        /// Meldet dem Server-Hub, das die Gruppen aktualisiert werden muss,
        /// indem eine Aktualisierungsnachricht an den Hub des Servers gesendet wird, 
        /// damit andere Clients im selben Konto die Daten abrufen können.
        /// </summary>
        /// <param name="email">Die E-Mail-Adresse des Benutzers.</param>
        public void GruppenAktualisieren(string email)
        {
            this.IstSender = true;
            if (ERP.UI.Properties.Settings.Default.Offline)
            {
                this.LokalerController.GruppenAktualisieren(email);
            }
            else
            {
                this.OnlineController.GruppenAktualisieren(email);
            }
        }

        #endregion Gruppenmeldung

    }
}