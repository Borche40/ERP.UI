using MahApps.Metro;
using ERP.Data;
using ERP.UI.Properties;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using ERP.UI.ViewModel;
using System.Reflection;
using System.Resources;
using MahApps.Metro.Controls;
using ControlzEx.Theming;
using MahApps.Metro.Theming;
using ERP.UI.Views;


namespace ERP.UI
{
    /// <summary>
    /// Hauptklasse für die Anwendung mit Sprach- und Theme-Management.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Ruft das Infrastrukturobjekt
        /// der Anwendung ab oder legt
        /// dieses fest
        /// </summary>
        protected Anwendung.Infrastruktur Kontext { get; set; } = null!;

        /// <summary>
        /// Löst das Startup Ereignis aus
        /// </summary>
        /// <param name="e">Zusatzdaten, z. B.
        /// mit den Parametern aus der Konsole</param>
        /// <remarks>Weil Main() versteckt ist,
        /// wird hier unsere Infrastruktur initialisiert</remarks>
        protected override void OnStartup(StartupEventArgs e)
        {
            // Zuerst einmal das, was sonst passiert
            base.OnStartup(e);
           
            
            // Unsere Ergänzungen ...
            this.Kontext = new Anwendung.Infrastruktur();



            // Damit das Protokoll threadsicher wird
            this.Kontext.Log.Dispatcher = this.Dispatcher;

            // Damit das Anwendungsprotokoll gespeichert wird...
            this.Kontext.Log.Dateiname
                = System.IO.Path.Combine(
                    this.Kontext.Log.LokalerDatenpfad,
                    ERP.UI.Properties.Settings.Default.Protokollname);

            this.Kontext.Log.Hinzufügen(
                "Die ERP.UI Client/Server Anwendung startet..");

            this.Kontext.Sprachen.Festlegen(
                ERP.UI.Properties.Settings
                .Default.AktuellerSprachcode
                );



            var ViewModel = this.Kontext.Produziere<ViewModel.HauptfensterViewModel>();

            // Nur im Offline Betrieb,
            // den Connectionstring aus der
            // Konfiguration einstellen
            if (ERP.UI.Properties.Settings.Default.Offline)
            {
                ViewModel.Verbindungszeichenfolge
                    = ERP.UI.Properties.Settings
                        .Default.OfflineDatenbank;
                this.Kontext.Log.Hinzufügen(
                    new Anwendung.Daten.Protokolleintrag
                    {
                        Text = "Die Anwendung wird offline betrieben!",
                        Typ = Anwendung.Daten.ProtokolleintragTyp.Fehler
                    });
            }

            
            //Theme anwenden
            if (!string.IsNullOrEmpty(ERP.UI.Properties.Settings.Default.AktuellesThema))
            {

                ViewModel.Fenster.Theme.ThemeÄndern
             (new Uri(ERP.UI.Properties.Settings.Default.AktuellesThema, UriKind.Relative));
            }

            ViewModel.Anzeigen<Views.Hauptfenster>();

        }

        /// <summary>
        /// Löst das Exit Ereignis aus
        /// </summary>
        /// <param name="e">Zusatzdaten, z. B. mit
        /// der Information, warum die Anwendung
        /// beendet wird</param>
        /// <remarks>Wird um das Herunterfahren
        /// der eigenen Infrastruktur erweitert</remarks>
        protected override void OnExit(ExitEventArgs e)
        {
            // Vorher unsere Ergänzungen...

            // Die Daten vom Fenstermanager speichern...
            this.Kontext.Fenster.Speichern();

            // Anwendungseinstellungen
            // übernehmen und speichern
            ERP.UI.Properties.Settings.Default.AktuellerSprachcode
                = this.Kontext.Sprachen.AktuelleSprache.Code;



            ERP.UI.Properties.Settings.Default.AktuellesThema = ThemenManager.AktuellesThemeAbrufen().ToString();

            ERP.UI.Properties.Settings.Default.Save();

            // Zum Schluss das, was sonst passiert...
            base.OnExit(e);
        }
    }
}



