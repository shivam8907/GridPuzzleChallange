# Grid Puzzle Challenge

A grid-based number sliding puzzle developed in **Unity** as part of a Game Developer Engineering Assignment. The objective is to arrange the shuffled numbered tiles into the correct order using swipe gestures within the available moves. The project emphasizes clean architecture, separation of concerns, efficient state management, and maintainable code.

---

## Gameplay

* The puzzle consists of an **N × M grid** with one empty space.
* Swipe **Up**, **Down**, **Left**, or **Right** to slide adjacent numbered tiles into the empty space.
* Arrange all numbers in ascending order to complete the puzzle.
* Complete the puzzle before reaching the move limit (if enabled).

---

## Features

* N × M Grid-Based Puzzle
* Swipe Gesture Input
* Solvable Board Generation
* Move Counter
* Score System
* Combo Score System
* Undo Functionality (Limited Uses)
* Peek/Hint Power-Up (Limited Uses)
* Win Panel
* Game Over Panel
* Tile Movement Animation
* UI Sound Effects
* Modular and Scalable Architecture

---

# Project Architecture

The project follows a modular architecture where gameplay logic is completely separated from the presentation layer.

```
Player Swipe
      │
      ▼
SwipeInputController
      │
      ▼
GameManager
      │
      ▼
GridModel
      │
      ├──────────────┐
      ▼              ▼
HistoryManager   ComboScoreSystem
      │              │
      └──────────────┘
             │
             ▼
UIManager
      │
      ▼
TileView
```

---

# Core Components

## GameManager

Acts as the central controller.

Responsibilities:

* Starts a new game
* Receives swipe input
* Coordinates gameplay systems
* Updates the UI
* Handles Win and Game Over states

---

## GridModel

The core gameplay model.

Responsibilities:

* Stores puzzle data
* Validates tile movement
* Swaps tiles
* Tracks move count
* Checks puzzle completion

The grid data is independent from the UI, ensuring clean separation between game logic and rendering.

---

## SwipeInputController

Responsible only for player input.

* Detects swipe gestures
* Converts gestures into movement directions
* Sends movement events to GameManager

---

## HistoryManager

Implements deterministic state history.

Responsibilities:

* Saves board state before each move
* Restores previous board state
* Manages limited Undo usage

---

## ComboScoreSystem

Tracks gameplay performance.

Responsibilities:

* Calculates score
* Maintains combo streak
* Updates score after successful moves

---

## PeekPowerUp

Provides a hint system.

Responsibilities:

* Highlights a correct movable tile
* Limits the number of peek usages
* Notifies the UI

---

## UIManager

Responsible only for presentation.

Responsibilities:

* Renders puzzle tiles
* Updates HUD
* Displays score and move count
* Updates Undo and Peek counters
* Shows Win and Game Over panels
* Plays UI sound effects
* Animates tile movement

---

# Game Flow

```
Player Swipe
      │
      ▼
SwipeInputController
      │
      ▼
GameManager
      │
      ▼
GridModel
      │
      ▼
Validate Move
      │
      ▼
Update Board State
      │
      ▼
History Manager
      │
      ▼
Score System
      │
      ▼
UI Manager
      │
      ▼
Win / Game Over Check
```

---

# Win Condition

The player wins when every numbered tile is arranged in ascending order and the empty tile reaches its correct final position.

---

# Game Over Condition

If a move limit is enabled, the game ends when the player exhausts all available moves before solving the puzzle.

---

# Engineering Principles

* Separation of Game Logic and UI
* Single Responsibility Principle (SRP)
* Event-Driven Communication
* Modular Component Design
* Maintainable and Scalable Codebase
* Deterministic State Management
* Clean Git Commit History using Conventional Commits

---

# Technologies

* Unity
* C#
* TextMeshPro
* Unity UI
* Git & GitHub

---

# Assignment Requirements Covered

| Requirement                            | Status |
| -------------------------------------- | ------ |
| N × M Grid Simulation                  | ✅      |
| Swipe-Based Input                      | ✅      |
| Independent Grid Model                 | ✅      |
| Undo / State History                   | ✅      |
| Move Counter                           | ✅      |
| Score System                           | ✅      |
| Win Condition                          | ✅      |
| Game Over State                        | ✅      |
| Extra Gameplay Feature (Peek Power-Up) | ✅      |
| Modular Architecture                   | ✅      |
| Git Version Control                    | ✅      |

---

## Future Improvements

* Multiple difficulty levels
* Timer mode
* Leaderboard
* Save & Resume
* Multiple puzzle sizes (3×3, 4×4, 5×5)
* Theme customization
* Additional power-ups
