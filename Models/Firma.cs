using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAT_Man.Models
{
    /// <summary>
    /// Repräsentiert eine Firma (Interessent/Kunde).
    /// </summary>
    public class Firma
    {
        // Primärschlüssel in der Datenbank
        public int Firma_ID { get; set; }

        // Fremdschlüssel: Verknüpfung zum User, der diesen Datensatz angelegt hat.
        // Dient der Daten-Isolation (jeder sieht nur seine Firmen).
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
        /// Diese Eigenschaft existiert nicht in der Tabelle 'Firma'.
        /// Sie wird vom Repository (Methode: GetAlleFirmenMitLetztemStatus) 
        /// über eine Unterabfrage/Join befüllt.
        /// </summary>
        public string LetzterStatus { get; set; }

        /// <summary>
        /// Hält den Kommentar der LETZTEN Aktivität.
        /// Wird vom Repository (GetAlleFirmenMitLetztemStatus) geladen.
        /// </summary>
        public string LetzteBemerkung { get; set; }
    }
}