# Unity : mise en route et reste à faire

La restructuration du projet est **terminée et testée sur le Quest 3** : import d'un décor, sélection au laser,
déplacement / agrandissement / rotation à la manette, sauvegarde / chargement.

Ce document contient :
1. ce qu'il faut faire pour ouvrir le projet sur **un nouvel ordinateur** ;
2. l'**état actuel** de la scène (pour s'y retrouver) ;
3. le **suivi du cahier des charges** (version 1, 9 octobre 2026) : ce qui est fait, ce qui manque ;
4. les **corrections techniques** qui restent à faire.

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

### Régler la hauteur du QR code (à refaire à chaque nouveau lieu)

Le décor est calé sur le QR code. Si le QR code **n'est pas posé à plat au sol** (collé sur un mur, un pied,
une table…), il faut dire à l'appli à quelle hauteur il se trouve, sinon tout le décor est décalé en hauteur.

1. **Mesurer** au mètre la distance **verticale** entre **le bord du bas du QR code** et **le sol du plateau**.
   * On mesure le **bord du bas** du carré imprimé, pas son centre ni le bord de la feuille.
   * Exemple : bord du bas du QR code à 1,20 m du sol → `1.2`. QR code posé à plat au sol → `0`.
2. **Indiquer la valeur** (en mètres, avec un point) dans `Assets/_Bauprobe/Core/Resources/StageSettings.asset`,
   champ **Qr Bottom Height Above Floor** :
   * dans Unity : cliquer sur le fichier dans la fenêtre *Project*, modifier la valeur dans l'*Inspector*, puis *Ctrl+S* ;
   * ou dans un éditeur de texte : dernière ligne du fichier, `qrBottomHeightAboveFloor: 1.2`.
