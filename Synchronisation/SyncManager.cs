using ERP.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ERP.UI.Models.Synchronisation
{
    /// <summary>
    /// Stellt einen Dienst zum Synchronisieren zwischen 
    /// der Online- und der Lokalen Datenbank
    /// bereit.
    /// </summary>
    internal class SyncManager : Anwendung.AppObjekt
    {
        #region Controller Dienste für die Online-Synchronisation

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private OnlineSpeicherController _OnlineController = null!;

        /// <summary>
        /// Ruft den Dienst zum Speichern von Daten in die Online-Datenbank ab
        /// </summary>
        private OnlineSpeicherController OnlineController
        {
            get
            {
                if (this._OnlineController == null)
                {
                    this._OnlineController = this.Kontext.Produziere<OnlineSpeicherController>();
                }
                return this._OnlineController;
            }
        }

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private DatenAbrufController _Controller = null!;

        /// <summary>
        /// Ruft den Dienst zum Synchronisieren der Demo 
        /// Daten ab
        /// </summary>
        private DatenAbrufController Controller
        {
            get
            {
                if (this._Controller == null)
                {

                    this._Controller
                        = Kontext
                            .Produziere<DatenAbrufController>();
                }

                return this._Controller;
            }
        }

        #endregion Controller Dienste für die Online-Synchronisation

        #region Controller Dienste für die Offline-Synchronisation

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private LokalerDatenController _LokalerAbrufController = null!;

        /// <summary>
        /// Ruft den Dienst zum Holen der
        /// Daten aus der Lokalen-Datenbank ab.
        /// </summary>
        private LokalerDatenController LokalerAbrufController
        {
            get
            {
                if (this._LokalerAbrufController == null)
                {
                    this._LokalerAbrufController = this.Kontext.Produziere<LokalerDatenController>();
                }
                return this._LokalerAbrufController;
            }
        }

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private OnlineDatenController _OnlineAbrufController = null!;

        /// <summary>
        /// Ruft den Dienst zum Holen der
        /// Daten aus der Online-Datenbank ab.
        /// </summary>
        private OnlineDatenController OnlineAbrufController
        {
            get
            {
                if (this._OnlineAbrufController == null)
                {
                    this._OnlineAbrufController = this.Kontext.Produziere<OnlineDatenController>();
                }
                return this._OnlineAbrufController;
            }
        }

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private LokalerSpeicherController _SpeicherController = null!;

        /// <summary>
        /// Ruft den Dienst zum Speichern der
        /// Daten auf die Lokale-Datenbank ab.
        /// </summary>
        private LokalerSpeicherController SpeicherController
        {
            get
            {
                if (this._SpeicherController == null)
                {
                    this._SpeicherController = this.Kontext.Produziere<LokalerSpeicherController>();
                }
                return this._SpeicherController;
            }
        }

        #endregion Controller Dienste für die Offline-Synchronisation

        #region Synchronisation: Offline zu Online

        /// <summary>
        /// Ereignis, das ausgelöst wird, 
        /// wenn die Synchronisation abgeschlossen ist.
        /// </summary>
        public event EventHandler<SpeichernEventArgs>? SynchronisationAbgeschlossen = null!;

        /// <summary>
        /// Löst das Ereignis aus, das signalisiert, 
        /// dass die Synchronisation abgeschlossen ist.
        /// </summary>
        /// <param name="e">Die Ereignisdaten, 
        /// die Informationen über den Abschluss der Synchronisationsaktion enthalten.</param>
        protected virtual void OnSynchronisationAbgeschlossen(SpeichernEventArgs e)
        {
            var behandlerKopie = SynchronisationAbgeschlossen;
            behandlerKopie?.Invoke(this, e);
        }

        /// <summary>
        /// Synchronisiert Daten von der Offline-Datenbank mit der Online-Datenbank.
        /// </summary>
        /// <param name="onlineSchlüssel">Der Schlüssel, der den zugriff auf die Online-Datenbank erlaubt.</param>
        /// <param name="lokalerSchlüssel">Der lokale Schlüssel zur Identifizierung der Daten.</param>
        public async void Synchronisieren(System.Guid onlineSchlüssel, System.Guid lokalerSchlüssel)
        {
            this.Kontext.Log.StartMelden();
            var syncgruppen = await this.Controller.HoleGruppenAsync(lokalerSchlüssel);
            if (syncgruppen.Count > 0)
            {
                foreach (var gruppe in syncgruppen)
                {
                    if (gruppe.SollGelöschtwerden)
                    {
                        var antwort = await this.OnlineController.GruppeLöschenAsync(onlineSchlüssel, gruppe.Name);
                        if (antwort == 0)
                        {
                            this.OnSynchronisationAbgeschlossen(new SpeichernEventArgs(antwort));
                            return;
                        }
                    }
                    else
                    {
                        var antwort = await this.OnlineController.SpeichereGruppeAsync(onlineSchlüssel, gruppe.Name);
                        if (antwort == 0)
                        {
                            this.OnSynchronisationAbgeschlossen(new SpeichernEventArgs(antwort));
                            return;
                        }
                    }
                }
            }
            this.AufgabenSpeichern(onlineSchlüssel, lokalerSchlüssel);
            this.LokaleDatenSynchronisieren(onlineSchlüssel, lokalerSchlüssel);
            this.Kontext.Log.EndeMelden();
        }

        /// <summary>
        /// Synchronisiert Aufgaben von der Offline-Datenbank in die Online-Datenbank.
        /// </summary>
        /// <param name="onlineSchlüssel">Der Schlüssel, der den zugriff auf die Online-Datenbank erlaubt.</param>
        /// <param name="lokalerSchlüssel">Der lokale Schlüssel zur Identifizierung der Daten.</param>
        private async void AufgabenSpeichern(System.Guid onlineSchlüssel, System.Guid lokalerSchlüssel)
        {
            var daten = await this.Controller.HoleAufgabenAsync(lokalerSchlüssel);
            if (daten.Count > 0)
            {
                foreach (var syncaufgabe in daten)
                {
                    if (syncaufgabe.SollGelöschtwerden)
                    {
                        var antwort =
                            await this.OnlineController.
                            AufgabeLöschenAsync(onlineSchlüssel, syncaufgabe.GruppenName, syncaufgabe.Aufgabe);
                        if (antwort == 0)
                        {
                            this.OnSynchronisationAbgeschlossen(new SpeichernEventArgs(antwort));
                            return;
                        }
                    }
                    else
                    {
                        var antwort =
                            await this.OnlineController.
                            SpeichereAufgabeAsync(syncaufgabe.GruppenName, onlineSchlüssel, syncaufgabe.Aufgabe);
                        if (antwort == 0)
                        {
                            this.OnSynchronisationAbgeschlossen(new SpeichernEventArgs(antwort));
                            return;
                        }
                    }
                }
            }

            await this.Controller.DatenLöschenAsync(lokalerSchlüssel);
            this.OnSynchronisationAbgeschlossen(new SpeichernEventArgs(1));
        }

        #endregion Synchronisation: Offline zu Online

        #region Synchronisation: Online zu Offline

        /// <summary>
        /// Ereignis, das ausgelöst wird, 
        /// wenn die Synchronisation der lokalen Daten abgeschlossen ist.
        /// </summary>
        public event EventHandler<SpeichernEventArgs>? LokaleSynchronisationAbgeschlossen = null!;

        /// <summary>
        /// Löst das Ereignis aus, das signalisiert, 
        /// dass die Synchronisation der lokalen Daten abgeschlossen ist.
        /// </summary>
        /// <param name="e">Die Ereignisdaten, 
        /// die Informationen über den Abschluss der Synchronisationsaktion enthalten.</param>
        protected virtual void OnLokaleSynchronisationAbgeschlossen(SpeichernEventArgs e)
        {
            var behandlerKopie = LokaleSynchronisationAbgeschlossen;
            behandlerKopie?.Invoke(this, e);
        }

        /// <summary>
        /// Synchronisiert Daten von der Online-Datenbank mit der Offline-Datenbank.
        /// </summary>
        /// <param name="onlineSchlüssel">Der Schlüssel, der den zugriff auf die Online-Datenbank erlaubt.</param>
        /// <param name="lokalerSchlüssel">Der lokale Schlüssel zur Identifizierung der Daten.</param>
        public async void LokaleDatenSynchronisieren(System.Guid onlineSchlüssel, System.Guid lokalerSchlüssel)
        {
            this.Kontext.Log.StartMelden();
            var onlineDaten = await this.HoleOnlineDaten(onlineSchlüssel);
            var lokaleDaten = await this.HoleLokaleDaten(lokalerSchlüssel);
            onlineDaten.Schlüssel = onlineSchlüssel;
            lokaleDaten.Schlüssel = lokalerSchlüssel;

            #region Gruppen löschen

            // Durchlaufen der lokalen Gruppen in den lokalen Daten
            foreach (var gruppe in lokaleDaten.Gruppen)
            {
                // Überprüfen, ob die lokale Gruppe in den online Daten vorhanden ist
                var onlineGruppe = onlineDaten.Gruppen.FirstOrDefault(g => g.Name == gruppe.Name);

                // Wenn die lokale Gruppe nicht in den online Daten vorhanden ist, löschen
                if (onlineGruppe == null)
                {

                    var antwort = await this.SpeicherController.GruppeLöschenAsync(lokalerSchlüssel, gruppe.Name);
                    if (antwort != 1)
                    {
                        this.OnLokaleSynchronisationAbgeschlossen(new SpeichernEventArgs(antwort));
                        return;
                    }

                }
            }
            #endregion Gruppen löschen

            #region Gruppen speicheren
            // Durchlaufen online Gruppen in den online Daten
            foreach (var neueGruppe in onlineDaten.Gruppen)
            {
                // Überprüfen, ob die online Gruppe in den lokalen Daten vorhanden ist
                var gruppe = lokaleDaten.Gruppen.FirstOrDefault(g => g.Name == neueGruppe.Name);

                // Wenn die online Gruppe nicht in den lokalen Daten vorhanden ist, speichern
                if (gruppe == null)
                {

                    var antwort = await this.SpeicherController.SpeichereGruppeAsync(lokalerSchlüssel, neueGruppe.Name);
                    if (antwort != 1)
                    {
                        this.OnLokaleSynchronisationAbgeschlossen(new SpeichernEventArgs(antwort));
                        return;
                    }

                }
            }
            #endregion Gruppen speicheren

            this.AufgabenSynchronisieren(onlineDaten, lokaleDaten);
            this.Kontext.Log.EndeMelden();
        }

        /// <summary>
        /// Synchronisiert die Aufgaben von der Online-Datenbank mit der Offline-Datenbank.
        /// </summary>
        /// <param name="onlineSchlüssel">Der Schlüssel, der den zugriff auf die Online-Datenbank erlaubt.</param>
        /// <param name="lokalerSchlüssel">Der lokale Schlüssel zur Identifizierung der Daten.</param>
        private async void AufgabenSynchronisieren(ERP.Data.Models.Benutzer onlineDaten, ERP.Data.Models.Benutzer lokaleDaten)
        {
            this.Kontext.Log.StartMelden();

            #region Aufgabe löschen

            // 1. Überprüfen, was gelöscht werden muss,
            // jede Aufgabe in den lokalen Daten, die nicht in den online Daten vorhanden ist.
            if (lokaleDaten.Gruppen != null)
            {
                foreach (var lokaleGruppe in lokaleDaten.Gruppen)
                {
                    // Überprüfen, ob es eine passende Gruppe in online Daten gibt
                    var onlineGruppe =
                        onlineDaten.Gruppen.FirstOrDefault(onlineGruppe => string.Compare(onlineGruppe.Name, lokaleGruppe.Name) == 0);
                    if (onlineGruppe != null)
                    {
                        if (lokaleGruppe.Aufgaben != null)
                        {
                            foreach (var lokaleAufgabe in lokaleGruppe.Aufgaben)
                            {
                                if (onlineGruppe.Aufgaben == null ||
                                    !onlineGruppe.Aufgaben.Any(neueAufgabe => neueAufgabe.ID == lokaleAufgabe.ID))
                                {
                                    var antwort =
                                        await this.SpeicherController.AufgabeLöschenAsync
                                        ((System.Guid)lokaleDaten.Schlüssel!, lokaleGruppe.Name, lokaleAufgabe);
                                    if (antwort != 1)
                                    {
                                        this.OnLokaleSynchronisationAbgeschlossen(new SpeichernEventArgs(antwort));
                                        return;
                                    }
                                }
                                else
                                {
                                    var neueAufgabe = onlineGruppe.Aufgaben.First(aufgabe => aufgabe.ID == lokaleAufgabe.ID);
                                    if (string.Compare(lokaleAufgabe.Inhalt, neueAufgabe.Inhalt) != 0
                                        || string.Compare(lokaleAufgabe.Zeit, neueAufgabe.Zeit) != 0)
                                    {
                                        var antwort = await this.SpeicherController.AufgabeLöschenAsync(
                                            (System.Guid)lokaleDaten.Schlüssel!, lokaleGruppe.Name, lokaleAufgabe);
                                        if (antwort != 1)
                                        {
                                            this.OnLokaleSynchronisationAbgeschlossen(new SpeichernEventArgs(antwort));
                                            return;
                                        }
                                    }
                                }
                            }

                        }
                    }
                }
            }
            #endregion Aufgabe löschen

            #region Aufgabe speichern

            // 2. Überprüfen, ob neue Gruppen hinzugefügt werden müssen
            if (onlineDaten.Gruppen != null)
            {
                foreach (var onlineGruppe in onlineDaten.Gruppen)
                {
                    // Überprüfen, ob die Gruppe neu ist oder bereits existiert
                    var lokaleGruppe =
                        lokaleDaten.Gruppen?.FirstOrDefault(alte => string.Compare(alte.Name, onlineGruppe.Name) == 0);
                    if (onlineGruppe.Aufgaben != null)
                    {
                        if (lokaleGruppe == null)
                        {

                            // Gruppe ist neu, speichern Sie alle Aufgaben in der neuen Gruppe
                            foreach (var neueAufgabe in onlineGruppe.Aufgaben)
                            {

                                var antwort =
                                    await this.SpeicherController.SpeichereAufgabeAsync
                                    (onlineGruppe.Name, (System.Guid)lokaleDaten.Schlüssel!, neueAufgabe);
                                if (antwort != 1)
                                {
                                    this.OnLokaleSynchronisationAbgeschlossen(new SpeichernEventArgs(antwort));
                                    return;
                                }

                            }

                        }
                        else
                        {
                            // Überprüfen Sie, ob Aufgaben in der vorhandenen Gruppe aktualisiert werden müssen
                            if (onlineGruppe.Aufgaben != null)
                            {
                                foreach (var neueAufgabe in onlineGruppe.Aufgaben)
                                {
                                    if (lokaleGruppe.Aufgaben != null)
                                    {
                                        var lokaleAufgabe =
                                            lokaleGruppe.Aufgaben.FirstOrDefault(aufgabe => aufgabe.ID == neueAufgabe.ID);

                                        if (lokaleAufgabe == null || (lokaleAufgabe.IstFertig != neueAufgabe.IstFertig &&
                                            string.Compare(lokaleAufgabe.Inhalt, neueAufgabe.Inhalt) == 0 && 
                                            string.Compare(lokaleAufgabe.Zeit, neueAufgabe.Zeit) == 0))
                                        {
                                            var antwort = await this.SpeicherController.
                                                SpeichereAufgabeAsync(onlineGruppe.Name, (System.Guid)lokaleDaten.Schlüssel!, neueAufgabe);
                                            if (antwort != 1)
                                            {
                                                this.OnLokaleSynchronisationAbgeschlossen(new SpeichernEventArgs(antwort));
                                                return;
                                            }

                                        }
                                        else if (string.Compare(lokaleAufgabe.Inhalt, neueAufgabe.Inhalt) != 0
                                            || string.Compare(lokaleAufgabe.Zeit, neueAufgabe.Zeit) != 0)
                                        {
                                            lokaleAufgabe.ID = 0;
                                            var antwort = await this.SpeicherController.
                                          SpeichereAufgabeAsync(onlineGruppe.Name, (System.Guid)lokaleDaten.Schlüssel!, neueAufgabe);
                                            if (antwort != 1)
                                            {
                                                this.OnLokaleSynchronisationAbgeschlossen(new SpeichernEventArgs(antwort));
                                                return;
                                            }
                                        }
                                    }

                                    else
                                    {

                                        var antwort = await this.SpeicherController.
                                            SpeichereAufgabeAsync(onlineGruppe.Name, (System.Guid)lokaleDaten.Schlüssel!, neueAufgabe);
                                        if (antwort != 1)
                                        {
                                            this.OnLokaleSynchronisationAbgeschlossen(new SpeichernEventArgs(antwort));
                                            return;
                                        }

                                    }

                                }
                            }
                        }
                    }
                }
            }

            #endregion Aufgabe speichern

            this.Kontext.Log.EndeMelden();
            this.OnLokaleSynchronisationAbgeschlossen(new SpeichernEventArgs(1));
        }

        /// <summary>
        /// Ruft die Daten aus der Lokalen-Datebank aab.
        /// </summary>
        /// <param name="schlüssel">Der lokale Schlüssel zur Identifizierung der Daten.</param>
        private async Task<ERP.Data.Models.Benutzer> HoleLokaleDaten(System.Guid schlüssel)
        {
            this.Kontext.Log.StartMelden();
            var daten = new ERP.Data.Models.Benutzer();
            daten.Schlüssel = schlüssel;

            var gruppen = await this.LokalerAbrufController.  HoleAufgabenGruppenAsync(daten);

            if (gruppen != null)
            {
                daten.Gruppen = gruppen;
                foreach (var gruppe in daten.Gruppen)
                {
                    gruppe.Aufgaben = await this.LokalerAbrufController.HoleAufgabenAsync(daten, gruppe.Name);
                }
            }
            this.Kontext.Log.EndeMelden();
            return daten;

        }

        /// <summary>
        /// Ruft die Daten aus der Online-Datebank aab.
        /// </summary>
        /// <param name="schlüssel">Der Schlüssel, der den zugriff auf die Online-Datenbank erlaubt.</param>
        private async Task<ERP.Data.Models.Benutzer> HoleOnlineDaten(System.Guid schlüssel)
        {
            this.Kontext.Log.StartMelden();
            var daten = new ERP.Data.Models.Benutzer();
            daten.Schlüssel = schlüssel;

            var gruppen = await this.OnlineAbrufController.HoleAufgabenGruppenAsync(daten);

            if (gruppen != null)
            {
                daten.Gruppen = gruppen;
                foreach (var gruppe in daten.Gruppen)
                {
                    gruppe.Aufgaben = await this.OnlineAbrufController.HoleAufgabenAsync(daten, gruppe.Name);
                }
            }
            this.Kontext.Log.EndeMelden();
            return daten;
        }

        #endregion Synchronisation: Online zu Offline

    }

}
