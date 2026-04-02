using ERP.Core.Services;
using ERP.UI.Services;
using ERP.UI.ViewModel;
using System;
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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ERP.UI.Views
{
    /// <summary>
    /// Interaktionslogik für AufgabenFenster.xaml
    /// </summary>
    public partial class AufgabenFenster : Window
    {
        
        public AufgabenFenster()
        {
            InitializeComponent();

            var dialogService = new DialogService();
            var aufgabenService = new AufgabenService();
            DataContext = new AufgabenViewModel(dialogService, aufgabenService);

        }


       
        
        // Schließt das Fenster, wenn der Benutzer auf die Schließen-Schaltfläche klickt.
        /// </summary>
        /// <param name="sender">Das Schließen-Button-Objekt.</param>
        /// <param name="e">Ereignisparameter.</param>
        private void Schließen_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
