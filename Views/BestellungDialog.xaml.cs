using ERP.Core.Services;
using ERP.Data.Models;
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

namespace ERP.UI.Views
{
    /// <summary>
    /// Interaktionslogik für BestellungDialog.xaml
    /// </summary>
    public partial class BestellungDialog : Window
    {
        public BestellungDialog(IDialogService dialogService,BestellungenService bestellungenService,Bestellung? bestellung = null)
        {

            InitializeComponent();
            DataContext = new BestellungDialogViewModel(dialogService, bestellungenService, bestellung);
        }
    }
}
