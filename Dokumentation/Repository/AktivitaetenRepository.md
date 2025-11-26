# Modul-Dokumentation: Aktivität-Repository

Dieses Modul ist für die Datenbankkommunikation bezüglich der Aktivitäten (Historie) zuständig. Es verwaltet das Laden (`SELECT`), Speichern (`INSERT`), Aktualisieren (`UPDATE`) und Löschen (`DELETE`) von Datensätzen.

---

## 1. Exkurs: Das `using`-Statement (Ressourcen-Management)

Im Code findet sich häufig folgendes Muster:

```csharp
using (MySqlConnection connection = GetConnection())
{
    // ... arbeite mit der Datenbank ...
}
```

### Warum ist das wichtig?
Datenbankverbindungen (`MySqlConnection`), Dateien oder Netzwerk-Streams sind **teure Ressourcen**.
* **Das Problem:** Wenn man eine Verbindung öffnet (`connection.Open()`) und vergisst, sie zu schließen (`connection.Close()`) – z.B. weil ein Fehler/Exception auftritt – bleibt die Verbindung im Hintergrund offen. Irgendwann sind alle verfügbaren Verbindungen zum Server verbraucht, und die Anwendung (oder der ganze Server) stürzt ab.

### Die Lösung: `using`
Das `using`-Statement ist "syntaktischer Zucker" (eine Abkürzung) für einen `try-finally`-Block.

**Was der Compiler daraus macht:**
```csharp
MySqlConnection connection = GetConnection();
try
{
    // Dein Code hier...
}
finally
{
    // Dieser Block wird IMMER ausgeführt, egal ob ein Fehler passiert oder nicht.
    if (connection != null)
        connection.Dispose(); // Schließt die Verbindung sicher.
}
```

**Vorteil:** Wir müssen uns keine Sorgen machen, ob wir `Close()` aufrufen. Sobald die geschweifte Klammer `}` des `using`-Blocks erreicht wird, räumt .NET automatisch auf – garantiert.

---

## 2. SQL-Besonderheiten

### 2.1 Mehrsprachigkeit via JOIN
Um den Status-Text (z.B. "Offen" vs. "Open") korrekt anzuzeigen, reicht ein einfaches Lesen der Tabelle `aktivitaet` nicht aus, da dort nur die ID (z.B. `1`) steht.

Die Lösung ist ein **JOIN** über drei Tabellen:
1.  `aktivitaet` (Verknüpft Firma mit Status-ID)
2.  `status` (Verknüpfungstabelle)
3.  `status_translation` (Enthält den Text in verschiedenen Sprachen)

Der Filter `WHERE st.LanguageCode = @Sprache` sorgt dafür, dass nur die deutsche (oder englische) Übersetzung geladen wird.

### 2.2 Parameter gegen SQL-Injection
In allen Methoden (`Add`, `Update`, `Delete`, `Get`) werden **Parameter** (`@FirmaID`, `@Datum`) verwendet.
Niemals werden Variablen direkt in den String geschrieben (`"SELECT ... ID=" + id`).
Dies verhindert **SQL-Injection**-Angriffe, bei denen böswilliger Code über Eingabefelder eingeschleust werden könnte.

---

## 3. NULL-Handling bei Datenbank-Werten

Datenbankfelder können `NULL` sein (z.B. kein Kommentar vorhanden). C#-Strings sind standardmäßig aber nicht kompatibel mit `DBNull`.

Der Code verwendet daher einen sicheren Cast:
```csharp
Kommentar = reader["Kommentar"] as string
```
* Wenn in der DB ein Text steht, wird er in einen C#-String umgewandelt.
* Wenn in der DB `NULL` steht, liefert der `as`-Operator `null` zurück, ohne dass das Programm abstürzt.