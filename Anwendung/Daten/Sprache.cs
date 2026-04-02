using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anwendung.Daten
{
    /// <summary>
    /// Stellt eine Auflistung von
    /// Anwendungssprachen bereit
    /// </summary>
    public class Sprachen : System.Collections.Generic.List<Sprache>
    {

    }

    /// <summary>
    /// Beschreibt eine Anwendungssprache
    /// </summary>
    public class Sprache : System.Object
    {
        /// <summary>
        /// Ruft das 2stellige Sprachkürzel
        /// der System.Globalization.CultureInfo
        /// ab oder legt dieses fest
        /// </summary>
        public string Code {  get; set; } = string.Empty;

        /// <summary>
        /// Ruft die lesbare Bezeichnung
        /// der Sprache ab oder legt diese fest
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Ruft einen Text mit
        /// der Beschreibung dieser Sprache ab
        /// </summary>
        public override string ToString()
        {
            return 
                $"{this.GetType().Name}(" +
                $"Code=\"{this.Code}\", " +
                $"Name=\"{this.Name}\")";
        }
    }
}
