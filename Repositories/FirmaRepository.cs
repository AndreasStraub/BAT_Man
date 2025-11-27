using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Windows;
using WPF_Test.Models;
using WPF_Test.Services;
using System;

namespace WPF_Test.Repositories
{
    /// <summary>
    /// Repository für die Verwaltung von Firmen-Datensätzen.
    /// Verwaltet das Laden, Speichern, Aktualisieren und Löschen von Firmen.
    /// </summary>
    public class FirmaRepository : BaseRepository
    {
        // --- Private Helfermethode ---

        /// <summary>
        /// Ermittelt die ID des aktuell angemeldeten Teilnehmers.
        /// Wirft einen Fehler, wenn niemand angemeldet ist (Sicherheits-Check).
        /// </summary>
        private int GetAktuelleTeilnehmerID()
        {
            if (!AktiveSitzung.Instance.IstAngemeldet())
            {
                throw new InvalidOperationException("Kein Teilnehmer angemeldet! Datenbankzugriff verweigert.");
            }
            return AktiveSitzung.Instance.AngemeldeterTeilnehmer.TeilnehmerID;
        }

        // --- Öffentliche Methoden ---

        /// <summary>
        /// Liest ALLE Firmen, die dem AKTUELLEN Teilnehmer gehören.
        /// Wird für Dropdowns (ComboBox) verwendet.
        /// </summary>
        public List<Firma> GetAlleFirmen()
        {
            var firmenListe = new List<Firma>();
            int teilnehmerId = GetAktuelleTeilnehmerID();
            string query = "SELECT * FROM Firma WHERE Teilnehmer_ID = @TeilnehmerID";

            try
            {
                using (MySqlConnection connection = GetConnection())
                {
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@TeilnehmerID", teilnehmerId);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Firma firma = new Firma
                                {
                                    // Pflichtfelder (NOT NULL in DB)
                                    Firma_ID = reader.GetInt32("Firma_ID"),
                                    Teilnehmer_ID = reader.GetInt32("Teilnehmer_ID"),
                                    Firmenname = reader.GetString("Firmenname"),

                                    // Optionale Felder (NULL in DB möglich)
                                    // 'as string' wandelt DBNull sicher in null um.
                                    Strasse = reader["Strasse"] as string,
                                    Hausnummer = reader["Hausnummer"] as string,
                                    PLZ = reader["PLZ"] as string,
                                    Ort = reader["Ort"] as string,
                                    Ansprechpartner = reader["Ansprechpartner"] as string,
                                    Telefon = reader["Telefon"] as string,
                                    EMail = reader["EMail"] as string
                                };
                                firmenListe.Add(firma);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                if (ex.InnerException != null) errorMessage += "\n\nInnerer Fehler: " + ex.InnerException.Message;
                MessageBox.Show($"Fehler beim Laden der Firmen:\n{errorMessage}", "Datenbankfehler");
            }

            return firmenListe;
        }

        /// <summary>
        /// Liest ALLE Firmen inkl. des LETZTEN Statusberichts und Kommentars.
        /// Wird für die Haupt-Übersichtstabelle verwendet.
        /// </summary>
        public List<Firma> GetAlleFirmenMitLetztemStatus()
        {
            var firmenListe = new List<Firma>();
            int teilnehmerId = GetAktuelleTeilnehmerID();
            string aktuelleSprache = LanguageService.Instance.AktuelleSprache;

            // Komplexe Abfrage: Holt zu jeder Firma den NEUESTEN Aktivitäts-Eintrag.
            // Nutzt 'ROW_NUMBER()' zur Sortierung.
            string query = @"
                SELECT 
                    f.*, 
                    la.StatusBezeichnung AS LetzterStatus, 
                    la.Kommentar AS LetzteBemerkung
                FROM 
                    Firma f
                LEFT JOIN 
                (
                    SELECT 
                        a.Firma_ID, 
                        a.Kommentar, 
                        st.Bezeichnung AS StatusBezeichnung,
                        ROW_NUMBER() OVER(PARTITION BY a.Firma_ID ORDER BY a.Datum DESC) as rn
                    FROM 
                        Aktivitaet a
                    JOIN 
                        Status s ON a.Status_ID = s.Status_ID
                    JOIN 
                        Status_Translation st ON s.Status_ID = st.Status_ID
                    WHERE 
                        st.LanguageCode = @Sprache
                ) AS la ON f.Firma_ID = la.Firma_ID AND la.rn = 1
                WHERE 
                    f.Teilnehmer_ID = @TeilnehmerID
                ORDER BY 
                    f.Firmenname";

            try
            {
                using (MySqlConnection connection = GetConnection())
                {
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@TeilnehmerID", teilnehmerId);
                        command.Parameters.AddWithValue("@Sprache", aktuelleSprache);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Firma firma = new Firma
                                {
                                    Firma_ID = reader.GetInt32("Firma_ID"),
                                    Teilnehmer_ID = reader.GetInt32("Teilnehmer_ID"),
                                    Firmenname = reader.GetString("Firmenname"),
                                    Strasse = reader["Strasse"] as string,
                                    Hausnummer = reader["Hausnummer"] as string,
                                    PLZ = reader["PLZ"] as string,
                                    Ort = reader["Ort"] as string,
                                    Ansprechpartner = reader["Ansprechpartner"] as string,
                                    Telefon = reader["Telefon"] as string,
                                    EMail = reader["EMail"] as string,

                                    // Zusatz-Felder aus dem JOIN
                                    LetzterStatus = reader["LetzterStatus"] as string,
                                    LetzteBemerkung = reader["LetzteBemerkung"] as string
                                };
                                firmenListe.Add(firma);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                if (ex.InnerException != null) errorMessage += "\n\nInnerer Fehler: " + ex.InnerException.Message;
                MessageBox.Show($"Fehler beim Laden der Übersicht:\n{errorMessage}", "Datenbankfehler");
            }

            return firmenListe;
        }

        /// <summary>
        /// Fügt eine neue Firma in die Datenbank ein.
        /// </summary>
        public void AddFirma(Firma neueFirma)
        {
            int teilnehmerId = GetAktuelleTeilnehmerID();

            string query = @"
                INSERT INTO Firma 
                (Teilnehmer_ID, Firmenname, Strasse, Hausnummer, PLZ, Ort, Ansprechpartner, Telefon, EMail) 
                VALUES 
                (@Teilnehmer_ID, @Firmenname, @Strasse, @Hausnummer, @PLZ, @Ort, @Ansprechpartner, @Telefon, @EMail)";

            try
            {
                using (MySqlConnection connection = GetConnection())
                {
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Teilnehmer_ID", teilnehmerId);
                        command.Parameters.AddWithValue("@Firmenname", neueFirma.Firmenname);
                        command.Parameters.AddWithValue("@Strasse", neueFirma.Strasse);
                        command.Parameters.AddWithValue("@Hausnummer", neueFirma.Hausnummer);
                        command.Parameters.AddWithValue("@PLZ", neueFirma.PLZ);
                        command.Parameters.AddWithValue("@Ort", neueFirma.Ort);
                        command.Parameters.AddWithValue("@Ansprechpartner", neueFirma.Ansprechpartner);
                        command.Parameters.AddWithValue("@Telefon", neueFirma.Telefon);
                        command.Parameters.AddWithValue("@EMail", neueFirma.EMail);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                if (ex.InnerException != null) errorMessage += "\n\nInnerer Fehler: " + ex.InnerException.Message;
                MessageBox.Show($"Fehler beim Hinzufügen der Firma:\n{errorMessage}", "Datenbankfehler");
            }
        }

        /// <summary>
        /// Aktualisiert eine bestehende Firma.
        /// </summary>
        public void UpdateFirma(Firma firma)
        {
            int teilnehmerId = GetAktuelleTeilnehmerID();

            string query = @"
                UPDATE Firma SET 
                    Firmenname = @Firmenname,
                    Strasse = @Strasse,
                    Hausnummer = @Hausnummer,
                    PLZ = @PLZ,
                    Ort = @Ort,
                    Ansprechpartner = @Ansprechpartner,
                    Telefon = @Telefon,
                    EMail = @EMail
                WHERE 
                    Firma_ID = @FirmaID 
                AND 
                    Teilnehmer_ID = @TeilnehmerID"; // Sicherheit: Nur eigene Firmen ändern!

            try
            {
                using (MySqlConnection connection = GetConnection())
                {
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Firmenname", firma.Firmenname);
                        command.Parameters.AddWithValue("@Strasse", firma.Strasse);
                        command.Parameters.AddWithValue("@Hausnummer", firma.Hausnummer);
                        command.Parameters.AddWithValue("@PLZ", firma.PLZ);
                        command.Parameters.AddWithValue("@Ort", firma.Ort);
                        command.Parameters.AddWithValue("@Ansprechpartner", firma.Ansprechpartner);
                        command.Parameters.AddWithValue("@Telefon", firma.Telefon);
                        command.Parameters.AddWithValue("@EMail", firma.EMail);

                        // WHERE-Klausel
                        command.Parameters.AddWithValue("@FirmaID", firma.Firma_ID);
                        command.Parameters.AddWithValue("@TeilnehmerID", teilnehmerId);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                if (ex.InnerException != null) errorMessage += "\n\nInnerer Fehler: " + ex.InnerException.Message;
                MessageBox.Show($"Fehler beim Aktualisieren der Firma:\n{errorMessage}", "Datenbankfehler");
            }
        }

        /// <summary>
        /// Löscht eine Firma UND alle ihre Aktivitäten endgültig (Transaktion).
        /// </summary>
        public void DeleteFirma(int firmaId)
        {
            int teilnehmerId = GetAktuelleTeilnehmerID();

            using (MySqlConnection connection = GetConnection())
            {
                // Start einer Transaktion: Alles oder Nichts.
                using (MySqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Zuerst alle Aktivitäten dieser Firma löschen (Referenzielle Integrität)
                        string deleteAktivitaetenQuery = "DELETE FROM Aktivitaet WHERE Firma_ID = @FirmaID";
                        using (MySqlCommand cmd1 = new MySqlCommand(deleteAktivitaetenQuery, connection, transaction))
                        {
                            cmd1.Parameters.AddWithValue("@FirmaID", firmaId);
                            cmd1.ExecuteNonQuery();
                        }

                        // 2. Dann die Firma selbst löschen
                        string deleteFirmaQuery = "DELETE FROM Firma WHERE Firma_ID = @FirmaID AND Teilnehmer_ID = @TeilnehmerID";
                        using (MySqlCommand cmd2 = new MySqlCommand(deleteFirmaQuery, connection, transaction))
                        {
                            cmd2.Parameters.AddWithValue("@FirmaID", firmaId);
                            cmd2.Parameters.AddWithValue("@TeilnehmerID", teilnehmerId);
                            cmd2.ExecuteNonQuery();
                        }

                        // Erfolg: Transaktion abschließen
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        // Fehler: Alles rückgängig machen
                        transaction.Rollback();
                        throw; // Fehler weiterwerfen
                    }
                }
            }
        }
    }
}