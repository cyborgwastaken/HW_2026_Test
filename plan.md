# Doofus Adventure — Implementation Plan

Source: `Game Developer - VIT Assignment 2026.docx` (Hitwicket Game Developer Challenge)
Engine: Unity 6 (6000.5.9f1 detected in this project), URP 3D template, C#.

## Config values (from Doofus Diary JSON)

Fetched from `https://s3.ap-south-1.amazonaws.com/superstars.assetbundles.testbuild/doofus_game/doofus_diary.json`:

```json
{
  "player_data": { "speed": 3 },
  "pulpit_data": {
    "min_pulpit_destroy_time": 4,
    "max_pulpit_destroy_time": 5,
    "pulpit_spawn_time": 2.5
  }
}
```

- `speed` = Doofus movement speed (units/sec).
- `pulpit_spawn_time` ("x") = seconds after a pulpit appears before the *next* pulpit spawns.
- `pulpit_destroy_time` = each pulpit's lifetime, randomized per-pulpit between `min` (4s, "y") and `max` (5s, "z").
- Since destroy time (4–5s) > spawn time (2.5s), there's always a ~1.5–2.5s overlap window where two pulpits coexist — this is what makes "only two pulpits exist simultaneously" meaningful.

The JSON will be copied into `Assets/StreamingAssets/doofus_diary.json` and read at runtime via `UnityWebRequest` (the only cross-platform-safe way to read StreamingAssets, including WebGL/mobile). If the fetch or parse fails, fall back to hardcoded defaults matching the values above and log a warning — game must never hard-crash on config load.

## Architecture

```
Assets/
  StreamingAssets/
    doofus_diary.json
  Scripts/
    Config/
      GameConfig.cs            // serializable POCOs matching JSON schema
      GameConfigLoader.cs      // singleton; async load + fallback defaults
    Player/
      DoofusController.cs      // WASD/arrow movement, applies speed from config
      DoofusFallDetector.cs    // detects falling below world -> raises GameOver
    Pulpit/
      Pulpit.cs                // per-pulpit lifetime timer, destroy/despawn behavior
      PulpitSpawner.cs         // spawn scheduling, max-2 rule, adjacent placement
      PulpitGrid.cs            // helper for adjacent-cell placement math (9x9 cells)
    Scoring/
      ScoreManager.cs          // increments on first successful landing per pulpit
    Core/
      GameManager.cs           // state machine: StartScreen -> Playing -> GameOver
      GameEvents.cs            // static C# events decoupling systems (no hard refs)
    UI/
      StartScreenUI.cs
      GameplayHUD.cs           // live score display
      GameOverUI.cs            // final score + restart
  Prefabs/
    Doofus.prefab
    Pulpit.prefab
  Scenes/
    SampleScene.unity          // reuse existing scene as the game scene
```

Systems talk through `GameEvents` (C# `static event Action<...>`) rather than direct references, so e.g. `ScoreManager` doesn't need to know about `GameplayHUD` and `Pulpit` doesn't need to know about `GameManager`. Keeps each script single-responsibility and testable in isolation — matches the "modular and robust code" evaluation criterion.

## Level 1 — Movement + JSON-driven Pulpit placement

- `GameConfigLoader` loads `doofus_diary.json` on boot before gameplay starts (loading indicator or just gate Start button on it).
- `DoofusController`: WASD + arrow keys move Doofus on the XZ plane at `config.speed`; no rotation-locking issues, simple `Transform.Translate`/`Rigidbody.MovePosition`.
- `Pulpit` prefab: 9x9 green metallic platform (scaled default cube/plane + emissive-ish metallic material).
- `PulpitSpawner`: spawns the first 1–2 pulpits at game start; enforces "max 2 pulpits alive at once"; each subsequent spawn:
  - Timer-driven by `pulpit_spawn_time` (2.5s) relative to the previous spawn.
  - Placement: random adjacent cell (N/E/S/W of previous pulpit's grid position, touching edges since platforms are 9x9 and there's no jump input) that is not the same cell as the current/previous pulpit.
  - Guard: never exceeds 2 active pulpits even if timers race (spawner checks active count before instantiating).
- Fall detection: `DoofusFallDetector` watches Y position / uses a trigger volume below the play area; if Doofus falls off an edge (no ground under him, or a pulpit despawns under his feet) → raises `GameEvents.OnPlayerFell` → game over.

## Level 2 — Scoring

- `ScoreManager` subscribes to a "landed on pulpit" event, raised by `Pulpit` via a ground-contact check (trigger/collision from Doofus, or overlap check) — dedup so re-touching the same pulpit doesn't double count.
- Score increments by 1 only the *first* time Doofus successfully stands on a given (newly spawned) pulpit.
- `GameplayHUD` listens to `GameEvents.OnScoreChanged` and updates a live score counter.

## Level 3 — Start Screen & Game Over Screen

- `GameManager` state machine: `StartScreen → Playing → GameOver`, driven by `GameEvents`.
- Start Screen: title, "Start" button (loads/reloads gameplay state, hides menu, spawns Doofus + first pulpits).
- Game Over Screen: triggered by `OnPlayerFell`; shows final score, "Retry" button that resets all state (destroy pulpits, respawn Doofus, reset score, restart spawner timers) without needing a full scene reload (cleaner, but scene reload via `SceneManager.LoadScene` is the safe fallback if reset logic gets fragile).

## Edge cases / exceptions to handle gracefully

- JSON fetch/parse failure → fallback defaults, `Debug.LogWarning`, game still playable.
- Race condition in pulpit spawn timing never exceeding the 2-pulpit cap.
- Double-scoring prevented via per-pulpit "already scored" flag.
- Player standing on a pulpit exactly as its timer expires → must fall (pulpit despawn should physically drop or disable collider so Doofus falls through, not float).
- Restart from Game Over must fully clear old pulpits/timers/coroutines (no leaked timers scoring or spawning after reset).
- Placement logic must never place a new pulpit overlapping an existing one.

## Git workflow

- Commit after each level is completed (per assignment instructions), plus at least hourly during active work.
- Suggested commit checkpoints:
  1. Project scaffold + config loader + JSON in StreamingAssets
  2. Level 1 complete (movement + spawner + placement + fall detection)
  3. Level 2 complete (scoring)
  4. Level 3 complete (start/game-over UI)
  5. Polish pass (materials, camera, screenshots/video of gameplay for repo)
- Final repo must include gameplay screenshots/video before submission, per the assignment's submission guidelines.

## Open items to confirm before/while building

- Exact adjacency distance for new pulpits (touching edges vs. small gap) — starting with touching-edge placement since there's no jump control; revisit if playtesting feels off.
- Visual style for Doofus/Pulpit — assignment explicitly says "no specific UI/UX requirements... surprise us," so simple primitives + a clean color/material pass is enough; not over-investing in art.
