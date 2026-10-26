# Architecture du projet

Ce document explique **comment le code est rangé et pourquoi**. Il est écrit pour des débutant·e·s en Unity.

## L'idée centrale : le QR code est le « point zéro » du plateau

On colle un QR code **à plat sur le sol du plateau** (par exemple au centre, au bord de la scène).
Quand le casque le voit, il sait où est la scène.

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
| **Core** | Le repère du plateau, partagé par tous | `StageSpace` |
| **Calibration** | Trouver le QR code et y caler le plateau | `StageCalibrator` |
| **Decor** | La liste des décors disponibles et leur apparition | `DecorItemDefinition`, `DecorCatalog`, `DecorSpawner`, `DecorInstance` |
| **Manipulation** | Les règles quand on déplace / tourne / agrandit un objet | `DecorConstraints` |
| **Persistence** | Sauvegarder / recharger une implantation (fichier JSON) | `LayoutManager`, `LayoutSerializer`, `LayoutData` |
| **UI** | Les menus dans le casque | `DecorMenu` |
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
3. **Le metteur en scène choisit un décor dans le menu** (`DecorMenu`).
   Le menu crée un bouton par fiche du catalogue (`DecorCatalog`).
4. **`DecorSpawner`** fait apparaître l'objet au sol, 1,5 m devant le casque, comme enfant de `StageRoot`.
5. **Le metteur en scène attrape l'objet** avec les mains ou les manettes. C'est l'**Interaction SDK** de
   Meta qui gère ça (composant `Grabbable`), on ne le recode pas.
   **`DecorConstraints`** passe juste après pour garder l'objet au sol, droit, et dans des tailles raisonnables.
6. **`LayoutManager.Save()`** écrit la liste des objets et leurs positions sur le plateau dans un fichier JSON
   sur le casque. **`LayoutManager.Load()`** la recharge.

## Ajouter un nouveau décor (sans code)

1. Mettre le modèle 3D (`.fbx`, `.glb`...) dans `Assets/_Bauprobe/Decor/Models/`.
   Le modèle doit être **en mètres, à l'échelle 1:1, avec le pivot en bas** (au sol).
2. En faire un prefab dans `Assets/_Bauprobe/Decor/Prefabs/` et y ajouter :
   * un **Collider** (par ex. Box Collider) ;
   * les composants de saisie de l'Interaction SDK : clic droit sur l'objet >
     *Interaction SDK > Add Grab Interaction* (vérifier que la mise à l'échelle à deux mains est activée) ;
   * le composant **`DecorConstraints`**.
3. Clic droit dans `Assets/_Bauprobe/Decor/Catalog/` > *Create > Bauprobe > Élément de décor*.
   Remplir le nom et glisser le prefab.
4. Ajouter cette fiche dans la liste du **`DecorCatalog`**.

## Le dossier ThirdParty

`Assets/ThirdParty/MetaQRCodeDetection` est une copie **non modifiée** de l'exemple « QR Code Detection » de Meta.
Il sert à afficher les QR codes détectés (pratique pour déboguer). **On n'y touche pas** :
si on a besoin d'un comportement différent, on écrit notre propre script dans `_Bauprobe`.
Comme ça, on peut mettre à jour l'exemple Meta sans perdre notre travail.

## Règles de code

* Un script = une classe = un fichier du même nom.
* Namespace = nom du module : `Bauprobe.Decor`, `Bauprobe.Calibration`, etc.
* Les champs réglables dans l'Inspector : `[SerializeField] private`, avec un `[Tooltip("...")]` en français.
* Pas de `FindObjectOfType` / `GameObject.Find` : on glisse les références dans l'Inspector.
* Un module parle à un autre par des **méthodes publiques** ou des **événements C#** (`event Action`),
  comme `DecorSpawner.Spawned` ou `StageSpace.Calibrated`.
