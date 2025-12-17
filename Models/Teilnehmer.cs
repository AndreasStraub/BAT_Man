using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAT_Man.Models
{
    /// <summary>
    /// Repräsentiert einen Benutzer (Teilnehmer) der Anwendung.
    /// </summary>
    public class Teilnehmer
    {
        // ==========================================================
        // 1. ECHTE DATENBANK-FELDER (Exaktes Mapping zur Tabelle)
        // ==========================================================

        public int Teilnehmer_ID { get; set; } // Primary Key (int)
        public string Vorname { get; set; }
        public string Nachname { get; set; }
        public string EMail { get; set; }
        public int Rolle_ID { get; set; }
        public int Kurs_ID { get; set; }
        public int Fachrichtung_ID { get; set; }
        public string Kurs { get; set; }

        // WICHTIG: tinyint(1) in MySQL wird in C# standardmäßig als bool behandelt.
        // Kennzeichnet, ob der Benutzer sich zum ersten Mal anmeldet (Passwort-Zwang).
        public bool Erstanmeldung { get; set; }

        // ==========================================================
        // 2. LOGIK-BRÜCKEN (Kompatibilitäts-Layer)
        // Dienen der Anpassung an bestehenden Code, der andere Namen erwartet.
        // ==========================================================

        /// <summary>
        /// Alias für Teilnehmer_ID (ohne Unterstrich).
        /// </summary>
        public int TeilnehmerID => Teilnehmer_ID;

        /// <summary>
        /// Alias für RehaNummer.
        /// Konvertiert die ID in einen String, da die Anwendungslogik dies erwartet.
        /// </summary>
        public string RehaNummer => Teilnehmer_ID.ToString();

        /// <summary>
        /// Alias für Erstanmeldung.
        /// Ermöglicht lesenden und schreibenden Zugriff auf das zugrundeliegende Feld.
        /// </summary>
        public bool MussPasswortAendern
        {
            get { return Erstanmeldung; }
            set { Erstanmeldung = value; }
        }
    }
}