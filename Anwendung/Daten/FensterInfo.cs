using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anwendung.Daten
{
    /// <summary>
    /// Stellt eine Auflistung von
    /// FensterInfo Objekten bereit
    /// </summary>
    public class FensterInfos 
        : System.Collections.Generic.List<FensterInfo>
    {

    }

    /// <summary>
    /// Stellt Information über ein
    /// Anwendungsfenster bereit, z. B. den
    /// Zustand, die Größe und Position
    /// </summary>
    /// <remarks>Die Klasse soll sowohl
    /// für Windows Forms als auch WPF
    /// benutzt werden können. Deshalb sind
    /// die Größenangaben in Integer (Forms) 
    /// und nicht Double (WPF)</remarks>
    public class FensterInfo : System.Object
    {
        /// <summary>
        /// Ruft die interne Bezeichnung
        /// das Fensters ab oder legt
        /// diese fest.
        /// </summary>
        /// <remarks>Dieser Wert wird als
        /// Schlüssel zum Wiederfinden benutzt</remarks>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Ruft den Fensterzustand Normal,
        /// Minimiert bzw. Maximiert ab
        /// oder legt diesen fest
        /// </summary>
        /// <remarks>Weil Windows Forms
        /// eine andere Auflistung als WPF
        /// benutzt, keine Enumeration sondern
        /// eine Standard Ganzzahl</remarks>
        public int Zustand { get;set; }

        /// <summary>
        /// Ruft die linke Position ab
        /// oder legt diese fest
        /// </summary>
        public int? Links { get; set; } = null!;

        /// <summary>
        /// Ruft die obere Position ab
        /// oder legt diese fest
        /// </summary>
        public int? Oben { get; set; } = null!;

        /// <summary>
        /// Ruft die Breite des Fensters ab
        /// oder legt diese fest
        /// </summary>
        public int? Breite { get; set; } = null!;

        /// <summary>
        /// Ruft die Höhe des Fensters ab
        /// oder legt diese fest
        /// </summary>
        public int? Höhe { get; set; } = null!;

        /// <summary>
        /// Gibt einen Text zurück,
        /// mit dem diese FensterInfo 
        /// beschrieben wird
        /// </summary>
        public override string ToString()
        {
            return $"{this.GetType().Name}(Name=\"{this.Name}\")"; 
        }
    }
}
