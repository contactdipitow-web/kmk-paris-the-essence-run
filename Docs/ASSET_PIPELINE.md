# Pipeline d'assets final

## Personnage Tyson

Format recommandé : FBX ou glTF converti en FBX, rig humanoïde, textures PBR 2K maximum pour mobile.

Animations minimales :

- idle ;
- run ;
- lane change left/right ;
- jump ;
- slide ;
- stumble/hit ;
- victory/celebration.

Le script `RunnerAvatar` encapsule l'avatar procédural actuel. Pour intégrer le modèle final, conserver le même point racine au niveau des pieds et exposer les mêmes états `Run`, `Jump` et `Slide` dans un Animator Controller.

## Produits KMK

Prévoir des modèles 3D optimisés des flacons, ou des billboards/meshes simples si seuls les packshots HD sont disponibles. Les noms officiels intégrés sont :

- LIANE LIBRE ;
- PALME D'HIVER ;
- RIVAGE CUIVRÉ.

## Musique

Format recommandé : WAV 48 kHz, boucle propre de 60 à 120 secondes. Préparer également des stems facultatifs (percussions, basse, atmosphère, motif) pour faire monter l'intensité avec la vitesse.

## Performance iPhone

- textures principales 2048 px maximum ;
- atlas de textures pour le décor ;
- matériaux partagés ;
- LOD sur les bâtiments et accessoires complexes ;
- éviter les lumières ponctuelles dynamiques nombreuses ;
- viser 60 fps sur les iPhone récents et prévoir un mode 30 fps pour les appareils plus anciens.
