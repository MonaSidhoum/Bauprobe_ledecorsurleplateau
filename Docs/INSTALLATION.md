# Installation pas à pas

Ce document explique comment récupérer le projet sur son ordinateur, installer l'appli sur un **Meta Quest 3** et
s'en servir sur un plateau. Il s'adresse aussi à quelqu'un qui n'a **jamais utilisé Unity, Git ni un Meta Quest**.
Le mode d'emploi dans le casque (boutons, manettes) est dans le [README](../README.md#utilisation-dans-le-casque).

1. [Ce qu'il faut avant de commencer](#1-ce-quil-faut-avant-de-commencer)
2. [Récupérer et ouvrir le projet](#2-récupérer-et-ouvrir-le-projet)
3. [Installer l'appli sur le casque](#3-installer-lappli-sur-le-casque)
4. [Premier lancement et calage sur le QR code](#4-premier-lancement-et-calage-sur-le-qr-code)
5. [Récupérer les sauvegardes et le tableau CSV](#5-récupérer-les-sauvegardes-et-le-tableau-csv)
6. [Ajouter son propre décor](#6-ajouter-son-propre-décor)

---

## 1. Ce qu'il faut avant de commencer

**Sur l'ordinateur :**
* Un **ordinateur Windows ou Mac** avec environ **30 Go libres** (Unity + le projet).
* **Git** et un **terminal** :
  * Windows : installer **Git Bash** (c'est dans lui qu'on tape toutes les commandes de ce document) ;
  * macOS / Linux : l'application **Terminal** suffit, Git s'installe si besoin.

  Tutoriel pas à pas (section « git (et bash sur Windows) ») : [Installations du cours d'informatique des Mines](https://intro.info-mines.paris/1-1-installations/).

**Pour le casque :**
* un **Meta Quest 3**, ses deux manettes, et un **câble USB-C qui transfère les données** (pas un simple câble de
  charge) ;
* un **compte Meta** qui est **propriétaire du casque** (il n'y a qu'un propriétaire par casque, et seul lui peut
  activer le mode développeur) ;
* l'**application mobile Meta Horizon** sur un téléphone (iOS ou Android).

**Sur le plateau (le « kit » à emporter) :**
* le casque chargé, ses manettes et son chargeur ;
* un **QR code imprimé** d'au moins **15 cm de côté** (n'importe quel QR code : son contenu n'est pas lu) et un pied ou
  un support pour le tenir à la verticale ;
* un **mètre ruban**, pour mesurer la position du QR code par rapport au point zéro de la scène et sa hauteur ;
* un ordinateur et le câble USB-C, seulement pour installer l'appli ou récupérer les tableaux CSV.

---

## 2. Récupérer et ouvrir le projet

1. **Installer Unity Hub** : [documentation d'installation de Unity Hub](https://docs.unity.com/en-us/hub/install-hub).
   Se connecter (ou créer) un compte Unity au premier lancement ; la licence *Personal* (gratuite) suffit.
2. **Installer l'éditeur Unity 6000.3.26f1** depuis Unity Hub :
   *Installs > Install Editor > Archive* (lien vers l'[archive des versions](https://unity.com/releases/editor/archive))
   > version **6000.3.26f1**, et **cocher le module Android Build Support** (avec *OpenJDK* et *Android SDK & NDK
   Tools*). Sans ce module, impossible d'installer l'appli sur le casque.
   Pour ajouter le module après coup : Unity Hub > *Installs* > ⚙ de la version > *Add modules*.
3. **Ouvrir un terminal** (Git Bash sur Windows, Terminal sur macOS / Linux) et se placer dans le dossier où ranger le
   projet, par exemple *Documents* :

```bash
cd ~/Documents
```

4. **Activer Git LFS** (une seule fois par ordinateur). Les modèles 3D du projet sont stockés avec Git LFS : sans
   lui, ils seraient remplacés par de petits fichiers texte et les décors seraient vides.

```bash
git lfs install
```

5. **Cloner le projet** (le télécharger) :

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
   10 à 20 minutes (Unity télécharge les packages Meta et importe tout le projet).
7. **Vérifications** :
   * Ouvrir la **Console** (*Window > General > Console*) : il ne doit pas y avoir d'erreur rouge.
   * *File > Build Profiles* : la plateforme active doit être **Meta Quest** (ou **Android**). Sinon, la choisir dans
     la liste et cliquer **Switch Platform** (quelques minutes).
   * Ouvrir la scène `Assets/_Bauprobe/Scenes/MenuGeneral.unity` (double-clic dans la fenêtre *Project*). C'est la
     seule scène de l'appli.
   * (Optionnel) Lancer les tests automatiques : *Window > General > Test Runner* > **EditMode** > *Run All*.

---

## 3. Installer l'appli sur le casque

### Préparer le casque (une seule fois)

1. **Appairer le casque avec l'application mobile Meta Horizon**, connectée avec le compte propriétaire du casque.
2. **Activer le mode développeur** dans l'appli mobile : *Appareils* > le casque > *Paramètres du casque* >
   *Mode développeur*. Si l'appli le demande, créer une « organisation » développeur (gratuite) sur
   [developer.meta.com](https://developer.meta.com/) avec le même compte. Redémarrer le casque.
3. **Faire reconnaître le casque par l'ordinateur.** La première fois, le casque branché en USB-C n'est souvent
   **pas reconnu** par Unity, et la fenêtre « Autoriser le débogage USB » n'apparaît pas forcément dans le casque.
   * **Windows** : ouvrir **Meta Horizon Link** (aussi appelé *Meta Quest Link* : c'est le même logiciel), et
     **appairer le casque** (*Appareils > Ajouter un casque*, en USB). Une fois appairé, il est reconnu par Unity.
   * **macOS** : Meta Horizon Link n'existe pas ; installer **Meta Quest Developer Hub** (MQDH) et y connecter le
     casque.
   * Dans le casque, accepter **« Autoriser le débogage USB »** (cocher *Toujours autoriser depuis cet ordinateur*).

### Installer depuis Unity

1. Brancher le casque en USB-C, l'allumer.
2. Dans Unity : *File > Build Profiles* > *Run Device* > **Refresh** : le Quest 3 apparaît dans la liste.
3. Le choisir, puis **Build And Run**. La compilation prend plusieurs minutes la première fois ; l'appli se lance
   toute seule dans le casque à la fin.

L'appli reste ensuite installée : on peut la relancer **sans ordinateur**, depuis la bibliothèque d'applications du
casque (*Bibliothèque* > *Sources inconnues* > **Bauprobe**).

### Installer sur un autre casque sans Unity (fichier `.apk`)

Pour installer l'appli sur un casque sans ouvrir Unity (par exemple au théâtre) :

1. Une fois, sur l'ordinateur qui a Unity : *File > Build Profiles* > **Build** (et non *Build And Run*), et
   enregistrer le fichier, par exemple `Bauprobe.apk`.
2. Sur n'importe quel ordinateur, avec le casque en mode développeur (étapes 1 à 3 ci-dessus) : ouvrir **Meta Quest
   Developer Hub**, sélectionner le casque et **glisser le fichier `.apk`** dans la fenêtre du casque. Ou, en ligne de
   commande (`adb` est installé avec MQDH) :

```bash
adb install -r Bauprobe.apk
```

### Si ça ne marche pas

* **Le casque n'apparaît pas dans *Run Device*** : vérifier que le câble transfère les données (en essayer un autre),
  remettre le casque, accepter le débogage USB, puis *Refresh*. Le mode développeur doit être activé.
* **Erreurs d'autorisation avec Meta Horizon Link** (par exemple au moment de choisir le dossier où il installe les
  applications), tests USB en erreur, petites choses qui ne marchent pas : la version de Meta Horizon Link installée
  par les outils de Unity peut être défectueuse. Le plus simple est de la **réinstaller à la main** :
  1. ouvrir la page d'aide de Meta [Installer l'application Meta Quest Link](https://www.meta.com/fr-ca/help/quest/1517439565442928/)
     et cliquer sur **Télécharger l'application** ;
  2. lancer le fichier `Setup.exe` téléchargé : il détecte la version déjà installée et propose de la
     **désinstaller** ;
  3. une fois la désinstallation terminée, **relancer le même `Setup.exe`** pour réinstaller Meta Horizon Link ;
  4. appairer de nouveau le casque.
* **Écran noir quelques secondes quand on remet le casque** : le Quest se met en veille dès qu'on l'enlève. Pour les
  répétitions, on peut désactiver le capteur de proximité dans Meta Quest Developer Hub (*Device Manager*) ; le
  réglage peut revenir après un redémarrage du casque.

---

## 4. Premier lancement et calage sur le QR code

### La permission « données spatiales »

Au premier lancement, accepter la permission **« données spatiales »** : sans elle, le QR code n'est pas détecté.
Si elle a été refusée : bouton **Enable Access** du menu général (main gauche). La ligne d'état du menu affiche
« Permission: granted » quand c'est bon.

### Le point zéro et l'offset du QR code

Toutes les positions (sauvegardes, tableau CSV, panneau INFO) sont mesurées depuis le **point zéro de la scène**,
choisi avec le bureau d'études (par ex. le **centre du nez de scène**), **au sol**. Le QR code n'a pas besoin d'être
sur ce point : on dit à l'appli **où il est**. Tout se saisit **dans le casque**, sans refaire de *Build*.

**Au lancement**, le menu général ouvre l'écran **« QR code offset »**, pré-rempli avec les dernières valeurs validées
(gardées sur le casque d'un lancement à l'autre ; désinstaller l'appli les efface). Si le QR code n'a pas bougé :
**Confirm** directement. Sinon, cliquer sur un champ, taper la valeur au pavé (la première touche remplace l'ancienne
valeur ; `+/-` change le signe ; **point** décimal : `1.2`), puis **Confirm**. *Back* annule.
L'écran est aussi accessible à tout moment par la **première ligne de l'écran Load** (« QR offset: … »).

Chaque champ rappelle le sens des valeurs, **vu par quelqu'un qui fait face au QR code** (depuis la salle) :

| Champ (dans le casque) | Ce qu'on mesure au mètre |
|---|---|
| **X (m)   + right / - left** | Position du **centre du QR code** depuis le point zéro : positif s'il est à **droite** du point zéro (cour), négatif à gauche (jardin) |
| **Y (m)   + far / - near** | Position du **centre du QR code** depuis le point zéro : positif s'il est plus **loin** (lointain), négatif plus près (salle) |
| **QR height (m)** | Distance **verticale** entre le **bord du bas** du carré imprimé (pas le centre, pas le bord de la feuille) et le **sol du plateau**. 0 = QR code posé au sol |

Exemple : QR code sur un pied, au milieu du plateau, 2 m derrière le nez de scène, bas du QR code à 1,20 m du sol
→ X = `0`, Y = `2`, QR height = `1.2`. Tout à 0 : le QR code (posé au sol) **est** le point zéro.

**Saisir l'offset avant de poser ou de charger le décor** : si on le change ensuite, le décor déjà posé bouge avec
le point zéro. Une erreur de mesure de 5 cm décale tout le décor de 5 cm.

### Poser le QR code

* **Coller le QR code à la verticale, face à la salle**, bien parallèle au nez de scène (il n'y a pas d'angle à
  saisir). L'appli prend comme « avant » (Y+) la direction dans laquelle on regarde quand on lit le QR code, et comme
  X+ la droite de celui qui le lit : QR code lu depuis la salle → Y+ = lointain, X+ = cour.
  Un QR code de travers de 5° fait tourner tout le décor de 5° : soigner l'alignement.
* Un QR code posé **à plat au sol** marche aussi : l'avant est alors la direction de son bord du haut.
* Ne laisser **qu'un seul QR code** visible sur le plateau.

Regarder le QR code : le menu affiche « QR codes detected: 1 », le décor est calé. Le calage est alors **figé** (le
décor ne tremble plus, même si le QR code sort du champ de vision). Si le QR code est déplacé, **relancer l'appli**.

### Vérifier le calage (une fois)

*Import* > *Cube*, le poser sur le point zéro, puis regarder son panneau **INFO** (ou *Save* et ouvrir le CSV) :
X et Y doivent valoir environ 0, et Z environ 0 (cube posé au sol). Le déplacer de 1 m vers le lointain : Y doit
augmenter de 1.

### Recharger une scène un autre jour

Les sauvegardes sont mesurées depuis le point zéro, pas depuis le QR code. Si le QR code a été recollé ailleurs, il
suffit de **saisir son nouvel offset au lancement**, puis *Load* : le décor revient au même endroit du plateau.

---

## 5. Récupérer les sauvegardes et le tableau CSV

Chaque *Save* écrit deux fichiers dans le casque : `<nom>.json` (relu par *Load*) et `<nom>.csv` (le tableau pour le
bureau d'études). Supprimer une sauvegarde dans l'appli supprime les deux. Le format du tableau est décrit dans le
[README](../README.md#le-tableau-csv-pour-le-bureau-détudes).

**Avec l'explorateur de fichiers** : brancher le casque en USB-C, accepter l'accès aux fichiers dans le casque, puis
ouvrir *Quest 3 > Stockage partagé interne > Android > data > fr.minesparis.bauprobe > files > Layouts*
(explorateur Windows, *Android File Transfer* sur Mac, ou le gestionnaire de fichiers de Meta Quest Developer Hub).

**En ligne de commande** (copie tout le dossier dans le dossier courant) :

```bash
adb pull /sdcard/Android/data/fr.minesparis.bauprobe/files/Layouts
```

Les sauvegardes et l'offset du QR code ne sont pas dans le projet Unity : réinstaller l'appli par-dessus ne les
efface pas, seule la **désinstallation** de l'appli les supprime. Penser à les copier avant.

---

## 6. Ajouter son propre décor

Un décor, dans Unity, c'est trois choses :
* le **modèle 3D** (fichier `.obj`, `.fbx`…) : la forme ;
* un **prefab** : un « objet modèle » prêt à être posé, qui contient le modèle 3D, un **collider** (la boîte
  invisible que touche le laser : sans lui, on ne peut pas sélectionner l'objet) et les composants qui permettent de
  le manipuler ;
* une **fiche** (`DecorItemDefinition`) dans le **catalogue** : c'est elle qui fait apparaître le décor dans la
  liste *Import* du casque.

### Étape 1 : importer le modèle 3D

1. Dans la fenêtre **Project** (en bas de Unity), aller dans `Assets/_Bauprobe/Decor/Models/` et créer un dossier
   au nom du décor (clic droit > *Create > Folder*).
2. **Glisser le fichier** du modèle (avec son `.mtl` pour un OBJ) depuis l'explorateur de fichiers de l'ordinateur
   dans ce dossier.
3. Cliquer sur le modèle dans *Project* : dans l'**Inspector** (à droite), onglet *Model*, régler le
   **Scale Factor** pour que le modèle soit **en mètres** : `0.001` pour un modèle en millimètres (c'est le cas des
   OBJ du bureau d'études), `1` s'il est déjà en mètres. Cliquer **Apply**.
   Le pivot du modèle peut être n'importe où : le **point d'origine** (bas au centre pour un élément au sol, haut au
   centre pour un élément au cintre) est calculé automatiquement.

### Étape 2 (méthode rapide) : l'outil « Ajouter au catalogue »

Clic droit sur le modèle dans *Project* > **Bauprobe > Ajouter au catalogue**. L'outil :
* crée le **prefab** à côté du modèle (`<nom>_Decor.prefab`), avec un **Box Collider** ajusté au modèle ;
* y recopie les composants de manipulation du premier décor du catalogue, le Cube (*Rigidbody*, *Grabbable*,
  *Decor Constraints*, *Interactable Object*…) ;
* crée la **fiche** dans `Decor/Catalog/` et l'**ajoute au catalogue**.

La Console (*Window > General > Console*) affiche ce qui a été fait. Ensuite :
* ouvrir la fiche créée (`Decor/Catalog/<nom>.asset`) pour vérifier le **nom affiché** et choisir **Sol** ou
  **Cintre** (pour le cintre, régler aussi la *Hang Height*, la hauteur du haut de l'objet à l'import) ;
* pour pouvoir **prendre le décor à la main** (grab) : l'outil ne l'ajoute pas. Double-cliquer sur le prefab créé,
  clic droit sur sa racine dans la *Hierarchy* > *Interaction SDK* > **Add Grab Interaction**, puis enregistrer
  (*Ctrl + S*). Sans cette étape, le décor se manipule quand même au laser et à la manette ;
* comme le Cube, le nouveau décor peut être **basculé** en mode ROTATE. Pour qu'il reste toujours droit, cocher
  *Keep Upright* sur son composant *Decor Constraints*.

Le décor apparaît dans la liste *Import* au prochain *Build And Run*.

### Étape 2 (méthode à la main), si on préfère tout faire soi-même

1. **Faire un prefab** : glisser le modèle depuis *Project* dans la fenêtre **Hierarchy** (à gauche) : il apparaît
   dans la scène. Puis **le glisser depuis la Hierarchy vers le dossier** `Assets/_Bauprobe/Decor/Prefabs/` dans
   *Project* ; choisir **Original Prefab** si Unity le demande. Supprimer ensuite l'objet de la Hierarchy (clic droit
   > *Delete*) : on ne garde que le prefab.
2. **Ouvrir le prefab** : double-clic dessus dans *Project* (flèche **<** en haut de la Hierarchy pour revenir à la
   scène).
3. **Ajouter un collider** : cliquer sur l'objet racine du prefab, puis dans l'Inspector **Add Component** > taper
   `Box Collider` > Entrée. Pour l'ajuster au modèle : clic droit sur le titre du composant *Box Collider* (ou ses
   **⋮**) > **Bauprobe : ajuster aux modèles 3D enfants**. Les dimensions en mètres s'affichent dans la Console
   (pratique pour vérifier l'échelle par rapport aux plans).
4. **Composants de manipulation**, sur l'objet racine :
   * clic droit sur l'objet dans la Hierarchy > *Interaction SDK* > **Add Grab Interaction** (saisie à la main) ;
   * **Add Component** > `Decor Constraints` et `Interactable Object`.
5. **Fiche** : clic droit dans `Assets/_Bauprobe/Decor/Catalog/` > *Create > Bauprobe > Élément de décor*. Remplir le
   nom, glisser le prefab dans le champ *Prefab* et choisir **Sol** ou **Cintre**.
6. **Ajouter la fiche au catalogue** : cliquer sur `Decor/Catalog/DecorCatalog.asset`, puis dans l'Inspector,
   **+** sous la liste *Items* et glisser la fiche dans la nouvelle ligne.

Si le projet est versionné avec Git, **committer le modèle, le prefab et la fiche avec leurs fichiers `.meta`**
(sans eux, les références cassent chez les autres).
