# 🌲 Multiplayer Resource Gathering & Co-op Game

<div align="center">

## 3D Multiplayer Co-op Game

A top-down 3D cooperative resource gathering game built with **Unity 6**, **Netcode for GameObjects (NGO)**, **Universal Render Pipeline (URP)**, and the **New Input System**.

![Unity](https://img.shields.io/badge/Unity-6000.3.24f1-black?logo=unity)
![Netcode](https://img.shields.io/badge/Netcode%20for%20GameObjects-2.13.3-blue)
![URP](https://img.shields.io/badge/URP-17.3.0-purple)
![C#](https://img.shields.io/badge/Language-C%23-239120?logo=csharp)
![Input System](https://img.shields.io/badge/Unity-New%20Input%20System-orange)
![UI Toolkit](https://img.shields.io/badge/UI-UI%20Toolkit-green)

</div>

---

# Project Overview

**Multiplayer Resource Gathering & Co-op Game** is a real-time cooperative multiplayer game developed in Unity 6 using a **server-authoritative networking model** with Unity Netcode for GameObjects.

Players can join as a **Host** or **Client**, move around the map, interact with tools such as an Axe and Pickaxe, gather resources, and cooperate with other players.

The project focuses on multiplayer fundamentals such as:

- Network player spawning and despawning
- Server-authoritative interactions
- RPC communication
- NetworkVariable synchronization
- Player ownership
- Tool pickup and drop
- Resource gathering
- Multiplayer movement and animation
- Local camera and UI handling

---

# Core Features

## Multiplayer Networking

- Host and Client connection using **Unity Netcode for GameObjects**
- Network player spawning
- Server-side validation for gameplay interactions
- NetworkObject lifecycle handling
- Multiplayer Play Mode support for local multiplayer testing

## Player Movement & Input

- Character movement using `CharacterController`
- Unity **New Input System**
- Owner-only input handling
- Movement and interaction animations
- Local camera following the owned player

## Interaction System

- Nearby object detection through `InteractionDetector`
- Interactable objects using the `IInteractable` interface
- Selection outline for nearby objects
- Animation events used to trigger interactions

## Tool & Item System

- Pick up Axe and Pickaxe tools
- Server validates whether an item is available
- Held item state synchronized using `NetworkVariable`
- Automatically updates the visible item in the player's hand
- Supports dropping held tools back into the world

## Resource Gathering

The game includes resource-related objects such as:

- Trees
- Stone nodes
- Wood resources
- Stone resources
- Wood pallets
- Stone pallets

Players can use the appropriate tools to interact with resource nodes and transport collected materials.

## UI & Camera

- UI Toolkit connection interface
- Host, Client, and Disconnect controls
- Local-player camera follow
- Billboard UI for world-space player information

---

# Player Roles

| Role | Description |
| --- | --- |
| **Host** | Runs the server and also participates as a player. |
| **Client** | Connects to the Host and controls a remote player. |
| **Lumberjack** | Uses an Axe to interact with wood-related resources. |
| **Miner** | Uses a Pickaxe to interact with stone-related resources. |

---

# Technology Stack

## Engine & Rendering

- **Unity 6** `6000.3.24f1`
- **Universal Render Pipeline (URP)** `17.3.0`
- Unity Shader Graph
- C#

## Multiplayer

- **Netcode for GameObjects** `2.13.3`
- Unity Multiplayer Services `2.3.3`
- Multiplayer Play Mode `2.0.2`
- Unity Multiplayer Center & Tools

## Input & UI

- Unity New Input System `1.20.0`
- UI Toolkit
- TextMesh Pro

## Assets

KayKit low-poly asset packs are used for characters, tools, resources, environment objects, and animations:

- `KayKit_Character_Animations_1.1`
- `KayKit_RPGToolsBits_1.0_FREE`
- `KayKit_ResourceBits_1.0_FREE`
- `KayKit_Forest_Nature_Pack_1.0_FREE`
- `KayKit_BlockBits_1.0_FREE`

---

# Main Gameplay Components

## Networking

```text
GameManager
MultiplayerUI
NetworkManager
