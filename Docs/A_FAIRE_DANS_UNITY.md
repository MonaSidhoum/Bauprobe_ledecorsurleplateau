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

Les prérequis (Git, Git Bash sur Windows, compte GitHub, Unity Hub) et le clonage sont expliqués pas à pas dans le
[README](../README.md#installation-et-prérequis). Ensuite :

1. Vérifier que **Unity 6000.3.26f1** est installé avec **Android Build Support** (+ *OpenJDK*, *Android SDK & NDK
   Tools*). Pour ajouter le module après coup : Unity Hub > *Installations* > ⚙ de la version > *Add modules*.
2. Dans Unity Hub : *Add > Add project from disk* > choisir **la racine du dépôt** (le dossier qui contient
   `Assets/`, `Packages/`, `ProjectSettings/`).
3. Attendre l'import (10 à 20 min la première fois).
4. Ouvrir la **Console** (*Window > General > Console*) : il ne doit pas y avoir d'erreur rouge.
5. *File > Build Profiles* : vérifier que la plateforme active est **Meta Quest / Android** (sinon *Switch Platform*).
6. Ouvrir la scène `Assets/_Bauprobe/Scenes/MenuGeneral.unity` (double-clic dans la fenêtre *Project*).
7. (Optionnel) *Window > General > Test Runner* > **EditMode** > *Run All* : tous les tests doivent être verts.

### Mettre à jour un clone existant

**Cas normal : `git pull` suffit.** Fermer Unity, puis dans un terminal (Git Bash sur Windows, Terminal sur
macOS / Linux), dans le dossier du projet :

```bash
cd ~/Documents/Bauprobe_ledecorsurleplateau
```

```bash
git checkout main
```

```bash
git pull
```

```bash
git lfs pull
```

Rouvrir le projet dans Unity Hub. (Adapter le chemin de la première commande à l'endroit où vous avez cloné.)

Si `git pull` refuse (« Your local changes would be overwritten… »), c'est que des fichiers du projet ont été
modifiés sur votre ordinateur. Voir lesquels avec `git status`. S'ils ne servent à rien, les annuler avec
`git restore .` puis relancer `git pull` ; sinon, les mettre de côté avant (voir l'encadré ci-dessous).

**Très ancien clone** (le projet était alors dans un sous-dossier `Unity-MRUtilityKitSample/`) : le plus simple est
de **re-cloner**. Ne supprimez pas l'ancien dossier tout de suite : renommez-le, il sert de sauvegarde.

```bash
cd ~/Documents
```

```bash
mv Bauprobe_ledecorsurleplateau Bauprobe_ancien
```

```bash
git clone https://github.com/MonaSidhoum/Bauprobe_ledecorsurleplateau
```

```bash
cd Bauprobe_ledecorsurleplateau
```

```bash
git lfs pull
```

Puis, dans Unity Hub, retirer l'ancien projet de la liste (*⋮ > Remove project from list*) et ajouter le nouveau
(*Add > Add project from disk*). Quand tout marche, supprimer `Bauprobe_ancien` à la main.

> **Attention aux fichiers ajoutés seulement sur votre ordinateur** (par exemple des modèles OBJ importés par le
> Châtelet pour les voir en AR, mais jamais envoyés sur GitHub) : ils **ne sont pas** dans un nouveau clone, et un
> *Build And Run* fait après re-clonage ne les contiendra plus. Avant de re-cloner, ou si `git pull` refuse :
> * les retrouver avec `git status` (rubrique *Untracked files*, en rouge) ;
> * les recopier depuis l'ancien dossier vers le nouveau, **avec leur fichier `.meta`** (par exemple tout le dossier
>   `Assets/_Bauprobe/Decor/Models/<décor>/`, et les prefabs et fiches du catalogue qui vont avec) ;
> * ou mieux : les envoyer sur GitHub dans une branche (voir [CONTRIBUER.md](CONTRIBUER.md)) pour que tout le monde
>   les ait.

Les sauvegardes faites **dans le casque** (implantations, offset du QR code) ne sont pas dans le projet : elles ne
risquent rien, ni avec `git pull` ni avec un re-clonage. Seule la **désinstallation** de l'appli du casque les efface.

### Installer sur le casque

**Prérequis** : un compte **Meta** qui est **propriétaire du casque** (un seul propriétaire par casque ; un autre
compte ne peut pas activer le mode développeur), et un câble **USB-C qui transfère les données**.

1. **Appairer le casque avec l'application mobile Meta Horizon** (appli pour téléphone, iOS ou Android), connectée
   avec le compte propriétaire du casque.
2. **Activer le mode développeur** dans l'appli mobile : *Appareils* > le casque > *Paramètres du casque* >
   *Mode développeur*. Si l'appli le demande, créer une « organisation » développeur (gratuite) sur
   [developer.meta.com](https://developer.meta.com/) avec le même compte. Redémarrer le casque.
3. **Faire reconnaître le casque par l'ordinateur.** La première fois, le casque branché en USB-C n'est souvent
   **pas reconnu** par Unity, et la fenêtre « Autoriser le débogage USB » n'apparaît pas forcément dans le casque.
   * **Windows** : ouvrir **Meta Horizon Link** (aussi appelé *Meta Quest Link* : c'est le même logiciel ; il est
     parfois installé en même temps que les outils Meta de Unity), et **appairer le casque** (*Appareils > Ajouter
     un casque*, en USB). Une fois appairé, il est reconnu par Unity.
   * **macOS** : Meta Horizon Link n'existe pas ; installer **Meta Quest Developer Hub** (MQDH) et y connecter le
     casque.
   * Dans le casque, accepter **« Autoriser le débogage USB »** (cocher *Toujours autoriser depuis cet ordinateur*).
4. Dans Unity : *File > Build Profiles* > *Run Device* > **Refresh** : le Quest 3 apparaît dans la liste.
   Le choisir, puis **Build And Run**. L'appli se lance toute seule dans le casque à la fin.
5. Au premier lancement, accepter la permission **« données spatiales »** (sinon : bouton **Enable Access** du menu
   général).

Ensuite, l'appli reste installée : on peut la relancer sans ordinateur, depuis la bibliothèque d'applications du
casque (*Bibliothèque* > *Sources inconnues*).

#### Si ça ne marche pas

* **Le casque n'apparaît pas dans *Run Device*** : vérifier que le câble transfère les données (en essayer un autre),
  remettre le casque, accepter le débogage USB, puis *Refresh*. Le mode développeur doit être activé (étape 2).
* **Erreurs d'autorisation avec Meta Horizon Link** (par exemple au moment de choisir le dossier où il installe les
  applications), tests USB en erreur, petites choses qui ne marchent pas : la version de Meta Horizon Link installée
  par les outils de Unity peut être défectueuse. Le plus simple est de la **réinstaller à la main** :
  1. ouvrir la page d'aide de Meta [Installer l'application Meta Quest Link](https://www.meta.com/fr-ca/help/quest/1517439565442928/)
     et cliquer sur **Télécharger l'application** (étape 1) ;
  2. lancer le fichier `Setup.exe` téléchargé : il détecte la version déjà installée et propose de la
     **désinstaller** ;
  3. une fois la désinstallation terminée, **relancer le même `Setup.exe`** pour réinstaller Meta Horizon Link ;
  4. appairer de nouveau le casque (étape 3).

### Offset et hauteur du QR code (dans le casque)

Le décor est calé sur un QR code posé sur le plateau. Toutes les positions (sauvegardes et tableau CSV) sont
mesurées depuis le **point zéro de la scène**, choisi avec le bureau d'études (par ex. le **centre du nez de scène**),
**au sol**. Le QR code n'a pas besoin d'être sur ce point : on dit à l'appli **où il est**. Tout se saisit **dans le
casque, au lancement** : pas de *Build*.

**Au lancement**, le menu général ouvre l'écran **« QR code offset »** (en anglais), pré-rempli avec les dernières
valeurs validées (gardées sur le casque d'un lancement à l'autre ; désinstaller l'appli les efface). Si le QR code n'a
pas bougé : **Confirm** directement. Sinon, cliquer sur un champ, taper la valeur au pavé (la première touche remplace
l'ancienne valeur ; `+/-` change le signe ; **point** décimal : `1.2`), puis **Confirm**. *Back* annule.
L'écran est aussi accessible à tout moment par la **première ligne de l'écran Charger** (« QR offset: … »).

Chaque champ rappelle le sens des valeurs, **vu par quelqu'un qui fait face au QR code** (depuis la salle) :

| Champ (dans le casque) | Ce qu'on mesure au mètre |
|---|---|
| **X (m)   + right / - left** | Position du **centre du QR code** depuis le point zéro : positif s'il est à **droite** du point zéro (cour), négatif à gauche (jardin) |
| **Y (m)   + far / - near** | Position du **centre du QR code** depuis le point zéro : positif s'il est plus **loin** (lointain), négatif plus près (salle) |
| **QR height (m)** | Distance **verticale** entre le **bord du bas** du carré imprimé (pas le centre, pas le bord de la feuille) et le **sol du plateau**. 0 = QR code posé au sol |

Exemple : QR code sur un pied, au milieu du plateau, 2 m derrière le nez de scène, bas du QR code à 1,20 m du sol
→ X = `0`, Y = `2`, QR height = `1.2`. Tout à 0 : le QR code (posé au sol) **est** le point zéro.

**Orientation : coller le QR code à la verticale, face à la salle**, bien parallèle au nez de scène (il n'y a pas
d'angle à saisir). L'appli prend comme « avant » (Y+) la direction dans laquelle on regarde quand on lit le QR code,
et comme X+ la droite de celui qui le lit : QR code lu depuis la salle → Y+ = lointain, X+ = cour.
Le côté est trouvé grâce à la position du casque au moment de la détection (on voit forcément le côté imprimé).
Un QR code de travers de 5° fait tourner tout le décor de 5° : soigner l'alignement.
(Un QR code posé à plat au sol marche aussi : l'avant est alors la direction de son bord du haut.)

**Vérifier une fois dans le casque** : importer le cube, le poser sur le point zéro, sauvegarder : dans le CSV, X et Y
doivent valoir environ 0, et Z environ 0 (cube posé au sol). Le déplacer de 1 m vers le lointain et resauvegarder :
Y doit augmenter de 1.

**Comment ça marche.** `StageCalibrator` cale `QRAnchor` **au sol** (bas du QR code moins sa hauteur), à la verticale
du centre du QR code et tourné comme lui (toujours à l'horizontale). Il décale ensuite `StageRoot` (son enfant) de
(-X, -Y) : `StageRoot` est le point zéro. Tout le décor étant enfant de `StageRoot`, ses positions sont mesurées depuis
le point zéro. À chaque *Confirm*, le sol et le point zéro sont replacés (`QrOffsetStore.Changed`).
Ce sol est celui qu'utilisent l'apparition des objets, *Keep On Floor*, la hauteur d'accroche au cintre et le
blocage « pas sous le sol ».

**À savoir.**
* Une erreur de mesure de 5 cm décale tout le décor de 5 cm.
* Si on change l'offset alors que du décor est déjà posé, **ce décor bouge avec le point zéro**. Saisir l'offset
  **avant** de poser ou de charger le décor.
* Les sauvegardes faites avant cette version étaient mesurées depuis le QR code : elles restent justes avec un
  offset X = Y = 0. Avec un QR code vertical, elles peuvent être tournées d'un demi-tour (le sens « avant » d'un QR
  code vertical est maintenant choisi d'après la position du casque).

**Recharger une scène le lendemain.** Les positions du JSON sont mesurées depuis le point zéro, pas depuis le QR
code. Si le QR code a été recollé ailleurs, il suffit de **saisir son nouvel offset au lancement**, puis *Charger* :
le décor revient au même endroit du plateau. L'offset utilisé au moment de la sauvegarde est recopié dans le JSON
(`qrOffset`, pour information) ; s'il est différent de l'offset actuel, la Console le signale au chargement.

Exemple de journée :
1. Jour 1 : lancer l'appli, saisir l'offset (QR code à X = 0, Y = 2, QR height 1.2), *Confirm*.
2. Placer le décor de la scène 1, *Sauvegarder* > *+ Nouvelle sauvegarde* > « Scene 1 ».
3. *Supprimer tout* (sinon le décor de la scène 1 reste et sera aussi dans la scène 2), placer la scène 2,
   *Sauvegarder* « Scene 2 ». Chaque sauvegarde est écrite sur le casque **au moment du clic** : rien d'autre à
   enregistrer à la fin.
4. Jour 2 : le QR code a été recollé ailleurs (X = 1,5, Y = 3, hauteur 1). Lancer l'appli, saisir ce nouvel offset,
   *Confirm*, puis *Charger* « Scene 1 » : le décor est au même endroit que la veille.

### Exporter le tableau CSV pour le bureau d'études

À chaque **Sauvegarder**, l'appli écrit deux fichiers dans le dossier des sauvegardes du casque :
`<nom>.json` (relu par *Charger*) et `<nom>.csv` (le tableau pour le bureau d'études, qui s'ouvre dans Excel).
*Supprimer* une sauvegarde supprime les deux.

**Récupérer les fichiers sur un ordinateur** : brancher le casque en USB-C, accepter l'accès aux fichiers dans le
casque, puis ouvrir *Quest 3 > Stockage partagé interne > Android > data > fr.minesparis.bauprobe > files > Layouts*
(explorateur Windows, ou *Android File Transfer* sur Mac, ou le gestionnaire de fichiers de Meta Quest Developer Hub).
En ligne de commande :

```bash
adb pull /sdcard/Android/data/fr.minesparis.bauprobe/files/Layouts
```

**Format** : une ligne par élément, colonnes séparées par des **points-virgules**, nombres avec une **virgule**
(réglages d'Excel en français ; décocher *Csv Decimal Comma* sur `LayoutManager` pour avoir des points, par exemple
pour un Excel en anglais).
Positions et angles dans le **repère du bureau d'études** : l'origine est le **point zéro de la scène, au sol**
(et pas le QR code, sauf si son offset X / Y vaut 0 ; voir ci-dessus). Les titres de colonnes sont **en anglais**
et rappellent le sens des valeurs (vu depuis la salle) :

| Colonne | Contenu |
|---|---|
| Scene | Nom de la sauvegarde, c'est-à-dire de la scène (une sauvegarde par scène) |
| Item ID / Item name | Identifiant et nom de l'élément dans le catalogue |
| Mounting | `Floor` (au sol) ou `Flown` (au cintre) |
| Reference point | Le point dont on donne la position : `Bottom centre` (bas au centre, au sol) ou `Top centre` (haut au centre, au cintre) |
| X from zero (m) [+ right / - left, seen from audience] | Depuis le point zéro : positif vers **cour**, négatif vers jardin |
| Y from zero (m) [+ far / - near] | Depuis le point zéro : positif vers le **lointain**, négatif vers la salle |
| Z above floor (m) | Hauteur du point de référence au-dessus du **sol du plateau** |
| Rotation (deg) [+ counterclockwise seen from above, 0 = item front faces far] | Rotation autour de la verticale, comme dans AutoCAD. 0 = l'avant du modèle regarde vers le lointain ; +90 = vers jardin ; -90 = vers cour ; 180 = vers la salle |
| Tilt X (deg) [+ top leans near] | Objet penché vers l'avant / l'arrière : positif = le haut penche vers la salle. 0 si l'objet est droit |
| Tilt Y (deg) [+ top leans right] | Objet penché sur le côté : positif = le haut penche vers cour. 0 si l'objet est droit |
| Width / Depth / Height (m) | Taille réelle de l'objet, le long de ses propres axes (avant rotation) |
| Scale width / depth / height | Facteur appliqué au modèle 3D (1 = taille d'origine) |
| Color (hex RGB) [empty = original model colour] | Couleur choisie avec le bouton Color, en code hexadécimal RVB (ex. `#B23A3A` = rouge théâtre, `#E0E0E0` = gris des décors) ; case vide = couleur d'origine du modèle |

Pour un objet à la fois penché et tourné : on le penche (Tilt Y, puis Tilt X), puis on le tourne de *Rotation*
autour de la verticale. À l'import, un objet est tourné vers la personne qui l'importe : sa *Rotation* dépend donc de
l'endroit où elle regardait (environ 180 si elle regardait vers le lointain).

Attention : le JSON garde le repère de Unity (**Y = hauteur**) ; seul le CSV utilise **Z = hauteur**.
Modifier le CSV ne change rien dans le casque (pas encore d'import du tableau).

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
└── ObjectMenu                            (prefab _Bauprobe/UI/Prefabs/ObjectMenu) : menu de l'objet sélectionné, collé au menu général (main gauche)
```

| Élément | Où | Rôle |
|---|---|---|
| Menu général | `BauprobeMenu` (main gauche) | Enable Access (permission), suivi QR, **Importer**, **Supprimer tout**, **Sauvegarder**, **Charger** |
| Catalogue | `Decor/Catalog/DecorCatalog.asset` | Liste des décors importables : `Cube` (test) et 7 éléments du bureau d'études pour *La Cage aux Folles* (`CafePlagePartieA/B`, `LogesPartieA/B`, `SalonPartieA/B`, `Volumecentral`) |
| Laser | `Laser` (main droite) | Vise un objet ; gâchette = ouvre son menu |
| Menu objet | `ObjectMenu` (racine) | Supprimer, Couleur, Déplacer, Scale, Rotation, Extrude (ajouté par le code sous Scale). S'affiche à la main gauche, à droite du menu général (au-dessus du « + » s'il est replié). Taille : *Size Multiplier* |
| Décors | prefabs `Decor/Prefabs/` (`Cube` + les 7 décors) | *Interactable Object* (manette) + *Decor Constraints* (taille, sol). Modèles OBJ dans `Decor/Models/<décor>/`, importés en mm → m (*Scale Factor* 0,001) |
| Offset et hauteur du QR code | saisis dans le casque (écran « QR code offset ») | Gardés sur le casque, pas dans le projet ([voir plus haut](#offset-et-hauteur-du-qr-code-dans-le-casque)) |

Pour ajouter un nouveau décor au catalogue : voir [ARCHITECTURE.md](ARCHITECTURE.md#ajouter-un-nouveau-décor-sans-code).
Deux raccourcis aident à préparer un prefab de décor (`Decor/Editor/DecorPrefabTools.cs`), par clic droit sur le
composant dans l'Inspector : *Transform > Bauprobe : poser les modèles sur le pivot (centre-bas)* et
*BoxCollider > Bauprobe : ajuster aux modèles 3D enfants* (affiche aussi les dimensions en mètres dans la Console,
pour vérifier l'échelle par rapport aux plans).

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
| 1 | Importer le décor du bureau d'études et le tableau de positionnement | 🟡 | *Importer* propose les **7 premiers décors du bureau d'études** (OBJ, en mm convertis en m) en plus du cube de test. Pas encore de lecture du tableau. |
| 2 | Afficher en AR la scène 1, puis les suivantes | ✅ | **Une sauvegarde par scène** (« Scene 1 », « Scene 2 »…) : *Charger* la scène 1, puis *Supprimer tout* et *Charger* la scène suivante. Pas besoin d'un bouton « scène suivante » (choix de l'équipe, novembre 2026). |
| 3 | Modifier placement et transformation, puis enregistrer dans la scène | ✅ | Déplacer / Scale / Rotation / EXTRUDE, puis *Sauvegarder* sous le nom de la scène (écrase la sauvegarde existante, ou *+ Nouvelle sauvegarde*). |
| 4 | Exporter le tableau modifié et les nouveaux modèles | 🟡 | Tableau : ✅ chaque sauvegarde écrit un **tableau CSV** (Excel) dans le repère du bureau d'études ; **c'est le format retenu** ([détails](#exporter-le-tableau-csv-pour-le-bureau-détudes)). Pas d'export des modèles. |

**Tâches :**

- [x] **Format du tableau de positionnement** : **fixé**, c'est le **CSV** écrit à chaque sauvegarde (`LayoutCsv`) :
      une ligne par élément, colonnes décrites [plus haut](#exporter-le-tableau-csv-pour-le-bureau-détudes)
      (scène, identifiant, sol / cintre, position X / Y / Z depuis le point zéro, angles, dimensions, échelle).
      À transmettre tel quel au volet « Voir la même chose ».
- [ ] **Importer le tableau** : lire ce même CSV et créer les éléments à leur place (une scène = un fichier).
      En attendant, une scène se recharge depuis sa sauvegarde JSON (*Charger*).
- [x] **Importer les modèles du bureau d'études** : format retenu **OBJ** (unités en mm, *Scale Factor* 0,001 à
      l'import). 7 éléments ajoutés dans `Decor/Models/`, avec prefab et fiche catalogue chacun ; ils apparaissent
      dans la liste « Importer ».
      Reste à vérifier dans le casque (taille réelle par rapport aux plans, pose au sol) et à confirmer le format
      avec le volet « Voir la même chose ».
- [x] **Gérer plusieurs scènes** : **une sauvegarde par scène** (« Scene 1 », « Scene 2 »…). Pour passer d'une scène
      à l'autre : *Charger* la scène, puis *Supprimer tout* et *Charger* la suivante. Pas de bouton « scène
      suivante » (choix de l'équipe, novembre 2026).
- [x] **Enregistrer dans la scène** : *Sauvegarder* sous le nom de la scène (la sauvegarde existante est écrasée,
      ou *+ Nouvelle sauvegarde*). Le nom de la sauvegarde est écrit dans la colonne *Scene* du CSV.
- [x] **Exporter** le tableau modifié : un `.csv` à côté de chaque sauvegarde (`LayoutCsv`), récupérable par câble USB
      ou MQDH ([détails](#exporter-le-tableau-csv-pour-le-bureau-détudes)). Repère : point zéro du bureau d'études
      (= QR code, ou offset du QR code saisi dans le casque). Format validé (novembre 2026).
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
- [x] **Premier jeu de modèles** : récupérer auprès de Thomas les modèles de *La Cage aux Folles*.
      Fait : 7 éléments au catalogue (Café-plage, Loges, Salon en deux parties chacun, Volume central).
- [ ] **Vue de dessus** : prévue en V6, pas pour l'instant.

### 3.3 Contraintes

- [ ] **Aucun élément de plus de 18 m** : plus appliqué. Le *Scale* n'a plus aucune limite de taille (ni 18 m, ni
      limites dans les fiches du catalogue), et un modèle importé plus grand que 18 m garde sa taille.
- [x] **Formats communs** avec « Voir la même chose » (décor et tableau) : nos formats sont fixés, **OBJ** pour les
      modèles et le **CSV** de `LayoutCsv` pour le tableau. À transmettre à l'autre équipe.
- [ ] **Exécutable déployable partout, en kit transportable** : documenter le kit (mètre ruban pour mesurer la hauteur
      du QR code, casque(s), câble, chargeur,
      QR code imprimé et sa taille, ordinateur pour l'import / export) et l'installation de l'`.apk` sans Unity.
- [ ] Le Quest se met en pause quand on enlève le casque (quelques secondes de noir au retour). Pour les répétitions,
      désactiver le capteur de proximité (Meta Quest Developer Hub > *Device Manager*) et l'ajouter au mode d'emploi du kit.

### 3.4 Livrables

| Livrable | État |
|---|---|
| Tableau de positionnement par scène (Excel en première approche) | ✅ format fixé : le CSV (section 3.1) ; 🟡 import du tableau pas encore fait |
| Tableau modifié et nouveaux modèles 3D | 🟡 tableau : ✅ CSV écrit à chaque sauvegarde ; modèles : à préciser (section 3.1) |
| Code source | 🟡 ce dépôt ; à finaliser |
| Documentation utilisateur | 🟡 le README explique l'utilisation dans le casque ; à compléter par un **mode d'emploi pour le technicien** (sans Unity) |

### 3.5 Tests prévus

- [ ] Une scène de *La Cage aux Folles* sur le plateau.
      Les modèles sont prêts (7 éléments au catalogue) ; reste à les placer selon une scène et tester sur le plateau.
- [ ] Un décor sans lien avec ce spectacle (pour vérifier que l'import marche avec d'autres modèles).
- [ ] Test commun avec le volet « Voir la même chose » : date à fixer.

### 3.6 Hors cahier des charges (à confirmer)

- [ ] **Points de vue depuis la salle** (orchestre, balcons…) : prévu au départ du projet, absent du cahier des
      charges. Demander aux commanditaires si c'est encore souhaité.

### 3.7 Demandes des commanditaires (réponses de novembre 2026)

> **Priorité** : d'abord une RA et un positionnement **fonctionnels** (calage QR code, placement, sauvegarde).
> Les fonctionnalités ci-dessous viennent ensuite.

- [x] **Extrusion de face** sur les bouts de décor : sélectionner une face et la prolonger ou la raccourcir
      (dans les deux sens), par ex. pour prolonger une porte ; c'est une manière d'étirer un côté pour faire
      d'un cube un rectangle.
      **Fait (première version)** : bouton **EXTRUDE** du menu objet. On vise une face au laser (bleu), gâchette pour
      la choisir (orange), puis joystick haut / bas = pousser / rentrer la face ; la longueur s'affiche sur la face.
      La face opposée ne bouge pas ; un objet au sol reste posé. Seule limite : au moins 5 cm (pas de longueur
      maximale). L'étirement est gardé dans la sauvegarde (échelle par axe).
      **Limite** : on étire la boîte englobante de l'objet, donc tout le modèle est étiré. C'est parfait pour les
      éléments simples (porte pleine, mur, praticable, pendrillon), mais un modèle détaillé (porte avec poignée,
      moulures) aura ses détails déformés.
- [ ] **Extrusion « vraie » d'un modèle détaillé** (seulement si besoin après les tests) : ne prolonger qu'une partie
      du modèle sans déformer ses détails. Pistes : découper le modèle en morceaux (partie fixe + partie extensible),
      ou modifier le maillage (ProBuilder, Read/Write activé sur les modèles importés).

### 3.8 Retours de fin de projet (novembre 2026)

- [x] **Grab à deux mains** (fonction jugée essentielle par Thomas, à garder) : quand on prenait un objet à deux mains,
      toutes ses modifications (Scale, EXTRUDE…) étaient perdues. Le comportement de saisie de l'Interaction SDK est
      remplacé par `DecorGrabTransformer` (ajouté par le code, rien dans les prefabs) : on part toujours de la taille
      et de la position **actuelles** de l'objet ; écarter les mains l'agrandit en gardant ses proportions.
- [x] **Grab parallèle au sol** : l'objet ne tourne plus à 360° pendant le grab, seulement autour de la verticale
      (s'il a été penché avec le mode Rotation, il le reste). Réglage *Keep Level* sur `DecorGrabTransformer`.
- [x] **Sélectionner un objet quand on est dedans** (ex. le module central) : le laser ne touche pas les parois vues de
      l'intérieur. Si le laser ne vise aucun autre objet, la gâchette sélectionne l'objet dans lequel est la manette
      (le plus petit, s'il y en a plusieurs).
- [x] **INFO** (menu objet, en anglais) : position du point d'origine depuis le point zéro (X, Y, Z, même repère que le
      CSV), angle, dimensions, hauteur (et hauteur du haut de l'objet), volume de la boîte englobante.
- [x] **Scale en valeur** : pavé numérique à la main gauche pendant le Scale ; on tape un pourcentage de la taille
      actuelle (ex. `150` = ×1,5) puis APPLY. Le Scale au joystick marche toujours.
- [x] **EXTRUDE sur un grand objet** (face du haut impossible à viser) : boutons TOP / BOTTOM / LEFT / RIGHT / FRONT /
      BACK à la main gauche, vus depuis la personne. Marche aussi depuis l'intérieur de l'objet.
- [x] **Retirer le cylindre de la bibliothèque** : fait (PR #15, `feature/suppression-cylindre`).

---

## 4. Reste à faire (technique)

### Manipulation (gênant sur le plateau)

- [x] **Scale** : l'objet grandit désormais autour de son **point d'origine** (section 3.2) : un élément au sol
      reste posé sur le plateau, un élément au cintre reste accroché par le haut.
- [x] **Déplacer** : l'objet est accroché au bout du laser (tourner la manette le déplace, loin s'il est loin) ;
      joystick haut / bas = l'éloigner / le rapprocher le long du laser, gauche / droite = sur le côté, grip maintenu
      = monter / descendre ; la vitesse augmente avec la distance (plus d'amplitude au loin). Avant, le déplacement
      au joystick ne suivait plus la manette et restait lent au loin.
      L'objet ne descend pas sous le sol du plateau (option *Prevent Below Stage* de `InteractableObject`, cochée
      par défaut ; le sol vient de la hauteur du QR code saisie dans le casque). Pour un décor qui doit
      glisser au sol uniquement, cocher *Keep On Floor* sur son `DecorConstraints`.
- [x] **Rotation** : joystick gauche / droite = tourner sur lui-même ; haut / bas = basculer vers l'avant / l'arrière
      (avant, seule la rotation autour de la verticale était possible). L'objet tourne autour de son centre sans
      s'enfoncer dans le sol. *Keep Upright* est décoché sur le prefab Cube pour permettre la bascule.
      Un bouton **Align with floor** apparaît à la main gauche, à côté du menu général, pendant la Rotation
      (laser + gâchette) : il pose
      l'objet parallèle au sol sur la face la plus proche de sa position (un peu penché → redressé, très penché →
      couché sur le côté).
- [x] **Menu objet** : il s'affichait au-dessus de l'objet, hors de portée pour un grand décor. Il s'affiche maintenant
      **à la main gauche**, à droite du menu général (au-dessus du bouton « + » quand le menu général est replié),
      et il est **2 fois plus grand** (*Size Multiplier* sur `ObjectMenu`).
- [ ] **Laser** : le menu général ne bloque pas le laser. Viser un bouton du menu général avec un objet
      derrière sélectionne aussi l'objet.

### Nettoyage du code

- [x] Limites de taille du Scale supprimées partout : `InteractableObject`, fiches du catalogue (*Limites de taille*),
      `DecorConstraints` et la limite de 18 m (`DecorLimits`). Le Scale, à la manette comme à la main, n'est plus
      limité.
- [ ] `ObjectMenu` plante si l'un de ses 6 boutons n'est pas renseigné : le rendre tolérant.
- [ ] `BauprobeMenuLogic` et `StageCalibrator` écoutent tous les deux MRUK : le menu pourrait lire
      `StageCalibrator.IsQrLocated`.
- [ ] `BauprobeMenu` est rangé dans le prefab Meta `QRCodeSampleUI` (dossier `ThirdParty`). À terme, lui donner
      son propre Canvas pour ne plus dépendre de l'exemple Meta.

### Petites fonctionnalités

- [x] **Couleur sauvegardée** : la couleur choisie avec le bouton Color est gardée dans la sauvegarde (format version 4)
      et remise quand on recharge l'implantation. Les anciennes sauvegardes se rechargent avec la couleur d'origine.
      Elle est aussi écrite dans le tableau CSV (dernière colonne, *Color*, en code hexadécimal ; vide = couleur
      d'origine du modèle).
- [x] **QR code pas au sol** : la distance entre le bas du QR code et le sol se saisit dans le casque (écran
      « QR code offset », avec la position du QR code par rapport au point zéro). Le repère du plateau est posé
      au vrai sol, à la verticale du QR code.
- [x] **QR code du plateau** : pas de texte imposé (réglage *Expected Payload* supprimé) : n'importe quel QR code
      sert de repère. Ne poser qu'un seul QR code visible sur le plateau.
