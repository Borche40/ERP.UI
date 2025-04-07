using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ERP.UI.Views
{
    /// <summary>
    /// Interaktionslogik für LoginView.xaml
    /// </summary>
    public partial class LoginView :UserControl
    {
        public LoginView()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Behandelt das PreviewKeyDown-Ereignis für das Überspringen 
        /// des Passwortfelds beim Drücken der Tabulatortaste.
        /// </summary>
        private void PasswortÜbersprungen(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                Passwort.Focus();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Behandelt das Ereignis, das ausgelöst wird, wenn das Passwort geändert wird, 
        /// und aktualisiert die Textbox Eigenschaft die für das Binding benutzt wird
        /// </summary>
        /// <param name="sender">Das Objekt, das das Ereignis ausgelöst hat.</param>
        /// <param name="e">Die Ereignisdaten.</param>
        private void PasswortGeändert(object sender, RoutedEventArgs e)
        {
            ZeigePasswort.Text = Passwort.Password;
        }

        /// <summary>
        /// Macht den Passworttext sichtbar, wenn die Schaltfläche gedrückt wird.
        /// </summary>
        /// <param name="sender">Das Objekt, das das Ereignis ausgelöst hat.</param>
        /// <param name="e">Die Ereignisdaten.</param>
        private void MausUnten(object sender, MouseButtonEventArgs e)
        {
            Passwort.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Versteckt den Passworttext, wenn die Schaltfläche losgelassen wird.
        /// </summary>
        /// <param name="sender">Das Objekt, das das Ereignis ausgelöst hat.</param>
        /// <param name="e">Die Ereignisdaten.</param>
        private void MausOben(object sender, MouseButtonEventArgs e)
        {
            Passwort.Visibility = Visibility.Visible;
        }




    }
}
