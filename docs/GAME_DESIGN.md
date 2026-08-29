# Game Design — v0.1

## One-line pitch
A premium Parisian endless runner where the player crosses KMK Paris' stylised world, collects Essence and protects the run from obstacles and noise.

## Core loop
1. Start the run.
2. Read the three lanes.
3. Swipe left/right.
4. Collect Essence bottles for bonus points.
5. Avoid obstacles.
6. Survive as speed increases.
7. Beat the best score and replay.

## Player controls
- Swipe left: move one lane left.
- Swipe right: move one lane right.
- On-screen arrows mirror the same actions for immediate prototype testing.

## Scoring
- Passive score rises while the run continues.
- Essence gives a score bonus.
- Level increases based on score and communicates progression.

## Art direction
- Background: near-black Parisian night.
- Accents: brass/gold and warm ivory.
- Skyline: deliberately abstract; Eiffel silhouette is a background cue rather than a realistic map.
- Character: miniature Tysonn placeholder built from React Native shapes until an approved visual is integrated.
- Collectible: minimal KMK bottle silhouette.

## Fragrance progression concept
Later builds can turn the existing KMK fragrance families into chapters:
- LIANE LIBRE — reference/origin run.
- PALME D’HIVER — colder, sharper environment and obstacle rhythm.
- RIVAGE CUIVRÉ — warmer copper atmosphere and faster late-run cadence.

## v0.1 acceptance criteria
- App launches in portrait.
- Start button begins a run.
- Player changes lanes through swipe and touch controls.
- Essence can be collected.
- Obstacle collision ends a run.
- Replay resets the run.
- TypeScript compiles.
- Expo Doctor returns without blocking compatibility errors.

## Not yet in v0.1
- Final character art.
- Final product packshots.
- Music/SFX/haptics.
- Authentication or cloud save.
- Shop / purchases.
- Leaderboards.
- Full level system.
