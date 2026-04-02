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
    /// Interaktionslogik für AuditDialog.xaml
    /// </summary>
    public partial class AuditDialog : Window
    {
        public AuditDialog()
        {
            InitializeComponent();
            DataContext = new AuditDialogViewModel();
        }
        /// <summary>
        /// Schließt den Dialog.
        /// </summary>
        private void Schließen_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
