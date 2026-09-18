# KMK Paris — The Essence Run

## Promesse

Un runner mobile coloré où les fragrances KMK Paris deviennent des mondes à traverser. Chaque monde possède son décor animé, ses obstacles, son flacon et un pouvoir qui transforme réellement la course.

## Boucle de jeu

1. Entrer dans **LIANE LIBRE**.
2. Lire les trois voies et glisser à gauche ou à droite.
3. Collecter les Essences pour augmenter le score et le combo.
4. Attraper un flacon-pouvoir clairement étiqueté.
5. Éviter des obstacles propres au monde en gardant toujours une voie praticable.
6. Franchir la jauge du chapitre et découvrir le monde suivant.
7. Boucler les trois univers avec une vitesse progressivement plus élevée.

## Les trois univers

### Chapitre I — LIANE LIBRE

- Lieu : forêt amazonienne stylisée.
- Décor : canopée émeraude, rivière, lianes, feuilles géantes, oiseaux, lucioles et voyageurs.
- Obstacle : tronc couvert de végétation.
- Pouvoir : **LIANE PROTECTRICE** — absorbe le prochain obstacle.

### Chapitre II — PALME D’HIVER

- Lieu : oasis polaire imaginaire.
- Décor : palmes givrées, aurore turquoise et dorée, reliefs enneigés, flocons et voyageurs emmitouflés.
- Obstacle : cristal de glace.
- Pouvoir : **SOUFFLE POLAIRE** — ralentit le monde pendant sept secondes.

### Chapitre III — RIVAGE CUIVRÉ

- Lieu : côte de Taghazout au coucher du soleil.
- Décor : océan turquoise, falaises cuivrées, vagues, palmiers, oiseaux, surfeur et silhouettes au rivage.
- Obstacle : roche cuivrée.
- Pouvoir : **APPEL DU RIVAGE** — attire les Essences proches pendant sept secondes.

## Lisibilité des pouvoirs

- Le nom et l’effet apparaissent directement au-dessus du flacon avant sa collecte.
- Une confirmation visuelle s’affiche au déclenchement.
- Le HUD conserve le nom, l’effet et la durée restante.
- Une aura autour du personnage matérialise le pouvoir actif.

## Améliorations de gameplay

- Collision calculée sur la hauteur réelle de la piste et non sur la hauteur totale de l’écran.
- Génération par rangées garantissant au moins une voie libre.
- Vitesse progressive plafonnée pour préserver la jouabilité.
- Ralentissement actif intégré au déplacement du décor et des objets.
- Combo jusqu’à ×4 et retour haptique sur déplacements, collectes, pouvoirs et collision.
- Pause sans perte de la durée du pouvoir.
- Transitions de chapitre, progression vers le monde suivant et bouton de reprise.

## Direction artistique

- Univers clair, solaire et saturé ; abandon du fond noir uniforme.
- Illustrations vectorielles embarquées dans l’application, sans dépendance à une image distante.
- Décors fixes profonds et premier plan animé en boucle pour donner la sensation de déplacement.
- Le personnage Tysonn reste le placeholder vectoriel validable jusqu’à réception d’un asset final approuvé.

## Critères de validation avant iOS

- Les trois univers apparaissent au cours d’une même partie.
- Chaque pouvoir peut être collecté et produit l’effet annoncé.
- Une rangée à deux obstacles laisse systématiquement une voie sûre.
- Pause, reprise, collision et redémarrage fonctionnent.
- TypeScript compile en mode strict.
- Expo Doctor ne remonte aucune incompatibilité bloquante.
- Aucun build ou envoi App Store n’est déclenché avant validation visuelle.
