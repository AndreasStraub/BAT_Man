// Dateipfad: Services/AuthenticationService.cs

using System;
using System.Linq; // WICHTIG: Für .Any() (Sonst geht IsPasswordStrong nicht)
using WPF_Test.Models;
using WPF_Test.Repositories;

namespace WPF_Test.Services
{
    public class AuthenticationService
    {
        // --- Singleton-Implementierung ---
        public static AuthenticationService Instance { get; } = new AuthenticationService();

        private readonly TeilnehmerRepository _teilnehmerRepository;

        private AuthenticationService()
        {
            _teilnehmerRepository = new TeilnehmerRepository();
        }



        // ==========================================================
        // 1. HILFSMETHODE (Muss INNERHALB der Klasse stehen)
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
        // 1. Die Logik-Methode (Der Orchestrator)
        // ==========================================================
        public Teilnehmer Login(string rehaNummer, string passwort)
        {
            // SCHRITT A: Externe Prüfung (Die "Blackbox" der anderen Gruppe)
            // Wir fragen: "Ist die Anmeldung gültig?"
            bool istLoginGueltig = PruefeLoginExtern(rehaNummer, passwort);

            if (istLoginGueltig)
            {
                // SCHRITT B: Wenn JA -> Lade UNSERE Daten
                // Wir nutzen die RehaNummer als Schlüssel, um den Datensatz aus unserer DB zu holen.
                Teilnehmer user = _teilnehmerRepository.GetTeilnehmerByRehaNummer(rehaNummer);

                // Falls der User extern "Ja" sagt, aber bei uns in der DB fehlt:
                if (user == null)
                {
                    // Optional: Fehler werfen oder null zurückgeben
                    // System.Windows.MessageBox.Show("User extern bestätigt, aber lokal nicht gefunden!");
                }

                return user;
            }

            // Wenn Login ungültig -> null zurückgeben
            return null;
        }

        // ==========================================================
        // 2. Die Flexibilitäts-Methode (Platzhalter für die PHP-Gruppe)
        // ==========================================================
        /// <summary>
        /// Diese Methode simuliert die Anfrage an das externe System der anderen Gruppe.
        /// </summary>
        private bool PruefeLoginExtern(string rehaNummer, string passwort)
        {
            // <!*-- 
            // PLATZHALTER LOGIK:
            // Aktuell simulieren wir einfach: "Wenn etwas eingegeben wurde, ist es okay".
            // 
            // SPÄTER: Hier kommt der Code der anderen Gruppe rein.
            // Zum Beispiel ein HTTP Request an deren API:
            // var response = await httpClient.PostAsync("https://php-server/login", ...);
            // return response.IsSuccessStatusCode;
            // --*!>

            if (!string.IsNullOrEmpty(rehaNummer) && !string.IsNullOrEmpty(passwort))
            {
                // Simulation: Login immer erfolgreich, wenn Felder nicht leer sind.
                return true;
            }

            return false;
        }

        // ==========================================================
        // 3. CHANGE PASSWORD (Nutzt die Hilfsmethode)
        // ==========================================================
        public bool ChangePassword(int teilnehmerId, string neuesPasswort)
        {
            // Prüfung aufrufen
            if (!IsPasswordStrong(neuesPasswort))
            {
                throw new ArgumentException("Das Passwort muss mind. 8 Zeichen, Groß-/Kleinbuchstaben, Ziffern und Sonderzeichen enthalten.");
            }

            // Simuliertes Speichern
            return true;
        }
    }
}