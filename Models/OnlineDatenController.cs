using ERP.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using ERP.UI.ViewModel;

namespace ERP.UI.Models
{
    /// <summary>
    /// Stellt einen Dienst zur Kommunikation
    /// mit dem Gateway REST Webdienst bereit
    /// </summary>
    internal class OnlineDatenController : OnlineController, IDatenController
    {
        #region Benutzerverwaltung

        /// <summary>
        /// Meldet den Benutzer mit der angegebenen Anmeldedaten.
        /// </summary>
        /// <param name="anmeldung">Die Daten die für eine Anmeldung benötigt sind.</param>
        /// <returns>Das Benutzerobjekt,
        /// falls die Anmeldung erfolgreich war, andernfalls null.</returns>
        public async Task<ERP.Data.Models.Benutzer?> Anmelden(ERP.Data.Anmeldung anmeldung)
        {
            var AnmeldenUrl = $"{this.BasisUrl}erpsystem/anmelden";

            var anmeldeDaten = new StringContent(
                                 JsonSerializer.Serialize(anmeldung),
                                 Encoding.UTF8,
                                 "application/json");
            try
            {
                using var Antwort = await this.HttpClient.PostAsync(AnmeldenUrl, anmeldeDaten);

                var JsonDaten = await Antwort?.Content?.ReadAsStringAsync()!;

                return System.Text.Json.JsonSerializer
                    .Deserialize<ERP.Data.Models.Benutzer?>(
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
                return null;
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
                return null;
            }
        }

        /// <summary>
        /// Registriert einen neuen Benutzer mit den angegebenen Daten.
        /// </summary>
        /// <param name="registrierung">Das Objekt mit den Registrierungsdaten.</param>
        /// <returns>Die Ergebniscode der Registrierung, 
        /// 1 wenn die Registrierung erfolgreich ist oder 0 falls nicht.</returns>
        public async Task<int> Registrieren(ERP.Data.Registrierung registrierung)
        {
            var RegistriereUrl = $"{this.BasisUrl}erpsystem/registrieren";

            var registrierungsDaten = new StringContent(
                JsonSerializer.Serialize(registrierung),
                Encoding.UTF8,
                "application/json"
            );
            try
            {


                using var Antwort = await this.HttpClient.PostAsync(RegistriereUrl, registrierungsDaten);

                var JsonDaten = await Antwort?.Content?.ReadAsStringAsync()!;

                return System.Text.Json.JsonSerializer
                    .Deserialize<int>(
                        JsonDaten, this.JsonOptionen)!;
            }
            catch (HttpRequestException e)
            {
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
                this.Kontext.Log.Hinzufügen(new Anwendung.Daten.Protokolleintrag
                {
                    Text = e.Message,
                    Typ = Anwendung.Daten.ProtokolleintragTyp.Warnung,
                    Zeitpunkt = DateTime.Now
                });
                return 2;
            }
        }

        #endregion Benutzerverwaltung

        #region Hole Daten

        /// <summary>
        /// Gibt die Aufgaben für den Benutzer aus dem Webdienst zurück.
        /// </summary>
        /// <param name="benutzer">Das Benutzerobjekt zur Authentifizierung.</param>
        /// <param name="gruppe">Die Gruppe, für die die Aufgaben geholt werden sollen.</param>
        public async Task<Aufgaben?> HoleAufgabenAsync(ERP.Data.Models.Benutzer benutzer, string gruppe)
        {
            // Adresse vom REST Api
            var AufgabenUrl =
                $"{this.BasisUrl}erpsystem/{benutzer.Schlüssel}/{gruppe}/aufgaben/";
            try
            {
                // Das http-Get absetzen
                using var Antwort = await this.HttpClient.GetAsync(AufgabenUrl);
                // sobald eine Rückmeldung ist,
                // den Inhalt als Text (Json) lesen
                var JsonDaten = await Antwort?.Content?.ReadAsStringAsync()!;

                // Die Json Daten in unsere Objekte mappen
                return System.Text.Json.JsonSerializer
                    .Deserialize<ERP.Data.Aufgaben>(
                        JsonDaten, this.JsonOptionen)!;
            }
            catch (HttpRequestException e)
            {

                this.Kontext.Log.Hinzufügen(new Anwendung.Daten.Protokolleintrag
                {
                    Text = e.Message,
                    Typ = Anwendung.Daten.ProtokolleintragTyp.Warnung,
                    Zeitpunkt = DateTime.Now
                });
                return null!;
            }
            catch (Exception e)
            {

                this.Kontext.Log.Hinzufügen(new Anwendung.Daten.Protokolleintrag
                {
                    Text = e.Message,
                    Typ = Anwendung.Daten.ProtokolleintragTyp.Warnung,
                    Zeitpunkt = DateTime.Now
                });
                return null!;
            }
        }

        /// <summary>
        /// Gibt die AufgabenGruppen für den Benutzer aus dem Webdienst zurück.
        /// </summary>
        /// <param name="benutzer">Das Benutzerobjekt zur Authentifizierung.</param>
        public async Task<ERP.Data.AufgabenGruppen?>
            HoleAufgabenGruppenAsync(ERP.Data.Models. Benutzer benutzer)
        {
            // Adresse vom REST Api
            var AufgabenUrl =
                $"{this.BasisUrl}erpsystem/{benutzer.Schlüssel}/aufgabengruppen";
            try
            {
                // Das http-Get absetzen
                using var Antwort = await this.HttpClient.GetAsync(AufgabenUrl);
                // sobald eine Rückmeldung ist,
                // den Inhalt als Text (Json) lesen
                var JsonDaten = await Antwort?.Content?.ReadAsStringAsync()!;

                // Die Json Daten in unsere Objekte mappen
                return System.Text.Json.JsonSerializer
                    .Deserialize<ERP.Data.AufgabenGruppen>(
                        JsonDaten, this.JsonOptionen)!;
            }
            catch (HttpRequestException e)
            {

                this.Kontext.Log.Hinzufügen(new Anwendung.Daten.Protokolleintrag
                {
                    Text = e.Message,
                    Typ = Anwendung.Daten.ProtokolleintragTyp.Warnung,
                    Zeitpunkt = DateTime.Now
                });
                return null!;
            }
            catch (Exception e)
            {
                this.Kontext.Log.Hinzufügen(new Anwendung.Daten.Protokolleintrag
                {
                    Text = e.Message,
                    Typ = Anwendung.Daten.ProtokolleintragTyp.Warnung,
                    Zeitpunkt = DateTime.Now
                });
                return null!;
            }
        }
        #endregion Hole Daten

    }
}
