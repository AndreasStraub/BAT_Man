using WPF_Test.Models;

namespace WPF_Test.Services
{
    /// <summary>
    /// Verwaltet den globalen Status der aktuellen Benutzersitzung (Singleton-Pattern).
    /// Diese Klasse stellt sicher, dass der angemeldeten Benutzer anwendungsweit verfügbar ist.
    /// -> (Siehe Dokumentation: 02_Login.md > 3. Singleton Pattern)
    /// </summary>
    public class AktiveSitzung
    {
        // Das private Feld zur Speicherung der einzigen Instanz.
        // Wurde im vorherigen Code vermisst (Fehler CS0103).
        private static AktiveSitzung _instance;

        /// <summary>
        /// Öffentlicher Zugriffspunkt auf die einzige Instanz der Klasse.
        /// Implementiert das Singleton-Pattern mit "Lazy Initialization" (wird erst bei Bedarf erstellt, 
        /// hier vereinfacht durch Property-Getter logik).
        /// </summary>
        public static AktiveSitzung Instance
        {
            get
            {
                // Wenn noch keine Instanz existiert, wird sie hier erstellt.
                if (_instance == null)
                {
                    _instance = new AktiveSitzung();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Speichert das Datenmodell des aktuell angemeldeten Benutzers.
        /// Der Setter ist 'private', damit der Status nur über die Methoden Anmelden/Abmelden geändert werden kann.
        /// </summary>
        public Teilnehmer AngemeldeterTeilnehmer { get; private set; }

        /// <summary>
        /// Privater Konstruktor.
        /// Verhindert, dass von außen neue Instanzen mit 'new AktiveSitzung()' erstellt werden.
        /// Dies ist die Kernmechanik des Singleton-Patterns.
        /// </summary>
        private AktiveSitzung()
        {
            // Initialisierung: Zu Beginn ist kein Benutzer angemeldet.
            AngemeldeterTeilnehmer = null;
        }

        /// <summary>
        /// Setzt den Status auf "Angemeldet".
        /// Wird vom LoginViewModel nach erfolgreicher Prüfung aufgerufen.
        /// </summary>
        /// <param name="teilnehmer">Das aus der Datenbank geladene Benutzerobjekt.</param>
        public void Anmelden(Teilnehmer teilnehmer)
        {
            if (teilnehmer != null)
            {
                AngemeldeterTeilnehmer = teilnehmer;
            }
        }

        /// <summary>
        /// Setzt den Status zurück (Logout).
        /// </summary>
        public void Abmelden()
        {
            AngemeldeterTeilnehmer = null;
        }

        /// <summary>
        /// Prüft, ob aktuell eine gültige Sitzung besteht.
        /// </summary>
        /// <returns>True, wenn ein Benutzer angemeldet ist.</returns>
        public bool IstAngemeldet()
        {
            return AngemeldeterTeilnehmer != null;
        }
    }
}