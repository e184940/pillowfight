# 🎮 Pillowfight - Setup Guide

**Sist oppdatert:** 3. januar 2026

---

## 🚀 Quick Start

### 1. Player Setup (5 min)
- Lag Empty GameObject → Navn: **"Player"**
- Add Component → **PlayerController**
- Add Component → **PlayerAnimationController**

### 2. Character Model (2 min)
- Dra `Assets/FREE/Pack_FREE_PartyCharacters/Prefabs/Character_040ae0.prefab` inn under Player
- Position: `(0, -1, 0)`
- Slett Camera child (hvis den finnes)

### 3. Platform Setup (1 min per platform)
- Hver platform må ha **Collider** (Is Trigger: OFF)
- Layer: **Ground**
- Ingen Rigidbody

### 4. Kamera Setup (2 min)

**Metode 1 - PlatformerCamera Script (Enklest, anbefalt):**
1. Velg **Main Camera** i Hierarchy
2. Add Component → **PlatformerCamera**
3. I Inspector:
   - **Target:** Dra **Player** hit
   - **Distance:** 8 (avstand fra spiller)
   - **Height:** 3 (høyde over spiller)
   - **Smooth Speed:** 0.125 (lavere = glattere)
   - **Rotate With Player:** ✓ (følger spillerens rotasjon)
   - **Avoid Obstacles:** ✓ (zoomer inn ved vegger)
   - **Collision Smoothing:** 0.3 (smooth zoom)
   - **Collision Buffer:** 0.8 (avstand til vegg)
   - **Zoom Dead Zone:** 0.5 (ignorer små zoom-endringer)

**Hvis kameraet zoomer inn/ut konstant:**
- Øk **Collision Smoothing** til 0.5
- Øk **Zoom Dead Zone** til 1.0
- Eller skru av **Avoid Obstacles**

**Metode 2 - Enkel Parent/Child (også bra):**
1. Dra **Main Camera** som child av **Player** i Hierarchy
2. Sett Camera Position: (0, 2, -6)
3. Sett Camera Rotation: (15, 0, 0)
4. Ferdig! Følger automatisk

### 5. Kanoner (5 min) - VALGFRITT

**For komplett kanon-setup, se:** `CANNON_SETUP.md`

**Quick start:**
1. Lag **Pillow** prefab (Cube med Rigidbody + Pillow.cs)
2. Lag **PillowCannon** (Cylinder med PillowCannon.cs)
3. Assign Pillow prefab til kanon
4. Kanonen skyter automatisk mot spilleren!

### 6. Health System (2 min) - ANBEFALT

**For komplett health setup, se:** `HEALTH_SETUP.md`

**Quick start:**
1. Velg **Player** → Add Component → **PlayerHealth**
   - Max Health: 100
   - Invincibility Duration: 1
2. Oppdater **Pillow prefab** → Damage: 10
3. Test: La pute treffe spilleren → Console viser damage!

### 7. Game Over & Restart (5 min) - ANBEFALT

**For komplett setup, se:** `GAMEOVER_SETUP.md`

**Quick start:**
1. Lag **GameOverPanel** på Canvas (UI → Panel)
2. Legg til "GAME OVER" text, score text, restart button
3. Velg Canvas → Add Component → **GameOverUI**
4. Assign alle referanser i Inspector
5. Deaktiver GameOverPanel i Hierarchy
6. Test: Dø → Game Over screen vises → Klikk Restart!

**Nå har du et KOMPLETT spill! 🎉**
- Health system ✅
- Damage fra kanoner ✅
- Game Over screen ✅
- Restart funksjon ✅
- Score (tid overlevd) ✅

---
---

## 🎯 Controls

- **W/S eller Pil Opp/Ned:** Frem/tilbake
- **A/D:** Strafe venstre/høyre
- **Pil Venstre/Høyre:** Roter (alltid)
- **Mus:** Roter kamera
- **Space:** Hopp

**Note:** A/D roterer KUN når W/S også er trykt

---

## ✅ Verification

Trykk Play - Du skal se i Console:
```
Player setup complete
Animator found: Character_Model
```

Test:
- [ ] Beveg deg (W/S) → Running animation
- [ ] Stå stille → Idle animation
- [ ] Hopp → Jump animation
- [ ] Collisions fungerer

---

## 🐛 Vanlige Problemer

**Går gjennom platformer?**
→ Sjekk at platform har Collider med Is Trigger OFF

**Spinner ukontrollert?**
→ Restart Play mode (Rigidbody settes opp automatisk)

**Ingen animasjoner?**
→ Sjekk at Character har Animator med char_AC controller

**Kamera følger ikke spilleren?**
→ Sjekk at Main Camera har PlatformerCamera script med Target = Player

**Kamera spinner ukontrollert?**
→ Ikke bruk både PlatformerCamera OG Main Camera som child - velg én metode!

---

## 🎥 Kamera Tips

**Juster kamera-vinkel (PlatformerCamera):**
- Distance: 6 = Nært, 8 = Normalt, 12 = Langt
- Height: 2 = Lavt, 3 = Normalt, 5 = Høyt
- Distance: 8, Height: 3 = Standard third-person
- Distance: 12, Height: 8 = Høyere overblikk

**Juster smoothness:**
- Smooth Speed: 0.05 = Veldig glatt (treg)
- Smooth Speed: 0.2 = Rask respons
- Collision Smoothing: 0.3 = Standard zoom ved vegger
- Collision Smoothing: 0.5 = Veldig smooth zoom ved vegger

**Hvis kamera "klikker" eller zoomer inn og ut konstant:**
1. **Øk Zoom Dead Zone** til 1.0 - ignorer små endringer
2. **Øk Collision Smoothing** til 0.5 - glattere zoom
3. **Øk Collision Buffer** til 1.2 - mer avstand til vegg
4. **Eller skru av Avoid Obstacles** - ingen auto-zoom

**Hvis kamera ikke roterer med spilleren:**
- Sjekk at Rotate With Player er ✓ (checked)

**For Camera som child av Player:**
- Position: (0, 2, -6) = Standard third-person
- Position: (0, 5, -10) = Lenger bak
- Position: (3, 2, -6) = Over skulder

**Debug:**
- Velg Main Camera med PlatformerCamera i Scene view
- Se gule/grønne/røde Gizmos som viser kamera-posisjon og raycast

---

**Det er alt! Resten håndteres automatisk av scriptene.** 🎉



