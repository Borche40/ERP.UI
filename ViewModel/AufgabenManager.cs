
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ERP.UI.Models;
using ERP.UI.Views;
using ERP.UI.Commands;
using ERP.UI.ViewModel;

namespace ERP.UI.ViewModel
{
    /// <summary>
    /// Stellt einen Dienst zum Verwalten
    /// der Daten des derzeit angemeldeten Benutzer bereit
    /// </summary>
    internal class AufgabenManager : BaseViewModel
    {
       
        
            #region Datendienst (Model)

            /// <summary>
            /// Internes Feld für die Eigenschaft
            /// </summary>
            private Models.DatenManager _DatenManager = null!;

            /// <summary>
            /// Ruft den Dienst zum Arbeiten
            /// mit den Benutzerdaten aus
            /// der Datenquelle ab
            /// </summary>
            protected Models.DatenManager DatenManager
            {
                get
                {
                    if (this._DatenManager == null)
                    {
                        this.Kontext.Log.StartMelden();

                        Verbindungsprüfer.EinstellungenEinrichten();

                        this._DatenManager = this.Kontext
                            .Produziere<Models.DatenManager>();

                        if (this._DatenManager.Benutzer == null)
                        {
                            this.IstBeschäftigt = true;
                        }

                        //Falls der DatenManager meldet,
                        //dass die Gruppen bereitstehen,
                        //PropertyChanged für die Gruppen auslösen
                        //und IstBeschäftigt wieder abschalten
                        this._DatenManager.GruppenAktualisiert
                            += (sender, e) =>
                            {
                                this.IstBeschäftigt = false;
                                this.OnPropertyChanged("Gruppen");
                                this.OnPropertyChanged("AktuelleGruppe");
                                this.OnPropertyChanged("Aufgaben");
                            };

                        this._DatenManager.AuthentifizierungAbgeschlossen +=
                             (sender, e) =>
                             {
                                 this.Oberfläche.Angemeldungläuft = false;
                                 if (e.Erfolgreich)
                                 {
                                     this.Oberfläche.IstAngemeldet = true;
                                     this.Authentifizierung.DatenLöschen();

                                 }
                                 else
                                 {
                                     this.Oberfläche.IstAngemeldet = false;
                                     this.Authentifizierung.Anmeldung.Nachricht = Views.Texte.AnmeldungFehler.ToString();

                                 }

                             };

                        this._DatenManager.AufgabenInitialisiert += (sender, e) =>
                        {
                            this.IstBeschäftigt = false;
                            this.OnPropertyChanged("Aufgaben");

                        };

                        this._DatenManager.SpeicherManager.SpeichernAbgeschlossen += (sender, e) =>
                        {
                            this.IstBeschäftigt = false;
                            if (e.Status == 0)
                            {
                                NachrichtBox.Anzeigen(Views.Texte.SchlüsselAbgelaufen, NachrichtTyp.Ok);

                            }
                        };


                        //Falls ein Fehler aufgetreten ist,
                        //sicherstellen, dass die Beschäftigt
                        //abgeschaltet wird
                        this._DatenManager.FehlerAufgetreten
                            += (sender, e)
                            => this.IstBeschäftigt = false;
                        this.Kontext.Log.EndeMelden();
                    }
                    return this._DatenManager;
                }
            }

            #endregion Datendienst

            #region OberflächeManager

            /// <summary>
            /// Internes Feld für die Eigenschaft
            /// </summary>
            private OberflächeManager _Oberfläche = null!;

            /// <summary>
            /// Ruft den Dienst zum verwalten der Benutzeroberfläche ab
            /// </summary>
            public OberflächeManager Oberfläche
            {
                get
                {
                    if (this._Oberfläche == null)
                    {
                        this._Oberfläche = this.Kontext.Produziere<OberflächeManager>();
                        this.Oberfläche.InternetVerbindungGeändert += (sender, e) =>
                        {
                            if (this.Oberfläche.IstAngemeldet)
                            {
                                this.DatenManager.OnlineAnmelden();
                            }
                        };
                    }
                    return this._Oberfläche;
                }
            }

