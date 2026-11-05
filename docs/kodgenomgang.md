# Kodgenomgång – förstå hela BilCirkeln

Den här filen är till för att **du** ska kunna förklara och försvara all kod på redovisningen.
Gå igenom den i ordning. Öppna filen som nämns, läs koden och svara på kontrollfrågorna *innan* du tittar på svaret.

---

## Del 0 – Helheten på en minut

```
Webbläsare (React)          API (ASP.NET Core)                         Databas (SQL Server)
──────────────────          ───────────────────────────────────        ────────────────────
SellCarPage.jsx  ──POST──►  CarSubmissionsController.Create()
  (axios + JWT)               │  [Authorize(Roles="Kund")]
                              ▼
                            CarSubmissionService.CreateAsync()  ──EF──►  INSERT CarSubmissions
                              │  affärsregler (dubblett?)
                              ▼
                 ◄──JSON──  CarSubmissionDto
```

**En mening att säga på redovisningen:**
> "Frontenden skickar HTTP-anrop med en JWT-token. Controllern tar emot anropet och skickar det vidare till en service som innehåller affärsreglerna. Servicen pratar med databasen via Entity Framework, och svaret skickas tillbaka som en DTO."

---

## Del 1 – Modeller och databas

**Filer:** [Models/](../src/BilCirkeln.Api/Models/), [Data/AppDbContext.cs](../src/BilCirkeln.Api/Data/AppDbContext.cs)

### Begrepp
- **Modell / entitet:** en C#-klass som blir en tabell i databasen. `CarSubmission` blir tabellen `CarSubmissions`.
- **Code first:** vi skriver C#-klasserna först, och EF Core skapar tabellerna utifrån dem.
- **DbContext:** klassen som *är* kopplingen till databasen. `_db.CarSubmissions` är tabellen, och `SaveChangesAsync()` skickar ändringarna till databasen.
- **Migration:** en fil som beskriver hur databasen ska ändras, till exempel "skapa tabellen Offers". Den skapas med `dotnet ef migrations add Namn` och ligger i `Data/Migrations/`.
- **Foreign key (främmande nyckel):** `CarSubmission.CustomerId` pekar på en användare. `Offer.CarSubmissionId` pekar på en bil.
- **Navigation property:** `CarSubmission.Offers` och `Offer.CarSubmission` gör att vi kan gå mellan objekten i C#.

### Viktiga rader
- `IdentityDbContext<ApplicationUser>`: ger oss Identity-tabellerna (`AspNetUsers`, `AspNetRoles` …) gratis.
- `base.OnModelCreating(builder)`: måste köras först, annars blir Identity-tabellerna fel.
- `HasConversion<string>()`: enum-värden sparas som text ("Pending") i stället för som siffror (0).
- `HasPrecision(18, 2)`: pengar sparas med 2 decimaler.
- `DeleteBehavior.Restrict` på `Offer.Staff`: SQL Server tillåter inte två "cascade delete"-vägar till samma tabell.

### Kontrollfrågor
1. Varför har vi två tabeller (`CarSubmissions` och `Offers`) i stället för att lägga budet i samma tabell?
2. Vad händer om du lägger till en ny property i `CarSubmission`? Vilka kommandon behöver du köra?
3. Vad är skillnaden mellan `CustomerId` och `Customer` i `CarSubmission`?

<details><summary>Svar</summary>

1. En bil kan få **flera** bud, till exempel ett nytt bud efter att kunden nekat. Med en egen tabell sparas hela budhistoriken (relationen är en-till-många).
2. Kör `dotnet ef migrations add LaggTillX -p src/BilCirkeln.Api -o Data/Migrations`. Migrationen körs sedan automatiskt när appen startar (`MigrateAsync` i `DbSeeder`).
3. `CustomerId` är själva kolumnen i databasen (foreign key, en sträng). `Customer` är navigation property, alltså hela användarobjektet. Det laddas bara om vi skriver `.Include(c => c.Customer)`.
</details>

---

## Del 2 – Dependency injection och Program.cs

**Fil:** [Program.cs](../src/BilCirkeln.Api/Program.cs)

### Begrepp
- **Dependency injection (DI):** i stället för att skriva `new CarSubmissionService(...)` själv registrerar vi klassen i `Program.cs`. ASP.NET Core skapar sedan objektet och skickar in det i konstruktorn där det behövs.
  ```csharp
  builder.Services.AddScoped<ICarSubmissionService, CarSubmissionService>();
  ```
  "När någon ber om `ICarSubmissionService`, ge dem en `CarSubmissionService`."
