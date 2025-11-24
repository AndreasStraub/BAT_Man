// Dateipfad: Models/Aktivitaet.cs
using System;

namespace WPF_Test.Models
{
    /// <summary>
    /// Repräsentiert einen einzelnen Aktivitäts-Eintrag (eine Zeile).
    /// </summary>
    public class Aktivitaet
    {
        public int Aktivitaet_ID { get; set; }
        public DateTime Datum { get; set; }
        public string Kommentar { get; set; }

        // ==========================================================
        // NEU: Wir speichern jetzt auch die ID des Status
        // ==========================================================
        // (Wird benötigt, um den RadioButton im "Bearbeiten"-Dialog
        // korrekt vor-auszuwählen)
        public int Status_ID { get; set; }

        // (Wird für die Anzeige in der ListView benötigt)
        public string StatusBezeichnung { get; set; }
    }
}