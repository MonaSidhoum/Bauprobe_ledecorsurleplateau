#  Bauprobe Immersive II - Le décor sur le plateau

**Projet MOVIE 2025 - Mines Paris PSL x Théâtre du Châtelet**

![Statut](https://img.shields.io/badge/Statut-En_développement-orange)
![Platform](https://img.shields.io/badge/Platforme-AR-green)


##  À propos du projet

Ce projet s'inscrit dans le cadre de l'unité d'enseignement MOVIE de **Mines Paris - PSL**, en collaboration directe avec le **Théâtre du Châtelet**.

**Bauprobe Immersive II** est une application de réalité augmentée développée sous Unity. Elle permet de **visualiser un décor de spectacle à l'échelle 1 sur la scène réelle, avant sa construction**. Elle fait suite à l'application **Bauprobe Immersive I** (en réalité virtuelle), dont elle reprend la méthodologie et l'interface de connexion Unity.

### Qu'est-ce qu'une Bauprobe ?
Au théâtre, la Bauprobe est une étape cruciale qui a lieu plusieurs mois avant l'arrivée du décor final sur le plateau. Elle consiste à simuler l'encombrement spatial du futur décor (historiquement avec des marquages au sol ou des éléments de substitution) afin de :
* Anticiper le jeu des acteurs et l'espace scénique.
* Valider les lignes de visibilité depuis la salle (qui compte environ 2000 places avec une configuration complexe à l'italienne).

Notre solution immersive permet aux metteurs en scène et aux équipes techniques de visualiser les maquettes 3D des décors directement à l'échelle sur le plateau.

### Les deux volets du projet

D'après le [cahier des charges](#cahier-des-charges) (version 1, 9 octobre 2026), le projet comporte deux volets :

| Volet | Principe | Ce dépôt |
|---|---|---|
| **Le décor sur le plateau** | Un technicien, seul, place et ajuste le décor, scène par scène. | ✅ **C'est ce dépôt.** |
| **Voir la même chose** | Un groupe de 5 personnes, connecté en réseau, voit le même décor ensemble, sur place ou à distance. | Développé à part. Les formats de décor et de tableau doivent rester **communs** aux deux volets. |

## État actuel

Testé sur Meta Quest 3 :

* ✅ Calage du décor sur un **QR code** posé sur le plateau (au sol, ou en hauteur avec sa distance au sol réglable)
* ✅ **Importer** un décor depuis le menu (catalogue ; pour l'instant un cube de test)
* ✅ Sélection au **laser** de la manette droite, puis **Déplacer / Scale / Rotation / Couleur / Supprimer**
* ✅ Les objets se déplacent dans toutes les directions, se tournent et se **basculent** sans s'enfoncer dans le sol
* ✅ **Supprimer tout**, **Sauvegarder** / **Charger** une implantation

Ce qui reste à faire : [Docs/A_FAIRE_DANS_UNITY.md](Docs/A_FAIRE_DANS_UNITY.md#3-cahier-des-charges--suivi) (suivi du cahier des charges, puis corrections techniques).

## Cahier des charges

Résumé du cahier des charges *Bauprobe Immersive II × MOVIE* (version 1, 9 octobre 2026) pour notre volet. Le suivi détaillé, point par point, est dans [Docs/A_FAIRE_DANS_UNITY.md](Docs/A_FAIRE_DANS_UNITY.md#3-cahier-des-charges--suivi).

### Le parcours de l'utilisateur

Un utilisateur unique (le technicien) travaille ainsi :

1. **Importer** le décor du bureau d'études et le **tableau de positionnement**.
2. **Afficher en AR la scène 1**, puis les suivantes.
3. **Modifier le placement et la transformation** (taille, échelle) des éléments, puis **enregistrer** dans la scène.
4. **Exporter** le tableau modifié et les nouveaux modèles.

### Règles sur le décor

* **Point d'origine des éléments** : pour un élément posé **au sol**, le point le plus bas, au centre de l'objet ; pour un élément **au cintre**, le point le plus haut.
* **Échelle du décor réductible** si l'espace manque (pour répéter dans un lieu plus petit que la scène).
* **Bibliothèque standard** en plus des décors du bureau d'études : praticables, draperie simple, pendrillons et frises, en tailles standard.
* **Premier jeu de modèles** : le décor de *La Cage aux Folles* (détenu par Thomas).
* **Vue de dessus** : prévue en V6.

### Contraintes

* Aucun élément de plus de **18 m** (appliqué : le *Scale* s'arrête à 18 m sur la plus grande dimension).
* **Exécutable déployable partout**, sous forme de **kit transportable** jusqu'à la scène musicale.
* **Formats de décor et de tableau communs** avec le volet « Voir la même chose ».

### Livrables

* **Tableau de positionnement par scène** (Excel en première approche).
* **Tableau modifié** et **nouveaux modèles 3D**.
* **Code source** et **documentation utilisateur**.

### Tests prévus

1. Une scène de *La Cage aux Folles*.
2. Puis un décor sans lien avec ce spectacle.
3. Un test commun avec le volet « Voir la même chose » (date à fixer).

### Pistes hors cahier des charges

* **Points de vue depuis la salle** (orchestre, balcons, corbeille) pour vérifier les visibilités : idée de départ du projet, absente du cahier des charges. À confirmer avec les commanditaires.
* **Compatibilité avec les outils du bureau d'études** (AutoCAD, SketchUp, Unreal Engine) : à préciser selon le format retenu pour l'import des modèles.

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
│   │   ├── Core/         le repère du plateau (StageSpace) + ses réglages (Resources/StageSettings.asset)
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
* **[Docs/A_FAIRE_DANS_UNITY.md](Docs/A_FAIRE_DANS_UNITY.md)** : ouvrir le projet sur un nouvel ordi, contenu de la scène, suivi du cahier des charges et reste à faire.

##  Installation et Prérequis

1. Installer **Unity 6000.3.26f1** (via Unity Hub) avec le module **Android Build Support**.
2. Installer **Git LFS** (une seule fois par ordinateur) : `git lfs install`
3. Cloner le projet :

```bash
git clone https://github.com/MonaSidhoum/Bauprobe_ledecorsurleplateau
cd Bauprobe_ledecorsurleplateau
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

1. **Calage** : posez le QR code sur le plateau (au moins 15 cm de côté), à plat au sol ou collé sur un support,
   et regardez-le. Le menu affiche « QR détectés : 1 » ; le décor est alors calé sur le plateau.
   **Si le QR code n'est pas au sol**, mesurez la distance entre **le bas du QR code et le sol**, et indiquez-la
   (en mètres) dans `Assets/_Bauprobe/Core/Resources/StageSettings.asset`, champ *Qr Bottom Height Above Floor*
   (dans l'Inspector de Unity, ou directement dans le fichier), puis refaites un *Build And Run*.
   Exemple : bas du QR code à 1,20 m du sol → `1.2`. Ce fichier est suivi par Git.
   Marche à suivre détaillée et fonctionnement : [Docs/A_FAIRE_DANS_UNITY.md](Docs/A_FAIRE_DANS_UNITY.md#régler-la-hauteur-du-qr-code-à-refaire-à-chaque-nouveau-lieu).
2. **Menu général** : il est attaché à la **main gauche**.
   * *Importer* > choisir un décor : il apparaît posé au sol, 1,5 m devant vous.
   * *Supprimer tout*, *Sauvegarder*, *Charger*.
3. **Manipuler un objet** (manette droite) :

| Action | Commande |
|---|---|
| Sélectionner un objet | viser avec le laser + **gâchette** → le menu de l'objet s'ouvre |
| Déplacer | bouton *Déplacer*, puis bouger la manette ; **joystick** = avant / arrière / gauche / droite (par rapport à votre regard) ; **grip maintenu + joystick haut / bas** = monter / descendre |
| Agrandir / rétrécir | bouton *Scale*, puis joystick haut / bas (l'objet reste posé au sol, ou accroché par le haut s'il est au cintre) |
| Tourner | bouton *Rotation*, puis joystick gauche / droite = tourner sur lui-même ; joystick haut / bas = basculer vers l'avant / l'arrière ; laser + gâchette sur le bouton **Aligner au sol** (au-dessus de l'objet) = le poser parallèle au sol, sur la face la plus proche (un objet un peu penché se redresse, un objet très penché se couche sur le côté) |
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
