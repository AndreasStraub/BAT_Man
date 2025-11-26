# Architektur-Dokumentation: Das Repository Pattern & Datenzugriff

## 1. Definition und Zweck
Das **Repository Pattern** fungiert als Abstraktionsschicht zwischen der Anwendungslogik (in MVVM: dem ViewModel) und der Datenhaltung (Datenbank, API, Dateisystem). Es zentralisiert den Datenzugriff.

### Vorteile der Architektur
* **Entkopplung:** Das ViewModel benötigt kein Wissen über die Art der Datenquelle (SQL, XML, REST-API). Es konsumiert lediglich C#-Objekte.
* **Wartbarkeit:** Änderungen an der Datenbankstruktur oder der Wechsel der Technologie (z.B. von MySQL zu PostgreSQL oder zu einer Web-API) erfordern lediglich Anpassungen im Repository, nicht in der gesamten Anwendung.
* **Testbarkeit:** Für Unit-Tests können Repositories durch "Mock-Objekte" ersetzt werden, die Testdaten liefern, ohne eine echte Datenbankverbindung aufzubauen.

---

## 2. Technische Umsetzung: BaseRepository
Die Klasse `BaseRepository` dient als Fundament für alle spezifischen Repositories (z.B. `FirmenRepository`).

* **Vermeidung von Redundanz (DRY):** Der Connection-String und die Logik zum Aufbau der Verbindung werden hier zentral verwaltet.
* **Vererbung:** Spezifische Repositories erben von dieser Basisklasse und nutzen die bereitgestellte Verbindungsmethode (`GetConnection`).

---

## 3. Architekturelle Entwicklung: Von 2-Schicht zu 3-Schicht (API)

### Aktueller Status: 2-Schicht-Architektur (Client-Server direkt)
In der aktuellen Implementierung baut die WPF-Anwendung (`Client`) direkt eine Verbindung zum MySQL-Server (`Datenbank`) auf.
* **Vorteil:** Einfache Implementierung, geringe Komplexität für Lehrzwecke.
* **Sicherheitsrisiko:** Der Connection-String inklusive Datenbank-Passwort muss in der kompilierten Anwendung hinterlegt werden. Dies ermöglicht es Dritten, durch Dekompilierung an Zugangsdaten zu gelangen und die Datenbank zu manipulieren.

### Zielbild: 3-Schicht-Architektur (Client - API - Datenbank)
In professionellen Produktionsumgebungen wird eine **Web-API** (z.B. ASP.NET Core) als Zwischenschicht eingeführt.

1.  **Client (WPF):** Sendet HTTP-Anfragen (Requests) an den API-Server (z.B. `GET https://server.com/api/firmen`). Der Client kennt keine Datenbank-Zugangsdaten.
2.  **Application Server (API):** Nimmt die Anfrage entgegen, authentifiziert den Benutzer, verarbeitet die Logik und führt die SQL-Abfrage aus. Das Datenbank-Passwort liegt geschützt auf dem Server.
3.  **Datenbank:** Liefert die Daten an die API zurück, welche diese als JSON an den Client sendet.

---

## 4. Praxisbeispiel: Refactoring von SQL zu API
Dieser Abschnitt verdeutlicht, wie sich der C#-Code im Repository ändert, wenn die Architektur von direkter SQL-Verbindung auf HTTP-Kommunikation umgestellt wird.

### Ausgangslage: Direkter SQL-Zugriff (2-Schicht)
Hier enthält der Client die SQL-Logik (`SELECT * ...`).

```csharp
// Synchrone Methode mit direkter DB-Verbindung
public List<Firma> GetAlleFirmen()
{
    var firmenListe = new List<Firma>();
    int teilnehmerId = GetAktuelleTeilnehmerID();
    
    // SQL-Befehl liegt im Client (Sicherheitsrisiko & starke Kopplung)
    string query = "SELECT * FROM firma WHERE Teilnehmer_ID = @TeilnehmerID";

    try
    {
        using (MySqlConnection connection = GetConnection())
        {
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@TeilnehmerID", teilnehmerId);
                connection.Open(); // Falls nicht im BaseRepository geschehen

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // Manuelles Mapping von DB-Spalten auf Objekteigenschaften
                        Firma firma = new Firma
                        {
                            Firma_ID = reader.GetInt32("Firma_ID"),
                            Firmenname = reader.GetString("Firmenname"),
                            // Sicheres Lesen von NULL-Werten
                            Strasse = reader["Strasse"] as string
                        };
                        firmenListe.Add(firma);
                    }
                }
            }
        }
    }
    catch (Exception ex)
    {
        // Fehlerbehandlung
    }
    return firmenListe;
}
```

### Zielzustand: Zugriff über Web-API (3-Schicht)
Der Client nutzt nun `HttpClient`. Die SQL-Logik entfällt vollständig im Client und verschiebt sich auf den Server.

**Wichtige technische Änderungen:**
1.  **Async/Await:** Netzwerkanfragen sollten immer asynchron durchgeführt werden, um die UI nicht zu blockieren (`Task<List<...>>`).
2.  **HttpClient:** Ersetzt `MySqlConnection`.
3.  **JSON Deserialisierung:** Ersetzt den `MySqlDataReader`. Die Daten kommen als Text (JSON) und werden in Objekte gewandelt.
4.  **Parameter:** Werden Teil der URL oder des Body, statt SQL-Parameter.

```csharp
using System.Net.Http;
using System.Text.Json; // Notwendig für die Umwandlung von JSON zu C#-Objekten
using System.Threading.Tasks;

// Asynchrone Methode für HTTP-Zugriff
public async Task<List<Firma>> GetAlleFirmenAsync()
{
    var firmenListe = new List<Firma>();
    int teilnehmerId = GetAktuelleTeilnehmerID();

    // 1. Die URL definiert das Ziel und die Parameter
    // Das SQL "WHERE Teilnehmer_ID = ..." wird serverseitig durch den Parameter '?id=...' aufgelöst.
    string apiUrl = $"[https://mein-backend-server.com/api/firmen/alle?teilnehmerId=](https://mein-backend-server.com/api/firmen/alle?teilnehmerId=){teilnehmerId}";

    try
    {
        // HttpClient wird idealerweise per Dependency Injection injiziert oder via IHttpClientFactory erstellt.
        using (HttpClient client = new HttpClient()) 
        {
            // Optional: Authentifizierungs-Token mitsenden (Sicherheit)
            // client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "mein-token");

            // 2. HTTP GET Anfrage senden (entspricht dem 'SELECT')
            HttpResponseMessage response = await client.GetAsync(apiUrl);

            // 3. Prüfen, ob der Server '200 OK' geantwortet hat
            if (response.IsSuccessStatusCode)
            {
                // 4. Den JSON-String aus der Antwort lesen
                string jsonResponse = await response.Content.ReadAsStringAsync();

                // 5. JSON in C#-Objekte umwandeln (Deserialisierung)
                // Optionen sorgen dafür, dass Groß-/Kleinschreibung (camelCase vs PascalCase) ignoriert wird.
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                firmenListe = JsonSerializer.Deserialize<List<Firma>>(jsonResponse, options);
            }
            else
            {
                // Logik für Fehler (z.B. 401 Unauthorized, 404 Not Found)
            }
        }
    }
    catch (HttpRequestException httpEx)
    {
        // Netzwerkfehler behandeln (z.B. kein Internet)
        Console.WriteLine($"Netzwerkfehler: {httpEx.Message}");
    }

    return firmenListe;
}
```