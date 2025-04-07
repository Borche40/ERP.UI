using MahApps.Metro.Controls;
using System.Text;
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
using ERP.Data;
using System.Windows.Controls.Primitives;
using ERP.UI.Views;
using Microsoft.EntityFrameworkCore;
using ControlzEx.Theming;

namespace ERP.UI.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Hauptfenster :MetroWindow
    {


        public Hauptfenster()
        {
            InitializeComponent();
            

        }
        /// <summary>
        /// Methode zum Bewegen des Fensters bei Mausklick und -ziehen.
        /// </summary>
        private void Ziehen(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        /// <summary>
        /// Methode zur Anpassung der Fenstergröße.
        /// Stellt sicher, dass das Fenster korrekt maximiert wird, ohne die Microsoft/Windows-Taskleiste am unteren Rand zu überdecken.
        /// </summary>
        private void GrößeGeändert(object sender, SizeChangedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.BorderThickness = new System.Windows.Thickness(8);
            }
            else
            {
                this.BorderThickness = new System.Windows.Thickness(0);
            }
        }



    }
}
