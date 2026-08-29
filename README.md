# KMK PARIS — THE ESSENCE RUN · Unity V2

Vertical slice 3D mobile du jeu KMK Paris. Cette version remplace visuellement le prototype Expo tout en conservant le même bundle iOS : `com.kmkparis.theessencerun`.

## Ce qui est déjà jouable

- endless runner 3D en caméra arrière ;
- trois voies avec swipe gauche/droite ;
- saut et glissade ;
- obstacles, score, combo et collecte d'Essence ;
- personnage 3D stylisé « mini Tyson » construit en primitives Unity ;
- trois chapitres visuels : LIANE LIBRE, PALME D'HIVER et RIVAGE CUIVRÉ ;
- menus, HUD, compte à rebours et écran de fin ;
- musique et bruitages originaux générés procéduralement au lancement ;
- configuration iOS et commande de build Xcode intégrées.

## Ouverture sur Mac

1. Installer **Unity Hub**.
2. Installer **Unity 6.3 LTS** avec le module **iOS Build Support**.
3. Dans Unity Hub, choisir **Add project from disk** et sélectionner ce dossier.
4. À la première ouverture, le menu de préparation crée automatiquement `Assets/KMK/Scenes/KMKMain.unity`.
5. Appuyer sur **Play**.

## Contrôles

- Swipe horizontal : changer de voie.
- Swipe vers le haut : sauter.
- Swipe vers le bas : glisser.
- Clavier dans l'éditeur : flèches ou A/D, Espace/W pour sauter, S/flèche bas pour glisser.

## Build iOS

Dans Unity :

`KMK Paris > Build iOS Xcode Project`

Le projet Xcode est généré dans `Builds/iOS`. Il faut ensuite l'ouvrir dans Xcode, choisir l'équipe Apple Developer et lancer sur l'iPhone ou archiver pour TestFlight.

## Important sur les assets

Le personnage est **réellement en 3D et animé**, mais reste un modèle stylisé procédural. Il a été conçu comme une base propre et remplaçable. Le modèle final Tyson, les packshots officiels et la musique master pourront être déposés sans refaire le gameplay ; voir `Docs/ASSET_PIPELINE.md`.
