# Scripts – Quest 3 en réalité mixte (manettes)

Tes scripts manettes fonctionnent tels quels en MR : le passthrough, c'est de la configuration de scène, pas du code.
Ce dossier contient donc tes scripts d'origine, avec **les mêmes noms de fichiers et les mêmes `.meta` (GUID)** :
tu peux les copier par-dessus ceux de ton projet, tes prefabs et tes boutons gardent leurs références.
Seules des corrections ont été faites.

## Corrections apportées

| Script | Problème | Correction |
|---|---|---|
| `PlanarGrabRestrictor` (`SmartConstrainToPlane_Fixed`) | `NullReferenceException` quand l'objet est détruit sans avoir été saisi (`deplacement` null dans `OnDestroy`), ou si `Locomotion` / `coordinateCanvas` est introuvable | Gardes null partout |
| `PlanarGrabRestrictor`, `ScalingArrow` | Dans un build Quest, un mesh sans *Read/Write Enabled* est illisible : le pivot devenait `Infinity` et l'objet disparaissait à la rotation / au scaling | Repli sur la boîte englobante du Renderer |
| `ScalingBis` | `NullReferenceException` si on change d'axe (A + grip) ou si on scale hors mode Scaling, car la flèche n'existe qu'en mode Scaling | Flèche optionnelle, gardes null |
| `ScalingBis` | `GameObject.Find("Right Controller Visual")` à chaque frame ; pivot faux si le mesh est dans un enfant ou illisible | Recherche mise en cache ; repli sur la boîte englobante |
| `Globalsupprimer` | `my_ControllerState` était un champ privé jamais renseigné : le mode Deleting n'était pas réellement détecté. Plantage si l'objet visé n'a pas `Destroy_Object_On_Click` | Utilise le `myControllerInputActionManager` de `Right Controller` (champ public, auto-trouvé) ; vérification null |
| `Apparitionobjet` | Plantage si le prefab n'a pas le script du mode courant | Gardes null |

Inchangés : `Destroy_Object_on_Click`, `Rotationobjet`, `Change_color_green`, `Change_color_white`, `vert_debut`.
`ScalingArrow` est l'ancienne version de `ScalingBis` (non utilisée par `Apparitionobjet`) ; elle est gardée avec les mêmes correctifs.

## Checklist passthrough / MR (configuration de scène, à vérifier chez toi)

- **Fond transparent** : sur la caméra de l'XR Origin, *Clear Flags* = Solid Color avec un fond noir **alpha 0**. Supprime le skybox, le sol et tout décor VR opaque.
- **Passthrough activé** : soit via Meta XR SDK (composant passthrough sur la caméra / Building Block « Passthrough »), soit via AR Foundation + le plugin Meta OpenXR (`AR Camera Manager` / `AR Session`), selon ce que tu utilises déjà.
- **Sol réel** : mets le *Tracking Origin Mode* de l'XR Origin sur **Floor**. Ainsi `y = 0` est le vrai sol de la pièce, et `fixedValue = 0` (snap trigger + grip) colle l'objet au sol réel, comme avant en VR.
- **Manifest / permissions** : le passthrough et la détection de scène demandent des réglages dans l'Android Manifest / les paramètres OpenXR Meta. Suis la doc Meta pour ta version du SDK.
- **Models 3D** : active *Read/Write Enabled* à l'import des modèles pour que le point bas soit calculé précisément (sinon la boîte englobante est utilisée, ce qui marche aussi).

> Remarque : je n'ai pas pu ouvrir Unity ici ; les scripts n'ont pas été compilés. Si une erreur apparaît à l'import, envoie-moi le message de la console.
