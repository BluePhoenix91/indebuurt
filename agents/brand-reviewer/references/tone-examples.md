# Tone & Voice Examples

This document provides concrete examples of good and bad brand voice for the Brand Reviewer to reference.

---

## Brand Voice Summary

**Our voice is:** Friendly, informative, honest — like advice from a fellow dog owner who knows the area.

**Our perspective:** Second person (je, jouw) — direct address to the reader.

**Our honesty:** Always mention trade-offs — no neighborhood is perfect.

---

## Good Examples

### Intro Opening

**Good:**
> "Dampoort is een compacte stadswijk aan de rand van het Gentse centrum. Met het station om de hoek en groen op wandelafstand combineer je hier stedelijk gemak met voldoende buitenruimte voor je dagelijkse routine."

*Why it works:*
- Second person ("je")
- Specific ("station om de hoek", "groen op wandelafstand")
- Practical ("dagelijkse routine")
- Not overselling

---

### Trade-off + Mitigation

**Good:**
> "De wijk heeft een praktische ligging, maar de Dendermondsesteenweg kan druk zijn met verkeer. Kies voor rustigere wandelingen de routes richting de waterkant bij de Schelde."

*Why it works:*
- Honest about limitation ("kan druk zijn met verkeer")
- Immediately offers solution ("Kies voor rustigere wandelingen")
- Specific alternative ("routes richting de waterkant")

---

### Sparse Data Handling

**Good:**
> "Een gespecialiseerde dierenwinkel vind je niet in de wijk zelf. De dichtstbijzijnde optie is Tom & Co aan de Dendermondsesteenweg, op 16 minuten wandelen. Als alternatief bieden de supermarkten in de wijk een basisassortiment voor je viervoeter."

*Why it works:*
- Acknowledges gap without apology
- Names specific alternative
- Gives distance/time
- Offers additional option

---

### Daily Life Description

**Good:**
> "Een typische dag in Dampoort begint met een ochtendwandeling naar een van de parken in de buurt. La Sapinière en het Cirkelspark liggen op loopafstand en bieden rustige paden voor de eerste ronde van de dag."

*Why it works:*
- Paints a picture ("typische dag", "ochtendwandeling")
- Names specific places
- Practical detail ("rustige paden")
- Friendly, conversational

---

### Benefit Statement

**Good:**
> "Met 5 parken binnen 15 minuten wandelen heb je voldoende afwisseling voor de dagelijkse routes. La Sapinière biedt schaduwrijke paden voor warme dagen, terwijl het Cirkelspark een open grasveld heeft."

*Why it works:*
- Specific numbers ("5 parken", "15 minuten")
- Direct address ("heb je")
- Practical distinction between options
- Local knowledge ("schaduwrijke paden", "open grasveld")

---

## Bad Examples

### Intro Opening

**Bad:**
> "Welkom in deze prachtige wijk! Hier vindt u alles wat u nodig heeft voor een ideale levensstijl met uw huisdier. Wij bieden een uitgebreide gids om u te helpen."

*Problems:*
- "u" instead of "je" (formal)
- "wij bieden" (corporate)
- "prachtige wijk" (vague superlative)
- "ideale levensstijl" (marketing speak)
- "huisdier" instead of "hond/viervoeter"

**Fixed:**
> "Dampoort is een compacte stadswijk met veel te bieden voor jou en je hond. In deze gids vind je wat je moet weten over parken, dierenartsen en het dagelijkse leven in de wijk."

---

### Trade-off Handling

**Bad:**
> "Er zijn helaas geen dierenwinkels in de wijk."

*Problems:*
- "helaas" (apologetic)
- No alternative offered
- Dead end statement

**Fixed:**
> "Een gespecialiseerde dierenwinkel vind je niet in de wijk zelf. De dichtstbijzijnde optie is Tom & Co op 16 minuten wandelen."

---

### Overly Corporate

**Bad:**
> "Onze wijk biedt een uitgebreid aanbod aan faciliteiten. Wij streven ernaar om u de beste ervaring te bieden."

*Problems:*
- "Onze wijk" (possessive)
- "wij streven ernaar" (corporate mission statement)
- "u" (formal)
- No specific information

**Fixed:**
> "De wijk heeft een goede basis voor het dagelijks leven, met supermarkten, parken en een dierenarts binnen handbereik."

---

### List-Like Prose

**Bad:**
> "Er zijn 5 parken. Er zijn 3 supermarkten. Er is 1 dierenarts. Er zijn 2 bushaltes."

*Problems:*
- Repetitive structure
- No narrative connection
- Reads like a database dump

**Fixed:**
> "Met 5 parken, 3 supermarkten en een dierenarts binnen handbereik heb je de meeste voorzieningen op wandelafstand. De 2 bushaltes maken ook verdere bestemmingen vlot bereikbaar."

---

### Vague Description

**Bad:**
> "De wijk heeft diverse groene ruimtes en voorzieningen voor honden. De sfeer is gezellig en de bewoners zijn vriendelijk."

*Problems:*
- "diverse groene ruimtes" (vague)
- "voorzieningen voor honden" (what kind?)
- "gezellig" (cliché)
- No specific names or details

**Fixed:**
> "Met de Hondenweide Dampoort op 12 minuten en parken als La Sapinière op loopafstand, heb je genoeg ruimte voor de dagelijkse wandeling. De levendige sfeer rond de Dendermondsesteenweg zorgt voor een praktische buurt om te wonen."

---

### Marketing Speak

**Bad:**
> "Ontdek de ideale wijk voor hondenbezitters! Profiteer van onze unieke locatie nabij alle voorzieningen die u nodig heeft."

*Problems:*
- "Ontdek" (promotional)
- "ideale wijk" (superlative)
- "hondenbezitters" instead of "baasjes"
- "Profiteer van" (sales language)
- "unieke locatie" (cliché)
- "u nodig heeft" (formal)

**Fixed:**
> "Dampoort is een praktische wijk voor baasjes die stedelijk willen wonen maar ook ruimte zoeken voor dagelijkse wandelingen. De parken en voorzieningen liggen op loopafstand."

---

## Terminology Reference

**Source of truth:** `../shared/terminology.json`

Do not duplicate the terminology rules here. Read from the JSON file for:
- Preferred terms (`use`)
- Acceptable alternatives (`alternativeAllowed`)
- Terms to avoid (`avoid`)
- Allowed exception phrases (`allowedPhrases`)

### Perspective (not in terminology.json)
| Avoid | Use Instead |
|-------|-------------|
| u, uw | je, jouw |
| wij, ons, onze | — (rephrase to second person) |

---

## Tone Markers Cheatsheet

### Positive Markers (aim for these)
- "je vindt", "je kunt", "jouw hond"
- "handig voor", "praktisch voor wie"
- "neem water mee", "houd er rekening mee"
- "de route via [place]"
- "richting [landmark]"
- "binnen X minuten"
- "op loopafstand"

### Negative Markers (avoid these)
- "wij bieden", "onze services"
- "u kunt", "men dient"
- "ontdek", "profiteer"
- "ideaal", "perfect", "uniek"
- "helaas", "jammer genoeg"
- "diverse", "verschillende" (without specifics)
