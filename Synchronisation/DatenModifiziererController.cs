using ERP.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.UI.Models.Synchronisation
{
    /// <summary>
    /// Stellt einen Dienst zum bearbeiten von Daten die 
    /// synchronisiert werden mussen
    /// </summary>
    internal class DatenModifiziererController : LokalerController
    {
        #region Add-In Controller
        /// <summary>
        /// Initialisiert einen neuen DatenModifiziererController
        /// </summary>
        public DatenModifiziererController() : base("Demo.Controllers.DatenSqlSynchronisationsController")
        {

        }
        #endregion Add-In Controller

        #region Daten hochladen

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private System.Reflection.MethodInfo _AufgabeHochladenMethode = null!;

        /// <summary>
        /// Ruft die Beschreibung der Methode
        /// zum Hochladen einer SyncAufgabe ab
        /// </summary>
        protected System.Reflection.MethodInfo AufgabeHochladenMethode
        {
            get
            {
                if (_AufgabeHochladenMethode == null)
                {
                    _AufgabeHochladenMethode
                        = AddInControllerTyp
                            .GetMethod("SyncAufgabeHochladenAsync")!;

                    Kontext.Log.Hinzufügen(
                        $"{this} hat die Methode zum Hochladen " +
                        $"einer SyncAufgabe gefunden.");
                }

                return _AufgabeHochladenMethode;
            }
        }

        /// <summary>
        /// Speichert die SyncAufgabe für den Benutzer in der Quelle.
        /// </summary>
        /// <param name="gruppe">Die SyncGruppe, die die SyncAufgabe enthält.</param>
        /// <param name="schlüssel">Ein Einmalschlüssel (Sitzungsschlüssel), 
        /// der dem Benutzer bei der Anmeldung zugewiesen wird, 
        /// um die aktuelle Sitzung zu identifizieren.</param>
        /// <param name="löschen">Gibt an ob die Aufgabe gelöscht(true) oder gespeichert wird(false)</param>
        /// <param name="aufgabe">Die SyncAufgabe, die gespeichert werden soll.</param>
        /// <returns>
        /// 1, wenn das speichern erfolgreich war,
        /// 0, wenn der Schlüssel ungültig ist.
        /// </returns>
        public async Task<int> SpeichereAufgabeAsync(string gruppe, System.Guid schlüssel, 
            ERP.Data.Aufgabe aufgabe,bool löschen)
        {
            var Antwort = await (AufgabeHochladenMethode
              .Invoke(
                  AddInController,
                  new object[] {
                        gruppe,schlüssel,aufgabe,löschen
                  }) as Task<int>)!;
            return Antwort;
        }

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private System.Reflection.MethodInfo _GruppeHochladenMethode = null!;

        /// <summary>
        /// Ruft die Beschreibung der Methode
        /// zum Hochladen einer SyncGruppe ab
        /// </summary>
        protected System.Reflection.MethodInfo GruppeHochladenMethode
        {
            get
            {
                if (_GruppeHochladenMethode == null)
                {
                    _GruppeHochladenMethode
                        = AddInControllerTyp
                            .GetMethod("SyncGruppeHochladenAsync")!;

                    Kontext.Log.Hinzufügen(
                        $"{this} hat die Methode zum Hochladen " +
                        $"einer SyncGruppe gefunden.");
                }

                return _GruppeHochladenMethode;
            }
        }

        /// <summary>
        /// Speichert eine SyncGruppe für den Benutzer in der Quelle.
        /// </summary>
        /// <param name="gruppe">Die SyncGruppe, die die SyncAufgabe enthält.</param>
        /// <param name="schlüssel">Ein Einmalschlüssel (Sitzungsschlüssel), 
        /// der dem Benutzer bei der Anmeldung zugewiesen wird, 
        /// um die aktuelle Sitzung zu identifizieren.</param>
        /// <param name="löschen">Gibt an ob die Gruppe gelöscht(true) oder gespeichert wird(false)</param>
        /// <param name="aufgabe">Die SyncAufgabe, die gespeichert werden soll.</param>
        /// <returns>
        /// 1, wenn das speichern erfolgreich war,
        /// 0, wenn der Schlüssel ungültig ist.
        /// </returns>
        public async Task<int> SpeichereGruppeAsync(string gruppe, System.Guid schlüssel,bool löschen)
        {
            var Antwort = await (GruppeHochladenMethode
              .Invoke(
                  AddInController,
                  new object[] {
                        schlüssel,gruppe,löschen
                  }) as Task<int>)!;
            return Antwort;
        }

        #endregion Daten hochladen

       
    }
}
