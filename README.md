#  Bauprobe Immersive II - Le décor sur le plateau

**Projet MOVIE 2025 - Mines Paris PSL x Théâtre du Châtelet**

![Statut](https://img.shields.io/badge/Statut-En_développement-orange)
![Platform](https://img.shields.io/badge/Platforme-XR-green)


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
* ✅ Sélection au **laser** de la manette droite (l'objet sélectionné est en surbrillance), puis **Déplacer / Scale / Rotation / Couleur / Supprimer**
* ✅ Les objets se déplacent dans toutes les directions, se tournent et se **basculent** sans s'enfoncer dans le sol
* ✅ **Étirer une face** (EXTRUDE) : prolonger ou raccourcir un objet par une de ses faces (ex. une porte plus haute)
* ✅ **Supprimer tout**, **Sauvegarder** / **Charger** une implantation
* ✅ **Saisie à deux mains** (grab) : l'objet garde ses modifications et reste parallèle au sol ; bouton **INFO** (position,
  dimensions, hauteur, volume) ; **Scale en %** au pavé numérique ; choix de la face à étirer par boutons ; sélection d'un
  objet quand on est à l'intérieur
* ✅ **Tableau CSV** pour le bureau d'études (Excel), écrit à chaque sauvegarde : position X / Y / Z, angle, dimensions, scène ; origine = le point zéro de la scène, au sol (offset du QR code saisi dans le casque). Voir [Docs/A_FAIRE_DANS_UNITY.md](Docs/A_FAIRE_DANS_UNITY.md#exporter-le-tableau-csv-pour-le-bureau-détudes)

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

* Aucun élément de plus de **18 m** (plus appliqué dans l'application : le *Scale* n'a plus de limite de taille).
* **Exécutable déployable partout**, sous forme de **kit transportable** jusqu'à la Seine Musicale.
* **Formats de décor et de tableau communs** avec le volet « Voir la même chose ».

### Comment ajouter un élément de décor dans le projet (première version)

* Glisser le modèle 3D (OBJ du bureau d'études) dans `Assets/_Bauprobe/Decor/Models/<nom du décor>/`.
* Clic droit sur le modèle dans la fenêtre Project > **Bauprobe > Ajouter au catalogue** : l'outil crée le prefab
  (avec son collider et les composants pour le manipuler), sa fiche, et l'ajoute au catalogue.
  Le bouton apparaît dans le menu *Objects* du casque.
* Pas à pas, et méthode à la main : [Docs/ARCHITECTURE.md](Docs/ARCHITECTURE.md#ajouter-un-nouveau-décor-sans-code).

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
* **Gestion de version** : Git + Git LFS / GitHub

##  Organisation du dépôt

```
/                         ← le projet Unity (à ouvrir avec Unity Hub)
├── Assets/
│   ├── _Bauprobe/        ← TOUT NOTRE TRAVAIL EST ICI
│   │   ├── Scenes/       MenuGeneral.unity (la scène du casque) + Sandbox/ (une scène de test par personne)
│   │   ├── Core/         le repère du plateau (StageSpace), l'offset du QR code (saisi dans le casque)
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

Cette partie s'adresse aussi à quelqu'un qui n'a **jamais utilisé Unity, Git ni un Meta Quest**.

### Ce qu'il faut avant de commencer

* Un **ordinateur Windows ou Mac** avec environ **30 Go libres** (Unity + le projet).
* **Git** et un **terminal** :
  * Windows : installer **Git Bash** (c'est dans lui qu'on tape toutes les commandes de ce document) ;
  * macOS / Linux : l'application **Terminal** suffit, Git s'installe si besoin.

  Tutoriel pas à pas (section « git (et bash sur Windows) ») : [Installations du cours d'informatique des Mines](https://intro.info-mines.paris/1-1-installations/).
* Un **compte GitHub** (pour cloner le dépôt et, si vous voulez proposer des modifications, pousser des branches).
* Pour installer l'appli sur le casque :
  * un **Meta Quest 3** et un **câble USB-C qui transfère les données** (pas un simple câble de charge) ;
  * un **compte Meta** qui est **propriétaire du casque** (il n'y a qu'un propriétaire par casque, et seul lui peut
    activer le mode développeur) ;
  * l'**application mobile Meta Horizon** sur un téléphone (iOS ou Android).

  Détails : [Docs/A_FAIRE_DANS_UNITY.md](Docs/A_FAIRE_DANS_UNITY.md#installer-sur-le-casque).

### Étapes

1. **Installer Unity Hub** : [documentation d'installation de Unity Hub](https://docs.unity.com/en-us/hub/install-hub).
   Se connecter (ou créer) un compte Unity au premier lancement ; la licence *Personal* (gratuite) suffit.
2. **Installer l'éditeur Unity 6000.3.26f1** depuis Unity Hub :
   *Installations > Installer l'éditeur > Archive* (lien vers l'[archive des versions](https://unity.com/releases/editor/archive))
   > version **6000.3.26f1**, et **cocher le module Android Build Support** (avec *OpenJDK* et *Android SDK & NDK Tools*).
   Sans ce module, impossible d'installer l'appli sur le casque.
3. **Ouvrir un terminal** (Git Bash sur Windows, Terminal sur macOS / Linux) et se placer dans le dossier où ranger le
   projet, par exemple *Documents* :

```bash
cd ~/Documents
```

4. **Activer Git LFS** (une seule fois par ordinateur ; il sert aux gros fichiers, comme les modèles 3D) :

```bash
git lfs install
```

5. **Cloner le projet** (le télécharger), toujours dans le terminal :

```bash
git clone https://github.com/MonaSidhoum/Bauprobe_ledecorsurleplateau
```

```bash
cd Bauprobe_ledecorsurleplateau
```

```bash
git lfs pull
```

6. Dans **Unity Hub** : *Projects > Add > Add project from disk*, choisir le dossier `Bauprobe_ledecorsurleplateau`
   (celui qui contient `Assets/`, `Packages/`, `ProjectSettings/`), puis l'ouvrir. La première ouverture prend
   10 à 20 minutes.

La suite (vérifications, installation sur le casque, **mise à jour d'un clone existant**) est dans
[Docs/A_FAIRE_DANS_UNITY.md](Docs/A_FAIRE_DANS_UNITY.md#1-ouvrir-le-projet-sur-un-nouvel-ordinateur).

## Utilisation

### Lancer l'appli

1. Ouvrez la scène `Assets/_Bauprobe/Scenes/MenuGeneral.unity`.
2. Branchez le casque (mode développeur activé) et lancez **Build And Run** (`File > Build Profiles`, plateforme Meta Quest / Android).
3. Au premier lancement, acceptez la permission **« données spatiales »** (sinon : bouton **Enable Access** du menu général).

Le détail (premier lancement sur un nouvel ordinateur, installation sur le casque) est dans
[Docs/A_FAIRE_DANS_UNITY.md](Docs/A_FAIRE_DANS_UNITY.md).

### Dans le casque

1. **Calage** : collez le QR code (au moins 15 cm de côté) à la verticale, **face à la salle**, sur un pied ou un
   support (ou posez-le à plat au sol), et regardez-le. Le menu affiche « QR détectés : 1 » ; le décor est alors calé.
   **Au lancement**, l'écran **QR code offset** (en anglais) demande où est le QR code par rapport au **point zéro** de la scène
   (X vers cour, Y vers le lointain, en mètres) et la **hauteur de son bord du bas** au-dessus du sol. Il est
   pré-rempli avec les dernières valeurs : *Confirm* si le QR code n'a pas bougé. Pas besoin de *Build*.
   Marche à suivre détaillée : [Docs/A_FAIRE_DANS_UNITY.md](Docs/A_FAIRE_DANS_UNITY.md#offset-et-hauteur-du-qr-code-dans-le-casque).
2. **Menu général** : il est attaché à la **main gauche**.
   * *Importer* > choisir un décor : il apparaît posé au sol, 1,5 m devant vous.
   * *Supprimer tout*, *Sauvegarder*, *Charger*.
   * L'écran **QR code offset** est aussi accessible par la première ligne de *Charger*.
3. **Manipuler un objet** (manette droite) :

| Action | Commande |
|---|---|
| Sélectionner un objet | viser avec le laser + **gâchette** → le menu de l'objet s'ouvre **à la main gauche**, à droite du menu général (au-dessus du bouton « + » si le menu général est replié) |
| Déplacer | bouton *Déplacer* : l'objet est accroché à la manette et un seul laser le relie à la manette (celui de Meta est caché pendant le déplacement) ; tourner la manette le déplace (loin si l'objet est loin) ; **joystick haut / bas** = l'éloigner / le rapprocher le long du laser, **gauche / droite** = le décaler sur le côté ; **grip maintenu + joystick haut / bas** = monter / descendre. Plus l'objet est loin, plus il va vite |
| Agrandir / rétrécir | bouton *Scale*, puis joystick haut / bas (l'objet reste posé au sol, ou accroché par le haut s'il est au cintre) ; ou taper un **pourcentage de la taille actuelle** sur le pavé de la main gauche (ex. `150` = 1,5 fois plus grand) puis **APPLY** |
| Tourner | bouton *Rotation*, puis joystick gauche / droite = tourner sur lui-même ; joystick haut / bas = basculer vers l'avant / l'arrière ; laser + gâchette sur le bouton **Align with floor** (à la main gauche, à la place du menu objet) = le poser parallèle au sol, sur la face la plus proche (un objet un peu penché se redresse, un objet très penché se couche sur le côté) |
| Valider / Annuler | **gâchette** / bouton **B** |
| Étirer une face | bouton *EXTRUDE*, viser une face au laser (bleu) + **gâchette** pour la choisir (orange), **ou** cliquer **TOP / BOTTOM / LEFT / RIGHT / FRONT / BACK** à la main gauche (vus depuis vous ; utile pour la face du haut d'un grand décor) ; puis joystick haut / bas = pousser / rentrer la face (grip maintenu = plus lent) ; la longueur s'affiche sur la face, la face opposée ne bouge pas |
| Infos de l'objet | bouton *INFO* du menu objet : panneau (en anglais) avec la position depuis le point zéro (X, Y, Z, comme le CSV), l'angle, les dimensions, la hauteur et le volume (boîte englobante) ; *INFO* à nouveau pour le fermer |
| Prendre à la main (grab) | à une ou deux mains (Interaction SDK) : l'objet **garde sa taille et ses étirements**, et **reste parallèle au sol** (seule la rotation autour de la verticale suit les mains) ; écarter / rapprocher les deux mains = agrandir / rétrécir |
| Objet dans lequel on se trouve | quand on est **à l'intérieur** d'un objet (ex. le module central), viser « le vide » (aucun autre objet) + **gâchette** sélectionne cet objet |
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
