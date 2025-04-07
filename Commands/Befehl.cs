using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ERP.UI.Commands
{
    /// <summary>
    /// Stellt ein Objekt bereit,
    /// das in WPF für die Command Bindung
    /// benutzt werden kann
    /// </summary>
    public class Befehl
        : System.Object,
        System.Windows.Input.ICommand
    {
        /// <summary>
        /// Wird ausgelöst, wenn sich
        /// die Voraussetzung für CanExecute
        /// geändert hat
        /// </summary>
        /// <remarks>Wird an RequerySuggested
        /// vom Windows CommandManager gekoppelt</remarks>
        public event EventHandler? CanExecuteChanged
        {
            add
            {
                System.Windows.Input.CommandManager
                    .RequerySuggested += value;
            }
            remove
            {
                System.Windows.Input.CommandManager
                    .RequerySuggested -= value;
            }
        }

        /// <summary>
        /// Gibt True zurück, wenn der
        /// Befehl aktuell zulässig ist
        /// </summary>
        /// <param name="parameter">Zusatzinformation
        /// aus der Datenbindung</param>
        /// <remarks>Sollte keine Methode
        /// zum Prüfen vorhanden sein, wird
        /// True zurückgegeben, sonst das
        /// Ergebnis der Prüfmethode</remarks>
        public bool CanExecute(object? parameter)
        {
            return
                this._CanExecute == null ?
                true :
                this._CanExecute(parameter!);
        }

        /// <summary>
        /// Führt die Methode aus,
        /// die in diesem Objekt 
        /// gekapselt ist
        /// </summary>
        /// <param name="parameter">Zusatzinformation
        /// aus der Datenbindung</param>
        public void Execute(object? parameter)
        {
            this._Execute(parameter!);
        }

        /// <summary>
        /// Stellt die Methode bereit,
        /// die prüft, ob dieser Befehl
        /// aktuell zulässig ist oder nicht
        /// </summary>
        private System.Predicate<object> _CanExecute = null!;

        /// <summary>
        /// Stellt die Methode dar,
        /// die in diesem Befehl gekapselt ist
        /// </summary>
        private System.Action<object> _Execute = null!;
        

        /// <summary>
        /// Initialisiert einen neuen Befehl
        /// </summary>
        /// <param name="execute">Die Methode,
        /// die durch diesen Befehl ausgeführt werden soll</param>
        /// <remarks>Ohne CanExecute ist dieser
        /// Befehl immer zulässig</remarks>
        public Befehl(System.Action<object> execute)
            : this(execute, canExecute: null!)
        {

        }

        /// <summary>
        /// Initialisiert einen neuen Befehl
        /// </summary>
        /// <param name="execute">Die Methode,
        /// die durch diesen Befehl ausgeführt werden soll</param>
        /// <param name="canExecute">Die Methode,
        /// die prüft, ob der Befehl aktuell zulässig ist</param>
        public Befehl(
            System.Action<object> execute,
            System.Predicate<object> canExecute)
        {
            this._Execute = execute;
            this._CanExecute = canExecute;
        }

       
    }
}
