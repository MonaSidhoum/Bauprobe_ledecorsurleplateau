# Architecture du projet

Ce document explique **comment le code est rangé et pourquoi**, pour quelqu'un qui voudrait le lire ou le modifier.
Il est écrit pour des débutant·e·s en Unity. Pour installer le projet : [INSTALLATION.md](INSTALLATION.md).

## L'idée centrale : un point zéro sur le plateau, retrouvé grâce au QR code

On pose un QR code sur le plateau : à plat au sol, ou collé sur un support. Quand le casque le voit, il sait où est
la scène.

Toutes les positions sont mesurées depuis le **point zéro de la scène** choisi avec le bureau d'études (par ex. le
centre du nez de scène), **au sol**, et pas forcément depuis le QR code. Au lancement, l'écran **« QR code offset »**
du menu général demande où est le QR code par rapport à ce point zéro (X, Y en mètres) et la hauteur de son bord
du bas au-dessus du sol. Ces valeurs sont gardées sur le casque (`QrOffsetStore`, dans les `PlayerPrefs`) : pas
besoin de *Build*. `StageCalibrator` place alors le repère du plateau **au sol, à la verticale du centre du QR code**,
puis décale `StageRoot` sur le point zéro. X = Y = 0 : le QR code **est** le point zéro.
Marche à suivre dans le casque : [INSTALLATION.md](INSTALLATION.md#4-premier-lancement-et-calage-sur-le-qr-code).

Ensuite, **chaque objet de décor est placé par rapport à ce point zéro**, et pas par rapport au casque.
Si le metteur en scène pose une table « 2 m à gauche du point zéro », elle sera encore au même
endroit le lendemain, même si le casque a été éteint entre-temps ou si le QR code a été recollé ailleurs
(il suffit de saisir son nouvel offset).

Dans la scène `MenuGeneral`, ça donne cette hiérarchie :

```
QRAnchor          ← déplacé sur le QR code par StageCalibrator (au sol, à l'horizontale)
└── StageRoot     ← porte le composant StageSpace : c'est « le plateau » (décalé au point zéro)
    ├── Table     ← les objets de décor sont TOUJOURS enfants de StageRoot
    ├── Escalier
    └── ...
```

Les positions des objets (`localPosition`) sont donc des positions **sur le plateau**, en mètres.

## Contenu de la scène `MenuGeneral`

```
MenuGeneral
├── [BuildingBlock] Camera Rig
│   └── TrackingSpace
│       ├── LeftHandAnchor
│       │   └── QRCodeSampleUI            (prefab Meta)
│       │       └── Canvas
│       │           └── BauprobeMenu      (prefab UI/Prefabs/BauprobeMenu) : menu général, main gauche
│       └── RightHandAnchor
│           └── Laser                     Right Controller Ray Selector + Line Renderer
├── Managers                              Decor Spawner + Layout Manager
├── MRUK                                  détection des QR codes (Meta)
├── QRAnchor                              Stage Calibrator : se cale sur le QR code
│   └── StageRoot                         Stage Space : le repère du plateau, parent de tous les décors
└── ObjectMenu                            (prefab UI/Prefabs/ObjectMenu) : menu de l'objet sélectionné
```

Les panneaux de la main gauche (pavé du Scale, faces d'EXTRUDE, INFO, *Align with floor*) et la surbrillance de
l'objet sélectionné sont **créés par le code** : ils ne sont ni dans la scène ni dans des prefabs.

## Les modules

Chaque dossier de `Assets/_Bauprobe` est un **module** : une partie de l'appli qui fait une seule chose.

| Module | Rôle | Scripts principaux |
|---|---|---|
| **Core** | Le repère du plateau, partagé par tous ; l'offset et la hauteur du QR code saisis dans le casque ; le passage au repère du bureau d'études | `StageSpace`, `QrOffset`, `QrOffsetStore`, `StagePlan` |
| **Calibration** | Trouver le QR code et y caler le plateau | `StageCalibrator` |
| **Decor** | La liste des décors disponibles, leur apparition, leur point d'origine et leurs faces | `DecorItemDefinition`, `DecorCatalog`, `DecorSpawner`, `DecorInstance`, `DecorOrigin`, `DecorFace` |
| **Manipulation** | Sélectionner, déplacer, tourner, agrandir, étirer, colorer un objet | `DecorConstraints` ; à la manette : `InteractableObject`, `ObjectMenu`, `ObjectColorPalette`, `RightControllerRaySelector`, `LevelButton`, `FaceHighlight`, `SelectionHighlight` ; grab à la main : `DecorGrabTransformer`, `GrabMath` ; panneaux de la main gauche : `HandPanel`, `ScaleKeypad`, `FacePicker`, `ObjectInfoPanel`, `GeneralMenuDock` |
| **Persistence** | Sauvegarder / recharger une implantation (fichier JSON) ; tableau CSV pour le bureau d'études | `LayoutManager`, `LayoutSerializer`, `LayoutData`, `LayoutCsv` |
| **UI** | Le menu général dans le casque | `BauprobeMenuLogic` (permission, suivi QR, Import, Erase all), `BauprobeMenuScreens` (écrans Objects, Save, Load, QR code offset, bouton − / +) |
| **Editor** | Outils de l'éditeur Unity (pas dans l'appli) | `AddToDecorCatalog` (*Bauprobe > Ajouter au catalogue*), `Decor/Editor/DecorPrefabTools` |
| **Tests** | Tests automatiques (*Window > General > Test Runner* > EditMode) | `StageSpaceTests`, `StagePlanTests`, `QrOffsetStoreTests`, `DecorOriginTests`, `DecorManipulationTests`, `GrabAndFacesTests`, `LayoutSerializerTests`, `LayoutCsvTests`, `MenuDockingTests`, `DecorLimitsTests` |

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
* **Manipulation** utilise aussi `Oculus.VR` (manettes) et l'Interaction SDK (grab) ; **UI** utilise aussi MRUK
  (état du suivi QR).

Si quelqu'un essaie par erreur d'utiliser l'UI depuis Decor, Unity affiche une erreur de compilation.
C'est voulu : ça évite que tout le code finisse par dépendre de tout.

> Si vous ajoutez un script qui a besoin d'un autre module ou d'un package, ajoutez la référence dans le `.asmdef`
> du dossier (cliquez dessus dans Unity > Inspector > *Assembly Definition References*).

`ObjectMenu` est dans `Manipulation` (et pas dans `UI`) parce qu'il appelle `InteractableObject` et
inversement : deux scripts qui s'appellent l'un l'autre doivent être dans le même module.

## Comment ça marche, étape par étape

1. **Au démarrage**, MRUK (le kit de Meta) cherche les QR codes.
2. **`StageCalibrator`** reçoit l'événement « QR code trouvé » et déplace `QRAnchor` dessus
   (n'importe quel QR code est accepté, son texte n'est pas vérifié).
   Le repère est gardé **horizontal** (option `keepLevel`) : seul le « cap » du QR code compte. Pour un QR code
   vertical, le côté « avant » est choisi d'après la position du casque (qui voit forcément le côté imprimé).
   Par défaut, il **fige** ensuite la position (`lockAfterFirstDetection`) pour éviter que le décor tremble.
   Les méthodes `Lock()` / `Unlock()` existent mais ne sont branchées sur aucun bouton.
3. **La personne ouvre *Import* dans le menu général** (`BauprobeMenuScreens`) et choisit un décor dans la liste,
   qui est celle du catalogue (`DecorCatalog`, dans l'ordre de l'asset).
4. **`DecorSpawner`** fait apparaître l'objet 1,5 m devant le casque, tourné vers la personne, posé sur le plateau
   (ou à sa hauteur d'accroche s'il est au cintre), comme enfant de `StageRoot`. *Erase all* passe aussi par
   `DecorSpawner`.
5. **Le point d'origine** de chaque objet (bas au centre pour un élément au sol, haut au centre pour un élément au
   cintre) est calculé à partir de ses modèles 3D (`DecorOrigin`) : le pivot des modèles peut être n'importe où.
   C'est ce point qui est sauvegardé, qui reste fixe pendant le Scale, et qui est écrit dans le CSV.
