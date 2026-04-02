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
using ERP.UI.ViewModel;

namespace ERP.UI.Views
{
    /// <summary>
    /// Interaktionslogik für LoginView.xaml
    /// </summary>
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Schreibt das Passwort aus der PasswordBox in das ViewModel:
        /// HauptfensterViewModel -> Benutzer (AufgabenManager) -> Authentifizierung -> Anmeldung.Passwort
        /// </summary>
        private void PasswortGeändert(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is ERP.UI.ViewModel.HauptfensterViewModel hf
                && hf.Benutzer?.Authentifizierung?.Anmeldung != null)
            {
                hf.Benutzer.Authentifizierung.Anmeldung.Passwort = ((PasswordBox)sender).Password;
            }
        }
    


        // >>> Дијагностика за клик на Anmelden
        private void Anmelden_Click(object sender, RoutedEventArgs e)
        {
            var dc = this.DataContext;

            // 1) Ако DataContext е HauptfensterViewModel (кај тебе најчесто е)
            if (dc is ERP.UI.ViewModel.HauptfensterViewModel hf)
            {
                var cmd = hf.Benutzer?.BenutzerAnmelden;
                if (cmd != null && cmd.CanExecute(null))
                {
                    cmd.Execute(null);
                    return;
                }
                MessageBox.Show("Command 'Benutzer.BenutzerAnmelden' не е достапен (null) или не може да се изврши.", "Diag 1");
                return;
            }

            // 2) Ако DataContext е директно AufgabenManager
            if (dc is ERP.UI.ViewModel.AufgabenManager am)
            {
                var cmd = am.BenutzerAnmelden;
                if (cmd != null && cmd.CanExecute(null))
                {
                    cmd.Execute(null);
                    return;
                }
                MessageBox.Show("Command 'BenutzerAnmelden' на AufgabenManager е null или не може да се изврши.", "Diag 2");
                return;
            }

            // 3) Инаку покажи што е DataContext за да видиме каде е проблемот
            MessageBox.Show($"Неочекуван DataContext: {dc?.GetType().FullName ?? "null"}", "Diag 3");
        }
    }
}

