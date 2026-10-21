#  Bauprobe Immersive II - Le décor sur le plateau

**Projet MOVIE 2025 - Mines Paris PSL x Théâtre du Châtelet**

![Statut](https://img.shields.io/badge/Statut-En_développement-orange)
![Platform](https://img.shields.io/badge/Platforme-AR-green)


##  À propos du projet

Ce projet s'inscrit dans le cadre de l'unité d'enseignement MOVIE de **Mines Paris - PSL**, en collaboration directe avec le **Théâtre du Châtelet**.

L'objectif est de concevoir un outil immersif en Réalité Augmentée (AR) pour améliorer la première version en réalité virtuelle de la **Bauprobe**. 

### Qu'est-ce qu'une Bauprobe ?
Au théâtre, la Bauprobe est une étape cruciale qui a lieu plusieurs mois avant l'arrivée du décor final sur le plateau. Elle consiste à simuler l'encombrement spatial du futur décor (historiquement avec des marquages au sol ou des éléments de substitution) afin de :
* Anticiper le jeu des acteurs et l'espace scénique.
* Valider les lignes de visibilité depuis la salle (qui compte environ 2000 places avec une configuration complexe à l'italienne).


Notre solution immersive permet aux metteurs en scène et aux équipes techniques de visualiser les maquettes 3D des décors directement à l'échelle sur le plateau.

## Fonctionnalités 

* **Visualisation 3D à l'échelle 1:1** : Import et affichage de maquettes 3D du décor sur la scène réelle du théâtre.
* **Vérification des visibilités** : Simulation du point de vue depuis différentes places critiques de la salle (orchestre, balcons, corbeille).
* **Manipulation en temps réel** : Possibilité de déplacer, ajuster ou masquer des éléments du décor virtuel.
* **Compatibilité avec le workflow existant** : Interfaçage avec les formats d'export des outils actuellement utilisés par le bureau d'études (AutoCAD, SketchUp, Unreal Engine).

##  Technologies utilisées

* **Moteur 3D / AR** : Unity3D
* **SDK Réalité Augmentée** : Meta XR Core SDK / Meta MR Utility Kit (MRUK)
* **Matériel Cible** : Casque Meta Quest 3
* **Gestion de version** : Git / GitLab

##  Crédits et Assets tiers

* **Détection de QR Code** : Pour le système d'ancrage et de détection spatiale, ce projet intègre et adapte le module de détection de QR Code issu du dépôt officiel [Unity-MRUtilityKitSample](https://github.com/oculus-samples/Unity-MRUtilityKitSample) fourni par Oculus/Meta. Le code source original a été modifié pour répondre aux contraintes spécifiques de l'environnement du Théâtre du Châtelet.


##  Installation et Prérequis

Pour cloner et lancer ce projet en local, assurez-vous d'avoir installé Unity 6.3 LTS et Git LFS.

```bash
git clone https://git.sophia.minesparis.psl.eu/projets-movie-2026/bauprobe-le-decor-sur-le-plateau

cd bauprobe-le-decor-sur-le-plateau
```

## Utilisation

1. Ouvrez le projet avec Unity.
2. Chargez la scène principale située dans ``. **A RAJOUTER**
3. Assurez-vous que votre casque Meta Quest est branché et configuré en mode développeur.
4. Lancez le Build and Run (via `File > Build Settings`).

## L'équipe

Ce projet est réalisé par des élèves de Mines Paris - PSL :

* Théophile Boisson de Chazournes
* Charlotte Guillemoto
* Violette Lecuyer
* Mona Sidhoum

**Commanditaires (Théâtre du Châtelet) :**

* **A RAJOUTER**

##  Licence

Ce projet est réalisé dans un cadre académique pour Mines Paris - PSL et le Théâtre du Châtelet. **A RAJOUTER**

Veuillez noter que le code provenant du [MR Utility Kit Sample](https://github.com/oculus-samples/Unity-MRUtilityKitSample) reste soumis à la licence propre à Oculus/Meta.