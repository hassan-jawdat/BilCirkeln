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
- [ ] Gå igenom all kod med hjälp av [kodgenomgang.md](kodgenomgang.md) och svara på kontrollfrågorna
- [ ] Testa hela flödet själv i webbläsaren
- [ ] Skapa GitHub-repo och pusha

---

<!-- Kopiera mallen nedan för nästa pass -->
## ÅÅÅÅ-MM-DD
**Gjort:**
**AI-hjälp:**
**Kontroll:**
**Problem:**
**Nästa steg:**
