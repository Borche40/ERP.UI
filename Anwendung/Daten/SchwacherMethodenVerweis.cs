using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anwendung.Daten
{
    /// <summary>
    /// Stellt eine typ-sichere Auflistung
    /// von SchwacherMethodenVerweis Objekte bereit
    /// </summary>
    public class SchwacherMethodenVerweise :
        System.Collections.Generic.List<SchwacherMethodenVerweis>
    {
        /// <summary>
        /// Führt alle hinterlegten Methoden aus,
        /// wenn der Besitzer noch existiert
        /// </summary>
        public void AlleAufrufen()
        {
            foreach (var m in this)
            {
                m?.Methode?.Invoke();
            }
        }

        /// <summary>
        /// Ruft die Anzahl der Methoden ab,
        /// wo der Besitzer nicht mehr existiert.
        /// </summary>
        public int AnzahlLeichen 
            => (from m in this 
                where m.Methode == null 
                select m).Count();
    }

    /// <summary>
    /// Ermöglicht das Kapseln einer Methode
    /// ohne die Garbage Collection an der
    /// Entfernung des Besitzers zu hindern
    /// </summary>
    public class SchwacherMethodenVerweis : System.Object
    {
        /// <summary>
        /// Internes Feld zum Hinterlegen
        /// der Methode ohne die Garbage 
        /// Collection am Entfernen des
        /// Besitzers zu hindern
        /// </summary>
        private System.WeakReference
            MethodenBeschreibung
        { get; set; } = null!;

        /// <summary>
        /// Initialisiert ein SchwacherMethodenVerweis Objekt
        /// </summary>
        /// <param name="methode">Die Speicheradresse
        /// einer Methode, wo die Garbage Collection
        /// nicht am Entfernen des Besitzers gehindert werden soll</param>
        public SchwacherMethodenVerweis(System.Action methode)
        {
            this.MethodenBeschreibung 
                = new WeakReference(methode);
        }

        /// <summary>
        /// Ruft die Methode ab, die
        /// in diesem SchwacherMethodenVerweis
        /// gekapselt ist, wenn der Besitzer
        /// noch existiert, sonst null
        /// </summary>
        public System.Action? Methode
        {
            get
            {
                return this.MethodenBeschreibung.IsAlive ?
                        this.MethodenBeschreibung.Target
                        as System.Action
                        : null;
            }
        }
    }
}
