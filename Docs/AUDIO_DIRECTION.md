# Direction audio

## Système livré dans le vertical slice
Le projet synthétise son audio au lancement : aucun fichier musical tiers, aucune musique sous licence et aucun téléchargement externe ne sont nécessaires pour tester.

Trois boucles originales sont générées :
- **LIANE LIBRE** — 112 BPM, pulse électro organique et élégante ;
- **PALME D'HIVER** — 118 BPM, timbres plus cristallins ;
- **RIVAGE CUIVRÉ** — 121 BPM, basse plus chaude et rythme plus tendu.

Le système réalise un crossfade à chaque changement de chapitre et augmente très légèrement l'intensité avec la vitesse.

## Bruitages générés
- collecte d'Essence avec hauteur progressive selon le combo ;
- changement de voie ;
- saut ;
- glissade ;
- collision ;
- validation d'un bouton.

## Étape de production finale
Pour une sortie commerciale, remplacer ou enrichir la synthèse par un master original mixé par un compositeur, tout en conservant le `ProceduralAudio` comme fallback et outil de prototypage. Prévoir un mix mobile, une normalisation cohérente, des stems par chapitre et des variations de 45 à 90 secondes.
