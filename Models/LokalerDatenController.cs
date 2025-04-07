using ERP.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ERP.UI.ViewModel;
using ERP.Data.Models;
using ERP.Data.Controllers;

namespace ERP.UI.Models
{
    /// <summary>
    /// Stellt einen Dienst zum Arbeiten
    /// mit den Demo Daten im Offline Betrieb bereit
    /// </summary>
    internal class LokalerDatenController
        : LokalerController,
        IDatenController
    {
        private new Type AddInControllerTyp { get; set; }

        #region Add-In Controller
        /// <summary>
        /// Initialisiert einen neuen LokalenDatenController
        /// </summary>
        public LokalerDatenController() : base("ERP.Dta.Controllers.DatenSqlController")
        {
            if (this.AddInControllerTyp == null)
            {
                this.AddInControllerTyp = typeof(DatenSqlController); // Initialize with the appropriate type
            }
        }
        #endregion Add-In Controller

        public async Task<Benutzer?> Anmelden(Anmeldung anmeldung)
        {
            // Implement the method logic here
            return await Task.FromResult<Benutzer?>(null);
        }

        public async Task<Aufgaben?> HoleAufgabenAsync(Benutzer benutzer, string gruppe)
        {
            // Implement the method logic here
            return await Task.FromResult<Aufgaben?>(null);
        }

        public async Task<AufgabenGruppen?> HoleAufgabenGruppenAsync(Benutzer benutzer)
        {
            // Implement the method logic here
            return await Task.FromResult<AufgabenGruppen?>(null);
        }

        public async Task<int> Registrieren(Registrierung registrierung)
        {
            // Implement the method logic here
            return await Task.FromResult(0);
        }

        // Rest of the code...
    }
}