6. **La personne manipule l'objet à la manette droite** :
   * elle vise l'objet avec le laser (`RightControllerRaySelector`) et appuie sur la gâchette. Si le laser ne vise
     rien et que la manette est **dans** un objet, c'est cet objet qui est choisi (`InteractableObject.FindEnclosing`) ;
   * l'objet sélectionné est en **surbrillance** (voile bleu qui pulse, `SelectionHighlight`). Les matériaux de
     l'objet ne sont pas touchés : ses modèles 3D sont redessinés par-dessus avec
     `Manipulation/Resources/SelectionHighlight.mat` (sa couleur *Base Map* règle celle du voile) ;
   * le menu de l'objet s'ouvre (`ObjectMenu`). Il se place **à la main gauche, collé au menu général** : à sa droite
     quand il est ouvert, au-dessus du bouton « + » quand il est replié (`GeneralMenuDock`, `MenuDocking`). Les
     boutons EXTRUDE et INFO sont ajoutés par le code en copiant le bouton SCALE : le prefab du menu ne contient
     que DELETE, COLOR, MOVE, SCALE, ROTATE et la croix ;
   * pendant MOVE / SCALE / ROTATE / EXTRUDE (`InteractableObject`) : joystick = ajuster, gâchette = valider,
     B = annuler. Quand le laser vise un panneau de la main gauche, la gâchette clique sur le panneau au lieu de
     valider (`ManipulationPanels.IsPointerOverAny`).

   **MOVE** : l'objet est accroché à la manette (sa position est gardée dans le repère de la manette) : tourner le
   poignet le déplace, d'autant plus loin qu'il est loin. Le joystick l'éloigne / le rapproche le long du laser, le
   décale sur le côté, ou le monte / descend (grip maintenu) ; la vitesse augmente avec la distance
   (*Distance Speed Factor*), entre *Min / Max Move Distance*. Le laser de Meta est caché pendant ce temps
   (*Hide Meta Ray While Moving*). L'objet ne descend pas sous le sol (*Prevent Below Stage*).

   **SCALE** : uniforme, autour du point d'origine (`DecorInstance.SetScaleAroundOrigin`), sans limite de taille. Le
   pavé `ScaleKeypad` applique un pourcentage de la taille du prefab (`ApplyScalePercentOfOriginal`).

   **ROTATE** : autour du centre de l'objet ; son point le plus bas (sol) ou le plus haut (cintre) reste à la même
   hauteur. Le bouton **Align with floor** (`LevelButton`) rend verticale la direction de l'objet la plus proche de
   la verticale, avec la plus petite rotation possible. Un décor dont *Keep Upright* est coché sur son
   `DecorConstraints` reste toujours droit (c'est le cas des 7 décors de *La Cage aux Folles* ; pas du Cube).

   **EXTRUDE** : `DecorInstance.ExtrudeFace` change l'échelle de l'objet sur un seul axe et le recale pour que la face
   opposée ne bouge pas (5 cm minimum, `DecorLimits.MinFaceLength`). `FaceHighlight` colore la face et affiche sa
   longueur ; `FacePicker` choisit une face par boutons, vue depuis la personne.

   **COLOR** : `ObjectColorPalette` (sur le prefab `ObjectMenu`) ouvre une palette de 15 pastilles ; la couleur est
   retenue par `DecorInstance` pour la sauvegarde.

   **INFO** : `ObjectInfoPanel` affiche la position du point d'origine dans le repère du bureau d'études
   (`StagePlan`, comme le CSV), l'angle, les dimensions et le volume de la boîte englobante.
7. **Grab (Interaction SDK)** : chaque décor a un `Grabbable` de Meta (et un enfant `ISDK_HandGrabInteraction`). Le
   comportement par défaut du SDK perdait les modifications de l'objet à deux mains et le faisait tourner dans tous
   les sens. `InteractableObject` le remplace au démarrage (`Awake`) par **`DecorGrabTransformer`** (option
   *Use Decor Grab*), grâce à `Grabbable.InjectOptionalOneGrabTransformer` / `InjectOptionalTwoGrabTransformer` :
   rien à changer dans les prefabs. Les calculs (une main, deux mains, rotation autour de la verticale seulement)
   sont dans **`GrabMath`**, testé.
8. **`LayoutManager.Save()`** écrit la liste des objets (identifiant du catalogue, point d'origine, rotation,
   échelle, couleur) dans un fichier JSON sur le casque (`Application.persistentDataPath/Layouts`), **et le même
   contenu en tableau CSV** pour le bureau d'études (`LayoutCsv`, même nom, `.csv`).
   **`LayoutManager.Load()`** recharge le JSON (le CSV sert seulement à l'export). Les anciennes versions du format
   JSON se rechargent toujours (`LayoutData.version`).
   Le CSV est dans le **repère du bureau d'études** (`StagePlan`) : X à droite, Y vers l'avant, **Z = hauteur**,
   angle en degrés dans le sens inverse des aiguilles d'une montre vu de dessus. Le JSON garde le repère de Unity
   (Y = hauteur). Colonnes : [README](../README.md#le-tableau-csv-pour-le-bureau-détudes).

## Ajouter un décor

Pas à pas (outil *Bauprobe > Ajouter au catalogue*, ou à la main) :
[INSTALLATION.md](INSTALLATION.md#6-ajouter-son-propre-décor).

Deux raccourcis aident à préparer un prefab de décor (`Decor/Editor/DecorPrefabTools.cs`), par clic droit sur le
composant dans l'Inspector : *Transform > Bauprobe : poser les modèles sur le pivot (centre-bas)* et
*BoxCollider > Bauprobe : ajuster aux modèles 3D enfants* (affiche aussi les dimensions en mètres dans la Console).

## Le dossier ThirdParty

`Assets/ThirdParty/MetaQRCodeDetection` est une copie de l'exemple « QR Code Detection » de Meta.
Son prefab `QRCodeSampleUI` sert de support au menu général (`BauprobeMenu` est placé dans son Canvas, et
`BauprobeMenuLogic` cache le panneau d'origine). **Pour le reste, on n'y touche pas** : si on a besoin d'un
comportement différent, on écrit notre propre script dans `_Bauprobe`.

## Règles de code

* Un script = une classe = un fichier du même nom.
* Namespace = nom du module : `Bauprobe.Decor`, `Bauprobe.Calibration`, etc. (sauf `BauprobeMenuLogic`, sans
  namespace).
* Les champs réglables dans l'Inspector : `[SerializeField] private`, avec un `[Tooltip("...")]` en français.
* Un module parle à un autre par des **méthodes publiques** ou des **événements C#** (`event Action`),
  comme `DecorSpawner.Spawned` ou `StageSpace.Calibrated`.
