using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Windows;
using BAT_Man.Models;
using BAT_Man.Services;

namespace BAT_Man.Repositories
{
    /// <summary>
    /// Repository für die Verwaltung von Status-Einträgen.
    /// Lädt die verfügbaren Optionen (z.B. "Anruf", "E-Mail") aus der Datenbank.
    /// Erbt von BaseRepository, um die zentrale Verbindungslogik zu nutzen.
    /// </summary>
    public class StatusRepository : BaseRepository
    {
        /// <summary>
        /// Ruft alle Status-Optionen in der aktuell gewählten Sprache ab.
        /// </summary>
        /// <returns>Eine Liste von Status-Objekten (ID und Bezeichnung).</returns>
        public List<Status> GetAlleStatus()
        {
            var statusListe = new List<Status>();

            // 1. Ermittlung der aktuellen Sprache (z.B. "de" oder "en") über den globalen Service.
            string aktuelleSprache = LanguageService.Instance.AktuelleSprache;

            // 2. SQL-Abfrage mit JOIN
            // Die Tabelle 'status' enthält nur die IDs.
            // Die Tabelle 'status_translation' enthält die Texte in verschiedenen Sprachen.
            // Beide werden über die 'Status_ID' verknüpft.
            string query = @"
                SELECT 
                    s.Status_ID, 
                    st.Bezeichnung 
                FROM 
                    Status s
                JOIN 
                    Status_Translation st ON s.Status_ID = st.Status_ID
                WHERE 
                    st.LanguageCode = @Sprache";

            try
            {
                // 3. Verbindung aufbauen (Using-Statement stellt das Schließen sicher)
                using (MySqlConnection connection = GetConnection())
                {
                    // 4. Befehl vorbereiten
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        // 5. Parameter binden (Verhindert SQL-Injection)
                        command.Parameters.AddWithValue("@Sprache", aktuelleSprache);

                        // 6. Datenbankabfrage ausführen
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            // 7. Ergebnisse zeilenweise lesen
                            while (reader.Read())
                            {
                                Status status = new Status
                                {
                                    Status_ID = reader.GetInt32("Status_ID"),
                                    Bezeichnung = reader.GetString("Bezeichnung")
                                };
                                statusListe.Add(status);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Fehlerbehandlung: Anzeige einer MessageBox (in Produktion: Logging)
                MessageBox.Show($"Fehler beim Lesen der Status-Tabelle: {ex.Message}");
            }

            return statusListe;
        }
    }
}