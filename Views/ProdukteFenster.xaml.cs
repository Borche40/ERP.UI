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
using ERP.UI.ViewModel;
using ERP.UI.Services;
using ERP.Core.Services;


namespace ERP.UI.Views

{
    /// <summary>
    /// Interaktionslogik für ProdukteFenster.xaml
    /// </summary>
    public partial class ProdukteFenster : Window
    {

        
        public ProdukteFenster()
        {
            InitializeComponent();
            var dialogService = new DialogService();
            var produkteService = new ProdukteService();

            DataContext = new ProdukteViewModel(dialogService, produkteService);

        }

        /// <summary>
        /// Schließt das Fenster, wenn der Benutzer auf die Schließen-Schaltfläche klickt.
        /// </summary>
        /// <param name="sender">Das Schließen-Button-Objekt.</param>
        /// <param name="e">Ereignisparameter.</param>
        private void Schließen_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