- **Scoped:** ett nytt objekt per HTTP-anrop. Det passar en DbContext.
- **Interface (`ICarSubmissionService`):** ett kontrakt. Controllern vet bara *vad* servicen kan göra, inte *hur*. Det gör koden lättare att byta ut och testa.
- **Middleware-pipeline:** `app.UseCors()`, `UseAuthentication()`, `UseAuthorization()` körs i ordning för varje anrop.

### Kontrollfrågor
4. Varför måste `UseAuthentication()` komma före `UseAuthorization()`?
5. Varför använder vi `AddIdentityCore` och inte `AddIdentity`?
6. Vad är CORS och varför behövs det här?

<details><summary>Svar</summary>

4. Först måste API:et ta reda på **vem** du är genom att läsa och kontrollera token (authentication). Först därefter kan det avgöra om du **får** göra något (authorization).
5. `AddIdentity` slår på cookie-inloggning som standard. Vi använder JWT, och `AddIdentityCore` ger bara användar- och lösenordshanteringen.
6. Webbläsaren blockerar anrop från en adress (`localhost:5173`) till en annan (`localhost:5221`) om servern inte uttryckligen tillåter det. Vår CORS-policy tillåter **bara** vår frontend.
</details>

---

## Del 3 – Inloggning: Identity + JWT

**Filer:** [Services/AuthService.cs](../src/BilCirkeln.Api/Services/AuthService.cs), [Services/TokenService.cs](../src/BilCirkeln.Api/Services/TokenService.cs), [Data/DbSeeder.cs](../src/BilCirkeln.Api/Data/DbSeeder.cs)

### Begrepp
- **Identity:** Microsofts färdiga system för användare. `UserManager` skapar användare, hashar lösenord och kontrollerar dem.
- **Hash:** lösenordet görs om till en sträng som inte går att räkna baklänges. Vid inloggning hashas det inskrivna lösenordet och jämförs med det sparade.
- **JWT (JSON Web Token):** ett "ID-kort" i tre delar, `header.payload.signatur`. Payloaden innehåller *claims* (id, e-post, roll). Signaturen görs med den hemliga nyckeln, så ingen kan ändra innehållet utan att det märks.
  - Testa: logga in i Swagger, kopiera token och klistra in på https://jwt.io för att se innehållet.
  - Payloaden är **inte krypterad**, bara signerad. Lägg aldrig hemligheter i den.
- **Claim:** en uppgift om användaren i token, till exempel `ClaimTypes.Role = "Kund"`.
- **Seed:** data som skapas automatiskt vid start, här rollerna och personalkontot.

### Flödet vid inloggning
1. React skickar `POST /api/auth/login` med e-post och lösenord.
2. `AuthService.LoginAsync` hittar användaren och kontrollerar lösenordet med `CheckPasswordAsync`.
3. `TokenService.CreateToken` skapar en JWT med id, e-post, namn och roll, signerad med `Jwt:Key`.
4. React sparar token i `localStorage`.
5. Vid varje anrop lägger axios till headern `Authorization: Bearer <token>`.
6. API:et kontrollerar signatur, utfärdare, mottagare och utgångstid (`TokenValidationParameters` i Program.cs).

### Kontrollfrågor
7. Varför får man samma felmeddelande vid fel e-post och fel lösenord?
8. Hur förhindrar vi att någon registrerar sig som `Personal`?
9. Var finns JWT-nyckeln, och varför inte i `appsettings.json`?
10. Om någon ändrar rollen i sin token från "Kund" till "Personal", vad händer?

<details><summary>Svar</summary>

7. Annars kan en angripare testa e-postadresser och ta reda på vilka konton som finns (user enumeration).
8. `RegisterAsync` ger **alltid** rollen `Kund`. Rollen kan inte skickas in. Personal skapas bara av `DbSeeder` med uppgifter från user secrets.
9. Den finns i **user secrets** (`dotnet user-secrets list`), som sparas utanför projektmappen. `appsettings.json` pushas till GitHub, och då skulle vem som helst kunna skapa giltiga tokens.
10. Signaturen stämmer inte längre. API:et avvisar token och svarar 401.
</details>

---

## Del 4 – Services och affärsregler

**Fil:** [Services/CarSubmissionService.cs](../src/BilCirkeln.Api/Services/CarSubmissionService.cs), [Services/ServiceResult.cs](../src/BilCirkeln.Api/Services/ServiceResult.cs)

