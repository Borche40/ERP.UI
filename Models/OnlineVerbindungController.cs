using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.UI.Models
{
    /// <summary>
    /// Stellt die Verbindung zum SignalR-Hub-Dienst 
    /// für die Kommunikation zwischen den Clients bereit.
    /// </summary>
    internal class OnlineVerbindungController:Anwendung.AppObjekt,IVerbindungController
    {
        #region Hubadresse

        /// <summary>
        /// Zum Cachen der Eigenschaft
        /// </summary>
        private static string _Url = null!;

        /// <summary>
        /// Ruft die Adresse des Hubdienst ab,
        /// der die Daten verwaltet
        /// </summary>
        /// <remarks>Der abschließende Schrägstrich
        /// ist sichergestellt. Die Adresse kann
        /// mit OnlineRestBasis konfigurieriert werden</remarks>
        protected string Url
        {
            get
            {
                if (_Url == null)
                {
                    _Url
                        = Properties.Settings.Default.OnlineRestApiBasis;

                    if (!_Url.EndsWith('/'))
                    {
                        _Url += '/';
                    }
                    _Url += "datenhub";
                }

                return _Url;
            }
        }

        #endregion Hubadresse

        #region Hubverbindung

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private HubConnection _Verbindung = null!;

        /// <summary>
        /// Bietet Zugriff auf die Verbindung zum Hub-Dienst.
        /// </summary>
        public HubConnection Verbindung
        {
            get
            {
                if (this._Verbindung == null)
                {
                    this._Verbindung = new HubConnectionBuilder()
                         .WithUrl(Url)
                         .WithAutomaticReconnect()
                         .Build();
                }
                return this._Verbindung;
            }
        }

        #endregion Hubverbindung

        #region Verbindungsmanagement

        /// <summary>
        /// Stellt eine Verbindung mit dem Hub service für den Benutzer bereit
        /// </summary>
        /// <param name="email">Die Email-adresse des Benutzers</param>
        /// <param name="aufgaben">Der Handler für Ereignisse, die Aufgaben betreffen.</param>
        /// <param name="gruppen">Der Handler für Ereignisse, die Gruppen betreffen.</param>
        public async void VerbindungHerstellen(string email, AufgabenEventHandler aufgaben,GruppenEventHandler gruppen)
        {

            this.Verbindung.On<string, string>("HoleAufgaben", (gruppe, email) =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    aufgaben?.Invoke(gruppe, email);
                });
            });


            this.Verbindung.On<string>("HoleGruppen", email =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    gruppen?.Invoke(email);
                });
            });

            if (Verbindung.State == Microsoft.AspNetCore.SignalR.Client.HubConnectionState.Disconnected)
            {
                await Verbindung.StartAsync();
            }


            await Verbindung.InvokeAsync("SitzungBeitreten", email);

        }

        /// <summary>
        /// Trennt die Verbindung mit dem Hub service
        /// </summary>
        /// <param name="email">Die Email-adresse des Benutzers</param>
        public async void VerbindungAbmelden(string email)
        {
            await this.Verbindung.InvokeAsync("Verlassen", email);
            await this.Verbindung.StopAsync();
        }

        #endregion Verbindungsmanagement

        #region Meldung

        /// <summary>
        /// Meldet dem Server-Hub, welche Gruppe aktualisiert werden muss,
        /// indem eine Aktualisierungsnachricht an den Hub des Servers gesendet wird, 
        /// damit andere Clients im selben Konto die Daten abrufen können.
        /// </summary>
        /// <param name="gruppe">Die AufgabenGruppe, die aktualisiert werden soll.</param>
        /// <param name="email">Die E-Mail-Adresse des Benutzers.</param>
        public async void AufgabenAktualisieren(string gruppe, string email)
        {
            await this.Verbindung.InvokeAsync("AufgabenAktualisieren", gruppe, email);
        }


        /// <summary>
        /// Meldet dem Server-Hub, das die Gruppen aktualisiert werden muss,
        /// indem eine Aktualisierungsnachricht an den Hub des Servers gesendet wird, 
        /// damit andere Clients im selben Konto die Daten abrufen können.
        /// </summary>
        /// <param name="email">Die E-Mail-Adresse des Benutzers.</param>
        public async void GruppenAktualisieren(string email)
        {
            await Verbindung.InvokeAsync("GruppenAktualisieren", email);
        }

        #endregion Meldung

    }
}
