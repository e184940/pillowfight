# 🎮 KOMPLETT SETUP GUIDE - Pillowfight
**Oppdatert:** 2. januar 2026  
**Alt du trenger å vite på ett sted!**
---
## 📋 Innholdsfortegnelse
1. [Animasjoner Setup](#animasjoner-setup)
2. [Collision Fix](#collision-fix)
3. [Rotasjon Fix](#rotasjon-fix)
4. [Feilsøking](#feilsøking)
5. [Forventet Resultat](#forventet-resultat)
---
## 🎬 Animasjoner Setup
### Steg 1: Legg til Character Model
1. I Unity, gå til `Assets/FREE/Pack_FREE_PartyCharacters/Prefabs/`
2. Dra en character prefab (f.eks. `Character_040ae0.prefab`) inn i Hierarchy
3. Dra character-objektet inn under **Player** GameObject
**Hierarchy skal se slik ut:**
```
Player (har PlayerController + PlayerAnimationController + Rigidbody)
  ├── Main Camera
  └── Character_040ae0 (har Animator med char_AC controller)
```
### Steg 2: Fjern Character's Camera
1. Ekspander Character i Hierarchy
2. Finn "Camera" child object (hvis den finnes)
3. **Slett eller deaktiver** denne
### Steg 3: Juster Character Position
1. Velg Character i Hierarchy
2. Inspector → Transform:
   - Position: `(0, -1, 0)` eller `(0, 0, 0)`
   - Rotation: `(0, 0, 0)`
   - Scale: `(1, 1, 1)`
### Steg 4: Legg til PlayerAnimationController Script
1. Velg **Player** GameObject (parent, ikke character)
2. Add Component → "PlayerAnimationController"
3. Scriptet auto-detekterer Animator og PlayerController

**Innstillinger i Inspector:**
- **Run Speed Threshold:** 0.1 (hvor raskt du må bevege deg for å trigge running)
- **Fall Speed Threshold:** -3 (hvor raskt du må falle for fall-animasjon - høyere = kun ved store fall)
- **Transition Time:** 0.05 (hvor raskt animasjoner bytter - lavere = mer immediate)

**Tips:** 
- For enda raskere animasjoner: Sett Transition Time til 0.01
- For å unngå fall-animasjon ved hopp: Øk Fall Speed Threshold til -5 eller høyere
### Steg 5: Test Animasjoner
Trykk Play og test:
- ✅ Stå stille → Idle animation
- ✅ W/S → Running animation  
- ✅ Space → Jump animation
- ✅ Fall ned → Fall animation
**Console skal vise:**
```
Animator found: Character_Model
Animator controller: char_AC
Available animator parameters:
  - idle (Trigger)
  - run (Trigger)
  - jump (Trigger)
  - fall (Trigger)
  - feel (Trigger)
  - getup (Trigger)
```
---
## 🔧 Collision Fix
### Problem: Spilleren går gjennom platformer
**Løsning er allerede i koden!** Scriptet gjør automatisk:
1. ✅ Setter Collision Detection til `Continuous`
2. ✅ Legger til Rigidbody med riktige innstillinger
3. ✅ Legger til Capsule Collider
4. ✅ Deaktiverer Character_Model colliders (de forårsaket konflikter)
5. ✅ Fjerner Rigidbody fra Character_Model
### Hva DU må gjøre:
#### A. Restart Unity Play Mode
```
Stop → Play igjen
```
**Console skal vise:**
```
Player setup complete - Rigidbody and Collider configured
Disabled collider on: body
Disabled collider on: head
(eller andre character parts)
```
#### B. Fiks Platformene
**Metode 1 - Bruk PlatformSetup Script (Enklest):**
1. Velg hver platform i Hierarchy
2. Add Component → "PlatformSetup"
3. Ferdig! Auto-fikset
**Metode 2 - Manuelt:**
For hver platform:
1. Sjekk at den har **Collider** (Box Collider, Mesh Collider)
2. Collider → **Is Trigger: OFF** (unchecked!)
3. GameObject → **Layer: Ground**
4. Hvis den har **Rigidbody** → Slett den
#### C. Test Collisions
- Gå mot platform → Stopper? ✅
- Hopp på platform → Lander? ✅
- Står på platform → Faller ikke gjennom? ✅
### Forventet Setup:
```
Player
  ├── Rigidbody
  │    ├── Mass: 1
  │    ├── Angular Damping: 0.99
  │    ├── Use Gravity: ON
  │    ├── Collision Detection: Continuous
  │    └── Constraints: Freeze Rotation X & Z (IKKE Y!)
  ├── Capsule Collider
  │    ├── Center: (0, 1, 0)
  │    ├── Radius: 0.5
  │    ├── Height: 2
  │    └── Is Trigger: OFF
  ├── PlayerController.cs
  ├── PlayerAnimationController.cs
  └── Character_Model
       ├── Animator (Controller: char_AC)
       ├── Colliders: DISABLED ✅ (auto)
       └── Rigidbody: REMOVED ✅ (auto)
Platform
  ├── Box Collider (Is Trigger: OFF)
  └── Layer: Ground
```
---
## 🔄 Rotasjon Fix
### Problem: Spilleren roterer ukontrollert
**Allerede fikset i koden!**
### Hva som ble gjort:
1. ✅ Fjernet `rb.freezeRotation = true` (konflikt med constraints)
2. ✅ Økt `rb.angularDamping` til **0.99** (stopper ukontrollert rotasjon)
3. ✅ Constraints freezer KUN X og Z rotasjon
4. ✅ Y-rotasjon er FRI (mus/tastatur kan rotere)
### Test:
- Står spilleren stille? ✅ (ingen spinning)
- Kan du rotere med mus? ✅
- Kan du rotere med A/D? ✅
- Velter spilleren? ❌ (skal IKKE velte)
### Hvis det fortsatt roterer:
1. Velg Player → Inspector → Rigidbody
2. Sjekk:
   - **Angular Damping: 0.99** (IKKE 0.05!)
   - **Constraints: Freeze Rotation X og Z** (IKKE alle tre!)
---
## 🐛 Feilsøking
### Problem: "No Animator found!"
**Løsning:**
- Character må være direct child av Player
- Character må ha Animator component med char_AC controller
### Problem: Animasjoner spiller ikke
**Løsning:**
1. Velg Character i Hierarchy
2. Inspector → Animator → Controller
3. Dra inn `char_AC.controller` fra:
   `Assets/FREE/.../Animations/char_AC.controller`
### Problem: Spilleren faller gjennom platformer
**Løsning:**
1. Sjekk Console for "Player setup complete" melding
2. Sjekk at platformene har Collider med **Is Trigger: OFF**
3. Sjekk at platformene er på **Layer: Ground**
### Problem: Spilleren spinner ukontrollert
**Løsning:**
1. Restart Play mode
2. Velg Player → Rigidbody
3. Sjekk at Angular Damping = 0.99
4. Sjekk at Character_Model IKKE har Rigidbody
### Problem: "MissingComponentException: No Rigidbody"
**Løsning:**
- Restart Play mode - scriptet legger automatisk til Rigidbody
### Problem: Console logger for mye
**Løsning:**
Debug logging er nå deaktivert for å unngå spam. Hvis du vil aktivere det igjen for debugging:

**I PlayerController.cs:**
```csharp
// Linje ~125 - Fjern kommentarene for å aktivere landing-logging
// if (!wasGrounded)
// {
//     Debug.Log($"Landed on: {hit.collider.gameObject.name}");
// }
```

**I PlayerAnimationController.cs:**
```csharp
// Linje ~83 - Fjern kommentarene for player state logging
// Debug.Log($"Player State - Grounded: {isGrounded}, HSpeed: {horizontalSpeed:F2}");

// Linje ~91 - Fjern kommentarene for landing logging
// Debug.Log("Player landed");

// Linje ~151 - Fjern kommentarene for animation change logging
// Debug.Log($"Animation crossfade to: {stateName}");
```
---
## ✅ Forventet Resultat
### Console Output ved Start:
```
Player setup complete - Rigidbody and Collider configured
Disabled collider on: body
Disabled collider on: head
Animator found: Character_Model
Animator controller: char_AC
Available animator parameters: (liste)
PlayerController found!
```
### In-Game Oppførsel:
- ✅ Spilleren faller til gulvet og stopper
- ✅ Spilleren kan ikke gå gjennom platformer
- ✅ Spilleren kan hoppe opp på platformer
- ✅ Spilleren står stabilt på platformer
- ✅ Animasjoner spiller korrekt (idle, run, jump)
- ✅ Ingen ukontrollert rotasjon
- ✅ Mus/tastatur kontrollerer rotasjon
- ✅ Spilleren velter ikke over
- ✅ Ground detection fungerer (kan hoppe på platformer)
### Controls:
- **W/S eller Pil Opp/Ned:** Beveg frem/tilbake
- **A/D:** Beveg til venstre/høyre (strafing)
- **W+A/D:** Beveg frem OG roter samtidig
- **Pil Venstre/Høyre:** Roter spilleren (alltid, uavhengig av bevegelse)
- **Mus:** Roter kamera og spiller (alltid)
- **Space:** Hopp

**Viktig oppsummering:**
- **A/D** = Strafing når alene, roterer når kombinert med W/S
- **Pil V/H** = Roterer alltid (som klassisk spill)
- **Fall-animasjon** = Trigges ved fall-hastighet < -3 m/s (både ved hopp og når man går av kanter)
---
## 📊 Verification Checklist
Før du sier "det fungerer ikke", sjekk ALLE disse:
**Player Setup:**
- [ ] Player har PlayerController script (aktivert)
- [ ] Player har PlayerAnimationController script (aktivert)
- [ ] Player har Rigidbody (auto-lagt til)
- [ ] Player har Capsule Collider (auto-lagt til)
- [ ] Rigidbody: Mass=1, Angular Damping=0.99, Use Gravity=ON
- [ ] Rigidbody: Collision Detection=Continuous
- [ ] Rigidbody: Constraints=Freeze Rotation X & Z (IKKE Y)
- [ ] Console viser "Player setup complete"
**Character Setup:**
- [ ] Character er direct child av Player
- [ ] Character har Animator component
- [ ] Animator Controller er satt til "char_AC"
- [ ] Character Position er (0, -1, 0) eller (0, 0, 0)
- [ ] Character colliders er deaktivert (grå checkbox)
- [ ] Character har IKKE Rigidbody
- [ ] Console viser "Disabled collider on: [navn]"
**Platform Setup:**
- [ ] Hver platform har Collider (Box, Mesh, etc)
- [ ] Collider Is Trigger er OFF (unchecked)
- [ ] Platform Layer er "Ground"
- [ ] Platform har IKKE Rigidbody
**Test:**
- [ ] Spilleren står stille (ingen spinning)
- [ ] Spilleren faller til gulvet og stopper
- [ ] Spilleren stopper når den treffer platform fra siden
- [ ] Spilleren lander på platform fra toppen
- [ ] Idle animation spiller når stille
- [ ] Running animation spiller når beveger seg
- [ ] Jump animation spiller når hopper
---
## 🚨 Emergency Reset
Hvis INGENTING fungerer:
1. **Slett Player GameObject**
2. **Lag ny Empty GameObject:** Hierarchy → Create Empty → Navn: "Player"
3. **Legg til scripts:** Add Component → PlayerController, PlayerAnimationController
4. **Legg til Camera:** Dra Main Camera som child av Player
5. **Legg til Character:** Dra character prefab som child av Player
6. **Trykk Play** - Alt skal auto-konfigureres
---
## 💡 Pro Tips

### Juster Animasjons-Responsivitet
I Inspector på Player → PlayerAnimationController:

**For immediate animasjoner (0 delay):**
```
Transition Time: 0 (helt immediate)
```

**For smooth men responsive animasjoner (anbefalt):**
```
Transition Time: 0.05 (standard - veldig rask)
```

**For smooth men tregere animasjoner:**
```
Transition Time: 0.2 (mer blend mellom animasjoner)
```

### Juster Fall-Animasjon
**Oppførsel:** Fall-animasjon trigges når fall-hastighet er < threshold (både ved hopp og ved å gå av kanter)

**Juster i Inspector:**
```
Fall Speed Threshold: -3 (standard - balansert)
Fall Speed Threshold: -5 (kun ved veldig store fall)
Fall Speed Threshold: -10 (nesten aldri)
Fall Speed Threshold: -1 (trigger raskt, selv ved små fall)
```

**Forklaring:** 
- Ved nedturen fra hopp faller spilleren ~-2 til -4 m/s
- Med threshold på -3 vil fall-animasjon trigge ved større hopp
- Fall-animasjon trigges nå både ved hopp OG når man går av platformer

### Third-Person View
For å se animasjonene bedre:
1. Velg Main Camera under Player
2. Position: (0, 2, -3)
3. Rotation: (15, 0, 0)
### Debug Visualization
I Scene view mens Play mode:
- Grønn linje fra player = På bakken ✓
- Rød linje fra player = I luften ✗
- Grønne outlines = Colliders
### Performance
Hvis spillet laggar:
- Reduser debug logging i scripts
- Sett Rigidbody Interpolation til "None"
- Sett Collision Detection til "Discrete"
---
## 📝 Oppsummering
**Scriptet gjør automatisk:**
- ✅ Setter opp Rigidbody med riktige innstillinger
- ✅ Legger til Capsule Collider
- ✅ Deaktiverer Character colliders
- ✅ Fjerner Character Rigidbody
- ✅ Kobler animasjoner til bevegelse
- ✅ Forhindrer ukontrollert rotasjon
**Du må gjøre:**
- ✅ Legg til character som child av Player
- ✅ Legg til PlayerAnimationController script
- ✅ Fiks platformer (Is Trigger OFF, Layer Ground)
- ✅ Test!
**Alt annet er automatisk!** 🎉
---
**Lykke til med spillet! 🚀**
