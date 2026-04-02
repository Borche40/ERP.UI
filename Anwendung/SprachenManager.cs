using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anwendung
{
    /// <summary>
    /// Stellt einen Dienst zum Verwalten
    /// der unterstützten Anwendungssprachen bereit
    /// </summary>
    public class SprachenManager : AppObjekt
    {
        #region Controller

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private Controller.SprachenXmlController
            _Controller = null!;

        /// <summary>
        /// Ruft den Dienst zum Lesen
        /// und Schreiben von Sprachen ab
        /// </summary>
        /// <remarks>Später durch ein Interface
        /// ersetzen, damit einfach auf eine
        /// andere Technologie gewechselt werden kann</remarks>
        private Controller.SprachenXmlController Controller
        {
            get
            {
                if (this._Controller == null)
                {
                    this._Controller
                        = this.Kontext
                            .Produziere<Anwendung.Controller.SprachenXmlController>();
                }
                return this._Controller;
            }
        }

        #endregion Controller

        #region Unterstützte Sprachen

        /// <summary>
        /// Internes Feld für die
        /// unterstützten Sprachen
        /// </summary>
        private Daten.Sprachen _Liste = null!;

        /// <summary>
        /// Ruft die Auflistung der 
        /// unterstützten Sprachen ab
        /// </summary>
        public Daten.Sprachen Liste
        {
            get
            {
                if (this._Liste == null)
                {
                    try
                    {
                        
                        var LinqAbfrage 
                            = from s in this.Controller.HoleAusRessourcen()
                              orderby s.Name select s; 
                        // wegen den Typ-Sicherheit
                        this._Liste = new Daten.Sprachen();
                        this._Liste.AddRange(LinqAbfrage);
                    }
                    catch (System.Exception ex)
                    {
                        //Eine Möglichkeit
                        // mit einer verbesserten Exception
                        // weiter abstürzen - throw Schlüsselwort

                        // State-Of-The-Art
                        this.OnFehlerAufgetreten(
                            new FehlerAufgetretenEventArgs(ex));

                        // damit nicht wiederholt
                        // die fehlerhaften Ressourcen
                        // gelesen werden ...
                        this._Liste = new Daten.Sprachen();
                    }
                    finally
                    {
                        // Wird auf alle Fälle
                        // abgearbeitet - mit und ohne Fehler
                        // macht nur Sinn, wenn im
                        // catch ein throw
                    }
                }

                return this._Liste;
            }

            private set => this._Liste = value;
        }

        #endregion Unterstützte Sprachen

        #region Aktuelle Sprache

        /// <summary>
        /// Legt die aktuelle Sprache
        /// auf die gewünschte fest.
        /// </summary>
        /// <param name="code">Iso2Code der Sprache,
        /// die zur aktuellen Sprache werden soll</param>
        /// <remarks>Sollte die Sprache nicht gefunden
        /// werden, wird Englisch (en) benutzt.
        /// Die Suche ist case-insenstiv</remarks>
        //
        // Versionsverlauf
        // 20240130 Die Sprache wird auch zur
        //          CurrentUICultureInfo
        public void Festlegen(string code)
        {
            #region Sprache suchen

            //Hr. Schatzl: String.Compare auf String.Equals
            //             geändert, weil sofort True geliefert
            //             wird...
            var NeueSprache
                = this.Liste.Find(
                    s => code.Equals(
                            s.Code,
                            StringComparison.CurrentCultureIgnoreCase)
                    );
            
            if (NeueSprache != null)
            {
                this.AktuelleSprache = NeueSprache;
            }
            else
            {
                this.AktuelleSprache 
                    = this.Liste.Find(
                        s => s.Code.Equals(
                            "en", 
                            StringComparison.CurrentCultureIgnoreCase))!;
            }

            #endregion Sprache suchen

            #region CurrentUICulture umstellen

            if (System.Globalization.CultureInfo.CurrentUICulture
                    .TwoLetterISOLanguageName
                != this.AktuelleSprache.Code)
            {
                System.Globalization.CultureInfo.CurrentUICulture
                    = new System.Globalization.CultureInfo(
                            this.AktuelleSprache.Code);

                //Aufpassen - wir arbeiten gecachet,
                //d.h. die Ressourcen wurden bereits gelesen
                //und müssen für die neue Sprache
                //neugelesen werden, einfach den Cache entfernen
                this.Liste = null!;

                //Wegen der Verweistypen haben die
                //Sprachen in der neuen Liste andere Adressen
                //und muss die AktuelleSprache wieder
                //synchronisiert werden (rekursiv)
                this.Festlegen(this.AktuelleSprache.Code);

            }

            #endregion CurrentUICulture umstellen

        }

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private Daten.Sprache _AktuelleSprache = null!;

        /// <summary>
        /// Ruft die derzeitige Anwendungssprache
        /// ab oder legt diese fest
        /// </summary>
        /// <remarks>Sollte keine Sprache voreingestellt
        /// werden, wird die aktuelle Betriebssystemsprache
        /// benutzt. Wird keine Lokalisierung gefunden,
        /// wird Englisch verwendet</remarks>
        public Anwendung. Daten.Sprache AktuelleSprache
        {
            get
            {
                if (this._AktuelleSprache == null)
                {
                    var Iso2Code = System.Globalization
                        .CultureInfo.CurrentUICulture
                            .TwoLetterISOLanguageName;

                    this._AktuelleSprache
                        = this.Liste
                            .Find(s => string.Compare(
                                                s.Code,
                                                Iso2Code,
                                                ignoreCase: true
                                            ) == 0)!;

                    // Sollte die Betriebssystemsprache
                    // nicht gefunden werden, Englisch benutzen
                    if (this._AktuelleSprache == null)
                    {
                        this._AktuelleSprache
                            = this.Liste
                                .Find(
                                    s => string.Compare(
                                        ignoreCase: true,
                                        strA: s.Code,
                                        strB: "En"
                                        )
                                    == 0)!;
                    }
                }

                return this._AktuelleSprache;
            }
            set => this._AktuelleSprache = value;
        }

        #endregion Aktuelle Sprache
    }
}
