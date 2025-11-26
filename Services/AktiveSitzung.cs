using WPF_Test.Models;

namespace WPF_Test.Services
{
    /// <summary>
    /// Verwaltet den globalen Status der aktuellen Benutzersitzung.
    /// <para>
    /// ARCHITEKTUR: Singleton-Pattern.
    /// Diese Klasse stellt sicher, dass der angemeldete Benutzer (Teilnehmer) 
    /// von überall in der Anwendung abgerufen werden kann.
    /// </para>
    /// </summary>
    public class AktiveSitzung
    {
        // 1. Das private Feld zur Speicherung der einzigen Instanz.
        // "static" bedeutet: Diese Variable gehört zur Klasse selbst, nicht zu einem Objekt.
        private static AktiveSitzung _instance;

        // 2. Der private Konstruktor.
        // Verhindert, dass von außen 'new AktiveSitzung()' aufgerufen werden kann.
        private AktiveSitzung()
        {
            // Initialisierung: Zu Beginn ist kein Benutzer angemeldet.
            AngemeldeterTeilnehmer = null;
        }

        /// <summary>
        /// Öffentlicher Zugriffspunkt auf die einzige Instanz der Klasse (Die "Schleuse").
        /// </summary>
        public static AktiveSitzung Instance
        {
            get
            {
                // Lazy Initialization: Die Instanz wird erst beim allerersten Zugriff erstellt.
                if (_instance == null)
                {
                    _instance = new AktiveSitzung();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Speichert das Datenmodell des aktuell angemeldeten Benutzers.
        /// Der Setter ist 'private', damit der Status nur kontrolliert geändert werden kann.
        /// </summary>
        public Teilnehmer AngemeldeterTeilnehmer { get; private set; }

        /// <summary>
        /// Setzt den Status auf "Angemeldet".
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