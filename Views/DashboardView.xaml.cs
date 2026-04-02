using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ERP.Core.Services;
using ERP.Data.Models;
using ERP.UI.Services;
using ERP.UI.ViewModel;



namespace ERP.UI.Views
{
    /// <summary>
    /// Interaktionslogik für DashboardView.xaml
    /// </summary>
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();

            DataContext = new Dashboard();
        }

        private void BellContainer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is ViewModel.Dashboard vm)
            {
                //Nur öffnen wen Warnungen vorhanden sind..
                if (vm.HatWarnungen && vm.KritischeProduktAnzahl > 0)
                {
                    //Popup ein - ausblenden toggle.
                    vm.IstWarnungsPanelSichtbar = !
                        vm.IstWarnungsPanelSichtbar;
                }
                else
                {
                    //Falls keine warnungen Popup schließen
                    vm.IstWarnungsPanelSichtbar = false;
                }
            }
        }
        /// <summary>
        /// Öffnet das Produktfenster und markiert das
        /// doppelte angeklickte 
        /// </summary>
        private void KritischeProdukteListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if(sender is ListBoxItem item && item.DataContext is ERP.Data.Models.Produkte produkt)
                {
                    var produkteService = new ERP.Core.Services.ProdukteService();
                    var dialogService = new ERP.UI.Services.DialogService();

                    var viewModel = new ERP.UI.ViewModel.ProduktDialogViewModel(
                        dialogService, produkteService, produkt);
                   

                    var fenster = new ERP.UI.Views.ProduktDialog
                    {
                        Owner = System.Windows.Application.Current.MainWindow,
                        DataContext = viewModel
                    };
                    fenster.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Fehler beim Öffnen des Produktfensters:\n{ex.Message}", "Fehler",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            

        }



        private void WarnungenPopup_Opened(object sender,EventArgs e)
        {
            var border = WarnungenPopup.Child as Border;
            if (border == null) return;
            border.Opacity = 0;
            border.RenderTransform = new TranslateTransform();

            var openAnim = (Storyboard)FindResource("PopupOpenAnimation");
            Storyboard.SetTarget(openAnim,border);
            openAnim.Begin();
        }
        /// <summary>
        /// Wird aufgerufen wenn das Warnungs-Popup 
        /// von WPF geschlossen wird.
        /// z.B durch klick außerhalb,Synchrosiert das
        /// ViewModel-flag
        /// </summary>
        private void WarnungenPopup_Closed(object sender, EventArgs e)
        {
            if(DataContext is ERP.UI.ViewModel.Dashboard vm)
            {
                //Falls das ViewModel noch offen denkt -zurücksetzen
                if (vm.IstWarnungsPanelSichtbar)
                    vm.IstWarnungsPanelSichtbar = false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void StartBellAnimation()
        {
            var anim = (Storyboard)FindResource("BellBounceAnimation");
            anim.Begin(BellContainer);
        }


        public void StartBadgePulse()
        {
            var anim = (Storyboard)FindResource("BadgePulseAnimation");
            anim.Begin(WarnungenPopup);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hatWarnungen"></param>
        public void UpdateWarnungenVisuals(bool hatWarnungen)
        {
            if (hatWarnungen)
            {
                StartBellAnimation();
                StartBadgePulse();
            }
        }
    }
}
