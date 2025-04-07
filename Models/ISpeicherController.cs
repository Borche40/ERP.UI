using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.UI.Models
{
    /// <summary>
    /// Stellt Methoden bereit, die ein Controller Dienst 
    /// bereitstellen muss, damit dieser vom SpeicherManager benutzt werden kann.
    /// </summary>
    interface ISpeicherController
    {
        /// <summary>
        /// Aktualisiert die Aufgabe für den Benutzer in der Quelle.
        /// </summary>
        /// <param name="gruppe">Die Gruppe, die die Aufgabe enthält.</param>
        /// <param name="schlüssel">Ein Einmalschlüssel (Sitzungsschlüssel), 
        /// der dem Benutzer bei der Anmeldung zugewiesen wird, 
        /// um die aktuelle Sitzung zu identifizieren.</param>
        /// <param name="aufgabe">Die Aufgabe, die gespeichert werden soll.</param>
        /// <returns>
        /// 1, wenn das Speichern erfolgreich war,
        /// 0, wenn der Schlüssel ungültig ist.
        /// </returns>
        System.Threading.Tasks.Task<int>
            SpeichereAufgabeAsync(string gruppe, System.Guid schlüssel, ERP.Data.Aufgabe aufgabe);

        /// <summary>
        /// Fügt eine AufgabenGruppe für den Benutzer in die Quelle.
        /// </summary>
        /// <param name="schlüssel">Ein Einmalschlüssel (Sitzungsschlüssel), 
        /// der dem Benutzer bei der Anmeldung zugewiesen wird, 
        /// um die aktuelle Sitzung zu identifizieren.</param>
        /// <param name="gruppe">Die Aufgaben-Gruppe, die gespeichert werden soll.</param>
        /// <returns>
        /// 1, wenn das Speichern erfolgreich war,
        /// 0, wenn der Schlüssel ungültig ist.
        /// </returns>
        System.Threading.Tasks.Task<int> SpeichereGruppeAsync(System.Guid schlüssel, string gruppe);

        /// <summary>
        /// Löscht eine AufgabenGruppe von dem angegebenen Benutzer.
        /// </summary>
        /// <param name="gruppe">Die Gruppe, die gelöscht werden soll.</param>
        /// <param name="schlüssel">Ein Einmalschlüssel (Sitzungsschlüssel), 
        /// der dem Benutzer bei der Anmeldung zugewiesen wird, 
        /// um die aktuelle Sitzung zu identifizieren.</param>
        /// <returns>
        /// 1, wenn das löschen erfolgreich war,
        /// 0, wenn der Schlüssel ungültig ist.
        /// </returns>
        System.Threading.Tasks.Task<int> GruppeLöschenAsync(System.Guid schlüssel, string gruppe);

        /// <summary>
        /// Löscht eine Aufgabe
        /// von dem angegebenen Benutzer.
        /// </summary>
        /// <param name="gruppe">Die Gruppe, die die Aufgabe enthält..</param>
        /// <param name="schlüssel">Ein Einmalschlüssel (Sitzungsschlüssel), 
        /// der dem Benutzer bei der Anmeldung zugewiesen wird, 
        /// um die aktuelle Sitzung zu identifizieren.</param>     
        /// <param name="aufgabe">Die Aufgabe, die gelöscht werden soll.</param>
        /// <returns>
        /// 1, wenn das löschen erfolgreich war,
        /// 0, wenn der Schlüssel ungültig ist.
        /// </returns>
        System.Threading.Tasks.Task<int> AufgabeLöschenAsync(System.Guid schlüssel,
            string gruppe, ERP.Data.Aufgabe aufgabe);

    }
}
