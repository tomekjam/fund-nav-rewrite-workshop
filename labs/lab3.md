# Lab 3 — Spec-driven: przepisanie modułu na Java Spring Boot

**Cel:** Przepisać moduł NAV/opłat na **Java Spring Boot**, zachowując
zachowanie (behaviour-preserving), pracując w stylu **spec-driven**:
`spec.md` → plan → zadania → testy charakteryzujące → implementacja → zielone
testy. Na końcu: **przegląd bezpieczeństwa**, który nie przepuści wzorców legacy.

> Projekt Java to *produkt tego labu* — w seedzie go nie ma. Tworzysz go sam
> (np. w katalogu `java/`).

**Czas:** ~90–120 min.

---

## Wejście

- `AGENTS.md` (lab 1) — kontekst i reguły dla agenta.
- `concept-map.md` (lab 2) — mapa pojęć i klasyfikacja kwirków.
- `golden/expected_nav.csv`, `golden/expected_fees.csv` — baseline zachowania.

---

## Krok 1 — `spec.md` (specyfikacja zachowania)

Napisz krótką specyfikację. Nie opisujesz „nowych ficzerów” — opisujesz
**zachowanie do odtworzenia** oraz **jedną świadomą zmianę** (poprawka błędu).

Szkielet `spec.md`:

```markdown
# Spec: port modułu NAV/opłat na Java Spring Boot

## Cel
Odtworzyć zachowanie modułu legacy (C#/.NET) na Java 21 + Spring Boot 3,
czytając tę samą bazę SQL Server. Wynik liczbowy musi zgadzać się z testami
charakteryzującymi, z jednym udokumentowanym wyjątkiem (poniżej).

## Zachowanie do ZACHOWANIA
- Jednostki w obrocie = Σ Subscription.Units − Σ Redemption.Units do daty.
- NavPerUnit = NetAssetValue / jednostki, zaokrąglone do 6 miejsc, HALF_UP
  (= AwayFromZero w .NET). Wartości muszą zgadzać się z expected_nav.csv.
- Dzienna opłata zaokrąglana W DÓŁ (floor) do 2 miejsc — REGUŁA BIZNESOWA.

## Zmiana ZAMIERZONA (poprawka błędu)
- Day-count opłaty: /365 zamiast legacy /360 (zgodnie z prospektem actual/365).
- Skutek: wartości opłat zmienią się względem expected_fees.csv. Aktualizujemy
  oczekiwane wartości testu opłat (decyzja świadoma, nie obejście testu).

## Czego NIE robimy (zakres)
- Nie zmieniamy schematu bazy ani nazw encji.
- Nie dodajemy nowej funkcjonalności.

## Bezpieczeństwo (twarde wymagania)
- Zapytania parametryzowane (zero konkatenacji SQL).
- Brak słabego haszowania (MD5). Jeśli hash potrzebny — SHA-256.
- Sekrety poza kodem (env / application.properties spoza repo).
- Licencje zależności sprawdzone.
```

## Krok 2 — plan i zadania

Z `spec.md` wyprowadź **plan** i rozbij go na **zadania** (TODO). Przykład:

1. Szkielet projektu Spring Boot (Maven, `pom.xml`, JDK 21).
2. Konfiguracja połączenia (datasource z env, nie z kodu).
3. Warstwa domeny: rekordy `ShareClass`, `Valuation`, wynik `NavResult`.
4. Repozytorium na `JdbcTemplate` z zapytaniami **parametryzowanymi**:
   share classes, valuations, units in issue (Σ subs − Σ reds do daty).
5. `NavCalculator` — NavPerUnit (`BigDecimal`, scale 6, `HALF_UP`).
6. `FeeCalculator` — opłata: `/365` + floor do 2 miejsc (`setScale(2, FLOOR)`).
7. Testy charakteryzujące (JUnit 5) wczytujące golden i porównujące wynik.
8. Przegląd bezpieczeństwa + sprawdzenie licencji.

## Krok 3 — testy charakteryzujące (najpierw siatka)

Zanim napiszesz logikę, postaw testy oparte o golden — to Twoja siatka
bezpieczeństwa:

- **NAV:** wczytaj `golden/expected_nav.csv`; dla każdego wiersza policz
  `NavPerUnit(NetAssetValue, UnitsInIssue)` i porównaj z `NavPerUnit`.
  Te testy muszą być **zielone bez zmian** — NAV nie ma poprawianego kwirku.
- **Opłata (reguła floor):** test sprawdzający, że opłata jest zaokrąglona w
  dół (np. wartość z niezerowymi setnymi nie jest zaokrąglona w górę).
