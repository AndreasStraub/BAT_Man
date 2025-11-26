# Modul-Dokumentation: Firma-Repository

Dieses Modul verwaltet die Datenbankzugriffe für Firmen-Datensätze. Es enthält spezielle Logik für den sicheren Umgang mit `NULL`-Werten und die Verwendung von Transaktionen bei Löschvorgängen.

---

## 1. Exkurs: Umgang mit NULL-Werten

In relationalen Datenbanken können Felder den Wert `NULL` annehmen (z.B. wenn keine Telefonnummer eingetragen wurde). In C# ist der Datentyp `string` zwar ein Referenztyp (und kann `null` sein), der `MySqlDataReader` gibt für leere Datenbankfelder jedoch ein spezielles Objekt namens `DBNull.Value` zurück.

### Das Problem
Eine direkte Umwandlung (Cast) oder der Aufruf von typisierten Getter-Methoden führt bei `DBNull` zu einem Programmabsturz (Exception):

```csharp
// Führt zu einer InvalidCastException, wenn das Feld in der DB NULL ist:
string telefon = reader.GetString("Telefon"); 
string telefon = (string)reader["Telefon"];
```

### Die Lösung: Der `as`-Operator
Der `as`-Operator führt eine sichere Typumwandlung durch. Ist das Objekt nicht kompatibel mit dem Zieltyp (wie `DBNull` zu `string`), wird kein Fehler geworfen, sondern das Ergebnis ist `null`.

```csharp
// Sicherer Zugriff:
string telefon = reader["Telefon"] as string;
```

---

## 2. SQL-Deep-Dive: `ROW_NUMBER()`

Für die Firmenübersicht soll pro Firma lediglich der **aktuellste** Status angezeigt werden. Ein einfacher `JOIN` würde alle Aktivitäten zurückgeben und somit zu Duplikaten in der Firmenliste führen.

**Die Lösung:** Es wird eine *Window Function* in SQL verwendet.

```sql
ROW_NUMBER() OVER(PARTITION BY a.Firma_ID ORDER BY a.Datum DESC) as rn
```

1.  **`PARTITION BY a.Firma_ID`**: Die Tabelle wird virtuell in Gruppen unterteilt (eine Gruppe pro Firma).
2.  **`ORDER BY a.Datum DESC`**: Innerhalb jeder Gruppe werden die Aktivitäten nach Datum sortiert (neueste zuerst).
3.  **`ROW_NUMBER()`**: Die Zeilen werden durchnummeriert (1, 2, 3...).
4.  **Ergebnis:** Die Zeile mit `rn = 1` repräsentiert die aktuellste Aktivität jeder Firma.

---

## 3. Transaktionen (`BeginTransaction`)

Beim Löschen einer Firma müssen zwingend auch alle zugehörigen Aktivitäten entfernt werden, um die referenzielle Integrität zu wahren (Vermeidung von "Datenbank-Leichen").

**Das Risiko:** Würde der Befehl "Lösche Aktivitäten" erfolgreich ausgeführt, der Befehl "Lösche Firma" jedoch fehlschlagen (z.B. durch einen Serverfehler), entstünde ein inkonsistenter Datenbestand.

**Die Lösung:** Eine Transaktion klammert beide Befehle zu einer atomaren Einheit.
* **`Commit()`:** Nur wenn beide Befehle fehlerfrei durchlaufen wurden, werden die Änderungen dauerhaft gespeichert.
* **`Rollback()`:** Tritt ein Fehler auf, wird die Datenbank auf den Zustand *vor* dem ersten Löschbefehl zurückgesetzt.