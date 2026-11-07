# Projektdagbok

Skriv en kort anteckning efter varje arbetspass:
- Vad gjorde jag?
- Vad hjälpte AI till med?
- Hur kontrollerade jag att det AI gjorde var rätt?
- Problem och nästa steg

---

## 2026-10-29
**Gjort:**
- Skapade projektstruktur (solution, API, tester, React-app) och git-repo
- Backend: modeller (`CarSubmission`, `Offer`, `ApplicationUser`), `AppDbContext`, första migrationen
- Identity + JWT med rollerna `Kund` och `Personal`, seed av personalkonto
- Services med affärsregler och tunna controllers med DTOs
- 17 xUnit-tester
- React-frontend: inloggning, registrering, sälj bil, mina bilar, personalvy, detaljsida med bud
- Dokumentation: README, projektplan, beslutslogg, rapportutkast

**AI-hjälp:**
- Claude Code skrev en första version av koden, testerna och dokumentationen utifrån kraven i `Krav.md`.

**Kontroll:**
- `dotnet build`: 0 varningar, 0 fel
- `dotnet test`: 17 av 17 tester godkända
- Hela flödet testat mot API:et med HTTP-anrop: registrera, logga in, skicka in bil, dubblett nekas, ogiltigt regnr nekas, kund får 403 på personallistan, bud, neka, nytt bud, acceptera, bud efter accepterad nekas, annan kund får 403
- CORS kontrollerat: bara `http://localhost:5173` får anropa API:et
- `npm run lint` och `npm run build` utan fel

**Problem:**
- Vid test med curl i Git Bash blev "ä" fel teckenkodat och API:et svarade 400. Felet låg i testverktyget, inte i appen. Webbläsaren skickar alltid UTF-8.

**Nästa steg:**
- [x] Gå igenom all kod med hjälp av [kodgenomgang.md](kodgenomgang.md)
- [x] Testa hela flödet själv i webbläsaren
- [x] Skapa GitHub-repo och pusha

---

## 2026-10-30
**Gjort:**
- Testade appen i webbläsaren som kund (max) och personal. Hela flödet fungerade: inskickad bil → bud → nekat → nytt bud → accepterat.
- Gjorde om säljformuläret efter förebild från en riktig inköpstjänst: ett fält som ser ut som en registreringsskylt, miltal i mil, frågor som visas ett steg i taget och en mätare som visar hur många procent som är ifyllt.
- La till 10 nya frågor om bilen (nycklar, däck, servicebok, servicehistorik, besiktning, säljartyp, import, finansiering, klädselns färg och material) som besvaras med knappar.
- Backend: nya fält i modellen och DTO:n, ny migration `AddCarDetails`, ett nytt test.

**AI-hjälp:**
- Jag beskrev med skärmbilder hur formuläret skulle se ut, och AI byggde komponenterna `LicensePlateInput`, `ProgressGauge` och `ChoiceButtons`.

**Kontroll:**
- `dotnet test`: 18 av 18 godkända
- Testade mot API:et att en bil med alla svar sparas, att det blir 400 om ett svar saknas och att gamla bilar fortfarande går att öppna
- Klickade igenom formuläret i webbläsaren

**Problem:**
- Gamla bilar i databasen saknade svar på de nya frågorna. Lösning: nullable kolumner i databasen, men `[Required]` i DTO:n.
- Drivmedel och växellåda var förvalda, så steget hoppades över. Lösning: de börjar tomma.

**Nästa steg:** Förstå koden och förbereda redovisningen.

---

## 2026-11-03
**Gjort:**
- Bokade tid för redovisningen i studentportalen.
- Gick igenom frontend, backend och databasen med hjälp av AI: vilka verktyg som används, hur lagren hänger ihop och vilka tabeller och relationer som finns.

**AI-hjälp:** Förklaringar och kontrollfrågor på enkel svenska.

**Kontroll:** Jag tittade i den riktiga databasen och såg tabellerna och buden som jag själv hade skapat i appen.

**Nästa steg:** GitHub och förbereda presentationen.

---

## 2026-11-05
**Gjort:**
- Första commit och publicering på GitHub: https://github.com/hassan-jawdat/BilCirkeln
- Förberedde redovisningen: HTTP, REST, HTTPS/TLS, JWT, hashning av lösenord, CORS och arkitekturen.

**AI-hjälp:** Förklaringar av begreppen och förslag på korta svar till läraren.

**Kontroll:** Övade på att förklara med egna ord.

**Problem:**
- Inlämningen av GitHub-länken blev sen (deadline kl. 12:00 dagen före redovisningen). Lärdom: pusha tidigt och ofta.
- Appen kör HTTP lokalt, inte HTTPS. Det är okej lokalt men inte i produktion. Lades in som begränsning i rapporten.

---

## 2026-11-06
**Gjort:**
- Startade appen och testade demon en sista gång.
- Muntlig redovisning genomförd.

---

## 2026-11-07
**Gjort:**
- Gjorde klart projektrapporten: la till säljformuläret, de nya frågorna och HTTP/TLS-begränsningen, och skrev avsnitten om AI, utmaningar och reflektion.
- Gjorde PDF-versioner av dokumentationen och en zip-fil med källkoden för att skicka till läraren via mail.

**AI-hjälp:** Uppdatering av rapporten och att skapa PDF-filerna.

**Kontroll:** Läste igenom PDF:en och kontrollerade att alla 10 avsnitt som kraven nämner finns med.
