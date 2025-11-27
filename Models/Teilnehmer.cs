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
        // Falls der MySQL-Treiber zickt, müsste man hier 'sbyte' oder 'int' nehmen und konvertieren.
        // Wir gehen davon aus, dass es als bool ankommt.
        public bool Erstanmeldung { get; set; }

        // ==========================================================
        // 2. LOGIK-BRÜCKEN (Für Kompatibilität zur App)
        // ==========================================================

        // Deine App nutzt "TeilnehmerID" (ohne Unterstrich). Wir leiten es weiter.
        public int TeilnehmerID => Teilnehmer_ID;

        // Deine App nutzt "RehaNummer" (als String).
        // LOGIK: Da RehaNummer == Teilnehmer_ID ist, geben wir die ID einfach als String zurück.
        public string RehaNummer => Teilnehmer_ID.ToString();

        // Deine App nutzt "MussPasswortAendern".
        // Das entspricht jetzt dem Feld "Erstanmeldung".
        public bool MussPasswortAendern
        {
            get { return Erstanmeldung; }
            set { Erstanmeldung = value; }
        }

        // Deine App erwartet einen Kurs-Namen (String).
        // Da wir nur die Kurs_ID haben, bauen wir einen provisorischen String.
        // (Für den echten Namen bräuchte man später einen SQL-JOIN auf die Kurs-Tabelle).
        public string Kurs => $"Kurs-ID: {Kurs_ID}";
    }
}