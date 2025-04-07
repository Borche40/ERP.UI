using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ERP.UI.ViewModel
{
    /// <Zusammenfassung>
    /// Stellt ein benutzerdefiniertes MessageBox-Dienstprogramm dar,
    /// um Nachrichten mit verschiedenen Optionen für die 
    /// Schaltflächenanordnung anzuzeigen (z. B. Ok, Ja/Nein).
    /// </Zusammenfassung>
    public enum NachrichtTyp
    {
        /// <Zusammenfassung>
        /// Gibt einen MessageBox mit nur der Schaltfläche "Ok" an.
        /// </Zusammenfassung>
        Ok,

        /// <Zusammenfassung>
        /// Gibt einen MessageBox mit den Schaltflächen "Ja" und "Nein" an.
        /// </Zusammenfassung>
        JaNein
    }


    /// <summary>
    /// Provides static methods for displaying custom message boxes
    /// with configurable button layouts and message content.
    /// </summary>
    internal static class NachrichtBox
    {
        #region Fenstersteuerung

        /// <summary>
        /// Der Name des Fensters
        /// </summary>
        private static string FensterName { get; set; } = "NachrichtBox";

        /// <summary>
        /// Zum wechseln zwischen Ok und JaNein View
        /// </summary>
        public static System.Windows.Visibility OkFenster
        {
            get
            {
                if (NachrichtBox.IstJaNeinNachricht == false)
                {
                    return System.Windows.Visibility.Visible;
                }
                else
                {
                    return System.Windows.Visibility.Collapsed;
                }
            }
        }

        /// <summary>
        /// Öffnet das angegebene Fenster.
        /// </summary>
        /// <typeparam name="T">Der Fenstertyp.</typeparam>
        /// <param name="fenster">Das Fenster, das als Besitzer des erstellten Fensters verwendet wird.</param>
        public static void Anzeigen<T>(System.Windows.Window fenster) where T :
            System.Windows.Window, new()
        {

            var Fenster = new T();
            Fenster.Name = NachrichtBox.FensterName;

            // Setze den Besitzer auf das aufrufende Fenster für eine einfache Steuerung
            Fenster.Owner = fenster;
            if (fenster.WindowState == WindowState.Maximized)
            {

                Fenster.Left = 0;
                Fenster.Top = 0;
                Fenster.Height = System.Windows.SystemParameters.PrimaryScreenHeight;
                Fenster.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
            }
            else
            {
                Fenster.Left = fenster.Left;
                Fenster.Top = fenster.Top;
                Fenster.Height = fenster.Height;
                Fenster.Width = fenster.Width;
            }


            // Setze die ShowInTaskbar-Eigenschaft auf false
            Fenster.ShowInTaskbar = false;
            Fenster.ShowDialog();
        }

        /// <summary>
        /// Schließt das Fenster.
        /// </summary>
        public static void FensterSchließen()
        {
            System.Windows.Application.Current.Windows.OfType<System.Windows.Window>().
                FirstOrDefault(w => string.Compare(w.Name, NachrichtBox.FensterName) == 0)!.Close();

        }

        #endregion Fenstersteuerung

        #region Daten

        /// <summary>
        /// Ruft das Ergebnis der MessageBox ab oder legt es fest.
        /// </summary>
        private static MessageBoxResult Ergebnis { get; set; }

        /// <summary>
        /// Ruft den Typ der Nachricht ab oder legt ihn fest.
        /// </summary>
        private static NachrichtTyp NachrichtTyp { get; set; }

        /// <summary>
        /// Internes Feld
        /// </summary>
        private static string _Nachricht = string.Empty;

        /// <summary>
        /// Ruft den Nachrichteninhalt ab oder legt ihn fest, 
        /// der in der MessageBox angezeigt werden soll.
        /// </summary>
        public static string Nachricht
        {
            get => NachrichtBox._Nachricht;

            set => NachrichtBox._Nachricht = value;
        }

        /// <summary>
        /// Ruft einen Wert ab, der angibt, ob die MessageBox über ein 
        /// Ja/Nein-Button-Layout verfügt, oder legt diesen fest.
        /// </summary>
        public static bool IstJaNeinNachricht { get; set; } = false;

        #endregion Daten

        #region Zum Anzeigen

        /// <summary>
        /// Zeigt eine benutzerdefinierte MessageBox mit dem 
        /// angegebenen Nachrichtentext und dem Button-Layout an.
        /// </summary>
        /// <param name="text">Der anzuzeigende Nachrichtentext.</param>
        /// <param name="typ">Der Typ des Button-Layouts für die MessageBox.</param>
        /// <param name="besitzer">Das Fenster, das die MessageBox anzeigen soll.</param>
        /// <returns>Das Ergebnis der Benutzerinteraktion mit der MessageBox.</returns>
        public static MessageBoxResult Anzeigen(string text, NachrichtTyp typ, System.Windows.Window besitzer = null!)
        {
            // Stellt sicher, dass UI-Operationen auf dem UI-Thread ausgeführt werden
            if (System.Windows.Application.Current.Dispatcher.CheckAccess())
            {
                // Wir befinden uns auf dem UI-Thread, daher können wir fortfahren
                AnzeigenInternal(text, typ, besitzer);
            }
            else
            {
                // Wir befinden uns nicht auf dem UI-Thread, daher rufen wir die Methode im UI-Thread auf
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    AnzeigenInternal(text, typ, besitzer);
                });
            }

            return NachrichtBox.Ergebnis;
        }

        /// <summary>
        /// Interne Methode zum Anzeigen der MessageBox.
        /// </summary>
        /// <param name="text">Der anzuzeigende Nachrichtentext.</param>
        /// <param name="typ">Der Typ des Button-Layouts für die MessageBox.</param>
        /// <param name="besitzer">Das Fenster, das die MessageBox anzeigen soll.</param>
        private static void AnzeigenInternal(string text, NachrichtTyp typ, System.Windows.Window besitzer)
        {
            // Ihre vorhandene Methodenlogik hier
            NachrichtBox.Nachricht = text;
            NachrichtBox.NachrichtTyp = typ;

            if (NachrichtBox.NachrichtTyp == NachrichtTyp.JaNein)
            {
                NachrichtBox.IstJaNeinNachricht = true;
            }
            else
            {
                NachrichtBox.IstJaNeinNachricht = false;
            }

            if (besitzer is System.Windows.Window fenster)
            {
                NachrichtBox.DialogÖffnen(fenster);
            }
            else
            {
                var hauptFenster = System.Windows.Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
                NachrichtBox.DialogÖffnen(hauptFenster!);
            }
        }



        /// <summary>
        /// Öffnet die MessageBox.
        /// </summary>
        /// <param name="p">Das System.Windows.Window, in dem die MessageBox angezeigt werden soll.</param>
        private static void DialogÖffnen(object p)
        {
            if (p is System.Windows.Window aufrufendesFenster)
            {
                    NachrichtBox.Anzeigen<Views.NachrichtBoxView>(aufrufendesFenster);
            }
        }

        #endregion Zum Anzeigen

        #region Antworten

        /// <summary>
        /// Internes Feld
        /// </summary>
        private static ERP.UI.Commands.Befehl _Ja = null!;

        /// <summary>
        /// Ruft den Befehl zum Behandeln des Klickereignisses der Ja-Schaltfläche in der MessageBox ab.
        /// </summary>
        public static ERP.UI.Commands.Befehl Ja
        {
            get
            {
                if (NachrichtBox._Ja == null)
                {
                    NachrichtBox._Ja = new ERP.UI.Commands.Befehl(d =>
                    {
                        NachrichtBox.Ergebnis = MessageBoxResult.Yes;
                        NachrichtBox.FensterSchließen();
                    });
                }
                return NachrichtBox._Ja;
            }
        }

        /// <summary>
        /// Internes Feld
        /// </summary>
        private static ERP.UI.Commands.Befehl _Nein = null!;

        /// <summary>
        /// Ruft den Befehl zum Behandeln des Klickereignisses der Nein-Schaltfläche in der MessageBox ab.
        /// </summary>
        public static ERP.UI.Commands.Befehl Nein
        {
            get
            {
                if (NachrichtBox._Nein == null)
                {
                    NachrichtBox._Nein = new ERP.UI.Commands.Befehl(d =>
                    {
                        NachrichtBox.Ergebnis = MessageBoxResult.No;
                        NachrichtBox.FensterSchließen();
                    });
                }
                return NachrichtBox._Nein;
            }
        }

        /// <summary>
        /// Internes Feld
        /// </summary>
        private static ERP.UI.Commands.Befehl _Abbrechen = null!;

        /// <summary>
        /// Ruft den Befehl zum Behandeln des Klickereignisses der Abbrechen-Schaltfläche in der MessageBox ab.
        /// </summary>
        public static ERP.UI.Commands.Befehl Abbrechen
        {
            get
            {
                if (NachrichtBox._Abbrechen == null)
                {
                    NachrichtBox._Abbrechen = new ERP.UI.Commands.Befehl(d =>
                    {
                        NachrichtBox.Ergebnis = MessageBoxResult.Cancel;
                        NachrichtBox.FensterSchließen();
                    });
                }
                return NachrichtBox._Abbrechen;
            }
        }

        /// <summary>
        /// Internes Feld
        /// </summary>
        private static ERP.UI.Commands.Befehl _Ok = null!;

        /// <summary>
        /// Ruft den Befehl zum Behandeln des Klickereignisses der OK-Schaltfläche in der MessageBox ab.
        /// </summary>
        public static ERP.UI.Commands.Befehl Ok
        {
            get
            {
                if (NachrichtBox._Ok == null)
                {
                    NachrichtBox._Ok = new ERP.UI.Commands.Befehl(d =>
                    {
                        NachrichtBox.Ergebnis = MessageBoxResult.OK;
                        NachrichtBox.FensterSchließen();
                    });
                }
                return NachrichtBox._Ok;
            }
        }

        #endregion Antworten


    }
}
