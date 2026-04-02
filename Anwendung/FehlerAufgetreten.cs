using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anwendung
{
    // Erster Schritt bei Ereignissen
    // Die Ereignisdatenklasse
    // (ohne Daten würde man System.EventArgs verwenden)

    /// <summary>
    /// Stellt die Daten für das
    /// FehlerAufgetreten Ereignis bereit
    /// </summary>
    public class FehlerAufgetretenEventArgs 
        : System.EventArgs
    {
        /// <summary>
        /// Ruft das Ausnahmeobjekt des Fehlers ab
        /// </summary>
        public System.Exception Ursache { get; private set; }

        /// <summary>
        /// Initialisiert ein neues 
        /// FehlerAufgetretenEventArgs Objekt
        /// </summary>
        /// <param name="ursache">Ausnahme Objekt
        /// mit der Beschreibung des Fehlers</param>
        public FehlerAufgetretenEventArgs(
                    System.Exception ursache)
        {
            this.Ursache = ursache;
        }
    }

    //Zweiter Schritt bei Ereignissen mit Daten
    //Die Signatur der erlaubten Methoden,
    //der so genannten Ereignis-Behandler deklarieren

    /// <summary>
    /// Stellt die Methode dar, die das
    /// FehlerAufgetreten Ereignis behandeln
    /// </summary>
    /// <param name="sender">Verweis auf den 
    /// Aufrufer der Methode</param>
    /// <param name="e">Ereignisdaten</param>
    public delegate void FehlerAufgetretenEventHandler(
                            object sender, 
                            FehlerAufgetretenEventArgs e);
}
