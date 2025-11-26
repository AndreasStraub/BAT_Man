# Modul-Dokumentation: Aktive Sitzung (Service)

Dieses Modul dient als zentraler Datenspeicher für den aktuell angemeldeten Benutzer. Es muss von überall in der Anwendung (Login, Hauptfenster, Datenbankzugriffe) erreichbar sein.

---

## 1. Architektur: Das Singleton Pattern

Um sicherzustellen, dass es in der gesamten Anwendung nur **einen einzigen** Zustand der Anmeldung gibt, wird das **Singleton-Entwurfsmuster** verwendet.

### Das Problem ohne Singleton
Würde man in jedem ViewModel `new AktiveSitzung()` aufrufen, hätte man viele verschiedene Objekte.
* Im `LoginViewModel` wäre der User angemeldet.
* Im `MainViewModel` wäre eine *andere* Sitzung aktiv (oder leer).
* Daten könnten nicht geteilt werden.

### Die Lösung
Das Singleton garantiert, dass eine Klasse nur genau **einmal** instanziiert wird.

#### Die drei Säulen des Singleton in C#

1.  **Privater Konstruktor:**
    ```csharp
    private AktiveSitzung() { ... }
    ```
    Verhindert, dass andere Klassen `new AktiveSitzung()` schreiben können.

2.  **Private statische Instanz:**
    ```csharp
    private static AktiveSitzung _instance;
    ```
    Eine interne Variable, die das einzige existierende Objekt speichert. `static` bedeutet, sie existiert unabhängig von konkreten Objekten im Speicher.

3.  **Öffentliche statische Eigenschaft (Instance):**
    ```csharp
    public static AktiveSitzung Instance { get { ... } }
    ```
    Dies ist der einzige Weg hinein. Beim ersten Zugriff prüft die Eigenschaft: "Gibt es schon eine Instanz?".
    * Falls **nein**: Erstelle sie jetzt (`new`).
    * Falls **ja**: Gib die bereits vorhandene zurück.

---

## 2. Verwendung in der Praxis

### Zugriff auf Daten
Da die Eigenschaft `Instance` statisch ist, kann von überall darauf zugegriffen werden, ohne Variablen herumreichen zu müssen:

```csharp
// Beispiel: Im MainViewModel den Namen anzeigen
string name = AktiveSitzung.Instance.AngemeldeterTeilnehmer.Nachname;
```

### Sicherheits-Checks
In Repositories wird oft geprüft, ob ein Zugriff erlaubt ist:

```csharp
if (AktiveSitzung.Instance.IstAngemeldet())
{
    // Laden erlauben...
}
```

---

## 3. Ablauf des Lebenszyklus

1.  **Programmstart:** `_instance` ist `null`.
2.  **Login-Prüfung:** Das `LoginViewModel` ruft `AktiveSitzung.Instance.Anmelden(...)` auf.
    * Jetzt wird die `_instance` erzeugt.
    * Das Benutzerobjekt wird gespeichert.
3.  **Laufzeit:** Andere ViewModels greifen auf `AktiveSitzung.Instance` zu und erhalten dasselbe Objekt mit denselben Benutzerdaten.
4.  **Logout:** Durch `Abmelden()` wird die interne Variable auf `null` gesetzt. Die Instanz der Klasse bleibt bestehen, aber sie ist nun "leer".