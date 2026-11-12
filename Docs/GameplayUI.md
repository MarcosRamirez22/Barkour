# Gameplay framing and Game Over

## Recovery HUD fix

The previous Game Canvas used Constant Pixel Size. RecoveryCountdown was center-anchored at Y=429, so its center became Y=789 at 1280x720 and was outside the screen. The baseline is saved in `Docs/GameplayUIBackup/Baseline.txt`.

Game now uses a screen-space overlay Canvas scaled to the playable viewport, and the recovery timer is anchored below the top edge. The score HUD is bottom-anchored with a cream background. Existing gameplay references and scene objects were retained.

## Camera behavior

`GameplayFraming` preserves the authored Full HD horizontal span: 35.55556 world units. It does not change camera scrolling or player movement.

- 16:9: original camera coverage.
- 16:10: same horizontal span and bottom edge, with more vertical view above.
- Between those ratios: smoothly adjusts vertical coverage.
- Wider than 16:9: black side borders.
- Taller than 16:10: black top/bottom borders.

The HUD and Game Over card stay inside that viewport. Resolution changes during play update both camera framing and UI scaling. The serialized reference camera half-height is 10; update this deliberately if the game's baseline camera design changes.

## Game Over

The existing panel is restyled with the main menu's cream, brown, sage and muted blue colors. It appears over dimmed, frozen gameplay and shows final distance, best distance and `LevelChunkSpawner.CurrentSeed`.

Retry reloads Game using its existing seed-generation configuration and resets the run. Main Menu fades to MainMenu. Both transitions take about 0.35 seconds and restore normal time. Navigation/focus animations work while gameplay is frozen. Desktop Quit exits the player (or stops Play Mode in the Editor); WebGL hides Quit and reconnects navigation.

`DistanceScore` only gained read-only score properties. `PlayerMovement`, `RunManager`, `FailureSystem` and MainMenu were hash-checked unchanged during this task. Source art, gameplay generation and failure timing were not modified. Other pre-existing user edits were left alone.

## Validation

Unity compiled the changes and `Barkour > Validate Gameplay UI` passed:

- Real off-screen recovery and timeout to Game Over.
- Recovery, scores and Game Over bounds at 1280x720, 1920x1080, 1920x1200, 1280x800, 1024x768 and 2560x1080.
- Constant horizontal span and fixed bottom edge through live resizing.
- Black borders on 4:3 and ultrawide screens; screenshots inspected.
- Actual seed, final score and best score on Game Over.
- Frozen player position, keyboard and simulated controller navigation.
- Retry returning to WaitingToStart at normal time, a second failure, and a pointer-click event on Main Menu returning to a working menu.

Results and screenshots: `Docs/GameplayUIValidation`. Controller input was simulated through the Input System. No standalone build, physical controller test or long gameplay regression session was performed.

Backups of the original Game scene and affected scripts are in `Docs/GameplayUIBackup`. `Barkour > Apply Gameplay UI` is an authoring migration, not needed for normal use; rerunning it reapplies the layout. The temporary setup-recovery hook was removed after successful migration.
