using ERP.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ERP.UI.Models
{
    /// <summary>
    /// Stellt einen Dienst zur Kommunikation
    /// mit dem Gateway REST Webdienst bereit
    /// </summary>
    internal class OnlineSpeicherController : OnlineController, ISpeicherController
    {
        #region Lösche Daten

        /// <summary>
        /// Löscht eine Aufgabe
        /// von dem angegebenen Benutzer.
        /// </summary>
        /// <param name="gruppe">Die Gruppe, die die Aufgabe enthält..</param>
        /// <param name="schlüssel">Ein Einmalschlüssel (Sitzungsschlüssel), 
        /// der dem Benutzer bei der Anmeldung zugewiesen wird, 
        /// um die aktuelle Sitzung zu identifizieren.</param>     
        /// <param name="aufgabe">Die Aufgabe, die gelöscht werden soll.</param>
        /// <returns>
        /// 1, wenn das löschen erfolgreich war,
        /// 0, wenn der Schlüssel ungültig ist,
        /// 2, falls keine Verbindung zum Server möglich ist
        /// </returns>
        public async Task<int> AufgabeLöschenAsync(System.Guid schlüssel,
            string gruppe, Aufgabe aufgabe)
        {
            var LöschenUrl =
                $"{this.BasisUrl}erpsystem/{schlüssel}/{gruppe}/{aufgabe.ID}/löschen";

            var daten =
                new StringContent(JsonSerializer.Serialize(aufgabe), Encoding.UTF8, "application/json");
            try
            {
                using var Antwort = await this.HttpClient.PostAsync(LöschenUrl, daten);
                // sobald eine Rückmeldung ist,
                // den Inhalt als Text (Json) lesen
                var JsonDaten = await Antwort?.Content?.ReadAsStringAsync()!;

                // Die Json Daten in unsere Objekte mappen
                return System.Text.Json.JsonSerializer
                    .Deserialize<int>(
                        JsonDaten, this.JsonOptionen)!;
            }
            catch (HttpRequestException e)
            {
                this.OnVerbindungFehlgeschlagen(EventArgs.Empty);
                this.Kontext.Log.Hinzufügen(new Anwendung.Daten.Protokolleintrag
                {
                    Text = e.Message,
                    Typ = Anwendung.Daten.ProtokolleintragTyp.Warnung,
                    Zeitpunkt = DateTime.Now
                });
                return 2;
            }
            catch (Exception e)
            {
                this.OnVerbindungFehlgeschlagen(EventArgs.Empty);
                this.Kontext.Log.Hinzufügen(new Anwendung.Daten.Protokolleintrag
                {
                    Text = e.Message,
                    Typ = Anwendung.Daten.ProtokolleintragTyp.Warnung,
                    Zeitpunkt = DateTime.Now
                });
                return 2;
            }
        }

        /// <summary>
        /// Löscht eine AufgabenGruppe von dem angegebenen Benutzer.
        /// </summary>
        /// <param name="gruppe">Die Gruppe, die gelöscht werden soll.</param>
        /// <param name="schlüssel">Ein Einmalschlüssel (Sitzungsschlüssel), 
        /// der dem Benutzer bei der Anmeldung zugewiesen wird, 
        /// um die aktuelle Sitzung zu identifizieren.</param>
        /// <returns>
        /// 1, wenn das löschen erfolgreich war,
        /// 2, falls keine Verbindung zum Server möglich ist
        /// 0, wenn der Schlüssel ungültig ist,
        /// </returns>
        public async Task<int> GruppeLöschenAsync(System.Guid schlüssel, string gruppe)
        {
            var LöschenUrl =
                $"{this.BasisUrl}erpsystem/{schlüssel}/{gruppe}/löschen";

            try
            {
                using var Antwort = await this.HttpClient.PostAsync(LöschenUrl, null);
                // sobald eine Rückmeldung ist,
                // den Inhalt als Text (Json) lesen
                var JsonDaten = await Antwort?.Content?.ReadAsStringAsync()!;

                // Die Json Daten in unsere Objekte mappen
                return System.Text.Json.JsonSerializer
                    .Deserialize<int>(
                        JsonDaten, this.JsonOptionen)!;

            }
            catch (HttpRequestException e)
            {
                this.OnVerbindungFehlgeschlagen(EventArgs.Empty);
                this.Kontext.Log.Hinzufügen(new Anwendung.Daten.Protokolleintrag
                {
                    Text = e.Message,
                    Typ = Anwendung.Daten.ProtokolleintragTyp.Warnung,
                    Zeitpunkt = DateTime.Now
                });
                return 2;
            }
            catch (Exception e)
            {
                this.OnVerbindungFehlgeschlagen(EventArgs.Empty);
                this.Kontext.Log.Hinzufügen(new Anwendung.Daten.Protokolleintrag
                {
                    Text = e.Message,
                    Typ = Anwendung.Daten.ProtokolleintragTyp.Warnung,
                    Zeitpunkt = DateTime.Now
                });
                return 2;
            }
        }

        #endregion Lösche Daten

        #region Speichere Daten

        /// <summary>
        /// Aktualisiert die Aufgabe für den Benutzer in der Quelle durch den Webdienst.
        /// </summary>
        /// <param name="gruppe">Die Gruppe, die die Aufgabe enthält.</param>
        /// <param name="schlüssel">Ein Einmalschlüssel (Sitzungsschlüssel), 
        /// der dem Benutzer bei der Anmeldung zugewiesen wird, 
        /// um die aktuelle Sitzung zu identifizieren.</param>
        /// <param name="aufgabe">Die Aufgabe, die gespeichert werden soll.</param>
        /// <returns>
        /// 1, wenn das Speichern erfolgreich war,
        /// 2, falls keine Verbindung zum Server möglich ist,
        /// 0, wenn der Schlüssel ungültig ist.
        /// </returns>
        public async Task<int> SpeichereAufgabeAsync
            (string gruppe, System.Guid schlüssel, Aufgabe aufgabe)
        {
            var SpeicherUrl =
                      $"{this.BasisUrl}erpsystem/{schlüssel}/{gruppe}/{aufgabe.ID}/speichern";

            var daten =
               new StringContent(JsonSerializer.Serialize(aufgabe), Encoding.UTF8,
               "application/json");
            try
            {
                using var Antwort = await this.HttpClient.PostAsync(SpeicherUrl, daten);
                // sobald eine Rückmeldung ist,
                // den Inhalt als Text (Json) lesen
                var JsonDaten = await Antwort?.Content?.ReadAsStringAsync()!;

                // Die Json Daten in unsere Objekte mappen
                return System.Text.Json.JsonSerializer
                    .Deserialize<int>(
                        JsonDaten, this.JsonOptionen)!;
            }
            catch (HttpRequestException e)
            {
                this.OnVerbindungFehlgeschlagen(EventArgs.Empty);
                this.Kontext.Log.Hinzufügen(new     Anwendung.Daten.Protokolleintrag
                {
                    Text = e.Message,
                    Typ = Anwendung.Daten.ProtokolleintragTyp.Warnung,
                    Zeitpunkt = DateTime.Now
                });
                return 2;
            }
            catch (Exception e)
            {
                this.OnVerbindungFehlgeschlagen(EventArgs.Empty);
                this.Kontext.Log.Hinzufügen(new Anwendung.Daten.Protokolleintrag
                {
                    Text = e.Message,
                    Typ =   Anwendung.Daten.ProtokolleintragTyp.Warnung,
                    Zeitpunkt = DateTime.Now
                });
                return 2;
            }
        }

        /// <summary>
        /// Fügt eine AufgabenGruppe für den Benutzer in die Quelle.
        /// </summary>
        /// <param name="schlüssel">Ein Einmalschlüssel (Sitzungsschlüssel), 
        /// der dem Benutzer bei der Anmeldung zugewiesen wird, 
        /// um die aktuelle Sitzung zu identifizieren.</param>
        /// <param name="gruppe">Die Aufgaben-Gruppe, die gespeichert werden soll.</param>
        /// <returns>
        /// 1, wenn das Speichern erfolgreich war,
        /// 2, falls keine Verbindung zum Server möglich ist,
        /// 0, wenn der Schlüssel ungültig ist.
        /// </returns>
        public async Task<int> SpeichereGruppeAsync(System.Guid schlüssel, string gruppe)
        {
            var SpeicherUrl =
                $"{this.BasisUrl}erpsystem/{schlüssel}/{gruppe}/speichern";
            try
            {
                using var Antwort = await this.HttpClient.PostAsync(SpeicherUrl, null);
                // sobald eine Rückmeldung ist,
                // den Inhalt als Text (Json) lesen
                var JsonDaten = await Antwort?.Content?.ReadAsStringAsync()!;

                // Die Json Daten in unsere Objekte mappen
                return System.Text.Json.JsonSerializer
                    .Deserialize<int>(
                        JsonDaten, this.JsonOptionen)!;
            }
            catch (HttpRequestException e)
            {
                this.OnVerbindungFehlgeschlagen(EventArgs.Empty);
                this.Kontext.Log.Hinzufügen(new Anwendung.Daten.Protokolleintrag
                {
                    Text = e.Message,
                    Typ = Anwendung.Daten.ProtokolleintragTyp.Warnung,
                    Zeitpunkt = DateTime.Now
                });
                return 2;
            }
            catch (Exception e)
            {
                this.OnVerbindungFehlgeschlagen(EventArgs.Empty);
                this.Kontext.Log.Hinzufügen(new Anwendung.Daten.Protokolleintrag
                {
                    Text = e.Message,
                    Typ = Anwendung.Daten.ProtokolleintragTyp.Warnung,
                    Zeitpunkt = DateTime.Now
                });
                return 2;
            }
        }
        #endregion Speichere Daten

    }
}
