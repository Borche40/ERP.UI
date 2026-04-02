using Anwendung.Werkzeuge;
using ERP.Core.Services;
using ERP.Data.Models;
using ERP.UI.Commands;
using ERP.UI.Views;
using ERP.UI.ViewModel;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using ERP.UI.Services;
using System.Collections.ObjectModel;
using DocumentFormat.OpenXml.Drawing;

namespace ERP.UI.ViewModel
{
    /// <summary>
    /// Repräsentiert das Haupt-Dashboard der ERP-Anwendung.
    /// Steuert Kennzahlen, Status und zwei Diagramme (Umsatz + Top-Produkte) mit OxyPlot.
    /// </summary>
    public class Dashboard : BaseViewModel
    {
        // Felder (Services & State) 

        private readonly KundenService _kundenService;//Service für Kunden-Daten
        private readonly StatistikService _statistikService;//Service für Statistik-Daten (Umsatz, Bestellstatus, Top-Produkte)
        private readonly ProdukteService _produkteService;
        private readonly ProdukteViewModel _produkteViewModel;
        private readonly AufgabenService _aufgabenService;
        private readonly BestellungenService _bestellungenService;
        private readonly BestellungenViewModel _bestellungenViewModel;

        private List<Produkte> _kritischeProdukte = new();

        private bool _istLaden;
        private bool _istOnline;
        private string _benutzerName = "Benutzer";
        private int _anzahlKunden;
        private int _anzahlProdukte;
        private int _anzahlAufgabenOffen;
        private int _anzahlBestellungenOffen;
        private decimal _umsatzHeute;
        private bool _istWarnungsPanelSichtbar;//Steuert ob das Warnungs-Pops im Dashboard sichtbar ist..

      
        private IEnumerable<object> _umsatzSerieDummy;
        private object _umsatzXAxesDummy;
        private object _umsatzYAxesDummy;
        private List<string> _umsatzLabels;
       


        //  OxyPlot  ( Dashboard XAML)
        private PlotModel _umsatzPlotModel;
        private PlotModel _produktePlotModel;
       
      

        // Status
        private bool _hatWarnungen;
        private bool _hatFehler;
        private DateTime _letzteAktualisierung;//Zeitpunkt der letzten Datenaktualisierung
        private Brush _statusBrush = new SolidColorBrush(Color.FromRgb(0x9A, 0xA0, 0xA6));// Standard: Grau
        private bool _istPuls;
        public int KritischeProduktAnzahl => _kritischeProdukte?.Count ?? 0;//Anzahl kritische produkte wird für das rote warn-icon im  dashboard genutzt.
        private List<string> _warnungen = new();   // text meldumgen für kritische produkte
                                        

        // Konstruktor 

        /// <summary>
        /// Erstellt ein neues Dashboard-ViewModel und 
        /// initialisiert Services, 
        /// </summary>
        public Dashboard()
        {
            _kundenService = new KundenService();
            _statistikService = new StatistikService();
            _produkteService = new ProdukteService();
            _produkteViewModel = new ProdukteViewModel(new DialogService(), _produkteService);
            _aufgabenService = new AufgabenService();
            _bestellungenService = new BestellungenService();


            AktualisierenCommand = new Befehl(async _ => await AktualisierenAsync(), _ => !IstLaden);//Async-Command mit CanExecute, damit während des Ladens nicht mehrfach ausgelöst werden kann.
            ÖffneKundenCommand = new Befehl(_ => ÖffneKundenFenster());
            ÖffneProdukteCommand = new Befehl(_ => ÖffneProdukteFenster());
            AufgabenCommand = new Befehl(_ => ÖffnenAufgabenFenster());
            ÖffneBestellungenCommand = new Befehl(_ => ÖffneBestellungenFenster());

            UmsatzNachMonatenPlot = BuildUmsatzNachMonatenPlotModel(new List<(string Monat, decimal Betrag)>
            {
                ("Jän",0),("Feb",0),("Mär",0),("Apr",0),("Mai",0),("Jun",0),
                ("Jul",0),("Aug",0),("Sep",0),("Okt",0),("Nov",0),("Dez",0)
            });
            BestellungenNachStatusPlot = BuildBestellungenNachStatusPlotModel(new List<(string Status, int Anzahl)>
            {
                ("Offen",0),
                ("Abgeschlossen",0),
                ("Versendet",0),
                ("Storniert",0)
            });

            IstLaden = false;
            IstOnline = true;
            HatWarnungen = false;
            HatFehler = false;
            LetzteAktualisierung = DateTime.Now;
            UpdateStatusBrush();

            // Async initialer Load
            _ = InitialisiereAsync();
        }

