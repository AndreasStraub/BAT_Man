using System;

namespace WPF_Test.Models
{
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

        // WICHTIG: tinyint(1) in MySQL wird in C# meist als bool behandelt.
        public bool Erstanmeldung { get; set; }

        // ==========================================================
        // 2. LOGIK-BRÜCKEN (Für Kompatibilität zur App)
        // ==========================================================

        // Die App nutzt "TeilnehmerID" (ohne Unterstrich). Es wird weitergeleitet.
        public int TeilnehmerID => Teilnehmer_ID;

        // Die App nutzt "RehaNummer" (als String).
        // LOGIK: Da RehaNummer == Teilnehmer_ID ist, wird die ID einfach als String zurück gegeben.
        public string RehaNummer => Teilnehmer_ID.ToString();

        // Die App nutzt "MussPasswortAendern".
        // Das entspricht jetzt dem Feld "Erstanmeldung".
        public bool MussPasswortAendern
        {
            get { return Erstanmeldung; }
            set { Erstanmeldung = value; }
        }

        // TODO: Kurs-Name ergänzen
        // Die App erwartet einen Kurs-Namen (String).
        // Da wir nur die Kurs_ID haben, wird ein provisorischer String gebaut.
        // (Für den echten Namen bräuchte man später einen SQL-JOIN auf die Kurs-Tabelle).
        public string Kurs => $"Kurs-ID: {Kurs_ID}";
    }
}