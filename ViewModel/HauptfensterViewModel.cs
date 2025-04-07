using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Resources;
using System.Threading.Tasks;
using System.Windows.Input;
using ERP.UI.Commands;
using ERP.UI.Views;
using System.Reflection;
using ControlzEx.Theming;
using ERP.UI.Properties;
using System.Diagnostics;
using System.Windows;
using System.Text;
using System.Linq;
using MahApps.Metro.Controls;
using MahApps.Metro.Theming;
using MahApps.Metro;
using Anwendung;
using ERP.UI.Models;
using ERP.UI.ViewModel;

namespace ERP.UI.ViewModel
{
    
    /// <summary>
    /// ViewModel für das Hauptfenster mit Navigation und Spracheinstellungen.
    /// </summary>
    internal class HauptfensterViewModel : BaseViewModel
    {
      

        #region Anwendungssprache

        /// <summary>
        /// Ruft die aktuelle Anwendungssprache
        /// ab oder legt diese fest
        /// </summary>
        public Anwendung.Daten.Sprache AktuelleSprache
        {
            get => this.Kontext.Sprachen.AktuelleSprache;
            set
            {
                if (this.Kontext.Sprachen.AktuelleSprache != value)
                {
                    this.IstBeschäftigt = true;
                    this.Kontext.Sprachen.AktuelleSprache = value;
                    this.OnPropertyChanged();

                    // Weil gecachet gearbeitet wird,
                    // darauf hinweisen, dass die Anwendung
                    // neu gestartet werden muss
                    System.Windows.MessageBox.Show(
                        Views.Texte.Sprachwechsel,
                        Views.Texte.AppTitel,
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Information);

                    this.Kontext.Log.Hinzufügen(
                        new Anwendung.Daten.Protokolleintrag
                        {
                            Text = Views.Texte.Sprachwechsel,
                            Typ = Anwendung.Daten.ProtokolleintragTyp.Fehler
                        });

                    this.IstBeschäftigt = false;
                }
            }
        }



        #endregion Anwendungssprache
       
        #region Benutzer
        

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private AufgabenManager _Benutzer = null!;

        /// <summary>
        /// Stellt einen Dienst bereit zum verwalten 
        /// von Benutzerinformationen bereit
        /// </summary>
        public AufgabenManager Benutzer
        {
            get
            {
                if (this._Benutzer == null)
                {
                    this._Benutzer = this.Kontext.Produziere<AufgabenManager>();

                    var Angemeldet = ERP.UI.Properties.Settings.Default.AngemeldetBleiben;

                    if (Angemeldet == true)
                    {
                        this._Benutzer.AnmeldenMitEinstellungen();
                    }
                }
                return this._Benutzer;
            }
          
        }

        #endregion Benutzer

        #region Fensterlogik

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private ERP.UI.Commands.Befehl _AllesSchließen = null!;

        /// <summary>
        /// Ruft den Befehl zum Schließen
        /// aller offenen Fenster und Beenden
        /// der Anwendung ab
        /// </summary>
        /// <remarks>Der Befehl ist nur zulässig,
        /// wenn mehr als ein Anwendungsfenster
        /// offen sind. Ist CommandParameter
        /// eingestellt, werden ohne Frage alle 
        /// anderen Fenster geschlossen. Dazu
        /// das ViewModel in den CommandParameter stellen.</remarks>
        public ERP.UI.Commands.Befehl AllesSchließen
        {
            get
            {
                if (this._AllesSchließen == null)
                {
                    this._AllesSchließen = new ERP.UI.Commands.Befehl(
                        
                        p =>
                        {
                            
                            var Antwort = System.Windows.MessageBoxResult.Yes;

                            if (p == null)
                            {
                                this.IstBeschäftigt = true;
                                Antwort = System.Windows.MessageBox.Show(
                                    Views.Texte.AllesSchließenFrage,
                                    Views.Texte.AppTitel,
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question);
                                this.IstBeschäftigt = false;
                            }

                            if (Antwort == System.Windows.MessageBoxResult.Yes)
                            {
                                foreach (System.Windows.Window w in App.Current.Windows)
                                {
                                    if (w.DataContext != p)
                                    {
                                        w.Close();
                                    }
                                }
                            }

                        },
                        // Methode zum Prüfen, ob der
                        // Befehl erlaubt ist
                        p => App.Current.Windows.Count > 1);

                    this.Kontext.Log.Hinzufügen(
                        $"{this} hat den Befehl " +
                        $"zum Schließen aller " +
                        $"Fenster initialisiert...");
                }

                return this._AllesSchließen;
            }
        }

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private ERP.UI.Commands.Befehl _NeuesFenster = null!;

        /// <summary>
        /// Ruft den Befehl zum Öffnen eines zusätzlichen
        /// Fensters mit dem Typ der beim Anzeigen&lt;T&gt;
        /// benutzt wurde ab
        /// </summary>
        /// <remarks>Dieser Befehl ist immer gültig.</remarks>
        public ERP.UI.Commands.Befehl NeuesFenster
        {
            get
            {
                if (this._NeuesFenster == null)
                {
                    this._NeuesFenster = new ERP.UI.Commands.Befehl(

                        p => this.NeuesFensterÖffnen()

                        );

                    this.Kontext.Log.Hinzufügen(
                        $"{this} hat den Befehl " +
                        $"zum Anzeigen eines neuen " +
                        $"Fensters initialisiert...");

                }

                return this._NeuesFenster;
            }
        }

