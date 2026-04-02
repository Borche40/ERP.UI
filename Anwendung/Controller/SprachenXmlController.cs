using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anwendung.Controller
{
    /// <summary>
    /// Stellt einen Dienst bereit zum
    /// Lesen und Schreiben von Sprachen
    /// im Xml-Format
    /// </summary>
    internal class SprachenXmlController 
        : XmlController<Daten.Sprachen>
    {
        /// <summary>
        /// Gibt die Liste mit den unterstützten 
        /// Sprachen aus den Assembly-Ressourcen zurück
        /// </summary>
        /// <exception cref="System.Exception">Tritt auf,
        /// wenn beim Mappen der Ressource ein Problem vorliegt</exception>
        public Anwendung. Daten.Sprachen HoleAusRessourcen()
        {
            // Den Ressourcen Sprachentext
            // in ein .Net XmlDocument laden
            var Xml = new System.Xml.XmlDocument();
            Xml.LoadXml(Anwendung.Properties
                .Resources.Sprachen);

            // Eine Ergebnisliste initialisieren
            var Sprachen = new Daten.Sprachen();

            // Im Wurzelelement alle
            // gefunden Sprachen in ein Sprache
            // Objekt mappen und in die Ergebnisliste
            // einfüllen
            foreach (System.Xml.XmlNode s
                in Xml.DocumentElement!.ChildNodes)
            {
                Sprachen.Add(new Daten.Sprache
                {
                    Code = s.Attributes!["code"]!.Value,
                    Name = s.Attributes["name"]!.Value
                });
            }

            // Ergebnisliste zurückgeben
            return Sprachen;
        }

    }
}
