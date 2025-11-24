// Dateipfad: Repositories/FirmaRepository.cs
// KORRIGIERTE VERSION (Behandelt NULL-Werte)

using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Windows;
using WPF_Test.Models;
using WPF_Test.Services;
using System;

namespace WPF_Test.Repositories
{
    public class FirmaRepository : BaseRepository
    {
        // --- Private Helfermethode (bleibt gleich) ---
        private int GetAktuelleTeilnehmerID()
        {
            if (!AktiveSitzung.Instance.IstAngemeldet())
            {
                throw new InvalidOperationException("...");
            }
            return AktiveSitzung.Instance.AngemeldeterTeilnehmer.TeilnehmerID;
        }


        /// <summary>
        /// Liest ALLE Firmen, die dem AKTUELLEN Teilnehmer gehören.
        /// </summary>
        public List<Firma> GetAlleFirmen()
        {
            var firmenListe = new List<Firma>();
            int teilnehmerId = GetAktuelleTeilnehmerID();
            string query = "SELECT * FROM firma WHERE Teilnehmer_ID = @TeilnehmerID";

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
                                    // Diese Felder sind (laut DB-Schema) 'NOT NULL'
                                    // und können 'GetInt32'/'GetString' sicher verwenden.
                                    Firma_ID = reader.GetInt32("Firma_ID"),
                                    Teilnehmer_ID = reader.GetInt32("Teilnehmer_ID"),
                                    Firmenname = reader.GetString("Firmenname"),

                                    // Diese Felder DÜRFEN NULL sein.
                                    // 'reader.GetString("Strasse")' würde abstürzen.
                                    //
                                    // 'reader["Strasse"] as string' ist der sichere Weg:
                                    // Es holt den Wert als Objekt und wandelt ihn in
                                    // einen 'string' um, ODER in 'null', falls der
                                    // DB-Wert 'DBNull.Value' ist (ohne Absturz).
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
                if (ex.InnerException != null)
                {
                    errorMessage += "\n\nInnerer Fehler: " + ex.InnerException.Message;
                }
                MessageBox.Show($"Fehler beim Lesen der Firmendaten:\n{errorMessage}", "Datenbankfehler");
            }


            return firmenListe;
        }

