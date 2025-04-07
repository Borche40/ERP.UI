using ERP.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.UI.Models.Synchronisation
{
    /// <summary>
    /// Stellt einen Dienst zum abrufen von Daten die 
    /// synchronisiert werden mussen
    /// </summary>
    internal class DatenAbrufController:Models.LokalerController
    {
        #region Add-In Controller
        /// <summary>
        /// Initialisiert einen neuen DatenAbrufController
        /// </summary>
        public DatenAbrufController() : base("Demo.Controllers.DatenSqlSynchronisationsController")
        {

        }
        #endregion Add-In Controller

        #region Datenabfrage

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private System.Reflection.MethodInfo _HoleSyncAufgabenMethode = null!;

        /// <summary>
        /// Ruft die Beschreibung der Methode
        /// zum Holen der SyncAufgaben ab
        /// </summary>
        protected System.Reflection.MethodInfo HoleSyncAufgabenMethode
        {
            get
            {
                if (this._HoleSyncAufgabenMethode == null)
                {
                    this._HoleSyncAufgabenMethode
                        = this.AddInControllerTyp
                            .GetMethod("HoleSyncAufgabenAsync")!;

                    this.Kontext.Log.Hinzufügen(
                        $"{this} hat die Methode zum Abrufen " +
                        $"der SyncAufgaben gefunden.");
                }

                return this._HoleSyncAufgabenMethode;
            }
        }

        /// <summary>
        /// Gibt die SyncAufgaben
        /// mit Hilfe eines Add-Ins zurück
        /// </summary>
        /// <param name="schlüssel">Ein Einmalschlüssel (Sitzungsschlüssel), 
        /// der dem Benutzer bei der Anmeldung zugewiesen wird, 
        /// um die aktuelle Sitzung zu identifizieren.</param>
        /// <returns>Die Aufgaben des Benutzers .</returns>
        public async Task<ERP.Data.Sync.SyncAufgaben> HoleAufgabenAsync(System.Guid schlüssel)
        {
            var Aufgaben = await (this.HoleSyncAufgabenMethode
               .Invoke(
                   this.AddInController,
                   new object[] {
                        schlüssel
                   }) as Task<ERP.Data.Sync.SyncAufgaben>)!;

            return Aufgaben;
        }

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private System.Reflection.MethodInfo _HoleSyncGruppenMethode = null!;

        /// <summary>
        /// Ruft die Beschreibung der Methode
        /// zum Holen der SyncGruppen ab
        /// </summary>
        protected System.Reflection.MethodInfo HoleSyncGruppenMethode
        {
            get
            {
                if (this._HoleSyncGruppenMethode == null)
                {
                    this._HoleSyncGruppenMethode
                        = this.AddInControllerTyp
                            .GetMethod("HoleSyncGruppenAsync")!;

                    this.Kontext.Log.Hinzufügen(
                        $"{this} hat die Methode zum Abrufen " +
                        $"der SyncGruppen gefunden.");
                }

                return this._HoleSyncGruppenMethode;
            }
        }

        /// <summary>
        /// Gibt die SyncGruppen
        /// mit Hilfe eines Add-Ins zurück
        /// </summary>
        /// <param name="schlüssel">Ein Einmalschlüssel (Sitzungsschlüssel), 
        /// der dem Benutzer bei der Anmeldung zugewiesen wird, 
        /// um die aktuelle Sitzung zu identifizieren.</param>
        /// <returns>Die SyncGruppen des Benutzers .</returns>
        public async Task<ERP.Data.Sync.SyncGruppen> HoleGruppenAsync(System.Guid schlüssel)
        {
            var Gruppen = await (this.HoleSyncGruppenMethode
               .Invoke(
                   this.AddInController,
                   new object[] {
                        schlüssel
                   }) as Task<ERP.Data.Sync.SyncGruppen>)!;

            return Gruppen;
        }


        #endregion Datenabfrage

        #region Daten löschen

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private System.Reflection.MethodInfo _DatenLöschenMethode = null!;

        /// <summary>
        /// Ruft die Beschreibung der Methode
        /// zum löschen der sync Daten ab
        /// </summary>
        protected System.Reflection.MethodInfo DatenLöschenMethode
        {
            get
            {
                if (_DatenLöschenMethode == null)
                {
                    _DatenLöschenMethode
                        = AddInControllerTyp
                            .GetMethod("SyncDatenLöschenAsync")!;

                    Kontext.Log.Hinzufügen(
                        $"{this} hat die Methode zum Hochladen " +
                        $"einer SyncAufgabe gefunden.");
                }

                return _DatenLöschenMethode;
            }
        }

        /// <summary>
        /// Löscht die SyncDaten für den Benutzer in der Quelle.
        /// </summary>
        /// <param name="schlüssel">Ein Einmalschlüssel (Sitzungsschlüssel), 
        /// der dem Benutzer bei der Anmeldung zugewiesen wird, 
        /// um die aktuelle Sitzung zu identifizieren.</param>
        /// <returns>
        /// 1, wenn das speichern erfolgreich war,
        /// 0, wenn der Schlüssel ungültig ist.
        /// </returns>
        public async Task<int> DatenLöschenAsync(System.Guid schlüssel)
        {
            var Antwort = await (DatenLöschenMethode
              .Invoke(
                  AddInController,
                  new object[] {
                        schlüssel
                  }) as Task<int>)!;
            return Antwort;
        }

        #endregion Daten löschen
    }
}