            #endregion OberflächeManager

            #region Infromationen über die Aufgaben des Benutzers
            /// <summary>
            /// Ruft den Namen des Benutzers ab
            /// </summary>
            public string BenutzerName
            {
                get => (this.DatenManager?.Benutzer?.Name)!;

                private set
                {
                    this.DatenManager.Benutzer.Name = value;
                    this.OnPropertyChanged();
                }
            }

            /// <summary>
            /// Ruft das Email des Benutzers ab
            /// </summary>
            public string BenutzerEmail
            {
                get => (this.DatenManager?.Benutzer?.Email)!;

                private set
                {
                    this.DatenManager.Benutzer.Email = value;
                    this.OnPropertyChanged();
                }
            }

            /// <summary>
            /// Ruft die aktuelle AufgabenGruppe 
            /// in der derzeit gearbeitet wird, oder legt diese fest
            /// </summary>
            public ERP.Data.AufgabenGruppe? AktuelleGruppe
            {
                get => this.DatenManager.AktuelleGruppe;
                set
                {
                    this.DatenManager.AktuelleGruppe = value;
                    this.OnPropertyChanged();
                    this.OnPropertyChanged("Aufgaben");
                }
            }

            /// <summary>
            /// Ruft die AufgabenGruppen des aktuellen Benutzer ab
            /// oder legt diese fest
            /// </summary>
            public ERP.Data.AufgabenGruppen? Gruppen
            {
                get => this.DatenManager.Gruppen;
                set
                {
                    this.DatenManager.Gruppen = value!;
                    this.OnPropertyChanged();
                }
            }

            /// <summary>
            /// Ruft die Aufgaben der Aktuellen Gruppe ab
            /// oder legt diese fest
            /// </summary>
            public ERP.Data.Aufgaben? Aufgaben
            {
                get
                {
                    if (this.AktuelleGruppe != null)
                    {
                        if (this.AktuelleGruppe.Aufgaben == null)
                        {
                            this.IstBeschäftigt = true;
                            this.DatenManager.InitialisiereAufgaben();

                            this.AktuelleGruppe.Aufgaben = new ERP.Data.Aufgaben();
                        }
                    }
                    return this.AktuelleGruppe?.Aufgaben;
                }
                set
                {
                    this.AktuelleGruppe!.Aufgaben = value!;
                    this.OnPropertyChanged();
                }
            }


            #endregion Infromationen über die Aufgaben des Benutzers

            #region Zum arbeiten mit Benutzer daten

            /// <summary>
            /// Internes Feld für die Eigenschaft
            /// </summary>
            private BenutzerDaten _Daten = null!;

            /// <summary>
            /// Ruft die Daten für die neuen Aufgaben und Gruppen ab
            /// </summary>
            public BenutzerDaten Daten
            {
                get
                {
                    if (this._Daten == null)
                    {
                        this._Daten = new BenutzerDaten();
                    }
                    return this._Daten;
                }
            }


            #endregion Zum arbeiten mit Benutzer daten

            #region Authentifizierung

            /// <summary>
            /// Internes Feld für die Eigenschaft
            /// </summary>
            private AuthentifizierungsManager _Authentifizierung = null!;

            /// <summary>
            /// Stellt einen Dienst bereit zum Verwalten von Anmelde- und 
            /// Registrierungsdaten.
            /// </summary>
            public AuthentifizierungsManager Authentifizierung
            {
                get
                {
                    if (this._Authentifizierung == null)
                    {
                        this._Authentifizierung
                            = this.Kontext.Produziere<AuthentifizierungsManager>();
                    }
                    return this._Authentifizierung;
                }
            }

            #endregion Authentifizierung

