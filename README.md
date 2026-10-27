# Stratego Online

**An unofficial, two-player, turn-based strategy game prototype built with Unity and Netcode for GameObjects.**

Stratego Online explores how to translate a hidden-information board game into a networked Unity experience. Two players deploy pieces on opposite sides of a procedurally generated board, take turns moving, and resolve battles according to piece ranks and special-case rules.

This is a **personal learning and portfolio project**, not an official Stratego release or a production-ready multiplayer service.

## What the prototype implements

- **Two-player host/client session flow:** a menu with host/client controls and a two-player requirement to transition to the game scene.
- **Networked game setup:** initialization and ready-state coordination using Netcode for GameObjects RPCs.
- **Configurable board generation:** a 10 × 10 grid with impassable lakes, tile highlighting, and player-specific camera viewpoints.
- **Pre-game deployment:** manual or randomized placement of piece sets within each player's starting area, with per-piece inventory counts.
- **Hidden information:** opponent pieces use a concealed mesh until revealed for battle; pieces are rendered differently for their owner.
- **Turn-based moves:** tile selection, move highlighting, turn switching, and different movement rules for ordinary pieces and Scouts. Bombs and Flags cannot move.
- **Rank-based combat:** piece comparisons and battle transitions, including branches for bombs, flag capture, and special piece interactions.

The project is inspired by the classic **Stratego** board game; its rules and match-completion behaviour should not be taken as a fully verified implementation of every official rule.

## Engineering highlights

### RPC-driven multiplayer flow

Gameplay coordination uses **Unity Netcode for GameObjects (NGO)**, including `NetworkBehaviour`, `ServerRpc`, and `ClientRpc`.

```text
Main menu
    |
    +-- Host / Client -> NetworkManager
    |
    v
Two clients connected -> synchronized scene transition
    |
    v
Bootstrap -> initialize boards and UI for each player
    |
    v
Pre-game piece placement -> server RPC -> client updates
    |
    v
Both players ready -> start turn-based game
    |
    v
Tile input -> move/battle RPCs -> visual updates
```

The [multiplayer manager](Stratego%20Online/Assets/Project/Scripts/MultiplayerManager.cs) handles connection callbacks and game-scene transition. [`Bootstrap`](Stratego%20Online/Assets/Project/Scripts/Bootstrap.cs) coordinates game initialization and player readiness. [`GameManager`](Stratego%20Online/Assets/Project/Scripts/Managers/GameManager.cs) handles turn gating, moves, and combat flows.

**Scope note:** Multiplayer/Lobby-related Unity packages are listed in the manifest, but the game's own scripts use direct NGO host/client startup. A complete Unity Lobby/Relay matchmaking experience is **not** claimed here.

### Configurable grid and piece rules

[`ConfigManager`](Stratego%20Online/Assets/Project/Scripts/Managers/ConfigManager.cs) and [`PieceData`](Stratego%20Online/Assets/Project/Scripts/PieceData.cs) are `ScriptableObject` assets holding board dimensions, tile appearances, and the available piece prefabs, ranks, counts, and icons.

The [board generator](Stratego%20Online/Assets/Project/Scripts/BoardGenerator.cs) creates board tiles, assigns lake cells and materials, and records grid positions.

Piece movement is implemented through an inheritance structure:
- [`Piece`](Stratego%20Online/Assets/Project/Scripts/Pieces/Piece.cs) defines shared presentation and piece behaviour.
- [`SingleStepPiece`](Stratego%20Online/Assets/Project/Scripts/Pieces/SingleStepPiece.cs) covers orthogonal one-cell moves for ordinary movable pieces.
- [`Scout`](Stratego%20Online/Assets/Project/Scripts/Pieces/Scout.cs) implements movement across multiple clear cells in a straight line.
- [`Bomb`](Stratego%20Online/Assets/Project/Scripts/Pieces/Bomb.cs) and [`Flag`](Stratego%20Online/Assets/Project/Scripts/Pieces/Flag.cs) have no legal movement options.

[`Tile`](Stratego%20Online/Assets/Project/Scripts/Tile.cs) tracks occupancy and responds to selection while using `MaterialPropertyBlock` for highlighting. [`CameraController`](Stratego%20Online/Assets/Project/Scripts/CameraController.cs) gives each side its own view of the board.

### Player setup and hidden pieces

