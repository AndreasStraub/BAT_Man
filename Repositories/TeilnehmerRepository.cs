using MySql.Data.MySqlClient;
using System;
using WPF_Test.Models;

namespace WPF_Test.Repositories
{
    public class TeilnehmerRepository : BaseRepository
    {
        /// <summary>
        /// Lädt einen Teilnehmer anhand der eingegebenen Nummer.
        /// Da RehaNummer = Teilnehmer_ID ist, suchen wir nach der ID.
        /// </summary>
        /// <param name="eingabeNummer">Der Text aus dem Login-Feld (z.B. "105")</param>
        public Teilnehmer GetTeilnehmerByRehaNummer(string eingabeNummer)
        {
            Teilnehmer gefunden = null;

            // SCHRITT 1: Konvertierung
            // Da die Datenbankspalte 'Teilnehmer_ID' ein INT ist, müssen wir den String umwandeln.
            if (!int.TryParse(eingabeNummer, out int gesuchteId))
            {
                // Wenn der User Buchstaben eingibt ("abc"), kann es keine ID sein. Abbruch.
                return null;
            }

            // SCHRITT 2: SQL Abfrage
            // Wir suchen in 'Teilnehmer_ID', laden aber alle Felder, die wir brauchen.
            string query = "SELECT * FROM Teilnehmer WHERE Teilnehmer_ID = @Id LIMIT 1";

            try
            {
                using (MySqlConnection connection = GetConnection())
                {
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", gesuchteId);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                gefunden = new Teilnehmer
                                {
                                    // Mapping: DB -> C#
                                    Teilnehmer_ID = reader.GetInt32("Teilnehmer_ID"),
                                    Vorname = reader["Vorname"] as string,
                                    Nachname = reader["Nachname"] as string,
                                    EMail = reader["EMail"] as string,

                                    // Umgang mit NULL bei IDs (falls das Feld leer sein darf)
                                    Rolle_ID = reader["Rolle_ID"] != DBNull.Value ? reader.GetInt32("Rolle_ID") : 0,
                                    Kurs_ID = reader["Kurs_ID"] != DBNull.Value ? reader.GetInt32("Kurs_ID") : 0,
                                    Fachrichtung_ID = reader["Fachrichtung_ID"] != DBNull.Value ? reader.GetInt32("Fachrichtung_ID") : 0,

                                    // Mapping tinyint(1) -> bool
                                    // Wir nutzen Convert.ToBoolean für maximale Sicherheit
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
                System.Windows.MessageBox.Show("Datenbankfehler beim Laden des Teilnehmers:\n" + ex.Message);
            }

            return gefunden;
        }
    }
}