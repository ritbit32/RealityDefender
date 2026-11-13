# Reality Defender (AR Target Practice)

A mobile AR target-practice mini-game built in **Unity with AR Foundation** for the BOA XR *XR Game Developer* assignment.
Scan a surface, tap to place the arena, then turn your phone into a cannon and blast the floating energy targets.

**Gameplay video and Shader Graph screenshots:** [Google Drive folder](https://drive.google.com/drive/folders/1p_rHdq3QYuwx5S1fPoVJ5S8Grjk7uZfu?usp=sharing)

An installable **APK** is in the Drive folder (Android, ARCore-supported device required).

---

## Tech
| | |
|---|---|
| Engine | Unity 6 LTS (6000.0.85f2), Universal Render Pipeline |
| AR | AR Foundation + Google ARCore XR Plugin |
| Input | Unity Input System |
| Tested on | Nothing Phone (2a), Android |
| Editor testing | AR Foundation XR Simulation |

---

## Part 1: The Arena (AR interaction)
**Script:** `Assets/Scripts/ARPlacement.cs` (on the XR Origin)

- **Plane detection:** `ARPlaneManager` set to *Horizontal* detection only (floors and tables).
- **Tap to place:** a screen tap is raycast with `ARRaycastManager.Raycast(..., TrackableType.PlaneWithinPolygon)`. Only hits on planes with `PlaneAlignment.HorizontalUp` are accepted, so walls and ceilings are ignored.
- **Surface alignment:** the base's *up* is set to the hit pose's surface normal. Its *forward* is the direction to the player projected onto the surface (`Vector3.ProjectOnPlane` + `Quaternion.LookRotation(forward, up)`), so it sits flat and faces the player without tilting.
- **No floating:** the base prefab's pivot is at the bottom of the platform (the cylinder child is offset by half its height), so it rests exactly on the surface.
- **Anchored and immovable:** an `ARAnchor` is added to the spawned base, and plane detection is then disabled and existing plane visuals hidden, so the arena can't be re-placed or drift.

## Part 2: The Juice (Shader Graph and VFX)
**Asset:** `Assets/Shaders/EnergyTarget.shadergraph` (URP Unlit), material `EnergyTarget_Mat`

The targets use a custom Shader Graph, not a standard material. It combines all three suggested techniques:

1. **Scrolling noise:** `Time × ScrollSpeed` drives the *Offset* of a `Tiling And Offset` node feeding `Simple Noise`, so the procedural noise flows across the surface. It's tinted by an HDR `BaseColor`.
2. **Fresnel rim light:** `Fresnel Effect` (controlled by `RimPower`) × HDR `RimColor`, added on top of the noise for a glowing edge.
3. **Vertex displacement (wobble):** each vertex is pushed along its object-space normal by `sin(positionY × 20 + Time × WobbleSpeed) × WobbleAmount`, so a ripple travels up the sphere like unstable energy.

All values are exposed as material properties (`BaseColor`, `RimColor`, `RimPower`, `NoiseScale`, `ScrollSpeed`, `WobbleAmount`, `WobbleSpeed`) for tuning without editing the graph.

**Bonus, explosion VFX:** the `Explosion` prefab is a one-shot particle burst (cyan/magenta sparks with gravity, fading and shrinking over their lifetime). It uses *Stop Action: Destroy*, so it cleans itself up.

## Part 3: The Game Loop (C#)
| Script | Responsibility |
|---|---|
| `Cannon.cs` | The **FIRE** button spawns a projectile just in front of the AR camera and launches it along the camera's forward vector (`Rigidbody.AddForce`, `ForceMode.VelocityChange`). It has a short fire cooldown. |
| `Projectile.cs` | A physics projectile (`Rigidbody`, gravity off, **Continuous Dynamic** collision detection so the small, fast sphere can't tunnel through targets). It destroys itself on any collision or once it's **more than 5 m** from its origin (plus a lifetime safety net). |
| `GameBase.cs` | Spawns **3 floating targets** around and above the base. They're parented to the anchored base, so they stay locked in the real world. |
| `Target.cs` | Bobs and spins (kinematic `Rigidbody`). On collision with a projectile it spawns the explosion, reports the hit, and destroys itself. |
| `GameManager.cs` | Score (**+10 per hit**), targets remaining, **WAVE CLEARED** state, play again, and quit. |

**UI:** screen-space score counter (top corner); "WAVE CLEARED" when all 3 targets are destroyed.

## Extra polish
- **Center crosshair:** projectiles travel straight from the camera, so the reticle shows exactly where shots will go.
- **Context-aware UI:** a scan hint before placement; FIRE and the crosshair appear after placement and hide on WAVE CLEARED.
- **PLAY AGAIN:** resets the score and spawns a fresh wave on the same anchored base, with no re-scanning needed.
- **EXIT button** and **Android back button / gesture** both quit the app cleanly.

---

## How to run
1. Open the project in **Unity 6000.0.85f2** (or a compatible Unity 6 LTS) with **Android Build Support**.
2. Open `Assets/Scenes/RealityDefender.unity`.
3. **In the editor:** press Play (XR Simulation is enabled for the editor). Hold right mouse + WASD to move, left-click a detected plane to place, and click FIRE.
4. **On device:** File → Build Profiles → Android → Build And Run on an [ARCore-supported phone](https://developers.google.com/ar/devices).
   Or just install `RealityDefender.apk` from the Drive folder (you may need to allow installs from unknown sources).

## How to play
1. Move the phone slowly to scan the floor or a table.
2. Tap a detected surface to place the arena.
3. Aim with the crosshair and tap **FIRE**. Destroy all 3 targets.
4. **PLAY AGAIN** for a new wave, or **EXIT** to quit.
