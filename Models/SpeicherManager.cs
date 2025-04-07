using ERP.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ERP.UI.Models
{
    /// <summary>
    /// Stellt einen Dienst zum schreiben
    /// der Demo Daten bereit 
    /// </summary>
    internal class SpeicherManager :Anwendung.AppObjekt
    {
        #region Controller Dienst

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private SpeicherController _Controller = null!;

        /// <summary>
        /// Ruft den Dienst zum Schreiben der Demo 
        /// Daten ab
        /// </summary>
        private SpeicherController Controller
        {
            get
            {
                if (this._Controller == null)
                {

                    this._Controller
                        = this.Kontext
                            .Produziere<SpeicherController>();
                }
                return this._Controller;
            }
        }

        #endregion Controller Dienst

        #region Sitzung

        /// <summary>
        /// Ruft die Email-Adresse des Benutzers ab, oder legt diese fest
        /// </summary>
        public string BenutzerEmail { get; set; } = string.Empty;

        #endregion Sitzung

        #region SpeicherEvent

        /// <summary>
        /// Ereignis, das ausgelöst wird, 
        /// wenn eine Speichernaktion abgeschlossen ist.
        /// </summary>
        public event EventHandler<SpeichernEventArgs>? SpeichernAbgeschlossen = null!;

        /// <summary>
        /// Löst das Ereignis aus, das signalisiert, 
        /// dass eine Speichernaktion abgeschlossen ist.
        /// </summary>
        /// <param name="e">Die Ereignisdaten, 
        /// die Informationen über den Abschluss der Speichernsaktion enthalten.</param>
        public virtual void OnSpeichernAbgeschlossen(SpeichernEventArgs e)
        {
            var behandlerKopie = SpeichernAbgeschlossen;
            behandlerKopie?.Invoke(this, e);
        }
        #endregion SpeicherEvent

        #region Speichern
        /// <summary>
        /// Fügt oder Aktualisiert die Aufgabe für den Benutzer in der Quelle.
        /// </summary>
        /// <param name="gruppe">Die Gruppe, die die Aufgabe enthält.</param>
        /// <param name="onlineSchlüssel">Der Schlüssel, der den zugriff auf die Online-Datenbank erlaubt.</param>
        /// <param name="lokalerSchlüssel">Der Schlüssel, der den zugriff auf die Lokale-Datenbank erlaubt.</param>
        /// <param name="aufgabe">Die Aufgabe, die gespeichert werden soll.</param>
        public async void AufgabeSpeichern(string gruppe, System.Guid? onlineSchlüssel, Aufgabe aufgabe, System.Guid lokalerSchlüssel)
        {
            this.HubVerbindung.IstSender = true;
            int antwort;
            if (ERP.UI.Properties.Settings.Default.Offline)
            {
                antwort = await this.Controller.SpeichereAufgabe(gruppe, aufgabe, lokalerSchlüssel, null);
            }
            else
            {
                antwort = await this.Controller.SpeichereAufgabe(gruppe,aufgabe, lokalerSchlüssel, onlineSchlüssel);
            }

            if (antwort == 1)
            {
                this.HubVerbindung.AufgabenAktualisieren(gruppe, this.BenutzerEmail);
            }
            else
            {
                this.OnSpeichernAbgeschlossen(new SpeichernEventArgs(antwort));
            }

        }

        /// <summary>
        /// Fügt eine AufgabenGruppe für den Benutzer in die Quelle.
        /// </summary>
        /// <param name="onlineSchlüssel">Der Schlüssel, der den zugriff auf die Online-Datenbank erlaubt.</param>
        /// <param name="lokalerSchlüssel">Der Schlüssel, der den zugriff auf die Lokale-Datenbank erlaubt.</param>
        /// <param name="gruppe">Die Aufgaben-Gruppe, die gespeichert werden soll.</param>
        public async void GruppeSpeichern(System.Guid? onlineSchlüssel, System.Guid lokalerSchlüssel, string gruppe)
        {
            this.HubVerbindung.IstSender = true;
            int antwort;
            if (ERP.UI.Properties.Settings.Default.Offline)
            {
                antwort = await this.Controller.SpeichereGruppe( gruppe, lokalerSchlüssel,null);
            }
            else
            {
                antwort = await this.Controller.SpeichereGruppe(gruppe, lokalerSchlüssel,onlineSchlüssel);
            }
            if (antwort == 1)
            {
                this.HubVerbindung.GruppenAktualisieren(this.BenutzerEmail);
            }
            else
            {
                this.OnSpeichernAbgeschlossen(new SpeichernEventArgs(antwort));
            }
        }

        #endregion Speichern

        #region Löschen

        /// <summary>
        /// Löscht eine Aufgabe aus der Quelle
        /// von dem angegebenen Benutzer.
        /// </summary>
        /// <param name="gruppe">Die Gruppe, die die Aufgabe enthält..</param>
        /// <param name="onlineSchlüssel">Der Schlüssel, der den zugriff auf die Online-Datenbank erlaubt.</param>
        /// <param name="lokalerSchlüssel">Der Schlüssel, der den zugriff auf die Lokale-Datenbank erlaubt.</param>
        /// <param name="aufgabe">Die Aufgabe, die gelöscht werden soll.</param>
        public async void AufgabeLöschen(string gruppe, System.Guid? onlineSchlüssel, System.Guid lokalerSchlüssel, Aufgabe aufgabe)
        {
            this.HubVerbindung.IstSender = true;
            int antwort;
            if (ERP.UI.Properties.Settings.Default.Offline)
            {
                antwort = await this.Controller.AufgabeLöschen(gruppe, aufgabe, lokalerSchlüssel, null);
            }
            else
            {
                antwort = await this.Controller.AufgabeLöschen(gruppe, aufgabe, lokalerSchlüssel,onlineSchlüssel);
            }

            if (antwort == 1)
            {
                this.HubVerbindung.AufgabenAktualisieren(gruppe, this.BenutzerEmail);
            }
            else
            {
                this.OnSpeichernAbgeschlossen(new SpeichernEventArgs(antwort));
            }
        }

        /// <summary>
        /// Löscht eine AufgabenGruppe von dem angegebenen Benutzer.
        /// </summary>
        /// <param name="gruppe">Die Gruppe, die gelöscht werden soll.</param>
        /// <param name="onlineSchlüssel">Der Schlüssel, der den zugriff auf die Online-Datenbank erlaubt.</param>
        /// <param name="lokalerSchlüssel">Der Schlüssel, der den zugriff auf die Lokale-Datenbank erlaubt.</param>
        public async void GruppeLöschen(System.Guid? onlineSchlüssel, System.Guid lokalerSchlüssel, string gruppe)
        {
            this.HubVerbindung.IstSender = true;
            int antwort;
            if (ERP.UI.Properties.Settings.Default.Offline)
            {
                antwort = await this.Controller.GruppeLöschen(gruppe,lokalerSchlüssel,null);
            }
            else
            {
                antwort = await this.Controller.GruppeLöschen(gruppe, lokalerSchlüssel,onlineSchlüssel);
            }

            if (antwort == 1)
            {
                this.HubVerbindung.GruppenAktualisieren(this.BenutzerEmail);
            }
            else
            {
                this.OnSpeichernAbgeschlossen(new SpeichernEventArgs(antwort));
            }
        }

        #endregion Löschen

        #region SyncManager

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private Synchronisation.SyncManager _SyncManager = null!;

        /// <summary>
        /// Ruft den Dienst zum Synchronisieren ab
        /// </summary>
        public Synchronisation.SyncManager SyncManager
        {
            get
            {
                if (this._SyncManager == null)
                {
                    this._SyncManager = this.Kontext.Produziere<Synchronisation.SyncManager>();
                    this._SyncManager.LokaleSynchronisationAbgeschlossen += (sender, e) =>
                    {
                        //Die Synchronisation ist abgeschlossen, das Objekt wird nicht mehr gebraucht
                        this._SyncManager = null!;
                    };
                }
                return this._SyncManager;
            }
            set => this._SyncManager = value;
        }

        #endregion SyncManager

        #region Hubverbindung

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private VerbindungManager _HubVerbindung = null!;

        /// <summary>
        /// Ruft den Dienst zum Kommunizieren mit anderen 
        /// Clients auf demselben Konto auf, damit die Daten synchronisiert werden.
        /// </summary>
        public VerbindungManager HubVerbindung
        {
            get
            {
                if (this._HubVerbindung == null)
                {
                    this._HubVerbindung = this.Kontext.Produziere<VerbindungManager>();

                }
                return this._HubVerbindung;
            }
        }
        #endregion Hubverbindung
    }

}
