

// Zum Aktivieren der
//  Erweiterungen...
using Anwendung.Werkzeuge;

namespace Anwendung
{
    /// <summary>
    /// Unterstützt sämtliche ERP Anwendungsobjekt
    /// mit der Basisinfrastruktur
    /// </summary>
    //          sind jetzt öffentlich
    // 20240227 HttpClient Eigenschaft ergänzt
    // 20240229 Verbindungszeichenfolge (ConnectionString)
    public abstract class AppObjekt : System.Object
    {
        /// <summary>
        /// Ruft die Infrastruktur der WIFI Anwendung
        /// ab oder legt diese fest.
        /// </summary>
        public Infrastruktur Kontext { get; set; } = null!;

        #region FehlerAufgetreten Ereignis

        // Dritter Schritt (oder Erster mit System.EventHandler)
        // Das "Ereignis" deklarieren

        /// <summary>
        /// Wird ausgelöst, wenn eine Ausnahme
        /// aufgetreten ist
        /// </summary>
        public event FehlerAufgetretenEventHandler
            FehlerAufgetreten = null!;

        // Vierter Schritt (oder Zweiter)
        // Eine Methode zum Aufrufen der
        // angehängten Methode, der Ereignis-Auslöser

        /// <summary>
        /// Ruft die Methode auf, die das
        /// FehlerAufgetreten Ereignis behandelt
        /// </summary>
        /// <param name="e">Daten mit der 
        /// Fehlerbeschreibung</param>
        protected virtual void OnFehlerAufgetreten(
            FehlerAufgetretenEventArgs e)
        {
            // Damit die Garbage Collection
            // das Objekt mit dem Behandler nicht entfernt
            var BehandlerKopie = this.FehlerAufgetreten;

            if (BehandlerKopie != null)
            {
                BehandlerKopie(this, e);
            }
        }

        #endregion FehlerAufgetreten Ereignis

        #region Datenpfade

        /// <summary>
        /// Internes Singleton Feld für die Eigenschaft
        /// </summary>
        private static string _LokalerDatenpfad = null!;

        /// <summary>
        /// Ruft den Pfad zum Speichern
        /// im lokalen AppData Ordner des Benutzers ab
        /// </summary>
        /// <remarks>Es ist sichergestellt,
        /// dass der Ordner existiert.</remarks>
        public string LokalerDatenpfad
        {
            get
            {
                if (AppObjekt._LokalerDatenpfad == null)
                {
                    AppObjekt._LokalerDatenpfad
                        = System.IO.Path.Combine(
                                    System.Environment
                                        .GetFolderPath(Environment.SpecialFolder
                                            .LocalApplicationData),
                                    // Muss um den Firmennamen ergänzt werden
                                    this.HoleFirmenname(),
                                    // Muss um den Produktnamen
                                    // ergänzt werden
                                    this.HoleProduktname(),
                                    // Muss um die Version
                                    // ergänzt werden
                                    this.HoleVersion()
                                );
                }

                // Bei jedem Zugriff sicherstellen,
                // das der Ordner existiert
                System.IO.Directory
                    .CreateDirectory(AppObjekt._LokalerDatenpfad);

                return AppObjekt._LokalerDatenpfad;
            }
        }

        /// <summary>
        /// Internes Singleton Feld für die Eigenschaft
        /// </summary>
        private static string _Datenpfad = null!;

        /// <summary>
        /// Ruft den Pfad zum Speichern
        /// im Roaming Ordner des Benutzers ab
        /// </summary>
        /// <remarks>Es ist sichergestellt,
        /// dass der Ordner existiert.</remarks>
        public string Datenpfad
        {
            get
            {
                if (AppObjekt._Datenpfad == null)
                {
                    AppObjekt._Datenpfad
                        = System.IO.Path.Combine(
                                    System.Environment
                                        .GetFolderPath(Environment.SpecialFolder
                                            .ApplicationData),
                                    // Muss um den Firmennamen ergänzt werden
                                    this.HoleFirmenname(),
                                    // Muss um den Produktnamen
                                    // ergänzt werden
                                    this.HoleProduktname(),
                                    // Muss um die Version
                                    // ergänzt werden
                                    this.HoleVersion()
                                );
                }

                // Bei jedem Zugriff sicherstellen,
                // das der Ordner existiert
                System.IO.Directory
                    .CreateDirectory(AppObjekt._Datenpfad);

                return AppObjekt._Datenpfad;
            }
        }

        #endregion Datenpfade

        #region Anwendungsfpad

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private static string _AppPfad = null!;

        /// <summary>
        /// Ruft den vollständigen Ordner ab,
        /// aus dem die Anwendung gestartet wurde
        /// </summary>
        public string AppPfad
        {
            get
            {
                if (AppObjekt._AppPfad == null)
                {
                    AppObjekt._AppPfad
                        = System.IO.Path.GetDirectoryName(
                                System.Reflection.Assembly
                                    .GetEntryAssembly()!.Location)!;
                }

                return AppObjekt._AppPfad;
            }
        }

        #endregion Anwendungspfad

        #region Internetzugriffe 20240227

        /// <summary>
        /// Internes statisches Feld zum Cachen
        /// des Objekts für Internetzugriffe
        /// </summary>
        /// <remarks>Im Handbuch von HttpClient steht,
        /// dass eine Anwendung nur eine Instanz
        /// benutzen darf</remarks>
        private static System.Net.Http.HttpClient _HttpClient = null!;

