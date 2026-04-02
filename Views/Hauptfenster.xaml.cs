using MahApps.Metro.Controls;
using System.Windows;
using System.Windows.Input;
using ERP.UI.ViewModel;
using ERP.UI.Views;
using System.Windows.Media;
using Anwendung;

namespace ERP.UI.Views
{
    /// <summary>
    /// Repräsentiert das Hauptfenster der ERP-Anwendung.
    /// Es dient als Container für die gesamte Benutzeroberfläche (Login, Dashboard, etc.)
    /// und steuert die Navigation zwischen den Ansichten.
    /// </summary>
    public partial class Hauptfenster : MetroWindow
    {
        /// <summary>
        /// Konstruktor – initialisiert das Fenster und setzt den DataContext korrekt.
        /// </summary>
        public Hauptfenster()
        {
            InitializeComponent();

            // Falls noch kein Hauptfenster registriert ist, dieses als MainWindow setzen
            if (Application.Current.MainWindow == null)
                Application.Current.MainWindow = this;

            // Das ViewModel wird über die zentrale Infrastruktur erzeugt,
            // damit alle globalen Objekte (z. B. OberflächeManager, AufgabenManager, etc.) geteilt werden.
            if (this.DataContext == null)
            {
                var context = new Anwendung.Infrastruktur();
                var hauptfensterViewModel = context.Produziere<HauptfensterViewModel>();
                this.DataContext = hauptfensterViewModel;
            }

            // Sobald das Hauptfenster geladen ist → Oberfläche initialisieren
            this.Loaded += Hauptfenster_Loaded;
        }

        /// <summary>
        /// Wird aufgerufen, sobald das Fenster vollständig geladen ist.
        /// Initialisiert die erste Benutzeroberfläche (Login oder Dashboard),
        /// abhängig vom Anmeldestatus.
        /// </summary>
        private void Hauptfenster_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Zugriff auf das Hauptfenster-ViewModel
                if (this.DataContext is HauptfensterViewModel hauptfensterVm)
                {
                    // OberflächeManager ermitteln (steuert, welche View im ContentControl angezeigt wird)
                    var oberfläche = hauptfensterVm.Benutzer?.Oberfläche;

                    if (oberfläche == null)
                    {
                        MessageBox.Show("Oberflächen-Manager konnte nicht initialisiert werden.", "Fehler",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Wenn der Benutzer nicht angemeldet ist → LoginView anzeigen
                    if (!oberfläche.IstAngemeldet)
                    {
                        oberfläche.Benutzeroberfläche = new LoginView();
                    }
                    else
                    {
                        // Wenn der Benutzer bereits angemeldet ist → DashboardView anzeigen
                        oberfläche.Benutzeroberfläche = new DashboardView();
                    }
                }
                else
                {
                    MessageBox.Show("Fehler: Kein Hauptfenster-ViewModel gefunden.", "Fehler",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Fehler beim Initialisieren der Oberfläche: {ex.Message}",
                    "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Hilfsmethode, um ein Kind-Element vom bestimmten Typ im visuellen Baum zu finden.
        /// </summary>
        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T found)
                    return found;

                var result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }

            return null;
        }

        /// <summary>
        /// Ermöglicht das Ziehen des Fensters mit der Maus.
        /// </summary>
        private void Ziehen(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        /// <summary>
        /// Passt den Fensterrand beim Maximieren an, um die Taskleiste sichtbar zu halten.
        /// </summary>
        private void GrößeGeändert(object sender, SizeChangedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.BorderThickness = new Thickness(8);
            else
                this.BorderThickness = new Thickness(0);
        }
    }
}
