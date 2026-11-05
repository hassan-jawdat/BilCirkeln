# Projektrapport – BilCirkeln "Vi köper din bil"

*Examensprojekt, fullstack .NET, 4 veckor, individuellt.*

> **Utkast.** Avsnitten markerade med ✏️ ska du skriva om med egna ord, särskilt reflektionen. Läraren vill höra din röst.

## 1. Bakgrund och problemformulering
Att sälja bil privat tar tid. Säljaren måste fotografera bilen, skriva en annons, svara på frågor och boka provkörningar, och ofta prutar köparen i sista stund.
Många vill hellre sälja direkt till en handlare, men då sker kontakten ofta via telefon och mejl. För handlaren blir det svårt att hålla ordning på vilka bilar som har fått bud och vad kunden har svarat.

**Problemformulering:** Hur kan en enkel webbapplikation göra det lätt för en privatperson att få ett bud på sin bil, och samtidigt ge personalen en tydlig överblick över alla förfrågningar?

## 2. Mål och avgränsningar
**Mål**
- Kunden kan registrera sig, skicka in en bil och följa statusen
- Personalen kan se alla förfrågningar, filtrera dem och lämna bud
- Kunden kan acceptera eller neka budet
- Säker inloggning med roller
- Affärsreglerna är testade med automatiska tester

**Avgränsningar:** ingen bilduppladdning, ingen betalning eller avtalssignering, ingen hämtning av bildata från Transportstyrelsen, ingen e-postavisering och ingen molndrift. Se [projektplanen](projektplan.md).

## 3. Vald lösning
En webbapplikation med en React-frontend som pratar med ett ASP.NET Core Web API via JSON. API:et sparar data i SQL Server via Entity Framework Core.

```
[React (Vite)]  --HTTP/JSON + JWT-->  [ASP.NET Core Web API]  --EF Core-->  [SQL Server]
   sidor, formulär                     Controllers → Services             AspNetUsers, CarSubmissions, Offers
```

**Datamodell**
- `ApplicationUser` (Identity-användare + `FullName`)
- `CarSubmission`: bilen som kunden vill sälja (regnr, märke, modell, år, miltal, drivmedel, växellåda, önskat pris, status)
- `Offer`: ett bud från personalen (belopp, meddelande, status). En `CarSubmission` kan ha **flera** `Offer`.

**Statusflöde**
```
Pending ──(personal lämnar bud)──► OfferMade ──(kund accepterar)──► Accepted
   │                                   │
   │                                   └──(kund nekar)──► Declined ──(nytt bud)──► OfferMade
   └──(personal avböjer)──► Rejected ◄──(personal avböjer)── Declined
```

## 4. Genomförande
Arbetet följde [projektplanen](projektplan.md) med prioriteringen måste/bör/kan:
1. Struktur: solution, API-projekt, testprojekt, React-app, git
2. Datamodell och `AppDbContext`, första migrationen
3. Identity + JWT + roller och seed av personalkonto
4. Services med affärsregler och tunna controllers med DTOs
5. xUnit-tester
6. Frontend: auth-context, routing, kund- och personalsidor
7. Manuell test av hela flödet, sedan dokumentation

Detaljer finns i [projektdagboken](projektdagbok.md) och [beslutsloggen](beslutslogg.md).

## 5. Tekniska val och motiveringar
| Val | Motivering |
|---|---|
| ASP.NET Core Web API (.NET 8 LTS) | Kursens fokus. Långtidssupport |
| EF Core code first + migrations | Databasen skapas från C#-klasserna, och ändringar versionshanteras |
| Identity | Färdig och beprövad lösning för användare och lösenordshashning, så man slipper skriva egen krypto |
| JWT | API:et blir stateless, och samma token fungerar för React-appen |
| Services + tunna controllers | Affärslogiken går att testa utan HTTP, och controllers blir lätta att läsa |
| DTOs | Vi bestämmer exakt vilken data som går in och ut, och läcker inga interna fält |
| `ServiceResult<T>` | Förväntade fel (hittas inte, ingen behörighet, ogiltig status) hanteras utan exceptions |
| React + Vite + React Router + Axios | Snabb utvecklingsmiljö, sidnavigering och enkel HTTP med interceptors |
| xUnit + InMemory-databas | Snabba tester av affärsreglerna utan SQL Server |

