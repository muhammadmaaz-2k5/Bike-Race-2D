# Bike-Race-2D

**Dirt Dash** is an original 2D physics-based motorbike time-trial racing game built for mobile (Android/iOS) and desktop in Unity.

## Key Features
- **2D Physics Bike Simulation**: Driven by `Rigidbody2D`, `WheelJoint2D` suspension, back-wheel motor drive, natural torque-reaction wheelies, and mid-air leaning/flips.
- **2.5D Rigged Visuals**: Rigged 3D biker models with animated armature mounted to 2D physics colliders and wheel joints with real-time wheel axle rotation sync.
- **Track System**: Point-based track generation using `EdgeCollider2D` and custom `LineRenderer` with friction physics materials.
- **Time-Trial & Star Rating**: 1 to 3 stars based on level completion times, crash detection (head impact & void kill boundary) with auto-restart.
- **Camera System**: Dynamic follow camera with forward velocity look-ahead, speed-based dynamic zoom, and vertical clamping.
- **Cross-Platform Controls**:
  - **Mobile**: Touch screen right (accelerate), left (brake/reverse), and accelerometer device tilt (mid-air rotation) or on-screen buttons.
  - **Desktop / Editor**: `D` / `Right Arrow` (accelerate), `A` / `Left Arrow` (brake/reverse), `W`/`S` (lean/flips), `R` (restart), `Z` (camera zoom).

## Project Structure
- `Assets/Bikers/`: Rigged models, textures, materials, and prefabs.
- `Assets/_Game/`:
  - `Prefabs/`: Physics bike prefabs, UI, and track components.
  - `Scenes/`: Boot, MainMenu, WorldSelect, LevelSelect, Game, TestTrack.
  - `ScriptableObjects/`: 24 level datasets and customizable BikeStats configurations.
  - `Scripts/`: Core game architecture (`GameManager`, `EventBus`, `SaveSystem`, `BikeController`, `CameraFollow`, `TrackBuilder`, `UI`).

## Quick Start
1. Open the project in Unity 2022 LTS or Unity 6.
2. Open `Assets/_Game/Scenes/TestTrack.unity` to test bike physics and track riding.
3. Open `Assets/_Game/Scenes/Boot.unity` to launch the complete game flow.
