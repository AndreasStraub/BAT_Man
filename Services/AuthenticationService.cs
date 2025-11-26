// Dateipfad: Services/AuthenticationService.cs

using System;
using System.Linq; // WICHTIG: Für .Any() (Sonst geht IsPasswordStrong nicht)
using WPF_Test.Models;

namespace WPF_Test.Services
{
    public class AuthenticationService
    {
        // --- Singleton-Implementierung ---
        public static AuthenticationService Instance { get; } = new AuthenticationService();
        private AuthenticationService() { }


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
        // 2. LOGIN (Simuliert "Zwang zur Passwortänderung")
        // ==========================================================
        public Teilnehmer Login(string email, string passwort)
        {
            // Test-User erstellen
            Teilnehmer testUser = new Teilnehmer
            {
                TeilnehmerID = 1,
                Vorname = "Andreas",
                Nachname = "Straub",
                Kurs = "IT202407 (Test-Login)",
                EMail = "andreas.straub-fiae202407@bfw-neueslernen.de",

                // HIER: Zwang aktivieren!
                MussPasswortAendern = false
            };

            return testUser;
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