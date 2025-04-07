using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers
{
    /// <summary>
    /// Unterstützt sämtliche Gateway REST Controller
    /// mit der Infrastruktur
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public abstract class BasisRestController : ControllerBase
    {
        #region Anwendungskonfiguration

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private static int? _BehalteInCacheMinuten = null!;

        /// <summary>
        /// Ruft die Verweildauer von Objekten
        /// im Cache in Minuten ab
        /// </summary>
        /// <remarks>Standardwert 20 Minuten. Der
        /// Wert kann über die Konfiguration geändert werden</remarks>
        protected int BehalteInCacheMinuten
        {
            get
            {
                if (BasisRestController._BehalteInCacheMinuten == null)
                {
                    int Einstellung = 20; //für den Standard

                    if (!int.TryParse(
                            this.Konfiguration["BehalteInCacheMinuten"],
                            out Einstellung))
                    {
                        this.Kontext.Log.Hinzufügen(new Anwendung.Daten.Protokolleintrag
                        {
                            Typ = Anwendung.Daten.ProtokolleintragTyp.Warnung,
                            Text = $"Die Konfigurationseinstellung " +
                            $"BehalteInCacheMinuten=\"{this.Konfiguration["BehalteInCacheMinuten"]}\" " +
                            $"kann nicht als Integer benutzt werden!"
                        });
                    }

                    BasisRestController._BehalteInCacheMinuten = Einstellung;
                    this.Kontext.Log.Hinzufügen(new Anwendung.Daten.Protokolleintrag
                    {
                        Typ = Anwendung.Daten.ProtokolleintragTyp.Normal,
                        Text = $"Die Verweildauer der Objekt im Cache " +
                        $"beträgt {Einstellung} Minuten."
                    });

                }

                return BasisRestController._BehalteInCacheMinuten.Value;
            }
        }

        /// <summary>
        /// Ruft die Anwendungskonfiguration ab
        /// </summary>
        /// <remarks>Muss über den Konstruktor
        /// initialisiert werden</remarks>
        protected IConfiguration Konfiguration { get; private set; }

        /// <summary>
        /// Initialisiert einen neuen REST Controller
        /// </summary>
        /// <param name="konfiguration">Stellt die
        /// Anwendungskonfiguration der ASP.Net Anwendung bereit</param>
        public BasisRestController(IConfiguration konfiguration)
        {
            this.Konfiguration = konfiguration;
        }

        #endregion Anwendungskonfiguration

        #region Infrastruktur

        /// <summary>
        /// Ruft die Basis Infrastruktur ab
        /// </summary>
        /// <remarks>Über die Konfigurationseinstellung
        /// TaskDB kann die Datenbankverbindungszeichenfolge
        /// angepasst werden.Das Objekt wird gecachet
        /// und ohne Zugriff 20 Minuten erhalten.
        /// Mit der Einstellung "BehalteInCacheMinuten"
        /// kann das geändert werden.</remarks>
        protected Anwendung.Infrastruktur Kontext
        {
            get
            {
                var Schlüssel = $"{this.GetType().FullName}.Kontext";
                //Prüfen, die Infrastruktur noch lebt
                var Kontext = System.Runtime.Caching
                    .MemoryCache.Default.Get(Schlüssel)
                    as Anwendung.Infrastruktur;

                if (Kontext == null)
                {
                    // Falls nicht, neu initialisieren...
                    Kontext = new Anwendung.Infrastruktur();

                    //Falls gewünscht, die Protokollierung aktiveren
                    var LogPfad = this.Konfiguration["ERPLog"]!.Trim();

                    if (LogPfad != string.Empty)
                    {
                        //Falls nicht absolut, bezogen auf
                        //das Anwendungsverzeichnis betrachten
                        if (!System.IO.Path.IsPathRooted(LogPfad))
                        {
                            LogPfad = System.IO.Path
                                .GetFullPath(
                                    LogPfad,
                                    AppObjekt.AppPfad);
                        }

                        Kontext.Log.Dateiname = LogPfad!;
                    }

                    Kontext.Log.Hinzufügen(
                        $"{this} initialisiert die Basis Infrastruktur");

                    //Die Datenbank Verbindungszeichen einstellen
                    //(Weil diese statisch gecachet wird,
                    // ist egal, bei welchem AppObjekt das
                    // gemacht wird)
                    Kontext.Log.Verbindungszeichenfolge
                        = this.Konfiguration.GetConnectionString("ERPDB")!;
                }

                // Damit das Objekt am Leben bleibt,
                // bei jedem Zugriff im Cache aktualisieren
                System.Runtime.Caching.MemoryCache.Default
                    .Set(
                        Schlüssel,
                        Kontext,
                        System.DateTimeOffset.Now
                            .AddMinutes(this.BehalteInCacheMinuten)
                    );

                return Kontext;
            }
        }

        #endregion

        #region Informationen vom Client (Request)

        /// <summary>
        /// Ruft die erste Einstellung von
        /// Accept-Language ab
        /// </summary>
        protected string? GewünschteRückgabeSprache
            => (from s in this.Request.Headers.AcceptLanguage
                select s).FirstOrDefault()?.Split(',')[0];

        #endregion Informationen vom Client
    }
}
