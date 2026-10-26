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

* **Moteur 3D / AR** : Unity 6.3 (version exacte : `6000.3.26f1`)
* **SDK Réalité Augmentée** : Meta XR Core SDK / Meta MR Utility Kit (MRUK) / Interaction SDK (v207)
* **Matériel Cible** : Casque Meta Quest 3 (application standalone)
* **Gestion de version** : Git + Git LFS / GitLab

##  Organisation du dépôt

```
/                         ← le projet Unity (à ouvrir avec Unity Hub)
├── Assets/
│   ├── _Bauprobe/        ← TOUT NOTRE TRAVAIL EST ICI
│   │   ├── Scenes/       MenuGeneral.unity (la scène du casque) + Sandbox/ (une scène de test par personne)
│   │   ├── Core/         le repère du plateau (StageSpace)
│   │   ├── Calibration/  QR code → position du plateau (StageCalibrator)
│   │   ├── Decor/        catalogue des décors, apparition des objets
│   │   ├── Manipulation/ règles quand on déplace / agrandit un objet
│   │   ├── UI/           menus
│   │   ├── Persistence/  sauvegarde / chargement d'une implantation
│   │   └── Tests/        tests automatiques
│   ├── ThirdParty/       code repris d'ailleurs (ex. détection QR de Meta), on n'y touche pas
│   └── (dossiers créés par les SDK : Oculus, MetaXR, XR, Resources...)
├── Packages/             liste des packages Unity
├── ProjectSettings/      réglages du projet
└── Docs/                 documentation de l'équipe
```

Pour comprendre comment le code est organisé : **[Docs/ARCHITECTURE.md](Docs/ARCHITECTURE.md)**.
Pour savoir comment travailler à plusieurs avec Git : **[Docs/CONTRIBUER.md](Docs/CONTRIBUER.md)**.

##  Installation et Prérequis

1. Installer **Unity 6000.3.26f1** (via Unity Hub) avec le module **Android Build Support**.
2. Installer **Git LFS** (une seule fois par ordinateur) : `git lfs install`
3. Cloner le projet :

```bash
git clone https://git.sophia.minesparis.psl.eu/projets-movie-2026/bauprobe-le-decor-sur-le-plateau
cd bauprobe-le-decor-sur-le-plateau
git lfs pull
```

4. Dans Unity Hub : **Add > Add project from disk** et choisir le dossier cloné (la racine du dépôt).

## Utilisation

1. Ouvrez le projet avec Unity.
2. Ouvrez la scène principale : `Assets/_Bauprobe/Scenes/MenuGeneral.unity`.
3. Assurez-vous que votre casque Meta Quest est branché et configuré en mode développeur.
4. Lancez le Build and Run (`File > Build Profiles`, plateforme Meta Quest / Android).
5. Sur le plateau : posez le QR code au sol, regardez-le avec le casque, le décor se cale dessus.
6. Menu « Importer » pour ajouter un décor ; laser de la manette droite + gâchette sur un objet pour le
   déplacer, le tourner ou l'agrandir (il reste toujours droit).

## L'équipe

Ce projet est réalisé par des élèves de Mines Paris - PSL :

* Théophile Boisson de Chazournes
* Charlotte Guillemoto
* Violette Lecuyer
* Mona Sidhoum

**Commanditaires (Théâtre du Châtelet) :**

* **A RAJOUTER**

##  Crédits et Assets tiers

* **Détection de QR Code** : le dossier `Assets/ThirdParty/MetaQRCodeDetection` provient du dépôt officiel [Unity-MRUtilityKitSample](https://github.com/oculus-samples/Unity-MRUtilityKitSample) fourni par Oculus/Meta. Il n'est pas modifié : notre code (`StageCalibrator`) écoute directement MRUK.

##  Licence

Ce projet est réalisé dans un cadre académique pour Mines Paris - PSL et le Théâtre du Châtelet. **A RAJOUTER**

Le code provenant du [MR Utility Kit Sample](https://github.com/oculus-samples/Unity-MRUtilityKitSample) reste soumis à sa propre licence : voir [Docs/licences/LICENSE-Meta-MRUtilityKitSample.txt](Docs/licences/LICENSE-Meta-MRUtilityKitSample.txt).
