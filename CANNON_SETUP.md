# 🎯 Kanon Setup Guide - Pillowfight

## 🚀 Quick Setup (5 minutter)

### 1. Lag Pillow Prefab

**A. Opprett Pillow GameObject:**
1. Hierarchy → Create → 3D Object → **Cube**
2. Navn: **"Pillow"**
3. Scale: (0.5, 0.3, 0.8) - pute-form
4. Tag: Sett til **"Projectile"** (lag tag hvis ikke finnes)

**B. Legg til components:**
1. Add Component → **Rigidbody**
   - Mass: 0.5
   - Use Gravity: ✓
2. Add Component → **Box Collider** (allerede der)
3. Add Component → **Pillow** (vårt script)
   - Lifetime: 10 (økt for å nå lengre)
   - Push Force: 10
   - **Damage: 10** (nytt!)
   - Destroy On Hit: ✓

**C. Lag Prefab:**
1. Dra **Pillow** fra Hierarchy til **Assets** mappen
2. Slett Pillow fra Hierarchy (den er nå en prefab)

---

### 3. Player Health System (2 min)

**A. Legg til PlayerHealth script:**
1. Velg **Player** i Hierarchy
2. Add Component → **PlayerHealth**
3. I Inspector:
   - **Max Health:** 100
   - **Invincibility Duration:** 1 (sekunder immun etter treff)
   - **Player Renderer:** La stå tom (finnes automatisk!)

**B. Test Health System:**
1. Trykk Play
2. La en pute treffe spilleren
3. Console skal vise:
   ```
   PlayerHealth: Auto-found renderer on Character_040ae0
   Pillow hit player! Dealt 10 damage.
   Player took 10 damage! Health: 90/100
   ```
4. Spilleren blinker i 1 sekund (invincibility)
5. Etter 10 treff (100 damage) → "Player died!"

---

### 2. Lag Pillow Cannon

**A. Opprett Cannon GameObject:**
1. Hierarchy → Create → 3D Object → **Cylinder**
2. Navn: **"PillowCannon"**
3. Rotation: **(0, 0, 0)** - standard orientering (Y-akse er forward!)
4. Scale: (0.5, 1, 0.5) - kanon-form

**B. Opprett FirePoint:**
1. Høyreklikk på PillowCannon → Create Empty
2. Navn: **"FirePoint"**
3. Position: **(0, 1, 0)** - toppen av kanonen (Y-akse fremover)

**C. Legg til PillowCannon Script:**
1. Velg PillowCannon i Hierarchy
2. Add Component → **PillowCannon**
3. I Inspector:
   - **Pillow Prefab:** Dra **Pillow** prefab fra Assets hit
   - **Fire Point:** Dra **FirePoint** child hit
   - **Shoot Force:** 25
   - **Fire Rate:** 0.8
   - **Auto Fire:** ✓
   - **Aim At Player:** ✓ (VIKTIG!)
   - **Random Spread:** 2
   - **Aim Speed:** 5
   - **Max Pitch Angle:** 45 (hvor langt opp/ned kanonen kan sikte)

**D. Plassering:**
- Flytt PillowCannon til ønsket posisjon i scenen
- Kanonen står **oppreist** med Y-akse som forward direction
- Scriptet roterer hele kanonen mot spilleren
- Pitch (opp/ned) begrenset til ±45 grader for naturlig oppførsel

**Tips for plassering:**
- På gulvet (Y: 0-2) = Skyter horisontalt eller litt opp
- På platform (Y: 5-10) = Skyter nedover (maks 45°)
- Avstand: 10-20 meter = Best for gameplay
- **Y-akse (grønn Gizmo) peker mot spilleren!**

---

## 🎯 Testing

1. **Trykk Play**
2. Du skal se:
   - Kanonen roterer mot spilleren
   - Kanonen skyter puter hvert 0.5 sekund
   - Puter treffer spilleren og dytter
   - Console viser: "Pillow hit player!"

---

## ⚙️ Justeringer

### Kanon Innstillinger:

**Balansert kanon (anbefalt):**
- Shoot Force: 25
- Fire Rate: 0.8
- Random Spread: 2
- Aim At Player: ✓

**Langsom kanon:**
- Fire Rate: 0.3 (ett skudd hvert 3. sekund)
- Shoot Force: 30
- Random Spread: 0 (perfekt aim)

**Rask kanon:**
- Fire Rate: 2 (2 skudd per sekund)
- Shoot Force: 20
- Random Spread: 5

