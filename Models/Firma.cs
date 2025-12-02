using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAT_Man.Models
{
    public class Firma
    {
        public int Firma_ID { get; set; }
        public int Teilnehmer_ID { get; set; }
        public string Firmenname { get; set; }
        public string Strasse { get; set; }
        public string Hausnummer { get; set; }
        public string PLZ { get; set; }
        public string Ort { get; set; }
        public string Ansprechpartner { get; set; }
        public string Telefon { get; set; }
        public string EMail { get; set; }

        /// <summary>
        /// Hält den Status-Text der LETZTEN Aktivität.
        /// Wird vom Repository (GetAlleFirmenMitLetztemStatus) geladen.
        /// </summary>
        public string LetzterStatus { get; set; }

        /// <summary>
        /// Hält den Kommentar der LETZTEN Aktivität.
        /// Wird vom Repository (GetAlleFirmenMitLetztemStatus) geladen.
        /// </summary>
        public string LetzteBemerkung { get; set; }
    }
}
