using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAT_Man.Models
{
    /// <summary>
    /// Repräsentiert eine einzelne Status-Option (z.B. "Anruf", "Email").
    /// Dient als Datenstruktur für die Auswahlmöglichkeiten im Dialog "Aktivität hinzufügen".
    /// </summary>
    public class Status
    {
        /// <summary>
        /// Die eindeutige ID aus der Datenbanktabelle 'Status'.
        /// </summary>
        public int Status_ID { get; set; }

        /// <summary>
        /// Der lokalisierte Anzeigetext (z.B. "Anruf" oder "Call"), 
        /// geladen aus der Tabelle 'Status_Translation'.
        /// </summary>
        public string Bezeichnung { get; set; }
    }
}