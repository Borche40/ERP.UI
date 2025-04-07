using ERP.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ERP.UI.Models
{
    /// <summary>
    /// Stellt einen Dienst zum überprüfen der Verbindung zum
    /// Gateway REST Webdienst bereit
    /// </summary>
    internal class ServerVerbindungController : OnlineController
    {
        private static HttpClient? _HttpClient;

        protected new System.Net.Http.HttpClient HttpClient
        {
            get
            {
                if (_HttpClient == null)
                {
                    if (this.Kontext == null)
                    {
                        throw new InvalidOperationException("Kontext ist nicht initialisiert.");
                    }

                    _HttpClient = new System.Net.Http.HttpClient();
                    this.Kontext.Log.Hinzufügen($"{this} hat das Objekt für Internetzugriffe initialisiert...");
                    _HttpClient.DefaultRequestHeaders.Add("Accept-Language", this.Kontext.Sprachen.AktuelleSprache.Code);
                    this.Kontext.Log.Hinzufügen($"Accept-Language Header auf \"{_HttpClient.DefaultRequestHeaders.AcceptLanguage}\" festgelegt.");
                }

                return _HttpClient;
            }
        }
        /// <summary>
        /// Überprüft die Verbindung zum Server.
        /// </summary>
        /// <returns>1, wenn die Verbindung erfolgreich ist; andernfalls 0.</returns>
        public async Task<int> VerbindungÜberprüfen()
        {
            // Adresse vom REST Api
            var Url =
                $"{this.BasisUrl}ERP.UI";
            try
            {
                // Das http-Get absetzen
                using var Antwort = await this.HttpClient.GetAsync(Url);

                return 1;
            }
            catch (HttpRequestException)
            {
                return 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}