## 6. Säkerhetsaspekter
- **Lösenord** hashas av Identity (PBKDF2) och sparas aldrig i klartext. Regel: minst 8 tecken, stor och liten bokstav och siffra.
- **JWT** signeras med HMAC-SHA256. API:et kontrollerar utfärdare, mottagare, giltighetstid (2 h) och signatur.
- **Hemligheter** (JWT-nyckel, connection string, personalens lösenord) ligger i *user secrets*, inte i git.
- **Behörighet:** `[Authorize(Roles = ...)]` på endpoints. Dessutom kontrollerar servicen att en kund bara kan se och svara på sina **egna** bilar (403 annars).
- **Användar-id tas från token**, inte från frontend, så man kan inte utge sig för att vara någon annan.
- **Registrering ger alltid rollen Kund**, så ingen kan göra sig själv till personal.
- **Samma felmeddelande** vid fel e-post och fel lösenord, så att en angripare inte kan ta reda på vilka konton som finns.
- **Validering** på servern med DataAnnotations (regnr-format, årtal, pris, längd på text). Frontend-valideringen är bara för användarvänlighet.
- **CORS** tillåter bara frontendens adress.
- **SQL-injektion** förhindras eftersom EF Core använder parametriserade frågor.
- **XSS:** React escapar all text automatiskt.
- *Kända svagheter:* token i `localStorage` kan stjälas vid en XSS-attack. Det finns ingen rate limiting på inloggningen och ingen utloggning på servern (token gäller tills den går ut).

## 7. Testning och resultat
**Automatiska tester (xUnit)**: 18 tester, alla godkända (`dotnet test`).

| Område | Exempel på testfall |
|---|---|
| Skapa förfrågan | Status blir `Pending`, regnr normaliseras ("abc 123" → "ABC123"), dubblett nekas, extra bildetaljer sparas |
| Behörighet | Kund ser bara sina egna. Annan kund får `Forbidden`. Personal ser alla |
| Bud | Bud på `Pending` ger `OfferMade`. Två öppna bud samtidigt nekas. Okänt id ger `NotFound` |
| Svar på bud | Acceptera ger `Accepted`. Neka och sedan nytt bud fungerar. Svar utan bud nekas. Annan kund nekas |
| Avböja | `Pending` → `Rejected`. Accepterad bil kan inte avböjas |
| JWT | Token innehåller id, e-post och roll och har rätt giltighetstid |

**Manuella tester (Swagger/HTTP + webbläsare)**
| Test | Förväntat | Resultat |
|---|---|---|
| Anrop utan token | 401 | ✅ |
| Kund anropar personallistan | 403 | ✅ |
| Fel lösenord | 400 + "Fel e-post eller lösenord." | ✅ |
| Ogiltigt regnr "X1" | 400 + valideringsmeddelande | ✅ |
| Hela flödet bud → neka → nytt bud → acceptera | Statusen följer flödet | ✅ |
| Bud efter accepterad | 400 | ✅ |
| CORS från annan origin | Ingen `Access-Control-Allow-Origin` | ✅ |

**Resultat:** Alla krav under *måste ha* och *bör ha* i projektplanen är uppfyllda.

## 8. Användning av AI
AI-verktyget **Claude Code** har använts för att:
- ta fram en första version av backend, frontend, tester och dokumentation utifrån mina krav
- förklara begrepp (DbContext, JWT, dependency injection, CORS m.m.), se [kodgenomgang.md](kodgenomgang.md)

**Hur jag kontrollerade AI:ns arbete:**
- byggde projektet och körde alla tester (18/18)
- testade alla endpoints, både rätt och fel fall, och kontrollerade statuskoder
- gick igenom koden fil för fil och svarade på kontrollfrågor
- ✏️ *Skriv här vad du själv ändrade, ifrågasatte eller lärde dig.*

## 9. Problem och utmaningar
- `AddIdentity` slår på cookie-inloggning som standard och krockar med JWT. Lösningen blev `AddIdentityCore` + `AddRoles`.
- SQL Server tillåter inte två cascade-delete-vägar till samma tabell (`Offer → User` både via bilen och via personalen). Lösningen blev `DeleteBehavior.Restrict` på personalrelationen.
- Teckenkodning vid manuella test i terminalen ("ä" skickades fel). Problemet låg i verktyget, inte i appen.
- ✏️ *Lägg till egna utmaningar.*

## 10. Reflektion och vidareutveckling
✏️ *Skriv med egna ord: vad gick bra, vad var svårt, vad skulle du göra annorlunda?*

**Begränsningar**
- Inga bilder av bilen
- Personalen kan inte se statistik
- Token kan inte återkallas före utgångstiden
- Körs bara lokalt

**Möjlig vidareutveckling**
- Bilduppladdning (t.ex. Azure Blob Storage)
- Hämta bildata automatiskt via regnr
- E-postavisering när ett bud lämnas eller besvaras
- Integrationstester med `WebApplicationFactory`
- Refresh tokens och token i HttpOnly-cookie
- Driftsättning i Azure
