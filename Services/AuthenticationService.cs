using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using BAT_Man.Models;
using BAT_Man.Repositories;

namespace BAT_Man.Services
{
    /// <summary>
    /// Zentraler Dienst für die Benutzer-Authentifizierung.
    /// <para>
    /// ARCHITEKTUR:
    /// Nutzt ein hybrides Modell:
    /// 1. Validierung der Zugangsdaten (Passwort) über eine externe Web-API (PHP).
    /// 2. Laden der Benutzerdaten aus der lokalen MySQL-Datenbank.
    /// </para>
    /// </summary>
    public class AuthenticationService
    {
        // --- Singleton-Implementierung ---

        /// <summary>
        /// Die einzige Instanz der Klasse (Singleton).
        /// </summary>
        public static AuthenticationService Instance { get; } = new AuthenticationService();

        // --- Private Felder ---

        private readonly TeilnehmerRepository _teilnehmerRepository;

        // HTTP-Client für Web-Anfragen.
        // WICHTIG: Muss statisch sein, um "Socket Exhaustion" zu vermeiden.
        private static readonly HttpClient _httpClient = new HttpClient();

        /// <summary>
        /// Privater Konstruktor verhindert Instanziierung von außen.
        /// </summary>
        private AuthenticationService()
        {
            _teilnehmerRepository = new TeilnehmerRepository();
        }

        // ==========================================================
        // 1. HILFSMETHODE: Validierung
        // ==========================================================

        /// <summary>
        /// Prüft, ob ein Passwort den Sicherheitsanforderungen entspricht.
        /// (Mind. 8 Zeichen, Groß-/Kleinbuchstaben, Ziffern, Sonderzeichen).
        /// </summary>
        /// <param name="password">Das zu prüfende Passwort.</param>
        /// <returns>True, wenn das Passwort sicher ist.</returns>
        private bool IsPasswordStrong(string password)
        {
            if (string.IsNullOrEmpty(password)) return false;
            if (password.Length < 8) return false;

            // LINQ-Abfragen prüfen das Vorhandensein bestimmter Zeichentypen
            if (!password.Any(char.IsUpper)) return false; // Muss Großbuchstaben enthalten
            if (!password.Any(char.IsLower)) return false; // Muss Kleinbuchstaben enthalten
            if (!password.Any(char.IsDigit)) return false; // Muss Ziffern enthalten

            // Prüfung auf Sonderzeichen (alles was kein Buchstabe oder Ziffer ist)
            if (!password.Any(ch => !char.IsLetterOrDigit(ch))) return false;

            return true;
        }

        // ==========================================================
        // 2. HAUPT-METHODE: Der Login-Prozess
        // ==========================================================

        /// <summary>
        /// Führt den vollständigen Login-Prozess durch.
        /// </summary>
        /// <param name="rehaNummer">Die Benutzer-ID.</param>
        /// <param name="passwort">Das Passwort im Klartext.</param>
        /// <returns>Das geladene Teilnehmer-Objekt bei Erfolg, sonst null.</returns>
        public async Task<Teilnehmer> Login(string rehaNummer, string passwort)
        {
            // SCHRITT A: Authentifizierung via HTTP (PHP-API)
            // Die Prüfung des Passwort-Hashs erfolgt auf dem Server, um Sicherheitsrisiken zu minimieren.
            // 'await' gibt die Kontrolle an die UI zurück, bis der Server antwortet.
            bool istLoginGueltig = await PruefeLoginExtern(rehaNummer, passwort);

            if (istLoginGueltig)
            {
                // SCHRITT B: Datenabruf via SQL (Direktverbindung)
                // Nach erfolgreicher Prüfung werden die Nutzerdaten über den MySQL-Treiber geladen.
                // Dies erfolgt auf demselben Server, nutzt aber den Port 3306 für Performanz.
                Teilnehmer user = _teilnehmerRepository.GetTeilnehmerByRehaNummer(rehaNummer);

                if (user == null)
                {
                    // Fehlerfall: Inkonsistenz oder Verbindungsproblem
                    // Dieser Fall tritt ein, wenn zwar der Webserver (Port 80) antwortet,
                    // aber die direkte Datenbankverbindung (Port 3306) scheitert
                    // oder das Repository eine Exception abgefangen hat.
                    MessageBox.Show("Benutzer konnte validiert werden, aber die Profildaten sind nicht abrufbar (SQL-Verbindungsfehler).", "Daten-Inkonsistenz");
                }

                return user;
            }

            // Login fehlgeschlagen (Passwort falsch oder API verweigert Zugriff)
            return null;
        }

        // ==========================================================
        // 3. INTERNE LOGIK: API-Kommunikation
        // ==========================================================

