#  Bauprobe Immersive II - Le décor sur le plateau

**Projet MOVIE 2025 - Mines Paris PSL x Théâtre du Châtelet**

![Statut](https://img.shields.io/badge/Statut-En_développement-orange)
![Platform](https://img.shields.io/badge/Platforme-AR-green)


##  À propos du projet

Ce projet s'inscrit dans le cadre de l'unité d'enseignement MOVIE de **Mines Paris - PSL**, en collaboration directe avec le **Théâtre du Châtelet**.

L'objectif est de concevoir un outil immersif en Réalité Mixte (MR) pour améliorer la première version en réalité virtuelle de la **Bauprobe**.

### Qu'est-ce qu'une Bauprobe ?
Au théâtre, la Bauprobe est une étape cruciale qui a lieu plusieurs mois avant l'arrivée du décor final sur le plateau. Elle consiste à simuler l'encombrement spatial du futur décor (historiquement avec des marquages au sol ou des éléments de substitution) afin de :
* Anticiper le jeu des acteurs et l'espace scénique.
* Valider les lignes de visibilité depuis la salle (qui compte environ 2000 places avec une configuration complexe à l'italienne).


Notre solution immersive permet aux metteurs en scène et aux équipes techniques de visualiser les maquettes 3D des décors directement à l'échelle sur le plateau.

## État actuel

Testé sur Meta Quest 3 :

* ✅ Calage du décor sur un **QR code** posé sur le plateau
* ✅ **Importer** un décor depuis le menu (catalogue ; pour l'instant un cube de test)
* ✅ Sélection au **laser** de la manette droite, puis **Déplacer / Scale / Rotation / Couleur / Supprimer**
* ✅ Les objets restent toujours **droits**
* ✅ **Supprimer tout**, **Sauvegarder** / **Charger** une implantation

Ce qui reste à faire : [Docs/A_FAIRE_DANS_UNITY.md](Docs/A_FAIRE_DANS_UNITY.md#3-reste-à-faire).

## Fonctionnalités visées

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
│   │   ├── Decor/        catalogue des décors (Catalog/), prefabs, apparition des objets
│   │   ├── Manipulation/ laser, menu de l'objet, déplacer / agrandir / tourner (à la manette)
│   │   ├── UI/           menu général (BauprobeMenu) ; prefabs des menus dans UI/Prefabs/
│   │   ├── Persistence/  sauvegarde / chargement d'une implantation
│   │   └── Tests/        tests automatiques
│   ├── ThirdParty/       code repris d'ailleurs (ex. détection QR de Meta), on n'y touche pas
│   └── (dossiers créés par les SDK : Oculus, MetaXR, XR, Resources...)
├── Packages/             liste des packages Unity
├── ProjectSettings/      réglages du projet
└── Docs/                 documentation de l'équipe
```

Documentation :
* **[Docs/ARCHITECTURE.md](Docs/ARCHITECTURE.md)** : comment le code est organisé, ajouter un décor.
* **[Docs/CONTRIBUER.md](Docs/CONTRIBUER.md)** : travailler à plusieurs avec Git.
* **[Docs/A_FAIRE_DANS_UNITY.md](Docs/A_FAIRE_DANS_UNITY.md)** : ouvrir le projet sur un nouvel ordi, contenu de la scène, reste à faire.

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

### Lancer l'appli

1. Ouvrez la scène `Assets/_Bauprobe/Scenes/MenuGeneral.unity`.
2. Branchez le casque (mode développeur activé) et lancez **Build And Run** (`File > Build Profiles`, plateforme Meta Quest / Android).
3. Au premier lancement, acceptez la permission **« données spatiales »**.

Le détail (premier lancement sur un nouvel ordinateur, installation sur le casque) est dans
[Docs/A_FAIRE_DANS_UNITY.md](Docs/A_FAIRE_DANS_UNITY.md).

### Dans le casque

1. **Calage** : posez le QR code **à plat au sol** sur le plateau (au moins 15 cm de côté) et regardez-le.
   Le menu affiche « QR détectés : 1 » ; le décor est alors calé sur le plateau.
2. **Menu général** : il est attaché à la **main gauche**.
   * *Importer* > choisir un décor : il apparaît posé au sol, 1,5 m devant vous.
   * *Supprimer tout*, *Sauvegarder*, *Charger*.
3. **Manipuler un objet** (manette droite) :

| Action | Commande |
|---|---|
| Sélectionner un objet | viser avec le laser + **gâchette** → le menu de l'objet s'ouvre |
| Déplacer | bouton *Déplacer*, puis bouger la manette ; joystick haut / bas = éloigner / rapprocher |
| Agrandir / rétrécir | bouton *Scale*, puis joystick haut / bas |
| Tourner | bouton *Rotation*, puis joystick gauche / droite (l'objet reste droit) |
| Valider / Annuler | **gâchette** / bouton **B** |
| Couleur, Supprimer | boutons du menu de l'objet ; ✕ pour fermer le menu |

## L'équipe

Ce projet est réalisé par des élèves de Mines Paris - PSL :

* Théophile Boisson de Chazournes
* Charlotte Guillemoto
* Violette Lecuyer
* Mona Sidhoum

**Commanditaires (Théâtre du Châtelet) :**

* **A RAJOUTER**

##  Crédits et Assets tiers

* **Détection de QR Code** : le dossier `Assets/ThirdParty/MetaQRCodeDetection` provient du dépôt officiel [Unity-MRUtilityKitSample](https://github.com/oculus-samples/Unity-MRUtilityKitSample) fourni par Oculus/Meta. Ses scripts ne sont pas modifiés : notre code (`StageCalibrator`) écoute directement MRUK. Son prefab `QRCodeSampleUI` sert de support au menu général `BauprobeMenu`.

##  Licence

Ce projet est réalisé dans un cadre académique pour Mines Paris - PSL et le Théâtre du Châtelet. **A RAJOUTER**

Le code provenant du [MR Utility Kit Sample](https://github.com/oculus-samples/Unity-MRUtilityKitSample) reste soumis à sa propre licence : voir [Docs/licences/LICENSE-Meta-MRUtilityKitSample.txt](Docs/licences/LICENSE-Meta-MRUtilityKitSample.txt).
