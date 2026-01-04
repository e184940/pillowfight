# 💚 Health System Setup Guide

## ✅ Hva du har fått:

1. **PlayerHealth.cs** - Håndterer damage, invincibility, death
2. **HealthUI.cs** - Viser health bar på skjermen
3. **Pillow.cs** - Oppdatert til å gjøre damage

---

## 🚀 Quick Setup (5 min)

### 1. Legg til PlayerHealth på Player

1. Velg **Player** i Hierarchy
2. Add Component → **PlayerHealth**
3. I Inspector:
   - Max Health: **100**
   - Invincibility Duration: **1**
   - Player Renderer: **La stå tom** (finnes automatisk!)

**Note:** Scriptet finner automatisk Renderer på Character model. Ingen manuell assignment nødvendig!

### 2. Oppdater Pillow Prefab

1. Velg **Pillow prefab** i Assets
2. I Inspector:
   - Damage: **10** (nytt felt!)
   - Push Force: 10
   - Destroy On Hit: ✓

### 3. Lag Health Bar UI (Optional men anbefalt)

**A. Opprett Canvas:**
1. Hierarchy → UI → Canvas
2. Canvas skal automatisk opprettes med EventSystem

**B. Opprett Health Slider:**
1. Høyreklikk på Canvas → UI → Slider
2. Navn: **HealthBar**
3. I Inspector på HealthBar:
   - Anchor Preset: **Top Left** (hold ALT+SHIFT og klikk)
   - Pos X: 100, Pos Y: -30
   - Width: 200, Height: 30

**C. Style Health Bar:**
1. Ekspander HealthBar i Hierarchy:
   ```
   HealthBar
   ├── Background
   ├── Fill Area
   │   └── Fill
   └── Handle Slide Area (kan slettes)
   ```
