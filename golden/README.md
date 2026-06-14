# Testy charakteryzujące (golden)

Te pliki opisują **dokładne zachowanie modułu legacy** dla ustalonych danych
wejściowych z `db/seed.sql`. To jest *charakterystyka* (ang. *characterization /
golden master test*): nie sprawdzamy, czy wynik jest „poprawny” w sensie
prospektu — sprawdzamy, czy **nie zmienił się** względem działającego systemu.

To jest siatka bezpieczeństwa dla przepisywania **behaviour-preserving**:
najpierw przykrywasz moduł testami, które oddają jego obecne zachowanie
(łącznie z kwirkami), a potem przepisujesz kod tak, żeby testy pozostały
zielone.

## Pliki

| Plik | Co opisuje |
|------|------------|
| `expected_nav.csv`  | `NavPerUnit` = `NetAssetValue / UnitsInIssue` (zaokr. do 6 miejsc) dla każdej (klasa, data). |
| `expected_fees.csv` | Dzienna opłata za zarządzanie **z kwirkami legacy**: day-count `/360` oraz floor do 2 miejsc. |
| `run_legacy.sh`     | Uruchamia moduł legacy i porównuje jego wyjście z plikami golden. |

Kolumny `nav`: `ShareClassId, AsOfDate, UnitsInIssue, NetAssetValue, NavPerUnit`.
Kolumny `fees`: `ShareClassId, AsOfDate, NetAssetValue, ManagementFeeBps, DailyManagementFee`.

Wiersze są posortowane po `ShareClassId`, potem `AsOfDate` — tak samo sortuje
moduł legacy, więc porównanie jest porównaniem bajt-w-bajt.

## Jak uruchomić

```bash
# 1. Postaw i zasiej bazę (synthetic data)
docker compose -f db/docker-compose.yml up -d

# 2. Uruchom moduł legacy i porównaj z golden
./golden/run_legacy.sh
```

Wynik `OK` oznacza, że zachowanie modułu jest zgodne z baseline.

## Uwaga o kwirkach (przeczytaj przed labem 3)

`expected_fees.csv` celowo zawiera **dwa** efekty obecne w kodzie legacy:

1. **Zaokrąglenie w dół do 2 miejsc (floor)** — to *prawdziwa reguła biznesowa*.
   Procesy uzgodnień i raportowania zależą od tego, że opłata nigdy nie jest
   zaokrąglana w górę. **To zachowanie zachowujemy** w nowym systemie.
2. **Day-count `/360` zamiast `/365`** — to *błąd*. Prospekt mówi actual/365.
   W labie 3 ten błąd **naprawiamy**, więc wartości opłat się zmienią. To jest
   świadoma, udokumentowana decyzja: aktualizujesz oczekiwane wartości testu
   (nie „naginasz” testu, tylko zmieniasz specyfikację zachowania).

Mapę „co zachować, a co poprawić” rozwijasz w labie 2 i 3.
