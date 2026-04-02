using ERP.Core.Services;
using ERP.Data;
using ERP.Data.Models;
using ERP.UI.Services;
using ERP.UI.ViewModel;
using System.Windows;

namespace ERP.UI.Views
{
    /// <summary>
    /// Code-Behind für NeueAufgabeDialog.xaml.
    /// Initialisiert den Dialog mit dem passenden ViewModel.
    /// </summary>
    public partial class NeueAufgabeDialog : Window
    {
        private readonly IDialogService _dialogService;
        private readonly AufgabenService _aufgabenService;
        /// <summary>
        /// K
        /// </summary>
        /// <param name="dialogService"></param>
        /// <param name="aufgabenService"></param>
        /// <param name="bestehendeAufgabe"></param>
        public NeueAufgabeDialog(IDialogService dialogService,
                                 AufgabenService aufgabenService,
                                 Aufgabe? bestehendeAufgabe = null )
        {
            InitializeComponent();
            DataContext = new NeueAufgabeDialogViewModel(dialogService, aufgabenService, bestehendeAufgabe, this);
        }

       public void Schließen(bool result)
        {
            DialogResult = result;
            Close();
        }
    }
}