**Sniper kanon (nøyaktig):**
- Fire Rate: 0.2
- Shoot Force: 40 (veldig langt)
- Random Spread: 0
- Aim At Player: ✓

**Shotgun kanon (spray):**
- Fire Rate: 0.5
- Shoot Force: 18
- Random Spread: 15 (stor spread)

### Pillow Innstillinger:

**Tyngre pute (mer push):**
- Push Force: 20
- Mass (Rigidbody): 1.0

**Lettere pute (mindre push):**
- Push Force: 5
- Mass (Rigidbody): 0.2

**Spretten pute:**
- Rigidbody → Material: Physics Material med Bounciness: 0.8

---

## 🎨 Visuell Forbedring

### Lag puten penere:

**Metode 1 - Enkel farge:**
1. Velg Pillow prefab
2. Create → Material → Navn: "PillowMaterial"
3. Sett Albedo farge til hvit/rosa/blå
4. Dra material på Pillow prefab

**Metode 2 - Bruk FREE assets:**
1. Sjekk om FREE pakken har pillow modeller
2. Erstatt Cube med pillow modell
3. Skalér til passende størrelse

### Lag kanonen penere:

1. Velg PillowCannon
2. Legg til flere Cylinders som children for mer detaljert design
3. Bruk Materials for å fargelegge

---

## 🎮 Flere Kanoner (Waves)

### Enkel wave setup:

**Wave 1 - Enkle kanoner:**
- 2 kanoner
- Fire Rate: 1
- Shoot Force: 15

**Wave 2 - Flere kanoner:**
- 4 kanoner rundt spilleren
- Fire Rate: 2
- Random Spread: 10

**Wave 3 - Kaos:**
- 6+ kanoner
- Fire Rate: 3
- Random Spread: 15

**Tips:** Dupliser kanoner (Ctrl+D) og plasser rundt i scenen

---

## 🐛 Vanlige Problemer

**Kanonen skyter ikke?**
→ Sjekk at Pillow Prefab er assigned i Inspector

**Puter går rett gjennom spilleren?**
→ Sjekk at Player har tag "Player"
→ Sjekk at Pillow har Rigidbody og Collider

**Puter dytter ikke spilleren?**
→ Sjekk at Player har Rigidbody (settes opp automatisk av PlayerController)
→ Øk Push Force på Pillow

**Kanonen roterer rart (ligger på siden)?**
→ Sjekk at kanon sin **initial Rotation = (0, 0, 0)** (står oppreist)
→ Sjekk at **Max Pitch Angle = 45** (begrenset opp/ned bevegelse)

**Kanonen "flipper" plutselig og peker opp/feil retning?**
→ FIKSET! Bruker nå Quaternion med Y-akse som forward
→ Restart Play mode for å laste inn den nye koden
→ Kanonen skal nå rotere smooth uten å flippe

**Kanonen skyter fra feil ende?**
→ Sjekk at **FirePoint Position = (0, 1, 0)** (toppen av Cylinder, Y-akse)
→ Hvis feil retning, sett FirePoint til (0, -1, 0)

**Puter forsvinner før de treffer?**
→ Øk **Lifetime** på Pillow script til 10-15 sekunder
→ Sjekk at putene faktisk spawner (se Console for "PillowCannon fired")
→ Øk **Shoot Force** hvis puter faller for tidlig pga. gravity

**Kanonen sikter ikke mot spilleren?**
→ Sjekk Console - skal si "PillowCannon: Found player target"
→ Sjekk at Player har tag "Player"
→ Sjekk at **Aim At Player** er ✓
→ Restart Play mode
→ I Scene view, se rød pil fra kanon (Gizmo) - skal peke mot spiller

**Puter går for kort/langt?**
→ Juster Shoot Force (15-40 range)
→ Sjekk at Pillow Rigidbody har Use Gravity ✓
→ Sjekk Pillow mass (0.3-1.0 range)

**Puter forsvinner for raskt?**
→ Øk Lifetime på Pillow script

**For mange puter i scenen (lag)?**
→ Reduser Lifetime til 3 sekunder
→ Reduser Fire Rate

---

## 💡 Neste Steg

**Power-ups:**
- Shield (blokkerer puter)
- Speed boost
- Slow-motion

**Score system:**
- Poeng for å unngå puter
- Tid overlevd
- High score

**Flere fiender:**
- Bevegelige kanoner
- Kanoner som følger spilleren
- Boss-kanoner

**Level design:**
- Flere platformer
- Dekninger å gjemme seg bak
- Moving platforms

---

**Du har nå et komplett kanon-system! Test og juster etter smak.** 🎉

