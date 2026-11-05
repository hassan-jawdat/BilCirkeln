# Beslutslogg

Här skrivs viktiga tekniska och praktiska beslut ner, och varför de togs.

| # | Datum | Beslut | Motivering | Alternativ som valdes bort |
|---|---|---|---|---|
| 1 | 2026-10-29 | Projektnamn **BilCirkeln**, tjänsten "Vi köper din bil" | Tydligt problem och avgränsad omfattning som går att göra på 4 veckor | Fullständig bilannonssajt, som var för stor |
| 2 | 2026-10-29 | .NET 8 (LTS) med SDK 9 installerat | Kursens krav. SDK 9 kan bygga för `net8.0` | .NET 9 |
| 3 | 2026-10-29 | Två tabeller: `CarSubmissions` och `Offers` (1 till många) | En bil kan få flera bud, till exempel ett nytt bud efter att kunden nekat. Då sparas historiken | Lägga budet som kolumner i samma tabell, men då försvinner historiken |
| 4 | 2026-10-29 | Status som enum, sparad som **text** i databasen | Lätt att läsa i databasen, och ordningen på enum-värdena spelar ingen roll | Spara som heltal |
| 5 | 2026-10-29 | `AddIdentityCore` i stället för `AddIdentity` | `AddIdentity` slår på cookie-inloggning, men vi använder bara JWT | `AddIdentity` + ändra default scheme |
| 6 | 2026-10-29 | Registrering ger alltid rollen `Kund`. Personal skapas via seed | Säkerhet: ingen kan göra sig själv till personal | Låta användaren välja roll vid registrering |
| 7 | 2026-10-29 | `ServiceResult<T>` i stället för exceptions för förväntade fel | Tydligt flöde, tunna controllers, lätt att testa | Kasta egna exceptions + middleware |
| 8 | 2026-10-29 | Användar-id läses från JWT-token, aldrig från request body | En användare kan inte låtsas vara någon annan | Skicka `customerId` från frontend |
| 9 | 2026-10-29 | Hemligheter i **user secrets** | Kravet: inga lösenord, nycklar eller connection strings i git | appsettings.json |
| 10 | 2026-10-29 | Tester med EF Core **InMemory** | Snabba tester som inte behöver SQL Server | SQLite in-memory, Testcontainers |
| 11 | 2026-10-29 | JWT sparas i `localStorage` i frontend | Enkelt och vanligt i skolprojekt. Risken är XSS, men React escapar output | HttpOnly-cookie, som är säkrare men mer komplext med CORS |
| 12 | 2026-10-29 | Samma regnr får inte ha två aktiva förfrågningar | Undviker dubbletter för personalen | Ingen kontroll |
| 13 | 2026-10-29 | Migrationer körs automatiskt vid start | Enkelt att starta projektet för läraren | Köra `dotnet ef database update` manuellt |
| 14 | 2026-10-30 | Nytt utseende på säljformuläret: registreringsskylt, miltal och mätare som visar hur mycket som är ifyllt | Tydligare och mer inbjudande för kunden. Inspirerat av hur riktiga inköpstjänster ser ut | Vanligt formulär |
| 15 | 2026-10-30 | Kunden skriver **mil**, men API:et sparar fortfarande **km** (×10 i frontend) | I Sverige anger man miltal i mil. Backend och tester behövde inte ändras | Byta kolumnen till mil, vilket kräver en ny migration |
| 16 | 2026-10-30 | Formuläret visas **ett steg i taget**. Mätaren står bredvid det senaste steget | Kunden ser bara en fråga åt gången och vet hur långt det är kvar | Alla fält på en gång |
| 17 | 2026-10-30 | 10 nya frågor om bilen (nycklar, däck, service, besiktning, säljartyp, import, finansiering, klädsel), besvaras med knappar | Personalen kan lämna ett mer korrekt bud | Bara fritext i beskrivningen |
| 18 | 2026-10-30 | De nya kolumnerna är **nullable** i databasen, men `[Required]` i DTO:n | Äldre förfrågningar saknar svaren och ska fortfarande fungera. Nya förfrågningar måste ha svar | Radera databasen, eller sätta påhittade standardvärden |
| 19 | 2026-10-30 | `CarSubmissionDto` ändrad från positional record till klass med `init`-properties | 25 parametrar i rad blev svårt att läsa och lätt att blanda ihop | Behålla record |

> Lägg till nya rader när du ändrar något under projektets gång.
