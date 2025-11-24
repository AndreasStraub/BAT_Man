// Dateipfad: Models/Status.cs

// WICHTIG: Der Namespace MUSS so lauten
namespace WPF_Test.Models
{
    /// <summary>
    /// Repräsentiert eine einzelne Status-Option (z.B. "Anruf", "Email").
    /// Dies ist der "Bauplan" für die Objekte, die in der
    /// RadioButton-Liste im "Aktivität hinzufügen"-Dialog angezeigt werden.
    /// </summary>
    public class Status
    {
        /// <summary>
        /// Die ID aus der 'status'-Tabelle (z.B. 1)
        /// </summary>
        public int Status_ID { get; set; }

        /// <summary>
        /// Der übersetzte Text (z.B. "Anruf" oder "Call")
        /// aus der 'status_translation'-Tabelle.
        /// </summary>
        public string Bezeichnung { get; set; }
    }
}