2. Slett **Handle Slide Area** (vi trenger ikke slider-håndtak)
3. Velg **Fill** → Inspector → Color: **Grønn** (#00FF00)
4. Velg **Background** → Color: **Mørk grå** (#404040)

**D. Legg til HealthUI Script:**
1. Velg **Canvas** i Hierarchy
2. Add Component → **HealthUI**
3. **VIKTIG - Assign alle referanser i Inspector:**
   
   **Player Health:**
   - Dra **Player** GameObject fra Hierarchy til dette feltet
   
   **Health Slider:**
   - Dra **HealthBar** GameObject fra Hierarchy til dette feltet
   
   **Fill Image:**
   - Ekspander HealthBar i Hierarchy
   - Dra **Fill** (under HealthBar → Fill Area → Fill) til dette feltet
   
   **Farger (optional):**
   - Healthy Color: Grønn (#00FF00)
   - Low Health Color: Rød (#FF0000)
   - Low Health Threshold: 0.3 (30%)

4. **Verifiser i Inspector:**
   - Player Health: Skal vise "Player (PlayerHealth)"
   - Health Slider: Skal vise "HealthBar (Slider)"
   - Fill Image: Skal vise "Fill (Image)"
   - **INGEN skal si "None" eller "Missing"!**

---

## 🧪 Testing

### Test 1: Damage System
1. Trykk Play
2. La en pute treffe spilleren
3. **Forventet:**
   - Console: "Pillow hit player! Dealt 10 damage."
   - Console: "Player took 10 damage! Health: 90/100"
   - Health bar går ned
   - Spilleren blinker i 1 sekund

### Test 2: Invincibility
1. La 2 puter treffe raskt etter hverandre
2. **Forventet:**
   - Første pute: Gjør damage
   - Andre pute (innen 1 sek): Gjør IKKE damage
   - Console: "Player is invincible - damage ignored"

### Test 3: Death
1. La 10+ puter treffe spilleren (100+ damage)
2. **Forventet:**
   - Console: "Player died!"
   - Spilleren kan ikke bevege seg lenger
   - Health bar på 0

---

## ⚙️ Justeringer

### Vanskelighetsgrad:

**Lettere:**
- Max Health: 200
- Pillow Damage: 5
- Invincibility Duration: 2

**Vanskeligere:**
- Max Health: 50
- Pillow Damage: 20
- Invincibility Duration: 0.5

**Insane Mode:**
- Max Health: 10
- Pillow Damage: 10
- Invincibility Duration: 0 (ingen immunity!)

### Flere Pillow-typer:

**Soft Pillow (lett):**
- Damage: 5
- Push Force: 5
- Color: Lys blå

**Heavy Pillow (hard):**
- Damage: 25
- Push Force: 20
- Color: Mørk rød

---

## 🎨 Forbedringer (Optional)

### 1. Health Text (vis tall)

1. Høyreklikk på Canvas → UI → **Text - TextMeshPro**
2. Navn: **HealthText**
3. Position: Like ved health bar
4. Text: "100 / 100"
5. I HealthUI script → Health Text: Dra HealthText hit

**Note:** HealthUI.cs bruker nå TMP_Text (TextMeshPro) som standard!

### 2. Damage Numbers (floating damage)

Lag script som spawner tekst når spilleren tar damage:
```csharp
// DamageNumber.cs
public class DamageNumber : MonoBehaviour
{
    public float lifetime = 1f;
    public float floatSpeed = 2f;
    
    void Start()
    {
        Destroy(gameObject, lifetime);
    }
    
    void Update()
    {
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;
    }
}
```

### 3. Low Health Warning

```csharp
// I HealthUI.cs - legg til:
public Image warningOverlay;

void UpdateHealthUI(int currentHealth, int maxHealth)
{
    // ...existing code...
    
    if (warningOverlay != null)
    {
        float alpha = healthPercentage <= 0.2f ? 0.3f : 0f;
        warningOverlay.color = new Color(1, 0, 0, alpha);
    }
}
```

### 4. Sound Effects

```csharp
// I PlayerHealth.cs - legg til:
public AudioClip damageSound;
public AudioClip deathSound;
private AudioSource audioSource;

void Start()
{
    audioSource = gameObject.AddComponent<AudioSource>();
}

public void TakeDamage(int damage)
{
    // ...existing code...
    if (audioSource != null && damageSound != null)
        audioSource.PlayOneShot(damageSound);
}
```

---

## 🐛 Vanlige Problemer

**Puter gjør ikke damage?**
→ Sjekk at Player har tag "Player"
→ Sjekk at PlayerHealth script er på Player
→ Sjekk Console for error messages

**Health bar vises ikke?**
→ Sjekk at Canvas har Canvas component
→ Sjekk at EventSystem finnes i scenen
→ Sjekk at HealthUI har alle referanser assigned

**Health bar går ikke ned når spilleren blir truffet?**
→ **VIKTIG:** Sjekk at disse er assigned i HealthUI Inspector:
   1. **Player Health:** Må være dradd fra Hierarchy (Player GameObject)
   2. **Health Slider:** Må være dradd fra Hierarchy (HealthBar GameObject)
   3. **Fill Image:** Må være dradd fra Hierarchy (Fill GameObject under HealthBar)
→ Sjekk Console for "PlayerHealth: Auto-found renderer on [navn]"
→ Sjekk Console for "Player took X damage!" - hvis dette vises, er health-systemet OK
→ Trykk Play og sjekk at HealthUI.Start() kjører (ingen errors i Console)
→ Prøv å manuelt endre Player Health i Inspector mens Play mode - bar skal oppdateres

**Hvordan sjekke om HealthUI fungerer:**
1. Trykk Play
2. Velg Canvas i Hierarchy
3. I Inspector, se på HealthUI script
4. Hvis Player Health/Health Slider/Fill Image er "None" eller "Missing" - dra de på nytt!
5. Console skal vise: "Player took 10 damage! Health: 90/100" når truffet

**Spilleren blinker ikke ved damage?**
→ Sjekk Console for "PlayerHealth: Auto-found renderer on [navn]"
→ Hvis "No renderer found" - Character model må ha Renderer component (SkinnedMeshRenderer eller MeshRenderer)
→ Du kan manuelt dra Renderer fra Character model til Player Renderer slot hvis auto-find ikke fungerer

**Game fortsetter etter død?**
→ Dette er normalt - vi har ikke laget Game Over screen enda
→ PlayerController.enabled = false stopper bevegelse

---

## 💡 Neste Steg

**Grunnleggende:**
1. ✅ Health system (ferdig!)
2. ⬜ Game Over screen med restart knapp
3. ⬜ Score system (tid overlevd)
4. ⬜ Wave system (mer og mer kanoner)

**Polish:**
5. ⬜ Main menu
6. ⬜ Sound effects
7. ⬜ Partikkel-effekter
8. ⬜ Power-ups (health pickup, shield)

---

**Health systemet er klart! Test det nå og juster damage/health etter behov.** 💚✨

