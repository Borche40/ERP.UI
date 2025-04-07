using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.UI.Models
{
    /// <summary>
    /// Stellt Methoden bereit, die ein Controller Dienst 
    /// bereitstellen muss, damit dieser vom VerbindumgManager benutzt werden kann.
    /// </summary>
    internal interface IVerbindungController
    {

        /// <summary>
        /// Stellt eine Verbindung mit dem Nachrichten-Dienst für den Benutzer bereit
        /// </summary>
        /// <param name="email">Die Email-adresse des Bebutzers</param>
        /// <param name="aufgaben">Der Handler für Ereignisse, die Aufgaben betreffen.</param>
        /// <param name="gruppen">Der Handler für Ereignisse, die Gruppen betreffen.</param>
        void VerbindungHerstellen(string email, AufgabenEventHandler aufgaben, GruppenEventHandler gruppen);

        /// <summary>
        /// Trennt die Verbindung mit dem Nachrichten-Dienst
        /// </summary>
        /// <param name="email">Die Email-adresse des Bebutzers</param>
        void VerbindungAbmelden(string email);

        /// <summary>
        /// Meldet dem Nachrichten-Dienst, welche Gruppe aktualisiert werden muss
        /// <param name="gruppe">Die AufgabenGruppe, die aktualisiert werden soll.</param>
        /// <param name="email">Die E-Mail-Adresse des Benutzers.</param>
        void AufgabenAktualisieren(string gruppe, string email);

        /// <summary>
        /// Meldet dem Nachrichten-Dienst, das die Gruppen aktualisiert werden muss
        /// </summary>
        /// <param name="email">Die E-Mail-Adresse des Benutzers.</param>
        void GruppenAktualisieren(string email);


    }
}
