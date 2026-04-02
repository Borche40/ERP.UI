using ERP.Core.Services;
using ERP.UI.ViewModel;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace ERP.UI.Views
{
    /// <summary>
    /// Repräsentiert das Fenster für die Kundenverwaltung.
    /// Dieses Fenster zeigt eine Kundenliste und ermöglicht die Verwaltung von Kundendaten.
    /// </summary>
    /// <remarks>
    /// Das Fenster initialisiert sein ViewModel, setzt den DataContext
    /// und lädt die Daten automatisch beim Öffnen.
    /// </remarks>
    public partial class KundenFenster : Window
    {
        #region Felder

        /// <summary>
        /// Referenz auf das zugehörige ViewModel.
        /// </summary>
        private readonly KundenViewModel _viewModel = null!;

        #endregion

        #region Konstruktor

        /// <summary>
        /// Initialisiert ein neues Fenster für die Kundenverwaltung.
        /// </summary>
        /// <remarks>
        /// Der <see cref="KundenService"/> lädt seine Konfiguration selbstständig
        /// aus der Anwendungskonfiguration.
        /// </remarks>
        public KundenFenster()
        {
            InitializeComponent();

            try
            {
                // Service und ViewModel initialisieren.
                var kundenService = new KundenService();
                _viewModel = new KundenViewModel(kundenService);

                // DataContext für Bindings setzen.
                DataContext = _viewModel;

                // Ereignisse registrieren.
                Loaded += KundenFenster_Loaded;
                Loaded += Window_Loaded;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Fehler beim Initialisieren des Kundenfensters:\n{ex.Message}",
                    "Fehler",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        #endregion

        #region Ereignisse

        /// <summary>
        /// Setzt nach dem Laden des Fensters den Fokus auf das Suchfeld.
        /// </summary>
        /// <param name="sender">Das aktuelle Fenster.</param>
        /// <param name="e">Ereignisparameter.</param>
        private void KundenFenster_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                SuchtextBox.Focus();
                Keyboard.Focus(SuchtextBox);
            }), DispatcherPriority.Input);
        }

        /// <summary>
        /// Lädt nach dem Öffnen des Fensters die Kundendaten.
        /// </summary>
        /// <param name="sender">Das aktuelle Fenster.</param>
        /// <param name="e">Ereignisparameter.</param>
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataContext is KundenViewModel vm)
                {
                    await vm.LadeKundenAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Fehler beim Laden der Kundendaten:\n{ex.Message}",
                    "Fehler",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Schließt das Fenster.
        /// </summary>
        /// <param name="sender">Auslösendes Objekt.</param>
        /// <param name="e">Ereignisparameter.</param>
        private void Schließen_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }




        /// <summary>
        /// Ermöglicht das Verschieben des Fensters über die benutzerdefinierte Titelleiste.
        /// Ein Doppelklick maximiert bzw. stellt das Fenster wieder her.
        /// </summary>
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                FensterMaximierenOderWiederherstellen();
                return;
            }

            DragMove();
        }

        /// <summary>
        /// Minimiert das Fenster.
        /// </summary>
        private void Minimieren_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Maximiert das Fenster oder stellt es wieder her.
        /// </summary>
        private void Maximieren_Click(object sender, RoutedEventArgs e)
        {
            FensterMaximierenOderWiederherstellen();
        }

        /// <summary>
        /// Schaltet zwischen maximiertem und normalem Fensterzustand um.
        /// </summary>
        private void FensterMaximierenOderWiederherstellen()
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }
        #endregion
    }
}