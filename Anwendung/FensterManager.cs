using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anwendung
{
    /// <summary>
    /// Stellt einen Dienst zum
    /// Verwalten der Anwendungsfenster bereit
    /// </summary>
    // 20240130 Die Anzahl der Monitore
    //          und Auflösung wird berücksichtigt
    public class FensterManager : AppObjekt
    {
        #region Win32 API Wrapper

        //Hr. Schatzl - wegen der einfacheren
        //              Benutzbarkeit eine Auflistung
        //              anstelle der Konstanten
        private enum SystemMetricsInfo : int
        {
            /// <summary>
            /// SM_CMONITORS
            /// </summary>
            AnzahlMonitore = 80,
            /// <summary>
            /// SM_CXSCREEN
            /// </summary>
            HorizontaleAuflösung = 0,
            /// <summary>
            /// SM_CYSCREEN
            /// </summary>
            VertikaleAuflösung = 1
        }

        /// <summary>
        /// Gibt gewünschte Information über
        /// die aktuelle Bildschirmsituation zurück
        /// </summary>
        /// <param name="info">Eine Ganzzahl
        /// mit der angegeben wird, welche Information
        /// benötigt wird</param>
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int GetSystemMetrics(SystemMetricsInfo info);

        /// <summary>
        /// Ruft einen Code ab, mit
        /// dem die aktuelle Monitorkonfiguration
        /// unterschieden werden kann.
        /// </summary>
        /// <remarks>Die Information ist
        /// nicht gecachet, weil sich die
        /// Anzahl der Monitor zur Laufzeit
        /// ändern kann</remarks>
        protected string MonitorSchlüssel
            => $"M{FensterManager.GetSystemMetrics(SystemMetricsInfo.AnzahlMonitore)}" +
            $"_{FensterManager.GetSystemMetrics(SystemMetricsInfo.HorizontaleAuflösung)}" +
            $"x{FensterManager.GetSystemMetrics(SystemMetricsInfo.VertikaleAuflösung)}_";

        #endregion Win32 API Wrapper

        #region Fensterliste

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private Daten.FensterInfos _Liste = null!;

        /// <summary>
        /// Ruft die Auflistung mit
        /// den verwalteten Fensterdaten ab
        /// </summary>
        protected Daten.FensterInfos Liste
        {
            get
            {
                if (this._Liste == null)
                {
                    this._Liste = this.Lesen();
                }

                return this._Liste;
            }
        }

        #endregion Fensterliste

        #region Hinzufügen und Abrufen

        /// <summary>
        /// Fügt die Fensterdaten
        /// im Manager ein oder aktualisiert
        /// die vorhandenen Daten
        /// </summary>
        /// <param name="fenster">Ein FensterInfo
        /// Objekt mit der Beschreibung des
        /// zu verwaltenden Fensters</param>
        /// <remarks>Die Eigenschaft Name
        /// wird als Schlüssel benutzt. Positions-
        /// und Größenangaben werden nur aktualisiert,
        /// wenn neue Daten vorhanden sind.</remarks>
        public void Hinterlegen(Daten.FensterInfo fenster)
        {
            //Vorsicht: Die Hole Methode ergänzt den Monitorschlüssel
            var VorhandeneInfo = this.Hole(fenster.Name);

            // Falls das Fenster noch
            // nicht vorhanden ist, hinterlegen
            if (VorhandeneInfo == null)
            {
                //20240130  Die Bildschirmkonfiguration berücksichtigen
                fenster.Name = this.MonitorSchlüssel + fenster.Name;
                this.Liste.Add(fenster);
            }
            else
            {
                // sonst aktualisieren...

                // Auf alle Fälle den Zustand
                VorhandeneInfo.Zustand = fenster.Zustand;

                // Positions- und Größenangaben
                // nur, wenn welche vorhanden sind,
                // damit frühere Werte nicht verloren werden

                // Original (Binärentscheidung als Anweisung) ...
                if (fenster.Links != null)
                {
                    VorhandeneInfo.Links = fenster.Links;
                }

                // Außerdem original (Binärentscheidung
                // als Funktion (in SQL oder BASIC die IIF Funktion) ...
                VorhandeneInfo.Oben
                    = fenster.Oben.HasValue ?
                        fenster.Oben :
                        VorhandeneInfo.Oben;

                // Später "Falls Null-Operator"
                VorhandeneInfo.Breite
                    = fenster.Breite ??
                        VorhandeneInfo.Breite;
                VorhandeneInfo.Höhe
                    = fenster.Höhe ??
                        VorhandeneInfo.Höhe;
            }
        }

        /// <summary>
        /// Gibt das FensterInfo Objekt für
        /// das gewünschte Fenster zurück
        /// </summary>
        /// <param name="fensterName">Der interne
        /// Fenstername als Schlüssel zum Finden</param>
        /// <returns>Entweder das FensterInfo Objekt
        /// mit dem Namen oder null, falls das
        /// Fenster noch nicht registriert ist
        /// hinterlegt ist.</returns>
        public Daten.FensterInfo? Hole(string fensterName)
        {
            //20240130  Monitorkonfiguration ergänzt
            return this.Liste
                .Find(f => f.Name == this.MonitorSchlüssel + fensterName);
        }

        #endregion Hinzufügen und Abrufen

        #region Speichern und Lesen

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private static string _Standardpfad = null!;

        /// <summary>
        /// Ruft den vollständigen Speicherort
        /// der Datei mit den verwalteten Fenstern ab.
        /// </summary>
        /// <remarks>Dieser befindet sich im
        /// Lokalen Datenpfad des Benutzerprofils
        /// und wird über die Assembly Informationen
        /// Company, Product und Version konfiguriert.</remarks>
        public string Standardpfad
        {
            get
            {
                if (FensterManager._Standardpfad == null)
                {
                    const string Datei = "Fenster.xml";
                    FensterManager._Standardpfad
                        = System.IO.Path.Combine(
                            this.LokalerDatenpfad,
                            Datei);
                }

                return FensterManager._Standardpfad;
            }
        }

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private Controller.FensterXmlController _Controller = null!;

        /// <summary>
        /// Ruft den Dienst zum Serialisieren
        /// und Deserialisieren der verwalteten
        /// Fenster ab.
        /// </summary>
        private Controller.FensterXmlController Controller
        {
            get
            {
                if (this._Controller == null)
                {
                    this._Controller
                        = this.Kontext
                            .Produziere<Controller.FensterXmlController>();
                }

                return this._Controller;
            }
        }

        /// <summary>
        /// Schreibt die Liste mit den
        /// verwalteten Fenstern in die
        /// Datei des Standardpfads
        /// </summary>
        /// <remarks>Beim Herunterfahren der
        /// Infrastruktur diese Methode aufrufen</remarks>
        public void Speichern()
        {
            try
            {
                this.Controller.Speichern(
                        this.Standardpfad,
                        this.Liste);
            }
            catch (Exception ex)
            {
                this.OnFehlerAufgetreten(
                    new FehlerAufgetretenEventArgs(ex));
            }
        }

        /// <summary>
        /// Gibt die Liste mit den gespeicherten
        /// FensterInfo Objekten aus der Datei
        /// des Standardpfads zurück
        /// </summary>
        /// <returns>Die Liste mit den Daten
        /// oder eine leere Liste, wenn
        /// ein Problem aufgetreten ist,
        /// z. B. einem neuen Benutzer</returns>
        protected Daten.FensterInfos Lesen()
        {
            try
            {
                return this.Controller.Lesen(this.Standardpfad);
            }
            catch (System.Exception ex)
            {
                this.OnFehlerAufgetreten(
                    new FehlerAufgetretenEventArgs(ex));

                // Damit im Fehlerfall nicht wieder versucht
                // wird, die Liste zu lesen ...
                return new Daten.FensterInfos();
            }
        }

        #endregion Speichern und Lesen

    }
}
