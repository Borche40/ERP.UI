using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlzEx.Theming;
using ERP.UI.Commands;
using ERP.UI.ViewModel;
using MaterialDesignThemes.Wpf;

namespace ERP.UI.ViewModel
{
    /// <summary>
    /// Verwaltet Befehle zur Steuerung von Fensteraktionen 
    /// wie Minimieren, Schließen und Wiederherstellen 
    /// in der Anwendung sowie das Thema der Anwendung.
    /// </summary>
   public class FensterManager:BaseViewModel
    {
        #region Hauptfenstersteureung

        #region Fenstersteuerung
        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private ERP.UI.Commands.Befehl _Herabsetzen = null!;

        /// <summary>
        /// Ruft den Befehl zum verkleinern eines WPF-fensters
        /// </summary>
        public ERP.UI.Commands.Befehl Herabsetzen
        {
            get
            {
                if (this._Herabsetzen == null)
                {
                    this._Herabsetzen = new ERP.UI.Commands.Befehl(d =>
                    {
                        if (d is System.Windows.Window f)
                        {
                            f.WindowState = System.Windows.WindowState.Minimized;

                        }
                    });
                }
                return this._Herabsetzen;
            }
        }

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private ERP.UI.Commands.Befehl _Schließen = null!;

        /// <summary>
        /// Ruft den Befehl zum Schließen eines WPF-Fensters
        /// </summary>
        public ERP.UI.Commands.Befehl Schließen
        {
            get
            {
                if (this._Schließen == null)
                {
                    this._Schließen = new ERP.UI.Commands .Befehl(d =>
                    {
                        if (d is System.Windows.Window f)
                        {
                            f.Close();
                        }
                    });
                }
                return this._Schließen;
            }
        }

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private ERP.UI.Commands.Befehl _Wiederherstellen = null!;

        /// <summary>
        /// Ruft den Befehl zum Wiederherstellen eines WPF-Fensters
        /// </summary>
        public ERP.UI.Commands.Befehl Wiederherstellen
        {
            get
            {
                if (this._Wiederherstellen == null)
                {
                    this._Wiederherstellen = new ERP.UI.Commands.Befehl(d =>
                    {
                        if (d is System.Windows.Window f)
                        {
                            if (f.WindowState == System.Windows.WindowState.Normal)
                            {
                                f.WindowState = System.Windows.WindowState.Maximized;
                                this.WiederherstellenText = (char)ERP.UI.Properties.Settings.Default.Maximieren;


                            }
                            else if (f.WindowState == System.Windows.WindowState.Maximized)
                            {
                                f.WindowState = System.Windows.WindowState.Normal;
                                this.WiederherstellenText = (char)ERP.UI.Properties.Settings.Default.Widerherstellen;
                            }
                        }
                    });
                }
                return this._Wiederherstellen;
            }
        }
        #endregion Fenstersteuerung

        /// <summary>
        /// Ruft das Zeichen für die Wiederherstellen Taste ab, 
        /// oder legt diese fest
        /// </summary>
        public char WiederherstellenText
        {
            get => (char)ERP.UI.Properties.Settings.Default.WiederherstellenTaste;
            set
            {
                ERP.UI.Properties.Settings.Default.WiederherstellenTaste = (int)value;
                this.OnPropertyChanged();
            }
        }

        #endregion Hauptfenstersteureung






        #region ThemenManager

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private ThemenManager _Theme = null!;

        /// <summary>
        /// Ruft den Dienst für das Verwalten des Anwendungsthemas ab.
        /// </summary>
        public ThemenManager Theme
        {
            get
            {
                if (this._Theme == null)
                {
                    this._Theme = this.Kontext.Produziere<ThemenManager>();
                }
                return this._Theme;
            }
            
        }

        #endregion ThemenManager
    }

   
}