### Affärsreglerna (kunna rabbla!)
| Regel | Metod |
|---|---|
| Samma regnr får inte ha två aktiva förfrågningar | `CreateAsync` |
| Kund ser bara sina egna bilar | `GetForCustomerAsync`, `GetByIdAsync` |
| Bud får bara lämnas när status är `Pending` eller `Declined` | `MakeOfferAsync` |
| Bara ägaren kan svara på bud, och bara om status är `OfferMade` | `RespondToOfferAsync` |
| Avböja går bara vid `Pending` eller `Declined` | `RejectAsync` |

### Begrepp
- **async/await:** databasanrop tar tid. `await` låter servern hantera andra anrop under tiden i stället för att stå och vänta.
- **LINQ:** `.Where()`, `.OrderByDescending()`, `.Include()` blir SQL-frågor som EF Core översätter.
- **`Include`:** hämtar relaterad data, till exempel kunden och buden, i samma fråga.
- **`ServiceResult<T>`:** servicen returnerar *antingen* data *eller* ett fel (`NotFound`, `Forbidden`, `BadRequest`) med ett meddelande. Controllern gör om felet till rätt HTTP-statuskod.

### Kontrollfrågor
11. Varför ligger reglerna i servicen och inte i controllern?
12. Vad gör `NormalizeRegNumber` och varför behövs den?
13. Vad returneras om en kund försöker öppna en annan kunds bil? Var bestäms det?

<details><summary>Svar</summary>

11. Då kan reglerna testas utan HTTP (se testerna), controllern blir kort och lätt att läsa, och samma regel används överallt.
12. Den gör om "abc 123" till "ABC123". Annars skulle samma bil kunna sparas på flera sätt, och dubblettkontrollen skulle inte fungera.
13. `GetByIdAsync` returnerar `ServiceError.Forbidden`. `ApiControllerBase.ToActionResult` gör om det till **403**.
</details>

---

## Del 5 – Controllers och DTOs

**Filer:** [Controllers/](../src/BilCirkeln.Api/Controllers/), [DTOs/](../src/BilCirkeln.Api/DTOs/)

### Begrepp
- **Controller:** tar emot HTTP-anrop. `[Route("api/submissions")]` och `[HttpPost("{id:int}/offers")]` ger URL:en `POST api/submissions/5/offers`.
- **`[ApiController]`:** validerar DTO:n automatiskt och svarar **400** om något fält är fel, innan vår kod ens körs.
- **`[Authorize(Roles = "Personal")]`:** bara inloggade med rollen Personal kommer in. Annars blir det 401 (inte inloggad) eller 403 (fel roll).
- **DTO (Data Transfer Object):** klassen som skickas in eller ut via API:et. Vi skickar aldrig entiteten direkt, för då kunde vi till exempel råka skicka med lösenordshashen från `ApplicationUser`.
- **DataAnnotations:** `[Required]`, `[Range]`, `[RegularExpression]` är valideringsregler på DTO:n.
- **`CurrentUserId`:** läses från token (`ClaimTypes.NameIdentifier`), aldrig från request body.

### Kontrollfrågor
14. Vad är skillnaden mellan 401 och 403?
15. Varför skickar inte frontend med `customerId` när en bil skickas in?
16. Frontenden validerar redan regnr-formatet. Varför validerar vi igen i backend?

<details><summary>Svar</summary>

14. **401 Unauthorized** betyder att API:et inte vet vem du är (ingen eller ogiltig token). **403 Forbidden** betyder att API:et vet vem du är, men att du inte får göra det här.
15. Då skulle vem som helst kunna skicka in någon annans id. Vi läser id:t från den signerade token.
16. Frontend-validering kan kringgås, till exempel med Swagger, curl eller Postman. Den finns bara för att göra det lättare för användaren. Backend-valideringen är den riktiga säkerheten.
</details>

---

## Del 6 – Tester

**Filer:** [tests/BilCirkeln.Tests/](../tests/BilCirkeln.Tests/)

### Begrepp
- **xUnit:** testramverket. `[Fact]` markerar ett test.
- **Arrange / Act / Assert:** förbered data, kör metoden och kontrollera resultatet. Titta på hur varje test är uppdelat med tomma rader.
- **InMemory-databas:** en låtsasdatabas i minnet. Varje test får en egen (`Guid.NewGuid()`), så testerna påverkar inte varandra.
- **Testnamn:** `Metod_Situation_FörväntatResultat`, till exempel `MakeOffer_WhenOfferAlreadyOpen_ReturnsBadRequest`.

### Testa själv
```bash
dotnet test
```
**Övning:** ändra regeln i `MakeOfferAsync` så att bud tillåts vid alla statusar. Kör `dotnet test` och se vilket test som går sönder. Ändra sedan tillbaka.

