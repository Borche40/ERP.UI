using ERP.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.UI.Models
{
    /// <summary>
    /// Stellt einen Dienst zum Speichern und Löschen
    /// von Daten im Offline Betrieb bereit
    /// </summary>
    internal class LokalerSpeicherController:LokalerController,ISpeicherController
    {
        #region Add-In Controller
        /// <summary>
        /// Initialisiert einen neuen LokalenSpeicherController
        /// </summary>
        public LokalerSpeicherController() : base("Demo.Controllers.SchreiberSqlController")
        {

        }
        #endregion Add-In Controller

        #region Speichere Daten

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private System.Reflection.MethodInfo _SpeichereAufgabeMethode = null!;

        /// <summary>
        /// Ruft die Beschreibung der Methode
        /// zum Speichern einer Aufgabe ab
        /// </summary>
        protected System.Reflection.MethodInfo SpeichereAufgabeMethode
        {
            get
            {
                if (this._SpeichereAufgabeMethode == null)
                {
                    this._SpeichereAufgabeMethode
                        = this.AddInControllerTyp
                            .GetMethod("SpeichereAufgabeAsync")!;

                    this.Kontext.Log.Hinzufügen(
                        $"{this} hat die Methode zum Speichern " +
                        $"einer Aufgabe gefunden.");
                }

                return this._SpeichereAufgabeMethode;
            }
        }

        /// <summary>
        /// Aktualisiert die Aufgabe für den Benutzer in der Quelle.
        /// </summary>
        /// <param name="gruppe">Die Gruppe, die die Aufgabe enthält.</param>
        /// <param name="schlüssel">Ein Einmalschlüssel (Sitzungsschlüssel), 
        /// der dem Benutzer bei der Anmeldung zugewiesen wird, 
        /// um die aktuelle Sitzung zu identifizieren.</param>
        /// <param name="aufgabe">Die Aufgabe, die gespeichert werden soll.</param>
        /// <returns>
        /// 1, wenn das löschen erfolgreich war,
        /// 0, wenn der Schlüssel ungültig ist.
        /// </returns>
        public async Task<int> SpeichereAufgabeAsync(string gruppe, System.Guid schlüssel, Aufgabe aufgabe)
        {
            var Antwort = await (this.SpeichereAufgabeMethode
              .Invoke(
                  this.AddInController,
                  new object[] {
                        gruppe,schlüssel,aufgabe
                  }) as Task<int>)!;
            return Antwort;
        }


        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private System.Reflection.MethodInfo _SpeichereGruppeMethode = null!;

        /// <summary>
        /// Ruft die Beschreibung der Methode
        /// zum Speichern der Aktuellen AufgabeGruppe ab
        /// </summary>
        protected System.Reflection.MethodInfo SpeichereGruppeMethode
        {
            get
            {
                if (this._SpeichereGruppeMethode == null)
                {
                    this._SpeichereGruppeMethode
                        = this.AddInControllerTyp
                            .GetMethod("SpeichereGruppeAsync")!;

                    this.Kontext.Log.Hinzufügen(
                        $"{this} hat die Methode zum Speichern " +
                        $"einer Gruppe gefunden.");
                }

                return this._SpeichereGruppeMethode;
            }
        }

        /// <summary>
        /// Fügt eine AufgabenGruppe für den Benutzer in die Quelle.
        /// </summary>
        /// <param name="schlüssel">Ein Einmalschlüssel (Sitzungsschlüssel), 
        /// der dem Benutzer bei der Anmeldung zugewiesen wird, 
        /// um die aktuelle Sitzung zu identifizieren.</param>
        /// <param name="gruppe">Die Aufgaben-Gruppe, die gespeichert werden soll.</param>
        /// <returns>
        /// 1, wenn das löschen erfolgreich war,
        /// 0, wenn der Schlüssel ungültig ist.
        /// </returns>
        public async Task<int> SpeichereGruppeAsync(System.Guid schlüssel, string gruppe)
        {
            var Antwort = await (this.SpeichereGruppeMethode
             .Invoke(
                 this.AddInController,
                 new object[] {
                        schlüssel,gruppe
                 }) as Task<int>)!;
            return Antwort;
        }


        #endregion Speichere Daten

        #region Lösche Daten

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private System.Reflection.MethodInfo _GruppeLöschenMethode = null!;

        /// <summary>
        /// Ruft die Beschreibung der Methode
        /// zum Löschen einer Gruppe ab
        /// </summary>
        protected System.Reflection.MethodInfo GruppeLöschenMethode
        {
            get
            {
                if (this._GruppeLöschenMethode == null)
                {
                    this._GruppeLöschenMethode
                        = this.AddInControllerTyp
                            .GetMethod("GruppeLöschenAsync")!;

                    this.Kontext.Log.Hinzufügen(
                        $"{this} hat die Methode zum löschen " +
                        $"einer Gruppe gefunden.");
                }

                return this._GruppeLöschenMethode;
            }
        }

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
        public async Task<int> GruppeLöschenAsync(System.Guid schlüssel, string gruppe)
        {
            var Antwort = await(this.GruppeLöschenMethode
             .Invoke(
                 this.AddInController,
                 new object[] {
                        schlüssel,gruppe
                 }) as Task<int>)!;
            return Antwort;
        }

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private System.Reflection.MethodInfo _AufgabeLöschenMethode = null!;

        /// <summary>
        /// Ruft die Beschreibung der Methode
        /// zum Löschen einer Aufgabe ab
        /// </summary>
        protected System.Reflection.MethodInfo AufgabeLöschenMethode
        {
            get
            {
                if (this._AufgabeLöschenMethode == null)
                {
                    this._AufgabeLöschenMethode
                        = this.AddInControllerTyp
                            .GetMethod("AufgabeLöschenAsync")!;

                    this.Kontext.Log.Hinzufügen(
                        $"{this} hat die Methode zum löschen " +
                        $"einer Aufgabe gefunden.");
                }

                return this._AufgabeLöschenMethode;
            }
        }

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
        public async Task<int> AufgabeLöschenAsync(System.Guid schlüssel, string gruppe, Aufgabe aufgabe)
        {
            var Antwort = await(this.AufgabeLöschenMethode
          .Invoke(
              this.AddInController,
              new object[] {
                        schlüssel,gruppe,aufgabe
              }) as Task<int>)!;
            return Antwort;
        }
        #endregion Lösche Daten
    }
}
