# Barkour

Barkour is a 2D platformer being developed in Unity. The player controls a dog moving through an obstacle course and must use different movement abilities to avoid obstacles and travel as far as possible.

The project is currently in early development, with the repository serving as the initial project skeleton for the game's core systems.

## Planned Gameplay

The core gameplay of Barkour will focus on movement and obstacle traversal. Planned mechanics include:

- Running and jumping
- Crouching, sliding, and crawling
- Wall jumping
- Different obstacle types
- Continuous level progression
- Distance-based scoring
- Personal best tracking
- Game over and restart system

Additional mechanics and polish will be added as development progresses.

## Technology

Barkour is currently being developed with:

- **Unity 6000.3.8f1**
- **C#**
- **Universal Render Pipeline (URP)**
- **Unity Input System**
- **Git**
- **GitHub**

## Current Project Structure

```text
Barkour/
├── Assets/
│   ├── Scenes/
│   │   └── main.unity
│   ├── Scripts/
│   │   └── PlayerMovement.cs
│   ├── Settings/
│   └── InputSystem_Actions.inputactions
├── Packages/
├── ProjectSettings/
├── .gitattributes
└── .gitignore
```

As development continues, the `Assets` directory will be expanded to separate gameplay scripts, art, animations, audio, prefabs, and other game systems.

## Requirements

To open and run the project, you will need:

- Unity Hub
- Unity Editor **6000.3.8f1**
- Git
- A C# development environment such as Visual Studio, Visual Studio Code, or JetBrains Rider

Using the project's specified Unity version is recommended to prevent compatibility issues.

## Running the Project

1. Clone the repository:

```bash
git clone https://github.com/MarcosRamirez22/Barkour.git
```

2. Enter the project directory:

```bash
cd Barkour
```

3. Active development takes place on the `develop` branch. Switch to it if necessary:

```bash
git checkout develop
```

4. Open **Unity Hub**.

5. Select **Add > Add project from disk** and select the cloned `Barkour` directory.

6. Open the project using **Unity 6000.3.8f1**.

7. Allow Unity to install and import the required packages and assets.

8. Open the main game scene:

```text
Assets/Scenes/main.unity
```

9. Press the **Play** button in the Unity Editor to run the project.

## Building the Project

To create a standalone build:

1. Open the project in Unity.
2. Go to **File > Build Profiles**.
3. Select the desired target platform.
4. Make sure `Assets/Scenes/main.unity` is included in the build.
5. Select **Build**.
6. Choose an output directory.

Unity will compile the project and create a standalone build for the selected platform.

## Development

Development work is performed primarily on the `develop` branch.

Git and GitHub are used to track changes throughout development so that the repository maintains a history of the project's implementation.

The current repository represents the early project skeleton. As development progresses, gameplay systems will be separated into appropriate scripts and asset directories to keep the project organized and maintainable.

## Author

**Marcos Ramirez**

Computer Science Capstone Project  
California State University Channel Islands
