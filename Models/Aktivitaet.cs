using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAT_Man.Models
{
    /// <summary>
    /// Repräsentiert einen einzelnen Aktivitäts-Eintrag (eine Zeile).
    /// </summary>
    public class Aktivitaet
    {
        public int Aktivitaet_ID { get; set; }
        public DateTime Datum { get; set; }
        public string Kommentar { get; set; }

        // Wird benötigt, um den RadioButton im "Bearbeiten"-Dialog korrekt vor-auszuwählen.
        // Entspricht dem Fremdschlüssel in der Datenbank.
        public int Status_ID { get; set; }

        // Hilfseigenschaft für die Anzeige in der ListView (kommt aus dem JOIN mit der Status-Tabelle).
        // Ist nicht direkt in der Tabelle 'Aktivitaet' gespeichert.
        public string StatusBezeichnung { get; set; }
    }
}