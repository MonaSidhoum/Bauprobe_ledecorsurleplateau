# Unity : mise en route et reste à faire

La restructuration du projet est **terminée et testée sur le Quest 3** : import d'un décor, sélection au laser,
déplacement / agrandissement / rotation à la manette, objets toujours droits, sauvegarde / chargement.

Ce document contient :
1. ce qu'il faut faire pour ouvrir le projet sur **un nouvel ordinateur** ;
2. l'**état actuel** de la scène (pour s'y retrouver) ;
3. la liste de ce qui **reste à faire**.

---

## 1. Ouvrir le projet sur un nouvel ordinateur

1. Installer **Unity 6000.3.26f1** (Unity Hub) avec **Android Build Support** (+ OpenJDK, Android SDK & NDK).
2. Une seule fois par ordinateur : `git lfs install`
3. Cloner le dépôt (voir le [README](../README.md)), puis dans Unity Hub : *Add > Add project from disk* >
   choisir **la racine du dépôt** (le dossier qui contient `Assets/`, `Packages/`, `ProjectSettings/`).
4. Attendre l'import (10 à 20 min la première fois).
5. Ouvrir la **Console** (*Window > General > Console*) : il ne doit pas y avoir d'erreur rouge.
6. *File > Build Profiles* : vérifier que la plateforme active est **Meta Quest / Android** (sinon *Switch Platform*).
7. Ouvrir la scène `Assets/_Bauprobe/Scenes/MenuGeneral.unity`.
8. (Optionnel) *Window > General > Test Runner* > **EditMode** > *Run All* : les 2 tests doivent être verts.

Si vous aviez un ancien clone (avec le dossier `Unity-MRUtilityKitSample/`), ne le réutilisez pas :
re-clonez, ou supprimez au moins son dossier `Library/`.

### Installer sur le casque

1. Casque en **mode développeur** (appli Meta Horizon > Appareils > Paramètres du casque > Mode développeur).
2. Brancher le casque en USB-C et accepter « Autoriser le débogage USB » dans le casque.
3. *File > Build Profiles* > *Run Device* = le Quest 3 > **Build And Run**.
4. Au premier lancement, accepter la permission **« données spatiales »** (sinon : bouton *Permission* du menu).

---

## 2. État actuel de la scène `MenuGeneral`

```
MenuGeneral
├── [BuildingBlock] Camera Rig
│   └── TrackingSpace
│       ├── LeftHandAnchor
│       │   └── QRCodeSampleUI            (prefab Meta)
│       │       └── Canvas [V]
│       │           └── BauprobeMenu      (prefab _Bauprobe/UI/Prefabs/BauprobeMenu) : menu général, main gauche
│       └── RightHandAnchor
│           └── Laser                     Right Controller Ray Selector + Line Renderer
├── Managers                              Decor Spawner + Layout Manager
├── MRUK                                  détection des QR codes (Meta)
├── QRAnchor                              Stage Calibrator : se cale sur le QR code
│   └── StageRoot                         Stage Space : le repère du plateau, parent de tous les décors
└── ObjectMenu                            (prefab _Bauprobe/UI/Prefabs/ObjectMenu) : menu au-dessus de l'objet sélectionné
```

| Élément | Où | Rôle |
|---|---|---|
| Menu général | `BauprobeMenu` (main gauche) | Permission, suivi QR, **Importer**, **Supprimer tout**, **Sauvegarder**, **Charger** |
| Catalogue | `Decor/Catalog/DecorCatalog.asset` | Liste des décors importables (pour l'instant : `Cube`) |
| Laser | `Laser` (main droite) | Vise un objet ; gâchette = ouvre son menu |
| Menu objet | `ObjectMenu` (racine) | Supprimer, Couleur, Déplacer, Scale, Rotation |
| Décor | prefab `Decor/Prefabs/Cube` | *Interactable Object* (manette) + *Decor Constraints* (toujours droit) |

Pour ajouter un nouveau décor au catalogue : voir [ARCHITECTURE.md](ARCHITECTURE.md#ajouter-un-nouveau-décor-sans-code).

Rappel : **une seule personne à la fois** modifie `MenuGeneral.unity`. Pour modifier un menu,
ouvrez son prefab (`_Bauprobe/UI/Prefabs/`) plutôt que la scène.

---

## 3. Reste à faire

### Manipulation (gênant sur le plateau)

- [ ] **Scale** : l'objet grandit à partir de son centre, donc il s'enfonce dans le sol quand on l'agrandit
      (et flotte quand on le rétrécit). Il faudrait le reposer au sol après chaque changement de taille.
- [ ] **Déplacer** : l'objet suit la manette dans les 3 dimensions et peut flotter ou passer sous la scène.
      Décider : le faire glisser sur le plateau uniquement, ou garder la possibilité de le soulever.
- [ ] **Menu objet** : il s'affiche au-dessus de l'objet. Pour un grand décor (mur de 4 m), il est hors de portée.
      Le placer plutôt à hauteur des yeux, devant l'objet.
- [ ] **Laser** : le menu général ne bloque pas le laser. Viser un bouton du menu général avec un objet
      derrière sélectionne aussi l'objet.

### Nettoyage du code

- [ ] Deux limites de taille en double : `InteractableObject` (0,02 à 5) et le catalogue (0,1 à 10, via
      `DecorConstraints`). Ensemble, elles donnent 0,1 à 5. Ne garder que celles du catalogue (réglables par décor).
- [ ] `ObjectMenu` plante si l'un de ses 6 boutons n'est pas renseigné : le rendre tolérant.
- [ ] `BauprobeMenuLogic` et `StageCalibrator` écoutent tous les deux MRUK : le menu pourrait lire
      `StageCalibrator.IsQrLocated`.
- [ ] `BauprobeMenu` est rangé dans le prefab Meta `QRCodeSampleUI` (dossier `ThirdParty`). À terme, lui donner
      son propre Canvas pour ne plus dépendre de l'exemple Meta.

### Fonctionnalités

- [ ] **Couleur** non sauvegardée : la sauvegarde garde la position, la rotation et la taille, pas la couleur.
- [ ] **Vrais décors** : importer les modèles 3D du bureau d'études (`Decor/Models/`) et les ajouter au catalogue,
      avec un bouton par décor dans la liste « Importer ».
- [ ] **QR code du plateau** : choisir le texte du QR code officiel et le mettre dans *Expected Payload*
      (Stage Calibrator), pour ignorer les autres QR codes présents dans la salle.
- [ ] **Plusieurs implantations** : aujourd'hui, Sauvegarder / Charger utilisent toujours le même nom
      (`implantation`). Permettre d'en garder plusieurs.
- [ ] **Points de vue depuis la salle** (orchestre, balcons...) : prévu dans le README, pas encore commencé.
