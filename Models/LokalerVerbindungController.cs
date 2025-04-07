using ERP.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ERP.UI.Models
{
    /// <summary>
    /// Stellt einen lokalen Verbindungskontroller bereit, der die Daten der Anwendung
    /// synchronisiert
    /// </summary>
    internal class LokalerVerbindungController :Anwendung.AppObjekt, IVerbindungController
    {
        #region Aufgabendaten

        /// <summary>
        /// Aktualisiert die Aufgaben für eine bestimmte Gruppe.
        /// </summary>
        /// <param name="gruppe">Die Aufgaben-Gruppe.</param>
        /// <param name="email">Die E-Mail-Adresse des Benutzers.</param>
        public void AufgabenAktualisieren(string gruppe, string email)
        {
            OfflineHub.AufgabenAktualisieren(gruppe, email);
        }

        #endregion Aufgabendaten

        #region Gruppendaten

        /// <summary>
        /// Aktualisiert die Gruppeninformationen für einen Benutzer.
        /// </summary>
        /// <param name="email">Die E-Mail-Adresse des Benutzers.</param>
        public void GruppenAktualisieren(string email)
        {
            OfflineHub.GruppenAktualisieren(email);
        }

        #endregion Gruppendaten

        #region Verbindungsmanagement

        /// <summary>
        /// Meldet die Verbindung eines Benutzers ab.
        /// </summary>
        /// <param name="email">Die E-Mail-Adresse des Benutzers.</param>
        public void VerbindungAbmelden(string email)
        {
            OfflineHub.Verlassen(email);
        }

        /// <summary>
        /// Stellt die Verbindung für einen Benutzer her und registriert
        /// Ereignishandler für Aufgaben- und Gruppenaktualisierungen.
        /// </summary>
        /// <param name="email">Die E-Mail-Adresse des Benutzers.</param>
        /// <param name="aufgaben">Der Ereignishandler für Aufgabenaktualisierungen.</param>
        /// <param name="gruppen">Der Ereignishandler für Gruppenaktualisierungen.</param>
        public void VerbindungHerstellen(string email,
            AufgabenEventHandler aufgaben, GruppenEventHandler gruppen)
        {
            OfflineHub.AufgabenBearbeitet += (sender, e) =>
            {
                aufgaben.Invoke(e.Gruppe, e.Email);
            };

            OfflineHub.GruppenBearbeitet += (sender, e) =>
            {
                gruppen.Invoke(e.Email);
            };

            OfflineHub.SitzungBeitreten(email);
        }

        #endregion Verbindungsmanagement
    }
}
