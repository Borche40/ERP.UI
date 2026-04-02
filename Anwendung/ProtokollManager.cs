using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Anwendung
{
    /// <summary>
    /// Stellt einen Dienst zum 
    /// Verwalten eines Anwendungslogs bereit
    /// </summary>
    /// <remarks>Soll das Protokoll gespeichert
    /// werden, die Eigenschaft Dateiname festlegen.
    /// Soll das Protkoll für WPF threadsicher sein,
    /// muss ein Dispatcher Objekt vorhanden sein
    /// </remarks>
    public class ProtokollManager : Anwendung.AppObjekt
    {
        #region Für WPF threadsicher

        /// <summary>
        /// Ruft den zentralen WPF Thread Manager
        /// ab oder legt diesen fest
        /// </summary>
        /// <remarks>Fall dieser vorhanden ist,
        /// ist die Hinzufügen Methode threadsicher</remarks>
        public object? Dispatcher { get; set; }

        #endregion Für WPF threadsicher

        #region Daten

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private Daten.Protokolleinträge _Einträge = null!;

        /// <summary>
        /// Ruft die Liste mit den
        /// Protokollzeilen ab
        /// </summary>
        public Daten.Protokolleinträge Einträge
        {
            get
            {
                if (this._Einträge == null)
                {
                    this._Einträge = new Daten.Protokolleinträge();
                }

                return this._Einträge;
            }
        }

        #endregion Daten

        #region Einträge

        /// <summary>
        /// Hinterlegt einen neuen Hinweis
        /// in den Protokolleinträgen
        /// </summary>
        /// <param name="eintrag">Die Daten des Protokolleintrags</param>
        /// <param name="rekursiv">Für interne Zwecke, damit
        /// ein threadsicherer Aufruf möglich ist</param>
        /// <remarks>Die Threadsicherheit ist für WPF gewährleistet,
        /// wenn ein Dispatcher vorhanden ist, sonst nicht</remarks>
        public virtual void Hinzufügen(
            Daten.Protokolleintrag eintrag,
            bool rekursiv = false)
        {

            //Die Threadsicherheit gewährleisten
            if (this.Dispatcher != null && !rekursiv)
            {
                this.Dispatcher.GetType()
                    .GetMethod(
                        "Invoke",
                        new Type[] {
                            typeof(System.Action)
                        })?.Invoke(
                                this.Dispatcher,
                                new object[]
                                {
                                    () => this.Hinzufügen(
                                            eintrag,
                                            rekursiv: true)
                                });
            }
            else
            {
                // nicht threadsicher

                this.Einträge.Add(eintrag);
                this.EintragSpeichern(eintrag);

                if (eintrag.Typ == Daten.ProtokolleintragTyp.Fehler)
                {
                    this.EnthältFehler = true;
                    this.OnEnthältFehlerGeändert(System.EventArgs.Empty);
                }

                this.Rückrufe.AlleAufrufen();

                //Schimpfen, wenn Besitzer nicht mehr leben...
                var Leichen = this.Rückrufe.AnzahlLeichen;
                if (Leichen > 0)
                {
                    this.Einträge.Add(new Daten.Protokolleintrag
                    {
                        Typ = Daten.ProtokolleintragTyp.Fehler,
                        Text = $"{this} hat {Leichen} Rückrufe " +
                        $"ohne Besitzer!\r" +
                        $"Nicht mehr benötigte Rückrufe stornieren!"
                    });
                    this.EnthältFehler = true;
                    this.OnEnthältFehlerGeändert(System.EventArgs.Empty);
                }
            }
        }
        /// <summary>
        /// Hinterlegt einen neuen normalen
        /// Eintrag in den Protokolleinträgen
        /// </summary>
        /// <param name="hinweis">Der Text, der
        /// als normaler Eintrag hinterlegt werden soll</param>
        public virtual void Hinzufügen(string hinweis)
        {
            this.Hinzufügen(new Daten.Protokolleintrag
            {
                Typ = Daten.ProtokolleintragTyp.Normal,
                Text = hinweis
            });
        }
        /// <summary>
        /// Fügt  einen neuen Fehler- Eintrag in den Protokolleinträgen hinzu
        /// </summary>
        /// <param name="fehlerText">Der Text,der als Fehler hinterlegt werden soll</param>
        public void Fehler(string fehlerText)
        {
            this.Hinzufügen(new Daten.Protokolleintrag
            {
                Typ = Daten.ProtokolleintragTyp.Fehler,
                Text = fehlerText
            });
        }
        #endregion Einträge

        #region Hilfsmethoden zum Protokollieren

        /// <summary>
        /// Hinterlegt im Protokoll einen 
        /// Eintrag, dass die Methode zu laufen beginnt.
        /// </summary>
        /// <param name="methodenName">Optional;
        /// falls nicht angegeben, wird der Name
        /// vom Aufrufer benutzt</param>
        public void StartMelden(
            [System.Runtime.CompilerServices.CallerMemberName]
            string methodenName = null!)
        {
            var Besitzer = new System.Diagnostics.StackTrace(1)
                .GetFrame(0)!
                .GetMethod()!
                .DeclaringType!.FullName;

            this.Hinzufügen($"{Besitzer}.{methodenName} startet...");
        }

        /// <summary>
        /// Hinterlegt im Protokoll einen 
        /// Eintrag, dass die Methode zu beendet ist.
        /// </summary>
        /// <param name="methodenName">Optional;
        /// falls nicht angegeben, wird der Name
        /// vom Aufrufer benutzt</param>
        public void EndeMelden(
            [System.Runtime.CompilerServices.CallerMemberName]
            string methodenName = null!)
        {
            var Besitzer = new System.Diagnostics.StackTrace(1)
                .GetFrame(0)!
                .GetMethod()!
                .DeclaringType!.FullName;

            this.Hinzufügen($"{Besitzer}.{methodenName} beendet.");
        }

        #endregion Hilfsmethoden zum Protokollieren

        #region Protokolldatei erstellen

        /// <summary>
        /// Löscht im Verzeichnis der Protokolldatei
        /// die älteste und nummeriert die existierenden neu
        /// damit der benötigte Speicherplatz nicht
        /// unnötigt steigt.
        /// </summary>
        /// <param name="behalteVersionen">Optional; Gibt an,
        /// wie viele alte Protokolldateien
        /// aufgehoben werden müssen. Standardwert 4,
        /// was zu insgesamt 5 Dateien führt. Die aktuelle
        /// und 4 Sicherungen</param>
        public virtual void Zusammenräumen(int behalteVersionen = 4)
        {
            this.StartMelden();
            if (this.Dateiname != null)
            {
                for (int i = behalteVersionen; i > 0; i--)
                {
                    var NeuerName = $"{this.Dateiname}.{i}";
                    var AlterName
                        = i > 1 ? $"{this.Dateiname}.{i - 1}"
                        : $"{this.Dateiname}";

                    
                    System.IO.File.Delete(NeuerName);
                    
                    if (System.IO.File.Exists(AlterName))
                    {
                        System.IO.File.Move(AlterName, NeuerName);
                    }
                }

                this.Hinzufügen($"{this} hat das alte " +
                    $"Protokoll gesichert...");
            }
            this.EndeMelden();
        }

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private string _Dateiname = null!;

        /// <summary>
        /// Ruft den vollständigen Pfad der 
        /// Protokolldatei ab oder legt 
        /// diesen fest
        /// </summary>
        /// <remarks>Sollte keine Datei
        /// geschrieben werden, 
        /// auf null festlegen.</remarks>
        public string Dateiname
        {
            get => this._Dateiname;

            set
            {
                if (value == string.Empty)
                {
                    //20240201 Hr. Schatzl
                    //
                    //Die irrtümliche Rekursion behoben
                    this._Dateiname = null!;
                }
                else
                {
                    this._Dateiname = value;
                    this.Zusammenräumen();
                }
            }
        }

        /// <summary>
        /// Fügt den Eintrag in die
        /// Protokolldatei hinzu
        /// </summary>
        /// <param name="eintrag">Information,
        /// die in die Protokolldatei 
        /// geschrieben werden soll</param>
        /// <remarks>Sollte eine Ausnahme auftreten,
        /// wird die Protokollierung durch
        /// Einstellen vom Dateinamen auf null
        /// abgeschaltet. Sonderzeichen im Text,
        /// z. B. Tabulatoren und Zeilenumbrüche
        /// werden durch Leerzeichen ersetzt,
        /// damit Importprogramme keine Probleme haben.</remarks>
        protected virtual void EintragSpeichern(
            Daten.Protokolleintrag eintrag)
        {

            if (this.Dateiname == null) return;

            try
            {
                using var Schreiber
                    = new System.IO.StreamWriter(
                        this.Dateiname,
                        append: true,
                        System.Text.Encoding.Unicode);

                const string ZeilenMuster = "{0}\t{1}\t{2}";

                Schreiber.WriteLine(
                    string.Format(
                        ZeilenMuster,
                        eintrag.Zeitpunkt,          //  {0}
                        eintrag.Typ,                //  {1}
                                                    // Zeichen mit einer Bedeutung
                                                    // für Importprogramme entfernen
                        eintrag.Text                //  {2}
                            .Replace('\t', ' ')      // kein Tabs
                            .Replace('\r', ' ')      // kein Return
                            .Replace('\n', ' ')     // kein Zeilenvorschub
                        ));

                Schreiber.Close();
            }
            catch (System.Exception ex)
            {
                // Damit wir nicht wiederholt
                // in die Ausnahme laufen,
                // das Schreiben deakivieren
                this.Dateiname = null!;
                this.OnFehlerAufgetreten(
                    new FehlerAufgetretenEventArgs(ex));
            }
        }

        #endregion Protokolldatei erstellen

        #region Fehler-Einträge steuern

        /// <summary>
        /// Ruft True ab, wenn
        /// im Protokoll ein Fehlereintrag
        /// enthalten ist
        /// </summary>
        public bool EnthältFehler
        { get; private set; } = false;

        /// <summary>
        /// Beim ersten Fehlereintrag wird
        /// die Eigenschaft EnthältFehler auf True
        /// festgelegt. Zum Abschalten, zum
        /// Bestätigen diese Methode benutzen.
        /// </summary>
        /// <remarks>Seit der Version 1.1.2.0</remarks>
        public void FehlerBestätigten()
        {
            this.EnthältFehler = false;
        }

        /// <summary>
        /// Wird ausgelöst, wenn sich
        /// der Zustand von der EnthältFehler
        /// Eigenschaft auf True geändert hat.
        /// </summary>
        public event System.EventHandler
            EnthältFehlerGeändert = null!;

        /// <summary>
        /// Löst das Ereignis EnthältFehlerGeändert aus
        /// </summary>
        /// <param name="e">Ohne Zusatzdaten</param>
        protected virtual void OnEnthältFehlerGeändert(
                                    System.EventArgs e)
        {
            var BehandlerKopie = this.EnthältFehlerGeändert;
            if (BehandlerKopie != null)
            {
                BehandlerKopie(this, e);
            }
        }

        #endregion Fehler-Einträge steuern

        #region Gewünschte Rückrufmethoden sammeln

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private Daten.SchwacherMethodenVerweise _Rückrufe = null!;

        /// <summary>
        /// Ruft die Liste mit den Methoden ab,
        /// die der ProtokollManager ausführen soll,
        /// wenn ein neuer Eintrag hinterlegt wurde
        /// </summary>
        protected Daten.SchwacherMethodenVerweise Rückrufe
        {
            get
            {
                if (this._Rückrufe == null)
                {
                    this._Rückrufe
                        = new Daten.SchwacherMethodenVerweise();
                    this.Hinzufügen(
                        $"{this} hat die Liste für " +
                        $"Rückrufe initialisiert.");
                }

                return this._Rückrufe;
            }
        }

        /// <summary>
        /// Hinterlegt eine Methode, die
        /// beim Hinzufügen eines neuen Eintrags
        /// ausgeführt werden soll
        /// </summary>
        /// <param name="methode">Die Speicheradresse
        /// einer Methode, die beim Hinzufügen eines
        /// neuen Eintrags ausgeführt werden soll</param>
        /// <remarks>Die Methode wird als WeakReference
        /// hinterlegt, damit die Garbage Collection
        /// nicht am Entfernen des Besitzers gehindert wird.
        /// Wird ein Rückruf nicht mehr benötigt,
        /// diesen stornieren!</remarks>
        public void RückrufBuchen(System.Action methode)
        {
            this.Rückrufe.Add(
                new Daten.SchwacherMethodenVerweis(
                        methode));

            this.Hinzufügen(new Daten.Protokolleintrag
            {
                Typ = Daten.ProtokolleintragTyp.Warnung,
                Text = $"{this} hat einen Rückruf " +
                $"für {methode.Target} " +
                $"(ID={methode.Target?.GetHashCode()}) gebucht!\r" +
                $"{this.Rückrufe.Count} Rückrufe vorhanden."
            });
        }

        /// <summary>
        /// Entfernt die Methode, die
        /// beim Hinzufügen eines neuen Eintrags
        /// nicht mehr ausgeführt werden soll
        /// </summary>
        /// <param name="methode">Die Speicheradresse
        /// der Methode, die beim Hinzufügen eines
        /// neuen Eintrags nicht mehr ausgeführt werden soll</param>
        public void RückrufStornieren(System.Action methode)
        {
            //Die Speicheradressse der Methode
            //in den Rückrufen finden und entfernen
            if (this.Rückrufe.Remove(
                        (from r in this.Rückrufe
                         where r.Methode == methode
                         select r).FirstOrDefault()!))
            {

                this.Hinzufügen(new Daten.Protokolleintrag
                {
                    Typ = Daten.ProtokolleintragTyp.Normal,
                    Text = $"{this} hat einen Rückruf " +
                            $"für {methode.Target} " +
                            $"(ID={methode.Target?.GetHashCode()}) storniert!\r" +
                            $"{this.Rückrufe.Count} Rückrufe vorhanden."
                });
            }
        }

        #endregion Gewünschte Rückrufmethoden sammeln
    }
}
