# KMK Paris — The Essence Run / Agent Context

## Mission
Build and ship a premium mobile runner set inside the KMK Paris fragrance universe. iOS is the first shipping target; Android follows from the same Expo codebase.

## Product identity
- Brand: KMK PARIS
- Game: KMK Paris — The Essence Run
- Brand signature: « Une brise d’élégance. »
- Visual direction: Parisian, premium, minimal, black / ivory / brass-gold accents.
- Player fantasy: run through a stylised Paris, collect fragrance Essence, avoid visual/noise obstacles and increase the score.
- Hero character: a miniature stylised version of Tysonn. The current in-code character is a temporary vector placeholder; replace it only with an approved/user-provided visual asset when available.

## KMK universe available for gameplay
Current fragrance names that can become collectible families, levels, rewards or themed runs:
- LIANE LIBRE
- PALME D’HIVER
- RIVAGE CUIVRÉ

Keep packaging/name representation aligned with the KMK Paris brand rather than inventing unrelated perfume products.

## Prototype gameplay (v0.1)
- Portrait endless runner.
- Three lanes.
- Swipe/touch left or right to change lane.
- Collect KMK Essence bottles.
- Avoid obstacles.
- Speed rises gradually with score.
- HUD: score, Essence count, best score, level.
- Start and restart flows are playable without authentication.

## Technical baseline
- Expo SDK 57 stable.
- React Native 0.86.3.
- React 19.2.3.
- TypeScript strict mode.
- EAS Build for iOS/TestFlight.
- iOS bundle identifier: `com.kmkparis.theessencerun`.
- No secrets, Apple credentials, Expo tokens, signing files, API keys or `.env` values in Git.
- Do not depend on Lovable or consume Lovable credits for this project.

## Engineering rules
1. Keep `main` buildable.
2. Prefer simple native/Expo APIs before adding dependencies.
3. Keep the game responsive on current iPhones and portrait-first.
4. Any new dependency must be compatible with the current Expo SDK.
5. Never invent production credentials or IDs.
6. Run TypeScript validation and Expo Doctor before treating a release candidate as ready.
7. Keep game constants easy to rebalance (spawn interval, speed, rewards, collision zone).

## Near-term roadmap
1. Validate v0.1 on CI.
2. Connect repository to the user's Expo/EAS project.
3. Produce an iOS preview build for physical-device testing.
4. Replace placeholder character and bottle visuals with final KMK assets.
5. Add themed levels around LIANE LIBRE, PALME D’HIVER and RIVAGE CUIVRÉ.
6. Add sound/haptics, missions, unlocks and persistent high score.
7. Prepare App Store/TestFlight metadata and production build.
