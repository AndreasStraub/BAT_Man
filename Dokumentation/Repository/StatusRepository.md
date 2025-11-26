# Modul-Dokumentation: Status-Repository

Dieses Modul ist für das Laden der Status-Optionen ("Anruf", "E-Mail", etc.) zuständig, die in Dropdown-Listen oder Auswahlfeldern benötigt werden.

---

## 1. Architektur: Mehrsprachigkeit in der Datenbank

Eine besondere Herausforderung in mehrsprachigen Anwendungen ist die Speicherung von Stammdaten (Lookup Tables). Es reicht nicht aus, den Text "Anruf" fest in der Datenbank zu speichern, da ein englischer Benutzer "Call" sehen möchte.

### Das Datenmodell
Die Lösung wird durch eine Aufteilung in zwei Tabellen realisiert:

1.  **Tabelle `status`:** Enthält nur die technischen Daten (Primärschlüssel `Status_ID`, evtl. Sortierreihenfolge oder Farbcodes). Sie ist sprachunabhängig.
2.  **Tabelle `status_translation`:** Enthält die Texte.
    * `Status_ID`: Fremdschlüssel zur Basistabelle.
    * `LanguageCode`: Der Sprachcode (z.B. "de", "en").
    * `Bezeichnung`: Der eigentliche Text.

### Die Abfrage (JOIN)
Um die korrekte Liste zu erhalten, werden diese Tabellen zur Laufzeit verknüpft:

```sql
SELECT s.Status_ID, st.Bezeichnung 
FROM status s
JOIN status_translation st ON s.Status_ID = st.Status_ID
WHERE st.LanguageCode = @Sprache
```

Der Parameter `@Sprache` wird zur Laufzeit aus dem `LanguageService` befüllt. Dadurch erhält die Anwendung immer nur die Datensätze, die zur aktuell eingestellten Sprache passen.

---

## 2. Technische Umsetzung (C#)

### 2.1 Parameter-Binding
Anstatt den Sprachcode direkt in den SQL-String zu integrieren, wird `command.Parameters.AddWithValue` verwendet.
* **Sicherheit:** Verhindert SQL-Injection.
* **Performance:** Die Datenbank kann den Ausführungsplan cachen.

### 2.2 Ressourcen-Management
Der Zugriff auf die Datenbank erfolgt innerhalb von `using`-Blöcken. Dies garantiert, dass die Verbindung (`MySqlConnection`) und der Befehl (`MySqlCommand`) sofort nach der Ausführung geschlossen und freigegeben werden, selbst wenn ein Fehler auftritt.