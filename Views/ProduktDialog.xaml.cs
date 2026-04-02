using ERP.Core.Services;
using ERP.Data.Models;
using ERP.UI.Services;
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

namespace ERP.UI.Views
{
    /// <summary>
    /// Interaktionslogik für ProduktDialog.xaml
    /// </summary>
    public partial class ProduktDialog : Window
    {
        private readonly IDialogService _dialogService;
        private readonly ProdukteService _produkteService;
        public ProduktDialog(IDialogService dialogService,ProdukteService produkteService,Produkte produkte)
        {
            InitializeComponent();
            _dialogService = dialogService;
            _produkteService = produkteService;

            var viewModel = new ProduktDialogViewModel(_dialogService, _produkteService, produkte);

            var hauptfensterVm = Application.Current.MainWindow?.DataContext as HauptfensterViewModel;
            var aktuellerBenutzer = hauptfensterVm?.Benutzer?.Authentifizierung?.AktuellerBenutzer;

            if (aktuellerBenutzer != null)
            {
                viewModel.AktuellerBenutzerId = aktuellerBenutzer.Email ?? "Unbekannt";
                viewModel.AktuellerBenutzerName = aktuellerBenutzer.Name ?? "Unbekannt";
            }

            DataContext = viewModel;
        }
        
        public ProduktDialog():this(new DialogService(),new ProdukteService(),null)
        {
            
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
