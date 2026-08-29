# KMK Paris — The Essence Run / Unity V2

## Mission
Build an original premium 3D mobile endless runner for iOS first, then Android. The experience can use familiar genre conventions, but must not copy protected characters, environments, music, names, UI, or assets from Temple Run or another game.

## Brand
- Game: **KMK Paris — The Essence Run**
- Signature: **Une brise d’élégance.**
- Bundle ID: `com.kmkparis.theessencerun`
- Visual language: Parisian night, near-black, warm ivory, brass gold, copper, restrained glow.
- Chapters: `LIANE LIBRE`, `PALME D’HIVER`, `RIVAGE CUIVRÉ`.

## Current vertical slice
- Third-person portrait runner.
- Three lanes, swipe left/right, jump, slide.
- Procedural Paris modules, hazards, Essence bottles, score, combo and best score.
- Procedural stylized 3D mini-Tyson placeholder, fully replaceable by an approved rigged model.
- Original runtime-generated adaptive music and SFX.
- Runtime bootstrap means the main scene intentionally contains no serialized gameplay objects.

## Technical baseline
- Unity Editor: `6000.3.17f1` (Unity 6.3 LTS).
- Built-in Render Pipeline for a dependency-light first slice.
- UGUI for the runtime interface.
- iOS: portrait, iPhone only, IL2CPP, minimum iOS 15, 60 fps target.

## Rules
1. Keep `unity-v2` independently playable; do not overwrite the Expo prototype on `main`.
2. Never commit Apple credentials, provisioning profiles, certificates, API keys, tokens, or `.env` files.
3. Preserve the bundle identifier unless the owner explicitly changes it.
4. Test lane change, jump, slide, collision, replay, safe area, audio mute, chapter transitions, and world recycling after gameplay changes.
5. Prefer pooled/recycled objects and shared materials; avoid per-frame allocation in the gameplay loop.
6. Final likeness assets for Tysonn must be approved/user-provided. The procedural avatar is a production placeholder, not a claim of final likeness.
7. Music and art must be original or properly licensed for commercial release.

## Near-term production path
1. Open and validate the vertical slice on a physical iPhone.
2. Replace procedural avatar with a rigged, optimized final model and authored animation set.
3. Replace modular blocks with approved KMK environment art, optimized using atlases, LODs and batching.
4. Replace/finalize soundtrack with mastered original stems while keeping procedural audio as fallback.
5. Profile, optimize, add missions/unlocks, then prepare App Store metadata and TestFlight.