        // Öffentliche Eigenschaften (Status/Kennzahlen) 

        /// <summary>
        /// Liste der Produkte deren Ablaufdatum
        /// kritisch ist weniger als 3 tage bis zum
        /// Ablauf.Wird im Dashboard für Warnungen  angezeigt.
        /// </summary>
        public List<Produkte> KritischeProdukte
        {
            get => _kritischeProdukte;
            set
            {
                this._kritischeProdukte = value;
                OnPropertyChanged();


                this.OnPropertyChanged(nameof(KritischeProduktAnzahl));//Anzahl der kritischen Produkte aktualisieren (für die rote Zahl an der Glocke)
                this.OnPropertyChanged(nameof(HatWarnungen));//Zahl an der Glocke aktualisieren
            }

            
        }
        /// <summary>
        /// Text Warnungen für die Anzeige
        /// in einem Poup /Panel
        /// </summary>
        public List<string> Warnungen
        {
            get => _warnungen;
            set
            {
                _warnungen = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Steuert ob das Warnungs-Popup Liste der bald ablaufenden 
        /// Produkte im Dashboard sichtbar ist
        /// </summary>
        public bool IstWarnungsPanelSichtbar
        {
            get => _istWarnungsPanelSichtbar;
            set
            {
                if(this._istWarnungsPanelSichtbar == value) return;
                _istWarnungsPanelSichtbar = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Lädt die Produkte deren,
        /// Ablaufdatum in der nächsten 3 tagen 
        /// erreicht wird.
        /// </summary>
        /// <returns></returns>
        public async Task LadeKritischeProdukteAsync()
        {
            var liste = await _produkteService.LadeProdukteMitBaldAblaufAsync();

            KritischeProdukte = liste.ToList();
            HatWarnungen = KritischeProdukte.Count > 0;

            Warnungen = KritischeProdukte.Select(p => $"{p.ProduktName}:läuft am {p.AblaufDatum:dd.MM.yyyy} ab. ").ToList();
        }
        /// <summary>
        /// Diagramm-Modell für den Umsatz nach Monaten.
        /// </summary>
        public PlotModel? UmsatzNachMonatenModel
        {
            get => _umsatzPlotModel;
            set { _umsatzPlotModel = value!; OnPropertyChanged(); }
        }
        /// <summary>
        /// Gibt an, ob aktuell ein Ladevorgang läuft.
        /// </summary>
        public bool IstLaden
        {
            get => _istLaden;
            private set
            {
                if (_istLaden == value) return;
                _istLaden = value;
                IstPuls = value;
                OnPropertyChanged(nameof(IstPuls));
                OnPropertyChanged(nameof(StatusText));
                UpdateStatusBrush();
            }
        }

        /// <summary>
        /// Gibt an, ob die Anwendung online ist.
        /// </summary>
        public bool IstOnline
        {
            get => _istOnline;
            set { _istOnline = value; OnPropertyChanged(); OnPropertyChanged(nameof(StatusText)); UpdateStatusBrush(); }
        }

        /// <summary>
        /// Der Anzeigename des angemeldeten Benutzers.
        /// </summary>
        public string BenutzerName { get => _benutzerName; set { _benutzerName = value; OnPropertyChanged(); } }

        /// <summary>
        /// Anzahl der Kunden.
        /// </summary>
        public int AnzahlKunden { get => _anzahlKunden; set { _anzahlKunden = value; OnPropertyChanged(); } }

        /// <summary>
        /// Anzahl der Produkte.
        /// </summary>
        public int AnzahlProdukte { get => _anzahlProdukte; set { _anzahlProdukte = value; OnPropertyChanged(); } }

        /// <summary>
        /// Anzahl offener Aufgaben.
        /// </summary>
        public int AnzahlAufgabenOffen { get => _anzahlAufgabenOffen; set { _anzahlAufgabenOffen = value; OnPropertyChanged(); } }

        /// <summary>
        /// Anzahl offener Bestellungen.
        /// </summary>
        public int AnzahlBestellungenOffen { get => _anzahlBestellungenOffen; set { _anzahlBestellungenOffen = value; OnPropertyChanged(); } }

        /// <summary>
        /// Heutiger Umsatz in Euro.
        /// </summary>
        public decimal UmsatzHeute { get => _umsatzHeute; set { _umsatzHeute = value; OnPropertyChanged(); } }
        /// <summary>
        /// Umsatz nach Monaten Plot Model
        /// </summary>
        public PlotModel? UmsatzNachMonatenPlot { get;private set; }
        /// <summary>
        /// Umsatz nach Bestellstatus Plot Model
        /// </summary>
        private PlotModel? _bestellungenNachStatusPlot;
        /// <summary>
        /// Räpräsentiert das PlotModel für Bestellungen nach Status.
        /// Status der Bestellungen (z.B. offen, in Bearbeitung, abgeschlossen,versendet,).
        /// </summary>
        public PlotModel? BestellungenNachStatusPlot
        {
            get => _bestellungenNachStatusPlot;
            private set { 
                _bestellungenNachStatusPlot = value!; 
                OnPropertyChanged(); }
        }



        /// <summary>
        /// Label-Liste (Monate) für den Umsatzzug.
        /// </summary>
        public List<string> UmsatzLabels
        {
            get => _umsatzLabels;
            set { _umsatzLabels = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Lesbarer Statustext für die Topbar.
        /// </summary>
        public string StatusText => IstLaden ? "Aktualisiere…" :
                                    HatFehler ? "Fehler" :
                                    HatWarnungen ? "Warnung" :
                                    IstOnline ? $"Onlline • Stand {LetzteAktualisierung:HH:mm}" :
                                    "Offline";

        /// <summary>
        /// Farbiger Indikator (Ellipse) für den Status.
        /// </summary>
        public Brush StatusBrush { get => _statusBrush; private set { _statusBrush = value; OnPropertyChanged(); } }

        /// <summary>
        /// Steuert die Puls-Animation des Statuspunkts.
        /// </summary>
        public bool IstPuls { get => _istPuls; set { _istPuls = value; OnPropertyChanged(nameof(IstPuls)); } }

        /// <summary>
        /// Letzter Aktualisierungszeitpunkt.
        /// </summary>
        public DateTime LetzteAktualisierung
        {
            get => _letzteAktualisierung;
            set { _letzteAktualisierung = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Gibt an, ob Warnungen vorliegen.
        /// </summary>
        public bool HatWarnungen { get => _hatWarnungen; 
            set
            {
                if (_hatWarnungen == value) return;
                _hatWarnungen = value;
                OnPropertyChanged(); 
                UpdateStatusBrush();

                if (!_hatWarnungen)
                {
                    IstWarnungsPanelSichtbar = false;
                }
            } 
        }

        /// <summary>
        /// Gibt an, ob Fehler vorliegen.
        /// </summary>
        public bool HatFehler { get => _hatFehler; set { _hatFehler = value; OnPropertyChanged(); UpdateStatusBrush(); } }

        // Kommand

        /// <summary>
        /// Aktualisiert Kennzahlen & Diagramme.
        /// </summary>
        public Befehl AktualisierenCommand { get; }

        /// <summary>
        /// Öffnet das Kunden-Fenster (Dialog).
        /// </summary>
        public Befehl ÖffneKundenCommand { get; }

        public Befehl AufgabenCommand { get; }
        public Befehl ÖffneProdukteCommand { get; }

        public Befehl ÖffneBestellungenCommand { get; }
        /// <summary>
        /// Optionaler Delegate für externe Navigation.
        /// </summary>
        public Action<string>? Navigation { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ProdukteViewModel Produkte { get; private set; } = null!;

        /// <summary>
        /// Abmelden/Logout (setzt OberflächeManager.IstAngemeldet = false).
        /// </summary>
        private Befehl _BenutzerAbmelden;

        /// <summary>
        /// Befehl zum Abmelden des Benutzers.
        /// Zeigt eine Bestätigungsdialog und 
        /// setzt dann die Oberfläche auf nicht angemeldet, 
        /// was zum Login-Fenster zurückführt.
        /// </summary>
        public Befehl BenutzerAbmelden
        {
            get
            {
                return _BenutzerAbmelden ??= new Befehl(_ =>
                {
                    try
                    {
                        var result = MessageBox.Show("Möchten Sie sich wirklich abmelden?", "Abmelden",
                            MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result != MessageBoxResult.Yes) return;

                        var hfvm = Application.Current.MainWindow?.DataContext as HauptfensterViewModel;
                        var oberf = hfvm?.Benutzer?.Oberfläche;
                        if (oberf == null)
                        {
                            MessageBox.Show("OberflächeManager konnte nicht gefunden werden",
                                "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        Application.Current.Dispatcher.Invoke(() => oberf.IstAngemeldet = false);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Fehler beim Abmelden:\n{ex.Message}",
                            "Abmeldung", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });
            }
        }

        // OxyPlot-Modelle (Binding für XAML)

        /// <summary>
        /// PlotModel für den monatlichen Umsatz 
        /// (Spline/Line + Flächenfüllung + Trend).
        /// </summary>
        public PlotModel UmsatzPlotModel
        {
            get => _umsatzPlotModel;
            private set { _umsatzPlotModel = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// PlotModel für Top-5 Produkte (Doughnut/Pie).
        /// </summary>
        public PlotModel ProduktePlotModel
        {
            get => _produktePlotModel!;
            private set { _produktePlotModel = value; OnPropertyChanged(); }
        }
        /// <summary>
        /// Erstellt ein Kreisdiagramm-PlotModel für die Top-Produkte.
        /// </summary>
        /// <param name="daten">Zeigt die Umsatzteile und in der
        /// Mitte den Gesamtumsatz</param>
        /// <returns></returns>
        private PlotModel ErstelleProduktePlot(List<(string Name, decimal Wert)> daten)
        {
            decimal gesamt = daten.Sum(d => d.Wert);

            var model = new PlotModel
            {
                Background = OxyColor.FromRgb(30, 30, 44),//Dunkeles Theme
                TextColor = OxyColors.White,
                PlotAreaBorderColor = OxyColors.Transparent,
                Padding = new OxyThickness(10)
            };
            //Kreisdiagramm-Serie
            var series = new PieSeries
            {
                Stroke = OxyColors.White,
                StrokeThickness = 1,
                AngleSpan = 360,
                StartAngle = 0,
                InsideLabelPosition = 0.65,
                FontSize = 14,
                InsideLabelColor = OxyColors.White,
                TickHorizontalLength = 0,
                TickRadialLength = 0,
               ToolTip = "Produktanteil",
            };
            //Farbепалlete дефинирање
            var farben = new[]
            {
                OxyColor.FromRgb(0,191,255),//DeepSkyBlue
                OxyColor.FromRgb(0,200,83),//Grün
                OxyColor.FromRgb(255,193,7),//Gelb
                OxyColor.FromRgb(255, 112,67),//Orange
                OxyColor.FromRgb(126,87,194)//Violett
            };
            for (int i = 0; i < daten.Count; i++)
            {
                var (name, wert) = daten[i];
                // PieSlice-Label wird im Konstruktor gesetzt, nicht per Zuweisung!
                var slice = new PieSlice($"{name}\n{wert:C0}", (double)wert)
                {
                    Fill = farben[i % farben.Length]
                };
                series.Slices.Add(slice);
            }
            model.Series.Add(series);

            //Text in der Mitte Gesamtumsatz
            model.Annotations.Add(new OxyPlot.Annotations.TextAnnotation
            {
                Text = $"Total\n{gesamt:C0}",
                TextPosition = new DataPoint(0, 0),
                FontSize = 16,
                FontWeight = OxyPlot.FontWeights.Bold,
                TextColor = OxyColors.White,
                Stroke = OxyColors.Transparent,
                Background = OxyColors.Transparent,
                TextHorizontalAlignment = OxyPlot.HorizontalAlignment.Center,
                TextVerticalAlignment = OxyPlot.VerticalAlignment.Middle

            });
            return model;
        }
        /// <summary>
        /// Erzeugt ein PlotModel für den Umsatz nach Monaten.
        /// </summary>
        /// <param name="daten"></param>

        private PlotModel BuildUmsatzNachMonatenPlotModel(List<(string Monat,decimal Betrag)> daten)
        {
            //1 Plot-Grundlayout im Dark-Theme erstellen
            var pm = new PlotModel
            {
                Background = OxyColor.FromArgb(255, 0x1E, 0x1E, 0x2C),

                PlotAreaBackground = OxyColor.FromArgb(255, 0x1E, 0x1E, 0x2C),
                TextColor = OxyColors.White,
                PlotAreaBorderColor = OxyColors.Transparent,
                

            };
            // 2 Die eigentliche Donut/Pie-Serie erstellen

            var pie = new PieSeries
            {
                InnerDiameter = 0.6,
                Stroke = OxyColors.White,
                StrokeThickness = 0.5,
                OutsideLabelFormat = null,
                InsideLabelFormat = "{0}\n{1:#,0}",
                TextColor = OxyColors.White,
                TickHorizontalLength = 0,
                TickRadialLength = 0
            };
            //3 Farbenpalette definieren
            var palette = new[]
            {
                OxyColor.FromRgb(3,169,244),
                OxyColor.FromRgb(126,87,194),
                OxyColor.FromRgb(0,200,83),
                OxyColor.FromRgb(255,193,7),
                OxyColor.FromRgb(255,112,67),
                OxyColor.FromRgb(158,158,158),
                OxyColor.FromRgb(0,151,167),
                OxyColor.FromRgb(233,30,99),
                OxyColor.FromRgb(76,175,80),
                OxyColor.FromRgb(63,81,181),
                OxyColor.FromRgb(205,220,57),
                OxyColor.FromRgb(121,85,72)
            };

            for(int i = 0; i < daten.Count; i++)
            {
                var (monat, betrag) = daten[i];
                pie.Slices.Add(new PieSlice(monat,Math.Max(0,(double)betrag))
                {
                    Fill = palette[i % palette.Length]
                });
            }
            pm.Series.Add(pie);
            return pm;

        }
        /// <summary>
        /// Erstellt ein Donut/Pie-PlotModel für Bestellungen nach Status.
        /// </summary>
        /// <param name="">Liste mit Status,Anzahl</param>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <returns></returns>
        private PlotModel BuildBestellungenNachStatusPlotModel(List<(string Status,int Anzahl)>daten)
        {
            var model = new PlotModel
            {
                Background = OxyColor.FromRgb(30, 30, 44), // Dunkles Theme
                TextColor = OxyColors.White,
                PlotAreaBorderColor = OxyColors.Transparent,
                Padding = new OxyThickness(10)
            };
            var farben = new Dictionary<string, OxyColor>
            {
                {"Offen", OxyColor.FromRgb(255, 193, 7)}, // Gelb
                {"In Bearbeitung", OxyColor.FromRgb(3, 169, 244)}, // Blau
                {"Abgeschlossen", OxyColor.FromRgb(0, 200, 83)}, // Grün
                {"Versendet", OxyColor.FromRgb(3, 169, 244)}, // Violett
                {"Storniert", OxyColor.FromRgb(244, 67, 54)} // Rot
            };

            // Die Donut/Pie-Serie erstellen
            var pie = new PieSeries
            {
                InnerDiameter = 0.6, // Doughnut
                Stroke = OxyColors.White,
                StrokeThickness = 0.5,
                InsideLabelFormat = "{0}\n{1:#,0}",
                FontSize = 13,
                TextColor = OxyColors.White,
                TickHorizontalLength = 0,
                TickRadialLength = 0
            };

            foreach(var(status,anzahl)in daten)
            {
                var farbe = farben.ContainsKey(status) ? farben[status] 
                    : OxyColor.FromRgb(158, 158, 158);   // Grau als Fallback

                pie.Slices.Add(new PieSlice(status, anzahl)
                {
                    Fill = farbe
                });
            }
            model.Series.Add(pie);

            // Text in der Mitte: Gesamtanzahl
            int gesamt = daten.Sum(d => d.Anzahl);
            model.Annotations.Add(new OxyPlot.Annotations.TextAnnotation
            {
               Text = $"Gesamt\n{gesamt:#,0}",
                TextPosition = new DataPoint(0, 0),
                FontSize = 15,
                FontWeight = OxyPlot.FontWeights.Bold,
                TextColor = OxyColors.White,
                Stroke = OxyColors.Transparent,
                TextHorizontalAlignment = OxyPlot.HorizontalAlignment.Center,
                TextVerticalAlignment = OxyPlot.VerticalAlignment.Middle
            });
            return model;
        }
        /// <summary>
        /// Öffnet das modale Kunden-Fenster.
        /// </summary>
        private void ÖffneKundenFenster()
        {
            try
            {
                var fenster = new KundenFenster { Owner = Application.Current.MainWindow };
                fenster.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Öffnen des Kundenfensters:\n{ex.Message}", "Fehler",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Öffnet das modale Produkte-Fenster
        /// </summary>
        private void ÖffneProdukteFenster()
        {
            try
            {
                var fenster = new
                    ProdukteFenster
                {
                    Owner = Application.Current.MainWindow,
                  
                };
                fenster.ShowDialog();
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Fehler beim Öffnen das Produktfensters:\n{ex.Message}","Fehler",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// Öffnet das modale Aufgaben-Fenster
        /// </summary>
        private void ÖffnenAufgabenFenster()
        {
            try
            {
                var fenster = new AufgabenFenster
                {
                    Owner = Application.Current.MainWindow
                };
                fenster.ShowDialog();
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Fehler beim Öffnen das Aufgabenfenster:\n{ex.Message}", "Fehler",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Öffnet das modale Bestellungen-Fenster,
        /// und zeigt die Bestellungen mit ihren Status an.
        /// </summary>
        private void ÖffneBestellungenFenster()
        {
            try
            {
                var fenster = new BestellungenFenster
                {
                    Owner = Application.Current.MainWindow
                };
                fenster.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Öffnen das Bestellungsfenster:\n{ex.Message}", "Fehler",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// Initialer Ladevorgang.
        /// </summary>
        private async Task InitialisiereAsync() => await AktualisierenAsync();

        /// <summary>
        /// Lädt Kennzahlen und baut 
        /// beide OxyPlot-Modelle
        /// auf Basis der echten Daten.
        /// </summary>
        private async Task AktualisierenAsync()
        {
            try
            {
                IstLaden = true;

                var kundenListe = await _kundenService.LadeAlleKundenAsync();
                AnzahlKunden = kundenListe?.Count ?? 0;

                var umsatze = await _statistikService.LadeMonatsUmsatzeAsync();
                UmsatzHeute = await _statistikService.LadeHeutigenUmsatzAsync();

                var produktListe = await _produkteService.LadeAlleProdukteAsync();
                AnzahlProdukte = produktListe?.Count ?? 0;

                var aufgabenListe = await _aufgabenService.LadeAlleAufgabenAsync();
                AnzahlAufgabenOffen = aufgabenListe.Count(a => a.Status != 3);

                var bestellungListe = await _bestellungenService.LadeAlleBestellungenAsync();
                AnzahlBestellungenOffen = bestellungListe.Count(b => b.StatusCode == 0);

                var bestellungenStatus = await
                    _statistikService.LadeBestellungenNachStatus();

                var kritische = await _produkteService.LadeProdukteMitBaldAblaufAsync();//kritische produkte laden.
                                                                                        //ObservableCollection sauber neu befüllen                                                                
                KritischeProdukte = kritische;

                
                

                //Warn-Status setzen..
                HatWarnungen = KritischeProdukte.Count > 0;

                //Optional:Warnungstexte für Popup Liste bauen..
                Warnungen = KritischeProdukte
                         .Select(p => $"⚠ Produkt '{p.ProduktName}' läuft am {p.AblaufDatum:dd.MM.yyyy} ab!")
                          .ToList();

              

                if (bestellungenStatus != null && bestellungenStatus.Count > 0)
                {
                    var statusDaten = bestellungenStatus
                        .Select(b => (b.StatusName, b.Anzahl)).ToList();
                    BestellungenNachStatusPlot = BuildBestellungenNachStatusPlotModel(statusDaten);
                    OnPropertyChanged(nameof(BestellungenNachStatusPlot));
                }
                else
                {
                    // Fallback falls keine echten Bestellstatus
                    BestellungenNachStatusPlot = BuildBestellungenNachStatusPlotModel(new List<(string, int)>
                    {
                        ("Offen", 1),
                        ("Abgeschlossen", 1),
                        ("Versendet", 1),
                        ("Storniert", 1)
                        
                    });
                }

                if (umsatze != null && umsatze.Count > 0)
                {
                    var monateDaten = umsatze.Select(u =>(u.Monat, u.Betrag)).ToList();

                    UmsatzNachMonatenPlot = BuildUmsatzNachMonatenPlotModel(monateDaten);
                    OnPropertyChanged(nameof(UmsatzNachMonatenPlot));

                    var topProdukte = await
                        _statistikService.LadeTop5ProdukteNachUmsatzAsync();

                    if(topProdukte != null && topProdukte.Count > 0)
                    {
                        var produktDaten = topProdukte
                            .Select(p => (p.ProduktName, p.GesamtUmsatz)).ToList();


                        ProduktePlotModel = BuildProduktePlotModel(produktDaten);
                            
                    }
                    else
                    {
                        // Fallback falls keine echten Top-Produkte
                        ProduktePlotModel = BuildProduktePlotModel(new List<(string, decimal)>
                        {
                            ("Keine Daten", 1)
                        });

                    }
                


                }

                LetzteAktualisierung = DateTime.Now;
                IstOnline = true;
            }
            catch (Exception ex)
            {
                IstOnline = false;
                MessageBox.Show($"Fehler beim Laden der Dashboard-Daten:\n{ex.Message}",
                    "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IstLaden = false;
            }
        }

        //  Private Hilfen: Statusfarbe

        /// <summary>
        /// Aktualisiert die Statusfarbe 
        /// basierend auf Flags 
        /// (Fehler/Warnung/Laden/Online).
        /// </summary>
        private void UpdateStatusBrush()
        {
            if (HatFehler)
                StatusBrush = new SolidColorBrush(Color.FromRgb(0xEA, 0x43, 0x35));
            else if (HatWarnungen)
                StatusBrush = new SolidColorBrush(Color.FromRgb(0xFB, 0xBC, 0x05));
            else if (IstLaden)
                StatusBrush = new SolidColorBrush(Color.FromRgb(0x42, 0x85, 0xF4));
            else if (IstOnline)
                StatusBrush = new SolidColorBrush(Color.FromRgb(0x34, 0xA8, 0x53));
            else
                StatusBrush = new SolidColorBrush(Color.FromRgb(0x9A, 0xA0, 0xA6));
        }

        // ───────────────────────────── Private Hilfen: OxyPlot Modelle ─────────────────────────────

        /// <summary>
        /// Baut das Umsatz-PlotModel (dunkles Theme, flache Gitter, Flächenfüllung + Trendlinie).
        /// </summary>
        /// <param name="labels">Monatslabels.</param>
        /// <param name="werte">Umsatzwerte je Monat.</param>
        private PlotModel BuildUmsatzPlotModel(List<string> labels, List<decimal> werte)
        {
            var pm = new PlotModel
            {
                Background = OxyColor.FromArgb(255, 0x1E, 0x1E, 0x2C),
                PlotAreaBackground = OxyColor.FromArgb(255, 0x10, 0x14, 0x26),
                TextColor = OxyColors.White,
                PlotAreaBorderColor = OxyColor.FromArgb(25, 255, 255, 255)
            };

            // X-Achse (Kategorie/Monate)
            pm.Axes.Add(new CategoryAxis
            {
                Position = AxisPosition.Bottom,
                ItemsSource = labels,
                LabelField = null,   // ItemsSource sind strings
                GapWidth = 0.3,
                IsZoomEnabled = false,
                IsPanEnabled = false,
                Angle = -25,
                TextColor = OxyColors.White,
                AxislineColor = OxyColor.FromArgb(40, 255, 255, 255),
                TicklineColor = OxyColor.FromArgb(40, 255, 255, 255)
            });

            // Y-Achse (Währung)
            pm.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                StringFormat = "€#,0",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.None,
                MajorGridlineColor = OxyColor.FromArgb(35, 255, 255, 255),
                TextColor = OxyColors.White,
                AxislineColor = OxyColor.FromArgb(40, 255, 255, 255),
                TicklineColor = OxyColor.FromArgb(40, 255, 255, 255)
            });

            // Flächenserie (област под линијата) – ќе нацртаме AreaSeries со база 0
            var area = new AreaSeries
            {
                Color = OxyColor.FromArgb(180, 0, 191, 255),     // stroke
                Fill = OxyColor.FromArgb(80, 0, 191, 255),       // fill
                StrokeThickness = 2,
                MarkerType = MarkerType.None,
               
                
            };

            // Линија со исти точки (за горниот кант на областа)
            var line = new LineSeries
            {
                Color = OxyColor.FromArgb(255, 0, 191, 255),
                StrokeThickness = 2.5,
                MarkerType = MarkerType.None,
                
            };
            line.TrackerFormatString = "{0}\n{1}: {2:0,0.00} €";

            for (int i = 0; i < werte.Count; i++)
            {
                double x = i;
                double y = (double)werte[i];

                // Горна линија
                line.Points.Add(new DataPoint(x, y));

                // Област: горна крива
                area.Points.Add(new DataPoint(x, y));
            }

            // Област: базна линија (0)
            for (int i = werte.Count - 1; i >= 0; i--)
            {
                area.Points2.Add(new DataPoint(i, 0));
            }

            // Тренд линија (лесен moving average 3)
            var trend = new LineSeries
            {
                Color = OxyColor.FromArgb(255, 135, 206, 250),
                StrokeThickness = 2,
                MarkerType = MarkerType.None
            };
            var avg = GlidingAverage(werte, 3);
            for (int i = 0; i < avg.Count; i++)
                trend.Points.Add(new DataPoint(i, (double)avg[i]));

            pm.Series.Add(area);
            pm.Series.Add(line);
            pm.Series.Add(trend);

            return pm;
        }

        /// <summary>
        /// Baut das Produkte-PlotModel 
        /// (dunkles Doughnut/Pie mit DataLabels).
        /// </summary>
        private PlotModel BuildProduktePlotModel(List<(string label, decimal value)> daten)
        {
            var pm = new PlotModel
            {
                Background = OxyColor.FromArgb(255, 0x1E, 0x1E, 0x2C),
                PlotAreaBackground = OxyColor.FromArgb(255, 0x1E, 0x1E, 0x2C),
                TextColor = OxyColors.White,
                PlotAreaBorderColor = OxyColors.Transparent
            };

            var palette = new[]
            {
                OxyColor.FromRgb(3,169,244),
                OxyColor.FromRgb(126,87,194),
                OxyColor.FromRgb(0,200,83),
                OxyColor.FromRgb(255,193,7),
                OxyColor.FromRgb(255,112,67)
            };

            var pie = new PieSeries
            {
                InnerDiameter = 0.6, // doughnut
                Stroke = OxyColors.White,
                StrokeThickness = 0.5,
                OutsideLabelFormat = null,   // keine Outside-Labels
                InsideLabelFormat = "{1}\n{0:#,0}", // {Label} + Wert
                TextColor = OxyColors.White,
                TickHorizontalLength = 0,
                TickRadialLength = 0
            };

            for (int i = 0; i < daten.Count; i++)
            {
                var (label, value) = daten[i];
                pie.Slices.Add(new PieSlice(label, Math.Max(0, (double)value))
                {
                    Fill = palette[i % palette.Length]
                });
            }

            pm.Series.Add(pie);
            return pm;
        }

        /// <summary>
        /// Erzeugt eine Top-5-Liste (Label, Wert) 
        /// aus Monatsumsätzen
        /// (Fallback-Buckets, wenn keine echte 
        /// Top5 verfügbar ist).
        /// </summary>
        private static List<(string label, decimal value)> BuildTop5From(List<UmsatzMonat> umsatze)
        {
            var werte = umsatze.Select(u => u.Betrag).ToList();
            while (werte.Count < 12) werte.Add(0);

            var buckets = new[]
            {
                ("Konbu",    werte.Take(2).Sum()),
                ("Guarana",  werte.Skip(2).Take(2).Sum()),
                ("Camembert",werte.Skip(4).Take(3).Sum()),
                ("Raclette", werte.Skip(7).Take(2).Sum()),
                ("Chang",    werte.Skip(9).Take(3).Sum())
            };
            return buckets.ToList();
        }

        /// <summary>
        /// Gleitender Durchschnitt über ein Fenster (z.B. 3 Monate) für Trendlinien.
        /// </summary>
        private static List<decimal> GlidingAverage(List<decimal> values, int window)
        {
            if (values == null || values.Count == 0) return new List<decimal>();
            var result = new List<decimal>(values.Count);
            for (int i = 0; i < values.Count; i++)
            {
                int a = Math.Max(0, i - (window - 1));
                int b = i;
                var slice = values.Skip(a).Take(b - a + 1);
                result.Add(Math.Round(slice.Average(), 2));
            }
            return result;
        }
    }
}
