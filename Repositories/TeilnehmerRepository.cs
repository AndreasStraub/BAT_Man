using MySql.Data.MySqlClient;
using System;
using BAT_Man.Models;
using System.Windows;

namespace BAT_Man.Repositories
{
    /// <summary>
    /// Kapselt den Datenbankzugriff für Teilnehmer-Daten.
    /// Erbt die Verbindungslogik von der Klasse 'BaseRepository'.
    /// </summary>
    public class TeilnehmerRepository : BaseRepository
    {
        /// <summary>
        /// Lädt einen Teilnehmer anhand der eingegebenen Reha-Nummer aus der Datenbank.
        /// </summary>
        /// <param name="eingabeNummer">Die Eingabe aus dem Login-Feld (als String).</param>
        /// <returns>Ein fertiges Teilnehmer-Objekt oder 'null', falls nicht gefunden/ungültig.</returns>
        public Teilnehmer GetTeilnehmerByRehaNummer(string eingabeNummer)
        {
            Teilnehmer gefunden = null;

            // SCHRITT 1: Validierung und Konvertierung
            if (!int.TryParse(eingabeNummer, out int gesuchteId))
            {
                // Ungültige Eingabe (keine Zahl) -> Kein Datenbankzugriff nötig.
                return null;
            }

            // SCHRITT 2: SQL-Abfrage vorbereiten
            string query = "select t.Teilnehmer_ID,  " +
                                    "t.Vorname, " +
                                    "t.Nachname, " +
                                    "t.EMail, " +
                                    "t.Rolle_ID, " +
                                    "t.Kurs_ID, " +
                                    "k.Name as KursName, " +
                                    "t.Fachrichtung_ID, " +
                                    "t.Erstanmeldung " +
                                    "from Teilnehmer t " +
                            "join Kurs k on t.Kurs_ID = k.Kurs_ID " +
                            "where t.Teilnehmer_ID = @Id LIMIT 1";
            try
            {
                // 'GetConnection()' stammt aus der Basisklasse BaseRepository (siehe Kap. 5).
                // Das 'using'-Statement garantiert, dass die Verbindung danach geschlossen wird.
                using (MySqlConnection connection = GetConnection())
                {
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        // WICHTIG: Parameter statt String-Verkettung verhindert SQL-Injection!
                        command.Parameters.AddWithValue("@Id", gesuchteId);
                        
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                gefunden = new Teilnehmer
                                {
                                    // Pflichtfelder (Integer und Strings)
                                    Teilnehmer_ID = reader.GetInt32("Teilnehmer_ID"),
                                    Vorname = reader["Vorname"] as string,
                                    Nachname = reader["Nachname"] as string,
                                    EMail = reader["EMail"] as string,
                                    Kurs = reader["KursName"] as string,

                                    // Optionale Felder / Umgang mit NULL
                                    // Prüfung: Ist der Wert in der DB 'DBNull'? Wenn ja -> 0, sonst -> Wert.
                                    Rolle_ID = reader["Rolle_ID"] != DBNull.Value ? reader.GetInt32("Rolle_ID") : 0,
                                    // Kurs_ID = reader["Kurs_ID"] != DBNull.Value ? reader.GetInt32("Kurs_ID") : 0,
                                    Fachrichtung_ID = reader["Fachrichtung_ID"] != DBNull.Value ? reader.GetInt32("Fachrichtung_ID") : 0,

                                    // Boolesche Werte (TINYINT in MySQL)
                                    // Convert.ToBoolean wandelt 1 in true und 0 in false um.
                                    Erstanmeldung = reader["Erstanmeldung"] != DBNull.Value
                                                    && Convert.ToBoolean(reader["Erstanmeldung"])
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Im Fehlerfall (z.B. Server nicht erreichbar) wird der Benutzer benachrichtigt.
                // In einer echten Anwendung würde man hier zusätzlich ein Error-Log schreiben.
                System.Windows.MessageBox.Show("Datenbankfehler beim Laden des Teilnehmers:\n" + ex.Message);
            }

            return gefunden;
        }
    }
}