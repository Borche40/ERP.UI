using ERP.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Media.Animation;
using ERP.UI.ViewModel;

namespace ERP.UI.Models
{
    /// <summary>
    /// Stellt einen Dienst zum Verwalten
    /// der Demo Daten bereit 
    /// </summary>
    internal class DatenManager : Anwendung.AppObjekt
    {
        #region Controller Dienst

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private IDatenController _OfflineController = null!;

        /// <summary>
        /// Ruft den Dienst zum zugriff auf die Offline-Datenbank
        /// </summary>
        /// <remarks>Wird nur benutzt wenn die Anwendung online ist, 
        /// damit die Anwendung einen Schlüssel für die Offline daten hat</remarks>
        private IDatenController OfflineController
        {
            get
            {
                if (this._OfflineController==null)
                {
                    this._OfflineController = this.Kontext.Produziere<LokalerDatenController>();
                }

                return this._OfflineController;
            }
        }

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private IDatenController _Controller = null!;

        /// <summary>
        /// Ruft den Dienst zum Lesen
        ///  der Demo 
        /// Daten ab
        /// </summary>
        /// <remarks>Falls die Online Konfigration
        /// False ist, werden die Daten lokal bezogen,
        /// sonst über einen Webdienst</remarks>
        private IDatenController Controller
        {
            get
            {
                if (this._Controller == null)
                {
                    this._Controller
                        = this.Kontext
                            .Produziere<OnlineDatenController>();
                    if (this._Controller is OnlineDatenController onlineController)
                    {
                        onlineController.VerbindungFehlgeschlagen += (sender, e) =>
                        {
                            this.VerbindungPrüfenLäuft = true;
                            this.IstOffline = true;
                            if (this.Benutzer == null)
                            {
                                this.Anmelden(this.CacheDaten);
                            }
                        };
                    }

                }

                return this._Controller;
            }
        }

        #endregion Controller Dienst

        #region Benutzer

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private ERP.Data.Models.Benutzer _Benutzer = null!;

        /// <summary>
        /// Ruft die Informationen des Aktuellen Benutzer ab 
        /// oder legt diese fest
        /// </summary>
        public  ERP.Data.Models.Benutzer Benutzer
        {
            get => this._Benutzer;
            set
            {
                if (value != null)
                {
                    this._Benutzer = value;
                    //Erst wenn der Benutzer angemeldet ist die Daten holen
                    this.InitialisiereGruppen();
                }

            }
        }

        #endregion Benutzer

        #region Benutzer Einstellungen
        /// <summary>
        /// Ruft True ab, wenn InitialisiereGruppen
        /// arbeitet, sonst false
        /// </summary>
        private bool InitialisiereGruppenLäuft { get; set; } = false;

        /// <summary>
        /// Ruft die Benutzer AufgabenGruppen aus der Quelle ab
        /// </summary>
        private async void InitialisiereGruppen()
        {
            if (this.InitialisiereGruppenLäuft) return;

            this.Kontext.Log.StartMelden();

            try
            {
                this.InitialisiereGruppenLäuft = true;
                if (this.IstOffline)
                {
                    this.Gruppen =
                                await this.OfflineController.HoleAufgabenGruppenAsync(this.Benutzer);
                }
                else
                {
                    ERP.Data.AufgabenGruppen? gruppen =
                                        await this.Controller.HoleAufgabenGruppenAsync(this.Benutzer);
                    if (gruppen is null)
                    {
                        this.Gruppen =
                                await this.OfflineController.HoleAufgabenGruppenAsync(this.Benutzer);

                    }
                    else
                    {
                        this.Gruppen = gruppen;
                    }
                }
            }
            catch (Exception ex)
            {
                this.OnFehlerAufgetreten(
                    new Anwendung.FehlerAufgetretenEventArgs(ex));
            }
            finally
            {
                this.InitialisiereGruppenLäuft = false;
            }

            this.Kontext.Log.EndeMelden();
        }

