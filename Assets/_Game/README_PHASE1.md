# Dirt Dash - Phase 1 Setup & Physics Verification

Welcome to **Dirt Dash**, an original 2D physics-based motorbike time-trial racing game!

---

## 1. Project Scan & Asset Inventory Report

### Biker Assets Scanned (`Assets/Bikers`):
* **3D Rigged Model (`Bikers.fbx`)**: Contains separate rigged mesh nodes for:
  * `moto` (motorcycle chassis and frame)
  * `rull` (handlebars / front fork steering node)
  * `wheel_1` (front wheel mesh)
  * `wheel_2` (rear wheel mesh)
  * `biker` (rider body mesh with rigged armature)
  * `head` (rider head bone, used for crash trigger)
  * Embedded armature animations: `Armature|go`, `Armature|idle`, `Armature|left`, `Armature|right`, `Armature|start`, `Armature|stop`.
* **4 Character Prefabs**:
  * `Bikers.prefab` (Colorway 1 - Red / Black)
  * `Bikers_2.prefab` (Colorway 2 - Blue / White)
  * `Bikers_3.prefab` (Colorway 3 - Green / Black)
  * `Bikers_4.prefab` (Colorway 4 - Yellow / Black)
* **Textures & Materials**:
  * 8 diffuse texture maps: `biker_1_diff.png` .. `biker_4_diff.png`, `moto_1_diff.png` .. `moto_4_diff.png`.
  * 9 materials: `biker_1.mat` .. `biker_4.mat`, `moto_1.mat` .. `moto_4.mat`, `ground.mat`.
* **Note on 2D Slicing vs 2.5D**:
  The imported asset pack uses 3D models with UV texture sheets rather than a flat 2D sprite sheet. We have mounted the 3D model with its separated parts as child visual transforms driven by **2D Rigidbody & WheelJoint2D physics**. This gives realistic 2.5D visual lighting and depth. If you prefer flat 2D sprites later, you can slice a 2D spritesheet in the Sprite Editor and drop it directly onto the visual slots.

---

## 2. Phase 1 Architecture & Created Files

```
Assets/_Game/
├── Art/
│   ├── WheelFriction.physicsMaterial2D    # High-grip tire material (friction 1.35)
│   └── TrackFriction.physicsMaterial2D    # Track surface material (friction 1.0)
├── Prefabs/
│   └── Bikes/
│       └── Bike_DirtRunner.prefab         # Rigged 2D physics bike prefab
├── Scenes/
│   └── TestTrack.unity                    # Flat test track + bump + ramp
├── ScriptableObjects/
│   └── BikeStats/
│       └── Bike_DirtRunner.asset          # Tunable motor, torque, air lean, suspension stats
└── Scripts/
    ├── DirtDash.Runtime.asmdef            # Runtime assembly definition
    ├── Core/
    │   ├── GameConstants.cs               # Game title "Dirt Dash", layers, tags, defaults
    │   ├── EventBus.cs                    # Decoupled strongly typed event system
    │   └── GameManager.cs                 # Race timer and quick level restarts
    ├── Bike/
    │   ├── BikeStats.cs                   # ScriptableObject for bike properties
    │   ├── BikeController.cs              # 2D physics bike controller (joints, motor, lean)
    │   ├── CrashDetector.cs               # Head contact & void fall detection
    │   └── FlipDetector.cs                # Air continuous rotation tracker (flips)
    ├── Camera/
    │   └── CameraFollow.cs                # Smooth follow, look-ahead, speed zoom
    ├── UI/
    │   └── TestHUD.cs                     # Lightweight on-screen telemetry & controls
    └── Editor/
        ├── DirtDash.Editor.asmdef         # Editor assembly definition
        └── Phase1Setup.cs                 # Automated setup tool (Menu: DirtDash -> Setup Phase 1)
```

---

## 3. Editor Setup & Configuration Checklist

1. **Open the Scene**:
   * Double-click `Assets/_Game/Scenes/TestTrack.unity` in the Project window.
2. **Build Settings**:
   * `Assets/_Game/Scenes/TestTrack.unity` is already added to `File -> Build Settings` as scene index 0.
3. **Automated Setup / Rebuild (Optional)**:
   * A custom menu item is available in Unity at **`DirtDash -> Setup Phase 1 Bike & Test Scene`**. You can trigger this at any time to re-generate the prefab or scene.

---

## 4. How to Test Phase 1

1. Open `Assets/_Game/Scenes/TestTrack.unity` in Unity Editor.
2. Press the **Play** button in Unity.
3. **Controls (Keyboard)**:
   * **`D` or `Right Arrow`**: Accelerate forward (smoothed throttle, back wheel drive, notice the wheelie pop when accelerating hard!).
   * **`A` or `Left Arrow`**: Brake (disc brake resistance when moving forward, slow reverse when stopped).
   * **`W` / `Up Arrow`**: Lean backward / pull up.
   * **`S` / `Down Arrow`**: Lean forward / push down.
   * **`R`**: Quick restart level.
   * **`Z`**: Toggle camera close / far view.
4. **Gameplay Verification**:
   * Drive along the flat launch zone.
   * Ride up the ramp at X = 55–65 to jump into the air.
   * While airborne, hold **`W` or `A`** to rotate and complete a **Backflip**, or hold **`S` or `D`** to do a **Frontflip**. Check the banner popup `+BACKFLIP!` or `+FRONTFLIP!`.
   * Try landing upside-down on the rider's head to test crash detection. The screen will notify `CRASH!` and auto-restart in 1 second.
