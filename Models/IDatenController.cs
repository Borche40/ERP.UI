using ERP.Data;
using ERP.UI.ViewModel;
namespace ERP.UI.Models
{
    /// <summary>
    /// Stellt Methoden bereit, die ein Controller Dienst 
    /// bereitstellen muss, damit dieser vom DatenManager benutzt werden kann.
    /// </summary>
    internal interface IDatenController
    {
        /// <summary>
        /// Gibt die Aufgaben für den Benutzer aus der Quelle zurück.
        /// </summary>
        /// <param name="benutzer">Das Benutzerobjekt zur Authentifizierung.</param>
        /// <param name="gruppe">Die Gruppe, für die die Aufgaben geholt werden sollen.</param>
        /// <returns>Die Aufgaben der aktuellen AufgabenGruppe des Benutzers .</returns>
        System.Threading.Tasks.Task<ERP.Data.Aufgaben?>
            HoleAufgabenAsync(ERP.Data.Models.Benutzer benutzer, string gruppe);

        /// <summary>
        /// Gibt die AufgabenGruppen für den Benutzer aus der Quelle zurück.
        /// </summary>
        /// <param name="benutzer">Das Benutzerobjekt zur Authentifizierung.</param>
        System.Threading.Tasks.Task<ERP.Data.AufgabenGruppen?>
            HoleAufgabenGruppenAsync(ERP.Data.Models.Benutzer benutzer);

        /// <summary>
        /// Meldet den Benutzer mit der angegebenen E-Mail-Adresse und dem Passwort an.
        /// </summary>
        /// <param name="anmeldung">Die Daten die für eine Anmeldung benötigt sind.</param>
        /// <returns>Das Benutzerobjekt,
        /// falls die Anmeldung erfolgreich war, andernfalls null.</returns>
        System.Threading.Tasks.Task<ERP.Data.Models.Benutzer?> Anmelden(ERP.Data.Anmeldung anmeldung);


        /// <summary>
        /// Registriert einen neuen Benutzer mit den angegebenen Daten.
        /// </summary>
        /// <param name="registrierung">Das Objekt mit den Registrierungsdaten.</param>
        /// <returns>Die Ergebniscode der Registrierung, 
        /// 1 wenn die Registrierung erfolgreich ist oder 0 falls nicht.</returns>
        /// <remarks>der Ergebniscode kann bei einem Online-Controller 2 sein, falls keine Verbindung beschtet</remarks>
        System.Threading.Tasks.Task<int> Registrieren(ERP.Data.Registrierung registrierung);

    }
}