            #region Benutzerverwaltung

            /// <summary>
            /// Internes Feld für die Eigenschaft
            /// </summary>
            private ERP.UI.Commands.Befehl _BenutzerAnmelden = null!;

            /// <summary>
            /// Ruft den Befehl zum Anmelden eines Benutzers ab
            /// </summary>
            public ERP.UI.Commands.Befehl BenutzerAnmelden
            {
                get
                {
                    if (this._BenutzerAnmelden == null)
                    {
                        this._BenutzerAnmelden = new ERP.UI.Commands.Befehl(d =>
                        {
                            this.Oberfläche.Angemeldungläuft = true;
                            var Anmeldung = this.Authentifizierung.HoleAnmeldungsDaten();

                            if (Anmeldung != null)
                            {

                                this.DatenManager.Anmelden(Anmeldung);
                            }
                            ERP.UI.Properties.Settings.Default.AngemeldetBleiben = false;

                        });
                    }
                    return this._BenutzerAnmelden;
                }
            }

            /// <summary>
            /// Internes Feld für die Eigenschaft
            /// </summary>
            private ERP.UI.Commands.Befehl _BenutzerRegistrieren = null!;

            /// <summary>
            /// Ruft den Befehl zum Registrieren eines Benutzers ab
            /// </summary>
            public ERP.UI.Commands.Befehl BenutzerRegistrieren
            {
                get
                {
                    if (this._BenutzerRegistrieren == null)
                    {
                        this._BenutzerRegistrieren = new ERP.UI.Commands.Befehl(d =>
                        {
                            this.Registrieren();

                        });
                    }
                    return this._BenutzerRegistrieren;
                }
            }

            /// <summary>
            /// Registriert einen neuen Benutzer falls die Daten dafür vorhaden sind.
            /// </summary>
            private async void Registrieren()
            {
                this.IstBeschäftigt = true;
                Verbindungsprüfer.EinstellungenEinrichten();
                if (ERP.UI.Properties.Settings.Default.Offline)
                {
                    NachrichtBox.Anzeigen(Views.Texte.RegistrierenWarnung, NachrichtTyp.Ok);
                }
                else
                {
                    var Registrierung = this.Authentifizierung.HoleRegistrierungsDaten();
                    if (Registrierung != null)
                    {
                        this.Oberfläche.Angemeldungläuft = true;
                        var r = await this.DatenManager.Registrieren(Registrierung!);
                        this.Oberfläche.Angemeldungläuft = false;
                        if (r == 1)
                        {
                            var result = NachrichtBox.Anzeigen
                                 (Views.Texte.RegistrierungErfolgreich, NachrichtTyp.JaNein);
                            if (result == MessageBoxResult.Yes)
                            {
                                this.Oberfläche.Angemeldungläuft = true;

                                this.DatenManager.Anmelden
                                    (new ERP.Data.Anmeldung
                                    { Email = Registrierung.Email, Passwort = Registrierung.Passwort });
                            }
                        }
                        else
                        {
                            NachrichtBox.Anzeigen(Views.Texte.RegistrierungFehler, NachrichtTyp.Ok);
                        }
                        this.IstBeschäftigt = false;
                    }
                }
            }

            /// <summary>
            /// Meldet einen Benutzer mit den Daten aus den Einstellungen.
            /// </summary>
            public void AnmeldenMitEinstellungen()
            {

                var Email = ERP.UI.Properties.Settings.Default.Email;
                var Passwort = ERP.UI.Properties.Settings.Default.Passwort;

                if (!string.IsNullOrEmpty(Email) && !string.IsNullOrEmpty(Passwort))
                {
                    var Anmeldung = new ERP.Data.Anmeldung
                    {
                        Email = Email,
                        Passwort = Passwort
                    };
                    this.Oberfläche.Angemeldungläuft = true;
                    this.DatenManager.Anmelden(Anmeldung);
                }
            }



