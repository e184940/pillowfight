# Unity Setup Instructions - Fikse Collision Problemer

## Problem: Spilleren går gjennom platformer

### 1. Player GameObject Setup

I Unity Editor, velg Player GameObjectet og sjekk disse innstillingene:

#### Rigidbody Component:
- ✅ Mass: 1
- ✅ Drag: 0
- ✅ Angular Drag: 0.05
- ✅ Use Gravity: ✓ (checked)
- ✅ Is Kinematic: ☐ (unchecked)
- ✅ Interpolate: Interpolate
- ✅ Collision Detection: Continuous Dynamic
- ✅ Constraints: Freeze Rotation X, Y, Z (alle checked)

#### Capsule Collider Component:
- ✅ Center: (0, 0, 0)
- ✅ Radius: 0.5
- ✅ Height: 2
- ✅ Direction: Y-Axis
- ✅ Is Trigger: ☐ (unchecked)

### 2. Platform/Ground Setup

For ALLE platformer og gulv-objekter:

#### Box Collider (eller annen collider):
- ✅ Is Trigger: ☐ (unchecked)
- ✅ Material: Ingen (eller Physics Material med Friction = 0.4)

#### Layer:
- ✅ Sett Layer til "Ground" (Layer 3)

#### Rigidbody (hvis platformen skal være statisk):
- ✅ IKKE ha Rigidbody, ELLER
- ✅ Ha Rigidbody med "Is Kinematic" checked

### 3. Layer Collision Matrix

I Unity: Edit → Project Settings → Physics

Sjekk at:
- ✅ "Default" layer kolliderer med "Ground" layer
- ✅ "Ground" layer kolliderer med "Default" layer

### 4. Physics Settings

I Unity: Edit → Project Settings → Physics

Sjekk disse verdiene:
- ✅ Default Contact Offset: 0.01
- ✅ Default Solver Iterations: 6
- ✅ Default Solver Velocity Iterations: 1
- ✅ Bounce Threshold: 2

### 5. Player Controller Script

I Inspector på Player GameObject:
- ✅ Ground Layer: Velg "Ground" layer
- ✅ Max Step Height: 0.3 - 0.4 (juster for mindre/større steg)
- ✅ Speed: 5
- ✅ Jump Force: 10

### 6. Teste Oppsettet

1. Start spillet
2. Beveg deg mot en platform
3. Du skal:
   - ✅ Stoppe når du treffer siden av platformen
   - ✅ Kunne hoppe opp på platformen
   - ✅ Kunne gå oppå platformen uten å falle gjennom
   - ✅ Automatisk "klatre" opp på små kanter (< 0.4 høyde)

## Hvis problemet fortsatt eksisterer:

### Sjekk 1: Fixed Timestep
Edit → Project Settings → Time
- Fixed Timestep: 0.02 (standard)

### Sjekk 2: Hastigheten
Hvis spilleren beveger seg FOR raskt, kan den gå gjennom ting.
- Reduser `speed` til 3-5
- Reduser `jumpForce` til 8-10

### Sjekk 3: Platform Thickness
- Platformene må være minst 0.1 units tykke
- Ikke bruk 2D planes for 3D platforming

### Sjekk 4: Collider størrelse
- Player Capsule må ha radius > 0.3
- Player Capsule må ha height > 1.5

## Debugging Tips

### Se Console Logger
Når du spiller, åpne Console (Window → General → Console) og se:
- "Landed on: [navn] (Layer: 3)" - når spilleren lander på en platform
- "Left ground - now airborne" - når spilleren forlater bakken

Hvis du ser "Landed on: [navn] (Layer: 0)" betyr det at platformen har feil layer!

### Se Raycast Linjer i Scene View
Mens du spiller, ha Scene view åpen (ikke bare Game view):
- **Grønn linje** = På bakken, ground detection fungerer ✓
- **Rød linje** = I luften, ground detection fungerer ikke ✗
- **Gul linje** = Faktisk punkt hvor raycast traff

Hvis linjen er rød mens du står på en platform, betyr det:
1. Platformen har feil Layer (ikke Layer 3 "Ground")
2. Platformen mangler collider
3. Collider er satt til "Is Trigger" (skal være unchecked)

### Fikse Platformer Som Ikke Fungerer

1. **Velg platformen i Hierarchy**
2. **I Inspector, sjekk:**
   - Layer: MÅ være "Ground" (ikke "Default")
   - Box Collider (eller annen collider): MÅ eksistere
   - Is Trigger: MÅ være unchecked (☐)
3. **Hvis Layer ikke vises som "Ground":**
   - Klikk på Layer dropdown (øverst i Inspector)
   - Velg "Ground"

### Fikse ALLE Platformer på En Gang

1. I Hierarchy, hold Cmd/Ctrl og klikk alle platformer
2. Høyreklikk → Layer → Ground
3. Dette setter alle valgte objekter til Ground layer

Legg til denne koden i Update() for å se raycasts:
```csharp
void Update()
{
    // ...existing code...
    
    // Debug: Visualiser ground check
    CapsuleCollider capsule = GetComponent<CapsuleCollider>();
    float rayDistance = (capsule.height / 2f) + 0.05f;
    Debug.DrawRay(transform.position, Vector3.down * rayDistance, 
        isGrounded ? Color.green : Color.red);
}
```

I Unity Scene view, aktivér "Gizmos" for å se raycast-linjen.

