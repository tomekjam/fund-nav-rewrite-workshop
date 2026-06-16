# Warsztat: przepisanie modułu na Javę (behaviour-preserving)

Repozytorium **startowe** do warsztatu „przepisanie na Javę”. Dostajesz mały,
**działający** moduł *legacy* w C#/.NET, który liczy wycenę funduszu na
jednostkę (NAV per unit) oraz dzienną opłatę za zarządzanie. Twoim zadaniem
przez trzy laby jest przepisać go na **Java Spring Boot**, **zachowując jego
zachowanie**, opierając się o **testy charakteryzujące**.

> To repo startowe celowo **nie** zawiera: `AGENTS.md`, mapy pojęć stare→nowe
> ani projektu Java. To są *produkty labów* — tworzysz je sam.

## 🚀 Start tutaj

1. **Przygotowanie:** przejdź [`SETUP.md`](SETUP.md) — instalacja narzędzi i test środowiska (zrób przed warsztatem).
2. **Sprawdź, że legacy działa:** uruchom [Szybki start](#szybki-start) — baza + `run_legacy.sh` powinien wypisać `OK`.
3. **Rób laby po kolei** — to są instrukcje krok po kroku (folder `labs/`):
   **[Lab 1](labs/lab1.md) → [Lab 2](labs/lab2.md) → [Lab 3](labs/lab3.md)**.

Każdy lab ma listę **zadań** i **Definicję ukończenia (DoD)** — checklistę „skończone, gdy…”.

---

## Scenariusz

Moduł obsługuje administrację funduszy inwestycyjnych. Działa, jest w produkcji,
ale jest „legacy”: logika wymieszana z dostępem do danych, liczby magiczne,
zaokrąglenia ad hoc i kilka pułapek. Biznes chce go na nowym stacku (Java),
**bez zmiany zachowania** — z jednym wyjątkiem, który odkryjesz po drodze.

Dane są **syntetyczne** (fikcyjne ID): 3 fundusze, 7 klas jednostek,
20 inwestorów, ~90 dni cen i wycen.

## Reguły domeny (kanon)

- **Jednostki w obrocie** = Σ `Subscription.Units` − Σ `Redemption.Units` do daty.
- **NavPerUnit** = `NetAssetValue / jednostki w obrocie`.
- **Dzienna opłata za zarządzanie** = `NAV × ManagementFeeBps / 10000 / 365`
  (actual/365).

Encje (SQL Server): `Fund`, `ShareClass`, `Investor`, `Instrument`, `PriceEOD`,
`Holding`, `Subscription`, `Redemption`, `Valuation` (NAV), `FeeAccrual`.

> W kodzie legacy zachowanie odbiega od kanonu w dwóch miejscach. Czy to reguły
> biznesowe, czy błędy? To rozstrzygasz w labie 2.

## Struktura repo

```
.
├─ legacy/   # działający moduł C#/.NET (NavService + FeeCalculator)
├─ db/       # schemat funduszy (SQL Server) + dane syntetyczne (docker-compose)
├─ golden/   # testy charakteryzujące: dokładne wyjścia legacy + runner
└─ labs/     # instrukcje labów (PL): lab1, lab2, lab3
```

## Wymagania

- **Docker** (SQL Server + zasilenie danymi),
- **.NET SDK 8** (uruchomienie modułu legacy),
- **JDK 21 + Maven** (lab 3 — projekt Java).

> **Przed warsztatem** przejdź [`SETUP.md`](SETUP.md) — instalacja narzędzi i
> test, że środowisko działa (pierwsze pobranie obrazu bazy trwa kilka minut,
> zrób to wcześniej).

## Szybki start

```bash
# 1. Postaw bazę i zasil danymi syntetycznymi
docker compose -f db/docker-compose.yml up -d

# 2. Uruchom moduł legacy i porównaj wynik z baseline (golden)
./golden/run_legacy.sh
```

Powinieneś zobaczyć `OK: legacy output matches the characterization baseline.`

## Tory labów

| Lab | Temat | Produkt |
|---|---|---|
| [lab1](labs/lab1.md) | `AGENTS.md` — kontekst projektu dla agenta + mapa stacku stary→nowy | `AGENTS.md` |
| [lab2](labs/lab2.md) | Wyciągnięcie domeny z legacy; mapa pojęć stare→nowe; intencja vs framework | `concept-map.md` |
| [lab3](labs/lab3.md) | Spec-driven: `spec.md` → plan → zadania → testy → port do Spring Boot; przegląd bezpieczeństwa | projekt Java + zielone testy |

Każdy lab ma sekcję **„W Twoim narzędziu”** (GitHub Copilot / Factory.ai Spec
Mode / Augment Intent).

## Zasady

- **Kod i komentarze po angielsku; materiały warsztatowe po polsku.**
- Nazewnictwo encji domeny jest kanoniczne — nie zmieniaj nazw.
- Pieniądze i jednostki na typach dziesiętnych (`decimal` / `BigDecimal`),
  nigdy na zmiennoprzecinkowych.
- Testy charakteryzujące są wyrocznią zachowania. Zmiana zachowania = świadoma,
  udokumentowana decyzja.

## Uwaga o bezpieczeństwie

Kod legacy **celowo** zawiera anty-wzorce do wyłapania w przeglądzie
bezpieczeństwa (lab 3): konkatenacja SQL (podatność na SQL injection), słabe
haszowanie (MD5) i sekret w konfiguracji w repo. **Nie przenoś ich** do nowego
systemu — to część ćwiczenia.