[`PiecePlacementManager`](Stratego%20Online/Assets/Project/Scripts/Managers/PiecePlacementManager.cs) contains the pre-game placement flow: starting-area checks, piece counts, placement/removal requests, and random deployment. [`UIManager`](Stratego%20Online/Assets/Project/Scripts/UI/UIManager.cs) generates piece-selection controls and tracks when a player's remaining count reaches zero.

Each side sees its own pieces with their actual appearance; opposing pieces are initially presented with a common hidden mesh. [`Piece`](Stratego%20Online/Assets/Project/Scripts/Pieces/Piece.cs) also implements select/deselect visuals and DOTween movement animations.

## Tech stack

| Area | Technology |
| --- | --- |
| Engine | **Unity 6 (6000.0.54f1)** |
| Language | **C#** |
| Networking | **Netcode for GameObjects 2.1.1** |
| Multiplayer testing | **Unity Multiplayer Play Mode 1.6.0** (package included) |
| Rendering | **Universal Render Pipeline 17.0.4** |
| Animation | **DOTween** (bundled plugin) |
| Configuration | **ScriptableObjects** |
| UI | **Unity UI + TextMesh Pro** |

The manifest additionally includes Unity Lobby and Multiplayer Services packages, but their presence alone does not demonstrate production-ready matchmaking or Relay integration.

## Open and run the project

This repository has a **nested Unity project**: open the `Stratego Online/` subfolder, **not** the Git repository root.

1. Install **Unity 6000.0.54f1** through Unity Hub, matching [`ProjectVersion.txt`](Stratego%20Online/ProjectSettings/ProjectVersion.txt).
2. Clone the repository:
   ```bash
   git clone https://github.com/AntoneskuOleksandr/Stratego-Online.git
   ```
3. In Unity Hub, choose **Add project from disk** and select the cloned **`Stratego-Online/Stratego Online`** directory.
4. Let Unity import the assets and resolve packages.
5. Open [`Assets/Project/Scenes/MainMenu.unity`](Stratego%20Online/Assets/Project/Scenes/MainMenu.unity) and enter Play Mode.
6. For a local two-player test, use **two Unity player instances** (for example, the Editor and a standalone build, or a compatible Multiplayer Play Mode configuration). Start one as **Host** and the other as **Client**, with matching network transport settings.
7. When both players are connected, use the menu's start control to enter the **Stratego** scene, deploy pieces, and mark each player ready.

The checked-in [Build Settings](Stratego%20Online/ProjectSettings/EditorBuildSettings.asset) include `MainMenu.unity` followed by `Stratego.unity`.

**Run-status disclaimer:** The instructions above are based on project files and code inspection. They have **not** been validated by a fresh Unity import, successful two-instance multiplayer test, or platform build during this documentation review. A public join-code service and downloadable release are not provided.

## Repository structure

```text
Stratego Online/                      <-- Unity Hub project folder
  Assets/
    Project/
      Scenes/                         MainMenu, Stratego, Test
      Scripts/
        Managers/                     Board, setup, turn and combat flow
        Pieces/                       Piece movement and visual logic
        UI/                           Network menu and setup UI
        BoardGenerator.cs             Tile generation
        Bootstrap.cs                  Networked initialization
        MultiplayerManager.cs         Connection and scene transition
        Tile.cs                        Board-cell state and interactions
      ScriptableObjects/              Board configuration and piece definitions
      Prefabs/                        Tiles, pieces, UI elements
    Plugins/                           DOTween
  Packages/                            Unity package dependencies
  ProjectSettings/                     Unity editor and build settings
```

## Status and limitations

**Status: personal prototype / development paused.**

This repository demonstrates networked turn-based gameplay, piece rules, pre-game deployment, and synchronization work. It is **not** presented as a polished, competitive, security-hardened or fully rules-complete game. Multiplayer edge cases, battle-rule details, disconnect recovery, and end-of-match presentation require additional validation before such claims could be made. At present, the flag-capture code logs the winner rather than displaying a complete post-match result flow.

No dedicated gameplay screenshots or video are currently committed. Visual documentation can be added when verified capture media is available.

## Author and attribution

Developed by **[Oleksandr Antonesku](https://github.com/AntoneskuOleksandr)** as a personal Unity learning and portfolio project.

**Stratego** is a trademark of its respective owner(s). This project is unofficial and not affiliated with or endorsed by the official game's publisher(s). Third-party code and assets remain subject to their respective licensing terms; this repository does not declare a general open-source license.
