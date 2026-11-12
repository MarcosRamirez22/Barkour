# Unity project context — 2026-11-11

Confirmed active checkout: `C:/Users/marco/Documents/Github Unity/Barkour` (the similarly named Unity Projects folder is an older separate project).

- Unity 6000.3.8f1; Universal Render Pipeline 17.3.0; Input System 1.18.0; uGUI 2.0.0 with TextMeshPro. No new packages were installed.
- First-party scripts use global namespaces and MonoBehaviours under Assets/Scripts. No first-party assembly definitions were found in the inspected feature area.
- Entry flow: MainMenu -> Game. RunManager owns WaitingToStart / Running / Failed. Loading Game recreates run state; first keyboard/controller input starts gameplay.
- Game has GrasslandsBackground with Clouds, Mountains, Trees, Hills; camera-dependent ParallaxLayer; level chunk spawning; DogVisual animation and PlayerMovement landing sound. Menu reuses source art and run clip with its own movement/controller.
- Audio had a landing WAV and no music/mixer. MenuAudioSettings adds persisted categories without editing gameplay scripts; future dynamically spawned sources need that component.
- UI uses Canvas, TMP, EventSystem and InputSystemUIInputModule with the existing UI action map.
- Existing local deletions of main/ObstacleTesting scenes, new Game/MainMenu scenes, Audio assets and PlayerMovement edits predated menu implementation. Do not revert them.
- Available validation: local Unity Editor, editor automation scripts and Windows UI control. No Unity MCP connection was exposed. Runtime checks and captures: Docs/MenuValidation. No standalone build or hardware controller verification.

Sources inspected: Packages/manifest.json, ProjectSettings/ProjectVersion.txt, EditorBuildSettings.asset, Game.unity, previous MainMenu.unity, RunManager.cs, PlayerRunAnimation.cs, PlayerMovement.cs audio usage, ParallaxLayer.cs, Kali_Run.anim, InputSystem_Actions.inputactions.
