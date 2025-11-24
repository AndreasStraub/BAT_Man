// Dateipfad: Repositories/AktivitaetRepository.cs

using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Windows;
using WPF_Test.Models;
using WPF_Test.Services;

namespace WPF_Test.Repositories
{
    public class AktivitaetRepository : BaseRepository
    {
        /// <summary>
        /// Holt alle Aktivitäten für EINE BESTIMMTE Firma
        /// in der AKTUELLEN Sprache.
        /// </summary>
        public List<Aktivitaet> GetAktivitaetenFuerFirma(int firmaId)
        {
            var aktivitaetenListe = new List<Aktivitaet>();
            string aktuelleSprache = LanguageService.Instance.AktuelleSprache;

            // ==========================================================
            // KORREKTUR: Wir laden 'a.Status_ID' jetzt mit
            // ==========================================================
            string query = @"
                SELECT 
                    a.Aktivitaet_ID, 
                    a.Datum, 
                    a.Kommentar, 
                    a.Status_ID,
                    st.Bezeichnung AS StatusBezeichnung
                FROM 
                    aktivitaet a
                JOIN 
                    status s ON a.Status_ID = s.Status_ID
                JOIN 
                    status_translation st ON s.Status_ID = st.Status_ID
                WHERE 
                    a.Firma_ID = @FirmaID 
                AND 
                    st.LanguageCode = @Sprache
                ORDER BY
                    a.Datum DESC";

            try
            {
                using (MySqlConnection connection = GetConnection())
                {
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@FirmaID", firmaId);
                        command.Parameters.AddWithValue("@Sprache", aktuelleSprache);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Aktivitaet akt = new Aktivitaet
                                {
                                    Aktivitaet_ID = reader.GetInt32("Aktivitaet_ID"),
                                    Datum = reader.GetDateTime("Datum"),
                                    Kommentar = reader["Kommentar"] as string,

                                    // ==================================================
                                    // KORREKTUR: Wir lesen die Status_ID
                                    // ==================================================
                                    Status_ID = reader.GetInt32("Status_ID"),

                                    StatusBezeichnung = reader.GetString("StatusBezeichnung")
                                };
                                aktivitaetenListe.Add(akt);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Lesen der Aktivitäten: {ex.Message}");
            }

            return aktivitaetenListe;
        }

        /// <summary>
        /// Fügt einen neuen Aktivitäts-Eintrag zur Datenbank hinzu.
        /// </summary>
        public void AddAktivitaet(Aktivitaet neueAktivitaet, int firmaId, int statusId)
        {
            string query = @"
                INSERT INTO aktivitaet
                (Firma_ID, Status_ID, Datum, Kommentar)
                VALUES
                (@FirmaID, @StatusID, @Datum, @Kommentar)";
            try
            {
                using (MySqlConnection connection = GetConnection())
                {
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@FirmaID", firmaId);
                        command.Parameters.AddWithValue("@StatusID", statusId);
                        command.Parameters.AddWithValue("@Datum", neueAktivitaet.Datum);
                        command.Parameters.AddWithValue("@Kommentar", neueAktivitaet.Kommentar);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // ... (Fehlermeldung) ...
            }
        }

        // ====================================================================
        // NEUE METHODE (Schritt 1 des neuen Plans)
        // ====================================================================

        /// <summary>
        /// Aktualisiert einen bestehenden Aktivitäts-Eintrag.
        /// </summary>
        public void UpdateAktivitaet(Aktivitaet aktivitaet, int statusId)
        {
            string query = @"
                UPDATE aktivitaet SET
                    Status_ID = @StatusID,
                    Datum = @Datum,
                    Kommentar = @Kommentar
                WHERE
                    Aktivitaet_ID = @AktivitaetID";

            try
            {
                using (MySqlConnection connection = GetConnection())
                {
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        // Parameter binden
                        command.Parameters.AddWithValue("@StatusID", statusId);
                        command.Parameters.AddWithValue("@Datum", aktivitaet.Datum);
                        command.Parameters.AddWithValue("@Kommentar", aktivitaet.Kommentar);
                        command.Parameters.AddWithValue("@AktivitaetID", aktivitaet.Aktivitaet_ID);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Aktualisieren der Aktivität: {ex.Message}");
            }
        }


        /// <summary>
        /// Löscht einen bestehenden Aktivitäts-Eintrag aus der Datenbank.
        /// </summary>
        public void DeleteAktivitaet(int aktivitaetId)
        {
            // 1. SQL-Befehl definieren
            string query = @"
                DELETE FROM aktivitaet 
                WHERE Aktivitaet_ID = @AktivitaetID";

            try
            {
                // 2. Verbindung holen (aus BaseRepository)
                using (MySqlConnection connection = GetConnection())
                {
                    // 3. Command erstellen
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        // 4. Parameter binden (Schutz vor SQL-Injection)
                        command.Parameters.AddWithValue("@AktivitaetID", aktivitaetId);

                        // 5. Löschbefehl ausführen
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // 6. Fehlerbehandlung (im Stil Ihrer anderen Methoden)
                MessageBox.Show($"Fehler beim Löschen der Aktivität: {ex.Message}");
            }
        }

    }
}