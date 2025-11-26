using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Windows;
using WPF_Test.Models;
using WPF_Test.Services;

namespace WPF_Test.Repositories
{
    /// <summary>
    /// Repository für die Verwaltung von Aktivitäts-Datensätzen.
    /// Erbt von BaseRepository, um die zentrale Verbindungslogik zu nutzen.
    /// </summary>
    public class AktivitaetRepository : BaseRepository
    {
        /// <summary>
        /// Liest alle Aktivitäten einer spezifischen Firma aus der Datenbank.
        /// Dabei wird die Bezeichnung des Status in der aktuell gewählten Sprache geladen.
        /// </summary>
        /// <param name="firmaId">Der Fremdschlüssel zur Identifikation der Firma.</param>
        /// <returns>Eine Liste von Aktivität-Objekten inkl. aufgelöstem Status-Text.</returns>
        public List<Aktivitaet> GetAktivitaetenFuerFirma(int firmaId)
        {
            var aktivitaetenListe = new List<Aktivitaet>();

            // Zugriff auf den globalen LanguageService (Singleton), um den aktuellen Sprachcode (z.B. "de-DE") zu erhalten.
            string aktuelleSprache = LanguageService.Instance.AktuelleSprache;

            // SQL-Query mit JOINs:
            // 1. Tabelle 'aktivitaet' (Basisdaten)
            // 2. JOIN 'status': Verknüpfung über Status_ID (technische Notwendigkeit)
            // 3. JOIN 'status_translation': Verknüpfung über Status_ID, um den Text in der korrekten Sprache zu finden.
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
                // Öffnen der Verbindung via BaseRepository.
                // Das 'using'-Statement garantiert, dass connection.Close() und connection.Dispose()
                // automatisch aufgerufen werden, sobald der Block verlassen wird.
                using (MySqlConnection connection = GetConnection())
                {
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        // Parameter-Binding verhindert SQL-Injection und sorgt für korrekte Typisierung.
                        command.Parameters.AddWithValue("@FirmaID", firmaId);
                        command.Parameters.AddWithValue("@Sprache", aktuelleSprache);

                        // ExecuteReader wird für SELECT-Abfragen verwendet.
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            // Iteration durch alle Ergebniszeilen.
                            while (reader.Read())
                            {
                                Aktivitaet akt = new Aktivitaet
                                {
                                    Aktivitaet_ID = reader.GetInt32("Aktivitaet_ID"),
                                    Datum = reader.GetDateTime("Datum"),

                                    // Sicherer Cast für Strings, die in der DB NULL sein könnten.
                                    // 'reader["Spalte"]' gibt 'DBNull' zurück, wenn das Feld leer ist.
                                    // Der 'as string' Operator wandelt 'DBNull' sicher in C# 'null' um.
                                    Kommentar = reader["Kommentar"] as string,

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
                // Fehlerbehandlung: In einer Produktionsumgebung würde hier zusätzlich ein Logging (z.B. in eine Datei) erfolgen.
                MessageBox.Show($"Fehler beim Lesen der Aktivitäten: {ex.Message}");
            }

            return aktivitaetenListe;
        }

        /// <summary>
        /// Fügt einen neuen Aktivitäts-Eintrag in die Datenbank ein.
        /// </summary>
        /// <param name="neueAktivitaet">Das zu speichernde Objekt.</param>
        /// <param name="firmaId">Die ID der zugehörigen Firma.</param>
        /// <param name="statusId">Die ID des gewählten Status.</param>
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

                        // ExecuteNonQuery wird für INSERT, UPDATE und DELETE verwendet.
                        // Es gibt die Anzahl der betroffenen Zeilen zurück (hier nicht ausgewertet).
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Hinzufügen der Aktivität: {ex.Message}");
            }
        }

        /// <summary>
        /// Aktualisiert einen bestehenden Aktivitäts-Eintrag.
        /// </summary>
        /// <param name="aktivitaet">Das Objekt mit den neuen Daten.</param>
        /// <param name="statusId">Die möglicherweise geänderte Status-ID.</param>
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
                        command.Parameters.AddWithValue("@StatusID", statusId);
                        command.Parameters.AddWithValue("@Datum", aktivitaet.Datum);
                        command.Parameters.AddWithValue("@Kommentar", aktivitaet.Kommentar);
                        // WICHTIG: Die ID wird für die WHERE-Klausel benötigt, damit nur der korrekte Datensatz geändert wird.
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
        /// <param name="aktivitaetId">Die ID des zu löschenden Datensatzes.</param>
        public void DeleteAktivitaet(int aktivitaetId)
        {
            string query = @"
                DELETE FROM aktivitaet 
                WHERE Aktivitaet_ID = @AktivitaetID";

            try
            {
                using (MySqlConnection connection = GetConnection())
                {
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@AktivitaetID", aktivitaetId);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Löschen der Aktivität: {ex.Message}");
            }
        }
    }
}