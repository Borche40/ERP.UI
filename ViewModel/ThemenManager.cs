using ERP.UI.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.UI.ViewModel
{
    /// <summary>
    /// Stellt einen Dienst zur Verwaltung des Anwendungsdesigns bereit
    /// </summary>
   public class ThemenManager:BaseViewModel
    {
        #region Themenlogik
        /// <summary>
        /// Aktuelles Theme abrufen (URI) oder null, falls nicht vorhanden
        /// </summary>
        public static Uri AktuellesThemeAbrufen()
        {
            // Letztes hinzugefügtes Theme-Wörterbuch abrufen
            var aktuellesThemeWörterbuch = System.Windows.Application.Current.Resources.MergedDictionaries.LastOrDefault();

            // Prüfen, ob ein Theme-Wörterbuch gefunden wurde
            if (aktuellesThemeWörterbuch != null)
            {
                // URI der Quelle des Theme-Wörterbuchs zurückgeben
                return aktuellesThemeWörterbuch.Source;
            }
            else
            {
                // Wenn kein Theme-Wörterbuch gefunden wird, null
                // zurückgeben oder eine Ausnahme auslösen, abhängig von Ihren Anforderungen
                return null!;
            }
        }

        /// <summary>
        /// Wendet das angegebene Theme-Wörterbuch auf andere Ressourcenwörterbücher an.
        /// </summary>
        /// <param name="themeDictionary">Das anzuwendende Theme-Wörterbuch.</param>
        private void AnderesThemeAnwenden(System.Windows.ResourceDictionary themeDictionary)
        {
            // Theme-Wörterbuch mit jedem zusätzlichen Ressourcenwörterbuch zusammenführen
            foreach (var resourceDictionary in System.Windows.Application.Current.Resources.MergedDictionaries)
            {
                if (resourceDictionary != themeDictionary)
                {
                    // Theme-Wörterbuch zusammenführen
                    resourceDictionary.MergedDictionaries.Add(themeDictionary);
                }
            }
        }

        /// <summary>
        /// Ändert die Farbvorlage der App
        /// </summary>
        /// <param name="theme">URI des Themes</param>
        public void ThemeÄndern(Uri theme)
        {
            // Neues Theme mit dem Theme aus der bereitgestellten Uri erstellen
            System.Windows.ResourceDictionary Theme = new System.Windows.ResourceDictionary()
            {
                Source = theme
            };

            // Neues Theme festlegen
            System.Windows.Application.Current.Resources.MergedDictionaries.Add(Theme);

            // Theme auf andere Ressourcenwörterbücher anwenden
            AnderesThemeAnwenden(Theme);
        }
        #endregion Themenlogik

        #region Themenwechsel
        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private ERP.UI.Commands.Befehl _HellesThemaAnwenden = null!;

        /// <summary>
        /// Ruft den Befehl zum wechseln auf die Helle Thema
        /// </summary>
        public ERP.UI.Commands.Befehl HellesThemaAnwenden
        {
            get
            {
                if (this._HellesThemaAnwenden == null)
                {
                    this._HellesThemaAnwenden = new ERP.UI.Commands.Befehl(d =>
                    { ThemeÄndern(new Uri(Properties.Settings.Default.LightTheme, UriKind.Relative)); });
                }
                return this._HellesThemaAnwenden;
            }
        }

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private ERP.UI.Commands.Befehl _DunklesThemaAnwenden = null!;

        /// <summary>
        /// Ruft den Befehl zum wechseln auf die Dunkele Thema
        /// </summary>
        public ERP.UI.Commands.Befehl DunklesThemaAnwenden
        {
            get
            {
                if (this._DunklesThemaAnwenden == null)
                {
                    this._DunklesThemaAnwenden = new ERP.UI.Commands.Befehl(d =>
                    { ThemeÄndern(new Uri(Properties.Settings.Default.DarkTheme, UriKind.Relative)); });
                }
                return this._DunklesThemaAnwenden;
            }
        }
        #endregion Themenwechsel

    }
}

