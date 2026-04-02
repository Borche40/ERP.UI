using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anwendung.Daten
{
    /// <summary>
    /// Unterscheidet Protokolleinträge
    /// </summary>
    public enum ProtokolleintragTyp
    {
        /// <summary>
        /// Kennzeichnet einen allgemeinen Hinweis
        /// </summary>
        Normal,
        /// <summary>
        /// Kennzeichnet einen Eintrag,
        /// dem Beachtung geschenkt werden soll
        /// </summary>
        Warnung,
        /// <summary>
        /// Kennzeichnet einen Eintrag,
        /// der auf eine Ausnahme hinweist
        /// </summary>
        Fehler
    }

    /// <summary>
    /// Stellt die Daten für eine
    /// Protokollzeile bereit
    /// </summary>
    public class Protokolleintrag : DatenObjekt
    {
        /// <summary>
        /// Ruft den Zeitstempel dieses
        /// Eintrags ab oder legt diesen fest
        /// </summary>
        public System.DateTime Zeitpunkt { get; set; }
            = System.DateTime.Now;

        /// <summary>
        /// Ruft die Information des
        /// Eintrags ab oder legt diese fest
        /// </summary>
        [InToString(position: 2)]
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Ruft die Art des Eintrags ab
        /// oder legt diese fest
        /// </summary>
        [InToString(position: 1)]
        public ProtokolleintragTyp Typ { get; set; }
            = ProtokolleintragTyp.Normal;

    }

    /// <summary>
    /// Stellt eine Auflistung von
    /// Protokolleintrag Objekten bereit
    /// </summary>
    public class Protokolleinträge
    //20240213 Hr. Schatzl - für die WPF ungeeignet
    //    : System.Collections.Generic.List<Protokolleintrag>
          : System.Collections.ObjectModel
            .ObservableCollection<Protokolleintrag>
    {

    }
}
