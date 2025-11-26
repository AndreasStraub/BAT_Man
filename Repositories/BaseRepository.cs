using MySql.Data.MySqlClient;
using System.Data;

namespace WPF_Test.Repositories
{
    /// <summary>
    /// Abstrakte Basisklasse für die Implementierung des Repository-Patterns.
    /// 
    /// Diese Klasse kapselt die Konfiguration und Erstellung der Datenbankverbindung.
    /// Sie stellt sicher, dass der Connection-String zentral verwaltet wird (DRY-Prinzip)
    /// und nicht in jedem einzelnen Repository redundant definiert werden muss.
    /// </summary>
    public abstract class BaseRepository
    {
        // Der Connection-String enthält die notwendigen Parameter für den Zugriff auf die MySQL-Datenbank.
        // 'readonly': Das Feld kann nur bei der Initialisierung gesetzt werden, was unbeabsichtigte Änderungen verhindert.
        private readonly string _connectionString;

        /// <summary>
        /// Konstruktor der Basisklasse.
        /// </summary>
        /// <remarks>
        /// SICHERHEITSHINWEIS (Architektur):
        /// In dieser Implementierung (2-Schicht-Architektur) liegt das Passwort im Quellcode vor ("Hardcoded").
        /// In einer Produktionsumgebung stellt dies ein Sicherheitsrisiko dar, da die Anwendung dekompiliert 
        /// und das Passwort ausgelesen werden kann.
        /// 
        /// LÖSUNG (3-Schicht-Architektur):
        /// Einsatz einer zwischengeschalteten Web-API (Middleware). Der Client authentifiziert sich 
        /// gegenüber der API (z.B. via Token), und nur die API kennt die Datenbank-Zugangsdaten.
        /// </remarks>
        protected BaseRepository()
        {
            // Konfiguration der Verbindungsparameter:
            // Server: Hostadresse des Datenbankservers (localhost für lokale Entwicklung).
            // Database: Name der Ziel-Datenbank.
            // User ID / Password: Authentifizierungsdaten für den MySQL-Server.
            //_connectionString = "Server=192.168.9.123;Database=it202407;User ID=batman;Password=;";
            _connectionString = "Server=localhost;Database=bat_man;User ID=root;Password=;";
        }

        /// <summary>
        /// Erstellt und öffnet eine neue Verbindung zur MySQL-Datenbank.
        /// </summary>
        /// <remarks>
        /// TRANSITION ZUR API:
        /// Bei einer Umstellung auf eine REST-API würde diese Methode entfallen.
        /// Statt 'MySqlConnection' würde hier ein 'HttpClient' konfiguriert werden.
        /// Anstelle von SQL-Befehlen würden HTTP-Requests (GET, POST, PUT, DELETE) 
        /// an Endpunkte wie '/api/firmen' gesendet werden.
        /// </remarks>
        /// <returns>Ein geöffnetes <see cref="MySqlConnection"/> Objekt.</returns>
        protected MySqlConnection GetConnection()
        {
            // Instanziierung des Verbindungsobjekts mit dem hinterlegten Connection-String.
            var connection = new MySqlConnection(_connectionString);

            // Explizites Öffnen der Verbindung.
            // Dies erleichtert die Handhabung in den abgeleiteten Klassen, da das Objekt
            // sofort einsatzbereit ist.
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            // Rückgabe der offenen Verbindung.
            // Hinweis zum Ressourcen-Management:
            // Der Aufrufer dieser Methode ist verantwortlich für das Schließen der Verbindung 
            // (idealerweise durch Verwendung eines 'using'-Blocks), um Memory Leaks und 
            // offene Ports zu vermeiden.
            return connection;
        }
    }
}