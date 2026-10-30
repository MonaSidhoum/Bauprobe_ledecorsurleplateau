# Architecture du projet

Ce document explique **comment le code est rangé et pourquoi**. Il est écrit pour des débutant·e·s en Unity.

## L'idée centrale : le QR code est le « point zéro » du plateau

On pose un QR code sur le plateau (par exemple au centre, au bord de la scène) : à plat au sol, ou collé sur un
support. Quand le casque le voit, il sait où est la scène.

Si le QR code n'est pas au sol, on indique la distance entre **le bas du QR code et le sol** dans
`Core/Resources/StageSettings.asset`. `StageCalibrator` place alors le repère du plateau **au sol, à la verticale
du centre du QR code** : la hauteur 0 du décor est le vrai sol. Détails : [A_FAIRE_DANS_UNITY.md](A_FAIRE_DANS_UNITY.md#régler-la-hauteur-du-qr-code-à-refaire-à-chaque-nouveau-lieu).

Ensuite, **chaque objet de décor est placé par rapport à ce QR code**, et pas par rapport au casque.
Si le metteur en scène pose une table « 2 m à gauche du QR code », elle sera encore au même
endroit le lendemain, même si le casque a été éteint entre-temps.

Dans la scène Unity, ça donne cette hiérarchie :

```
QRAnchor          ← déplacé sur le QR code par StageCalibrator
└── StageRoot     ← porte le composant StageSpace : c'est « le plateau »
    ├── Table     ← les objets de décor sont TOUJOURS enfants de StageRoot
    ├── Escalier
    └── ...
```

Les positions des objets (`localPosition`) sont donc des positions **sur le plateau**, en mètres.

## Les modules

Chaque dossier de `Assets/_Bauprobe` est un **module** : une partie de l'appli qui fait une seule chose.

| Module | Rôle | Scripts principaux |
|---|---|---|
| **Core** | Le repère du plateau, partagé par tous ; ses réglages (hauteur du QR code) | `StageSpace`, `StageSettings` |
| **Calibration** | Trouver le QR code et y caler le plateau | `StageCalibrator` |
| **Decor** | La liste des décors disponibles et leur apparition | `DecorItemDefinition`, `DecorCatalog`, `DecorSpawner`, `DecorInstance` |
| **Manipulation** | Déplacer / tourner / agrandir un objet | `DecorConstraints` ; à la manette : `InteractableObject`, `ObjectMenu`, `RightControllerRaySelector` |
| **Persistence** | Sauvegarder / recharger une implantation (fichier JSON) | `LayoutManager`, `LayoutSerializer`, `LayoutData` |
| **UI** | Les menus dans le casque | `BauprobeMenuLogic` (menu général : Importer, Supprimer tout, suivi QR) |
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
   * un petit menu s'ouvre au-dessus de l'objet (`ObjectMenu`) : Supprimer, Couleur, Déplacer, Scale, Rotation ;
   * pendant Déplacer / Scale / Rotation (`InteractableObject`) : joystick = ajuster, gâchette = valider, B = annuler.

   **Déplacer** : l'objet suit la manette, et le joystick le déplace dans toutes les directions
   (grip maintenu = monter / descendre). Il ne descend pas sous le sol du plateau (option *Prevent Below Stage*
   de `InteractableObject`, cochée par défaut).
   **Rotation** : joystick gauche / droite = tourner sur lui-même, haut / bas = basculer. La rotation se fait
   autour du centre de l'objet, et son point le plus bas (sol) ou le plus haut (cintre) reste à la même hauteur.
   Pendant la Rotation, un bouton **Aligner au sol** (`LevelButton`, créé par le code, rien à ajouter dans la scène)
   s'affiche au-dessus de l'objet : la direction de l'objet la plus proche de la verticale devient exactement
   verticale, avec la plus petite rotation possible. L'objet est donc posé à plat sur la face la plus proche.
   Pour interdire la bascule sur un décor, cocher *Keep Upright* sur son `DecorConstraints`
   (et décocher *Allow Tilt* sur son `InteractableObject`).
6. **`LayoutManager.Save()`** écrit la liste des objets et leurs positions sur le plateau dans un fichier JSON
   sur le casque. **`LayoutManager.Load()`** la recharge.

## Code venu de la branche `test/mr-quest3`

Les scripts écrits sur la branche de test ont été intégrés :

| Script | Rangé dans | Ce qu'il fait |
|---|---|---|
| `BauprobeMenuLogic` | `UI/` | Menu général : permission QR, suivi QR, Importer, Supprimer tout |
| `InteractableObject` | `Manipulation/` | Les 5 actions sur un objet : supprimer, couleur, déplacer, scale, rotation (manette droite) |
| `ObjectMenu` | `Manipulation/` | Le petit menu qui s'affiche au-dessus de l'objet sélectionné |
| `RightControllerRaySelector` | `Manipulation/` | Le laser de la manette droite qui sélectionne un objet |

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
