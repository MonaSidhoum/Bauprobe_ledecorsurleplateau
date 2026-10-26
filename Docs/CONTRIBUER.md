# Travailler à plusieurs sur le projet

## Une fois par ordinateur

```bash
git lfs install
```

Et dans Unity : **Edit > Project Settings > Editor**, vérifier :
* *Asset Serialization* : **Force Text**
* *Version Control* : **Visible Meta Files**

### (Recommandé) L'outil de fusion de Unity

Unity fournit un outil (« UnityYAMLMerge ») qui sait fusionner les scènes et les prefabs bien mieux que Git.
Pour l'activer, ajoutez ces lignes dans le fichier `.git/config` de votre clone (en adaptant le chemin
à votre version de Unity) :

```ini
[merge "unityyamlmerge"]
    name = Unity SmartMerge
    driver = '<CHEMIN_UNITY>/Data/Tools/UnityYAMLMerge' merge -p %O %B %A %A
    recursive = binary
```

* Windows : `C:/Program Files/Unity/Hub/Editor/6000.3.26f1/Editor/Data/Tools/UnityYAMLMerge.exe`
* macOS : `/Applications/Unity/Hub/Editor/6000.3.26f1/Unity.app/Contents/Tools/UnityYAMLMerge`

Si vous ne le faites pas, Git fait une fusion classique (ça marche, c'est juste moins malin).

## Le cycle de travail (à chaque nouvelle tâche)

```bash
# 1. Partir de la dernière version de main
git checkout main
git pull

# 2. Créer sa branche (une branche = une tâche)
git checkout -b feature/decor-catalogue

# 3. Travailler, puis enregistrer régulièrement
git add Assets/_Bauprobe/Decor
git commit -m "Ajout du catalogue de décors"

# 4. Envoyer sa branche
git push -u origin feature/decor-catalogue
```

5. Sur GitLab, ouvrir une **Merge Request** vers `main`, et demander à une autre personne de relire.
6. Une fois validée, on fusionne. On supprime la branche.

Noms de branche : `feature/<module>-<sujet>` (nouveauté), `fix/<sujet>` (correction).

## Les règles d'or

1. **`main` doit toujours marcher.** On ne pousse jamais directement sur `main`.
2. **Une personne par module.** Si vous devez toucher au module de quelqu'un d'autre, prévenez-le.
3. **Ne modifiez pas à plusieurs la même scène.**
   * Chacun teste dans **sa scène** : `Assets/_Bauprobe/Scenes/Sandbox/<Prénom>.unity`.
   * **Une seule personne** modifie `MenuGeneral.unity` à la fois (annoncez-le sur le groupe).
   * Travaillez le plus possible dans des **prefabs** : deux personnes qui modifient deux prefabs différents
     n'ont jamais de conflit.
4. **Toujours committer le `.meta` avec son fichier.** Le `.meta` contient l'identifiant de l'asset :
   sans lui, les références cassent chez les autres (objets « Missing »).
5. **Ne jamais committer** : `Library/`, `Temp/`, `Logs/`, `UserSettings/`, les `.apk`, les `.csproj`/`.sln`.
   Le `.gitignore` s'en occupe, mais vérifiez avec `git status` avant de committer.
6. **Les réglages du projet** (`ProjectSettings/`, `Packages/manifest.json`, version de Unity) :
   une seule personne responsable. On ne change pas la version de Unity ou d'un package sans en parler.
7. **Gros fichiers** (modèles 3D, images, sons) : ils passent automatiquement par Git LFS grâce au
   fichier `.gitattributes`. Vérifiez avec `git lfs ls-files` qu'ils y sont bien.

## Répartition proposée (4 personnes)

| Personne | Module(s) |
|---|---|
| A | Calibration (QR code, précision du calage sur le plateau) |
| B | Decor (catalogue, import des modèles du bureau d'études) |
| C | Manipulation (saisie, déplacement, agrandissement) |
| D | UI + Persistence (menus, sauvegarde des implantations) |

## En cas de conflit sur une scène ou un prefab

Si Git signale un conflit sur un `.unity` ou un `.prefab` et que vous ne savez pas le résoudre :

```bash
# garder la version de main (et refaire vos changements à la main dans Unity)
git checkout --theirs chemin/vers/Fichier.unity
# ou garder votre version
git checkout --ours chemin/vers/Fichier.unity
```

Puis `git add` le fichier et terminer la fusion. En cas de doute, demandez avant de forcer quoi que ce soit.
