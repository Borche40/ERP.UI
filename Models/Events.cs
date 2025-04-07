using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.UI.Models
{
    #region Speicher-Event

    /// <summary>
    /// Stellt Ereignisdaten für das SpeichernAbgeschlossen-Ereignis bereit.
    /// </summary>
    public class SpeichernEventArgs : EventArgs
    {
        /// <summary>
        /// Gibt den Status der Speichernaktion an.
        /// Mögliche Werte sind: 
        /// 0 für "0" also Schlüssel ist nich gültig, 
        /// 1 für "1", Speichern erfolgreich
        /// </summary>
        public int Status { get; }

        /// <summary>
        /// Initialisiert eine neue Instanz der SpeichernEventArgs-Klasse.
        /// </summary>
        /// <param name="status">Der Status der Speichernaktion.</param>
        public SpeichernEventArgs(int status)
        {
            this.Status = status;
        }
    }

    #endregion Speicher-Event

    #region Authentifizierung-Event

    /// <summary>
    /// Stellt Daten für das Ereignis bereit, das ausgelöst wird, 
    /// wenn eine Authentifizierungsaktion abgeschlossen ist.
    /// </summary>
    public class AuthentifizierungEventArgs : EventArgs
    {
        /// <summary>
        /// Ruft einen Wert ab, der angibt, 
        /// ob die Authentifizierung erfolgreich war.
        /// </summary>
        public bool Erfolgreich { get; }

        /// <summary>
        /// Initialisiert eine neue Instanz der AuthentifizierungEventArgs-Klasse 
        /// mit einem Wert, der angibt, ob die Authentifizierung erfolgreich war.
        /// </summary>
        /// <param name="erfolgreich">True falls 
        /// die Authentifizierung erfolgreich war, false falls nicht</param>
        public AuthentifizierungEventArgs(bool erfolgreich)
        {
           this.Erfolgreich = erfolgreich;
        }
    }


    #endregion Authentifizierung-Event

    #region Hub-Event

    /// <summary>
    /// Stellt Daten für das Ereignis bereit, das ausgelöst wird, 
    /// wenn eine Gruppenaktualisierung abgeschlossen ist.
    /// </summary>
    public class HubEventArgs : EventArgs
    {
        /// <summary>
        /// Ruft die Gruppe ab, die aktualisiert werden soll.
        /// </summary>
        public string Gruppe { get; }

        /// <summary>
        /// Initialisiert eine neue Instanz der HubEventArgs-Klasse 
        /// mit einem Wert, der angibt, welche Gruppe aktualisiert werden soll. 
        /// </summary>
        /// <param name="gruppe">Die Gruppe, die aktualisiert werden soll.</param>
        public HubEventArgs(string gruppe)
        {
            this.Gruppe= gruppe;
        }
    }

    #endregion Hub-Event

    #region Offline Hub-Event

    /// <summary>
    /// Stellt Daten für das Ereignis bereit, das ausgelöst wird, 
    /// wenn eine Gruppenaktualisierung abgeschlossen ist.
    /// </summary>
    public class OfflineEventArgs : EventArgs
    {
        /// <summary>
        /// Ruft die Email-adresse der Gruppe ab.
        /// </summary>
        public string Email { get; }

        /// <summary>
        /// Ruft die Gruppe von der die Aufgaben bearbeitet wurden ab.
        /// </summary>
        internal string Gruppe { get; }

        /// <summary>
        /// Initialisiert eine neue Instanz der OfflineEventArgs-Klasse 
        /// mit einem Wert, der angibt, welche Gruppe aktualisiert werden soll. 
        /// </summary>        
        /// /// <param name="email">Die Email-adresse des Benutzers</param>
        /// <param name="gruppe">Die Gruppe für die die Aufgaben geholt werden sollen, falls null war keine Gruppe bearbeitet.</param>
        internal OfflineEventArgs( string email,string gruppe=null!)
        { 
            this.Email = email;
            this.Gruppe=gruppe;
        }
    }

    #endregion Offline Hub-Event

    #region Online-Event
    /// <summary>
    /// Stellt Ereignisargumente für den Online-Status dar.
    /// </summary>
    public class OnlineEventArgs : EventArgs
    {
        /// <summary>
        /// Ruft einen Wert ab, der angibt, ob die Verbindung online ist.
        /// </summary>
        public bool IstOnline { get; }

        /// <summary>
        /// Initialisiert eine neue Instanz der <see cref="OnlineEventArgs"/>-Klasse mit dem angegebenen Online-Status.
        /// </summary>
        /// <param name="istOnline">True, wenn die Verbindung online ist, andernfalls False.</param>
        public OnlineEventArgs(bool istOnline)
        {
            this.IstOnline = istOnline;
        }
    }
    #endregion Online-Event

    #region HoleAufgaben-Event

    /// <summary>
    /// Stellt Daten für das Ereignis bereit, das ausgelöst wird, 
    /// wenn eine Gruppenaktualisierung abgeschlossen ist.
    /// </summary>
    public class AufgabenEventArgs : EventArgs
    { 

        /// <summary>
        /// Ruft die Gruppe von der die Aufgaben bearbeitet wurden ab.
        /// </summary>
        internal string Gruppe { get; }

        /// <summary>
        /// Initialisiert eine neue Instanz der AufgabenEventArgs-Klasse 
        /// mit einem Wert, der angibt, welche Gruppe aktualisiert werden soll. 
        /// </summary>        
        /// <param name="gruppe">Die Gruppe für die die Aufgaben geholt werden sollen.</param>
        internal AufgabenEventArgs(string gruppe)
        {
            this.Gruppe = gruppe;
        }
    }

    #endregion HoleAufgaben-Event

}
