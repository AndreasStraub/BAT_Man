using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http; // <!*-- WICHTIG: Für HttpClient --*!>
using System.Text.Json; // <!*-- WICHTIG: Für JSON Parsing (statt Newtonsoft) --*!>
using System.Threading.Tasks; // <!*-- WICHTIG: Für async/await --*!>
using System.Windows;
using BAT_Man.Models;
using BAT_Man.Repositories;

namespace BAT_Man.Services
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
                    // Fallback: User existiert im PHP-System, aber nicht lokal.
                    MessageBox.Show("Benutzer im externen System gefunden, aber nicht in der lokalen Datenbank.", "Daten-Inkonsistenz");
                }

                return user;
            }

            return null;
        }

        // ==========================================================
        // 3. Die externe Prüfung (Login)
        // ==========================================================
        private async Task<bool> PruefeLoginExtern(string rehaNummer, string passwort)
        {
            try
            {
                // Datenpaket schnüren
                var values = new Dictionary<string, string>
                {
                    { "login_type", "Teilnehmer" },
                    { "reha_nr", rehaNummer },
                    { "passwort", passwort }
                };

                var content = new FormUrlEncodedContent(values);

                // Hinweis: URL ggf. anpassen, wenn sich IP ändert
                var response = await _httpClient.PostAsync("http://192.168.9.123/it202407/auth/login.php", content);

                // Antwort lesen
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();

                    // JSON parsen mit System.Text.Json
                    using (JsonDocument doc = JsonDocument.Parse(jsonString))
                    {
                        if (doc.RootElement.TryGetProperty("status", out JsonElement statusElement))
                        {
                            string status = statusElement.GetString();

                            // <!*-- HINWEIS: message ist optional, daher TryGet --*!>
                            string meldung = "";
                            if (doc.RootElement.TryGetProperty("message", out JsonElement msgElement))
                            {
                                meldung = msgElement.GetString();
                            }

                            if (status == "success")
                            {
                                return true; // Login erfolgreich
                            }
                            else if (status == "password_change_required")
                            {
                                // <!*-- WICHTIG: KEINE Rekursion hier! --*!>
                                // Wir geben 'true' zurück. Das bedeutet: "Nutzerdaten sind korrekt."
                                // Dass das Passwort geändert werden MUSS, steht in der Datenbank (Feld: Erstanmeldung).
                                // Die App.xaml.cs prüft dieses Feld NACH dem Login und öffnet dann den Dialog.
                                MessageBox.Show(meldung, "Hinweis", MessageBoxButton.OK, MessageBoxImage.Information);
                                return true;
                            }
                            else
                            {
                                // Fehler anzeigen (z.B. falsches Passwort)
                                if (!string.IsNullOrEmpty(meldung)) MessageBox.Show(meldung);
                                return false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Login-Server: " + ex.Message);
                return false;
            }

            return false;
        }

        // ==========================================================
        // 4. Change Password (API Variante)
        // ==========================================================
        // <!*-- Implementierung von 'Möglichkeit 1' via API --*!>
        public async Task<bool> ChangePasswordAsync(int teilnehmerId, string rehaNummer, string neuesPasswort)
        {
            // 1. Validierung
            if (!IsPasswordStrong(neuesPasswort))
            {
                MessageBox.Show("Das Passwort muss mind. 8 Zeichen, Groß-/Kleinbuchstaben, Ziffern und Sonderzeichen enthalten.", "Passwort unsicher");
                return false;
            }

            //return true; // VORÜBERGEHEND, bis die API fertig ist.

            try
            {
                // 2. Datenpaket für change_password.php
                var values = new Dictionary<string, string>
                {
                    //{ "action", "change_password" },
                    { "reha_nr", rehaNummer }, // API identifiziert User über RehaNr
                    { "neues_passwort", neuesPasswort }
                };

                var content = new FormUrlEncodedContent(values);

                var response = await _httpClient.PostAsync("http://192.168.9.123/it202407/auth/teilnehmer/change_password.php", content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();

                    // Einfache Prüfung auf Erfolg
                    if (jsonString.Contains("success"))
                    {
                        // <!*-- WICHTIG: RAM-UPDATE --*!>
                        // Wir müssen dem Singleton sagen, dass der User sein Passwort jetzt geändert hat.
                        // Sonst fragt die App beim nächsten Klick wieder nach.
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