        #endregion Benutzer Einstellungen

        #region Aufgabengruppen

        /// <summary>
        /// Ruft die Aufgabengruppen des Aktuellen Benutzers ab
        /// oder legt diese fest
        /// </summary>
        internal ERP.Data.AufgabenGruppen? Gruppen
        {
            get
            {
                if (this.Benutzer != null)
                {
                    if (this.Benutzer.Gruppen == null)
                    {
                        this.Benutzer.Gruppen = new ERP.Data.AufgabenGruppen();
                    }
                }
                return this.Benutzer?.Gruppen;
            }

            set
            {
                this.Benutzer.Gruppen = value!;
                this.OnGruppenAktualisiert(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Wird ausgelöst, wenn InitialisiereAufgaben fertig ist
        /// </summary>
        public event System.EventHandler? AufgabenInitialisiert = null!;

        /// <summary>
        /// Löst das Ereignis AufgabenInitialisiert aus
        /// </summary>
        /// <param name="e">Ereignisdaten</param>
        protected virtual void OnAufgabenInitialisiert(System.EventArgs e)
        {
            this.AufgabenInitialisiert?.Invoke(this, e);
        }

        /// <summary>
        /// Wird ausgelöst, wenn sich der Inhalt
        /// der Gruppen Eigenschaft verändert hat
        /// </summary>
        public event System.EventHandler? GruppenAktualisiert = null!;

        /// <summary>
        /// Löst das Ereignis GruppenAktualisiert aus
        /// </summary>
        /// <param name="e">Keine weiteren Zusatzdaten</param>
        protected virtual void OnGruppenAktualisiert(System.EventArgs e)
        {
            var BehandlerKopie = this.GruppenAktualisiert;

            BehandlerKopie?.Invoke(this, e);
        }

        #endregion Aufgabengruppen

        #region Aktuelle Gruppe

        /// <summary>
        /// Ruft die Aktuelle AufgabenGruppe des Benutzers ab oder legt
        /// diese fest
        /// </summary>
        public ERP.Data.AufgabenGruppe? AktuelleGruppe
        {
            get
            {
                //Falls die Benutzer AufgabenGruppen geladen sind
                //und die Aktuelle Gruppe keinen Wert hat
                if (this.Benutzer?.Gruppen?.Count > 0)
                {
                    if (this.Benutzer.AktuelleGruppe == null)
                    {
                        //nur dann soll die Aktuele Gruppe
                        //die erste aus der liste der Gruppen sein
                        this.Benutzer.AktuelleGruppe = this.Benutzer.Gruppen.FirstOrDefault()!;
                        this.InitialisiereAufgaben(); //und die Daten für die Gruppe sollen geholt werden
                    }
                }
                return this.Benutzer?.AktuelleGruppe;
            }

            set
            {
                this.Benutzer.AktuelleGruppe = value!;
                //immer wenn sich die Aktuelle Gruppe
                //ändert und die Aufgaben nicht bereits vorhanden sind
                //sollen die daten geholt werden
                if (this.Benutzer.AktuelleGruppe?.HatDaten == false)
                    this.InitialisiereAufgaben();
            }
        }

        /// <summary>
        /// Ruft True ab, wenn InitialisiereAufgaben
        /// arbeitet, sonst false
        /// </summary>
        private bool InitialisiereAufgabenLäuft { get; set; } = false;


        /// <summary>
        /// Ruft die Aufgaben der AktuellenGruppe aus der Quelle ab
        /// </summary>
        public async void InitialisiereAufgaben()
        {
            if (this.InitialisiereAufgabenLäuft) return;

            this.Kontext.Log.StartMelden();

            try
            {
                this.InitialisiereAufgabenLäuft = true;
                if (this.IstOffline)
                {
                    this.Benutzer!.AktuelleGruppe!.Aufgaben
                             = await this.OfflineController.HoleAufgabenAsync(this.Benutzer, this.Benutzer.AktuelleGruppe.Name);
                }
                else
                {
                    ERP.Data.Aufgaben? aufgaben
                         = await this.Controller.HoleAufgabenAsync(this.Benutzer!, this.Benutzer.AktuelleGruppe!.Name);

                    if (aufgaben == null)
                    {
                        this.Benutzer!.AktuelleGruppe!.Aufgaben
                             = await this.OfflineController.HoleAufgabenAsync(this.Benutzer!, this.Benutzer.AktuelleGruppe.Name);
                    }
                    else
                    {
                        this.Benutzer!.AktuelleGruppe!.Aufgaben = aufgaben;
                    }
                }
            }
            catch (Exception ex)
            {
                this.OnFehlerAufgetreten(
                    new Anwendung.FehlerAufgetretenEventArgs(ex));
            }
            finally
            {
                this.InitialisiereAufgabenLäuft = false;
            }
            this.OnAufgabenInitialisiert(EventArgs.Empty);
            if (this.Benutzer.AktuelleGruppe != null)
            {
                this.Benutzer.AktuelleGruppe.HatDaten = true; //Die Daten worden geholt

            }
            this.Kontext.Log.EndeMelden();
        }


        #endregion Aktuelle Gruppe

        #region Benutzerverwaltung

        /// <summary>
        /// Melden den Benutzer Online an falls es bei dem 
        /// erstem zugriff auf die Daten nicht möglich war
        /// </summary>
        public async void OnlineAnmelden()
        {
            this.Kontext.Log.StartMelden();
            if (this.IstOffline == false)
            {
                if (this.IstOnlineAngemeldet == false)
                {
                    var benutzer = await this.Controller.Anmelden(this.CacheDaten);
                    if (benutzer != null)
                    {
                        this.Benutzer.Schlüssel = (System.Guid)benutzer.Schlüssel!;
                        this.IstOnlineAngemeldet = true;
                    }
                    this.SpeicherManager.HubVerbindung.VerbindungHerstellen(this.Benutzer.Email);
                }
                if (this.IstOnlineAngemeldet)
                {
                    this.SpeicherManager.SyncManager.
                        Synchronisieren((System.Guid)this.Benutzer.Schlüssel!, this.Benutzer.LokalerSchlüssel);
                }
            }
            else
            {
                this.SpeicherManager.HubVerbindung.VerbindungHerstellen(this.Benutzer.Email);
            }
            this.Kontext.Log.EndeMelden();

        }


        /// <summary>
        /// Ereignis, das ausgelöst wird, 
        /// wenn eine Authentifizierungsaktion abgeschlossen ist.
        /// </summary>
        public event EventHandler<AuthentifizierungEventArgs>? AuthentifizierungAbgeschlossen = null!;

        /// <summary>
        /// Löst das Ereignis aus, das signalisiert, 
        /// dass eine Authentifizierungsaktion abgeschlossen ist.
        /// </summary>
        /// <param name="e">Die Ereignisdaten, 
        /// die Informationen über den Abschluss der Authentifizierungsaktion enthalten.</param>
        protected virtual void OnAuthentifizierungAbgeschlossen(AuthentifizierungEventArgs e)
        {
            var behandlerKopie = AuthentifizierungAbgeschlossen;
            behandlerKopie?.Invoke(this, e);
        }

        /// <summary>
        /// Damit falls keine Verbindung mit dem Server möglich ist, 
        /// eine Verbindung zur Lokalen-Datenbank ermöglicht wird
        /// </summary>
        private Anmeldung CacheDaten = null!;

        /// <summary>
        /// Ruft den Warheitswert ab der 
        /// beschreibt ob die Anwendung Offline ist
        /// </summary>
        private  bool IstOffline
        {
            get => ERP.UI.Properties.Settings.Default.Offline;
            set
            {
                ERP.UI.Properties.Settings.Default.Offline = value;
            }
        }

        /// <summary>
        /// Ruft die Kopie der Benutzer-Daten ab oder legt diese fest
        /// </summary>
        /// <remarks>Wird benötigt falls die Synchronisation läuft</remarks>
        private ERP.Data.Models.Benutzer BenutzerKopie { get; set; } = null!;

        /// <summary>
        /// Beschreibt, ob die Verbindung mit dem Server überprüft wird.
        /// </summary>
        private bool VerbindungPrüfenLäuft { get; set; } = false;

        /// <summary>
        /// Beschreibt, ob der Benutzer einen Online-Schlüssel hat.
        /// </summary>
        private bool IstOnlineAngemeldet { get; set; } = false;

        /// <summary>
        /// Führt einen Anmeldevorgang durch
        /// </summary>
        /// <param name="anmeldung">Die Anmeldeinformationen für den Anmeldevorgang.</param>
        public async void Anmelden(ERP.Data.Anmeldung anmeldung)
        {
            this.Kontext.Log.StartMelden();
            //Hole den Benutzer aus der Quelle, wird null sein falls kein Benutzer mit
            //dem Email und Passwort vorhanden ist.
            this.CacheDaten = anmeldung;

            ERP.Data.Models.Benutzer benutzer=null!;

            if (this.IstOffline || this.VerbindungPrüfenLäuft)
            {
                var LokalerBenutzer=await this.OfflineController.Anmelden(anmeldung);
                if (LokalerBenutzer != null)
                {
                    LokalerBenutzer.LokalerSchlüssel = (System.Guid)LokalerBenutzer.Schlüssel!;
                    LokalerBenutzer.Schlüssel = null!;
                    benutzer= LokalerBenutzer;
                }
            }
            else
            {
                var OnlineBenutzer = await this.Controller.Anmelden(anmeldung);
                var LokaleDaten=await this.OfflineController.Anmelden(anmeldung);
                if (OnlineBenutzer != null && LokaleDaten!=null)
                {
                    benutzer = OnlineBenutzer;
                    benutzer.LokalerSchlüssel=(System.Guid)LokaleDaten.Schlüssel!;
                    this.IstOnlineAngemeldet = true;
                    this.OnAuthentifizierungAbgeschlossen(new AuthentifizierungEventArgs(true));
                    if (ERP.UI.Properties.Settings.Default.WarOffline && benutzer != null)
                    {
                        this.SpeicherManager.SyncManager.
                            Synchronisieren((System.Guid)benutzer.Schlüssel!, benutzer.LokalerSchlüssel);
                    }
                    else if (!ERP.UI.Properties.Settings.Default.WarOffline && benutzer != null)
                    {
                        this.SpeicherManager.SyncManager.
                            LokaleDatenSynchronisieren((System.Guid)benutzer.Schlüssel!, benutzer.LokalerSchlüssel);
                    }
                }
            }

            if (benutzer != null)
            {
                this.SpeicherManager.BenutzerEmail = benutzer.Email;
                if (!ERP.UI.Properties.Settings.Default.WarOffline || this.IstOffline)
                {
                    this.Benutzer = benutzer;
                    this.OnAuthentifizierungAbgeschlossen(new AuthentifizierungEventArgs(true));
                    this.SpeicherManager.HubVerbindung.VerbindungHerstellen(this.Benutzer.Email);
                }
                else
                {
                    this.BenutzerKopie=benutzer;
                }
            }
            else //Falls der Benutzer nicht in der TaskManager.2024 db vorhanden ist
            {
                if (this.VerbindungPrüfenLäuft == false)
                {
                    this.OnAuthentifizierungAbgeschlossen(new AuthentifizierungEventArgs(false));
                }
            }
            if (this.IstOffline)
            {
                ERP.UI.Properties.Settings.Default.WarOffline = true;
            }
            this.VerbindungPrüfenLäuft = false;
            this.Kontext.Log.EndeMelden();
        }

        /// <summary>
        /// Führt einen Registrierungsvorgang durch.
        /// </summary>
        /// <param name="registrierung">Die Registrierungsinformationen
        /// für den Registrierungsvorgang.</param>
        /// <returns>Gibt 1 züruck falls die Registrierung erfolgreich war, oder 0 falls nicht</returns>
        public async Task<int> Registrieren(ERP.Data.Registrierung registrierung)
        {
            this.Kontext.Log.StartMelden();

            var IstRegistriert = await this.Controller.Registrieren(registrierung);
            if (IstRegistriert != 2)
            {
                var offlineAntwort = await this.OfflineController.Registrieren(registrierung);
                if (offlineAntwort == 0)
                {
                    IstRegistriert = 0;
                }
            }
            else
            {
                return 0;
            }
            this.Kontext.Log.EndeMelden();

            return IstRegistriert;
        }

        #endregion Benutzerverwaltung

        #region SpeicherManager

        /// <summary>
        /// Internes Feld für die Eigenschaft
        /// </summary>
        private SpeicherManager _SpeicherManager = null!;

        /// <summary>
        /// Ruft den Dienst zum Speichern von Benutzer daten ab
        /// </summary>
        public SpeicherManager SpeicherManager
        {
            get
            {
                if (this._SpeicherManager == null)
                {
                    this._SpeicherManager = this.Kontext.Produziere<SpeicherManager>();
                    this._SpeicherManager.SyncManager.SynchronisationAbgeschlossen += (sender, e) =>
                    {
                        if (e.Status == 0)
                        {
                            this._SpeicherManager.OnSpeichernAbgeschlossen(new SpeichernEventArgs(0));
                        }
                        else
                        {
                            if (this.Benutzer == null)
                            {
                                this.Benutzer = this.BenutzerKopie;
                                this.BenutzerKopie = null!;
                            }
                            this.SpeicherManager.HubVerbindung.VerbindungHerstellen(this.Benutzer.Email);
                            this.OnAuthentifizierungAbgeschlossen(new AuthentifizierungEventArgs(true));
                            this.SpeicherManager.HubVerbindung.GruppenAktualisieren(this.Benutzer.Email);

                            if (this.Gruppen != null)
                            {
                                foreach (var gruppe in this.Gruppen)
                                {
                                    if (gruppe.Aufgaben != null)
                                    {
                                        this.SpeicherManager.HubVerbindung.
                                        AufgabenAktualisieren(gruppe.Name, this.Benutzer.Email);
                                    }
                                }
                            }
                            ERP.UI.Properties.Settings.Default.WarOffline = false;

                        }
                    };
                    this._SpeicherManager.SyncManager.LokaleSynchronisationAbgeschlossen += (sender, e) =>
                    {
                        if (e.Status == 0)
                        {
                            this._SpeicherManager.OnSpeichernAbgeschlossen(new SpeichernEventArgs(0));
                        }
                    };
                    this._SpeicherManager.HubVerbindung.AufgabenAktualisiert += (sender, e) =>
                    {
                        if (e.Gruppe != null)
                        {
                            if (this.Gruppen != null)
                            {
                                if (!this._SpeicherManager.HubVerbindung.IstSender || !this.IstOffline)
                                {
 
                                    this.HoleAufgaben(e);
                                }
                            }
                        }
                        this._SpeicherManager.HubVerbindung.IstSender = false;

                    };
                    this._SpeicherManager.HubVerbindung.GruppenAktualisiert += (sender, e) =>
                    {
                        if (this.Gruppen != null)
                        {
                            if (!this._SpeicherManager.HubVerbindung.IstSender || !this.IstOffline)
                            {
                                this.HoleGruppen();
                            }
                        }

                        this._SpeicherManager.HubVerbindung.IstSender = false;

                    };
                }
                return this._SpeicherManager;
            }
        }

        #endregion SpeicherManager

        #region Datenverwaltung

        /// <summary>
        /// Löscht eine Aufgabe aus der AktuellenGruppe
        /// </summary>
        /// <param name="aufgabe">Die Aufgabe die gelöscht werden soll</param>
        public void AufgabeLöschen(ERP.Data.Aufgabe aufgabe)
        {
            if (this.AktuelleGruppe != null)
            {
                this.SpeicherManager.AufgabeLöschen
                    (this.AktuelleGruppe.Name,(System.Guid)this.Benutzer.Schlüssel!,this.Benutzer.LokalerSchlüssel,aufgabe);
                this.AktuelleGruppe.Aufgaben?.Remove(aufgabe);
            }
        }

        /// <summary>
        /// Löscht eine Gruppe aus der Gruppen liste des Benutzers
        /// </summary>
        /// <param name="gruppe">Die Gruppe die gelöscht werden soll</param>
        public void GruppeLöschen(ERP.Data.AufgabenGruppe gruppe)
        {
            this.SpeicherManager.GruppeLöschen(this.Benutzer.Schlüssel,this.Benutzer.LokalerSchlüssel, gruppe.Name);
            this.Gruppen?.Remove(gruppe);
        }

        /// <summary>
        /// Aktualisiert den Status einer Aufgabe
        /// </summary>
        /// <param name="aufgabe">Die Aufgabe, deren Status aktualisiert werden soll.</param>
        public void StatusAktualisieren(ERP.Data.Aufgabe aufgabe)
        {
            if (this.AktuelleGruppe != null)
            {
                this.SpeicherManager.AufgabeSpeichern
                    (this.AktuelleGruppe.Name,this.Benutzer.Schlüssel, aufgabe, this.Benutzer.LokalerSchlüssel);

            }
        }

        /// <summary>
        /// Fügt der Liste der Aufgabengruppen eine neue Gruppe hinzu.
        /// </summary>
        /// <param name="gruppe">Die AufgabenGruppe, die hinzugefügt werden soll.</param>
        public void GruppeHinzufügen(ERP.Data.AufgabenGruppe gruppe)
        {
            if (this.Benutzer.Gruppen.Any(g => string.Compare(g.Name, gruppe.Name, ignoreCase: true) == 0))
            {
                gruppe.Name = gruppe.Name + $"({this.ErmittleGruppeNameNummer(gruppe.Name)})";

            }

            if (this.Gruppen != null)
            {
                this.Gruppen.Add(gruppe);
                if (this.Gruppen?.Count == 1)
                {
                    this.AktuelleGruppe = this.Gruppen?.First();
                    this.OnGruppenAktualisiert(EventArgs.Empty);
                }
                this.SpeicherManager.GruppeSpeichern(this.Benutzer.Schlüssel, this.Benutzer.LokalerSchlüssel, gruppe.Name);
            }

        }

        /// <summary>
        /// Fügt der Liste der Aufgaben der AktuellenGruppe eine neue Aufgabe hinzu.
        /// </summary>
        /// <param name="gruppe">Die Aufgabe, die hinzugefügt werden soll.</param>
        public void AufgabenHinzufügen(ERP.Data.Aufgabe aufgabe)
        {

            this.AktuelleGruppe?.Aufgaben?.Add(aufgabe);
            if (this.AktuelleGruppe != null)
            {
                this.SpeicherManager.AufgabeSpeichern
                    (this.AktuelleGruppe.Name, this.Benutzer.Schlüssel, aufgabe, this.Benutzer.LokalerSchlüssel);
            }
        }

        /// <summary>
        /// Gibt die erste freie laufende Nummer
        /// für den Gruppen Namen zurück
        /// </summary>
        /// <param name="name">die Namen Eigenschaft der Gruppe,
        /// das für den Benutzer unterschieden
        /// werden soll</param>
        private int ErmittleGruppeNameNummer(string name)
        {
            this.Kontext.Log.StartMelden();
            int Nummer = 1;

            if (this.Benutzer.Gruppen.Count > 0)
            {
                var NamenListe = new System.Collections.ArrayList(this.Benutzer.Gruppen.Count());

                foreach (ERP.Data.AufgabenGruppe gruppe in this.Benutzer.Gruppen)
                {
                    NamenListe.Add(gruppe.Name.ToLower());
                }

                string lowercasedName = name.ToLower();

                while (NamenListe.Contains(lowercasedName + $"({Nummer})"))
                {
                    Nummer++;
                }
            }

            this.Kontext.Log.EndeMelden();
            return Nummer;
        }


        #endregion Datenverwaltung

        #region Hubverbindung

        /// <summary>
        /// Holt die Aufgaben für eine durch den Event bestimmte grupe.
        /// </summary>
        /// <param name="e">Das Event das die Gruppe enthält die sich bei einem anderem Benutzer geändert hat</param>
        private async void HoleAufgaben(HubEventArgs e)
        {
            if (this.IstOffline)
            {
                this.Gruppen!.FirstOrDefault(gruppe => 0 == string.Compare(gruppe.Name, e.Gruppe))!.Aufgaben 
                    = await this.OfflineController.HoleAufgabenAsync(this.Benutzer, e.Gruppe);
            }
            else
            {
                var aufgaben= await this.Controller.HoleAufgabenAsync(this.Benutzer, e.Gruppe);
                if (aufgaben != null)
                {
                    this.Gruppen!.FirstOrDefault(gruppe => 0 == string.Compare(gruppe.Name, e.Gruppe))!.Aufgaben = aufgaben;
                }
            }
            this.OnAufgabenInitialisiert(EventArgs.Empty);
        }

        /// <summary>
        /// Holt die Gruppen des Benutzers aus der Quelle.
        /// </summary>
        /// <remarks>Falls ein anderer Benutzer auf dem selben Konto die Daten gleichzeitig bearbeitet
        /// wird diese Methode für die aktualisierung der Daten verwendet.</remarks>
        private async void HoleGruppen()
        {
            ERP.Data.AufgabenGruppen? gruppen;
            if (this.IstOffline)
            {
                gruppen = await this.OfflineController.HoleAufgabenGruppenAsync(this.Benutzer);
            }
            else
            {
                gruppen = await this.Controller.HoleAufgabenGruppenAsync(this.Benutzer);
            }
             
            #region Gruppe hinzufügen
            if (gruppen != null)
            {
                // Neue Gruppen zu this.Gruppen hinzufügen
                foreach (var gruppe in gruppen)
                {
                    bool existiert = false;
                    foreach (var existingGruppe in this.Gruppen!)
                    {
                        if (0 == string.Compare(gruppe.Name, existingGruppe.Name))
                        {
                            existiert = true;
                            break;
                        }
                    }

                    if (!existiert)
                    {
                        this.Gruppen.Add(gruppe);
                    }
                }

                #endregion Gruppe hinzufügen

                #region Gruppe entfernen
                // Erstellen einer Liste mit den Namen der Gruppen in gruppen
                var gruppenNamen = gruppen.Select(gruppe => gruppe.Name).ToList();

                // Alle Gruppen in this.Gruppen auswählen, deren Name nicht in gruppen enthalten ist
                var gruppenZumEntfernen = this.Gruppen?.Where(gruppe => !gruppenNamen.Contains(gruppe.Name)).ToList();
                if (gruppenZumEntfernen != null)
                    foreach (var gruppe in gruppenZumEntfernen)
                    {
                        this.Gruppen?.Remove(gruppe); // Entferne jede gefundene Gruppe aus this.Gruppen
                    }
            }
            #endregion Gruppe entfernen

            this.OnGruppenAktualisiert(EventArgs.Empty);
        }
        #endregion Hubverbindung

    }
}
