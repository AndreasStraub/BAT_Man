using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http; // <!*-- WICHTIG: Für HttpClient --*!>
using System.Text.Json; // <!*-- WICHTIG: Für JSON Parsing (statt Newtonsoft) --*!>
using System.Threading.Tasks; // <!*-- WICHTIG: Für async/await --*!>
using System.Windows;
using WPF_Test.Models;
using WPF_Test.Repositories;

namespace WPF_Test.Services
{
    public class AuthenticationService
    {
        public static AuthenticationService Instance { get; } = new AuthenticationService();

        private readonly TeilnehmerRepository _teilnehmerRepository;

        // <!*-- 
        // HttpClient sollte statisch sein und wiederverwendet werden, 
        // um "Socket Exhaustion" zu vermeiden.
        // --*!>
        private static readonly HttpClient _httpClient = new HttpClient();

        private AuthenticationService()
        {
            _teilnehmerRepository = new TeilnehmerRepository();
        }

        // ==========================================================
        // 1. HILFSMETHODE
        // ==========================================================
        private bool IsPasswordStrong(string password)
        {
            if (string.IsNullOrEmpty(password)) return false;
            if (password.Length < 8) return false;
            if (!password.Any(char.IsUpper)) return false;
            if (!password.Any(char.IsLower)) return false;
            if (!password.Any(char.IsDigit)) return false;
            if (!password.Any(ch => !char.IsLetterOrDigit(ch))) return false;

            return true;
        }

        // ==========================================================
        // 2. Die Logik-Methode (Jetzt ASYNCHRON)
        // ==========================================================
        // <!*-- ÄNDERUNG: Rückgabetyp ist jetzt Task<Teilnehmer> statt nur Teilnehmer --*!>
        public async Task<Teilnehmer> Login(string rehaNummer, string passwort)
        {
            // SCHRITT A: Externe Prüfung via HTTP (wartet auf Antwort...)
            bool istLoginGueltig = await PruefeLoginExtern(rehaNummer, passwort);

            if (istLoginGueltig)
            {
                // SCHRITT B: Wenn JA -> Lade UNSERE Daten aus lokaler DB
                Teilnehmer user = _teilnehmerRepository.GetTeilnehmerByRehaNummer(rehaNummer);

                if (user == null)
                {
                    // Fallback: User existiert im PHP-System, aber nicht bei uns lokal.
                    // Hier könnte man Logging betreiben.
                }

                return user;
            }

            return null;
        }

        // ==========================================================
        // 3. Die externe Prüfung (Implementiert mit Code der Auth-Gruppe)
        // ==========================================================
        private async Task<bool> PruefeLoginExtern(string rehaNummer, string passwort)
        {
            try
            {
                // Datenpaket schnüren (wie von der anderen Gruppe vorgegeben)
                var values = new Dictionary<string, string>
                {
                    { "login_type", "Teilnehmer" },
                    { "reha_nr", rehaNummer },
                    { "passwort", passwort }
                };

                var content = new FormUrlEncodedContent(values);

                // Anfrage senden (an die IP der anderen Gruppe)
                // <!*-- Hinweis: Timeout beachten, falls Server offline ist --*!>
                var response = await _httpClient.PostAsync("http://192.168.9.123/BAT-Man-AT/auth/login.php", content);

                // Antwort lesen
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();

                    // JSON parsen mit System.Text.Json
                    using (JsonDocument doc = JsonDocument.Parse(jsonString))
                    {
                        // Wir suchen nach dem Feld "status"
                        if (doc.RootElement.TryGetProperty("status", out JsonElement statusElement))
                        {
                            string status = statusElement.GetString();
                            return status == "success";
                        }
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Es gab einen Netzwerkfehler. Bitte überprüfen Sie Ihre Verbindung.");
                // Bei Netzwerkfehlern (Server nicht erreichbar) geben wir "false" zurück.
                // In einer echten App würde man hier eine spezifische Fehlermeldung speichern.
                return false;
            }

            return false;
        }

        // ==========================================================
        // 4. Change Password
        // ==========================================================
        public bool ChangePassword(int teilnehmerId, string neuesPasswort)
        {
            if (!IsPasswordStrong(neuesPasswort))
            {
                throw new ArgumentException("Das Passwort muss mind. 8 Zeichen, Groß-/Kleinbuchstaben, Ziffern und Sonderzeichen enthalten.");
            }
            return true;
        }
    }
}