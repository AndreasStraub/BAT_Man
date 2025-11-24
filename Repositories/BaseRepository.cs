using MySql.Data.MySqlClient;


namespace WPF_Test.Repositories // (Stell sicher, dass dies dein Namespace ist)
{
    /// <summary>
    /// Eine abstrakte Basisklasse für alle Repositorys.
    /// Sie verwaltet den Connection-String und stellt eine Methode
    /// zur Verfügung, um eine geöffnete Datenbankverbindung zu erhalten.
    /// </summary>
    public abstract class BaseRepository
    {
        // Der Connection-String ist 'readonly' und 'private',
        // er wird nur einmal im Konstruktor gesetzt.
        private readonly string _connectionString;

        /// <summary>
        /// Konstruktor: Wird von den erbenden Klassen (z.B. FirmaRepository)
        /// automatisch aufgerufen.
        /// </summary>
        protected BaseRepository()
        {
            // Dein Connection-String an EINER zentralen Stelle:
            _connectionString = "Server=localhost;Database=bat_man;User ID=root;Password=;";
        }

        /// <summary>
        /// Stellt eine geöffnete MySql-Verbindung bereit.
        /// 'protected' = Nur diese Klasse und ihre Kinder können die Methode sehen.
        /// </summary>
        /// <returns>Eine geöffnete MySqlConnection</returns>
        protected MySqlConnection GetConnection()
        {
            // Erstellt die Verbindung...
            var connection = new MySqlConnection(_connectionString);

            // ...öffnet sie...
            connection.Open();

            // ...und gibt sie zurück.
            return connection;
        }
    }
}