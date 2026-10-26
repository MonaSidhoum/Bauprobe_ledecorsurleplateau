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

Il y a pour l'instant **deux scènes** :

* `Assets/_Bauprobe/Scenes/Main.unity` (anciennement `QRCodeDetection.unity`, avec le cube de test) ;
* `Assets/_Bauprobe/Scenes/MenuGeneral.unity` (anciennement `menu général.unity`, de la branche de test,
  avec le menu Bauprobe).

- [ ] Choisir la scène principale. Le plus simple : ouvrir `MenuGeneral.unity` (la plus avancée),
      y copier le Cube de `Main.unity`, puis la renommer `Main.unity` et supprimer l'ancienne.
- [ ] Le prefab Meta `ThirdParty/MetaQRCodeDetection/Prefabs/QRCodeSampleUI.prefab` a été modifié sur la
      branche de test. À terme, sortir le menu Bauprobe dans un prefab à nous (`_Bauprobe/UI/Prefabs/`).
- [ ] `Assets/UniversalRenderPipelineGlobalSettings 1.asset` est un doublon créé par Unity quand l'original
      était cassé. Une fois l'étape 1 faite, vérifier lequel est utilisé
      (*Project Settings > Graphics*) et supprimer l'autre.

Dans la scène choisie :

- [ ] L'objet **QRAnchor** a le composant **Stage Calibrator** (anciennement *QR Scene Coordinator*),
      avec *Qr Anchor* = QRAnchor et *Stage Root* = StageRoot.
- [ ] L'objet **StageRoot** a le composant **Stage Space**.
- [ ] Le **Cube** de test est toujours enfant de StageRoot (son prefab est maintenant dans `_Bauprobe/Decor/Prefabs/`).
- [ ] Optionnel : remplir *Expected Payload* avec le texte de votre QR code du plateau.

## 4. Brancher les nouveaux composants

Sur l'objet **Managers** (ou un nouvel objet vide) :

- [ ] Ajouter **Decor Spawner** : *Stage* = StageRoot, *Catalog* = le catalogue (voir ci-dessous).
- [ ] Ajouter **Layout Manager** : *Spawner* = le Decor Spawner.

Créer le catalogue :

- [ ] Dans `Assets/_Bauprobe/Decor/Catalog/` : clic droit > *Create > Bauprobe > Catalogue de décor*.
- [ ] Créer une fiche *Create > Bauprobe > Élément de décor* pour le Cube (prefab = `Decor/Prefabs/Cube`),
      et l'ajouter au catalogue.
- [ ] Sur le prefab Cube : ajouter un Collider, la saisie Interaction SDK (clic droit >
      *Interaction SDK > Add Grab Interaction*) et **Decor Constraints**.

Menu (peut être fait plus tard par la personne chargée de l'UI) :

- [ ] Créer un Canvas en World Space utilisable avec l'Interaction SDK (*Interaction SDK > Add Ray Interaction to Canvas*).
- [ ] Y mettre **Decor Menu** + un bouton modèle (Button + TextMeshPro).
- [ ] Ajouter des boutons « Sauvegarder » (→ `LayoutManager.Save`), « Charger » (→ `LayoutManager.Load`),
      « Recaler » (→ `StageCalibrator.Unlock`), « Figer » (→ `StageCalibrator.Lock`).

## 5. Réglages de build

- [ ] *File > Build Profiles* : ajouter `Assets/_Bauprobe/Scenes/Main.unity` dans la liste des scènes
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