        /// <summary>
        /// Sendet die Zugangsdaten an die PHP-API zur Überprüfung.
        /// </summary>
        /// <returns>True, wenn die API "success" meldet.</returns>
        private async Task<bool> PruefeLoginExtern(string rehaNummer, string passwort)
        {
            try
            {
                // 1. Datenpaket für POST-Request vorbereiten (Key-Value Paare)
                var values = new Dictionary<string, string>
                {
                    { "login_type", "Teilnehmer" },
                    { "reha_nr", rehaNummer },
                    { "passwort", passwort }
                };

                // Verpacken als Form-Data (wie ein HTML-Formular)
                var content = new FormUrlEncodedContent(values);

                // 2. HTTP POST Anfrage senden
                var response = await _httpClient.PostAsync("http://192.168.9.123/it202407/auth/login.php", content);

                // 3. Antwort auswerten
                if (response.IsSuccessStatusCode)
                {
                    // Den Antwort-Text (JSON) lesen
                    var jsonString = await response.Content.ReadAsStringAsync();

                    // JSON parsen (nutzt das moderne System.Text.Json)
                    using (JsonDocument doc = JsonDocument.Parse(jsonString))
                    {
                        // Wir suchen nach der Eigenschaft "status" im JSON
                        if (doc.RootElement.TryGetProperty("status", out JsonElement statusElement))
                        {
                            string status = statusElement.GetString();

                            // Optionale Nachricht vom Server lesen (z.B. Fehlergrund)
                            string meldung = "";
                            if (doc.RootElement.TryGetProperty("message", out JsonElement msgElement))
                            {
                                meldung = msgElement.GetString();
                            }

                            // Fallunterscheidung basierend auf der API-Antwort
                            if (status == "success")
                            {
                                return true; // Login erfolgreich
                            }
                            else if (status == "password_change_required")
                            {
                                // Sonderfall: Login ist korrekt, aber Passwort muss geändert werden.
                                // Der User kann die App starten (return true), aber die App.xaml.cs 
                                // wird das Flag in der DB erkennen und den Dialog erzwingen.
                                MessageBox.Show(meldung, "Hinweis", MessageBoxButton.OK, MessageBoxImage.Information);
                                return true;
                            }
                            else
                            {
                                // Fehler anzeigen (z.B. "Falsches Passwort")
                                if (!string.IsNullOrEmpty(meldung)) MessageBox.Show(meldung);
                                return false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Netzwerkfehler (z.B. Server down, kein Kabel)
                MessageBox.Show("Fehler beim Login-Server: " + ex.Message);
                return false;
            }

            return false;
        }

        // ==========================================================
        // 4. PASSWORT ÄNDERN (API)
        // ==========================================================

        /// <summary>
        /// Sendet eine Anfrage zur Passwortänderung an die API.
        /// </summary>
        /// <param name="teilnehmerId">Interne ID (wird hier nicht direkt für den Request genutzt, aber zur Verifizierung).</param>
        /// <param name="rehaNummer">Die Benutzer-ID für die API.</param>
        /// <param name="neuesPasswort">Das neue Passwort.</param>
        public async Task<bool> ChangePasswordAsync(int teilnehmerId, string rehaNummer, string neuesPasswort)
        {
            // 1. Client-seitige Validierung (spart unnötigen Server-Traffic)
            if (!IsPasswordStrong(neuesPasswort))
            {
                MessageBox.Show("Das Passwort muss mind. 8 Zeichen, Groß-/Kleinbuchstaben, Ziffern und Sonderzeichen enthalten.", "Passwort unsicher");
                return false;
            }

            try
            {
                // 2. Datenpaket schnüren
                var values = new Dictionary<string, string>
                {
                    { "reha_nr", rehaNummer },
                    { "neues_passwort", neuesPasswort }
                };

                var content = new FormUrlEncodedContent(values);

                // 3. Request senden
                var response = await _httpClient.PostAsync("http://192.168.9.123/it202407/auth/teilnehmer/change_password.php", content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();

                    // Einfache Erfolgsprüfung im Antwort-String
                    if (jsonString.Contains("success"))
                    {
                        // WICHTIG: Lokalen Status aktualisieren (RAM-Update)
                        var sitzung = AktiveSitzung.Instance;
                        if (sitzung.IstAngemeldet() && sitzung.AngemeldeterTeilnehmer.TeilnehmerID == teilnehmerId)
                        {
                            sitzung.AngemeldeterTeilnehmer.MussPasswortAendern = false;
                        }

                        return true;
                    }
                    else
                    {
                        MessageBox.Show("Server meldet Fehler beim Ändern: " + jsonString);
                    }
                }
                else
                {
                    MessageBox.Show("Server-Fehler: " + response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Verbindungsfehler beim Passwort-Ändern: " + ex.Message);
            }

            return false;
        }
    }
}