        /// <summary>
        /// Initialisiert mit dem HauptfensterTyp
        /// ein neues Fenster und zeigt dieses an
        /// </summary>
        /// <remarks>Stellt die Execute Methode
        /// für den Befehl NeuesFenster bereit</remarks>
        protected virtual void NeuesFensterÖffnen()
        {
            this.IstBeschäftigt = true;
            this.Kontext.Log.StartMelden();

            var NeuesFenster = (System.Activator
                .CreateInstance(this.HauptfensterTyp)
                as System.Windows.Window)!;

            // Damit ein jedes Fenster selbstständig ist,
            // ein neues ViewModel dafür initialisieren...
            var NeuesViewModel = this.Kontext
                .Produziere<ViewModel.HauptfensterViewModel>();

            // dem neuen ViewModel den Fenstertyp
            // vom Anzeigen mitteilen...
            NeuesViewModel.HauptfensterTyp
                = this.HauptfensterTyp;

            // Die View mit dem ViewModel verbinden...
            NeuesFenster.DataContext = NeuesViewModel;

            // Fenster initialisieren
            NeuesViewModel.Initialisieren(NeuesFenster);

            // Anzeigen
            NeuesFenster.Show();

            this.Kontext.Log.EndeMelden();
            this.IstBeschäftigt = false;
        }
       
        /// <summary>
        /// Ruft den Fenstertyp ab,
        /// der beim Anzeigen benutzt wurde,
        /// oder legt diesen fest
        /// </summary>
        /// <remarks>Muss im Anzeigen eingestellt
        /// werden und wird bei NeuesFensterÖffnen
        /// benutzt</remarks>
        private System.Type HauptfensterTyp { get; set; } = null!;
       

        /// <summary>
        /// Öffnet die angegebene View
        /// </summary>
        /// <typeparam name="T">Ein WPF Fenster,
        /// das angezeigt werden soll</typeparam>
        public void Anzeigen<T>()
            where T : System.Windows.Window, new()
        {
            this.Kontext.Log.StartMelden();
            this.HauptfensterTyp = typeof(T);

            var View = new T();
            View.DataContext = this;

            this.Initialisieren(View);

            View.Show();

            this.Kontext.Log.EndeMelden();
        }

        /// <summary>
        /// Bereitet eine View für die Anzeige vor
        /// </summary>
        /// <param name="fenster">Ein WPF Fenster,
        /// das vorbereitet werden soll</param>
        /// <remarks>Hier wird die alte Fensterposition
        /// über den Fenstermanager der Infrastruktur
        /// wiederhergestellt oder hinterlegt.</remarks>
        protected virtual void Initialisieren(
            System.Windows.Window fenster)
        {
            this.Kontext.Log.StartMelden();

            //Für den benötigten Schlüssel
            //das Fenster benennen
            fenster.Name = fenster.GetType().Name;
            //und nummerieren
            fenster.Name += this.ErmittleFensterNummer(fenster);

            //Damit der Fenstermanager mehrere 
            //unterscheiden kann, diese durchnummerieren

            this.ZustandWiederherstellen(fenster);

            fenster.Closing
                += (sender, e)
                => this.ZustandHinterlegen(fenster);

            this.Kontext.Log.EndeMelden();
        }

        /// <summary>
        /// Gibt die erste freie laufende Nummer
        /// für den Fensterschlüssel zurück
        /// </summary>
        /// <param name="fenster">Ein WPF Fenster,
        /// das für den Fenstermanager unterschieden
        /// werden soll</param>
        protected virtual int ErmittleFensterNummer(
                    System.Windows.Window fenster)
        {
            this.Kontext.Log.StartMelden();

            int Nummer = 1;

            //Prüfen, ob die Nummer
            //in den offenen Fenstern
            //bereits vorhanden ist.
            //Wenn ja, um eins erhöhen

            //Geht ned...
            //Die vorhandenen Namen in LINQ ermitteln
            //var OffeneFensterNamen = from f in App.Current.Windows

            //in einer eigenen Schleife
            //eine Liste bauen, wo wir 
            //feststellen können, ob ein Element enthalten ist
            var OffeneFensterNamen
                = new System.Collections.ArrayList(
                    App.Current.Windows.Count);
            foreach (System.Windows.Window f in App.Current.Windows)
            {
                OffeneFensterNamen.Add(f.Name);
            }

            while (OffeneFensterNamen.Contains(fenster.Name + Nummer))
            {
                Nummer++;
            }

            this.Kontext.Log.EndeMelden();
            return Nummer;
        }

        #endregion Fensterlogik
        #region FensterManager 

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private FensterManager _Fenster = null!;

        /// <summary>
        /// Ruft den Dienst zum Steuren des Hauptfensters ab
        /// </summary>
        public FensterManager Fenster
        {
            get
            {
                if (this._Fenster == null)
                {
                    this._Fenster = this.Kontext.Produziere<FensterManager>();
                }
                return this._Fenster;
            }
          
             

        }


        #endregion FensterManager 


      


    }
}
