#  Bauprobe Immersive II - Le décor sur le plateau

**Projet MOVIE 2025 - Mines Paris PSL x Théâtre du Châtelet**

![Statut](https://img.shields.io/badge/Statut-Version_finale-blue)
![Platform](https://img.shields.io/badge/Platforme-XR-green)


##  À propos du projet

Ce projet s'inscrit dans le cadre de l'unité d'enseignement MOVIE de **Mines Paris - PSL**, en collaboration directe avec le **Théâtre du Châtelet**.

**Bauprobe Immersive II** est une application de réalité mixte développée sous Unity pour le **Meta Quest 3**. Elle permet de **visualiser un décor de spectacle à l'échelle 1 sur la scène réelle, avant sa construction**. Elle fait suite à l'application **Bauprobe Immersive I** (en réalité virtuelle), dont elle reprend la méthodologie et l'interface de connexion Unity.

### Qu'est-ce qu'une Bauprobe ?
Au théâtre, la Bauprobe est une étape cruciale qui a lieu plusieurs mois avant l'arrivée du décor final sur le plateau. Elle consiste à simuler l'encombrement spatial du futur décor (historiquement avec des marquages au sol ou des éléments de substitution) afin de :
* Anticiper le jeu des acteurs et le volume de l'espace scénique.
* Valider les lignes de visibilité depuis la salle (qui compte environ 2000 places avec une configuration complexe à l'italienne).

Notre solution immersive permet aux metteurs en scène et aux équipes techniques de visualiser les maquettes 3D des décors directement à l'échelle sur le plateau.

### Les deux volets du projet

| Volet | Principe | Ce dépôt |
|---|---|---|
| **Le décor sur le plateau** | Un technicien, seul, place et ajuste le décor, scène par scène. | ✅ **C'est ce dépôt.** |
| **Voir la même chose** | Un groupe de personnes, connecté en réseau, voit le même décor ensemble. | Développé à part. |

**➡️ Pour installer le projet et l'appli sur le casque : [Docs/INSTALLATION.md](Docs/INSTALLATION.md).**

## Fonctionnalités

* **Calage sur un QR code** posé sur le plateau (au sol, ou en hauteur sur un pied). Toutes les positions sont
  mesurées depuis le **point zéro de la scène** (par ex. le centre du nez de scène), au sol : on saisit dans le casque
  où est le QR code par rapport à ce point zéro.
* **Catalogue de décors** : un cube de test et 7 éléments du bureau d'études pour *La Cage aux Folles*
  (`CafePlagePartieA/B`, `LogesPartieA/B`, `SalonPartieA/B`, `VolumeCentral`, modèles OBJ).
  On peut y ajouter ses propres modèles ([pas à pas](Docs/INSTALLATION.md#6-ajouter-son-propre-décor)).
* **Manipulation à la manette droite** : sélection au laser (objet en surbrillance bleue), déplacer, agrandir
  (au joystick ou en tapant un pourcentage), tourner, étirer une face (EXTRUDE), changer la couleur, supprimer,
  afficher les infos (position, dimensions, volume).
* **Saisie à la main** (Interaction SDK), à une ou deux mains, sans perdre les modifications de l'objet.
* **Sauvegarde / chargement** d'implantations dans le casque (une sauvegarde par scène du spectacle).
* **Tableau CSV** pour le bureau d'études (s'ouvre dans Excel), écrit à chaque sauvegarde.

## Utilisation dans le casque

Les textes de l'appli sont **en anglais**. Le premier lancement (permission, calage) est détaillé dans
[INSTALLATION.md](Docs/INSTALLATION.md#4-premier-lancement-et-calage-sur-le-qr-code).

### Calage

1. Coller le QR code (au moins 15 cm de côté) **à la verticale, face à la salle**, bien parallèle au nez de scène,
   sur un pied ou un support (ou le poser à plat au sol). **Un seul QR code** visible sur le plateau. L'idéal est de mettre le QR code vers le fond de la scène et le plus grand possible pour que la détection du QR code soit stable. 
2. Au lancement, l'écran **QR code offset** demande où est le QR code par rapport au point zéro (X, Y, en mètres) et la
   hauteur de son bord du bas au-dessus du sol. Il est pré-rempli avec les dernières valeurs : **Confirm** si le QR
   code n'a pas bougé.
3. Regarder le QR code : le menu affiche « QR codes detected: 1 », le décor est calé. Le calage est ensuite **figé**
   (le décor ne tremble pas) : si le QR code est déplacé, relancer l'appli.

### Menu général (main gauche)

| Bouton | Action |
|---|---|
| **Enable Access** | Demande la permission « données spatiales » (indispensable pour détecter le QR code) |
| **QR Tracking** | Active / coupe la détection du QR code |
| **Import** | Liste des décors (écran *Objects*) : un clic fait apparaître le décor posé au sol, 1,5 m devant soi, tourné vers soi |
| **Erase all** | Supprime tous les décors posés |
| **Save** | **+ New Save** (nom tapé au clavier du menu ; vide = nom automatique « Save 1 »…) ou clic sur une sauvegarde existante pour l'écraser ; petite croix = la supprimer |
| **Load** | Recharge une sauvegarde (la plus récente en haut). La première ligne (« QR offset: … ») rouvre l'écran **QR code offset** |
| **− / +** (au-dessus du menu) | Replie / rouvre le menu |

Une sauvegarde par scène du spectacle : placer la scène 1, *Save* « Scene 1 » ; *Erase all*, placer la scène 2,
*Save* « Scene 2 »… Chaque sauvegarde est écrite sur le casque au moment du clic. Le lendemain, *Load* « Scene 1 »
remet le décor au même endroit du plateau, même si le QR code a été recollé ailleurs (il suffit de saisir son
nouvel offset avant de charger).

### Manipuler un objet (manette droite)

Viser un objet avec le laser + **gâchette** : il passe en surbrillance et son menu s'ouvre **à la main gauche**, à
droite du menu général. Pendant une manipulation : **gâchette = valider**, **B = annuler** (retour à l'état d'avant).

| Bouton du menu objet | Commande |
|---|---|
| **MOVE** | L'objet est accroché au bout du laser : tourner la manette le déplace (loin si l'objet est loin). **Joystick haut / bas** = l'éloigner / le rapprocher, **gauche / droite** = le décaler sur le côté (un joystick poussé presque droit devant ou derrière, à moins de 25°, ne décale pas sur le côté), **grip maintenu + joystick haut / bas** = monter / descendre. Plus l'objet est loin, plus il va vite. Il ne descend pas sous le sol |
| **SCALE** | **Joystick haut / bas** = agrandir / rétrécir (l'objet reste posé au sol, ou accroché par le haut s'il est au cintre) ; ou taper un **pourcentage de la taille d'origine** sur le pavé de la main gauche puis **APPLY** (`100` = taille d'origine en gardant la forme donnée par EXTRUDE ; `150` = 1,5 fois). Pas de limite de taille |
| **ROTATE** | **Joystick gauche / droite** = tourner sur lui-même, autour de la verticale seulement (pas de bascule) |
| **EXTRUDE** | Viser une face au laser (bleu) + **gâchette** pour la choisir (orange), **ou** cliquer **TOP / BOTTOM / LEFT / RIGHT / FRONT / BACK** à la main gauche (vus depuis soi ; utile pour la face du haut d'un grand décor). Puis **joystick haut / bas** = pousser / rentrer la face (grip maintenu = plus lent). La longueur s'affiche sur la face ; la face opposée ne bouge pas ; 5 cm minimum |
| **INFO** | Panneau avec la position du point d'origine depuis le point zéro (X, Y, Z, comme le CSV), l'angle, les dimensions, la hauteur (et celle du haut de l'objet) et le volume de la boîte englobante. *INFO* à nouveau pour le fermer |
| **COLOR** | Palette de 15 teintes de décor (dont le gris d'origine des décors du bureau d'études) ; la couleur est sauvegardée |
| **DELETE** | Supprime l'objet |
| **x** | Ferme le menu |

Autres gestes :
* **Objet dans lequel on se trouve** (ex. le volume central) : viser le vide (aucun autre objet) + gâchette
  sélectionne l'objet dans lequel est la manette.
* **Prendre à la main (grab)**, à une ou deux mains : l'objet garde sa taille et ses étirements et reste parallèle au
  sol (seule la rotation autour de la verticale suit les mains) ; écarter / rapprocher les deux mains = agrandir /
  rétrécir.

### Le tableau CSV pour le bureau d'études

À chaque *Save*, l'appli écrit dans le casque `<nom>.json` (relu par *Load*) et `<nom>.csv` (le tableau, qui s'ouvre
dans Excel). Pour les récupérer sur un ordinateur : [INSTALLATION.md](Docs/INSTALLATION.md#5-récupérer-les-sauvegardes-et-le-tableau-csv).

Une ligne par élément, colonnes séparées par des **points-virgules**, nombres avec une **virgule** (Excel en
français ; décocher *Csv Decimal Comma* sur le composant `LayoutManager` de l'objet *Managers* pour avoir des points).
Repère du bureau d'études, vu depuis la salle : origine = **point zéro de la scène, au sol**.

| Colonne | Contenu |
|---|---|
| Scene | Nom de la sauvegarde (une sauvegarde par scène) |
| Item ID / Item name | Identifiant et nom de l'élément dans le catalogue |
| Mounting | `Floor` (au sol) ou `Flown` (au cintre) |
| Reference point | Point dont on donne la position : `Bottom centre` (au sol) ou `Top centre` (au cintre) |
| X from zero (m) | Positif vers **cour** (à droite vu de la salle), négatif vers jardin |
| Y from zero (m) | Positif vers le **lointain**, négatif vers la salle |
| Z above floor (m) | Hauteur du point de référence au-dessus du sol du plateau |
| Rotation (deg) | Autour de la verticale, + = sens inverse des aiguilles d'une montre vu de dessus (comme AutoCAD). 0 = l'avant du modèle regarde vers le lointain ; 180 = vers la salle |
| Tilt X / Tilt Y (deg) | Objet penché : + = le haut penche vers la salle (X) / vers cour (Y). 0 si l'objet est droit |
| Width / Depth / Height (m) | Taille réelle de l'objet, le long de ses propres axes |
| Scale width / depth / height | Facteur appliqué au modèle 3D (1 = taille d'origine) |
| Color (hex RGB) | Couleur choisie (ex. `#B23A3A`) ; vide = couleur d'origine du modèle |

Le CSV est un **export** : le modifier ne change rien dans le casque (c'est le JSON qui est rechargé).

## Limites connues

* Le **tableau CSV n'est pas relu** par l'appli : une scène se recharge depuis sa sauvegarde JSON.
* **EXTRUDE étire tout le modèle** (sa boîte englobante) : parfait pour un mur, une porte pleine ou un praticable,
  mais les détails d'un modèle travaillé (moulures, poignée) sont déformés.
* Le **menu général ne bloque pas le laser** : viser un de ses boutons avec un décor juste derrière sélectionne aussi
  le décor.
* Le **calage est figé** à la première détection du QR code : pour recaler après avoir déplacé le QR code, relancer
  l'appli.

## Pistes d'amélioration

* **Vue de dessus** : un plan du plateau vu de dessus avec les éléments posés (prévu en V6 dans le cahier des
  charges).
* **Réduire l'échelle de tout le décor** pour répéter dans un lieu plus petit que la scène (1:2, 1:5…). Tout le
  décor étant enfant de `StageRoot`, une échelle appliquée à cet objet suffirait, et les sauvegardes resteraient en
  mètres réels.
* **Exporter la scène au format OBJ** : écrire dans un seul fichier les modèles posés, avec leur position, rotation,
  échelle et étirements, pour les rouvrir dans les logiciels du bureau d'études. Il faudra cocher *Read/Write* à
  l'import des modèles (désactivé aujourd'hui) pour lire leurs maillages dans le casque.
* **Obstruction par les objets de la vraie vie** : aujourd'hui le décor virtuel est toujours dessiné par-dessus le
  réel (un comédien qui passe devant est caché par le décor). L'API de profondeur de Meta (*Depth API*, occlusion)
  permettrait de cacher le décor derrière les personnes et les objets réels.
* **Extrusion d'un modèle détaillé** : ne prolonger qu'une partie du modèle sans déformer ses détails, en le découpant
  en morceaux (partie fixe + partie extensible) ou en modifiant son maillage (ProBuilder).

##  Technologies utilisées

* **Moteur 3D / AR** : Unity 6.3 (version exacte : `6000.3.26f1`), Universal Render Pipeline
* **SDK Réalité Augmentée** : Meta XR Core SDK / Meta MR Utility Kit (MRUK) / Interaction SDK (v207)
* **Matériel cible** : casque Meta Quest 3 (application autonome, sans ordinateur)
* **Gestion de version** : Git + Git LFS / GitHub

##  Organisation du dépôt

```
/                         ← le projet Unity (à ouvrir avec Unity Hub)
├── Assets/
│   ├── _Bauprobe/        ← tout le code et les contenus du projet
│   │   ├── Scenes/       MenuGeneral.unity (la scène de l'appli) ; Sandbox/ : scènes de test
│   │   ├── Core/         le repère du plateau, l'offset du QR code
│   │   ├── Calibration/  QR code → position du plateau
│   │   ├── Decor/        catalogue (Catalog/), modèles 3D (Models/), prefabs (Prefabs/)
│   │   ├── Manipulation/ laser, menu de l'objet, déplacer / agrandir / tourner / étirer, grab
│   │   ├── UI/           menu général et ses écrans ; prefabs des menus dans UI/Prefabs/
│   │   ├── Persistence/  sauvegarde / chargement (JSON) et tableau CSV
│   │   ├── Editor/       outil « Bauprobe > Ajouter au catalogue »
│   │   └── Tests/        tests automatiques (EditMode)
│   ├── ThirdParty/       exemple « QR Code Detection » de Meta (voir Crédits)
│   └── (dossiers créés par les SDK : Oculus, MetaXR, XR, Resources...)
├── Packages/             liste des packages Unity
├── ProjectSettings/      réglages du projet
└── Docs/
    ├── INSTALLATION.md   installer le projet et l'appli, pas à pas ; ajouter son décor
    └── ARCHITECTURE.md   comment le code est organisé
```

## L'équipe

Ce projet est réalisé par des élèves de Mines Paris - PSL :

* Théophile Boisson de Chazournes
* Charlotte Guillemoto
* Violette Lecuyer
* Mona Sidhoum

**Collaborateurs :**

* Marion Delemazure (experte et consultante dans le secteur des Industries Culturelles et Créatives)
* Thomas Lauret (Théâtre du Châtelet)
* Jean-Philippe Lagarde (Théâtre du Châtelet)
* Inès Mimouni (Théâtre du Châtelet)

##  Crédits et Assets tiers

* **Détection de QR Code** : le dossier `Assets/ThirdParty/MetaQRCodeDetection` provient du dépôt officiel [Unity-MRUtilityKitSample](https://github.com/oculus-samples/Unity-MRUtilityKitSample) fourni par Oculus/Meta. Ses scripts ne sont pas modifiés : notre code (`StageCalibrator`) écoute directement MRUK. Son prefab `QRCodeSampleUI` sert de support au menu général `BauprobeMenu`.
* **Modèles 3D** de *La Cage aux Folles* : fournis par le bureau d'études du Théâtre du Châtelet.

##  Licence

Ce projet a été réalisé dans un cadre académique par des élèves de Mines Paris - PSL pour le Théâtre du Châtelet.
Tous droits réservés : son utilisation est réservée au Théâtre du Châtelet. Toute utilisation, copie, modification ou diffusion de l’application, de son code ou des modèles 3D de décor en dehors du Théâtre du Châtelet nécessite son accord préalable.

Le code provenant du [MR Utility Kit Sample](https://github.com/oculus-samples/Unity-MRUtilityKitSample) reste soumis à sa propre licence : voir [Docs/licences/LICENSE-Meta-MRUtilityKitSample.txt](Docs/licences/LICENSE-Meta-MRUtilityKitSample.txt).
