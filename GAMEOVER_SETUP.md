# 🎮 Game Over & Restart Setup Guide

## ✅ Hva du får:

1. **GameOverUI.cs** - Håndterer game over screen og restart
2. **Score system** - Viser tid overlevd
3. **Restart button** - Last inn scene på nytt
4. **Pause game** - Fryser spillet ved død

---

## 🚀 Quick Setup (5 min)

### 1. Lag Game Over Panel

**A. Opprett Panel:**
1. Høyreklikk på **Canvas** i Hierarchy
2. UI → **Panel**
3. Navn: **GameOverPanel**
4. I Inspector:
   - Anchor: Stretch (ALT+SHIFT + klikk stretch icon)
   - Color: Semi-transparent mørk (RGBA: 0, 0, 0, 200)

**B. Legg til "GAME OVER" Text:**
1. Høyreklikk på GameOverPanel → UI → **Text - TextMeshPro**
2. Navn: **GameOverText**
3. I Inspector:
   - Text: "GAME OVER"
   - Font Size: 60-80
   - Alignment: Center (både horizontal og vertikal)
   - Color: Rød (#FF0000)
   - Anchor: Center
   - Position: (0, 100, 0)

**C. Legg til Score Text:**
1. Høyreklikk på GameOverPanel → UI → **Text - TextMeshPro**
2. Navn: **ScoreText**
3. I Inspector:
   - Text: "You survived: 00:00"
   - Font Size: 30-40
   - Alignment: Center
   - Color: Hvit
   - Anchor: Center
   - Position: (0, 0, 0)

**D. Legg til Restart Button:**
1. Høyreklikk på GameOverPanel → UI → **Button - TextMeshPro** (eller Button)
2. Navn: **RestartButton**
3. I Inspector:
   - Anchor: Center
   - Position: (0, -80, 0)
   - Width: 200, Height: 60
4. Velg **Text** child under RestartButton
5. Endre Text til: "RESTART"
6. Font Size: 24

**E. Legg til Quit Button (optional):**
1. Høyreklikk på GameOverPanel → UI → **Button - TextMeshPro** (eller Button)
2. Navn: **QuitButton**
3. I Inspector:
   - Anchor: Center
   - Position: (0, -160, 0)
   - Width: 200, Height: 60
4. Velg **Text** child under QuitButton
5. Endre Text til: "QUIT"

---

### 2. Setup GameOverUI Script

**A. Legg til Script på Canvas:**
1. Velg **Canvas** i Hierarchy
2. Add Component → **GameOverUI**

**B. Assign Referanser i Inspector:**
1. **Game Over Panel:** Dra **GameOverPanel** hit
2. **Game Over Text:** Dra **GameOverText** hit
3. **Score Text:** Dra **ScoreText** hit
4. **Restart Button:** Dra **RestartButton** hit
5. **Quit Button:** Dra **QuitButton** hit (optional)
6. **Pause Game On Death:** ✓ (checked)

**C. Skjul Panel ved start:**
1. Velg **GameOverPanel** i Hierarchy
2. I Inspector, øverst: **Deaktiver checkboxen** (panel skal være hidden ved start)

---

## 🧪 Testing

### Test 1: Game Over Screen
1. Trykk Play
2. La 10+ puter treffe spilleren (100+ damage)
3. **Forventet:**
   - Console: "Player died!"
   - Console: "GameOverUI: Showing game over screen. Survived: X.Xs"
   - Game Over panel vises
   - Spillet pauses (Time.timeScale = 0)
   - Score viser tid overlevd

### Test 2: Restart Button
1. Når game over screen vises
2. Klikk **RESTART** button
3. **Forventet:**
   - Scene laster på nytt
   - Spiller har full health
   - Timer resetter
   - Kanoner starter på nytt

### Test 3: Quit Button
1. Når game over screen vises
2. Klikk **QUIT** button
3. **Forventet:**
   - I Unity Editor: Play mode stopper
   - I build: Spillet lukkes

---

## ⚙️ Justeringer

### Vanskelighetsgrad:

**Lettere (flere forsøk):**
- Player Max Health: 200
- Pillow Damage: 5
- Flere restarts tillatt

**Vanskeligere (quick death):**
- Player Max Health: 50
- Pillow Damage: 25
- Høyere Fire Rate på kanoner

### UI Styling:

**Mer dramatisk game over:**
```
GameOverText:
- Font Size: 100
- Color: Blood red (#8B0000)
- Add Outline effect
```

**Bedre score display:**
```
ScoreText:
- Text: "TIME SURVIVED\n00:00"
- Add shadow effect
- Font Size: 35
```

### Pause vs No Pause:

**Med pause (anbefalt):**
- Pause Game On Death: ✓
- Spilleren kan se hva som skjedde
- Kanoner stopper

**Uten pause (mer arcade):**
- Pause Game On Death: ✗
- Kanoner fortsetter å skyte
- Mer hektisk følelse

---

## 🎨 Forbedringer (Optional)

### 1. High Score System

```csharp
// I GameOverUI.cs - legg til:
private float highScore = 0f;

void Start()
{
    highScore = PlayerPrefs.GetFloat("HighScore", 0f);
}

void ShowGameOver()
{
    // ...existing code...
    
    if (survivalTime > highScore)
    {
        highScore = survivalTime;
        PlayerPrefs.SetFloat("HighScore", highScore);
        scoreText.text += "\nNEW HIGH SCORE!";
    }
    else
    {
        scoreText.text += $"\nHigh Score: {FormatTime(highScore)}";
    }
}

string FormatTime(float time)
{
    int minutes = Mathf.FloorToInt(time / 60f);
    int seconds = Mathf.FloorToInt(time % 60f);
    return $"{minutes:00}:{seconds:00}";
}
```

### 2. Fade In Animation

```csharp
// I GameOverUI.cs - legg til:
using System.Collections;

IEnumerator FadeInPanel()
{
    CanvasGroup canvasGroup = gameOverPanel.GetComponent<CanvasGroup>();
    if (canvasGroup == null)
        canvasGroup = gameOverPanel.AddComponent<CanvasGroup>();
    
    canvasGroup.alpha = 0;
    float duration = 1f;
    float elapsed = 0f;
    
    while (elapsed < duration)
    {
        elapsed += Time.unscaledDeltaTime; // Unscaled pga pause
        canvasGroup.alpha = Mathf.Lerp(0, 1, elapsed / duration);
        yield return null;
    }
}

void ShowGameOver()
{
    // ...existing code...
    StartCoroutine(FadeInPanel());
}
```

### 3. Sound Effects

```csharp
// I GameOverUI.cs - legg til:
[Header("Audio")]
public AudioClip gameOverSound;
private AudioSource audioSource;

void Start()
{
    audioSource = gameObject.AddComponent<AudioSource>();
}

void ShowGameOver()
{
    // ...existing code...
    
    if (audioSource != null && gameOverSound != null)
    {
        audioSource.PlayOneShot(gameOverSound);
    }
}
```

### 4. Statistics Panel

Lag ekstra panel som viser:
- Puter dodged
- Puter truffet
- Accuracy
- Longest survival streak

---

## 🐛 Vanlige Problemer

**Game Over panel kommer ikke opp når health er tom?**
→ **VIKTIGSTE:** Sjekk Console når spilleren dør - skal vise:
   ```
   Player died!
   GameOverUI: Showing game over screen. Survived: X.Xs
   ```
→ Hvis du IKKE ser "GameOverUI: Showing game over screen":
   1. Sjekk at GameOverUI script er på Canvas
   2. Sjekk Console for "GameOverUI: Game Over Panel not assigned!" 
   3. Sjekk at Game Over Panel er assigned i GameOverUI Inspector
   4. Restart Play mode
→ Hvis du ser "No PlayerHealth found":
   - Legg til PlayerHealth script på Player
→ Sjekk at PlayerHealth faktisk kaller OnDeath event (se PlayerHealth.cs Die() method)

**Game Over panel vises ikke?**
→ Sjekk at GameOverPanel er **DEAKTIVERT** i Hierarchy ved start
→ Sjekk at alle referanser er assigned i GameOverUI Inspector
→ Console skal vise: "GameOverUI: Found PlayerHealth on Player"
→ Console skal vise: "GameOverUI: Subscribed to OnDeath event"
→ Console skal vise: "GameOverUI: Game Over Panel hidden at start"

**Steg-for-steg debug:**
1. Trykk Play
2. Sjekk Console - skal vise:
   ```
   GameOverUI: Found PlayerHealth on Player
   GameOverUI: Subscribed to OnDeath event
   GameOverUI: Game Over Panel hidden at start
   GameOverUI: Ready
   ```
3. La spilleren ta damage til health = 0
4. Console skal vise:
   ```
   Player took 10 damage! Health: 0/100
   Player died!
   GameOverUI: Showing game over screen. Survived: X.Xs
   GameOverUI: Score text set to 'You survived: 00:XX'
   ```
5. Hvis noe av dette mangler - se på feilmeldingene!

**Score text viser 00:00 selv om Console viser riktig tid?**
→ **VIKTIG:** Sjekk at **Score Text** er assigned i GameOverUI Inspector
→ Velg Canvas → GameOverUI component → Score Text må være "ScoreText (TMP_Text)" - IKKE "None"!
→ Hvis None: Dra ScoreText fra GameOverPanel til Score Text feltet
→ Sjekk Console for "GameOverUI: Score text set to..." når spilleren dør
→ Hvis du ser "Score Text is null!" - Score Text er ikke assigned!
→ **Sjekk at du brukte "Text - TextMeshPro" (ikke gammeldags "Text")**

**Restart button fungerer ikke?**
→ Sjekk at Restart Button er assigned i GameOverUI
→ Sjekk at Button har "Button" component
→ Sjekk Console for "GameOverUI: Restarting game..."
→ Sjekk at EventSystem finnes i scenen

**Spillet pauses ikke?**
→ Sjekk at "Pause Game On Death" er ✓ i GameOverUI
→ Note: Kanoner vil fortsatt bevege seg visuelt (Unity limitation)

**Restart laster ikke scenen?**
→ Sjekk at scenen er **SAVED** (File → Save)
→ Sjekk at scenen er i **Build Settings** (File → Build Settings → Add Open Scenes)

**Cursor vises ikke?**
→ Sjekk PlayerController - den kan låse cursor
→ GameOverUI setter `Cursor.visible = true` automatisk

**Tid fortsetter å telle etter død?**
→ Dette er fikset - `isDead` flag stopper timer
→ Sjekk at ShowGameOver() faktisk kalles (se Console)

**Kan ikke dra text til Script Inspector feltet?**
→ Sjekk at du har brukt **Text - TextMeshPro** (ikke gammeldags "Text")
→ GameOverUI.cs bruker nå TMP_Text som standard
→ Hvis du har gammeldags Text: Slett og lag ny med TextMeshPro

---

## 💡 Neste Steg

**Grunnleggende (komplett spill):**
1. ✅ Health system (ferdig!)
2. ✅ Game Over screen (ferdig!)
3. ⬜ Main Menu (start screen)
4. ⬜ Wave system (øke vanskelighetsgrad over tid)

**Polish:**
5. ⬜ Sound effects (damage, death, restart)
6. ⬜ Partikkel-effekter (pute eksplosjon)
7. ⬜ Music (bakgrunnsmusikk)
8. ⬜ Power-ups (health pickup, shield, slow-mo)

**Juice:**
9. ⬜ Screen shake ved treff
10. ⬜ Camera zoom ved lav health
11. ⬜ Chromatic aberration ved damage
12. ⬜ Leaderboard (online eller lokal)

---

## 📝 Hierarchy Oversikt

Etter setup skal det se slik ut:

```
Canvas
├── HealthBar (Slider)
│   ├── Background
│   └── Fill Area
│       └── Fill (Image)
├── GameOverPanel (Panel) [DEAKTIVERT]
│   ├── GameOverText (Text)
│   ├── ScoreText (Text)
│   ├── RestartButton (Button)
│   │   └── Text
│   └── QuitButton (Button)
│       └── Text
├── HealthUI (Script)
└── GameOverUI (Script)

EventSystem (må finnes!)
```

---

**Du har nå et komplett spill med health, death og restart! 🎉**

**Test det nå og si fra hvis noe ikke fungerer!** 🚀

