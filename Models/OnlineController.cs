

namespace ERP.UI.Models
{
    /// <summary>
    /// Unterstützt sämtliche Online Controller mit einer 
    /// Basisadresse und Optionen für den REST Webdienstes.
    /// </summary>
    internal abstract class OnlineController:Anwendung.AppObjekt
    {
        #region Basisadresse und Optionen für den REST Webdienstes

        /// <summary>
        /// Ruft das Objekt zum Steuern
        /// des Mappings für die Json Daten ab.
        /// </summary>
        protected System.Text.Json.JsonSerializerOptions
            JsonOptionen => new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

        /// <summary>
        /// Zum Cachen der Eigenschaft
        /// </summary>
        private static string _BasisUrl = null!;

        /// <summary>
        /// Ruft die Adresse des Webdienstes ab,
        /// der die Daten verwaltet
        /// </summary>
        /// <remarks>Der abschließende Schrägstrich
        /// ist sichergestellt. Die Adresse kann
        /// mit OnlineRestBasis konfigurieriert werden</remarks>
        protected string BasisUrl
        {
            get
            {
                if (OnlineController._BasisUrl == null)
                {
                    OnlineController._BasisUrl
                        = ERP.UI.Properties.Settings.Default.OnlineRestApiBasis;

                    if (!OnlineController._BasisUrl.EndsWith('/'))
                    {
                        OnlineController._BasisUrl += '/';
                    }
                }

                return OnlineController._BasisUrl;
            }
        }

        #endregion Basisadresse und Optionen für den REST Webdienstes

        /// <summary>
        /// Ereignis, das ausgelöst wird, wenn keine Verbindung zum Server hergestellt werden kann.
        /// </summary>
        public event EventHandler<EventArgs>? VerbindungFehlgeschlagen = null!;

        /// <summary>
        /// Löst das Ereignis aus, das signalisiert, dass keine Verbindung zum Server hergestellt werden kann.
        /// </summary>
        protected virtual void OnVerbindungFehlgeschlagen(EventArgs e)
        {
            var handlerKopie = VerbindungFehlgeschlagen;
            handlerKopie?.Invoke(this, e);
        }

    }
}
