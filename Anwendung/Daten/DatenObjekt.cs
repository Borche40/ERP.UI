using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anwendung.Daten
{
    #region Zum Steuern vom DatenObjekt.ToString

    /// <summary>
    /// Kennzeichnet eine Eigenschaft eines
    /// DatenObjekts, dass der Inhalt im
    /// ToString Ergebnis aufgelistet werden soll
    /// </summary>
    public class InToStringAttribute : System.Attribute
    {
        /// <summary>
        /// Initialisiert ein neues
        /// InToStringAttribute
        /// </summary>
        public InToStringAttribute()
        {

        }

        /// <summary>
        /// Initialisiert ein neues
        /// InToStringAttribute
        /// </summary>
        /// <param name="position">Die Stelle
        /// für die Sortierung</param>
        public InToStringAttribute(int position)
        {
            this._Position = position;
        }



        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private int _Position = int.MaxValue;

        /// <summary>
        /// Ruft zum Sortieren die Stelle ab, 
        /// wo die Eigenschaft in ToString
        /// angeführt werden soll
        /// </summary>
        public int Position => this._Position;

    }

    #endregion Zum Steuern vom DatenObjekt.ToString

    #region DatenObjekt

    /// <summary>
    /// Stellt Basisfunktionalität
    /// für ein WIFI Datentransferobjekt bereit
    /// </summary>
    /// <remarks>Zum Steuern der ToString Methode
    /// die benötigten Inhalte mit InToStringAttribute
    /// kennzeichnen</remarks>
    public abstract class DatenObjekt : System.Object
    {

        #region Hilfsklassen

        /// <summary>
        /// Stellt eine typsichere Liste
        /// für Eigenschaften bereit, deren
        /// Inhalt im ToString benötigt wird
        /// </summary>
        private class InToStringEigenschaften
            : System.Collections.Generic.List<System.Reflection.PropertyInfo>
        {

        }

        /// <summary>
        /// Stellt eine typsichere Hashtable zum
        /// Sammeln von Typen mit Eigenschaften
        /// für das ToString() Ergebnis bereit
        /// </summary>
        private class TypenMitInToStringEigenschaften
            : System.Collections.Generic
                .Dictionary<string, InToStringEigenschaften>
        {

        }

        #endregion Hilfsklassen

        #region Analysebereich

        /// <summary>
        /// Internes singletone Feld für die Eigenschaft
        /// </summary>
        private static TypenMitInToStringEigenschaften _BekannteTypen = null!;

        /// <summary>
        /// Ruft die Hashtable mit bereits
        /// analysierten Typen für ToString() ab
        /// </summary>
        private static TypenMitInToStringEigenschaften BekannteTypen
        {
            get
            {
                if (DatenObjekt._BekannteTypen == null)
                {
                    DatenObjekt._BekannteTypen
                        = new TypenMitInToStringEigenschaften();
                }

                return DatenObjekt._BekannteTypen;
            }
        }

        /// <summary>
        /// Gibt die Liste der Eigenschaften
        /// vom aktuellen Objekt zurück,
        /// die mit InToStringAttribute
        /// markiert sind
        /// </summary>
        /// <remarks>Das Ergebnis ist mit
        /// Hilfe der Position Eigenschaft
        /// vom InToStringAttribute sortiert</remarks>
        private InToStringEigenschaften
            HoleEigenschaftenFürToString()
        {
            var Ergebnis = new InToStringEigenschaften();

            var ListeZumSortieren = new System.Collections.Generic
                .Dictionary<System.Reflection.PropertyInfo, int>();

            foreach (var Eigenschaft
                        in this.GetType().GetProperties())
            {
                var InToStringAttribute
                        = Eigenschaft.GetCustomAttributes(
                            typeof(InToStringAttribute),
                            inherit: true);

                if (InToStringAttribute.Length > 0)
                {
                    var Position = (InToStringAttribute[0]
                        as InToStringAttribute)!.Position;

                    ListeZumSortieren.Add(Eigenschaft, Position);
                }
            }

            Ergebnis.AddRange(from e in ListeZumSortieren 
                              orderby e.Value select e.Key);

            return Ergebnis;
        }

        #endregion Analysebereich

        /// <summary>
        /// Gibt einen Text zurück,
        /// der das aktuelle Datentransferobjekt beschreibt
        /// </summary>
        public override string ToString()
        {
            var TypSchlüssel = this.GetType().FullName!;

            #region Liste mit Eigenschaften für die Ausgabe

            InToStringEigenschaften EigenschaftenInAusgabe = null!;

            if (DatenObjekt.BekannteTypen.ContainsKey(TypSchlüssel))
            {
                EigenschaftenInAusgabe
                    = DatenObjekt.BekannteTypen[TypSchlüssel];
            }
            else
            {
                EigenschaftenInAusgabe = this.HoleEigenschaftenFürToString();

                //Wird diese ToString() selbst debugged,
                //ruft der Debugger diese Methode vorher auf
                //deshalb prüfen wir, ob der Typ bereits bekannt ist
#if DEBUG
                if (!DatenObjekt.BekannteTypen
                        .ContainsKey(TypSchlüssel))
                {
                    DatenObjekt.BekannteTypen
                        .Add(TypSchlüssel, EigenschaftenInAusgabe);
                }
#else
                DatenObjekt.BekannteTypen
                    .Add(TypSchlüssel, EigenschaftenInAusgabe);
#endif
            }

            #endregion Liste mit Eigenschaften für die Ausgabe

            #region Hilfsmethode für das Ergebnis
            string BaueErgebnis()
            {
                var Ergebnis = $"{this.GetType().Name}(";

                foreach (var e in EigenschaftenInAusgabe)
                {
                    Ergebnis += $"{e.Name}=";

                    var Wert = e.GetValue(this);

                    if (Wert is string)
                    {
                        Ergebnis += $"\"{Wert}\", ";
                    }
                    else
                    {
                        Ergebnis += $"{Wert}, ";
                    }
                }

                return $"{Ergebnis.TrimEnd(',', ' ')})";
            }

            #endregion Hilfsmethode für das Ergebnis

            if (EigenschaftenInAusgabe.Count == 0)
            {
                // Standard Ergebnis von ToString()
                return base.ToString()!;
            }
            else
            {
                // Nur den Klassename inkl.
                // den Eigenschaften und Werten
                return BaueErgebnis();
            }
        }
    }

    #endregion DatenObjekt
}
