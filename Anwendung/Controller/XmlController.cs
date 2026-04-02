using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anwendung.Controller
{
    /// <summary>
    /// Stellt einen Dienst zum
    /// Serialisieren im Xml Format bereit
    /// </summary>
    /// <remarks>Beim Benutzen &lt;T&gt; durch
    /// den benötigten &lt;Listentyp&gt; ersetzen</remarks>
    public class XmlController<T> 
        : Anwendung.AppObjekt 
        where T : class
    {
        /// <summary>
        /// Serialisiert die Liste
        /// im Xml Format in die Datei
        /// </summary>
        /// <param name="pfad">Vollständige Pfadangabe
        /// zur benutzten Datei</param>
        /// <param name="daten">Eine Liste mit
        /// den zu serialisierenden Informationen</param>
        /// <exception cref="System.Exception">Tritt auf,
        /// wenn beim Serialisieren ein Fehler aufgetreten ist</exception>
        public void Speichern(
                        string pfad,
                        T daten)
        {
            using var Schreiber
                = new System.IO.StreamWriter(
                    pfad,
                    append: false,
                    System.Text.Encoding.UTF8);

            var Serialisierer
                = new System.Xml.Serialization.XmlSerializer(
                        daten!.GetType());

            Serialisierer.Serialize(Schreiber, daten);
        }

        /// <summary>
        /// Gibt die Liste mit den deserialisierten
        /// Informationen aus der Datei zurück
        /// </summary>
        /// <param name="pfad">Vollständige Pfadangabe
        /// zur Xml Datei mit den Informationen</param>
        /// <exception cref="System.Exception">Tritt auf,
        /// wenn beim Deserialisieren ein Fehler aufgetreten ist</exception>
        public T Lesen(string pfad)
        {
            using var Leser 
                = new System.IO.StreamReader(
                    pfad, 
                    System.Text.Encoding.UTF8);

            //20231214  Hr. Gnese - Fehlerbehebung
            /*
            var Serialisierer 
                = new System.Xml.Serialization.XmlSerializer(
                    typeof(Daten.FensterInfos));
            */
            var Serialisierer
                = new System.Xml.Serialization.XmlSerializer(
                    typeof(T));

            return (Serialisierer.Deserialize(Leser) 
                as T)!;
        }
    }
}
