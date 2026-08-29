# Préparation iOS

## Identité
- Product name: `KMK Paris — The Essence Run`
- Bundle identifier: `com.kmkparis.theessencerun`
- Version du vertical slice: `0.2.0`
- Orientation: portrait
- Backend: IL2CPP
- Cible iOS minimale configurée: iOS 15

## Création du projet Xcode
Dans Unity :

1. Ouvrir le menu `KMK Paris`.
2. Cliquer sur `Prepare Unity Project`.
3. Tester avec le bouton Play.
4. Cliquer sur `Build iOS Xcode Project`.
5. Ouvrir `Builds/iOS` dans Xcode.
6. Sélectionner la Team Apple Developer, vérifier le Signing & Capabilities puis archiver.

Le script Editor ajoute automatiquement la scène `Assets/KMK/Scenes/KMKMain.unity` aux Build Settings lors du premier import.

## Avant TestFlight
- remplacer l'icône et le splash temporaires ;
- intégrer le modèle 3D final ;
- tester sur plusieurs iPhone physiques ;
- vérifier les performances, la chauffe, le son, les vibrations et les safe areas ;
- incrémenter le build number ;
- compléter les métadonnées App Store Connect.
