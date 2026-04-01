# Dossier de Game Design - Stay Safe

## Concept

Survival Horror 

Barricader les issues afin d’éviter que le cambrioleur ne rentre dans la maison.

## Cibles

les amateurs de jeux vidéos type horreur, avec de la tension, qui aiment les ambiances un peu sombre.

## Histoire

Le joueur se retrouve seul chez lui, dans sa maison. Un kidnappeur rôde autour de la maison et tente de forcer les portes et fenêtres pour entrer. Le joueur doit barricader les accès et réagir à ses tentatives pour entrer dans la maison jusqu’à l’arrivée de la police. 

## DA

**Type de jeu** : 3D dans un environnement clos avec caméra 1ère personne

**Assets :** 

- Maison semi-réaliste multi-pièces
- Lumière sombre
- Son d’ambiance, voix kidnappeur en audio 3D
- Texte d’informations

![image.png](attachment:9c4008cd-b8ca-48ab-9e5e-6b853d5713f6:image.png)

![](https://assetstorev1-prd-cdn.unity3d.com/key-image/ca8f325e-fafb-4009-af7e-9769d833a675.webp)

## Mécaniques

**Déplacement :** Vue 1ère personne, le joueur se déplace librement dans la maison de pièces en pièces. Pas de possibilité de sauter et s’accroupir.

**Indication** : Au début du jeu des informations sur le jeu (Maison, Kidnappeur, explication du principe de barricades) se fait via du texte qui s’affiche simulant la parole du joueur.

**Gestion Objets** : Le joueur peut s’approcher d’un objet et le ramasser. Il peut ensuite barricader un point d’accès avec son objet. Le joueur voit visuellement l’objet qu’il porte.

**Points d'accès (5-6) :** Chaque porte/fenêtre a un état : ouvert, barricadé. Barricader nécessite de rester immobile quelques secondes devant le point pour poser la barricade. 

**Le kidnappeur** : Le kidnappeur n’est jamais vu réellement, il tourne autour de la maison et choisit des points d’accès non barricadés de façon aléatoire afin de forcer l’accès. Le joueur est averti par un son positionné de l’endroit où il tente d’accéder. Le joueur a un délai pour venir barricader avant que l'accès cède. Si l'accès cède → game over le kidnappeur est entré dans la maison. Si le joueur barricade à temps → le kidnappeur se retire et choisit un autre point. Au fil du temps, le délai de forçage raccourcit (accélération de la difficulté).

**Barricade impossible en phase d'exploration :** Le joueur ne peut barricader qu'après le premier assaut du kidnappeur 

**Timer "police" :** Décompte visible à l'écran indiquant l’arrivée de la police. Si le joueur survit jusqu’à la fin du timer la police entre et c’est une victoire.

**Ambiance :** Lumière faible, lumière qui clignote ou s’éteind, point light qui suit le joueur, sons d'ambiance , audio 3D pour localiser le kidnappeur.

**Victoire :** Sirènes au loin, lumières bleues/rouges, écran de victoire.

**Défaite :** Écran noir, son de personne qui entre dans la maison, écran game over.

## Contrôles

Le joueur peut effectuer des déplacements classiques via le clavier (Avant (Z), Arrière (S), Gauche (Q), Droite(D)), afin de se déplacer dans les différentes pièces de la maison. Il déplace la vision de son joueur via sa souris.

Le joueur peut courir en appuyant sur la touche Shift + Q / S /D ou Z.

Pour barricader un point d’accès le joueur doit d’abord avoir ramassé un objet. Il peut le faire grâce à la touche E. Une fois l’objet récupéré, il doit se trouver devant le point d’accès et maintenir la touche E jusqu’à ce que la barricade soit mise en place.

## Planification

Tâches à réaliser :

- Construction de la maison avec les différents points d’accès
- Mise en place du déplacement et caméra du personnage à la 1ère personne
- Implémentation de la récupération d’un objet et de la pose sur un plan d’accès
- Développement de la logique de barricader un point d’accès
- Développement du déplacement et interactions du kidnappeur avec les points d’accès
- Mise en place du timer de jeu
- Intégration des visuels de victoire ou de défaites
- Implémentation d’audio 3D permettant de suivre et d’indiquer la position du kidnappeur
- Ajout de l’ambiance horrifique visuelle et sonore
- Création de la scène d’intro et explication de l’objectif du jeu

## Trame d’une partie

**Phase 1 — Exploration.** La maison est calme. Le joueur découvre les lieux, repère les accès. Sons d'ambiance légers. Impossible de barricader. Le joueur parle donnant des indications sur la situation

**Phase 2 — Intervention du kidnappeur** Des coups violents sur la porte d'entrée. Le joueur comprend qu'il doit aller barricader . Pas de possibilité de perdre sur cette première tentative. Une fois barricadé, message ou indication : la mécanique est comprise. Le barricadage est maintenant débloqué partout.

**Phase 3 — Rondes lentes.** Le kidnappeur commence à tester d'autres accès, un par un, avec des délais confortables. Le joueur peut alterner entre barricader préventivement les accès libres et réagir aux assauts.

**Phase 4 — Accélération.** Les délais raccourcissent. Le kidnappeur enchaîne plus vite ses rondes et certaines barricades déjà mise en place sautent.

**Phase 5 — Assaut final.** Délais très courts. Le kidnappeur teste les accès restants rapidement.

**Fin.** Victoire ou défaite.

## Links

[AOS Fog of War](https://assetstore.unity.com/packages/vfx/shaders/fullscreen-camera-effects/aos-fog-of-war-249249)

[Simple Fake Volume Fog](https://assetstore.unity.com/packages/vfx/shaders/simple-fake-volume-fog-299560)

[Modular First Person Controller](https://assetstore.unity.com/packages/3d/characters/modular-first-person-controller-189884)

[pixel low poly contemporary house(Modular)](https://assetstore.unity.com/packages/3d/environments/urban/pixel-low-poly-contemporary-house-modular-239952#publisher)