- **Opłata (poprawka /365):** to **nie** są wartości z `expected_fees.csv`
  (tam jest /360). Wygeneruj nowy zestaw oczekiwany dla `/365` (policz z
  `NetAssetValue` i `Bps`) i testuj przeciw niemu. Udokumentuj w `spec.md`,
  że to zamierzona rozbieżność.

> Tip: jeśli chcesz porównać też end-to-end z bazą, użyj profilu testowego z
> tą samą bazą SQL Server (np. Testcontainers). Ale rdzeń charakterystyki da
> się sprawdzić na samej arytmetyce, bez bazy — to szybsze i wystarczające do
> zachowania zachowania.

## Krok 4 — implementacja, aż testy są zielone

Przepisuj wg `concept-map.md`. Pilnuj:
- `BigDecimal` (nigdy `double`) dla pieniędzy i jednostek;
- tryb zaokrąglenia NavPerUnit = `HALF_UP` (odpowiednik .NET AwayFromZero dla
  wartości dodatnich) → inaczej testy NAV się rozjadą;
- floor opłaty = `setScale(2, RoundingMode.FLOOR)`;
- day-count = `365`.

## Krok 5 — przegląd bezpieczeństwa (gate)

Zanim uznasz lab za skończony, przejdź checklistę — **nie przenoś** wzorców
legacy:

- [ ] **SQL injection:** wszystkie zapytania parametryzowane (`?` + argumenty).
      Zero konkatenacji wartości do SQL. (W legacy:
      `NavService.GetUnitsInIssue` / `GetValuations` konkatenują — to anty-wzór.)
- [ ] **Słabe haszowanie:** brak MD5. (W legacy: `LegacyHash` używa MD5.) Jeśli
      hash potrzebny — SHA-256. Hash nie wpływa na liczby NAV/opłat, więc jego
      zmiana nie psuje testów.
- [ ] **Sekrety:** brak haseł/connection stringów w kodzie i repo. (W legacy:
      hasło w `appsettings.json`.) Użyj zmiennych środowiskowych /
      `application.properties` poza repo / sejfu.
- [ ] **Licencje zależności:** przejrzyj licencje dodanych bibliotek (np.
      sterownik JDBC do SQL Server, Spring). Odnotuj, że są zgodne z polityką.
- [ ] **Walidacja wejścia / typy:** daty i identyfikatory jako typowane
      parametry, nie sklejane stringi.

## Definicja ukończenia (DoD)

- [ ] `spec.md` opisuje zachowanie do zachowania **i** poprawkę /360 → /365.
- [ ] Projekt Spring Boot buduje się i uruchamia.
- [ ] Testy NAV zielone przeciw `expected_nav.csv` (bez zmian wartości).
- [ ] Testy opłat zielone przeciw zestawowi `/365` (floor zachowany).
- [ ] Checklista bezpieczeństwa przejdzie: brak konkatenacji SQL, brak MD5,
      sekrety poza kodem, licencje sprawdzone.
- [ ] Rozbieżność opłat (/360 → /365) jest udokumentowana.

---

## W Twoim narzędziu

**GitHub Copilot**
- Copilot ma tryb spec/plan ograniczony — zrób to jawnie: utwórz `spec.md`,
  potem poproś Copilot Chat o **plan i listę zadań** na jego podstawie
  (`@workspace`). Generuj testy **przed** implementacją; przy każdej metodzie
  przypominaj o regule floor i day-count /365. Po wygenerowaniu kodu poproś o
  **self-review** pod kątem checklisty bezpieczeństwa, ale zweryfikuj sam.

**Factory.ai (Spec Mode)**
- To naturalne środowisko dla tego labu: wrzuć `spec.md` jako specyfikację,
  pozwól wygenerować plan i zadania, iteruj. Trzymaj `AGENTS.md` i
  `concept-map.md` jako kontekst, żeby reguły bezpieczeństwa i mapa pojęć były
  egzekwowane. Pilnuj, by Spec Mode **nie** „posprzątał” reguły floor jako
  rzekomego błędu.

**Augment (Intent)**
- Sformułuj intencję: „przepisz moduł zachowując zachowanie, popraw tylko
  day-count /360 → /365”. Użyj kontekstu repo (golden, schemat). Po
  wygenerowaniu poproś o przejście checklisty bezpieczeństwa i o listę dodanych
  zależności z licencjami.

> We wszystkich narzędziach zasada jest ta sama: **testy są wyrocznią**, a
> przegląd bezpieczeństwa to bramka, nie formalność. Agent ma odtwarzać reguły,
> a nie kopiować wzorce, które celowo zostawiliśmy jako pułapki.
