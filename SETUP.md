# Przygotowanie przed warsztatem (prework)

Przeczytaj i wykonaj to **przed** warsztatem — żeby na miejscu nie tracić czasu
na instalacje. Zajmie ~20–30 min. Nie musisz znać się na finansach ani na
języku C#.

## 1. Co zainstalować

| Narzędzie | Po co | Link |
|---|---|---|
| **Git** | pobranie repozytorium | https://git-scm.com/downloads |
| **Docker Desktop** | uruchomienie bazy danych | https://www.docker.com/products/docker-desktop/ |
| **.NET SDK 8** | uruchomienie starego modułu | https://dotnet.microsoft.com/download/dotnet/8.0 |
| **JDK 21** | przepisanie na Javę (lab 3) | https://adoptium.net/ |
| **Apache Maven** | budowanie projektu Java | https://maven.apache.org/download.cgi |
| **Edytor + asystent AI** | praca z kodem | VS Code + GitHub Copilot, albo Factory.ai, albo Augment |

> Na macOS najłatwiej przez Homebrew:
> `brew install git maven temurin@21 dotnet-sdk@8` oraz Docker Desktop ręcznie.
> Uwaga: musi być `dotnet-sdk@8` — samo `dotnet-sdk` instaluje najnowszą wersję
> SDK, a moduł legacy wymaga .NET 8.
> Na Windows: instalatory z linków powyżej (lub `winget`).

## 2. Sprawdź, że masz wszystko

W terminalu:
```bash
git --version
docker --version
dotnet --version     # powinno pokazać 8.x
java -version        # powinno pokazać 21
mvn -version
```
Każda komenda powinna coś wypisać (a nie „command not found”).

## 3. Pobierz repozytorium

```bash
git clone <ADRES_REPO_OD_PROWADZĄCEGO>
cd fund-nav-rewrite-workshop
```
(Adres poda prowadzący. Jeśli dostałeś plik ZIP — rozpakuj go i wejdź do
folderu.)

## 4. Test, że środowisko działa (ważne!)

```bash
# 1. Postaw bazę i zasil ją danymi
docker compose -f db/docker-compose.yml up -d

# 2. Uruchom stary moduł i porównaj wynik z plikami "golden"
./golden/run_legacy.sh
```
Sukces = na końcu zobaczysz:
```
OK: legacy output matches the characterization baseline.
```
Jeśli tak — jesteś gotowy. Jeśli nie — zgłoś to prowadzącemu **przed**
warsztatem, podając komunikat błędu.

> **Port 1433 zajęty?** (błąd `Bind for 0.0.0.0:1433 failed: port is already
> allocated`) — masz już inny SQL Server na tym porcie. Uruchom bazę warsztatu
> na innym porcie i wskaż go modułowi legacy:
> ```bash
> FUNDNAV_DB_PORT=14330 docker compose -f db/docker-compose.yml up -d
> export FUNDNAV_CONNECTION="Server=localhost,14330;Database=FundNav;User Id=sa;Password=Workshop_Passw0rd!;TrustServerCertificate=True;Encrypt=False"
> ./golden/run_legacy.sh
> ```

> Pierwsze uruchomienie Dockera pobiera obraz SQL Server (kilka minut) — zrób to
> wcześniej, nie na sali.

## 5. Co przeczytać wcześniej (10 min)

- [`README.md`](README.md) — o co chodzi w warsztacie.
- Wstęp do [`labs/lab1.md`](labs/lab1.md) — od czego zaczniemy.

To wszystko. Resztę zrobimy razem na warsztacie.