            /// <summary>
            /// Internes Feld für die Eigenschaft
            /// </summary>
            private ERP.UI.Commands.Befehl _BenutzerAbmelden = null!;

            /// <summary>
            /// Ruft den Befehl zum Abmelden des aktuell angemeldeten Benutzer ab
            /// </summary>
            public ERP.UI.Commands.Befehl BenutzerAbmelden
            {
                get
                {
                    if (this._BenutzerAbmelden == null)
                    {
                        this._BenutzerAbmelden = new ERP.UI.Commands.Befehl(d =>    
                        {
                            this.Oberfläche.IstAngemeldet = false;
                            if (this.Gruppen != null && this.AktuelleGruppe != null)
                            {
                                this.Aufgaben = null!;
                                this.Gruppen = null!;
                                this.AktuelleGruppe = null!;
                                this.BenutzerEmail = null!;
                                this.BenutzerName = null!;
                                this.DatenManager!.Benutzer = null!;
                            }
                            ERP.UI.Properties.Settings.Default.Passwort = null;
                            ERP.UI.Properties.Settings.Default.Email = null;
                            ERP.UI.Properties.Settings.Default.AngemeldetBleiben = false;
                        });
                    }
                    return this._BenutzerAbmelden;
                }
            }
            #endregion Benutzerverwaltung

            #region Bearbeiten der Aufgaben und Gruppen

            /// <summary>
            /// Internes Feld für die Eigenschaft
            /// </summary>
            private ERP.UI.Commands.Befehl _GruppeHinzufügen = null!;

            /// <summary>
            /// Ruft den Befehl zum Hinzufügen von neuen Gruppen ab
            /// </summary>
            public ERP.UI.Commands.Befehl GruppeHinzufügen
            {
                get
                {
                    if (this._GruppeHinzufügen == null)
                    {
                        this._GruppeHinzufügen =
                            new ERP.UI.Commands.Befehl(d =>
                            {
                                var gruppe = this.Daten.HoleNeueGruppe();
                                if (gruppe != null)
                                {
                                    this.DatenManager.GruppeHinzufügen(gruppe);

                                }
                                this.Daten.Gruppe = null!;

                            });
                    }
                    return this._GruppeHinzufügen;
                }
            }

            /// <summary>
            /// Internes Feld für die Eigenschaft
            /// </summary>
            private ERP.UI.Commands.Befehl _AufgabeHinzufügen = null!;

            /// <summary>
            /// Ruft den Befehl zum Hinzufügen 
            /// von neuen Aufgaben für die Aktuelle Gruppe ab
            /// </summary>
            public ERP.UI.Commands.Befehl AufgabeHinzufügen
            {
                get
                {
                    if (this._AufgabeHinzufügen == null)
                    {
                        this._AufgabeHinzufügen = new ERP.UI.Commands.Befehl(d =>
                        {
                            var aufgabe = this.Daten.HoleNeueAufgabe();
                            if (aufgabe != null)
                            {
                                if (this.Aufgaben != null && this.Aufgaben.Count > 0)
                                {
                                    aufgabe.ID = this.Aufgaben.Max(a => a.ID) + 1;
                                }
                                else
                                {
                                    aufgabe.ID = 1;
                                }

                                this.DatenManager.AufgabenHinzufügen(aufgabe);
                                this.Daten.Inhalt = null!;
                                this.Daten.Zeit = null!;

                            }

                        }, d => !(this.AktuelleGruppe == null));//Falls es keine AktuelleGruppe gibt, kann auch keine Aufgaben hinzufügen
                    }
                    return this._AufgabeHinzufügen;
                }
            }

            /// <summary>
            /// Internes Feld für die Eigenschaft
            /// </summary>
            private ERP.UI.Commands.Befehl _StatusÄndern = null!;

