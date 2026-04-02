using Microsoft.Win32;
using System.Windows;

namespace ERP.UI.Services
{
    /// <summary>
    /// Dienst zum Anzeigen von Dialogen, Nachrichten
    /// und Auswahlfenstern (z. B. Datei-/Bildauswahl).
    /// Wird von den ViewModels verwendet, um UI-Dialoge
    /// ohne direkte Abhängigkeit von der View zu öffnen.
    /// </summary>
    public interface IDialogService
    {
        bool Confirm(string message, string title);// Zeigt ein Bestätigungsfenster (Ja/Nein) und gibt true zurück, wenn "Ja" gewählt wird.
        void ShowInfo(string message, string title);// Zeigt eine Informationsnachricht an.
        void ShowWarning(string message, string title);// Zeigt eine Warnmeldung an.
        void ShowError(string message, string title);// Zeigt eine Fehlermeldung an.
        string? PickImageFile();// Öffnet einen Datei-Dialog zur Auswahl eines Bildes.
        bool? ShowProduktDialog(object viewModel);// Öffnet den Produkt-Dialog und gibt das Dialogergebnis zurück.
    }

    /// <summary>
    /// Implementierung des IDialogService.
    /// Verwaltet MessageBoxen, Datei-Auswahl und Produktdialoge.
    /// </summary>
    public class DialogService : IDialogService
    {
        /// <summary>
        /// Zeigt ein Bestätigungsfenster (Ja/Nein).
        /// Gibt true zurück, wenn der Benutzer "Ja" klickt.
        /// </summary>
        public bool Confirm(string message, string title)
            => MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

        /// <summary>
        /// Zeigt eine Info-Nachricht (blauer Kreis mit „i“).
        /// </summary>
        public void ShowInfo(string message, string title)
            => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);

        /// <summary>
        /// Zeigt eine Warnmeldung (gelbes Dreieck).
        /// </summary>
        public void ShowWarning(string message, string title)
            => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);

        /// <summary>
        /// Zeigt eine Fehlermeldung (rotes Symbol).
        /// </summary>
        public void ShowError(string message, string title)
            => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);

        /// <summary>
        /// Öffnet einen Datei-Dialog zur Auswahl eines Bildes.
        /// Unterstützte Formate: PNG, JPG, JPEG, BMP, GIF.
        /// </summary>
        public string? PickImageFile()
        {
            var dlg = new OpenFileDialog
            {
                Title = "Bild auswählen",
                Filter = "Bilder|*.png;*.jpg;*.jpeg;*.bmp;*.gif",
                Multiselect = false
            };
            return dlg.ShowDialog() == true ? dlg.FileName : null;
        }

        /// <summary>
        /// Öffnet den Produkt-Dialog (Window) und setzt das ViewModel als DataContext.
        /// Gibt DialogResult (true/false/null) zurück.
        /// </summary>
        public bool? ShowProduktDialog(object viewModel)
        {
            var dlg = new ERP.UI.Views.ProduktDialog
            {
                DataContext = viewModel,
                Owner = Application.Current.MainWindow
            };
            return dlg.ShowDialog();
        }
    }
}
