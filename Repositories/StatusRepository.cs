// Dateipfad: Repositories/StatusRepository.cs

using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Windows;
// WICHTIG: Imports für unsere eigenen Klassen
using WPF_Test.Models;
using WPF_Test.Services;

// WICHTIG: Der Namespace MUSS so lauten
namespace WPF_Test.Repositories
{
    /// <summary>
    /// Holt die 'Status'-Einträge (z.B. "Anruf", "Email")
    /// aus der Datenbank.
    /// Erbt von BaseRepository, um GetConnection() zu erhalten.
    /// </summary>
    public class StatusRepository : BaseRepository
    {
        /// <summary>
        /// Holt ALLE Status-Optionen in der AKTUELL ausgewählten Sprache.
        /// (Diese Methode wird vom 'AddAktivitaetViewModel' aufgerufen)
        /// </summary>
        /// <returns>Eine Liste von Status-Objekten</returns>
        public List<Status> GetAlleStatus()
        {
            var statusListe = new List<Status>();

            // 1. Hole die aktuelle Sprache (z.B. "de" oder "en")
            string aktuelleSprache = LanguageService.Instance.AktuelleSprache;

            // 2. Die SQL-Abfrage (JOIN über 2 Tabellen)
            //    Wir verknüpfen 'status' (s) mit 'status_translation' (st)
            string query = @"
                SELECT 
                    s.Status_ID, 
                    st.Bezeichnung 
                FROM 
                    status s
                JOIN 
                    status_translation st ON s.Status_ID = st.Status_ID
                WHERE 
                    st.LanguageCode = @Sprache";

            try
            {
                // 3. Verbindung holen (von BaseRepository)
                using (MySqlConnection connection = GetConnection())
                {
                    // 4. Command erstellen
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        // 5. Den Sprach-Parameter an die Abfrage binden
                        command.Parameters.AddWithValue("@Sprache", aktuelleSprache);

                        // 6. Reader ausführen
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            // 7. Jede Zeile lesen
                            while (reader.Read())
                            {
                                // 8. 'Status'-Objekt füllen
                                Status status = new Status
                                {
                                    Status_ID = reader.GetInt32("Status_ID"),
                                    Bezeichnung = reader.GetString("Bezeichnung")
                                };
                                statusListe.Add(status);
                            }
                        }
                    }
                } // 9. Verbindung wird hier automatisch geschlossen
            }
            catch (Exception ex)
            {
                // BESSER: Echten Logger verwenden
                MessageBox.Show($"Fehler beim Lesen der Status-Tabelle: {ex.Message}");
            }

            return statusListe;
        }
    }
}