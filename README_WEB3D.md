# KMK Paris — The Essence Run · Web3D V2

Version 3D navigateur, sans Unity et sans installation locale.

## Stack
- React + Vite
- Three.js via React Three Fiber
- GitHub Pages pour la preview gratuite
- Supabase volontairement non utilisé dans la vertical slice afin de garder le coût backend à zéro

## Gameplay
- runner 3D caméra arrière
- 3 voies
- swipe gauche/droite
- swipe haut pour sauter
- swipe bas pour glisser
- obstacles, Essence, score, combo, meilleur score
- trois univers : LIANE LIBRE, PALME D'HIVER, RIVAGE CUIVRÉ
- bande-son et SFX procéduraux via Web Audio

## Déploiement
La branche `web3d-v2` est compilée par GitHub Actions et publiée sur GitHub Pages.

## iOS ensuite
Une fois le rendu Web3D approuvé, la même app sera encapsulée avec Capacitor. Le build iOS pourra être automatisé sur un runner macOS GitHub Actions sans installer Unity sur le Mac local.