### Kontrollfrågor
17. Varför testar vi services och inte controllers?
18. Vad är en nackdel med InMemory-databasen?

<details><summary>Svar</summary>

17. Affärsreglerna finns i servicen, och där finns mest logik att testa. Controllerna gör nästan ingenting själva. De kan testas med integrationstester (`WebApplicationFactory`), som finns under "kan ha".
18. Den beter sig inte exakt som SQL Server. Den kontrollerar till exempel inte maxlängder, och vissa frågor fungerar annorlunda. Därför har vi även testat manuellt mot den riktiga databasen.
</details>

---

## Del 7 – Frontend (React)

**Filer:** [src/bilcirkeln-web/src/](../src/bilcirkeln-web/src/)

| Fil | Vad den gör |
|---|---|
| `main.jsx` | Startpunkt. Omsluter appen med `BrowserRouter` och `AuthProvider` |
| `App.jsx` | Alla routes (URL → sida) |
| `api/client.js` | Axios-instans. Interceptors lägger till token och loggar ut vid 401 |
| `api/*Api.js` | En funktion per API-anrop |
| `context/AuthContext.jsx` | Delar inloggad användare med alla komponenter |
| `components/ProtectedRoute.jsx` | Skickar till /login om man inte är inloggad eller har fel roll |
| `pages/*` | En komponent per sida |
| `labels.js` | Översätter "Pending" → "Inskickad" osv. |

### Begrepp
- **Komponent:** en funktion som returnerar JSX (HTML-liknande kod).
- **`useState`:** sparar data som kan ändras. När den ändras ritas komponenten om.
- **`useEffect`:** kör kod när komponenten visas, till exempel hämtar data. `[]` betyder "bara en gång", `[status]` betyder "varje gång status ändras".
- **Context:** delar data globalt utan att skicka props genom alla nivåer.
- **Interceptor:** kod som körs automatiskt före varje anrop eller efter varje svar.
- **Kontrollerade formulär:** `value={form.make}` + `onChange` gör att React alltid vet vad som står i fältet.

### Kontrollfrågor
19. Räcker `ProtectedRoute` som säkerhet? Varför inte?
20. Var läggs JWT-token till i anropen?
21. Varför har `StaffDashboardPage` `[status]` i sin `useEffect`?

<details><summary>Svar</summary>

19. Nej. Den döljer bara sidor i webbläsaren, och vem som helst kan anropa API:et direkt. Den riktiga säkerheten är `[Authorize]` och kontrollerna i servicen.
20. I request-interceptorn i `api/client.js`, som läser token från `localStorage`.
21. Listan ska hämtas på nytt varje gång personalen byter filter.
</details>

---

## Del 8 – Demo-manus för redovisningen (5–7 min)
1. **Problem** (30 s): att sälja bil privat är krångligt, och handlaren tappar överblicken.
2. **Kund** (1,5 min): skapa konto → sälj bil → visa felmeddelande vid ogiltigt regnr → mina bilar.
3. **Personal** (1,5 min): logga in → filtrera "Inskickad" → lämna bud.
4. **Kund** (30 s): neka budet. **Personal**: lämna nytt bud. **Kund**: acceptera.
5. **Säkerhet** (1 min): Swagger utan token ger 401, kundtoken på personallistan ger 403, user secrets och token på jwt.io.
6. **Tester** (30 s): `dotnet test` → 18 gröna.
7. **Kod** (1 min): följ ett anrop: `SellCarPage` → `client.js` → `CarSubmissionsController.Create` → `CarSubmissionService.CreateAsync` → databasen.
8. **Reflektion** (30 s): begränsningar och vidareutveckling.

## Del 9 – Troliga följdfrågor från läraren
- *"Vad händer om två personal lämnar bud samtidigt?"* Båda läser status `Pending` och båda kan spara. Det kallas race condition. Lösningen vore en concurrency token (`[Timestamp]`) i EF Core. Det är en känd begränsning.
- *"Varför inte spara token i en cookie?"* En HttpOnly-cookie är säkrare mot XSS, men kräver mer CORS-konfiguration och CSRF-skydd. `localStorage` valdes för att hålla lösningen enkel (beslut 11 i beslutsloggen).
- *"Hur skulle du driftsätta detta?"* Azure App Service för API:et, Azure SQL för databasen och Static Web Apps för React. Hemligheterna läggs i App Settings eller Key Vault.
- *"Vad gör `[ApiController]`?"* Den validerar DTO:n automatiskt och svarar 400, och den läser JSON från body automatiskt.
