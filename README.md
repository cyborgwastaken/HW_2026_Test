# Doofus Adventure - By Ayushman Das

A 3D platform-hopping game built for the Hitwicket Game Developer Challenge, made in Unity 6.

Guide **Doofus** across a chain of **Pulpits** — green platforms that count down and disappear — for as long as you can. The challenge: walk on at least 50 of them.

## Demo Video

https://github.com/user-attachments/assets/cea89790-13f2-4598-890a-29bc1a563af3

## Start Screen
<img width="1710" height="1112" alt="Screenshot 2026-08-21 at 1 47 33 AM" src="https://github.com/user-attachments/assets/edba8a41-5b5e-46ab-8392-43b7ca44aa54" />

## Game Over Screen
<img width="1710" height="1112" alt="Screenshot 2026-08-21 at 1 47 51 AM" src="https://github.com/user-attachments/assets/eea1cb44-7ab0-4941-8c59-7051ad33e31f" />

## InGame HUD
<img width="1710" height="1112" alt="Screenshot 2026-08-21 at 1 50 12 AM" src="https://github.com/user-attachments/assets/44d225fd-e043-48c7-b716-2f018293b62e" />



## Controls

| Action | Input |
|---|---|
| Move | `WASD` / Arrow Keys |
| Jump | `Space` |
| Sprint | `Left Shift` |
| Look around | Mouse / trackpad |

## Levels implemented

**Level 1 — Movement & JSON-driven platforms**
Doofus's move speed and each pulpit's spawn/lifetime timing are read live from [`Assets/StreamingAssets/doofus_diary.json`](Assets/StreamingAssets/doofus_diary.json) ("Doofus's Diary") at runtime — not hardcoded — with safe fallback defaults if the file is ever missing or malformed. At most two pulpits are alive at once, each spawned adjacent to the last one, never overlapping and never immediately re-appearing where Doofus just walked from.

**Level 2 — Scoring**
Score increases once per *new* pulpit landed on (the starting pulpit doesn't count). The next pulpit spawns once the current one has burned through a score-scaled fraction of its own randomized lifetime — 40% at the start of a run, ramping up to 70% by score 50 — so the reaction window tightens as the run goes on.

**Level 3 — Start & Game Over screens**
A Start screen gates the run. Falling off ends it, stops any pulpit still spawning in the background, and shows the final score with a Retry option that resets score, pulpits, player position, and camera back to their initial state.

## Edge cases handled

- Config file missing/unreadable/malformed → falls back to sane defaults, game stays playable.
- Pulpit placement never overlaps an existing platform or the one just vacated.
- Landing/scoring is deduplicated per pulpit (walking back onto an already-scored pulpit doesn't re-score it).
- Retry fully resets game state — no leaked timers, spawners, or stale pulpits from the previous run.

## How to run

1. Unity 6+ (built and tested on `6000.5.9f1`).
2. Open the project, then open [`Assets/MilkyWay/Scenes/Default.unity`](Assets/MilkyWay/Scenes/Default.unity) — the active game scene.
3. Press Play, click **Start**.

## Project structure

```
Assets/Scripts/
  Config/        - reads Doofus's Diary JSON, with fallback defaults
  Core/          - game state machine (Start -> Playing -> Game Over) and a small
                   event bus that decouples the systems below from each other
  Player/        - player rig (reset/respawn) and fall detection
  Pulpits/       - pulpit spawning, lifetime, placement, scoring trigger
  Scoring/       - score tracking
  Screens/       - Start / HUD / Game Over UI
  CameraSystem/  - dead-zone follow camera
```

Player movement itself comes from Unity's Starter Assets `ThirdPersonController` (`CharacterController`-based); the scripts above integrate with it without modifying its source.
