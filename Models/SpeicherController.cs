using ERP.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.UI.Models
{
    /// <summary>
    /// Stellt einen Dienst für die Synchronisation von Daten 
    /// zwischen der lokalen und der Online-Datenbank bereit.
    /// </summary>
    internal class SpeicherController : Anwendung.AppObjekt
    {
        #region Online-Controller

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private ISpeicherController _OnlineController = null!;

        /// <summary>
        /// Ruft den Dienst zum Speichern der
        /// Daten auf die Online-Datenbank ab.
        /// </summary>
        private ISpeicherController OnlineController
        {
            get
            {
                if (this._OnlineController == null)
                {
                    this._OnlineController = this.Kontext.Produziere<OnlineSpeicherController>();
                }
                return this._OnlineController;
            }
        }
        #endregion Online-Controller

        #region Lokaler-Controller

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private ISpeicherController _LokalerController = null!;

        /// <summary>
        /// Ruft den Dienst zum Speichern der
        /// Daten auf die lokale Datenbank ab.
        /// </summary>
        private ISpeicherController LokalerController
        {
            get
            {
                if (this._LokalerController == null)
                {
                    this._LokalerController = this.Kontext.Produziere<LokalerSpeicherController>();
                }
                return this._LokalerController;
            }
        }

        #endregion Lokaler-Controller

        #region Sync-Controller

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private Synchronisation.DatenModifiziererController _SyncController = null!;

        /// <summary>
        /// Ruft den Dienst zum Markieren von Daten in der Quelle im Offline-Modus ab.
        /// </summary>
        /// <remarks>Damit die App bei der Synchronisation weiß, 
        /// welche Daten gespeichert und welche gelöscht werden müssen.</remarks>
        private Synchronisation.DatenModifiziererController SyncController
        {
            get
            {
                if (this._SyncController == null)
                {
                    this._SyncController = this.Kontext.Produziere<Synchronisation.DatenModifiziererController>();
                }
                return this._SyncController;
            }
        }

        #endregion Sync-Controller

        #region Aufgaben-logik

        /// <summary>
        /// Speichert eine Aufgabe in der angegebenen Gruppe für den Benutzer. 
        /// Falls der Benutzer online ist werden die Offline-Daten angegeben 
        /// und die Daten werden auf die Lokale und Online-Datebank gespeichert.
        /// </summary>
        /// <param name="gruppe">Der Name der Gruppe.</param>
        /// <param name="aufgabe">Die zu speichernde Aufgabe.</param>
        /// <param name="onlineSchlüssel">Der Schlüssel, der den zugriff auf die Online-Datenbank erlaubt.</param>
        /// <param name="lokalerSchlüssel">Der Schlüssel, der den zugriff auf die Lokale-Datenbank erlaubt.</param>
        /// <returns>1 falls das Speichern erfolgreich war, oder 0 falls nicht</returns>
        public async Task<int> SpeichereAufgabe(string gruppe, Aufgabe aufgabe, System.Guid lokalerSchlüssel, System.Guid? onlineSchlüssel = null)
        {
            int SpeicherAntwort;

            if (onlineSchlüssel == null)
            {
                var antwort = await this.LokalerController.SpeichereAufgabeAsync(gruppe, lokalerSchlüssel, aufgabe);
                if (antwort != 0)
                {
                    var syncAntwort =
                        await this.SyncController.SpeichereAufgabeAsync(gruppe, lokalerSchlüssel, aufgabe, false);
                    if (syncAntwort == 0)
                    {
                        antwort = syncAntwort;
                    }
                }
                SpeicherAntwort = antwort;
            }
            else
            {
                var OnlineAntwort = await this.OnlineController.SpeichereAufgabeAsync
                    (gruppe, (System.Guid)onlineSchlüssel, aufgabe);
                var OfflineAntwort = await this.LokalerController.SpeichereAufgabeAsync
                    (gruppe,lokalerSchlüssel, aufgabe);
                if (OnlineAntwort == 2 && OfflineAntwort != 0)
                {
                    var syncAntwort =
                      await this.SyncController.SpeichereAufgabeAsync(gruppe, lokalerSchlüssel, aufgabe, false);
                    if (syncAntwort == 0)
                    {
                        OfflineAntwort= syncAntwort;
                    }
                }

                if (OfflineAntwort == OnlineAntwort)
                {
                    SpeicherAntwort = OfflineAntwort;
                }
                else
                {
                    if (OfflineAntwort == 0)
                    {
                        SpeicherAntwort = OfflineAntwort;

                    }
                    else
                    {
                        SpeicherAntwort = OnlineAntwort;
                    }
                }

            }
            return SpeicherAntwort;
        }

        /// <summary>
        /// Löscht eine Aufgabe in der angegebenen Gruppe für den Benutzer.
        /// Falls der Benutzer online ist werden die Offline-Daten angegeben 
        /// und die Daten werden in der Lokalen und Online-Datebank gelöscht.
        /// </summary>
        /// <param name="gruppe">Der Name der Gruppe.</param>
        /// <param name="aufgabe">Die zu löschende Aufgabe.</param>
        /// <param name="onlineSchlüssel">Der Schlüssel, der den zugriff auf die Online-Datenbank erlaubt.</param>
        /// <param name="lokalerSchlüssel">Der Schlüssel, der den zugriff auf die Lokale-Datenbank erlaubt.</param>
        /// <returns>1 falls das Löschen erfolgreich war, oder 0 falls nicht</returns>
        public async Task<int> AufgabeLöschen(
            string gruppe, Aufgabe aufgabe, System.Guid lokalerSchlüssel,System.Guid? onlineSchlüssel = null)
        {
            int Löschen;
            if (onlineSchlüssel == null)
            {
                var antwort = await this.LokalerController.AufgabeLöschenAsync(lokalerSchlüssel, gruppe, aufgabe);
                if (antwort != 0)
                {
                    var syncAntwort = await this.SyncController.SpeichereAufgabeAsync(gruppe, lokalerSchlüssel, aufgabe, true);
                    if (syncAntwort == 0)
                    {
                        antwort = syncAntwort;
                    }
                }
                Löschen = antwort;
            }
            else
            {
                var OnlineAntwort =
                    await this.OnlineController.AufgabeLöschenAsync((System.Guid)onlineSchlüssel, gruppe, aufgabe);
                var OfflineAntwort =
                    await this.LokalerController.AufgabeLöschenAsync(lokalerSchlüssel, gruppe, aufgabe);
                if (OnlineAntwort == 2 && OfflineAntwort!=0)
                {
                    var syncAntwort =
                      await this.SyncController.SpeichereAufgabeAsync(gruppe, lokalerSchlüssel, aufgabe, true);
                    if (syncAntwort == 0)
                    {
                        OfflineAntwort= syncAntwort;
                    }
                }
                if (OfflineAntwort == OnlineAntwort)
                {
                    Löschen = OfflineAntwort;
                }
                else
                {
                    if (OfflineAntwort == 0)
                    {
                        Löschen = OfflineAntwort;
                    }
                    else
                    {
                        Löschen = OnlineAntwort;
                    }
                }
            }
            return Löschen;
        }

        #endregion Aufgaben-logik

        #region Gruppen-logik
        /// <summary>
        /// Speichert eine Gruppe für den Benutzer.
        /// Falls der Benutzer online ist werden die Offline-Daten angegeben 
        /// und die Daten werden auf die Lokale und Online-Datebank gespeichert.
        /// </summary>
        /// <param name="onlineSchlüssel">Der Schlüssel, der den zugriff auf die Online-Datenbank erlaubt.</param>
        /// <param name="lokalerSchlüssel">Der Schlüssel, der den zugriff auf die Lokale-Datenbank erlaubt.</param>
        /// <param name="gruppe">Der Name der Gruppe.</param>
        /// <returns>1 falls das Speichern erfolgreich war, oder 0 falls nicht</returns>
        public async Task<int> SpeichereGruppe(string gruppe, System.Guid lokalerSchlüssel, System.Guid? onlineSchlüssel = null)
        {
            int SpeicherAntwort;
            if (onlineSchlüssel == null)
            {
                var antwort = await this.LokalerController.SpeichereGruppeAsync(lokalerSchlüssel, gruppe);
                if (antwort != 0)
                {
                    var syncAntwort = await this.SyncController.SpeichereGruppeAsync(gruppe, lokalerSchlüssel, false);

                    if (syncAntwort == 0)
                    {
                        antwort = syncAntwort;
                    }
                }
                SpeicherAntwort = antwort;
            }
            else
            {
                var OnlineAntwort = await this.OnlineController.SpeichereGruppeAsync((System.Guid)onlineSchlüssel, gruppe);
                var OfflineAntwort = await this.LokalerController.SpeichereGruppeAsync(lokalerSchlüssel, gruppe);
                if (OnlineAntwort == 2 && OfflineAntwort!=0)
                {
                    var syncAntwort = await this.SyncController.SpeichereGruppeAsync(gruppe, lokalerSchlüssel, false);

                    if (syncAntwort == 0)
                    {
                        OfflineAntwort = syncAntwort;
                    }
                }

                if (OfflineAntwort == OnlineAntwort)
                {
                    SpeicherAntwort = OnlineAntwort;
                }
                else
                {
                    if (OfflineAntwort == 0)
                    {
                        SpeicherAntwort = OfflineAntwort;
                    }
                    else
                    {
                        SpeicherAntwort = OnlineAntwort;
                    }
                }
            }

            return SpeicherAntwort;
        }

        /// <summary>
        /// Löscht eine Gruppe für den Benutzer.
        /// Falls der Benutzer online ist werden die Offline-Daten angegeben 
        /// und die Daten werden in der Lokalen und Online-Datebank gelöscht.
        /// </summary>
        /// <param name="benutzer">Der Benutzer, der die Gruppe löscht.</param>
        /// <param name="gruppe">Der Name der Gruppe.</param>
        /// <param name="onlineSchlüssel">Der Schlüssel, der den zugriff auf die Online-Datenbank erlaubt.</param>
        /// <param name="lokalerSchlüssel">Der Schlüssel, der den zugriff auf die Lokale-Datenbank erlaubt.</param>
        /// <returns>1 falls das Löschen erfolgreich war, oder 0 falls nicht</returns>
        public async Task<int> GruppeLöschen(string gruppe, System.Guid lokalerSchlüssel, System.Guid? onlineSchlüssel = null)
        {
            int Löschen;
            if (onlineSchlüssel == null)
            {
                var antwort = await this.LokalerController.GruppeLöschenAsync(lokalerSchlüssel, gruppe);
                if (antwort != 0)
                {
                    var syncAntwort = await this.SyncController.SpeichereGruppeAsync(gruppe, lokalerSchlüssel, true);

                    if (syncAntwort == 0)
                    {
                        antwort = syncAntwort;
                    }
                }
                Löschen = antwort;
            }
            else
            {
                var OnlineAntwort = await this.OnlineController.GruppeLöschenAsync((System.Guid)onlineSchlüssel, gruppe);
                var OfflineAntwort = await this.LokalerController.GruppeLöschenAsync(lokalerSchlüssel, gruppe);

                if(OnlineAntwort==2 && OfflineAntwort != 0)
                {
                    var syncAntwort = await this.SyncController.SpeichereGruppeAsync(gruppe, lokalerSchlüssel, true);

                    if (syncAntwort == 0)
                    {
                        OfflineAntwort = syncAntwort;
                    }
                }


                if (OfflineAntwort == OnlineAntwort)
                {
                    Löschen = OfflineAntwort;
                }
                else
                {
                    if (OfflineAntwort == 0)
                    {
                        Löschen = OfflineAntwort;
                    }
                    else
                    {
                        Löschen = OnlineAntwort;
                    }
                }
            }
            return Löschen;
        }
        #endregion Gruppen-logik

    }
}