        public void AddFirma(Firma neueFirma)
        {
            int teilnehmerId = GetAktuelleTeilnehmerID();

            string query = @"
                INSERT INTO firma 
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
                if (ex.InnerException != null)
                {
                    errorMessage += "\n\nInnerer Fehler: " + ex.InnerException.Message;
                }
                MessageBox.Show($"Fehler beim Hinzufügen der Firma:\n{errorMessage}", "Datenbankfehler");
            }
        }

        /// <summary>
        /// Aktualisiert eine bestehende Firma in der Datenbank.
        /// (Stellt sicher, dass sie dem angemeldeten Teilnehmer gehört)
        /// </summary>
        public void UpdateFirma(Firma firma)
        {
            // 1. Hole die ID des angemeldeten Benutzers (Sicherheits-Check)
            int teilnehmerId = GetAktuelleTeilnehmerID();

            // 2. Die SQL-Abfrage für das Update
            string query = @"
                UPDATE firma SET 
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
                    Teilnehmer_ID = @TeilnehmerID"; // Extra Sicherheit

            try
            {
                using (MySqlConnection connection = GetConnection())
                {
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        // 3. Alle Parameter binden
                        command.Parameters.AddWithValue("@Firmenname", firma.Firmenname);
                        command.Parameters.AddWithValue("@Strasse", firma.Strasse);
                        command.Parameters.AddWithValue("@Hausnummer", firma.Hausnummer);
                        command.Parameters.AddWithValue("@PLZ", firma.PLZ);
                        command.Parameters.AddWithValue("@Ort", firma.Ort);
                        command.Parameters.AddWithValue("@Ansprechpartner", firma.Ansprechpartner);
                        command.Parameters.AddWithValue("@Telefon", firma.Telefon);
                        command.Parameters.AddWithValue("@EMail", firma.EMail);

                        // Die 'WHERE'-Parameter
                        command.Parameters.AddWithValue("@FirmaID", firma.Firma_ID);
                        command.Parameters.AddWithValue("@TeilnehmerID", teilnehmerId);

                        // 4. Befehl ausführen
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // (Der verbesserte Catch-Block)
                string errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += "\n\nInnerer Fehler: " + ex.InnerException.Message;
                }
                MessageBox.Show($"Fehler beim Aktualisieren der Firma:\n{errorMessage}", "Datenbankfehler");
            }
        }

        /// <summary>
        /// Liest ALLE Firmen inkl. des LETZTEN Statusberichts und Kommentars.
        /// (Aufwändige Abfrage, nur für die Firmenübersicht).
        /// </summary>
        public List<Firma> GetAlleFirmenMitLetztemStatus()
        {
            var firmenListe = new List<Firma>();
            int teilnehmerId = GetAktuelleTeilnehmerID();

            // WICHTIG: Wir brauchen die aktuelle Sprache für den JOIN
            string aktuelleSprache = LanguageService.Instance.AktuelleSprache;

            // Diese Abfrage ist komplex:
            // 1. Wähle alle Firmen des Teilnehmers (f.*)
            // 2. Erstelle eine Sub-Query (la), die alle Aktivitäten...
            // 3. ...mit einem 'ROW_NUMBER()' versieht, partitioniert (gruppiert)
            //    nach Firma_ID und absteigend nach Datum sortiert.
            // 4. 'la.rn = 1' ist somit der *neueste* Eintrag pro Firma.
            // 5. 'LEFT JOIN' stellt sicher, dass auch Firmen ohne Aktivität
            //    (mit NULL-Status) angezeigt werden.
            string query = @"
                SELECT 
                    f.*, 
                    la.StatusBezeichnung AS LetzterStatus, 
                    la.Kommentar AS LetzteBemerkung
                FROM 
                    firma f
                LEFT JOIN 
                (
                    SELECT 
                        a.Firma_ID, 
                        a.Kommentar, 
                        st.Bezeichnung AS StatusBezeichnung,
                        ROW_NUMBER() OVER(PARTITION BY a.Firma_ID ORDER BY a.Datum DESC) as rn
                    FROM 
                        aktivitaet a
                    JOIN 
                        status s ON a.Status_ID = s.Status_ID
                    JOIN 
                        status_translation st ON s.Status_ID = st.Status_ID
                    WHERE 
                        st.LanguageCode = @Sprache
                ) AS la ON f.Firma_ID = la.Firma_ID AND la.rn = 1
                WHERE 
                    f.Teilnehmer_ID = @TeilnehmerID
                ORDER BY 
                    f.Firmenname;
            ";

            try
            {
                using (MySqlConnection connection = GetConnection())
                {
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        // Parameter für die Abfrage binden
                        command.Parameters.AddWithValue("@TeilnehmerID", teilnehmerId);
                        command.Parameters.AddWithValue("@Sprache", aktuelleSprache);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Firma firma = new Firma
                                {
                                    // Standard-Felder (wie in GetAlleFirmen)
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

                                    // NEUE Felder (aus dem JOIN)
                                    // Wichtig: 'as string' verwenden, da sie NULL sein können!
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
                // (Der bestehende, gute Error-Handler)
                string errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += "\n\nInnerer Fehler: " + ex.InnerException.Message;
                }
                MessageBox.Show($"Fehler beim Lesen der Firmen-Übersicht:\n{errorMessage}", "Datenbankfehler");
            }

            return firmenListe;
        }

        /// <summary>
        /// Löscht eine Firma UND alle ihre Aktivitäten endgültig.
        /// </summary>
        public void DeleteFirma(int firmaId)
        {
            int teilnehmerId = GetAktuelleTeilnehmerID();

            // Wir nutzen eine TRANSAKTION, damit entweder alles gelöscht wird
            // oder gar nichts (falls ein Fehler passiert).
            using (MySqlConnection connection = GetConnection())
            {
                using (MySqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Zuerst alle Aktivitäten dieser Firma löschen
                        string deleteAktivitaetenQuery = "DELETE FROM aktivitaet WHERE Firma_ID = @FirmaID";
                        using (MySqlCommand cmd1 = new MySqlCommand(deleteAktivitaetenQuery, connection, transaction))
                        {
                            cmd1.Parameters.AddWithValue("@FirmaID", firmaId);
                            cmd1.ExecuteNonQuery();
                        }

                        // 2. Dann die Firma selbst löschen
                        // (Wir prüfen auch hier auf Teilnehmer_ID zur Sicherheit)
                        string deleteFirmaQuery = "DELETE FROM firma WHERE Firma_ID = @FirmaID AND Teilnehmer_ID = @TeilnehmerID";
                        using (MySqlCommand cmd2 = new MySqlCommand(deleteFirmaQuery, connection, transaction))
                        {
                            cmd2.Parameters.AddWithValue("@FirmaID", firmaId);
                            cmd2.Parameters.AddWithValue("@TeilnehmerID", teilnehmerId);
                            cmd2.ExecuteNonQuery();
                        }

                        // Wenn wir hier sind, hat alles geklappt -> Bestätigen
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        // Fehler passiert -> Alles rückgängig machen
                        transaction.Rollback();
                        throw; // Fehler weiterwerfen, damit das ViewModel ihn anzeigen kann
                    }
                }
            }
        }

    }
}