        /// <summary>
        /// Ruft das Objekt für Internetzugriffe ab
        /// </summary>
        /// <remarks>Es ist sichergestellt, dass
        /// die Anwendung nur mit einer Instanz arbeitet.
        /// Das Accept-Language Header-Feld wird
        /// auf die aktuelle Anwendungssprache
        /// voreingestellt.</remarks>
        protected System.Net.Http.HttpClient HttpClient
        {
            get
            {
                if (AppObjekt._HttpClient == null)
                {
                    AppObjekt._HttpClient
                        = new System.Net.Http.HttpClient();

                    this.Kontext.Log.Hinzufügen(
                        $"{this} hat das Objekt für " +
                        $"Internetzugriffe initialisiert...");

                    AppObjekt._HttpClient.DefaultRequestHeaders
                        .Add(
                            "Accept-Language",
                            this.Kontext.Sprachen
                            .AktuelleSprache.Code);

                    this.Kontext.Log.Hinzufügen(
                        $"Accept-Language Header auf " +
                        $"\"{AppObjekt._HttpClient.DefaultRequestHeaders.AcceptLanguage}\" festgelegt.");

                }

                return AppObjekt._HttpClient;
            }
        }

        #endregion Internetzugriffe 20240227

        #region ConnectionString für Datenbankzugriffe

        /// <summary>
        /// Internes Feld zum Cachen des
        /// Connectionstrings
        /// </summary>
        private static string _Verbindungszeichenfolge = null!;

        /// <summary>
        /// Ruft den ConnectionString zum
        /// Aufbau einer Datenbankverbindung ab
        /// oder legt diesen fest
        /// </summary>
        /// <remarks>Die Einstellung kann über
        /// die Konfiguration vorgenommen werden.
        /// Bei dynamisch angehängten SQL Server
        /// Datenbanken werden relative Pfadangaben
        /// bezogen auf das Anwendungsverzeichnis
        /// akzeptiert. Die Information wird 
        /// statisch gecachet, d.h. bei Änderungen
        /// die Anwendung neu starten. Es ist
        /// egal, bei welchem Objekt die
        /// Einstellung vorgenommen wird</remarks>
        /// <exception cref="NullReferenceException">
        /// Tritt auf, wenn der ConnectionString 
        /// nicht konfiguriert ist</exception>
        public string Verbindungszeichenfolge
        {
            get
            {
                if (AppObjekt._Verbindungszeichenfolge == null)
                {
                    throw new NullReferenceException(
                        "Die Datenbank Verbindungszeichenfolge fehlt!");
                }

                return AppObjekt._Verbindungszeichenfolge;
            }
            set
            {
                AppObjekt._Verbindungszeichenfolge
                    = this.PrüfeDatenbankPfad(value);
                this.Kontext.Log.Hinzufügen(
                    $"{this} hat die " +
                    $"Verbindungszeichenfolge=" +
                    $"\"{AppObjekt._Verbindungszeichenfolge}\" " +
                    $"erhalten.");
            }
        }

        /// <summary>
        /// Gibt eine kontrollierte Verbindungszeichenfolge zurück
        /// </summary>
        /// <param name="einstellung">Ein ConnectionString
        /// aus der Anwendungskonfiguration</param>
        /// <returns>Eine absolute Pfadangabe, wenn
        /// es eine SQL Server AttachDbFilename
        /// Verbindungszeichenfolge mit einem relativen Pfad ist.</returns>
        private string PrüfeDatenbankPfad(string einstellung)
        {
            this.Kontext.Log.StartMelden();
            const string MdfKennung = "AttachDBFilename";

            // Nur, wenn MdfKennung enthalten ist
            if (einstellung.Contains(
                    MdfKennung,
                    StringComparison.InvariantCultureIgnoreCase))
            {
                // Den ConnectionString zerlegen
                var Teile = einstellung.Split(';');

                // Den Teil mit der MdfKennung finden
                var PfadEinstellung = (from t in Teile
                                       where t.Contains(
                                                MdfKennung,
                                                StringComparison.InvariantCultureIgnoreCase)
                                       select t
                                       ).First();

                // Aus diesem Teil nach dem = den Pfad ermitteln
                PfadEinstellung
                    = PfadEinstellung
                        .Substring(
                            PfadEinstellung
                                .IndexOf('=') + 1);
                
                var OriginalPfad = PfadEinstellung;

                // Diesen Pfad prüfen, ob er absolut ist
                // Wenn nicht mit Hilfe vom AppPfad
                // absolut machen
                if (!System.IO.Path.IsPathRooted(PfadEinstellung))
                {
                    PfadEinstellung
                        = System.IO.Path.GetFullPath(
                                            PfadEinstellung,
                                            this.AppPfad);
                }

                this.Kontext.Log.Hinzufügen(
                    $"{this} hat den " +
                    $"Datenbank Pfad \"{PfadEinstellung}\" " +
                    $"ermittelt.");

                // Den gesamten ConnectionString wieder berechnen
                einstellung 
                    = einstellung.Replace(
                                    OriginalPfad, 
                                    PfadEinstellung);
            }

            this.Kontext.Log.EndeMelden();

            return einstellung;
        }
        #endregion ConnectionString für Datenbankzugriffe
    }
}
