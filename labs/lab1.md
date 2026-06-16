> Nawigacja: **Lab 1** · [Lab 2 →](lab2.md) · [Lab 3 →](lab3.md) · [↑ README](../README.md)

# Lab 1 — `AGENTS.md`: kontekst projektu dla agenta

**Cel:** Zanim poprosisz asystenta AI o przepisanie czegokolwiek, dajesz mu
**pisemny kontekst projektu**. W tym labie tworzysz plik `AGENTS.md` w korzeniu
repozytorium. To trwały brief, który agent (i kolejny człowiek) czyta na
starcie: czym jest projekt, jaki jest stack, jak go uruchomić, jakie są
konwencje, czego **nie wolno** robić i o co **pytać przed** działaniem.

> `AGENTS.md` to *produkt tego labu* — w seedzie go nie ma. Tworzysz go sam.

**Czas:** ~30–40 min.

---

## Co masz na wejściu

- `legacy/` — działający moduł C#/.NET (NAV per unit + dzienna opłata).
- `db/` — schemat funduszy (SQL Server) + dane syntetyczne.
- `golden/` — testy charakteryzujące (oczekiwane wyjścia legacy).

Zacznij od rozejrzenia się: przeczytaj `README.md`, zajrzyj do `legacy/` i
`db/schema.sql`. Nie musisz jeszcze rozumieć całej logiki — dziś opisujesz
*kontekst*, nie algorytm.

---

## Zadanie

Utwórz `AGENTS.md` w korzeniu repo z **siedmioma sekcjami** poniżej. Każda
sekcja ma być krótka i konkretna — to brief, nie dokumentacja.

### 1. Projekt
Jedno-dwa zdania: co to za moduł i po co istnieje. Po co przepisujemy go na
Javę (cel: zachowanie zachowania — *behaviour-preserving* — na nowym stacku).

### 2. Stack
Stan obecny i docelowy. **Dołącz mapę stacku stary → nowy** (to klucz tego
labu):

| Warstwa | Stary (legacy) | Nowy (docelowy) |
|---|---|---|
| Język / runtime | C# / .NET 8 | Java 21 |
| Framework aplikacji | konsola .NET | Spring Boot 3 |
| Dostęp do danych | ADO.NET (`Microsoft.Data.SqlClient`) | Spring `JdbcTemplate` (JDBC) |
| Zapytania | SQL budowany przez konkatenację stringów | SQL **parametryzowany** (`?`) |
| Konfiguracja | `appsettings.json` | `application.properties` + zmienne środowiskowe |
| Build | `dotnet` / `.csproj` | Maven / `pom.xml` |
| Testy | (brak — dokładamy golden) | JUnit 5 |
| Baza | SQL Server | SQL Server (bez zmian) |

### 3. Setup
Jak postawić środowisko i uruchomić moduł oraz testy charakteryzujące:
```bash
docker compose -f db/docker-compose.yml up -d   # baza + seed
./golden/run_legacy.sh                           # uruchom legacy i porównaj z golden
```
Dopisz wymagania (Docker, .NET SDK 8, później JDK 21 + Maven).

### 4. Konwencje
- Kod i komentarze po **angielsku**; materiały warsztatowe po polsku.
- Nazewnictwo encji domeny jest **kanoniczne** — nie zmieniaj nazw
  (`Fund`, `ShareClass`, `Subscription`, `Redemption`, `Valuation`, …).
- Pieniądze i jednostki liczymy na typach dziesiętnych (`decimal`/`BigDecimal`),
  **nigdy** na `double`/`float`.
- Nowy kod: logika oddzielona od dostępu do danych (nie powtarzamy „wymieszania”
  z legacy).

### 5. Bezpieczeństwo
Reguły, których agent ma przestrzegać (i które będą sprawdzane w labie 3):
- **Nie przenoś** konkatenacji SQL — używaj zapytań parametryzowanych.
- **Nie przenoś** słabego haszowania (MD5) — jeśli hash jest potrzebny, użyj
  współczesnego (np. SHA-256).
- **Sekrety** (hasła, connection stringi) w konfiguracji / zmiennych
  środowiskowych, **nigdy** w kodzie ani w repo.
- Sprawdzaj **licencje** dodawanych zależności.

### 6. Zapytaj najpierw (przed działaniem)
Wymień rzeczy, przy których agent ma się **zatrzymać i zapytać**, zamiast
zgadywać. Sugestie:
- zmiana **zachowania** widocznego w testach golden (np. inny sposób
  zaokrąglania),
- zmiana **schematu bazy** lub nazw kolumn/encji,
- dodanie nowej zależności zewnętrznej,
- cokolwiek, co wygląda jak **kwirk** (zanim go „naprawisz”, ustal, czy to
  reguła biznesowa, czy błąd — patrz lab 2 i 3).

### 7. Wiedza domenowa
Minimalny słownik, żeby agent rozumiał o czym mowa:
- jednostki w obrocie = Σ `Subscription.Units` − Σ `Redemption.Units` do daty,
- `NavPerUnit` = `NetAssetValue / jednostki w obrocie`,
- dzienna opłata za zarządzanie ≈ `NAV × ManagementFeeBps / 10000 / 365`
  (uwaga: w legacy jest pewien kwirk — rozkminisz go w labie 2).

---

## Definicja ukończenia (DoD)

- [ ] `AGENTS.md` istnieje w korzeniu repo i ma wszystkie 7 sekcji.
- [ ] Sekcja **Stack** zawiera tabelę mapy stary → nowy.
- [ ] Sekcja **Bezpieczeństwo** wymienia: konkatenacja SQL, słabe haszowanie,
      sekrety w konfiguracji, licencje.
- [ ] Sekcja **Zapytaj najpierw** chroni zachowanie z golden i schemat bazy.
- [ ] Tekst jest zwięzły (cały plik mieści się na ~1 ekranie–dwóch).

---

## W Twoim narzędziu

**GitHub Copilot**
- Utwórz `AGENTS.md` (Copilot czyta też `.github/copilot-instructions.md` —
  możesz potraktować je jako alias/skrót do tych samych zasad).
- Zacznij od pustego pliku i poproś Copilot Chat: „Zaproponuj szkielet
  `AGENTS.md` na podstawie `README.md`, `db/schema.sql` i katalogu `legacy/`.
  Trzymaj się sekcji: Projekt, Stack, Setup, Konwencje, Bezpieczeństwo,
  Zapytaj najpierw, Wiedza domenowa.” Potem dopracuj ręcznie.

**Factory.ai (Spec Mode)**
- Dodaj te zasady jako *Droid/Project rules* (kontekst projektu). W Spec Mode
  ten brief jest czytany przy każdej sesji — zadbaj, by mapa stacku i reguły
  bezpieczeństwa były tam wprost.

**Augment (Intent)**
- Zapisz brief jako *Guidelines* / kontekst repozytorium, żeby Augment używał go
  przy generowaniu intencji. Sekcję „Zapytaj najpierw” sformułuj jako twarde
  ograniczenia (constraints), nie sugestie.

> Niezależnie od narzędzia: **przejrzyj wygenerowany tekst zdanie po zdaniu.**
> `AGENTS.md` to Twoja umowa z agentem — odpowiadasz za jego treść.

---

➡️ **Następny:** [Lab 2 — Wyciągnięcie domeny: intencja vs framework](lab2.md)
