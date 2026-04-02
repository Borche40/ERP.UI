using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anwendung.Werkzeuge
{
    /// <summary>
    /// Stellt Erweiterungsmethoden zum Abrufen
    /// vom Firmennamen, des Produktnamens
    /// und anderer Metadaten bereit
    /// </summary>
    public static class AssemblyInfo : System.Object
    {
        #region Firmenname ...

        /// <summary>
        /// Gibt aus der Assembly, die
        /// zum Starten der Anwendung benutzt wurde,
        /// die Einstellung von AssemblyCompanyAttribute
        /// zurück.
        /// </summary>
        /// <param name="quelle">Verweis auf das
        /// Objekt, für das die Information benötigt wird</param>
        /// <remarks>Gibt die Einstellung zurück
        /// oder einen leeren Text, wenn das Attribut
        /// nicht eingestellt ist</remarks>
        public static string HoleFirmenname(this object quelle)
        {
            object[] Einstellungen
                = System.Reflection.Assembly.GetEntryAssembly()!
                    .GetCustomAttributes(
                        typeof(System.Reflection
                            .AssemblyCompanyAttribute),
                        inherit: true);

            if (Einstellungen.Length > 0)
            {
                return
                    (Einstellungen[0]
                        as System.Reflection
                            .AssemblyCompanyAttribute)!.Company;
            }

            return string.Empty;
        }

        #endregion Firmenname ...

        #region Produktname ...

        /// <summary>
        /// Gibt aus der Assembly, die
        /// zum Starten der Anwendung benutzt wurde,
        /// die Einstellung von AssemblyProductAttribute
        /// zurück.
        /// </summary>
        /// <param name="quelle">Verweis auf das
        /// Objekt, für das die Information benötigt wird</param>
        /// <remarks>Gibt die Einstellung zurück
        /// oder einen leeren Text, wenn das Attribut
        /// nicht eingestellt ist</remarks>
        public static string HoleProduktname(this object quelle)
        {
            object[] Einstellungen
                = System.Reflection.Assembly.GetEntryAssembly()!
                    .GetCustomAttributes(
                        typeof(System.Reflection
                            .AssemblyProductAttribute),
                        inherit: true);

            if (Einstellungen.Length > 0)
            {
                return
                    (Einstellungen[0]
                        as System.Reflection
                            .AssemblyProductAttribute)!.Product;
            }

            return string.Empty;
        }

        #endregion Produktname ...

        #region Assembly Version

        /// <summary>
        /// Gibt aus der Assembly, die
        /// zum Starten der Anwendung benutzt wurde,
        /// die Versionseinstellung zurück.
        /// </summary>
        /// <param name="quelle">Verweis auf das
        /// Objekt, für das die Information benötigt wird</param>
        /// <remarks>Gibt die Einstellung Hauptnummer.Subnummer zurück
        /// oder einen leeren Text, wenn das Attribut
        /// nicht eingestellt ist</remarks>
        public static string HoleVersion(this object quelle)
        {
            var Version = System.Reflection.Assembly
                            .GetEntryAssembly()!
                            .GetName().Version!;

            return $"{Version.Major}.{Version.Minor}";
        }

        #endregion Assembly Version

        #region Copyright...

        /// <summary>
        /// Gibt aus der Assembly, die
        /// zum Starten der Anwendung benutzt wurde,
        /// die Einstellung von AssemblyCopyrightAttribute 
        /// zurück.
        /// </summary>
        /// <param name="quelle">Verweis auf das
        /// Objekt, für das die Information benötigt wird</param>
        /// <remarks>Gibt die Einstellung zurück
        /// oder einen leeren Text, wenn das Attribut
        /// nicht eingestellt ist</remarks>
        public static string HoleCopyright(this object quelle)
        {
            object[] Einstellungen
                = System.Reflection.Assembly.GetEntryAssembly()!
                    .GetCustomAttributes(
                        typeof(System.Reflection
                            .AssemblyCopyrightAttribute),
                        inherit: true);

            if (Einstellungen.Length > 0)
            {
                return
                    (Einstellungen[0]
                        as System.Reflection
                            .AssemblyCopyrightAttribute)!.Copyright;
            }

            return string.Empty;
        }

        #endregion Copyright...


    }
}
