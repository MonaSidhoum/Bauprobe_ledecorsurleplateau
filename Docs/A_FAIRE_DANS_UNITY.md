# À faire dans Unity après la restructuration

La restructuration du dépôt a été faite sans pouvoir ouvrir Unity. Voici ce qu'**une personne** doit
faire (dans l'ordre) en ouvrant le projet la première fois. Cochez au fur et à mesure.

---

## 1. ⚠️ Réparer les fichiers « pointeurs LFS » (le plus urgent)

### Le problème

41 fichiers de réglages Unity ne contiennent pas leur vrai contenu, mais seulement 3 lignes comme celles-ci :

```
version https://git-lfs.github.com/spec/v1
oid sha256:b7655b0be836...
size 25156
```

C'est un « pointeur LFS » : une étiquette qui dit « le vrai fichier est stocké ailleurs ». Ça concerne
**tout `ProjectSettings/`** et plusieurs fichiers de `Assets/` (réglages XR, URP, Meta, matériaux du QR code).
(Il y en avait 56 : 4 ont été repris de l'exemple Meta, et 11 réparés grâce à la branche `test/mr-quest3`.)
Avec ces fichiers, Unity n'a pas les bons réglages (XR, Android, rendu...) et le build sur le Quest ne marchera pas.

Pour voir la liste exacte :

```bash
git grep -l "^version https://git-lfs" -- Assets ProjectSettings
```

Les réglages Unity sont des fichiers **texte** : ils ne doivent pas passer par LFS. On veut donc remettre
leur vrai contenu, en texte normal.

### Solution A : depuis l'ordinateur où le projet marche

Sur l'ordinateur où le projet s'ouvre correctement dans Unity (celui qui a fait le commit « Fix Unity folder »),
les vrais fichiers sont présents. Il suffit de les recopier dans le nouveau dépôt :

| Ancien emplacement (sur l'ordi qui marche) | Nouvel emplacement |
|---|---|
| `Unity-MRUtilityKitSample/ProjectSettings/*` | `ProjectSettings/*` |
| `Unity-MRUtilityKitSample/Assets/<fichier>.asset` | `Assets/<fichier>.asset` (même chemin) |
| `Unity-MRUtilityKitSample/Assets/MRUKSamples/QRCodeDetection/Materials/*.mat` | `Assets/ThirdParty/MetaQRCodeDetection/Materials/*.mat` |

Puis :

```bash
git grep -l "^version https://git-lfs" -- Assets ProjectSettings   # doit maintenant ne plus rien afficher
git add ProjectSettings Assets
git commit -m "Remet le vrai contenu des réglages Unity (au lieu des pointeurs LFS)"
```

### Solution B : récupérer le contenu depuis le serveur GitLab

Si les fichiers ont bien été envoyés sur le serveur LFS du GitLab de l'école :

```bash
git lfs install
git lfs fetch origin --all
git lfs checkout
git grep -l "^version https://git-lfs" -- Assets ProjectSettings   # doit afficher moins de fichiers, idéalement aucun
git add ProjectSettings Assets
git commit -m "Remet le vrai contenu des réglages Unity (au lieu des pointeurs LFS)"
```

### Solution C (dernier recours) : regénérer les réglages

Supprimer les fichiers pointeurs, ouvrir le projet : Unity recrée des réglages par défaut. Il faut ensuite
refaire la configuration avec **Meta > Tools > Project Setup Tool** (bouton *Fix All*), puis remettre
les réglages Android (plateforme Android, passthrough, scene support...).

---

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
- [ ] `Assets/UniversalRenderPipelineGlobalSettings 1.asset` est un doublon créé par Unity quand l'original
      était cassé. Une fois l'étape 1 faite, vérifier lequel est utilisé
      (*Project Settings > Graphics*) et supprimer l'autre.

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

- [ ] *File > Build Profiles* : ajouter `Assets/_Bauprobe/Scenes/MenuGeneral.unity` dans la liste des scènes
      (la liste était vide).
- [ ] *Project Settings > Player* :
  * *Company Name* : `MinesParis`
  * *Product Name* : `Bauprobe`
  * *Package Name* (Android) : `fr.minesparis.bauprobe` (s'il s'agit encore de celui de l'exemple Meta)
- [ ] *Player > Splash Image* : retirer le logo « metaquest_samples » s'il apparaît comme manquant
      (l'image de l'exemple Meta a été supprimée).

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
