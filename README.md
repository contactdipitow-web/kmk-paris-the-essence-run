# KMK Paris — The Essence Run

Prototype mobile iOS/Android de l'univers KMK Paris, construit avec Expo + React Native.

## Objectif du prototype

- Runner mobile en 3 voies
- Collecte d'Essence
- Obstacles et montée progressive de la vitesse
- Direction artistique inspirée de KMK Paris
- Base prête pour EAS Build et TestFlight

## Démarrage

```bash
npm install
npx expo start
```

## Build iOS

```bash
npx eas-cli@latest build --platform ios --profile preview
```

Le projet cible Expo SDK 57 et React Native 0.86.x.
