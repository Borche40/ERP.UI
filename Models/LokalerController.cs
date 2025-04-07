using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.UI.Models
{
    /// <summary>
    /// Unterstützt sämtliche Lokale Controller
    /// mit der Add-In Assembly
    /// </summary>
    internal abstract class LokalerController : Anwendung.AppObjekt
    {

        #region Add-In Assembly und Controller

        /// <summary>
        /// Ruft den Namen des AddInControllers ab oder legt diesen fest
        /// </summary>
        protected string ControllerName { get; set; } = null!;

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private System.Reflection.Assembly _AddIn = null!;

        /// <summary>
        /// Ruft die Assembly ab, mit der
        /// die Demo Daten verwaltet werden.
        /// </summary>
        /// <remarks>Der Name der Assembly
        /// kann über die Konfigurationseinstellung
        /// OfflineAddIn gesteuert werden. Die
        /// Assembly muss sich im Verzeichnis
        /// der Hauptanwendung befinden</remarks>
        protected System.Reflection.Assembly AddIn
        {
            get
            {
                if (this._AddIn == null)
                {
                    var AddInPfad = System.IO.Path.Combine(
                        Anwendung.AppObjekt.AppPfad, // Use 'Anwendung.AppObjekt' to access the static property
                        Properties.Settings.Default.OfflineAddIn);
                    this._AddIn = System.Reflection
                        .Assembly.LoadFile(AddInPfad);

                    this.Kontext.Log
                        .Hinzufügen(
                        $"{this} hat die AddIn " +
                        $"Assembly \"{this._AddIn}\" geladen.");
                }

                return this._AddIn;
            }
        }

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private System.Type _AddInControllerTyp = null!;

        /// <summary>
        /// Ruft die Klasse ab, mit der
        /// die Demodaten aus der Datenbank
        /// geholt werden
        /// </summary>
        /// <remarks>Diese Klasse muss DatenSqlController
        /// heißen, sich im Namespace Demo.Controller
        /// in der AddIn Assembly befinden</remarks>
        protected System.Type AddInControllerTyp
        {
            get
            {
                if (this._AddInControllerTyp == null)
                {
                    this._AddInControllerTyp
                        = this.AddIn.GetType(this.ControllerName)!;

                    this.Kontext.Log.Hinzufügen(
                        $"{this} hat die Klasse \"{this._AddInControllerTyp}\" " +
                        $"zum Arbeiten mit der Datenbank geladen.");
                }

                return this._AddInControllerTyp;
            }
        }

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private object? _AddInController = null;

        /// <summary>
        /// Ruft das Objekt ab, mit dem
        /// die Demodaten geholt werden
        /// </summary>
        protected object? AddInController
        {
            get
            {
                if (this._AddInController == null)
                {
                    this._AddInController
                        = this.Kontext.Produziere(
                            this.AddInControllerTyp);
                }
                return this._AddInController;
            }
        }
        #endregion Add-In Assembly und Controller

        /// <summary>
        /// Initialisiert einen neuen Lokalen Controller
        /// </summary>
        /// <param name="controllerName">Der Name des AddInControllers der benutzt werden soll</param>
        public LokalerController(string controllerName)
        {
            this.ControllerName = controllerName;
        }


    }
}
