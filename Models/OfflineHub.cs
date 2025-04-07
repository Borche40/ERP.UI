using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.UI.Models
{
    /// <summary>
    /// Stellt einen Dienst zum senden von Daten zwischen lokalen Anwendungen.
    /// </summary>
    public static class OfflineHub
    {
        #region Benutzerliste

        /// <summary>
        /// Ruft die Liste der Aktuellen Benutzers der Anwendung ab, oder legt diese fest
        /// </summary>
        private static System.Collections.Generic.List<string> Daten { get; set; } =new List<string>();

        #endregion Benutzerliste

        #region Sitzung-logik
        /// <summary>
        /// Tritt einer Sitzung bei.
        /// </summary>
        /// <param name="email">Die Email-adresse des Benutzers</param>
        public static Task SitzungBeitreten(string email)
        {
            OfflineHub.Daten.Add(email);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Verlässt eine Gruppe.
        /// </summary>
        /// <param name="email">Die Email-adresse des Benutzers</param>
        internal static Task Verlassen(string email)
        {
            OfflineHub.Daten.Remove(email);
            return Task.CompletedTask;
        }

        #endregion Sitzung-logik

        #region Aufgabenmeldung

        /// <summary>
        /// Aktualisiert die Aufgaben für eine bestimmte Gruppe.
        /// </summary>
        /// <param name="gruppe">Die Aufgaben-Gruppe.</param>
        /// <param name="email">Die E-Mail-Adresse des Benutzers.</param>
        internal static Task AufgabenAktualisieren(string gruppe, string email)
        {
            foreach(var daten in OfflineHub.Daten)
            {
                if (0 == string.Compare(email,daten))
                {
                    OfflineHub.OnAufgabenBearbeitet(new OfflineEventArgs(email, gruppe));
                }
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Wird ausgelöst, wenn die Aufgaben bearbeitet wurden.
        /// </summary>
        public static event EventHandler<OfflineEventArgs> AufgabenBearbeitet = null!;

        /// <summary>
        /// Löst das Ereignis "AufgabenBearbeitet" aus.
        /// </summary>
        /// <param name="e">Ereignisdaten</param>
        public static void OnAufgabenBearbeitet(OfflineEventArgs e)
        {
            OfflineHub.AufgabenBearbeitet?.Invoke(new object(), e);
        }
        #endregion Aufgabenmeldung

        #region Gruppenmeldung
        /// <summary>
        /// Wird ausgelöst, wenn die Gruppen bearbeitet wurden.
        /// </summary>
        public static event EventHandler<OfflineEventArgs> GruppenBearbeitet = null!;

        /// <summary>
        /// Löst das Ereignis "GruppenBearbeitet" aus.
        /// </summary>
        /// <param name="e">Ereignisdaten</param>
        public static void OnGruppenBearbeitet(OfflineEventArgs e)
        {
            OfflineHub.GruppenBearbeitet?.Invoke(new object(), e);
        }

        /// <summary>
        /// Aktualisiert die Gruppeninformationen für einen Benutzer.
        /// </summary>
        /// <param name="email">Die E-Mail-Adresse des Benutzers.</param>
        public static Task GruppenAktualisieren(string email)
        {
            foreach(var daten in OfflineHub.Daten)
            {
                if (0 == string.Compare(email, email))
                {
                    OfflineHub.OnGruppenBearbeitet(new OfflineEventArgs(email));
                }
            }
            return Task.CompletedTask;
        }
        #endregion Gruppenmeldung

    }
}
