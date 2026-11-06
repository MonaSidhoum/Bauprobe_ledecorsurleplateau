# Architecture du projet

Ce document explique **comment le code est rangé et pourquoi**. Il est écrit pour des débutant·e·s en Unity.

## L'idée centrale : un point zéro sur le plateau, retrouvé grâce au QR code

On pose un QR code sur le plateau (par exemple au centre, au bord de la scène) : à plat au sol, ou collé sur un
support. Quand le casque le voit, il sait où est la scène.

Toutes les positions sont mesurées depuis le **point zéro de la scène** choisi avec le bureau d'études (par ex. le
centre du nez de scène), **au sol**, et pas forcément depuis le QR code. Au lancement, l'écran **« QR code offset »**
du menu général demande où est le QR code par rapport à ce point zéro (X, Y en mètres) et la hauteur de son bord
du bas au-dessus du sol. Ces valeurs sont gardées sur le casque (`QrOffsetStore`) : pas besoin de *Build*.
`StageCalibrator` place alors le repère du plateau **au sol, à la verticale du centre du QR code**, puis décale
`StageRoot` sur le point zéro. X = Y = 0 : le QR code **est** le point zéro.
Détails : [A_FAIRE_DANS_UNITY.md](A_FAIRE_DANS_UNITY.md#offset-et-hauteur-du-qr-code-dans-le-casque).

Ensuite, **chaque objet de décor est placé par rapport à ce point zéro**, et pas par rapport au casque.
Si le metteur en scène pose une table « 2 m à gauche du point zéro », elle sera encore au même
endroit le lendemain, même si le casque a été éteint entre-temps ou si le QR code a été recollé ailleurs
(il suffit de saisir son nouvel offset).

Dans la scène Unity, ça donne cette hiérarchie :

```
QRAnchor          ← déplacé sur le QR code par StageCalibrator
└── StageRoot     ← porte le composant StageSpace : c'est « le plateau » (décalé au point zéro si offset)
    ├── Table     ← les objets de décor sont TOUJOURS enfants de StageRoot
    ├── Escalier
    └── ...
```

Les positions des objets (`localPosition`) sont donc des positions **sur le plateau**, en mètres.

## Les modules

Chaque dossier de `Assets/_Bauprobe` est un **module** : une partie de l'appli qui fait une seule chose.

| Module | Rôle | Scripts principaux |
|---|---|---|
| **Core** | Le repère du plateau, partagé par tous ; l'offset et la hauteur du QR code saisis dans le casque ; le passage au repère du bureau d'études | `StageSpace`, `QrOffset`, `QrOffsetStore`, `StagePlan` |
| **Calibration** | Trouver le QR code et y caler le plateau | `StageCalibrator` |
| **Decor** | La liste des décors disponibles et leur apparition | `DecorItemDefinition`, `DecorCatalog`, `DecorSpawner`, `DecorInstance` |
| **Manipulation** | Déplacer / tourner / agrandir / étirer un objet | `DecorConstraints` ; à la manette : `InteractableObject`, `ObjectMenu`, `RightControllerRaySelector`, `LevelButton`, `FaceHighlight`, `SelectionHighlight` ; grab à la main : `DecorGrabTransformer` ; panneaux de la main gauche : `HandPanel`, `ScaleKeypad`, `FacePicker`, `ObjectInfoPanel` |
| **Persistence** | Sauvegarder / recharger une implantation (fichier JSON) ; tableau CSV pour le bureau d'études | `LayoutManager`, `LayoutSerializer`, `LayoutData`, `LayoutCsv` |
| **UI** | Les menus dans le casque | `BauprobeMenuLogic` (menu général : Importer, Supprimer tout, suivi QR), `BauprobeMenuScreens` (écrans Décors, Sauvegarder, Charger, QR code offset) |
| **Tests** | Tests automatiques (Window > General > Test Runner) | `StageSpaceTests`, `LayoutSerializerTests` |

### Qui a le droit d'utiliser qui ?

Chaque module a un fichier `.asmdef` (*Assembly Definition*). C'est un fichier qui dit à Unity
« ce dossier est un bloc de code séparé, et il a le droit d'utiliser tels autres blocs ».

```
            Core
          ▲  ▲  ▲
          │  │  └──────────────┐
   Calibration   Decor ◄───────┤
                 ▲  ▲  ▲       │
                 │  │  └── UI  │
      Manipulation  Persistence┘
```

* **Core** ne dépend de personne.
* **Decor** utilise Core.
* **Manipulation**, **Persistence** et **UI** utilisent Decor (et Core).
* **Calibration** utilise Core et le kit Meta MRUK.
* **Manipulation** utilise aussi `Oculus.VR` (manettes) ; **UI** utilise aussi MRUK (état du suivi QR).

Si quelqu'un essaie par erreur d'utiliser l'UI depuis Decor, Unity affiche une erreur de compilation.
C'est voulu : ça évite que tout le code finisse par dépendre de tout.

> Si vous ajoutez un script qui a besoin d'un autre module ou d'un package (par ex. l'Interaction SDK),
> ajoutez la référence dans le `.asmdef` du dossier (cliquez dessus dans Unity > Inspector >
> *Assembly Definition References*).

## Comment ça marche, étape par étape

1. **Au démarrage**, MRUK (le kit de Meta) cherche les QR codes.
2. **`StageCalibrator`** reçoit l'événement « QR code trouvé ». Si le texte du QR code correspond
   à `expectedPayload` (laisser vide pour accepter n'importe lequel), il déplace `QRAnchor` dessus.
   Le repère est gardé **horizontal** (option `keepLevel`) : seul le « cap » du QR code compte.
   Par défaut, il **fige** ensuite la position (`lockAfterFirstDetection`) pour éviter que le décor tremble.
   Les méthodes `Lock()` / `Unlock()` peuvent être branchées sur des boutons.
3. **Le metteur en scène ouvre « Importer » dans le menu général** (`BauprobeMenuLogic`) et choisit un décor.
   Le bouton n°0 de la liste importe le décor n°0 du catalogue (`DecorCatalog`), le bouton n°1 le décor n°1, etc.
4. **`DecorSpawner`** fait apparaître l'objet 1,5 m devant le casque, posé sur le plateau, comme enfant de `StageRoot`.
   « Supprimer tout » passe aussi par `DecorSpawner`.
5. **Le metteur en scène manipule l'objet à la manette droite** :
   * il vise l'objet avec le laser (`RightControllerRaySelector`) et appuie sur la gâchette ;
   * l'objet sélectionné est en **surbrillance** (voile bleu qui pulse, `SelectionHighlight`, créé par le code) tant
     que son menu est ouvert et pendant Déplacer / Scale / Rotation. Les matériaux de l'objet ne sont pas touchés :
     ses modèles 3D sont redessinés par-dessus avec `Manipulation/Resources/SelectionHighlight.mat` (sa couleur
     *Base Map* règle celle du voile) ;
   * le menu de l'objet s'ouvre (`ObjectMenu`) : Supprimer, Couleur, Déplacer, Scale, Rotation, Extrude.
     Il se place **à la main gauche, collé au menu général** : à sa droite quand il est ouvert, au-dessus du
     bouton « + » quand il est replié. Le menu général est trouvé tout seul sous la main gauche (objet
     « BauprobeMenu ») ; « replié » est détecté en regardant ce qui en est visible (`MenuDocking`), donc sans
     dépendre du code du menu général. Sans menu général, le menu objet s'affiche au-dessus de l'objet ;
   * pendant Déplacer / Scale / Rotation (`InteractableObject`) : joystick = ajuster, gâchette = valider, B = annuler.

   **Déplacer** : l'objet est accroché à la manette, comme au bout du laser (sa position est gardée dans le repère
   de la manette) : tourner le poignet le déplace, d'autant plus loin qu'il est loin. Le joystick l'éloigne /
   le rapproche le long du laser (ligne manette → objet), le décale sur le côté, ou le monte / descend (grip
   maintenu) ; la vitesse augmente avec la distance (*Distance Speed Factor*), entre *Min / Max Move Distance*.
   Le laser relie la manette à l'objet pendant le déplacement ; le laser de Meta (`OVRRayHelper` de la manette
   droite, toujours tout droit) est caché pendant ce temps par `RightControllerRaySelector` (*Hide Meta Ray While Moving*). Il ne descend pas sous le sol du plateau
   (option *Prevent Below Stage* de `InteractableObject`, cochée par défaut).
   **Rotation** : joystick gauche / droite = tourner sur lui-même, haut / bas = basculer. La rotation se fait
   autour du centre de l'objet, et son point le plus bas (sol) ou le plus haut (cintre) reste à la même hauteur.
   Pendant la Rotation, un bouton **Align with floor** (`LevelButton`, créé par le code, rien à ajouter dans la scène)
   s'affiche à la main gauche, collé au menu général comme le menu objet (`GeneralMenuDock`) : la direction de
   l'objet la plus proche de la verticale devient exactement
   verticale, avec la plus petite rotation possible. L'objet est donc posé à plat sur la face la plus proche.
   Pour interdire la bascule sur un décor, cocher *Keep Upright* sur son `DecorConstraints`
   (et décocher *Allow Tilt* sur son `InteractableObject`).
   **EXTRUDE** (étirer une face) : on vise une face au laser, gâchette pour la choisir, puis joystick pour la
   pousser / rentrer. `DecorInstance.ExtrudeFace` change l'échelle de l'objet sur un seul axe et le recale pour
   que la face opposée ne bouge pas. `FaceHighlight` (créé par le code) colore la face et affiche sa longueur.
   Le bouton EXTRUDE est ajouté par `ObjectMenu` en copiant le bouton Scale : le prefab du menu n'est pas modifié.
   La taille n'est pas limitée (pas de minimum ni de maximum au Scale) ; l'étirement ne descend pas sous 5 cm.
6. **`LayoutManager.Save()`** écrit la liste des objets et leurs positions sur le plateau dans un fichier JSON
   sur le casque, **et le même contenu en tableau CSV** pour le bureau d'études (`LayoutCsv`, même nom, `.csv`).
   **`LayoutManager.Load()`** recharge le JSON (le CSV sert seulement à l'export).
   Le CSV est dans le **repère du bureau d'études** (`StagePlan`) : X à droite, Y vers l'avant, **Z = hauteur**,
   angle en degrés dans le sens inverse des aiguilles d'une montre vu de dessus. Le JSON garde le repère de Unity
   (Y = hauteur). Format détaillé : [A_FAIRE_DANS_UNITY.md](A_FAIRE_DANS_UNITY.md#exporter-le-tableau-csv-pour-le-bureau-détudes).

## Saisie à la main et panneaux de la main gauche (fin de projet)

* **Grab (Interaction SDK)** : chaque décor a un `Grabbable` de Meta. Son comportement par défaut perdait les
  modifications de l'objet à deux mains et le faisait tourner dans tous les sens. `InteractableObject` le remplace au
  démarrage (`Awake`) par **`DecorGrabTransformer`** (option *Use Decor Grab*), grâce à
  `Grabbable.InjectOptionalOneGrabTransformer` / `InjectOptionalTwoGrabTransformer` : rien à changer dans les prefabs.
  Les calculs (une main, deux mains, rotation autour de la verticale seulement) sont dans **`GrabMath`**, testé.
* **Sélection depuis l'intérieur** : `InteractableObject` tient la liste des objets de la scène (`All`) et sait si un point
  est dans sa boîte englobante (`Encloses`). `RightControllerRaySelector` s'en sert quand le laser ne vise rien.
* **Panneaux de la main gauche** (créés par le code, base commune **`HandPanel`**) : `ScaleKeypad` (Scale en %),
  `FacePicker` (faces d'EXTRUDE), `ObjectInfoPanel` (INFO, à droite du menu objet). Ils se placent comme le menu objet
  (`GeneralMenuDock`). Quand le laser les vise, la gâchette clique dessus sans valider la manipulation
  (`ManipulationPanels.IsPointerOverAny`). Le bouton INFO est ajouté par `ObjectMenu` en copiant le bouton Scale.

## Code venu de la branche `test/mr-quest3`

Les scripts écrits sur la branche de test ont été intégrés :

| Script | Rangé dans | Ce qu'il fait |
|---|---|---|
| `BauprobeMenuLogic` | `UI/` | Menu général : permission QR, suivi QR, Importer, Supprimer tout |
| `InteractableObject` | `Manipulation/` | Les 5 actions sur un objet : supprimer, couleur, déplacer, scale, rotation (manette droite) |
| `ObjectMenu` | `Manipulation/` | Le menu de l'objet sélectionné, collé au menu général de la main gauche |
| `RightControllerRaySelector` | `Manipulation/` | Le laser de la manette droite qui sélectionne un objet |
| `SelectionHighlight` | `Manipulation/` | La surbrillance de l'objet sélectionné (créée par le code) |

`ObjectMenu` est dans `Manipulation` (et pas dans `UI`) parce qu'il appelle `InteractableObject` et
inversement : deux scripts qui s'appellent l'un l'autre doivent être dans le même module.

La scène de la branche de test (`menu général.unity`) est devenue `Scenes/MenuGeneral.unity`.

### Choix faits par l'équipe

* **Une seule scène** : `Scenes/MenuGeneral.unity` (l'ancienne `Main.unity`, avec le cube posé à la main, a été supprimée).
* **Faire apparaître un objet** : le bouton « Importer » de `BauprobeMenuLogic`, qui passe par `DecorSpawner`
  et le catalogue (indispensable pour la sauvegarde).
* **Manipuler un objet** : à la manette (`InteractableObject`). Les objets peuvent être basculés (choix de l’équipe, octobre 2026) ; avant, ils restaient toujours droits.
* **Reste à faire plus tard** : `BauprobeMenuLogic` et `StageCalibrator` écoutent tous les deux MRUK.
  C'est sans danger, mais à terme le menu pourrait simplement lire `StageCalibrator.IsQrLocated`.

## Ajouter un nouveau décor (sans code)

1. Mettre le modèle 3D (`.fbx`, `.glb`...) dans `Assets/_Bauprobe/Decor/Models/`.
   Le modèle doit être **en mètres, à l'échelle 1:1**. Son pivot peut être n'importe où : le **point d'origine**
   (bas au centre pour le sol, haut au centre pour le cintre) est calculé automatiquement à partir du modèle.
2. En faire un prefab dans `Assets/_Bauprobe/Decor/Prefabs/` et y ajouter :
   * un **Collider** (par ex. Box Collider) ;
   * les composants de saisie de l'Interaction SDK : clic droit sur l'objet >
     *Interaction SDK > Add Grab Interaction* (vérifier que la mise à l'échelle à deux mains est activée) ;
   * le composant **`DecorConstraints`**.
3. Clic droit dans `Assets/_Bauprobe/Decor/Catalog/` > *Create > Bauprobe > Élément de décor*.
   Remplir le nom, glisser le prefab et choisir **Sol** ou **Cintre** (pour le cintre, régler aussi la
   hauteur d'accroche à l'import).
4. Ajouter cette fiche dans la liste du **`DecorCatalog`**.

## Le dossier ThirdParty

`Assets/ThirdParty/MetaQRCodeDetection` est une copie de l'exemple « QR Code Detection » de Meta.
Il sert à afficher les QR codes détectés (pratique pour déboguer). Seul son prefab `QRCodeSampleUI` a été
modifié sur la branche de test (il accueille le menu Bauprobe). **Pour le reste, on n'y touche pas** :
si on a besoin d'un comportement différent, on écrit notre propre script dans `_Bauprobe`.
Comme ça, on peut mettre à jour l'exemple Meta sans perdre notre travail.

## Règles de code

* Un script = une classe = un fichier du même nom.
* Namespace = nom du module : `Bauprobe.Decor`, `Bauprobe.Calibration`, etc.
* Les champs réglables dans l'Inspector : `[SerializeField] private`, avec un `[Tooltip("...")]` en français.
* Pas de `FindObjectOfType` / `GameObject.Find` : on glisse les références dans l'Inspector.
* Un module parle à un autre par des **méthodes publiques** ou des **événements C#** (`event Action`),
  comme `DecorSpawner.Spawned` ou `StageSpace.Calibrated`.
