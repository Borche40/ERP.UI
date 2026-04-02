using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anwendung
{
    /// <summary>
    /// Stellt den Anwendungskontext für
    /// eine Anwendung bereit
    /// </summary>
    public class Infrastruktur : System.Object
    {
        #region Objektfabrik

        /// <summary>
        /// Gibt ein initialisiertes Anwendungsobjekt
        /// mit eingestelltem Kontext zurück
        /// </summary>
        /// <param name="typ">Eine Klasse, 
        /// die AppObjekt erweitert und einen 
        /// öffentlichen parameterlosen 
        /// Konstruktor besitzt</param>
        /// <returns>Ein AppObjekt mit 
        /// eingestelltem Kontext</returns>
        //20240314  
        public object? Produziere(System.Type typ)
        {

            //Über die Reflection das
            //generische Produziere aufrufen
            return this.GetType().GetMethod(
                "Produziere", 
                //Herr Gnese, performanter
                //new Type[0]
                Type.EmptyTypes
                )?
                .MakeGenericMethod(typ).Invoke(this, null);
        }

        /// <summary>
        /// Gibt ein initialisiertes Anwendungsobjekt
        /// mit eingestelltem Kontext zurück
        /// </summary>
        /// <typeparam name="T">Eine Klasse, 
        /// die AppObjekt erweitert und einen 
        /// öffentlichen parameterlosen 
        /// Konstruktor besitzt</typeparam>
        /// <returns>Ein AppObjekt mit 
        /// eingestelltem Kontext</returns>
        public T Produziere<T>() where T : AppObjekt, new()
        {
            var NeuesObjekt = new T();

            // Die aktuelle Infrastruktur
            // an das neue Objekt weitergeben
            NeuesObjekt.Kontext = this;

            // Automatisch im Fehlerfall
            // eine Protokollzeile im Ausgabefenster
            NeuesObjekt.FehlerAufgetreten
                            += this.AusnahmeMelden;

            System.Diagnostics.Debug.WriteLine(
                    $"{NeuesObjekt} initialisiert...");

            // Damit keine Rekursion (Absturz Stapelüberlauf)
            // auftritt...
            if (NeuesObjekt is not ProtokollManager)
            {
                this.Log.Hinzufügen($"{NeuesObjekt} initialisiert...");
            }

            // Hier weitere Produktionsschritte
            // ergänzen ...

            return NeuesObjekt;
        }

        /// <summary>
        /// Hinterlegt im Studio Ausgabefenster
        /// eine Meldung, dass eine Ausnahme 
        /// aufgetreten ist, behandelt also
        /// das FehlerAufgetreten Ereignis
        /// </summary>
        /// <remarks>Hier handelt es sich um
        /// eine benannte Methode</remarks>
        // 20240206 Die Ausnahme wird jetzt auch
        //          in das Anwendungsprotokoll geschrieben
        private void AusnahmeMelden(
                        object s,
                        FehlerAufgetretenEventArgs e)
        {
#if DEBUG
            // in .Net Framework (bis 4.8)
            //System.Console.WriteLine()
            // seit .Net
            System.Diagnostics.Debug.WriteLine(
                $"FEHLER! {s} löste eine Ausnahme {e.Ursache.Message} aus.");
#endif

            this.Log.Hinzufügen(new Daten.Protokolleintrag
            {
                Typ = Daten.ProtokolleintragTyp.Fehler,
                Text = $"FEHLER! {s} löste eine Ausnahme" +
                        $" {e.Ursache.Message} aus."
            });
        }

        #endregion Objektfabrik

        #region Sprachendienst

        /// <summary>
        /// Internes Feld zum Cachen
        /// </summary>
        private SprachenManager _Sprachen = null!;

        /// <summary>
        /// Ruft den Dienst zum Verwalten
        /// der Anwendungssprachen ab
        /// </summary>
        public SprachenManager Sprachen
        {
            get
            {
                if (this._Sprachen == null)
                {
                    /*
                    //Ohne Objektfabrik
                    this._Sprachen = new SprachenManager();
                    this._Sprachen.Kontext = this;
                    */

                    this._Sprachen
                        = this.Produziere<SprachenManager>();
                }
                return this._Sprachen;
            }
        }

        #endregion Sprachendienst

        #region Fensterdienst

        /// <summary>
        /// Internes Feld zum Cachen
        /// des Fenster-Managers der Anwendung
        /// </summary>
        private FensterManager _Fenster = null!;

        /// <summary>
        /// Ruft den Dienst zum
        /// Verwalten der Anwendungsfenster ab
        /// </summary>
        public FensterManager Fenster
        {
            get
            {
                if (this._Fenster == null)
                {
                    this._Fenster
                        = this.Produziere<FensterManager>();
                }

                return this._Fenster;
            }
        }

        #endregion Fensterdienst

        #region Protokolldienst

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private ProtokollManager _Log = null!;

        /// <summary>
        /// Ruft das Anwendungsprotokoll ab
        /// </summary>
        public ProtokollManager Log
        {
            get
            {
                if (this._Log == null)
                {
                    this._Log = this.Produziere<ProtokollManager>();
                }

                return this._Log;
            }
        }

        #endregion Protokolldienst
    }
}
