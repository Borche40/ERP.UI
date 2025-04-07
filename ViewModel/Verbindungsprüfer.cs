using ERP.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.UI.ViewModel
{
    /// <summary>
    /// Statische Klasse zur Überprüfung der Internetverbindung.
    /// </summary>
    public static class Verbindungsprüfer
    {
        #region Controller

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private static ServerVerbindungController _Controller = null!;

        /// <summary>
        /// Ruft den Dienst zum prüfen ob eine Verbindung zum Server besteht ab.
        /// </summary>
        private static ServerVerbindungController Controller
        {
            get
            {
                if (Verbindungsprüfer._Controller == null)
                {
                    Verbindungsprüfer._Controller = new ServerVerbindungController();
                }
                return Verbindungsprüfer._Controller;
            }
        }
        #endregion Controller

        /// <summary>
        /// Überprüft, ob eine Verbindung mit dem Server besteht.
        /// </summary>
        /// <param name="timeoutMs">Die Zeitüberschreitung in Millisekunden für die Anfrage. 
        /// Standardwert ist 5000 sekunden.</param>
        /// <returns>Ein Task, der ein boolesches Ergebnis liefert: 
        /// true, wenn eine Internetverbindung besteht, andernfalls false.</returns>
        public static async Task<bool> PrüfeServerVerbindung(int timeoutMs = 5000)
        {
            var antwort = await Verbindungsprüfer.Controller.VerbindungÜberprüfen();

            if (antwort == 1)
            {
                ERP.UI.Properties.Settings.Default.Offline = false;
                return true;
            }
            else
            {
                ERP.UI.Properties.Settings.Default.Offline = true;
                return false;
            }
        }

        /// <summary>
        /// Stellt die Anwendung in den Online oder Offline modus
        /// </summary>
        public static async void EinstellungenEinrichten()
        {
            ERP.UI.Properties.Settings.Default.Offline =
                !await Verbindungsprüfer.PrüfeServerVerbindung();
        }
    }
}
