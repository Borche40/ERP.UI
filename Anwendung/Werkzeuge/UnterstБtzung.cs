using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anwendung.Werkzeuge
{
    /// <summary>
    /// Stellt Hilfserweiterungsmethoden bereit
    /// </summary>
    public static class Unterstützung : System.Object
    {
        /// <summary>
        /// Gibt den Text zurück, der um
        /// einen eventuell vorhandenen 
        /// Sprachordner ergänzt wurde
        /// </summary>
        /// <param name="basis">Die Pfadangabe,
        /// wo geprüft werden soll, ob
        /// ein lokalisierter Unterordner 
        /// für die aktuelle Kultur existiert</param>
        public static string HoleLokalisierung(this string basis)
        {
            // Die aktuelle Kultur ermitteln
            var Kultur = System.Globalization.CultureInfo
                .CurrentUICulture.Name;

            // Prüfen, ob in der Basis ein Ordner
            // mit dieser Kultur existiert
            while (!System.IO.Directory.Exists(
                        System.IO.Path.Combine(
                                            basis,
                                            Kultur))
                    && Kultur.Length > 0)
            {
                // Wenn nicht, die Subkultur entfernen
                // und wieder prüfen
                var LetzterBindestrich = Kultur.LastIndexOf('-');
                if (LetzterBindestrich == -1)
                {
                    //Es wurde alles geprüft
                    Kultur = string.Empty;
                }
                else
                {
                    Kultur = Kultur.Substring(
                                        0, 
                                        LetzterBindestrich);
                }
            }

            // Sollten alle Kulturen geprüft worden sein,
            // das Original zurückgeben
            return System.IO.Path.Combine(basis, Kultur);
        }
    }
}
