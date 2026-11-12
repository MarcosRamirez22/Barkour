# Barkour main menu

Implemented in the active project at `C:/Users/marco/Documents/Github Unity/Barkour`, Unity 6000.3.8f1, URP, uGUI/TextMeshPro and Input System 1.18.0.

Open `Assets/Scenes/MainMenu.unity` and press Play. MainMenu is first in Build Settings, followed by Game. Gameplay scripts, the Game scene, source art, input bindings and Kali's original animation/controller are unchanged by this work. Existing uncommitted changes were preserved.

## Menu

- Existing Grasslands sprites scroll independently of gameplay. Alternating mirrored background pairs meet at matching edge pixels; each pair wraps by its exact width. Ground reuses a full-width bottom-row terrain sprite. No gameplay components or colliders are included.
- Kali uses a dedicated controller referencing the original looping `Kali_Run` clip, with uniform scaling. She remains bottom-left as the aspect ratio changes.
- The editable collar-tag wordmark includes floppy ears and a paw. Cream, brown, sage and muted blue UI uses a 1920x1080 CanvasScaler with balanced scaling. Buttons have explicit wraparound navigation and focus motion.
- Play accelerates scrolling and Kali, moves her right, fades the interface and loads Game in about 1.2 seconds. Duplicate activation is ignored. Game keeps its existing wait-for-input behavior.
- Mouse, arrow keys/WASD, Enter and the existing controller UI actions are supported. Escape/controller Cancel closes Settings. Left/right adjusts sliders.
- Desktop Quit exits the application (stops Play Mode in the Editor). WebGL hides Quit and adjusts navigation; no browser build was made.

## Audio

Master, Music and SFX are stored under `Barkour.Audio.*` in PlayerPrefs. Changes apply immediately and save on closing Settings, starting Game, pause and quit. Master controls AudioListener volume; individual categories multiply the source's authored volume. Existing scene audio sources default to SFX via a scene-load hook, without changing player movement or landing logic.

No appropriate music track exists in the project. `MenuMusic_Optional` is ready for a licensed track: assign its AudioSource clip. Its MenuAudioSettings component marks it as music. For future dynamically spawned audio sources, attach MenuAudioSettings to their prefabs and mark music sources explicitly. Do not also apply a second volume multiplier in another audio system.

## Validation and maintenance

`Barkour > Validate Main Menu` runs the Editor smoke checks, including injected keyboard/controller input, slider persistence, screenshots and bounds at 1920x1080 and 1920x1200, Play transition, fresh Game state and starting a run. Results and captures are in `Docs/MenuValidation`. It restores the previous volume settings and exits Play Mode. Injected controller checks are not physical-device testing.

`Barkour > Build Main Menu` regenerates the authored menu; save open scenes first. Custom manual changes to MainMenu would be replaced, so normally edit the scene directly and do not rerun the builder. The original MainMenu and build settings are retained as text backups in `Docs/MainMenuBackup`. Existing `.meta` files and GUIDs were preserved; Unity generated metadata for new assets.

No standalone player build, physical controller/audio-listening test, or extended gameplay regression test was performed. The runtime smoke checks do not certify those.
