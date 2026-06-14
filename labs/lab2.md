# Lab 2 — Wyciągnięcie domeny: intencja vs framework

**Cel:** Zrozumieć, **co** moduł legacy naprawdę robi (reguły domeny), oddzielając
to od **jak** to robi (ADO.NET, konkatenacja SQL, konsola). Efektem jest
**mapa pojęć stare → nowe** — Twój przewodnik przy przepisywaniu w labie 3.

> Mapa pojęć to *produkt tego labu* — w seedzie jej nie ma. Tworzysz ją sam.

**Czas:** ~40–50 min.

---

## Dlaczego to robimy

Przepisanie behaviour-preserving udaje się tylko wtedy, gdy umiesz oddzielić:

- **Intencję domenową** — reguły, które muszą przetrwać zmianę technologii
  (jak liczymy jednostki w obrocie, NAV per unit, opłatę).
- **Przypadkowość frameworka** — to, co wynika tylko z tego, że kod jest w
  .NET/ADO.NET (sposób otwierania połączeń, budowanie SQL, format CSV).

Jeśli przepiszesz framework 1:1, przeniesiesz też jego **kwirki** — w tym te,
które są błędami. Dlatego najpierw nazywamy intencje.

---

## Zadanie A — przeczytaj legacy i wypisz reguły

Przejrzyj `legacy/NavService.cs` i `legacy/FeeCalculator.cs`. Dla każdego
fragmentu logiki zapisz **jedno zdanie intencji** (bez odwołań do .NET):

- Jak liczone są **jednostki w obrocie**?
- Jak liczony jest **NavPerUnit**? Jak jest zaokrąglany?
- Jak liczona jest **dzienna opłata za zarządzanie**? Jaki day-count? Jakie
  zaokrąglenie?
- Co jest **dostępem do danych / frameworkiem**, a nie regułą domeny?

## Zadanie B — sklasyfikuj kwirki

W kodzie są zachowania, które odbiegają od „podręcznikowej” reguły. Dla
każdego rozstrzygnij: **reguła biznesowa (zachować)** czy **błąd (poprawić)**?
Uzasadnij.

Wskazówki, gdzie patrzeć:
- zaokrąglenie naliczenia opłaty (w którą stronę? do ilu miejsc? dlaczego?),
- day-count w opłacie (czy zgadza się z regułą actual/365 z `README`?),
- zaokrąglenie `NavPerUnit` (tryb zaokrąglania ma znaczenie przy przenoszeniu
  na inny język!).

> Nie zgaduj w pojedynkę: to jest dokładnie przypadek z sekcji **„Zapytaj
> najpierw”** w Twoim `AGENTS.md`. Jeśli nie masz pewności, czy coś jest regułą
> czy błędem — oznacz to i ustal z prowadzącym/„biznesem”.

## Zadanie C — napisz mapę pojęć stare → nowe

Utwórz plik (np. `concept-map.md`) z tabelą tłumaczącą pojęcia legacy na
docelowy projekt Java. Zaproponowany szkielet:

| Pojęcie / element | Legacy (C#/.NET) | Nowe (Java/Spring) | Uwagi / intencja |
|---|---|---|---|
| Jednostki w obrocie | `NavService.GetUnitsInIssue` (SQL przez konkatenację) | repozytorium + zapytanie **parametryzowane** | Σ subs − Σ reds do daty; reguła do zachowania |
| NAV per unit | `FeeCalculator.NavPerUnit` (`Math.Round`, AwayFromZero, 6 miejsc) | `NavCalculator` (`BigDecimal`, `HALF_UP`, scale 6) | tryb zaokrąglenia musi się zgadzać! |
| Dzienna opłata | `FeeCalculator.DailyManagementFee` | `FeeCalculator` (Java) | day-count + floor — patrz klasyfikacja kwirków |
| Zaokrąglenie opłaty | `Math.Floor(raw*100)/100` | `setScale(2, FLOOR)` | **reguła** — zachować |
| Day-count opłaty | `/360` | `/365` | **błąd** — poprawić (zmienia golden) |
| Encje domeny | `ShareClassRow`, `ValuationRow` (anemiczne) | rekordy/POJO domenowe | nazwy kanoniczne bez zmian |
| Dostęp do danych | ADO.NET `SqlConnection`/`SqlCommand` | `JdbcTemplate` | framework — przepisać, nie kopiować wzorca |
| Konfiguracja | `appsettings.json` (hasło w repo) | `application.properties` + env | sekret poza kodem |
| Hash audytowy | `LegacyHash` (MD5) | SHA-256 (lub usunąć) | **nie przenosić** słabego haszowania |

Dodaj wiersze dla pozostałych encji ze schematu (`Fund`, `Investor`,
`Instrument`, `PriceEOD`, `Holding`, `FeeAccrual`), nawet jeśli moduł NAV/opłat
z nich nie korzysta — to część kontekstu domeny.

---

## Definicja ukończenia (DoD)

- [ ] Lista reguł domeny (intencji) opisana **bez** odwołań do .NET.
- [ ] Oba kwirki zidentyfikowane i sklasyfikowane (reguła vs błąd) z
      uzasadnieniem.
- [ ] `concept-map.md` zawiera mapę stare → nowe dla logiki, dostępu do danych,
      konfiguracji i haszowania.
- [ ] Decyzje „zachować / poprawić” są jawne — wejdą do specyfikacji w labie 3.

---

## W Twoim narzędziu

**GitHub Copilot**
- Zaznacz `FeeCalculator.cs` i zapytaj w Copilot Chat: „Wyjaśnij intencję
  biznesową tej metody w jednym zdaniu, bez odwołań do API .NET. Wskaż liczby
  magiczne i nietypowe zaokrąglenia.” Użyj `@workspace`, by Copilot widział
  `README` i `golden/`.
- Poproś o draft tabeli mapy pojęć — potem zweryfikuj każdy wiersz ręcznie.

**Factory.ai (Spec Mode)**
- Użyj trybu eksploracji/wyjaśniania, by streścić zachowanie modułu. Mapę pojęć
  zapisz jako artefakt wejściowy do późniejszego `spec.md` (lab 3).

**Augment (Intent)**
- Poproś o „intent summary” modułu i listę założeń. Każde założenie skonfrontuj
  z `golden/expected_fees.csv` — czy liczby potwierdzają regułę, którą opisałeś?

> Klasyfikacja kwirków (reguła vs błąd) to **decyzja człowieka**. Narzędzie
> pomoże ją nazwać, ale nie powinno jej podejmować za Ciebie.
