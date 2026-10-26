# À faire dans Unity après la restructuration

La restructuration du dépôt a été faite sans pouvoir ouvrir Unity. Voici ce qu'**une personne** doit
faire (dans l'ordre) en ouvrant le projet la première fois. Cochez au fur et à mesure.

---

## 1. ✅ Fichiers « pointeurs LFS » : réparé

56 fichiers de réglages Unity ne contenaient qu'une étiquette LFS (3 lignes) au lieu de leur vrai contenu.
Ils ont tous été remis en texte normal, avec un contenu vérifié identique à l'original (même empreinte sha256,
aux fins de ligne près). Pour vérifier qu'il n'en reste aucun :

```bash
git grep -l "^version https://git-lfs" -- Assets ProjectSettings   # ne doit rien afficher
```

Les réglages Unity sont des fichiers **texte** : ils ne doivent jamais passer par LFS
(le `.gitattributes` s'en assure).

## 2. Ouvrir le projet

1. Fermer Unity. Supprimer le dossier `Library/` de votre ancien clone s'il existe
   (le projet a changé de dossier : il est maintenant **à la racine du dépôt**).
2. Unity Hub > *Add project from disk* > choisir la racine du dépôt.
3. Attendre l'import (long la première fois).
4. Ouvrir la **Console** (*Window > General > Console*) : il ne doit pas y avoir d'erreur rouge.
   * Si un `.asmdef` signale une référence introuvable (`meta.xr.mrutilitykit`, `Oculus.VR`,
     `Unity.TextMeshPro`...), cliquer sur le `.asmdef` concerné et corriger la référence dans l'Inspector.

## 3. Vérifier la scène principale

La seule scène est `Assets/_Bauprobe/Scenes/MenuGeneral.unity` (anciennement `menu général.unity`).
Ouvrez-la et vérifiez :

- [ ] **QRAnchor** a le composant **Stage Calibrator** (anciennement *QR Scene Coordinator*),
      avec *Qr Anchor* = QRAnchor et *Stage Root* = StageRoot.
- [ ] **StageRoot** a le composant **Stage Space**.
- [ ] **Managers** a le composant **Decor Spawner**, avec *Stage* = StageRoot et
      *Catalog* = `Decor/Catalog/DecorCatalog`.
- [ ] **BauprobeMenu** (composant *Bauprobe Menu Logic*) a *Spawner* = le Decor Spawner de Managers.
- [ ] Le bouton de la liste « Importer » appelle bien `BauprobeMenuLogic.SpawnIndex` avec `0` (= le Cube).
      Si l'appel apparaît comme *Missing*, le rebrancher (le script a changé de dossier).
- [ ] Optionnel : remplir *Expected Payload* (Stage Calibrator) avec le texte du QR code du plateau.
- [ ] Le prefab Meta `ThirdParty/MetaQRCodeDetection/Prefabs/QRCodeSampleUI.prefab` a été modifié sur la
      branche de test. À terme, sortir le menu Bauprobe dans un prefab à nous (`_Bauprobe/UI/Prefabs/`).

## 4. Mettre en place la manipulation à la manette

Le prefab Cube a déjà **Interactable Object** et **Decor Constraints** (objet toujours droit).
Il reste à ajouter dans la scène les deux éléments écrits par Théophile (ils n'étaient encore dans aucune scène) :

- [ ] Le **laser** : créer un objet vide enfant de `RightHandAnchor` (dans `[BuildingBlock] Camera Rig`),
      lui ajouter **Right Controller Ray Selector** et, si on veut voir le laser, un **Line Renderer**
      (fin, environ 0,005) glissé dans le champ *Laser*.
- [ ] Le **menu de l'objet** : créer un Canvas en *World Space* (petit : échelle 0,001), lui ajouter
      **Object Menu** et 6 boutons : ✕ (fermer), Supprimer, Couleur, Déplacer, Scale, Rotation,
      à glisser dans les champs correspondants. Laisser l'objet **actif** : il se cache tout seul au lancement.
- [ ] Le Canvas doit pouvoir être cliqué à la manette, comme le menu général
      (même réglage que le Canvas de `QRCodeSampleUI` : *OVR Raycaster*).
- [ ] Optionnel : ajouter **Layout Manager** sur Managers (*Spawner* = le Decor Spawner) et deux boutons
      « Sauvegarder » / « Charger » dans le menu général (→ `LayoutManager.Save` / `LayoutManager.Load`).

Le Cube garde aussi sa saisie à la main d'origine (Interaction SDK). Elle peut rester : Decor Constraints
empêche aussi de pencher l'objet quand on l'attrape à la main.

## 5. Réglages de build

Déjà faits dans le dépôt, à vérifier seulement :

- [ ] *File > Build Profiles* : la liste des scènes contient uniquement `Assets/_Bauprobe/Scenes/MenuGeneral.unity`.
- [ ] *Project Settings > Player* : *Company Name* = `MinesParis`, *Product Name* = `Bauprobe`,
      *Package Name* (Android) = `fr.minesparis.bauprobe`.
      L'appli s'installe donc à côté de l'ancienne « MRUKSample » dans le casque : on peut désinstaller cette dernière.

## 6. Lancer les tests

- [ ] *Window > General > Test Runner* > onglet **EditMode** > *Run All*. Les 2 tests doivent être verts.

## 7. Committer

```bash
git status            # vérifier : pas de Library/, pas de .csproj
git add -A
git commit -m "Ouverture dans Unity : scène et réglages vérifiés"
```

Unity va probablement générer `Packages/packages-lock.json` : **il faut le committer** (il garantit que
tout le monde a les mêmes versions de packages).
