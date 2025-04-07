using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ERP.UI.Commands
{
    /// <summary>
    /// Basisklasse für alle ViewModels.
    /// Implementiert das INotifyPropertyChanged Interface
    /// </summary>
    public class BaseViewModel:Anwendung.AppObjekt,
        System.ComponentModel . INotifyPropertyChanged
    {



        public BaseViewModel()
        {
            
        }




        #region PropertyChanged

        /// <summary>
        /// Wird ausgelöst, wenn sich der
        /// Inhalt einer Eigenschaft ändert,
        /// damit die Datenbindung informiert wird
        /// </summary>
        /// <remarks>Es genügt nicht, dieses
        /// Ereignis zu implementieren. WPF prüft
        /// nur, ob INotifyPropertyChanged 
        /// implementiert ist</remarks>
        public event PropertyChangedEventHandler?
                        PropertyChanged = null;

        /// <summary>
        /// Löst das Ereignis PropertyChanged aus
        /// </summary>
        /// <param name="e">Ereignisdaten mit
        /// dem Namen der geänderten Eigenschaft</param>
        protected virtual void OnPropertyChanged(
                    PropertyChangedEventArgs e)
        {
            var BehandlerKopie = this.PropertyChanged;

            if (BehandlerKopie != null)
            {
                BehandlerKopie(this, e);
            }
        }

        /// <summary>
        /// Löst das Ereignis PropertyChanged aus
        /// </summary>
        /// <param name="name">Die Bezeichnung der
        /// geänderten Eigenschaft</param>
        /// <remarks>Wird der Name nicht angegeben,
        /// wird automatisch der Name vom Aufrufer
        /// eingesetzt</remarks>
        protected virtual void OnPropertyChanged(
            [System.Runtime.CompilerServices.CallerMemberName]
            string name = null!)
        {
            this.OnPropertyChanged(
                new PropertyChangedEventArgs(name));
        }

        #endregion PropertyChanged

        #region Beschäftigt-Zustand

        /// <summary>
        /// Ruft ab, wie oft IstBeschäftigt
        /// eingeschaltet wurde, oder legt
        /// dieses fest
        /// </summary>
        private int IstBeschäftigtZähler { get; set; } = 0;

        /// <summary>
        /// Ruft einen Wahrheitswert ab,
        /// der bestimmt, ob die Anwendung
        /// gerade eine Arbeit ausführt 
        /// oder nicht und legt dieses fest
        /// </summary>
        public bool IstBeschäftigt
        {
            get => this.IstBeschäftigtZähler > 0;
            set
            {
                if (value)
                {
                    this.IstBeschäftigtZähler++;
                }
                else
                {
                    this.IstBeschäftigtZähler--;
                    // Sollte zu oft IstBeschäftigt = false
                    // benutzt wurden sein...
                    if (this.IstBeschäftigtZähler < 0)
                        this.IstBeschäftigtZähler = 0;
                }

                this.OnPropertyChanged();
            }
        }


        #endregion Beschäftigt-Zustand
        #region Fensterpositionen verwalten

        /// <summary>
        /// Übergibt die Fenster-Informationen
        /// an den FensterManager der Infrastruktur
        /// </summary>
        /// <param name="fenster">Ein WPF Window Objekt</param>
        /// <remarks>Als Schlüssel wird die Eigenschaft Name
        /// benutzt. Die Positionsdaten Links,
        /// Oben, Breite und Höhe werden nur benutzt,
        /// wenn das Fenster im Normalzustand, weil
        /// sonst die Daten ungültig sind</remarks>
        protected void ZustandHinterlegen(
            System.Windows.Window fenster)
        {
            // Für das Fenster ein Info Objekt
            // mit dem Namen als Schlüssel
            var Info = new Anwendung.Daten.FensterInfo
            {
                Name = fenster.Name
            };

            // Den Zustand auf alle Fälle merken
            Info.Zustand = (int)fenster.WindowState;

            // Die Positionsdaten nur,
            // wenn wir im Normal Zustand sind
            if (fenster.WindowState
                    == System.Windows.WindowState.Normal)
            {
                Info.Links = (int)fenster.Left;
                Info.Oben = (int)fenster.Top;
                Info.Breite = (int)fenster.Width;
                Info.Höhe = (int)fenster.Height;
            }

            // Das Info Objekt an den
            // FensterManager der Infrastruktur
            // übergeben
            this.Kontext.Fenster.Hinterlegen(Info);
        }

        /// <summary>
        /// Stellt mit den in der Infrastruktur 
        /// hinterlegen Fensterdaten den
        /// alten Zustand und Position wiederher
        /// </summary>
        /// <param name="fenster">Ein WPF Window Objekt</param>
        /// <remarks>Zum Finden wird die Eigenschaft Name
        /// benutzt. Als Fensterzustand wird nur Maximiert
        /// wiederhergestellt. Minimiert wird als Normal
        /// betrachtet, weil das Benutzer übersehen</remarks>
        protected void ZustandWiederherstellen(
            System.Windows.Window fenster)
        {
            var AlterZustand = this.Kontext
                .Fenster.Hole(fenster.Name);

            if (AlterZustand != null)
            {
                // Positions- und Größenangaben
                // nur wiederherstellen, wenn
                // welche vorhanden sind
                fenster.Left = AlterZustand.Links ?? fenster.Left;
                fenster.Top = AlterZustand.Oben ?? fenster.Top;
                fenster.Width = AlterZustand.Breite ?? fenster.Width;
                fenster.Height = AlterZustand.Höhe ?? fenster.Height;

                // Als Zustand nur Maximiert,
                // Minimiert wird als Normal
                // seit Window 95 betrachtet,
                // weil minimimierte Anwendungen
                // von den Benutzern nicht gesehen werden
                if (AlterZustand.Zustand
                    == (int)System.Windows.WindowState.Maximized)
                {
                    fenster.WindowState
                        = System.Windows.WindowState.Maximized;
                }
                else
                {
                    fenster.WindowState
                        = System.Windows.WindowState.Normal;
                }
            }

        }
        
       

      

        #endregion Fensterpositionen verwalten
    }


}
