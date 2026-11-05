# BilCirkeln – Vi köper din bil

Examensprojekt (fullstack .NET, 4 veckor, individuellt).

BilCirkeln är en webbapplikation där privatpersoner kan sälja sin bil direkt till en bilhandlare.
Kunden fyller i uppgifter om bilen, personalen lämnar ett bud och kunden accepterar eller nekar budet.

## Problem som lösningen utgår från
Att sälja bil privat tar tid. Man måste skriva annonser, svara på frågor och ta emot provkörningar.
För bilhandlaren kommer förfrågningar ofta in via telefon och mejl, och då är det svårt att hålla ordning på dem.
BilCirkeln samlar hela flödet på ett ställe: förfrågan, bud och svar.

## Funktioner
**Kund**
- Skapa konto och logga in
- Skicka in en bil i ett formulär som visas steg för steg, med en mätare som visar hur mycket som är ifyllt:
  regnr, miltal, märke, modell, år, drivmedel, växellåda, önskat pris, beskrivning, samt
  nycklar, extra däck, servicebok och servicehistorik, besiktning, säljartyp, import, finansiering och klädsel
- Se sina inskickade bilar och deras status
- Acceptera eller neka ett bud

**Personal**
- Se alla förfrågningar och filtrera på status
- Lämna bud med meddelande (ett nytt bud kan lämnas om kunden nekade)
- Avböja en bil ("Inte intresserad")

**Statusflöde:** `Inskickad → Bud lämnat → Accepterad` eller `→ Bud nekat → (nytt bud) → Bud lämnat …`.
Personalen kan avböja en förfrågan som är *Inskickad* eller *Bud nekat*.

## Teknik
| Del | Teknik |
|---|---|
| Backend | ASP.NET Core Web API, C#, .NET 8 |
| Databas | SQL Server (LocalDB) + Entity Framework Core (code first, migrations) |
| Inloggning | ASP.NET Core Identity + JWT, roller `Kund` och `Personal` |
| Frontend | React (Vite), React Router, Axios |
| Tester | xUnit + EF Core InMemory |

## Mappstruktur
```
BilCirkeln/
├── src/BilCirkeln.Api/       backend (Controllers, Services, Models, DTOs, Data)
├── src/bilcirkeln-web/       frontend (React)
├── tests/BilCirkeln.Tests/   xUnit-tester
├── docs/                     projektplan, rapport, dagbok, beslutslogg, kodgenomgång
└── README.md
```

## Installera och starta

### Krav
- .NET 8 SDK (eller nyare SDK med .NET 8-runtime)
- SQL Server LocalDB (ingår i Visual Studio) eller annan SQL Server
- Node.js 20+
- `dotnet-ef` (`dotnet tool install --global dotnet-ef`) om du vill skapa nya migrationer

### 1. Hemligheter (user secrets)
Inga lösenord, nycklar eller connection strings finns i koden. De sätts lokalt med user secrets:

```bash
cd src/BilCirkeln.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\MSSQLLocalDB;Database=BilCirkelnDb;Trusted_Connection=True;TrustServerCertificate=True"
dotnet user-secrets set "Jwt:Key" "<en slumpad sträng på minst 32 tecken>"
dotnet user-secrets set "SeedStaff:Email" "personal@bilcirkeln.se"
dotnet user-secrets set "SeedStaff:Password" "<lösenord, minst 8 tecken, stor+liten bokstav och siffra>"
```

### 2. Starta backend
```bash
cd src/BilCirkeln.Api
dotnet run --launch-profile http
```
- Databasen skapas automatiskt vid start, eftersom migrationerna körs då.
- Rollerna `Kund` och `Personal` och personalkontot från user secrets skapas automatiskt.
- Swagger: http://localhost:5221/swagger

### 3. Starta frontend
```bash
cd src/bilcirkeln-web
npm install
npm run dev
```
Öppna http://localhost:5173

### 4. Kör testerna
```bash
dotnet test
```

## Användning
1. Gå till *Skapa konto* och registrera en kund.
2. Klicka på *Sälj din bil* och fyll i formuläret.
3. Logga ut och logga in med personalkontot. Öppna förfrågan och lämna ett bud.
4. Logga in som kunden igen. Öppna bilen under *Mina bilar* och acceptera eller neka budet.

## API-endpoints
| Metod | URL | Roll | Beskrivning |
|---|---|---|---|
| POST | `/api/auth/register` | – | Skapa kundkonto, returnerar JWT |
| POST | `/api/auth/login` | – | Logga in, returnerar JWT |
| POST | `/api/submissions` | Kund | Skicka in bil |
| GET | `/api/submissions/mine` | Kund | Mina bilar |
| GET | `/api/submissions?status=Pending` | Personal | Alla förfrågningar (filter valfritt) |
| GET | `/api/submissions/{id}` | Kund (egen) / Personal | En förfrågan med bud |
| POST | `/api/submissions/{id}/offers` | Personal | Lämna bud |
| POST | `/api/submissions/{id}/respond` | Kund | `{ "accept": true/false }` |
| POST | `/api/submissions/{id}/reject` | Personal | Avböj bilen |

## Dokumentation
- [Projektplan](docs/projektplan.md)
- [Projektrapport](docs/rapport.md)
- [Projektdagbok](docs/projektdagbok.md)
- [Beslutslogg](docs/beslutslogg.md)
- [Kodgenomgång och kontrollfrågor](docs/kodgenomgang.md)

## Användning av AI
AI (Claude Code) har använts för att ta fram kod, tester och dokumentation.
All kod har gåtts igenom, testats manuellt och med automatiska tester.
Se avsnittet *Användning av AI* i [rapporten](docs/rapport.md) och [projektdagboken](docs/projektdagbok.md).