            /// <summary>
            /// Ruft den Befehl zum Aktualisiern des Status einer Aufgabe
            /// </summary>
            public ERP.UI.Commands.Befehl StatusÄndern
            {
                get
                {
                    if (this._StatusÄndern == null)
                    {
                        this._StatusÄndern = new ERP.UI.Commands.Befehl(d =>
                        {
                            if (d is ERP.Data.Aufgabe aufgabe)
                            {
                                this.DatenManager.StatusAktualisieren(aufgabe);
                                this.Aufgaben?.AktualisiereAufgaben();

                            }
                        });
                    }
                    return this._StatusÄndern;
                }
            }

            /// <summary>
            /// Internes Feld für die Eigenschaft
            /// </summary>
            private ERP.UI.Commands.Befehl _LöscheAufgabe = null!;

            /// <summary>
            /// Ruft den Befehl zum löschen einer Aufgabe 
            /// aus der AktuellenGruppe Aufgaben liste ab
            /// </summary>
            public ERP.UI.Commands.Befehl LöscheAufgabe
            {
                get
                {
                    if (this._LöscheAufgabe == null)
                    {
                        this._LöscheAufgabe = new ERP.UI.Commands.Befehl(d =>
                        {
                            if (d is ERP.Data.Aufgabe aufgabe)
                            {
                                this.DatenManager?.AufgabeLöschen(aufgabe);
                                //todo pitaj prije brisanja
                            }
                        });
                    }
                    return this._LöscheAufgabe;
                }
            }


            /// <summary>
            /// Internes Feld für die Eigenschaft
            /// </summary>
            private ERP.UI.Commands.Befehl _LöscheGruppe = null!;

            /// <summary>
            /// Ruft den Befehl zum löschen einer Gruppe 
            /// aus der Gruppen liste ab
            /// </summary>
            public ERP.UI.Commands.Befehl LöscheGruppe
            {
                get
                {
                    if (this._LöscheGruppe == null)
                    {
                        this._LöscheGruppe = new ERP.UI.Commands.Befehl(d =>
                        {
                            if (d is ERP.Data.AufgabenGruppe gruppe)
                            {
                                if (0 == string.Compare(this.AktuelleGruppe?.Name, gruppe.Name))
                                {
                                    var gruppeIndex = this.Gruppen?.IndexOf(gruppe);

                                    if (gruppeIndex != null)
                                    {
                                        if (gruppeIndex > 0)
                                        {
                                            this.AktuelleGruppe = this.Gruppen?[(int)gruppeIndex - 1];
                                        }
                                        else if (this.Gruppen?.Count > 1)
                                        {
                                            this.AktuelleGruppe = this.Gruppen?[(int)gruppeIndex + 1];

                                        }
                                    }
                                }
                                this.DatenManager?.GruppeLöschen(gruppe);

                            }
                        });
                    }
                    return this._LöscheGruppe;
                }
            }

            #endregion Bearbeiten der Aufgaben

            #region Anmeldeeinstellung

            /// <summary>
            /// Ruft einen Wahrheitswert ab,
            /// der bestimmt, ob die Anwendung die Benutzerdaten merken soll
            /// oder nicht, und legt dieses fest
            /// </summary>
            public bool SollAngemeldetSein
            {
                get => ERP.UI.Properties.Settings.Default.AngemeldetBleiben;
                set
                {
                    if (value == false)
                    {
                        ERP.UI.Properties.Settings.Default.Email
                               = null!;
                        ERP.UI.Properties.Settings.Default.Passwort
                        = null!;
                    }
                    else
                    {
                        ERP.UI.Properties.Settings.Default.Email
                               = this._DatenManager?.Benutzer?.Email;
                        ERP.UI.Properties.Settings.Default.Passwort
                        = this._DatenManager?.Benutzer?.Passwort;
                    }
                    ERP.UI.Properties.Settings.Default.AngemeldetBleiben = value;
                    this.OnPropertyChanged();
                }
            }


            #endregion Anmeldeeinstellung

        }
    }

