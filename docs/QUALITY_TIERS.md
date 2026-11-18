# QUALITY TIERS SPECIFICATION — KING WORLD WAR

This document details the configuration, device heuristics, and profiling considerations for the three visual quality tiers in KING WORLD WAR.

---

## 1. QUALITY TIERS OVERVIEW

To deliver fluid performance across the fragmented Android and iOS hardware landscape, KING WORLD WAR defines three standardized quality tiers managed at runtime by `KingWorldWar.Infrastructure.Quality.QualityService`.

| Parameter | Low Tier (Tier 0) | Medium Tier (Tier 1) | High Tier (Tier 2) |
|---|---|---|---|
| **Target Frame Rate** | 30 FPS | 60 FPS | 60 FPS |
| **Render Scale** | 0.80x (or FSR balanced) | 1.00x Native | 1.00x Native |
| **Shadow Distance** | 25 meters | 50 meters | 80 meters |
| **Shadow Quality** | Hard shadows (512 map) | Soft shadows (1024 map) | High-res soft (2048 map) |
| **MSAA Anti-Aliasing** | Disabled (1x) | 2x MSAA | 4x MSAA |
| **HDR** | Disabled (8-bit buffer) | Enabled (FP16) | Enabled (FP16) |
| **SRP Batcher** | Enabled | Enabled | Enabled |
| **Recommended Devices** | 2–3 GB RAM, Quad-core | 4–6 GB RAM, Octa-core | 6–12+ GB RAM, Flagship SoC |
| **Examples (Android)** | Mali-G52 / Adreno 610 | Mali-G77 / Adreno 640 | Adreno 730+ / Dimensity 9000+ |
| **Examples (iOS)** | iPhone 8 / SE 2020 | iPhone 11 / 12 | iPhone 13 Pro / 14 / 15 / iPad Pro |

---

## 2. AUTOMATIC HARDWARE HEURISTICS

When no manual override is stored in `PlayerPrefs` (`kww_quality_tier`), `QualityService.EvaluateOptimalTier()` selects the initial tier based on `SystemInfo`:

1. **Low Tier Conditions (Any true):**
   - System RAM < 3000 MB.
   - Graphics memory < 1024 MB.
   - CPU processor count <= 4 cores.
2. **High Tier Conditions (All true):**
   - System RAM >= 6000 MB.
   - Graphics memory >= 2048 MB (or unified memory).
   - CPU processor count >= 8 cores.
3. **Medium Tier Conditions:**
   - All intermediate devices (4GB–6GB RAM with modern mid-range GPU).

---

## 3. EDITOR PROFILING OBSERVATIONS

> [!WARNING]
> **Important:** The measurements below were recorded inside the Unity Editor on a macOS host. Editor profiling reflects development overhead and is **NOT device-verified**. Actual mobile performance requires on-device profiling.

- **Scene:** `Assets/KINGWorldWar/Scenes/VisualTest.unity`
- **Objects Rendered:** 1 Ground terrain (120x120m), 5 Complex modular building prefabs, 2 Unit prefabs (Tank & Aircraft), 1 StrategyCameraRig, 1 Debug Canvas.
- **Draw Calls (SetPass Calls):** ~6–9 calls with SRP Batcher active.
- **Batched Meshes:** 100% of static placeholder primitives grouped into SRP batches.
- **Frame Rate (macOS Editor):** Stable 60+ FPS (unlocked vsync).
- **Camera Bounds & Damping:** Zero GC allocations during continuous panning or zoom damping.

---

## 4. OWNER ON-DEVICE PROFILING CHECKLIST

To verify performance on real mobile hardware before production release:

1. **Android (via ADB & Unity Profiler):**
   - Connect target device via USB with USB debugging enabled.
   - Build Development APK with *Autoconnect Profiler* enabled.
   - Open *Unity Profiler* (`Window > Analysis > Profiler`).
   - Monitor:
     - **CPU Usage:** Main thread frame time must stay <= 16.6ms (for 60 FPS) or <= 33.3ms (for 30 FPS).
     - **GC Allocations:** 0 B per frame during camera drag / zoom.
     - **GPU Time / Thermal Throttling:** Observe after 10 minutes of active camera movement.
2. **iOS (via Xcode & Metal System Trace):**
   - Build Xcode project from Unity.
   - Run Metal System Trace instrument in Instruments.
   - Verify:
     - Tiler/Renderer utilization < 75%.
     - Energy impact stays in "Low" or "Moderate" band.