3. **Refaire un *Build And Run*** : la valeur est intégrée à l'appli, elle ne change pas toute seule sur le casque.
4. **Committer** `StageSettings.asset` (sur une branche, comme d'habitude) pour que toute l'équipe ait la même valeur.
5. **Vérifier dans le casque** : après le calage, importer le cube. Il doit être posé **exactement sur le sol**.
   La Console (en Play avec Link) affiche aussi « Plateau calé : sol à 1,20 m sous le bas du QR code ».

**Comment ça marche.** Quand le casque détecte le QR code, `StageCalibrator` :
1. calcule la hauteur du **coin le plus bas** du QR code (ça marche qu'il soit à plat, droit sur un mur ou penché) ;
2. retire la distance réglée dans `StageSettings` : c'est la hauteur du **vrai sol** ;
3. pose le repère du plateau (`QRAnchor` > `StageRoot`) **à cette hauteur, à la verticale du centre du QR code**,
   tourné comme le QR code mais toujours à l'horizontale.

Tout le décor est enfant de `StageRoot` : la hauteur 0 du décor est donc le vrai sol. C'est ce sol qu'utilisent
l'apparition des objets, *Keep On Floor*, la hauteur d'accroche au cintre et le blocage « pas sous le sol ».

**À savoir.**
* Si on **déplace le QR code** ou qu'on change de lieu, il faut **remesurer** et refaire un *Build*.
* Une erreur de mesure de 5 cm décale tout le décor de 5 cm en hauteur.
* Les positions sauvegardées sont relatives au point du sol sous le QR code : remettre le QR code **au même endroit**
  pour retrouver une implantation.

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
| Décor | prefab `Decor/Prefabs/Cube` | *Interactable Object* (manette) + *Decor Constraints* (taille, sol) |
| Réglages du plateau | `Core/Resources/StageSettings.asset` | Distance entre le bas du QR code et le sol ([voir plus haut](#régler-la-hauteur-du-qr-code-à-refaire-à-chaque-nouveau-lieu)) |

Pour ajouter un nouveau décor au catalogue : voir [ARCHITECTURE.md](ARCHITECTURE.md#ajouter-un-nouveau-décor-sans-code).

Rappel : **une seule personne à la fois** modifie `MenuGeneral.unity`. Pour modifier un menu,
ouvrez son prefab (`_Bauprobe/UI/Prefabs/`) plutôt que la scène.

---

## 3. Cahier des charges : suivi

Le résumé du cahier des charges est dans le [README](../README.md#cahier-des-charges). Ici, chaque point est
traduit en tâches pour notre volet, **Le décor sur le plateau**.

Légende : ✅ fait · 🟡 en partie · ⬜ pas commencé

### 3.1 Le parcours de l'utilisateur

| # | Étape du cahier des charges | État | Ce qui existe / ce qui manque |
|---|---|---|---|
| 1 | Importer le décor du bureau d'études et le tableau de positionnement | 🟡 | *Importer* fait apparaître un décor du catalogue, mais seul un **cube de test** existe. Pas encore de vrais modèles, ni de lecture du tableau. |
| 2 | Afficher en AR la scène 1, puis les suivantes | ⬜ | Le calage sur le QR code marche, mais il n'y a qu'**une seule implantation** à la fois : pas de notion de « scène 1, 2, 3… ». |
| 3 | Modifier placement et transformation, puis enregistrer dans la scène | 🟡 | Déplacer / Scale / Rotation et *Sauvegarder* marchent, mais toujours dans le même fichier (`implantation`). |
| 4 | Exporter le tableau modifié et les nouveaux modèles | ⬜ | La sauvegarde est un fichier JSON sur le casque, qu'on ne peut pas ouvrir dans Excel. |

**Tâches :**

- [ ] **Format du tableau de positionnement** : le définir **avec le volet « Voir la même chose »** (format commun
      obligatoire). Une ligne par élément, par exemple : scène, identifiant du modèle, position X / Y / Z (en m,
      dans le repère du QR code), rotation, échelle, au sol ou au cintre.
      Piste : un **CSV**, qu'Excel ouvre et enregistre, est bien plus simple à lire dans Unity qu'un `.xlsx`.
- [ ] **Importer le tableau** : lire le fichier et créer les éléments à leur place, scène par scène
      (réutiliser `LayoutSerializer` / `LayoutData`, à compléter avec le numéro de scène).
- [ ] **Importer les modèles du bureau d'études** : choisir le format d'échange (FBX, glTF, OBJ…), les placer
      dans `Decor/Models/` et les ajouter au catalogue, avec un bouton par décor dans la liste « Importer ».
- [ ] **Gérer plusieurs scènes** : ajouter au menu général un choix « Scène précédente / Scène suivante » qui
      affiche l'implantation de la scène choisie (remplace l'actuel nom unique `implantation`).
- [ ] **Enregistrer dans la scène** : *Sauvegarder* écrit dans la scène affichée.
- [ ] **Exporter** le tableau modifié dans le même format que l'import, et le récupérer sur un ordinateur
      (câble USB / MQDH, ou partage réseau à étudier).
- [ ] **Exporter les nouveaux modèles** : préciser avec les commanditaires ce qu'on appelle un « nouveau modèle »
      (un modèle redimensionné ? un modèle créé dans l'appli ?).

### 3.2 Règles sur le décor

- [x] **Point d'origine** : pour un élément au sol, le point le plus bas au centre de l'objet ; pour un élément
      au cintre, le point le plus haut. Calculé automatiquement à partir des modèles 3D (`DecorOrigin`), donc le
      pivot des modèles peut être n'importe où. Réglage **Sol / Cintre** (et hauteur d'accroche pour le cintre)
      dans chaque fiche `DecorItemDefinition`. La sauvegarde enregistre désormais la position du point d'origine
      (format version 2 ; les anciennes sauvegardes se rechargent toujours).
      Ça règle aussi le problème du **Scale** qui enfonçait l'objet dans le sol (section 4).
      Reste à vérifier dans le casque, et à décider s'il faut empêcher un élément au cintre de descendre sous le plateau.
- [ ] **Réduire l'échelle de tout le décor** si l'espace manque : un réglage d'échelle globale sur `StageRoot`
      (par exemple 1:1, 1:2, 1:5) dans le menu général. La sauvegarde doit rester en mètres réels.
- [ ] **Bibliothèque standard** : modéliser et ajouter au catalogue des praticables, une draperie simple, des
      pendrillons et des frises, en tailles standard. Demander les dimensions standard au théâtre.
- [ ] **Premier jeu de modèles** : récupérer auprès de Thomas les modèles de *La Cage aux Folles*.
- [ ] **Vue de dessus** : prévue en V6, pas pour l'instant.

### 3.3 Contraintes

- [x] **Aucun élément de plus de 18 m** : la taille réelle de l'objet (calculée à partir de ses modèles 3D) ne peut
      pas dépasser 18 m sur sa plus grande dimension (`DecorLimits`). Le *Scale* s'arrête à 18 m, et un modèle
      importé plus grand est réduit à 18 m (avec un avertissement dans la Console).
- [ ] **Formats communs** avec « Voir la même chose » (décor et tableau) : fixer les formats avec l'autre équipe
      **avant** de coder l'import et l'export.
- [ ] **Exécutable déployable partout, en kit transportable** : documenter le kit (mètre ruban pour mesurer la hauteur
      du QR code, casque(s), câble, chargeur,
      QR code imprimé et sa taille, ordinateur pour l'import / export) et l'installation de l'`.apk` sans Unity.
- [ ] Le Quest se met en pause quand on enlève le casque (quelques secondes de noir au retour). Pour les répétitions,
      désactiver le capteur de proximité (Meta Quest Developer Hub > *Device Manager*) et l'ajouter au mode d'emploi du kit.

### 3.4 Livrables

| Livrable | État |
|---|---|
| Tableau de positionnement par scène (Excel en première approche) | ⬜ format à définir (section 3.1) |
| Tableau modifié et nouveaux modèles 3D | ⬜ dépend de l'export (section 3.1) |
| Code source | 🟡 ce dépôt ; à finaliser |
| Documentation utilisateur | 🟡 le README explique l'utilisation dans le casque ; à compléter par un **mode d'emploi pour le technicien** (sans Unity) |

### 3.5 Tests prévus

- [ ] Une scène de *La Cage aux Folles* sur le plateau.
- [ ] Un décor sans lien avec ce spectacle (pour vérifier que l'import marche avec d'autres modèles).
- [ ] Test commun avec le volet « Voir la même chose » : date à fixer.

### 3.6 Hors cahier des charges (à confirmer)

- [ ] **Points de vue depuis la salle** (orchestre, balcons…) : prévu au départ du projet, absent du cahier des
      charges. Demander aux commanditaires si c'est encore souhaité.

---

## 4. Reste à faire (technique)

### Manipulation (gênant sur le plateau)

- [x] **Scale** : l'objet grandit désormais autour de son **point d'origine** (section 3.2) : un élément au sol
      reste posé sur le plateau, un élément au cintre reste accroché par le haut.
- [x] **Déplacer** : le joystick déplace l'objet dans toutes les directions (avant / arrière / gauche / droite par
      rapport au regard ; grip maintenu = monter / descendre), en plus de suivre la manette, sans limite de hauteur.
      L'objet ne descend pas sous le sol du plateau (option *Prevent Below Stage* de `InteractableObject`, cochée
      par défaut ; le sol vient de la hauteur du QR code réglée dans `StageSettings`). Pour un décor qui doit
      glisser au sol uniquement, cocher *Keep On Floor* sur son `DecorConstraints`.
- [x] **Rotation** : joystick gauche / droite = tourner sur lui-même ; haut / bas = basculer vers l'avant / l'arrière
      (avant, seule la rotation autour de la verticale était possible). L'objet tourne autour de son centre sans
      s'enfoncer dans le sol. *Keep Upright* est décoché sur le prefab Cube pour permettre la bascule.
      Un bouton **Aligner au sol** apparaît au-dessus de l'objet pendant la Rotation (laser + gâchette) : il pose
      l'objet parallèle au sol sur la face la plus proche de sa position (un peu penché → redressé, très penché →
      couché sur le côté).
- [ ] **Menu objet** : il s'affiche au-dessus de l'objet. Pour un grand décor (mur de 4 m), il est hors de portée.
      Le placer plutôt à hauteur des yeux, devant l'objet.
- [ ] **Laser** : le menu général ne bloque pas le laser. Viser un bouton du menu général avec un objet
      derrière sélectionne aussi l'objet.

### Nettoyage du code

- [ ] Deux limites de taille en double : `InteractableObject` (0,02 à 5) et le catalogue (0,1 à 10, via
      `DecorConstraints`). Ensemble, elles donnent 0,1 à 5. Ne garder que celles du catalogue (réglables par décor).
      La limite de **18 m** du cahier des charges est déjà appliquée en plus (section 3.3).
- [ ] `ObjectMenu` plante si l'un de ses 6 boutons n'est pas renseigné : le rendre tolérant.
- [ ] `BauprobeMenuLogic` et `StageCalibrator` écoutent tous les deux MRUK : le menu pourrait lire
      `StageCalibrator.IsQrLocated`.
- [ ] `BauprobeMenu` est rangé dans le prefab Meta `QRCodeSampleUI` (dossier `ThirdParty`). À terme, lui donner
      son propre Canvas pour ne plus dépendre de l'exemple Meta.

### Petites fonctionnalités

- [ ] **Couleur** non sauvegardée : la sauvegarde garde la position, la rotation et la taille, pas la couleur.
- [x] **QR code pas au sol** : la distance entre le bas du QR code et le sol se règle dans
      `Core/Resources/StageSettings.asset` (*Qr Bottom Height Above Floor*, en mètres). Le repère du plateau est posé
      au vrai sol, à la verticale du QR code.
- [ ] **QR code du plateau** : choisir le texte du QR code officiel et le mettre dans *Expected Payload*
      (Stage Calibrator), pour ignorer les autres QR codes présents dans la salle.
